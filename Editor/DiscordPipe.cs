using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace DiscordEditorPresence
{
    internal sealed class DiscordPipe : IDisposable
    {
        private const int Handshake = 0;
        private const int Frame = 1;
        private const int Close = 2;
        private const int Ping = 3;
        private const int Pong = 4;
        private const int HeaderSize = 8;
        private const int ResponseTimeout = 5000;
        private const int UpdateInterval = 5000;

        private readonly string applicationId;
        private readonly string processId;
        private readonly TaskCompletionSource<bool> stopping =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Task worker;
        private string previousDetails;
        private string previousState;
        private long previousStart;
        private volatile string activity;
        private volatile string status = "Discordに接続中…";
        private volatile SafeFileHandle activePipe;

        internal DiscordPipe(string applicationId, int processId)
        {
            this.applicationId = applicationId;
            this.processId = processId.ToString(CultureInfo.InvariantCulture);
            worker = Task.Run(RunAsync);
        }

        internal string Status => status;

        internal void SetActivity(string details, string state, long start)
        {
            if (details == previousDetails && state == previousState && start == previousStart)
                return;

            activity = JsonUtility.ToJson(new Activity
            {
                details = details,
                state = state,
                timestamps = new Timestamps { start = start }
            });
            previousDetails = details;
            previousState = state;
            previousStart = start;
        }

        public void Dispose()
        {
            stopping.TrySetResult(true);
            // Give Discord a chance to receive the clear command before Unity unloads the assembly.
            if (!worker.Wait(200))
                activePipe?.Dispose();
        }

        private async Task RunAsync()
        {
            while (!stopping.Task.IsCompleted)
            {
                try
                {
                    using (SafeFileHandle pipe = Connect())
                    {
                        activePipe = pipe;
                        if (pipe == null)
                            status = "Discordデスクトップ版を待っています。";
                        else if (!stopping.Task.IsCompleted)
                            await RunSessionAsync(pipe);
                    }
                }
                catch (Exception error) when (error is IOException || error is TimeoutException ||
                                              error is UnauthorizedAccessException ||
                                              error is ArgumentException || error is ObjectDisposedException)
                {
                    status = "Discordへの接続または共有に失敗しました。再試行します。";
                }
                finally
                {
                    activePipe = null;
                }

                await Task.WhenAny(stopping.Task, Task.Delay(5000));
            }
        }

        private SafeFileHandle Connect()
        {
            for (int index = 0; index < 10 && !stopping.Task.IsCompleted; index++)
            {
                // Unity's bundled Mono can route managed named pipes through Unix sockets.
                SafeFileHandle pipe = NativePipe.CreateFile(@"\\.\pipe\discord-ipc-" + index,
                    NativePipe.GenericReadWrite, 0, IntPtr.Zero, NativePipe.OpenExisting,
                    NativePipe.IdentifyOnly, IntPtr.Zero);
                if (pipe.IsInvalid)
                {
                    pipe.Dispose();
                    continue;
                }

                uint mode = NativePipe.NoWait;
                if (NativePipe.SetNamedPipeHandleState(pipe, ref mode, IntPtr.Zero, IntPtr.Zero))
                    return pipe;

                pipe.Dispose();
                throw new IOException("Could not configure Discord IPC pipe.");
            }

            return null;
        }

        private async Task RunSessionAsync(SafeFileHandle pipe)
        {
            status = "Discordの応答を待っています…";
            byte[] handshake = Encoding.UTF8.GetBytes("{\"v\":1,\"client_id\":\"" + applicationId + "\"}");
            Send(pipe, Handshake, handshake);
            Task<Packet> incoming = ReadPacketAsync(pipe);
            Task readyDeadline = Task.Delay(ResponseTimeout);

            while (!stopping.Task.IsCompleted)
            {
                Task completed = await Task.WhenAny(incoming, readyDeadline, stopping.Task);
                if (completed == stopping.Task)
                    return;
                if (completed == readyDeadline)
                    throw new TimeoutException();

                if (HandlePacket(pipe, await incoming)?.evt == "READY")
                    break;
                incoming = ReadPacketAsync(pipe);
            }

            if (stopping.Task.IsCompleted)
                return;

            var clock = Stopwatch.StartNew();
            long lastSend = -UpdateInterval;
            string lastActivity = null;
            string pendingNonce = null;
            incoming = ReadPacketAsync(pipe);

            try
            {
                while (!stopping.Task.IsCompleted)
                {
                    if (incoming.IsCompleted)
                    {
                        Reply reply = HandlePacket(pipe, await incoming);
                        if (pendingNonce != null && reply?.nonce == pendingNonce)
                        {
                            if (reply.evt == "ERROR")
                                throw new IOException("Discord rejected SET_ACTIVITY.");
                            pendingNonce = null;
                            status = "Discordに共有中です。";
                        }
                        incoming = ReadPacketAsync(pipe);
                    }

                    long elapsed = clock.ElapsedMilliseconds - lastSend;
                    if (pendingNonce != null && elapsed >= ResponseTimeout)
                        throw new TimeoutException();

                    string currentActivity = activity;
                    if (currentActivity != null && currentActivity != lastActivity &&
                        pendingNonce == null && elapsed >= UpdateInterval)
                    {
                        pendingNonce = Guid.NewGuid().ToString("N");
                        SendActivity(pipe, currentActivity, pendingNonce);
                        lastActivity = currentActivity;
                        lastSend = clock.ElapsedMilliseconds;
                    }

                    await Task.WhenAny(incoming, stopping.Task, Task.Delay(500));
                }
            }
            finally
            {
                if (stopping.Task.IsCompleted && lastActivity != null && !pipe.IsClosed)
                    SendActivity(pipe, "null", Guid.NewGuid().ToString("N"));
            }
        }

        private void SendActivity(SafeFileHandle pipe, string payload, string nonce)
        {
            string command = "{\"cmd\":\"SET_ACTIVITY\",\"nonce\":\"" + nonce + "\",\"args\":{\"pid\":" +
                processId + ",\"activity\":" + payload + "}}";
            Send(pipe, Frame, Encoding.UTF8.GetBytes(command));
        }

        private static Reply HandlePacket(SafeFileHandle pipe, Packet packet)
        {
            switch (packet.Opcode)
            {
                case Frame:
                    return JsonUtility.FromJson<Reply>(Encoding.UTF8.GetString(packet.Payload));
                case Ping:
                    Send(pipe, Pong, packet.Payload);
                    break;
                case Close:
                    throw new IOException("Discord closed the connection.");
            }

            return null;
        }

        private static void Send(SafeFileHandle pipe, int opcode, byte[] payload)
        {
            var buffer = new byte[HeaderSize + payload.Length];
            Buffer.BlockCopy(BitConverter.GetBytes(opcode), 0, buffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(payload.Length), 0, buffer, 4, 4);
            Buffer.BlockCopy(payload, 0, buffer, HeaderSize, payload.Length);
            if (!NativePipe.WriteFile(pipe, buffer, buffer.Length, out int written, IntPtr.Zero) ||
                written != buffer.Length)
                throw new IOException("Could not send a complete Discord IPC frame.");
        }

        private static async Task<Packet> ReadPacketAsync(SafeFileHandle pipe)
        {
            var header = new byte[HeaderSize];
            await ReadExactlyAsync(pipe, header);
            int length = BitConverter.ToInt32(header, 4);
            if (length < 0 || length > 64 * 1024)
                throw new IOException("Invalid Discord IPC frame length.");

            var payload = new byte[length];
            await ReadExactlyAsync(pipe, payload);
            return new Packet(BitConverter.ToInt32(header, 0), payload);
        }

        private static async Task ReadExactlyAsync(SafeFileHandle pipe, byte[] buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                if (pipe.IsClosed ||
                    !NativePipe.PeekNamedPipe(pipe, IntPtr.Zero, 0, IntPtr.Zero, out int available, IntPtr.Zero))
                    throw new IOException("Discord IPC pipe was disconnected.");

                if (available == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                int count = Math.Min(available, buffer.Length - offset);
                byte[] target = offset == 0 ? buffer : new byte[count];
                if (!NativePipe.ReadFile(pipe, target, count, out int read, IntPtr.Zero) || read == 0)
                    throw new IOException("Could not read the Discord IPC frame.");
                if (offset != 0)
                    Buffer.BlockCopy(target, 0, buffer, offset, read);
                offset += read;
            }
        }

        private static class NativePipe
        {
            internal const uint GenericReadWrite = 0xC0000000;
            internal const uint OpenExisting = 3;
            internal const uint NoWait = 1;
            // SECURITY_SQOS_PRESENT | SECURITY_IDENTIFICATION
            internal const uint IdentifyOnly = 0x00110000;

            [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, ExactSpelling = true)]
            internal static extern SafeFileHandle CreateFile(string path, uint access, uint share,
                IntPtr security, uint creation, uint flags, IntPtr template);

            [DllImport("kernel32.dll")]
            internal static extern bool SetNamedPipeHandleState(SafeFileHandle pipe, ref uint mode,
                IntPtr maxCollection, IntPtr collectionTimeout);

            [DllImport("kernel32.dll")]
            internal static extern bool PeekNamedPipe(SafeFileHandle pipe, IntPtr buffer, int length,
                IntPtr bytesRead, out int available, IntPtr bytesRemaining);

            [DllImport("kernel32.dll")]
            internal static extern bool ReadFile(SafeFileHandle pipe, [Out] byte[] buffer, int length,
                out int read, IntPtr overlapped);

            [DllImport("kernel32.dll")]
            internal static extern bool WriteFile(SafeFileHandle pipe, byte[] buffer, int length,
                out int written, IntPtr overlapped);
        }

        private readonly struct Packet
        {
            internal readonly int Opcode;
            internal readonly byte[] Payload;

            internal Packet(int opcode, byte[] payload)
            {
                Opcode = opcode;
                Payload = payload;
            }
        }

        [Serializable]
        private sealed class Activity
        {
            public string details;
            public string state;
            public Timestamps timestamps;
        }

        [Serializable]
        private sealed class Timestamps
        {
            public long start;
        }

        [Serializable]
        private sealed class Reply
        {
            public string evt = null;
            public string nonce = null;
        }
    }
}

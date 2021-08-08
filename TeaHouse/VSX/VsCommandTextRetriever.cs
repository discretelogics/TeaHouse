using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;

namespace TeaTime.VSX
{
    class VsCommandTextRetriever
    {
        private const int EmbeddedArraySizeInBytes = 512;
        private const int EmbeddedArraySizeInCharacters = (EmbeddedArraySizeInBytes / sizeof(char));
        private Guid commandGuid;

        public VsCommandTextRetriever(IOleCommandTarget cmdTarget, Guid commandGuid, uint commandId)
        {
            if (cmdTarget == null)
            {
                throw new ArgumentNullException("cmdTarget");
            }

            CommandTarget = cmdTarget;
            CommandGuid = commandGuid;
            CommandId = commandId;
        }

        private IOleCommandTarget CommandTarget
        {
            get;
            set;
        }

        public Guid CommandGuid
        {
            get
            {
                return this.commandGuid;
            }
            private set
            {
                this.commandGuid = value;
            }
        }

        public uint CommandId
        {
            get;
            set;
        }

        public string RetrieveText()
        {
            IntPtr rawOleCmdTextObj = IntPtr.Zero;
            string resultText = null;

            try
            {
                rawOleCmdTextObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OLECMDTEXT)) + EmbeddedArraySizeInBytes);

                OLECMDTEXT cmdText = new OLECMDTEXT() { cmdtextf = (uint)OLECMDTEXTF.OLECMDTEXTF_NAME, cwActual = 0, cwBuf = EmbeddedArraySizeInCharacters };
                Marshal.StructureToPtr(cmdText, rawOleCmdTextObj, false /*fDelete*/);

                OLECMD[] cmds = new OLECMD[] { new OLECMD() { cmdID = CommandId } };
                int queryStatusResult = CommandTarget.QueryStatus(ref this.commandGuid, (uint)cmds.Length, cmds, rawOleCmdTextObj);
                if (ErrorHandler.Succeeded(queryStatusResult) && VsCommandTextRetriever.IsSupportedEnabledAndVisible((OLECMDF)cmds[0].cmdf))
                {
                    int stringLength = Marshal.ReadInt32(new IntPtr(rawOleCmdTextObj.ToInt32() + Marshal.OffsetOf(typeof(OLECMDTEXT), "cwActual").ToInt32()));

                    resultText = Marshal.PtrToStringUni(new IntPtr(rawOleCmdTextObj.ToInt32() + Marshal.OffsetOf(typeof(OLECMDTEXT), "rgwz").ToInt32()), stringLength);
                }

                return resultText;
            }
            finally
            {
                if (rawOleCmdTextObj != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(rawOleCmdTextObj);
                }
            }
        }

        private static bool IsSupportedEnabledAndVisible(OLECMDF flags)
        {
            OLECMDF requiredFlags = (OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);

            return (((flags & requiredFlags) == requiredFlags) && ((flags & OLECMDF.OLECMDF_INVISIBLE) == 0));
        }
    }
}

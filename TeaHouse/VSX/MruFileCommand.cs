using System;

namespace TeaTime.VSX
{
    public class MruFileCommand
    {
        public Guid CommandGuid;
        public uint CommandId;

        public string Text;
        public string Caption
        {
            get
            {
                string s = Text.TrimStart('&');
                s = s.Substring(s.IndexOf(" "));
                s = s.Trim();
                return IOUtils.GetCompactPath(s, 48);
            }
        }

        public MruFileCommand(Guid commandGuid, uint commandId, string text)
        {
            this.CommandGuid = commandGuid;
            this.CommandId = commandId;
            this.Text = text;
        }
    }
}

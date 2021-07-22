using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using WinSCP;
using System.IO;

namespace CoStarNightlyFeedCounter
{
    public class FTPLogin
    {
        public static SessionOptions CostarFTP()
        {
            SessionOptions sessionsettings = new SessionOptions();

            sessionsettings.Protocol = Protocol.Sftp;
            sessionsettings.HostName = "download.targusinfo.com";
            sessionsettings.UserName = "costar";
            sessionsettings.Password = "CS1027ft!";
            sessionsettings.SshHostKeyFingerprint = "ssh-rsa 1024 bP0rkp9/gF5HkewPta4i6HL6kItfx6b1cLJS7+K3HmA=";

            return sessionsettings;
        }



    }
}

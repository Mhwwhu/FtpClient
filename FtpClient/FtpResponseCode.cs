using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	public enum FtpResponseCode
	{
		// 1xx - Positive Preliminary reply


		// 2xx - Positive Completion reply
		CommandOK = 200,
		FileStatus = 213,
		ServiceReadyForNewUser = 220,
		EnteringPassiveMode = 227,
		UserLoggedIn = 230,

		// 3xx - Positive Intermediate reply
		UserNameOK = 331,
		NeedAccountForLogin = 332,

		// 4xx - Transient Negative Completion reply


		// 5xx - Permanent Negative Completion reply

	}
}

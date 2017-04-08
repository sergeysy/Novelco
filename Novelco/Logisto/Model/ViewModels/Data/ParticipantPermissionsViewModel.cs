using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class ParticipantPermissionsViewModel
	{
		public IEnumerable<Action> Actions { get; set; }
		public IEnumerable<ParticipantRole> Roles { get; set; }
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout
{
	public class RequiredAttribute : PropertyAttribute
	{
		public string Message = string.Empty;

		public RequiredAttribute() { }
		
		public RequiredAttribute(string message)
		{
			this.Message = message;
		}
	}
}
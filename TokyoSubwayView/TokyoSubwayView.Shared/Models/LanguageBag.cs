using System;
using System.Collections.Generic;
using System.Text;

namespace TokyoSubwayView.Models
{
	public class LanguageBag
	{
		/// <summary>
		/// Language subtag (e.g. "en")
		/// </summary>
		public LanguageSubtag Subtag { get; set; }

		/// <summary>
		/// Language tag composed of language and culture subtags (e.g. "en-US")
		/// </summary>
		public string FullTag { get; set; }

		/// <summary>
		/// Display name (e.g. "English")
		/// </summary>
		public string DisplayName { get; set; }

		public LanguageBag(LanguageSubtag subtag, string fullTag, string displayName)
		{
			this.Subtag = subtag;
			this.FullTag = fullTag;
			this.DisplayName = displayName;
		}
	}
}
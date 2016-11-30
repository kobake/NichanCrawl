using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NichanCrawl.Models
{
	public class BoardInfo
	{
		//public string error { get; set; } // ページ取得時に何かしらエラーが発生 (NOT FOUND 等) が発生した場合はここにエラー内容が入ります
		public string url { get; set; }
		public string name { get; set; }
		public string unicode { get; set; }
	}
}

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NichanCrawl.Models
{
	public class HtmlGetter
	{
		// HTML取得
		public static async Task<string> GetHtml(string url, string encoding = null)
		{
			string exedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string cachedir = Path.GetFullPath(exedir + "\\..\\..\\..\\cache");
			string fname = Regex.Replace(url, @"[:/]", "_");
			string fpath = Path.Combine(cachedir, fname);
			if (!File.Exists(fpath))
			{
				// サーバアクセスは 0.3秒待ってから行う（サーバ過負荷防止)
				await Task.Delay(300);

				// サーバアクセス
				HttpClient client = new HttpClient();
				string _html = "";
				try
				{
					// _html = await client.GetStringAsync(url);
					HttpResponseMessage response = await client.GetAsync(url);
					if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
					{
						return "404 NOT FOUND";
					}
					// ※encoding 指定があればそれに従い読み込む
					if(encoding != null)
					{
						Encoding enc = Encoding.GetEncoding(encoding);
						using (var stream = (await response.Content.ReadAsStreamAsync()))
						using (var reader = (new StreamReader(stream, enc, true)) as TextReader)
						{
							_html = await reader.ReadToEndAsync();
						}
					}
					else
					{
						_html = await response.Content.ReadAsStringAsync();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Fetch Error:" + ex.Message);
				}
				File.WriteAllText(fpath, _html);
			}
			string html = File.ReadAllText(fpath);

			// コメント要素は全削除した結果を返す
			{
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(html);
				var comments = doc.DocumentNode.SelectNodes("//comment()");
				if(comments != null)
				{
					foreach (HtmlNode comment in comments)
					{
						comment.ParentNode.RemoveChild(comment);
					}
				}
				html = doc.DocumentNode.OuterHtml;
			}
			return html;
		}
	}
}

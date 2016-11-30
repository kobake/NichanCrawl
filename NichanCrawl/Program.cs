using CsQuery;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NichanCrawl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NichanCrawl
{
	class Program
	{
		static void Main(string[] args)
		{
			Task.Run(async () =>
			{
				var s = new Scraper();
				await s.DoAll();
			}).Wait();
		}
	}
	public class Scraper
	{
		// Key:BoardUrl Value:PageInfo
		Dictionary<string, BoardInfo> m_boards = new Dictionary<string, BoardInfo>();

		// 全巡回
		public async Task DoAll()
		{
			string menuUrl = "http://menu.2ch.net/bbsmenu.html";
			string menuHtml = await HtmlGetter.GetHtml(menuUrl, "shift_jis");
			new CQ(menuHtml).Find("a").Each(_a =>
			{
				var a = new CQ(_a);
				var url = a.Attr("href");
				var name = a.Text().Trim();
				if (Regex.IsMatch(url, "^http://[^/]+/[^/]+/$"))
				{
					Console.WriteLine("-----------");
					Console.WriteLine(url);
					Console.WriteLine(name);
					Task.Run(async () =>
					{
						await ScrapeBoard(url, name);
					}).Wait();
				}
			});

			// JSON出力
			OutputJson();
		}

		void OutputJson()
		{
			string exedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string outdir = Path.GetFullPath(exedir + "\\..\\..\\..");
			string fpath = Path.Combine(outdir, "result.json");
			string json = JsonConvert.SerializeObject(m_boards, Formatting.Indented);
			File.WriteAllText(fpath, json);
			Console.WriteLine(json);
		}

		// http://potato.2ch.net/smartphone/ のような板URL
		async Task ScrapeBoard(string boardUrl, string boardName)
		{
			// 設定テキスト読み込み -> settings
			Dictionary<string, string> settings = new Dictionary<string, string>();
			string settingUrl = boardUrl + "SETTING.TXT";
			string settingText = await HtmlGetter.GetHtml(settingUrl, "shift_jis");
			settingText = settingText.Replace("\r\n", "\n");
			var lines = settingText.Split('\n');
			foreach(var line in lines)
			{
				var kv = line.Split('=');
				if (kv.Length < 2) continue;
				settings[kv[0]] = kv[1];
			}

			// オブジェクト構築
			var boardInfo = new Models.BoardInfo
			{
				url = boardUrl,
				name = boardName
			};
			if(settings.ContainsKey("BBS_UNICODE") && settings["BBS_UNICODE"] == "pass")
			{
				boardInfo.unicode = "SUPPORTED";
			}
			m_boards[boardUrl] = boardInfo;
		}
	}
}

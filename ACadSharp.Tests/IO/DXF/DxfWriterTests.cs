﻿using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DXF;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF
{
	public class DxfWriterTests
	{
		private const string _samplesFolder = "../../../../samples";

		protected readonly ITestOutputHelper _output;

		static DxfWriterTests()
		{
			if (!Directory.Exists(_samplesFolder))
			{
				Directory.CreateDirectory(_samplesFolder);
			}
		}

		public DxfWriterTests(ITestOutputHelper output)
		{
			this._output = output;
		}

		[Fact]
		public void WriteAsciiTest()
		{
			CadDocument doc = new CadDocument();
			string path = Path.Combine(_samplesFolder, "out/out_empty_sample_ascii.dxf");

			using (var wr = new DxfWriter(path, doc, false))
			{
				wr.Write();
			}

			using (var re = new DxfReader(path, onNotification))
			{
				CadDocument readed = re.Read();
			}

			this.checkDocumentInAutocad(Path.GetFullPath(path));
		}

		[Fact(Skip = "Not implemented")]
		public void WriteBinaryTest()
		{
			CadDocument doc = new CadDocument();
			string path = Path.Combine(_samplesFolder, "out/out_empty_sample_binary.dxf");

			using (var wr = new DxfWriter(path, doc, true))
			{
				wr.Write();
			}

			//using (var re = new DxfReader(path, onNotification))
			//{
			//	CadDocument readed = re.Read();
			//}

			this.checkDocumentInAutocad(path);
		}

		protected void onNotification(object sender, NotificationEventArgs e)
		{
			_output.WriteLine(e.Message);
		}

		private void checkDocumentInAutocad(string path)
		{
			if (Environment.GetEnvironmentVariable("GITHUB_WORKFLOW") != null)
				return;

			System.Diagnostics.Process process = new System.Diagnostics.Process();

			try
			{
				process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				process.StartInfo.FileName = "\"D:\\Programs\\Autodesk\\AutoCAD 2023\\accoreconsole.exe\"";
				process.StartInfo.Arguments = $"/i \"{ Path.Combine(_samplesFolder, "sample_base/empty.dwg")}\" /l en - US";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.StandardOutputEncoding = Encoding.ASCII;

				Assert.True(process.Start());

				process.StandardInput.WriteLine($"_DXFIN");
				process.StandardInput.WriteLine($"{path}");

				string l = process.StandardOutput.ReadLine();
				bool testPassed = true;
				while (!process.StandardOutput.EndOfStream)
				{
					string li = l.Replace("\0", "");
					if (!string.IsNullOrEmpty(li))
					{
						if (li.Contains("Invalid or incomplete DXF input -- drawing discarded."))
						{
							testPassed = false;
						}

						_output.WriteLine(li);
					}

					var t = process.StandardOutput.ReadLineAsync();

					//The last line gets into an infinite loop
					if (t.Wait(1000))
					{
						l = t.Result;
					}
					else
					{
						break;
					}
				}

				if (!testPassed)
					throw new Exception("File loading with accoreconsole failed");
			}
			finally
			{
				process.Kill();
			}
		}
	}
}
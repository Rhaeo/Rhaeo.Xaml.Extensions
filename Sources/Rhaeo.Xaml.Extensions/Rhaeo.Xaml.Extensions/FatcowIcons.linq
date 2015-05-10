<Query Kind="Program">
  <Reference Relative="..\packages\Microsoft.CodeAnalysis.CSharp.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.CSharp.Desktop.dll">&lt;MyDocuments&gt;\GitHub\Rhaeo.Xaml.Extensions\Sources\Rhaeo.Xaml.Extensions\packages\Microsoft.CodeAnalysis.CSharp.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.CSharp.Desktop.dll</Reference>
  <Reference Relative="..\packages\Microsoft.CodeAnalysis.CSharp.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.CSharp.dll">&lt;MyDocuments&gt;\GitHub\Rhaeo.Xaml.Extensions\Sources\Rhaeo.Xaml.Extensions\packages\Microsoft.CodeAnalysis.CSharp.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.CSharp.dll</Reference>
  <Reference Relative="..\packages\Microsoft.CodeAnalysis.CSharp.Workspaces.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll">&lt;MyDocuments&gt;\GitHub\Rhaeo.Xaml.Extensions\Sources\Rhaeo.Xaml.Extensions\packages\Microsoft.CodeAnalysis.CSharp.Workspaces.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll</Reference>
  <Reference Relative="..\packages\Microsoft.CodeAnalysis.CSharp.Workspaces.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.CSharp.Workspaces.dll">&lt;MyDocuments&gt;\GitHub\Rhaeo.Xaml.Extensions\Sources\Rhaeo.Xaml.Extensions\packages\Microsoft.CodeAnalysis.CSharp.Workspaces.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.CSharp.Workspaces.dll</Reference>
  <Reference Relative="..\packages\Microsoft.CodeAnalysis.Common.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.Desktop.dll">&lt;MyDocuments&gt;\GitHub\Rhaeo.Xaml.Extensions\Sources\Rhaeo.Xaml.Extensions\packages\Microsoft.CodeAnalysis.Common.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.Desktop.dll</Reference>
  <Reference Relative="..\packages\Microsoft.CodeAnalysis.Common.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.dll">&lt;MyDocuments&gt;\GitHub\Rhaeo.Xaml.Extensions\Sources\Rhaeo.Xaml.Extensions\packages\Microsoft.CodeAnalysis.Common.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.dll</Reference>
  <Reference Relative="..\packages\Microsoft.CodeAnalysis.Workspaces.Common.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.Workspaces.Desktop.dll">&lt;MyDocuments&gt;\GitHub\Rhaeo.Xaml.Extensions\Sources\Rhaeo.Xaml.Extensions\packages\Microsoft.CodeAnalysis.Workspaces.Common.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.Workspaces.Desktop.dll</Reference>
  <Reference Relative="..\packages\Microsoft.CodeAnalysis.Workspaces.Common.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.Workspaces.dll">&lt;MyDocuments&gt;\GitHub\Rhaeo.Xaml.Extensions\Sources\Rhaeo.Xaml.Extensions\packages\Microsoft.CodeAnalysis.Workspaces.Common.1.0.0-rc2\lib\net45\Microsoft.CodeAnalysis.Workspaces.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\WindowsBase.dll</Reference>
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.Formatting</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.IO.Packaging</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	// http://www.fatcow.com/free-icons
	var zipStream = default(Stream);
	var directoryPath = Path.GetDirectoryName(Util.CurrentQueryPath);
	var directoryName = Path.GetFileName(directoryPath);
	var version = "3.9.2";
	var publisher = "Fatcow";
	var tab = "  ";
	
	// Download the icon set ZIP archive or use a cached local file.
	using (var webClient = new WebClient())
	{
		var zipFilePath = String.Format(@"C:\Users\Tomas.Hubelbauer\Downloads\fatcow-hosting-icons-{0}.zip", version);
		zipStream = new MemoryStream(File.Exists(zipFilePath) ? File.ReadAllBytes(zipFilePath) : webClient.DownloadData(String.Format("http://www.fatcow.com/images/fatcow-icons/fatcow-hosting-icons-{0}.zip", version)));
	}
	
	// Unpack the ZIP archive and process the sprites.
	using (var zipPackage = ZipArchive.OpenOnStream(zipStream))
	{
		var zipFiles = zipPackage.Files.ToArray();
		var sprites = 
			zipFiles.
			Where(zf => String.Equals(Path.GetExtension(zf.Name), ".png", StringComparison.OrdinalIgnoreCase)).
			ToDictionary
			(
				zf => { try { return new Bitmap(zf.GetStream()); } catch {} return null; },
				zf => zf
			).
			Where(b => b.Key != null).
			GroupBy(b => String.Format("{0}x{1}", b.Key.Width, b.Key.Height)).
			ToDictionary(g => g.Key, g => g.OrderBy(zf => String.Join(null, Path.GetFileNameWithoutExtension(zf.Value.Name).Split('_').Select(n => String.Concat(Char.ToUpper(n[0]), n.Substring(1))))).ToArray());
		
		foreach (var sprite in sprites)
		{
			var bitmaps = sprite.Value;
			var dimension = (Int32)Math.Ceiling(Math.Sqrt(bitmaps.Length));	
			var w = bitmaps.First().Key.Width;
			var h = bitmaps.First().Key.Height;
			
			var compilationUnit = SyntaxFactory.CompilationUnit();
			
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("using System.CodeDom.Compiler;");
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("namespace {0}", directoryName).AppendLine();
			stringBuilder.AppendLine("{");
			stringBuilder.AppendFormat("\t/// <summary>").AppendLine();
			stringBuilder.AppendFormat("\t/// Enumerates the indices of icons in the {0} {1} {2}x{3} sprite.", publisher, version, w, h).AppendLine();
			stringBuilder.AppendFormat("\t/// </summary>").AppendLine();
			stringBuilder.AppendFormat("\t[GeneratedCodeAttribute(\"Rhaeo sprite set generator\", \"1.0.0.0\")]").AppendLine();
			stringBuilder.AppendFormat("\tpublic enum {0}{1}x{2}Icons", publisher, w, h).AppendLine();
			stringBuilder.AppendLine("\t{");
			var bitmap = new Bitmap(dimension * w, dimension * h);
			using (var canvas = Graphics.FromImage(bitmap))
			{
				for (var bitmapIndex = 0; bitmapIndex < bitmaps.Length; bitmapIndex++)
				{
					var x = bitmapIndex % dimension;
					var y = bitmapIndex / dimension;
					var info = bitmaps[bitmapIndex].Value;
					var name = String.Join(null, Path.GetFileNameWithoutExtension(info.Name).Split('_').Select(n => String.Concat(Char.ToUpper(n[0]), n.Substring(1))));
					canvas.DrawImage(bitmaps[bitmapIndex].Key, x * w, y * h);
					stringBuilder.AppendFormat("\t\t/// <summary>").AppendLine();
					stringBuilder.AppendFormat("\t\t/// The {0} icon (#{1}), located at {2}×{3} in the {4} {5} sprite.", Path.GetFileName(info.Name), bitmapIndex, x, y, publisher, version).AppendLine();
					stringBuilder.AppendFormat("\t\t/// The original full path of this icon in the archive was {0}.", info.Name).AppendLine();
					stringBuilder.AppendFormat("\t\t/// </summary>").AppendLine();
					//stringBuilder.AppendFormat("\t\t// ReSharper disable once UnusedMember.Global").AppendLine();
					stringBuilder.AppendFormat("\t\t{0} = {1}{2}", String.Concat(Char.IsDigit(name[0]) ? "_" : null, name), bitmapIndex, bitmapIndex == bitmaps.Length - 1 ? null : ",").AppendLine();
					if (bitmapIndex != bitmaps.Length - 1)
					{
						stringBuilder.AppendLine();
					}
				}
			}
			
			bitmap.Save(Path.Combine(directoryPath, String.Format("{0}{1}x{2}Icons.generated.png", publisher, w, h)));
			stringBuilder.AppendLine("\t}");
			stringBuilder.Append("}");
			File.WriteAllText(Path.Combine(directoryPath, String.Format("{0}{1}x{2}Icons.generated.cs", publisher, w, h)), stringBuilder.Replace("\t", tab).ToString(), Encoding.UTF8);
			
			var extensionStringBuilder = new StringBuilder();
			extensionStringBuilder.AppendLine("using System;");
			extensionStringBuilder.AppendLine("using System.CodeDom.Compiler;");
			extensionStringBuilder.AppendLine("using System.Reflection;");
			extensionStringBuilder.AppendLine("using System.Windows;");
			extensionStringBuilder.AppendLine("using System.Windows.Markup;");
			extensionStringBuilder.AppendLine("using System.Windows.Media.Imaging;");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendFormat("namespace {0}", directoryName).AppendLine();
			extensionStringBuilder.AppendLine("{");
			extensionStringBuilder.AppendLine("\t/// <summary>");
			extensionStringBuilder.AppendLine("\t/// ");
			extensionStringBuilder.AppendLine("\t/// </summary>");
			extensionStringBuilder.AppendFormat("\t[GeneratedCodeAttribute(\"Rhaeo sprite set generator\", \"1.0.0.0\")]").AppendLine();
			extensionStringBuilder.AppendFormat("\tpublic sealed class {0}{1}x{2}IconsExtension", publisher, w, h).AppendLine();
			extensionStringBuilder.AppendLine("\t\t: MarkupExtension");
			extensionStringBuilder.AppendLine("\t{");
			extensionStringBuilder.AppendLine("\t\t#region Constructors");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendFormat("\t\tpublic {0}{1}x{2}IconsExtension({0}{1}x{2}Icons icon)", publisher, w, h).AppendLine();
			extensionStringBuilder.AppendLine("\t\t{");
			extensionStringBuilder.AppendLine("\t\t\tthis.Icon = icon;");
			extensionStringBuilder.AppendLine("\t\t}");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendLine("\t\t#endregion");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendLine("\t\t#region Properties");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendLine("\t\t[ConstructorArgument(\"icon\")]");
			extensionStringBuilder.AppendFormat("\t\tpublic {0}{1}x{2}Icons Icon", publisher, w, h).AppendLine();
			extensionStringBuilder.AppendLine("\t\t{");
			extensionStringBuilder.AppendLine("\t\t\tget;");
			extensionStringBuilder.AppendLine("\t\t\tset;");
			extensionStringBuilder.AppendLine("\t\t}");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendLine("\t\t#endregion");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendLine("\t\t#region Methods");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendLine("\t\tpublic override Object ProvideValue(IServiceProvider serviceProvider)");
			extensionStringBuilder.AppendLine("\t\t{");
			extensionStringBuilder.AppendFormat("\t\t\treturn {0}Icons.GetIcon(this.Icon);", publisher).AppendLine();
			extensionStringBuilder.AppendLine("\t\t}");
			extensionStringBuilder.AppendLine();
			extensionStringBuilder.AppendLine("\t\t#endregion");
			extensionStringBuilder.AppendLine("\t}");
			extensionStringBuilder.Append("}");
			
			File.WriteAllText(Path.Combine(directoryPath, String.Format("{0}{1}x{2}IconsExtension.generated.cs", publisher, w, h)), extensionStringBuilder.Replace("\t", tab).ToString(), Encoding.UTF8);
		}
		
		var apiStringBuilder = new StringBuilder();
		apiStringBuilder.AppendLine("using System;");
		apiStringBuilder.AppendLine("using System.CodeDom.Compiler;");
		apiStringBuilder.AppendLine("using System.Reflection;");
		apiStringBuilder.AppendLine("using System.Windows;");
		apiStringBuilder.AppendLine("using System.Windows.Media.Imaging;");
		apiStringBuilder.AppendLine();
		apiStringBuilder.AppendFormat("namespace {0}", directoryName).AppendLine();
		apiStringBuilder.AppendLine("{");
		apiStringBuilder.AppendLine("\t/// <summary>");
		apiStringBuilder.AppendLine("\t/// ");
		apiStringBuilder.AppendLine("\t/// </summary>");
		apiStringBuilder.AppendFormat("\t[GeneratedCodeAttribute(\"Rhaeo sprite set generator\", \"1.0.0.0\")]").AppendLine();
		apiStringBuilder.AppendFormat("\tpublic static class {0}Icons", publisher).AppendLine();
		apiStringBuilder.AppendLine("\t{");
		apiStringBuilder.AppendLine("\t\t#region Methods");
		foreach (var sprite in sprites)
		{
			var bitmaps = sprite.Value;
			var dimension = (Int32)Math.Ceiling(Math.Sqrt(bitmaps.Length));	
			var w = bitmaps.First().Key.Width;
			var h = bitmaps.First().Key.Height;
			apiStringBuilder.AppendLine();
			apiStringBuilder.AppendFormat("\t\tpublic static BitmapSource GetIcon({0}{1}Icons icon)", publisher, sprite.Key).AppendLine();
			apiStringBuilder.AppendLine("\t\t{");
			apiStringBuilder.AppendLine("\t\t\tvar bitmapImage = new BitmapImage();");
			apiStringBuilder.AppendLine("\t\t\tbitmapImage.BeginInit();");
			apiStringBuilder.AppendFormat("\t\t\tbitmapImage.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream(\"{0}.{1}{2}Icons.generated.png\");", directoryName, publisher, sprite.Key).AppendLine();
			apiStringBuilder.AppendLine("\t\t\tbitmapImage.CacheOption = BitmapCacheOption.OnLoad;");
			apiStringBuilder.AppendLine("\t\t\tbitmapImage.EndInit();");
			apiStringBuilder.AppendLine("\t\t\tbitmapImage.Freeze();");
			apiStringBuilder.AppendFormat("\t\t\treturn new CroppedBitmap(bitmapImage, new Int32Rect(((Int32)icon % {0}) * {1}, (Int32)icon / {0} * {2}, {1}, {2}));", dimension, w, h).AppendLine();
			apiStringBuilder.AppendLine("\t\t}");
		}

		apiStringBuilder.AppendLine();
		apiStringBuilder.AppendLine("\t\t#endregion");
		apiStringBuilder.AppendLine("\t}");
		apiStringBuilder.Append("}");
		File.WriteAllText(Path.Combine(directoryPath, String.Format("{0}Icons.generated.cs", publisher)), apiStringBuilder.Replace("\t", tab).ToString(), Encoding.UTF8);
	}
}

// Define other methods and classes here
/// <summary>
/// http://www.codeproject.com/Articles/209731/Csharp-use-Zip-archives-without-external-libraries
/// </summary>
class ZipArchive : IDisposable
{
  private object external;
  private ZipArchive() { }
  public enum CompressionMethodEnum { Stored, Deflated };
  public enum DeflateOptionEnum { Normal, Maximum, Fast, SuperFast };
  //...
  public static ZipArchive OpenOnFile(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read, bool streaming = false)
  {
    var type = typeof(System.IO.Packaging.Package).Assembly.GetType("MS.Internal.IO.Zip.ZipArchive");
    var meth = type.GetMethod("OpenOnFile", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    return new ZipArchive { external = meth.Invoke(null, new object[] { path, mode, access, share, streaming }) };
  }
  public static ZipArchive OpenOnStream(Stream stream, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, bool streaming = false)
  {
    var type = typeof(System.IO.Packaging.Package).Assembly.GetType("MS.Internal.IO.Zip.ZipArchive");
    var meth = type.GetMethod("OpenOnStream", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    return new ZipArchive { external = meth.Invoke(null, new object[] { stream, mode, access, streaming }) };
  }
  public ZipFileInfo AddFile(string path, CompressionMethodEnum compmeth = CompressionMethodEnum.Deflated, DeflateOptionEnum option = DeflateOptionEnum.Normal)
  {
    var type = external.GetType();
    var meth = type.GetMethod("AddFile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    var comp = type.Assembly.GetType("MS.Internal.IO.Zip.CompressionMethodEnum").GetField(compmeth.ToString()).GetValue(null);
    var opti = type.Assembly.GetType("MS.Internal.IO.Zip.DeflateOptionEnum").GetField(option.ToString()).GetValue(null);
    return new ZipFileInfo { external = meth.Invoke(external, new object[] { path, comp, opti }) };
  }
  public void DeleteFile(string name)
  {
    var meth = external.GetType().GetMethod("DeleteFile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    meth.Invoke(external, new object[] { name });
  }
  public void Dispose()
  {
    ((IDisposable)external).Dispose();
  }
  public ZipFileInfo GetFile(string name)
  {
    var meth = external.GetType().GetMethod("GetFile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    return new ZipFileInfo { external = meth.Invoke(external, new object[] { name }) };
  }

  public IEnumerable<ZipFileInfo> Files
  {
    get
    {
      var meth = external.GetType().GetMethod("GetFiles", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      var coll = meth.Invoke(external, null) as System.Collections.IEnumerable; //ZipFileInfoCollection
      foreach (var p in coll) yield return new ZipFileInfo { external = p };
    }
  }
  public IEnumerable<string> FileNames
  {
    get { return Files.Select(p => p.Name).OrderBy(p => p); }
  }

  public struct ZipFileInfo
  {
    internal object external;
    private object GetProperty(string name)
    {
      return external.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(external, null);
    }
    public override string ToString()
    {
      return Name;// base.ToString();
    }
    public string Name
    {
      get { return (string)GetProperty("Name"); }
    }
    public DateTime LastModFileDateTime
    {
      get { return (DateTime)GetProperty("LastModFileDateTime"); }
    }
    public bool FolderFlag
    {
      get { return (bool)GetProperty("FolderFlag"); }
    }
    public bool VolumeLabelFlag
    {
      get { return (bool)GetProperty("VolumeLabelFlag"); }
    }
    public object CompressionMethod
    {
      get { return GetProperty("CompressionMethod"); }
    }
    public object DeflateOption
    {
      get { return GetProperty("DeflateOption"); }
    }
    public Stream GetStream(FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read)
    {
      var meth = external.GetType().GetMethod("GetStream", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      return (Stream)meth.Invoke(external, new object[] { mode, access });
    }
  }
}
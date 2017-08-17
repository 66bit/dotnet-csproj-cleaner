using System;
using System.IO;
using System.Xml;

namespace dotnet_csproj_cleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid number of parameters. Expected filename");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File not found");
                return;
            }
            using (FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                try
                {
                    bool fileIsChanged = false;
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(fs);
                    var root = xmldoc.FirstChild;
                    var rootChilds = root.ChildNodes;
                    for (int i = 0; i < rootChilds.Count; i++)
                    {
                        if (rootChilds[i].Name == "ItemGroup")
                        {
                            for (int j = 0; j < rootChilds[i].ChildNodes.Count; j++)
                            {
                                bool isContent = rootChilds[i].ChildNodes[j].Name == "Content";
                                if (!isContent)
                                {
                                    continue;
                                }
                                bool containsInclude = rootChilds[i].ChildNodes[j].Attributes["Include"] != null;
                                if (!containsInclude)
                                {
                                    continue;
                                }
                                bool isRemoving = rootChilds[i].ChildNodes[j].Attributes["Include"].Value.TrimStart(@"\/".ToCharArray()).StartsWith("wwwroot");
                                if (isRemoving)
                                {
                                    rootChilds[i].RemoveChild((rootChilds[i].ChildNodes[j]));
                                    fileIsChanged = true;
                                    j--;
                                }
                                if (rootChilds[i].ChildNodes.Count == 0 || j == -1)
                                {
                                    root.RemoveChild(rootChilds[i]);
                                    break;
                                }
                            }
                        }
                    }
                    if (!fileIsChanged)
                    {
                        Console.WriteLine(String.Format("File {0} - OK", args[0]));
                        return;
                    }
                    using (FileStream writer = new FileStream(args[0], FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite))
                    {
                        xmldoc.Save(writer);
                    }
                    Console.WriteLine(String.Format("File {0} changed", args[0]));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

    }
}
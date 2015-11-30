using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SvgToXaml
{
    class Program
    {
        static void Main(string[] args)
        {

            List<string> filesToConvert = new List<string>();

            if (args.Length > 0)
            {
                if (args[0] == "/help")
                {
                    Console.WriteLine(">> Conversion utility from SVG simplified icons to XAML <<");
                    Console.WriteLine(">> Type SvgToXaml filename1.svg [filename2.svg]... [filenameN.svg] to convert individual icons ");
                    Console.WriteLine(">> Type SvgToXaml /all to convert all svg icons in current folder");
                    Console.WriteLine(">> Notes: Only one path allowed in svg - XAML FILES WILL BE OVERWRITEN");
                    return;
                }
                else if (args[0] == "/all")
                {
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    DirectoryInfo d = new DirectoryInfo(path);
                    d.GetFiles("*.svg").ToList().ForEach(f => filesToConvert.Add(f.Name));
                }
                else
                {
                    filesToConvert.AddRange(args.ToList());
                }
                
            }
            else
            {
                Console.WriteLine(">> Conversion utility from SVG simplified icons to XAML <<");
                Console.WriteLine(">> Type SvgToXaml /help to see usage options ");
                return;
            }


            Console.WriteLine(">> SvgToXaml > Starting convertion of SVG simplified icons to XAML <<");
            Console.WriteLine();

            filesToConvert.ForEach(s => ConvertFile(s, filesToConvert.IndexOf(s)+1, filesToConvert.Count));

            Console.WriteLine();
            Console.WriteLine(">> SvgToXaml > FINISHED!");

        }


        private static void ConvertFile(string filename, double fileCount, double total)
        {
            string svgFilename = (filename.EndsWith(".svg")) ? filename : $"{filename}.svg";
            string xamlFilename = svgFilename.Replace(".svg",".xaml");

            double percent = Math.Round((fileCount / total * 100), 0);
            XDocument svgFile;
            try
            {
                svgFile = XDocument.Load(svgFilename);
            }
            catch
            {
                Console.WriteLine($">> SvgToXaml > Error with file '{svgFilename}' > File not found or invalid!");
                return;
            }

            XElement svgRootNode = svgFile.Root;
            // check size
            XAttribute sizeAttrib = svgRootNode.Attributes().ToList().FirstOrDefault(a => a.Name == "viewBox");
            string size = (sizeAttrib != null) ? sizeAttrib.Value.Split(' ').ElementAtOrDefault(3) : null;

            //check color
            XAttribute styleAttrib = svgRootNode.Attributes().ToList().FirstOrDefault(a => a.Name == "style");
            int fillStartIndex = styleAttrib.Value.IndexOf("fill:") + 5 ;
            int searchFinishIndex = styleAttrib.Value.IndexOf(";", fillStartIndex );
            string fillAttributte = styleAttrib.Value.Substring(fillStartIndex , searchFinishIndex - fillStartIndex).Trim();
            string color = DetermineFillString(fillAttributte);

            //path
            string pathToAdd = svgRootNode.Elements().FirstOrDefault(d => d.Name.LocalName == "path")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "d")?.Value;
            if (!string.IsNullOrEmpty(pathToAdd))
            {
                try
                {
                    XDocument xamlDoc = ComposeIcon(pathToAdd, color, size);
                    xamlDoc.Save(xamlFilename);
                    string percentString = $"{ percent.ToString("N0") }%";
                    percentString = percentString.PadLeft(4);
                    Console.WriteLine($">> SvgToXaml > ({percentString}) File '{xamlFilename}' saved! ...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">> SvgToXaml > Error  > Conversion failed: {ex.Message} ");
                    File.Delete(xamlFilename);
                }
            }
            else
            {
                Console.WriteLine($">> SvgToXaml > Error with file '{svgFilename}' > No Path found!");
            }
        }

        private static XDocument ComposeIcon(string pathToAdd, string color, string size = null)
        {
            //double check for empty
            color = !string.IsNullOrEmpty(color) ? color : "White";

            string iconSize = size ?? "171";
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

            XDocument xmlIcon = new XDocument
            (
                new XElement(ns + "Viewbox" , 
                                              new XAttribute("Height",iconSize),
                                              new XAttribute("Width", iconSize),
                            new XElement(ns + "Canvas", 
                                                      new XAttribute("Height", iconSize),
                                                      new XAttribute("Width", iconSize),
                                        new XElement(ns + "Path", 
                                                                  new XAttribute("Fill", color),                                                                  
                                                                  new XAttribute("Data", pathToAdd)
                                                    )
                                        )
                            )
            );
            return xmlIcon;
        }



        private static string DetermineFillString(string colorString)
        {
            Regex regHEX = new Regex("^#([0-9a-fA-F]{2}){3}$");
            Match match = regHEX.Match(colorString);
            if (match.Success)
            {
                return colorString;
            }
            else
            {
                Regex regRGB = new Regex(@"rgb\([ ]*(?<r>[0-9]{1,3})[ ]*,[ ]*(?<g>[0-9]{1,3})[ ]*,[ ]*(?<b>[0-9]{1,3})[ ]*\)");
                match = regRGB.Match(colorString);
                if (match.Success)
                {
                    return "#" + Int32.Parse(match.Groups["r"].Value).ToString("X2") +
                                 Int32.Parse(match.Groups["g"].Value).ToString("X2") +
                                 Int32.Parse(match.Groups["b"].Value).ToString("X2");

                }
            }
            //default for white
            return "White"; 
        }

        
    }
}

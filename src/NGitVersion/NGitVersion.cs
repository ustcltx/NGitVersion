using Antlr4.StringTemplate;
using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NGitVersion
{
    public static class NGitVersion
    {
        private const string MAIN_TEMPLATE_NAME = @"MainTemplate";
        private const string MODEL_VAR = @"m";

        private static string OUTPUT_DIR = Path.Combine(BASE_DIR, "Generated");

        private static string TEMPLATE_DIR = Path.Combine(BASE_DIR, "Templates");

        // directories are relative to output project directory ${ProjectDir}\bin
        private static string BASE_DIR
        {
            get
            {
                var paths = Regex.Split(AppDomain.CurrentDomain.BaseDirectory, "bin", RegexOptions.IgnoreCase).ToList();
                paths.RemoveAt(paths.Count - 1);
                return string.Join("\\", paths);
            }
        }

        public static void Main(string[] args)
        {
            var model = new Model.Model(new Repository(GetGitRoot()));

            Directory.GetFiles(TEMPLATE_DIR, "*.stg")
                     .Select(Path.GetFullPath)
                     .ToList()
                     .ForEach(templateFile => ProcessTemplate(templateFile, model));
        }

        private static string BuildTargetFileName(string templateFile)
        {
            string outputFileName = Path.GetFileNameWithoutExtension(templateFile);
            return Path.GetFullPath(Path.Combine(OUTPUT_DIR, outputFileName));
        }

        private static string GetGitRoot()
        {
            const string gitDir = ".git";
            string hierarchy = @"." + Path.DirectorySeparatorChar;
            while (true)
            {
                bool exists = Directory.Exists(hierarchy + gitDir);
                if (exists)
                    return hierarchy;

                hierarchy = hierarchy + @".." + Path.DirectorySeparatorChar;

                if (!Directory.Exists(hierarchy))
                    throw new ApplicationException("No .git folder found in the current path hierarchy");
            }
        }

        private static void ProcessTemplate(string templateFile, Model.Model model)
        {
            string targetFile = BuildTargetFileName(templateFile);

            Console.WriteLine("\ttemplate: " + templateFile);
            try
            {
                Template template = new TemplateGroupFile(templateFile)
                    .GetInstanceOf(MAIN_TEMPLATE_NAME);

                template.Add(MODEL_VAR, model);

                File.WriteAllText(targetFile, template.Render());
            }
            finally
            {
                Console.WriteLine("\toutput:   " + targetFile);
            }
        }
    }
}
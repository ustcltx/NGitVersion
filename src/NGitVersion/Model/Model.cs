using LibGit2Sharp;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NGitVersion.Model
{
    public class Model
    {
        private readonly IRepository mRepository;
        private readonly Lazy<string> mBranch;
        private readonly Lazy<string> mHasLocalChange;
        private readonly Lazy<CompanyInfo> mCompanyInfo;
        private readonly Lazy<VersionStruct> mVersion;
        private readonly Lazy<string> mVersionCode;

        private

        struct CompanyInfo
        {
            public string Company;
            public string Copyright;
        }

        private struct VersionStruct
        {
            public string Major;
            public string Minor;
            public string Build;
            public string Revision;
            public string ShortHash;
        }

        private VersionStruct ParseGitVersion(string version)
        {
            var s = new VersionStruct();
            string[] parts = version.Split('-');
            if (parts.Length != 3) return s;
            string[] vs = parts[0].Split('.');
            if (vs.Length != 3) return s;
            s.Major = vs[0];
            s.Minor = vs[1];
            s.Build = vs[2];
            s.Revision = parts[1];
            s.ShortHash = parts[2].TrimStart('g');
            return s;
        }

        private CompanyInfo CreateCompanyInfo()
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            AssemblyCopyrightAttribute asmcpr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute));
            AssemblyCompanyAttribute asmcpn = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute));

            string Copyright = (asmcpr == null) ? string.Empty : asmcpr.Copyright;
            string Company = (asmcpn == null) ? string.Empty : asmcpn.Company;

            return new CompanyInfo()
            {
                Copyright = Copyright,
                Company = Company
            };
        }

        public Model(IRepository repository)
        {
            mRepository = repository;
            mCompanyInfo = new Lazy<CompanyInfo>(() => CreateCompanyInfo());
            mVersion = new Lazy<VersionStruct>(() => ParseGitVersion(mRepository.Describe(mRepository.Commits.First())));
            mVersionCode = new Lazy<string>(() => mRepository.Commits.Count().ToString());
            mBranch = new Lazy<string>(() => mRepository.Head.CanonicalName);
            mHasLocalChange = new Lazy<string>(() => mRepository.RetrieveStatus().IsDirty.ToString(CultureInfo.InvariantCulture));
#if DEBUG
            BuildConfig = "DEBUG";
#else
            BuildConfig = "RELEASE";
#endif
        }

        public string Company { get { return mCompanyInfo.Value.Company; } }
        public string Product { get { return "TODO Product"; } }
        public string Copyright { get { return mCompanyInfo.Value.Copyright; } }
        public string Trademark { get { return "TODO Trademark"; } }
        public string Culture { get { return ""; } }

        public string Major { get { return mVersion.Value.Major; } }
        public string Minor { get { return mVersion.Value.Minor; } }
        public string Build { get { return mVersion.Value.Build; } }

        public string Revision { get { return mVersion.Value.Revision; } }
        public string ShortHash { get { return mVersion.Value.ShortHash; } }
        public string VersionCode { get { return mVersionCode.Value; } }
        public string Branch { get { return mBranch.Value; } }
        public string HasLocalChange { get { return mHasLocalChange.Value; } }
        public string BuildConfig { get; set; }
    }
}
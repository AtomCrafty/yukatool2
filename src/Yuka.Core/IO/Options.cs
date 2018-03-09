
namespace Yuka.IO {
	public static class Options {

		public static bool YkgConvertFrmToAniOnExport = true;
		public static bool YkgMergeAlphaChannelOnExport = true;

		public static byte YksScriptDataXorKey = 0xAA;
		public static bool YksEncryptScriptDataOnExport = true;

		public static bool YkdExternalizeStrings = true;
		public static string[] YkdLineMethods = { "StrOut", "StrOutNW" };
		public static string[] YkdNameMethods = { "NameOut", "StrOutNWC" };
		public static string[] YkdResetSpeakerMethods = { "PF" };

		public static bool CsvStrict = true;
		public static string CsvIdColumnName = "ID";
		public static string CsvSpeakerColumnName = "Speaker";
		public static string CsvCommentColumnName = "Comment";
		public static string CsvFallbackColumnName = "Original";
		public static string CsvTextColumnNameRegex = @"^\[(.*)\]$"; //"(.*)"; 
		public static string CsvLinePrefix = "L";
		public static string CsvNamePrefix = "N";
		public static string CsvOtherPrefix = "S";
		public static string CsvIgnorePrefix = "#";
		public static string CsvSkipTextField = ".";
		public static string[] CsvGeneratedColumns = { "ID", "Speaker", "Original", "[Translation]", "[TLC]", "[Edit]", "[QC]", "Comment" };
	}
}

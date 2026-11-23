using System.IO;
using SomaFrameQD.Framework;

namespace SomaFrameQD.Modules.Output
{
	public interface IOutput : ITool
	{
		TextWriter Writer { get; }
		void AppendLine(string text);
		void Append(string text);
		void Clear();
	}
}
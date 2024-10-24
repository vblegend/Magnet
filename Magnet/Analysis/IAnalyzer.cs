using System;


namespace Magnet.Analysis
{
    /// <summary>
    /// reusable script analyzer
    /// </summary>
    public interface IAnalyzer
    {
        /// <summary>
        /// Analyzer is connected to MagnetScript
        /// </summary>
        /// <param name="magnet"></param>
        void Connect(MagnetScript magnet);

        /// <summary>
        /// Analyzer disconnects from MagnetScript
        /// </summary>
        void Disconnect(MagnetScript magnet);

    }
}

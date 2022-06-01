using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.FeatureProcessing
{
    public interface IFeatureProcessor
    {
        IEnumerable<FeatureRecord> ExtractFeatures(IEnumerable<RawDataSnapshot> input, List<FeatureTypes> featureTypes);
        IEnumerable<RawDataSnapshot> MergeFeatures();
    }
}

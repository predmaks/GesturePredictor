using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.FeatureProcessing
{
    public interface IFeatureProcessor
    {
        IEnumerable<FeatureRecord> ExtractFeatures(IEnumerable<GestureDataSnapshot> input, List<FeatureTypes> featureTypes);
        IEnumerable<FeatureTransposed> MergeFeatures(IEnumerable<FeatureRecord> input1, IEnumerable<FeatureRecord> input2);
    }
}

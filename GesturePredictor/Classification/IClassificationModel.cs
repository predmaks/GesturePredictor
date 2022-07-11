using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor
{
    public interface IClassificationModel
    {
        public void Create();
        public void Load<TInput>(string filePath);
        public void Save(string filePath);
    }
}

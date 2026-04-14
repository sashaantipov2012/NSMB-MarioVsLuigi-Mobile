using Quantum;
using System;

namespace NSMB.Quantum {

    public interface IQuantumBakeStep {
        virtual int Order => 0;
        void OnBake(QuantumMapData data, VersusStageData stage);
    }

    public class QuantumBakeException : Exception {
        public QuantumBakeException(string message) : base(message) { }
    }
}
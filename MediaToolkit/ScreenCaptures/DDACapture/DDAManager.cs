using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using MediaToolkit.Logging;


namespace MediaToolkit.ScreenCaptures
{

    public class DDAOutputManager
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.ScreenCaptures");

        private Dictionary<int, Dictionary<int, DDAOutput>> OutputDict = new Dictionary<int, Dictionary<int, DDAOutput>>();
        private object syncObj = new object();

        public DDAOutput GetOutput(int adapterIndex, int outputIndex)
        {
            logger.Debug("DDAOutputManager::GetOutput(...) " + adapterIndex + " " + outputIndex);
            DDAOutput output = null;
            lock (syncObj)
            {
                if (OutputDict.ContainsKey(adapterIndex))
                {
                    var outputs = OutputDict[adapterIndex];
                    if (outputs.ContainsKey(outputIndex))
                    {
                        logger.Verb("Getting exist DDAOutput");

                        output = outputs[outputIndex];
                    }
                    else
                    {
                        logger.Verb("Create new DDAOutput");

                        output = new DDAOutput();
                        output.Init(adapterIndex, outputIndex);
                        outputs[outputIndex] = output;
                    }
                }
                else
                {
                    logger.Verb("Create new outputs dict...");

                    var outputs = new Dictionary<int, DDAOutput>();
                    output = new DDAOutput();
                    output.Init(adapterIndex, outputIndex);
                    outputs[outputIndex] = output;

                    OutputDict[adapterIndex] = outputs;
                }

            }

            return output;
        }



        public void ReleaseOutput(DDAOutput output)
        {
            var adapterIndex = output.AdapterIndex;
            var outputIndex = output.OutputIndex;

            ReleaseOutput(adapterIndex, outputIndex);
        }

        public void ReleaseOutput(int adapterIndex, int outputIndex)
        {
            lock (syncObj)
            {
                if (OutputDict.ContainsKey(adapterIndex))
                {
                    var outputs = OutputDict[adapterIndex];
                    if (outputs.ContainsKey(outputIndex))
                    {
                        var output = outputs[outputIndex];
                        if (output != null)
                        {
                            var activates = output.Deactivate();
                            if (activates <= 0)
                            {
                                output.Close();
                                output = null;

                                outputs.Remove(outputIndex);
                            }

                        }
                    }
                }
            }

        }

        public void Dispose()
        {
            logger.Debug("DDAOutputManager::Dispose()");

            lock (syncObj)
            {
                foreach (var adapter in OutputDict.Keys)
                {
                    var outputs = OutputDict[adapter];
                    foreach (var output in outputs.Values)
                    {
                        output.Close();
                    }
                }

                OutputDict.Clear();
            }

        }
    }


}

﻿using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class GCodeData : FanucMultiStrategyCollector
    {
        public GCodeData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.GCodeBlocks), "gcode");
        }
        
        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            await strategy.Peel("gcode",
                await strategy.SetNativeKeyed($"blkcount", 
                    await strategy.Platform.RdBlkCountAsync()),
                await strategy.SetNativeKeyed($"actpt", 
                    await strategy.Platform.RdActPtAsync()),
                await strategy.SetNativeKeyed($"execprog", 
                    await strategy.Platform.RdExecProgAsync(256)));
        }
    }
}
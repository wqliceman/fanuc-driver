﻿using System.Dynamic;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class OpMsgsStateful : OpMsgs
{
    public OpMsgsStateful(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers,
        name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        /*
            nativeInputs
                0: current messages
                1: previous messages
            
            additionalInputs
                0: currentPath
        */
        if (nativeInputs[0].success == true)
        {
            var path = additionalInputs[0];
            var currentInput = nativeInputs[0];
            
            // message list from NC
            List<dynamic> currentMessageList = GetMessageListFromMessages(currentInput, path);
            // message states from previous sweep
            List<dynamic> previousStatesList = LastChangedValue == null ? new List<dynamic>() : LastChangedValue.messages;
            // message states for this sweep
            List<dynamic> currentStatesList = new List<dynamic>();
            
            foreach (var state in previousStatesList)
            {
                dynamic newState = new ExpandoObject();
                
                newState.id = state.id;
                newState.time_triggered = state.time_triggered;
                newState.time_cleared = state.time_cleared;
                newState.time_elapsed = state.time_elapsed;
                newState.is_triggered = state.is_triggered;
                newState.trigger_count = state.trigger_count;
                // base message
                newState.path = state.path;
                newState.type = state.type;
                newState.number = state.number;
                newState.message = state.message;
                
                // increment elapsed time
                if (state.is_triggered == true)
                {
                    newState.time_elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - state.time_triggered;
                    
                    // alarm was removed from NC
                    if (!currentMessageList.Exists(msg => state.id == $"{msg.number:D4}"))
                    {
                        newState.time_cleared = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        newState.is_triggered = false;
                    }
                }
                
                currentStatesList.Add(newState);
            }
            
            // iterate NC alarms
            foreach (var msg in currentMessageList)
            {
                var id = $"{msg.number:D4}";
                var state = currentStatesList.FirstOrDefault(state => state.id == id, null);
                
                // new trigger
                if (state == null)
                {
                    dynamic newState = new ExpandoObject();
                    
                    newState.id = id;
                    newState.time_triggered = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    newState.time_cleared = 0;
                    newState.time_elapsed = 0;
                    newState.is_triggered = true;
                    newState.trigger_count = 1;
                    // base message
                    newState.path = msg.path;
                    newState.type = msg.type;
                    newState.number = msg.number;
                    newState.message = msg.message;
                    currentStatesList.Add(newState);
                }
                // re-trigger
                else if (state.is_triggered == false)
                {
                    state.time_triggered = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    state.time_cleared = 0;
                    state.time_elapsed = 0;
                    state.is_triggered = true;
                    state.trigger_count = state.trigger_count + 1;
                }
            }
            
            var currentValue = new
            {
                messages = currentStatesList
            };

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (currentValue.IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }

        return new {veneer = this};
    }
}
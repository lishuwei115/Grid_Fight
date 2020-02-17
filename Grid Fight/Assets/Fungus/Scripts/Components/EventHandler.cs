// This code is part of the Fungus library (http://fungusgames.com) maintained by Chris Gregan (http://twitter.com/gofungus).
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;

namespace Fungus
{
    /// <summary>
    /// Attribute class for Fungus event handlers.
    /// </summary>
    public class EventHandlerInfoAttribute : Attribute
    {
        public EventHandlerInfoAttribute(string category, string eventHandlerName, string helpText)
        {
            this.Category = category;
            this.EventHandlerName = eventHandlerName;
            this.HelpText = helpText;
        }
        
        public string Category { get; set; }
        public string EventHandlerName { get; set; }
        public string HelpText { get; set; }
    }

    /// <summary>
    /// A Block may have an associated Event Handler which starts executing commands when
    /// a specific event occurs. 
    /// To create a custom Event Handler, simply subclass EventHandler and call the ExecuteBlock() method
    /// when the event occurs. 
    /// Add an EventHandlerInfo attibute and your new EventHandler class will automatically appear in the
    /// 'Execute On Event' dropdown menu when a block is selected.
    /// </summary>
    [RequireComponent(typeof(Block))]
    [RequireComponent(typeof(Flowchart))]
    [AddComponentMenu("")]
    public class EventHandler : MonoBehaviour
    {   
        [HideInInspector]
        [FormerlySerializedAs("parentSequence")]
        [SerializeField] protected Block parentBlock;

        #region Public members

        /// <summary>
        /// The parent Block which owns this Event Handler.
        /// </summary>
        public virtual Block ParentBlock { get { return parentBlock; } set { parentBlock = value; } }

        /// <summary>
        /// The Event Handler should call this method when the event is detected to start executing the Block.
        /// </summary>
        public virtual bool ExecuteBlock()
        {
            if (ParentBlock == null)
            {
                return false;
            }

            if (ParentBlock._EventHandler != this)
            {
                return false;
            }

            var flowchart = ParentBlock.GetFlowchart();

            //if somehow the flowchart is invalid or has been disabled we don't want to continue
            if(flowchart == null || !flowchart.isActiveAndEnabled)
            {
                return false;
            }

            // Auto-follow the executing block if none is currently selected
            if (flowchart.SelectedBlock == null)
            {
                flowchart.SelectedBlock = ParentBlock;
            }

            return flowchart.ExecuteBlock(ParentBlock);
        }

        /// <summary>
        /// Returns custom summary text for the event handler.
        /// </summary>
        public virtual string GetSummary()
        {
            return "";
        }

        public void BlockTriggered(string blockName)
        {
            Debug.Log("Added start execution for " + blockName);
            Block block = ParentBlock;
            if (block != null)
            {
                if (block.BlockName == blockName)
                {
                    block.StartExecution();
                }
                    Debug.Log(block.BlockName);
            }
        }

        public IEnumerator BlockTriggeredWithCallBack(string blockName)
        {
            Flowchart flowchart = ParentBlock.GetFlowchart();
            Block block = flowchart.FindBlock(blockName);
            if (block != null)
            {
                yield return block.WaitForExecutionComplete();
                Debug.Log(block.BlockName);
            }
        }

        #endregion
    }
}

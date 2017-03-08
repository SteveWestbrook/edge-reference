/**
 * Edge reference tracker
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

'use strict';

const edge = require('edge');

module.exports = collectorFactory;

/**
 * Removes a reference in .NET code to an object that is no longer used 
 * by JavaScript code.  If no JS references remain, the object is reclaimed.
 * 
 * @param input {long} The ID to be removed from the reference collection.
 */
var Unregister = edge.func(function() {/*
  #r "./bin/EdgeReference.dll"

  using EdgeReference;
  using System.Threading.Tasks;

  public class Startup {
    public async Task<object> Invoke(object input) {
      long referenceId;

      if (input is int) {
        referenceId = (long)(int)input;
      } else {
        referenceId = (long)input;
      }

      ReferenceManager.Instance.RemoveReference(referenceId);
      return null;
    }
  }
*/});


/**
 * Generates and returns a callback that will clean up .NET references to a 
 * garbage-collected JavaScript object.
 * 
 * @param referenceId {Number} The id of the object to be reclaimed when the 
 * returned callback is executed.
 */
function collectorFactory(referenceId) {
  return () => {
    referenceCollected(referenceId);
  };
}

/**
 * Called when a reference is reclaimed by the garbage collector.
 * 
 * @param id {Number} The id of the object to be reclaimed.
 */
function referenceCollected(id) {
  // No callback needed - just kick off the dereg process.
  Unregister(id);
}

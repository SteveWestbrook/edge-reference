/**
 * Edge reference tracker
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

'use strict';

const edge = require('edge');

module.exports = collectorFactory;

/**
 * .NET function to remove a reference
 * @param input {long} The ID to be removed from the reference collection.
 */
var Unregister = edge.func(function() {/*
  #r "EdgeReference.dll"

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
 * Generates a callback that will clean up .NET references to a 
 * garbage-collected JavaScript object.
 */
function collectorFactory(referenceId) {
  return () => {
    referenceCollected(referenceId);
  };
}

/**
 * Called when a reference is reclaimed by the garbage collector.
 */
function referenceCollected(id) {
  console.log('gc underway for %d', id);

  // No callback needed - just kick off the dereg process.
  Unregister(id);
}



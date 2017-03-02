/**
 * EdgeReference class
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

'use strict';

const edge = require('edge');
const weak = require('weak');
const tracker = require('./edge-track.js');

var refs = [];

/**
 * A base class for proxies to .NET objects.  Contains some useful tools to 
 * make references match one another.
 */
class EdgeReference {
  /*
   * @param constructorArgs An associative array containing the parameters to 
   *        be passed to the constructor with matching parameters.
   */
  constructor(referenceId, referenceConstructor, constructorArgs) {
    if (referenceId) {
      this._referenceId = referenceId;
    } else {
      this._referenceId = referenceConstructor(constructorArgs, true);
    }

    EdgeReference.register(this);
  }

  /**
   * Gets the reference ID for this instance.  This value is used to retrieve 
   * the underlying .NET object that corresponds to this instance.
   */
  get _referenceId() {
    return this.__referenceId;
  }

  /**
   * Assigns a new ID to this reference.  Note that this is equivalent to 
   * changing the object represented by this instance.
   */
  set _referenceId(value) {
    this.__referenceId = value;
  }

  /**
   * Begins tracking a reference in order to allow tracking of when it is 
   * garbage collected.
   * 
   * @param toRegister {EdgeReference} The object to be tracked.
   */
  static register(toRegister) {
    // The tracker here has to be generated in order to store a reference to 
    // the object's _referenceId, since after GC the reference is of course 
    // gone.
    weak(toRegister, tracker(toRegister._referenceId));
  }

  /**
   * Calls the specified function and either returns its converted value or 
   * calls the specified callback with the converted value included.
   * 
   * @param callTarget {Function} The function to be called.  Takes one 
   *        argument and an optional callback.
   * @param params {Object} An argument to be given to the callTarget.
   * @param [conversionType] {Function} An optional type.  Takes a numeric 
   *        value and wraps it in a proxy.
   * @param [callback] {Function} A callback to be triggered when the call 
   *        target has completed.  If not supplied, the operation will run 
   *        synchronously.
   */
  static callbackOrReturn(callTarget, params, conversionType, callback) {
    // Create a callback wrapper that will create a proxy to an object if one 
    // is returned.
    var wrapper = null;
    if (conversionType) {
      wrapper = (err, result) => {
        if (result) {
          result = new conversionType(result);
        } else {
          result = null;
        }

        callback(err, result);
      }
    }

    // Call asynchronously if possible
    if (callback && callback instanceof Function) {
      callTarget(params, wrapper || callback);
    } else {
      // Call synchronously if no callback is given
      var result = callTarget(params, true);

      if (conversionType) {
        if (result) {
          result = new conversionType(result);
        } else {
          result = null;
        }
      }

      return result;
    }
  }

  /**
   * Checks the provided .NET proxy object against this one.
   * @param against {EdgeReference} A .NET proxy.  If it represents the same 
   *        underlying .NET object, this function will return true.
   * @return {Boolean} If the parameter provided is non-null and has the same 
   *         _referenceId property as this instance, returns true.
   */
  referenceEquals(against) {
    return against &&
      against._referenceId === this._referenceId;
  }

}

module.exports = EdgeReference;


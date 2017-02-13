/**
 * index of is-valid-var-name module
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

'use strict';

const EdgeReference = require('./src/edge-reference.js');

module.exports = EdgeReference;

const edge = require('edge');
const generator = require('./src/proxy-generator.js');
const path = require('path');

      generator.generateProxy(
        'DotNetTests.TestType1',
        path.join(__dirname, 'DotNetTests.dll'),
        path.resolve(__dirname, '../junk'),
        done);



/* eslint-disable */

import React from 'react'
import Highcharts from 'highcharts'
import StockModule from 'highcharts/modules/stock'
import MapModule from 'highcharts/modules/map'
import Heatmap from 'highcharts/modules/heatmap'
import Boost from 'highcharts/modules/boost'
StockModule(Highcharts)
MapModule(Highcharts)
Heatmap(Highcharts)
Boost(Highcharts)

import HighchartsReact from 'highcharts-react-official';
require("./_highcharts.scss");

window.Highcharts = window.Highcharts || Highcharts;

let bpOptions = {

  chart: {
      type: 'boxplot'
  },

  title: {
      text: 'Highcharts Box Plot Example'
  },

  xAxis: {
      categories: ['1', '2', '3', '4', '5'],
      title: {
          text: 'Experiment No.'
      }
  },

  yAxis: {
      title: {
          text: 'Observations'
      },
  },

  series: [{
      name: 'Observations',
      data: [
          [760, 801, 848, 895, 965],
          [733, 853, 939, 980, 1080],
          [714, 762, 817, 870, 918],
          [724, 802, 806, 871, 950],
          [834, 836, 864, 882, 910]
      ],
      //tooltip: {
      //    headerFormat: '<em>Experiment No {point.key}</em><br/>'
      //}
  }]

}

function objectWithFields(o, fieldNames) {
  let res = {}
  for (let k in o) {
    //console.log("k=",k, fieldNames.includes(k), fieldNames);
    if (!o.hasOwnProperty(k))
      continue;
    if (fieldNames.includes(k)) {
      res[k] = o[k];
    }
  }
  return res;
}

function genericArray(array, keepFields) {
  let result = [];
  for (let x of array) {
    result.push(objectWithFields(x, keepFields));
  }
  //console.log("mapped as:", result)
  return result;
}

function loadScript(src, onLoad) {
    var head = document.getElementsByTagName('head')[0];
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.onload = onLoad;
    script.src = src;
    head.appendChild(script);
}

/// workaround to replace labelFormatter with labelFormatter(this)
function wrapLabelFormatterWithThis({legend, ...options}) {
    if (!!legend) {
        let labelFormatter = legend.labelFormatter;
        if (!!labelFormatter) {
            function lfmt() {return labelFormatter(this);}
            legend.labelFormatter = lfmt;
        }
        return {legend, ...options}
    } else {
        return options;
    }

}

function renderChart(options) {
  options = wrapLabelFormatterWithThis(options);
  return React.createElement(HighchartsReact, {
    highcharts: Highcharts,
    containerProps: {style: {height:"100%"}},
    options: {...options},
  }, null);
}

function renderChartFromWindow(options) {
  options = wrapLabelFormatterWithThis(options);
  return React.createElement(
    HighchartsReact,
    {
      highcharts: Highcharts,
      constructorType: 'stockChart',
      containerProps: { style: { height: '100%' }},
      options: { ...options },
    },
    null
  )
}

function renderMap(options) {
    options = wrapLabelFormatterWithThis(options);
    return React.createElement(HighchartsReact, {
        highcharts: Highcharts,
        constructorType: "mapChart",
        containerProps: {style: {height:"100%"}},
        options: {...options }
    }, null);
}

function sparklineChart (documentElementId, options) {
  Highcharts.chart(documentElementId, options)
}

export {
    genericArray,
    loadScript,
    renderChart,
    renderChartFromWindow,
    renderMap,
    sparklineChart,
    Highcharts
}

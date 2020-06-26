/* eslint-disable */

// sorry for all the junk inhere

import React from 'react'
//import * as Highcharts from 'highcharts/highcharts.js'
import * as Highcharts from 'highcharts/highstock.js'
require("highcharts/highcharts-more")(Highcharts);
require("highcharts/modules/map.js")(Highcharts);

import HighchartsReact from 'highcharts-react-official';

//require("highcharts/css/highcharts.scss");
require("./_highcharts.scss");

import i18n from "../i18n"
import moment from 'moment'

// TODO: this shouldn't be hardcoded
moment.locale('sl')

window.Highcharts = window.Highcharts || Highcharts;

Highcharts.lo

function setHighchartsOptions (highcharts) {
    highcharts.setOptions({
        global: {
            useUTC: false
        },
        lang: {
            loading: i18n.t("charts.common.loading"),
            months: i18n.t("month"),
            shortMonths: i18n.t("shortMonth"),
            weekdays: i18n.t("weekday"),
            rangeSelectorFrom: i18n.t("charts.common.from"),
            rangeSelectorTo: i18n.t("charts.common.to"),
            rangeSelectorZoom: i18n.t("charts.common.zoom"),
            resetZoom: i18n.t("charts.common.resetZoom"),
            resetZoomTitle: i18n.t("charts.common.resetZoomTitle"),
            thousandsSep: i18n.t("charts.common.thousandsSep"),
            decimalPoint: i18n.t("charts.common.decimalPoint"),
        }
    });
};

(function(H) {
  H.Legend.prototype.setItemEvents = function(item, legendItem, useHTML) {
      let legend = this;
      let boxWrapper = legend.chart.renderer.boxWrapper;
      let activeClass = 'highcharts-legend-' + (item.series ? 'point' : 'series') + '-active';
      function hasLinkedSeries(item) {
           return ((item.linkedSeries && item.linkedSeries.length) ? true : false);
      };
      function getLinkedSeries(item, excludeOrigin=false) {
        let seen = [];
        let explore = [item];
        while (explore.length > 0) {
            let item = explore.pop();
            seen.push(item);
            let peers = item.linkedSeries;
            if (peers && peers.length) {
                for (let peer of peers) {
                    if (!seen.includes(peer) && !explore.includes(peer)) {
                        explore.push(peer);
                    }
                }
            }
        }
        if (excludeOrigin) {
            seen.unshift(); // remove first item
        }
        return seen;
      };
      function setLinkedSeriesState(item, state) {
          item.linkedSeries.forEach(function(elem) {
              elem.setState(state)
          })
      };

  // Set the events on the item group, or in case of useHTML, the item itself (#1249)
  (useHTML ? legendItem : item.legendGroup).on('mouseover', function () {
      if (item.visible) {
          let peers = getLinkedSeries(item);
          //console.log("peers:", peers.length, peers);
          for (let peer of peers) {
            peer.chart.hoverSeries = peer;
            peer.setState('hover');
            //peer.chart.setState('hover');
          //item.setState('hover');
          }
          //console.log("hover", item, item.linkedSeries, hasLinkedSeries(item));

          // Add hover state to linked series
          //if (hasLinkedSeries(item)) {

          //    setLinkedSeriesState(item, 'hover')
          //}
          // A CSS class to dim or hide other than the hovered series
          boxWrapper.addClass(activeClass);

          /*= if (build.classic) { =*/
          legendItem.css(legend.options.itemHoverStyle);
          /*= } =*/
      }
  })
  .on('mouseout', function () {
      /*= if (build.classic) { =*/
      legendItem.css(H.merge(item.visible ? legend.itemStyle : legend.itemHiddenStyle));
      /*= } =*/

      // A CSS class to dim or hide other than the hovered series
      boxWrapper.removeClass(activeClass);

      let peers = getLinkedSeries(item);
      for (let peer of peers) peer.setState();

      // Remove hover state from linked series
      //if(hasLinkedSeries(item)) {
      //    setLinkedSeriesState(item)
      //}

      item.setState();


  })
  .on('click', function (event) {
      var strLegendItemClick = 'legendItemClick',
          fnLegendItemClick = function () {
              if (item.setVisible) {
                  item.setVisible();
              }
          };

      // Pass over the click/touch event. #4.
      event = {
          browserEvent: event
      };

      // click the name or symbol
      if (item.firePointEvent) { // point
          item.firePointEvent(strLegendItemClick, event, fnLegendItemClick);
      } else {
          H.fireEvent(item, strLegendItemClick, event, fnLegendItemClick);
      }
  });
  };
})(Highcharts);

/*
// add custom axes
(function (H) {
    H.addEvent(H.Axis, 'init', function (e) {
        //this.allowNegativeLog = e.userOptions.allowNegativeLog;
        this.allowNegativeLog = true;
    });

    // Override conversions
    H.wrap(H.Axis.prototype, 'log2lin', function (proceed, num) {

        //if (!this.allowNegativeLog) {
        //    return proceed.call(this, num);
        //}
        //return Math.asinh(num);
        return Math.sqrt(num);
    });
    H.wrap(H.Axis.prototype, 'lin2log', function (proceed, num) {
        //if (!this.allowNegativeLog) {
        //    return proceed.call(this, num);
        //}
        //return Math.sinh(num);
        return num*num;
    });
}(Highcharts));
*/

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
  setHighchartsOptions(Highcharts);
  return React.createElement(HighchartsReact, {
    highcharts: Highcharts,
    containerProps: {style: {height:"100%"}},
    options: {...options},
  }, null);
}

function renderChartFromWindow(options) {
  options = wrapLabelFormatterWithThis(options);
  setHighchartsOptions(window.Highcharts);
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
    setHighchartsOptions(Highcharts);
    return React.createElement(HighchartsReact, {
        highcharts: Highcharts,
        constructorType: "mapChart",
        containerProps: {style: {height:"100%"}},
        options: {...options, credits: {enabled: false}}
    }, null);
}

export {
    genericArray,
    loadScript,
    renderChart,
    renderChartFromWindow,
    renderMap
}

import React from 'react'
//import * as Highcharts from 'highcharts/highcharts.js'
import * as Highcharts from 'highcharts/highstock.js'
require("highcharts/highcharts-more")(Highcharts);
require("highcharts/modules/map.js")(Highcharts);
//require('highcharts/modules/timeline')(Highcharts);
import HighchartsReact from 'highcharts-react-official';

//require("highcharts/css/highcharts.scss");
require("./_highcharts.scss");

window.Highcharts = window.Highcharts || Highcharts;

Highcharts.setOptions({
	global: { 
		useUTC: false // true by default
	}
});

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
  //console.log("yy", genericArray (options.yAxis, ["title"]));
  //options["yAxis"] = genericArray (options.yAxis, ["title"])
  //options["yAxis"] = [{title: {name:"aaa"}}, {title: {name:"bbb"}}]
  //options["yAxis"] = options.yAxis.map((item) => objectWithFields(item, ["title"]));
  //options = bpOptions;
  options = wrapLabelFormatterWithThis(options);
  //console.log("chart options:", options);
  return <HighchartsReact
    highcharts={Highcharts}
    options={{...options, credits: {enabled: false}}}
  />
}

export {
    renderChart,
}
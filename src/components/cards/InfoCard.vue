<template>
  <div class="hp-card-holder" :class="['cardtype-'+cardName]">
    <div class="hp-card" v-if="!loading">
      <div class="card-title">{{ cardData.title }}</div>
      <div class="card-number">
        <span>{{ cardData.value | number }}<span class="card-number-extra" v-if="valueExtraText"> {{ valueExtraText | percent }}</span></span>
        <div
          v-if="cardData.diffPercentage"
          class="card-percentage-diff"
          :class="percentageDirection"
        >
          {{ cardData.diffPercentage | percent }}
        </div>
      </div>
      <div :id="cardName" class="card-diff">
        <div v-if="cardData.subTitle">
          <span class="card-note">{{ cardData.subTitle }} <span>{{ subLabelExtraText }}</span></span>
        </div>
        <div
          v-if="cardData.subValues && cardData.subValues.in"
          class="card-diff-item in"
        >
          <div class="trend-icon in" :class="[cardData.flipGoodDirection ? 'good up' : 'bad up']"></div>
          <span class="in" :class="[cardData.flipGoodDirection ? 'good' : 'bad']">{{ cardData.subValues.in | number }}</span>
        </div>
        <div
          v-if="cardData.subValues && cardData.subValues.out"
          class="card-diff-item out"
        >
          <div class="trend-icon out good down"></div>
          <span class="out good">{{ cardData.subValues.out | number }}</span>
        </div>
        <div
          v-if="cardData.subValues && cardData.subValues.deceased"
          class="card-diff-item deceased"
        >
          <div class="trend-icon deceased"></div>
          <span class="deceased"
            >{{ cardData.subValues.deceased | number }}
          </span>
        </div>
        <div
          v-if="cardData.subValues && cardData.subValues.positive"
          class="card-diff-item positive"
        >
          <div class="trend-icon positive bad"></div>
          <span class="positive bad">{{ cardData.subValues.positive | number }}</span>
        </div>
        <div
          v-if="cardData.subValues && cardData.subValues.percent"
          class="card-diff-item percent"
        >
          <div class="trend-icon percent tests"></div>
          <span class="percent tests">{{ cardData.subValues.percent | number }}</span>
        </div>
      </div>
      <div class="data-time" :class="{ outdated }">
        {{
          $t("infocard.lastUpdated", {
            date: date,
          })
        }}
      </div>
    </div>
    <div class="hp-card" v-else>
      <div class="card-title">{{ cardData.title }}</div>
      <font-awesome-icon icon="spinner" spin />
    </div>
  </div>
</template>
<script>
import { mapGetters, mapState } from "vuex";

export default {
  props: {
    cardData: Object,
    loading: Boolean,
    cardName: String,
  },
  computed: {
    subLabelExtraText(){
      const fieldNameToPut = _.get(this.cardData, 'extraFields.withSubLabel')
      const value = _.get(this.cardData, `subValues.${fieldNameToPut}`)
      return value
    },
    valueExtraText(){
      const fieldNameToPut = _.get(this.cardData, 'extraFields.withValue')
      const value = _.get(this.cardData, `subValues.${fieldNameToPut}`)
      return value
    },
    date() {
      const date = new Date();
      date.setFullYear(this.cardData.year);
      date.setMonth(this.cardData.month - 1); // Javascript thinks January is 0
      date.setDate(this.cardData.day);
      return date;
    },
    outdated() {
      return new Date() - 1000 * 3600 * 60 > this.date;
    },
    percentageDirection() {
      if (this.cardData.diffPercentage === 0) {
        return "no-change";
      } else if (this.cardData.diffPercentage > 0 && !this.cardData.reverseGoodDirection) {
        return "bad";
      }
      return "good";
    },
  },
};
</script>

<style lang="scss">
.hp-card-holder {
  flex: 1;
}

.hp-card {
  display: flex;
  flex-direction: column;
  // display: grid;
  // grid-template-rows: auto auto 1fr auto; // TODO: fix for other languages (hr,de)
  min-height: 166px;
  height: 100%;
  padding: 16px;
  background: #fff;
  box-shadow: $element-box-shadow;
  border-radius: 6px;

  @media only screen and (min-width: 480px) {
    padding: 26px;
  }

  @media only screen and (min-width: 768px) {
    padding: 20px 32px;
  }
}

.card-title {
  font-size: 13px;
  font-weight: 700;
  margin-bottom: 0.5rem !important;

  span {
    margin-right: 5px;
  }
}

.card-number {
  font-size: 32px;
  font-weight: 700;
  white-space: nowrap;
}
.card-number-extra{
  font-size: 14px;
}

.card-percentage-diff {
  display: inline-block;
  font-size: 14px;
  font-weight: normal;
  margin-left: 7px;
}

.card-diff {
  font-size: 14px;
  margin-bottom: 0.7rem;

  .card-diff-item {
    display: inline-block;
  }

  .card-diff-item:not(:last-child) {
    margin-right: 4px;

    @media only screen and (min-width: 992px) {
      margin-right: 8px;
    }
  }
}

.card-note {
  font-size: 12px;
}

.card-average {
  font-size: 12px;
  margin-bottom: 4px;
}

.trend-icon {
  display: inline-block;
  width: 22px;
  height: 22px;
  object-fit: contain;
  vertical-align: bottom;

  &.bad {
    background-color: #bf5747;
  }

  &.good {
    background-color: #20b16d;
  }

  &.up {
    -webkit-mask: url(../../assets/svg/close-circle-up.svg) no-repeat center;
    mask: url(../../assets/svg/close-circle-up.svg) no-repeat center;
  }

  &.down {
    -webkit-mask: url(../../assets/svg/close-circle-down.svg) no-repeat center;
    mask: url(../../assets/svg/close-circle-down.svg) no-repeat center;
  }

  &.deceased {
    -webkit-mask: url(../../assets/svg/close-circle-deceased.svg) no-repeat
      center;
    mask: url(../../assets/svg/close-circle-deceased.svg) no-repeat center;
    background-color: #404040;
  }

  &.positive {
    -webkit-mask: url(../../assets/svg/close-circle-plus.svg) no-repeat center;
    mask: url(../../assets/svg/close-circle-plus.svg) no-repeat center;
    background-color: #bf5747;
  }

  &.percent {
    -webkit-mask: url(../../assets/svg/close-circle-percent.svg) no-repeat center;
    mask: url(../../assets/svg/close-circle-percent.svg) no-repeat center;
    background-color: #665191;
  }

  &.none {
    display: none;
  }

  &.no-change {
    background-color: #a0a0a0;
  }
}

.bad {
  color: #bf5747;
}

.good {
  color: #20b16d;
}

.tests {
  color: #665191;
}

.no-change,
.deceased {
  color: #a0a0a0;
}

.data-time {
  font-size: 12px;
  color: #a0a0a0;
  margin-top: auto;

  &.outdated {
    color: #bf5747;
  }
}

/**
  SPECIAL CARD STYLES
 */

.cardtype-vaccinationSummary {
  .percent {
    &.trend-icon {
      background-color: #a0a0a0;
    }
    &.tests {
      color: #a0a0a0;
    }
  }
  .up{
    -webkit-mask: url(../../assets/svg/syringe.svg) no-repeat center;
    mask: url(../../assets/svg/syringe.svg) no-repeat center;
    -webkit-mask-size: 20px;
    mask-size: 20px;
  }
  .card-diff{
    display: flex;
    &:before{
      content: "";
      width: 20px;
      height: 20px;
      background-color: #20b16d;
      display: inline-block;
      -webkit-mask: url(../../assets/svg/syringe.svg) no-repeat center;
      mask: url(../../assets/svg/syringe.svg) no-repeat center;
      -webkit-mask-size: 20px;
      mask-size: 20px;
      margin-right: -8px;
    }
  }
  .card-number{
    display: flex;
    align-items: center;
    &::before{
      content: "";
      width: 24px;
      height: 24px;
      background-color: #000000;
      display: inline-block;
      -webkit-mask: url(../../assets/svg/syringe.svg) no-repeat center;
      mask: url(../../assets/svg/syringe.svg) no-repeat center;
      -webkit-mask-size: 24px;
      mask-size: 24px;
    }
  }
}
</style>

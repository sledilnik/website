<template>
  <div class="custom-container">
    <div v-if="isStripeSuccess" class="static-page-wrapper stripeSuccess">
      <div class="header-wrapper">
          <div>
            <img src="../assets/donate-thanks.png" />
          </div>
          <h1 v-if="successTranslationKey!=null">{{ $t(`donation.${successTranslationKey}success.title`) }}</h1>
          <h1 v-else>{{ $t(`donation.success.title`) }}</h1>
      </div>

      <div v-html-md="$t('donation.success.description')" />
      <div v-if="successTranslationKey!=null" v-html-md="$t(`donation.${successTranslationKey}success.description`)" />

      <div class="session">
        {{ $t("donation.success.session") }}
        {{ stripeSessionId }}
      </div>
    </div>

    <div v-else class="static-page-wrapper donation">
      <h1>{{ $t("donation.title") }}</h1>
      <div v-html-md="$t('donation.intro')" />

      <div>
        <div class="paymentMethod">
          <font-awesome-icon icon="credit-card" pull="left" size="2x" class="icon" />
          <div v-html-md="$t('donation.monthly.stripe.description')" />
          <div class="stripeCheckout">
            <stripe-checkout
              ref="checkoutSubscriptionRef"
              mode="subscription"
              :pk="publishableKey"
              :success-url="successURL+'&type=monthly'"
              :cancel-url="cancelURL"
              :locale="language"
              @loading="v => loading = v"
            />
            <span v-for="(item) in stripeSubscriptions" :key="item.price">
              <button @click="submitSubscription(item)">{{ $t("donation.monthly.stripe.donateButton", {amount: item.amount + " EUR"}) }}</button>
            </span>
          </div>

          <div v-html-md="$t('donation.onetime.stripe.description')" />
          <div class="stripeCheckout">
            <stripe-checkout
              ref="checkoutOneTimeDonationRef"
              mode="payment"
              :pk="publishableKey"
              :success-url="successURL+'&type=onetime'"
              :cancel-url="cancelURL"
              :locale="language"
              @loading="v => loading = v"
            />
            <span v-for="(item) in stripeOneTimeDonations" :key="item.price">
              <button @click="submitOneTimeDonation(item)">{{ item.amount + " EUR" }}</button>
            </span>
          </div>
        </div>

        <div class="paymentMethod">
          <font-awesome-icon icon="university" pull="left" size="2x" class="icon" />
          <div v-html-md="$t('donation.banktransfer.description')" />
          <div class="stripeCheckout">
            <span v-if="language=='sl'">
              <button @click="showBanktransferDetails=false; showPermanentBankTransferOrderDetails = !showPermanentBankTransferOrderDetails">{{ $t("donation.monthly.permanentBankTransferOrder.detailsButton") }}</button>
            </span>
            <span>
              <button @click="showPermanentBankTransferOrderDetails=false; showBanktransferDetails = !showBanktransferDetails">{{ $t("donation.onetime.banktransfer.detailsButton") }}</button>
            </span>
          </div>
          <div v-if="showPermanentBankTransferOrderDetails">
            <div v-html-md="$t('donation.monthly.permanentBankTransferOrder.description')" />
            <table class="bankDetails">
              <tr>
                <td>{{ $t("donation.banktransfer.details.recipient") }}</td>
                <td>
                  Znanstveno društvo Sledilnik
                  <br>Celovška cesta 111
                  <br><span v-if="language!='sl'">1000 </span>Ljubljana
                  <span v-if="language!='sl'"><br>Slovenia</span>
                </td>
              </tr>
              <tr>
                <td>{{ $t("donation.banktransfer.details.iban") }}</td>
                <td>SI56 6100 0002 5152 059</td>
              </tr>
              <tr>
                <td>{{ $t("donation.banktransfer.details.purposeCode") }}</td>
                <td>CHAR</td>
              </tr>
              <tr v-if="language=='sl'">
                <td>{{ $t("donation.banktransfer.details.purpose") }}</td>
                <td>Donacija</td>
              </tr>
              <tr v-if="language=='sl'">
                <td>{{ $t("donation.banktransfer.details.reference") }}</td>
                <td>SI99</td>
              </tr>
              <tr v-if="language!='sl'">
                <td>{{ $t("donation.banktransfer.details.bankBicSwift") }}</td>
                <td>HDELSI22</td>
              </tr>
              <tr v-if="language!='sl'">
                <td>{{ $t("donation.banktransfer.details.bank") }}</td>
                <td>
                  Delavska hranilnica d.d.
                  <br>Miklošičeva 5
                  <br><span v-if="language!='sl'">1000 </span>Ljubljana
                  <span v-if="language!='sl'"><br>Slovenia</span>
                </td>
              </tr>

            </table>
            <!-- <div v-html-md="$t('donation.monthly.permanentBankTransferOrder.authorisation')" /> -->
          </div>
          <div v-if="showBanktransferDetails">
            <div v-html-md="$t('donation.onetime.banktransfer.description')" />
            <img v-if="language=='sl'" src="../assets/donate-qr.png" class="qr" />
            <table class="bankDetails">
              <tr>
                <td>{{ $t("donation.banktransfer.details.recipient") }}</td>
                <td>
                  Znanstveno društvo Sledilnik
                  <br>Celovška cesta 111
                  <br><span v-if="language!='sl'">1000 </span>Ljubljana
                  <span v-if="language!='sl'"><br>Slovenia</span>
                </td>
              </tr>
              <tr>
                <td>{{ $t("donation.banktransfer.details.iban") }}</td>
                <td>SI56 6100 0002 5152 059</td>
              </tr>
              <tr>
                <td>{{ $t("donation.banktransfer.details.purposeCode") }}</td>
                <td>CHAR</td>
              </tr>
              <tr v-if="language=='sl'">
                <td>{{ $t("donation.banktransfer.details.purpose") }}</td>
                <td>Donacija</td>
              </tr>
              <tr v-if="language=='sl'">
                <td>{{ $t("donation.banktransfer.details.reference") }}</td>
                <td>SI99</td>
              </tr>
              <tr v-if="language!='sl'">
                <td>{{ $t("donation.banktransfer.details.bankBicSwift") }}</td>
                <td>HDELSI22</td>
              </tr>
              <tr v-if="language!='sl'">
                <td>{{ $t("donation.banktransfer.details.bank") }}</td>
                <td>
                  Delavska hranilnica d.d.
                  <br>Miklošičeva 5
                  <br><span v-if="language!='sl'">1000 </span>Ljubljana
                  <span v-if="language!='sl'"><br>Slovenia</span>
                </td>
              </tr>
            </table>
            <img v-if="language=='sl'" src="../assets/donate-upn.png" class="upn" />
          </div>
        </div>


        <div v-if="language=='sl'" class="paymentMethod" id="sms">
          <font-awesome-icon icon="mobile-alt" pull="left" size="2x" class="icon" />
          <div v-html-md="$t('donation.onetime.sms.description')" />
          <div class="stripeCheckout">
            <span v-for="(item) in smsAmounts" :key="item">
                <button v-if="showSmsButtons" @click="smsDonateClick(item)">{{ item + " EUR" }}</button>
            </span>
          </div>
          <div v-if="smsShowDetails" class="smsDetails">
            <img :src="smsQrImage(smsSelectedItem)" @click="triggerSms(smsSelectedItem)" class="smsqr">
            <div v-html="smsDetailsHtml"></div>
          </div>
        </div>

      </div>

      <!--
      <div v-if="language=='sl'" class="paymentMethod">
        <h2>{{ $t("donation.incomeTax.title") }}</h2>
        <div v-html-md="$t('donation.incomeTax.description')" />
      </div>
      -->

      <div>
        <h2>{{ $t("donation.companies.title") }}</h2>
        <font-awesome-icon icon="file-contract" pull="left" size="2x" class="icon" />
        <div v-html-md="$t('donation.companies.description')" />
      </div>

      <div>
        <h2>{{ $t("donation.other.title") }}</h2>
        <div v-html-md="$t('donation.other.description')" />
      </div>
    </div>
  </div>
</template>

<script>
import { StripeCheckout } from '@vue-stripe/vue-stripe';
import marked from "marked";

function parseStripeItemsFromConfig(config) {
  var items = [];
  var configItems = config.split(";");
  configItems.forEach((configItem) => {
    var pair = configItem.split(":")
    items.push({
        amount: pair[0],
        lineItems:[
          {
            price: pair[1],
            quantity: 1,
          },
        ]
      })
    });
  return items
}

export default {
  name: 'DonationPage',
  components: {
      StripeCheckout
  },
  data () {
    this.publishableKey = process.env.VUE_APP_STRIPE_PUBLISHABLE_KEY;

    let urlParams = new URLSearchParams(window.location.search);
    var stripeSuccess = urlParams.has('stripeSessionId');
    var successTranslationKey;
    if (stripeSuccess) {
      var stripeSessionId = urlParams.get('stripeSessionId')
      if (urlParams.has('type')) {
        successTranslationKey = urlParams.get('type') + '.stripe.'
      }
      window.history.replaceState({}, '', `${location.pathname}`);
    }
    return {
      isStripeSuccess: stripeSuccess,
      stripeSessionId: stripeSessionId,
      successTranslationKey: successTranslationKey,
      loading: false,
      stripeSubscriptions: parseStripeItemsFromConfig(process.env.VUE_APP_STRIPE_SUBSCRIPTIONS),
      stripeOneTimeDonations: parseStripeItemsFromConfig(process.env.VUE_APP_STRIPE_ONETIME),
      successURL: `${location.origin}/${this.$i18n.i18next.language}/donate?stripeSessionId={CHECKOUT_SESSION_ID}`,
      cancelURL: `${location.origin}/${this.$i18n.i18next.language}/donate`,
      language: `${this.$i18n.i18next.language}`,
      smsNumber: 1919,
      smsKeyword: "SLEDILNIK",
      smsAmounts: [1,5,10],
      smsSelectedItem: 10,
      smsShowDetails: false,
      smsDetailsHtml: "",
      showSmsButtons: true,
      showBanktransferDetails: false,
      showPermanentBankTransferOrderDetails: false
    };
  },
  methods: {
    // You will be redirected to Stripe's secure checkout page
    submitSubscription (item) {
      this.$refs.checkoutSubscriptionRef.lineItems = item.lineItems;
      this.$refs.checkoutSubscriptionRef.redirectToCheckout();
    },
    submitOneTimeDonation (item) {
      this.$refs.checkoutOneTimeDonationRef.lineItems = item.lineItems;
      this.$refs.checkoutOneTimeDonationRef.redirectToCheckout();
    },
    smsDonateClick (item) {
      this.smsShowDetails = !this.smsShowDetails || item!=this.smsSelectedItem;
      if (!this.smsShowDetails) {
        return
      }
      this.smsSelectedItem = item;
      this.smsDetailsHtml = marked(this.$t('donation.onetime.sms.donateDetails', {amount: item + ' EUR', number: 1919, keyword: this.smsKeyword + item }))
      if(screen.width < 600 || screen.height < 600) {
        this.triggerSms(item);
      }
    },
    triggerSms (item) {
      window.location.href=`sms:${this.smsNumber}?&body=${this.smsKeyword}${item}`;
    },
    smsQrImage(amount) {
      var images = require.context('../assets/donate/', false, /sms.*\.png$/)
      return images(`./sms-${this.smsNumber}-${this.smsKeyword}${amount}.png`)
    }
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">

.donation {
  h2 {
    margin-top: 48px;
  }

  .icon {
    margin-top: 4px;
  }
}

table.bankDetails {
  font-size: 14px;
  margin: 10px 0;
  width:auto;
  min-width: 350px;

  @media only screen and (max-width: 633px) {
    width: 100%;
    padding: 0;
    min-width: auto;
  }

  td {
    padding: 5px 0;
    width: auto;
  }
}

img {
  max-width: 100%;

  &.upn {
    margin: 0 0 20px;
    box-shadow: $element-box-shadow;
  }

  &.qr {
    float: right;
    margin: 0 0 20px 20px;
    image-rendering: crisp-edges;

    @media only screen and (max-width: 400px) {
      width: 100%;
      margin: 0 0 20px 0;
    }
  }

  &.smsqr {
    float: right;
    image-rendering: crisp-edges;
    width: 84px;
    margin: 0 0 20px 20px;

    @media only screen and (max-width: 240px) {
      width: 100%;
      margin: 0 0 20px 0;
    }
  }
}



.paymentMethod {
  margin: 24px 0;

  .smsDetails {
    min-height: 84px;
  }
}

.stripeCheckout {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
  margin-right: -20px;

  span {
    flex: 1 1 0px;
    margin-right: 20px;
    margin-bottom: 20px;

    button {
      width: 100%;
      white-space: nowrap;
      padding: 10px;
      background-color: $yellow;
      border: none;
      box-shadow: $element-box-shadow;
    }
  }
}

.stripeSuccess {
  .header-wrapper {
    position: relative;
    div {
      text-align:center;
      img {
          width: 100%;
          height: 100%;
          max-width:300px;
          margin: 16px 0;

          @media only screen and (max-width: 300px) {
            margin-top: 48px;
          }
          @media only screen and (min-width: 768px) {
            margin-top: 64px;
          }
      }
    }

    h1{
      position: absolute;
      width: 100%;
      top: 0;
      margin-top:0;
    }
  }

  .session {
    margin-top: 50px;
    font-size: 10px;
    color: #999999;
    overflow-wrap: break-word;
    word-wrap: break-word;
  }
}
</style>

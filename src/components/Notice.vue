<template>
  <div class="container">
    <a
      class="notice-link"
      href="#municipalites-chart"
      v-scroll-to="{ el: '#municipalities-chart', offset: -115 }"
    >
      <div class="notice">
          <span>
            <b>Nov prikaz po občinah: </b>
            nov prikaz za aktivne primere, umrle in oceno prebolelih v posamezni občini.
          </span>
          <div class="notice-button">
            <img src="../assets/svg/go-to.svg" alt="Go to news" />
          </div>
      </div>
    </a>
  </div>
</template>

<style scoped lang="scss">
.container {
  padding-left: 0;
  padding-right: 0;

  @media only screen and (min-width: 768px) {
    padding-left: 15px;
    padding-right: 15px;
  }
}

.notice-link {
  text-decoration: none;
}

.notice {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fffbe8;
  padding: 18px;
  margin: 0px auto 48px;
  width: 100%;
  cursor: pointer;
  font-size: 14px;
  color: $text-c;
  text-decoration: none;
  box-shadow: $element-box-shadow;

  @media only screen and (min-width: 480px) {
    padding: 26px;
  }

  @media only screen and (min-width: 768px) {
    padding: 20px 32px;
  }

  &-button {
    margin-left: 16px;
  }
}

</style>

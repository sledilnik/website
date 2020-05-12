<template>
  <div class="container">
    <div class="notice">
      <span>
        <b>Zadnja novica: </b>
        Največji posvet znanstvenikov: ‘Združiti je treba moč institucij in
        znanstvene skupnosti v boju proti COVID-19’
      </span>
      <a href="#" class="notice-button">
        <img src="../assets/svg/go-to.svg" alt="Go to news" />
      </a>
    </div>
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

.notice {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: rgba($yellow, 0.1);
  padding: 20px 32px;
  margin: 0px auto 48px;
  width: 100%;
  cursor: pointer;
  font-size: 14px;
  box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3),
    0 11px 12px -12px rgba(0, 0, 0, 0.22);

  &-button {
    margin-left: 16px;
  }
}
</style>

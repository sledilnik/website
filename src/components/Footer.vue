<template>
  <footer>
    <div class="content-wrapper">
      <div class="copy">
        <span>{{ $t('footer.copyright') }}</span>
      </div>
      <div class="email">
        <a href="mailto:info@sledilnik.org">
          <span>info@sledilnik.org</span>
        </a>
      </div>
      <div class="social">
        <a href="https://fb.me/SledilnikOrg" target="_blank" rel="noreferrer">
          <img loading="lazy" src="../assets/svg/fb-icon.svg" alt="Facebook" />
        </a>
        <a href="https://twitter.com/sledilnik" target="_blank" rel="noreferrer">
          <img loading="lazy" src="../assets/svg/tw-icon.svg" alt="Twitter" />
        </a>
        <a href="https://medium.com/sledilnik" target="_blank" rel="noreferrer">
          <img loading="lazy" src="../assets/svg/medium-icon.svg" alt="Medium" />
        </a>
        <a href="https://www.youtube.com/channel/UCM_Sk2GZ8vTMiyBQ0SMHgJw/videos" target="_blank" rel="noreferrer">
          <img loading="lazy" src="../assets/svg/youtube.svg" alt="YouTube" />
        </a>
        <a href="https://github.com/sledilnik" target="_blank" rel="noreferrer">
          <img loading="lazy" src="../assets/svg/gh-icon.svg" alt="GitHub" />
        </a>
      </div>
    </div>
  </footer>
</template>

<script>
export default {
  name: "Footer"
};
</script>

<style scopped lang="sass">
footer
  border-top: 1px solid rgba(0, 0, 0, 0.13)
  background: #fff
  padding: 0 15px
  min-height: 66px
  display: flex
  align-items: center
  justify-content: center
  margin-top: auto

  @media only screen and (min-width: 768px)
    padding: 16px 32px

.content-wrapper
  position: relative
  color: rgba(0, 0, 0, 0.56)
  font-size: 13px
  line-height: 1.3
  padding: 15px 0

  @media only screen and (min-width: 768px)
    display: flex
    align-items: center
    padding: 0

  .copy
    margin-bottom: 8px

    @media only screen and (min-width: 768px)
      margin-bottom: 0
    @media only screen and (max-width: 520px)
      display: inline-block

  .email
    margin-left: auto
    @media only screen and (max-width: 520px)
      display: inline-block
      float: right
      margin-left: 16px
      margin-bottom: 16px

  a
    line-height: 1.3
    font-size: 13px
    color: rgba(0, 0, 0, 0.56)
    text-decoration: none

    &:hover
      color: rgba(0, 0, 0, 0.5)

  .social
    position: absolute
    right: 0
    top: 50%
    transform: translateY(-50%)
    // display: block
    // font-size: 20px
    // flex-basis: 100%
    // text-align: right

    @media only screen and (min-width: 768px)
      position: static
      margin-left: 32px
      transform: none

    @media only screen and (max-width: 520px)
      width: 100%
      position: static
      transform: none
      display: flex
      justify-content: space-around

    img
      width: 40px
      opacity: 0.25

      @media only screen and (min-width: 768px)
        width: 32px

    a + a
      margin-left: 32px

      @media only screen and (max-width: 520px)
        margin-left: 16px
</style>

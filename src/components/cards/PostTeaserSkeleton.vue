<template>
  <div class="post__teaser">
    <b-card no-body class="overflow-hidden h-100">
      <b-row no-gutters class="h-100">
        <b-col lg="4" class="d-none d-lg-block h-100 p-2 pr-0">
          <div class="teaser__image skeleton"></div>
        </b-col>
        <b-col lg="8" class="h-100">
          <b-card-body>
            <p class="skeleton"></p>
            <p class="skeleton col-6"></p>
            <p class="skeleton col-8"></p>
          </b-card-body>
        </b-col>
      </b-row>
    </b-card>
  </div>
</template>
<style lang="scss">
.skeleton {
  background-color: #0000001f;
  &:after {
    background: linear-gradient(
      90deg,
      hsla(0, 0%, 100%, 0),
      hsla(0, 0%, 100%, 0.3),
      hsla(0, 0%, 100%, 0)
    );
    animation: loading 1.5s infinite;
    content: "";
    height: 100%;
    left: 0;
    position: absolute;
    right: 0;
    top: 0;
    transform: translateX(-100%);
    z-index: 1;
  }
}
p.skeleton {
  border-radius: 15px;
  margin-bottom: 20px;
  height: 20px;
}
@keyframes loading {
  to {
    transform: translateX(100%);
  }
}
</style>

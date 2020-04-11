module.exports = {
  extends: [
    "plugin:vue/essential"
  ],
  plugins: [
    "mocha"
  ],
  parserOptions: {
    ecmaFeatures: {
        jsx: true,
        modules: true
    }
  }
}
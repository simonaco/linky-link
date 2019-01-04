import Vue from "vue";
import Vuex from "vuex";
import ListModule from "@/store/ListModule";
import UserModule from "@/store/UserModule";
import AppModule from "@/store/AppModule";

Vue.use(Vuex);

export const store = new Vuex.Store({
  state: {},
  modules: {
    ListModule,
    UserModule,
    AppModule
  }
});

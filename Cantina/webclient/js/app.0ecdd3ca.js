(function(e){function r(r){for(var s,n,i=r[0],u=r[1],l=r[2],d=0,m=[];d<i.length;d++)n=i[d],Object.prototype.hasOwnProperty.call(a,n)&&a[n]&&m.push(a[n][0]),a[n]=0;for(s in u)Object.prototype.hasOwnProperty.call(u,s)&&(e[s]=u[s]);c&&c(r);while(m.length)m.shift()();return o.push.apply(o,l||[]),t()}function t(){for(var e,r=0;r<o.length;r++){for(var t=o[r],s=!0,i=1;i<t.length;i++){var u=t[i];0!==a[u]&&(s=!1)}s&&(o.splice(r--,1),e=n(n.s=t[0]))}return e}var s={},a={app:0},o=[];function n(r){if(s[r])return s[r].exports;var t=s[r]={i:r,l:!1,exports:{}};return e[r].call(t.exports,t,t.exports,n),t.l=!0,t.exports}n.m=e,n.c=s,n.d=function(e,r,t){n.o(e,r)||Object.defineProperty(e,r,{enumerable:!0,get:t})},n.r=function(e){"undefined"!==typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(e,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(e,"__esModule",{value:!0})},n.t=function(e,r){if(1&r&&(e=n(e)),8&r)return e;if(4&r&&"object"===typeof e&&e&&e.__esModule)return e;var t=Object.create(null);if(n.r(t),Object.defineProperty(t,"default",{enumerable:!0,value:e}),2&r&&"string"!=typeof e)for(var s in e)n.d(t,s,function(r){return e[r]}.bind(null,s));return t},n.n=function(e){var r=e&&e.__esModule?function(){return e["default"]}:function(){return e};return n.d(r,"a",r),r},n.o=function(e,r){return Object.prototype.hasOwnProperty.call(e,r)},n.p="/";var i=window["webpackJsonp"]=window["webpackJsonp"]||[],u=i.push.bind(i);i.push=r,i=i.slice();for(var l=0;l<i.length;l++)r(i[l]);var c=u;o.push([0,"chunk-vendors"]),t()})({0:function(e,r,t){e.exports=t("56d7")},"14c4":function(e,r,t){"use strict";var s=t("4101"),a=t.n(s);a.a},2395:function(e,r,t){},"274b":function(e,r,t){},4101:function(e,r,t){},"4fe7":function(e,r,t){"use strict";var s=t("274b"),a=t.n(s);a.a},"56d7":function(e,r,t){"use strict";t.r(r);t("e260"),t("e6cf"),t("cca6"),t("a79d");var s=t("2b0e"),a=function(){var e=this,r=e.$createElement,t=e._self._c||r;return t("div",{attrs:{id:"app"}},[t("ModalContainer")],1)},o=[],n=function(){var e=this,r=e.$createElement,t=e._self._c||r;return t("div",{directives:[{name:"show",rawName:"v-show",value:e.display,expression:"display"}],attrs:{id:"modalContainer"}},[t("register-form")],1)},i=[],u=function(){var e=this,r=e.$createElement,t=e._self._c||r;return t("div",{staticClass:"modalWindow",attrs:{id:"registerForm"}},[t("h2",[e._v("Регистрация:")]),t("form",[t("label",{attrs:{for:"email"}},[e._v("Ваш актуальный e-mail:")]),t("p",{directives:[{name:"show",rawName:"v-show",value:e.errorField.email.show,expression:"errorField.email.show"}],staticClass:"error"},[e._v(e._s(e.errorField.email.message))]),t("input",{directives:[{name:"model",rawName:"v-model",value:e.request.email,expression:"request.email"}],attrs:{type:"email",name:"email",maxlength:"32",autofocus:""},domProps:{value:e.request.email},on:{change:e.checkEmail,input:function(r){r.target.composing||e.$set(e.request,"email",r.target.value)}}}),t("label",{attrs:{for:"password"}},[e._v("Придумайте пароль:")]),t("p",{directives:[{name:"show",rawName:"v-show",value:e.errorField.password,expression:"errorField.password"}],staticClass:"error"},[e._v("Пароль слишком короткий")]),t("input",{directives:[{name:"model",rawName:"v-model",value:e.request.password,expression:"request.password"}],attrs:{type:"password",name:"password",maxlength:"18"},domProps:{value:e.request.password},on:{change:e.checkPassword,input:function(r){r.target.composing||e.$set(e.request,"password",r.target.value)}}}),t("label",{attrs:{for:"repeatPass"}},[e._v("Повторите пароль ещё раз:")]),t("p",{directives:[{name:"show",rawName:"v-show",value:e.errorField.repeatPassword,expression:"errorField.repeatPassword"}],staticClass:"error"},[e._v("Пароли не совпадают")]),t("input",{directives:[{name:"model",rawName:"v-model",value:e.repeatPassword,expression:"repeatPassword"}],attrs:{type:"password",name:"repeatPass",maxlength:"18"},domProps:{value:e.repeatPassword},on:{change:e.checkRepeatPassword,input:function(r){r.target.composing||(e.repeatPassword=r.target.value)}}}),t("label",{attrs:{for:"name"}},[e._v("Никнейм (отображается в чате):")]),t("p",{directives:[{name:"show",rawName:"v-show",value:e.errorField.name.show,expression:"errorField.name.show"}],staticClass:"error"},[e._v(e._s(e.errorField.name.message))]),t("input",{directives:[{name:"model",rawName:"v-model",value:e.request.name,expression:"request.name"}],attrs:{type:"text",name:"name",maxlength:"18"},domProps:{value:e.request.name},on:{change:e.checkName,input:function(r){r.target.composing||e.$set(e.request,"name",r.target.value)}}}),t("label",{attrs:{for:"gender"}},[e._v("Ваш пол:")]),t("input",{directives:[{name:"model",rawName:"v-model",value:e.request.gender,expression:"request.gender"}],attrs:{type:"radio",name:"gender",value:"0"},domProps:{checked:e._q(e.request.gender,"0")},on:{change:function(r){return e.$set(e.request,"gender","0")}}}),e._v(" Не знаю "),t("input",{directives:[{name:"model",rawName:"v-model",value:e.request.gender,expression:"request.gender"}],attrs:{type:"radio",name:"gender",value:"1"},domProps:{checked:e._q(e.request.gender,"1")},on:{change:function(r){return e.$set(e.request,"gender","1")}}}),e._v(" Мужской "),t("input",{directives:[{name:"model",rawName:"v-model",value:e.request.gender,expression:"request.gender"}],attrs:{type:"radio",name:"gender",value:"2"},domProps:{checked:e._q(e.request.gender,"2")},on:{change:function(r){return e.$set(e.request,"gender","2")}}}),e._v(" Женский "),t("label",{attrs:{for:"location"}},[e._v("Откуда вы:")]),t("p",{directives:[{name:"show",rawName:"v-show",value:e.errorField.location,expression:"errorField.location"}],staticClass:"error"},[e._v("Недопустимые символы в тексте")]),t("input",{directives:[{name:"model",rawName:"v-model",value:e.request.location,expression:"request.location"}],attrs:{type:"text",maxlength:"32",name:"location"},domProps:{value:e.request.location},on:{input:function(r){r.target.composing||e.$set(e.request,"location",r.target.value)}}}),t("input",{attrs:{type:"submit",value:"Зарегистрироваться"}})])])},l=[],c=(t("b0c0"),{name:"registerForm",data:function(){return{request:{email:"",password:"",name:"",gender:0,location:""},repeatPassword:"",errorField:{email:{show:!1,message:""},password:!1,repeatPassword:!1,name:{show:!1,message:""},location:!1},requestIsCorrect:!1}},methods:{checkEmail:function(){var e=/^([a-z0-9_\-.])+@([a-z0-9_\-.])+\.([a-z]{2,4})$/i;e.test(this.request.email)?(this.errorField.email.show=!1,this.requestIsCorrect=!0):(this.errorField.email.message="Некорректный e-mail",this.errorField.email.show=!0,this.requestIsCorrect=!1)},checkPassword:function(){this.request.password.length<5?(this.errorField.password=!0,this.requestIsCorrect=!1):(this.errorField.password=!1,this.requestIsCorrect=!0)},checkRepeatPassword:function(){this.request.password!=this.repeatPassword?(this.errorField.repeatPassword=!0,this.requestIsCorrect=!1):(this.errorField.repeatPassword=!1,this.requestIsCorrect=!0)},checkName:function(){var e=/^[a-zа-я]{2,9}\s?[a-zа-я0-9]{2,10}$/i;e.test(this.request.name)?(this.errorField.name.show=!1,this.requestIsCorrect=!0):(this.errorField.name.message="Некорректный никнейм (допустимы только буквы и цифры)",this.errorField.name.show=!0,this.requestIsCorrect=!1)},checkLocation:function(){var e=/^\w{4,32}$/i;e.test(this.request.location)||""==this.request.location?(this.errorField.location=!1,this.requestIsCorrect=!0):(this.errorField.location=!0,this.requestIsCorrect=!1)},checkRequest:function(){return this.checkEmail(),this.checkPassword(),this.checkRepeatPassword(),this.checkName(),this.checkLocation(),this.requestIsCorrect},submitForm:function(){this.checkRequest()}}}),d=c,m=(t("14c4"),t("2877")),p=Object(m["a"])(d,u,l,!1,null,"0401b3e4",null),h=p.exports,v={name:"ModalContaner",data:function(){return{display:!0}},components:{RegisterForm:h}},f=v,w=(t("821a"),t("4fe7"),Object(m["a"])(f,n,i,!1,null,"25e1cf7a",null)),g=w.exports,q={name:"app",components:{ModalContainer:g},data:function(){return{}},methods:{ShowModal:function(){}}},b=q,P=(t("7c55"),Object(m["a"])(b,a,o,!1,null,null,null)),_=P.exports;s["a"].config.productionTip=!1,new s["a"]({render:function(e){return e(_)}}).$mount("#app")},"7b80":function(e,r,t){},"7c55":function(e,r,t){"use strict";var s=t("2395"),a=t.n(s);a.a},"821a":function(e,r,t){"use strict";var s=t("7b80"),a=t.n(s);a.a}});
//# sourceMappingURL=app.0ecdd3ca.js.map
var e=Object.create,t=Object.defineProperty,n=Object.getOwnPropertyDescriptor,r=Object.getOwnPropertyNames,i=Object.getPrototypeOf,a=Object.prototype.hasOwnProperty,o=(e,t)=>()=>(t||e((t={exports:{}}).exports,t),t.exports),s=(e,i,o,s)=>{if(i&&typeof i==`object`||typeof i==`function`)for(var c=r(i),l=0,u=c.length,d;l<u;l++)d=c[l],!a.call(e,d)&&d!==o&&t(e,d,{get:(e=>i[e]).bind(null,d),enumerable:!(s=n(i,d))||s.enumerable});return e},c=(n,r,a)=>(a=n==null?{}:e(i(n)),s(r||!n||!n.__esModule?t(a,`default`,{value:n,enumerable:!0}):a,n));(function(){let e=document.createElement(`link`).relList;if(e&&e.supports&&e.supports(`modulepreload`))return;for(let e of document.querySelectorAll(`link[rel="modulepreload"]`))n(e);new MutationObserver(e=>{for(let t of e)if(t.type===`childList`)for(let e of t.addedNodes)e.tagName===`LINK`&&e.rel===`modulepreload`&&n(e)}).observe(document,{childList:!0,subtree:!0});function t(e){let t={};return e.integrity&&(t.integrity=e.integrity),e.referrerPolicy&&(t.referrerPolicy=e.referrerPolicy),e.crossOrigin===`use-credentials`?t.credentials=`include`:e.crossOrigin===`anonymous`?t.credentials=`omit`:t.credentials=`same-origin`,t}function n(e){if(e.ep)return;e.ep=!0;let n=t(e);fetch(e.href,n)}})();var l=o((e=>{var t=Symbol.for(`react.transitional.element`),n=Symbol.for(`react.portal`),r=Symbol.for(`react.fragment`),i=Symbol.for(`react.strict_mode`),a=Symbol.for(`react.profiler`),o=Symbol.for(`react.consumer`),s=Symbol.for(`react.context`),c=Symbol.for(`react.forward_ref`),l=Symbol.for(`react.suspense`),u=Symbol.for(`react.memo`),d=Symbol.for(`react.lazy`),f=Symbol.for(`react.activity`),p=Symbol.iterator;function m(e){return typeof e!=`object`||!e?null:(e=p&&e[p]||e[`@@iterator`],typeof e==`function`?e:null)}var h={isMounted:function(){return!1},enqueueForceUpdate:function(){},enqueueReplaceState:function(){},enqueueSetState:function(){}},g=Object.assign,_={};function v(e,t,n){this.props=e,this.context=t,this.refs=_,this.updater=n||h}v.prototype.isReactComponent={},v.prototype.setState=function(e,t){if(typeof e!=`object`&&typeof e!=`function`&&e!=null)throw Error(`takes an object of state variables to update or a function which returns an object of state variables.`);this.updater.enqueueSetState(this,e,t,`setState`)},v.prototype.forceUpdate=function(e){this.updater.enqueueForceUpdate(this,e,`forceUpdate`)};function y(){}y.prototype=v.prototype;function b(e,t,n){this.props=e,this.context=t,this.refs=_,this.updater=n||h}var x=b.prototype=new y;x.constructor=b,g(x,v.prototype),x.isPureReactComponent=!0;var S=Array.isArray;function C(){}var w={H:null,A:null,T:null,S:null},T=Object.prototype.hasOwnProperty;function E(e,n,r){var i=r.ref;return{$$typeof:t,type:e,key:n,ref:i===void 0?null:i,props:r}}function D(e,t){return E(e.type,t,e.props)}function O(e){return typeof e==`object`&&!!e&&e.$$typeof===t}function k(e){var t={"=":`=0`,":":`=2`};return`$`+e.replace(/[=:]/g,function(e){return t[e]})}var ee=/\/+/g;function A(e,t){return typeof e==`object`&&e&&e.key!=null?k(``+e.key):t.toString(36)}function j(e){switch(e.status){case`fulfilled`:return e.value;case`rejected`:throw e.reason;default:switch(typeof e.status==`string`?e.then(C,C):(e.status=`pending`,e.then(function(t){e.status===`pending`&&(e.status=`fulfilled`,e.value=t)},function(t){e.status===`pending`&&(e.status=`rejected`,e.reason=t)})),e.status){case`fulfilled`:return e.value;case`rejected`:throw e.reason}}throw e}function M(e,r,i,a,o){var s=typeof e;(s===`undefined`||s===`boolean`)&&(e=null);var c=!1;if(e===null)c=!0;else switch(s){case`bigint`:case`string`:case`number`:c=!0;break;case`object`:switch(e.$$typeof){case t:case n:c=!0;break;case d:return c=e._init,M(c(e._payload),r,i,a,o)}}if(c)return o=o(e),c=a===``?`.`+A(e,0):a,S(o)?(i=``,c!=null&&(i=c.replace(ee,`$&/`)+`/`),M(o,r,i,``,function(e){return e})):o!=null&&(O(o)&&(o=D(o,i+(o.key==null||e&&e.key===o.key?``:(``+o.key).replace(ee,`$&/`)+`/`)+c)),r.push(o)),1;c=0;var l=a===``?`.`:a+`:`;if(S(e))for(var u=0;u<e.length;u++)a=e[u],s=l+A(a,u),c+=M(a,r,i,s,o);else if(u=m(e),typeof u==`function`)for(e=u.call(e),u=0;!(a=e.next()).done;)a=a.value,s=l+A(a,u++),c+=M(a,r,i,s,o);else if(s===`object`){if(typeof e.then==`function`)return M(j(e),r,i,a,o);throw r=String(e),Error(`Objects are not valid as a React child (found: `+(r===`[object Object]`?`object with keys {`+Object.keys(e).join(`, `)+`}`:r)+`). If you meant to render a collection of children, use an array instead.`)}return c}function te(e,t,n){if(e==null)return e;var r=[],i=0;return M(e,r,``,``,function(e){return t.call(n,e,i++)}),r}function N(e){if(e._status===-1){var t=e._result;t=t(),t.then(function(t){(e._status===0||e._status===-1)&&(e._status=1,e._result=t)},function(t){(e._status===0||e._status===-1)&&(e._status=2,e._result=t)}),e._status===-1&&(e._status=0,e._result=t)}if(e._status===1)return e._result.default;throw e._result}var P=typeof reportError==`function`?reportError:function(e){if(typeof window==`object`&&typeof window.ErrorEvent==`function`){var t=new window.ErrorEvent(`error`,{bubbles:!0,cancelable:!0,message:typeof e==`object`&&e&&typeof e.message==`string`?String(e.message):String(e),error:e});if(!window.dispatchEvent(t))return}else if(typeof process==`object`&&typeof process.emit==`function`){process.emit(`uncaughtException`,e);return}console.error(e)},F={map:te,forEach:function(e,t,n){te(e,function(){t.apply(this,arguments)},n)},count:function(e){var t=0;return te(e,function(){t++}),t},toArray:function(e){return te(e,function(e){return e})||[]},only:function(e){if(!O(e))throw Error(`React.Children.only expected to receive a single React element child.`);return e}};e.Activity=f,e.Children=F,e.Component=v,e.Fragment=r,e.Profiler=a,e.PureComponent=b,e.StrictMode=i,e.Suspense=l,e.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE=w,e.__COMPILER_RUNTIME={__proto__:null,c:function(e){return w.H.useMemoCache(e)}},e.cache=function(e){return function(){return e.apply(null,arguments)}},e.cacheSignal=function(){return null},e.cloneElement=function(e,t,n){if(e==null)throw Error(`The argument must be a React element, but you passed `+e+`.`);var r=g({},e.props),i=e.key;if(t!=null)for(a in t.key!==void 0&&(i=``+t.key),t)!T.call(t,a)||a===`key`||a===`__self`||a===`__source`||a===`ref`&&t.ref===void 0||(r[a]=t[a]);var a=arguments.length-2;if(a===1)r.children=n;else if(1<a){for(var o=Array(a),s=0;s<a;s++)o[s]=arguments[s+2];r.children=o}return E(e.type,i,r)},e.createContext=function(e){return e={$$typeof:s,_currentValue:e,_currentValue2:e,_threadCount:0,Provider:null,Consumer:null},e.Provider=e,e.Consumer={$$typeof:o,_context:e},e},e.createElement=function(e,t,n){var r,i={},a=null;if(t!=null)for(r in t.key!==void 0&&(a=``+t.key),t)T.call(t,r)&&r!==`key`&&r!==`__self`&&r!==`__source`&&(i[r]=t[r]);var o=arguments.length-2;if(o===1)i.children=n;else if(1<o){for(var s=Array(o),c=0;c<o;c++)s[c]=arguments[c+2];i.children=s}if(e&&e.defaultProps)for(r in o=e.defaultProps,o)i[r]===void 0&&(i[r]=o[r]);return E(e,a,i)},e.createRef=function(){return{current:null}},e.forwardRef=function(e){return{$$typeof:c,render:e}},e.isValidElement=O,e.lazy=function(e){return{$$typeof:d,_payload:{_status:-1,_result:e},_init:N}},e.memo=function(e,t){return{$$typeof:u,type:e,compare:t===void 0?null:t}},e.startTransition=function(e){var t=w.T,n={};w.T=n;try{var r=e(),i=w.S;i!==null&&i(n,r),typeof r==`object`&&r&&typeof r.then==`function`&&r.then(C,P)}catch(e){P(e)}finally{t!==null&&n.types!==null&&(t.types=n.types),w.T=t}},e.unstable_useCacheRefresh=function(){return w.H.useCacheRefresh()},e.use=function(e){return w.H.use(e)},e.useActionState=function(e,t,n){return w.H.useActionState(e,t,n)},e.useCallback=function(e,t){return w.H.useCallback(e,t)},e.useContext=function(e){return w.H.useContext(e)},e.useDebugValue=function(){},e.useDeferredValue=function(e,t){return w.H.useDeferredValue(e,t)},e.useEffect=function(e,t){return w.H.useEffect(e,t)},e.useEffectEvent=function(e){return w.H.useEffectEvent(e)},e.useId=function(){return w.H.useId()},e.useImperativeHandle=function(e,t,n){return w.H.useImperativeHandle(e,t,n)},e.useInsertionEffect=function(e,t){return w.H.useInsertionEffect(e,t)},e.useLayoutEffect=function(e,t){return w.H.useLayoutEffect(e,t)},e.useMemo=function(e,t){return w.H.useMemo(e,t)},e.useOptimistic=function(e,t){return w.H.useOptimistic(e,t)},e.useReducer=function(e,t,n){return w.H.useReducer(e,t,n)},e.useRef=function(e){return w.H.useRef(e)},e.useState=function(e){return w.H.useState(e)},e.useSyncExternalStore=function(e,t,n){return w.H.useSyncExternalStore(e,t,n)},e.useTransition=function(){return w.H.useTransition()},e.version=`19.2.0`})),u=o(((e,t)=>{t.exports=l()})),d=o((e=>{function t(e,t){var n=e.length;e.push(t);a:for(;0<n;){var r=n-1>>>1,a=e[r];if(0<i(a,t))e[r]=t,e[n]=a,n=r;else break a}}function n(e){return e.length===0?null:e[0]}function r(e){if(e.length===0)return null;var t=e[0],n=e.pop();if(n!==t){e[0]=n;a:for(var r=0,a=e.length,o=a>>>1;r<o;){var s=2*(r+1)-1,c=e[s],l=s+1,u=e[l];if(0>i(c,n))l<a&&0>i(u,c)?(e[r]=u,e[l]=n,r=l):(e[r]=c,e[s]=n,r=s);else if(l<a&&0>i(u,n))e[r]=u,e[l]=n,r=l;else break a}}return t}function i(e,t){var n=e.sortIndex-t.sortIndex;return n===0?e.id-t.id:n}if(e.unstable_now=void 0,typeof performance==`object`&&typeof performance.now==`function`){var a=performance;e.unstable_now=function(){return a.now()}}else{var o=Date,s=o.now();e.unstable_now=function(){return o.now()-s}}var c=[],l=[],u=1,d=null,f=3,p=!1,m=!1,h=!1,g=!1,_=typeof setTimeout==`function`?setTimeout:null,v=typeof clearTimeout==`function`?clearTimeout:null,y=typeof setImmediate<`u`?setImmediate:null;function b(e){for(var i=n(l);i!==null;){if(i.callback===null)r(l);else if(i.startTime<=e)r(l),i.sortIndex=i.expirationTime,t(c,i);else break;i=n(l)}}function x(e){if(h=!1,b(e),!m)if(n(c)!==null)m=!0,S||(S=!0,O());else{var t=n(l);t!==null&&A(x,t.startTime-e)}}var S=!1,C=-1,w=5,T=-1;function E(){return g?!0:!(e.unstable_now()-T<w)}function D(){if(g=!1,S){var t=e.unstable_now();T=t;var i=!0;try{a:{m=!1,h&&(h=!1,v(C),C=-1),p=!0;var a=f;try{b:{for(b(t),d=n(c);d!==null&&!(d.expirationTime>t&&E());){var o=d.callback;if(typeof o==`function`){d.callback=null,f=d.priorityLevel;var s=o(d.expirationTime<=t);if(t=e.unstable_now(),typeof s==`function`){d.callback=s,b(t),i=!0;break b}d===n(c)&&r(c),b(t)}else r(c);d=n(c)}if(d!==null)i=!0;else{var u=n(l);u!==null&&A(x,u.startTime-t),i=!1}}break a}finally{d=null,f=a,p=!1}i=void 0}}finally{i?O():S=!1}}}var O;if(typeof y==`function`)O=function(){y(D)};else if(typeof MessageChannel<`u`){var k=new MessageChannel,ee=k.port2;k.port1.onmessage=D,O=function(){ee.postMessage(null)}}else O=function(){_(D,0)};function A(t,n){C=_(function(){t(e.unstable_now())},n)}e.unstable_IdlePriority=5,e.unstable_ImmediatePriority=1,e.unstable_LowPriority=4,e.unstable_NormalPriority=3,e.unstable_Profiling=null,e.unstable_UserBlockingPriority=2,e.unstable_cancelCallback=function(e){e.callback=null},e.unstable_forceFrameRate=function(e){0>e||125<e?console.error(`forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported`):w=0<e?Math.floor(1e3/e):5},e.unstable_getCurrentPriorityLevel=function(){return f},e.unstable_next=function(e){switch(f){case 1:case 2:case 3:var t=3;break;default:t=f}var n=f;f=t;try{return e()}finally{f=n}},e.unstable_requestPaint=function(){g=!0},e.unstable_runWithPriority=function(e,t){switch(e){case 1:case 2:case 3:case 4:case 5:break;default:e=3}var n=f;f=e;try{return t()}finally{f=n}},e.unstable_scheduleCallback=function(r,i,a){var o=e.unstable_now();switch(typeof a==`object`&&a?(a=a.delay,a=typeof a==`number`&&0<a?o+a:o):a=o,r){case 1:var s=-1;break;case 2:s=250;break;case 5:s=1073741823;break;case 4:s=1e4;break;default:s=5e3}return s=a+s,r={id:u++,callback:i,priorityLevel:r,startTime:a,expirationTime:s,sortIndex:-1},a>o?(r.sortIndex=a,t(l,r),n(c)===null&&r===n(l)&&(h?(v(C),C=-1):h=!0,A(x,a-o))):(r.sortIndex=s,t(c,r),m||p||(m=!0,S||(S=!0,O()))),r},e.unstable_shouldYield=E,e.unstable_wrapCallback=function(e){var t=f;return function(){var n=f;f=t;try{return e.apply(this,arguments)}finally{f=n}}}})),f=o(((e,t)=>{t.exports=d()})),p=o((e=>{var t=u();function n(e){var t=`https://react.dev/errors/`+e;if(1<arguments.length){t+=`?args[]=`+encodeURIComponent(arguments[1]);for(var n=2;n<arguments.length;n++)t+=`&args[]=`+encodeURIComponent(arguments[n])}return`Minified React error #`+e+`; visit `+t+` for the full message or use the non-minified dev environment for full errors and additional helpful warnings.`}function r(){}var i={d:{f:r,r:function(){throw Error(n(522))},D:r,C:r,L:r,m:r,X:r,S:r,M:r},p:0,findDOMNode:null},a=Symbol.for(`react.portal`);function o(e,t,n){var r=3<arguments.length&&arguments[3]!==void 0?arguments[3]:null;return{$$typeof:a,key:r==null?null:``+r,children:e,containerInfo:t,implementation:n}}var s=t.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE;function c(e,t){if(e===`font`)return``;if(typeof t==`string`)return t===`use-credentials`?t:``}e.__DOM_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE=i,e.createPortal=function(e,t){var r=2<arguments.length&&arguments[2]!==void 0?arguments[2]:null;if(!t||t.nodeType!==1&&t.nodeType!==9&&t.nodeType!==11)throw Error(n(299));return o(e,t,null,r)},e.flushSync=function(e){var t=s.T,n=i.p;try{if(s.T=null,i.p=2,e)return e()}finally{s.T=t,i.p=n,i.d.f()}},e.preconnect=function(e,t){typeof e==`string`&&(t?(t=t.crossOrigin,t=typeof t==`string`?t===`use-credentials`?t:``:void 0):t=null,i.d.C(e,t))},e.prefetchDNS=function(e){typeof e==`string`&&i.d.D(e)},e.preinit=function(e,t){if(typeof e==`string`&&t&&typeof t.as==`string`){var n=t.as,r=c(n,t.crossOrigin),a=typeof t.integrity==`string`?t.integrity:void 0,o=typeof t.fetchPriority==`string`?t.fetchPriority:void 0;n===`style`?i.d.S(e,typeof t.precedence==`string`?t.precedence:void 0,{crossOrigin:r,integrity:a,fetchPriority:o}):n===`script`&&i.d.X(e,{crossOrigin:r,integrity:a,fetchPriority:o,nonce:typeof t.nonce==`string`?t.nonce:void 0})}},e.preinitModule=function(e,t){if(typeof e==`string`)if(typeof t==`object`&&t){if(t.as==null||t.as===`script`){var n=c(t.as,t.crossOrigin);i.d.M(e,{crossOrigin:n,integrity:typeof t.integrity==`string`?t.integrity:void 0,nonce:typeof t.nonce==`string`?t.nonce:void 0})}}else t??i.d.M(e)},e.preload=function(e,t){if(typeof e==`string`&&typeof t==`object`&&t&&typeof t.as==`string`){var n=t.as,r=c(n,t.crossOrigin);i.d.L(e,n,{crossOrigin:r,integrity:typeof t.integrity==`string`?t.integrity:void 0,nonce:typeof t.nonce==`string`?t.nonce:void 0,type:typeof t.type==`string`?t.type:void 0,fetchPriority:typeof t.fetchPriority==`string`?t.fetchPriority:void 0,referrerPolicy:typeof t.referrerPolicy==`string`?t.referrerPolicy:void 0,imageSrcSet:typeof t.imageSrcSet==`string`?t.imageSrcSet:void 0,imageSizes:typeof t.imageSizes==`string`?t.imageSizes:void 0,media:typeof t.media==`string`?t.media:void 0})}},e.preloadModule=function(e,t){if(typeof e==`string`)if(t){var n=c(t.as,t.crossOrigin);i.d.m(e,{as:typeof t.as==`string`&&t.as!==`script`?t.as:void 0,crossOrigin:n,integrity:typeof t.integrity==`string`?t.integrity:void 0})}else i.d.m(e)},e.requestFormReset=function(e){i.d.r(e)},e.unstable_batchedUpdates=function(e,t){return e(t)},e.useFormState=function(e,t,n){return s.H.useFormState(e,t,n)},e.useFormStatus=function(){return s.H.useHostTransitionStatus()},e.version=`19.2.0`})),m=o(((e,t)=>{function n(){if(!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__>`u`||typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE!=`function`))try{__REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(n)}catch(e){console.error(e)}}n(),t.exports=p()})),h=o((e=>{var t=f(),n=u(),r=m();function i(e){var t=`https://react.dev/errors/`+e;if(1<arguments.length){t+=`?args[]=`+encodeURIComponent(arguments[1]);for(var n=2;n<arguments.length;n++)t+=`&args[]=`+encodeURIComponent(arguments[n])}return`Minified React error #`+e+`; visit `+t+` for the full message or use the non-minified dev environment for full errors and additional helpful warnings.`}function a(e){return!(!e||e.nodeType!==1&&e.nodeType!==9&&e.nodeType!==11)}function o(e){var t=e,n=e;if(e.alternate)for(;t.return;)t=t.return;else{e=t;do t=e,t.flags&4098&&(n=t.return),e=t.return;while(e)}return t.tag===3?n:null}function s(e){if(e.tag===13){var t=e.memoizedState;if(t===null&&(e=e.alternate,e!==null&&(t=e.memoizedState)),t!==null)return t.dehydrated}return null}function c(e){if(e.tag===31){var t=e.memoizedState;if(t===null&&(e=e.alternate,e!==null&&(t=e.memoizedState)),t!==null)return t.dehydrated}return null}function l(e){if(o(e)!==e)throw Error(i(188))}function d(e){var t=e.alternate;if(!t){if(t=o(e),t===null)throw Error(i(188));return t===e?e:null}for(var n=e,r=t;;){var a=n.return;if(a===null)break;var s=a.alternate;if(s===null){if(r=a.return,r!==null){n=r;continue}break}if(a.child===s.child){for(s=a.child;s;){if(s===n)return l(a),e;if(s===r)return l(a),t;s=s.sibling}throw Error(i(188))}if(n.return!==r.return)n=a,r=s;else{for(var c=!1,u=a.child;u;){if(u===n){c=!0,n=a,r=s;break}if(u===r){c=!0,r=a,n=s;break}u=u.sibling}if(!c){for(u=s.child;u;){if(u===n){c=!0,n=s,r=a;break}if(u===r){c=!0,r=s,n=a;break}u=u.sibling}if(!c)throw Error(i(189))}}if(n.alternate!==r)throw Error(i(190))}if(n.tag!==3)throw Error(i(188));return n.stateNode.current===n?e:t}function p(e){var t=e.tag;if(t===5||t===26||t===27||t===6)return e;for(e=e.child;e!==null;){if(t=p(e),t!==null)return t;e=e.sibling}return null}var h=Object.assign,g=Symbol.for(`react.element`),_=Symbol.for(`react.transitional.element`),v=Symbol.for(`react.portal`),y=Symbol.for(`react.fragment`),b=Symbol.for(`react.strict_mode`),x=Symbol.for(`react.profiler`),S=Symbol.for(`react.consumer`),C=Symbol.for(`react.context`),w=Symbol.for(`react.forward_ref`),T=Symbol.for(`react.suspense`),E=Symbol.for(`react.suspense_list`),D=Symbol.for(`react.memo`),O=Symbol.for(`react.lazy`),k=Symbol.for(`react.activity`),ee=Symbol.for(`react.memo_cache_sentinel`),A=Symbol.iterator;function j(e){return typeof e!=`object`||!e?null:(e=A&&e[A]||e[`@@iterator`],typeof e==`function`?e:null)}var M=Symbol.for(`react.client.reference`);function te(e){if(e==null)return null;if(typeof e==`function`)return e.$$typeof===M?null:e.displayName||e.name||null;if(typeof e==`string`)return e;switch(e){case y:return`Fragment`;case x:return`Profiler`;case b:return`StrictMode`;case T:return`Suspense`;case E:return`SuspenseList`;case k:return`Activity`}if(typeof e==`object`)switch(e.$$typeof){case v:return`Portal`;case C:return e.displayName||`Context`;case S:return(e._context.displayName||`Context`)+`.Consumer`;case w:var t=e.render;return e=e.displayName,e||=(e=t.displayName||t.name||``,e===``?`ForwardRef`:`ForwardRef(`+e+`)`),e;case D:return t=e.displayName||null,t===null?te(e.type)||`Memo`:t;case O:t=e._payload,e=e._init;try{return te(e(t))}catch{}}return null}var N=Array.isArray,P=n.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE,F=r.__DOM_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE,ne={pending:!1,data:null,method:null,action:null},re=[],ie=-1;function ae(e){return{current:e}}function oe(e){0>ie||(e.current=re[ie],re[ie]=null,ie--)}function I(e,t){ie++,re[ie]=e.current,e.current=t}var se=ae(null),ce=ae(null),le=ae(null),ue=ae(null);function de(e,t){switch(I(le,t),I(ce,e),I(se,null),t.nodeType){case 9:case 11:e=(e=t.documentElement)&&(e=e.namespaceURI)?Zd(e):0;break;default:if(e=t.tagName,t=t.namespaceURI)t=Zd(t),e=Qd(t,e);else switch(e){case`svg`:e=1;break;case`math`:e=2;break;default:e=0}}oe(se),I(se,e)}function fe(){oe(se),oe(ce),oe(le)}function pe(e){e.memoizedState!==null&&I(ue,e);var t=se.current,n=Qd(t,e.type);t!==n&&(I(ce,e),I(se,n))}function me(e){ce.current===e&&(oe(se),oe(ce)),ue.current===e&&(oe(ue),cp._currentValue=ne)}var he,ge;function _e(e){if(he===void 0)try{throw Error()}catch(e){var t=e.stack.trim().match(/\n( *(at )?)/);he=t&&t[1]||``,ge=-1<e.stack.indexOf(`
    at`)?` (<anonymous>)`:-1<e.stack.indexOf(`@`)?`@unknown:0:0`:``}return`
`+he+e+ge}var ve=!1;function ye(e,t){if(!e||ve)return``;ve=!0;var n=Error.prepareStackTrace;Error.prepareStackTrace=void 0;try{var r={DetermineComponentFrameRoot:function(){try{if(t){var n=function(){throw Error()};if(Object.defineProperty(n.prototype,`props`,{set:function(){throw Error()}}),typeof Reflect==`object`&&Reflect.construct){try{Reflect.construct(n,[])}catch(e){var r=e}Reflect.construct(e,[],n)}else{try{n.call()}catch(e){r=e}e.call(n.prototype)}}else{try{throw Error()}catch(e){r=e}(n=e())&&typeof n.catch==`function`&&n.catch(function(){})}}catch(e){if(e&&r&&typeof e.stack==`string`)return[e.stack,r.stack]}return[null,null]}};r.DetermineComponentFrameRoot.displayName=`DetermineComponentFrameRoot`;var i=Object.getOwnPropertyDescriptor(r.DetermineComponentFrameRoot,`name`);i&&i.configurable&&Object.defineProperty(r.DetermineComponentFrameRoot,`name`,{value:`DetermineComponentFrameRoot`});var a=r.DetermineComponentFrameRoot(),o=a[0],s=a[1];if(o&&s){var c=o.split(`
`),l=s.split(`
`);for(i=r=0;r<c.length&&!c[r].includes(`DetermineComponentFrameRoot`);)r++;for(;i<l.length&&!l[i].includes(`DetermineComponentFrameRoot`);)i++;if(r===c.length||i===l.length)for(r=c.length-1,i=l.length-1;1<=r&&0<=i&&c[r]!==l[i];)i--;for(;1<=r&&0<=i;r--,i--)if(c[r]!==l[i]){if(r!==1||i!==1)do if(r--,i--,0>i||c[r]!==l[i]){var u=`
`+c[r].replace(` at new `,` at `);return e.displayName&&u.includes(`<anonymous>`)&&(u=u.replace(`<anonymous>`,e.displayName)),u}while(1<=r&&0<=i);break}}}finally{ve=!1,Error.prepareStackTrace=n}return(n=e?e.displayName||e.name:``)?_e(n):``}function be(e,t){switch(e.tag){case 26:case 27:case 5:return _e(e.type);case 16:return _e(`Lazy`);case 13:return e.child!==t&&t!==null?_e(`Suspense Fallback`):_e(`Suspense`);case 19:return _e(`SuspenseList`);case 0:case 15:return ye(e.type,!1);case 11:return ye(e.type.render,!1);case 1:return ye(e.type,!0);case 31:return _e(`Activity`);default:return``}}function xe(e){try{var t=``,n=null;do t+=be(e,n),n=e,e=e.return;while(e);return t}catch(e){return`
Error generating stack: `+e.message+`
`+e.stack}}var Se=Object.prototype.hasOwnProperty,Ce=t.unstable_scheduleCallback,we=t.unstable_cancelCallback,Te=t.unstable_shouldYield,Ee=t.unstable_requestPaint,De=t.unstable_now,Oe=t.unstable_getCurrentPriorityLevel,ke=t.unstable_ImmediatePriority,Ae=t.unstable_UserBlockingPriority,je=t.unstable_NormalPriority,Me=t.unstable_LowPriority,Ne=t.unstable_IdlePriority,Pe=t.log,Fe=t.unstable_setDisableYieldValue,Ie=null,Le=null;function Re(e){if(typeof Pe==`function`&&Fe(e),Le&&typeof Le.setStrictMode==`function`)try{Le.setStrictMode(Ie,e)}catch{}}var ze=Math.clz32?Math.clz32:He,Be=Math.log,Ve=Math.LN2;function He(e){return e>>>=0,e===0?32:31-(Be(e)/Ve|0)|0}var Ue=256,We=262144,Ge=4194304;function Ke(e){var t=e&42;if(t!==0)return t;switch(e&-e){case 1:return 1;case 2:return 2;case 4:return 4;case 8:return 8;case 16:return 16;case 32:return 32;case 64:return 64;case 128:return 128;case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:return e&261888;case 262144:case 524288:case 1048576:case 2097152:return e&3932160;case 4194304:case 8388608:case 16777216:case 33554432:return e&62914560;case 67108864:return 67108864;case 134217728:return 134217728;case 268435456:return 268435456;case 536870912:return 536870912;case 1073741824:return 0;default:return e}}function qe(e,t,n){var r=e.pendingLanes;if(r===0)return 0;var i=0,a=e.suspendedLanes,o=e.pingedLanes;e=e.warmLanes;var s=r&134217727;return s===0?(s=r&~a,s===0?o===0?n||(n=r&~e,n!==0&&(i=Ke(n))):i=Ke(o):i=Ke(s)):(r=s&~a,r===0?(o&=s,o===0?n||(n=s&~e,n!==0&&(i=Ke(n))):i=Ke(o)):i=Ke(r)),i===0?0:t!==0&&t!==i&&(t&a)===0&&(a=i&-i,n=t&-t,a>=n||a===32&&n&4194048)?t:i}function Je(e,t){return(e.pendingLanes&~(e.suspendedLanes&~e.pingedLanes)&t)===0}function Ye(e,t){switch(e){case 1:case 2:case 4:case 8:case 64:return t+250;case 16:case 32:case 128:case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:case 262144:case 524288:case 1048576:case 2097152:return t+5e3;case 4194304:case 8388608:case 16777216:case 33554432:return-1;case 67108864:case 134217728:case 268435456:case 536870912:case 1073741824:return-1;default:return-1}}function Xe(){var e=Ge;return Ge<<=1,!(Ge&62914560)&&(Ge=4194304),e}function Ze(e){for(var t=[],n=0;31>n;n++)t.push(e);return t}function Qe(e,t){e.pendingLanes|=t,t!==268435456&&(e.suspendedLanes=0,e.pingedLanes=0,e.warmLanes=0)}function $e(e,t,n,r,i,a){var o=e.pendingLanes;e.pendingLanes=n,e.suspendedLanes=0,e.pingedLanes=0,e.warmLanes=0,e.expiredLanes&=n,e.entangledLanes&=n,e.errorRecoveryDisabledLanes&=n,e.shellSuspendCounter=0;var s=e.entanglements,c=e.expirationTimes,l=e.hiddenUpdates;for(n=o&~n;0<n;){var u=31-ze(n),d=1<<u;s[u]=0,c[u]=-1;var f=l[u];if(f!==null)for(l[u]=null,u=0;u<f.length;u++){var p=f[u];p!==null&&(p.lane&=-536870913)}n&=~d}r!==0&&et(e,r,0),a!==0&&i===0&&e.tag!==0&&(e.suspendedLanes|=a&~(o&~t))}function et(e,t,n){e.pendingLanes|=t,e.suspendedLanes&=~t;var r=31-ze(t);e.entangledLanes|=t,e.entanglements[r]=e.entanglements[r]|1073741824|n&261930}function tt(e,t){var n=e.entangledLanes|=t;for(e=e.entanglements;n;){var r=31-ze(n),i=1<<r;i&t|e[r]&t&&(e[r]|=t),n&=~i}}function nt(e,t){var n=t&-t;return n=n&42?1:rt(n),(n&(e.suspendedLanes|t))===0?n:0}function rt(e){switch(e){case 2:e=1;break;case 8:e=4;break;case 32:e=16;break;case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:case 262144:case 524288:case 1048576:case 2097152:case 4194304:case 8388608:case 16777216:case 33554432:e=128;break;case 268435456:e=134217728;break;default:e=0}return e}function it(e){return e&=-e,2<e?8<e?e&134217727?32:268435456:8:2}function at(){var e=F.p;return e===0?(e=window.event,e===void 0?32:wp(e.type)):e}function ot(e,t){var n=F.p;try{return F.p=e,t()}finally{F.p=n}}var st=Math.random().toString(36).slice(2),ct=`__reactFiber$`+st,lt=`__reactProps$`+st,ut=`__reactContainer$`+st,dt=`__reactEvents$`+st,ft=`__reactListeners$`+st,pt=`__reactHandles$`+st,mt=`__reactResources$`+st,ht=`__reactMarker$`+st;function gt(e){delete e[ct],delete e[lt],delete e[dt],delete e[ft],delete e[pt]}function _t(e){var t=e[ct];if(t)return t;for(var n=e.parentNode;n;){if(t=n[ut]||n[ct]){if(n=t.alternate,t.child!==null||n!==null&&n.child!==null)for(e=xf(e);e!==null;){if(n=e[ct])return n;e=xf(e)}return t}e=n,n=e.parentNode}return null}function vt(e){if(e=e[ct]||e[ut]){var t=e.tag;if(t===5||t===6||t===13||t===31||t===26||t===27||t===3)return e}return null}function yt(e){var t=e.tag;if(t===5||t===26||t===27||t===6)return e.stateNode;throw Error(i(33))}function bt(e){var t=e[mt];return t||=e[mt]={hoistableStyles:new Map,hoistableScripts:new Map},t}function xt(e){e[ht]=!0}var St=new Set,Ct={};function wt(e,t){Tt(e,t),Tt(e+`Capture`,t)}function Tt(e,t){for(Ct[e]=t,e=0;e<t.length;e++)St.add(t[e])}var Et=RegExp(`^[:A-Z_a-z\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD][:A-Z_a-z\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD\\-.0-9\\u00B7\\u0300-\\u036F\\u203F-\\u2040]*$`),Dt={},Ot={};function kt(e){return Se.call(Ot,e)?!0:Se.call(Dt,e)?!1:Et.test(e)?Ot[e]=!0:(Dt[e]=!0,!1)}function At(e,t,n){if(kt(t))if(n===null)e.removeAttribute(t);else{switch(typeof n){case`undefined`:case`function`:case`symbol`:e.removeAttribute(t);return;case`boolean`:var r=t.toLowerCase().slice(0,5);if(r!==`data-`&&r!==`aria-`){e.removeAttribute(t);return}}e.setAttribute(t,``+n)}}function jt(e,t,n){if(n===null)e.removeAttribute(t);else{switch(typeof n){case`undefined`:case`function`:case`symbol`:case`boolean`:e.removeAttribute(t);return}e.setAttribute(t,``+n)}}function Mt(e,t,n,r){if(r===null)e.removeAttribute(n);else{switch(typeof r){case`undefined`:case`function`:case`symbol`:case`boolean`:e.removeAttribute(n);return}e.setAttributeNS(t,n,``+r)}}function Nt(e){switch(typeof e){case`bigint`:case`boolean`:case`number`:case`string`:case`undefined`:return e;case`object`:return e;default:return``}}function Pt(e){var t=e.type;return(e=e.nodeName)&&e.toLowerCase()===`input`&&(t===`checkbox`||t===`radio`)}function Ft(e,t,n){var r=Object.getOwnPropertyDescriptor(e.constructor.prototype,t);if(!e.hasOwnProperty(t)&&r!==void 0&&typeof r.get==`function`&&typeof r.set==`function`){var i=r.get,a=r.set;return Object.defineProperty(e,t,{configurable:!0,get:function(){return i.call(this)},set:function(e){n=``+e,a.call(this,e)}}),Object.defineProperty(e,t,{enumerable:r.enumerable}),{getValue:function(){return n},setValue:function(e){n=``+e},stopTracking:function(){e._valueTracker=null,delete e[t]}}}}function It(e){if(!e._valueTracker){var t=Pt(e)?`checked`:`value`;e._valueTracker=Ft(e,t,``+e[t])}}function Lt(e){if(!e)return!1;var t=e._valueTracker;if(!t)return!0;var n=t.getValue(),r=``;return e&&(r=Pt(e)?e.checked?`true`:`false`:e.value),e=r,e===n?!1:(t.setValue(e),!0)}function Rt(e){if(e||=typeof document<`u`?document:void 0,e===void 0)return null;try{return e.activeElement||e.body}catch{return e.body}}var zt=/[\n"\\]/g;function Bt(e){return e.replace(zt,function(e){return`\\`+e.charCodeAt(0).toString(16)+` `})}function Vt(e,t,n,r,i,a,o,s){e.name=``,o!=null&&typeof o!=`function`&&typeof o!=`symbol`&&typeof o!=`boolean`?e.type=o:e.removeAttribute(`type`),t==null?o!==`submit`&&o!==`reset`||e.removeAttribute(`value`):o===`number`?(t===0&&e.value===``||e.value!=t)&&(e.value=``+Nt(t)):e.value!==``+Nt(t)&&(e.value=``+Nt(t)),t==null?n==null?r!=null&&e.removeAttribute(`value`):Ut(e,o,Nt(n)):Ut(e,o,Nt(t)),i==null&&a!=null&&(e.defaultChecked=!!a),i!=null&&(e.checked=i&&typeof i!=`function`&&typeof i!=`symbol`),s!=null&&typeof s!=`function`&&typeof s!=`symbol`&&typeof s!=`boolean`?e.name=``+Nt(s):e.removeAttribute(`name`)}function Ht(e,t,n,r,i,a,o,s){if(a!=null&&typeof a!=`function`&&typeof a!=`symbol`&&typeof a!=`boolean`&&(e.type=a),t!=null||n!=null){if(!(a!==`submit`&&a!==`reset`||t!=null)){It(e);return}n=n==null?``:``+Nt(n),t=t==null?n:``+Nt(t),s||t===e.value||(e.value=t),e.defaultValue=t}r??=i,r=typeof r!=`function`&&typeof r!=`symbol`&&!!r,e.checked=s?e.checked:!!r,e.defaultChecked=!!r,o!=null&&typeof o!=`function`&&typeof o!=`symbol`&&typeof o!=`boolean`&&(e.name=o),It(e)}function Ut(e,t,n){t===`number`&&Rt(e.ownerDocument)===e||e.defaultValue===``+n||(e.defaultValue=``+n)}function Wt(e,t,n,r){if(e=e.options,t){t={};for(var i=0;i<n.length;i++)t[`$`+n[i]]=!0;for(n=0;n<e.length;n++)i=t.hasOwnProperty(`$`+e[n].value),e[n].selected!==i&&(e[n].selected=i),i&&r&&(e[n].defaultSelected=!0)}else{for(n=``+Nt(n),t=null,i=0;i<e.length;i++){if(e[i].value===n){e[i].selected=!0,r&&(e[i].defaultSelected=!0);return}t!==null||e[i].disabled||(t=e[i])}t!==null&&(t.selected=!0)}}function Gt(e,t,n){if(t!=null&&(t=``+Nt(t),t!==e.value&&(e.value=t),n==null)){e.defaultValue!==t&&(e.defaultValue=t);return}e.defaultValue=n==null?``:``+Nt(n)}function Kt(e,t,n,r){if(t==null){if(r!=null){if(n!=null)throw Error(i(92));if(N(r)){if(1<r.length)throw Error(i(93));r=r[0]}n=r}n??=``,t=n}n=Nt(t),e.defaultValue=n,r=e.textContent,r===n&&r!==``&&r!==null&&(e.value=r),It(e)}function qt(e,t){if(t){var n=e.firstChild;if(n&&n===e.lastChild&&n.nodeType===3){n.nodeValue=t;return}}e.textContent=t}var Jt=new Set(`animationIterationCount aspectRatio borderImageOutset borderImageSlice borderImageWidth boxFlex boxFlexGroup boxOrdinalGroup columnCount columns flex flexGrow flexPositive flexShrink flexNegative flexOrder gridArea gridRow gridRowEnd gridRowSpan gridRowStart gridColumn gridColumnEnd gridColumnSpan gridColumnStart fontWeight lineClamp lineHeight opacity order orphans scale tabSize widows zIndex zoom fillOpacity floodOpacity stopOpacity strokeDasharray strokeDashoffset strokeMiterlimit strokeOpacity strokeWidth MozAnimationIterationCount MozBoxFlex MozBoxFlexGroup MozLineClamp msAnimationIterationCount msFlex msZoom msFlexGrow msFlexNegative msFlexOrder msFlexPositive msFlexShrink msGridColumn msGridColumnSpan msGridRow msGridRowSpan WebkitAnimationIterationCount WebkitBoxFlex WebKitBoxFlexGroup WebkitBoxOrdinalGroup WebkitColumnCount WebkitColumns WebkitFlex WebkitFlexGrow WebkitFlexPositive WebkitFlexShrink WebkitLineClamp`.split(` `));function Yt(e,t,n){var r=t.indexOf(`--`)===0;n==null||typeof n==`boolean`||n===``?r?e.setProperty(t,``):t===`float`?e.cssFloat=``:e[t]=``:r?e.setProperty(t,n):typeof n!=`number`||n===0||Jt.has(t)?t===`float`?e.cssFloat=n:e[t]=(``+n).trim():e[t]=n+`px`}function Xt(e,t,n){if(t!=null&&typeof t!=`object`)throw Error(i(62));if(e=e.style,n!=null){for(var r in n)!n.hasOwnProperty(r)||t!=null&&t.hasOwnProperty(r)||(r.indexOf(`--`)===0?e.setProperty(r,``):r===`float`?e.cssFloat=``:e[r]=``);for(var a in t)r=t[a],t.hasOwnProperty(a)&&n[a]!==r&&Yt(e,a,r)}else for(var o in t)t.hasOwnProperty(o)&&Yt(e,o,t[o])}function Zt(e){if(e.indexOf(`-`)===-1)return!1;switch(e){case`annotation-xml`:case`color-profile`:case`font-face`:case`font-face-src`:case`font-face-uri`:case`font-face-format`:case`font-face-name`:case`missing-glyph`:return!1;default:return!0}}var Qt=new Map([[`acceptCharset`,`accept-charset`],[`htmlFor`,`for`],[`httpEquiv`,`http-equiv`],[`crossOrigin`,`crossorigin`],[`accentHeight`,`accent-height`],[`alignmentBaseline`,`alignment-baseline`],[`arabicForm`,`arabic-form`],[`baselineShift`,`baseline-shift`],[`capHeight`,`cap-height`],[`clipPath`,`clip-path`],[`clipRule`,`clip-rule`],[`colorInterpolation`,`color-interpolation`],[`colorInterpolationFilters`,`color-interpolation-filters`],[`colorProfile`,`color-profile`],[`colorRendering`,`color-rendering`],[`dominantBaseline`,`dominant-baseline`],[`enableBackground`,`enable-background`],[`fillOpacity`,`fill-opacity`],[`fillRule`,`fill-rule`],[`floodColor`,`flood-color`],[`floodOpacity`,`flood-opacity`],[`fontFamily`,`font-family`],[`fontSize`,`font-size`],[`fontSizeAdjust`,`font-size-adjust`],[`fontStretch`,`font-stretch`],[`fontStyle`,`font-style`],[`fontVariant`,`font-variant`],[`fontWeight`,`font-weight`],[`glyphName`,`glyph-name`],[`glyphOrientationHorizontal`,`glyph-orientation-horizontal`],[`glyphOrientationVertical`,`glyph-orientation-vertical`],[`horizAdvX`,`horiz-adv-x`],[`horizOriginX`,`horiz-origin-x`],[`imageRendering`,`image-rendering`],[`letterSpacing`,`letter-spacing`],[`lightingColor`,`lighting-color`],[`markerEnd`,`marker-end`],[`markerMid`,`marker-mid`],[`markerStart`,`marker-start`],[`overlinePosition`,`overline-position`],[`overlineThickness`,`overline-thickness`],[`paintOrder`,`paint-order`],[`panose-1`,`panose-1`],[`pointerEvents`,`pointer-events`],[`renderingIntent`,`rendering-intent`],[`shapeRendering`,`shape-rendering`],[`stopColor`,`stop-color`],[`stopOpacity`,`stop-opacity`],[`strikethroughPosition`,`strikethrough-position`],[`strikethroughThickness`,`strikethrough-thickness`],[`strokeDasharray`,`stroke-dasharray`],[`strokeDashoffset`,`stroke-dashoffset`],[`strokeLinecap`,`stroke-linecap`],[`strokeLinejoin`,`stroke-linejoin`],[`strokeMiterlimit`,`stroke-miterlimit`],[`strokeOpacity`,`stroke-opacity`],[`strokeWidth`,`stroke-width`],[`textAnchor`,`text-anchor`],[`textDecoration`,`text-decoration`],[`textRendering`,`text-rendering`],[`transformOrigin`,`transform-origin`],[`underlinePosition`,`underline-position`],[`underlineThickness`,`underline-thickness`],[`unicodeBidi`,`unicode-bidi`],[`unicodeRange`,`unicode-range`],[`unitsPerEm`,`units-per-em`],[`vAlphabetic`,`v-alphabetic`],[`vHanging`,`v-hanging`],[`vIdeographic`,`v-ideographic`],[`vMathematical`,`v-mathematical`],[`vectorEffect`,`vector-effect`],[`vertAdvY`,`vert-adv-y`],[`vertOriginX`,`vert-origin-x`],[`vertOriginY`,`vert-origin-y`],[`wordSpacing`,`word-spacing`],[`writingMode`,`writing-mode`],[`xmlnsXlink`,`xmlns:xlink`],[`xHeight`,`x-height`]]),$t=/^[\u0000-\u001F ]*j[\r\n\t]*a[\r\n\t]*v[\r\n\t]*a[\r\n\t]*s[\r\n\t]*c[\r\n\t]*r[\r\n\t]*i[\r\n\t]*p[\r\n\t]*t[\r\n\t]*:/i;function en(e){return $t.test(``+e)?`javascript:throw new Error('React has blocked a javascript: URL as a security precaution.')`:e}function tn(){}var nn=null;function rn(e){return e=e.target||e.srcElement||window,e.correspondingUseElement&&(e=e.correspondingUseElement),e.nodeType===3?e.parentNode:e}var an=null,on=null;function sn(e){var t=vt(e);if(t&&(e=t.stateNode)){var n=e[lt]||null;a:switch(e=t.stateNode,t.type){case`input`:if(Vt(e,n.value,n.defaultValue,n.defaultValue,n.checked,n.defaultChecked,n.type,n.name),t=n.name,n.type===`radio`&&t!=null){for(n=e;n.parentNode;)n=n.parentNode;for(n=n.querySelectorAll(`input[name="`+Bt(``+t)+`"][type="radio"]`),t=0;t<n.length;t++){var r=n[t];if(r!==e&&r.form===e.form){var a=r[lt]||null;if(!a)throw Error(i(90));Vt(r,a.value,a.defaultValue,a.defaultValue,a.checked,a.defaultChecked,a.type,a.name)}}for(t=0;t<n.length;t++)r=n[t],r.form===e.form&&Lt(r)}break a;case`textarea`:Gt(e,n.value,n.defaultValue);break a;case`select`:t=n.value,t!=null&&Wt(e,!!n.multiple,t,!1)}}}var cn=!1;function ln(e,t,n){if(cn)return e(t,n);cn=!0;try{return e(t)}finally{if(cn=!1,(an!==null||on!==null)&&(Ou(),an&&(t=an,e=on,on=an=null,sn(t),e)))for(t=0;t<e.length;t++)sn(e[t])}}function un(e,t){var n=e.stateNode;if(n===null)return null;var r=n[lt]||null;if(r===null)return null;n=r[t];a:switch(t){case`onClick`:case`onClickCapture`:case`onDoubleClick`:case`onDoubleClickCapture`:case`onMouseDown`:case`onMouseDownCapture`:case`onMouseMove`:case`onMouseMoveCapture`:case`onMouseUp`:case`onMouseUpCapture`:case`onMouseEnter`:(r=!r.disabled)||(e=e.type,r=!(e===`button`||e===`input`||e===`select`||e===`textarea`)),e=!r;break a;default:e=!1}if(e)return null;if(n&&typeof n!=`function`)throw Error(i(231,t,typeof n));return n}var dn=!(typeof window>`u`||window.document===void 0||window.document.createElement===void 0),fn=!1;if(dn)try{var pn={};Object.defineProperty(pn,`passive`,{get:function(){fn=!0}}),window.addEventListener(`test`,pn,pn),window.removeEventListener(`test`,pn,pn)}catch{fn=!1}var mn=null,hn=null,gn=null;function _n(){if(gn)return gn;var e,t=hn,n=t.length,r,i=`value`in mn?mn.value:mn.textContent,a=i.length;for(e=0;e<n&&t[e]===i[e];e++);var o=n-e;for(r=1;r<=o&&t[n-r]===i[a-r];r++);return gn=i.slice(e,1<r?1-r:void 0)}function vn(e){var t=e.keyCode;return`charCode`in e?(e=e.charCode,e===0&&t===13&&(e=13)):e=t,e===10&&(e=13),32<=e||e===13?e:0}function yn(){return!0}function bn(){return!1}function xn(e){function t(t,n,r,i,a){for(var o in this._reactName=t,this._targetInst=r,this.type=n,this.nativeEvent=i,this.target=a,this.currentTarget=null,e)e.hasOwnProperty(o)&&(t=e[o],this[o]=t?t(i):i[o]);return this.isDefaultPrevented=(i.defaultPrevented==null?!1===i.returnValue:i.defaultPrevented)?yn:bn,this.isPropagationStopped=bn,this}return h(t.prototype,{preventDefault:function(){this.defaultPrevented=!0;var e=this.nativeEvent;e&&(e.preventDefault?e.preventDefault():typeof e.returnValue!=`unknown`&&(e.returnValue=!1),this.isDefaultPrevented=yn)},stopPropagation:function(){var e=this.nativeEvent;e&&(e.stopPropagation?e.stopPropagation():typeof e.cancelBubble!=`unknown`&&(e.cancelBubble=!0),this.isPropagationStopped=yn)},persist:function(){},isPersistent:yn}),t}var Sn={eventPhase:0,bubbles:0,cancelable:0,timeStamp:function(e){return e.timeStamp||Date.now()},defaultPrevented:0,isTrusted:0},Cn=xn(Sn),wn=h({},Sn,{view:0,detail:0}),Tn=xn(wn),En,Dn,On,kn=h({},wn,{screenX:0,screenY:0,clientX:0,clientY:0,pageX:0,pageY:0,ctrlKey:0,shiftKey:0,altKey:0,metaKey:0,getModifierState:Bn,button:0,buttons:0,relatedTarget:function(e){return e.relatedTarget===void 0?e.fromElement===e.srcElement?e.toElement:e.fromElement:e.relatedTarget},movementX:function(e){return`movementX`in e?e.movementX:(e!==On&&(On&&e.type===`mousemove`?(En=e.screenX-On.screenX,Dn=e.screenY-On.screenY):Dn=En=0,On=e),En)},movementY:function(e){return`movementY`in e?e.movementY:Dn}}),An=xn(kn),jn=xn(h({},kn,{dataTransfer:0})),Mn=xn(h({},wn,{relatedTarget:0})),Nn=xn(h({},Sn,{animationName:0,elapsedTime:0,pseudoElement:0})),Pn=xn(h({},Sn,{clipboardData:function(e){return`clipboardData`in e?e.clipboardData:window.clipboardData}})),Fn=xn(h({},Sn,{data:0})),In={Esc:`Escape`,Spacebar:` `,Left:`ArrowLeft`,Up:`ArrowUp`,Right:`ArrowRight`,Down:`ArrowDown`,Del:`Delete`,Win:`OS`,Menu:`ContextMenu`,Apps:`ContextMenu`,Scroll:`ScrollLock`,MozPrintableKey:`Unidentified`},Ln={8:`Backspace`,9:`Tab`,12:`Clear`,13:`Enter`,16:`Shift`,17:`Control`,18:`Alt`,19:`Pause`,20:`CapsLock`,27:`Escape`,32:` `,33:`PageUp`,34:`PageDown`,35:`End`,36:`Home`,37:`ArrowLeft`,38:`ArrowUp`,39:`ArrowRight`,40:`ArrowDown`,45:`Insert`,46:`Delete`,112:`F1`,113:`F2`,114:`F3`,115:`F4`,116:`F5`,117:`F6`,118:`F7`,119:`F8`,120:`F9`,121:`F10`,122:`F11`,123:`F12`,144:`NumLock`,145:`ScrollLock`,224:`Meta`},Rn={Alt:`altKey`,Control:`ctrlKey`,Meta:`metaKey`,Shift:`shiftKey`};function zn(e){var t=this.nativeEvent;return t.getModifierState?t.getModifierState(e):(e=Rn[e])?!!t[e]:!1}function Bn(){return zn}var Vn=xn(h({},wn,{key:function(e){if(e.key){var t=In[e.key]||e.key;if(t!==`Unidentified`)return t}return e.type===`keypress`?(e=vn(e),e===13?`Enter`:String.fromCharCode(e)):e.type===`keydown`||e.type===`keyup`?Ln[e.keyCode]||`Unidentified`:``},code:0,location:0,ctrlKey:0,shiftKey:0,altKey:0,metaKey:0,repeat:0,locale:0,getModifierState:Bn,charCode:function(e){return e.type===`keypress`?vn(e):0},keyCode:function(e){return e.type===`keydown`||e.type===`keyup`?e.keyCode:0},which:function(e){return e.type===`keypress`?vn(e):e.type===`keydown`||e.type===`keyup`?e.keyCode:0}})),Hn=xn(h({},kn,{pointerId:0,width:0,height:0,pressure:0,tangentialPressure:0,tiltX:0,tiltY:0,twist:0,pointerType:0,isPrimary:0})),Un=xn(h({},wn,{touches:0,targetTouches:0,changedTouches:0,altKey:0,metaKey:0,ctrlKey:0,shiftKey:0,getModifierState:Bn})),Wn=xn(h({},Sn,{propertyName:0,elapsedTime:0,pseudoElement:0})),Gn=xn(h({},kn,{deltaX:function(e){return`deltaX`in e?e.deltaX:`wheelDeltaX`in e?-e.wheelDeltaX:0},deltaY:function(e){return`deltaY`in e?e.deltaY:`wheelDeltaY`in e?-e.wheelDeltaY:`wheelDelta`in e?-e.wheelDelta:0},deltaZ:0,deltaMode:0})),Kn=xn(h({},Sn,{newState:0,oldState:0})),qn=[9,13,27,32],Jn=dn&&`CompositionEvent`in window,Yn=null;dn&&`documentMode`in document&&(Yn=document.documentMode);var Xn=dn&&`TextEvent`in window&&!Yn,Zn=dn&&(!Jn||Yn&&8<Yn&&11>=Yn),Qn=` `,$n=!1;function er(e,t){switch(e){case`keyup`:return qn.indexOf(t.keyCode)!==-1;case`keydown`:return t.keyCode!==229;case`keypress`:case`mousedown`:case`focusout`:return!0;default:return!1}}function tr(e){return e=e.detail,typeof e==`object`&&`data`in e?e.data:null}var nr=!1;function rr(e,t){switch(e){case`compositionend`:return tr(t);case`keypress`:return t.which===32?($n=!0,Qn):null;case`textInput`:return e=t.data,e===Qn&&$n?null:e;default:return null}}function ir(e,t){if(nr)return e===`compositionend`||!Jn&&er(e,t)?(e=_n(),gn=hn=mn=null,nr=!1,e):null;switch(e){case`paste`:return null;case`keypress`:if(!(t.ctrlKey||t.altKey||t.metaKey)||t.ctrlKey&&t.altKey){if(t.char&&1<t.char.length)return t.char;if(t.which)return String.fromCharCode(t.which)}return null;case`compositionend`:return Zn&&t.locale!==`ko`?null:t.data;default:return null}}var ar={color:!0,date:!0,datetime:!0,"datetime-local":!0,email:!0,month:!0,number:!0,password:!0,range:!0,search:!0,tel:!0,text:!0,time:!0,url:!0,week:!0};function or(e){var t=e&&e.nodeName&&e.nodeName.toLowerCase();return t===`input`?!!ar[e.type]:t===`textarea`}function sr(e,t,n,r){an?on?on.push(r):on=[r]:an=r,t=Id(t,`onChange`),0<t.length&&(n=new Cn(`onChange`,`change`,null,n,r),e.push({event:n,listeners:t}))}var cr=null,lr=null;function ur(e){Od(e,0)}function dr(e){if(Lt(yt(e)))return e}function fr(e,t){if(e===`change`)return t}var pr=!1;if(dn){var mr;if(dn){var hr=`oninput`in document;if(!hr){var gr=document.createElement(`div`);gr.setAttribute(`oninput`,`return;`),hr=typeof gr.oninput==`function`}mr=hr}else mr=!1;pr=mr&&(!document.documentMode||9<document.documentMode)}function _r(){cr&&(cr.detachEvent(`onpropertychange`,vr),lr=cr=null)}function vr(e){if(e.propertyName===`value`&&dr(lr)){var t=[];sr(t,lr,e,rn(e)),ln(ur,t)}}function yr(e,t,n){e===`focusin`?(_r(),cr=t,lr=n,cr.attachEvent(`onpropertychange`,vr)):e===`focusout`&&_r()}function br(e){if(e===`selectionchange`||e===`keyup`||e===`keydown`)return dr(lr)}function xr(e,t){if(e===`click`)return dr(t)}function Sr(e,t){if(e===`input`||e===`change`)return dr(t)}function Cr(e,t){return e===t&&(e!==0||1/e==1/t)||e!==e&&t!==t}var wr=typeof Object.is==`function`?Object.is:Cr;function Tr(e,t){if(wr(e,t))return!0;if(typeof e!=`object`||!e||typeof t!=`object`||!t)return!1;var n=Object.keys(e),r=Object.keys(t);if(n.length!==r.length)return!1;for(r=0;r<n.length;r++){var i=n[r];if(!Se.call(t,i)||!wr(e[i],t[i]))return!1}return!0}function Er(e){for(;e&&e.firstChild;)e=e.firstChild;return e}function Dr(e,t){var n=Er(e);e=0;for(var r;n;){if(n.nodeType===3){if(r=e+n.textContent.length,e<=t&&r>=t)return{node:n,offset:t-e};e=r}a:{for(;n;){if(n.nextSibling){n=n.nextSibling;break a}n=n.parentNode}n=void 0}n=Er(n)}}function Or(e,t){return e&&t?e===t?!0:e&&e.nodeType===3?!1:t&&t.nodeType===3?Or(e,t.parentNode):`contains`in e?e.contains(t):e.compareDocumentPosition?!!(e.compareDocumentPosition(t)&16):!1:!1}function kr(e){e=e!=null&&e.ownerDocument!=null&&e.ownerDocument.defaultView!=null?e.ownerDocument.defaultView:window;for(var t=Rt(e.document);t instanceof e.HTMLIFrameElement;){try{var n=typeof t.contentWindow.location.href==`string`}catch{n=!1}if(n)e=t.contentWindow;else break;t=Rt(e.document)}return t}function Ar(e){var t=e&&e.nodeName&&e.nodeName.toLowerCase();return t&&(t===`input`&&(e.type===`text`||e.type===`search`||e.type===`tel`||e.type===`url`||e.type===`password`)||t===`textarea`||e.contentEditable===`true`)}var jr=dn&&`documentMode`in document&&11>=document.documentMode,Mr=null,Nr=null,Pr=null,Fr=!1;function Ir(e,t,n){var r=n.window===n?n.document:n.nodeType===9?n:n.ownerDocument;Fr||Mr==null||Mr!==Rt(r)||(r=Mr,`selectionStart`in r&&Ar(r)?r={start:r.selectionStart,end:r.selectionEnd}:(r=(r.ownerDocument&&r.ownerDocument.defaultView||window).getSelection(),r={anchorNode:r.anchorNode,anchorOffset:r.anchorOffset,focusNode:r.focusNode,focusOffset:r.focusOffset}),Pr&&Tr(Pr,r)||(Pr=r,r=Id(Nr,`onSelect`),0<r.length&&(t=new Cn(`onSelect`,`select`,null,t,n),e.push({event:t,listeners:r}),t.target=Mr)))}function Lr(e,t){var n={};return n[e.toLowerCase()]=t.toLowerCase(),n[`Webkit`+e]=`webkit`+t,n[`Moz`+e]=`moz`+t,n}var Rr={animationend:Lr(`Animation`,`AnimationEnd`),animationiteration:Lr(`Animation`,`AnimationIteration`),animationstart:Lr(`Animation`,`AnimationStart`),transitionrun:Lr(`Transition`,`TransitionRun`),transitionstart:Lr(`Transition`,`TransitionStart`),transitioncancel:Lr(`Transition`,`TransitionCancel`),transitionend:Lr(`Transition`,`TransitionEnd`)},zr={},Br={};dn&&(Br=document.createElement(`div`).style,`AnimationEvent`in window||(delete Rr.animationend.animation,delete Rr.animationiteration.animation,delete Rr.animationstart.animation),`TransitionEvent`in window||delete Rr.transitionend.transition);function Vr(e){if(zr[e])return zr[e];if(!Rr[e])return e;var t=Rr[e],n;for(n in t)if(t.hasOwnProperty(n)&&n in Br)return zr[e]=t[n];return e}var Hr=Vr(`animationend`),Ur=Vr(`animationiteration`),Wr=Vr(`animationstart`),Gr=Vr(`transitionrun`),Kr=Vr(`transitionstart`),qr=Vr(`transitioncancel`),Jr=Vr(`transitionend`),Yr=new Map,Xr=`abort auxClick beforeToggle cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel`.split(` `);Xr.push(`scrollEnd`);function Zr(e,t){Yr.set(e,t),wt(t,[e])}var Qr=typeof reportError==`function`?reportError:function(e){if(typeof window==`object`&&typeof window.ErrorEvent==`function`){var t=new window.ErrorEvent(`error`,{bubbles:!0,cancelable:!0,message:typeof e==`object`&&e&&typeof e.message==`string`?String(e.message):String(e),error:e});if(!window.dispatchEvent(t))return}else if(typeof process==`object`&&typeof process.emit==`function`){process.emit(`uncaughtException`,e);return}console.error(e)},$r=[],ei=0,ti=0;function ni(){for(var e=ei,t=ti=ei=0;t<e;){var n=$r[t];$r[t++]=null;var r=$r[t];$r[t++]=null;var i=$r[t];$r[t++]=null;var a=$r[t];if($r[t++]=null,r!==null&&i!==null){var o=r.pending;o===null?i.next=i:(i.next=o.next,o.next=i),r.pending=i}a!==0&&oi(n,i,a)}}function ri(e,t,n,r){$r[ei++]=e,$r[ei++]=t,$r[ei++]=n,$r[ei++]=r,ti|=r,e.lanes|=r,e=e.alternate,e!==null&&(e.lanes|=r)}function ii(e,t,n,r){return ri(e,t,n,r),si(e)}function ai(e,t){return ri(e,null,null,t),si(e)}function oi(e,t,n){e.lanes|=n;var r=e.alternate;r!==null&&(r.lanes|=n);for(var i=!1,a=e.return;a!==null;)a.childLanes|=n,r=a.alternate,r!==null&&(r.childLanes|=n),a.tag===22&&(e=a.stateNode,e===null||e._visibility&1||(i=!0)),e=a,a=a.return;return e.tag===3?(a=e.stateNode,i&&t!==null&&(i=31-ze(n),e=a.hiddenUpdates,r=e[i],r===null?e[i]=[t]:r.push(t),t.lane=n|536870912),a):null}function si(e){if(50<yu)throw yu=0,bu=null,Error(i(185));for(var t=e.return;t!==null;)e=t,t=e.return;return e.tag===3?e.stateNode:null}var ci={};function li(e,t,n,r){this.tag=e,this.key=n,this.sibling=this.child=this.return=this.stateNode=this.type=this.elementType=null,this.index=0,this.refCleanup=this.ref=null,this.pendingProps=t,this.dependencies=this.memoizedState=this.updateQueue=this.memoizedProps=null,this.mode=r,this.subtreeFlags=this.flags=0,this.deletions=null,this.childLanes=this.lanes=0,this.alternate=null}function ui(e,t,n,r){return new li(e,t,n,r)}function di(e){return e=e.prototype,!(!e||!e.isReactComponent)}function fi(e,t){var n=e.alternate;return n===null?(n=ui(e.tag,t,e.key,e.mode),n.elementType=e.elementType,n.type=e.type,n.stateNode=e.stateNode,n.alternate=e,e.alternate=n):(n.pendingProps=t,n.type=e.type,n.flags=0,n.subtreeFlags=0,n.deletions=null),n.flags=e.flags&65011712,n.childLanes=e.childLanes,n.lanes=e.lanes,n.child=e.child,n.memoizedProps=e.memoizedProps,n.memoizedState=e.memoizedState,n.updateQueue=e.updateQueue,t=e.dependencies,n.dependencies=t===null?null:{lanes:t.lanes,firstContext:t.firstContext},n.sibling=e.sibling,n.index=e.index,n.ref=e.ref,n.refCleanup=e.refCleanup,n}function pi(e,t){e.flags&=65011714;var n=e.alternate;return n===null?(e.childLanes=0,e.lanes=t,e.child=null,e.subtreeFlags=0,e.memoizedProps=null,e.memoizedState=null,e.updateQueue=null,e.dependencies=null,e.stateNode=null):(e.childLanes=n.childLanes,e.lanes=n.lanes,e.child=n.child,e.subtreeFlags=0,e.deletions=null,e.memoizedProps=n.memoizedProps,e.memoizedState=n.memoizedState,e.updateQueue=n.updateQueue,e.type=n.type,t=n.dependencies,e.dependencies=t===null?null:{lanes:t.lanes,firstContext:t.firstContext}),e}function mi(e,t,n,r,a,o){var s=0;if(r=e,typeof e==`function`)di(e)&&(s=1);else if(typeof e==`string`)s=$f(e,n,se.current)?26:e===`html`||e===`head`||e===`body`?27:5;else a:switch(e){case k:return e=ui(31,n,t,a),e.elementType=k,e.lanes=o,e;case y:return hi(n.children,a,o,t);case b:s=8,a|=24;break;case x:return e=ui(12,n,t,a|2),e.elementType=x,e.lanes=o,e;case T:return e=ui(13,n,t,a),e.elementType=T,e.lanes=o,e;case E:return e=ui(19,n,t,a),e.elementType=E,e.lanes=o,e;default:if(typeof e==`object`&&e)switch(e.$$typeof){case C:s=10;break a;case S:s=9;break a;case w:s=11;break a;case D:s=14;break a;case O:s=16,r=null;break a}s=29,n=Error(i(130,e===null?`null`:typeof e,``)),r=null}return t=ui(s,n,t,a),t.elementType=e,t.type=r,t.lanes=o,t}function hi(e,t,n,r){return e=ui(7,e,r,t),e.lanes=n,e}function gi(e,t,n){return e=ui(6,e,null,t),e.lanes=n,e}function _i(e){var t=ui(18,null,null,0);return t.stateNode=e,t}function vi(e,t,n){return t=ui(4,e.children===null?[]:e.children,e.key,t),t.lanes=n,t.stateNode={containerInfo:e.containerInfo,pendingChildren:null,implementation:e.implementation},t}var yi=new WeakMap;function bi(e,t){if(typeof e==`object`&&e){var n=yi.get(e);return n===void 0?(t={value:e,source:t,stack:xe(t)},yi.set(e,t),t):n}return{value:e,source:t,stack:xe(t)}}var xi=[],Si=0,Ci=null,wi=0,Ti=[],Ei=0,Di=null,Oi=1,ki=``;function Ai(e,t){xi[Si++]=wi,xi[Si++]=Ci,Ci=e,wi=t}function ji(e,t,n){Ti[Ei++]=Oi,Ti[Ei++]=ki,Ti[Ei++]=Di,Di=e;var r=Oi;e=ki;var i=32-ze(r)-1;r&=~(1<<i),n+=1;var a=32-ze(t)+i;if(30<a){var o=i-i%5;a=(r&(1<<o)-1).toString(32),r>>=o,i-=o,Oi=1<<32-ze(t)+i|n<<i|r,ki=a+e}else Oi=1<<a|n<<i|r,ki=e}function Mi(e){e.return!==null&&(Ai(e,1),ji(e,1,0))}function Ni(e){for(;e===Ci;)Ci=xi[--Si],xi[Si]=null,wi=xi[--Si],xi[Si]=null;for(;e===Di;)Di=Ti[--Ei],Ti[Ei]=null,ki=Ti[--Ei],Ti[Ei]=null,Oi=Ti[--Ei],Ti[Ei]=null}function Pi(e,t){Ti[Ei++]=Oi,Ti[Ei++]=ki,Ti[Ei++]=Di,Oi=t.id,ki=t.overflow,Di=e}var Fi=null,Ii=null,Li=!1,Ri=null,zi=!1,Bi=Error(i(519));function Vi(e){throw Ki(bi(Error(i(418,1<arguments.length&&arguments[1]!==void 0&&arguments[1]?`text`:`HTML`,``)),e)),Bi}function L(e){var t=e.stateNode,n=e.type,r=e.memoizedProps;switch(t[ct]=e,t[lt]=r,n){case`dialog`:kd(`cancel`,t),kd(`close`,t);break;case`iframe`:case`object`:case`embed`:kd(`load`,t);break;case`video`:case`audio`:for(n=0;n<Ed.length;n++)kd(Ed[n],t);break;case`source`:kd(`error`,t);break;case`img`:case`image`:case`link`:kd(`error`,t),kd(`load`,t);break;case`details`:kd(`toggle`,t);break;case`input`:kd(`invalid`,t),Ht(t,r.value,r.defaultValue,r.checked,r.defaultChecked,r.type,r.name,!0);break;case`select`:kd(`invalid`,t);break;case`textarea`:kd(`invalid`,t),Kt(t,r.value,r.defaultValue,r.children)}n=r.children,typeof n!=`string`&&typeof n!=`number`&&typeof n!=`bigint`||t.textContent===``+n||!0===r.suppressHydrationWarning||Hd(t.textContent,n)?(r.popover!=null&&(kd(`beforetoggle`,t),kd(`toggle`,t)),r.onScroll!=null&&kd(`scroll`,t),r.onScrollEnd!=null&&kd(`scrollend`,t),r.onClick!=null&&(t.onclick=tn),t=!0):t=!1,t||Vi(e,!0)}function Hi(e){for(Fi=e.return;Fi;)switch(Fi.tag){case 5:case 31:case 13:zi=!1;return;case 27:case 3:zi=!0;return;default:Fi=Fi.return}}function Ui(e){if(e!==Fi)return!1;if(!Li)return Hi(e),Li=!0,!1;var t=e.tag,n;if((n=t!==3&&t!==27)&&((n=t===5)&&(n=e.type,n=!(n!==`form`&&n!==`button`)||$d(e.type,e.memoizedProps)),n=!n),n&&Ii&&Vi(e),Hi(e),t===13){if(e=e.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(317));Ii=bf(e)}else if(t===31){if(e=e.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(317));Ii=bf(e)}else t===27?(t=Ii,cf(e.type)?(e=yf,yf=null,Ii=e):Ii=t):Ii=Fi?vf(e.stateNode.nextSibling):null;return!0}function Wi(){Ii=Fi=null,Li=!1}function Gi(){var e=Ri;return e!==null&&(au===null?au=e:au.push.apply(au,e),Ri=null),e}function Ki(e){Ri===null?Ri=[e]:Ri.push(e)}var qi=ae(null),Ji=null,Yi=null;function Xi(e,t,n){I(qi,t._currentValue),t._currentValue=n}function Zi(e){e._currentValue=qi.current,oe(qi)}function Qi(e,t,n){for(;e!==null;){var r=e.alternate;if((e.childLanes&t)===t?r!==null&&(r.childLanes&t)!==t&&(r.childLanes|=t):(e.childLanes|=t,r!==null&&(r.childLanes|=t)),e===n)break;e=e.return}}function $i(e,t,n,r){var a=e.child;for(a!==null&&(a.return=e);a!==null;){var o=a.dependencies;if(o!==null){var s=a.child;o=o.firstContext;a:for(;o!==null;){var c=o;o=a;for(var l=0;l<t.length;l++)if(c.context===t[l]){o.lanes|=n,c=o.alternate,c!==null&&(c.lanes|=n),Qi(o.return,n,e),r||(s=null);break a}o=c.next}}else if(a.tag===18){if(s=a.return,s===null)throw Error(i(341));s.lanes|=n,o=s.alternate,o!==null&&(o.lanes|=n),Qi(s,n,e),s=null}else s=a.child;if(s!==null)s.return=a;else for(s=a;s!==null;){if(s===e){s=null;break}if(a=s.sibling,a!==null){a.return=s.return,s=a;break}s=s.return}a=s}}function ea(e,t,n,r){e=null;for(var a=t,o=!1;a!==null;){if(!o){if(a.flags&524288)o=!0;else if(a.flags&262144)break}if(a.tag===10){var s=a.alternate;if(s===null)throw Error(i(387));if(s=s.memoizedProps,s!==null){var c=a.type;wr(a.pendingProps.value,s.value)||(e===null?e=[c]:e.push(c))}}else if(a===ue.current){if(s=a.alternate,s===null)throw Error(i(387));s.memoizedState.memoizedState!==a.memoizedState.memoizedState&&(e===null?e=[cp]:e.push(cp))}a=a.return}e!==null&&$i(t,e,n,r),t.flags|=262144}function ta(e){for(e=e.firstContext;e!==null;){if(!wr(e.context._currentValue,e.memoizedValue))return!0;e=e.next}return!1}function na(e){Ji=e,Yi=null,e=e.dependencies,e!==null&&(e.firstContext=null)}function ra(e){return aa(Ji,e)}function ia(e,t){return Ji===null&&na(e),aa(e,t)}function aa(e,t){var n=t._currentValue;if(t={context:t,memoizedValue:n,next:null},Yi===null){if(e===null)throw Error(i(308));Yi=t,e.dependencies={lanes:0,firstContext:t},e.flags|=524288}else Yi=Yi.next=t;return n}var oa=typeof AbortController<`u`?AbortController:function(){var e=[],t=this.signal={aborted:!1,addEventListener:function(t,n){e.push(n)}};this.abort=function(){t.aborted=!0,e.forEach(function(e){return e()})}},sa=t.unstable_scheduleCallback,ca=t.unstable_NormalPriority,la={$$typeof:C,Consumer:null,Provider:null,_currentValue:null,_currentValue2:null,_threadCount:0};function ua(){return{controller:new oa,data:new Map,refCount:0}}function da(e){e.refCount--,e.refCount===0&&sa(ca,function(){e.controller.abort()})}var fa=null,pa=0,ma=0,ha=null;function ga(e,t){if(fa===null){var n=fa=[];pa=0,ma=bd(),ha={status:`pending`,value:void 0,then:function(e){n.push(e)}}}return pa++,t.then(_a,_a),t}function _a(){if(--pa===0&&fa!==null){ha!==null&&(ha.status=`fulfilled`);var e=fa;fa=null,ma=0,ha=null;for(var t=0;t<e.length;t++)(0,e[t])()}}function va(e,t){var n=[],r={status:`pending`,value:null,reason:null,then:function(e){n.push(e)}};return e.then(function(){r.status=`fulfilled`,r.value=t;for(var e=0;e<n.length;e++)(0,n[e])(t)},function(e){for(r.status=`rejected`,r.reason=e,e=0;e<n.length;e++)(0,n[e])(void 0)}),r}var ya=P.S;P.S=function(e,t){cu=De(),typeof t==`object`&&t&&typeof t.then==`function`&&ga(e,t),ya!==null&&ya(e,t)};var ba=ae(null);function xa(){var e=ba.current;return e===null?Ul.pooledCache:e}function Sa(e,t){t===null?I(ba,ba.current):I(ba,t.pool)}function Ca(){var e=xa();return e===null?null:{parent:la._currentValue,pool:e}}var wa=Error(i(460)),Ta=Error(i(474)),Ea=Error(i(542)),Da={then:function(){}};function Oa(e){return e=e.status,e===`fulfilled`||e===`rejected`}function ka(e,t,n){switch(n=e[n],n===void 0?e.push(t):n!==t&&(t.then(tn,tn),t=n),t.status){case`fulfilled`:return t.value;case`rejected`:throw e=t.reason,Na(e),e;default:if(typeof t.status==`string`)t.then(tn,tn);else{if(e=Ul,e!==null&&100<e.shellSuspendCounter)throw Error(i(482));e=t,e.status=`pending`,e.then(function(e){if(t.status===`pending`){var n=t;n.status=`fulfilled`,n.value=e}},function(e){if(t.status===`pending`){var n=t;n.status=`rejected`,n.reason=e}})}switch(t.status){case`fulfilled`:return t.value;case`rejected`:throw e=t.reason,Na(e),e}throw ja=t,wa}}function Aa(e){try{var t=e._init;return t(e._payload)}catch(e){throw typeof e==`object`&&e&&typeof e.then==`function`?(ja=e,wa):e}}var ja=null;function Ma(){if(ja===null)throw Error(i(459));var e=ja;return ja=null,e}function Na(e){if(e===wa||e===Ea)throw Error(i(483))}var Pa=null,Fa=0;function Ia(e){var t=Fa;return Fa+=1,Pa===null&&(Pa=[]),ka(Pa,e,t)}function La(e,t){t=t.props.ref,e.ref=t===void 0?null:t}function Ra(e,t){throw t.$$typeof===g?Error(i(525)):(e=Object.prototype.toString.call(t),Error(i(31,e===`[object Object]`?`object with keys {`+Object.keys(t).join(`, `)+`}`:e)))}function za(e){function t(t,n){if(e){var r=t.deletions;r===null?(t.deletions=[n],t.flags|=16):r.push(n)}}function n(n,r){if(!e)return null;for(;r!==null;)t(n,r),r=r.sibling;return null}function r(e){for(var t=new Map;e!==null;)e.key===null?t.set(e.index,e):t.set(e.key,e),e=e.sibling;return t}function a(e,t){return e=fi(e,t),e.index=0,e.sibling=null,e}function o(t,n,r){return t.index=r,e?(r=t.alternate,r===null?(t.flags|=67108866,n):(r=r.index,r<n?(t.flags|=67108866,n):r)):(t.flags|=1048576,n)}function s(t){return e&&t.alternate===null&&(t.flags|=67108866),t}function c(e,t,n,r){return t===null||t.tag!==6?(t=gi(n,e.mode,r),t.return=e,t):(t=a(t,n),t.return=e,t)}function l(e,t,n,r){var i=n.type;return i===y?d(e,t,n.props.children,r,n.key):t!==null&&(t.elementType===i||typeof i==`object`&&i&&i.$$typeof===O&&Aa(i)===t.type)?(t=a(t,n.props),La(t,n),t.return=e,t):(t=mi(n.type,n.key,n.props,null,e.mode,r),La(t,n),t.return=e,t)}function u(e,t,n,r){return t===null||t.tag!==4||t.stateNode.containerInfo!==n.containerInfo||t.stateNode.implementation!==n.implementation?(t=vi(n,e.mode,r),t.return=e,t):(t=a(t,n.children||[]),t.return=e,t)}function d(e,t,n,r,i){return t===null||t.tag!==7?(t=hi(n,e.mode,r,i),t.return=e,t):(t=a(t,n),t.return=e,t)}function f(e,t,n){if(typeof t==`string`&&t!==``||typeof t==`number`||typeof t==`bigint`)return t=gi(``+t,e.mode,n),t.return=e,t;if(typeof t==`object`&&t){switch(t.$$typeof){case _:return n=mi(t.type,t.key,t.props,null,e.mode,n),La(n,t),n.return=e,n;case v:return t=vi(t,e.mode,n),t.return=e,t;case O:return t=Aa(t),f(e,t,n)}if(N(t)||j(t))return t=hi(t,e.mode,n,null),t.return=e,t;if(typeof t.then==`function`)return f(e,Ia(t),n);if(t.$$typeof===C)return f(e,ia(e,t),n);Ra(e,t)}return null}function p(e,t,n,r){var i=t===null?null:t.key;if(typeof n==`string`&&n!==``||typeof n==`number`||typeof n==`bigint`)return i===null?c(e,t,``+n,r):null;if(typeof n==`object`&&n){switch(n.$$typeof){case _:return n.key===i?l(e,t,n,r):null;case v:return n.key===i?u(e,t,n,r):null;case O:return n=Aa(n),p(e,t,n,r)}if(N(n)||j(n))return i===null?d(e,t,n,r,null):null;if(typeof n.then==`function`)return p(e,t,Ia(n),r);if(n.$$typeof===C)return p(e,t,ia(e,n),r);Ra(e,n)}return null}function m(e,t,n,r,i){if(typeof r==`string`&&r!==``||typeof r==`number`||typeof r==`bigint`)return e=e.get(n)||null,c(t,e,``+r,i);if(typeof r==`object`&&r){switch(r.$$typeof){case _:return e=e.get(r.key===null?n:r.key)||null,l(t,e,r,i);case v:return e=e.get(r.key===null?n:r.key)||null,u(t,e,r,i);case O:return r=Aa(r),m(e,t,n,r,i)}if(N(r)||j(r))return e=e.get(n)||null,d(t,e,r,i,null);if(typeof r.then==`function`)return m(e,t,n,Ia(r),i);if(r.$$typeof===C)return m(e,t,n,ia(t,r),i);Ra(t,r)}return null}function h(i,a,s,c){for(var l=null,u=null,d=a,h=a=0,g=null;d!==null&&h<s.length;h++){d.index>h?(g=d,d=null):g=d.sibling;var _=p(i,d,s[h],c);if(_===null){d===null&&(d=g);break}e&&d&&_.alternate===null&&t(i,d),a=o(_,a,h),u===null?l=_:u.sibling=_,u=_,d=g}if(h===s.length)return n(i,d),Li&&Ai(i,h),l;if(d===null){for(;h<s.length;h++)d=f(i,s[h],c),d!==null&&(a=o(d,a,h),u===null?l=d:u.sibling=d,u=d);return Li&&Ai(i,h),l}for(d=r(d);h<s.length;h++)g=m(d,i,h,s[h],c),g!==null&&(e&&g.alternate!==null&&d.delete(g.key===null?h:g.key),a=o(g,a,h),u===null?l=g:u.sibling=g,u=g);return e&&d.forEach(function(e){return t(i,e)}),Li&&Ai(i,h),l}function g(a,s,c,l){if(c==null)throw Error(i(151));for(var u=null,d=null,h=s,g=s=0,_=null,v=c.next();h!==null&&!v.done;g++,v=c.next()){h.index>g?(_=h,h=null):_=h.sibling;var y=p(a,h,v.value,l);if(y===null){h===null&&(h=_);break}e&&h&&y.alternate===null&&t(a,h),s=o(y,s,g),d===null?u=y:d.sibling=y,d=y,h=_}if(v.done)return n(a,h),Li&&Ai(a,g),u;if(h===null){for(;!v.done;g++,v=c.next())v=f(a,v.value,l),v!==null&&(s=o(v,s,g),d===null?u=v:d.sibling=v,d=v);return Li&&Ai(a,g),u}for(h=r(h);!v.done;g++,v=c.next())v=m(h,a,g,v.value,l),v!==null&&(e&&v.alternate!==null&&h.delete(v.key===null?g:v.key),s=o(v,s,g),d===null?u=v:d.sibling=v,d=v);return e&&h.forEach(function(e){return t(a,e)}),Li&&Ai(a,g),u}function b(e,r,o,c){if(typeof o==`object`&&o&&o.type===y&&o.key===null&&(o=o.props.children),typeof o==`object`&&o){switch(o.$$typeof){case _:a:{for(var l=o.key;r!==null;){if(r.key===l){if(l=o.type,l===y){if(r.tag===7){n(e,r.sibling),c=a(r,o.props.children),c.return=e,e=c;break a}}else if(r.elementType===l||typeof l==`object`&&l&&l.$$typeof===O&&Aa(l)===r.type){n(e,r.sibling),c=a(r,o.props),La(c,o),c.return=e,e=c;break a}n(e,r);break}else t(e,r);r=r.sibling}o.type===y?(c=hi(o.props.children,e.mode,c,o.key),c.return=e,e=c):(c=mi(o.type,o.key,o.props,null,e.mode,c),La(c,o),c.return=e,e=c)}return s(e);case v:a:{for(l=o.key;r!==null;){if(r.key===l)if(r.tag===4&&r.stateNode.containerInfo===o.containerInfo&&r.stateNode.implementation===o.implementation){n(e,r.sibling),c=a(r,o.children||[]),c.return=e,e=c;break a}else{n(e,r);break}else t(e,r);r=r.sibling}c=vi(o,e.mode,c),c.return=e,e=c}return s(e);case O:return o=Aa(o),b(e,r,o,c)}if(N(o))return h(e,r,o,c);if(j(o)){if(l=j(o),typeof l!=`function`)throw Error(i(150));return o=l.call(o),g(e,r,o,c)}if(typeof o.then==`function`)return b(e,r,Ia(o),c);if(o.$$typeof===C)return b(e,r,ia(e,o),c);Ra(e,o)}return typeof o==`string`&&o!==``||typeof o==`number`||typeof o==`bigint`?(o=``+o,r!==null&&r.tag===6?(n(e,r.sibling),c=a(r,o),c.return=e,e=c):(n(e,r),c=gi(o,e.mode,c),c.return=e,e=c),s(e)):n(e,r)}return function(e,t,n,r){try{Fa=0;var i=b(e,t,n,r);return Pa=null,i}catch(t){if(t===wa||t===Ea)throw t;var a=ui(29,t,null,e.mode);return a.lanes=r,a.return=e,a}}}var Ba=za(!0),Va=za(!1),Ha=!1;function Ua(e){e.updateQueue={baseState:e.memoizedState,firstBaseUpdate:null,lastBaseUpdate:null,shared:{pending:null,lanes:0,hiddenCallbacks:null},callbacks:null}}function Wa(e,t){e=e.updateQueue,t.updateQueue===e&&(t.updateQueue={baseState:e.baseState,firstBaseUpdate:e.firstBaseUpdate,lastBaseUpdate:e.lastBaseUpdate,shared:e.shared,callbacks:null})}function Ga(e){return{lane:e,tag:0,payload:null,callback:null,next:null}}function Ka(e,t,n){var r=e.updateQueue;if(r===null)return null;if(r=r.shared,Hl&2){var i=r.pending;return i===null?t.next=t:(t.next=i.next,i.next=t),r.pending=t,t=si(e),oi(e,null,n),t}return ri(e,r,t,n),si(e)}function qa(e,t,n){if(t=t.updateQueue,t!==null&&(t=t.shared,n&4194048)){var r=t.lanes;r&=e.pendingLanes,n|=r,t.lanes=n,tt(e,n)}}function Ja(e,t){var n=e.updateQueue,r=e.alternate;if(r!==null&&(r=r.updateQueue,n===r)){var i=null,a=null;if(n=n.firstBaseUpdate,n!==null){do{var o={lane:n.lane,tag:n.tag,payload:n.payload,callback:null,next:null};a===null?i=a=o:a=a.next=o,n=n.next}while(n!==null);a===null?i=a=t:a=a.next=t}else i=a=t;n={baseState:r.baseState,firstBaseUpdate:i,lastBaseUpdate:a,shared:r.shared,callbacks:r.callbacks},e.updateQueue=n;return}e=n.lastBaseUpdate,e===null?n.firstBaseUpdate=t:e.next=t,n.lastBaseUpdate=t}var Ya=!1;function Xa(){if(Ya){var e=ha;if(e!==null)throw e}}function Za(e,t,n,r){Ya=!1;var i=e.updateQueue;Ha=!1;var a=i.firstBaseUpdate,o=i.lastBaseUpdate,s=i.shared.pending;if(s!==null){i.shared.pending=null;var c=s,l=c.next;c.next=null,o===null?a=l:o.next=l,o=c;var u=e.alternate;u!==null&&(u=u.updateQueue,s=u.lastBaseUpdate,s!==o&&(s===null?u.firstBaseUpdate=l:s.next=l,u.lastBaseUpdate=c))}if(a!==null){var d=i.baseState;o=0,u=l=c=null,s=a;do{var f=s.lane&-536870913,p=f!==s.lane;if(p?(Gl&f)===f:(r&f)===f){f!==0&&f===ma&&(Ya=!0),u!==null&&(u=u.next={lane:0,tag:s.tag,payload:s.payload,callback:null,next:null});a:{var m=e,g=s;f=t;var _=n;switch(g.tag){case 1:if(m=g.payload,typeof m==`function`){d=m.call(_,d,f);break a}d=m;break a;case 3:m.flags=m.flags&-65537|128;case 0:if(m=g.payload,f=typeof m==`function`?m.call(_,d,f):m,f==null)break a;d=h({},d,f);break a;case 2:Ha=!0}}f=s.callback,f!==null&&(e.flags|=64,p&&(e.flags|=8192),p=i.callbacks,p===null?i.callbacks=[f]:p.push(f))}else p={lane:f,tag:s.tag,payload:s.payload,callback:s.callback,next:null},u===null?(l=u=p,c=d):u=u.next=p,o|=f;if(s=s.next,s===null){if(s=i.shared.pending,s===null)break;p=s,s=p.next,p.next=null,i.lastBaseUpdate=p,i.shared.pending=null}}while(1);u===null&&(c=d),i.baseState=c,i.firstBaseUpdate=l,i.lastBaseUpdate=u,a===null&&(i.shared.lanes=0),$l|=o,e.lanes=o,e.memoizedState=d}}function Qa(e,t){if(typeof e!=`function`)throw Error(i(191,e));e.call(t)}function $a(e,t){var n=e.callbacks;if(n!==null)for(e.callbacks=null,e=0;e<n.length;e++)Qa(n[e],t)}var eo=ae(null),to=ae(0);function no(e,t){e=Zl,I(to,e),I(eo,t),Zl=e|t.baseLanes}function ro(){I(to,Zl),I(eo,eo.current)}function io(){Zl=to.current,oe(eo),oe(to)}var ao=ae(null),oo=null;function so(e){var t=e.alternate;I(po,po.current&1),I(ao,e),oo===null&&(t===null||eo.current!==null||t.memoizedState!==null)&&(oo=e)}function co(e){I(po,po.current),I(ao,e),oo===null&&(oo=e)}function lo(e){e.tag===22?(I(po,po.current),I(ao,e),oo===null&&(oo=e)):uo(e)}function uo(){I(po,po.current),I(ao,ao.current)}function fo(e){oe(ao),oo===e&&(oo=null),oe(po)}var po=ae(0);function mo(e){for(var t=e;t!==null;){if(t.tag===13){var n=t.memoizedState;if(n!==null&&(n=n.dehydrated,n===null||hf(n)||gf(n)))return t}else if(t.tag===19&&(t.memoizedProps.revealOrder===`forwards`||t.memoizedProps.revealOrder===`backwards`||t.memoizedProps.revealOrder===`unstable_legacy-backwards`||t.memoizedProps.revealOrder===`together`)){if(t.flags&128)return t}else if(t.child!==null){t.child.return=t,t=t.child;continue}if(t===e)break;for(;t.sibling===null;){if(t.return===null||t.return===e)return null;t=t.return}t.sibling.return=t.return,t=t.sibling}return null}var ho=0,R=null,go=null,_o=null,vo=!1,yo=!1,bo=!1,xo=0,So=0,Co=null,wo=0;function To(){throw Error(i(321))}function Eo(e,t){if(t===null)return!1;for(var n=0;n<t.length&&n<e.length;n++)if(!wr(e[n],t[n]))return!1;return!0}function Do(e,t,n,r,i,a){return ho=a,R=t,t.memoizedState=null,t.updateQueue=null,t.lanes=0,P.H=e===null||e.memoizedState===null?Us:Ws,bo=!1,a=n(r,i),bo=!1,yo&&(a=ko(t,n,r,i)),Oo(e),a}function Oo(e){P.H=Hs;var t=go!==null&&go.next!==null;if(ho=0,_o=go=R=null,vo=!1,So=0,Co=null,t)throw Error(i(300));e===null||sc||(e=e.dependencies,e!==null&&ta(e)&&(sc=!0))}function ko(e,t,n,r){R=e;var a=0;do{if(yo&&(Co=null),So=0,yo=!1,25<=a)throw Error(i(301));if(a+=1,_o=go=null,e.updateQueue!=null){var o=e.updateQueue;o.lastEffect=null,o.events=null,o.stores=null,o.memoCache!=null&&(o.memoCache.index=0)}P.H=Gs,o=t(n,r)}while(yo);return o}function Ao(){var e=P.H,t=e.useState()[0];return t=typeof t.then==`function`?Io(t):t,e=e.useState()[0],(go===null?null:go.memoizedState)!==e&&(R.flags|=1024),t}function jo(){var e=xo!==0;return xo=0,e}function Mo(e,t,n){t.updateQueue=e.updateQueue,t.flags&=-2053,e.lanes&=~n}function No(e){if(vo){for(e=e.memoizedState;e!==null;){var t=e.queue;t!==null&&(t.pending=null),e=e.next}vo=!1}ho=0,_o=go=R=null,yo=!1,So=xo=0,Co=null}function Po(){var e={memoizedState:null,baseState:null,baseQueue:null,queue:null,next:null};return _o===null?R.memoizedState=_o=e:_o=_o.next=e,_o}function Fo(){if(go===null){var e=R.alternate;e=e===null?null:e.memoizedState}else e=go.next;var t=_o===null?R.memoizedState:_o.next;if(t!==null)_o=t,go=e;else{if(e===null)throw R.alternate===null?Error(i(467)):Error(i(310));go=e,e={memoizedState:go.memoizedState,baseState:go.baseState,baseQueue:go.baseQueue,queue:go.queue,next:null},_o===null?R.memoizedState=_o=e:_o=_o.next=e}return _o}function z(){return{lastEffect:null,events:null,stores:null,memoCache:null}}function Io(e){var t=So;return So+=1,Co===null&&(Co=[]),e=ka(Co,e,t),t=R,(_o===null?t.memoizedState:_o.next)===null&&(t=t.alternate,P.H=t===null||t.memoizedState===null?Us:Ws),e}function Lo(e){if(typeof e==`object`&&e){if(typeof e.then==`function`)return Io(e);if(e.$$typeof===C)return ra(e)}throw Error(i(438,String(e)))}function Ro(e){var t=null,n=R.updateQueue;if(n!==null&&(t=n.memoCache),t==null){var r=R.alternate;r!==null&&(r=r.updateQueue,r!==null&&(r=r.memoCache,r!=null&&(t={data:r.data.map(function(e){return e.slice()}),index:0})))}if(t??={data:[],index:0},n===null&&(n=z(),R.updateQueue=n),n.memoCache=t,n=t.data[t.index],n===void 0)for(n=t.data[t.index]=Array(e),r=0;r<e;r++)n[r]=ee;return t.index++,n}function zo(e,t){return typeof t==`function`?t(e):t}function Bo(e){return Vo(Fo(),go,e)}function Vo(e,t,n){var r=e.queue;if(r===null)throw Error(i(311));r.lastRenderedReducer=n;var a=e.baseQueue,o=r.pending;if(o!==null){if(a!==null){var s=a.next;a.next=o.next,o.next=s}t.baseQueue=a=o,r.pending=null}if(o=e.baseState,a===null)e.memoizedState=o;else{t=a.next;var c=s=null,l=null,u=t,d=!1;do{var f=u.lane&-536870913;if(f===u.lane?(ho&f)===f:(Gl&f)===f){var p=u.revertLane;if(p===0)l!==null&&(l=l.next={lane:0,revertLane:0,gesture:null,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null}),f===ma&&(d=!0);else if((ho&p)===p){u=u.next,p===ma&&(d=!0);continue}else f={lane:0,revertLane:u.revertLane,gesture:null,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null},l===null?(c=l=f,s=o):l=l.next=f,R.lanes|=p,$l|=p;f=u.action,bo&&n(o,f),o=u.hasEagerState?u.eagerState:n(o,f)}else p={lane:f,revertLane:u.revertLane,gesture:u.gesture,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null},l===null?(c=l=p,s=o):l=l.next=p,R.lanes|=f,$l|=f;u=u.next}while(u!==null&&u!==t);if(l===null?s=o:l.next=c,!wr(o,e.memoizedState)&&(sc=!0,d&&(n=ha,n!==null)))throw n;e.memoizedState=o,e.baseState=s,e.baseQueue=l,r.lastRenderedState=o}return a===null&&(r.lanes=0),[e.memoizedState,r.dispatch]}function Ho(e){var t=Fo(),n=t.queue;if(n===null)throw Error(i(311));n.lastRenderedReducer=e;var r=n.dispatch,a=n.pending,o=t.memoizedState;if(a!==null){n.pending=null;var s=a=a.next;do o=e(o,s.action),s=s.next;while(s!==a);wr(o,t.memoizedState)||(sc=!0),t.memoizedState=o,t.baseQueue===null&&(t.baseState=o),n.lastRenderedState=o}return[o,r]}function Uo(e,t,n){var r=R,a=Fo(),o=Li;if(o){if(n===void 0)throw Error(i(407));n=n()}else n=t();var s=!wr((go||a).memoizedState,n);if(s&&(a.memoizedState=n,sc=!0),a=a.queue,ms(Ko.bind(null,r,a,e),[e]),a.getSnapshot!==t||s||_o!==null&&_o.memoizedState.tag&1){if(r.flags|=2048,ls(9,{destroy:void 0},Go.bind(null,r,a,n,t),null),Ul===null)throw Error(i(349));o||ho&127||Wo(r,t,n)}return n}function Wo(e,t,n){e.flags|=16384,e={getSnapshot:t,value:n},t=R.updateQueue,t===null?(t=z(),R.updateQueue=t,t.stores=[e]):(n=t.stores,n===null?t.stores=[e]:n.push(e))}function Go(e,t,n,r){t.value=n,t.getSnapshot=r,qo(t)&&Jo(e)}function Ko(e,t,n){return n(function(){qo(t)&&Jo(e)})}function qo(e){var t=e.getSnapshot;e=e.value;try{var n=t();return!wr(e,n)}catch{return!0}}function Jo(e){var t=ai(e,2);t!==null&&Cu(t,e,2)}function Yo(e){var t=Po();if(typeof e==`function`){var n=e;if(e=n(),bo){Re(!0);try{n()}finally{Re(!1)}}}return t.memoizedState=t.baseState=e,t.queue={pending:null,lanes:0,dispatch:null,lastRenderedReducer:zo,lastRenderedState:e},t}function Xo(e,t,n,r){return e.baseState=n,Vo(e,go,typeof r==`function`?r:zo)}function Zo(e,t,n,r,a){if(zs(e))throw Error(i(485));if(e=t.action,e!==null){var o={payload:a,action:e,next:null,isTransition:!0,status:`pending`,value:null,reason:null,listeners:[],then:function(e){o.listeners.push(e)}};P.T===null?o.isTransition=!1:n(!0),r(o),n=t.pending,n===null?(o.next=t.pending=o,Qo(t,o)):(o.next=n.next,t.pending=n.next=o)}}function Qo(e,t){var n=t.action,r=t.payload,i=e.state;if(t.isTransition){var a=P.T,o={};P.T=o;try{var s=n(i,r),c=P.S;c!==null&&c(o,s),$o(e,t,s)}catch(n){ts(e,t,n)}finally{a!==null&&o.types!==null&&(a.types=o.types),P.T=a}}else try{a=n(i,r),$o(e,t,a)}catch(n){ts(e,t,n)}}function $o(e,t,n){typeof n==`object`&&n&&typeof n.then==`function`?n.then(function(n){es(e,t,n)},function(n){return ts(e,t,n)}):es(e,t,n)}function es(e,t,n){t.status=`fulfilled`,t.value=n,ns(t),e.state=n,t=e.pending,t!==null&&(n=t.next,n===t?e.pending=null:(n=n.next,t.next=n,Qo(e,n)))}function ts(e,t,n){var r=e.pending;if(e.pending=null,r!==null){r=r.next;do t.status=`rejected`,t.reason=n,ns(t),t=t.next;while(t!==r)}e.action=null}function ns(e){e=e.listeners;for(var t=0;t<e.length;t++)(0,e[t])()}function rs(e,t){return t}function is(e,t){if(Li){var n=Ul.formState;if(n!==null){a:{var r=R;if(Li){if(Ii){b:{for(var i=Ii,a=zi;i.nodeType!==8;){if(!a){i=null;break b}if(i=vf(i.nextSibling),i===null){i=null;break b}}a=i.data,i=a===`F!`||a===`F`?i:null}if(i){Ii=vf(i.nextSibling),r=i.data===`F!`;break a}}Vi(r)}r=!1}r&&(t=n[0])}}return n=Po(),n.memoizedState=n.baseState=t,r={pending:null,lanes:0,dispatch:null,lastRenderedReducer:rs,lastRenderedState:t},n.queue=r,n=Is.bind(null,R,r),r.dispatch=n,r=Yo(!1),a=Rs.bind(null,R,!1,r.queue),r=Po(),i={state:t,dispatch:null,action:e,pending:null},r.queue=i,n=Zo.bind(null,R,i,a,n),i.dispatch=n,r.memoizedState=e,[t,n,!1]}function as(e){return os(Fo(),go,e)}function os(e,t,n){if(t=Vo(e,t,rs)[0],e=Bo(zo)[0],typeof t==`object`&&t&&typeof t.then==`function`)try{var r=Io(t)}catch(e){throw e===wa?Ea:e}else r=t;t=Fo();var i=t.queue,a=i.dispatch;return n!==t.memoizedState&&(R.flags|=2048,ls(9,{destroy:void 0},ss.bind(null,i,n),null)),[r,a,e]}function ss(e,t){e.action=t}function cs(e){var t=Fo(),n=go;if(n!==null)return os(t,n,e);Fo(),t=t.memoizedState,n=Fo();var r=n.queue.dispatch;return n.memoizedState=e,[t,r,!1]}function ls(e,t,n,r){return e={tag:e,create:n,deps:r,inst:t,next:null},t=R.updateQueue,t===null&&(t=z(),R.updateQueue=t),n=t.lastEffect,n===null?t.lastEffect=e.next=e:(r=n.next,n.next=e,e.next=r,t.lastEffect=e),e}function us(){return Fo().memoizedState}function ds(e,t,n,r){var i=Po();R.flags|=e,i.memoizedState=ls(1|t,{destroy:void 0},n,r===void 0?null:r)}function fs(e,t,n,r){var i=Fo();r=r===void 0?null:r;var a=i.memoizedState.inst;go!==null&&r!==null&&Eo(r,go.memoizedState.deps)?i.memoizedState=ls(t,a,n,r):(R.flags|=e,i.memoizedState=ls(1|t,a,n,r))}function ps(e,t){ds(8390656,8,e,t)}function ms(e,t){fs(2048,8,e,t)}function hs(e){R.flags|=4;var t=R.updateQueue;if(t===null)t=z(),R.updateQueue=t,t.events=[e];else{var n=t.events;n===null?t.events=[e]:n.push(e)}}function gs(e){var t=Fo().memoizedState;return hs({ref:t,nextImpl:e}),function(){if(Hl&2)throw Error(i(440));return t.impl.apply(void 0,arguments)}}function _s(e,t){return fs(4,2,e,t)}function vs(e,t){return fs(4,4,e,t)}function ys(e,t){if(typeof t==`function`){e=e();var n=t(e);return function(){typeof n==`function`?n():t(null)}}if(t!=null)return e=e(),t.current=e,function(){t.current=null}}function bs(e,t,n){n=n==null?null:n.concat([e]),fs(4,4,ys.bind(null,t,e),n)}function xs(){}function Ss(e,t){var n=Fo();t=t===void 0?null:t;var r=n.memoizedState;return t!==null&&Eo(t,r[1])?r[0]:(n.memoizedState=[e,t],e)}function Cs(e,t){var n=Fo();t=t===void 0?null:t;var r=n.memoizedState;if(t!==null&&Eo(t,r[1]))return r[0];if(r=e(),bo){Re(!0);try{e()}finally{Re(!1)}}return n.memoizedState=[r,t],r}function ws(e,t,n){return n===void 0||ho&1073741824&&!(Gl&261930)?e.memoizedState=t:(e.memoizedState=n,e=Su(),R.lanes|=e,$l|=e,n)}function Ts(e,t,n,r){return wr(n,t)?n:eo.current===null?!(ho&42)||ho&1073741824&&!(Gl&261930)?(sc=!0,e.memoizedState=n):(e=Su(),R.lanes|=e,$l|=e,t):(e=ws(e,n,r),wr(e,t)||(sc=!0),e)}function Es(e,t,n,r,i){var a=F.p;F.p=a!==0&&8>a?a:8;var o=P.T,s={};P.T=s,Rs(e,!1,t,n);try{var c=i(),l=P.S;l!==null&&l(s,c),typeof c==`object`&&c&&typeof c.then==`function`?Ls(e,t,va(c,r),xu(e)):Ls(e,t,r,xu(e))}catch(n){Ls(e,t,{then:function(){},status:`rejected`,reason:n},xu())}finally{F.p=a,o!==null&&s.types!==null&&(o.types=s.types),P.T=o}}function Ds(){}function Os(e,t,n,r){if(e.tag!==5)throw Error(i(476));var a=ks(e).queue;Es(e,a,t,ne,n===null?Ds:function(){return As(e),n(r)})}function ks(e){var t=e.memoizedState;if(t!==null)return t;t={memoizedState:ne,baseState:ne,baseQueue:null,queue:{pending:null,lanes:0,dispatch:null,lastRenderedReducer:zo,lastRenderedState:ne},next:null};var n={};return t.next={memoizedState:n,baseState:n,baseQueue:null,queue:{pending:null,lanes:0,dispatch:null,lastRenderedReducer:zo,lastRenderedState:n},next:null},e.memoizedState=t,e=e.alternate,e!==null&&(e.memoizedState=t),t}function As(e){var t=ks(e);t.next===null&&(t=e.alternate.memoizedState),Ls(e,t.next.queue,{},xu())}function js(){return ra(cp)}function Ms(){return Fo().memoizedState}function Ns(){return Fo().memoizedState}function Ps(e){for(var t=e.return;t!==null;){switch(t.tag){case 24:case 3:var n=xu();e=Ga(n);var r=Ka(t,e,n);r!==null&&(Cu(r,t,n),qa(r,t,n)),t={cache:ua()},e.payload=t;return}t=t.return}}function Fs(e,t,n){var r=xu();n={lane:r,revertLane:0,gesture:null,action:n,hasEagerState:!1,eagerState:null,next:null},zs(e)?Bs(t,n):(n=ii(e,t,n,r),n!==null&&(Cu(n,e,r),Vs(n,t,r)))}function Is(e,t,n){Ls(e,t,n,xu())}function Ls(e,t,n,r){var i={lane:r,revertLane:0,gesture:null,action:n,hasEagerState:!1,eagerState:null,next:null};if(zs(e))Bs(t,i);else{var a=e.alternate;if(e.lanes===0&&(a===null||a.lanes===0)&&(a=t.lastRenderedReducer,a!==null))try{var o=t.lastRenderedState,s=a(o,n);if(i.hasEagerState=!0,i.eagerState=s,wr(s,o))return ri(e,t,i,0),Ul===null&&ni(),!1}catch{}if(n=ii(e,t,i,r),n!==null)return Cu(n,e,r),Vs(n,t,r),!0}return!1}function Rs(e,t,n,r){if(r={lane:2,revertLane:bd(),gesture:null,action:r,hasEagerState:!1,eagerState:null,next:null},zs(e)){if(t)throw Error(i(479))}else t=ii(e,n,r,2),t!==null&&Cu(t,e,2)}function zs(e){var t=e.alternate;return e===R||t!==null&&t===R}function Bs(e,t){yo=vo=!0;var n=e.pending;n===null?t.next=t:(t.next=n.next,n.next=t),e.pending=t}function Vs(e,t,n){if(n&4194048){var r=t.lanes;r&=e.pendingLanes,n|=r,t.lanes=n,tt(e,n)}}var Hs={readContext:ra,use:Lo,useCallback:To,useContext:To,useEffect:To,useImperativeHandle:To,useLayoutEffect:To,useInsertionEffect:To,useMemo:To,useReducer:To,useRef:To,useState:To,useDebugValue:To,useDeferredValue:To,useTransition:To,useSyncExternalStore:To,useId:To,useHostTransitionStatus:To,useFormState:To,useActionState:To,useOptimistic:To,useMemoCache:To,useCacheRefresh:To};Hs.useEffectEvent=To;var Us={readContext:ra,use:Lo,useCallback:function(e,t){return Po().memoizedState=[e,t===void 0?null:t],e},useContext:ra,useEffect:ps,useImperativeHandle:function(e,t,n){n=n==null?null:n.concat([e]),ds(4194308,4,ys.bind(null,t,e),n)},useLayoutEffect:function(e,t){return ds(4194308,4,e,t)},useInsertionEffect:function(e,t){ds(4,2,e,t)},useMemo:function(e,t){var n=Po();t=t===void 0?null:t;var r=e();if(bo){Re(!0);try{e()}finally{Re(!1)}}return n.memoizedState=[r,t],r},useReducer:function(e,t,n){var r=Po();if(n!==void 0){var i=n(t);if(bo){Re(!0);try{n(t)}finally{Re(!1)}}}else i=t;return r.memoizedState=r.baseState=i,e={pending:null,lanes:0,dispatch:null,lastRenderedReducer:e,lastRenderedState:i},r.queue=e,e=e.dispatch=Fs.bind(null,R,e),[r.memoizedState,e]},useRef:function(e){var t=Po();return e={current:e},t.memoizedState=e},useState:function(e){e=Yo(e);var t=e.queue,n=Is.bind(null,R,t);return t.dispatch=n,[e.memoizedState,n]},useDebugValue:xs,useDeferredValue:function(e,t){return ws(Po(),e,t)},useTransition:function(){var e=Yo(!1);return e=Es.bind(null,R,e.queue,!0,!1),Po().memoizedState=e,[!1,e]},useSyncExternalStore:function(e,t,n){var r=R,a=Po();if(Li){if(n===void 0)throw Error(i(407));n=n()}else{if(n=t(),Ul===null)throw Error(i(349));Gl&127||Wo(r,t,n)}a.memoizedState=n;var o={value:n,getSnapshot:t};return a.queue=o,ps(Ko.bind(null,r,o,e),[e]),r.flags|=2048,ls(9,{destroy:void 0},Go.bind(null,r,o,n,t),null),n},useId:function(){var e=Po(),t=Ul.identifierPrefix;if(Li){var n=ki,r=Oi;n=(r&~(1<<32-ze(r)-1)).toString(32)+n,t=`_`+t+`R_`+n,n=xo++,0<n&&(t+=`H`+n.toString(32)),t+=`_`}else n=wo++,t=`_`+t+`r_`+n.toString(32)+`_`;return e.memoizedState=t},useHostTransitionStatus:js,useFormState:is,useActionState:is,useOptimistic:function(e){var t=Po();t.memoizedState=t.baseState=e;var n={pending:null,lanes:0,dispatch:null,lastRenderedReducer:null,lastRenderedState:null};return t.queue=n,t=Rs.bind(null,R,!0,n),n.dispatch=t,[e,t]},useMemoCache:Ro,useCacheRefresh:function(){return Po().memoizedState=Ps.bind(null,R)},useEffectEvent:function(e){var t=Po(),n={impl:e};return t.memoizedState=n,function(){if(Hl&2)throw Error(i(440));return n.impl.apply(void 0,arguments)}}},Ws={readContext:ra,use:Lo,useCallback:Ss,useContext:ra,useEffect:ms,useImperativeHandle:bs,useInsertionEffect:_s,useLayoutEffect:vs,useMemo:Cs,useReducer:Bo,useRef:us,useState:function(){return Bo(zo)},useDebugValue:xs,useDeferredValue:function(e,t){return Ts(Fo(),go.memoizedState,e,t)},useTransition:function(){var e=Bo(zo)[0],t=Fo().memoizedState;return[typeof e==`boolean`?e:Io(e),t]},useSyncExternalStore:Uo,useId:Ms,useHostTransitionStatus:js,useFormState:as,useActionState:as,useOptimistic:function(e,t){return Xo(Fo(),go,e,t)},useMemoCache:Ro,useCacheRefresh:Ns};Ws.useEffectEvent=gs;var Gs={readContext:ra,use:Lo,useCallback:Ss,useContext:ra,useEffect:ms,useImperativeHandle:bs,useInsertionEffect:_s,useLayoutEffect:vs,useMemo:Cs,useReducer:Ho,useRef:us,useState:function(){return Ho(zo)},useDebugValue:xs,useDeferredValue:function(e,t){var n=Fo();return go===null?ws(n,e,t):Ts(n,go.memoizedState,e,t)},useTransition:function(){var e=Ho(zo)[0],t=Fo().memoizedState;return[typeof e==`boolean`?e:Io(e),t]},useSyncExternalStore:Uo,useId:Ms,useHostTransitionStatus:js,useFormState:cs,useActionState:cs,useOptimistic:function(e,t){var n=Fo();return go===null?(n.baseState=e,[e,n.queue.dispatch]):Xo(n,go,e,t)},useMemoCache:Ro,useCacheRefresh:Ns};Gs.useEffectEvent=gs;function Ks(e,t,n,r){t=e.memoizedState,n=n(r,t),n=n==null?t:h({},t,n),e.memoizedState=n,e.lanes===0&&(e.updateQueue.baseState=n)}var qs={enqueueSetState:function(e,t,n){e=e._reactInternals;var r=xu(),i=Ga(r);i.payload=t,n!=null&&(i.callback=n),t=Ka(e,i,r),t!==null&&(Cu(t,e,r),qa(t,e,r))},enqueueReplaceState:function(e,t,n){e=e._reactInternals;var r=xu(),i=Ga(r);i.tag=1,i.payload=t,n!=null&&(i.callback=n),t=Ka(e,i,r),t!==null&&(Cu(t,e,r),qa(t,e,r))},enqueueForceUpdate:function(e,t){e=e._reactInternals;var n=xu(),r=Ga(n);r.tag=2,t!=null&&(r.callback=t),t=Ka(e,r,n),t!==null&&(Cu(t,e,n),qa(t,e,n))}};function Js(e,t,n,r,i,a,o){return e=e.stateNode,typeof e.shouldComponentUpdate==`function`?e.shouldComponentUpdate(r,a,o):t.prototype&&t.prototype.isPureReactComponent?!Tr(n,r)||!Tr(i,a):!0}function Ys(e,t,n,r){e=t.state,typeof t.componentWillReceiveProps==`function`&&t.componentWillReceiveProps(n,r),typeof t.UNSAFE_componentWillReceiveProps==`function`&&t.UNSAFE_componentWillReceiveProps(n,r),t.state!==e&&qs.enqueueReplaceState(t,t.state,null)}function Xs(e,t){var n=t;if(`ref`in t)for(var r in n={},t)r!==`ref`&&(n[r]=t[r]);if(e=e.defaultProps)for(var i in n===t&&(n=h({},n)),e)n[i]===void 0&&(n[i]=e[i]);return n}function Zs(e){Qr(e)}function Qs(e){console.error(e)}function $s(e){Qr(e)}function ec(e,t){try{var n=e.onUncaughtError;n(t.value,{componentStack:t.stack})}catch(e){setTimeout(function(){throw e})}}function tc(e,t,n){try{var r=e.onCaughtError;r(n.value,{componentStack:n.stack,errorBoundary:t.tag===1?t.stateNode:null})}catch(e){setTimeout(function(){throw e})}}function nc(e,t,n){return n=Ga(n),n.tag=3,n.payload={element:null},n.callback=function(){ec(e,t)},n}function rc(e){return e=Ga(e),e.tag=3,e}function ic(e,t,n,r){var i=n.type.getDerivedStateFromError;if(typeof i==`function`){var a=r.value;e.payload=function(){return i(a)},e.callback=function(){tc(t,n,r)}}var o=n.stateNode;o!==null&&typeof o.componentDidCatch==`function`&&(e.callback=function(){tc(t,n,r),typeof i!=`function`&&(du===null?du=new Set([this]):du.add(this));var e=r.stack;this.componentDidCatch(r.value,{componentStack:e===null?``:e})})}function ac(e,t,n,r,a){if(n.flags|=32768,typeof r==`object`&&r&&typeof r.then==`function`){if(t=n.alternate,t!==null&&ea(t,n,a,!0),n=ao.current,n!==null){switch(n.tag){case 31:case 13:return oo===null?Fu():n.alternate===null&&Ql===0&&(Ql=3),n.flags&=-257,n.flags|=65536,n.lanes=a,r===Da?n.flags|=16384:(t=n.updateQueue,t===null?n.updateQueue=new Set([r]):t.add(r),ed(e,r,a)),!1;case 22:return n.flags|=65536,r===Da?n.flags|=16384:(t=n.updateQueue,t===null?(t={transitions:null,markerInstances:null,retryQueue:new Set([r])},n.updateQueue=t):(n=t.retryQueue,n===null?t.retryQueue=new Set([r]):n.add(r)),ed(e,r,a)),!1}throw Error(i(435,n.tag))}return ed(e,r,a),Fu(),!1}if(Li)return t=ao.current,t===null?(r!==Bi&&(t=Error(i(423),{cause:r}),Ki(bi(t,n))),e=e.current.alternate,e.flags|=65536,a&=-a,e.lanes|=a,r=bi(r,n),a=nc(e.stateNode,r,a),Ja(e,a),Ql!==4&&(Ql=2)):(!(t.flags&65536)&&(t.flags|=256),t.flags|=65536,t.lanes=a,r!==Bi&&(e=Error(i(422),{cause:r}),Ki(bi(e,n)))),!1;var o=Error(i(520),{cause:r});if(o=bi(o,n),iu===null?iu=[o]:iu.push(o),Ql!==4&&(Ql=2),t===null)return!0;r=bi(r,n),n=t;do{switch(n.tag){case 3:return n.flags|=65536,e=a&-a,n.lanes|=e,e=nc(n.stateNode,r,e),Ja(n,e),!1;case 1:if(t=n.type,o=n.stateNode,!(n.flags&128)&&(typeof t.getDerivedStateFromError==`function`||o!==null&&typeof o.componentDidCatch==`function`&&(du===null||!du.has(o))))return n.flags|=65536,a&=-a,n.lanes|=a,a=rc(a),ic(a,e,n,r),Ja(n,a),!1}n=n.return}while(n!==null);return!1}var oc=Error(i(461)),sc=!1;function cc(e,t,n,r){t.child=e===null?Va(t,null,n,r):Ba(t,e.child,n,r)}function lc(e,t,n,r,i){n=n.render;var a=t.ref;if(`ref`in r){var o={};for(var s in r)s!==`ref`&&(o[s]=r[s])}else o=r;return na(t),r=Do(e,t,n,o,a,i),s=jo(),e!==null&&!sc?(Mo(e,t,i),Nc(e,t,i)):(Li&&s&&Mi(t),t.flags|=1,cc(e,t,r,i),t.child)}function uc(e,t,n,r,i){if(e===null){var a=n.type;return typeof a==`function`&&!di(a)&&a.defaultProps===void 0&&n.compare===null?(t.tag=15,t.type=a,dc(e,t,a,r,i)):(e=mi(n.type,null,r,t,t.mode,i),e.ref=t.ref,e.return=t,t.child=e)}if(a=e.child,!Pc(e,i)){var o=a.memoizedProps;if(n=n.compare,n=n===null?Tr:n,n(o,r)&&e.ref===t.ref)return Nc(e,t,i)}return t.flags|=1,e=fi(a,r),e.ref=t.ref,e.return=t,t.child=e}function dc(e,t,n,r,i){if(e!==null){var a=e.memoizedProps;if(Tr(a,r)&&e.ref===t.ref)if(sc=!1,t.pendingProps=r=a,Pc(e,i))e.flags&131072&&(sc=!0);else return t.lanes=e.lanes,Nc(e,t,i)}return yc(e,t,n,r,i)}function fc(e,t,n,r){var i=r.children,a=e===null?null:e.memoizedState;if(e===null&&t.stateNode===null&&(t.stateNode={_visibility:1,_pendingMarkers:null,_retryCache:null,_transitions:null}),r.mode===`hidden`){if(t.flags&128){if(a=a===null?n:a.baseLanes|n,e!==null){for(r=t.child=e.child,i=0;r!==null;)i=i|r.lanes|r.childLanes,r=r.sibling;r=i&~a}else r=0,t.child=null;return mc(e,t,a,n,r)}if(n&536870912)t.memoizedState={baseLanes:0,cachePool:null},e!==null&&Sa(t,a===null?null:a.cachePool),a===null?ro():no(t,a),lo(t);else return r=t.lanes=536870912,mc(e,t,a===null?n:a.baseLanes|n,n,r)}else a===null?(e!==null&&Sa(t,null),ro(),uo(t)):(Sa(t,a.cachePool),no(t,a),uo(t),t.memoizedState=null);return cc(e,t,i,n),t.child}function pc(e,t){return e!==null&&e.tag===22||t.stateNode!==null||(t.stateNode={_visibility:1,_pendingMarkers:null,_retryCache:null,_transitions:null}),t.sibling}function mc(e,t,n,r,i){var a=xa();return a=a===null?null:{parent:la._currentValue,pool:a},t.memoizedState={baseLanes:n,cachePool:a},e!==null&&Sa(t,null),ro(),lo(t),e!==null&&ea(e,t,r,!0),t.childLanes=i,null}function hc(e,t){return t=Oc({mode:t.mode,children:t.children},e.mode),t.ref=e.ref,e.child=t,t.return=e,t}function gc(e,t,n){return Ba(t,e.child,null,n),e=hc(t,t.pendingProps),e.flags|=2,fo(t),t.memoizedState=null,e}function _c(e,t,n){var r=t.pendingProps,a=(t.flags&128)!=0;if(t.flags&=-129,e===null){if(Li){if(r.mode===`hidden`)return e=hc(t,r),t.lanes=536870912,pc(null,e);if(co(t),(e=Ii)?(e=mf(e,zi),e=e!==null&&e.data===`&`?e:null,e!==null&&(t.memoizedState={dehydrated:e,treeContext:Di===null?null:{id:Oi,overflow:ki},retryLane:536870912,hydrationErrors:null},n=_i(e),n.return=t,t.child=n,Fi=t,Ii=null)):e=null,e===null)throw Vi(t);return t.lanes=536870912,null}return hc(t,r)}var o=e.memoizedState;if(o!==null){var s=o.dehydrated;if(co(t),a)if(t.flags&256)t.flags&=-257,t=gc(e,t,n);else if(t.memoizedState!==null)t.child=e.child,t.flags|=128,t=null;else throw Error(i(558));else if(sc||ea(e,t,n,!1),a=(n&e.childLanes)!==0,sc||a){if(r=Ul,r!==null&&(s=nt(r,n),s!==0&&s!==o.retryLane))throw o.retryLane=s,ai(e,s),Cu(r,e,s),oc;Fu(),t=gc(e,t,n)}else e=o.treeContext,Ii=vf(s.nextSibling),Fi=t,Li=!0,Ri=null,zi=!1,e!==null&&Pi(t,e),t=hc(t,r),t.flags|=4096;return t}return e=fi(e.child,{mode:r.mode,children:r.children}),e.ref=t.ref,t.child=e,e.return=t,e}function vc(e,t){var n=t.ref;if(n===null)e!==null&&e.ref!==null&&(t.flags|=4194816);else{if(typeof n!=`function`&&typeof n!=`object`)throw Error(i(284));(e===null||e.ref!==n)&&(t.flags|=4194816)}}function yc(e,t,n,r,i){return na(t),n=Do(e,t,n,r,void 0,i),r=jo(),e!==null&&!sc?(Mo(e,t,i),Nc(e,t,i)):(Li&&r&&Mi(t),t.flags|=1,cc(e,t,n,i),t.child)}function bc(e,t,n,r,i,a){return na(t),t.updateQueue=null,n=ko(t,r,n,i),Oo(e),r=jo(),e!==null&&!sc?(Mo(e,t,a),Nc(e,t,a)):(Li&&r&&Mi(t),t.flags|=1,cc(e,t,n,a),t.child)}function xc(e,t,n,r,i){if(na(t),t.stateNode===null){var a=ci,o=n.contextType;typeof o==`object`&&o&&(a=ra(o)),a=new n(r,a),t.memoizedState=a.state!==null&&a.state!==void 0?a.state:null,a.updater=qs,t.stateNode=a,a._reactInternals=t,a=t.stateNode,a.props=r,a.state=t.memoizedState,a.refs={},Ua(t),o=n.contextType,a.context=typeof o==`object`&&o?ra(o):ci,a.state=t.memoizedState,o=n.getDerivedStateFromProps,typeof o==`function`&&(Ks(t,n,o,r),a.state=t.memoizedState),typeof n.getDerivedStateFromProps==`function`||typeof a.getSnapshotBeforeUpdate==`function`||typeof a.UNSAFE_componentWillMount!=`function`&&typeof a.componentWillMount!=`function`||(o=a.state,typeof a.componentWillMount==`function`&&a.componentWillMount(),typeof a.UNSAFE_componentWillMount==`function`&&a.UNSAFE_componentWillMount(),o!==a.state&&qs.enqueueReplaceState(a,a.state,null),Za(t,r,a,i),Xa(),a.state=t.memoizedState),typeof a.componentDidMount==`function`&&(t.flags|=4194308),r=!0}else if(e===null){a=t.stateNode;var s=t.memoizedProps,c=Xs(n,s);a.props=c;var l=a.context,u=n.contextType;o=ci,typeof u==`object`&&u&&(o=ra(u));var d=n.getDerivedStateFromProps;u=typeof d==`function`||typeof a.getSnapshotBeforeUpdate==`function`,s=t.pendingProps!==s,u||typeof a.UNSAFE_componentWillReceiveProps!=`function`&&typeof a.componentWillReceiveProps!=`function`||(s||l!==o)&&Ys(t,a,r,o),Ha=!1;var f=t.memoizedState;a.state=f,Za(t,r,a,i),Xa(),l=t.memoizedState,s||f!==l||Ha?(typeof d==`function`&&(Ks(t,n,d,r),l=t.memoizedState),(c=Ha||Js(t,n,c,r,f,l,o))?(u||typeof a.UNSAFE_componentWillMount!=`function`&&typeof a.componentWillMount!=`function`||(typeof a.componentWillMount==`function`&&a.componentWillMount(),typeof a.UNSAFE_componentWillMount==`function`&&a.UNSAFE_componentWillMount()),typeof a.componentDidMount==`function`&&(t.flags|=4194308)):(typeof a.componentDidMount==`function`&&(t.flags|=4194308),t.memoizedProps=r,t.memoizedState=l),a.props=r,a.state=l,a.context=o,r=c):(typeof a.componentDidMount==`function`&&(t.flags|=4194308),r=!1)}else{a=t.stateNode,Wa(e,t),o=t.memoizedProps,u=Xs(n,o),a.props=u,d=t.pendingProps,f=a.context,l=n.contextType,c=ci,typeof l==`object`&&l&&(c=ra(l)),s=n.getDerivedStateFromProps,(l=typeof s==`function`||typeof a.getSnapshotBeforeUpdate==`function`)||typeof a.UNSAFE_componentWillReceiveProps!=`function`&&typeof a.componentWillReceiveProps!=`function`||(o!==d||f!==c)&&Ys(t,a,r,c),Ha=!1,f=t.memoizedState,a.state=f,Za(t,r,a,i),Xa();var p=t.memoizedState;o!==d||f!==p||Ha||e!==null&&e.dependencies!==null&&ta(e.dependencies)?(typeof s==`function`&&(Ks(t,n,s,r),p=t.memoizedState),(u=Ha||Js(t,n,u,r,f,p,c)||e!==null&&e.dependencies!==null&&ta(e.dependencies))?(l||typeof a.UNSAFE_componentWillUpdate!=`function`&&typeof a.componentWillUpdate!=`function`||(typeof a.componentWillUpdate==`function`&&a.componentWillUpdate(r,p,c),typeof a.UNSAFE_componentWillUpdate==`function`&&a.UNSAFE_componentWillUpdate(r,p,c)),typeof a.componentDidUpdate==`function`&&(t.flags|=4),typeof a.getSnapshotBeforeUpdate==`function`&&(t.flags|=1024)):(typeof a.componentDidUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=4),typeof a.getSnapshotBeforeUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=1024),t.memoizedProps=r,t.memoizedState=p),a.props=r,a.state=p,a.context=c,r=u):(typeof a.componentDidUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=4),typeof a.getSnapshotBeforeUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=1024),r=!1)}return a=r,vc(e,t),r=(t.flags&128)!=0,a||r?(a=t.stateNode,n=r&&typeof n.getDerivedStateFromError!=`function`?null:a.render(),t.flags|=1,e!==null&&r?(t.child=Ba(t,e.child,null,i),t.child=Ba(t,null,n,i)):cc(e,t,n,i),t.memoizedState=a.state,e=t.child):e=Nc(e,t,i),e}function Sc(e,t,n,r){return Wi(),t.flags|=256,cc(e,t,n,r),t.child}var Cc={dehydrated:null,treeContext:null,retryLane:0,hydrationErrors:null};function wc(e){return{baseLanes:e,cachePool:Ca()}}function Tc(e,t,n){return e=e===null?0:e.childLanes&~n,t&&(e|=nu),e}function Ec(e,t,n){var r=t.pendingProps,a=!1,o=(t.flags&128)!=0,s;if((s=o)||(s=e!==null&&e.memoizedState===null?!1:(po.current&2)!=0),s&&(a=!0,t.flags&=-129),s=(t.flags&32)!=0,t.flags&=-33,e===null){if(Li){if(a?so(t):uo(t),(e=Ii)?(e=mf(e,zi),e=e!==null&&e.data!==`&`?e:null,e!==null&&(t.memoizedState={dehydrated:e,treeContext:Di===null?null:{id:Oi,overflow:ki},retryLane:536870912,hydrationErrors:null},n=_i(e),n.return=t,t.child=n,Fi=t,Ii=null)):e=null,e===null)throw Vi(t);return gf(e)?t.lanes=32:t.lanes=536870912,null}var c=r.children;return r=r.fallback,a?(uo(t),a=t.mode,c=Oc({mode:`hidden`,children:c},a),r=hi(r,a,n,null),c.return=t,r.return=t,c.sibling=r,t.child=c,r=t.child,r.memoizedState=wc(n),r.childLanes=Tc(e,s,n),t.memoizedState=Cc,pc(null,r)):(so(t),Dc(t,c))}var l=e.memoizedState;if(l!==null&&(c=l.dehydrated,c!==null)){if(o)t.flags&256?(so(t),t.flags&=-257,t=kc(e,t,n)):t.memoizedState===null?(uo(t),c=r.fallback,a=t.mode,r=Oc({mode:`visible`,children:r.children},a),c=hi(c,a,n,null),c.flags|=2,r.return=t,c.return=t,r.sibling=c,t.child=r,Ba(t,e.child,null,n),r=t.child,r.memoizedState=wc(n),r.childLanes=Tc(e,s,n),t.memoizedState=Cc,t=pc(null,r)):(uo(t),t.child=e.child,t.flags|=128,t=null);else if(so(t),gf(c)){if(s=c.nextSibling&&c.nextSibling.dataset,s)var u=s.dgst;s=u,r=Error(i(419)),r.stack=``,r.digest=s,Ki({value:r,source:null,stack:null}),t=kc(e,t,n)}else if(sc||ea(e,t,n,!1),s=(n&e.childLanes)!==0,sc||s){if(s=Ul,s!==null&&(r=nt(s,n),r!==0&&r!==l.retryLane))throw l.retryLane=r,ai(e,r),Cu(s,e,r),oc;hf(c)||Fu(),t=kc(e,t,n)}else hf(c)?(t.flags|=192,t.child=e.child,t=null):(e=l.treeContext,Ii=vf(c.nextSibling),Fi=t,Li=!0,Ri=null,zi=!1,e!==null&&Pi(t,e),t=Dc(t,r.children),t.flags|=4096);return t}return a?(uo(t),c=r.fallback,a=t.mode,l=e.child,u=l.sibling,r=fi(l,{mode:`hidden`,children:r.children}),r.subtreeFlags=l.subtreeFlags&65011712,u===null?(c=hi(c,a,n,null),c.flags|=2):c=fi(u,c),c.return=t,r.return=t,r.sibling=c,t.child=r,pc(null,r),r=t.child,c=e.child.memoizedState,c===null?c=wc(n):(a=c.cachePool,a===null?a=Ca():(l=la._currentValue,a=a.parent===l?a:{parent:l,pool:l}),c={baseLanes:c.baseLanes|n,cachePool:a}),r.memoizedState=c,r.childLanes=Tc(e,s,n),t.memoizedState=Cc,pc(e.child,r)):(so(t),n=e.child,e=n.sibling,n=fi(n,{mode:`visible`,children:r.children}),n.return=t,n.sibling=null,e!==null&&(s=t.deletions,s===null?(t.deletions=[e],t.flags|=16):s.push(e)),t.child=n,t.memoizedState=null,n)}function Dc(e,t){return t=Oc({mode:`visible`,children:t},e.mode),t.return=e,e.child=t}function Oc(e,t){return e=ui(22,e,null,t),e.lanes=0,e}function kc(e,t,n){return Ba(t,e.child,null,n),e=Dc(t,t.pendingProps.children),e.flags|=2,t.memoizedState=null,e}function Ac(e,t,n){e.lanes|=t;var r=e.alternate;r!==null&&(r.lanes|=t),Qi(e.return,t,n)}function jc(e,t,n,r,i,a){var o=e.memoizedState;o===null?e.memoizedState={isBackwards:t,rendering:null,renderingStartTime:0,last:r,tail:n,tailMode:i,treeForkCount:a}:(o.isBackwards=t,o.rendering=null,o.renderingStartTime=0,o.last=r,o.tail=n,o.tailMode=i,o.treeForkCount=a)}function Mc(e,t,n){var r=t.pendingProps,i=r.revealOrder,a=r.tail;r=r.children;var o=po.current,s=(o&2)!=0;if(s?(o=o&1|2,t.flags|=128):o&=1,I(po,o),cc(e,t,r,n),r=Li?wi:0,!s&&e!==null&&e.flags&128)a:for(e=t.child;e!==null;){if(e.tag===13)e.memoizedState!==null&&Ac(e,n,t);else if(e.tag===19)Ac(e,n,t);else if(e.child!==null){e.child.return=e,e=e.child;continue}if(e===t)break a;for(;e.sibling===null;){if(e.return===null||e.return===t)break a;e=e.return}e.sibling.return=e.return,e=e.sibling}switch(i){case`forwards`:for(n=t.child,i=null;n!==null;)e=n.alternate,e!==null&&mo(e)===null&&(i=n),n=n.sibling;n=i,n===null?(i=t.child,t.child=null):(i=n.sibling,n.sibling=null),jc(t,!1,i,n,a,r);break;case`backwards`:case`unstable_legacy-backwards`:for(n=null,i=t.child,t.child=null;i!==null;){if(e=i.alternate,e!==null&&mo(e)===null){t.child=i;break}e=i.sibling,i.sibling=n,n=i,i=e}jc(t,!0,n,null,a,r);break;case`together`:jc(t,!1,null,null,void 0,r);break;default:t.memoizedState=null}return t.child}function Nc(e,t,n){if(e!==null&&(t.dependencies=e.dependencies),$l|=t.lanes,(n&t.childLanes)===0)if(e!==null){if(ea(e,t,n,!1),(n&t.childLanes)===0)return null}else return null;if(e!==null&&t.child!==e.child)throw Error(i(153));if(t.child!==null){for(e=t.child,n=fi(e,e.pendingProps),t.child=n,n.return=t;e.sibling!==null;)e=e.sibling,n=n.sibling=fi(e,e.pendingProps),n.return=t;n.sibling=null}return t.child}function Pc(e,t){return(e.lanes&t)===0?(e=e.dependencies,!!(e!==null&&ta(e))):!0}function Fc(e,t,n){switch(t.tag){case 3:de(t,t.stateNode.containerInfo),Xi(t,la,e.memoizedState.cache),Wi();break;case 27:case 5:pe(t);break;case 4:de(t,t.stateNode.containerInfo);break;case 10:Xi(t,t.type,t.memoizedProps.value);break;case 31:if(t.memoizedState!==null)return t.flags|=128,co(t),null;break;case 13:var r=t.memoizedState;if(r!==null)return r.dehydrated===null?(n&t.child.childLanes)===0?(so(t),e=Nc(e,t,n),e===null?null:e.sibling):Ec(e,t,n):(so(t),t.flags|=128,null);so(t);break;case 19:var i=(e.flags&128)!=0;if(r=(n&t.childLanes)!==0,r||=(ea(e,t,n,!1),(n&t.childLanes)!==0),i){if(r)return Mc(e,t,n);t.flags|=128}if(i=t.memoizedState,i!==null&&(i.rendering=null,i.tail=null,i.lastEffect=null),I(po,po.current),r)break;return null;case 22:return t.lanes=0,fc(e,t,n,t.pendingProps);case 24:Xi(t,la,e.memoizedState.cache)}return Nc(e,t,n)}function Ic(e,t,n){if(e!==null)if(e.memoizedProps!==t.pendingProps)sc=!0;else{if(!Pc(e,n)&&!(t.flags&128))return sc=!1,Fc(e,t,n);sc=!!(e.flags&131072)}else sc=!1,Li&&t.flags&1048576&&ji(t,wi,t.index);switch(t.lanes=0,t.tag){case 16:a:{var r=t.pendingProps;if(e=Aa(t.elementType),t.type=e,typeof e==`function`)di(e)?(r=Xs(e,r),t.tag=1,t=xc(null,t,e,r,n)):(t.tag=0,t=yc(null,t,e,r,n));else{if(e!=null){var a=e.$$typeof;if(a===w){t.tag=11,t=lc(null,t,e,r,n);break a}else if(a===D){t.tag=14,t=uc(null,t,e,r,n);break a}}throw t=te(e)||e,Error(i(306,t,``))}}return t;case 0:return yc(e,t,t.type,t.pendingProps,n);case 1:return r=t.type,a=Xs(r,t.pendingProps),xc(e,t,r,a,n);case 3:a:{if(de(t,t.stateNode.containerInfo),e===null)throw Error(i(387));r=t.pendingProps;var o=t.memoizedState;a=o.element,Wa(e,t),Za(t,r,null,n);var s=t.memoizedState;if(r=s.cache,Xi(t,la,r),r!==o.cache&&$i(t,[la],n,!0),Xa(),r=s.element,o.isDehydrated)if(o={element:r,isDehydrated:!1,cache:s.cache},t.updateQueue.baseState=o,t.memoizedState=o,t.flags&256){t=Sc(e,t,r,n);break a}else if(r!==a){a=bi(Error(i(424)),t),Ki(a),t=Sc(e,t,r,n);break a}else{switch(e=t.stateNode.containerInfo,e.nodeType){case 9:e=e.body;break;default:e=e.nodeName===`HTML`?e.ownerDocument.body:e}for(Ii=vf(e.firstChild),Fi=t,Li=!0,Ri=null,zi=!0,n=Va(t,null,r,n),t.child=n;n;)n.flags=n.flags&-3|4096,n=n.sibling}else{if(Wi(),r===a){t=Nc(e,t,n);break a}cc(e,t,r,n)}t=t.child}return t;case 26:return vc(e,t),e===null?(n=zf(t.type,null,t.pendingProps,null))?t.memoizedState=n:Li||(n=t.type,e=t.pendingProps,r=U(le.current).createElement(n),r[ct]=t,r[lt]=e,Gd(r,n,e),xt(r),t.stateNode=r):t.memoizedState=zf(t.type,e.memoizedProps,t.pendingProps,e.memoizedState),null;case 27:return pe(t),e===null&&Li&&(r=t.stateNode=Sf(t.type,t.pendingProps,le.current),Fi=t,zi=!0,a=Ii,cf(t.type)?(yf=a,Ii=vf(r.firstChild)):Ii=a),cc(e,t,t.pendingProps.children,n),vc(e,t),e===null&&(t.flags|=4194304),t.child;case 5:return e===null&&Li&&((a=r=Ii)&&(r=ff(r,t.type,t.pendingProps,zi),r===null?a=!1:(t.stateNode=r,Fi=t,Ii=vf(r.firstChild),zi=!1,a=!0)),a||Vi(t)),pe(t),a=t.type,o=t.pendingProps,s=e===null?null:e.memoizedProps,r=o.children,$d(a,o)?r=null:s!==null&&$d(a,s)&&(t.flags|=32),t.memoizedState!==null&&(a=Do(e,t,Ao,null,null,n),cp._currentValue=a),vc(e,t),cc(e,t,r,n),t.child;case 6:return e===null&&Li&&((e=n=Ii)&&(n=pf(n,t.pendingProps,zi),n===null?e=!1:(t.stateNode=n,Fi=t,Ii=null,e=!0)),e||Vi(t)),null;case 13:return Ec(e,t,n);case 4:return de(t,t.stateNode.containerInfo),r=t.pendingProps,e===null?t.child=Ba(t,null,r,n):cc(e,t,r,n),t.child;case 11:return lc(e,t,t.type,t.pendingProps,n);case 7:return cc(e,t,t.pendingProps,n),t.child;case 8:return cc(e,t,t.pendingProps.children,n),t.child;case 12:return cc(e,t,t.pendingProps.children,n),t.child;case 10:return r=t.pendingProps,Xi(t,t.type,r.value),cc(e,t,r.children,n),t.child;case 9:return a=t.type._context,r=t.pendingProps.children,na(t),a=ra(a),r=r(a),t.flags|=1,cc(e,t,r,n),t.child;case 14:return uc(e,t,t.type,t.pendingProps,n);case 15:return dc(e,t,t.type,t.pendingProps,n);case 19:return Mc(e,t,n);case 31:return _c(e,t,n);case 22:return fc(e,t,n,t.pendingProps);case 24:return na(t),r=ra(la),e===null?(a=xa(),a===null&&(a=Ul,o=ua(),a.pooledCache=o,o.refCount++,o!==null&&(a.pooledCacheLanes|=n),a=o),t.memoizedState={parent:r,cache:a},Ua(t),Xi(t,la,a)):((e.lanes&n)!==0&&(Wa(e,t),Za(t,null,null,n),Xa()),a=e.memoizedState,o=t.memoizedState,a.parent===r?(r=o.cache,Xi(t,la,r),r!==a.cache&&$i(t,[la],n,!0)):(a={parent:r,cache:r},t.memoizedState=a,t.lanes===0&&(t.memoizedState=t.updateQueue.baseState=a),Xi(t,la,r))),cc(e,t,t.pendingProps.children,n),t.child;case 29:throw t.pendingProps}throw Error(i(156,t.tag))}function Lc(e){e.flags|=4}function Rc(e,t,n,r,i){if((t=(e.mode&32)!=0)&&(t=!1),t){if(e.flags|=16777216,(i&335544128)===i)if(e.stateNode.complete)e.flags|=8192;else if(Mu())e.flags|=8192;else throw ja=Da,Ta}else e.flags&=-16777217}function zc(e,t){if(t.type!==`stylesheet`||t.state.loading&4)e.flags&=-16777217;else if(e.flags|=16777216,!ep(t))if(Mu())e.flags|=8192;else throw ja=Da,Ta}function Bc(e,t){t!==null&&(e.flags|=4),e.flags&16384&&(t=e.tag===22?536870912:Xe(),e.lanes|=t,ru|=t)}function Vc(e,t){if(!Li)switch(e.tailMode){case`hidden`:t=e.tail;for(var n=null;t!==null;)t.alternate!==null&&(n=t),t=t.sibling;n===null?e.tail=null:n.sibling=null;break;case`collapsed`:n=e.tail;for(var r=null;n!==null;)n.alternate!==null&&(r=n),n=n.sibling;r===null?t||e.tail===null?e.tail=null:e.tail.sibling=null:r.sibling=null}}function Hc(e){var t=e.alternate!==null&&e.alternate.child===e.child,n=0,r=0;if(t)for(var i=e.child;i!==null;)n|=i.lanes|i.childLanes,r|=i.subtreeFlags&65011712,r|=i.flags&65011712,i.return=e,i=i.sibling;else for(i=e.child;i!==null;)n|=i.lanes|i.childLanes,r|=i.subtreeFlags,r|=i.flags,i.return=e,i=i.sibling;return e.subtreeFlags|=r,e.childLanes=n,t}function Uc(e,t,n){var r=t.pendingProps;switch(Ni(t),t.tag){case 16:case 15:case 0:case 11:case 7:case 8:case 12:case 9:case 14:return Hc(t),null;case 1:return Hc(t),null;case 3:return n=t.stateNode,r=null,e!==null&&(r=e.memoizedState.cache),t.memoizedState.cache!==r&&(t.flags|=2048),Zi(la),fe(),n.pendingContext&&=(n.context=n.pendingContext,null),(e===null||e.child===null)&&(Ui(t)?Lc(t):e===null||e.memoizedState.isDehydrated&&!(t.flags&256)||(t.flags|=1024,Gi())),Hc(t),null;case 26:var a=t.type,o=t.memoizedState;return e===null?(Lc(t),o===null?(Hc(t),Rc(t,a,null,r,n)):(Hc(t),zc(t,o))):o?o===e.memoizedState?(Hc(t),t.flags&=-16777217):(Lc(t),Hc(t),zc(t,o)):(e=e.memoizedProps,e!==r&&Lc(t),Hc(t),Rc(t,a,e,r,n)),null;case 27:if(me(t),n=le.current,a=t.type,e!==null&&t.stateNode!=null)e.memoizedProps!==r&&Lc(t);else{if(!r){if(t.stateNode===null)throw Error(i(166));return Hc(t),null}e=se.current,Ui(t)?L(t,e):(e=Sf(a,r,n),t.stateNode=e,Lc(t))}return Hc(t),null;case 5:if(me(t),a=t.type,e!==null&&t.stateNode!=null)e.memoizedProps!==r&&Lc(t);else{if(!r){if(t.stateNode===null)throw Error(i(166));return Hc(t),null}if(o=se.current,Ui(t))L(t,o);else{var s=U(le.current);switch(o){case 1:o=s.createElementNS(`http://www.w3.org/2000/svg`,a);break;case 2:o=s.createElementNS(`http://www.w3.org/1998/Math/MathML`,a);break;default:switch(a){case`svg`:o=s.createElementNS(`http://www.w3.org/2000/svg`,a);break;case`math`:o=s.createElementNS(`http://www.w3.org/1998/Math/MathML`,a);break;case`script`:o=s.createElement(`div`),o.innerHTML=`<script><\/script>`,o=o.removeChild(o.firstChild);break;case`select`:o=typeof r.is==`string`?s.createElement(`select`,{is:r.is}):s.createElement(`select`),r.multiple?o.multiple=!0:r.size&&(o.size=r.size);break;default:o=typeof r.is==`string`?s.createElement(a,{is:r.is}):s.createElement(a)}}o[ct]=t,o[lt]=r;a:for(s=t.child;s!==null;){if(s.tag===5||s.tag===6)o.appendChild(s.stateNode);else if(s.tag!==4&&s.tag!==27&&s.child!==null){s.child.return=s,s=s.child;continue}if(s===t)break a;for(;s.sibling===null;){if(s.return===null||s.return===t)break a;s=s.return}s.sibling.return=s.return,s=s.sibling}t.stateNode=o;a:switch(Gd(o,a,r),a){case`button`:case`input`:case`select`:case`textarea`:r=!!r.autoFocus;break a;case`img`:r=!0;break a;default:r=!1}r&&Lc(t)}}return Hc(t),Rc(t,t.type,e===null?null:e.memoizedProps,t.pendingProps,n),null;case 6:if(e&&t.stateNode!=null)e.memoizedProps!==r&&Lc(t);else{if(typeof r!=`string`&&t.stateNode===null)throw Error(i(166));if(e=le.current,Ui(t)){if(e=t.stateNode,n=t.memoizedProps,r=null,a=Fi,a!==null)switch(a.tag){case 27:case 5:r=a.memoizedProps}e[ct]=t,e=!!(e.nodeValue===n||r!==null&&!0===r.suppressHydrationWarning||Hd(e.nodeValue,n)),e||Vi(t,!0)}else e=U(e).createTextNode(r),e[ct]=t,t.stateNode=e}return Hc(t),null;case 31:if(n=t.memoizedState,e===null||e.memoizedState!==null){if(r=Ui(t),n!==null){if(e===null){if(!r)throw Error(i(318));if(e=t.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(557));e[ct]=t}else Wi(),!(t.flags&128)&&(t.memoizedState=null),t.flags|=4;Hc(t),e=!1}else n=Gi(),e!==null&&e.memoizedState!==null&&(e.memoizedState.hydrationErrors=n),e=!0;if(!e)return t.flags&256?(fo(t),t):(fo(t),null);if(t.flags&128)throw Error(i(558))}return Hc(t),null;case 13:if(r=t.memoizedState,e===null||e.memoizedState!==null&&e.memoizedState.dehydrated!==null){if(a=Ui(t),r!==null&&r.dehydrated!==null){if(e===null){if(!a)throw Error(i(318));if(a=t.memoizedState,a=a===null?null:a.dehydrated,!a)throw Error(i(317));a[ct]=t}else Wi(),!(t.flags&128)&&(t.memoizedState=null),t.flags|=4;Hc(t),a=!1}else a=Gi(),e!==null&&e.memoizedState!==null&&(e.memoizedState.hydrationErrors=a),a=!0;if(!a)return t.flags&256?(fo(t),t):(fo(t),null)}return fo(t),t.flags&128?(t.lanes=n,t):(n=r!==null,e=e!==null&&e.memoizedState!==null,n&&(r=t.child,a=null,r.alternate!==null&&r.alternate.memoizedState!==null&&r.alternate.memoizedState.cachePool!==null&&(a=r.alternate.memoizedState.cachePool.pool),o=null,r.memoizedState!==null&&r.memoizedState.cachePool!==null&&(o=r.memoizedState.cachePool.pool),o!==a&&(r.flags|=2048)),n!==e&&n&&(t.child.flags|=8192),Bc(t,t.updateQueue),Hc(t),null);case 4:return fe(),e===null&&Md(t.stateNode.containerInfo),Hc(t),null;case 10:return Zi(t.type),Hc(t),null;case 19:if(oe(po),r=t.memoizedState,r===null)return Hc(t),null;if(a=(t.flags&128)!=0,o=r.rendering,o===null)if(a)Vc(r,!1);else{if(Ql!==0||e!==null&&e.flags&128)for(e=t.child;e!==null;){if(o=mo(e),o!==null){for(t.flags|=128,Vc(r,!1),e=o.updateQueue,t.updateQueue=e,Bc(t,e),t.subtreeFlags=0,e=n,n=t.child;n!==null;)pi(n,e),n=n.sibling;return I(po,po.current&1|2),Li&&Ai(t,r.treeForkCount),t.child}e=e.sibling}r.tail!==null&&De()>lu&&(t.flags|=128,a=!0,Vc(r,!1),t.lanes=4194304)}else{if(!a)if(e=mo(o),e!==null){if(t.flags|=128,a=!0,e=e.updateQueue,t.updateQueue=e,Bc(t,e),Vc(r,!0),r.tail===null&&r.tailMode===`hidden`&&!o.alternate&&!Li)return Hc(t),null}else 2*De()-r.renderingStartTime>lu&&n!==536870912&&(t.flags|=128,a=!0,Vc(r,!1),t.lanes=4194304);r.isBackwards?(o.sibling=t.child,t.child=o):(e=r.last,e===null?t.child=o:e.sibling=o,r.last=o)}return r.tail===null?(Hc(t),null):(e=r.tail,r.rendering=e,r.tail=e.sibling,r.renderingStartTime=De(),e.sibling=null,n=po.current,I(po,a?n&1|2:n&1),Li&&Ai(t,r.treeForkCount),e);case 22:case 23:return fo(t),io(),r=t.memoizedState!==null,e===null?r&&(t.flags|=8192):e.memoizedState!==null!==r&&(t.flags|=8192),r?n&536870912&&!(t.flags&128)&&(Hc(t),t.subtreeFlags&6&&(t.flags|=8192)):Hc(t),n=t.updateQueue,n!==null&&Bc(t,n.retryQueue),n=null,e!==null&&e.memoizedState!==null&&e.memoizedState.cachePool!==null&&(n=e.memoizedState.cachePool.pool),r=null,t.memoizedState!==null&&t.memoizedState.cachePool!==null&&(r=t.memoizedState.cachePool.pool),r!==n&&(t.flags|=2048),e!==null&&oe(ba),null;case 24:return n=null,e!==null&&(n=e.memoizedState.cache),t.memoizedState.cache!==n&&(t.flags|=2048),Zi(la),Hc(t),null;case 25:return null;case 30:return null}throw Error(i(156,t.tag))}function Wc(e,t){switch(Ni(t),t.tag){case 1:return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 3:return Zi(la),fe(),e=t.flags,e&65536&&!(e&128)?(t.flags=e&-65537|128,t):null;case 26:case 27:case 5:return me(t),null;case 31:if(t.memoizedState!==null){if(fo(t),t.alternate===null)throw Error(i(340));Wi()}return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 13:if(fo(t),e=t.memoizedState,e!==null&&e.dehydrated!==null){if(t.alternate===null)throw Error(i(340));Wi()}return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 19:return oe(po),null;case 4:return fe(),null;case 10:return Zi(t.type),null;case 22:case 23:return fo(t),io(),e!==null&&oe(ba),e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 24:return Zi(la),null;case 25:return null;default:return null}}function Gc(e,t){switch(Ni(t),t.tag){case 3:Zi(la),fe();break;case 26:case 27:case 5:me(t);break;case 4:fe();break;case 31:t.memoizedState!==null&&fo(t);break;case 13:fo(t);break;case 19:oe(po);break;case 10:Zi(t.type);break;case 22:case 23:fo(t),io(),e!==null&&oe(ba);break;case 24:Zi(la)}}function Kc(e,t){try{var n=t.updateQueue,r=n===null?null:n.lastEffect;if(r!==null){var i=r.next;n=i;do{if((n.tag&e)===e){r=void 0;var a=n.create,o=n.inst;r=a(),o.destroy=r}n=n.next}while(n!==i)}}catch(e){$u(t,t.return,e)}}function qc(e,t,n){try{var r=t.updateQueue,i=r===null?null:r.lastEffect;if(i!==null){var a=i.next;r=a;do{if((r.tag&e)===e){var o=r.inst,s=o.destroy;if(s!==void 0){o.destroy=void 0,i=t;var c=n,l=s;try{l()}catch(e){$u(i,c,e)}}}r=r.next}while(r!==a)}}catch(e){$u(t,t.return,e)}}function Jc(e){var t=e.updateQueue;if(t!==null){var n=e.stateNode;try{$a(t,n)}catch(t){$u(e,e.return,t)}}}function Yc(e,t,n){n.props=Xs(e.type,e.memoizedProps),n.state=e.memoizedState;try{n.componentWillUnmount()}catch(n){$u(e,t,n)}}function Xc(e,t){try{var n=e.ref;if(n!==null){switch(e.tag){case 26:case 27:case 5:var r=e.stateNode;break;case 30:r=e.stateNode;break;default:r=e.stateNode}typeof n==`function`?e.refCleanup=n(r):n.current=r}}catch(n){$u(e,t,n)}}function Zc(e,t){var n=e.ref,r=e.refCleanup;if(n!==null)if(typeof r==`function`)try{r()}catch(n){$u(e,t,n)}finally{e.refCleanup=null,e=e.alternate,e!=null&&(e.refCleanup=null)}else if(typeof n==`function`)try{n(null)}catch(n){$u(e,t,n)}else n.current=null}function Qc(e){var t=e.type,n=e.memoizedProps,r=e.stateNode;try{a:switch(t){case`button`:case`input`:case`select`:case`textarea`:n.autoFocus&&r.focus();break a;case`img`:n.src?r.src=n.src:n.srcSet&&(r.srcset=n.srcSet)}}catch(t){$u(e,e.return,t)}}function $c(e,t,n){try{var r=e.stateNode;Kd(r,e.type,n,t),r[lt]=t}catch(t){$u(e,e.return,t)}}function el(e){return e.tag===5||e.tag===3||e.tag===26||e.tag===27&&cf(e.type)||e.tag===4}function tl(e){a:for(;;){for(;e.sibling===null;){if(e.return===null||el(e.return))return null;e=e.return}for(e.sibling.return=e.return,e=e.sibling;e.tag!==5&&e.tag!==6&&e.tag!==18;){if(e.tag===27&&cf(e.type)||e.flags&2||e.child===null||e.tag===4)continue a;e.child.return=e,e=e.child}if(!(e.flags&2))return e.stateNode}}function nl(e,t,n){var r=e.tag;if(r===5||r===6)e=e.stateNode,t?(n.nodeType===9?n.body:n.nodeName===`HTML`?n.ownerDocument.body:n).insertBefore(e,t):(t=n.nodeType===9?n.body:n.nodeName===`HTML`?n.ownerDocument.body:n,t.appendChild(e),n=n._reactRootContainer,n!=null||t.onclick!==null||(t.onclick=tn));else if(r!==4&&(r===27&&cf(e.type)&&(n=e.stateNode,t=null),e=e.child,e!==null))for(nl(e,t,n),e=e.sibling;e!==null;)nl(e,t,n),e=e.sibling}function rl(e,t,n){var r=e.tag;if(r===5||r===6)e=e.stateNode,t?n.insertBefore(e,t):n.appendChild(e);else if(r!==4&&(r===27&&cf(e.type)&&(n=e.stateNode),e=e.child,e!==null))for(rl(e,t,n),e=e.sibling;e!==null;)rl(e,t,n),e=e.sibling}function il(e){var t=e.stateNode,n=e.memoizedProps;try{for(var r=e.type,i=t.attributes;i.length;)t.removeAttributeNode(i[0]);Gd(t,r,n),t[ct]=e,t[lt]=n}catch(t){$u(e,e.return,t)}}var B=!1,al=!1,ol=!1,sl=typeof WeakSet==`function`?WeakSet:Set,cl=null;function ll(e,t){if(e=e.containerInfo,Yd=_p,e=kr(e),Ar(e)){if(`selectionStart`in e)var n={start:e.selectionStart,end:e.selectionEnd};else a:{n=(n=e.ownerDocument)&&n.defaultView||window;var r=n.getSelection&&n.getSelection();if(r&&r.rangeCount!==0){n=r.anchorNode;var a=r.anchorOffset,o=r.focusNode;r=r.focusOffset;try{n.nodeType,o.nodeType}catch{n=null;break a}var s=0,c=-1,l=-1,u=0,d=0,f=e,p=null;b:for(;;){for(var m;f!==n||a!==0&&f.nodeType!==3||(c=s+a),f!==o||r!==0&&f.nodeType!==3||(l=s+r),f.nodeType===3&&(s+=f.nodeValue.length),(m=f.firstChild)!==null;)p=f,f=m;for(;;){if(f===e)break b;if(p===n&&++u===a&&(c=s),p===o&&++d===r&&(l=s),(m=f.nextSibling)!==null)break;f=p,p=f.parentNode}f=m}n=c===-1||l===-1?null:{start:c,end:l}}else n=null}n||={start:0,end:0}}else n=null;for(Xd={focusedElem:e,selectionRange:n},_p=!1,cl=t;cl!==null;)if(t=cl,e=t.child,t.subtreeFlags&1028&&e!==null)e.return=t,cl=e;else for(;cl!==null;){switch(t=cl,o=t.alternate,e=t.flags,t.tag){case 0:if(e&4&&(e=t.updateQueue,e=e===null?null:e.events,e!==null))for(n=0;n<e.length;n++)a=e[n],a.ref.impl=a.nextImpl;break;case 11:case 15:break;case 1:if(e&1024&&o!==null){e=void 0,n=t,a=o.memoizedProps,o=o.memoizedState,r=n.stateNode;try{var h=Xs(n.type,a);e=r.getSnapshotBeforeUpdate(h,o),r.__reactInternalSnapshotBeforeUpdate=e}catch(e){$u(n,n.return,e)}}break;case 3:if(e&1024){if(e=t.stateNode.containerInfo,n=e.nodeType,n===9)df(e);else if(n===1)switch(e.nodeName){case`HEAD`:case`HTML`:case`BODY`:df(e);break;default:e.textContent=``}}break;case 5:case 26:case 27:case 6:case 4:case 17:break;default:if(e&1024)throw Error(i(163))}if(e=t.sibling,e!==null){e.return=t.return,cl=e;break}cl=t.return}}function ul(e,t,n){var r=n.flags;switch(n.tag){case 0:case 11:case 15:wl(e,n),r&4&&Kc(5,n);break;case 1:if(wl(e,n),r&4)if(e=n.stateNode,t===null)try{e.componentDidMount()}catch(e){$u(n,n.return,e)}else{var i=Xs(n.type,t.memoizedProps);t=t.memoizedState;try{e.componentDidUpdate(i,t,e.__reactInternalSnapshotBeforeUpdate)}catch(e){$u(n,n.return,e)}}r&64&&Jc(n),r&512&&Xc(n,n.return);break;case 3:if(wl(e,n),r&64&&(e=n.updateQueue,e!==null)){if(t=null,n.child!==null)switch(n.child.tag){case 27:case 5:t=n.child.stateNode;break;case 1:t=n.child.stateNode}try{$a(e,t)}catch(e){$u(n,n.return,e)}}break;case 27:t===null&&r&4&&il(n);case 26:case 5:wl(e,n),t===null&&r&4&&Qc(n),r&512&&Xc(n,n.return);break;case 12:wl(e,n);break;case 31:wl(e,n),r&4&&gl(e,n);break;case 13:wl(e,n),r&4&&_l(e,n),r&64&&(e=n.memoizedState,e!==null&&(e=e.dehydrated,e!==null&&(n=rd.bind(null,n),_f(e,n))));break;case 22:if(r=n.memoizedState!==null||B,!r){t=t!==null&&t.memoizedState!==null||al,i=B;var a=al;B=r,(al=t)&&!a?H(e,n,(n.subtreeFlags&8772)!=0):wl(e,n),B=i,al=a}break;case 30:break;default:wl(e,n)}}function dl(e){var t=e.alternate;t!==null&&(e.alternate=null,dl(t)),e.child=null,e.deletions=null,e.sibling=null,e.tag===5&&(t=e.stateNode,t!==null&&gt(t)),e.stateNode=null,e.return=null,e.dependencies=null,e.memoizedProps=null,e.memoizedState=null,e.pendingProps=null,e.stateNode=null,e.updateQueue=null}var fl=null,pl=!1;function ml(e,t,n){for(n=n.child;n!==null;)hl(e,t,n),n=n.sibling}function hl(e,t,n){if(Le&&typeof Le.onCommitFiberUnmount==`function`)try{Le.onCommitFiberUnmount(Ie,n)}catch{}switch(n.tag){case 26:al||Zc(n,t),ml(e,t,n),n.memoizedState?n.memoizedState.count--:n.stateNode&&(n=n.stateNode,n.parentNode.removeChild(n));break;case 27:al||Zc(n,t);var r=fl,i=pl;cf(n.type)&&(fl=n.stateNode,pl=!1),ml(e,t,n),Cf(n.stateNode),fl=r,pl=i;break;case 5:al||Zc(n,t);case 6:if(r=fl,i=pl,fl=null,ml(e,t,n),fl=r,pl=i,fl!==null)if(pl)try{(fl.nodeType===9?fl.body:fl.nodeName===`HTML`?fl.ownerDocument.body:fl).removeChild(n.stateNode)}catch(e){$u(n,t,e)}else try{fl.removeChild(n.stateNode)}catch(e){$u(n,t,e)}break;case 18:fl!==null&&(pl?(e=fl,lf(e.nodeType===9?e.body:e.nodeName===`HTML`?e.ownerDocument.body:e,n.stateNode),Up(e)):lf(fl,n.stateNode));break;case 4:r=fl,i=pl,fl=n.stateNode.containerInfo,pl=!0,ml(e,t,n),fl=r,pl=i;break;case 0:case 11:case 14:case 15:qc(2,n,t),al||qc(4,n,t),ml(e,t,n);break;case 1:al||(Zc(n,t),r=n.stateNode,typeof r.componentWillUnmount==`function`&&Yc(n,t,r)),ml(e,t,n);break;case 21:ml(e,t,n);break;case 22:al=(r=al)||n.memoizedState!==null,ml(e,t,n),al=r;break;default:ml(e,t,n)}}function gl(e,t){if(t.memoizedState===null&&(e=t.alternate,e!==null&&(e=e.memoizedState,e!==null))){e=e.dehydrated;try{Up(e)}catch(e){$u(t,t.return,e)}}}function _l(e,t){if(t.memoizedState===null&&(e=t.alternate,e!==null&&(e=e.memoizedState,e!==null&&(e=e.dehydrated,e!==null))))try{Up(e)}catch(e){$u(t,t.return,e)}}function vl(e){switch(e.tag){case 31:case 13:case 19:var t=e.stateNode;return t===null&&(t=e.stateNode=new sl),t;case 22:return e=e.stateNode,t=e._retryCache,t===null&&(t=e._retryCache=new sl),t;default:throw Error(i(435,e.tag))}}function V(e,t){var n=vl(e);t.forEach(function(t){if(!n.has(t)){n.add(t);var r=id.bind(null,e,t);t.then(r,r)}})}function yl(e,t){var n=t.deletions;if(n!==null)for(var r=0;r<n.length;r++){var a=n[r],o=e,s=t,c=s;a:for(;c!==null;){switch(c.tag){case 27:if(cf(c.type)){fl=c.stateNode,pl=!1;break a}break;case 5:fl=c.stateNode,pl=!1;break a;case 3:case 4:fl=c.stateNode.containerInfo,pl=!0;break a}c=c.return}if(fl===null)throw Error(i(160));hl(o,s,a),fl=null,pl=!1,o=a.alternate,o!==null&&(o.return=null),a.return=null}if(t.subtreeFlags&13886)for(t=t.child;t!==null;)xl(t,e),t=t.sibling}var bl=null;function xl(e,t){var n=e.alternate,r=e.flags;switch(e.tag){case 0:case 11:case 14:case 15:yl(t,e),Sl(e),r&4&&(qc(3,e,e.return),Kc(3,e),qc(5,e,e.return));break;case 1:yl(t,e),Sl(e),r&512&&(al||n===null||Zc(n,n.return)),r&64&&B&&(e=e.updateQueue,e!==null&&(r=e.callbacks,r!==null&&(n=e.shared.hiddenCallbacks,e.shared.hiddenCallbacks=n===null?r:n.concat(r))));break;case 26:var a=bl;if(yl(t,e),Sl(e),r&512&&(al||n===null||Zc(n,n.return)),r&4){var o=n===null?null:n.memoizedState;if(r=e.memoizedState,n===null)if(r===null)if(e.stateNode===null){a:{r=e.type,n=e.memoizedProps,a=a.ownerDocument||a;b:switch(r){case`title`:o=a.getElementsByTagName(`title`)[0],(!o||o[ht]||o[ct]||o.namespaceURI===`http://www.w3.org/2000/svg`||o.hasAttribute(`itemprop`))&&(o=a.createElement(r),a.head.insertBefore(o,a.querySelector(`head > title`))),Gd(o,r,n),o[ct]=e,xt(o),r=o;break a;case`link`:var s=Zf(`link`,`href`,a).get(r+(n.href||``));if(s){for(var c=0;c<s.length;c++)if(o=s[c],o.getAttribute(`href`)===(n.href==null||n.href===``?null:n.href)&&o.getAttribute(`rel`)===(n.rel==null?null:n.rel)&&o.getAttribute(`title`)===(n.title==null?null:n.title)&&o.getAttribute(`crossorigin`)===(n.crossOrigin==null?null:n.crossOrigin)){s.splice(c,1);break b}}o=a.createElement(r),Gd(o,r,n),a.head.appendChild(o);break;case`meta`:if(s=Zf(`meta`,`content`,a).get(r+(n.content||``))){for(c=0;c<s.length;c++)if(o=s[c],o.getAttribute(`content`)===(n.content==null?null:``+n.content)&&o.getAttribute(`name`)===(n.name==null?null:n.name)&&o.getAttribute(`property`)===(n.property==null?null:n.property)&&o.getAttribute(`http-equiv`)===(n.httpEquiv==null?null:n.httpEquiv)&&o.getAttribute(`charset`)===(n.charSet==null?null:n.charSet)){s.splice(c,1);break b}}o=a.createElement(r),Gd(o,r,n),a.head.appendChild(o);break;default:throw Error(i(468,r))}o[ct]=e,xt(o),r=o}e.stateNode=r}else Qf(a,e.type,e.stateNode);else e.stateNode=Kf(a,r,e.memoizedProps);else o===r?r===null&&e.stateNode!==null&&$c(e,e.memoizedProps,n.memoizedProps):(o===null?n.stateNode!==null&&(n=n.stateNode,n.parentNode.removeChild(n)):o.count--,r===null?Qf(a,e.type,e.stateNode):Kf(a,r,e.memoizedProps))}break;case 27:yl(t,e),Sl(e),r&512&&(al||n===null||Zc(n,n.return)),n!==null&&r&4&&$c(e,e.memoizedProps,n.memoizedProps);break;case 5:if(yl(t,e),Sl(e),r&512&&(al||n===null||Zc(n,n.return)),e.flags&32){a=e.stateNode;try{qt(a,``)}catch(t){$u(e,e.return,t)}}r&4&&e.stateNode!=null&&(a=e.memoizedProps,$c(e,a,n===null?a:n.memoizedProps)),r&1024&&(ol=!0);break;case 6:if(yl(t,e),Sl(e),r&4){if(e.stateNode===null)throw Error(i(162));r=e.memoizedProps,n=e.stateNode;try{n.nodeValue=r}catch(t){$u(e,e.return,t)}}break;case 3:if(Xf=null,a=bl,bl=Ef(t.containerInfo),yl(t,e),bl=a,Sl(e),r&4&&n!==null&&n.memoizedState.isDehydrated)try{Up(t.containerInfo)}catch(t){$u(e,e.return,t)}ol&&(ol=!1,Cl(e));break;case 4:r=bl,bl=Ef(e.stateNode.containerInfo),yl(t,e),Sl(e),bl=r;break;case 12:yl(t,e),Sl(e);break;case 31:yl(t,e),Sl(e),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,V(e,r)));break;case 13:yl(t,e),Sl(e),e.child.flags&8192&&e.memoizedState!==null!=(n!==null&&n.memoizedState!==null)&&(su=De()),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,V(e,r)));break;case 22:a=e.memoizedState!==null;var l=n!==null&&n.memoizedState!==null,u=B,d=al;if(B=u||a,al=d||l,yl(t,e),al=d,B=u,Sl(e),r&8192)a:for(t=e.stateNode,t._visibility=a?t._visibility&-2:t._visibility|1,a&&(n===null||l||B||al||Tl(e)),n=null,t=e;;){if(t.tag===5||t.tag===26){if(n===null){l=n=t;try{if(o=l.stateNode,a)s=o.style,typeof s.setProperty==`function`?s.setProperty(`display`,`none`,`important`):s.display=`none`;else{c=l.stateNode;var f=l.memoizedProps.style,p=f!=null&&f.hasOwnProperty(`display`)?f.display:null;c.style.display=p==null||typeof p==`boolean`?``:(``+p).trim()}}catch(e){$u(l,l.return,e)}}}else if(t.tag===6){if(n===null){l=t;try{l.stateNode.nodeValue=a?``:l.memoizedProps}catch(e){$u(l,l.return,e)}}}else if(t.tag===18){if(n===null){l=t;try{var m=l.stateNode;a?uf(m,!0):uf(l.stateNode,!1)}catch(e){$u(l,l.return,e)}}}else if((t.tag!==22&&t.tag!==23||t.memoizedState===null||t===e)&&t.child!==null){t.child.return=t,t=t.child;continue}if(t===e)break a;for(;t.sibling===null;){if(t.return===null||t.return===e)break a;n===t&&(n=null),t=t.return}n===t&&(n=null),t.sibling.return=t.return,t=t.sibling}r&4&&(r=e.updateQueue,r!==null&&(n=r.retryQueue,n!==null&&(r.retryQueue=null,V(e,n))));break;case 19:yl(t,e),Sl(e),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,V(e,r)));break;case 30:break;case 21:break;default:yl(t,e),Sl(e)}}function Sl(e){var t=e.flags;if(t&2){try{for(var n,r=e.return;r!==null;){if(el(r)){n=r;break}r=r.return}if(n==null)throw Error(i(160));switch(n.tag){case 27:var a=n.stateNode;rl(e,tl(e),a);break;case 5:var o=n.stateNode;n.flags&32&&(qt(o,``),n.flags&=-33),rl(e,tl(e),o);break;case 3:case 4:var s=n.stateNode.containerInfo;nl(e,tl(e),s);break;default:throw Error(i(161))}}catch(t){$u(e,e.return,t)}e.flags&=-3}t&4096&&(e.flags&=-4097)}function Cl(e){if(e.subtreeFlags&1024)for(e=e.child;e!==null;){var t=e;Cl(t),t.tag===5&&t.flags&1024&&t.stateNode.reset(),e=e.sibling}}function wl(e,t){if(t.subtreeFlags&8772)for(t=t.child;t!==null;)ul(e,t.alternate,t),t=t.sibling}function Tl(e){for(e=e.child;e!==null;){var t=e;switch(t.tag){case 0:case 11:case 14:case 15:qc(4,t,t.return),Tl(t);break;case 1:Zc(t,t.return);var n=t.stateNode;typeof n.componentWillUnmount==`function`&&Yc(t,t.return,n),Tl(t);break;case 27:Cf(t.stateNode);case 26:case 5:Zc(t,t.return),Tl(t);break;case 22:t.memoizedState===null&&Tl(t);break;case 30:Tl(t);break;default:Tl(t)}e=e.sibling}}function H(e,t,n){for(n&&=(t.subtreeFlags&8772)!=0,t=t.child;t!==null;){var r=t.alternate,i=e,a=t,o=a.flags;switch(a.tag){case 0:case 11:case 15:H(i,a,n),Kc(4,a);break;case 1:if(H(i,a,n),r=a,i=r.stateNode,typeof i.componentDidMount==`function`)try{i.componentDidMount()}catch(e){$u(r,r.return,e)}if(r=a,i=r.updateQueue,i!==null){var s=r.stateNode;try{var c=i.shared.hiddenCallbacks;if(c!==null)for(i.shared.hiddenCallbacks=null,i=0;i<c.length;i++)Qa(c[i],s)}catch(e){$u(r,r.return,e)}}n&&o&64&&Jc(a),Xc(a,a.return);break;case 27:il(a);case 26:case 5:H(i,a,n),n&&r===null&&o&4&&Qc(a),Xc(a,a.return);break;case 12:H(i,a,n);break;case 31:H(i,a,n),n&&o&4&&gl(i,a);break;case 13:H(i,a,n),n&&o&4&&_l(i,a);break;case 22:a.memoizedState===null&&H(i,a,n),Xc(a,a.return);break;case 30:break;default:H(i,a,n)}t=t.sibling}}function El(e,t){var n=null;e!==null&&e.memoizedState!==null&&e.memoizedState.cachePool!==null&&(n=e.memoizedState.cachePool.pool),e=null,t.memoizedState!==null&&t.memoizedState.cachePool!==null&&(e=t.memoizedState.cachePool.pool),e!==n&&(e!=null&&e.refCount++,n!=null&&da(n))}function Dl(e,t){e=null,t.alternate!==null&&(e=t.alternate.memoizedState.cache),t=t.memoizedState.cache,t!==e&&(t.refCount++,e!=null&&da(e))}function Ol(e,t,n,r){if(t.subtreeFlags&10256)for(t=t.child;t!==null;)kl(e,t,n,r),t=t.sibling}function kl(e,t,n,r){var i=t.flags;switch(t.tag){case 0:case 11:case 15:Ol(e,t,n,r),i&2048&&Kc(9,t);break;case 1:Ol(e,t,n,r);break;case 3:Ol(e,t,n,r),i&2048&&(e=null,t.alternate!==null&&(e=t.alternate.memoizedState.cache),t=t.memoizedState.cache,t!==e&&(t.refCount++,e!=null&&da(e)));break;case 12:if(i&2048){Ol(e,t,n,r),e=t.stateNode;try{var a=t.memoizedProps,o=a.id,s=a.onPostCommit;typeof s==`function`&&s(o,t.alternate===null?`mount`:`update`,e.passiveEffectDuration,-0)}catch(e){$u(t,t.return,e)}}else Ol(e,t,n,r);break;case 31:Ol(e,t,n,r);break;case 13:Ol(e,t,n,r);break;case 23:break;case 22:a=t.stateNode,o=t.alternate,t.memoizedState===null?a._visibility&2?Ol(e,t,n,r):(a._visibility|=2,Al(e,t,n,r,(t.subtreeFlags&10256)!=0||!1)):a._visibility&2?Ol(e,t,n,r):jl(e,t),i&2048&&El(o,t);break;case 24:Ol(e,t,n,r),i&2048&&Dl(t.alternate,t);break;default:Ol(e,t,n,r)}}function Al(e,t,n,r,i){for(i&&=(t.subtreeFlags&10256)!=0||!1,t=t.child;t!==null;){var a=e,o=t,s=n,c=r,l=o.flags;switch(o.tag){case 0:case 11:case 15:Al(a,o,s,c,i),Kc(8,o);break;case 23:break;case 22:var u=o.stateNode;o.memoizedState===null?(u._visibility|=2,Al(a,o,s,c,i)):u._visibility&2?Al(a,o,s,c,i):jl(a,o),i&&l&2048&&El(o.alternate,o);break;case 24:Al(a,o,s,c,i),i&&l&2048&&Dl(o.alternate,o);break;default:Al(a,o,s,c,i)}t=t.sibling}}function jl(e,t){if(t.subtreeFlags&10256)for(t=t.child;t!==null;){var n=e,r=t,i=r.flags;switch(r.tag){case 22:jl(n,r),i&2048&&El(r.alternate,r);break;case 24:jl(n,r),i&2048&&Dl(r.alternate,r);break;default:jl(n,r)}t=t.sibling}}var Ml=8192;function Nl(e,t,n){if(e.subtreeFlags&Ml)for(e=e.child;e!==null;)Pl(e,t,n),e=e.sibling}function Pl(e,t,n){switch(e.tag){case 26:Nl(e,t,n),e.flags&Ml&&e.memoizedState!==null&&tp(n,bl,e.memoizedState,e.memoizedProps);break;case 5:Nl(e,t,n);break;case 3:case 4:var r=bl;bl=Ef(e.stateNode.containerInfo),Nl(e,t,n),bl=r;break;case 22:e.memoizedState===null&&(r=e.alternate,r!==null&&r.memoizedState!==null?(r=Ml,Ml=16777216,Nl(e,t,n),Ml=r):Nl(e,t,n));break;default:Nl(e,t,n)}}function Fl(e){var t=e.alternate;if(t!==null&&(e=t.child,e!==null)){t.child=null;do t=e.sibling,e.sibling=null,e=t;while(e!==null)}}function Il(e){var t=e.deletions;if(e.flags&16){if(t!==null)for(var n=0;n<t.length;n++){var r=t[n];cl=r,zl(r,e)}Fl(e)}if(e.subtreeFlags&10256)for(e=e.child;e!==null;)Ll(e),e=e.sibling}function Ll(e){switch(e.tag){case 0:case 11:case 15:Il(e),e.flags&2048&&qc(9,e,e.return);break;case 3:Il(e);break;case 12:Il(e);break;case 22:var t=e.stateNode;e.memoizedState!==null&&t._visibility&2&&(e.return===null||e.return.tag!==13)?(t._visibility&=-3,Rl(e)):Il(e);break;default:Il(e)}}function Rl(e){var t=e.deletions;if(e.flags&16){if(t!==null)for(var n=0;n<t.length;n++){var r=t[n];cl=r,zl(r,e)}Fl(e)}for(e=e.child;e!==null;){switch(t=e,t.tag){case 0:case 11:case 15:qc(8,t,t.return),Rl(t);break;case 22:n=t.stateNode,n._visibility&2&&(n._visibility&=-3,Rl(t));break;default:Rl(t)}e=e.sibling}}function zl(e,t){for(;cl!==null;){var n=cl;switch(n.tag){case 0:case 11:case 15:qc(8,n,t);break;case 23:case 22:if(n.memoizedState!==null&&n.memoizedState.cachePool!==null){var r=n.memoizedState.cachePool.pool;r!=null&&r.refCount++}break;case 24:da(n.memoizedState.cache)}if(r=n.child,r!==null)r.return=n,cl=r;else a:for(n=e;cl!==null;){r=cl;var i=r.sibling,a=r.return;if(dl(r),r===n){cl=null;break a}if(i!==null){i.return=a,cl=i;break a}cl=a}}}var Bl={getCacheForType:function(e){var t=ra(la),n=t.data.get(e);return n===void 0&&(n=e(),t.data.set(e,n)),n},cacheSignal:function(){return ra(la).controller.signal}},Vl=typeof WeakMap==`function`?WeakMap:Map,Hl=0,Ul=null,Wl=null,Gl=0,Kl=0,ql=null,Jl=!1,Yl=!1,Xl=!1,Zl=0,Ql=0,$l=0,eu=0,tu=0,nu=0,ru=0,iu=null,au=null,ou=!1,su=0,cu=0,lu=1/0,uu=null,du=null,fu=0,pu=null,mu=null,hu=0,gu=0,_u=null,vu=null,yu=0,bu=null;function xu(){return Hl&2&&Gl!==0?Gl&-Gl:P.T===null?at():bd()}function Su(){if(nu===0)if(!(Gl&536870912)||Li){var e=We;We<<=1,!(We&3932160)&&(We=262144),nu=e}else nu=536870912;return e=ao.current,e!==null&&(e.flags|=32),nu}function Cu(e,t,n){(e===Ul&&(Kl===2||Kl===9)||e.cancelPendingCommit!==null)&&(Au(e,0),Du(e,Gl,nu,!1)),Qe(e,n),(!(Hl&2)||e!==Ul)&&(e===Ul&&(!(Hl&2)&&(eu|=n),Ql===4&&Du(e,Gl,nu,!1)),fd(e))}function wu(e,t,n){if(Hl&6)throw Error(i(327));var r=!n&&(t&127)==0&&(t&e.expiredLanes)===0||Je(e,t),a=r?Ru(e,t):Iu(e,t,!0),o=r;do{if(a===0){Yl&&!r&&Du(e,t,0,!1);break}else{if(n=e.current.alternate,o&&!Eu(n)){a=Iu(e,t,!1),o=!1;continue}if(a===2){if(o=t,e.errorRecoveryDisabledLanes&o)var s=0;else s=e.pendingLanes&-536870913,s=s===0?s&536870912?536870912:0:s;if(s!==0){t=s;a:{var c=e;a=iu;var l=c.current.memoizedState.isDehydrated;if(l&&(Au(c,s).flags|=256),s=Iu(c,s,!1),s!==2){if(Xl&&!l){c.errorRecoveryDisabledLanes|=o,eu|=o,a=4;break a}o=au,au=a,o!==null&&(au===null?au=o:au.push.apply(au,o))}a=s}if(o=!1,a!==2)continue}}if(a===1){Au(e,0),Du(e,t,0,!0);break}a:{switch(r=e,o=a,o){case 0:case 1:throw Error(i(345));case 4:if((t&4194048)!==t)break;case 6:Du(r,t,nu,!Jl);break a;case 2:au=null;break;case 3:case 5:break;default:throw Error(i(329))}if((t&62914560)===t&&(a=su+300-De(),10<a)){if(Du(r,t,nu,!Jl),qe(r,0,!0)!==0)break a;hu=t,r.timeoutHandle=nf(Tu.bind(null,r,n,au,uu,ou,t,nu,eu,ru,Jl,o,`Throttled`,-0,0),a);break a}Tu(r,n,au,uu,ou,t,nu,eu,ru,Jl,o,null,-0,0)}}break}while(1);fd(e)}function Tu(e,t,n,r,i,a,o,s,c,l,u,d,f,p){if(e.timeoutHandle=-1,d=t.subtreeFlags,d&8192||(d&16785408)==16785408){d={stylesheets:null,count:0,imgCount:0,imgBytes:0,suspenseyImages:[],waitingForImages:!0,waitingForViewTransition:!1,unsuspend:tn},Pl(t,a,d);var m=(a&62914560)===a?su-De():(a&4194048)===a?cu-De():0;if(m=rp(d,m),m!==null){hu=a,e.cancelPendingCommit=m(Gu.bind(null,e,t,a,n,r,i,o,s,c,u,d,null,f,p)),Du(e,a,o,!l);return}}Gu(e,t,a,n,r,i,o,s,c)}function Eu(e){for(var t=e;;){var n=t.tag;if((n===0||n===11||n===15)&&t.flags&16384&&(n=t.updateQueue,n!==null&&(n=n.stores,n!==null)))for(var r=0;r<n.length;r++){var i=n[r],a=i.getSnapshot;i=i.value;try{if(!wr(a(),i))return!1}catch{return!1}}if(n=t.child,t.subtreeFlags&16384&&n!==null)n.return=t,t=n;else{if(t===e)break;for(;t.sibling===null;){if(t.return===null||t.return===e)return!0;t=t.return}t.sibling.return=t.return,t=t.sibling}}return!0}function Du(e,t,n,r){t&=~tu,t&=~eu,e.suspendedLanes|=t,e.pingedLanes&=~t,r&&(e.warmLanes|=t),r=e.expirationTimes;for(var i=t;0<i;){var a=31-ze(i),o=1<<a;r[a]=-1,i&=~o}n!==0&&et(e,n,t)}function Ou(){return Hl&6?!0:(pd(0,!1),!1)}function ku(){if(Wl!==null){if(Kl===0)var e=Wl.return;else e=Wl,Yi=Ji=null,No(e),Pa=null,Fa=0,e=Wl;for(;e!==null;)Gc(e.alternate,e),e=e.return;Wl=null}}function Au(e,t){var n=e.timeoutHandle;n!==-1&&(e.timeoutHandle=-1,rf(n)),n=e.cancelPendingCommit,n!==null&&(e.cancelPendingCommit=null,n()),hu=0,ku(),Ul=e,Wl=n=fi(e.current,null),Gl=t,Kl=0,ql=null,Jl=!1,Yl=Je(e,t),Xl=!1,ru=nu=tu=eu=$l=Ql=0,au=iu=null,ou=!1,t&8&&(t|=t&32);var r=e.entangledLanes;if(r!==0)for(e=e.entanglements,r&=t;0<r;){var i=31-ze(r),a=1<<i;t|=e[i],r&=~a}return Zl=t,ni(),n}function ju(e,t){R=null,P.H=Hs,t===wa||t===Ea?(t=Ma(),Kl=3):t===Ta?(t=Ma(),Kl=4):Kl=t===oc?8:typeof t==`object`&&t&&typeof t.then==`function`?6:1,ql=t,Wl===null&&(Ql=1,ec(e,bi(t,e.current)))}function Mu(){var e=ao.current;return e===null?!0:(Gl&4194048)===Gl?oo===null:(Gl&62914560)===Gl||Gl&536870912?e===oo:!1}function Nu(){var e=P.H;return P.H=Hs,e===null?Hs:e}function Pu(){var e=P.A;return P.A=Bl,e}function Fu(){Ql=4,Jl||(Gl&4194048)!==Gl&&ao.current!==null||(Yl=!0),!($l&134217727)&&!(eu&134217727)||Ul===null||Du(Ul,Gl,nu,!1)}function Iu(e,t,n){var r=Hl;Hl|=2;var i=Nu(),a=Pu();(Ul!==e||Gl!==t)&&(uu=null,Au(e,t)),t=!1;var o=Ql;a:do try{if(Kl!==0&&Wl!==null){var s=Wl,c=ql;switch(Kl){case 8:ku(),o=6;break a;case 3:case 2:case 9:case 6:ao.current===null&&(t=!0);var l=Kl;if(Kl=0,ql=null,Hu(e,s,c,l),n&&Yl){o=0;break a}break;default:l=Kl,Kl=0,ql=null,Hu(e,s,c,l)}}Lu(),o=Ql;break}catch(t){ju(e,t)}while(1);return t&&e.shellSuspendCounter++,Yi=Ji=null,Hl=r,P.H=i,P.A=a,Wl===null&&(Ul=null,Gl=0,ni()),o}function Lu(){for(;Wl!==null;)Bu(Wl)}function Ru(e,t){var n=Hl;Hl|=2;var r=Nu(),a=Pu();Ul!==e||Gl!==t?(uu=null,lu=De()+500,Au(e,t)):Yl=Je(e,t);a:do try{if(Kl!==0&&Wl!==null){t=Wl;var o=ql;b:switch(Kl){case 1:Kl=0,ql=null,Hu(e,t,o,1);break;case 2:case 9:if(Oa(o)){Kl=0,ql=null,Vu(t);break}t=function(){Kl!==2&&Kl!==9||Ul!==e||(Kl=7),fd(e)},o.then(t,t);break a;case 3:Kl=7;break a;case 4:Kl=5;break a;case 7:Oa(o)?(Kl=0,ql=null,Vu(t)):(Kl=0,ql=null,Hu(e,t,o,7));break;case 5:var s=null;switch(Wl.tag){case 26:s=Wl.memoizedState;case 5:case 27:var c=Wl;if(s?ep(s):c.stateNode.complete){Kl=0,ql=null;var l=c.sibling;if(l!==null)Wl=l;else{var u=c.return;u===null?Wl=null:(Wl=u,Uu(u))}break b}}Kl=0,ql=null,Hu(e,t,o,5);break;case 6:Kl=0,ql=null,Hu(e,t,o,6);break;case 8:ku(),Ql=6;break a;default:throw Error(i(462))}}zu();break}catch(t){ju(e,t)}while(1);return Yi=Ji=null,P.H=r,P.A=a,Hl=n,Wl===null?(Ul=null,Gl=0,ni(),Ql):0}function zu(){for(;Wl!==null&&!Te();)Bu(Wl)}function Bu(e){var t=Ic(e.alternate,e,Zl);e.memoizedProps=e.pendingProps,t===null?Uu(e):Wl=t}function Vu(e){var t=e,n=t.alternate;switch(t.tag){case 15:case 0:t=bc(n,t,t.pendingProps,t.type,void 0,Gl);break;case 11:t=bc(n,t,t.pendingProps,t.type.render,t.ref,Gl);break;case 5:No(t);default:Gc(n,t),t=Wl=pi(t,Zl),t=Ic(n,t,Zl)}e.memoizedProps=e.pendingProps,t===null?Uu(e):Wl=t}function Hu(e,t,n,r){Yi=Ji=null,No(t),Pa=null,Fa=0;var i=t.return;try{if(ac(e,i,t,n,Gl)){Ql=1,ec(e,bi(n,e.current)),Wl=null;return}}catch(t){if(i!==null)throw Wl=i,t;Ql=1,ec(e,bi(n,e.current)),Wl=null;return}t.flags&32768?(Li||r===1?e=!0:Yl||Gl&536870912?e=!1:(Jl=e=!0,(r===2||r===9||r===3||r===6)&&(r=ao.current,r!==null&&r.tag===13&&(r.flags|=16384))),Wu(t,e)):Uu(t)}function Uu(e){var t=e;do{if(t.flags&32768){Wu(t,Jl);return}e=t.return;var n=Uc(t.alternate,t,Zl);if(n!==null){Wl=n;return}if(t=t.sibling,t!==null){Wl=t;return}Wl=t=e}while(t!==null);Ql===0&&(Ql=5)}function Wu(e,t){do{var n=Wc(e.alternate,e);if(n!==null){n.flags&=32767,Wl=n;return}if(n=e.return,n!==null&&(n.flags|=32768,n.subtreeFlags=0,n.deletions=null),!t&&(e=e.sibling,e!==null)){Wl=e;return}Wl=e=n}while(e!==null);Ql=6,Wl=null}function Gu(e,t,n,r,a,o,s,c,l){e.cancelPendingCommit=null;do Xu();while(fu!==0);if(Hl&6)throw Error(i(327));if(t!==null){if(t===e.current)throw Error(i(177));if(o=t.lanes|t.childLanes,o|=ti,$e(e,n,o,s,c,l),e===Ul&&(Wl=Ul=null,Gl=0),mu=t,pu=e,hu=n,gu=o,_u=a,vu=r,t.subtreeFlags&10256||t.flags&10256?(e.callbackNode=null,e.callbackPriority=0,ad(je,function(){return Zu(),null})):(e.callbackNode=null,e.callbackPriority=0),r=(t.flags&13878)!=0,t.subtreeFlags&13878||r){r=P.T,P.T=null,a=F.p,F.p=2,s=Hl,Hl|=4;try{ll(e,t,n)}finally{Hl=s,F.p=a,P.T=r}}fu=1,Ku(),qu(),Ju()}}function Ku(){if(fu===1){fu=0;var e=pu,t=mu,n=(t.flags&13878)!=0;if(t.subtreeFlags&13878||n){n=P.T,P.T=null;var r=F.p;F.p=2;var i=Hl;Hl|=4;try{xl(t,e);var a=Xd,o=kr(e.containerInfo),s=a.focusedElem,c=a.selectionRange;if(o!==s&&s&&s.ownerDocument&&Or(s.ownerDocument.documentElement,s)){if(c!==null&&Ar(s)){var l=c.start,u=c.end;if(u===void 0&&(u=l),`selectionStart`in s)s.selectionStart=l,s.selectionEnd=Math.min(u,s.value.length);else{var d=s.ownerDocument||document,f=d&&d.defaultView||window;if(f.getSelection){var p=f.getSelection(),m=s.textContent.length,h=Math.min(c.start,m),g=c.end===void 0?h:Math.min(c.end,m);!p.extend&&h>g&&(o=g,g=h,h=o);var _=Dr(s,h),v=Dr(s,g);if(_&&v&&(p.rangeCount!==1||p.anchorNode!==_.node||p.anchorOffset!==_.offset||p.focusNode!==v.node||p.focusOffset!==v.offset)){var y=d.createRange();y.setStart(_.node,_.offset),p.removeAllRanges(),h>g?(p.addRange(y),p.extend(v.node,v.offset)):(y.setEnd(v.node,v.offset),p.addRange(y))}}}}for(d=[],p=s;p=p.parentNode;)p.nodeType===1&&d.push({element:p,left:p.scrollLeft,top:p.scrollTop});for(typeof s.focus==`function`&&s.focus(),s=0;s<d.length;s++){var b=d[s];b.element.scrollLeft=b.left,b.element.scrollTop=b.top}}_p=!!Yd,Xd=Yd=null}finally{Hl=i,F.p=r,P.T=n}}e.current=t,fu=2}}function qu(){if(fu===2){fu=0;var e=pu,t=mu,n=(t.flags&8772)!=0;if(t.subtreeFlags&8772||n){n=P.T,P.T=null;var r=F.p;F.p=2;var i=Hl;Hl|=4;try{ul(e,t.alternate,t)}finally{Hl=i,F.p=r,P.T=n}}fu=3}}function Ju(){if(fu===4||fu===3){fu=0,Ee();var e=pu,t=mu,n=hu,r=vu;t.subtreeFlags&10256||t.flags&10256?fu=5:(fu=0,mu=pu=null,Yu(e,e.pendingLanes));var i=e.pendingLanes;if(i===0&&(du=null),it(n),t=t.stateNode,Le&&typeof Le.onCommitFiberRoot==`function`)try{Le.onCommitFiberRoot(Ie,t,void 0,(t.current.flags&128)==128)}catch{}if(r!==null){t=P.T,i=F.p,F.p=2,P.T=null;try{for(var a=e.onRecoverableError,o=0;o<r.length;o++){var s=r[o];a(s.value,{componentStack:s.stack})}}finally{P.T=t,F.p=i}}hu&3&&Xu(),fd(e),i=e.pendingLanes,n&261930&&i&42?e===bu?yu++:(yu=0,bu=e):yu=0,pd(0,!1)}}function Yu(e,t){(e.pooledCacheLanes&=t)===0&&(t=e.pooledCache,t!=null&&(e.pooledCache=null,da(t)))}function Xu(){return Ku(),qu(),Ju(),Zu()}function Zu(){if(fu!==5)return!1;var e=pu,t=gu;gu=0;var n=it(hu),r=P.T,a=F.p;try{F.p=32>n?32:n,P.T=null,n=_u,_u=null;var o=pu,s=hu;if(fu=0,mu=pu=null,hu=0,Hl&6)throw Error(i(331));var c=Hl;if(Hl|=4,Ll(o.current),kl(o,o.current,s,n),Hl=c,pd(0,!1),Le&&typeof Le.onPostCommitFiberRoot==`function`)try{Le.onPostCommitFiberRoot(Ie,o)}catch{}return!0}finally{F.p=a,P.T=r,Yu(e,t)}}function Qu(e,t,n){t=bi(n,t),t=nc(e.stateNode,t,2),e=Ka(e,t,2),e!==null&&(Qe(e,2),fd(e))}function $u(e,t,n){if(e.tag===3)Qu(e,e,n);else for(;t!==null;){if(t.tag===3){Qu(t,e,n);break}else if(t.tag===1){var r=t.stateNode;if(typeof t.type.getDerivedStateFromError==`function`||typeof r.componentDidCatch==`function`&&(du===null||!du.has(r))){e=bi(n,e),n=rc(2),r=Ka(t,n,2),r!==null&&(ic(n,r,t,e),Qe(r,2),fd(r));break}}t=t.return}}function ed(e,t,n){var r=e.pingCache;if(r===null){r=e.pingCache=new Vl;var i=new Set;r.set(t,i)}else i=r.get(t),i===void 0&&(i=new Set,r.set(t,i));i.has(n)||(Xl=!0,i.add(n),e=td.bind(null,e,t,n),t.then(e,e))}function td(e,t,n){var r=e.pingCache;r!==null&&r.delete(t),e.pingedLanes|=e.suspendedLanes&n,e.warmLanes&=~n,Ul===e&&(Gl&n)===n&&(Ql===4||Ql===3&&(Gl&62914560)===Gl&&300>De()-su?!(Hl&2)&&Au(e,0):tu|=n,ru===Gl&&(ru=0)),fd(e)}function nd(e,t){t===0&&(t=Xe()),e=ai(e,t),e!==null&&(Qe(e,t),fd(e))}function rd(e){var t=e.memoizedState,n=0;t!==null&&(n=t.retryLane),nd(e,n)}function id(e,t){var n=0;switch(e.tag){case 31:case 13:var r=e.stateNode,a=e.memoizedState;a!==null&&(n=a.retryLane);break;case 19:r=e.stateNode;break;case 22:r=e.stateNode._retryCache;break;default:throw Error(i(314))}r!==null&&r.delete(t),nd(e,n)}function ad(e,t){return Ce(e,t)}var od=null,sd=null,cd=!1,ld=!1,ud=!1,dd=0;function fd(e){e!==sd&&e.next===null&&(sd===null?od=sd=e:sd=sd.next=e),ld=!0,cd||(cd=!0,yd())}function pd(e,t){if(!ud&&ld){ud=!0;do for(var n=!1,r=od;r!==null;){if(!t)if(e!==0){var i=r.pendingLanes;if(i===0)var a=0;else{var o=r.suspendedLanes,s=r.pingedLanes;a=(1<<31-ze(42|e)+1)-1,a&=i&~(o&~s),a=a&201326741?a&201326741|1:a?a|2:0}a!==0&&(n=!0,vd(r,a))}else a=Gl,a=qe(r,r===Ul?a:0,r.cancelPendingCommit!==null||r.timeoutHandle!==-1),!(a&3)||Je(r,a)||(n=!0,vd(r,a));r=r.next}while(n);ud=!1}}function md(){hd()}function hd(){ld=cd=!1;var e=0;dd!==0&&tf()&&(e=dd);for(var t=De(),n=null,r=od;r!==null;){var i=r.next,a=gd(r,t);a===0?(r.next=null,n===null?od=i:n.next=i,i===null&&(sd=n)):(n=r,(e!==0||a&3)&&(ld=!0)),r=i}fu!==0&&fu!==5||pd(e,!1),dd!==0&&(dd=0)}function gd(e,t){for(var n=e.suspendedLanes,r=e.pingedLanes,i=e.expirationTimes,a=e.pendingLanes&-62914561;0<a;){var o=31-ze(a),s=1<<o,c=i[o];c===-1?((s&n)===0||(s&r)!==0)&&(i[o]=Ye(s,t)):c<=t&&(e.expiredLanes|=s),a&=~s}if(t=Ul,n=Gl,n=qe(e,e===t?n:0,e.cancelPendingCommit!==null||e.timeoutHandle!==-1),r=e.callbackNode,n===0||e===t&&(Kl===2||Kl===9)||e.cancelPendingCommit!==null)return r!==null&&r!==null&&we(r),e.callbackNode=null,e.callbackPriority=0;if(!(n&3)||Je(e,n)){if(t=n&-n,t===e.callbackPriority)return t;switch(r!==null&&we(r),it(n)){case 2:case 8:n=Ae;break;case 32:n=je;break;case 268435456:n=Ne;break;default:n=je}return r=_d.bind(null,e),n=Ce(n,r),e.callbackPriority=t,e.callbackNode=n,t}return r!==null&&r!==null&&we(r),e.callbackPriority=2,e.callbackNode=null,2}function _d(e,t){if(fu!==0&&fu!==5)return e.callbackNode=null,e.callbackPriority=0,null;var n=e.callbackNode;if(Xu()&&e.callbackNode!==n)return null;var r=Gl;return r=qe(e,e===Ul?r:0,e.cancelPendingCommit!==null||e.timeoutHandle!==-1),r===0?null:(wu(e,r,t),gd(e,De()),e.callbackNode!=null&&e.callbackNode===n?_d.bind(null,e):null)}function vd(e,t){if(Xu())return null;wu(e,t,!0)}function yd(){of(function(){Hl&6?Ce(ke,md):hd()})}function bd(){if(dd===0){var e=ma;e===0&&(e=Ue,Ue<<=1,!(Ue&261888)&&(Ue=256)),dd=e}return dd}function xd(e){return e==null||typeof e==`symbol`||typeof e==`boolean`?null:typeof e==`function`?e:en(``+e)}function Sd(e,t){var n=t.ownerDocument.createElement(`input`);return n.name=t.name,n.value=t.value,e.id&&n.setAttribute(`form`,e.id),t.parentNode.insertBefore(n,t),e=new FormData(e),n.parentNode.removeChild(n),e}function Cd(e,t,n,r,i){if(t===`submit`&&n&&n.stateNode===i){var a=xd((i[lt]||null).action),o=r.submitter;o&&(t=(t=o[lt]||null)?xd(t.formAction):o.getAttribute(`formAction`),t!==null&&(a=t,o=null));var s=new Cn(`action`,`action`,null,r,i);e.push({event:s,listeners:[{instance:null,listener:function(){if(r.defaultPrevented){if(dd!==0){var e=o?Sd(i,o):new FormData(i);Os(n,{pending:!0,data:e,method:i.method,action:a},null,e)}}else typeof a==`function`&&(s.preventDefault(),e=o?Sd(i,o):new FormData(i),Os(n,{pending:!0,data:e,method:i.method,action:a},a,e))},currentTarget:i}]})}}for(var wd=0;wd<Xr.length;wd++){var Td=Xr[wd];Zr(Td.toLowerCase(),`on`+(Td[0].toUpperCase()+Td.slice(1)))}Zr(Hr,`onAnimationEnd`),Zr(Ur,`onAnimationIteration`),Zr(Wr,`onAnimationStart`),Zr(`dblclick`,`onDoubleClick`),Zr(`focusin`,`onFocus`),Zr(`focusout`,`onBlur`),Zr(Gr,`onTransitionRun`),Zr(Kr,`onTransitionStart`),Zr(qr,`onTransitionCancel`),Zr(Jr,`onTransitionEnd`),Tt(`onMouseEnter`,[`mouseout`,`mouseover`]),Tt(`onMouseLeave`,[`mouseout`,`mouseover`]),Tt(`onPointerEnter`,[`pointerout`,`pointerover`]),Tt(`onPointerLeave`,[`pointerout`,`pointerover`]),wt(`onChange`,`change click focusin focusout input keydown keyup selectionchange`.split(` `)),wt(`onSelect`,`focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange`.split(` `)),wt(`onBeforeInput`,[`compositionend`,`keypress`,`textInput`,`paste`]),wt(`onCompositionEnd`,`compositionend focusout keydown keypress keyup mousedown`.split(` `)),wt(`onCompositionStart`,`compositionstart focusout keydown keypress keyup mousedown`.split(` `)),wt(`onCompositionUpdate`,`compositionupdate focusout keydown keypress keyup mousedown`.split(` `));var Ed=`abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting`.split(` `),Dd=new Set(`beforetoggle cancel close invalid load scroll scrollend toggle`.split(` `).concat(Ed));function Od(e,t){t=(t&4)!=0;for(var n=0;n<e.length;n++){var r=e[n],i=r.event;r=r.listeners;a:{var a=void 0;if(t)for(var o=r.length-1;0<=o;o--){var s=r[o],c=s.instance,l=s.currentTarget;if(s=s.listener,c!==a&&i.isPropagationStopped())break a;a=s,i.currentTarget=l;try{a(i)}catch(e){Qr(e)}i.currentTarget=null,a=c}else for(o=0;o<r.length;o++){if(s=r[o],c=s.instance,l=s.currentTarget,s=s.listener,c!==a&&i.isPropagationStopped())break a;a=s,i.currentTarget=l;try{a(i)}catch(e){Qr(e)}i.currentTarget=null,a=c}}}}function kd(e,t){var n=t[dt];n===void 0&&(n=t[dt]=new Set);var r=e+`__bubble`;n.has(r)||(Nd(t,e,2,!1),n.add(r))}function Ad(e,t,n){var r=0;t&&(r|=4),Nd(n,e,r,t)}var jd=`_reactListening`+Math.random().toString(36).slice(2);function Md(e){if(!e[jd]){e[jd]=!0,St.forEach(function(t){t!==`selectionchange`&&(Dd.has(t)||Ad(t,!1,e),Ad(t,!0,e))});var t=e.nodeType===9?e:e.ownerDocument;t===null||t[jd]||(t[jd]=!0,Ad(`selectionchange`,!1,t))}}function Nd(e,t,n,r){switch(wp(t)){case 2:var i=vp;break;case 8:i=yp;break;default:i=bp}n=i.bind(null,t,n,e),i=void 0,!fn||t!==`touchstart`&&t!==`touchmove`&&t!==`wheel`||(i=!0),r?i===void 0?e.addEventListener(t,n,!0):e.addEventListener(t,n,{capture:!0,passive:i}):i===void 0?e.addEventListener(t,n,!1):e.addEventListener(t,n,{passive:i})}function Pd(e,t,n,r,i){var a=r;if(!(t&1)&&!(t&2)&&r!==null)a:for(;;){if(r===null)return;var s=r.tag;if(s===3||s===4){var c=r.stateNode.containerInfo;if(c===i)break;if(s===4)for(s=r.return;s!==null;){var l=s.tag;if((l===3||l===4)&&s.stateNode.containerInfo===i)return;s=s.return}for(;c!==null;){if(s=_t(c),s===null)return;if(l=s.tag,l===5||l===6||l===26||l===27){r=a=s;continue a}c=c.parentNode}}r=r.return}ln(function(){var r=a,i=rn(n),s=[];a:{var c=Yr.get(e);if(c!==void 0){var l=Cn,u=e;switch(e){case`keypress`:if(vn(n)===0)break a;case`keydown`:case`keyup`:l=Vn;break;case`focusin`:u=`focus`,l=Mn;break;case`focusout`:u=`blur`,l=Mn;break;case`beforeblur`:case`afterblur`:l=Mn;break;case`click`:if(n.button===2)break a;case`auxclick`:case`dblclick`:case`mousedown`:case`mousemove`:case`mouseup`:case`mouseout`:case`mouseover`:case`contextmenu`:l=An;break;case`drag`:case`dragend`:case`dragenter`:case`dragexit`:case`dragleave`:case`dragover`:case`dragstart`:case`drop`:l=jn;break;case`touchcancel`:case`touchend`:case`touchmove`:case`touchstart`:l=Un;break;case Hr:case Ur:case Wr:l=Nn;break;case Jr:l=Wn;break;case`scroll`:case`scrollend`:l=Tn;break;case`wheel`:l=Gn;break;case`copy`:case`cut`:case`paste`:l=Pn;break;case`gotpointercapture`:case`lostpointercapture`:case`pointercancel`:case`pointerdown`:case`pointermove`:case`pointerout`:case`pointerover`:case`pointerup`:l=Hn;break;case`toggle`:case`beforetoggle`:l=Kn}var d=(t&4)!=0,f=!d&&(e===`scroll`||e===`scrollend`),p=d?c===null?null:c+`Capture`:c;d=[];for(var m=r,h;m!==null;){var g=m;if(h=g.stateNode,g=g.tag,g!==5&&g!==26&&g!==27||h===null||p===null||(g=un(m,p),g!=null&&d.push(Fd(m,g,h))),f)break;m=m.return}0<d.length&&(c=new l(c,u,null,n,i),s.push({event:c,listeners:d}))}}if(!(t&7)){a:{if(c=e===`mouseover`||e===`pointerover`,l=e===`mouseout`||e===`pointerout`,c&&n!==nn&&(u=n.relatedTarget||n.fromElement)&&(_t(u)||u[ut]))break a;if((l||c)&&(c=i.window===i?i:(c=i.ownerDocument)?c.defaultView||c.parentWindow:window,l?(u=n.relatedTarget||n.toElement,l=r,u=u?_t(u):null,u!==null&&(f=o(u),d=u.tag,u!==f||d!==5&&d!==27&&d!==6)&&(u=null)):(l=null,u=r),l!==u)){if(d=An,g=`onMouseLeave`,p=`onMouseEnter`,m=`mouse`,(e===`pointerout`||e===`pointerover`)&&(d=Hn,g=`onPointerLeave`,p=`onPointerEnter`,m=`pointer`),f=l==null?c:yt(l),h=u==null?c:yt(u),c=new d(g,m+`leave`,l,n,i),c.target=f,c.relatedTarget=h,g=null,_t(i)===r&&(d=new d(p,m+`enter`,u,n,i),d.target=h,d.relatedTarget=f,g=d),f=g,l&&u)b:{for(d=Ld,p=l,m=u,h=0,g=p;g;g=d(g))h++;g=0;for(var _=m;_;_=d(_))g++;for(;0<h-g;)p=d(p),h--;for(;0<g-h;)m=d(m),g--;for(;h--;){if(p===m||m!==null&&p===m.alternate){d=p;break b}p=d(p),m=d(m)}d=null}else d=null;l!==null&&Rd(s,c,l,d,!1),u!==null&&f!==null&&Rd(s,f,u,d,!0)}}a:{if(c=r?yt(r):window,l=c.nodeName&&c.nodeName.toLowerCase(),l===`select`||l===`input`&&c.type===`file`)var v=fr;else if(or(c))if(pr)v=Sr;else{v=br;var y=yr}else l=c.nodeName,!l||l.toLowerCase()!==`input`||c.type!==`checkbox`&&c.type!==`radio`?r&&Zt(r.elementType)&&(v=fr):v=xr;if(v&&=v(e,r)){sr(s,v,n,i);break a}y&&y(e,c,r),e===`focusout`&&r&&c.type===`number`&&r.memoizedProps.value!=null&&Ut(c,`number`,c.value)}switch(y=r?yt(r):window,e){case`focusin`:(or(y)||y.contentEditable===`true`)&&(Mr=y,Nr=r,Pr=null);break;case`focusout`:Pr=Nr=Mr=null;break;case`mousedown`:Fr=!0;break;case`contextmenu`:case`mouseup`:case`dragend`:Fr=!1,Ir(s,n,i);break;case`selectionchange`:if(jr)break;case`keydown`:case`keyup`:Ir(s,n,i)}var b;if(Jn)b:{switch(e){case`compositionstart`:var x=`onCompositionStart`;break b;case`compositionend`:x=`onCompositionEnd`;break b;case`compositionupdate`:x=`onCompositionUpdate`;break b}x=void 0}else nr?er(e,n)&&(x=`onCompositionEnd`):e===`keydown`&&n.keyCode===229&&(x=`onCompositionStart`);x&&(Zn&&n.locale!==`ko`&&(nr||x!==`onCompositionStart`?x===`onCompositionEnd`&&nr&&(b=_n()):(mn=i,hn=`value`in mn?mn.value:mn.textContent,nr=!0)),y=Id(r,x),0<y.length&&(x=new Fn(x,e,null,n,i),s.push({event:x,listeners:y}),b?x.data=b:(b=tr(n),b!==null&&(x.data=b)))),(b=Xn?rr(e,n):ir(e,n))&&(x=Id(r,`onBeforeInput`),0<x.length&&(y=new Fn(`onBeforeInput`,`beforeinput`,null,n,i),s.push({event:y,listeners:x}),y.data=b)),Cd(s,e,r,n,i)}Od(s,t)})}function Fd(e,t,n){return{instance:e,listener:t,currentTarget:n}}function Id(e,t){for(var n=t+`Capture`,r=[];e!==null;){var i=e,a=i.stateNode;if(i=i.tag,i!==5&&i!==26&&i!==27||a===null||(i=un(e,n),i!=null&&r.unshift(Fd(e,i,a)),i=un(e,t),i!=null&&r.push(Fd(e,i,a))),e.tag===3)return r;e=e.return}return[]}function Ld(e){if(e===null)return null;do e=e.return;while(e&&e.tag!==5&&e.tag!==27);return e||null}function Rd(e,t,n,r,i){for(var a=t._reactName,o=[];n!==null&&n!==r;){var s=n,c=s.alternate,l=s.stateNode;if(s=s.tag,c!==null&&c===r)break;s!==5&&s!==26&&s!==27||l===null||(c=l,i?(l=un(n,a),l!=null&&o.unshift(Fd(n,l,c))):i||(l=un(n,a),l!=null&&o.push(Fd(n,l,c)))),n=n.return}o.length!==0&&e.push({event:t,listeners:o})}var zd=/\r\n?/g,Bd=/\u0000|\uFFFD/g;function Vd(e){return(typeof e==`string`?e:``+e).replace(zd,`
`).replace(Bd,``)}function Hd(e,t){return t=Vd(t),Vd(e)===t}function Ud(e,t,n,r,a,o){switch(n){case`children`:typeof r==`string`?t===`body`||t===`textarea`&&r===``||qt(e,r):(typeof r==`number`||typeof r==`bigint`)&&t!==`body`&&qt(e,``+r);break;case`className`:jt(e,`class`,r);break;case`tabIndex`:jt(e,`tabindex`,r);break;case`dir`:case`role`:case`viewBox`:case`width`:case`height`:jt(e,n,r);break;case`style`:Xt(e,r,o);break;case`data`:if(t!==`object`){jt(e,`data`,r);break}case`src`:case`href`:if(r===``&&(t!==`a`||n!==`href`)){e.removeAttribute(n);break}if(r==null||typeof r==`function`||typeof r==`symbol`||typeof r==`boolean`){e.removeAttribute(n);break}r=en(``+r),e.setAttribute(n,r);break;case`action`:case`formAction`:if(typeof r==`function`){e.setAttribute(n,`javascript:throw new Error('A React form was unexpectedly submitted. If you called form.submit() manually, consider using form.requestSubmit() instead. If you\\'re trying to use event.stopPropagation() in a submit event handler, consider also calling event.preventDefault().')`);break}else typeof o==`function`&&(n===`formAction`?(t!==`input`&&Ud(e,t,`name`,a.name,a,null),Ud(e,t,`formEncType`,a.formEncType,a,null),Ud(e,t,`formMethod`,a.formMethod,a,null),Ud(e,t,`formTarget`,a.formTarget,a,null)):(Ud(e,t,`encType`,a.encType,a,null),Ud(e,t,`method`,a.method,a,null),Ud(e,t,`target`,a.target,a,null)));if(r==null||typeof r==`symbol`||typeof r==`boolean`){e.removeAttribute(n);break}r=en(``+r),e.setAttribute(n,r);break;case`onClick`:r!=null&&(e.onclick=tn);break;case`onScroll`:r!=null&&kd(`scroll`,e);break;case`onScrollEnd`:r!=null&&kd(`scrollend`,e);break;case`dangerouslySetInnerHTML`:if(r!=null){if(typeof r!=`object`||!(`__html`in r))throw Error(i(61));if(n=r.__html,n!=null){if(a.children!=null)throw Error(i(60));e.innerHTML=n}}break;case`multiple`:e.multiple=r&&typeof r!=`function`&&typeof r!=`symbol`;break;case`muted`:e.muted=r&&typeof r!=`function`&&typeof r!=`symbol`;break;case`suppressContentEditableWarning`:case`suppressHydrationWarning`:case`defaultValue`:case`defaultChecked`:case`innerHTML`:case`ref`:break;case`autoFocus`:break;case`xlinkHref`:if(r==null||typeof r==`function`||typeof r==`boolean`||typeof r==`symbol`){e.removeAttribute(`xlink:href`);break}n=en(``+r),e.setAttributeNS(`http://www.w3.org/1999/xlink`,`xlink:href`,n);break;case`contentEditable`:case`spellCheck`:case`draggable`:case`value`:case`autoReverse`:case`externalResourcesRequired`:case`focusable`:case`preserveAlpha`:r!=null&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,``+r):e.removeAttribute(n);break;case`inert`:case`allowFullScreen`:case`async`:case`autoPlay`:case`controls`:case`default`:case`defer`:case`disabled`:case`disablePictureInPicture`:case`disableRemotePlayback`:case`formNoValidate`:case`hidden`:case`loop`:case`noModule`:case`noValidate`:case`open`:case`playsInline`:case`readOnly`:case`required`:case`reversed`:case`scoped`:case`seamless`:case`itemScope`:r&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,``):e.removeAttribute(n);break;case`capture`:case`download`:!0===r?e.setAttribute(n,``):!1!==r&&r!=null&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,r):e.removeAttribute(n);break;case`cols`:case`rows`:case`size`:case`span`:r!=null&&typeof r!=`function`&&typeof r!=`symbol`&&!isNaN(r)&&1<=r?e.setAttribute(n,r):e.removeAttribute(n);break;case`rowSpan`:case`start`:r==null||typeof r==`function`||typeof r==`symbol`||isNaN(r)?e.removeAttribute(n):e.setAttribute(n,r);break;case`popover`:kd(`beforetoggle`,e),kd(`toggle`,e),At(e,`popover`,r);break;case`xlinkActuate`:Mt(e,`http://www.w3.org/1999/xlink`,`xlink:actuate`,r);break;case`xlinkArcrole`:Mt(e,`http://www.w3.org/1999/xlink`,`xlink:arcrole`,r);break;case`xlinkRole`:Mt(e,`http://www.w3.org/1999/xlink`,`xlink:role`,r);break;case`xlinkShow`:Mt(e,`http://www.w3.org/1999/xlink`,`xlink:show`,r);break;case`xlinkTitle`:Mt(e,`http://www.w3.org/1999/xlink`,`xlink:title`,r);break;case`xlinkType`:Mt(e,`http://www.w3.org/1999/xlink`,`xlink:type`,r);break;case`xmlBase`:Mt(e,`http://www.w3.org/XML/1998/namespace`,`xml:base`,r);break;case`xmlLang`:Mt(e,`http://www.w3.org/XML/1998/namespace`,`xml:lang`,r);break;case`xmlSpace`:Mt(e,`http://www.w3.org/XML/1998/namespace`,`xml:space`,r);break;case`is`:At(e,`is`,r);break;case`innerText`:case`textContent`:break;default:(!(2<n.length)||n[0]!==`o`&&n[0]!==`O`||n[1]!==`n`&&n[1]!==`N`)&&(n=Qt.get(n)||n,At(e,n,r))}}function Wd(e,t,n,r,a,o){switch(n){case`style`:Xt(e,r,o);break;case`dangerouslySetInnerHTML`:if(r!=null){if(typeof r!=`object`||!(`__html`in r))throw Error(i(61));if(n=r.__html,n!=null){if(a.children!=null)throw Error(i(60));e.innerHTML=n}}break;case`children`:typeof r==`string`?qt(e,r):(typeof r==`number`||typeof r==`bigint`)&&qt(e,``+r);break;case`onScroll`:r!=null&&kd(`scroll`,e);break;case`onScrollEnd`:r!=null&&kd(`scrollend`,e);break;case`onClick`:r!=null&&(e.onclick=tn);break;case`suppressContentEditableWarning`:case`suppressHydrationWarning`:case`innerHTML`:case`ref`:break;case`innerText`:case`textContent`:break;default:if(!Ct.hasOwnProperty(n))a:{if(n[0]===`o`&&n[1]===`n`&&(a=n.endsWith(`Capture`),t=n.slice(2,a?n.length-7:void 0),o=e[lt]||null,o=o==null?null:o[n],typeof o==`function`&&e.removeEventListener(t,o,a),typeof r==`function`)){typeof o!=`function`&&o!==null&&(n in e?e[n]=null:e.hasAttribute(n)&&e.removeAttribute(n)),e.addEventListener(t,r,a);break a}n in e?e[n]=r:!0===r?e.setAttribute(n,``):At(e,n,r)}}}function Gd(e,t,n){switch(t){case`div`:case`span`:case`svg`:case`path`:case`a`:case`g`:case`p`:case`li`:break;case`img`:kd(`error`,e),kd(`load`,e);var r=!1,a=!1,o;for(o in n)if(n.hasOwnProperty(o)){var s=n[o];if(s!=null)switch(o){case`src`:r=!0;break;case`srcSet`:a=!0;break;case`children`:case`dangerouslySetInnerHTML`:throw Error(i(137,t));default:Ud(e,t,o,s,n,null)}}a&&Ud(e,t,`srcSet`,n.srcSet,n,null),r&&Ud(e,t,`src`,n.src,n,null);return;case`input`:kd(`invalid`,e);var c=o=s=a=null,l=null,u=null;for(r in n)if(n.hasOwnProperty(r)){var d=n[r];if(d!=null)switch(r){case`name`:a=d;break;case`type`:s=d;break;case`checked`:l=d;break;case`defaultChecked`:u=d;break;case`value`:o=d;break;case`defaultValue`:c=d;break;case`children`:case`dangerouslySetInnerHTML`:if(d!=null)throw Error(i(137,t));break;default:Ud(e,t,r,d,n,null)}}Ht(e,o,c,l,u,s,a,!1);return;case`select`:for(a in kd(`invalid`,e),r=s=o=null,n)if(n.hasOwnProperty(a)&&(c=n[a],c!=null))switch(a){case`value`:o=c;break;case`defaultValue`:s=c;break;case`multiple`:r=c;default:Ud(e,t,a,c,n,null)}t=o,n=s,e.multiple=!!r,t==null?n!=null&&Wt(e,!!r,n,!0):Wt(e,!!r,t,!1);return;case`textarea`:for(s in kd(`invalid`,e),o=a=r=null,n)if(n.hasOwnProperty(s)&&(c=n[s],c!=null))switch(s){case`value`:r=c;break;case`defaultValue`:a=c;break;case`children`:o=c;break;case`dangerouslySetInnerHTML`:if(c!=null)throw Error(i(91));break;default:Ud(e,t,s,c,n,null)}Kt(e,r,a,o);return;case`option`:for(l in n)if(n.hasOwnProperty(l)&&(r=n[l],r!=null))switch(l){case`selected`:e.selected=r&&typeof r!=`function`&&typeof r!=`symbol`;break;default:Ud(e,t,l,r,n,null)}return;case`dialog`:kd(`beforetoggle`,e),kd(`toggle`,e),kd(`cancel`,e),kd(`close`,e);break;case`iframe`:case`object`:kd(`load`,e);break;case`video`:case`audio`:for(r=0;r<Ed.length;r++)kd(Ed[r],e);break;case`image`:kd(`error`,e),kd(`load`,e);break;case`details`:kd(`toggle`,e);break;case`embed`:case`source`:case`link`:kd(`error`,e),kd(`load`,e);case`area`:case`base`:case`br`:case`col`:case`hr`:case`keygen`:case`meta`:case`param`:case`track`:case`wbr`:case`menuitem`:for(u in n)if(n.hasOwnProperty(u)&&(r=n[u],r!=null))switch(u){case`children`:case`dangerouslySetInnerHTML`:throw Error(i(137,t));default:Ud(e,t,u,r,n,null)}return;default:if(Zt(t)){for(d in n)n.hasOwnProperty(d)&&(r=n[d],r!==void 0&&Wd(e,t,d,r,n,void 0));return}}for(c in n)n.hasOwnProperty(c)&&(r=n[c],r!=null&&Ud(e,t,c,r,n,null))}function Kd(e,t,n,r){switch(t){case`div`:case`span`:case`svg`:case`path`:case`a`:case`g`:case`p`:case`li`:break;case`input`:var a=null,o=null,s=null,c=null,l=null,u=null,d=null;for(m in n){var f=n[m];if(n.hasOwnProperty(m)&&f!=null)switch(m){case`checked`:break;case`value`:break;case`defaultValue`:l=f;default:r.hasOwnProperty(m)||Ud(e,t,m,null,r,f)}}for(var p in r){var m=r[p];if(f=n[p],r.hasOwnProperty(p)&&(m!=null||f!=null))switch(p){case`type`:o=m;break;case`name`:a=m;break;case`checked`:u=m;break;case`defaultChecked`:d=m;break;case`value`:s=m;break;case`defaultValue`:c=m;break;case`children`:case`dangerouslySetInnerHTML`:if(m!=null)throw Error(i(137,t));break;default:m!==f&&Ud(e,t,p,m,r,f)}}Vt(e,s,c,l,u,d,o,a);return;case`select`:for(o in m=s=c=p=null,n)if(l=n[o],n.hasOwnProperty(o)&&l!=null)switch(o){case`value`:break;case`multiple`:m=l;default:r.hasOwnProperty(o)||Ud(e,t,o,null,r,l)}for(a in r)if(o=r[a],l=n[a],r.hasOwnProperty(a)&&(o!=null||l!=null))switch(a){case`value`:p=o;break;case`defaultValue`:c=o;break;case`multiple`:s=o;default:o!==l&&Ud(e,t,a,o,r,l)}t=c,n=s,r=m,p==null?!!r!=!!n&&(t==null?Wt(e,!!n,n?[]:``,!1):Wt(e,!!n,t,!0)):Wt(e,!!n,p,!1);return;case`textarea`:for(c in m=p=null,n)if(a=n[c],n.hasOwnProperty(c)&&a!=null&&!r.hasOwnProperty(c))switch(c){case`value`:break;case`children`:break;default:Ud(e,t,c,null,r,a)}for(s in r)if(a=r[s],o=n[s],r.hasOwnProperty(s)&&(a!=null||o!=null))switch(s){case`value`:p=a;break;case`defaultValue`:m=a;break;case`children`:break;case`dangerouslySetInnerHTML`:if(a!=null)throw Error(i(91));break;default:a!==o&&Ud(e,t,s,a,r,o)}Gt(e,p,m);return;case`option`:for(var h in n)if(p=n[h],n.hasOwnProperty(h)&&p!=null&&!r.hasOwnProperty(h))switch(h){case`selected`:e.selected=!1;break;default:Ud(e,t,h,null,r,p)}for(l in r)if(p=r[l],m=n[l],r.hasOwnProperty(l)&&p!==m&&(p!=null||m!=null))switch(l){case`selected`:e.selected=p&&typeof p!=`function`&&typeof p!=`symbol`;break;default:Ud(e,t,l,p,r,m)}return;case`img`:case`link`:case`area`:case`base`:case`br`:case`col`:case`embed`:case`hr`:case`keygen`:case`meta`:case`param`:case`source`:case`track`:case`wbr`:case`menuitem`:for(var g in n)p=n[g],n.hasOwnProperty(g)&&p!=null&&!r.hasOwnProperty(g)&&Ud(e,t,g,null,r,p);for(u in r)if(p=r[u],m=n[u],r.hasOwnProperty(u)&&p!==m&&(p!=null||m!=null))switch(u){case`children`:case`dangerouslySetInnerHTML`:if(p!=null)throw Error(i(137,t));break;default:Ud(e,t,u,p,r,m)}return;default:if(Zt(t)){for(var _ in n)p=n[_],n.hasOwnProperty(_)&&p!==void 0&&!r.hasOwnProperty(_)&&Wd(e,t,_,void 0,r,p);for(d in r)p=r[d],m=n[d],!r.hasOwnProperty(d)||p===m||p===void 0&&m===void 0||Wd(e,t,d,p,r,m);return}}for(var v in n)p=n[v],n.hasOwnProperty(v)&&p!=null&&!r.hasOwnProperty(v)&&Ud(e,t,v,null,r,p);for(f in r)p=r[f],m=n[f],!r.hasOwnProperty(f)||p===m||p==null&&m==null||Ud(e,t,f,p,r,m)}function qd(e){switch(e){case`css`:case`script`:case`font`:case`img`:case`image`:case`input`:case`link`:return!0;default:return!1}}function Jd(){if(typeof performance.getEntriesByType==`function`){for(var e=0,t=0,n=performance.getEntriesByType(`resource`),r=0;r<n.length;r++){var i=n[r],a=i.transferSize,o=i.initiatorType,s=i.duration;if(a&&s&&qd(o)){for(o=0,s=i.responseEnd,r+=1;r<n.length;r++){var c=n[r],l=c.startTime;if(l>s)break;var u=c.transferSize,d=c.initiatorType;u&&qd(d)&&(c=c.responseEnd,o+=u*(c<s?1:(s-l)/(c-l)))}if(--r,t+=8*(a+o)/(i.duration/1e3),e++,10<e)break}}if(0<e)return t/e/1e6}return navigator.connection&&(e=navigator.connection.downlink,typeof e==`number`)?e:5}var Yd=null,Xd=null;function U(e){return e.nodeType===9?e:e.ownerDocument}function Zd(e){switch(e){case`http://www.w3.org/2000/svg`:return 1;case`http://www.w3.org/1998/Math/MathML`:return 2;default:return 0}}function Qd(e,t){if(e===0)switch(t){case`svg`:return 1;case`math`:return 2;default:return 0}return e===1&&t===`foreignObject`?0:e}function $d(e,t){return e===`textarea`||e===`noscript`||typeof t.children==`string`||typeof t.children==`number`||typeof t.children==`bigint`||typeof t.dangerouslySetInnerHTML==`object`&&t.dangerouslySetInnerHTML!==null&&t.dangerouslySetInnerHTML.__html!=null}var ef=null;function tf(){var e=window.event;return e&&e.type===`popstate`?e===ef?!1:(ef=e,!0):(ef=null,!1)}var nf=typeof setTimeout==`function`?setTimeout:void 0,rf=typeof clearTimeout==`function`?clearTimeout:void 0,af=typeof Promise==`function`?Promise:void 0,of=typeof queueMicrotask==`function`?queueMicrotask:af===void 0?nf:function(e){return af.resolve(null).then(e).catch(sf)};function sf(e){setTimeout(function(){throw e})}function cf(e){return e===`head`}function lf(e,t){var n=t,r=0;do{var i=n.nextSibling;if(e.removeChild(n),i&&i.nodeType===8)if(n=i.data,n===`/$`||n===`/&`){if(r===0){e.removeChild(i),Up(t);return}r--}else if(n===`$`||n===`$?`||n===`$~`||n===`$!`||n===`&`)r++;else if(n===`html`)Cf(e.ownerDocument.documentElement);else if(n===`head`){n=e.ownerDocument.head,Cf(n);for(var a=n.firstChild;a;){var o=a.nextSibling,s=a.nodeName;a[ht]||s===`SCRIPT`||s===`STYLE`||s===`LINK`&&a.rel.toLowerCase()===`stylesheet`||n.removeChild(a),a=o}}else n===`body`&&Cf(e.ownerDocument.body);n=i}while(n);Up(t)}function uf(e,t){var n=e;e=0;do{var r=n.nextSibling;if(n.nodeType===1?t?(n._stashedDisplay=n.style.display,n.style.display=`none`):(n.style.display=n._stashedDisplay||``,n.getAttribute(`style`)===``&&n.removeAttribute(`style`)):n.nodeType===3&&(t?(n._stashedText=n.nodeValue,n.nodeValue=``):n.nodeValue=n._stashedText||``),r&&r.nodeType===8)if(n=r.data,n===`/$`){if(e===0)break;e--}else n!==`$`&&n!==`$?`&&n!==`$~`&&n!==`$!`||e++;n=r}while(n)}function df(e){var t=e.firstChild;for(t&&t.nodeType===10&&(t=t.nextSibling);t;){var n=t;switch(t=t.nextSibling,n.nodeName){case`HTML`:case`HEAD`:case`BODY`:df(n),gt(n);continue;case`SCRIPT`:case`STYLE`:continue;case`LINK`:if(n.rel.toLowerCase()===`stylesheet`)continue}e.removeChild(n)}}function ff(e,t,n,r){for(;e.nodeType===1;){var i=n;if(e.nodeName.toLowerCase()!==t.toLowerCase()){if(!r&&(e.nodeName!==`INPUT`||e.type!==`hidden`))break}else if(r){if(!e[ht])switch(t){case`meta`:if(!e.hasAttribute(`itemprop`))break;return e;case`link`:if(a=e.getAttribute(`rel`),a===`stylesheet`&&e.hasAttribute(`data-precedence`)||a!==i.rel||e.getAttribute(`href`)!==(i.href==null||i.href===``?null:i.href)||e.getAttribute(`crossorigin`)!==(i.crossOrigin==null?null:i.crossOrigin)||e.getAttribute(`title`)!==(i.title==null?null:i.title))break;return e;case`style`:if(e.hasAttribute(`data-precedence`))break;return e;case`script`:if(a=e.getAttribute(`src`),(a!==(i.src==null?null:i.src)||e.getAttribute(`type`)!==(i.type==null?null:i.type)||e.getAttribute(`crossorigin`)!==(i.crossOrigin==null?null:i.crossOrigin))&&a&&e.hasAttribute(`async`)&&!e.hasAttribute(`itemprop`))break;return e;default:return e}}else if(t===`input`&&e.type===`hidden`){var a=i.name==null?null:``+i.name;if(i.type===`hidden`&&e.getAttribute(`name`)===a)return e}else return e;if(e=vf(e.nextSibling),e===null)break}return null}function pf(e,t,n){if(t===``)return null;for(;e.nodeType!==3;)if((e.nodeType!==1||e.nodeName!==`INPUT`||e.type!==`hidden`)&&!n||(e=vf(e.nextSibling),e===null))return null;return e}function mf(e,t){for(;e.nodeType!==8;)if((e.nodeType!==1||e.nodeName!==`INPUT`||e.type!==`hidden`)&&!t||(e=vf(e.nextSibling),e===null))return null;return e}function hf(e){return e.data===`$?`||e.data===`$~`}function gf(e){return e.data===`$!`||e.data===`$?`&&e.ownerDocument.readyState!==`loading`}function _f(e,t){var n=e.ownerDocument;if(e.data===`$~`)e._reactRetry=t;else if(e.data!==`$?`||n.readyState!==`loading`)t();else{var r=function(){t(),n.removeEventListener(`DOMContentLoaded`,r)};n.addEventListener(`DOMContentLoaded`,r),e._reactRetry=r}}function vf(e){for(;e!=null;e=e.nextSibling){var t=e.nodeType;if(t===1||t===3)break;if(t===8){if(t=e.data,t===`$`||t===`$!`||t===`$?`||t===`$~`||t===`&`||t===`F!`||t===`F`)break;if(t===`/$`||t===`/&`)return null}}return e}var yf=null;function bf(e){e=e.nextSibling;for(var t=0;e;){if(e.nodeType===8){var n=e.data;if(n===`/$`||n===`/&`){if(t===0)return vf(e.nextSibling);t--}else n!==`$`&&n!==`$!`&&n!==`$?`&&n!==`$~`&&n!==`&`||t++}e=e.nextSibling}return null}function xf(e){e=e.previousSibling;for(var t=0;e;){if(e.nodeType===8){var n=e.data;if(n===`$`||n===`$!`||n===`$?`||n===`$~`||n===`&`){if(t===0)return e;t--}else n!==`/$`&&n!==`/&`||t++}e=e.previousSibling}return null}function Sf(e,t,n){switch(t=U(n),e){case`html`:if(e=t.documentElement,!e)throw Error(i(452));return e;case`head`:if(e=t.head,!e)throw Error(i(453));return e;case`body`:if(e=t.body,!e)throw Error(i(454));return e;default:throw Error(i(451))}}function Cf(e){for(var t=e.attributes;t.length;)e.removeAttributeNode(t[0]);gt(e)}var wf=new Map,Tf=new Set;function Ef(e){return typeof e.getRootNode==`function`?e.getRootNode():e.nodeType===9?e:e.ownerDocument}var Df=F.d;F.d={f:Of,r:kf,D:Mf,C:Nf,L:Pf,m:Ff,X:Lf,S:If,M:Rf};function Of(){var e=Df.f(),t=Ou();return e||t}function kf(e){var t=vt(e);t!==null&&t.tag===5&&t.type===`form`?As(t):Df.r(e)}var Af=typeof document>`u`?null:document;function jf(e,t,n){var r=Af;if(r&&typeof t==`string`&&t){var i=Bt(t);i=`link[rel="`+e+`"][href="`+i+`"]`,typeof n==`string`&&(i+=`[crossorigin="`+n+`"]`),Tf.has(i)||(Tf.add(i),e={rel:e,crossOrigin:n,href:t},r.querySelector(i)===null&&(t=r.createElement(`link`),Gd(t,`link`,e),xt(t),r.head.appendChild(t)))}}function Mf(e){Df.D(e),jf(`dns-prefetch`,e,null)}function Nf(e,t){Df.C(e,t),jf(`preconnect`,e,t)}function Pf(e,t,n){Df.L(e,t,n);var r=Af;if(r&&e&&t){var i=`link[rel="preload"][as="`+Bt(t)+`"]`;t===`image`&&n&&n.imageSrcSet?(i+=`[imagesrcset="`+Bt(n.imageSrcSet)+`"]`,typeof n.imageSizes==`string`&&(i+=`[imagesizes="`+Bt(n.imageSizes)+`"]`)):i+=`[href="`+Bt(e)+`"]`;var a=i;switch(t){case`style`:a=Bf(e);break;case`script`:a=Wf(e)}wf.has(a)||(e=h({rel:`preload`,href:t===`image`&&n&&n.imageSrcSet?void 0:e,as:t},n),wf.set(a,e),r.querySelector(i)!==null||t===`style`&&r.querySelector(Vf(a))||t===`script`&&r.querySelector(Gf(a))||(t=r.createElement(`link`),Gd(t,`link`,e),xt(t),r.head.appendChild(t)))}}function Ff(e,t){Df.m(e,t);var n=Af;if(n&&e){var r=t&&typeof t.as==`string`?t.as:`script`,i=`link[rel="modulepreload"][as="`+Bt(r)+`"][href="`+Bt(e)+`"]`,a=i;switch(r){case`audioworklet`:case`paintworklet`:case`serviceworker`:case`sharedworker`:case`worker`:case`script`:a=Wf(e)}if(!wf.has(a)&&(e=h({rel:`modulepreload`,href:e},t),wf.set(a,e),n.querySelector(i)===null)){switch(r){case`audioworklet`:case`paintworklet`:case`serviceworker`:case`sharedworker`:case`worker`:case`script`:if(n.querySelector(Gf(a)))return}r=n.createElement(`link`),Gd(r,`link`,e),xt(r),n.head.appendChild(r)}}}function If(e,t,n){Df.S(e,t,n);var r=Af;if(r&&e){var i=bt(r).hoistableStyles,a=Bf(e);t||=`default`;var o=i.get(a);if(!o){var s={loading:0,preload:null};if(o=r.querySelector(Vf(a)))s.loading=5;else{e=h({rel:`stylesheet`,href:e,"data-precedence":t},n),(n=wf.get(a))&&Jf(e,n);var c=o=r.createElement(`link`);xt(c),Gd(c,`link`,e),c._p=new Promise(function(e,t){c.onload=e,c.onerror=t}),c.addEventListener(`load`,function(){s.loading|=1}),c.addEventListener(`error`,function(){s.loading|=2}),s.loading|=4,qf(o,t,r)}o={type:`stylesheet`,instance:o,count:1,state:s},i.set(a,o)}}}function Lf(e,t){Df.X(e,t);var n=Af;if(n&&e){var r=bt(n).hoistableScripts,i=Wf(e),a=r.get(i);a||(a=n.querySelector(Gf(i)),a||(e=h({src:e,async:!0},t),(t=wf.get(i))&&Yf(e,t),a=n.createElement(`script`),xt(a),Gd(a,`link`,e),n.head.appendChild(a)),a={type:`script`,instance:a,count:1,state:null},r.set(i,a))}}function Rf(e,t){Df.M(e,t);var n=Af;if(n&&e){var r=bt(n).hoistableScripts,i=Wf(e),a=r.get(i);a||(a=n.querySelector(Gf(i)),a||(e=h({src:e,async:!0,type:`module`},t),(t=wf.get(i))&&Yf(e,t),a=n.createElement(`script`),xt(a),Gd(a,`link`,e),n.head.appendChild(a)),a={type:`script`,instance:a,count:1,state:null},r.set(i,a))}}function zf(e,t,n,r){var a=(a=le.current)?Ef(a):null;if(!a)throw Error(i(446));switch(e){case`meta`:case`title`:return null;case`style`:return typeof n.precedence==`string`&&typeof n.href==`string`?(t=Bf(n.href),n=bt(a).hoistableStyles,r=n.get(t),r||(r={type:`style`,instance:null,count:0,state:null},n.set(t,r)),r):{type:`void`,instance:null,count:0,state:null};case`link`:if(n.rel===`stylesheet`&&typeof n.href==`string`&&typeof n.precedence==`string`){e=Bf(n.href);var o=bt(a).hoistableStyles,s=o.get(e);if(s||(a=a.ownerDocument||a,s={type:`stylesheet`,instance:null,count:0,state:{loading:0,preload:null}},o.set(e,s),(o=a.querySelector(Vf(e)))&&!o._p&&(s.instance=o,s.state.loading=5),wf.has(e)||(n={rel:`preload`,as:`style`,href:n.href,crossOrigin:n.crossOrigin,integrity:n.integrity,media:n.media,hrefLang:n.hrefLang,referrerPolicy:n.referrerPolicy},wf.set(e,n),o||Uf(a,e,n,s.state))),t&&r===null)throw Error(i(528,``));return s}if(t&&r!==null)throw Error(i(529,``));return null;case`script`:return t=n.async,n=n.src,typeof n==`string`&&t&&typeof t!=`function`&&typeof t!=`symbol`?(t=Wf(n),n=bt(a).hoistableScripts,r=n.get(t),r||(r={type:`script`,instance:null,count:0,state:null},n.set(t,r)),r):{type:`void`,instance:null,count:0,state:null};default:throw Error(i(444,e))}}function Bf(e){return`href="`+Bt(e)+`"`}function Vf(e){return`link[rel="stylesheet"][`+e+`]`}function Hf(e){return h({},e,{"data-precedence":e.precedence,precedence:null})}function Uf(e,t,n,r){e.querySelector(`link[rel="preload"][as="style"][`+t+`]`)?r.loading=1:(t=e.createElement(`link`),r.preload=t,t.addEventListener(`load`,function(){return r.loading|=1}),t.addEventListener(`error`,function(){return r.loading|=2}),Gd(t,`link`,n),xt(t),e.head.appendChild(t))}function Wf(e){return`[src="`+Bt(e)+`"]`}function Gf(e){return`script[async]`+e}function Kf(e,t,n){if(t.count++,t.instance===null)switch(t.type){case`style`:var r=e.querySelector(`style[data-href~="`+Bt(n.href)+`"]`);if(r)return t.instance=r,xt(r),r;var a=h({},n,{"data-href":n.href,"data-precedence":n.precedence,href:null,precedence:null});return r=(e.ownerDocument||e).createElement(`style`),xt(r),Gd(r,`style`,a),qf(r,n.precedence,e),t.instance=r;case`stylesheet`:a=Bf(n.href);var o=e.querySelector(Vf(a));if(o)return t.state.loading|=4,t.instance=o,xt(o),o;r=Hf(n),(a=wf.get(a))&&Jf(r,a),o=(e.ownerDocument||e).createElement(`link`),xt(o);var s=o;return s._p=new Promise(function(e,t){s.onload=e,s.onerror=t}),Gd(o,`link`,r),t.state.loading|=4,qf(o,n.precedence,e),t.instance=o;case`script`:return o=Wf(n.src),(a=e.querySelector(Gf(o)))?(t.instance=a,xt(a),a):(r=n,(a=wf.get(o))&&(r=h({},n),Yf(r,a)),e=e.ownerDocument||e,a=e.createElement(`script`),xt(a),Gd(a,`link`,r),e.head.appendChild(a),t.instance=a);case`void`:return null;default:throw Error(i(443,t.type))}else t.type===`stylesheet`&&!(t.state.loading&4)&&(r=t.instance,t.state.loading|=4,qf(r,n.precedence,e));return t.instance}function qf(e,t,n){for(var r=n.querySelectorAll(`link[rel="stylesheet"][data-precedence],style[data-precedence]`),i=r.length?r[r.length-1]:null,a=i,o=0;o<r.length;o++){var s=r[o];if(s.dataset.precedence===t)a=s;else if(a!==i)break}a?a.parentNode.insertBefore(e,a.nextSibling):(t=n.nodeType===9?n.head:n,t.insertBefore(e,t.firstChild))}function Jf(e,t){e.crossOrigin??=t.crossOrigin,e.referrerPolicy??=t.referrerPolicy,e.title??=t.title}function Yf(e,t){e.crossOrigin??=t.crossOrigin,e.referrerPolicy??=t.referrerPolicy,e.integrity??=t.integrity}var Xf=null;function Zf(e,t,n){if(Xf===null){var r=new Map,i=Xf=new Map;i.set(n,r)}else i=Xf,r=i.get(n),r||(r=new Map,i.set(n,r));if(r.has(e))return r;for(r.set(e,null),n=n.getElementsByTagName(e),i=0;i<n.length;i++){var a=n[i];if(!(a[ht]||a[ct]||e===`link`&&a.getAttribute(`rel`)===`stylesheet`)&&a.namespaceURI!==`http://www.w3.org/2000/svg`){var o=a.getAttribute(t)||``;o=e+o;var s=r.get(o);s?s.push(a):r.set(o,[a])}}return r}function Qf(e,t,n){e=e.ownerDocument||e,e.head.insertBefore(n,t===`title`?e.querySelector(`head > title`):null)}function $f(e,t,n){if(n===1||t.itemProp!=null)return!1;switch(e){case`meta`:case`title`:return!0;case`style`:if(typeof t.precedence!=`string`||typeof t.href!=`string`||t.href===``)break;return!0;case`link`:if(typeof t.rel!=`string`||typeof t.href!=`string`||t.href===``||t.onLoad||t.onError)break;switch(t.rel){case`stylesheet`:return e=t.disabled,typeof t.precedence==`string`&&e==null;default:return!0}case`script`:if(t.async&&typeof t.async!=`function`&&typeof t.async!=`symbol`&&!t.onLoad&&!t.onError&&t.src&&typeof t.src==`string`)return!0}return!1}function ep(e){return!(e.type===`stylesheet`&&!(e.state.loading&3))}function tp(e,t,n,r){if(n.type===`stylesheet`&&(typeof r.media!=`string`||!1!==matchMedia(r.media).matches)&&!(n.state.loading&4)){if(n.instance===null){var i=Bf(r.href),a=t.querySelector(Vf(i));if(a){t=a._p,typeof t==`object`&&t&&typeof t.then==`function`&&(e.count++,e=ip.bind(e),t.then(e,e)),n.state.loading|=4,n.instance=a,xt(a);return}a=t.ownerDocument||t,r=Hf(r),(i=wf.get(i))&&Jf(r,i),a=a.createElement(`link`),xt(a);var o=a;o._p=new Promise(function(e,t){o.onload=e,o.onerror=t}),Gd(a,`link`,r),n.instance=a}e.stylesheets===null&&(e.stylesheets=new Map),e.stylesheets.set(n,t),(t=n.state.preload)&&!(n.state.loading&3)&&(e.count++,n=ip.bind(e),t.addEventListener(`load`,n),t.addEventListener(`error`,n))}}var np=0;function rp(e,t){return e.stylesheets&&e.count===0&&op(e,e.stylesheets),0<e.count||0<e.imgCount?function(n){var r=setTimeout(function(){if(e.stylesheets&&op(e,e.stylesheets),e.unsuspend){var t=e.unsuspend;e.unsuspend=null,t()}},6e4+t);0<e.imgBytes&&np===0&&(np=62500*Jd());var i=setTimeout(function(){if(e.waitingForImages=!1,e.count===0&&(e.stylesheets&&op(e,e.stylesheets),e.unsuspend)){var t=e.unsuspend;e.unsuspend=null,t()}},(e.imgBytes>np?50:800)+t);return e.unsuspend=n,function(){e.unsuspend=null,clearTimeout(r),clearTimeout(i)}}:null}function ip(){if(this.count--,this.count===0&&(this.imgCount===0||!this.waitingForImages)){if(this.stylesheets)op(this,this.stylesheets);else if(this.unsuspend){var e=this.unsuspend;this.unsuspend=null,e()}}}var ap=null;function op(e,t){e.stylesheets=null,e.unsuspend!==null&&(e.count++,ap=new Map,t.forEach(sp,e),ap=null,ip.call(e))}function sp(e,t){if(!(t.state.loading&4)){var n=ap.get(e);if(n)var r=n.get(null);else{n=new Map,ap.set(e,n);for(var i=e.querySelectorAll(`link[data-precedence],style[data-precedence]`),a=0;a<i.length;a++){var o=i[a];(o.nodeName===`LINK`||o.getAttribute(`media`)!==`not all`)&&(n.set(o.dataset.precedence,o),r=o)}r&&n.set(null,r)}i=t.instance,o=i.getAttribute(`data-precedence`),a=n.get(o)||r,a===r&&n.set(null,i),n.set(o,i),this.count++,r=ip.bind(this),i.addEventListener(`load`,r),i.addEventListener(`error`,r),a?a.parentNode.insertBefore(i,a.nextSibling):(e=e.nodeType===9?e.head:e,e.insertBefore(i,e.firstChild)),t.state.loading|=4}}var cp={$$typeof:C,Provider:null,Consumer:null,_currentValue:ne,_currentValue2:ne,_threadCount:0};function lp(e,t,n,r,i,a,o,s,c){this.tag=1,this.containerInfo=e,this.pingCache=this.current=this.pendingChildren=null,this.timeoutHandle=-1,this.callbackNode=this.next=this.pendingContext=this.context=this.cancelPendingCommit=null,this.callbackPriority=0,this.expirationTimes=Ze(-1),this.entangledLanes=this.shellSuspendCounter=this.errorRecoveryDisabledLanes=this.expiredLanes=this.warmLanes=this.pingedLanes=this.suspendedLanes=this.pendingLanes=0,this.entanglements=Ze(0),this.hiddenUpdates=Ze(null),this.identifierPrefix=r,this.onUncaughtError=i,this.onCaughtError=a,this.onRecoverableError=o,this.pooledCache=null,this.pooledCacheLanes=0,this.formState=c,this.incompleteTransitions=new Map}function up(e,t,n,r,i,a,o,s,c,l,u,d){return e=new lp(e,t,n,o,c,l,u,d,s),t=1,!0===a&&(t|=24),a=ui(3,null,null,t),e.current=a,a.stateNode=e,t=ua(),t.refCount++,e.pooledCache=t,t.refCount++,a.memoizedState={element:r,isDehydrated:n,cache:t},Ua(a),e}function dp(e){return e?(e=ci,e):ci}function fp(e,t,n,r,i,a){i=dp(i),r.context===null?r.context=i:r.pendingContext=i,r=Ga(t),r.payload={element:n},a=a===void 0?null:a,a!==null&&(r.callback=a),n=Ka(e,r,t),n!==null&&(Cu(n,e,t),qa(n,e,t))}function pp(e,t){if(e=e.memoizedState,e!==null&&e.dehydrated!==null){var n=e.retryLane;e.retryLane=n!==0&&n<t?n:t}}function mp(e,t){pp(e,t),(e=e.alternate)&&pp(e,t)}function hp(e){if(e.tag===13||e.tag===31){var t=ai(e,67108864);t!==null&&Cu(t,e,67108864),mp(e,67108864)}}function gp(e){if(e.tag===13||e.tag===31){var t=xu();t=rt(t);var n=ai(e,t);n!==null&&Cu(n,e,t),mp(e,t)}}var _p=!0;function vp(e,t,n,r){var i=P.T;P.T=null;var a=F.p;try{F.p=2,bp(e,t,n,r)}finally{F.p=a,P.T=i}}function yp(e,t,n,r){var i=P.T;P.T=null;var a=F.p;try{F.p=8,bp(e,t,n,r)}finally{F.p=a,P.T=i}}function bp(e,t,n,r){if(_p){var i=xp(r);if(i===null)Pd(e,t,r,Sp,n),Np(e,r);else if(Fp(i,e,t,n,r))r.stopPropagation();else if(Np(e,r),t&4&&-1<Mp.indexOf(e)){for(;i!==null;){var a=vt(i);if(a!==null)switch(a.tag){case 3:if(a=a.stateNode,a.current.memoizedState.isDehydrated){var o=Ke(a.pendingLanes);if(o!==0){var s=a;for(s.pendingLanes|=2,s.entangledLanes|=2;o;){var c=1<<31-ze(o);s.entanglements[1]|=c,o&=~c}fd(a),!(Hl&6)&&(lu=De()+500,pd(0,!1))}}break;case 31:case 13:s=ai(a,2),s!==null&&Cu(s,a,2),Ou(),mp(a,2)}if(a=xp(r),a===null&&Pd(e,t,r,Sp,n),a===i)break;i=a}i!==null&&r.stopPropagation()}else Pd(e,t,r,null,n)}}function xp(e){return e=rn(e),Cp(e)}var Sp=null;function Cp(e){if(Sp=null,e=_t(e),e!==null){var t=o(e);if(t===null)e=null;else{var n=t.tag;if(n===13){if(e=s(t),e!==null)return e;e=null}else if(n===31){if(e=c(t),e!==null)return e;e=null}else if(n===3){if(t.stateNode.current.memoizedState.isDehydrated)return t.tag===3?t.stateNode.containerInfo:null;e=null}else t!==e&&(e=null)}}return Sp=e,null}function wp(e){switch(e){case`beforetoggle`:case`cancel`:case`click`:case`close`:case`contextmenu`:case`copy`:case`cut`:case`auxclick`:case`dblclick`:case`dragend`:case`dragstart`:case`drop`:case`focusin`:case`focusout`:case`input`:case`invalid`:case`keydown`:case`keypress`:case`keyup`:case`mousedown`:case`mouseup`:case`paste`:case`pause`:case`play`:case`pointercancel`:case`pointerdown`:case`pointerup`:case`ratechange`:case`reset`:case`resize`:case`seeked`:case`submit`:case`toggle`:case`touchcancel`:case`touchend`:case`touchstart`:case`volumechange`:case`change`:case`selectionchange`:case`textInput`:case`compositionstart`:case`compositionend`:case`compositionupdate`:case`beforeblur`:case`afterblur`:case`beforeinput`:case`blur`:case`fullscreenchange`:case`focus`:case`hashchange`:case`popstate`:case`select`:case`selectstart`:return 2;case`drag`:case`dragenter`:case`dragexit`:case`dragleave`:case`dragover`:case`mousemove`:case`mouseout`:case`mouseover`:case`pointermove`:case`pointerout`:case`pointerover`:case`scroll`:case`touchmove`:case`wheel`:case`mouseenter`:case`mouseleave`:case`pointerenter`:case`pointerleave`:return 8;case`message`:switch(Oe()){case ke:return 2;case Ae:return 8;case je:case Me:return 32;case Ne:return 268435456;default:return 32}default:return 32}}var Tp=!1,Ep=null,Dp=null,Op=null,kp=new Map,Ap=new Map,jp=[],Mp=`mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset`.split(` `);function Np(e,t){switch(e){case`focusin`:case`focusout`:Ep=null;break;case`dragenter`:case`dragleave`:Dp=null;break;case`mouseover`:case`mouseout`:Op=null;break;case`pointerover`:case`pointerout`:kp.delete(t.pointerId);break;case`gotpointercapture`:case`lostpointercapture`:Ap.delete(t.pointerId)}}function Pp(e,t,n,r,i,a){return e===null||e.nativeEvent!==a?(e={blockedOn:t,domEventName:n,eventSystemFlags:r,nativeEvent:a,targetContainers:[i]},t!==null&&(t=vt(t),t!==null&&hp(t)),e):(e.eventSystemFlags|=r,t=e.targetContainers,i!==null&&t.indexOf(i)===-1&&t.push(i),e)}function Fp(e,t,n,r,i){switch(t){case`focusin`:return Ep=Pp(Ep,e,t,n,r,i),!0;case`dragenter`:return Dp=Pp(Dp,e,t,n,r,i),!0;case`mouseover`:return Op=Pp(Op,e,t,n,r,i),!0;case`pointerover`:var a=i.pointerId;return kp.set(a,Pp(kp.get(a)||null,e,t,n,r,i)),!0;case`gotpointercapture`:return a=i.pointerId,Ap.set(a,Pp(Ap.get(a)||null,e,t,n,r,i)),!0}return!1}function Ip(e){var t=_t(e.target);if(t!==null){var n=o(t);if(n!==null){if(t=n.tag,t===13){if(t=s(n),t!==null){e.blockedOn=t,ot(e.priority,function(){gp(n)});return}}else if(t===31){if(t=c(n),t!==null){e.blockedOn=t,ot(e.priority,function(){gp(n)});return}}else if(t===3&&n.stateNode.current.memoizedState.isDehydrated){e.blockedOn=n.tag===3?n.stateNode.containerInfo:null;return}}}e.blockedOn=null}function Lp(e){if(e.blockedOn!==null)return!1;for(var t=e.targetContainers;0<t.length;){var n=xp(e.nativeEvent);if(n===null){n=e.nativeEvent;var r=new n.constructor(n.type,n);nn=r,n.target.dispatchEvent(r),nn=null}else return t=vt(n),t!==null&&hp(t),e.blockedOn=n,!1;t.shift()}return!0}function Rp(e,t,n){Lp(e)&&n.delete(t)}function zp(){Tp=!1,Ep!==null&&Lp(Ep)&&(Ep=null),Dp!==null&&Lp(Dp)&&(Dp=null),Op!==null&&Lp(Op)&&(Op=null),kp.forEach(Rp),Ap.forEach(Rp)}function Bp(e,n){e.blockedOn===n&&(e.blockedOn=null,Tp||(Tp=!0,t.unstable_scheduleCallback(t.unstable_NormalPriority,zp)))}var Vp=null;function Hp(e){Vp!==e&&(Vp=e,t.unstable_scheduleCallback(t.unstable_NormalPriority,function(){Vp===e&&(Vp=null);for(var t=0;t<e.length;t+=3){var n=e[t],r=e[t+1],i=e[t+2];if(typeof r!=`function`){if(Cp(r||n)===null)continue;break}var a=vt(n);a!==null&&(e.splice(t,3),t-=3,Os(a,{pending:!0,data:i,method:n.method,action:r},r,i))}}))}function Up(e){function t(t){return Bp(t,e)}Ep!==null&&Bp(Ep,e),Dp!==null&&Bp(Dp,e),Op!==null&&Bp(Op,e),kp.forEach(t),Ap.forEach(t);for(var n=0;n<jp.length;n++){var r=jp[n];r.blockedOn===e&&(r.blockedOn=null)}for(;0<jp.length&&(n=jp[0],n.blockedOn===null);)Ip(n),n.blockedOn===null&&jp.shift();if(n=(e.ownerDocument||e).$$reactFormReplay,n!=null)for(r=0;r<n.length;r+=3){var i=n[r],a=n[r+1],o=i[lt]||null;if(typeof a==`function`)o||Hp(n);else if(o){var s=null;if(a&&a.hasAttribute(`formAction`)){if(i=a,o=a[lt]||null)s=o.formAction;else if(Cp(i)!==null)continue}else s=o.action;typeof s==`function`?n[r+1]=s:(n.splice(r,3),r-=3),Hp(n)}}}function Wp(){function e(e){e.canIntercept&&e.info===`react-transition`&&e.intercept({handler:function(){return new Promise(function(e){return i=e})},focusReset:`manual`,scroll:`manual`})}function t(){i!==null&&(i(),i=null),r||setTimeout(n,20)}function n(){if(!r&&!navigation.transition){var e=navigation.currentEntry;e&&e.url!=null&&navigation.navigate(e.url,{state:e.getState(),info:`react-transition`,history:`replace`})}}if(typeof navigation==`object`){var r=!1,i=null;return navigation.addEventListener(`navigate`,e),navigation.addEventListener(`navigatesuccess`,t),navigation.addEventListener(`navigateerror`,t),setTimeout(n,100),function(){r=!0,navigation.removeEventListener(`navigate`,e),navigation.removeEventListener(`navigatesuccess`,t),navigation.removeEventListener(`navigateerror`,t),i!==null&&(i(),i=null)}}}function Gp(e){this._internalRoot=e}Kp.prototype.render=Gp.prototype.render=function(e){var t=this._internalRoot;if(t===null)throw Error(i(409));var n=t.current;fp(n,xu(),e,t,null,null)},Kp.prototype.unmount=Gp.prototype.unmount=function(){var e=this._internalRoot;if(e!==null){this._internalRoot=null;var t=e.containerInfo;fp(e.current,2,null,e,null,null),Ou(),t[ut]=null}};function Kp(e){this._internalRoot=e}Kp.prototype.unstable_scheduleHydration=function(e){if(e){var t=at();e={blockedOn:null,target:e,priority:t};for(var n=0;n<jp.length&&t!==0&&t<jp[n].priority;n++);jp.splice(n,0,e),n===0&&Ip(e)}};var qp=n.version;if(qp!==`19.2.0`)throw Error(i(527,qp,`19.2.0`));F.findDOMNode=function(e){var t=e._reactInternals;if(t===void 0)throw typeof e.render==`function`?Error(i(188)):(e=Object.keys(e).join(`,`),Error(i(268,e)));return e=d(t),e=e===null?null:p(e),e=e===null?null:e.stateNode,e};var Jp={bundleType:0,version:`19.2.0`,rendererPackageName:`react-dom`,currentDispatcherRef:P,reconcilerVersion:`19.2.0`};if(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__<`u`){var Yp=__REACT_DEVTOOLS_GLOBAL_HOOK__;if(!Yp.isDisabled&&Yp.supportsFiber)try{Ie=Yp.inject(Jp),Le=Yp}catch{}}e.createRoot=function(e,t){if(!a(e))throw Error(i(299));var n=!1,r=``,o=Zs,s=Qs,c=$s;return t!=null&&(!0===t.unstable_strictMode&&(n=!0),t.identifierPrefix!==void 0&&(r=t.identifierPrefix),t.onUncaughtError!==void 0&&(o=t.onUncaughtError),t.onCaughtError!==void 0&&(s=t.onCaughtError),t.onRecoverableError!==void 0&&(c=t.onRecoverableError)),t=up(e,1,!1,null,null,n,r,null,o,s,c,Wp),e[ut]=t.current,Md(e),new Gp(t)}})),g=o(((e,t)=>{function n(){if(!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__>`u`||typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE!=`function`))try{__REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(n)}catch(e){console.error(e)}}n(),t.exports=h()})),_=`modulepreload`,v=function(e){return`/`+e},y={};const b=function(e,t,n){let r=Promise.resolve();if(t&&t.length>0){let e=document.getElementsByTagName(`link`),i=document.querySelector(`meta[property=csp-nonce]`),a=i?.nonce||i?.getAttribute(`nonce`);function o(e){return Promise.all(e.map(e=>Promise.resolve(e).then(e=>({status:`fulfilled`,value:e}),e=>({status:`rejected`,reason:e}))))}r=o(t.map(t=>{if(t=v(t,n),t in y)return;y[t]=!0;let r=t.endsWith(`.css`),i=r?`[rel="stylesheet"]`:``;if(n)for(let n=e.length-1;n>=0;n--){let i=e[n];if(i.href===t&&(!r||i.rel===`stylesheet`))return}else if(document.querySelector(`link[href="${t}"]${i}`))return;let o=document.createElement(`link`);if(o.rel=r?`stylesheet`:_,r||(o.as=`script`),o.crossOrigin=``,o.href=t,a&&o.setAttribute(`nonce`,a),document.head.appendChild(o),r)return new Promise((e,n)=>{o.addEventListener(`load`,e),o.addEventListener(`error`,()=>n(Error(`Unable to preload CSS for ${t}`)))})}))}function i(e){let t=new Event(`vite:preloadError`,{cancelable:!0});if(t.payload=e,window.dispatchEvent(t),!t.defaultPrevented)throw e}return r.then(t=>{for(let e of t||[])e.status===`rejected`&&i(e.reason);return e().catch(i)})};var x=c(u(),1),S=`popstate`;function C(e={}){function t(e,t){let{pathname:n,search:r,hash:i}=e.location;return O(``,{pathname:n,search:r,hash:i},t.state&&t.state.usr||null,t.state&&t.state.key||`default`)}function n(e,t){return typeof t==`string`?t:k(t)}return A(t,n,null,e)}function w(e,t){if(e===!1||e==null)throw Error(t)}function T(e,t){if(!e){typeof console<`u`&&console.warn(t);try{throw Error(t)}catch{}}}function E(){return Math.random().toString(36).substring(2,10)}function D(e,t){return{usr:e.state,key:e.key,idx:t}}function O(e,t,n=null,r){return{pathname:typeof e==`string`?e:e.pathname,search:``,hash:``,...typeof t==`string`?ee(t):t,state:n,key:t&&t.key||r||E()}}function k({pathname:e=`/`,search:t=``,hash:n=``}){return t&&t!==`?`&&(e+=t.charAt(0)===`?`?t:`?`+t),n&&n!==`#`&&(e+=n.charAt(0)===`#`?n:`#`+n),e}function ee(e){let t={};if(e){let n=e.indexOf(`#`);n>=0&&(t.hash=e.substring(n),e=e.substring(0,n));let r=e.indexOf(`?`);r>=0&&(t.search=e.substring(r),e=e.substring(0,r)),e&&(t.pathname=e)}return t}function A(e,t,n,r={}){let{window:i=document.defaultView,v5Compat:a=!1}=r,o=i.history,s=`POP`,c=null,l=u();l??(l=0,o.replaceState({...o.state,idx:l},``));function u(){return(o.state||{idx:null}).idx}function d(){s=`POP`;let e=u(),t=e==null?null:e-l;l=e,c&&c({action:s,location:h.location,delta:t})}function f(e,t){s=`PUSH`;let r=O(h.location,e,t);n&&n(r,e),l=u()+1;let d=D(r,l),f=h.createHref(r);try{o.pushState(d,``,f)}catch(e){if(e instanceof DOMException&&e.name===`DataCloneError`)throw e;i.location.assign(f)}a&&c&&c({action:s,location:h.location,delta:1})}function p(e,t){s=`REPLACE`;let r=O(h.location,e,t);n&&n(r,e),l=u();let i=D(r,l),d=h.createHref(r);o.replaceState(i,``,d),a&&c&&c({action:s,location:h.location,delta:0})}function m(e){return j(e)}let h={get action(){return s},get location(){return e(i,o)},listen(e){if(c)throw Error(`A history only accepts one active listener`);return i.addEventListener(S,d),c=e,()=>{i.removeEventListener(S,d),c=null}},createHref(e){return t(i,e)},createURL:m,encodeLocation(e){let t=m(e);return{pathname:t.pathname,search:t.search,hash:t.hash}},push:f,replace:p,go(e){return o.go(e)}};return h}function j(e,t=!1){let n=`http://localhost`;typeof window<`u`&&(n=window.location.origin===`null`?window.location.href:window.location.origin),w(n,`No window.location.(origin|href) available to create URL`);let r=typeof e==`string`?e:k(e);return r=r.replace(/ $/,`%20`),!t&&r.startsWith(`//`)&&(r=n+r),new URL(r,n)}function M(e,t,n=`/`){return te(e,t,n,!1)}function te(e,t,n,r){let i=he((typeof t==`string`?ee(t):t).pathname||`/`,n);if(i==null)return null;let a=P(e);ne(a);let o=null;for(let e=0;o==null&&e<a.length;++e){let t=me(i);o=de(a[e],t,r)}return o}function N(e,t){let{route:n,pathname:r,params:i}=e;return{id:n.id,pathname:r,params:i,data:t[n.id],loaderData:t[n.id],handle:n.handle}}function P(e,t=[],n=[],r=``,i=!1){let a=(e,a,o=i,s)=>{let c={relativePath:s===void 0?e.path||``:s,caseSensitive:e.caseSensitive===!0,childrenIndex:a,route:e};if(c.relativePath.startsWith(`/`)){if(!c.relativePath.startsWith(r)&&o)return;w(c.relativePath.startsWith(r),`Absolute route path "${c.relativePath}" nested under path "${r}" is not valid. An absolute child route path must start with the combined path of all its parent routes.`),c.relativePath=c.relativePath.slice(r.length)}let l=we([r,c.relativePath]),u=n.concat(c);e.children&&e.children.length>0&&(w(e.index!==!0,`Index routes must not have child routes. Please remove all child routes from route path "${l}".`),P(e.children,t,u,l,o)),!(e.path==null&&!e.index)&&t.push({path:l,score:le(l,e.index),routesMeta:u})};return e.forEach((e,t)=>{if(e.path===``||!e.path?.includes(`?`))a(e,t);else for(let n of F(e.path))a(e,t,!0,n)}),t}function F(e){let t=e.split(`/`);if(t.length===0)return[];let[n,...r]=t,i=n.endsWith(`?`),a=n.replace(/\?$/,``);if(r.length===0)return i?[a,``]:[a];let o=F(r.join(`/`)),s=[];return s.push(...o.map(e=>e===``?a:[a,e].join(`/`))),i&&s.push(...o),s.map(t=>e.startsWith(`/`)&&t===``?`/`:t)}function ne(e){e.sort((e,t)=>e.score===t.score?ue(e.routesMeta.map(e=>e.childrenIndex),t.routesMeta.map(e=>e.childrenIndex)):t.score-e.score)}var re=/^:[\w-]+$/,ie=3,ae=2,oe=1,I=10,se=-2,ce=e=>e===`*`;function le(e,t){let n=e.split(`/`),r=n.length;return n.some(ce)&&(r+=se),t&&(r+=ae),n.filter(e=>!ce(e)).reduce((e,t)=>e+(re.test(t)?ie:t===``?oe:I),r)}function ue(e,t){return e.length===t.length&&e.slice(0,-1).every((e,n)=>e===t[n])?e[e.length-1]-t[t.length-1]:0}function de(e,t,n=!1){let{routesMeta:r}=e,i={},a=`/`,o=[];for(let e=0;e<r.length;++e){let s=r[e],c=e===r.length-1,l=a===`/`?t:t.slice(a.length)||`/`,u=fe({path:s.relativePath,caseSensitive:s.caseSensitive,end:c},l),d=s.route;if(!u&&c&&n&&!r[r.length-1].route.index&&(u=fe({path:s.relativePath,caseSensitive:s.caseSensitive,end:!1},l)),!u)return null;Object.assign(i,u.params),o.push({params:i,pathname:we([a,u.pathname]),pathnameBase:Te(we([a,u.pathnameBase])),route:d}),u.pathnameBase!==`/`&&(a=we([a,u.pathnameBase]))}return o}function fe(e,t){typeof e==`string`&&(e={path:e,caseSensitive:!1,end:!0});let[n,r]=pe(e.path,e.caseSensitive,e.end),i=t.match(n);if(!i)return null;let a=i[0],o=a.replace(/(.)\/+$/,`$1`),s=i.slice(1);return{params:r.reduce((e,{paramName:t,isOptional:n},r)=>{if(t===`*`){let e=s[r]||``;o=a.slice(0,a.length-e.length).replace(/(.)\/+$/,`$1`)}let i=s[r];return n&&!i?e[t]=void 0:e[t]=(i||``).replace(/%2F/g,`/`),e},{}),pathname:a,pathnameBase:o,pattern:e}}function pe(e,t=!1,n=!0){T(e===`*`||!e.endsWith(`*`)||e.endsWith(`/*`),`Route path "${e}" will be treated as if it were "${e.replace(/\*$/,`/*`)}" because the \`*\` character must always follow a \`/\` in the pattern. To get rid of this warning, please change the route path to "${e.replace(/\*$/,`/*`)}".`);let r=[],i=`^`+e.replace(/\/*\*?$/,``).replace(/^\/*/,`/`).replace(/[\\.*+^${}|()[\]]/g,`\\$&`).replace(/\/:([\w-]+)(\?)?/g,(e,t,n)=>(r.push({paramName:t,isOptional:n!=null}),n?`/?([^\\/]+)?`:`/([^\\/]+)`)).replace(/\/([\w-]+)\?(\/|$)/g,`(/$1)?$2`);return e.endsWith(`*`)?(r.push({paramName:`*`}),i+=e===`*`||e===`/*`?`(.*)$`:`(?:\\/(.+)|\\/*)$`):n?i+=`\\/*$`:e!==``&&e!==`/`&&(i+=`(?:(?=\\/|$))`),[new RegExp(i,t?void 0:`i`),r]}function me(e){try{return e.split(`/`).map(e=>decodeURIComponent(e).replace(/\//g,`%2F`)).join(`/`)}catch(t){return T(!1,`The URL path "${e}" could not be decoded because it is a malformed URL segment. This is probably due to a bad percent encoding (${t}).`),e}}function he(e,t){if(t===`/`)return e;if(!e.toLowerCase().startsWith(t.toLowerCase()))return null;let n=t.endsWith(`/`)?t.length-1:t.length,r=e.charAt(n);return r&&r!==`/`?null:e.slice(n)||`/`}var ge=/^(?:[a-z][a-z0-9+.-]*:|\/\/)/i,_e=e=>ge.test(e);function ve(e,t=`/`){let{pathname:n,search:r=``,hash:i=``}=typeof e==`string`?ee(e):e,a;if(n)if(_e(n))a=n;else{if(n.includes(`//`)){let e=n;n=n.replace(/\/\/+/g,`/`),T(!1,`Pathnames cannot have embedded double slashes - normalizing ${e} -> ${n}`)}a=n.startsWith(`/`)?ye(n.substring(1),`/`):ye(n,t)}else a=t;return{pathname:a,search:Ee(r),hash:De(i)}}function ye(e,t){let n=t.replace(/\/+$/,``).split(`/`);return e.split(`/`).forEach(e=>{e===`..`?n.length>1&&n.pop():e!==`.`&&n.push(e)}),n.length>1?n.join(`/`):`/`}function be(e,t,n,r){return`Cannot include a '${e}' character in a manually specified \`to.${t}\` field [${JSON.stringify(r)}].  Please separate it out to the \`to.${n}\` field. Alternatively you may provide the full path as a string in <Link to="..."> and the router will parse it for you.`}function xe(e){return e.filter((e,t)=>t===0||e.route.path&&e.route.path.length>0)}function Se(e){let t=xe(e);return t.map((e,n)=>n===t.length-1?e.pathname:e.pathnameBase)}function Ce(e,t,n,r=!1){let i;typeof e==`string`?i=ee(e):(i={...e},w(!i.pathname||!i.pathname.includes(`?`),be(`?`,`pathname`,`search`,i)),w(!i.pathname||!i.pathname.includes(`#`),be(`#`,`pathname`,`hash`,i)),w(!i.search||!i.search.includes(`#`),be(`#`,`search`,`hash`,i)));let a=e===``||i.pathname===``,o=a?`/`:i.pathname,s;if(o==null)s=n;else{let e=t.length-1;if(!r&&o.startsWith(`..`)){let t=o.split(`/`);for(;t[0]===`..`;)t.shift(),--e;i.pathname=t.join(`/`)}s=e>=0?t[e]:`/`}let c=ve(i,s),l=o&&o!==`/`&&o.endsWith(`/`),u=(a||o===`.`)&&n.endsWith(`/`);return!c.pathname.endsWith(`/`)&&(l||u)&&(c.pathname+=`/`),c}var we=e=>e.join(`/`).replace(/\/\/+/g,`/`),Te=e=>e.replace(/\/+$/,``).replace(/^\/*/,`/`),Ee=e=>!e||e===`?`?``:e.startsWith(`?`)?e:`?`+e,De=e=>!e||e===`#`?``:e.startsWith(`#`)?e:`#`+e;function Oe(e){return e!=null&&typeof e.status==`number`&&typeof e.statusText==`string`&&typeof e.internal==`boolean`&&`data`in e}Object.getOwnPropertyNames(Object.prototype).sort().join(`\0`);var ke=x.createContext(null);ke.displayName=`DataRouter`;var Ae=x.createContext(null);Ae.displayName=`DataRouterState`,x.createContext(!1);var je=x.createContext({isTransitioning:!1});je.displayName=`ViewTransition`;var Me=x.createContext(new Map);Me.displayName=`Fetchers`;var Ne=x.createContext(null);Ne.displayName=`Await`;var Pe=x.createContext(null);Pe.displayName=`Navigation`;var Fe=x.createContext(null);Fe.displayName=`Location`;var Ie=x.createContext({outlet:null,matches:[],isDataRoute:!1});Ie.displayName=`Route`;var Le=x.createContext(null);Le.displayName=`RouteError`;function Re(e,{relative:t}={}){w(ze(),`useHref() may be used only in the context of a <Router> component.`);let{basename:n,navigator:r}=x.useContext(Pe),{hash:i,pathname:a,search:o}=Ge(e,{relative:t}),s=a;return n!==`/`&&(s=a===`/`?n:we([n,a])),r.createHref({pathname:s,search:o,hash:i})}function ze(){return x.useContext(Fe)!=null}function Be(){return w(ze(),`useLocation() may be used only in the context of a <Router> component.`),x.useContext(Fe).location}var Ve=`You should call navigate() in a React.useEffect(), not when your component is first rendered.`;function He(e){x.useContext(Pe).static||x.useLayoutEffect(e)}function Ue(){let{isDataRoute:e}=x.useContext(Ie);return e?ct():We()}function We(){w(ze(),`useNavigate() may be used only in the context of a <Router> component.`);let e=x.useContext(ke),{basename:t,navigator:n}=x.useContext(Pe),{matches:r}=x.useContext(Ie),{pathname:i}=Be(),a=JSON.stringify(Se(r)),o=x.useRef(!1);return He(()=>{o.current=!0}),x.useCallback((r,s={})=>{if(T(o.current,Ve),!o.current)return;if(typeof r==`number`){n.go(r);return}let c=Ce(r,JSON.parse(a),i,s.relative===`path`);e==null&&t!==`/`&&(c.pathname=c.pathname===`/`?t:we([t,c.pathname])),(s.replace?n.replace:n.push)(c,s.state,s)},[t,n,a,i,e])}x.createContext(null);function Ge(e,{relative:t}={}){let{matches:n}=x.useContext(Ie),{pathname:r}=Be(),i=JSON.stringify(Se(n));return x.useMemo(()=>Ce(e,JSON.parse(i),r,t===`path`),[e,i,r,t])}function Ke(e,t){return qe(e,t)}function qe(e,t,n,r,i){w(ze(),`useRoutes() may be used only in the context of a <Router> component.`);let{navigator:a}=x.useContext(Pe),{matches:o}=x.useContext(Ie),s=o[o.length-1],c=s?s.params:{},l=s?s.pathname:`/`,u=s?s.pathnameBase:`/`,d=s&&s.route;{let e=d&&d.path||``;ut(l,!d||e.endsWith(`*`)||e.endsWith(`*?`),`You rendered descendant <Routes> (or called \`useRoutes()\`) at "${l}" (under <Route path="${e}">) but the parent route path has no trailing "*". This means if you navigate deeper, the parent won't match anymore and therefore the child routes will never render.

Please change the parent <Route path="${e}"> to <Route path="${e===`/`?`*`:`${e}/*`}">.`)}let f=Be(),p;if(t){let e=typeof t==`string`?ee(t):t;w(u===`/`||e.pathname?.startsWith(u),`When overriding the location using \`<Routes location>\` or \`useRoutes(routes, location)\`, the location pathname must begin with the portion of the URL pathname that was matched by all parent routes. The current pathname base is "${u}" but pathname "${e.pathname}" was given in the \`location\` prop.`),p=e}else p=f;let m=p.pathname||`/`,h=m;if(u!==`/`){let e=u.replace(/^\//,``).split(`/`);h=`/`+m.replace(/^\//,``).split(`/`).slice(e.length).join(`/`)}let g=M(e,{pathname:h});T(d||g!=null,`No routes matched location "${p.pathname}${p.search}${p.hash}" `),T(g==null||g[g.length-1].route.element!==void 0||g[g.length-1].route.Component!==void 0||g[g.length-1].route.lazy!==void 0,`Matched leaf route at location "${p.pathname}${p.search}${p.hash}" does not have an element or Component. This means it will render an <Outlet /> with a null value by default resulting in an "empty" page.`);let _=Qe(g&&g.map(e=>Object.assign({},e,{params:Object.assign({},c,e.params),pathname:we([u,a.encodeLocation?a.encodeLocation(e.pathname.replace(/\?/g,`%3F`).replace(/#/g,`%23`)).pathname:e.pathname]),pathnameBase:e.pathnameBase===`/`?u:we([u,a.encodeLocation?a.encodeLocation(e.pathnameBase.replace(/\?/g,`%3F`).replace(/#/g,`%23`)).pathname:e.pathnameBase])})),o,n,r,i);return t&&_?x.createElement(Fe.Provider,{value:{location:{pathname:`/`,search:``,hash:``,state:null,key:`default`,...p},navigationType:`POP`}},_):_}function Je(){let e=st(),t=Oe(e)?`${e.status} ${e.statusText}`:e instanceof Error?e.message:JSON.stringify(e),n=e instanceof Error?e.stack:null,r=`rgba(200,200,200, 0.5)`,i={padding:`0.5rem`,backgroundColor:r},a={padding:`2px 4px`,backgroundColor:r},o=null;return console.error(`Error handled by React Router default ErrorBoundary:`,e),o=x.createElement(x.Fragment,null,x.createElement(`p`,null,`💿 Hey developer 👋`),x.createElement(`p`,null,`You can provide a way better UX than this when your app throws errors by providing your own `,x.createElement(`code`,{style:a},`ErrorBoundary`),` or`,` `,x.createElement(`code`,{style:a},`errorElement`),` prop on your route.`)),x.createElement(x.Fragment,null,x.createElement(`h2`,null,`Unexpected Application Error!`),x.createElement(`h3`,{style:{fontStyle:`italic`}},t),n?x.createElement(`pre`,{style:i},n):null,o)}var Ye=x.createElement(Je,null),Xe=class extends x.Component{constructor(e){super(e),this.state={location:e.location,revalidation:e.revalidation,error:e.error}}static getDerivedStateFromError(e){return{error:e}}static getDerivedStateFromProps(e,t){return t.location!==e.location||t.revalidation!==`idle`&&e.revalidation===`idle`?{error:e.error,location:e.location,revalidation:e.revalidation}:{error:e.error===void 0?t.error:e.error,location:t.location,revalidation:e.revalidation||t.revalidation}}componentDidCatch(e,t){this.props.onError?this.props.onError(e,t):console.error(`React Router caught the following error during render`,e)}render(){return this.state.error===void 0?this.props.children:x.createElement(Ie.Provider,{value:this.props.routeContext},x.createElement(Le.Provider,{value:this.state.error,children:this.props.component}))}};function Ze({routeContext:e,match:t,children:n}){let r=x.useContext(ke);return r&&r.static&&r.staticContext&&(t.route.errorElement||t.route.ErrorBoundary)&&(r.staticContext._deepestRenderedBoundaryId=t.route.id),x.createElement(Ie.Provider,{value:e},n)}function Qe(e,t=[],n=null,r=null,i=null){if(e==null){if(!n)return null;if(n.errors)e=n.matches;else if(t.length===0&&!n.initialized&&n.matches.length>0)e=n.matches;else return null}let a=e,o=n?.errors;if(o!=null){let e=a.findIndex(e=>e.route.id&&o?.[e.route.id]!==void 0);w(e>=0,`Could not find a matching route for errors on route IDs: ${Object.keys(o).join(`,`)}`),a=a.slice(0,Math.min(a.length,e+1))}let s=!1,c=-1;if(n)for(let e=0;e<a.length;e++){let t=a[e];if((t.route.HydrateFallback||t.route.hydrateFallbackElement)&&(c=e),t.route.id){let{loaderData:e,errors:r}=n,i=t.route.loader&&!e.hasOwnProperty(t.route.id)&&(!r||r[t.route.id]===void 0);if(t.route.lazy||i){s=!0,a=c>=0?a.slice(0,c+1):[a[0]];break}}}let l=n&&r?(e,t)=>{r(e,{location:n.location,params:n.matches?.[0]?.params??{},errorInfo:t})}:void 0;return a.reduceRight((e,r,i)=>{let u,d=!1,f=null,p=null;n&&(u=o&&r.route.id?o[r.route.id]:void 0,f=r.route.errorElement||Ye,s&&(c<0&&i===0?(ut(`route-fallback`,!1,"No `HydrateFallback` element provided to render during initial hydration"),d=!0,p=null):c===i&&(d=!0,p=r.route.hydrateFallbackElement||null)));let m=t.concat(a.slice(0,i+1)),h=()=>{let t;return t=u?f:d?p:r.route.Component?x.createElement(r.route.Component,null):r.route.element?r.route.element:e,x.createElement(Ze,{match:r,routeContext:{outlet:e,matches:m,isDataRoute:n!=null},children:t})};return n&&(r.route.ErrorBoundary||r.route.errorElement||i===0)?x.createElement(Xe,{location:n.location,revalidation:n.revalidation,component:f,error:u,children:h(),routeContext:{outlet:null,matches:m,isDataRoute:!0},onError:l}):h()},null)}function $e(e){return`${e} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`}function et(e){let t=x.useContext(ke);return w(t,$e(e)),t}function tt(e){let t=x.useContext(Ae);return w(t,$e(e)),t}function nt(e){let t=x.useContext(Ie);return w(t,$e(e)),t}function rt(e){let t=nt(e),n=t.matches[t.matches.length-1];return w(n.route.id,`${e} can only be used on routes that contain a unique "id"`),n.route.id}function it(){return rt(`useRouteId`)}function at(){return tt(`useNavigation`).navigation}function ot(){let{matches:e,loaderData:t}=tt(`useMatches`);return x.useMemo(()=>e.map(e=>N(e,t)),[e,t])}function st(){let e=x.useContext(Le),t=tt(`useRouteError`),n=rt(`useRouteError`);return e===void 0?t.errors?.[n]:e}function ct(){let{router:e}=et(`useNavigate`),t=rt(`useNavigate`),n=x.useRef(!1);return He(()=>{n.current=!0}),x.useCallback(async(r,i={})=>{T(n.current,Ve),n.current&&(typeof r==`number`?e.navigate(r):await e.navigate(r,{fromRouteId:t,...i}))},[e,t])}var lt={};function ut(e,t,n){!t&&!lt[e]&&(lt[e]=!0,T(!1,n))}x.memo(dt);function dt({routes:e,future:t,state:n,unstable_onError:r}){return qe(e,void 0,n,r,t)}function ft({to:e,replace:t,state:n,relative:r}){w(ze(),`<Navigate> may be used only in the context of a <Router> component.`);let{static:i}=x.useContext(Pe);T(!i,`<Navigate> must not be used on the initial render in a <StaticRouter>. This is a no-op, but you should modify your code so the <Navigate> is only ever rendered in response to some user interaction or state change.`);let{matches:a}=x.useContext(Ie),{pathname:o}=Be(),s=Ue(),c=Ce(e,Se(a),o,r===`path`),l=JSON.stringify(c);return x.useEffect(()=>{s(JSON.parse(l),{replace:t,state:n,relative:r})},[s,l,r,t,n]),null}function pt(e){w(!1,`A <Route> is only ever to be used as the child of <Routes> element, never rendered directly. Please wrap your <Route> in a <Routes>.`)}function mt({basename:e=`/`,children:t=null,location:n,navigationType:r=`POP`,navigator:i,static:a=!1}){w(!ze(),`You cannot render a <Router> inside another <Router>. You should never have more than one in your app.`);let o=e.replace(/^\/*/,`/`),s=x.useMemo(()=>({basename:o,navigator:i,static:a,future:{}}),[o,i,a]);typeof n==`string`&&(n=ee(n));let{pathname:c=`/`,search:l=``,hash:u=``,state:d=null,key:f=`default`}=n,p=x.useMemo(()=>{let e=he(c,o);return e==null?null:{location:{pathname:e,search:l,hash:u,state:d,key:f},navigationType:r}},[o,c,l,u,d,f,r]);return T(p!=null,`<Router basename="${o}"> is not able to match the URL "${c}${l}${u}" because it does not start with the basename, so the <Router> won't render anything.`),p==null?null:x.createElement(Pe.Provider,{value:s},x.createElement(Fe.Provider,{children:t,value:p}))}function ht({children:e,location:t}){return Ke(gt(e),t)}function gt(e,t=[]){let n=[];return x.Children.forEach(e,(e,r)=>{if(!x.isValidElement(e))return;let i=[...t,r];if(e.type===x.Fragment){n.push.apply(n,gt(e.props.children,i));return}w(e.type===pt,`[${typeof e.type==`string`?e.type:e.type.name}] is not a <Route> component. All component children of <Routes> must be a <Route> or <React.Fragment>`),w(!e.props.index||!e.props.children,`An index route cannot have child routes.`);let a={id:e.props.id||i.join(`-`),caseSensitive:e.props.caseSensitive,element:e.props.element,Component:e.props.Component,index:e.props.index,path:e.props.path,middleware:e.props.middleware,loader:e.props.loader,action:e.props.action,hydrateFallbackElement:e.props.hydrateFallbackElement,HydrateFallback:e.props.HydrateFallback,errorElement:e.props.errorElement,ErrorBoundary:e.props.ErrorBoundary,hasErrorBoundary:e.props.hasErrorBoundary===!0||e.props.ErrorBoundary!=null||e.props.errorElement!=null,shouldRevalidate:e.props.shouldRevalidate,handle:e.props.handle,lazy:e.props.lazy};e.props.children&&(a.children=gt(e.props.children,i)),n.push(a)}),n}var _t=`get`,vt=`application/x-www-form-urlencoded`;function yt(e){return e!=null&&typeof e.tagName==`string`}function bt(e){return yt(e)&&e.tagName.toLowerCase()===`button`}function xt(e){return yt(e)&&e.tagName.toLowerCase()===`form`}function St(e){return yt(e)&&e.tagName.toLowerCase()===`input`}function Ct(e){return!!(e.metaKey||e.altKey||e.ctrlKey||e.shiftKey)}function wt(e,t){return e.button===0&&(!t||t===`_self`)&&!Ct(e)}var Tt=null;function Et(){if(Tt===null)try{new FormData(document.createElement(`form`),0),Tt=!1}catch{Tt=!0}return Tt}var Dt=new Set([`application/x-www-form-urlencoded`,`multipart/form-data`,`text/plain`]);function Ot(e){return e!=null&&!Dt.has(e)?(T(!1,`"${e}" is not a valid \`encType\` for \`<Form>\`/\`<fetcher.Form>\` and will default to "${vt}"`),null):e}function kt(e,t){let n,r,i,a,o;if(xt(e)){let o=e.getAttribute(`action`);r=o?he(o,t):null,n=e.getAttribute(`method`)||_t,i=Ot(e.getAttribute(`enctype`))||vt,a=new FormData(e)}else if(bt(e)||St(e)&&(e.type===`submit`||e.type===`image`)){let o=e.form;if(o==null)throw Error(`Cannot submit a <button> or <input type="submit"> without a <form>`);let s=e.getAttribute(`formaction`)||o.getAttribute(`action`);if(r=s?he(s,t):null,n=e.getAttribute(`formmethod`)||o.getAttribute(`method`)||_t,i=Ot(e.getAttribute(`formenctype`))||Ot(o.getAttribute(`enctype`))||vt,a=new FormData(o,e),!Et()){let{name:t,type:n,value:r}=e;if(n===`image`){let e=t?`${t}.`:``;a.append(`${e}x`,`0`),a.append(`${e}y`,`0`)}else t&&a.append(t,r)}}else if(yt(e))throw Error(`Cannot submit element that is not <form>, <button>, or <input type="submit|image">`);else n=_t,r=null,i=vt,o=e;return a&&i===`text/plain`&&(o=a,a=void 0),{action:r,method:n.toLowerCase(),encType:i,formData:a,body:o}}Object.getOwnPropertyNames(Object.prototype).sort().join(`\0`);function At(e,t){if(e===!1||e==null)throw Error(t)}function jt(e,t,n){let r=typeof e==`string`?new URL(e,typeof window>`u`?`server://singlefetch/`:window.location.origin):e;return r.pathname===`/`?r.pathname=`_root.${n}`:t&&he(r.pathname,t)===`/`?r.pathname=`${t.replace(/\/$/,``)}/_root.${n}`:r.pathname=`${r.pathname.replace(/\/$/,``)}.${n}`,r}async function Mt(e,t){if(e.id in t)return t[e.id];try{let n=await b(()=>import(e.module),[]);return t[e.id]=n,n}catch(t){return console.error(`Error loading route module \`${e.module}\`, reloading page...`),console.error(t),window.__reactRouterContext&&window.__reactRouterContext.isSpaMode,window.location.reload(),new Promise(()=>{})}}function Nt(e){return e!=null&&typeof e.page==`string`}function Pt(e){return e==null?!1:e.href==null?e.rel===`preload`&&typeof e.imageSrcSet==`string`&&typeof e.imageSizes==`string`:typeof e.rel==`string`&&typeof e.href==`string`}async function Ft(e,t,n){return Bt((await Promise.all(e.map(async e=>{let r=t.routes[e.route.id];if(r){let e=await Mt(r,n);return e.links?e.links():[]}return[]}))).flat(1).filter(Pt).filter(e=>e.rel===`stylesheet`||e.rel===`preload`).map(e=>e.rel===`stylesheet`?{...e,rel:`prefetch`,as:`style`}:{...e,rel:`prefetch`}))}function It(e,t,n,r,i,a){let o=(e,t)=>n[t]?e.route.id!==n[t].route.id:!0,s=(e,t)=>n[t].pathname!==e.pathname||n[t].route.path?.endsWith(`*`)&&n[t].params[`*`]!==e.params[`*`];return a===`assets`?t.filter((e,t)=>o(e,t)||s(e,t)):a===`data`?t.filter((t,a)=>{let c=r.routes[t.route.id];if(!c||!c.hasLoader)return!1;if(o(t,a)||s(t,a))return!0;if(t.route.shouldRevalidate){let r=t.route.shouldRevalidate({currentUrl:new URL(i.pathname+i.search+i.hash,window.origin),currentParams:n[0]?.params||{},nextUrl:new URL(e,window.origin),nextParams:t.params,defaultShouldRevalidate:!0});if(typeof r==`boolean`)return r}return!0}):[]}function Lt(e,t,{includeHydrateFallback:n}={}){return Rt(e.map(e=>{let r=t.routes[e.route.id];if(!r)return[];let i=[r.module];return r.clientActionModule&&(i=i.concat(r.clientActionModule)),r.clientLoaderModule&&(i=i.concat(r.clientLoaderModule)),n&&r.hydrateFallbackModule&&(i=i.concat(r.hydrateFallbackModule)),r.imports&&(i=i.concat(r.imports)),i}).flat(1))}function Rt(e){return[...new Set(e)]}function zt(e){let t={},n=Object.keys(e).sort();for(let r of n)t[r]=e[r];return t}function Bt(e,t){let n=new Set,r=new Set(t);return e.reduce((e,i)=>{if(t&&!Nt(i)&&i.as===`script`&&i.href&&r.has(i.href))return e;let a=JSON.stringify(zt(i));return n.has(a)||(n.add(a),e.push({key:a,link:i})),e},[])}function Vt(){let e=x.useContext(ke);return At(e,`You must render this element inside a <DataRouterContext.Provider> element`),e}function Ht(){let e=x.useContext(Ae);return At(e,`You must render this element inside a <DataRouterStateContext.Provider> element`),e}var Ut=x.createContext(void 0);Ut.displayName=`FrameworkContext`;function Wt(){let e=x.useContext(Ut);return At(e,`You must render this element inside a <HydratedRouter> element`),e}function Gt(e,t){let n=x.useContext(Ut),[r,i]=x.useState(!1),[a,o]=x.useState(!1),{onFocus:s,onBlur:c,onMouseEnter:l,onMouseLeave:u,onTouchStart:d}=t,f=x.useRef(null);x.useEffect(()=>{if(e===`render`&&o(!0),e===`viewport`){let e=new IntersectionObserver(e=>{e.forEach(e=>{o(e.isIntersecting)})},{threshold:.5});return f.current&&e.observe(f.current),()=>{e.disconnect()}}},[e]),x.useEffect(()=>{if(r){let e=setTimeout(()=>{o(!0)},100);return()=>{clearTimeout(e)}}},[r]);let p=()=>{i(!0)},m=()=>{i(!1),o(!1)};return n?e===`intent`?[a,f,{onFocus:Kt(s,p),onBlur:Kt(c,m),onMouseEnter:Kt(l,p),onMouseLeave:Kt(u,m),onTouchStart:Kt(d,p)}]:[a,f,{}]:[!1,f,{}]}function Kt(e,t){return n=>{e&&e(n),n.defaultPrevented||t(n)}}function qt({page:e,...t}){let{router:n}=Vt(),r=x.useMemo(()=>M(n.routes,e,n.basename),[n.routes,e,n.basename]);return r?x.createElement(Yt,{page:e,matches:r,...t}):null}function Jt(e){let{manifest:t,routeModules:n}=Wt(),[r,i]=x.useState([]);return x.useEffect(()=>{let r=!1;return Ft(e,t,n).then(e=>{r||i(e)}),()=>{r=!0}},[e,t,n]),r}function Yt({page:e,matches:t,...n}){let r=Be(),{manifest:i,routeModules:a}=Wt(),{basename:o}=Vt(),{loaderData:s,matches:c}=Ht(),l=x.useMemo(()=>It(e,t,c,i,r,`data`),[e,t,c,i,r]),u=x.useMemo(()=>It(e,t,c,i,r,`assets`),[e,t,c,i,r]),d=x.useMemo(()=>{if(e===r.pathname+r.search+r.hash)return[];let n=new Set,c=!1;if(t.forEach(e=>{let t=i.routes[e.route.id];!t||!t.hasLoader||(!l.some(t=>t.route.id===e.route.id)&&e.route.id in s&&a[e.route.id]?.shouldRevalidate||t.hasClientLoader?c=!0:n.add(e.route.id))}),n.size===0)return[];let u=jt(e,o,`data`);return c&&n.size>0&&u.searchParams.set(`_routes`,t.filter(e=>n.has(e.route.id)).map(e=>e.route.id).join(`,`)),[u.pathname+u.search]},[o,s,r,i,l,t,e,a]),f=x.useMemo(()=>Lt(u,i),[u,i]),p=Jt(u);return x.createElement(x.Fragment,null,d.map(e=>x.createElement(`link`,{key:e,rel:`prefetch`,as:`fetch`,href:e,...n})),f.map(e=>x.createElement(`link`,{key:e,rel:`modulepreload`,href:e,...n})),p.map(({key:e,link:t})=>x.createElement(`link`,{key:e,nonce:n.nonce,...t})))}function Xt(...e){return t=>{e.forEach(e=>{typeof e==`function`?e(t):e!=null&&(e.current=t)})}}var Zt=typeof window<`u`&&window.document!==void 0&&window.document.createElement!==void 0;try{Zt&&(window.__reactRouterVersion=`7.9.6`)}catch{}function Qt({basename:e,children:t,window:n}){let r=x.useRef();r.current??=C({window:n,v5Compat:!0});let i=r.current,[a,o]=x.useState({action:i.action,location:i.location}),s=x.useCallback(e=>{x.startTransition(()=>o(e))},[o]);return x.useLayoutEffect(()=>i.listen(s),[i,s]),x.createElement(mt,{basename:e,children:t,location:a.location,navigationType:a.action,navigator:i})}function $t({basename:e,children:t,history:n}){let[r,i]=x.useState({action:n.action,location:n.location}),a=x.useCallback(e=>{x.startTransition(()=>i(e))},[i]);return x.useLayoutEffect(()=>n.listen(a),[n,a]),x.createElement(mt,{basename:e,children:t,location:r.location,navigationType:r.action,navigator:n})}$t.displayName=`unstable_HistoryRouter`;var en=/^(?:[a-z][a-z0-9+.-]*:|\/\/)/i,tn=x.forwardRef(function({onClick:e,discover:t=`render`,prefetch:n=`none`,relative:r,reloadDocument:i,replace:a,state:o,target:s,to:c,preventScrollReset:l,viewTransition:u,...d},f){let{basename:p}=x.useContext(Pe),m=typeof c==`string`&&en.test(c),h,g=!1;if(typeof c==`string`&&m&&(h=c,Zt))try{let e=new URL(window.location.href),t=c.startsWith(`//`)?new URL(e.protocol+c):new URL(c),n=he(t.pathname,p);t.origin===e.origin&&n!=null?c=n+t.search+t.hash:g=!0}catch{T(!1,`<Link to="${c}"> contains an invalid URL which will probably break when clicked - please update to a valid URL path.`)}let _=Re(c,{relative:r}),[v,y,b]=Gt(n,d),S=ln(c,{replace:a,state:o,target:s,preventScrollReset:l,relative:r,viewTransition:u});function C(t){e&&e(t),t.defaultPrevented||S(t)}let w=x.createElement(`a`,{...d,...b,href:h||_,onClick:g||i?e:C,ref:Xt(f,y),target:s,"data-discover":!m&&t===`render`?`true`:void 0});return v&&!m?x.createElement(x.Fragment,null,w,x.createElement(qt,{page:_})):w});tn.displayName=`Link`;var nn=x.forwardRef(function({"aria-current":e=`page`,caseSensitive:t=!1,className:n=``,end:r=!1,style:i,to:a,viewTransition:o,children:s,...c},l){let u=Ge(a,{relative:c.relative}),d=Be(),f=x.useContext(Ae),{navigator:p,basename:m}=x.useContext(Pe),h=f!=null&&yn(u)&&o===!0,g=p.encodeLocation?p.encodeLocation(u).pathname:u.pathname,_=d.pathname,v=f&&f.navigation&&f.navigation.location?f.navigation.location.pathname:null;t||(_=_.toLowerCase(),v=v?v.toLowerCase():null,g=g.toLowerCase()),v&&m&&(v=he(v,m)||v);let y=g!==`/`&&g.endsWith(`/`)?g.length-1:g.length,b=_===g||!r&&_.startsWith(g)&&_.charAt(y)===`/`,S=v!=null&&(v===g||!r&&v.startsWith(g)&&v.charAt(g.length)===`/`),C={isActive:b,isPending:S,isTransitioning:h},w=b?e:void 0,T;T=typeof n==`function`?n(C):[n,b?`active`:null,S?`pending`:null,h?`transitioning`:null].filter(Boolean).join(` `);let E=typeof i==`function`?i(C):i;return x.createElement(tn,{...c,"aria-current":w,className:T,ref:l,style:E,to:a,viewTransition:o},typeof s==`function`?s(C):s)});nn.displayName=`NavLink`;var rn=x.forwardRef(({discover:e=`render`,fetcherKey:t,navigate:n,reloadDocument:r,replace:i,state:a,method:o=_t,action:s,onSubmit:c,relative:l,preventScrollReset:u,viewTransition:d,...f},p)=>{let m=fn(),h=pn(s,{relative:l}),g=o.toLowerCase()===`get`?`get`:`post`,_=typeof s==`string`&&en.test(s);return x.createElement(`form`,{ref:p,method:g,action:h,onSubmit:r?c:e=>{if(c&&c(e),e.defaultPrevented)return;e.preventDefault();let r=e.nativeEvent.submitter,s=r?.getAttribute(`formmethod`)||o;m(r||e.currentTarget,{fetcherKey:t,method:s,navigate:n,replace:i,state:a,relative:l,preventScrollReset:u,viewTransition:d})},...f,"data-discover":!_&&e===`render`?`true`:void 0})});rn.displayName=`Form`;function an({getKey:e,storageKey:t,...n}){let r=x.useContext(Ut),{basename:i}=x.useContext(Pe),a=Be(),o=ot();_n({getKey:e,storageKey:t});let s=x.useMemo(()=>{if(!r||!e)return null;let t=gn(a,o,i,e);return t===a.key?null:t},[]);if(!r||r.isSpaMode)return null;let c=((e,t)=>{if(!window.history.state||!window.history.state.key){let e=Math.random().toString(32).slice(2);window.history.replaceState({key:e},``)}try{let n=JSON.parse(sessionStorage.getItem(e)||`{}`)[t||window.history.state.key];typeof n==`number`&&window.scrollTo(0,n)}catch(t){console.error(t),sessionStorage.removeItem(e)}}).toString();return x.createElement(`script`,{...n,suppressHydrationWarning:!0,dangerouslySetInnerHTML:{__html:`(${c})(${JSON.stringify(t||mn)}, ${JSON.stringify(s)})`}})}an.displayName=`ScrollRestoration`;function on(e){return`${e} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`}function sn(e){let t=x.useContext(ke);return w(t,on(e)),t}function cn(e){let t=x.useContext(Ae);return w(t,on(e)),t}function ln(e,{target:t,replace:n,state:r,preventScrollReset:i,relative:a,viewTransition:o}={}){let s=Ue(),c=Be(),l=Ge(e,{relative:a});return x.useCallback(u=>{wt(u,t)&&(u.preventDefault(),s(e,{replace:n===void 0?k(c)===k(l):n,state:r,preventScrollReset:i,relative:a,viewTransition:o}))},[c,s,l,n,r,t,e,i,a,o])}var un=0,dn=()=>`__${String(++un)}__`;function fn(){let{router:e}=sn(`useSubmit`),{basename:t}=x.useContext(Pe),n=it();return x.useCallback(async(r,i={})=>{let{action:a,method:o,encType:s,formData:c,body:l}=kt(r,t);if(i.navigate===!1){let t=i.fetcherKey||dn();await e.fetch(t,n,i.action||a,{preventScrollReset:i.preventScrollReset,formData:c,body:l,formMethod:i.method||o,formEncType:i.encType||s,flushSync:i.flushSync})}else await e.navigate(i.action||a,{preventScrollReset:i.preventScrollReset,formData:c,body:l,formMethod:i.method||o,formEncType:i.encType||s,replace:i.replace,state:i.state,fromRouteId:n,flushSync:i.flushSync,viewTransition:i.viewTransition})},[e,t,n])}function pn(e,{relative:t}={}){let{basename:n}=x.useContext(Pe),r=x.useContext(Ie);w(r,`useFormAction must be used inside a RouteContext`);let[i]=r.matches.slice(-1),a={...Ge(e||`.`,{relative:t})},o=Be();if(e==null){a.search=o.search;let e=new URLSearchParams(a.search),t=e.getAll(`index`);if(t.some(e=>e===``)){e.delete(`index`),t.filter(e=>e).forEach(t=>e.append(`index`,t));let n=e.toString();a.search=n?`?${n}`:``}}return(!e||e===`.`)&&i.route.index&&(a.search=a.search?a.search.replace(/^\?/,`?index&`):`?index`),n!==`/`&&(a.pathname=a.pathname===`/`?n:we([n,a.pathname])),k(a)}var mn=`react-router-scroll-positions`,hn={};function gn(e,t,n,r){let i=null;return r&&(i=r(n===`/`?e:{...e,pathname:he(e.pathname,n)||e.pathname},t)),i??=e.key,i}function _n({getKey:e,storageKey:t}={}){let{router:n}=sn(`useScrollRestoration`),{restoreScrollPosition:r,preventScrollReset:i}=cn(`useScrollRestoration`),{basename:a}=x.useContext(Pe),o=Be(),s=ot(),c=at();x.useEffect(()=>(window.history.scrollRestoration=`manual`,()=>{window.history.scrollRestoration=`auto`}),[]),vn(x.useCallback(()=>{if(c.state===`idle`){let t=gn(o,s,a,e);hn[t]=window.scrollY}try{sessionStorage.setItem(t||mn,JSON.stringify(hn))}catch(e){T(!1,`Failed to save scroll positions in sessionStorage, <ScrollRestoration /> will not work properly (${e}).`)}window.history.scrollRestoration=`auto`},[c.state,e,a,o,s,t])),typeof document<`u`&&(x.useLayoutEffect(()=>{try{let e=sessionStorage.getItem(t||mn);e&&(hn=JSON.parse(e))}catch{}},[t]),x.useLayoutEffect(()=>{let t=n?.enableScrollRestoration(hn,()=>window.scrollY,e?(t,n)=>gn(t,n,a,e):void 0);return()=>t&&t()},[n,a,e]),x.useLayoutEffect(()=>{if(r!==!1){if(typeof r==`number`){window.scrollTo(0,r);return}try{if(o.hash){let e=document.getElementById(decodeURIComponent(o.hash.slice(1)));if(e){e.scrollIntoView();return}}}catch{T(!1,`"${o.hash.slice(1)}" is not a decodable element ID. The view will not scroll to it.`)}i!==!0&&window.scrollTo(0,0)}},[o,r,i]))}function vn(e,t){let{capture:n}=t||{};x.useEffect(()=>{let t=n==null?void 0:{capture:n};return window.addEventListener(`pagehide`,e,t),()=>{window.removeEventListener(`pagehide`,e,t)}},[e,n])}function yn(e,{relative:t}={}){let n=x.useContext(je);w(n!=null,"`useViewTransitionState` must be used within `react-router-dom`'s `RouterProvider`.  Did you accidentally import `RouterProvider` from `react-router`?");let{basename:r}=sn(`useViewTransitionState`),i=Ge(e,{relative:t});if(!n.isTransitioning)return!1;let a=he(n.currentLocation.pathname,r)||n.currentLocation.pathname,o=he(n.nextLocation.pathname,r)||n.nextLocation.pathname;return fe(i.pathname,o)!=null||fe(i.pathname,a)!=null}var bn={black:`#000`,white:`#fff`},xn={50:`#ffebee`,100:`#ffcdd2`,200:`#ef9a9a`,300:`#e57373`,400:`#ef5350`,500:`#f44336`,600:`#e53935`,700:`#d32f2f`,800:`#c62828`,900:`#b71c1c`,A100:`#ff8a80`,A200:`#ff5252`,A400:`#ff1744`,A700:`#d50000`},Sn={50:`#f3e5f5`,100:`#e1bee7`,200:`#ce93d8`,300:`#ba68c8`,400:`#ab47bc`,500:`#9c27b0`,600:`#8e24aa`,700:`#7b1fa2`,800:`#6a1b9a`,900:`#4a148c`,A100:`#ea80fc`,A200:`#e040fb`,A400:`#d500f9`,A700:`#aa00ff`},Cn={50:`#e3f2fd`,100:`#bbdefb`,200:`#90caf9`,300:`#64b5f6`,400:`#42a5f5`,500:`#2196f3`,600:`#1e88e5`,700:`#1976d2`,800:`#1565c0`,900:`#0d47a1`,A100:`#82b1ff`,A200:`#448aff`,A400:`#2979ff`,A700:`#2962ff`},wn={50:`#e1f5fe`,100:`#b3e5fc`,200:`#81d4fa`,300:`#4fc3f7`,400:`#29b6f6`,500:`#03a9f4`,600:`#039be5`,700:`#0288d1`,800:`#0277bd`,900:`#01579b`,A100:`#80d8ff`,A200:`#40c4ff`,A400:`#00b0ff`,A700:`#0091ea`},Tn={50:`#e8f5e9`,100:`#c8e6c9`,200:`#a5d6a7`,300:`#81c784`,400:`#66bb6a`,500:`#4caf50`,600:`#43a047`,700:`#388e3c`,800:`#2e7d32`,900:`#1b5e20`,A100:`#b9f6ca`,A200:`#69f0ae`,A400:`#00e676`,A700:`#00c853`},En={50:`#fff3e0`,100:`#ffe0b2`,200:`#ffcc80`,300:`#ffb74d`,400:`#ffa726`,500:`#ff9800`,600:`#fb8c00`,700:`#f57c00`,800:`#ef6c00`,900:`#e65100`,A100:`#ffd180`,A200:`#ffab40`,A400:`#ff9100`,A700:`#ff6d00`},Dn={50:`#fafafa`,100:`#f5f5f5`,200:`#eeeeee`,300:`#e0e0e0`,400:`#bdbdbd`,500:`#9e9e9e`,600:`#757575`,700:`#616161`,800:`#424242`,900:`#212121`,A100:`#f5f5f5`,A200:`#eeeeee`,A400:`#bdbdbd`,A700:`#616161`};function On(e,...t){let n=new URL(`https://mui.com/production-error/?code=${e}`);return t.forEach(e=>n.searchParams.append(`args[]`,e)),`Minified MUI error #${e}; visit ${n} for the full message.`}var kn=`$$material`;function An(){return An=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)({}).hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e},An.apply(null,arguments)}var jn=!1;function Mn(e){if(e.sheet)return e.sheet;for(var t=0;t<document.styleSheets.length;t++)if(document.styleSheets[t].ownerNode===e)return document.styleSheets[t]}function Nn(e){var t=document.createElement(`style`);return t.setAttribute(`data-emotion`,e.key),e.nonce!==void 0&&t.setAttribute(`nonce`,e.nonce),t.appendChild(document.createTextNode(``)),t.setAttribute(`data-s`,``),t}var Pn=function(){function e(e){var t=this;this._insertTag=function(e){var n=t.tags.length===0?t.insertionPoint?t.insertionPoint.nextSibling:t.prepend?t.container.firstChild:t.before:t.tags[t.tags.length-1].nextSibling;t.container.insertBefore(e,n),t.tags.push(e)},this.isSpeedy=e.speedy===void 0?!jn:e.speedy,this.tags=[],this.ctr=0,this.nonce=e.nonce,this.key=e.key,this.container=e.container,this.prepend=e.prepend,this.insertionPoint=e.insertionPoint,this.before=null}var t=e.prototype;return t.hydrate=function(e){e.forEach(this._insertTag)},t.insert=function(e){this.ctr%(this.isSpeedy?65e3:1)==0&&this._insertTag(Nn(this));var t=this.tags[this.tags.length-1];if(this.isSpeedy){var n=Mn(t);try{n.insertRule(e,n.cssRules.length)}catch{}}else t.appendChild(document.createTextNode(e));this.ctr++},t.flush=function(){this.tags.forEach(function(e){return e.parentNode?.removeChild(e)}),this.tags=[],this.ctr=0},e}(),Fn=`-ms-`,In=`-moz-`,Ln=`-webkit-`,Rn=`comm`,zn=`rule`,Bn=`decl`,Vn=`@import`,Hn=`@keyframes`,Un=`@layer`,Wn=Math.abs,Gn=String.fromCharCode,Kn=Object.assign;function qn(e,t){return Qn(e,0)^45?(((t<<2^Qn(e,0))<<2^Qn(e,1))<<2^Qn(e,2))<<2^Qn(e,3):0}function Jn(e){return e.trim()}function Yn(e,t){return(e=t.exec(e))?e[0]:e}function Xn(e,t,n){return e.replace(t,n)}function Zn(e,t){return e.indexOf(t)}function Qn(e,t){return e.charCodeAt(t)|0}function $n(e,t,n){return e.slice(t,n)}function er(e){return e.length}function tr(e){return e.length}function nr(e,t){return t.push(e),e}function rr(e,t){return e.map(t).join(``)}var ir=1,ar=1,or=0,sr=0,cr=0,lr=``;function ur(e,t,n,r,i,a,o){return{value:e,root:t,parent:n,type:r,props:i,children:a,line:ir,column:ar,length:o,return:``}}function dr(e,t){return Kn(ur(``,null,null,``,null,null,0),e,{length:-e.length},t)}function fr(){return cr}function pr(){return cr=sr>0?Qn(lr,--sr):0,ar--,cr===10&&(ar=1,ir--),cr}function mr(){return cr=sr<or?Qn(lr,sr++):0,ar++,cr===10&&(ar=1,ir++),cr}function hr(){return Qn(lr,sr)}function gr(){return sr}function _r(e,t){return $n(lr,e,t)}function vr(e){switch(e){case 0:case 9:case 10:case 13:case 32:return 5;case 33:case 43:case 44:case 47:case 62:case 64:case 126:case 59:case 123:case 125:return 4;case 58:return 3;case 34:case 39:case 40:case 91:return 2;case 41:case 93:return 1}return 0}function yr(e){return ir=ar=1,or=er(lr=e),sr=0,[]}function br(e){return lr=``,e}function xr(e){return Jn(_r(sr-1,wr(e===91?e+2:e===40?e+1:e)))}function Sr(e){for(;(cr=hr())&&cr<33;)mr();return vr(e)>2||vr(cr)>3?``:` `}function Cr(e,t){for(;--t&&mr()&&!(cr<48||cr>102||cr>57&&cr<65||cr>70&&cr<97););return _r(e,gr()+(t<6&&hr()==32&&mr()==32))}function wr(e){for(;mr();)switch(cr){case e:return sr;case 34:case 39:e!==34&&e!==39&&wr(cr);break;case 40:e===41&&wr(e);break;case 92:mr();break}return sr}function Tr(e,t){for(;mr()&&e+cr!==57&&!(e+cr===84&&hr()===47););return`/*`+_r(t,sr-1)+`*`+Gn(e===47?e:mr())}function Er(e){for(;!vr(hr());)mr();return _r(e,sr)}function Dr(e){return br(Or(``,null,null,null,[``],e=yr(e),0,[0],e))}function Or(e,t,n,r,i,a,o,s,c){for(var l=0,u=0,d=o,f=0,p=0,m=0,h=1,g=1,_=1,v=0,y=``,b=i,x=a,S=r,C=y;g;)switch(m=v,v=mr()){case 40:if(m!=108&&Qn(C,d-1)==58){Zn(C+=Xn(xr(v),`&`,`&\f`),`&\f`)!=-1&&(_=-1);break}case 34:case 39:case 91:C+=xr(v);break;case 9:case 10:case 13:case 32:C+=Sr(m);break;case 92:C+=Cr(gr()-1,7);continue;case 47:switch(hr()){case 42:case 47:nr(Ar(Tr(mr(),gr()),t,n),c);break;default:C+=`/`}break;case 123*h:s[l++]=er(C)*_;case 125*h:case 59:case 0:switch(v){case 0:case 125:g=0;case 59+u:_==-1&&(C=Xn(C,/\f/g,``)),p>0&&er(C)-d&&nr(p>32?jr(C+`;`,r,n,d-1):jr(Xn(C,` `,``)+`;`,r,n,d-2),c);break;case 59:C+=`;`;default:if(nr(S=kr(C,t,n,l,u,i,s,y,b=[],x=[],d),a),v===123)if(u===0)Or(C,t,S,S,b,a,d,s,x);else switch(f===99&&Qn(C,3)===110?100:f){case 100:case 108:case 109:case 115:Or(e,S,S,r&&nr(kr(e,S,S,0,0,i,s,y,i,b=[],d),x),i,x,d,s,r?b:x);break;default:Or(C,S,S,S,[``],x,0,s,x)}}l=u=p=0,h=_=1,y=C=``,d=o;break;case 58:d=1+er(C),p=m;default:if(h<1){if(v==123)--h;else if(v==125&&h++==0&&pr()==125)continue}switch(C+=Gn(v),v*h){case 38:_=u>0?1:(C+=`\f`,-1);break;case 44:s[l++]=(er(C)-1)*_,_=1;break;case 64:hr()===45&&(C+=xr(mr())),f=hr(),u=d=er(y=C+=Er(gr())),v++;break;case 45:m===45&&er(C)==2&&(h=0)}}return a}function kr(e,t,n,r,i,a,o,s,c,l,u){for(var d=i-1,f=i===0?a:[``],p=tr(f),m=0,h=0,g=0;m<r;++m)for(var _=0,v=$n(e,d+1,d=Wn(h=o[m])),y=e;_<p;++_)(y=Jn(h>0?f[_]+` `+v:Xn(v,/&\f/g,f[_])))&&(c[g++]=y);return ur(e,t,n,i===0?zn:s,c,l,u)}function Ar(e,t,n){return ur(e,t,n,Rn,Gn(fr()),$n(e,2,-2),0)}function jr(e,t,n,r){return ur(e,t,n,Bn,$n(e,0,r),$n(e,r+1,-1),r)}function Mr(e,t){for(var n=``,r=tr(e),i=0;i<r;i++)n+=t(e[i],i,e,t)||``;return n}function Nr(e,t,n,r){switch(e.type){case Un:if(e.children.length)break;case Vn:case Bn:return e.return=e.return||e.value;case Rn:return``;case Hn:return e.return=e.value+`{`+Mr(e.children,r)+`}`;case zn:e.value=e.props.join(`,`)}return er(n=Mr(e.children,r))?e.return=e.value+`{`+n+`}`:``}function Pr(e){var t=tr(e);return function(n,r,i,a){for(var o=``,s=0;s<t;s++)o+=e[s](n,r,i,a)||``;return o}}function Fr(e){return function(t){t.root||(t=t.return)&&e(t)}}function Ir(e){var t=Object.create(null);return function(n){return t[n]===void 0&&(t[n]=e(n)),t[n]}}var Lr=function(e,t,n){for(var r=0,i=0;r=i,i=hr(),r===38&&i===12&&(t[n]=1),!vr(i);)mr();return _r(e,sr)},Rr=function(e,t){var n=-1,r=44;do switch(vr(r)){case 0:r===38&&hr()===12&&(t[n]=1),e[n]+=Lr(sr-1,t,n);break;case 2:e[n]+=xr(r);break;case 4:if(r===44){e[++n]=hr()===58?`&\f`:``,t[n]=e[n].length;break}default:e[n]+=Gn(r)}while(r=mr());return e},zr=function(e,t){return br(Rr(yr(e),t))},Br=new WeakMap,Vr=function(e){if(!(e.type!==`rule`||!e.parent||e.length<1)){for(var t=e.value,n=e.parent,r=e.column===n.column&&e.line===n.line;n.type!==`rule`;)if(n=n.parent,!n)return;if(!(e.props.length===1&&t.charCodeAt(0)!==58&&!Br.get(n))&&!r){Br.set(e,!0);for(var i=[],a=zr(t,i),o=n.props,s=0,c=0;s<a.length;s++)for(var l=0;l<o.length;l++,c++)e.props[c]=i[s]?a[s].replace(/&\f/g,o[l]):o[l]+` `+a[s]}}},Hr=function(e){if(e.type===`decl`){var t=e.value;t.charCodeAt(0)===108&&t.charCodeAt(2)===98&&(e.return=``,e.value=``)}};function Ur(e,t){switch(qn(e,t)){case 5103:return Ln+`print-`+e+e;case 5737:case 4201:case 3177:case 3433:case 1641:case 4457:case 2921:case 5572:case 6356:case 5844:case 3191:case 6645:case 3005:case 6391:case 5879:case 5623:case 6135:case 4599:case 4855:case 4215:case 6389:case 5109:case 5365:case 5621:case 3829:return Ln+e+e;case 5349:case 4246:case 4810:case 6968:case 2756:return Ln+e+In+e+Fn+e+e;case 6828:case 4268:return Ln+e+Fn+e+e;case 6165:return Ln+e+Fn+`flex-`+e+e;case 5187:return Ln+e+Xn(e,/(\w+).+(:[^]+)/,Ln+`box-$1$2`+Fn+`flex-$1$2`)+e;case 5443:return Ln+e+Fn+`flex-item-`+Xn(e,/flex-|-self/,``)+e;case 4675:return Ln+e+Fn+`flex-line-pack`+Xn(e,/align-content|flex-|-self/,``)+e;case 5548:return Ln+e+Fn+Xn(e,`shrink`,`negative`)+e;case 5292:return Ln+e+Fn+Xn(e,`basis`,`preferred-size`)+e;case 6060:return Ln+`box-`+Xn(e,`-grow`,``)+Ln+e+Fn+Xn(e,`grow`,`positive`)+e;case 4554:return Ln+Xn(e,/([^-])(transform)/g,`$1`+Ln+`$2`)+e;case 6187:return Xn(Xn(Xn(e,/(zoom-|grab)/,Ln+`$1`),/(image-set)/,Ln+`$1`),e,``)+e;case 5495:case 3959:return Xn(e,/(image-set\([^]*)/,Ln+"$1$`$1");case 4968:return Xn(Xn(e,/(.+:)(flex-)?(.*)/,Ln+`box-pack:$3`+Fn+`flex-pack:$3`),/s.+-b[^;]+/,`justify`)+Ln+e+e;case 4095:case 3583:case 4068:case 2532:return Xn(e,/(.+)-inline(.+)/,Ln+`$1$2`)+e;case 8116:case 7059:case 5753:case 5535:case 5445:case 5701:case 4933:case 4677:case 5533:case 5789:case 5021:case 4765:if(er(e)-1-t>6)switch(Qn(e,t+1)){case 109:if(Qn(e,t+4)!==45)break;case 102:return Xn(e,/(.+:)(.+)-([^]+)/,`$1`+Ln+`$2-$3$1`+In+(Qn(e,t+3)==108?`$3`:`$2-$3`))+e;case 115:return~Zn(e,`stretch`)?Ur(Xn(e,`stretch`,`fill-available`),t)+e:e}break;case 4949:if(Qn(e,t+1)!==115)break;case 6444:switch(Qn(e,er(e)-3-(~Zn(e,`!important`)&&10))){case 107:return Xn(e,`:`,`:`+Ln)+e;case 101:return Xn(e,/(.+:)([^;!]+)(;|!.+)?/,`$1`+Ln+(Qn(e,14)===45?`inline-`:``)+`box$3$1`+Ln+`$2$3$1`+Fn+`$2box$3`)+e}break;case 5936:switch(Qn(e,t+11)){case 114:return Ln+e+Fn+Xn(e,/[svh]\w+-[tblr]{2}/,`tb`)+e;case 108:return Ln+e+Fn+Xn(e,/[svh]\w+-[tblr]{2}/,`tb-rl`)+e;case 45:return Ln+e+Fn+Xn(e,/[svh]\w+-[tblr]{2}/,`lr`)+e}return Ln+e+Fn+e+e}return e}var Wr=[function(e,t,n,r){if(e.length>-1&&!e.return)switch(e.type){case Bn:e.return=Ur(e.value,e.length);break;case Hn:return Mr([dr(e,{value:Xn(e.value,`@`,`@`+Ln)})],r);case zn:if(e.length)return rr(e.props,function(t){switch(Yn(t,/(::plac\w+|:read-\w+)/)){case`:read-only`:case`:read-write`:return Mr([dr(e,{props:[Xn(t,/:(read-\w+)/,`:`+In+`$1`)]})],r);case`::placeholder`:return Mr([dr(e,{props:[Xn(t,/:(plac\w+)/,`:`+Ln+`input-$1`)]}),dr(e,{props:[Xn(t,/:(plac\w+)/,`:`+In+`$1`)]}),dr(e,{props:[Xn(t,/:(plac\w+)/,Fn+`input-$1`)]})],r)}return``})}}],Gr=function(e){var t=e.key;if(t===`css`){var n=document.querySelectorAll(`style[data-emotion]:not([data-s])`);Array.prototype.forEach.call(n,function(e){e.getAttribute(`data-emotion`).indexOf(` `)!==-1&&(document.head.appendChild(e),e.setAttribute(`data-s`,``))})}var r=e.stylisPlugins||Wr,i={},a,o=[];a=e.container||document.head,Array.prototype.forEach.call(document.querySelectorAll(`style[data-emotion^="`+t+` "]`),function(e){for(var t=e.getAttribute(`data-emotion`).split(` `),n=1;n<t.length;n++)i[t[n]]=!0;o.push(e)});var s,c=[Vr,Hr],l,u=[Nr,Fr(function(e){l.insert(e)})],d=Pr(c.concat(r,u)),f=function(e){return Mr(Dr(e),d)};s=function(e,t,n,r){l=n,f(e?e+`{`+t.styles+`}`:t.styles),r&&(p.inserted[t.name]=!0)};var p={key:t,sheet:new Pn({key:t,container:a,nonce:e.nonce,speedy:e.speedy,prepend:e.prepend,insertionPoint:e.insertionPoint}),nonce:e.nonce,inserted:i,registered:{},insert:s};return p.sheet.hydrate(o),p},Kr=o((e=>{var t=typeof Symbol==`function`&&Symbol.for,n=t?Symbol.for(`react.element`):60103,r=t?Symbol.for(`react.portal`):60106,i=t?Symbol.for(`react.fragment`):60107,a=t?Symbol.for(`react.strict_mode`):60108,o=t?Symbol.for(`react.profiler`):60114,s=t?Symbol.for(`react.provider`):60109,c=t?Symbol.for(`react.context`):60110,l=t?Symbol.for(`react.async_mode`):60111,u=t?Symbol.for(`react.concurrent_mode`):60111,d=t?Symbol.for(`react.forward_ref`):60112,f=t?Symbol.for(`react.suspense`):60113,p=t?Symbol.for(`react.suspense_list`):60120,m=t?Symbol.for(`react.memo`):60115,h=t?Symbol.for(`react.lazy`):60116,g=t?Symbol.for(`react.block`):60121,_=t?Symbol.for(`react.fundamental`):60117,v=t?Symbol.for(`react.responder`):60118,y=t?Symbol.for(`react.scope`):60119;function b(e){if(typeof e==`object`&&e){var t=e.$$typeof;switch(t){case n:switch(e=e.type,e){case l:case u:case i:case o:case a:case f:return e;default:switch(e&&=e.$$typeof,e){case c:case d:case h:case m:case s:return e;default:return t}}case r:return t}}}function x(e){return b(e)===u}e.AsyncMode=l,e.ConcurrentMode=u,e.ContextConsumer=c,e.ContextProvider=s,e.Element=n,e.ForwardRef=d,e.Fragment=i,e.Lazy=h,e.Memo=m,e.Portal=r,e.Profiler=o,e.StrictMode=a,e.Suspense=f,e.isAsyncMode=function(e){return x(e)||b(e)===l},e.isConcurrentMode=x,e.isContextConsumer=function(e){return b(e)===c},e.isContextProvider=function(e){return b(e)===s},e.isElement=function(e){return typeof e==`object`&&!!e&&e.$$typeof===n},e.isForwardRef=function(e){return b(e)===d},e.isFragment=function(e){return b(e)===i},e.isLazy=function(e){return b(e)===h},e.isMemo=function(e){return b(e)===m},e.isPortal=function(e){return b(e)===r},e.isProfiler=function(e){return b(e)===o},e.isStrictMode=function(e){return b(e)===a},e.isSuspense=function(e){return b(e)===f},e.isValidElementType=function(e){return typeof e==`string`||typeof e==`function`||e===i||e===u||e===o||e===a||e===f||e===p||typeof e==`object`&&!!e&&(e.$$typeof===h||e.$$typeof===m||e.$$typeof===s||e.$$typeof===c||e.$$typeof===d||e.$$typeof===_||e.$$typeof===v||e.$$typeof===y||e.$$typeof===g)},e.typeOf=b})),qr=o(((e,t)=>{t.exports=Kr()})),Jr=o(((e,t)=>{var n=qr(),r={childContextTypes:!0,contextType:!0,contextTypes:!0,defaultProps:!0,displayName:!0,getDefaultProps:!0,getDerivedStateFromError:!0,getDerivedStateFromProps:!0,mixins:!0,propTypes:!0,type:!0},i={name:!0,length:!0,prototype:!0,caller:!0,callee:!0,arguments:!0,arity:!0},a={$$typeof:!0,render:!0,defaultProps:!0,displayName:!0,propTypes:!0},o={$$typeof:!0,compare:!0,defaultProps:!0,displayName:!0,propTypes:!0,type:!0},s={};s[n.ForwardRef]=a,s[n.Memo]=o;function c(e){return n.isMemo(e)?o:s[e.$$typeof]||r}var l=Object.defineProperty,u=Object.getOwnPropertyNames,d=Object.getOwnPropertySymbols,f=Object.getOwnPropertyDescriptor,p=Object.getPrototypeOf,m=Object.prototype;function h(e,t,n){if(typeof t!=`string`){if(m){var r=p(t);r&&r!==m&&h(e,r,n)}var a=u(t);d&&(a=a.concat(d(t)));for(var o=c(e),s=c(t),g=0;g<a.length;++g){var _=a[g];if(!i[_]&&!(n&&n[_])&&!(s&&s[_])&&!(o&&o[_])){var v=f(t,_);try{l(e,_,v)}catch{}}}}return e}t.exports=h})),Yr=!0;function Xr(e,t,n){var r=``;return n.split(` `).forEach(function(n){e[n]===void 0?n&&(r+=n+` `):t.push(e[n]+`;`)}),r}var Zr=function(e,t,n){var r=e.key+`-`+t.name;(n===!1||Yr===!1)&&e.registered[r]===void 0&&(e.registered[r]=t.styles)},Qr=function(e,t,n){Zr(e,t,n);var r=e.key+`-`+t.name;if(e.inserted[t.name]===void 0){var i=t;do e.insert(t===i?`.`+r:``,i,e.sheet,!0),i=i.next;while(i!==void 0)}};function $r(e){for(var t=0,n,r=0,i=e.length;i>=4;++r,i-=4)n=e.charCodeAt(r)&255|(e.charCodeAt(++r)&255)<<8|(e.charCodeAt(++r)&255)<<16|(e.charCodeAt(++r)&255)<<24,n=(n&65535)*1540483477+((n>>>16)*59797<<16),n^=n>>>24,t=(n&65535)*1540483477+((n>>>16)*59797<<16)^(t&65535)*1540483477+((t>>>16)*59797<<16);switch(i){case 3:t^=(e.charCodeAt(r+2)&255)<<16;case 2:t^=(e.charCodeAt(r+1)&255)<<8;case 1:t^=e.charCodeAt(r)&255,t=(t&65535)*1540483477+((t>>>16)*59797<<16)}return t^=t>>>13,t=(t&65535)*1540483477+((t>>>16)*59797<<16),((t^t>>>15)>>>0).toString(36)}var ei={animationIterationCount:1,aspectRatio:1,borderImageOutset:1,borderImageSlice:1,borderImageWidth:1,boxFlex:1,boxFlexGroup:1,boxOrdinalGroup:1,columnCount:1,columns:1,flex:1,flexGrow:1,flexPositive:1,flexShrink:1,flexNegative:1,flexOrder:1,gridRow:1,gridRowEnd:1,gridRowSpan:1,gridRowStart:1,gridColumn:1,gridColumnEnd:1,gridColumnSpan:1,gridColumnStart:1,msGridRow:1,msGridRowSpan:1,msGridColumn:1,msGridColumnSpan:1,fontWeight:1,lineHeight:1,opacity:1,order:1,orphans:1,scale:1,tabSize:1,widows:1,zIndex:1,zoom:1,WebkitLineClamp:1,fillOpacity:1,floodOpacity:1,stopOpacity:1,strokeDasharray:1,strokeDashoffset:1,strokeMiterlimit:1,strokeOpacity:1,strokeWidth:1},ti=!1,ni=/[A-Z]|^ms/g,ri=/_EMO_([^_]+?)_([^]*?)_EMO_/g,ii=function(e){return e.charCodeAt(1)===45},ai=function(e){return e!=null&&typeof e!=`boolean`},oi=Ir(function(e){return ii(e)?e:e.replace(ni,`-$&`).toLowerCase()}),si=function(e,t){switch(e){case`animation`:case`animationName`:if(typeof t==`string`)return t.replace(ri,function(e,t,n){return fi={name:t,styles:n,next:fi},t})}return ei[e]!==1&&!ii(e)&&typeof t==`number`&&t!==0?t+`px`:t},ci=`Component selectors can only be used in conjunction with @emotion/babel-plugin, the swc Emotion plugin, or another Emotion-aware compiler transform.`;function li(e,t,n){if(n==null)return``;var r=n;if(r.__emotion_styles!==void 0)return r;switch(typeof n){case`boolean`:return``;case`object`:var i=n;if(i.anim===1)return fi={name:i.name,styles:i.styles,next:fi},i.name;var a=n;if(a.styles!==void 0){var o=a.next;if(o!==void 0)for(;o!==void 0;)fi={name:o.name,styles:o.styles,next:fi},o=o.next;return a.styles+`;`}return ui(e,t,n);case`function`:if(e!==void 0){var s=fi,c=n(e);return fi=s,li(e,t,c)}break}var l=n;if(t==null)return l;var u=t[l];return u===void 0?l:u}function ui(e,t,n){var r=``;if(Array.isArray(n))for(var i=0;i<n.length;i++)r+=li(e,t,n[i])+`;`;else for(var a in n){var o=n[a];if(typeof o!=`object`){var s=o;t!=null&&t[s]!==void 0?r+=a+`{`+t[s]+`}`:ai(s)&&(r+=oi(a)+`:`+si(a,s)+`;`)}else{if(a===`NO_COMPONENT_SELECTOR`&&ti)throw Error(ci);if(Array.isArray(o)&&typeof o[0]==`string`&&(t==null||t[o[0]]===void 0))for(var c=0;c<o.length;c++)ai(o[c])&&(r+=oi(a)+`:`+si(a,o[c])+`;`);else{var l=li(e,t,o);switch(a){case`animation`:case`animationName`:r+=oi(a)+`:`+l+`;`;break;default:r+=a+`{`+l+`}`}}}}return r}var di=/label:\s*([^\s;{]+)\s*(;|$)/g,fi;function pi(e,t,n){if(e.length===1&&typeof e[0]==`object`&&e[0]!==null&&e[0].styles!==void 0)return e[0];var r=!0,i=``;fi=void 0;var a=e[0];a==null||a.raw===void 0?(r=!1,i+=li(n,t,a)):i+=a[0];for(var o=1;o<e.length;o++)i+=li(n,t,e[o]),r&&(i+=a[o]);di.lastIndex=0;for(var s=``,c;(c=di.exec(i))!==null;)s+=`-`+c[1];return{name:$r(i)+s,styles:i,next:fi}}var mi=function(e){return e()},hi=x.useInsertionEffect?x.useInsertionEffect:!1,gi=hi||mi,_i=hi||x.useLayoutEffect,vi=x.createContext(typeof HTMLElement<`u`?Gr({key:`css`}):null);vi.Provider;var yi=function(e){return(0,x.forwardRef)(function(t,n){return e(t,(0,x.useContext)(vi),n)})},bi=x.createContext({}),xi={}.hasOwnProperty,Si=`__EMOTION_TYPE_PLEASE_DO_NOT_USE__`,Ci=function(e,t){var n={};for(var r in t)xi.call(t,r)&&(n[r]=t[r]);return n[Si]=e,n},wi=function(e){var t=e.cache,n=e.serialized,r=e.isStringTag;return Zr(t,n,r),gi(function(){return Qr(t,n,r)}),null},Ti=yi(function(e,t,n){var r=e.css;typeof r==`string`&&t.registered[r]!==void 0&&(r=t.registered[r]);var i=e[Si],a=[r],o=``;typeof e.className==`string`?o=Xr(t.registered,a,e.className):e.className!=null&&(o=e.className+` `);var s=pi(a,void 0,x.useContext(bi));o+=t.key+`-`+s.name;var c={};for(var l in e)xi.call(e,l)&&l!==`css`&&l!==Si&&(c[l]=e[l]);return c.className=o,n&&(c.ref=n),x.createElement(x.Fragment,null,x.createElement(wi,{cache:t,serialized:s,isStringTag:typeof i==`string`}),x.createElement(i,c))});Jr();var Ei=function(e,t){var n=arguments;if(t==null||!xi.call(t,`css`))return x.createElement.apply(void 0,n);var r=n.length,i=Array(r);i[0]=Ti,i[1]=Ci(e,t);for(var a=2;a<r;a++)i[a]=n[a];return x.createElement.apply(null,i)};(function(e){var t;(function(e){})(t||=e.JSX||={})})(Ei||={});var Di=yi(function(e,t){var n=e.styles,r=pi([n],void 0,x.useContext(bi)),i=x.useRef();return _i(function(){var e=t.key+`-global`,n=new t.sheet.constructor({key:e,nonce:t.sheet.nonce,container:t.sheet.container,speedy:t.sheet.isSpeedy}),a=!1,o=document.querySelector(`style[data-emotion="`+e+` `+r.name+`"]`);return t.sheet.tags.length&&(n.before=t.sheet.tags[0]),o!==null&&(a=!0,o.setAttribute(`data-emotion`,e),n.hydrate([o])),i.current=[n,a],function(){n.flush()}},[t]),_i(function(){var e=i.current,n=e[0];if(e[1]){e[1]=!1;return}r.next!==void 0&&Qr(t,r.next,!0),n.tags.length&&(n.before=n.tags[n.tags.length-1].nextElementSibling,n.flush()),t.insert(``,r,n,!1)},[t,r.name]),null});function Oi(){return pi([...arguments])}function ki(){var e=Oi.apply(void 0,arguments),t=`animation-`+e.name;return{name:t,styles:`@keyframes `+t+`{`+e.styles+`}`,anim:1,toString:function(){return`_EMO_`+this.name+`_`+this.styles+`_EMO_`}}}var Ai=/^((children|dangerouslySetInnerHTML|key|ref|autoFocus|defaultValue|defaultChecked|innerHTML|suppressContentEditableWarning|suppressHydrationWarning|valueLink|abbr|accept|acceptCharset|accessKey|action|allow|allowUserMedia|allowPaymentRequest|allowFullScreen|allowTransparency|alt|async|autoComplete|autoPlay|capture|cellPadding|cellSpacing|challenge|charSet|checked|cite|classID|className|cols|colSpan|content|contentEditable|contextMenu|controls|controlsList|coords|crossOrigin|data|dateTime|decoding|default|defer|dir|disabled|disablePictureInPicture|disableRemotePlayback|download|draggable|encType|enterKeyHint|fetchpriority|fetchPriority|form|formAction|formEncType|formMethod|formNoValidate|formTarget|frameBorder|headers|height|hidden|high|href|hrefLang|htmlFor|httpEquiv|id|inputMode|integrity|is|keyParams|keyType|kind|label|lang|list|loading|loop|low|marginHeight|marginWidth|max|maxLength|media|mediaGroup|method|min|minLength|multiple|muted|name|nonce|noValidate|open|optimum|pattern|placeholder|playsInline|popover|popoverTarget|popoverTargetAction|poster|preload|profile|radioGroup|readOnly|referrerPolicy|rel|required|reversed|role|rows|rowSpan|sandbox|scope|scoped|scrolling|seamless|selected|shape|size|sizes|slot|span|spellCheck|src|srcDoc|srcLang|srcSet|start|step|style|summary|tabIndex|target|title|translate|type|useMap|value|width|wmode|wrap|about|datatype|inlist|prefix|property|resource|typeof|vocab|autoCapitalize|autoCorrect|autoSave|color|incremental|fallback|inert|itemProp|itemScope|itemType|itemID|itemRef|on|option|results|security|unselectable|accentHeight|accumulate|additive|alignmentBaseline|allowReorder|alphabetic|amplitude|arabicForm|ascent|attributeName|attributeType|autoReverse|azimuth|baseFrequency|baselineShift|baseProfile|bbox|begin|bias|by|calcMode|capHeight|clip|clipPathUnits|clipPath|clipRule|colorInterpolation|colorInterpolationFilters|colorProfile|colorRendering|contentScriptType|contentStyleType|cursor|cx|cy|d|decelerate|descent|diffuseConstant|direction|display|divisor|dominantBaseline|dur|dx|dy|edgeMode|elevation|enableBackground|end|exponent|externalResourcesRequired|fill|fillOpacity|fillRule|filter|filterRes|filterUnits|floodColor|floodOpacity|focusable|fontFamily|fontSize|fontSizeAdjust|fontStretch|fontStyle|fontVariant|fontWeight|format|from|fr|fx|fy|g1|g2|glyphName|glyphOrientationHorizontal|glyphOrientationVertical|glyphRef|gradientTransform|gradientUnits|hanging|horizAdvX|horizOriginX|ideographic|imageRendering|in|in2|intercept|k|k1|k2|k3|k4|kernelMatrix|kernelUnitLength|kerning|keyPoints|keySplines|keyTimes|lengthAdjust|letterSpacing|lightingColor|limitingConeAngle|local|markerEnd|markerMid|markerStart|markerHeight|markerUnits|markerWidth|mask|maskContentUnits|maskUnits|mathematical|mode|numOctaves|offset|opacity|operator|order|orient|orientation|origin|overflow|overlinePosition|overlineThickness|panose1|paintOrder|pathLength|patternContentUnits|patternTransform|patternUnits|pointerEvents|points|pointsAtX|pointsAtY|pointsAtZ|preserveAlpha|preserveAspectRatio|primitiveUnits|r|radius|refX|refY|renderingIntent|repeatCount|repeatDur|requiredExtensions|requiredFeatures|restart|result|rotate|rx|ry|scale|seed|shapeRendering|slope|spacing|specularConstant|specularExponent|speed|spreadMethod|startOffset|stdDeviation|stemh|stemv|stitchTiles|stopColor|stopOpacity|strikethroughPosition|strikethroughThickness|string|stroke|strokeDasharray|strokeDashoffset|strokeLinecap|strokeLinejoin|strokeMiterlimit|strokeOpacity|strokeWidth|surfaceScale|systemLanguage|tableValues|targetX|targetY|textAnchor|textDecoration|textRendering|textLength|to|transform|u1|u2|underlinePosition|underlineThickness|unicode|unicodeBidi|unicodeRange|unitsPerEm|vAlphabetic|vHanging|vIdeographic|vMathematical|values|vectorEffect|version|vertAdvY|vertOriginX|vertOriginY|viewBox|viewTarget|visibility|widths|wordSpacing|writingMode|x|xHeight|x1|x2|xChannelSelector|xlinkActuate|xlinkArcrole|xlinkHref|xlinkRole|xlinkShow|xlinkTitle|xlinkType|xmlBase|xmlns|xmlnsXlink|xmlLang|xmlSpace|y|y1|y2|yChannelSelector|z|zoomAndPan|for|class|autofocus)|(([Dd][Aa][Tt][Aa]|[Aa][Rr][Ii][Aa]|x)-.*))$/,ji=Ir(function(e){return Ai.test(e)||e.charCodeAt(0)===111&&e.charCodeAt(1)===110&&e.charCodeAt(2)<91}),Mi=!1,Ni=ji,Pi=function(e){return e!==`theme`},Fi=function(e){return typeof e==`string`&&e.charCodeAt(0)>96?Ni:Pi},Ii=function(e,t,n){var r;if(t){var i=t.shouldForwardProp;r=e.__emotion_forwardProp&&i?function(t){return e.__emotion_forwardProp(t)&&i(t)}:i}return typeof r!=`function`&&n&&(r=e.__emotion_forwardProp),r},Li=function(e){var t=e.cache,n=e.serialized,r=e.isStringTag;return Zr(t,n,r),gi(function(){return Qr(t,n,r)}),null},Ri=function e(t,n){var r=t.__emotion_real===t,i=r&&t.__emotion_base||t,a,o;n!==void 0&&(a=n.label,o=n.target);var s=Ii(t,n,r),c=s||Fi(i),l=!c(`as`);return function(){var u=arguments,d=r&&t.__emotion_styles!==void 0?t.__emotion_styles.slice(0):[];if(a!==void 0&&d.push(`label:`+a+`;`),u[0]==null||u[0].raw===void 0)d.push.apply(d,u);else{var f=u[0];d.push(f[0]);for(var p=u.length,m=1;m<p;m++)d.push(u[m],f[m])}var h=yi(function(e,t,n){var r=l&&e.as||i,a=``,u=[],f=e;if(e.theme==null){for(var p in f={},e)f[p]=e[p];f.theme=x.useContext(bi)}typeof e.className==`string`?a=Xr(t.registered,u,e.className):e.className!=null&&(a=e.className+` `);var m=pi(d.concat(u),t.registered,f);a+=t.key+`-`+m.name,o!==void 0&&(a+=` `+o);var h=l&&s===void 0?Fi(r):c,g={};for(var _ in e)l&&_===`as`||h(_)&&(g[_]=e[_]);return g.className=a,n&&(g.ref=n),x.createElement(x.Fragment,null,x.createElement(Li,{cache:t,serialized:m,isStringTag:typeof r==`string`}),x.createElement(r,g))});return h.displayName=a===void 0?`Styled(`+(typeof i==`string`?i:i.displayName||i.name||`Component`)+`)`:a,h.defaultProps=t.defaultProps,h.__emotion_real=h,h.__emotion_base=i,h.__emotion_styles=d,h.__emotion_forwardProp=s,Object.defineProperty(h,`toString`,{value:function(){return o===void 0&&Mi?`NO_COMPONENT_SELECTOR`:`.`+o}}),h.withComponent=function(t,r){return e(t,An({},n,r,{shouldForwardProp:Ii(h,r,!0)})).apply(void 0,d)},h}},zi=`a.abbr.address.area.article.aside.audio.b.base.bdi.bdo.big.blockquote.body.br.button.canvas.caption.cite.code.col.colgroup.data.datalist.dd.del.details.dfn.dialog.div.dl.dt.em.embed.fieldset.figcaption.figure.footer.form.h1.h2.h3.h4.h5.h6.head.header.hgroup.hr.html.i.iframe.img.input.ins.kbd.keygen.label.legend.li.link.main.map.mark.marquee.menu.menuitem.meta.meter.nav.noscript.object.ol.optgroup.option.output.p.param.picture.pre.progress.q.rp.rt.ruby.s.samp.script.section.select.small.source.span.strong.style.sub.summary.sup.table.tbody.td.textarea.tfoot.th.thead.time.title.tr.track.u.ul.var.video.wbr.circle.clipPath.defs.ellipse.foreignObject.g.image.line.linearGradient.mask.path.pattern.polygon.polyline.radialGradient.rect.stop.svg.text.tspan`.split(`.`),Bi=Ri.bind(null);zi.forEach(function(e){Bi[e]=Bi(e)});var Vi=o((e=>{var t=Symbol.for(`react.transitional.element`),n=Symbol.for(`react.fragment`);function r(e,n,r){var i=null;if(r!==void 0&&(i=``+r),n.key!==void 0&&(i=``+n.key),`key`in n)for(var a in r={},n)a!==`key`&&(r[a]=n[a]);else r=n;return n=r.ref,{$$typeof:t,type:e,key:i,ref:n===void 0?null:n,props:r}}e.Fragment=n,e.jsx=r,e.jsxs=r})),L=o(((e,t)=>{t.exports=Vi()}))();function Hi(e){return e==null||Object.keys(e).length===0}function Ui(e){let{styles:t,defaultTheme:n={}}=e;return(0,L.jsx)(Di,{styles:typeof t==`function`?e=>t(Hi(e)?n:e):t})}function Wi(e,t){return Bi(e,t)}function Gi(e,t){Array.isArray(e.__emotion_styles)&&(e.__emotion_styles=t(e.__emotion_styles))}var Ki=[];function qi(e){return Ki[0]=e,pi(Ki)}var Ji=o((e=>{var t=Symbol.for(`react.fragment`),n=Symbol.for(`react.strict_mode`),r=Symbol.for(`react.profiler`),i=Symbol.for(`react.consumer`),a=Symbol.for(`react.context`),o=Symbol.for(`react.forward_ref`),s=Symbol.for(`react.suspense`),c=Symbol.for(`react.suspense_list`),l=Symbol.for(`react.memo`),u=Symbol.for(`react.lazy`),d=Symbol.for(`react.client.reference`);e.isValidElementType=function(e){return!!(typeof e==`string`||typeof e==`function`||e===t||e===r||e===n||e===s||e===c||typeof e==`object`&&e&&(e.$$typeof===u||e.$$typeof===l||e.$$typeof===a||e.$$typeof===i||e.$$typeof===o||e.$$typeof===d||e.getModuleId!==void 0))}})),Yi=o(((e,t)=>{t.exports=Ji()}))();function Xi(e){if(typeof e!=`object`||!e)return!1;let t=Object.getPrototypeOf(e);return(t===null||t===Object.prototype||Object.getPrototypeOf(t)===null)&&!(Symbol.toStringTag in e)&&!(Symbol.iterator in e)}function Zi(e){if(x.isValidElement(e)||(0,Yi.isValidElementType)(e)||!Xi(e))return e;let t={};return Object.keys(e).forEach(n=>{t[n]=Zi(e[n])}),t}function Qi(e,t,n={clone:!0}){let r=n.clone?{...e}:e;return Xi(e)&&Xi(t)&&Object.keys(t).forEach(i=>{x.isValidElement(t[i])||(0,Yi.isValidElementType)(t[i])?r[i]=t[i]:Xi(t[i])&&Object.prototype.hasOwnProperty.call(e,i)&&Xi(e[i])?r[i]=Qi(e[i],t[i],n):n.clone?r[i]=Xi(t[i])?Zi(t[i]):t[i]:r[i]=t[i]}),r}var $i=e=>{let t=Object.keys(e).map(t=>({key:t,val:e[t]}))||[];return t.sort((e,t)=>e.val-t.val),t.reduce((e,t)=>({...e,[t.key]:t.val}),{})};function ea(e){let{values:t={xs:0,sm:600,md:900,lg:1200,xl:1536},unit:n=`px`,step:r=5,...i}=e,a=$i(t),o=Object.keys(a);function s(e){return`@media (min-width:${typeof t[e]==`number`?t[e]:e}${n})`}function c(e){return`@media (max-width:${(typeof t[e]==`number`?t[e]:e)-r/100}${n})`}function l(e,i){let a=o.indexOf(i);return`@media (min-width:${typeof t[e]==`number`?t[e]:e}${n}) and (max-width:${(a!==-1&&typeof t[o[a]]==`number`?t[o[a]]:i)-r/100}${n})`}function u(e){return o.indexOf(e)+1<o.length?l(e,o[o.indexOf(e)+1]):s(e)}function d(e){let t=o.indexOf(e);return t===0?s(o[1]):t===o.length-1?c(o[t]):l(e,o[o.indexOf(e)+1]).replace(`@media`,`@media not all and`)}return{keys:o,values:a,up:s,down:c,between:l,only:u,not:d,unit:n,...i}}function ta(e,t){if(!e.containerQueries)return t;let n=Object.keys(t).filter(e=>e.startsWith(`@container`)).sort((e,t)=>{let n=/min-width:\s*([0-9.]+)/;return(e.match(n)?.[1]||0)-+(t.match(n)?.[1]||0)});return n.length?n.reduce((e,n)=>{let r=t[n];return delete e[n],e[n]=r,e},{...t}):t}function na(e,t){return t===`@`||t.startsWith(`@`)&&(e.some(e=>t.startsWith(`@${e}`))||!!t.match(/^@\d/))}function ra(e,t){let n=t.match(/^@([^/]+)?\/?(.+)?$/);if(!n)return null;let[,r,i]=n,a=Number.isNaN(+r)?r||0:+r;return e.containerQueries(i).up(a)}function ia(e){let t=(e,t)=>e.replace(`@media`,t?`@container ${t}`:`@container`);function n(n,r){n.up=(...n)=>t(e.breakpoints.up(...n),r),n.down=(...n)=>t(e.breakpoints.down(...n),r),n.between=(...n)=>t(e.breakpoints.between(...n),r),n.only=(...n)=>t(e.breakpoints.only(...n),r),n.not=(...n)=>{let i=t(e.breakpoints.not(...n),r);return i.includes(`not all and`)?i.replace(`not all and `,``).replace(`min-width:`,`width<`).replace(`max-width:`,`width>`).replace(`and`,`or`):i}}let r={},i=e=>(n(r,e),r);return n(i),{...e,containerQueries:i}}var aa={borderRadius:4};function oa(e,t){return t?Qi(e,t,{clone:!1}):e}var sa=oa;const ca={xs:0,sm:600,md:900,lg:1200,xl:1536};var la={keys:[`xs`,`sm`,`md`,`lg`,`xl`],up:e=>`@media (min-width:${ca[e]}px)`},ua={containerQueries:e=>({up:t=>{let n=typeof t==`number`?t:ca[t]||t;return typeof n==`number`&&(n=`${n}px`),e?`@container ${e} (min-width:${n})`:`@container (min-width:${n})`}})};function da(e,t,n){let r=e.theme||{};if(Array.isArray(t)){let e=r.breakpoints||la;return t.reduce((r,i,a)=>(r[e.up(e.keys[a])]=n(t[a]),r),{})}if(typeof t==`object`){let e=r.breakpoints||la;return Object.keys(t).reduce((i,a)=>{if(na(e.keys,a)){let e=ra(r.containerQueries?r:ua,a);e&&(i[e]=n(t[a],a))}else if(Object.keys(e.values||ca).includes(a)){let r=e.up(a);i[r]=n(t[a],a)}else{let e=a;i[e]=t[e]}return i},{})}return n(t)}function fa(e={}){return e.keys?.reduce((t,n)=>{let r=e.up(n);return t[r]={},t},{})||{}}function pa(e,t){return e.reduce((e,t)=>{let n=e[t];return(!n||Object.keys(n).length===0)&&delete e[t],e},t)}function ma(e){if(typeof e!=`string`)throw Error(On(7));return e.charAt(0).toUpperCase()+e.slice(1)}function ha(e,t,n=!0){if(!t||typeof t!=`string`)return null;if(e&&e.vars&&n){let n=`vars.${t}`.split(`.`).reduce((e,t)=>e&&e[t]?e[t]:null,e);if(n!=null)return n}return t.split(`.`).reduce((e,t)=>e&&e[t]!=null?e[t]:null,e)}function ga(e,t,n,r=n){let i;return i=typeof e==`function`?e(n):Array.isArray(e)?e[n]||r:ha(e,n)||r,t&&(i=t(i,r,e)),i}function _a(e){let{prop:t,cssProperty:n=e.prop,themeKey:r,transform:i}=e,a=e=>{if(e[t]==null)return null;let a=e[t],o=e.theme,s=ha(o,r)||{};return da(e,a,e=>{let r=ga(s,i,e);return e===r&&typeof e==`string`&&(r=ga(s,i,`${t}${e===`default`?``:ma(e)}`,e)),n===!1?r:{[n]:r}})};return a.propTypes={},a.filterProps=[t],a}var va=_a;function ya(e){let t={};return n=>(t[n]===void 0&&(t[n]=e(n)),t[n])}var ba={m:`margin`,p:`padding`},xa={t:`Top`,r:`Right`,b:`Bottom`,l:`Left`,x:[`Left`,`Right`],y:[`Top`,`Bottom`]},Sa={marginX:`mx`,marginY:`my`,paddingX:`px`,paddingY:`py`},Ca=ya(e=>{if(e.length>2)if(Sa[e])e=Sa[e];else return[e];let[t,n]=e.split(``),r=ba[t],i=xa[n]||``;return Array.isArray(i)?i.map(e=>r+e):[r+i]});const wa=[`m`,`mt`,`mr`,`mb`,`ml`,`mx`,`my`,`margin`,`marginTop`,`marginRight`,`marginBottom`,`marginLeft`,`marginX`,`marginY`,`marginInline`,`marginInlineStart`,`marginInlineEnd`,`marginBlock`,`marginBlockStart`,`marginBlockEnd`],Ta=[`p`,`pt`,`pr`,`pb`,`pl`,`px`,`py`,`padding`,`paddingTop`,`paddingRight`,`paddingBottom`,`paddingLeft`,`paddingX`,`paddingY`,`paddingInline`,`paddingInlineStart`,`paddingInlineEnd`,`paddingBlock`,`paddingBlockStart`,`paddingBlockEnd`];var Ea=[...wa,...Ta];function Da(e,t,n,r){let i=ha(e,t,!0)??n;return typeof i==`number`||typeof i==`string`?e=>typeof e==`string`?e:typeof i==`string`?i.startsWith(`var(`)&&e===0?0:i.startsWith(`var(`)&&e===1?i:`calc(${e} * ${i})`:i*e:Array.isArray(i)?e=>{if(typeof e==`string`)return e;let t=i[Math.abs(e)];return e>=0?t:typeof t==`number`?-t:typeof t==`string`&&t.startsWith(`var(`)?`calc(-1 * ${t})`:`-${t}`}:typeof i==`function`?i:()=>void 0}function Oa(e){return Da(e,`spacing`,8,`spacing`)}function ka(e,t){return typeof t==`string`||t==null?t:e(t)}function Aa(e,t){return n=>e.reduce((e,r)=>(e[r]=ka(t,n),e),{})}function ja(e,t,n,r){if(!t.includes(n))return null;let i=Aa(Ca(n),r),a=e[n];return da(e,a,i)}function Ma(e,t){let n=Oa(e.theme);return Object.keys(e).map(r=>ja(e,t,r,n)).reduce(sa,{})}function Na(e){return Ma(e,wa)}Na.propTypes={},Na.filterProps=wa;function Pa(e){return Ma(e,Ta)}Pa.propTypes={},Pa.filterProps=Ta;function Fa(e){return Ma(e,Ea)}Fa.propTypes={},Fa.filterProps=Ea;function Ia(e=8,t=Oa({spacing:e})){if(e.mui)return e;let n=(...e)=>(e.length===0?[1]:e).map(e=>{let n=t(e);return typeof n==`number`?`${n}px`:n}).join(` `);return n.mui=!0,n}function La(...e){let t=e.reduce((e,t)=>(t.filterProps.forEach(n=>{e[n]=t}),e),{}),n=e=>Object.keys(e).reduce((n,r)=>t[r]?sa(n,t[r](e)):n,{});return n.propTypes={},n.filterProps=e.reduce((e,t)=>e.concat(t.filterProps),[]),n}var Ra=La;function za(e){return typeof e==`number`?`${e}px solid`:e}function Ba(e,t){return va({prop:e,themeKey:`borders`,transform:t})}const Va=Ba(`border`,za),Ha=Ba(`borderTop`,za),Ua=Ba(`borderRight`,za),Wa=Ba(`borderBottom`,za),Ga=Ba(`borderLeft`,za),Ka=Ba(`borderColor`),qa=Ba(`borderTopColor`),Ja=Ba(`borderRightColor`),Ya=Ba(`borderBottomColor`),Xa=Ba(`borderLeftColor`),Za=Ba(`outline`,za),Qa=Ba(`outlineColor`),$a=e=>{if(e.borderRadius!==void 0&&e.borderRadius!==null){let t=Da(e.theme,`shape.borderRadius`,4,`borderRadius`);return da(e,e.borderRadius,e=>({borderRadius:ka(t,e)}))}return null};$a.propTypes={},$a.filterProps=[`borderRadius`],Ra(Va,Ha,Ua,Wa,Ga,Ka,qa,Ja,Ya,Xa,$a,Za,Qa);const eo=e=>{if(e.gap!==void 0&&e.gap!==null){let t=Da(e.theme,`spacing`,8,`gap`);return da(e,e.gap,e=>({gap:ka(t,e)}))}return null};eo.propTypes={},eo.filterProps=[`gap`];const to=e=>{if(e.columnGap!==void 0&&e.columnGap!==null){let t=Da(e.theme,`spacing`,8,`columnGap`);return da(e,e.columnGap,e=>({columnGap:ka(t,e)}))}return null};to.propTypes={},to.filterProps=[`columnGap`];const no=e=>{if(e.rowGap!==void 0&&e.rowGap!==null){let t=Da(e.theme,`spacing`,8,`rowGap`);return da(e,e.rowGap,e=>({rowGap:ka(t,e)}))}return null};no.propTypes={},no.filterProps=[`rowGap`],Ra(eo,to,no,va({prop:`gridColumn`}),va({prop:`gridRow`}),va({prop:`gridAutoFlow`}),va({prop:`gridAutoColumns`}),va({prop:`gridAutoRows`}),va({prop:`gridTemplateColumns`}),va({prop:`gridTemplateRows`}),va({prop:`gridTemplateAreas`}),va({prop:`gridArea`}));function ro(e,t){return t===`grey`?t:e}Ra(va({prop:`color`,themeKey:`palette`,transform:ro}),va({prop:`bgcolor`,cssProperty:`backgroundColor`,themeKey:`palette`,transform:ro}),va({prop:`backgroundColor`,themeKey:`palette`,transform:ro}));function io(e){return e<=1&&e!==0?`${e*100}%`:e}const ao=va({prop:`width`,transform:io}),oo=e=>e.maxWidth!==void 0&&e.maxWidth!==null?da(e,e.maxWidth,t=>{let n=e.theme?.breakpoints?.values?.[t]||ca[t];return n?e.theme?.breakpoints?.unit===`px`?{maxWidth:n}:{maxWidth:`${n}${e.theme.breakpoints.unit}`}:{maxWidth:io(t)}}):null;oo.filterProps=[`maxWidth`];const so=va({prop:`minWidth`,transform:io}),co=va({prop:`height`,transform:io}),lo=va({prop:`maxHeight`,transform:io}),uo=va({prop:`minHeight`,transform:io});va({prop:`size`,cssProperty:`width`,transform:io}),va({prop:`size`,cssProperty:`height`,transform:io}),Ra(ao,oo,so,co,lo,uo,va({prop:`boxSizing`}));var fo={border:{themeKey:`borders`,transform:za},borderTop:{themeKey:`borders`,transform:za},borderRight:{themeKey:`borders`,transform:za},borderBottom:{themeKey:`borders`,transform:za},borderLeft:{themeKey:`borders`,transform:za},borderColor:{themeKey:`palette`},borderTopColor:{themeKey:`palette`},borderRightColor:{themeKey:`palette`},borderBottomColor:{themeKey:`palette`},borderLeftColor:{themeKey:`palette`},outline:{themeKey:`borders`,transform:za},outlineColor:{themeKey:`palette`},borderRadius:{themeKey:`shape.borderRadius`,style:$a},color:{themeKey:`palette`,transform:ro},bgcolor:{themeKey:`palette`,cssProperty:`backgroundColor`,transform:ro},backgroundColor:{themeKey:`palette`,transform:ro},p:{style:Pa},pt:{style:Pa},pr:{style:Pa},pb:{style:Pa},pl:{style:Pa},px:{style:Pa},py:{style:Pa},padding:{style:Pa},paddingTop:{style:Pa},paddingRight:{style:Pa},paddingBottom:{style:Pa},paddingLeft:{style:Pa},paddingX:{style:Pa},paddingY:{style:Pa},paddingInline:{style:Pa},paddingInlineStart:{style:Pa},paddingInlineEnd:{style:Pa},paddingBlock:{style:Pa},paddingBlockStart:{style:Pa},paddingBlockEnd:{style:Pa},m:{style:Na},mt:{style:Na},mr:{style:Na},mb:{style:Na},ml:{style:Na},mx:{style:Na},my:{style:Na},margin:{style:Na},marginTop:{style:Na},marginRight:{style:Na},marginBottom:{style:Na},marginLeft:{style:Na},marginX:{style:Na},marginY:{style:Na},marginInline:{style:Na},marginInlineStart:{style:Na},marginInlineEnd:{style:Na},marginBlock:{style:Na},marginBlockStart:{style:Na},marginBlockEnd:{style:Na},displayPrint:{cssProperty:!1,transform:e=>({"@media print":{display:e}})},display:{},overflow:{},textOverflow:{},visibility:{},whiteSpace:{},flexBasis:{},flexDirection:{},flexWrap:{},justifyContent:{},alignItems:{},alignContent:{},order:{},flex:{},flexGrow:{},flexShrink:{},alignSelf:{},justifyItems:{},justifySelf:{},gap:{style:eo},rowGap:{style:no},columnGap:{style:to},gridColumn:{},gridRow:{},gridAutoFlow:{},gridAutoColumns:{},gridAutoRows:{},gridTemplateColumns:{},gridTemplateRows:{},gridTemplateAreas:{},gridArea:{},position:{},zIndex:{themeKey:`zIndex`},top:{},right:{},bottom:{},left:{},boxShadow:{themeKey:`shadows`},width:{transform:io},maxWidth:{style:oo},minWidth:{transform:io},height:{transform:io},maxHeight:{transform:io},minHeight:{transform:io},boxSizing:{},font:{themeKey:`font`},fontFamily:{themeKey:`typography`},fontSize:{themeKey:`typography`},fontStyle:{themeKey:`typography`},fontWeight:{themeKey:`typography`},letterSpacing:{},textTransform:{},lineHeight:{},textAlign:{},typography:{cssProperty:!1,themeKey:`typography`}};function po(...e){let t=e.reduce((e,t)=>e.concat(Object.keys(t)),[]),n=new Set(t);return e.every(e=>n.size===Object.keys(e).length)}function mo(e,t){return typeof e==`function`?e(t):e}function ho(){function e(e,t,n,r){let i={[e]:t,theme:n},a=r[e];if(!a)return{[e]:t};let{cssProperty:o=e,themeKey:s,transform:c,style:l}=a;if(t==null)return null;if(s===`typography`&&t===`inherit`)return{[e]:t};let u=ha(n,s)||{};return l?l(i):da(i,t,t=>{let n=ga(u,c,t);return t===n&&typeof t==`string`&&(n=ga(u,c,`${e}${t===`default`?``:ma(t)}`,t)),o===!1?n:{[o]:n}})}function t(n){let{sx:r,theme:i={},nested:a}=n||{};if(!r)return null;let o=i.unstable_sxConfig??fo;function s(n){let r=n;if(typeof n==`function`)r=n(i);else if(typeof n!=`object`)return n;if(!r)return null;let s=fa(i.breakpoints),c=Object.keys(s),l=s;return Object.keys(r).forEach(n=>{let a=mo(r[n],i);if(a!=null)if(typeof a==`object`)if(o[n])l=sa(l,e(n,a,i,o));else{let e=da({theme:i},a,e=>({[n]:e}));po(e,a)?l[n]=t({sx:a,theme:i,nested:!0}):l=sa(l,e)}else l=sa(l,e(n,a,i,o))}),!a&&i.modularCssLayers?{"@layer sx":ta(i,pa(c,l))}:ta(i,pa(c,l))}return Array.isArray(r)?r.map(s):s(r)}return t}var R=ho();R.filterProps=[`sx`];var go=R;function _o(e,t){let n=this;if(n.vars){if(!n.colorSchemes?.[e]||typeof n.getColorSchemeSelector!=`function`)return{};let r=n.getColorSchemeSelector(e);return r===`&`?t:((r.includes(`data-`)||r.includes(`.`))&&(r=`*:where(${r.replace(/\s*&$/,``)}) &`),{[r]:t})}return n.palette.mode===e?t:{}}function vo(e={},...t){let{breakpoints:n={},palette:r={},spacing:i,shape:a={},...o}=e,s=ea(n),c=Ia(i),l=Qi({breakpoints:s,direction:`ltr`,components:{},palette:{mode:`light`,...r},spacing:c,shape:{...aa,...a}},o);return l=ia(l),l.applyStyles=_o,l=t.reduce((e,t)=>Qi(e,t),l),l.unstable_sxConfig={...fo,...o?.unstable_sxConfig},l.unstable_sx=function(e){return go({sx:e,theme:this})},l}var yo=vo;function bo(e){return Object.keys(e).length===0}function xo(e=null){let t=x.useContext(bi);return!t||bo(t)?e:t}var So=xo;const Co=yo();function wo(e=Co){return So(e)}var To=wo;function Eo(e){let t=qi(e);return e!==t&&t.styles?(t.styles.match(/^@layer\s+[^{]*$/)||(t.styles=`@layer global{${t.styles}}`),t):e}function Do({styles:e,themeId:t,defaultTheme:n={}}){let r=To(n),i=t&&r[t]||r,a=typeof e==`function`?e(i):e;return i.modularCssLayers&&(a=Array.isArray(a)?a.map(e=>Eo(typeof e==`function`?e(i):e)):Eo(a)),(0,L.jsx)(Ui,{styles:a})}var Oo=Do,ko=e=>{let t={systemProps:{},otherProps:{}},n=e?.theme?.unstable_sxConfig??fo;return Object.keys(e).forEach(r=>{n[r]?t.systemProps[r]=e[r]:t.otherProps[r]=e[r]}),t};function Ao(e){let{sx:t,...n}=e,{systemProps:r,otherProps:i}=ko(n),a;return a=Array.isArray(t)?[r,...t]:typeof t==`function`?(...e)=>{let n=t(...e);return Xi(n)?{...r,...n}:r}:{...r,...t},{...i,sx:a}}var jo=e=>e,Mo=(()=>{let e=jo;return{configure(t){e=t},generate(t){return e(t)},reset(){e=jo}}})(),No=g();function Po(e){var t,n,r=``;if(typeof e==`string`||typeof e==`number`)r+=e;else if(typeof e==`object`)if(Array.isArray(e)){var i=e.length;for(t=0;t<i;t++)e[t]&&(n=Po(e[t]))&&(r&&(r+=` `),r+=n)}else for(n in e)e[n]&&(r&&(r+=` `),r+=n);return r}function Fo(){for(var e,t,n=0,r=``,i=arguments.length;n<i;n++)(e=arguments[n])&&(t=Po(e))&&(r&&(r+=` `),r+=t);return r}var z=Fo;function Io(e={}){let{themeId:t,defaultTheme:n,defaultClassName:r=`MuiBox-root`,generateClassName:i}=e,a=Wi(`div`,{shouldForwardProp:e=>e!==`theme`&&e!==`sx`&&e!==`as`})(go);return x.forwardRef(function(e,o){let s=To(n),{className:c,component:l=`div`,...u}=Ao(e);return(0,L.jsx)(a,{as:l,ref:o,className:z(c,i?i(r):r),theme:t&&s[t]||s,...u})})}const Lo={active:`active`,checked:`checked`,completed:`completed`,disabled:`disabled`,error:`error`,expanded:`expanded`,focused:`focused`,focusVisible:`focusVisible`,open:`open`,readOnly:`readOnly`,required:`required`,selected:`selected`};function Ro(e,t,n=`Mui`){let r=Lo[t];return r?`${n}-${r}`:`${Mo.generate(e)}-${t}`}function zo(e,t,n=`Mui`){let r={};return t.forEach(t=>{r[t]=Ro(e,t,n)}),r}function Bo(e){let{variants:t,...n}=e,r={variants:t,style:qi(n),isProcessed:!0};return r.style===n||t&&t.forEach(e=>{typeof e.style!=`function`&&(e.style=qi(e.style))}),r}const Vo=yo();function Ho(e){return e!==`ownerState`&&e!==`theme`&&e!==`sx`&&e!==`as`}function Uo(e,t){return t&&e&&typeof e==`object`&&e.styles&&!e.styles.startsWith(`@layer`)&&(e.styles=`@layer ${t}{${String(e.styles)}}`),e}function Wo(e){return e?(t,n)=>n[e]:null}function Go(e,t,n){e.theme=Yo(e.theme)?n:e.theme[t]||e.theme}function Ko(e,t,n){let r=typeof t==`function`?t(e):t;if(Array.isArray(r))return r.flatMap(t=>Ko(e,t,n));if(Array.isArray(r?.variants)){let t;if(r.isProcessed)t=n?Uo(r.style,n):r.style;else{let{variants:e,...i}=r;t=n?Uo(qi(i),n):i}return qo(e,r.variants,[t],n)}return r?.isProcessed?n?Uo(qi(r.style),n):r.style:n?Uo(qi(r),n):r}function qo(e,t,n=[],r=void 0){let i;variantLoop:for(let a=0;a<t.length;a+=1){let o=t[a];if(typeof o.props==`function`){if(i??={...e,...e.ownerState,ownerState:e.ownerState},!o.props(i))continue}else for(let t in o.props)if(e[t]!==o.props[t]&&e.ownerState?.[t]!==o.props[t])continue variantLoop;typeof o.style==`function`?(i??={...e,...e.ownerState,ownerState:e.ownerState},n.push(r?Uo(qi(o.style(i)),r):o.style(i))):n.push(r?Uo(qi(o.style),r):o.style)}return n}function Jo(e={}){let{themeId:t,defaultTheme:n=Vo,rootShouldForwardProp:r=Ho,slotShouldForwardProp:i=Ho}=e;function a(e){Go(e,t,n)}return(e,t={})=>{Gi(e,e=>e.filter(e=>e!==go));let{name:n,slot:o,skipVariantsResolver:s,skipSx:c,overridesResolver:l=Wo(Zo(o)),...u}=t,d=n&&n.startsWith(`Mui`)||o?`components`:`custom`,f=s===void 0?o&&o!==`Root`&&o!==`root`||!1:s,p=c||!1,m=Ho;o===`Root`||o===`root`?m=r:o?m=i:Xo(e)&&(m=void 0);let h=Wi(e,{shouldForwardProp:m,label:void 0,...u}),g=e=>{if(e.__emotion_real===e)return e;if(typeof e==`function`)return function(t){return Ko(t,e,t.theme.modularCssLayers?d:void 0)};if(Xi(e)){let t=Bo(e);return function(e){return t.variants?Ko(e,t,e.theme.modularCssLayers?d:void 0):e.theme.modularCssLayers?Uo(t.style,d):t.style}}return e},_=(...t)=>{let r=[],i=t.map(g),o=[];if(r.push(a),n&&l&&o.push(function(e){let t=e.theme.components?.[n]?.styleOverrides;if(!t)return null;let r={};for(let n in t)r[n]=Ko(e,t[n],e.theme.modularCssLayers?`theme`:void 0);return l(e,r)}),n&&!f&&o.push(function(e){let t=e.theme?.components?.[n]?.variants;return t?qo(e,t,[],e.theme.modularCssLayers?`theme`:void 0):null}),p||o.push(go),Array.isArray(i[0])){let e=i.shift(),t=Array(r.length).fill(``),n=Array(o.length).fill(``),a;a=[...t,...e,...n],a.raw=[...t,...e.raw,...n],r.unshift(a)}let s=h(...r,...i,...o);return e.muiName&&(s.muiName=e.muiName),s};return h.withConfig&&(_.withConfig=h.withConfig),_}}function Yo(e){for(let t in e)return!1;return!0}function Xo(e){return typeof e==`string`&&e.charCodeAt(0)>96}function Zo(e){return e&&e.charAt(0).toLowerCase()+e.slice(1)}function Qo(e,t,n=!1){let r={...t};for(let i in e)if(Object.prototype.hasOwnProperty.call(e,i)){let a=i;if(a===`components`||a===`slots`)r[a]={...e[a],...r[a]};else if(a===`componentsProps`||a===`slotProps`){let i=e[a],o=t[a];if(!o)r[a]=i||{};else if(!i)r[a]=o;else for(let e in r[a]={...o},i)if(Object.prototype.hasOwnProperty.call(i,e)){let t=e;r[a][t]=Qo(i[t],o[t],n)}}else a===`className`&&n&&t.className?r.className=z(e?.className,t?.className):a===`style`&&n&&t.style?r.style={...e?.style,...t?.style}:r[a]===void 0&&(r[a]=e[a])}return r}var $o=typeof window<`u`?x.useLayoutEffect:x.useEffect;function es(e,t=-(2**53-1),n=2**53-1){return Math.max(t,Math.min(e,n))}var ts=es;function ns(e,t=0,n=1){return ts(e,t,n)}function rs(e){e=e.slice(1);let t=RegExp(`.{1,${e.length>=6?2:1}}`,`g`),n=e.match(t);return n&&n[0].length===1&&(n=n.map(e=>e+e)),n?`rgb${n.length===4?`a`:``}(${n.map((e,t)=>t<3?parseInt(e,16):Math.round(parseInt(e,16)/255*1e3)/1e3).join(`, `)})`:``}function is(e){if(e.type)return e;if(e.charAt(0)===`#`)return is(rs(e));let t=e.indexOf(`(`),n=e.substring(0,t);if(![`rgb`,`rgba`,`hsl`,`hsla`,`color`].includes(n))throw Error(On(9,e));let r=e.substring(t+1,e.length-1),i;if(n===`color`){if(r=r.split(` `),i=r.shift(),r.length===4&&r[3].charAt(0)===`/`&&(r[3]=r[3].slice(1)),![`srgb`,`display-p3`,`a98-rgb`,`prophoto-rgb`,`rec-2020`].includes(i))throw Error(On(10,i))}else r=r.split(`,`);return r=r.map(e=>parseFloat(e)),{type:n,values:r,colorSpace:i}}const as=e=>{let t=is(e);return t.values.slice(0,3).map((e,n)=>t.type.includes(`hsl`)&&n!==0?`${e}%`:e).join(` `)},os=(e,t)=>{try{return as(e)}catch{return e}};function ss(e){let{type:t,colorSpace:n}=e,{values:r}=e;return t.includes(`rgb`)?r=r.map((e,t)=>t<3?parseInt(e,10):e):t.includes(`hsl`)&&(r[1]=`${r[1]}%`,r[2]=`${r[2]}%`),r=t.includes(`color`)?`${n} ${r.join(` `)}`:`${r.join(`, `)}`,`${t}(${r})`}function cs(e){e=is(e);let{values:t}=e,n=t[0],r=t[1]/100,i=t[2]/100,a=r*Math.min(i,1-i),o=(e,t=(e+n/30)%12)=>i-a*Math.max(Math.min(t-3,9-t,1),-1),s=`rgb`,c=[Math.round(o(0)*255),Math.round(o(8)*255),Math.round(o(4)*255)];return e.type===`hsla`&&(s+=`a`,c.push(t[3])),ss({type:s,values:c})}function ls(e){e=is(e);let t=e.type===`hsl`||e.type===`hsla`?is(cs(e)).values:e.values;return t=t.map(t=>(e.type!==`color`&&(t/=255),t<=.03928?t/12.92:((t+.055)/1.055)**2.4)),Number((.2126*t[0]+.7152*t[1]+.0722*t[2]).toFixed(3))}function us(e,t){let n=ls(e),r=ls(t);return(Math.max(n,r)+.05)/(Math.min(n,r)+.05)}function ds(e,t){return e=is(e),t=ns(t),(e.type===`rgb`||e.type===`hsl`)&&(e.type+=`a`),e.type===`color`?e.values[3]=`/${t}`:e.values[3]=t,ss(e)}function fs(e,t,n){try{return ds(e,t)}catch{return e}}function ps(e,t){if(e=is(e),t=ns(t),e.type.includes(`hsl`))e.values[2]*=1-t;else if(e.type.includes(`rgb`)||e.type.includes(`color`))for(let n=0;n<3;n+=1)e.values[n]*=1-t;return ss(e)}function ms(e,t,n){try{return ps(e,t)}catch{return e}}function hs(e,t){if(e=is(e),t=ns(t),e.type.includes(`hsl`))e.values[2]+=(100-e.values[2])*t;else if(e.type.includes(`rgb`))for(let n=0;n<3;n+=1)e.values[n]+=(255-e.values[n])*t;else if(e.type.includes(`color`))for(let n=0;n<3;n+=1)e.values[n]+=(1-e.values[n])*t;return ss(e)}function gs(e,t,n){try{return hs(e,t)}catch{return e}}function _s(e,t=.15){return ls(e)>.5?ps(e,t):hs(e,t)}function vs(e,t,n){try{return _s(e,t)}catch{return e}}var ys=x.createContext(null);function bs(){return x.useContext(ys)}var xs=typeof Symbol==`function`&&Symbol.for?Symbol.for(`mui.nested`):`__THEME_NESTED__`;function Ss(e,t){return typeof t==`function`?t(e):{...e,...t}}function Cs(e){let{children:t,theme:n}=e,r=bs(),i=x.useMemo(()=>{let e=r===null?{...n}:Ss(r,n);return e!=null&&(e[xs]=r!==null),e},[n,r]);return(0,L.jsx)(ys.Provider,{value:i,children:t})}var ws=Cs,Ts=x.createContext();function Es({value:e,...t}){return(0,L.jsx)(Ts.Provider,{value:e??!0,...t})}const Ds=()=>x.useContext(Ts)??!1;var Os=Es,ks=x.createContext(void 0);function As({value:e,children:t}){return(0,L.jsx)(ks.Provider,{value:e,children:t})}function js(e){let{theme:t,name:n,props:r}=e;if(!t||!t.components||!t.components[n])return r;let i=t.components[n];return i.defaultProps?Qo(i.defaultProps,r,t.components.mergeClassNameAndStyle):!i.styleOverrides&&!i.variants?Qo(i,r,t.components.mergeClassNameAndStyle):r}function Ms({props:e,name:t}){return js({props:e,name:t,theme:{components:x.useContext(ks)}})}var Ns=As,Ps=0;function Fs(e){let[t,n]=x.useState(e),r=e||t;return x.useEffect(()=>{t??(Ps+=1,n(`mui-${Ps}`))},[t]),r}var Is={...x}.useId;function Ls(e){if(Is!==void 0){let t=Is();return e??t}return Fs(e)}function Rs(e){let t=So(),n=Ls()||``,{modularCssLayers:r}=e,i=`mui.global, mui.components, mui.theme, mui.custom, mui.sx`;return i=!r||t!==null?``:typeof r==`string`?r.replace(/mui(?!\.)/g,i):`@layer ${i};`,$o(()=>{let e=document.querySelector(`head`);if(!e)return;let t=e.firstChild;if(i){if(t&&t.hasAttribute?.(`data-mui-layer-order`)&&t.getAttribute(`data-mui-layer-order`)===n)return;let r=document.createElement(`style`);r.setAttribute(`data-mui-layer-order`,n),r.textContent=i,e.prepend(r)}else e.querySelector(`style[data-mui-layer-order="${n}"]`)?.remove()},[i,n]),i?(0,L.jsx)(Oo,{styles:i}):null}var zs={};function Bs(e,t,n,r=!1){return x.useMemo(()=>{let i=e&&t[e]||t;if(typeof n==`function`){let a=n(i),o=e?{...t,[e]:a}:a;return r?()=>o:o}return e?{...t,[e]:n}:{...t,...n}},[e,t,n,r])}function Vs(e){let{children:t,theme:n,themeId:r}=e,i=So(zs),a=bs()||zs,o=Bs(r,i,n),s=Bs(r,a,n,!0),c=(r?o[r]:o).direction===`rtl`,l=Rs(o);return(0,L.jsx)(ws,{theme:s,children:(0,L.jsx)(bi.Provider,{value:o,children:(0,L.jsx)(Os,{value:c,children:(0,L.jsxs)(Ns,{value:r?o[r].components:o.components,children:[l,t]})})})})}var Hs=Vs,Us={theme:void 0};function Ws(e){let t,n;return function(r){let i=t;return(i===void 0||r.theme!==n)&&(Us.theme=r.theme,i=Bo(e(Us)),t=i,n=r.theme),i}}const Gs=`mode`,Ks=`color-scheme`;function qs(e){let{defaultMode:t=`system`,defaultLightColorScheme:n=`light`,defaultDarkColorScheme:r=`dark`,modeStorageKey:i=Gs,colorSchemeStorageKey:a=Ks,attribute:o=`data-color-scheme`,colorSchemeNode:s=`document.documentElement`,nonce:c}=e||{},l=``,u=o;if(o===`class`&&(u=`.%s`),o===`data`&&(u=`[data-%s]`),u.startsWith(`.`)){let e=u.substring(1);l+=`${s}.classList.remove('${e}'.replace('%s', light), '${e}'.replace('%s', dark));
      ${s}.classList.add('${e}'.replace('%s', colorScheme));`}let d=u.match(/\[([^[\]]+)\]/);if(d){let[e,t]=d[1].split(`=`);t||(l+=`${s}.removeAttribute('${e}'.replace('%s', light));
      ${s}.removeAttribute('${e}'.replace('%s', dark));`),l+=`
      ${s}.setAttribute('${e}'.replace('%s', colorScheme), ${t?`${t}.replace('%s', colorScheme)`:`""`});`}else l+=`${s}.setAttribute('${u}', colorScheme);`;return(0,L.jsx)(`script`,{suppressHydrationWarning:!0,nonce:typeof window>`u`?c:``,dangerouslySetInnerHTML:{__html:`(function() {
try {
  let colorScheme = '';
  const mode = localStorage.getItem('${i}') || '${t}';
  const dark = localStorage.getItem('${a}-dark') || '${r}';
  const light = localStorage.getItem('${a}-light') || '${n}';
  if (mode === 'system') {
    // handle system mode
    const mql = window.matchMedia('(prefers-color-scheme: dark)');
    if (mql.matches) {
      colorScheme = dark
    } else {
      colorScheme = light
    }
  }
  if (mode === 'light') {
    colorScheme = light;
  }
  if (mode === 'dark') {
    colorScheme = dark;
  }
  if (colorScheme) {
    ${l}
  }
} catch(e){}})();`}},`mui-color-scheme-init`)}function Js(){}var Ys=({key:e,storageWindow:t})=>(!t&&typeof window<`u`&&(t=window),{get(n){if(typeof window>`u`)return;if(!t)return n;let r;try{r=t.localStorage.getItem(e)}catch{}return r||n},set:n=>{if(t)try{t.localStorage.setItem(e,n)}catch{}},subscribe:n=>{if(!t)return Js;let r=t=>{let r=t.newValue;t.key===e&&n(r)};return t.addEventListener(`storage`,r),()=>{t.removeEventListener(`storage`,r)}}});function Xs(){}function Zs(e){if(typeof window<`u`&&typeof window.matchMedia==`function`&&e===`system`)return window.matchMedia(`(prefers-color-scheme: dark)`).matches?`dark`:`light`}function Qs(e,t){if(e.mode===`light`||e.mode===`system`&&e.systemMode===`light`)return t(`light`);if(e.mode===`dark`||e.mode===`system`&&e.systemMode===`dark`)return t(`dark`)}function $s(e){return Qs(e,t=>{if(t===`light`)return e.lightColorScheme;if(t===`dark`)return e.darkColorScheme})}function ec(e){let{defaultMode:t=`light`,defaultLightColorScheme:n,defaultDarkColorScheme:r,supportedColorSchemes:i=[],modeStorageKey:a=Gs,colorSchemeStorageKey:o=Ks,storageWindow:s=typeof window>`u`?void 0:window,storageManager:c=Ys,noSsr:l=!1}=e,u=i.join(`,`),d=i.length>1,f=x.useMemo(()=>c?.({key:a,storageWindow:s}),[c,a,s]),p=x.useMemo(()=>c?.({key:`${o}-light`,storageWindow:s}),[c,o,s]),m=x.useMemo(()=>c?.({key:`${o}-dark`,storageWindow:s}),[c,o,s]),[h,g]=x.useState(()=>{let e=f?.get(t)||t,i=p?.get(n)||n,a=m?.get(r)||r;return{mode:e,systemMode:Zs(e),lightColorScheme:i,darkColorScheme:a}}),[_,v]=x.useState(l||!d);x.useEffect(()=>{v(!0)},[]);let y=$s(h),b=x.useCallback(e=>{g(n=>{if(e===n.mode)return n;let r=e??t;return f?.set(r),{...n,mode:r,systemMode:Zs(r)}})},[f,t]),S=x.useCallback(e=>{e?typeof e==`string`?e&&!u.includes(e)?console.error(`\`${e}\` does not exist in \`theme.colorSchemes\`.`):g(t=>{let n={...t};return Qs(t,t=>{t===`light`&&(p?.set(e),n.lightColorScheme=e),t===`dark`&&(m?.set(e),n.darkColorScheme=e)}),n}):g(t=>{let i={...t},a=e.light===null?n:e.light,o=e.dark===null?r:e.dark;return a&&(u.includes(a)?(i.lightColorScheme=a,p?.set(a)):console.error(`\`${a}\` does not exist in \`theme.colorSchemes\`.`)),o&&(u.includes(o)?(i.darkColorScheme=o,m?.set(o)):console.error(`\`${o}\` does not exist in \`theme.colorSchemes\`.`)),i}):g(e=>(p?.set(n),m?.set(r),{...e,lightColorScheme:n,darkColorScheme:r}))},[u,p,m,n,r]),C=x.useCallback(e=>{h.mode===`system`&&g(t=>{let n=e?.matches?`dark`:`light`;return t.systemMode===n?t:{...t,systemMode:n}})},[h.mode]),w=x.useRef(C);return w.current=C,x.useEffect(()=>{if(typeof window.matchMedia!=`function`||!d)return;let e=(...e)=>w.current(...e),t=window.matchMedia(`(prefers-color-scheme: dark)`);return t.addListener(e),e(t),()=>{t.removeListener(e)}},[d]),x.useEffect(()=>{if(d){let e=f?.subscribe(e=>{(!e||[`light`,`dark`,`system`].includes(e))&&b(e||t)})||Xs,n=p?.subscribe(e=>{(!e||u.match(e))&&S({light:e})})||Xs,r=m?.subscribe(e=>{(!e||u.match(e))&&S({dark:e})})||Xs;return()=>{e(),n(),r()}}},[S,b,u,t,s,d,f,p,m]),{...h,mode:_?h.mode:void 0,systemMode:_?h.systemMode:void 0,colorScheme:_?y:void 0,setMode:b,setColorScheme:S}}function tc(e){let{themeId:t,theme:n={},modeStorageKey:r=Gs,colorSchemeStorageKey:i=Ks,disableTransitionOnChange:a=!1,defaultColorScheme:o,resolveTheme:s}=e,c={allColorSchemes:[],colorScheme:void 0,darkColorScheme:void 0,lightColorScheme:void 0,mode:void 0,setColorScheme:()=>{},setMode:()=>{},systemMode:void 0},l=x.createContext(void 0),u=()=>x.useContext(l)||c,d={},f={};function p(e){let{children:c,theme:u,modeStorageKey:p=r,colorSchemeStorageKey:m=i,disableTransitionOnChange:h=a,storageManager:g,storageWindow:_=typeof window>`u`?void 0:window,documentNode:v=typeof document>`u`?void 0:document,colorSchemeNode:y=typeof document>`u`?void 0:document.documentElement,disableNestedContext:b=!1,disableStyleSheetGeneration:S=!1,defaultMode:C=`system`,forceThemeRerender:w=!1,noSsr:T}=e,E=x.useRef(!1),D=bs(),O=x.useContext(l),k=!!O&&!b,ee=x.useMemo(()=>u||(typeof n==`function`?n():n),[u]),A=ee[t],j=A||ee,{colorSchemes:M=d,components:te=f,cssVarPrefix:N}=j,P=Object.keys(M).filter(e=>!!M[e]).join(`,`),F=x.useMemo(()=>P.split(`,`),[P]),ne=typeof o==`string`?o:o.light,re=typeof o==`string`?o:o.dark,{mode:ie,setMode:ae,systemMode:oe,lightColorScheme:I,darkColorScheme:se,colorScheme:ce,setColorScheme:le}=ec({supportedColorSchemes:F,defaultLightColorScheme:ne,defaultDarkColorScheme:re,modeStorageKey:p,colorSchemeStorageKey:m,defaultMode:M[ne]&&M[re]?C:M[j.defaultColorScheme]?.palette?.mode||j.palette?.mode,storageManager:g,storageWindow:_,noSsr:T}),ue=ie,de=ce;k&&(ue=O.mode,de=O.colorScheme);let fe=de||j.defaultColorScheme;j.vars&&!w&&(fe=j.defaultColorScheme);let pe=x.useMemo(()=>{let e=j.generateThemeVars?.()||j.vars,t={...j,components:te,colorSchemes:M,cssVarPrefix:N,vars:e};if(typeof t.generateSpacing==`function`&&(t.spacing=t.generateSpacing()),fe){let e=M[fe];e&&typeof e==`object`&&Object.keys(e).forEach(n=>{e[n]&&typeof e[n]==`object`?t[n]={...t[n],...e[n]}:t[n]=e[n]})}return s?s(t):t},[j,fe,te,M,N]),me=j.colorSchemeSelector;$o(()=>{if(de&&y&&me&&me!==`media`){let e=me,t=me;if(e===`class`&&(t=`.%s`),e===`data`&&(t=`[data-%s]`),e?.startsWith(`data-`)&&!e.includes(`%s`)&&(t=`[${e}="%s"]`),t.startsWith(`.`))y.classList.remove(...F.map(e=>t.substring(1).replace(`%s`,e))),y.classList.add(t.substring(1).replace(`%s`,de));else{let e=t.replace(`%s`,de).match(/\[([^\]]+)\]/);if(e){let[t,n]=e[1].split(`=`);n||F.forEach(e=>{y.removeAttribute(t.replace(de,e))}),y.setAttribute(t,n?n.replace(/"|'/g,``):``)}else y.setAttribute(t,de)}}},[de,me,y,F]),x.useEffect(()=>{let e;if(h&&E.current&&v){let t=v.createElement(`style`);t.appendChild(v.createTextNode(`*{-webkit-transition:none!important;-moz-transition:none!important;-o-transition:none!important;-ms-transition:none!important;transition:none!important}`)),v.head.appendChild(t),window.getComputedStyle(v.body),e=setTimeout(()=>{v.head.removeChild(t)},1)}return()=>{clearTimeout(e)}},[de,h,v]),x.useEffect(()=>(E.current=!0,()=>{E.current=!1}),[]);let he=x.useMemo(()=>({allColorSchemes:F,colorScheme:de,darkColorScheme:se,lightColorScheme:I,mode:ue,setColorScheme:le,setMode:ae,systemMode:oe}),[F,de,se,I,ue,le,ae,oe,pe.colorSchemeSelector]),ge=!0;(S||j.cssVariables===!1||k&&D?.cssVarPrefix===N)&&(ge=!1);let _e=(0,L.jsxs)(x.Fragment,{children:[(0,L.jsx)(Hs,{themeId:A?t:void 0,theme:pe,children:c}),ge&&(0,L.jsx)(Ui,{styles:pe.generateStyleSheets?.()||[]})]});return k?_e:(0,L.jsx)(l.Provider,{value:he,children:_e})}let m=typeof o==`string`?o:o.light,h=typeof o==`string`?o:o.dark;return{CssVarsProvider:p,useColorScheme:u,getInitColorSchemeScript:e=>qs({colorSchemeStorageKey:i,defaultLightColorScheme:m,defaultDarkColorScheme:h,modeStorageKey:r,...e})}}function nc(e=``){function t(...n){if(!n.length)return``;let r=n[0];return typeof r==`string`&&!r.match(/(#|\(|\)|(-?(\d*\.)?\d+)(px|em|%|ex|ch|rem|vw|vh|vmin|vmax|cm|mm|in|pt|pc))|^(-?(\d*\.)?\d+)$|(\d+ \d+ \d+)/)?`, var(--${e?`${e}-`:``}${r}${t(...n.slice(1))})`:`, ${r}`}return(n,...r)=>`var(--${e?`${e}-`:``}${n}${t(...r)})`}const rc=(e,t,n,r=[])=>{let i=e;t.forEach((e,a)=>{a===t.length-1?Array.isArray(i)?i[Number(e)]=n:i&&typeof i==`object`&&(i[e]=n):i&&typeof i==`object`&&(i[e]||(i[e]=r.includes(e)?[]:{}),i=i[e])})},ic=(e,t,n)=>{function r(e,i=[],a=[]){Object.entries(e).forEach(([e,o])=>{(!n||n&&!n([...i,e]))&&o!=null&&(typeof o==`object`&&Object.keys(o).length>0?r(o,[...i,e],Array.isArray(o)?[...a,e]:a):t([...i,e],o,a))})}r(e)};var ac=(e,t)=>typeof t==`number`?[`lineHeight`,`fontWeight`,`opacity`,`zIndex`].some(t=>e.includes(t))||e[e.length-1].toLowerCase().includes(`opacity`)?t:`${t}px`:t;function oc(e,t){let{prefix:n,shouldSkipGeneratingVar:r}=t||{},i={},a={},o={};return ic(e,(e,t,s)=>{if((typeof t==`string`||typeof t==`number`)&&(!r||!r(e,t))){let r=`--${n?`${n}-`:``}${e.join(`-`)}`,c=ac(e,t);Object.assign(i,{[r]:c}),rc(a,e,`var(${r})`,s),rc(o,e,`var(${r}, ${c})`,s)}},e=>e[0]===`vars`),{css:i,vars:a,varsWithDefaults:o}}function sc(e,t={}){let{getSelector:n=_,disableCssColorScheme:r,colorSchemeSelector:i,enableContrastVars:a}=t,{colorSchemes:o={},components:s,defaultColorScheme:c=`light`,...l}=e,{vars:u,css:d,varsWithDefaults:f}=oc(l,t),p=f,m={},{[c]:h,...g}=o;if(Object.entries(g||{}).forEach(([e,n])=>{let{vars:r,css:i,varsWithDefaults:a}=oc(n,t);p=Qi(p,a),m[e]={css:i,vars:r}}),h){let{css:e,vars:n,varsWithDefaults:r}=oc(h,t);p=Qi(p,r),m[c]={css:e,vars:n}}function _(t,n){let r=i;if(i===`class`&&(r=`.%s`),i===`data`&&(r=`[data-%s]`),i?.startsWith(`data-`)&&!i.includes(`%s`)&&(r=`[${i}="%s"]`),t){if(r===`media`)return e.defaultColorScheme===t?`:root`:{[`@media (prefers-color-scheme: ${o[t]?.palette?.mode||t})`]:{":root":n}};if(r)return e.defaultColorScheme===t?`:root, ${r.replace(`%s`,String(t))}`:r.replace(`%s`,String(t))}return`:root`}return{vars:p,generateThemeVars:()=>{let e={...u};return Object.entries(m).forEach(([,{vars:t}])=>{e=Qi(e,t)}),e},generateStyleSheets:()=>{let t=[],i=e.defaultColorScheme||`light`;function s(e,n){Object.keys(n).length&&t.push(typeof e==`string`?{[e]:{...n}}:e)}s(n(void 0,{...d}),d);let{[i]:c,...l}=m;if(c){let{css:e}=c,t=o[i]?.palette?.mode,a=!r&&t?{colorScheme:t,...e}:{...e};s(n(i,{...a}),a)}return Object.entries(l).forEach(([e,{css:t}])=>{let i=o[e]?.palette?.mode,a=!r&&i?{colorScheme:i,...t}:{...t};s(n(e,{...a}),a)}),a&&t.push({":root":{"--__l-threshold":`0.7`,"--__l":`clamp(0, (l / var(--__l-threshold) - 1) * -infinity, 1)`,"--__a":`clamp(0.87, (l / var(--__l-threshold) - 1) * -infinity, 1)`}}),t}}}var cc=sc;function lc(e){return function(t){return e===`media`?`@media (prefers-color-scheme: ${t})`:e?e.startsWith(`data-`)&&!e.includes(`%s`)?`[${e}="${t}"] &`:e===`class`?`.${t} &`:e===`data`?`[data-${t}] &`:`${e.replace(`%s`,t)} &`:`&`}}function uc(e,t,n=void 0){let r={};for(let i in e){let a=e[i],o=``,s=!0;for(let e=0;e<a.length;e+=1){let r=a[e];r&&(o+=(s===!0?``:` `)+t(r),s=!1,n&&n[r]&&(o+=` `+n[r]))}r[i]=o}return r}function dc(e,t){return x.isValidElement(e)&&t.indexOf(e.type.muiName??e.type?._payload?.value?.muiName)!==-1}function fc(){return{text:{primary:`rgba(0, 0, 0, 0.87)`,secondary:`rgba(0, 0, 0, 0.6)`,disabled:`rgba(0, 0, 0, 0.38)`},divider:`rgba(0, 0, 0, 0.12)`,background:{paper:bn.white,default:bn.white},action:{active:`rgba(0, 0, 0, 0.54)`,hover:`rgba(0, 0, 0, 0.04)`,hoverOpacity:.04,selected:`rgba(0, 0, 0, 0.08)`,selectedOpacity:.08,disabled:`rgba(0, 0, 0, 0.26)`,disabledBackground:`rgba(0, 0, 0, 0.12)`,disabledOpacity:.38,focus:`rgba(0, 0, 0, 0.12)`,focusOpacity:.12,activatedOpacity:.12}}}const pc=fc();function mc(){return{text:{primary:bn.white,secondary:`rgba(255, 255, 255, 0.7)`,disabled:`rgba(255, 255, 255, 0.5)`,icon:`rgba(255, 255, 255, 0.5)`},divider:`rgba(255, 255, 255, 0.12)`,background:{paper:`#121212`,default:`#121212`},action:{active:bn.white,hover:`rgba(255, 255, 255, 0.08)`,hoverOpacity:.08,selected:`rgba(255, 255, 255, 0.16)`,selectedOpacity:.16,disabled:`rgba(255, 255, 255, 0.3)`,disabledBackground:`rgba(255, 255, 255, 0.12)`,disabledOpacity:.38,focus:`rgba(255, 255, 255, 0.12)`,focusOpacity:.12,activatedOpacity:.24}}}const hc=mc();function gc(e,t,n,r){let i=r.light||r,a=r.dark||r*1.5;e[t]||(e.hasOwnProperty(n)?e[t]=e[n]:t===`light`?e.light=hs(e.main,i):t===`dark`&&(e.dark=ps(e.main,a)))}function _c(e,t,n,r,i){let a=i.light||i,o=i.dark||i*1.5;t[n]||(t.hasOwnProperty(r)?t[n]=t[r]:n===`light`?t.light=`color-mix(in ${e}, ${t.main}, #fff ${(a*100).toFixed(0)}%)`:n===`dark`&&(t.dark=`color-mix(in ${e}, ${t.main}, #000 ${(o*100).toFixed(0)}%)`))}function vc(e=`light`){return e===`dark`?{main:Cn[200],light:Cn[50],dark:Cn[400]}:{main:Cn[700],light:Cn[400],dark:Cn[800]}}function yc(e=`light`){return e===`dark`?{main:Sn[200],light:Sn[50],dark:Sn[400]}:{main:Sn[500],light:Sn[300],dark:Sn[700]}}function bc(e=`light`){return e===`dark`?{main:xn[500],light:xn[300],dark:xn[700]}:{main:xn[700],light:xn[400],dark:xn[800]}}function xc(e=`light`){return e===`dark`?{main:wn[400],light:wn[300],dark:wn[700]}:{main:wn[700],light:wn[500],dark:wn[900]}}function Sc(e=`light`){return e===`dark`?{main:Tn[400],light:Tn[300],dark:Tn[700]}:{main:Tn[800],light:Tn[500],dark:Tn[900]}}function Cc(e=`light`){return e===`dark`?{main:En[400],light:En[300],dark:En[700]}:{main:`#ed6c02`,light:En[500],dark:En[900]}}function wc(e){return`oklch(from ${e} var(--__l) 0 h / var(--__a))`}function Tc(e){let{mode:t=`light`,contrastThreshold:n=3,tonalOffset:r=.2,colorSpace:i,...a}=e,o=e.primary||vc(t),s=e.secondary||yc(t),c=e.error||bc(t),l=e.info||xc(t),u=e.success||Sc(t),d=e.warning||Cc(t);function f(e){return i?wc(e):us(e,hc.text.primary)>=n?hc.text.primary:pc.text.primary}let p=({color:e,name:t,mainShade:n=500,lightShade:a=300,darkShade:o=700})=>{if(e={...e},!e.main&&e[n]&&(e.main=e[n]),!e.hasOwnProperty(`main`))throw Error(On(11,t?` (${t})`:``,n));if(typeof e.main!=`string`)throw Error(On(12,t?` (${t})`:``,JSON.stringify(e.main)));return i?(_c(i,e,`light`,a,r),_c(i,e,`dark`,o,r)):(gc(e,`light`,a,r),gc(e,`dark`,o,r)),e.contrastText||=f(e.main),e},m;return t===`light`?m=fc():t===`dark`&&(m=mc()),Qi({common:{...bn},mode:t,primary:p({color:o,name:`primary`}),secondary:p({color:s,name:`secondary`,mainShade:`A400`,lightShade:`A200`,darkShade:`A700`}),error:p({color:c,name:`error`}),warning:p({color:d,name:`warning`}),info:p({color:l,name:`info`}),success:p({color:u,name:`success`}),grey:Dn,contrastThreshold:n,getContrastText:f,augmentColor:p,tonalOffset:r,...m},a)}function Ec(e){let t={};return Object.entries(e).forEach(e=>{let[n,r]=e;typeof r==`object`&&(t[n]=`${r.fontStyle?`${r.fontStyle} `:``}${r.fontVariant?`${r.fontVariant} `:``}${r.fontWeight?`${r.fontWeight} `:``}${r.fontStretch?`${r.fontStretch} `:``}${r.fontSize||``}${r.lineHeight?`/${r.lineHeight} `:``}${r.fontFamily||``}`)}),t}function Dc(e,t){return{toolbar:{minHeight:56,[e.up(`xs`)]:{"@media (orientation: landscape)":{minHeight:48}},[e.up(`sm`)]:{minHeight:64}},...t}}function Oc(e){return Math.round(e*1e5)/1e5}var kc={textTransform:`uppercase`},Ac=`"Roboto", "Helvetica", "Arial", sans-serif`;function jc(e,t){let{fontFamily:n=Ac,fontSize:r=14,fontWeightLight:i=300,fontWeightRegular:a=400,fontWeightMedium:o=500,fontWeightBold:s=700,htmlFontSize:c=16,allVariants:l,pxToRem:u,...d}=typeof t==`function`?t(e):t,f=r/14,p=u||(e=>`${e/c*f}rem`),m=(e,t,r,i,a)=>({fontFamily:n,fontWeight:e,fontSize:p(t),lineHeight:r,...n===Ac?{letterSpacing:`${Oc(i/t)}em`}:{},...a,...l});return Qi({htmlFontSize:c,pxToRem:p,fontFamily:n,fontSize:r,fontWeightLight:i,fontWeightRegular:a,fontWeightMedium:o,fontWeightBold:s,h1:m(i,96,1.167,-1.5),h2:m(i,60,1.2,-.5),h3:m(a,48,1.167,0),h4:m(a,34,1.235,.25),h5:m(a,24,1.334,0),h6:m(o,20,1.6,.15),subtitle1:m(a,16,1.75,.15),subtitle2:m(o,14,1.57,.1),body1:m(a,16,1.5,.15),body2:m(a,14,1.43,.15),button:m(o,14,1.75,.4,kc),caption:m(a,12,1.66,.4),overline:m(a,12,2.66,1,kc),inherit:{fontFamily:`inherit`,fontWeight:`inherit`,fontSize:`inherit`,lineHeight:`inherit`,letterSpacing:`inherit`}},d,{clone:!1})}var Mc=.2,Nc=.14,Pc=.12;function Fc(...e){return[`${e[0]}px ${e[1]}px ${e[2]}px ${e[3]}px rgba(0,0,0,${Mc})`,`${e[4]}px ${e[5]}px ${e[6]}px ${e[7]}px rgba(0,0,0,${Nc})`,`${e[8]}px ${e[9]}px ${e[10]}px ${e[11]}px rgba(0,0,0,${Pc})`].join(`,`)}var Ic=[`none`,Fc(0,2,1,-1,0,1,1,0,0,1,3,0),Fc(0,3,1,-2,0,2,2,0,0,1,5,0),Fc(0,3,3,-2,0,3,4,0,0,1,8,0),Fc(0,2,4,-1,0,4,5,0,0,1,10,0),Fc(0,3,5,-1,0,5,8,0,0,1,14,0),Fc(0,3,5,-1,0,6,10,0,0,1,18,0),Fc(0,4,5,-2,0,7,10,1,0,2,16,1),Fc(0,5,5,-3,0,8,10,1,0,3,14,2),Fc(0,5,6,-3,0,9,12,1,0,3,16,2),Fc(0,6,6,-3,0,10,14,1,0,4,18,3),Fc(0,6,7,-4,0,11,15,1,0,4,20,3),Fc(0,7,8,-4,0,12,17,2,0,5,22,4),Fc(0,7,8,-4,0,13,19,2,0,5,24,4),Fc(0,7,9,-4,0,14,21,2,0,5,26,4),Fc(0,8,9,-5,0,15,22,2,0,6,28,5),Fc(0,8,10,-5,0,16,24,2,0,6,30,5),Fc(0,8,11,-5,0,17,26,2,0,6,32,5),Fc(0,9,11,-5,0,18,28,2,0,7,34,6),Fc(0,9,12,-6,0,19,29,2,0,7,36,6),Fc(0,10,13,-6,0,20,31,3,0,8,38,7),Fc(0,10,13,-6,0,21,33,3,0,8,40,7),Fc(0,10,14,-6,0,22,35,3,0,8,42,7),Fc(0,11,14,-7,0,23,36,3,0,9,44,8),Fc(0,11,15,-7,0,24,38,3,0,9,46,8)];const Lc={easeInOut:`cubic-bezier(0.4, 0, 0.2, 1)`,easeOut:`cubic-bezier(0.0, 0, 0.2, 1)`,easeIn:`cubic-bezier(0.4, 0, 1, 1)`,sharp:`cubic-bezier(0.4, 0, 0.6, 1)`},Rc={shortest:150,shorter:200,short:250,standard:300,complex:375,enteringScreen:225,leavingScreen:195};function zc(e){return`${Math.round(e)}ms`}function Bc(e){if(!e)return 0;let t=e/36;return Math.min(Math.round((4+15*t**.25+t/5)*10),3e3)}function Vc(e){let t={...Lc,...e.easing},n={...Rc,...e.duration};return{getAutoHeightDuration:Bc,create:(e=[`all`],r={})=>{let{duration:i=n.standard,easing:a=t.easeInOut,delay:o=0,...s}=r;return(Array.isArray(e)?e:[e]).map(e=>`${e} ${typeof i==`string`?i:zc(i)} ${a} ${typeof o==`string`?o:zc(o)}`).join(`,`)},...e,easing:t,duration:n}}var Hc={mobileStepper:1e3,fab:1050,speedDial:1050,appBar:1100,drawer:1200,modal:1300,snackbar:1400,tooltip:1500};function Uc(e){return Xi(e)||e===void 0||typeof e==`string`||typeof e==`boolean`||typeof e==`number`||Array.isArray(e)}function Wc(e={}){let t={...e};function n(e){let t=Object.entries(e);for(let r=0;r<t.length;r++){let[i,a]=t[r];!Uc(a)||i.startsWith(`unstable_`)?delete e[i]:Xi(a)&&(e[i]={...a},n(e[i]))}}return n(t),`import { unstable_createBreakpoints as createBreakpoints, createTransitions } from '@mui/material/styles';

const theme = ${JSON.stringify(t,null,2)};

theme.breakpoints = createBreakpoints(theme.breakpoints || {});
theme.transitions = createTransitions(theme.transitions || {});

export default theme;`}function Gc(e){return typeof e==`number`?`${(e*100).toFixed(0)}%`:`calc((${e}) * 100%)`}var Kc=e=>{if(!Number.isNaN(+e))return+e;let t=e.match(/\d*\.?\d+/g);if(!t)return 0;let n=0;for(let e=0;e<t.length;e+=1)n+=+t[e];return n};function qc(e){Object.assign(e,{alpha(t,n){let r=this||e;return r.colorSpace?`oklch(from ${t} l c h / ${typeof n==`string`?`calc(${n})`:n})`:r.vars?`rgba(${t.replace(/var\(--([^,\s)]+)(?:,[^)]+)?\)+/g,`var(--$1Channel)`)} / ${typeof n==`string`?`calc(${n})`:n})`:ds(t,Kc(n))},lighten(t,n){let r=this||e;return r.colorSpace?`color-mix(in ${r.colorSpace}, ${t}, #fff ${Gc(n)})`:hs(t,n)},darken(t,n){let r=this||e;return r.colorSpace?`color-mix(in ${r.colorSpace}, ${t}, #000 ${Gc(n)})`:ps(t,n)}})}function Jc(e={},...t){let{breakpoints:n,mixins:r={},spacing:i,palette:a={},transitions:o={},typography:s={},shape:c,colorSpace:l,...u}=e;if(e.vars&&e.generateThemeVars===void 0)throw Error(On(20));let d=Tc({...a,colorSpace:l}),f=yo(e),p=Qi(f,{mixins:Dc(f.breakpoints,r),palette:d,shadows:Ic.slice(),typography:jc(d,s),transitions:Vc(o),zIndex:{...Hc}});return p=Qi(p,u),p=t.reduce((e,t)=>Qi(e,t),p),p.unstable_sxConfig={...fo,...u?.unstable_sxConfig},p.unstable_sx=function(e){return go({sx:e,theme:this})},p.toRuntimeSource=Wc,qc(p),p}var Yc=Jc;function Xc(e){let t;return t=e<1?5.11916*e**2:4.5*Math.log(e+1)+2,Math.round(t*10)/1e3}var Zc=[...Array(25)].map((e,t)=>{if(t===0)return`none`;let n=Xc(t);return`linear-gradient(rgba(255 255 255 / ${n}), rgba(255 255 255 / ${n}))`});function Qc(e){return{inputPlaceholder:e===`dark`?.5:.42,inputUnderline:e===`dark`?.7:.42,switchTrackDisabled:e===`dark`?.2:.12,switchTrack:e===`dark`?.3:.38}}function $c(e){return e===`dark`?Zc:[]}function el(e){let{palette:t={mode:`light`},opacity:n,overlays:r,colorSpace:i,...a}=e,o=Tc({...t,colorSpace:i});return{palette:o,opacity:{...Qc(o.mode),...n},overlays:r||$c(o.mode),...a}}function tl(e){return!!e[0].match(/(cssVarPrefix|colorSchemeSelector|modularCssLayers|rootSelector|typography|mixins|breakpoints|direction|transitions)/)||!!e[0].match(/sxConfig$/)||e[0]===`palette`&&!!e[1]?.match(/(mode|contrastThreshold|tonalOffset)/)}var nl=e=>[...[...Array(25)].map((t,n)=>`--${e?`${e}-`:``}overlays-${n}`),`--${e?`${e}-`:``}palette-AppBar-darkBg`,`--${e?`${e}-`:``}palette-AppBar-darkColor`],rl=e=>(t,n)=>{let r=e.rootSelector||`:root`,i=e.colorSchemeSelector,a=i;if(i===`class`&&(a=`.%s`),i===`data`&&(a=`[data-%s]`),i?.startsWith(`data-`)&&!i.includes(`%s`)&&(a=`[${i}="%s"]`),e.defaultColorScheme===t){if(t===`dark`){let i={};return nl(e.cssVarPrefix).forEach(e=>{i[e]=n[e],delete n[e]}),a===`media`?{[r]:n,"@media (prefers-color-scheme: dark)":{[r]:i}}:a?{[a.replace(`%s`,t)]:i,[`${r}, ${a.replace(`%s`,t)}`]:n}:{[r]:{...n,...i}}}if(a&&a!==`media`)return`${r}, ${a.replace(`%s`,String(t))}`}else if(t){if(a===`media`)return{[`@media (prefers-color-scheme: ${String(t)})`]:{[r]:n}};if(a)return a.replace(`%s`,String(t))}return r};function il(e,t){t.forEach(t=>{e[t]||(e[t]={})})}function B(e,t,n){!e[t]&&n&&(e[t]=n)}function al(e){return typeof e!=`string`||!e.startsWith(`hsl`)?e:cs(e)}function ol(e,t){`${t}Channel`in e||(e[`${t}Channel`]=os(al(e[t]),`MUI: Can't create \`palette.${t}Channel\` because \`palette.${t}\` is not one of these formats: #nnn, #nnnnnn, rgb(), rgba(), hsl(), hsla(), color().
To suppress this warning, you need to explicitly provide the \`palette.${t}Channel\` as a string (in rgb format, for example "12 12 12") or undefined if you want to remove the channel token.`))}function sl(e){return typeof e==`number`?`${e}px`:typeof e==`string`||typeof e==`function`||Array.isArray(e)?e:`8px`}var cl=e=>{try{return e()}catch{}};const ll=(e=`mui`)=>nc(e);function ul(e,t,n,r,i){if(!n)return;n=n===!0?{}:n;let a=i===`dark`?`dark`:`light`;if(!r){t[i]=el({...n,palette:{mode:a,...n?.palette},colorSpace:e});return}let{palette:o,...s}=Yc({...r,palette:{mode:a,...n?.palette},colorSpace:e});return t[i]={...n,palette:o,opacity:{...Qc(a),...n?.opacity},overlays:n?.overlays||$c(a)},s}function dl(e={},...t){let{colorSchemes:n={light:!0},defaultColorScheme:r,disableCssColorScheme:i=!1,cssVarPrefix:a=`mui`,nativeColor:o=!1,shouldSkipGeneratingVar:s=tl,colorSchemeSelector:c=n.light&&n.dark?`media`:void 0,rootSelector:l=`:root`,...u}=e,d=Object.keys(n)[0],f=r||(n.light&&d!==`light`?`light`:d),p=ll(a),{[f]:m,light:h,dark:g,..._}=n,v={..._},y=m;if((f===`dark`&&!(`dark`in n)||f===`light`&&!(`light`in n))&&(y=!0),!y)throw Error(On(21,f));let b;o&&(b=`oklch`);let x=ul(b,v,y,u,f);h&&!v.light&&ul(b,v,h,void 0,`light`),g&&!v.dark&&ul(b,v,g,void 0,`dark`);let S={defaultColorScheme:f,...x,cssVarPrefix:a,colorSchemeSelector:c,rootSelector:l,getCssVar:p,colorSchemes:v,font:{...Ec(x.typography),...x.font},spacing:sl(u.spacing)};Object.keys(S.colorSchemes).forEach(e=>{let t=S.colorSchemes[e].palette,n=e=>{let n=e.split(`-`),r=n[1],i=n[2];return p(e,t[r][i])};t.mode===`light`&&(B(t.common,`background`,`#fff`),B(t.common,`onBackground`,`#000`)),t.mode===`dark`&&(B(t.common,`background`,`#000`),B(t.common,`onBackground`,`#fff`));function r(e,t,n){if(b){let r;return e===fs&&(r=`transparent ${((1-n)*100).toFixed(0)}%`),e===ms&&(r=`#000 ${(n*100).toFixed(0)}%`),e===gs&&(r=`#fff ${(n*100).toFixed(0)}%`),`color-mix(in ${b}, ${t}, ${r})`}return e(t,n)}if(il(t,[`Alert`,`AppBar`,`Avatar`,`Button`,`Chip`,`FilledInput`,`LinearProgress`,`Skeleton`,`Slider`,`SnackbarContent`,`SpeedDialAction`,`StepConnector`,`StepContent`,`Switch`,`TableCell`,`Tooltip`]),t.mode===`light`){B(t.Alert,`errorColor`,r(ms,t.error.light,.6)),B(t.Alert,`infoColor`,r(ms,t.info.light,.6)),B(t.Alert,`successColor`,r(ms,t.success.light,.6)),B(t.Alert,`warningColor`,r(ms,t.warning.light,.6)),B(t.Alert,`errorFilledBg`,n(`palette-error-main`)),B(t.Alert,`infoFilledBg`,n(`palette-info-main`)),B(t.Alert,`successFilledBg`,n(`palette-success-main`)),B(t.Alert,`warningFilledBg`,n(`palette-warning-main`)),B(t.Alert,`errorFilledColor`,cl(()=>t.getContrastText(t.error.main))),B(t.Alert,`infoFilledColor`,cl(()=>t.getContrastText(t.info.main))),B(t.Alert,`successFilledColor`,cl(()=>t.getContrastText(t.success.main))),B(t.Alert,`warningFilledColor`,cl(()=>t.getContrastText(t.warning.main))),B(t.Alert,`errorStandardBg`,r(gs,t.error.light,.9)),B(t.Alert,`infoStandardBg`,r(gs,t.info.light,.9)),B(t.Alert,`successStandardBg`,r(gs,t.success.light,.9)),B(t.Alert,`warningStandardBg`,r(gs,t.warning.light,.9)),B(t.Alert,`errorIconColor`,n(`palette-error-main`)),B(t.Alert,`infoIconColor`,n(`palette-info-main`)),B(t.Alert,`successIconColor`,n(`palette-success-main`)),B(t.Alert,`warningIconColor`,n(`palette-warning-main`)),B(t.AppBar,`defaultBg`,n(`palette-grey-100`)),B(t.Avatar,`defaultBg`,n(`palette-grey-400`)),B(t.Button,`inheritContainedBg`,n(`palette-grey-300`)),B(t.Button,`inheritContainedHoverBg`,n(`palette-grey-A100`)),B(t.Chip,`defaultBorder`,n(`palette-grey-400`)),B(t.Chip,`defaultAvatarColor`,n(`palette-grey-700`)),B(t.Chip,`defaultIconColor`,n(`palette-grey-700`)),B(t.FilledInput,`bg`,`rgba(0, 0, 0, 0.06)`),B(t.FilledInput,`hoverBg`,`rgba(0, 0, 0, 0.09)`),B(t.FilledInput,`disabledBg`,`rgba(0, 0, 0, 0.12)`),B(t.LinearProgress,`primaryBg`,r(gs,t.primary.main,.62)),B(t.LinearProgress,`secondaryBg`,r(gs,t.secondary.main,.62)),B(t.LinearProgress,`errorBg`,r(gs,t.error.main,.62)),B(t.LinearProgress,`infoBg`,r(gs,t.info.main,.62)),B(t.LinearProgress,`successBg`,r(gs,t.success.main,.62)),B(t.LinearProgress,`warningBg`,r(gs,t.warning.main,.62)),B(t.Skeleton,`bg`,b?r(fs,t.text.primary,.11):`rgba(${n(`palette-text-primaryChannel`)} / 0.11)`),B(t.Slider,`primaryTrack`,r(gs,t.primary.main,.62)),B(t.Slider,`secondaryTrack`,r(gs,t.secondary.main,.62)),B(t.Slider,`errorTrack`,r(gs,t.error.main,.62)),B(t.Slider,`infoTrack`,r(gs,t.info.main,.62)),B(t.Slider,`successTrack`,r(gs,t.success.main,.62)),B(t.Slider,`warningTrack`,r(gs,t.warning.main,.62));let e=b?r(ms,t.background.default,.6825):vs(t.background.default,.8);B(t.SnackbarContent,`bg`,e),B(t.SnackbarContent,`color`,cl(()=>b?hc.text.primary:t.getContrastText(e))),B(t.SpeedDialAction,`fabHoverBg`,vs(t.background.paper,.15)),B(t.StepConnector,`border`,n(`palette-grey-400`)),B(t.StepContent,`border`,n(`palette-grey-400`)),B(t.Switch,`defaultColor`,n(`palette-common-white`)),B(t.Switch,`defaultDisabledColor`,n(`palette-grey-100`)),B(t.Switch,`primaryDisabledColor`,r(gs,t.primary.main,.62)),B(t.Switch,`secondaryDisabledColor`,r(gs,t.secondary.main,.62)),B(t.Switch,`errorDisabledColor`,r(gs,t.error.main,.62)),B(t.Switch,`infoDisabledColor`,r(gs,t.info.main,.62)),B(t.Switch,`successDisabledColor`,r(gs,t.success.main,.62)),B(t.Switch,`warningDisabledColor`,r(gs,t.warning.main,.62)),B(t.TableCell,`border`,r(gs,r(fs,t.divider,1),.88)),B(t.Tooltip,`bg`,r(fs,t.grey[700],.92))}if(t.mode===`dark`){B(t.Alert,`errorColor`,r(gs,t.error.light,.6)),B(t.Alert,`infoColor`,r(gs,t.info.light,.6)),B(t.Alert,`successColor`,r(gs,t.success.light,.6)),B(t.Alert,`warningColor`,r(gs,t.warning.light,.6)),B(t.Alert,`errorFilledBg`,n(`palette-error-dark`)),B(t.Alert,`infoFilledBg`,n(`palette-info-dark`)),B(t.Alert,`successFilledBg`,n(`palette-success-dark`)),B(t.Alert,`warningFilledBg`,n(`palette-warning-dark`)),B(t.Alert,`errorFilledColor`,cl(()=>t.getContrastText(t.error.dark))),B(t.Alert,`infoFilledColor`,cl(()=>t.getContrastText(t.info.dark))),B(t.Alert,`successFilledColor`,cl(()=>t.getContrastText(t.success.dark))),B(t.Alert,`warningFilledColor`,cl(()=>t.getContrastText(t.warning.dark))),B(t.Alert,`errorStandardBg`,r(ms,t.error.light,.9)),B(t.Alert,`infoStandardBg`,r(ms,t.info.light,.9)),B(t.Alert,`successStandardBg`,r(ms,t.success.light,.9)),B(t.Alert,`warningStandardBg`,r(ms,t.warning.light,.9)),B(t.Alert,`errorIconColor`,n(`palette-error-main`)),B(t.Alert,`infoIconColor`,n(`palette-info-main`)),B(t.Alert,`successIconColor`,n(`palette-success-main`)),B(t.Alert,`warningIconColor`,n(`palette-warning-main`)),B(t.AppBar,`defaultBg`,n(`palette-grey-900`)),B(t.AppBar,`darkBg`,n(`palette-background-paper`)),B(t.AppBar,`darkColor`,n(`palette-text-primary`)),B(t.Avatar,`defaultBg`,n(`palette-grey-600`)),B(t.Button,`inheritContainedBg`,n(`palette-grey-800`)),B(t.Button,`inheritContainedHoverBg`,n(`palette-grey-700`)),B(t.Chip,`defaultBorder`,n(`palette-grey-700`)),B(t.Chip,`defaultAvatarColor`,n(`palette-grey-300`)),B(t.Chip,`defaultIconColor`,n(`palette-grey-300`)),B(t.FilledInput,`bg`,`rgba(255, 255, 255, 0.09)`),B(t.FilledInput,`hoverBg`,`rgba(255, 255, 255, 0.13)`),B(t.FilledInput,`disabledBg`,`rgba(255, 255, 255, 0.12)`),B(t.LinearProgress,`primaryBg`,r(ms,t.primary.main,.5)),B(t.LinearProgress,`secondaryBg`,r(ms,t.secondary.main,.5)),B(t.LinearProgress,`errorBg`,r(ms,t.error.main,.5)),B(t.LinearProgress,`infoBg`,r(ms,t.info.main,.5)),B(t.LinearProgress,`successBg`,r(ms,t.success.main,.5)),B(t.LinearProgress,`warningBg`,r(ms,t.warning.main,.5)),B(t.Skeleton,`bg`,b?r(fs,t.text.primary,.13):`rgba(${n(`palette-text-primaryChannel`)} / 0.13)`),B(t.Slider,`primaryTrack`,r(ms,t.primary.main,.5)),B(t.Slider,`secondaryTrack`,r(ms,t.secondary.main,.5)),B(t.Slider,`errorTrack`,r(ms,t.error.main,.5)),B(t.Slider,`infoTrack`,r(ms,t.info.main,.5)),B(t.Slider,`successTrack`,r(ms,t.success.main,.5)),B(t.Slider,`warningTrack`,r(ms,t.warning.main,.5));let e=b?r(gs,t.background.default,.985):vs(t.background.default,.98);B(t.SnackbarContent,`bg`,e),B(t.SnackbarContent,`color`,cl(()=>b?pc.text.primary:t.getContrastText(e))),B(t.SpeedDialAction,`fabHoverBg`,vs(t.background.paper,.15)),B(t.StepConnector,`border`,n(`palette-grey-600`)),B(t.StepContent,`border`,n(`palette-grey-600`)),B(t.Switch,`defaultColor`,n(`palette-grey-300`)),B(t.Switch,`defaultDisabledColor`,n(`palette-grey-600`)),B(t.Switch,`primaryDisabledColor`,r(ms,t.primary.main,.55)),B(t.Switch,`secondaryDisabledColor`,r(ms,t.secondary.main,.55)),B(t.Switch,`errorDisabledColor`,r(ms,t.error.main,.55)),B(t.Switch,`infoDisabledColor`,r(ms,t.info.main,.55)),B(t.Switch,`successDisabledColor`,r(ms,t.success.main,.55)),B(t.Switch,`warningDisabledColor`,r(ms,t.warning.main,.55)),B(t.TableCell,`border`,r(ms,r(fs,t.divider,1),.68)),B(t.Tooltip,`bg`,r(fs,t.grey[700],.92))}ol(t.background,`default`),ol(t.background,`paper`),ol(t.common,`background`),ol(t.common,`onBackground`),ol(t,`divider`),Object.keys(t).forEach(e=>{let n=t[e];e!==`tonalOffset`&&n&&typeof n==`object`&&(n.main&&B(t[e],`mainChannel`,os(al(n.main))),n.light&&B(t[e],`lightChannel`,os(al(n.light))),n.dark&&B(t[e],`darkChannel`,os(al(n.dark))),n.contrastText&&B(t[e],`contrastTextChannel`,os(al(n.contrastText))),e===`text`&&(ol(t[e],`primary`),ol(t[e],`secondary`)),e===`action`&&(n.active&&ol(t[e],`active`),n.selected&&ol(t[e],`selected`)))})}),S=t.reduce((e,t)=>Qi(e,t),S);let C={prefix:a,disableCssColorScheme:i,shouldSkipGeneratingVar:s,getSelector:rl(S),enableContrastVars:o},{vars:w,generateThemeVars:T,generateStyleSheets:E}=cc(S,C);return S.vars=w,Object.entries(S.colorSchemes[S.defaultColorScheme]).forEach(([e,t])=>{S[e]=t}),S.generateThemeVars=T,S.generateStyleSheets=E,S.generateSpacing=function(){return Ia(u.spacing,Oa(this))},S.getColorSchemeSelector=lc(c),S.spacing=S.generateSpacing(),S.shouldSkipGeneratingVar=s,S.unstable_sxConfig={...fo,...u?.unstable_sxConfig},S.unstable_sx=function(e){return go({sx:e,theme:this})},S.toRuntimeSource=Wc,S}function fl(e,t,n){e.colorSchemes&&n&&(e.colorSchemes[t]={...n!==!0&&n,palette:Tc({...n===!0?{}:n.palette,mode:t})})}function pl(e={},...t){let{palette:n,cssVariables:r=!1,colorSchemes:i=n?void 0:{light:!0},defaultColorScheme:a=n?.mode,...o}=e,s=a||`light`,c=i?.[s],l={...i,...n?{[s]:{...typeof c!=`boolean`&&c,palette:n}}:void 0};if(r===!1){if(!(`colorSchemes`in e))return Yc(e,...t);let r=n;`palette`in e||l[s]&&(l[s]===!0?s===`dark`&&(r={mode:`dark`}):r=l[s].palette);let i=Yc({...e,palette:r},...t);return i.defaultColorScheme=s,i.colorSchemes=l,i.palette.mode===`light`&&(i.colorSchemes.light={...l.light!==!0&&l.light,palette:i.palette},fl(i,`dark`,l.dark)),i.palette.mode===`dark`&&(i.colorSchemes.dark={...l.dark!==!0&&l.dark,palette:i.palette},fl(i,`light`,l.light)),i}return!n&&!(`light`in l)&&s===`light`&&(l.light=!0),dl({...o,colorSchemes:l,defaultColorScheme:s,...typeof r!=`boolean`&&r},...t)}var ml=pl();function hl(){let e=To(ml);return e.$$material||e}function gl(e){return e!==`ownerState`&&e!==`theme`&&e!==`sx`&&e!==`as`}var _l=gl,vl=e=>_l(e)&&e!==`classes`,V=Jo({themeId:kn,defaultTheme:ml,rootShouldForwardProp:vl});function yl({theme:e,...t}){let n=`$$material`in e?e[kn]:void 0;return(0,L.jsx)(Hs,{...t,themeId:n?kn:void 0,theme:n||e})}const bl={attribute:`data-mui-color-scheme`,colorSchemeStorageKey:`mui-color-scheme`,defaultLightColorScheme:`light`,defaultDarkColorScheme:`dark`,modeStorageKey:`mui-mode`};var{CssVarsProvider:xl,useColorScheme:Sl,getInitColorSchemeScript:Cl}=tc({themeId:kn,theme:()=>pl({cssVariables:!0}),colorSchemeStorageKey:bl.colorSchemeStorageKey,modeStorageKey:bl.modeStorageKey,defaultColorScheme:{light:bl.defaultLightColorScheme,dark:bl.defaultDarkColorScheme},resolveTheme:e=>{let t={...e,typography:jc(e.palette,e.typography)};return t.unstable_sx=function(e){return go({sx:e,theme:this})},t}});const wl=xl;function Tl({theme:e,...t}){let n=x.useMemo(()=>{if(typeof e==`function`)return e;let t=`$$material`in e?e[kn]:e;return`colorSchemes`in t?null:`vars`in t?e:{...e,vars:null}},[e]);return n?(0,L.jsx)(yl,{theme:n,...t}):(0,L.jsx)(wl,{theme:e,...t})}var H=ma;function El(...e){return e.reduce((e,t)=>t==null?e:function(...n){e.apply(this,n),t.apply(this,n)},()=>{})}function Dl(e){return(0,L.jsx)(Oo,{...e,defaultTheme:ml,themeId:kn})}var Ol=Dl;function kl(e){return function(t){return(0,L.jsx)(Ol,{styles:typeof e==`function`?n=>e({theme:n,...t}):e})}}function Al(){return Ao}var jl=Ws;function Ml(e){return Ms(e)}function Nl(e){return Ro(`MuiSvgIcon`,e)}zo(`MuiSvgIcon`,[`root`,`colorPrimary`,`colorSecondary`,`colorAction`,`colorError`,`colorDisabled`,`fontSizeInherit`,`fontSizeSmall`,`fontSizeMedium`,`fontSizeLarge`]);var Pl=e=>{let{color:t,fontSize:n,classes:r}=e;return uc({root:[`root`,t!==`inherit`&&`color${H(t)}`,`fontSize${H(n)}`]},Nl,r)},Fl=V(`svg`,{name:`MuiSvgIcon`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.color!==`inherit`&&t[`color${H(n.color)}`],t[`fontSize${H(n.fontSize)}`]]}})(jl(({theme:e})=>({userSelect:`none`,width:`1em`,height:`1em`,display:`inline-block`,flexShrink:0,transition:e.transitions?.create?.(`fill`,{duration:(e.vars??e).transitions?.duration?.shorter}),variants:[{props:e=>!e.hasSvgAsChild,style:{fill:`currentColor`}},{props:{fontSize:`inherit`},style:{fontSize:`inherit`}},{props:{fontSize:`small`},style:{fontSize:e.typography?.pxToRem?.(20)||`1.25rem`}},{props:{fontSize:`medium`},style:{fontSize:e.typography?.pxToRem?.(24)||`1.5rem`}},{props:{fontSize:`large`},style:{fontSize:e.typography?.pxToRem?.(35)||`2.1875rem`}},...Object.entries((e.vars??e).palette).filter(([,e])=>e&&e.main).map(([t])=>({props:{color:t},style:{color:(e.vars??e).palette?.[t]?.main}})),{props:{color:`action`},style:{color:(e.vars??e).palette?.action?.active}},{props:{color:`disabled`},style:{color:(e.vars??e).palette?.action?.disabled}},{props:{color:`inherit`},style:{color:void 0}}]}))),Il=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiSvgIcon`}),{children:r,className:i,color:a=`inherit`,component:o=`svg`,fontSize:s=`medium`,htmlColor:c,inheritViewBox:l=!1,titleAccess:u,viewBox:d=`0 0 24 24`,...f}=n,p=x.isValidElement(r)&&r.type===`svg`,m={...n,color:a,component:o,fontSize:s,instanceFontSize:e.fontSize,inheritViewBox:l,viewBox:d,hasSvgAsChild:p},h={};return l||(h.viewBox=d),(0,L.jsxs)(Fl,{as:o,className:z(Pl(m).root,i),focusable:`false`,color:c,"aria-hidden":u?void 0:!0,role:u?`img`:void 0,ref:t,...h,...f,...p&&r.props,ownerState:m,children:[p?r.props.children:r,u?(0,L.jsx)(`title`,{children:u}):null]})});Il.muiName=`SvgIcon`;var Ll=Il;function Rl(e,t){function n(t,n){return(0,L.jsx)(Ll,{"data-testid":void 0,ref:n,...t,children:e})}return n.muiName=Ll.muiName,x.memo(x.forwardRef(n))}function zl(e,t=166){let n;function r(...r){clearTimeout(n),n=setTimeout(()=>{e.apply(this,r)},t)}return r.clear=()=>{clearTimeout(n)},r}var Bl=dc;function Vl(e){return e&&e.ownerDocument||document}function Hl(e){return Vl(e).defaultView||window}function Ul(e,t){typeof e==`function`?e(t):e&&(e.current=t)}var Wl=$o,Gl=Ls;function Kl(e){let{controlled:t,default:n,name:r,state:i=`value`}=e,{current:a}=x.useRef(t!==void 0),[o,s]=x.useState(n);return[a?t:o,x.useCallback(e=>{a||s(e)},[])]}var ql=Kl;function Jl(e){let t=x.useRef(e);return $o(()=>{t.current=e}),x.useRef((...e)=>(0,t.current)(...e)).current}var Yl=Jl,Xl=Yl;function Zl(...e){let t=x.useRef(void 0),n=x.useCallback(t=>{let n=e.map(e=>{if(e==null)return null;if(typeof e==`function`){let n=e,r=n(t);return typeof r==`function`?r:()=>{n(null)}}return e.current=t,()=>{e.current=null}});return()=>{n.forEach(e=>e?.())}},e);return x.useMemo(()=>e.every(e=>e==null)?null:e=>{t.current&&=(t.current(),void 0),e!=null&&(t.current=n(e))},e)}var Ql=Zl;function $l(e,t){if(e==null)return{};var n={};for(var r in e)if({}.hasOwnProperty.call(e,r)){if(t.indexOf(r)!==-1)continue;n[r]=e[r]}return n}function eu(e,t){return eu=Object.setPrototypeOf?Object.setPrototypeOf.bind():function(e,t){return e.__proto__=t,e},eu(e,t)}function tu(e,t){e.prototype=Object.create(t.prototype),e.prototype.constructor=e,eu(e,t)}var nu={disabled:!1},ru=x.createContext(null),iu=function(e){return e.scrollTop},au=c(m()),ou=`unmounted`,su=`exited`,cu=`entering`,lu=`entered`,uu=`exiting`,du=function(e){tu(t,e);function t(t,n){var r=e.call(this,t,n)||this,i=n,a=i&&!i.isMounting?t.enter:t.appear,o;return r.appearStatus=null,t.in?a?(o=su,r.appearStatus=cu):o=lu:o=t.unmountOnExit||t.mountOnEnter?ou:su,r.state={status:o},r.nextCallback=null,r}t.getDerivedStateFromProps=function(e,t){return e.in&&t.status===`unmounted`?{status:su}:null};var n=t.prototype;return n.componentDidMount=function(){this.updateStatus(!0,this.appearStatus)},n.componentDidUpdate=function(e){var t=null;if(e!==this.props){var n=this.state.status;this.props.in?n!==`entering`&&n!==`entered`&&(t=cu):(n===`entering`||n===`entered`)&&(t=uu)}this.updateStatus(!1,t)},n.componentWillUnmount=function(){this.cancelNextCallback()},n.getTimeouts=function(){var e=this.props.timeout,t=n=r=e,n,r;return e!=null&&typeof e!=`number`&&(t=e.exit,n=e.enter,r=e.appear===void 0?n:e.appear),{exit:t,enter:n,appear:r}},n.updateStatus=function(e,t){if(e===void 0&&(e=!1),t!==null)if(this.cancelNextCallback(),t===`entering`){if(this.props.unmountOnExit||this.props.mountOnEnter){var n=this.props.nodeRef?this.props.nodeRef.current:au.default.findDOMNode(this);n&&iu(n)}this.performEnter(e)}else this.performExit();else this.props.unmountOnExit&&this.state.status===`exited`&&this.setState({status:ou})},n.performEnter=function(e){var t=this,n=this.props.enter,r=this.context?this.context.isMounting:e,i=this.props.nodeRef?[r]:[au.default.findDOMNode(this),r],a=i[0],o=i[1],s=this.getTimeouts(),c=r?s.appear:s.enter;if(!e&&!n||nu.disabled){this.safeSetState({status:lu},function(){t.props.onEntered(a)});return}this.props.onEnter(a,o),this.safeSetState({status:cu},function(){t.props.onEntering(a,o),t.onTransitionEnd(c,function(){t.safeSetState({status:lu},function(){t.props.onEntered(a,o)})})})},n.performExit=function(){var e=this,t=this.props.exit,n=this.getTimeouts(),r=this.props.nodeRef?void 0:au.default.findDOMNode(this);if(!t||nu.disabled){this.safeSetState({status:su},function(){e.props.onExited(r)});return}this.props.onExit(r),this.safeSetState({status:uu},function(){e.props.onExiting(r),e.onTransitionEnd(n.exit,function(){e.safeSetState({status:su},function(){e.props.onExited(r)})})})},n.cancelNextCallback=function(){this.nextCallback!==null&&(this.nextCallback.cancel(),this.nextCallback=null)},n.safeSetState=function(e,t){t=this.setNextCallback(t),this.setState(e,t)},n.setNextCallback=function(e){var t=this,n=!0;return this.nextCallback=function(r){n&&(n=!1,t.nextCallback=null,e(r))},this.nextCallback.cancel=function(){n=!1},this.nextCallback},n.onTransitionEnd=function(e,t){this.setNextCallback(t);var n=this.props.nodeRef?this.props.nodeRef.current:au.default.findDOMNode(this),r=e==null&&!this.props.addEndListener;if(!n||r){setTimeout(this.nextCallback,0);return}if(this.props.addEndListener){var i=this.props.nodeRef?[this.nextCallback]:[n,this.nextCallback],a=i[0],o=i[1];this.props.addEndListener(a,o)}e!=null&&setTimeout(this.nextCallback,e)},n.render=function(){var e=this.state.status;if(e===`unmounted`)return null;var t=this.props,n=t.children;t.in,t.mountOnEnter,t.unmountOnExit,t.appear,t.enter,t.exit,t.timeout,t.addEndListener,t.onEnter,t.onEntering,t.onEntered,t.onExit,t.onExiting,t.onExited,t.nodeRef;var r=$l(t,[`children`,`in`,`mountOnEnter`,`unmountOnExit`,`appear`,`enter`,`exit`,`timeout`,`addEndListener`,`onEnter`,`onEntering`,`onEntered`,`onExit`,`onExiting`,`onExited`,`nodeRef`]);return x.createElement(ru.Provider,{value:null},typeof n==`function`?n(e,r):x.cloneElement(x.Children.only(n),r))},t}(x.Component);du.contextType=ru,du.propTypes={};function fu(){}du.defaultProps={in:!1,mountOnEnter:!1,unmountOnExit:!1,appear:!1,enter:!0,exit:!0,onEnter:fu,onEntering:fu,onEntered:fu,onExit:fu,onExiting:fu,onExited:fu},du.UNMOUNTED=ou,du.EXITED=su,du.ENTERING=cu,du.ENTERED=lu,du.EXITING=uu;var pu=du;function mu(e){if(e===void 0)throw ReferenceError(`this hasn't been initialised - super() hasn't been called`);return e}function hu(e,t){var n=function(e){return t&&(0,x.isValidElement)(e)?t(e):e},r=Object.create(null);return e&&x.Children.map(e,function(e){return e}).forEach(function(e){r[e.key]=n(e)}),r}function gu(e,t){e||={},t||={};function n(n){return n in t?t[n]:e[n]}var r=Object.create(null),i=[];for(var a in e)a in t?i.length&&(r[a]=i,i=[]):i.push(a);var o,s={};for(var c in t){if(r[c])for(o=0;o<r[c].length;o++){var l=r[c][o];s[r[c][o]]=n(l)}s[c]=n(c)}for(o=0;o<i.length;o++)s[i[o]]=n(i[o]);return s}function _u(e,t,n){return n[t]==null?e.props[t]:n[t]}function vu(e,t){return hu(e.children,function(n){return(0,x.cloneElement)(n,{onExited:t.bind(null,n),in:!0,appear:_u(n,`appear`,e),enter:_u(n,`enter`,e),exit:_u(n,`exit`,e)})})}function yu(e,t,n){var r=hu(e.children),i=gu(t,r);return Object.keys(i).forEach(function(a){var o=i[a];if((0,x.isValidElement)(o)){var s=a in t,c=a in r,l=t[a],u=(0,x.isValidElement)(l)&&!l.props.in;c&&(!s||u)?i[a]=(0,x.cloneElement)(o,{onExited:n.bind(null,o),in:!0,exit:_u(o,`exit`,e),enter:_u(o,`enter`,e)}):!c&&s&&!u?i[a]=(0,x.cloneElement)(o,{in:!1}):c&&s&&(0,x.isValidElement)(l)&&(i[a]=(0,x.cloneElement)(o,{onExited:n.bind(null,o),in:l.props.in,exit:_u(o,`exit`,e),enter:_u(o,`enter`,e)}))}}),i}var bu=Object.values||function(e){return Object.keys(e).map(function(t){return e[t]})},xu={component:`div`,childFactory:function(e){return e}},Su=function(e){tu(t,e);function t(t,n){var r=e.call(this,t,n)||this;return r.state={contextValue:{isMounting:!0},handleExited:r.handleExited.bind(mu(r)),firstRender:!0},r}var n=t.prototype;return n.componentDidMount=function(){this.mounted=!0,this.setState({contextValue:{isMounting:!1}})},n.componentWillUnmount=function(){this.mounted=!1},t.getDerivedStateFromProps=function(e,t){var n=t.children,r=t.handleExited;return{children:t.firstRender?vu(e,r):yu(e,n,r),firstRender:!1}},n.handleExited=function(e,t){var n=hu(this.props.children);e.key in n||(e.props.onExited&&e.props.onExited(t),this.mounted&&this.setState(function(t){var n=An({},t.children);return delete n[e.key],{children:n}}))},n.render=function(){var e=this.props,t=e.component,n=e.childFactory,r=$l(e,[`component`,`childFactory`]),i=this.state.contextValue,a=bu(this.state.children).map(n);return delete r.appear,delete r.enter,delete r.exit,t===null?x.createElement(ru.Provider,{value:i},a):x.createElement(ru.Provider,{value:i},x.createElement(t,r,a))},t}(x.Component);Su.propTypes={},Su.defaultProps=xu;var Cu=Su,wu={};function Tu(e,t){let n=x.useRef(wu);return n.current===wu&&(n.current=e(t)),n}var Eu=[];function Du(e){x.useEffect(e,Eu)}var Ou=class e{static create(){return new e}currentId=null;start(e,t){this.clear(),this.currentId=setTimeout(()=>{this.currentId=null,t()},e)}clear=()=>{this.currentId!==null&&(clearTimeout(this.currentId),this.currentId=null)};disposeEffect=()=>this.clear};function ku(){let e=Tu(Ou.create).current;return Du(e.disposeEffect),e}const Au=e=>e.scrollTop;function ju(e,t){let{timeout:n,easing:r,style:i={}}=e;return{duration:i.transitionDuration??(typeof n==`number`?n:n[t.mode]||0),easing:i.transitionTimingFunction??(typeof r==`object`?r[t.mode]:r),delay:i.transitionDelay}}function Mu(e){return typeof e==`string`}var Nu=Mu;function Pu(e,t,n){return e===void 0||Nu(e)?t:{...t,ownerState:{...t.ownerState,...n}}}var Fu=Pu;function Iu(e,t,n){return typeof e==`function`?e(t,n):e}var Lu=Iu;function Ru(e,t=[]){if(e===void 0)return{};let n={};return Object.keys(e).filter(n=>n.match(/^on[A-Z]/)&&typeof e[n]==`function`&&!t.includes(n)).forEach(t=>{n[t]=e[t]}),n}var zu=Ru;function Bu(e){if(e===void 0)return{};let t={};return Object.keys(e).filter(t=>!(t.match(/^on[A-Z]/)&&typeof e[t]==`function`)).forEach(n=>{t[n]=e[n]}),t}var Vu=Bu;function Hu(e){let{getSlotProps:t,additionalProps:n,externalSlotProps:r,externalForwardedProps:i,className:a}=e;if(!t){let e=z(n?.className,a,i?.className,r?.className),t={...n?.style,...i?.style,...r?.style},o={...n,...i,...r};return e.length>0&&(o.className=e),Object.keys(t).length>0&&(o.style=t),{props:o,internalRef:void 0}}let o=zu({...i,...r}),s=Vu(r),c=Vu(i),l=t(o),u=z(l?.className,n?.className,a,i?.className,r?.className),d={...l?.style,...n?.style,...i?.style,...r?.style},f={...l,...n,...c,...s};return u.length>0&&(f.className=u),Object.keys(d).length>0&&(f.style=d),{props:f,internalRef:l.ref}}var Uu=Hu;function Wu(e,t){let{className:n,elementType:r,ownerState:i,externalForwardedProps:a,internalForwardedProps:o,shouldForwardComponentProp:s=!1,...c}=t,{component:l,slots:u={[e]:void 0},slotProps:d={[e]:void 0},...f}=a,p=u[e]||r,m=Lu(d[e],i),{props:{component:h,...g},internalRef:_}=Uu({className:n,...c,externalForwardedProps:e===`root`?f:void 0,externalSlotProps:m}),v=Zl(_,m?.ref,t.ref),y=e===`root`?h||l:h;return[p,Fu(p,{...e===`root`&&!l&&!u[e]&&o,...e!==`root`&&!u[e]&&o,...g,...y&&!s&&{as:y},...y&&s&&{component:y},ref:v},i)]}function Gu(e){return Ro(`MuiCollapse`,e)}zo(`MuiCollapse`,[`root`,`horizontal`,`vertical`,`entered`,`hidden`,`wrapper`,`wrapperInner`]);var Ku=e=>{let{orientation:t,classes:n}=e;return uc({root:[`root`,`${t}`],entered:[`entered`],hidden:[`hidden`],wrapper:[`wrapper`,`${t}`],wrapperInner:[`wrapperInner`,`${t}`]},Gu,n)},qu=V(`div`,{name:`MuiCollapse`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.orientation],n.state===`entered`&&t.entered,n.state===`exited`&&!n.in&&n.collapsedSize===`0px`&&t.hidden]}})(jl(({theme:e})=>({height:0,overflow:`hidden`,transition:e.transitions.create(`height`),variants:[{props:{orientation:`horizontal`},style:{height:`auto`,width:0,transition:e.transitions.create(`width`)}},{props:{state:`entered`},style:{height:`auto`,overflow:`visible`}},{props:{state:`entered`,orientation:`horizontal`},style:{width:`auto`}},{props:({ownerState:e})=>e.state===`exited`&&!e.in&&e.collapsedSize===`0px`,style:{visibility:`hidden`}}]}))),Ju=V(`div`,{name:`MuiCollapse`,slot:`Wrapper`})({display:`flex`,width:`100%`,variants:[{props:{orientation:`horizontal`},style:{width:`auto`,height:`100%`}}]}),Yu=V(`div`,{name:`MuiCollapse`,slot:`WrapperInner`})({width:`100%`,variants:[{props:{orientation:`horizontal`},style:{width:`auto`,height:`100%`}}]}),Xu=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiCollapse`}),{addEndListener:r,children:i,className:a,collapsedSize:o=`0px`,component:s,easing:c,in:l,onEnter:u,onEntered:d,onEntering:f,onExit:p,onExited:m,onExiting:h,orientation:g=`vertical`,slots:_={},slotProps:v={},style:y,timeout:b=Rc.standard,TransitionComponent:S=pu,...C}=n,w={...n,orientation:g,collapsedSize:o},T=Ku(w),E=hl(),D=ku(),O=x.useRef(null),k=x.useRef(),ee=typeof o==`number`?`${o}px`:o,A=g===`horizontal`,j=A?`width`:`height`,M=x.useRef(null),te=Ql(t,M),N=e=>t=>{if(e){let n=M.current;t===void 0?e(n):e(n,t)}},P=()=>O.current?O.current[A?`clientWidth`:`clientHeight`]:0,F=N((e,t)=>{O.current&&A&&(O.current.style.position=`absolute`),e.style[j]=ee,u&&u(e,t)}),ne=N((e,t)=>{let n=P();O.current&&A&&(O.current.style.position=``);let{duration:r,easing:i}=ju({style:y,timeout:b,easing:c},{mode:`enter`});if(b===`auto`){let t=E.transitions.getAutoHeightDuration(n);e.style.transitionDuration=`${t}ms`,k.current=t}else e.style.transitionDuration=typeof r==`string`?r:`${r}ms`;e.style[j]=`${n}px`,e.style.transitionTimingFunction=i,f&&f(e,t)}),re=N((e,t)=>{e.style[j]=`auto`,d&&d(e,t)}),ie=N(e=>{e.style[j]=`${P()}px`,p&&p(e)}),ae=N(m),oe=N(e=>{let t=P(),{duration:n,easing:r}=ju({style:y,timeout:b,easing:c},{mode:`exit`});if(b===`auto`){let n=E.transitions.getAutoHeightDuration(t);e.style.transitionDuration=`${n}ms`,k.current=n}else e.style.transitionDuration=typeof n==`string`?n:`${n}ms`;e.style[j]=ee,e.style.transitionTimingFunction=r,h&&h(e)}),I=e=>{b===`auto`&&D.start(k.current||0,e),r&&r(M.current,e)},se={slots:_,slotProps:v,component:s},[ce,le]=Wu(`root`,{ref:te,className:z(T.root,a),elementType:qu,externalForwardedProps:se,ownerState:w,additionalProps:{style:{[A?`minWidth`:`minHeight`]:ee,...y}}}),[ue,de]=Wu(`wrapper`,{ref:O,className:T.wrapper,elementType:Ju,externalForwardedProps:se,ownerState:w}),[fe,pe]=Wu(`wrapperInner`,{className:T.wrapperInner,elementType:Yu,externalForwardedProps:se,ownerState:w});return(0,L.jsx)(S,{in:l,onEnter:F,onEntered:re,onEntering:ne,onExit:ie,onExited:ae,onExiting:oe,addEndListener:I,nodeRef:M,timeout:b===`auto`?null:b,...C,children:(e,{ownerState:t,...n})=>{let r={...w,state:e};return(0,L.jsx)(ce,{...le,className:z(le.className,{entered:T.entered,exited:!l&&ee===`0px`&&T.hidden}[e]),ownerState:r,...n,children:(0,L.jsx)(ue,{...de,ownerState:r,children:(0,L.jsx)(fe,{...pe,ownerState:r,children:i})})})}})});Xu&&(Xu.muiSupportAuto=!0);var Zu=Xu;function Qu(e){return Ro(`MuiPaper`,e)}zo(`MuiPaper`,`root.rounded.outlined.elevation.elevation0.elevation1.elevation2.elevation3.elevation4.elevation5.elevation6.elevation7.elevation8.elevation9.elevation10.elevation11.elevation12.elevation13.elevation14.elevation15.elevation16.elevation17.elevation18.elevation19.elevation20.elevation21.elevation22.elevation23.elevation24`.split(`.`));var $u=e=>{let{square:t,elevation:n,variant:r,classes:i}=e;return uc({root:[`root`,r,!t&&`rounded`,r===`elevation`&&`elevation${n}`]},Qu,i)},ed=V(`div`,{name:`MuiPaper`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],!n.square&&t.rounded,n.variant===`elevation`&&t[`elevation${n.elevation}`]]}})(jl(({theme:e})=>({backgroundColor:(e.vars||e).palette.background.paper,color:(e.vars||e).palette.text.primary,transition:e.transitions.create(`box-shadow`),variants:[{props:({ownerState:e})=>!e.square,style:{borderRadius:e.shape.borderRadius}},{props:{variant:`outlined`},style:{border:`1px solid ${(e.vars||e).palette.divider}`}},{props:{variant:`elevation`},style:{boxShadow:`var(--Paper-shadow)`,backgroundImage:`var(--Paper-overlay)`}}]}))),td=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiPaper`}),r=hl(),{className:i,component:a=`div`,elevation:o=1,square:s=!1,variant:c=`elevation`,...l}=n,u={...n,component:a,elevation:o,square:s,variant:c};return(0,L.jsx)(ed,{as:a,ownerState:u,className:z($u(u).root,i),ref:t,...l,style:{...c===`elevation`&&{"--Paper-shadow":(r.vars||r).shadows[o],...r.vars&&{"--Paper-overlay":r.vars.overlays?.[o]},...!r.vars&&r.palette.mode===`dark`&&{"--Paper-overlay":`linear-gradient(${ds(`#fff`,Xc(o))}, ${ds(`#fff`,Xc(o))})`}},...l.style}})});function nd(e){try{return e.matches(`:focus-visible`)}catch{}return!1}var rd=class e{static create(){return new e}static use(){let t=Tu(e.create).current,[n,r]=x.useState(!1);return t.shouldMount=n,t.setShouldMount=r,x.useEffect(t.mountEffect,[n]),t}constructor(){this.ref={current:null},this.mounted=null,this.didMount=!1,this.shouldMount=!1,this.setShouldMount=null}mount(){return this.mounted||(this.mounted=ad(),this.shouldMount=!0,this.setShouldMount(this.shouldMount)),this.mounted}mountEffect=()=>{this.shouldMount&&!this.didMount&&this.ref.current!==null&&(this.didMount=!0,this.mounted.resolve())};start(...e){this.mount().then(()=>this.ref.current?.start(...e))}stop(...e){this.mount().then(()=>this.ref.current?.stop(...e))}pulsate(...e){this.mount().then(()=>this.ref.current?.pulsate(...e))}};function id(){return rd.use()}function ad(){let e,t,n=new Promise((n,r)=>{e=n,t=r});return n.resolve=e,n.reject=t,n}function od(e){let{className:t,classes:n,pulsate:r=!1,rippleX:i,rippleY:a,rippleSize:o,in:s,onExited:c,timeout:l}=e,[u,d]=x.useState(!1),f=z(t,n.ripple,n.rippleVisible,r&&n.ripplePulsate),p={width:o,height:o,top:-(o/2)+a,left:-(o/2)+i},m=z(n.child,u&&n.childLeaving,r&&n.childPulsate);return!s&&!u&&d(!0),x.useEffect(()=>{if(!s&&c!=null){let e=setTimeout(c,l);return()=>{clearTimeout(e)}}},[c,s,l]),(0,L.jsx)(`span`,{className:f,style:p,children:(0,L.jsx)(`span`,{className:m})})}var sd=od,cd=zo(`MuiTouchRipple`,[`root`,`ripple`,`rippleVisible`,`ripplePulsate`,`child`,`childLeaving`,`childPulsate`]),ld=550,ud=ki`
  0% {
    transform: scale(0);
    opacity: 0.1;
  }

  100% {
    transform: scale(1);
    opacity: 0.3;
  }
`,dd=ki`
  0% {
    opacity: 1;
  }

  100% {
    opacity: 0;
  }
`,fd=ki`
  0% {
    transform: scale(1);
  }

  50% {
    transform: scale(0.92);
  }

  100% {
    transform: scale(1);
  }
`;const pd=V(`span`,{name:`MuiTouchRipple`,slot:`Root`})({overflow:`hidden`,pointerEvents:`none`,position:`absolute`,zIndex:0,top:0,right:0,bottom:0,left:0,borderRadius:`inherit`}),md=V(sd,{name:`MuiTouchRipple`,slot:`Ripple`})`
  opacity: 0;
  position: absolute;

  &.${cd.rippleVisible} {
    opacity: 0.3;
    transform: scale(1);
    animation-name: ${ud};
    animation-duration: ${ld}ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
  }

  &.${cd.ripplePulsate} {
    animation-duration: ${({theme:e})=>e.transitions.duration.shorter}ms;
  }

  & .${cd.child} {
    opacity: 1;
    display: block;
    width: 100%;
    height: 100%;
    border-radius: 50%;
    background-color: currentColor;
  }

  & .${cd.childLeaving} {
    opacity: 0;
    animation-name: ${dd};
    animation-duration: ${ld}ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
  }

  & .${cd.childPulsate} {
    position: absolute;
    /* @noflip */
    left: 0px;
    top: 0;
    animation-name: ${fd};
    animation-duration: 2500ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
    animation-iteration-count: infinite;
    animation-delay: 200ms;
  }
`;var hd=x.forwardRef(function(e,t){let{center:n=!1,classes:r={},className:i,...a}=Ml({props:e,name:`MuiTouchRipple`}),[o,s]=x.useState([]),c=x.useRef(0),l=x.useRef(null);x.useEffect(()=>{l.current&&=(l.current(),null)},[o]);let u=x.useRef(!1),d=ku(),f=x.useRef(null),p=x.useRef(null),m=x.useCallback(e=>{let{pulsate:t,rippleX:n,rippleY:i,rippleSize:a,cb:o}=e;s(e=>[...e,(0,L.jsx)(md,{classes:{ripple:z(r.ripple,cd.ripple),rippleVisible:z(r.rippleVisible,cd.rippleVisible),ripplePulsate:z(r.ripplePulsate,cd.ripplePulsate),child:z(r.child,cd.child),childLeaving:z(r.childLeaving,cd.childLeaving),childPulsate:z(r.childPulsate,cd.childPulsate)},timeout:ld,pulsate:t,rippleX:n,rippleY:i,rippleSize:a},c.current)]),c.current+=1,l.current=o},[r]),h=x.useCallback((e={},t={},r=()=>{})=>{let{pulsate:i=!1,center:a=n||t.pulsate,fakeElement:o=!1}=t;if(e?.type===`mousedown`&&u.current){u.current=!1;return}e?.type===`touchstart`&&(u.current=!0);let s=o?null:p.current,c=s?s.getBoundingClientRect():{width:0,height:0,left:0,top:0},l,h,g;if(a||e===void 0||e.clientX===0&&e.clientY===0||!e.clientX&&!e.touches)l=Math.round(c.width/2),h=Math.round(c.height/2);else{let{clientX:t,clientY:n}=e.touches&&e.touches.length>0?e.touches[0]:e;l=Math.round(t-c.left),h=Math.round(n-c.top)}if(a)g=Math.sqrt((2*c.width**2+c.height**2)/3),g%2==0&&(g+=1);else{let e=Math.max(Math.abs((s?s.clientWidth:0)-l),l)*2+2,t=Math.max(Math.abs((s?s.clientHeight:0)-h),h)*2+2;g=Math.sqrt(e**2+t**2)}e?.touches?f.current===null&&(f.current=()=>{m({pulsate:i,rippleX:l,rippleY:h,rippleSize:g,cb:r})},d.start(80,()=>{f.current&&=(f.current(),null)})):m({pulsate:i,rippleX:l,rippleY:h,rippleSize:g,cb:r})},[n,m,d]),g=x.useCallback(()=>{h({},{pulsate:!0})},[h]),_=x.useCallback((e,t)=>{if(d.clear(),e?.type===`touchend`&&f.current){f.current(),f.current=null,d.start(0,()=>{_(e,t)});return}f.current=null,s(e=>e.length>0?e.slice(1):e),l.current=t},[d]);return x.useImperativeHandle(t,()=>({pulsate:g,start:h,stop:_}),[g,h,_]),(0,L.jsx)(pd,{className:z(cd.root,r.root,i),ref:p,...a,children:(0,L.jsx)(Cu,{component:null,exit:!0,children:o})})});function gd(e){return Ro(`MuiButtonBase`,e)}var _d=zo(`MuiButtonBase`,[`root`,`disabled`,`focusVisible`]),vd=e=>{let{disabled:t,focusVisible:n,focusVisibleClassName:r,classes:i}=e,a=uc({root:[`root`,t&&`disabled`,n&&`focusVisible`]},gd,i);return n&&r&&(a.root+=` ${r}`),a};const yd=V(`button`,{name:`MuiButtonBase`,slot:`Root`})({display:`inline-flex`,alignItems:`center`,justifyContent:`center`,position:`relative`,boxSizing:`border-box`,WebkitTapHighlightColor:`transparent`,backgroundColor:`transparent`,outline:0,border:0,margin:0,borderRadius:0,padding:0,cursor:`pointer`,userSelect:`none`,verticalAlign:`middle`,MozAppearance:`none`,WebkitAppearance:`none`,textDecoration:`none`,color:`inherit`,"&::-moz-focus-inner":{borderStyle:`none`},[`&.${_d.disabled}`]:{pointerEvents:`none`,cursor:`default`},"@media print":{colorAdjust:`exact`}});var bd=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiButtonBase`}),{action:r,centerRipple:i=!1,children:a,className:o,component:s=`button`,disabled:c=!1,disableRipple:l=!1,disableTouchRipple:u=!1,focusRipple:d=!1,focusVisibleClassName:f,LinkComponent:p=`a`,onBlur:m,onClick:h,onContextMenu:g,onDragLeave:_,onFocus:v,onFocusVisible:y,onKeyDown:b,onKeyUp:S,onMouseDown:C,onMouseLeave:w,onMouseUp:T,onTouchEnd:E,onTouchMove:D,onTouchStart:O,tabIndex:k=0,TouchRippleProps:ee,touchRippleRef:A,type:j,...M}=n,te=x.useRef(null),N=id(),P=Ql(N.ref,A),[F,ne]=x.useState(!1);c&&F&&ne(!1),x.useImperativeHandle(r,()=>({focusVisible:()=>{ne(!0),te.current.focus()}}),[]);let re=N.shouldMount&&!l&&!c;x.useEffect(()=>{F&&d&&!l&&N.pulsate()},[l,d,F,N]);let ie=xd(N,`start`,C,u),ae=xd(N,`stop`,g,u),oe=xd(N,`stop`,_,u),I=xd(N,`stop`,T,u),se=xd(N,`stop`,e=>{F&&e.preventDefault(),w&&w(e)},u),ce=xd(N,`start`,O,u),le=xd(N,`stop`,E,u),ue=xd(N,`stop`,D,u),de=xd(N,`stop`,e=>{nd(e.target)||ne(!1),m&&m(e)},!1),fe=Xl(e=>{te.current||=e.currentTarget,nd(e.target)&&(ne(!0),y&&y(e)),v&&v(e)}),pe=()=>{let e=te.current;return s&&s!==`button`&&!(e.tagName===`A`&&e.href)},me=Xl(e=>{d&&!e.repeat&&F&&e.key===` `&&N.stop(e,()=>{N.start(e)}),e.target===e.currentTarget&&pe()&&e.key===` `&&e.preventDefault(),b&&b(e),e.target===e.currentTarget&&pe()&&e.key===`Enter`&&!c&&(e.preventDefault(),h&&h(e))}),he=Xl(e=>{d&&e.key===` `&&F&&!e.defaultPrevented&&N.stop(e,()=>{N.pulsate(e)}),S&&S(e),h&&e.target===e.currentTarget&&pe()&&e.key===` `&&!e.defaultPrevented&&h(e)}),ge=s;ge===`button`&&(M.href||M.to)&&(ge=p);let _e={};ge===`button`?(_e.type=j===void 0?`button`:j,_e.disabled=c):(!M.href&&!M.to&&(_e.role=`button`),c&&(_e[`aria-disabled`]=c));let ve=Ql(t,te),ye={...n,centerRipple:i,component:s,disabled:c,disableRipple:l,disableTouchRipple:u,focusRipple:d,tabIndex:k,focusVisible:F},be=vd(ye);return(0,L.jsxs)(yd,{as:ge,className:z(be.root,o),ownerState:ye,onBlur:de,onClick:h,onContextMenu:ae,onFocus:fe,onKeyDown:me,onKeyUp:he,onMouseDown:ie,onMouseLeave:se,onMouseUp:I,onDragLeave:oe,onTouchEnd:le,onTouchMove:ue,onTouchStart:ce,ref:ve,tabIndex:c?-1:k,type:j,..._e,...M,children:[a,re?(0,L.jsx)(hd,{ref:P,center:i,...ee}):null]})});function xd(e,t,n,r=!1){return Xl(i=>(n&&n(i),r||e[t](i),!0))}var Sd=bd;function Cd(e){return typeof e.main==`string`}function wd(e,t=[]){if(!Cd(e))return!1;for(let n of t)if(!e.hasOwnProperty(n)||typeof e[n]!=`string`)return!1;return!0}function Td(e=[]){return([,t])=>t&&wd(t,e)}function Ed(e){return Ro(`MuiCircularProgress`,e)}zo(`MuiCircularProgress`,[`root`,`determinate`,`indeterminate`,`colorPrimary`,`colorSecondary`,`svg`,`track`,`circle`,`circleDeterminate`,`circleIndeterminate`,`circleDisableShrink`]);var Dd=44,Od=ki`
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
`,kd=ki`
  0% {
    stroke-dasharray: 1px, 200px;
    stroke-dashoffset: 0;
  }

  50% {
    stroke-dasharray: 100px, 200px;
    stroke-dashoffset: -15px;
  }

  100% {
    stroke-dasharray: 1px, 200px;
    stroke-dashoffset: -126px;
  }
`,Ad=typeof Od==`string`?null:Oi`
        animation: ${Od} 1.4s linear infinite;
      `,jd=typeof kd==`string`?null:Oi`
        animation: ${kd} 1.4s ease-in-out infinite;
      `,Md=e=>{let{classes:t,variant:n,color:r,disableShrink:i}=e;return uc({root:[`root`,n,`color${H(r)}`],svg:[`svg`],track:[`track`],circle:[`circle`,`circle${H(n)}`,i&&`circleDisableShrink`]},Ed,t)},Nd=V(`span`,{name:`MuiCircularProgress`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`color${H(n.color)}`]]}})(jl(({theme:e})=>({display:`inline-block`,variants:[{props:{variant:`determinate`},style:{transition:e.transitions.create(`transform`)}},{props:{variant:`indeterminate`},style:Ad||{animation:`${Od} 1.4s linear infinite`}},...Object.entries(e.palette).filter(Td()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}}))]}))),Pd=V(`svg`,{name:`MuiCircularProgress`,slot:`Svg`})({display:`block`}),Fd=V(`circle`,{name:`MuiCircularProgress`,slot:`Circle`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.circle,t[`circle${H(n.variant)}`],n.disableShrink&&t.circleDisableShrink]}})(jl(({theme:e})=>({stroke:`currentColor`,variants:[{props:{variant:`determinate`},style:{transition:e.transitions.create(`stroke-dashoffset`)}},{props:{variant:`indeterminate`},style:{strokeDasharray:`80px, 200px`,strokeDashoffset:0}},{props:({ownerState:e})=>e.variant===`indeterminate`&&!e.disableShrink,style:jd||{animation:`${kd} 1.4s ease-in-out infinite`}}]}))),Id=V(`circle`,{name:`MuiCircularProgress`,slot:`Track`})(jl(({theme:e})=>({stroke:`currentColor`,opacity:(e.vars||e).palette.action.activatedOpacity}))),Ld=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiCircularProgress`}),{className:r,color:i=`primary`,disableShrink:a=!1,enableTrackSlot:o=!1,size:s=40,style:c,thickness:l=3.6,value:u=0,variant:d=`indeterminate`,...f}=n,p={...n,color:i,disableShrink:a,size:s,thickness:l,value:u,variant:d,enableTrackSlot:o},m=Md(p),h={},g={},_={};if(d===`determinate`){let e=2*Math.PI*((Dd-l)/2);h.strokeDasharray=e.toFixed(3),_[`aria-valuenow`]=Math.round(u),h.strokeDashoffset=`${((100-u)/100*e).toFixed(3)}px`,g.transform=`rotate(-90deg)`}return(0,L.jsx)(Nd,{className:z(m.root,r),style:{width:s,height:s,...g,...c},ownerState:p,ref:t,role:`progressbar`,..._,...f,children:(0,L.jsxs)(Pd,{className:m.svg,ownerState:p,viewBox:`${Dd/2} ${Dd/2} ${Dd} ${Dd}`,children:[o?(0,L.jsx)(Id,{className:m.track,ownerState:p,cx:Dd,cy:Dd,r:(Dd-l)/2,fill:`none`,strokeWidth:l,"aria-hidden":`true`}):null,(0,L.jsx)(Fd,{className:m.circle,style:h,ownerState:p,cx:Dd,cy:Dd,r:(Dd-l)/2,fill:`none`,strokeWidth:l})]})})});function Rd(e){return Ro(`MuiIconButton`,e)}var zd=zo(`MuiIconButton`,[`root`,`disabled`,`colorInherit`,`colorPrimary`,`colorSecondary`,`colorError`,`colorInfo`,`colorSuccess`,`colorWarning`,`edgeStart`,`edgeEnd`,`sizeSmall`,`sizeMedium`,`sizeLarge`,`loading`,`loadingIndicator`,`loadingWrapper`]),Bd=e=>{let{classes:t,disabled:n,color:r,edge:i,size:a,loading:o}=e;return uc({root:[`root`,o&&`loading`,n&&`disabled`,r!==`default`&&`color${H(r)}`,i&&`edge${H(i)}`,`size${H(a)}`],loadingIndicator:[`loadingIndicator`],loadingWrapper:[`loadingWrapper`]},Rd,t)},Vd=V(Sd,{name:`MuiIconButton`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.loading&&t.loading,n.color!==`default`&&t[`color${H(n.color)}`],n.edge&&t[`edge${H(n.edge)}`],t[`size${H(n.size)}`]]}})(jl(({theme:e})=>({textAlign:`center`,flex:`0 0 auto`,fontSize:e.typography.pxToRem(24),padding:8,borderRadius:`50%`,color:(e.vars||e).palette.action.active,transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),variants:[{props:e=>!e.disableRipple,style:{"--IconButton-hoverBg":e.alpha((e.vars||e).palette.action.active,(e.vars||e).palette.action.hoverOpacity),"&:hover":{backgroundColor:`var(--IconButton-hoverBg)`,"@media (hover: none)":{backgroundColor:`transparent`}}}},{props:{edge:`start`},style:{marginLeft:-12}},{props:{edge:`start`,size:`small`},style:{marginLeft:-3}},{props:{edge:`end`},style:{marginRight:-12}},{props:{edge:`end`,size:`small`},style:{marginRight:-3}}]})),jl(({theme:e})=>({variants:[{props:{color:`inherit`},style:{color:`inherit`}},...Object.entries(e.palette).filter(Td()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}})),...Object.entries(e.palette).filter(Td()).map(([t])=>({props:{color:t},style:{"--IconButton-hoverBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)}})),{props:{size:`small`},style:{padding:5,fontSize:e.typography.pxToRem(18)}},{props:{size:`large`},style:{padding:12,fontSize:e.typography.pxToRem(28)}}],[`&.${zd.disabled}`]:{backgroundColor:`transparent`,color:(e.vars||e).palette.action.disabled},[`&.${zd.loading}`]:{color:`transparent`}}))),Hd=V(`span`,{name:`MuiIconButton`,slot:`LoadingIndicator`})(({theme:e})=>({display:`none`,position:`absolute`,visibility:`visible`,top:`50%`,left:`50%`,transform:`translate(-50%, -50%)`,color:(e.vars||e).palette.action.disabled,variants:[{props:{loading:!0},style:{display:`flex`}}]})),Ud=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiIconButton`}),{edge:r=!1,children:i,className:a,color:o=`default`,disabled:s=!1,disableFocusRipple:c=!1,size:l=`medium`,id:u,loading:d=null,loadingIndicator:f,...p}=n,m=Gl(u),h=f??(0,L.jsx)(Ld,{"aria-labelledby":m,color:`inherit`,size:16}),g={...n,edge:r,color:o,disabled:s,disableFocusRipple:c,loading:d,loadingIndicator:h,size:l},_=Bd(g);return(0,L.jsxs)(Vd,{id:d?m:u,className:z(_.root,a),centerRipple:!0,focusRipple:!c,disabled:s||d,ref:t,...p,ownerState:g,children:[typeof d==`boolean`&&(0,L.jsx)(`span`,{className:_.loadingWrapper,style:{display:`contents`},children:(0,L.jsx)(Hd,{className:_.loadingIndicator,ownerState:g,children:d&&h})}),i]})});function Wd(e){return Ro(`MuiTypography`,e)}var Gd=zo(`MuiTypography`,[`root`,`h1`,`h2`,`h3`,`h4`,`h5`,`h6`,`subtitle1`,`subtitle2`,`body1`,`body2`,`inherit`,`button`,`caption`,`overline`,`alignLeft`,`alignRight`,`alignCenter`,`alignJustify`,`noWrap`,`gutterBottom`,`paragraph`]),Kd={primary:!0,secondary:!0,error:!0,info:!0,success:!0,warning:!0,textPrimary:!0,textSecondary:!0,textDisabled:!0},qd=Al(),Jd=e=>{let{align:t,gutterBottom:n,noWrap:r,paragraph:i,variant:a,classes:o}=e;return uc({root:[`root`,a,e.align!==`inherit`&&`align${H(t)}`,n&&`gutterBottom`,r&&`noWrap`,i&&`paragraph`]},Wd,o)};const Yd=V(`span`,{name:`MuiTypography`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.variant&&t[n.variant],n.align!==`inherit`&&t[`align${H(n.align)}`],n.noWrap&&t.noWrap,n.gutterBottom&&t.gutterBottom,n.paragraph&&t.paragraph]}})(jl(({theme:e})=>({margin:0,variants:[{props:{variant:`inherit`},style:{font:`inherit`,lineHeight:`inherit`,letterSpacing:`inherit`}},...Object.entries(e.typography).filter(([e,t])=>e!==`inherit`&&t&&typeof t==`object`).map(([e,t])=>({props:{variant:e},style:t})),...Object.entries(e.palette).filter(Td()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}})),...Object.entries(e.palette?.text||{}).filter(([,e])=>typeof e==`string`).map(([t])=>({props:{color:`text${H(t)}`},style:{color:(e.vars||e).palette.text[t]}})),{props:({ownerState:e})=>e.align!==`inherit`,style:{textAlign:`var(--Typography-textAlign)`}},{props:({ownerState:e})=>e.noWrap,style:{overflow:`hidden`,textOverflow:`ellipsis`,whiteSpace:`nowrap`}},{props:({ownerState:e})=>e.gutterBottom,style:{marginBottom:`0.35em`}},{props:({ownerState:e})=>e.paragraph,style:{marginBottom:16}}]})));var Xd={h1:`h1`,h2:`h2`,h3:`h3`,h4:`h4`,h5:`h5`,h6:`h6`,subtitle1:`h6`,subtitle2:`h6`,body1:`p`,body2:`p`,inherit:`p`},U=x.forwardRef(function(e,t){let{color:n,...r}=Ml({props:e,name:`MuiTypography`}),i=!Kd[n],a=qd({...r,...i&&{color:n}}),{align:o=`inherit`,className:s,component:c,gutterBottom:l=!1,noWrap:u=!1,paragraph:d=!1,variant:f=`body1`,variantMapping:p=Xd,...m}=a,h={...a,align:o,color:n,className:s,component:c,gutterBottom:l,noWrap:u,paragraph:d,variant:f,variantMapping:p};return(0,L.jsx)(Yd,{as:c||(d?`p`:p[f]||Xd[f])||`span`,ref:t,className:z(Jd(h).root,s),...m,ownerState:h,style:{...o!==`inherit`&&{"--Typography-textAlign":o},...m.style}})});function Zd(e){return Ro(`MuiAppBar`,e)}zo(`MuiAppBar`,[`root`,`positionFixed`,`positionAbsolute`,`positionSticky`,`positionStatic`,`positionRelative`,`colorDefault`,`colorPrimary`,`colorSecondary`,`colorInherit`,`colorTransparent`,`colorError`,`colorInfo`,`colorSuccess`,`colorWarning`]);var Qd=e=>{let{color:t,position:n,classes:r}=e;return uc({root:[`root`,`color${H(t)}`,`position${H(n)}`]},Zd,r)},$d=(e,t)=>e?`${e?.replace(`)`,``)}, ${t})`:t,ef=V(td,{name:`MuiAppBar`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[`position${H(n.position)}`],t[`color${H(n.color)}`]]}})(jl(({theme:e})=>({display:`flex`,flexDirection:`column`,width:`100%`,boxSizing:`border-box`,flexShrink:0,variants:[{props:{position:`fixed`},style:{position:`fixed`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0,"@media print":{position:`absolute`}}},{props:{position:`absolute`},style:{position:`absolute`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0}},{props:{position:`sticky`},style:{position:`sticky`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0}},{props:{position:`static`},style:{position:`static`}},{props:{position:`relative`},style:{position:`relative`}},{props:{color:`inherit`},style:{"--AppBar-color":`inherit`}},{props:{color:`default`},style:{"--AppBar-background":e.vars?e.vars.palette.AppBar.defaultBg:e.palette.grey[100],"--AppBar-color":e.vars?e.vars.palette.text.primary:e.palette.getContrastText(e.palette.grey[100]),...e.applyStyles(`dark`,{"--AppBar-background":e.vars?e.vars.palette.AppBar.defaultBg:e.palette.grey[900],"--AppBar-color":e.vars?e.vars.palette.text.primary:e.palette.getContrastText(e.palette.grey[900])})}},...Object.entries(e.palette).filter(Td([`contrastText`])).map(([t])=>({props:{color:t},style:{"--AppBar-background":(e.vars??e).palette[t].main,"--AppBar-color":(e.vars??e).palette[t].contrastText}})),{props:e=>e.enableColorOnDark===!0&&![`inherit`,`transparent`].includes(e.color),style:{backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`}},{props:e=>e.enableColorOnDark===!1&&![`inherit`,`transparent`].includes(e.color),style:{backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`,...e.applyStyles(`dark`,{backgroundColor:e.vars?$d(e.vars.palette.AppBar.darkBg,`var(--AppBar-background)`):null,color:e.vars?$d(e.vars.palette.AppBar.darkColor,`var(--AppBar-color)`):null})}},{props:{color:`transparent`},style:{"--AppBar-background":`transparent`,"--AppBar-color":`inherit`,backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`,...e.applyStyles(`dark`,{backgroundImage:`none`})}}]}))),tf=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiAppBar`}),{className:r,color:i=`primary`,enableColorOnDark:a=!1,position:o=`fixed`,...s}=n,c={...n,color:i,position:o,enableColorOnDark:a};return(0,L.jsx)(ef,{square:!0,component:`header`,ownerState:c,elevation:4,className:z(Qd(c).root,r,o===`fixed`&&`mui-fixed`),ref:t,...s})}),nf=`bottom`,rf=`right`,af=`left`,of=`auto`,sf=[`top`,nf,rf,af],cf=`start`,lf=`clippingParents`,uf=`viewport`,df=`popper`,ff=`reference`,pf=sf.reduce(function(e,t){return e.concat([t+`-`+cf,t+`-end`])},[]),mf=[].concat(sf,[of]).reduce(function(e,t){return e.concat([t,t+`-`+cf,t+`-end`])},[]),hf=[`beforeRead`,`read`,`afterRead`,`beforeMain`,`main`,`afterMain`,`beforeWrite`,`write`,`afterWrite`];function gf(e){return e?(e.nodeName||``).toLowerCase():null}function _f(e){if(e==null)return window;if(e.toString()!==`[object Window]`){var t=e.ownerDocument;return t&&t.defaultView||window}return e}function vf(e){return e instanceof _f(e).Element||e instanceof Element}function yf(e){return e instanceof _f(e).HTMLElement||e instanceof HTMLElement}function bf(e){return typeof ShadowRoot>`u`?!1:e instanceof _f(e).ShadowRoot||e instanceof ShadowRoot}function xf(e){var t=e.state;Object.keys(t.elements).forEach(function(e){var n=t.styles[e]||{},r=t.attributes[e]||{},i=t.elements[e];!yf(i)||!gf(i)||(Object.assign(i.style,n),Object.keys(r).forEach(function(e){var t=r[e];t===!1?i.removeAttribute(e):i.setAttribute(e,t===!0?``:t)}))})}function Sf(e){var t=e.state,n={popper:{position:t.options.strategy,left:`0`,top:`0`,margin:`0`},arrow:{position:`absolute`},reference:{}};return Object.assign(t.elements.popper.style,n.popper),t.styles=n,t.elements.arrow&&Object.assign(t.elements.arrow.style,n.arrow),function(){Object.keys(t.elements).forEach(function(e){var r=t.elements[e],i=t.attributes[e]||{},a=Object.keys(t.styles.hasOwnProperty(e)?t.styles[e]:n[e]).reduce(function(e,t){return e[t]=``,e},{});!yf(r)||!gf(r)||(Object.assign(r.style,a),Object.keys(i).forEach(function(e){r.removeAttribute(e)}))})}}var Cf={name:`applyStyles`,enabled:!0,phase:`write`,fn:xf,effect:Sf,requires:[`computeStyles`]};function wf(e){return e.split(`-`)[0]}var Tf=Math.max,Ef=Math.min,Df=Math.round;function Of(){var e=navigator.userAgentData;return e!=null&&e.brands&&Array.isArray(e.brands)?e.brands.map(function(e){return e.brand+`/`+e.version}).join(` `):navigator.userAgent}function kf(){return!/^((?!chrome|android).)*safari/i.test(Of())}function Af(e,t,n){t===void 0&&(t=!1),n===void 0&&(n=!1);var r=e.getBoundingClientRect(),i=1,a=1;t&&yf(e)&&(i=e.offsetWidth>0&&Df(r.width)/e.offsetWidth||1,a=e.offsetHeight>0&&Df(r.height)/e.offsetHeight||1);var o=(vf(e)?_f(e):window).visualViewport,s=!kf()&&n,c=(r.left+(s&&o?o.offsetLeft:0))/i,l=(r.top+(s&&o?o.offsetTop:0))/a,u=r.width/i,d=r.height/a;return{width:u,height:d,top:l,right:c+u,bottom:l+d,left:c,x:c,y:l}}function jf(e){var t=Af(e),n=e.offsetWidth,r=e.offsetHeight;return Math.abs(t.width-n)<=1&&(n=t.width),Math.abs(t.height-r)<=1&&(r=t.height),{x:e.offsetLeft,y:e.offsetTop,width:n,height:r}}function Mf(e,t){var n=t.getRootNode&&t.getRootNode();if(e.contains(t))return!0;if(n&&bf(n)){var r=t;do{if(r&&e.isSameNode(r))return!0;r=r.parentNode||r.host}while(r)}return!1}function Nf(e){return _f(e).getComputedStyle(e)}function Pf(e){return[`table`,`td`,`th`].indexOf(gf(e))>=0}function Ff(e){return((vf(e)?e.ownerDocument:e.document)||window.document).documentElement}function If(e){return gf(e)===`html`?e:e.assignedSlot||e.parentNode||(bf(e)?e.host:null)||Ff(e)}function Lf(e){return!yf(e)||Nf(e).position===`fixed`?null:e.offsetParent}function Rf(e){var t=/firefox/i.test(Of());if(/Trident/i.test(Of())&&yf(e)&&Nf(e).position===`fixed`)return null;var n=If(e);for(bf(n)&&(n=n.host);yf(n)&&[`html`,`body`].indexOf(gf(n))<0;){var r=Nf(n);if(r.transform!==`none`||r.perspective!==`none`||r.contain===`paint`||[`transform`,`perspective`].indexOf(r.willChange)!==-1||t&&r.willChange===`filter`||t&&r.filter&&r.filter!==`none`)return n;n=n.parentNode}return null}function zf(e){for(var t=_f(e),n=Lf(e);n&&Pf(n)&&Nf(n).position===`static`;)n=Lf(n);return n&&(gf(n)===`html`||gf(n)===`body`&&Nf(n).position===`static`)?t:n||Rf(e)||t}function Bf(e){return[`top`,`bottom`].indexOf(e)>=0?`x`:`y`}function Vf(e,t,n){return Tf(e,Ef(t,n))}function Hf(e,t,n){var r=Vf(e,t,n);return r>n?n:r}function Uf(){return{top:0,right:0,bottom:0,left:0}}function Wf(e){return Object.assign({},Uf(),e)}function Gf(e,t){return t.reduce(function(t,n){return t[n]=e,t},{})}var Kf=function(e,t){return e=typeof e==`function`?e(Object.assign({},t.rects,{placement:t.placement})):e,Wf(typeof e==`number`?Gf(e,sf):e)};function qf(e){var t,n=e.state,r=e.name,i=e.options,a=n.elements.arrow,o=n.modifiersData.popperOffsets,s=wf(n.placement),c=Bf(s),l=[`left`,`right`].indexOf(s)>=0?`height`:`width`;if(!(!a||!o)){var u=Kf(i.padding,n),d=jf(a),f=c===`y`?`top`:af,p=c===`y`?nf:rf,m=n.rects.reference[l]+n.rects.reference[c]-o[c]-n.rects.popper[l],h=o[c]-n.rects.reference[c],g=zf(a),_=g?c===`y`?g.clientHeight||0:g.clientWidth||0:0,v=m/2-h/2,y=u[f],b=_-d[l]-u[p],x=_/2-d[l]/2+v,S=Vf(y,x,b),C=c;n.modifiersData[r]=(t={},t[C]=S,t.centerOffset=S-x,t)}}function Jf(e){var t=e.state,n=e.options.element,r=n===void 0?`[data-popper-arrow]`:n;r!=null&&(typeof r==`string`&&(r=t.elements.popper.querySelector(r),!r)||Mf(t.elements.popper,r)&&(t.elements.arrow=r))}var Yf={name:`arrow`,enabled:!0,phase:`main`,fn:qf,effect:Jf,requires:[`popperOffsets`],requiresIfExists:[`preventOverflow`]};function Xf(e){return e.split(`-`)[1]}var Zf={top:`auto`,right:`auto`,bottom:`auto`,left:`auto`};function Qf(e,t){var n=e.x,r=e.y,i=t.devicePixelRatio||1;return{x:Df(n*i)/i||0,y:Df(r*i)/i||0}}function $f(e){var t,n=e.popper,r=e.popperRect,i=e.placement,a=e.variation,o=e.offsets,s=e.position,c=e.gpuAcceleration,l=e.adaptive,u=e.roundOffsets,d=e.isFixed,f=o.x,p=f===void 0?0:f,m=o.y,h=m===void 0?0:m,g=typeof u==`function`?u({x:p,y:h}):{x:p,y:h};p=g.x,h=g.y;var _=o.hasOwnProperty(`x`),v=o.hasOwnProperty(`y`),y=af,b=`top`,x=window;if(l){var S=zf(n),C=`clientHeight`,w=`clientWidth`;if(S===_f(n)&&(S=Ff(n),Nf(S).position!==`static`&&s===`absolute`&&(C=`scrollHeight`,w=`scrollWidth`)),S=S,i===`top`||(i===`left`||i===`right`)&&a===`end`){b=nf;var T=d&&S===x&&x.visualViewport?x.visualViewport.height:S[C];h-=T-r.height,h*=c?1:-1}if(i===`left`||(i===`top`||i===`bottom`)&&a===`end`){y=rf;var E=d&&S===x&&x.visualViewport?x.visualViewport.width:S[w];p-=E-r.width,p*=c?1:-1}}var D=Object.assign({position:s},l&&Zf),O=u===!0?Qf({x:p,y:h},_f(n)):{x:p,y:h};if(p=O.x,h=O.y,c){var k;return Object.assign({},D,(k={},k[b]=v?`0`:``,k[y]=_?`0`:``,k.transform=(x.devicePixelRatio||1)<=1?`translate(`+p+`px, `+h+`px)`:`translate3d(`+p+`px, `+h+`px, 0)`,k))}return Object.assign({},D,(t={},t[b]=v?h+`px`:``,t[y]=_?p+`px`:``,t.transform=``,t))}function ep(e){var t=e.state,n=e.options,r=n.gpuAcceleration,i=r===void 0?!0:r,a=n.adaptive,o=a===void 0?!0:a,s=n.roundOffsets,c=s===void 0?!0:s,l={placement:wf(t.placement),variation:Xf(t.placement),popper:t.elements.popper,popperRect:t.rects.popper,gpuAcceleration:i,isFixed:t.options.strategy===`fixed`};t.modifiersData.popperOffsets!=null&&(t.styles.popper=Object.assign({},t.styles.popper,$f(Object.assign({},l,{offsets:t.modifiersData.popperOffsets,position:t.options.strategy,adaptive:o,roundOffsets:c})))),t.modifiersData.arrow!=null&&(t.styles.arrow=Object.assign({},t.styles.arrow,$f(Object.assign({},l,{offsets:t.modifiersData.arrow,position:`absolute`,adaptive:!1,roundOffsets:c})))),t.attributes.popper=Object.assign({},t.attributes.popper,{"data-popper-placement":t.placement})}var tp={name:`computeStyles`,enabled:!0,phase:`beforeWrite`,fn:ep,data:{}},np={passive:!0};function rp(e){var t=e.state,n=e.instance,r=e.options,i=r.scroll,a=i===void 0?!0:i,o=r.resize,s=o===void 0?!0:o,c=_f(t.elements.popper),l=[].concat(t.scrollParents.reference,t.scrollParents.popper);return a&&l.forEach(function(e){e.addEventListener(`scroll`,n.update,np)}),s&&c.addEventListener(`resize`,n.update,np),function(){a&&l.forEach(function(e){e.removeEventListener(`scroll`,n.update,np)}),s&&c.removeEventListener(`resize`,n.update,np)}}var ip={name:`eventListeners`,enabled:!0,phase:`write`,fn:function(){},effect:rp,data:{}},ap={left:`right`,right:`left`,bottom:`top`,top:`bottom`};function op(e){return e.replace(/left|right|bottom|top/g,function(e){return ap[e]})}var sp={start:`end`,end:`start`};function cp(e){return e.replace(/start|end/g,function(e){return sp[e]})}function lp(e){var t=_f(e);return{scrollLeft:t.pageXOffset,scrollTop:t.pageYOffset}}function up(e){return Af(Ff(e)).left+lp(e).scrollLeft}function dp(e,t){var n=_f(e),r=Ff(e),i=n.visualViewport,a=r.clientWidth,o=r.clientHeight,s=0,c=0;if(i){a=i.width,o=i.height;var l=kf();(l||!l&&t===`fixed`)&&(s=i.offsetLeft,c=i.offsetTop)}return{width:a,height:o,x:s+up(e),y:c}}function fp(e){var t=Ff(e),n=lp(e),r=e.ownerDocument?.body,i=Tf(t.scrollWidth,t.clientWidth,r?r.scrollWidth:0,r?r.clientWidth:0),a=Tf(t.scrollHeight,t.clientHeight,r?r.scrollHeight:0,r?r.clientHeight:0),o=-n.scrollLeft+up(e),s=-n.scrollTop;return Nf(r||t).direction===`rtl`&&(o+=Tf(t.clientWidth,r?r.clientWidth:0)-i),{width:i,height:a,x:o,y:s}}function pp(e){var t=Nf(e),n=t.overflow,r=t.overflowX,i=t.overflowY;return/auto|scroll|overlay|hidden/.test(n+i+r)}function mp(e){return[`html`,`body`,`#document`].indexOf(gf(e))>=0?e.ownerDocument.body:yf(e)&&pp(e)?e:mp(If(e))}function hp(e,t){t===void 0&&(t=[]);var n=mp(e),r=n===e.ownerDocument?.body,i=_f(n),a=r?[i].concat(i.visualViewport||[],pp(n)?n:[]):n,o=t.concat(a);return r?o:o.concat(hp(If(a)))}function gp(e){return Object.assign({},e,{left:e.x,top:e.y,right:e.x+e.width,bottom:e.y+e.height})}function _p(e,t){var n=Af(e,!1,t===`fixed`);return n.top+=e.clientTop,n.left+=e.clientLeft,n.bottom=n.top+e.clientHeight,n.right=n.left+e.clientWidth,n.width=e.clientWidth,n.height=e.clientHeight,n.x=n.left,n.y=n.top,n}function vp(e,t,n){return t===`viewport`?gp(dp(e,n)):vf(t)?_p(t,n):gp(fp(Ff(e)))}function yp(e){var t=hp(If(e)),n=[`absolute`,`fixed`].indexOf(Nf(e).position)>=0&&yf(e)?zf(e):e;return vf(n)?t.filter(function(e){return vf(e)&&Mf(e,n)&&gf(e)!==`body`}):[]}function bp(e,t,n,r){var i=t===`clippingParents`?yp(e):[].concat(t),a=[].concat(i,[n]),o=a[0],s=a.reduce(function(t,n){var i=vp(e,n,r);return t.top=Tf(i.top,t.top),t.right=Ef(i.right,t.right),t.bottom=Ef(i.bottom,t.bottom),t.left=Tf(i.left,t.left),t},vp(e,o,r));return s.width=s.right-s.left,s.height=s.bottom-s.top,s.x=s.left,s.y=s.top,s}function xp(e){var t=e.reference,n=e.element,r=e.placement,i=r?wf(r):null,a=r?Xf(r):null,o=t.x+t.width/2-n.width/2,s=t.y+t.height/2-n.height/2,c;switch(i){case`top`:c={x:o,y:t.y-n.height};break;case nf:c={x:o,y:t.y+t.height};break;case rf:c={x:t.x+t.width,y:s};break;case af:c={x:t.x-n.width,y:s};break;default:c={x:t.x,y:t.y}}var l=i?Bf(i):null;if(l!=null){var u=l===`y`?`height`:`width`;switch(a){case cf:c[l]=c[l]-(t[u]/2-n[u]/2);break;case`end`:c[l]=c[l]+(t[u]/2-n[u]/2);break;default:}}return c}function Sp(e,t){t===void 0&&(t={});var n=t,r=n.placement,i=r===void 0?e.placement:r,a=n.strategy,o=a===void 0?e.strategy:a,s=n.boundary,c=s===void 0?lf:s,l=n.rootBoundary,u=l===void 0?uf:l,d=n.elementContext,f=d===void 0?df:d,p=n.altBoundary,m=p===void 0?!1:p,h=n.padding,g=h===void 0?0:h,_=Wf(typeof g==`number`?Gf(g,sf):g),v=f===`popper`?ff:df,y=e.rects.popper,b=e.elements[m?v:f],x=bp(vf(b)?b:b.contextElement||Ff(e.elements.popper),c,u,o),S=Af(e.elements.reference),C=xp({reference:S,element:y,strategy:`absolute`,placement:i}),w=gp(Object.assign({},y,C)),T=f===`popper`?w:S,E={top:x.top-T.top+_.top,bottom:T.bottom-x.bottom+_.bottom,left:x.left-T.left+_.left,right:T.right-x.right+_.right},D=e.modifiersData.offset;if(f===`popper`&&D){var O=D[i];Object.keys(E).forEach(function(e){var t=[`right`,`bottom`].indexOf(e)>=0?1:-1,n=[`top`,`bottom`].indexOf(e)>=0?`y`:`x`;E[e]+=O[n]*t})}return E}function Cp(e,t){t===void 0&&(t={});var n=t,r=n.placement,i=n.boundary,a=n.rootBoundary,o=n.padding,s=n.flipVariations,c=n.allowedAutoPlacements,l=c===void 0?mf:c,u=Xf(r),d=u?s?pf:pf.filter(function(e){return Xf(e)===u}):sf,f=d.filter(function(e){return l.indexOf(e)>=0});f.length===0&&(f=d);var p=f.reduce(function(t,n){return t[n]=Sp(e,{placement:n,boundary:i,rootBoundary:a,padding:o})[wf(n)],t},{});return Object.keys(p).sort(function(e,t){return p[e]-p[t]})}function wp(e){if(wf(e)===`auto`)return[];var t=op(e);return[cp(e),t,cp(t)]}function Tp(e){var t=e.state,n=e.options,r=e.name;if(!t.modifiersData[r]._skip){for(var i=n.mainAxis,a=i===void 0?!0:i,o=n.altAxis,s=o===void 0?!0:o,c=n.fallbackPlacements,l=n.padding,u=n.boundary,d=n.rootBoundary,f=n.altBoundary,p=n.flipVariations,m=p===void 0?!0:p,h=n.allowedAutoPlacements,g=t.options.placement,_=wf(g)===g,v=c||(_||!m?[op(g)]:wp(g)),y=[g].concat(v).reduce(function(e,n){return e.concat(wf(n)===`auto`?Cp(t,{placement:n,boundary:u,rootBoundary:d,padding:l,flipVariations:m,allowedAutoPlacements:h}):n)},[]),b=t.rects.reference,x=t.rects.popper,S=new Map,C=!0,w=y[0],T=0;T<y.length;T++){var E=y[T],D=wf(E),O=Xf(E)===cf,k=[`top`,nf].indexOf(D)>=0,ee=k?`width`:`height`,A=Sp(t,{placement:E,boundary:u,rootBoundary:d,altBoundary:f,padding:l}),j=k?O?rf:af:O?nf:`top`;b[ee]>x[ee]&&(j=op(j));var M=op(j),te=[];if(a&&te.push(A[D]<=0),s&&te.push(A[j]<=0,A[M]<=0),te.every(function(e){return e})){w=E,C=!1;break}S.set(E,te)}if(C)for(var N=m?3:1,P=function(e){var t=y.find(function(t){var n=S.get(t);if(n)return n.slice(0,e).every(function(e){return e})});if(t)return w=t,`break`},F=N;F>0&&P(F)!==`break`;F--);t.placement!==w&&(t.modifiersData[r]._skip=!0,t.placement=w,t.reset=!0)}}var Ep={name:`flip`,enabled:!0,phase:`main`,fn:Tp,requiresIfExists:[`offset`],data:{_skip:!1}};function Dp(e,t,n){return n===void 0&&(n={x:0,y:0}),{top:e.top-t.height-n.y,right:e.right-t.width+n.x,bottom:e.bottom-t.height+n.y,left:e.left-t.width-n.x}}function Op(e){return[`top`,rf,nf,af].some(function(t){return e[t]>=0})}function kp(e){var t=e.state,n=e.name,r=t.rects.reference,i=t.rects.popper,a=t.modifiersData.preventOverflow,o=Sp(t,{elementContext:`reference`}),s=Sp(t,{altBoundary:!0}),c=Dp(o,r),l=Dp(s,i,a),u=Op(c),d=Op(l);t.modifiersData[n]={referenceClippingOffsets:c,popperEscapeOffsets:l,isReferenceHidden:u,hasPopperEscaped:d},t.attributes.popper=Object.assign({},t.attributes.popper,{"data-popper-reference-hidden":u,"data-popper-escaped":d})}var Ap={name:`hide`,enabled:!0,phase:`main`,requiresIfExists:[`preventOverflow`],fn:kp};function jp(e,t,n){var r=wf(e),i=[`left`,`top`].indexOf(r)>=0?-1:1,a=typeof n==`function`?n(Object.assign({},t,{placement:e})):n,o=a[0],s=a[1];return o||=0,s=(s||0)*i,[`left`,`right`].indexOf(r)>=0?{x:s,y:o}:{x:o,y:s}}function Mp(e){var t=e.state,n=e.options,r=e.name,i=n.offset,a=i===void 0?[0,0]:i,o=mf.reduce(function(e,n){return e[n]=jp(n,t.rects,a),e},{}),s=o[t.placement],c=s.x,l=s.y;t.modifiersData.popperOffsets!=null&&(t.modifiersData.popperOffsets.x+=c,t.modifiersData.popperOffsets.y+=l),t.modifiersData[r]=o}var Np={name:`offset`,enabled:!0,phase:`main`,requires:[`popperOffsets`],fn:Mp};function Pp(e){var t=e.state,n=e.name;t.modifiersData[n]=xp({reference:t.rects.reference,element:t.rects.popper,strategy:`absolute`,placement:t.placement})}var Fp={name:`popperOffsets`,enabled:!0,phase:`read`,fn:Pp,data:{}};function Ip(e){return e===`x`?`y`:`x`}function Lp(e){var t=e.state,n=e.options,r=e.name,i=n.mainAxis,a=i===void 0?!0:i,o=n.altAxis,s=o===void 0?!1:o,c=n.boundary,l=n.rootBoundary,u=n.altBoundary,d=n.padding,f=n.tether,p=f===void 0?!0:f,m=n.tetherOffset,h=m===void 0?0:m,g=Sp(t,{boundary:c,rootBoundary:l,padding:d,altBoundary:u}),_=wf(t.placement),v=Xf(t.placement),y=!v,b=Bf(_),x=Ip(b),S=t.modifiersData.popperOffsets,C=t.rects.reference,w=t.rects.popper,T=typeof h==`function`?h(Object.assign({},t.rects,{placement:t.placement})):h,E=typeof T==`number`?{mainAxis:T,altAxis:T}:Object.assign({mainAxis:0,altAxis:0},T),D=t.modifiersData.offset?t.modifiersData.offset[t.placement]:null,O={x:0,y:0};if(S){if(a){var k=b===`y`?`top`:af,ee=b===`y`?nf:rf,A=b===`y`?`height`:`width`,j=S[b],M=j+g[k],te=j-g[ee],N=p?-w[A]/2:0,P=v===`start`?C[A]:w[A],F=v===`start`?-w[A]:-C[A],ne=t.elements.arrow,re=p&&ne?jf(ne):{width:0,height:0},ie=t.modifiersData[`arrow#persistent`]?t.modifiersData[`arrow#persistent`].padding:Uf(),ae=ie[k],oe=ie[ee],I=Vf(0,C[A],re[A]),se=y?C[A]/2-N-I-ae-E.mainAxis:P-I-ae-E.mainAxis,ce=y?-C[A]/2+N+I+oe+E.mainAxis:F+I+oe+E.mainAxis,le=t.elements.arrow&&zf(t.elements.arrow),ue=le?b===`y`?le.clientTop||0:le.clientLeft||0:0,de=D?.[b]??0,fe=j+se-de-ue,pe=j+ce-de,me=Vf(p?Ef(M,fe):M,j,p?Tf(te,pe):te);S[b]=me,O[b]=me-j}if(s){var he=b===`x`?`top`:af,ge=b===`x`?nf:rf,_e=S[x],ve=x===`y`?`height`:`width`,ye=_e+g[he],be=_e-g[ge],xe=[`top`,af].indexOf(_)!==-1,Se=D?.[x]??0,Ce=xe?ye:_e-C[ve]-w[ve]-Se+E.altAxis,we=xe?_e+C[ve]+w[ve]-Se-E.altAxis:be,Te=p&&xe?Hf(Ce,_e,we):Vf(p?Ce:ye,_e,p?we:be);S[x]=Te,O[x]=Te-_e}t.modifiersData[r]=O}}var Rp={name:`preventOverflow`,enabled:!0,phase:`main`,fn:Lp,requiresIfExists:[`offset`]};function zp(e){return{scrollLeft:e.scrollLeft,scrollTop:e.scrollTop}}function Bp(e){return e===_f(e)||!yf(e)?lp(e):zp(e)}function Vp(e){var t=e.getBoundingClientRect(),n=Df(t.width)/e.offsetWidth||1,r=Df(t.height)/e.offsetHeight||1;return n!==1||r!==1}function Hp(e,t,n){n===void 0&&(n=!1);var r=yf(t),i=yf(t)&&Vp(t),a=Ff(t),o=Af(e,i,n),s={scrollLeft:0,scrollTop:0},c={x:0,y:0};return(r||!r&&!n)&&((gf(t)!==`body`||pp(a))&&(s=Bp(t)),yf(t)?(c=Af(t,!0),c.x+=t.clientLeft,c.y+=t.clientTop):a&&(c.x=up(a))),{x:o.left+s.scrollLeft-c.x,y:o.top+s.scrollTop-c.y,width:o.width,height:o.height}}function Up(e){var t=new Map,n=new Set,r=[];e.forEach(function(e){t.set(e.name,e)});function i(e){n.add(e.name),[].concat(e.requires||[],e.requiresIfExists||[]).forEach(function(e){if(!n.has(e)){var r=t.get(e);r&&i(r)}}),r.push(e)}return e.forEach(function(e){n.has(e.name)||i(e)}),r}function Wp(e){var t=Up(e);return hf.reduce(function(e,n){return e.concat(t.filter(function(e){return e.phase===n}))},[])}function Gp(e){var t;return function(){return t||=new Promise(function(n){Promise.resolve().then(function(){t=void 0,n(e())})}),t}}function Kp(e){var t=e.reduce(function(e,t){var n=e[t.name];return e[t.name]=n?Object.assign({},n,t,{options:Object.assign({},n.options,t.options),data:Object.assign({},n.data,t.data)}):t,e},{});return Object.keys(t).map(function(e){return t[e]})}var qp={placement:`bottom`,modifiers:[],strategy:`absolute`};function Jp(){return![...arguments].some(function(e){return!(e&&typeof e.getBoundingClientRect==`function`)})}function Yp(e){e===void 0&&(e={});var t=e,n=t.defaultModifiers,r=n===void 0?[]:n,i=t.defaultOptions,a=i===void 0?qp:i;return function(e,t,n){n===void 0&&(n=a);var i={placement:`bottom`,orderedModifiers:[],options:Object.assign({},qp,a),modifiersData:{},elements:{reference:e,popper:t},attributes:{},styles:{}},o=[],s=!1,c={state:i,setOptions:function(n){var o=typeof n==`function`?n(i.options):n;return u(),i.options=Object.assign({},a,i.options,o),i.scrollParents={reference:vf(e)?hp(e):e.contextElement?hp(e.contextElement):[],popper:hp(t)},i.orderedModifiers=Wp(Kp([].concat(r,i.options.modifiers))).filter(function(e){return e.enabled}),l(),c.update()},forceUpdate:function(){if(!s){var e=i.elements,t=e.reference,n=e.popper;if(Jp(t,n)){i.rects={reference:Hp(t,zf(n),i.options.strategy===`fixed`),popper:jf(n)},i.reset=!1,i.placement=i.options.placement,i.orderedModifiers.forEach(function(e){return i.modifiersData[e.name]=Object.assign({},e.data)});for(var r=0;r<i.orderedModifiers.length;r++){if(i.reset===!0){i.reset=!1,r=-1;continue}var a=i.orderedModifiers[r],o=a.fn,l=a.options,u=l===void 0?{}:l,d=a.name;typeof o==`function`&&(i=o({state:i,options:u,name:d,instance:c})||i)}}}},update:Gp(function(){return new Promise(function(e){c.forceUpdate(),e(i)})}),destroy:function(){u(),s=!0}};if(!Jp(e,t))return c;c.setOptions(n).then(function(e){!s&&n.onFirstUpdate&&n.onFirstUpdate(e)});function l(){i.orderedModifiers.forEach(function(e){var t=e.name,n=e.options,r=n===void 0?{}:n,a=e.effect;if(typeof a==`function`){var s=a({state:i,name:t,instance:c,options:r});o.push(s||function(){})}})}function u(){o.forEach(function(e){return e()}),o=[]}return c}}var Xp=Yp({defaultModifiers:[ip,Fp,tp,Cf,Np,Ep,Rp,Yf,Ap]});function Zp(e){let{elementType:t,externalSlotProps:n,ownerState:r,skipResolvingSlotProps:i=!1,...a}=e,o=i?{}:Lu(n,r),{props:s,internalRef:c}=Uu({...a,externalSlotProps:o}),l=Zl(c,o?.ref,e.additionalProps?.ref);return Fu(t,{...s,ref:l},r)}var Qp=Zp;function $p(e){return e?.props?.ref||null}var em=c(m());function tm(e){return typeof e==`function`?e():e}var nm=x.forwardRef(function(e,t){let{children:n,container:r,disablePortal:i=!1}=e,[a,o]=x.useState(null),s=Zl(x.isValidElement(n)?$p(n):null,t);if($o(()=>{i||o(tm(r)||document.body)},[r,i]),$o(()=>{if(a&&!i)return Ul(t,a),()=>{Ul(t,null)}},[t,a,i]),i){if(x.isValidElement(n)){let e={ref:s};return x.cloneElement(n,e)}return n}return a&&em.createPortal(n,a)});function rm(e){return Ro(`MuiPopper`,e)}zo(`MuiPopper`,[`root`]);function im(e,t){if(t===`ltr`)return e;switch(e){case`bottom-end`:return`bottom-start`;case`bottom-start`:return`bottom-end`;case`top-end`:return`top-start`;case`top-start`:return`top-end`;default:return e}}function am(e){return typeof e==`function`?e():e}function om(e){return e.nodeType!==void 0}var sm=e=>{let{classes:t}=e;return uc({root:[`root`]},rm,t)},cm={},lm=x.forwardRef(function(e,t){let{anchorEl:n,children:r,direction:i,disablePortal:a,modifiers:o,open:s,placement:c,popperOptions:l,popperRef:u,slotProps:d={},slots:f={},TransitionProps:p,ownerState:m,...h}=e,g=x.useRef(null),_=Zl(g,t),v=x.useRef(null),y=Zl(v,u),b=x.useRef(y);$o(()=>{b.current=y},[y]),x.useImperativeHandle(u,()=>v.current,[]);let S=im(c,i),[C,w]=x.useState(S),[T,E]=x.useState(am(n));x.useEffect(()=>{v.current&&v.current.forceUpdate()}),x.useEffect(()=>{n&&E(am(n))},[n]),$o(()=>{if(!T||!s)return;let e=e=>{w(e.placement)},t=[{name:`preventOverflow`,options:{altBoundary:a}},{name:`flip`,options:{altBoundary:a}},{name:`onUpdate`,enabled:!0,phase:`afterWrite`,fn:({state:t})=>{e(t)}}];o!=null&&(t=t.concat(o)),l&&l.modifiers!=null&&(t=t.concat(l.modifiers));let n=Xp(T,g.current,{placement:S,...l,modifiers:t});return b.current(n),()=>{n.destroy(),b.current(null)}},[T,a,o,s,l,S]);let D={placement:C};p!==null&&(D.TransitionProps=p);let O=sm(e),k=f.root??`div`;return(0,L.jsx)(k,{...Qp({elementType:k,externalSlotProps:d.root,externalForwardedProps:h,additionalProps:{role:`tooltip`,ref:_},ownerState:e,className:O.root}),children:typeof r==`function`?r(D):r})}),um=V(x.forwardRef(function(e,t){let{anchorEl:n,children:r,container:i,direction:a=`ltr`,disablePortal:o=!1,keepMounted:s=!1,modifiers:c,open:l,placement:u=`bottom`,popperOptions:d=cm,popperRef:f,style:p,transition:m=!1,slotProps:h={},slots:g={},..._}=e,[v,y]=x.useState(!0),b=()=>{y(!1)},S=()=>{y(!0)};if(!s&&!l&&(!m||v))return null;let C;if(i)C=i;else if(n){let e=am(n);C=e&&om(e)?Vl(e).body:Vl(null).body}let w=!l&&s&&(!m||v)?`none`:void 0,T=m?{in:l,onEnter:b,onExited:S}:void 0;return(0,L.jsx)(nm,{disablePortal:o,container:C,children:(0,L.jsx)(lm,{anchorEl:n,direction:a,disablePortal:o,modifiers:c,ref:t,open:m?!v:l,placement:u,popperOptions:d,popperRef:f,slotProps:h,slots:g,..._,style:{position:`fixed`,top:0,left:0,display:w,...p},TransitionProps:T,children:r})})}),{name:`MuiPopper`,slot:`Root`})({}),dm=x.forwardRef(function(e,t){let n=Ds(),{anchorEl:r,component:i,components:a,componentsProps:o,container:s,disablePortal:c,keepMounted:l,modifiers:u,open:d,placement:f,popperOptions:p,popperRef:m,transition:h,slots:g,slotProps:_,...v}=Ml({props:e,name:`MuiPopper`}),y=g?.root??a?.Root,b={anchorEl:r,container:s,disablePortal:c,keepMounted:l,modifiers:u,open:d,placement:f,popperOptions:p,popperRef:m,transition:h,...v};return(0,L.jsx)(um,{as:i,direction:n?`rtl`:`ltr`,slots:{root:y},slotProps:_??o,...b,ref:t})}),fm=Rl((0,L.jsx)(`path`,{d:`M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z`}),`Cancel`);function pm(e){return Ro(`MuiChip`,e)}var mm=zo(`MuiChip`,`root.sizeSmall.sizeMedium.colorDefault.colorError.colorInfo.colorPrimary.colorSecondary.colorSuccess.colorWarning.disabled.clickable.clickableColorPrimary.clickableColorSecondary.deletable.deletableColorPrimary.deletableColorSecondary.outlined.filled.outlinedPrimary.outlinedSecondary.filledPrimary.filledSecondary.avatar.avatarSmall.avatarMedium.avatarColorPrimary.avatarColorSecondary.icon.iconSmall.iconMedium.iconColorPrimary.iconColorSecondary.label.labelSmall.labelMedium.deleteIcon.deleteIconSmall.deleteIconMedium.deleteIconColorPrimary.deleteIconColorSecondary.deleteIconOutlinedColorPrimary.deleteIconOutlinedColorSecondary.deleteIconFilledColorPrimary.deleteIconFilledColorSecondary.focusVisible`.split(`.`)),hm=e=>{let{classes:t,disabled:n,size:r,color:i,iconColor:a,onDelete:o,clickable:s,variant:c}=e;return uc({root:[`root`,c,n&&`disabled`,`size${H(r)}`,`color${H(i)}`,s&&`clickable`,s&&`clickableColor${H(i)}`,o&&`deletable`,o&&`deletableColor${H(i)}`,`${c}${H(i)}`],label:[`label`,`label${H(r)}`],avatar:[`avatar`,`avatar${H(r)}`,`avatarColor${H(i)}`],icon:[`icon`,`icon${H(r)}`,`iconColor${H(a)}`],deleteIcon:[`deleteIcon`,`deleteIcon${H(r)}`,`deleteIconColor${H(i)}`,`deleteIcon${H(c)}Color${H(i)}`]},pm,t)},gm=V(`div`,{name:`MuiChip`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e,{color:r,iconColor:i,clickable:a,onDelete:o,size:s,variant:c}=n;return[{[`& .${mm.avatar}`]:t.avatar},{[`& .${mm.avatar}`]:t[`avatar${H(s)}`]},{[`& .${mm.avatar}`]:t[`avatarColor${H(r)}`]},{[`& .${mm.icon}`]:t.icon},{[`& .${mm.icon}`]:t[`icon${H(s)}`]},{[`& .${mm.icon}`]:t[`iconColor${H(i)}`]},{[`& .${mm.deleteIcon}`]:t.deleteIcon},{[`& .${mm.deleteIcon}`]:t[`deleteIcon${H(s)}`]},{[`& .${mm.deleteIcon}`]:t[`deleteIconColor${H(r)}`]},{[`& .${mm.deleteIcon}`]:t[`deleteIcon${H(c)}Color${H(r)}`]},t.root,t[`size${H(s)}`],t[`color${H(r)}`],a&&t.clickable,a&&r!==`default`&&t[`clickableColor${H(r)})`],o&&t.deletable,o&&r!==`default`&&t[`deletableColor${H(r)}`],t[c],t[`${c}${H(r)}`]]}})(jl(({theme:e})=>{let t=e.palette.mode===`light`?e.palette.grey[700]:e.palette.grey[300];return{maxWidth:`100%`,fontFamily:e.typography.fontFamily,fontSize:e.typography.pxToRem(13),display:`inline-flex`,alignItems:`center`,justifyContent:`center`,height:32,lineHeight:1.5,color:(e.vars||e).palette.text.primary,backgroundColor:(e.vars||e).palette.action.selected,borderRadius:32/2,whiteSpace:`nowrap`,transition:e.transitions.create([`background-color`,`box-shadow`]),cursor:`unset`,outline:0,textDecoration:`none`,border:0,padding:0,verticalAlign:`middle`,boxSizing:`border-box`,[`&.${mm.disabled}`]:{opacity:(e.vars||e).palette.action.disabledOpacity,pointerEvents:`none`},[`& .${mm.avatar}`]:{marginLeft:5,marginRight:-6,width:24,height:24,color:e.vars?e.vars.palette.Chip.defaultAvatarColor:t,fontSize:e.typography.pxToRem(12)},[`& .${mm.avatarColorPrimary}`]:{color:(e.vars||e).palette.primary.contrastText,backgroundColor:(e.vars||e).palette.primary.dark},[`& .${mm.avatarColorSecondary}`]:{color:(e.vars||e).palette.secondary.contrastText,backgroundColor:(e.vars||e).palette.secondary.dark},[`& .${mm.avatarSmall}`]:{marginLeft:4,marginRight:-4,width:18,height:18,fontSize:e.typography.pxToRem(10)},[`& .${mm.icon}`]:{marginLeft:5,marginRight:-6},[`& .${mm.deleteIcon}`]:{WebkitTapHighlightColor:`transparent`,color:e.alpha((e.vars||e).palette.text.primary,.26),fontSize:22,cursor:`pointer`,margin:`0 5px 0 -6px`,"&:hover":{color:e.alpha((e.vars||e).palette.text.primary,.4)}},variants:[{props:{size:`small`},style:{height:24,[`& .${mm.icon}`]:{fontSize:18,marginLeft:4,marginRight:-4},[`& .${mm.deleteIcon}`]:{fontSize:16,marginRight:4,marginLeft:-4}}},...Object.entries(e.palette).filter(Td([`contrastText`])).map(([t])=>({props:{color:t},style:{backgroundColor:(e.vars||e).palette[t].main,color:(e.vars||e).palette[t].contrastText,[`& .${mm.deleteIcon}`]:{color:e.alpha((e.vars||e).palette[t].contrastText,.7),"&:hover, &:active":{color:(e.vars||e).palette[t].contrastText}}}})),{props:e=>e.iconColor===e.color,style:{[`& .${mm.icon}`]:{color:e.vars?e.vars.palette.Chip.defaultIconColor:t}}},{props:e=>e.iconColor===e.color&&e.color!==`default`,style:{[`& .${mm.icon}`]:{color:`inherit`}}},{props:{onDelete:!0},style:{[`&.${mm.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)}}},...Object.entries(e.palette).filter(Td([`dark`])).map(([t])=>({props:{color:t,onDelete:!0},style:{[`&.${mm.focusVisible}`]:{background:(e.vars||e).palette[t].dark}}})),{props:{clickable:!0},style:{userSelect:`none`,WebkitTapHighlightColor:`transparent`,cursor:`pointer`,"&:hover":{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.hoverOpacity}`)},[`&.${mm.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)},"&:active":{boxShadow:(e.vars||e).shadows[1]}}},...Object.entries(e.palette).filter(Td([`dark`])).map(([t])=>({props:{color:t,clickable:!0},style:{[`&:hover, &.${mm.focusVisible}`]:{backgroundColor:(e.vars||e).palette[t].dark}}})),{props:{variant:`outlined`},style:{backgroundColor:`transparent`,border:e.vars?`1px solid ${e.vars.palette.Chip.defaultBorder}`:`1px solid ${e.palette.mode===`light`?e.palette.grey[400]:e.palette.grey[700]}`,[`&.${mm.clickable}:hover`]:{backgroundColor:(e.vars||e).palette.action.hover},[`&.${mm.focusVisible}`]:{backgroundColor:(e.vars||e).palette.action.focus},[`& .${mm.avatar}`]:{marginLeft:4},[`& .${mm.avatarSmall}`]:{marginLeft:2},[`& .${mm.icon}`]:{marginLeft:4},[`& .${mm.iconSmall}`]:{marginLeft:2},[`& .${mm.deleteIcon}`]:{marginRight:5},[`& .${mm.deleteIconSmall}`]:{marginRight:3}}},...Object.entries(e.palette).filter(Td()).map(([t])=>({props:{variant:`outlined`,color:t},style:{color:(e.vars||e).palette[t].main,border:`1px solid ${e.alpha((e.vars||e).palette[t].main,.7)}`,[`&.${mm.clickable}:hover`]:{backgroundColor:e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)},[`&.${mm.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.focusOpacity)},[`& .${mm.deleteIcon}`]:{color:e.alpha((e.vars||e).palette[t].main,.7),"&:hover, &:active":{color:(e.vars||e).palette[t].main}}}}))]}})),_m=V(`span`,{name:`MuiChip`,slot:`Label`,overridesResolver:(e,t)=>{let{ownerState:n}=e,{size:r}=n;return[t.label,t[`label${H(r)}`]]}})({overflow:`hidden`,textOverflow:`ellipsis`,paddingLeft:12,paddingRight:12,whiteSpace:`nowrap`,variants:[{props:{variant:`outlined`},style:{paddingLeft:11,paddingRight:11}},{props:{size:`small`},style:{paddingLeft:8,paddingRight:8}},{props:{size:`small`,variant:`outlined`},style:{paddingLeft:7,paddingRight:7}}]});function vm(e){return e.key===`Backspace`||e.key===`Delete`}var ym=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiChip`}),{avatar:r,className:i,clickable:a,color:o=`default`,component:s,deleteIcon:c,disabled:l=!1,icon:u,label:d,onClick:f,onDelete:p,onKeyDown:m,onKeyUp:h,size:g=`medium`,variant:_=`filled`,tabIndex:v,skipFocusWhenDisabled:y=!1,slots:b={},slotProps:S={},...C}=n,w=Ql(x.useRef(null),t),T=e=>{e.stopPropagation(),p&&p(e)},E=e=>{e.currentTarget===e.target&&vm(e)&&e.preventDefault(),m&&m(e)},D=e=>{e.currentTarget===e.target&&p&&vm(e)&&p(e),h&&h(e)},O=a!==!1&&f?!0:a,k=O||p?Sd:s||`div`,ee={...n,component:k,disabled:l,size:g,color:o,iconColor:x.isValidElement(u)&&u.props.color||o,onDelete:!!p,clickable:O,variant:_},A=hm(ee),j=k===Sd?{component:s||`div`,focusVisibleClassName:A.focusVisible,...p&&{disableRipple:!0}}:{},M=null;p&&(M=c&&x.isValidElement(c)?x.cloneElement(c,{className:z(c.props.className,A.deleteIcon),onClick:T}):(0,L.jsx)(fm,{className:A.deleteIcon,onClick:T}));let te=null;r&&x.isValidElement(r)&&(te=x.cloneElement(r,{className:z(A.avatar,r.props.className)}));let N=null;u&&x.isValidElement(u)&&(N=x.cloneElement(u,{className:z(A.icon,u.props.className)}));let P={slots:b,slotProps:S},[F,ne]=Wu(`root`,{elementType:gm,externalForwardedProps:{...P,...C},ownerState:ee,shouldForwardComponentProp:!0,ref:w,className:z(A.root,i),additionalProps:{disabled:O&&l?!0:void 0,tabIndex:y&&l?-1:v,...j},getSlotProps:e=>({...e,onClick:t=>{e.onClick?.(t),f?.(t)},onKeyDown:t=>{e.onKeyDown?.(t),E(t)},onKeyUp:t=>{e.onKeyUp?.(t),D(t)}})}),[re,ie]=Wu(`label`,{elementType:_m,externalForwardedProps:P,ownerState:ee,className:A.label});return(0,L.jsxs)(F,{as:k,...ne,children:[te||N,(0,L.jsx)(re,{...ie,children:d}),M]})});function bm(e){return parseInt(e,10)||0}var xm={shadow:{visibility:`hidden`,position:`absolute`,overflow:`hidden`,height:0,top:0,left:0,transform:`translateZ(0)`}};function Sm(e){for(let t in e)return!1;return!0}function Cm(e){return Sm(e)||e.outerHeightStyle===0&&!e.overflowing}var wm=x.forwardRef(function(e,t){let{onChange:n,maxRows:r,minRows:i=1,style:a,value:o,...s}=e,{current:c}=x.useRef(o!=null),l=x.useRef(null),u=Zl(t,l),d=x.useRef(null),f=x.useRef(null),p=x.useCallback(()=>{let t=l.current,n=f.current;if(!t||!n)return;let a=Hl(t).getComputedStyle(t);if(a.width===`0px`)return{outerHeightStyle:0,overflowing:!1};n.style.width=a.width,n.value=t.value||e.placeholder||`x`,n.value.slice(-1)===`
`&&(n.value+=` `);let o=a.boxSizing,s=bm(a.paddingBottom)+bm(a.paddingTop),c=bm(a.borderBottomWidth)+bm(a.borderTopWidth),u=n.scrollHeight;n.value=`x`;let d=n.scrollHeight,p=u;return i&&(p=Math.max(Number(i)*d,p)),r&&(p=Math.min(Number(r)*d,p)),p=Math.max(p,d),{outerHeightStyle:p+(o===`border-box`?s+c:0),overflowing:Math.abs(p-u)<=1}},[r,i,e.placeholder]),m=Yl(()=>{let e=l.current,t=p();if(!e||!t||Cm(t))return!1;let n=t.outerHeightStyle;return d.current!=null&&d.current!==n}),h=x.useCallback(()=>{let e=l.current,t=p();if(!e||!t||Cm(t))return;let n=t.outerHeightStyle;d.current!==n&&(d.current=n,e.style.height=`${n}px`),e.style.overflow=t.overflowing?`hidden`:``},[p]),g=x.useRef(-1);return $o(()=>{let e=zl(h),t=l?.current;if(!t)return;let n=Hl(t);n.addEventListener(`resize`,e);let r;return typeof ResizeObserver<`u`&&(r=new ResizeObserver(()=>{m()&&(r.unobserve(t),cancelAnimationFrame(g.current),h(),g.current=requestAnimationFrame(()=>{r.observe(t)}))}),r.observe(t)),()=>{e.clear(),cancelAnimationFrame(g.current),n.removeEventListener(`resize`,e),r&&r.disconnect()}},[p,h,m]),$o(()=>{h()}),(0,L.jsxs)(x.Fragment,{children:[(0,L.jsx)(`textarea`,{value:o,onChange:e=>{c||h();let t=e.target,r=t.value.length,i=t.value.endsWith(`
`),a=t.selectionStart===r;i&&a&&t.setSelectionRange(r,r),n&&n(e)},ref:u,rows:i,style:a,...s}),(0,L.jsx)(`textarea`,{"aria-hidden":!0,className:e.className,readOnly:!0,ref:f,tabIndex:-1,style:{...xm.shadow,...a,paddingTop:0,paddingBottom:0}})]})});function Tm({props:e,states:t,muiFormControl:n}){return t.reduce((t,r)=>(t[r]=e[r],n&&e[r]===void 0&&(t[r]=n[r]),t),{})}var Em=x.createContext(void 0);function Dm(){return x.useContext(Em)}function Om(e){return e!=null&&!(Array.isArray(e)&&e.length===0)}function km(e,t=!1){return e&&(Om(e.value)&&e.value!==``||t&&Om(e.defaultValue)&&e.defaultValue!==``)}function Am(e){return Ro(`MuiInputBase`,e)}var jm=zo(`MuiInputBase`,[`root`,`formControl`,`focused`,`disabled`,`adornedStart`,`adornedEnd`,`error`,`sizeSmall`,`multiline`,`colorSecondary`,`fullWidth`,`hiddenLabel`,`readOnly`,`input`,`inputSizeSmall`,`inputMultiline`,`inputTypeSearch`,`inputAdornedStart`,`inputAdornedEnd`,`inputHiddenLabel`]),Mm;const Nm=(e,t)=>{let{ownerState:n}=e;return[t.root,n.formControl&&t.formControl,n.startAdornment&&t.adornedStart,n.endAdornment&&t.adornedEnd,n.error&&t.error,n.size===`small`&&t.sizeSmall,n.multiline&&t.multiline,n.color&&t[`color${H(n.color)}`],n.fullWidth&&t.fullWidth,n.hiddenLabel&&t.hiddenLabel]},Pm=(e,t)=>{let{ownerState:n}=e;return[t.input,n.size===`small`&&t.inputSizeSmall,n.multiline&&t.inputMultiline,n.type===`search`&&t.inputTypeSearch,n.startAdornment&&t.inputAdornedStart,n.endAdornment&&t.inputAdornedEnd,n.hiddenLabel&&t.inputHiddenLabel]};var Fm=e=>{let{classes:t,color:n,disabled:r,error:i,endAdornment:a,focused:o,formControl:s,fullWidth:c,hiddenLabel:l,multiline:u,readOnly:d,size:f,startAdornment:p,type:m}=e;return uc({root:[`root`,`color${H(n)}`,r&&`disabled`,i&&`error`,c&&`fullWidth`,o&&`focused`,s&&`formControl`,f&&f!==`medium`&&`size${H(f)}`,u&&`multiline`,p&&`adornedStart`,a&&`adornedEnd`,l&&`hiddenLabel`,d&&`readOnly`],input:[`input`,r&&`disabled`,m===`search`&&`inputTypeSearch`,u&&`inputMultiline`,f===`small`&&`inputSizeSmall`,l&&`inputHiddenLabel`,p&&`inputAdornedStart`,a&&`inputAdornedEnd`,d&&`readOnly`]},Am,t)};const Im=V(`div`,{name:`MuiInputBase`,slot:`Root`,overridesResolver:Nm})(jl(({theme:e})=>({...e.typography.body1,color:(e.vars||e).palette.text.primary,lineHeight:`1.4375em`,boxSizing:`border-box`,position:`relative`,cursor:`text`,display:`inline-flex`,alignItems:`center`,[`&.${jm.disabled}`]:{color:(e.vars||e).palette.text.disabled,cursor:`default`},variants:[{props:({ownerState:e})=>e.multiline,style:{padding:`4px 0 5px`}},{props:({ownerState:e,size:t})=>e.multiline&&t===`small`,style:{paddingTop:1}},{props:({ownerState:e})=>e.fullWidth,style:{width:`100%`}}]}))),Lm=V(`input`,{name:`MuiInputBase`,slot:`Input`,overridesResolver:Pm})(jl(({theme:e})=>{let t=e.palette.mode===`light`,n={color:`currentColor`,...e.vars?{opacity:e.vars.opacity.inputPlaceholder}:{opacity:t?.42:.5},transition:e.transitions.create(`opacity`,{duration:e.transitions.duration.shorter})},r={opacity:`0 !important`},i=e.vars?{opacity:e.vars.opacity.inputPlaceholder}:{opacity:t?.42:.5};return{font:`inherit`,letterSpacing:`inherit`,color:`currentColor`,padding:`4px 0 5px`,border:0,boxSizing:`content-box`,background:`none`,height:`1.4375em`,margin:0,WebkitTapHighlightColor:`transparent`,display:`block`,minWidth:0,width:`100%`,"&::-webkit-input-placeholder":n,"&::-moz-placeholder":n,"&::-ms-input-placeholder":n,"&:focus":{outline:0},"&:invalid":{boxShadow:`none`},"&::-webkit-search-decoration":{WebkitAppearance:`none`},[`label[data-shrink=false] + .${jm.formControl} &`]:{"&::-webkit-input-placeholder":r,"&::-moz-placeholder":r,"&::-ms-input-placeholder":r,"&:focus::-webkit-input-placeholder":i,"&:focus::-moz-placeholder":i,"&:focus::-ms-input-placeholder":i},[`&.${jm.disabled}`]:{opacity:1,WebkitTextFillColor:(e.vars||e).palette.text.disabled},variants:[{props:({ownerState:e})=>!e.disableInjectingGlobalStyles,style:{animationName:`mui-auto-fill-cancel`,animationDuration:`10ms`,"&:-webkit-autofill":{animationDuration:`5000s`,animationName:`mui-auto-fill`}}},{props:{size:`small`},style:{paddingTop:1}},{props:({ownerState:e})=>e.multiline,style:{height:`auto`,resize:`none`,padding:0,paddingTop:0}},{props:{type:`search`},style:{MozAppearance:`textfield`}}]}}));var Rm=kl({"@keyframes mui-auto-fill":{from:{display:`block`}},"@keyframes mui-auto-fill-cancel":{from:{display:`block`}}}),zm=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiInputBase`}),{"aria-describedby":r,autoComplete:i,autoFocus:a,className:o,color:s,components:c={},componentsProps:l={},defaultValue:u,disabled:d,disableInjectingGlobalStyles:f,endAdornment:p,error:m,fullWidth:h=!1,id:g,inputComponent:_=`input`,inputProps:v={},inputRef:y,margin:b,maxRows:S,minRows:C,multiline:w=!1,name:T,onBlur:E,onChange:D,onClick:O,onFocus:k,onKeyDown:ee,onKeyUp:A,placeholder:j,readOnly:M,renderSuffix:te,rows:N,size:P,slotProps:F={},slots:ne={},startAdornment:re,type:ie=`text`,value:ae,...oe}=n,I=v.value==null?ae:v.value,{current:se}=x.useRef(I!=null),ce=x.useRef(),le=x.useCallback(e=>{},[]),ue=Ql(ce,y,v.ref,le),[de,fe]=x.useState(!1),pe=Dm(),me=Tm({props:n,muiFormControl:pe,states:[`color`,`disabled`,`error`,`hiddenLabel`,`size`,`required`,`filled`]});me.focused=pe?pe.focused:de,x.useEffect(()=>{!pe&&d&&de&&(fe(!1),E&&E())},[pe,d,de,E]);let he=pe&&pe.onFilled,ge=pe&&pe.onEmpty,_e=x.useCallback(e=>{km(e)?he&&he():ge&&ge()},[he,ge]);Wl(()=>{se&&_e({value:I})},[I,_e,se]);let ve=e=>{k&&k(e),v.onFocus&&v.onFocus(e),pe&&pe.onFocus?pe.onFocus(e):fe(!0)},ye=e=>{E&&E(e),v.onBlur&&v.onBlur(e),pe&&pe.onBlur?pe.onBlur(e):fe(!1)},be=(e,...t)=>{if(!se){let t=e.target||ce.current;if(t==null)throw Error(On(1));_e({value:t.value})}v.onChange&&v.onChange(e,...t),D&&D(e,...t)};x.useEffect(()=>{_e(ce.current)},[]);let xe=e=>{ce.current&&e.currentTarget===e.target&&ce.current.focus(),O&&O(e)},Se=_,Ce=v;w&&Se===`input`&&(Ce=N?{type:void 0,minRows:N,maxRows:N,...Ce}:{type:void 0,maxRows:S,minRows:C,...Ce},Se=wm);let we=e=>{_e(e.animationName===`mui-auto-fill-cancel`?ce.current:{value:`x`})};x.useEffect(()=>{pe&&pe.setAdornedStart(!!re)},[pe,re]);let Te={...n,color:me.color||`primary`,disabled:me.disabled,endAdornment:p,error:me.error,focused:me.focused,formControl:pe,fullWidth:h,hiddenLabel:me.hiddenLabel,multiline:w,size:me.size,startAdornment:re,type:ie},Ee=Fm(Te),De=ne.root||c.Root||Im,Oe=F.root||l.root||{},ke=ne.input||c.Input||Lm;return Ce={...Ce,...F.input??l.input},(0,L.jsxs)(x.Fragment,{children:[!f&&typeof Rm==`function`&&(Mm||=(0,L.jsx)(Rm,{})),(0,L.jsxs)(De,{...Oe,ref:t,onClick:xe,...oe,...!Nu(De)&&{ownerState:{...Te,...Oe.ownerState}},className:z(Ee.root,Oe.className,o,M&&`MuiInputBase-readOnly`),children:[re,(0,L.jsx)(Em.Provider,{value:null,children:(0,L.jsx)(ke,{"aria-invalid":me.error,"aria-describedby":r,autoComplete:i,autoFocus:a,defaultValue:u,disabled:me.disabled,id:g,onAnimationStart:we,name:T,placeholder:j,readOnly:M,required:me.required,rows:N,value:I,onKeyDown:ee,onKeyUp:A,type:ie,...Ce,...!Nu(ke)&&{as:Se,ownerState:{...Te,...Ce.ownerState}},ref:ue,className:z(Ee.input,Ce.className,M&&`MuiInputBase-readOnly`),onBlur:ye,onChange:be,onFocus:ve})}),p,te?te({...me,startAdornment:re}):null]})]})}),Bm={entering:{opacity:1},entered:{opacity:1}},Vm=x.forwardRef(function(e,t){let n=hl(),r={enter:n.transitions.duration.enteringScreen,exit:n.transitions.duration.leavingScreen},{addEndListener:i,appear:a=!0,children:o,easing:s,in:c,onEnter:l,onEntered:u,onEntering:d,onExit:f,onExited:p,onExiting:m,style:h,timeout:g=r,TransitionComponent:_=pu,...v}=e,y=x.useRef(null),b=Ql(y,$p(o),t),S=e=>t=>{if(e){let n=y.current;t===void 0?e(n):e(n,t)}},C=S(d),w=S((e,t)=>{Au(e);let r=ju({style:h,timeout:g,easing:s},{mode:`enter`});e.style.webkitTransition=n.transitions.create(`opacity`,r),e.style.transition=n.transitions.create(`opacity`,r),l&&l(e,t)}),T=S(u),E=S(m),D=S(e=>{let t=ju({style:h,timeout:g,easing:s},{mode:`exit`});e.style.webkitTransition=n.transitions.create(`opacity`,t),e.style.transition=n.transitions.create(`opacity`,t),f&&f(e)}),O=S(p);return(0,L.jsx)(_,{appear:a,in:c,nodeRef:y,onEnter:w,onEntered:T,onEntering:C,onExit:D,onExited:O,onExiting:E,addEndListener:e=>{i&&i(y.current,e)},timeout:g,...v,children:(e,{ownerState:t,...n})=>x.cloneElement(o,{style:{opacity:0,visibility:e===`exited`&&!c?`hidden`:void 0,...Bm[e],...h,...o.props.style},ref:b,...n})})});function Hm(e){return Ro(`MuiBackdrop`,e)}zo(`MuiBackdrop`,[`root`,`invisible`]);var Um=e=>{let{classes:t,invisible:n}=e;return uc({root:[`root`,n&&`invisible`]},Hm,t)},Wm=V(`div`,{name:`MuiBackdrop`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.invisible&&t.invisible]}})({position:`fixed`,display:`flex`,alignItems:`center`,justifyContent:`center`,right:0,bottom:0,top:0,left:0,backgroundColor:`rgba(0, 0, 0, 0.5)`,WebkitTapHighlightColor:`transparent`,variants:[{props:{invisible:!0},style:{backgroundColor:`transparent`}}]}),Gm=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiBackdrop`}),{children:r,className:i,component:a=`div`,invisible:o=!1,open:s,components:c={},componentsProps:l={},slotProps:u={},slots:d={},TransitionComponent:f,transitionDuration:p,...m}=n,h={...n,component:a,invisible:o},g=Um(h),_={component:a,slots:{transition:f,root:c.Root,...d},slotProps:{...l,...u}},[v,y]=Wu(`root`,{elementType:Wm,externalForwardedProps:_,className:z(g.root,i),ownerState:h}),[b,x]=Wu(`transition`,{elementType:Vm,externalForwardedProps:_,ownerState:h});return(0,L.jsx)(b,{in:s,timeout:p,...m,...x,children:(0,L.jsx)(v,{"aria-hidden":!0,...y,classes:g,ref:t,children:r})})}),Km=zo(`MuiBox`,[`root`]),W=Io({themeId:kn,defaultTheme:pl(),defaultClassName:Km.root,generateClassName:Mo.generate});function qm(e){return Ro(`MuiButton`,e)}var Jm=zo(`MuiButton`,`root.text.textInherit.textPrimary.textSecondary.textSuccess.textError.textInfo.textWarning.outlined.outlinedInherit.outlinedPrimary.outlinedSecondary.outlinedSuccess.outlinedError.outlinedInfo.outlinedWarning.contained.containedInherit.containedPrimary.containedSecondary.containedSuccess.containedError.containedInfo.containedWarning.disableElevation.focusVisible.disabled.colorInherit.colorPrimary.colorSecondary.colorSuccess.colorError.colorInfo.colorWarning.textSizeSmall.textSizeMedium.textSizeLarge.outlinedSizeSmall.outlinedSizeMedium.outlinedSizeLarge.containedSizeSmall.containedSizeMedium.containedSizeLarge.sizeMedium.sizeSmall.sizeLarge.fullWidth.startIcon.endIcon.icon.iconSizeSmall.iconSizeMedium.iconSizeLarge.loading.loadingWrapper.loadingIconPlaceholder.loadingIndicator.loadingPositionCenter.loadingPositionStart.loadingPositionEnd`.split(`.`)),Ym=x.createContext({}),Xm=x.createContext(void 0),Zm=e=>{let{color:t,disableElevation:n,fullWidth:r,size:i,variant:a,loading:o,loadingPosition:s,classes:c}=e,l=uc({root:[`root`,o&&`loading`,a,`${a}${H(t)}`,`size${H(i)}`,`${a}Size${H(i)}`,`color${H(t)}`,n&&`disableElevation`,r&&`fullWidth`,o&&`loadingPosition${H(s)}`],startIcon:[`icon`,`startIcon`,`iconSize${H(i)}`],endIcon:[`icon`,`endIcon`,`iconSize${H(i)}`],loadingIndicator:[`loadingIndicator`],loadingWrapper:[`loadingWrapper`]},qm,c);return{...c,...l}},Qm=[{props:{size:`small`},style:{"& > *:nth-of-type(1)":{fontSize:18}}},{props:{size:`medium`},style:{"& > *:nth-of-type(1)":{fontSize:20}}},{props:{size:`large`},style:{"& > *:nth-of-type(1)":{fontSize:22}}}],$m=V(Sd,{shouldForwardProp:e=>vl(e)||e===`classes`,name:`MuiButton`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`${n.variant}${H(n.color)}`],t[`size${H(n.size)}`],t[`${n.variant}Size${H(n.size)}`],n.color===`inherit`&&t.colorInherit,n.disableElevation&&t.disableElevation,n.fullWidth&&t.fullWidth,n.loading&&t.loading]}})(jl(({theme:e})=>{let t=e.palette.mode===`light`?e.palette.grey[300]:e.palette.grey[800],n=e.palette.mode===`light`?e.palette.grey.A100:e.palette.grey[700];return{...e.typography.button,minWidth:64,padding:`6px 16px`,border:0,borderRadius:(e.vars||e).shape.borderRadius,transition:e.transitions.create([`background-color`,`box-shadow`,`border-color`,`color`],{duration:e.transitions.duration.short}),"&:hover":{textDecoration:`none`},[`&.${Jm.disabled}`]:{color:(e.vars||e).palette.action.disabled},variants:[{props:{variant:`contained`},style:{color:`var(--variant-containedColor)`,backgroundColor:`var(--variant-containedBg)`,boxShadow:(e.vars||e).shadows[2],"&:hover":{boxShadow:(e.vars||e).shadows[4],"@media (hover: none)":{boxShadow:(e.vars||e).shadows[2]}},"&:active":{boxShadow:(e.vars||e).shadows[8]},[`&.${Jm.focusVisible}`]:{boxShadow:(e.vars||e).shadows[6]},[`&.${Jm.disabled}`]:{color:(e.vars||e).palette.action.disabled,boxShadow:(e.vars||e).shadows[0],backgroundColor:(e.vars||e).palette.action.disabledBackground}}},{props:{variant:`outlined`},style:{padding:`5px 15px`,border:`1px solid currentColor`,borderColor:`var(--variant-outlinedBorder, currentColor)`,backgroundColor:`var(--variant-outlinedBg)`,color:`var(--variant-outlinedColor)`,[`&.${Jm.disabled}`]:{border:`1px solid ${(e.vars||e).palette.action.disabledBackground}`}}},{props:{variant:`text`},style:{padding:`6px 8px`,color:`var(--variant-textColor)`,backgroundColor:`var(--variant-textBg)`}},...Object.entries(e.palette).filter(Td()).map(([t])=>({props:{color:t},style:{"--variant-textColor":(e.vars||e).palette[t].main,"--variant-outlinedColor":(e.vars||e).palette[t].main,"--variant-outlinedBorder":e.alpha((e.vars||e).palette[t].main,.5),"--variant-containedColor":(e.vars||e).palette[t].contrastText,"--variant-containedBg":(e.vars||e).palette[t].main,"@media (hover: hover)":{"&:hover":{"--variant-containedBg":(e.vars||e).palette[t].dark,"--variant-textBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity),"--variant-outlinedBorder":(e.vars||e).palette[t].main,"--variant-outlinedBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)}}}})),{props:{color:`inherit`},style:{color:`inherit`,borderColor:`currentColor`,"--variant-containedBg":e.vars?e.vars.palette.Button.inheritContainedBg:t,"@media (hover: hover)":{"&:hover":{"--variant-containedBg":e.vars?e.vars.palette.Button.inheritContainedHoverBg:n,"--variant-textBg":e.alpha((e.vars||e).palette.text.primary,(e.vars||e).palette.action.hoverOpacity),"--variant-outlinedBg":e.alpha((e.vars||e).palette.text.primary,(e.vars||e).palette.action.hoverOpacity)}}}},{props:{size:`small`,variant:`text`},style:{padding:`4px 5px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`text`},style:{padding:`8px 11px`,fontSize:e.typography.pxToRem(15)}},{props:{size:`small`,variant:`outlined`},style:{padding:`3px 9px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`outlined`},style:{padding:`7px 21px`,fontSize:e.typography.pxToRem(15)}},{props:{size:`small`,variant:`contained`},style:{padding:`4px 10px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`contained`},style:{padding:`8px 22px`,fontSize:e.typography.pxToRem(15)}},{props:{disableElevation:!0},style:{boxShadow:`none`,"&:hover":{boxShadow:`none`},[`&.${Jm.focusVisible}`]:{boxShadow:`none`},"&:active":{boxShadow:`none`},[`&.${Jm.disabled}`]:{boxShadow:`none`}}},{props:{fullWidth:!0},style:{width:`100%`}},{props:{loadingPosition:`center`},style:{transition:e.transitions.create([`background-color`,`box-shadow`,`border-color`],{duration:e.transitions.duration.short}),[`&.${Jm.loading}`]:{color:`transparent`}}}]}})),eh=V(`span`,{name:`MuiButton`,slot:`StartIcon`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.startIcon,n.loading&&t.startIconLoadingStart,t[`iconSize${H(n.size)}`]]}})(({theme:e})=>({display:`inherit`,marginRight:8,marginLeft:-4,variants:[{props:{size:`small`},style:{marginLeft:-2}},{props:{loadingPosition:`start`,loading:!0},style:{transition:e.transitions.create([`opacity`],{duration:e.transitions.duration.short}),opacity:0}},{props:{loadingPosition:`start`,loading:!0,fullWidth:!0},style:{marginRight:-8}},...Qm]})),th=V(`span`,{name:`MuiButton`,slot:`EndIcon`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.endIcon,n.loading&&t.endIconLoadingEnd,t[`iconSize${H(n.size)}`]]}})(({theme:e})=>({display:`inherit`,marginRight:-4,marginLeft:8,variants:[{props:{size:`small`},style:{marginRight:-2}},{props:{loadingPosition:`end`,loading:!0},style:{transition:e.transitions.create([`opacity`],{duration:e.transitions.duration.short}),opacity:0}},{props:{loadingPosition:`end`,loading:!0,fullWidth:!0},style:{marginLeft:-8}},...Qm]})),nh=V(`span`,{name:`MuiButton`,slot:`LoadingIndicator`})(({theme:e})=>({display:`none`,position:`absolute`,visibility:`visible`,variants:[{props:{loading:!0},style:{display:`flex`}},{props:{loadingPosition:`start`},style:{left:14}},{props:{loadingPosition:`start`,size:`small`},style:{left:10}},{props:{variant:`text`,loadingPosition:`start`},style:{left:6}},{props:{loadingPosition:`center`},style:{left:`50%`,transform:`translate(-50%)`,color:(e.vars||e).palette.action.disabled}},{props:{loadingPosition:`end`},style:{right:14}},{props:{loadingPosition:`end`,size:`small`},style:{right:10}},{props:{variant:`text`,loadingPosition:`end`},style:{right:6}},{props:{loadingPosition:`start`,fullWidth:!0},style:{position:`relative`,left:-10}},{props:{loadingPosition:`end`,fullWidth:!0},style:{position:`relative`,right:-10}}]})),rh=V(`span`,{name:`MuiButton`,slot:`LoadingIconPlaceholder`})({display:`inline-block`,width:`1em`,height:`1em`}),ih=x.forwardRef(function(e,t){let n=x.useContext(Ym),r=x.useContext(Xm),i=Ml({props:Qo(n,e),name:`MuiButton`}),{children:a,color:o=`primary`,component:s=`button`,className:c,disabled:l=!1,disableElevation:u=!1,disableFocusRipple:d=!1,endIcon:f,focusVisibleClassName:p,fullWidth:m=!1,id:h,loading:g=null,loadingIndicator:_,loadingPosition:v=`center`,size:y=`medium`,startIcon:b,type:S,variant:C=`text`,...w}=i,T=Gl(h),E=_??(0,L.jsx)(Ld,{"aria-labelledby":T,color:`inherit`,size:16}),D={...i,color:o,component:s,disabled:l,disableElevation:u,disableFocusRipple:d,fullWidth:m,loading:g,loadingIndicator:E,loadingPosition:v,size:y,type:S,variant:C},O=Zm(D),k=(b||g&&v===`start`)&&(0,L.jsx)(eh,{className:O.startIcon,ownerState:D,children:b||(0,L.jsx)(rh,{className:O.loadingIconPlaceholder,ownerState:D})}),ee=(f||g&&v===`end`)&&(0,L.jsx)(th,{className:O.endIcon,ownerState:D,children:f||(0,L.jsx)(rh,{className:O.loadingIconPlaceholder,ownerState:D})}),A=r||``,j=typeof g==`boolean`?(0,L.jsx)(`span`,{className:O.loadingWrapper,style:{display:`contents`},children:g&&(0,L.jsx)(nh,{className:O.loadingIndicator,ownerState:D,children:E})}):null;return(0,L.jsxs)($m,{ownerState:D,className:z(n.className,O.root,c,A),component:s,disabled:l||g,focusRipple:!d,focusVisibleClassName:z(O.focusVisible,p),ref:t,type:S,id:g?T:h,...w,classes:O,children:[k,v!==`end`&&j,a,v===`end`&&j,ee]})}),ah=typeof kl({})==`function`;const oh=(e,t)=>({WebkitFontSmoothing:`antialiased`,MozOsxFontSmoothing:`grayscale`,boxSizing:`border-box`,WebkitTextSizeAdjust:`100%`,...t&&!e.vars&&{colorScheme:e.palette.mode}}),sh=e=>({color:(e.vars||e).palette.text.primary,...e.typography.body1,backgroundColor:(e.vars||e).palette.background.default,"@media print":{backgroundColor:(e.vars||e).palette.common.white}}),ch=(e,t=!1)=>{let n={};t&&e.colorSchemes&&typeof e.getColorSchemeSelector==`function`&&Object.entries(e.colorSchemes).forEach(([t,r])=>{let i=e.getColorSchemeSelector(t);i.startsWith(`@`)?n[i]={":root":{colorScheme:r.palette?.mode}}:n[i.replace(/\s*&/,``)]={colorScheme:r.palette?.mode}});let r={html:oh(e,t),"*, *::before, *::after":{boxSizing:`inherit`},"strong, b":{fontWeight:e.typography.fontWeightBold},body:{margin:0,...sh(e),"&::backdrop":{backgroundColor:(e.vars||e).palette.background.default}},...n},i=e.components?.MuiCssBaseline?.styleOverrides;return i&&(r=[r,i]),r};var lh=`mui-ecs`,uh=e=>{let t=ch(e,!1),n=Array.isArray(t)?t[0]:t;return!e.vars&&n&&(n.html[`:root:has(${lh})`]={colorScheme:e.palette.mode}),e.colorSchemes&&Object.entries(e.colorSchemes).forEach(([t,r])=>{let i=e.getColorSchemeSelector(t);i.startsWith(`@`)?n[i]={[`:root:not(:has(.${lh}))`]:{colorScheme:r.palette?.mode}}:n[i.replace(/\s*&/,``)]={[`&:not(:has(.${lh}))`]:{colorScheme:r.palette?.mode}}}),t},dh=kl(ah?({theme:e,enableColorScheme:t})=>ch(e,t):({theme:e})=>uh(e));function fh(e){let{children:t,enableColorScheme:n=!1}=Ml({props:e,name:`MuiCssBaseline`});return(0,L.jsxs)(x.Fragment,{children:[ah&&(0,L.jsx)(dh,{enableColorScheme:n}),!ah&&!n&&(0,L.jsx)(`span`,{className:lh,style:{display:`none`}}),t]})}var ph=fh;function mh(e=window){let t=e.document.documentElement.clientWidth;return e.innerWidth-t}function hh(e){let t=Vl(e);return t.body===e?Hl(e).innerWidth>t.documentElement.clientWidth:e.scrollHeight>e.clientHeight}function gh(e,t){t?e.setAttribute(`aria-hidden`,`true`):e.removeAttribute(`aria-hidden`)}function _h(e){return parseInt(Hl(e).getComputedStyle(e).paddingRight,10)||0}function vh(e){let t=[`TEMPLATE`,`SCRIPT`,`STYLE`,`LINK`,`MAP`,`META`,`NOSCRIPT`,`PICTURE`,`COL`,`COLGROUP`,`PARAM`,`SLOT`,`SOURCE`,`TRACK`].includes(e.tagName),n=e.tagName===`INPUT`&&e.getAttribute(`type`)===`hidden`;return t||n}function yh(e,t,n,r,i){let a=[t,n,...r];[].forEach.call(e.children,e=>{let t=!a.includes(e),n=!vh(e);t&&n&&gh(e,i)})}function bh(e,t){let n=-1;return e.some((e,r)=>t(e)?(n=r,!0):!1),n}function xh(e,t){let n=[],r=e.container;if(!t.disableScrollLock){if(hh(r)){let e=mh(Hl(r));n.push({value:r.style.paddingRight,property:`padding-right`,el:r}),r.style.paddingRight=`${_h(r)+e}px`;let t=Vl(r).querySelectorAll(`.mui-fixed`);[].forEach.call(t,t=>{n.push({value:t.style.paddingRight,property:`padding-right`,el:t}),t.style.paddingRight=`${_h(t)+e}px`})}let e;if(r.parentNode instanceof DocumentFragment)e=Vl(r).body;else{let t=r.parentElement,n=Hl(r);e=t?.nodeName===`HTML`&&n.getComputedStyle(t).overflowY===`scroll`?t:r}n.push({value:e.style.overflow,property:`overflow`,el:e},{value:e.style.overflowX,property:`overflow-x`,el:e},{value:e.style.overflowY,property:`overflow-y`,el:e}),e.style.overflow=`hidden`}return()=>{n.forEach(({value:e,el:t,property:n})=>{e?t.style.setProperty(n,e):t.style.removeProperty(n)})}}function Sh(e){let t=[];return[].forEach.call(e.children,e=>{e.getAttribute(`aria-hidden`)===`true`&&t.push(e)}),t}var Ch=class{constructor(){this.modals=[],this.containers=[]}add(e,t){let n=this.modals.indexOf(e);if(n!==-1)return n;n=this.modals.length,this.modals.push(e),e.modalRef&&gh(e.modalRef,!1);let r=Sh(t);yh(t,e.mount,e.modalRef,r,!0);let i=bh(this.containers,e=>e.container===t);return i===-1?(this.containers.push({modals:[e],container:t,restore:null,hiddenSiblings:r}),n):(this.containers[i].modals.push(e),n)}mount(e,t){let n=bh(this.containers,t=>t.modals.includes(e)),r=this.containers[n];r.restore||=xh(r,t)}remove(e,t=!0){let n=this.modals.indexOf(e);if(n===-1)return n;let r=bh(this.containers,t=>t.modals.includes(e)),i=this.containers[r];if(i.modals.splice(i.modals.indexOf(e),1),this.modals.splice(n,1),i.modals.length===0)i.restore&&i.restore(),e.modalRef&&gh(e.modalRef,t),yh(i.container,e.mount,e.modalRef,i.hiddenSiblings,!1),this.containers.splice(r,1);else{let e=i.modals[i.modals.length-1];e.modalRef&&gh(e.modalRef,!1)}return n}isTopModal(e){return this.modals.length>0&&this.modals[this.modals.length-1]===e}},wh=[`input`,`select`,`textarea`,`a[href]`,`button`,`[tabindex]`,`audio[controls]`,`video[controls]`,`[contenteditable]:not([contenteditable="false"])`].join(`,`);function Th(e){let t=parseInt(e.getAttribute(`tabindex`)||``,10);return Number.isNaN(t)?e.contentEditable===`true`||(e.nodeName===`AUDIO`||e.nodeName===`VIDEO`||e.nodeName===`DETAILS`)&&e.getAttribute(`tabindex`)===null?0:e.tabIndex:t}function Eh(e){if(e.tagName!==`INPUT`||e.type!==`radio`||!e.name)return!1;let t=t=>e.ownerDocument.querySelector(`input[type="radio"]${t}`),n=t(`[name="${e.name}"]:checked`);return n||=t(`[name="${e.name}"]`),n!==e}function Dh(e){return!(e.disabled||e.tagName===`INPUT`&&e.type===`hidden`||Eh(e))}function Oh(e){let t=[],n=[];return Array.from(e.querySelectorAll(wh)).forEach((e,r)=>{let i=Th(e);i===-1||!Dh(e)||(i===0?t.push(e):n.push({documentOrder:r,tabIndex:i,node:e}))}),n.sort((e,t)=>e.tabIndex===t.tabIndex?e.documentOrder-t.documentOrder:e.tabIndex-t.tabIndex).map(e=>e.node).concat(t)}function kh(){return!0}function Ah(e){let{children:t,disableAutoFocus:n=!1,disableEnforceFocus:r=!1,disableRestoreFocus:i=!1,getTabbable:a=Oh,isEnabled:o=kh,open:s}=e,c=x.useRef(!1),l=x.useRef(null),u=x.useRef(null),d=x.useRef(null),f=x.useRef(null),p=x.useRef(!1),m=x.useRef(null),h=Zl($p(t),m),g=x.useRef(null);x.useEffect(()=>{!s||!m.current||(p.current=!n)},[n,s]),x.useEffect(()=>{if(!s||!m.current)return;let e=Vl(m.current);return m.current.contains(e.activeElement)||(m.current.hasAttribute(`tabIndex`)||m.current.setAttribute(`tabIndex`,`-1`),p.current&&m.current.focus()),()=>{i||(d.current&&d.current.focus&&(c.current=!0,d.current.focus()),d.current=null)}},[s]),x.useEffect(()=>{if(!s||!m.current)return;let e=Vl(m.current),t=t=>{g.current=t,!(r||!o()||t.key!==`Tab`)&&e.activeElement===m.current&&t.shiftKey&&(c.current=!0,u.current&&u.current.focus())},n=()=>{let t=m.current;if(t===null)return;if(!e.hasFocus()||!o()||c.current){c.current=!1;return}if(t.contains(e.activeElement)||r&&e.activeElement!==l.current&&e.activeElement!==u.current)return;if(e.activeElement!==f.current)f.current=null;else if(f.current!==null)return;if(!p.current)return;let n=[];if((e.activeElement===l.current||e.activeElement===u.current)&&(n=a(m.current)),n.length>0){let e=!!(g.current?.shiftKey&&g.current?.key===`Tab`),t=n[0],r=n[n.length-1];typeof t!=`string`&&typeof r!=`string`&&(e?r.focus():t.focus())}else t.focus()};e.addEventListener(`focusin`,n),e.addEventListener(`keydown`,t,!0);let i=setInterval(()=>{e.activeElement&&e.activeElement.tagName===`BODY`&&n()},50);return()=>{clearInterval(i),e.removeEventListener(`focusin`,n),e.removeEventListener(`keydown`,t,!0)}},[n,r,i,o,s,a]);let _=e=>{d.current===null&&(d.current=e.relatedTarget),p.current=!0,f.current=e.target;let n=t.props.onFocus;n&&n(e)},v=e=>{d.current===null&&(d.current=e.relatedTarget),p.current=!0};return(0,L.jsxs)(x.Fragment,{children:[(0,L.jsx)(`div`,{tabIndex:s?0:-1,onFocus:v,ref:l,"data-testid":`sentinelStart`}),x.cloneElement(t,{ref:h,onFocus:_}),(0,L.jsx)(`div`,{tabIndex:s?0:-1,onFocus:v,ref:u,"data-testid":`sentinelEnd`})]})}var jh=Ah;function Mh(e){return typeof e==`function`?e():e}function Nh(e){return e?e.props.hasOwnProperty(`in`):!1}var Ph=()=>{},Fh=new Ch;function Ih(e){let{container:t,disableEscapeKeyDown:n=!1,disableScrollLock:r=!1,closeAfterTransition:i=!1,onTransitionEnter:a,onTransitionExited:o,children:s,onClose:c,open:l,rootRef:u}=e,d=x.useRef({}),f=x.useRef(null),p=x.useRef(null),m=Zl(p,u),[h,g]=x.useState(!l),_=Nh(s),v=!0;(e[`aria-hidden`]===`false`||e[`aria-hidden`]===!1)&&(v=!1);let y=()=>Vl(f.current),b=()=>(d.current.modalRef=p.current,d.current.mount=f.current,d.current),S=()=>{Fh.mount(b(),{disableScrollLock:r}),p.current&&(p.current.scrollTop=0)},C=Yl(()=>{let e=Mh(t)||y().body;Fh.add(b(),e),p.current&&S()}),w=()=>Fh.isTopModal(b()),T=Yl(e=>{f.current=e,e&&(l&&w()?S():p.current&&gh(p.current,v))}),E=x.useCallback(()=>{Fh.remove(b(),v)},[v]);x.useEffect(()=>()=>{E()},[E]),x.useEffect(()=>{l?C():(!_||!i)&&E()},[l,E,_,i,C]);let D=e=>t=>{e.onKeyDown?.(t),!(t.key!==`Escape`||t.which===229||!w())&&(n||(t.stopPropagation(),c&&c(t,`escapeKeyDown`)))},O=e=>t=>{e.onClick?.(t),t.target===t.currentTarget&&c&&c(t,`backdropClick`)};return{getRootProps:(t={})=>{let n=zu(e);delete n.onTransitionEnter,delete n.onTransitionExited;let r={...n,...t};return{role:`presentation`,...r,onKeyDown:D(r),ref:m}},getBackdropProps:(e={})=>{let t=e;return{"aria-hidden":!0,...t,onClick:O(t),open:l}},getTransitionProps:()=>({onEnter:El(()=>{g(!1),a&&a()},s?.props.onEnter??Ph),onExited:El(()=>{g(!0),o&&o(),i&&E()},s?.props.onExited??Ph)}),rootRef:m,portalRef:T,isTopModal:w,exited:h,hasTransition:_}}var Lh=Ih;function Rh(e){return Ro(`MuiModal`,e)}zo(`MuiModal`,[`root`,`hidden`,`backdrop`]);var zh=e=>{let{open:t,exited:n,classes:r}=e;return uc({root:[`root`,!t&&n&&`hidden`],backdrop:[`backdrop`]},Rh,r)},Bh=V(`div`,{name:`MuiModal`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.open&&n.exited&&t.hidden]}})(jl(({theme:e})=>({position:`fixed`,zIndex:(e.vars||e).zIndex.modal,right:0,bottom:0,top:0,left:0,variants:[{props:({ownerState:e})=>!e.open&&e.exited,style:{visibility:`hidden`}}]}))),Vh=V(Gm,{name:`MuiModal`,slot:`Backdrop`})({zIndex:-1}),Hh=x.forwardRef(function(e,t){let n=Ml({name:`MuiModal`,props:e}),{BackdropComponent:r=Vh,BackdropProps:i,classes:a,className:o,closeAfterTransition:s=!1,children:c,container:l,component:u,components:d={},componentsProps:f={},disableAutoFocus:p=!1,disableEnforceFocus:m=!1,disableEscapeKeyDown:h=!1,disablePortal:g=!1,disableRestoreFocus:_=!1,disableScrollLock:v=!1,hideBackdrop:y=!1,keepMounted:b=!1,onClose:S,onTransitionEnter:C,onTransitionExited:w,open:T,slotProps:E={},slots:D={},theme:O,...k}=n,ee={...n,closeAfterTransition:s,disableAutoFocus:p,disableEnforceFocus:m,disableEscapeKeyDown:h,disablePortal:g,disableRestoreFocus:_,disableScrollLock:v,hideBackdrop:y,keepMounted:b},{getRootProps:A,getBackdropProps:j,getTransitionProps:M,portalRef:te,isTopModal:N,exited:P,hasTransition:F}=Lh({...ee,rootRef:t}),ne={...ee,exited:P},re=zh(ne),ie={};if(c.props.tabIndex===void 0&&(ie.tabIndex=`-1`),F){let{onEnter:e,onExited:t}=M();ie.onEnter=e,ie.onExited=t}let ae={slots:{root:d.Root,backdrop:d.Backdrop,...D},slotProps:{...f,...E}},[oe,I]=Wu(`root`,{ref:t,elementType:Bh,externalForwardedProps:{...ae,...k,component:u},getSlotProps:A,ownerState:ne,className:z(o,re?.root,!ne.open&&ne.exited&&re?.hidden)}),[se,ce]=Wu(`backdrop`,{ref:i?.ref,elementType:r,externalForwardedProps:ae,shouldForwardComponentProp:!0,additionalProps:i,getSlotProps:e=>j({...e,onClick:t=>{e?.onClick&&e.onClick(t)}}),className:z(i?.className,re?.backdrop),ownerState:ne});return!b&&!T&&(!F||P)?null:(0,L.jsx)(nm,{ref:te,container:l,disablePortal:g,children:(0,L.jsxs)(oe,{...I,children:[!y&&r?(0,L.jsx)(se,{...ce}):null,(0,L.jsx)(jh,{disableEnforceFocus:m,disableAutoFocus:p,disableRestoreFocus:_,isEnabled:N,open:T,children:x.cloneElement(c,ie)})]})})});function Uh(e){return Ro(`MuiDialog`,e)}var Wh=zo(`MuiDialog`,[`root`,`scrollPaper`,`scrollBody`,`container`,`paper`,`paperScrollPaper`,`paperScrollBody`,`paperWidthFalse`,`paperWidthXs`,`paperWidthSm`,`paperWidthMd`,`paperWidthLg`,`paperWidthXl`,`paperFullWidth`,`paperFullScreen`]),Gh=x.createContext({}),Kh=V(Gm,{name:`MuiDialog`,slot:`Backdrop`,overrides:(e,t)=>t.backdrop})({zIndex:-1}),qh=e=>{let{classes:t,scroll:n,maxWidth:r,fullWidth:i,fullScreen:a}=e;return uc({root:[`root`],container:[`container`,`scroll${H(n)}`],paper:[`paper`,`paperScroll${H(n)}`,`paperWidth${H(String(r))}`,i&&`paperFullWidth`,a&&`paperFullScreen`]},Uh,t)},Jh=V(Hh,{name:`MuiDialog`,slot:`Root`})({"@media print":{position:`absolute !important`}}),Yh=V(`div`,{name:`MuiDialog`,slot:`Container`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.container,t[`scroll${H(n.scroll)}`]]}})({height:`100%`,"@media print":{height:`auto`},outline:0,variants:[{props:{scroll:`paper`},style:{display:`flex`,justifyContent:`center`,alignItems:`center`}},{props:{scroll:`body`},style:{overflowY:`auto`,overflowX:`hidden`,textAlign:`center`,"&::after":{content:`""`,display:`inline-block`,verticalAlign:`middle`,height:`100%`,width:`0`}}}]}),Xh=V(td,{name:`MuiDialog`,slot:`Paper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.paper,t[`scrollPaper${H(n.scroll)}`],t[`paperWidth${H(String(n.maxWidth))}`],n.fullWidth&&t.paperFullWidth,n.fullScreen&&t.paperFullScreen]}})(jl(({theme:e})=>({margin:32,position:`relative`,overflowY:`auto`,"@media print":{overflowY:`visible`,boxShadow:`none`},variants:[{props:{scroll:`paper`},style:{display:`flex`,flexDirection:`column`,maxHeight:`calc(100% - 64px)`}},{props:{scroll:`body`},style:{display:`inline-block`,verticalAlign:`middle`,textAlign:`initial`}},{props:({ownerState:e})=>!e.maxWidth,style:{maxWidth:`calc(100% - 64px)`}},{props:{maxWidth:`xs`},style:{maxWidth:e.breakpoints.unit===`px`?Math.max(e.breakpoints.values.xs,444):`max(${e.breakpoints.values.xs}${e.breakpoints.unit}, 444px)`,[`&.${Wh.paperScrollBody}`]:{[e.breakpoints.down(Math.max(e.breakpoints.values.xs,444)+64)]:{maxWidth:`calc(100% - 64px)`}}}},...Object.keys(e.breakpoints.values).filter(e=>e!==`xs`).map(t=>({props:{maxWidth:t},style:{maxWidth:`${e.breakpoints.values[t]}${e.breakpoints.unit}`,[`&.${Wh.paperScrollBody}`]:{[e.breakpoints.down(e.breakpoints.values[t]+64)]:{maxWidth:`calc(100% - 64px)`}}}})),{props:({ownerState:e})=>e.fullWidth,style:{width:`calc(100% - 64px)`}},{props:({ownerState:e})=>e.fullScreen,style:{margin:0,width:`100%`,maxWidth:`100%`,height:`100%`,maxHeight:`none`,borderRadius:0,[`&.${Wh.paperScrollBody}`]:{margin:0,maxWidth:`100%`}}}]}))),Zh=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiDialog`}),r=hl(),i={enter:r.transitions.duration.enteringScreen,exit:r.transitions.duration.leavingScreen},{"aria-describedby":a,"aria-labelledby":o,"aria-modal":s=!0,BackdropComponent:c,BackdropProps:l,children:u,className:d,disableEscapeKeyDown:f=!1,fullScreen:p=!1,fullWidth:m=!1,maxWidth:h=`sm`,onClick:g,onClose:_,open:v,PaperComponent:y=td,PaperProps:b={},scroll:S=`paper`,slots:C={},slotProps:w={},TransitionComponent:T=Vm,transitionDuration:E=i,TransitionProps:D,...O}=n,k={...n,disableEscapeKeyDown:f,fullScreen:p,fullWidth:m,maxWidth:h,scroll:S},ee=qh(k),A=x.useRef(),j=e=>{A.current=e.target===e.currentTarget},M=e=>{g&&g(e),A.current&&(A.current=null,_&&_(e,`backdropClick`))},te=Ls(o),N=x.useMemo(()=>({titleId:te}),[te]),P={slots:{transition:T,...C},slotProps:{transition:D,paper:b,backdrop:l,...w}},[F,ne]=Wu(`root`,{elementType:Jh,shouldForwardComponentProp:!0,externalForwardedProps:P,ownerState:k,className:z(ee.root,d),ref:t}),[re,ie]=Wu(`backdrop`,{elementType:Kh,shouldForwardComponentProp:!0,externalForwardedProps:P,ownerState:k}),[ae,oe]=Wu(`paper`,{elementType:Xh,shouldForwardComponentProp:!0,externalForwardedProps:P,ownerState:k,className:z(ee.paper,b.className)}),[I,se]=Wu(`container`,{elementType:Yh,externalForwardedProps:P,ownerState:k,className:ee.container}),[ce,le]=Wu(`transition`,{elementType:Vm,externalForwardedProps:P,ownerState:k,additionalProps:{appear:!0,in:v,timeout:E,role:`presentation`}});return(0,L.jsx)(F,{closeAfterTransition:!0,slots:{backdrop:re},slotProps:{backdrop:{transitionDuration:E,as:c,...ie}},disableEscapeKeyDown:f,onClose:_,open:v,onClick:M,...ne,...O,children:(0,L.jsx)(ce,{...le,children:(0,L.jsx)(I,{onMouseDown:j,...se,children:(0,L.jsx)(ae,{as:y,elevation:24,role:`dialog`,"aria-describedby":a,"aria-labelledby":te,"aria-modal":s,...oe,children:(0,L.jsx)(Gh.Provider,{value:N,children:u})})})})})});function Qh(e){return Ro(`MuiDialogContent`,e)}zo(`MuiDialogContent`,[`root`,`dividers`]);var $h=zo(`MuiDialogTitle`,[`root`]),eg=e=>{let{classes:t,dividers:n}=e;return uc({root:[`root`,n&&`dividers`]},Qh,t)},tg=V(`div`,{name:`MuiDialogContent`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.dividers&&t.dividers]}})(jl(({theme:e})=>({flex:`1 1 auto`,WebkitOverflowScrolling:`touch`,overflowY:`auto`,padding:`20px 24px`,variants:[{props:({ownerState:e})=>e.dividers,style:{padding:`16px 24px`,borderTop:`1px solid ${(e.vars||e).palette.divider}`,borderBottom:`1px solid ${(e.vars||e).palette.divider}`}},{props:({ownerState:e})=>!e.dividers,style:{[`.${$h.root} + &`]:{paddingTop:0}}}]}))),ng=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiDialogContent`}),{className:r,dividers:i=!1,...a}=n,o={...n,dividers:i};return(0,L.jsx)(tg,{className:z(eg(o).root,r),ownerState:o,ref:t,...a})});function rg(e){return Ro(`MuiDivider`,e)}zo(`MuiDivider`,[`root`,`absolute`,`fullWidth`,`inset`,`middle`,`flexItem`,`light`,`vertical`,`withChildren`,`withChildrenVertical`,`textAlignRight`,`textAlignLeft`,`wrapper`,`wrapperVertical`]);var ig=e=>{let{absolute:t,children:n,classes:r,flexItem:i,light:a,orientation:o,textAlign:s,variant:c}=e;return uc({root:[`root`,t&&`absolute`,c,a&&`light`,o===`vertical`&&`vertical`,i&&`flexItem`,n&&`withChildren`,n&&o===`vertical`&&`withChildrenVertical`,s===`right`&&o!==`vertical`&&`textAlignRight`,s===`left`&&o!==`vertical`&&`textAlignLeft`],wrapper:[`wrapper`,o===`vertical`&&`wrapperVertical`]},rg,r)},ag=V(`div`,{name:`MuiDivider`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.absolute&&t.absolute,t[n.variant],n.light&&t.light,n.orientation===`vertical`&&t.vertical,n.flexItem&&t.flexItem,n.children&&t.withChildren,n.children&&n.orientation===`vertical`&&t.withChildrenVertical,n.textAlign===`right`&&n.orientation!==`vertical`&&t.textAlignRight,n.textAlign===`left`&&n.orientation!==`vertical`&&t.textAlignLeft]}})(jl(({theme:e})=>({margin:0,flexShrink:0,borderWidth:0,borderStyle:`solid`,borderColor:(e.vars||e).palette.divider,borderBottomWidth:`thin`,variants:[{props:{absolute:!0},style:{position:`absolute`,bottom:0,left:0,width:`100%`}},{props:{light:!0},style:{borderColor:e.alpha((e.vars||e).palette.divider,.08)}},{props:{variant:`inset`},style:{marginLeft:72}},{props:{variant:`middle`,orientation:`horizontal`},style:{marginLeft:e.spacing(2),marginRight:e.spacing(2)}},{props:{variant:`middle`,orientation:`vertical`},style:{marginTop:e.spacing(1),marginBottom:e.spacing(1)}},{props:{orientation:`vertical`},style:{height:`100%`,borderBottomWidth:0,borderRightWidth:`thin`}},{props:{flexItem:!0},style:{alignSelf:`stretch`,height:`auto`}},{props:({ownerState:e})=>!!e.children,style:{display:`flex`,textAlign:`center`,border:0,borderTopStyle:`solid`,borderLeftStyle:`solid`,"&::before, &::after":{content:`""`,alignSelf:`center`}}},{props:({ownerState:e})=>e.children&&e.orientation!==`vertical`,style:{"&::before, &::after":{width:`100%`,borderTop:`thin solid ${(e.vars||e).palette.divider}`,borderTopStyle:`inherit`}}},{props:({ownerState:e})=>e.orientation===`vertical`&&e.children,style:{flexDirection:`column`,"&::before, &::after":{height:`100%`,borderLeft:`thin solid ${(e.vars||e).palette.divider}`,borderLeftStyle:`inherit`}}},{props:({ownerState:e})=>e.textAlign===`right`&&e.orientation!==`vertical`,style:{"&::before":{width:`90%`},"&::after":{width:`10%`}}},{props:({ownerState:e})=>e.textAlign===`left`&&e.orientation!==`vertical`,style:{"&::before":{width:`10%`},"&::after":{width:`90%`}}}]}))),og=V(`span`,{name:`MuiDivider`,slot:`Wrapper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.wrapper,n.orientation===`vertical`&&t.wrapperVertical]}})(jl(({theme:e})=>({display:`inline-block`,paddingLeft:`calc(${e.spacing(1)} * 1.2)`,paddingRight:`calc(${e.spacing(1)} * 1.2)`,whiteSpace:`nowrap`,variants:[{props:{orientation:`vertical`},style:{paddingTop:`calc(${e.spacing(1)} * 1.2)`,paddingBottom:`calc(${e.spacing(1)} * 1.2)`}}]}))),sg=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiDivider`}),{absolute:r=!1,children:i,className:a,orientation:o=`horizontal`,component:s=i||o===`vertical`?`div`:`hr`,flexItem:c=!1,light:l=!1,role:u=s===`hr`?void 0:`separator`,textAlign:d=`center`,variant:f=`fullWidth`,...p}=n,m={...n,absolute:r,component:s,flexItem:c,light:l,orientation:o,role:u,textAlign:d,variant:f},h=ig(m);return(0,L.jsx)(ag,{as:s,className:z(h.root,a),role:u,ref:t,ownerState:m,"aria-orientation":u===`separator`&&(s!==`hr`||o===`vertical`)?o:void 0,...p,children:i?(0,L.jsx)(og,{className:h.wrapper,ownerState:m,children:i}):null})});sg&&(sg.muiSkipListHighlight=!0);var cg=sg;function lg(e){return`scale(${e}, ${e**2})`}var ug={entering:{opacity:1,transform:lg(1)},entered:{opacity:1,transform:`none`}},dg=typeof navigator<`u`&&/^((?!chrome|android).)*(safari|mobile)/i.test(navigator.userAgent)&&/(os |version\/)15(.|_)4/i.test(navigator.userAgent),fg=x.forwardRef(function(e,t){let{addEndListener:n,appear:r=!0,children:i,easing:a,in:o,onEnter:s,onEntered:c,onEntering:l,onExit:u,onExited:d,onExiting:f,style:p,timeout:m=`auto`,TransitionComponent:h=pu,...g}=e,_=ku(),v=x.useRef(),y=hl(),b=x.useRef(null),S=Ql(b,$p(i),t),C=e=>t=>{if(e){let n=b.current;t===void 0?e(n):e(n,t)}},w=C(l),T=C((e,t)=>{Au(e);let{duration:n,delay:r,easing:i}=ju({style:p,timeout:m,easing:a},{mode:`enter`}),o;m===`auto`?(o=y.transitions.getAutoHeightDuration(e.clientHeight),v.current=o):o=n,e.style.transition=[y.transitions.create(`opacity`,{duration:o,delay:r}),y.transitions.create(`transform`,{duration:dg?o:o*.666,delay:r,easing:i})].join(`,`),s&&s(e,t)}),E=C(c),D=C(f),O=C(e=>{let{duration:t,delay:n,easing:r}=ju({style:p,timeout:m,easing:a},{mode:`exit`}),i;m===`auto`?(i=y.transitions.getAutoHeightDuration(e.clientHeight),v.current=i):i=t,e.style.transition=[y.transitions.create(`opacity`,{duration:i,delay:n}),y.transitions.create(`transform`,{duration:dg?i:i*.666,delay:dg?n:n||i*.333,easing:r})].join(`,`),e.style.opacity=0,e.style.transform=lg(.75),u&&u(e)}),k=C(d);return(0,L.jsx)(h,{appear:r,in:o,nodeRef:b,onEnter:T,onEntered:E,onEntering:w,onExit:O,onExited:k,onExiting:D,addEndListener:e=>{m===`auto`&&_.start(v.current||0,e),n&&n(b.current,e)},timeout:m===`auto`?null:m,...g,children:(e,{ownerState:t,...n})=>x.cloneElement(i,{style:{opacity:0,transform:lg(.75),visibility:e===`exited`&&!o?`hidden`:void 0,...ug[e],...p,...i.props.style},ref:S,...n})})});fg&&(fg.muiSupportAuto=!0);var pg=fg;function mg(e){return Ro(`MuiLink`,e)}var hg=zo(`MuiLink`,[`root`,`underlineNone`,`underlineHover`,`underlineAlways`,`button`,`focusVisible`]),gg=({theme:e,ownerState:t})=>{let n=t.color;if(`colorSpace`in e&&e.colorSpace){let r=ha(e,`palette.${n}.main`)||ha(e,`palette.${n}`)||t.color;return e.alpha(r,.4)}let r=ha(e,`palette.${n}.main`,!1)||ha(e,`palette.${n}`,!1)||t.color,i=ha(e,`palette.${n}.mainChannel`)||ha(e,`palette.${n}Channel`);return`vars`in e&&i?`rgba(${i} / 0.4)`:ds(r,.4)},_g={primary:!0,secondary:!0,error:!0,info:!0,success:!0,warning:!0,textPrimary:!0,textSecondary:!0,textDisabled:!0},vg=e=>{let{classes:t,component:n,focusVisible:r,underline:i}=e;return uc({root:[`root`,`underline${H(i)}`,n===`button`&&`button`,r&&`focusVisible`]},mg,t)},yg=V(U,{name:`MuiLink`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[`underline${H(n.underline)}`],n.component===`button`&&t.button]}})(jl(({theme:e})=>({variants:[{props:{underline:`none`},style:{textDecoration:`none`}},{props:{underline:`hover`},style:{textDecoration:`none`,"&:hover":{textDecoration:`underline`}}},{props:{underline:`always`},style:{textDecoration:`underline`,"&:hover":{textDecorationColor:`inherit`}}},{props:({underline:e,ownerState:t})=>e===`always`&&t.color!==`inherit`,style:{textDecorationColor:`var(--Link-underlineColor)`}},{props:({underline:e,ownerState:t})=>e===`always`&&t.color===`inherit`,style:e.colorSpace?{textDecorationColor:e.alpha(`currentColor`,.4)}:null},...Object.entries(e.palette).filter(Td()).map(([t])=>({props:{underline:`always`,color:t},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette[t].main,.4)}})),{props:{underline:`always`,color:`textPrimary`},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette.text.primary,.4)}},{props:{underline:`always`,color:`textSecondary`},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette.text.secondary,.4)}},{props:{underline:`always`,color:`textDisabled`},style:{"--Link-underlineColor":(e.vars||e).palette.text.disabled}},{props:{component:`button`},style:{position:`relative`,WebkitTapHighlightColor:`transparent`,backgroundColor:`transparent`,outline:0,border:0,margin:0,borderRadius:0,padding:0,cursor:`pointer`,userSelect:`none`,verticalAlign:`middle`,MozAppearance:`none`,WebkitAppearance:`none`,"&::-moz-focus-inner":{borderStyle:`none`},[`&.${hg.focusVisible}`]:{outline:`auto`}}}]}))),bg=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiLink`}),r=hl(),{className:i,color:a=`primary`,component:o=`a`,onBlur:s,onFocus:c,TypographyClasses:l,underline:u=`always`,variant:d=`inherit`,sx:f,...p}=n,[m,h]=x.useState(!1),g=e=>{nd(e.target)||h(!1),s&&s(e)},_=e=>{nd(e.target)&&h(!0),c&&c(e)},v={...n,color:a,component:o,focusVisible:m,underline:u,variant:d};return(0,L.jsx)(yg,{color:a,className:z(vg(v).root,i),classes:l,component:o,onBlur:g,onFocus:_,ref:t,ownerState:v,variant:d,...p,sx:[..._g[a]===void 0?[{color:a}]:[],...Array.isArray(f)?f:[f]],style:{...p.style,...u===`always`&&a!==`inherit`&&!_g[a]&&{"--Link-underlineColor":gg({theme:r,ownerState:v})}}})}),xg=x.createContext({});function Sg(e){return Ro(`MuiList`,e)}zo(`MuiList`,[`root`,`padding`,`dense`,`subheader`]);var Cg=e=>{let{classes:t,disablePadding:n,dense:r,subheader:i}=e;return uc({root:[`root`,!n&&`padding`,r&&`dense`,i&&`subheader`]},Sg,t)},wg=V(`ul`,{name:`MuiList`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.disablePadding&&t.padding,n.dense&&t.dense,n.subheader&&t.subheader]}})({listStyle:`none`,margin:0,padding:0,position:`relative`,variants:[{props:({ownerState:e})=>!e.disablePadding,style:{paddingTop:8,paddingBottom:8}},{props:({ownerState:e})=>e.subheader,style:{paddingTop:0}}]}),G=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiList`}),{children:r,className:i,component:a=`ul`,dense:o=!1,disablePadding:s=!1,subheader:c,...l}=n,u=x.useMemo(()=>({dense:o}),[o]),d={...n,component:a,dense:o,disablePadding:s},f=Cg(d);return(0,L.jsx)(xg.Provider,{value:u,children:(0,L.jsxs)(wg,{as:a,className:z(f.root,i),ref:t,ownerState:d,...l,children:[c,r]})})});function Tg(e){return Ro(`MuiListItem`,e)}zo(`MuiListItem`,[`root`,`container`,`dense`,`alignItemsFlexStart`,`divider`,`gutters`,`padding`,`secondaryAction`]);function Eg(e){return Ro(`MuiListItemButton`,e)}var Dg=zo(`MuiListItemButton`,[`root`,`focusVisible`,`dense`,`alignItemsFlexStart`,`disabled`,`divider`,`gutters`,`selected`]);const Og=(e,t)=>{let{ownerState:n}=e;return[t.root,n.dense&&t.dense,n.alignItems===`flex-start`&&t.alignItemsFlexStart,n.divider&&t.divider,!n.disableGutters&&t.gutters]};var kg=e=>{let{alignItems:t,classes:n,dense:r,disabled:i,disableGutters:a,divider:o,selected:s}=e,c=uc({root:[`root`,r&&`dense`,!a&&`gutters`,o&&`divider`,i&&`disabled`,t===`flex-start`&&`alignItemsFlexStart`,s&&`selected`]},Eg,n);return{...n,...c}},Ag=V(Sd,{shouldForwardProp:e=>vl(e)||e===`classes`,name:`MuiListItemButton`,slot:`Root`,overridesResolver:Og})(jl(({theme:e})=>({display:`flex`,flexGrow:1,justifyContent:`flex-start`,alignItems:`center`,position:`relative`,textDecoration:`none`,minWidth:0,boxSizing:`border-box`,textAlign:`left`,paddingTop:8,paddingBottom:8,transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),"&:hover":{textDecoration:`none`,backgroundColor:(e.vars||e).palette.action.hover,"@media (hover: none)":{backgroundColor:`transparent`}},[`&.${Dg.selected}`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,(e.vars||e).palette.action.selectedOpacity),[`&.${Dg.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)}},[`&.${Dg.selected}:hover`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.hoverOpacity}`),"@media (hover: none)":{backgroundColor:e.alpha((e.vars||e).palette.primary.main,(e.vars||e).palette.action.selectedOpacity)}},[`&.${Dg.focusVisible}`]:{backgroundColor:(e.vars||e).palette.action.focus},[`&.${Dg.disabled}`]:{opacity:(e.vars||e).palette.action.disabledOpacity},variants:[{props:({ownerState:e})=>e.divider,style:{borderBottom:`1px solid ${(e.vars||e).palette.divider}`,backgroundClip:`padding-box`}},{props:{alignItems:`flex-start`},style:{alignItems:`flex-start`}},{props:({ownerState:e})=>!e.disableGutters,style:{paddingLeft:16,paddingRight:16}},{props:({ownerState:e})=>e.dense,style:{paddingTop:4,paddingBottom:4}}]}))),jg=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiListItemButton`}),{alignItems:r=`center`,autoFocus:i=!1,component:a=`div`,children:o,dense:s=!1,disableGutters:c=!1,divider:l=!1,focusVisibleClassName:u,selected:d=!1,className:f,...p}=n,m=x.useContext(xg),h=x.useMemo(()=>({dense:s||m.dense||!1,alignItems:r,disableGutters:c}),[r,m.dense,s,c]),g=x.useRef(null);Wl(()=>{i&&g.current&&g.current.focus()},[i]);let _={...n,alignItems:r,dense:h.dense,disableGutters:c,divider:l,selected:d},v=kg(_),y=Ql(g,t);return(0,L.jsx)(xg.Provider,{value:h,children:(0,L.jsx)(Ag,{ref:y,href:p.href||p.to,component:(p.href||p.to)&&a===`div`?`button`:a,focusVisibleClassName:z(v.focusVisible,u),ownerState:_,className:z(v.root,f),...p,classes:v,children:o})})});function Mg(e){return Ro(`MuiListItemSecondaryAction`,e)}zo(`MuiListItemSecondaryAction`,[`root`,`disableGutters`]);var Ng=e=>{let{disableGutters:t,classes:n}=e;return uc({root:[`root`,t&&`disableGutters`]},Mg,n)},Pg=V(`div`,{name:`MuiListItemSecondaryAction`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.disableGutters&&t.disableGutters]}})({position:`absolute`,right:16,top:`50%`,transform:`translateY(-50%)`,variants:[{props:({ownerState:e})=>e.disableGutters,style:{right:0}}]}),Fg=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiListItemSecondaryAction`}),{className:r,...i}=n,a=x.useContext(xg),o={...n,disableGutters:a.disableGutters};return(0,L.jsx)(Pg,{className:z(Ng(o).root,r),ownerState:o,ref:t,...i})});Fg.muiName=`ListItemSecondaryAction`;var Ig=Fg;const Lg=(e,t)=>{let{ownerState:n}=e;return[t.root,n.dense&&t.dense,n.alignItems===`flex-start`&&t.alignItemsFlexStart,n.divider&&t.divider,!n.disableGutters&&t.gutters,!n.disablePadding&&t.padding,n.hasSecondaryAction&&t.secondaryAction]};var Rg=e=>{let{alignItems:t,classes:n,dense:r,disableGutters:i,disablePadding:a,divider:o,hasSecondaryAction:s}=e;return uc({root:[`root`,r&&`dense`,!i&&`gutters`,!a&&`padding`,o&&`divider`,t===`flex-start`&&`alignItemsFlexStart`,s&&`secondaryAction`],container:[`container`]},Tg,n)};const zg=V(`div`,{name:`MuiListItem`,slot:`Root`,overridesResolver:Lg})(jl(({theme:e})=>({display:`flex`,justifyContent:`flex-start`,alignItems:`center`,position:`relative`,textDecoration:`none`,width:`100%`,boxSizing:`border-box`,textAlign:`left`,variants:[{props:({ownerState:e})=>!e.disablePadding,style:{paddingTop:8,paddingBottom:8}},{props:({ownerState:e})=>!e.disablePadding&&e.dense,style:{paddingTop:4,paddingBottom:4}},{props:({ownerState:e})=>!e.disablePadding&&!e.disableGutters,style:{paddingLeft:16,paddingRight:16}},{props:({ownerState:e})=>!e.disablePadding&&!!e.secondaryAction,style:{paddingRight:48}},{props:({ownerState:e})=>!!e.secondaryAction,style:{[`& > .${Dg.root}`]:{paddingRight:48}}},{props:{alignItems:`flex-start`},style:{alignItems:`flex-start`}},{props:({ownerState:e})=>e.divider,style:{borderBottom:`1px solid ${(e.vars||e).palette.divider}`,backgroundClip:`padding-box`}},{props:({ownerState:e})=>e.button,style:{transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),"&:hover":{textDecoration:`none`,backgroundColor:(e.vars||e).palette.action.hover,"@media (hover: none)":{backgroundColor:`transparent`}}}},{props:({ownerState:e})=>e.hasSecondaryAction,style:{paddingRight:48}}]})));var Bg=V(`li`,{name:`MuiListItem`,slot:`Container`})({position:`relative`}),K=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiListItem`}),{alignItems:r=`center`,children:i,className:a,component:o,components:s={},componentsProps:c={},ContainerComponent:l=`li`,ContainerProps:{className:u,...d}={},dense:f=!1,disableGutters:p=!1,disablePadding:m=!1,divider:h=!1,secondaryAction:g,slotProps:_={},slots:v={},...y}=n,b=x.useContext(xg),S=x.useMemo(()=>({dense:f||b.dense||!1,alignItems:r,disableGutters:p}),[r,b.dense,f,p]),C=x.useRef(null),w=x.Children.toArray(i),T=w.length&&Bl(w[w.length-1],[`ListItemSecondaryAction`]),E={...n,alignItems:r,dense:S.dense,disableGutters:p,disablePadding:m,divider:h,hasSecondaryAction:T},D=Rg(E),O=Ql(C,t),k=v.root||s.Root||zg,ee=_.root||c.root||{},A={className:z(D.root,ee.className,a),...y},j=o||`li`;return T?(j=!A.component&&!o?`div`:j,l===`li`&&(j===`li`?j=`div`:A.component===`li`&&(A.component=`div`)),(0,L.jsx)(xg.Provider,{value:S,children:(0,L.jsxs)(Bg,{as:l,className:z(D.container,u),ref:O,ownerState:E,...d,children:[(0,L.jsx)(k,{...ee,...!Nu(k)&&{as:j,ownerState:{...E,...ee.ownerState}},...A,children:w}),w.pop()]})})):(0,L.jsx)(xg.Provider,{value:S,children:(0,L.jsxs)(k,{...ee,as:j,ref:O,...!Nu(k)&&{ownerState:{...E,...ee.ownerState}},...A,children:[w,g&&(0,L.jsx)(Ig,{children:g})]})})});function Vg(e){return Ro(`MuiListItemText`,e)}var Hg=zo(`MuiListItemText`,[`root`,`multiline`,`dense`,`inset`,`primary`,`secondary`]),Ug=e=>{let{classes:t,inset:n,primary:r,secondary:i,dense:a}=e;return uc({root:[`root`,n&&`inset`,a&&`dense`,r&&i&&`multiline`],primary:[`primary`],secondary:[`secondary`]},Vg,t)},Wg=V(`div`,{name:`MuiListItemText`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[{[`& .${Hg.primary}`]:t.primary},{[`& .${Hg.secondary}`]:t.secondary},t.root,n.inset&&t.inset,n.primary&&n.secondary&&t.multiline,n.dense&&t.dense]}})({flex:`1 1 auto`,minWidth:0,marginTop:4,marginBottom:4,[`.${Gd.root}:where(& .${Hg.primary})`]:{display:`block`},[`.${Gd.root}:where(& .${Hg.secondary})`]:{display:`block`},variants:[{props:({ownerState:e})=>e.primary&&e.secondary,style:{marginTop:6,marginBottom:6}},{props:({ownerState:e})=>e.inset,style:{paddingLeft:56}}]}),q=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiListItemText`}),{children:r,className:i,disableTypography:a=!1,inset:o=!1,primary:s,primaryTypographyProps:c,secondary:l,secondaryTypographyProps:u,slots:d={},slotProps:f={},...p}=n,{dense:m}=x.useContext(xg),h=s??r,g=l,_={...n,disableTypography:a,inset:o,primary:!!h,secondary:!!g,dense:m},v=Ug(_),y={slots:d,slotProps:{primary:c,secondary:u,...f}},[b,S]=Wu(`root`,{className:z(v.root,i),elementType:Wg,externalForwardedProps:{...y,...p},ownerState:_,ref:t}),[C,w]=Wu(`primary`,{className:v.primary,elementType:U,externalForwardedProps:y,ownerState:_}),[T,E]=Wu(`secondary`,{className:v.secondary,elementType:U,externalForwardedProps:y,ownerState:_});return h!=null&&h.type!==U&&!a&&(h=(0,L.jsx)(C,{variant:m?`body2`:`body1`,component:w?.variant?void 0:`span`,...w,children:h})),g!=null&&g.type!==U&&!a&&(g=(0,L.jsx)(T,{variant:`body2`,color:`textSecondary`,...E,children:g})),(0,L.jsxs)(b,{...S,children:[h,g]})});function Gg(e){return Ro(`MuiTooltip`,e)}var Kg=zo(`MuiTooltip`,[`popper`,`popperInteractive`,`popperArrow`,`popperClose`,`tooltip`,`tooltipArrow`,`touch`,`tooltipPlacementLeft`,`tooltipPlacementRight`,`tooltipPlacementTop`,`tooltipPlacementBottom`,`arrow`]);function qg(e){return Math.round(e*1e5)/1e5}var Jg=e=>{let{classes:t,disableInteractive:n,arrow:r,touch:i,placement:a}=e;return uc({popper:[`popper`,!n&&`popperInteractive`,r&&`popperArrow`],tooltip:[`tooltip`,r&&`tooltipArrow`,i&&`touch`,`tooltipPlacement${H(a.split(`-`)[0])}`],arrow:[`arrow`]},Gg,t)},Yg=V(dm,{name:`MuiTooltip`,slot:`Popper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.popper,!n.disableInteractive&&t.popperInteractive,n.arrow&&t.popperArrow,!n.open&&t.popperClose]}})(jl(({theme:e})=>({zIndex:(e.vars||e).zIndex.tooltip,pointerEvents:`none`,variants:[{props:({ownerState:e})=>!e.disableInteractive,style:{pointerEvents:`auto`}},{props:({open:e})=>!e,style:{pointerEvents:`none`}},{props:({ownerState:e})=>e.arrow,style:{[`&[data-popper-placement*="bottom"] .${Kg.arrow}`]:{top:0,marginTop:`-0.71em`,"&::before":{transformOrigin:`0 100%`}},[`&[data-popper-placement*="top"] .${Kg.arrow}`]:{bottom:0,marginBottom:`-0.71em`,"&::before":{transformOrigin:`100% 0`}},[`&[data-popper-placement*="right"] .${Kg.arrow}`]:{height:`1em`,width:`0.71em`,"&::before":{transformOrigin:`100% 100%`}},[`&[data-popper-placement*="left"] .${Kg.arrow}`]:{height:`1em`,width:`0.71em`,"&::before":{transformOrigin:`0 0`}}}},{props:({ownerState:e})=>e.arrow&&!e.isRtl,style:{[`&[data-popper-placement*="right"] .${Kg.arrow}`]:{left:0,marginLeft:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!!e.isRtl,style:{[`&[data-popper-placement*="right"] .${Kg.arrow}`]:{right:0,marginRight:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!e.isRtl,style:{[`&[data-popper-placement*="left"] .${Kg.arrow}`]:{right:0,marginRight:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!!e.isRtl,style:{[`&[data-popper-placement*="left"] .${Kg.arrow}`]:{left:0,marginLeft:`-0.71em`}}}]}))),Xg=V(`div`,{name:`MuiTooltip`,slot:`Tooltip`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.tooltip,n.touch&&t.touch,n.arrow&&t.tooltipArrow,t[`tooltipPlacement${H(n.placement.split(`-`)[0])}`]]}})(jl(({theme:e})=>({backgroundColor:e.vars?e.vars.palette.Tooltip.bg:e.alpha(e.palette.grey[700],.92),borderRadius:(e.vars||e).shape.borderRadius,color:(e.vars||e).palette.common.white,fontFamily:e.typography.fontFamily,padding:`4px 8px`,fontSize:e.typography.pxToRem(11),maxWidth:300,margin:2,wordWrap:`break-word`,fontWeight:e.typography.fontWeightMedium,[`.${Kg.popper}[data-popper-placement*="left"] &`]:{transformOrigin:`right center`},[`.${Kg.popper}[data-popper-placement*="right"] &`]:{transformOrigin:`left center`},[`.${Kg.popper}[data-popper-placement*="top"] &`]:{transformOrigin:`center bottom`,marginBottom:`14px`},[`.${Kg.popper}[data-popper-placement*="bottom"] &`]:{transformOrigin:`center top`,marginTop:`14px`},variants:[{props:({ownerState:e})=>e.arrow,style:{position:`relative`,margin:0}},{props:({ownerState:e})=>e.touch,style:{padding:`8px 16px`,fontSize:e.typography.pxToRem(14),lineHeight:`${qg(16/14)}em`,fontWeight:e.typography.fontWeightRegular}},{props:({ownerState:e})=>!e.isRtl,style:{[`.${Kg.popper}[data-popper-placement*="left"] &`]:{marginRight:`14px`},[`.${Kg.popper}[data-popper-placement*="right"] &`]:{marginLeft:`14px`}}},{props:({ownerState:e})=>!e.isRtl&&e.touch,style:{[`.${Kg.popper}[data-popper-placement*="left"] &`]:{marginRight:`24px`},[`.${Kg.popper}[data-popper-placement*="right"] &`]:{marginLeft:`24px`}}},{props:({ownerState:e})=>!!e.isRtl,style:{[`.${Kg.popper}[data-popper-placement*="left"] &`]:{marginLeft:`14px`},[`.${Kg.popper}[data-popper-placement*="right"] &`]:{marginRight:`14px`}}},{props:({ownerState:e})=>!!e.isRtl&&e.touch,style:{[`.${Kg.popper}[data-popper-placement*="left"] &`]:{marginLeft:`24px`},[`.${Kg.popper}[data-popper-placement*="right"] &`]:{marginRight:`24px`}}},{props:({ownerState:e})=>e.touch,style:{[`.${Kg.popper}[data-popper-placement*="top"] &`]:{marginBottom:`24px`}}},{props:({ownerState:e})=>e.touch,style:{[`.${Kg.popper}[data-popper-placement*="bottom"] &`]:{marginTop:`24px`}}}]}))),Zg=V(`span`,{name:`MuiTooltip`,slot:`Arrow`})(jl(({theme:e})=>({overflow:`hidden`,position:`absolute`,width:`1em`,height:`0.71em`,boxSizing:`border-box`,color:e.vars?e.vars.palette.Tooltip.bg:e.alpha(e.palette.grey[700],.9),"&::before":{content:`""`,margin:`auto`,display:`block`,width:`100%`,height:`100%`,backgroundColor:`currentColor`,transform:`rotate(45deg)`}}))),Qg=!1,$g=new Ou,e_={x:0,y:0};function t_(e,t){return(n,...r)=>{t&&t(n,...r),e(n,...r)}}var n_=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiTooltip`}),{arrow:r=!1,children:i,classes:a,components:o={},componentsProps:s={},describeChild:c=!1,disableFocusListener:l=!1,disableHoverListener:u=!1,disableInteractive:d=!1,disableTouchListener:f=!1,enterDelay:p=100,enterNextDelay:m=0,enterTouchDelay:h=700,followCursor:g=!1,id:_,leaveDelay:v=0,leaveTouchDelay:y=1500,onClose:b,onOpen:S,open:C,placement:w=`bottom`,PopperComponent:T,PopperProps:E={},slotProps:D={},slots:O={},title:k,TransitionComponent:ee,TransitionProps:A,...j}=n,M=x.isValidElement(i)?i:(0,L.jsx)(`span`,{children:i}),te=hl(),N=Ds(),[P,F]=x.useState(),[ne,re]=x.useState(null),ie=x.useRef(!1),ae=d||g,oe=ku(),I=ku(),se=ku(),ce=ku(),[le,ue]=ql({controlled:C,default:!1,name:`Tooltip`,state:`open`}),de=le,fe=Gl(_),pe=x.useRef(),me=Xl(()=>{pe.current!==void 0&&(document.body.style.WebkitUserSelect=pe.current,pe.current=void 0),ce.clear()});x.useEffect(()=>me,[me]);let he=e=>{$g.clear(),Qg=!0,ue(!0),S&&!de&&S(e)},ge=Xl(e=>{$g.start(800+v,()=>{Qg=!1}),ue(!1),b&&de&&b(e),oe.start(te.transitions.duration.shortest,()=>{ie.current=!1})}),_e=e=>{ie.current&&e.type!==`touchstart`||(P&&P.removeAttribute(`title`),I.clear(),se.clear(),p||Qg&&m?I.start(Qg?m:p,()=>{he(e)}):he(e))},ve=e=>{I.clear(),se.start(v,()=>{ge(e)})},[,ye]=x.useState(!1),be=e=>{nd(e.target)||(ye(!1),ve(e))},xe=e=>{P||F(e.currentTarget),nd(e.target)&&(ye(!0),_e(e))},Se=e=>{ie.current=!0;let t=M.props;t.onTouchStart&&t.onTouchStart(e)},Ce=e=>{Se(e),se.clear(),oe.clear(),me(),pe.current=document.body.style.WebkitUserSelect,document.body.style.WebkitUserSelect=`none`,ce.start(h,()=>{document.body.style.WebkitUserSelect=pe.current,_e(e)})},we=e=>{M.props.onTouchEnd&&M.props.onTouchEnd(e),me(),se.start(y,()=>{ge(e)})};x.useEffect(()=>{if(!de)return;function e(e){e.key===`Escape`&&ge(e)}return document.addEventListener(`keydown`,e),()=>{document.removeEventListener(`keydown`,e)}},[ge,de]);let Te=Ql($p(M),F,t);!k&&k!==0&&(de=!1);let Ee=x.useRef(),De=e=>{let t=M.props;t.onMouseMove&&t.onMouseMove(e),e_={x:e.clientX,y:e.clientY},Ee.current&&Ee.current.update()},Oe={},ke=typeof k==`string`;c?(Oe.title=!de&&ke&&!u?k:null,Oe[`aria-describedby`]=de?fe:null):(Oe[`aria-label`]=ke?k:null,Oe[`aria-labelledby`]=de&&!ke?fe:null);let Ae={...Oe,...j,...M.props,className:z(j.className,M.props.className),onTouchStart:Se,ref:Te,...g?{onMouseMove:De}:{}},je={};f||(Ae.onTouchStart=Ce,Ae.onTouchEnd=we),u||(Ae.onMouseOver=t_(_e,Ae.onMouseOver),Ae.onMouseLeave=t_(ve,Ae.onMouseLeave),ae||(je.onMouseOver=_e,je.onMouseLeave=ve)),l||(Ae.onFocus=t_(xe,Ae.onFocus),Ae.onBlur=t_(be,Ae.onBlur),ae||(je.onFocus=xe,je.onBlur=be));let Me={...n,isRtl:N,arrow:r,disableInteractive:ae,placement:w,PopperComponentProp:T,touch:ie.current},Ne=typeof D.popper==`function`?D.popper(Me):D.popper,Pe=x.useMemo(()=>{let e=[{name:`arrow`,enabled:!!ne,options:{element:ne,padding:4}}];return E.popperOptions?.modifiers&&(e=e.concat(E.popperOptions.modifiers)),Ne?.popperOptions?.modifiers&&(e=e.concat(Ne.popperOptions.modifiers)),{...E.popperOptions,...Ne?.popperOptions,modifiers:e}},[ne,E.popperOptions,Ne?.popperOptions]),Fe=Jg(Me),Ie=typeof D.transition==`function`?D.transition(Me):D.transition,Le={slots:{popper:o.Popper,transition:o.Transition??ee,tooltip:o.Tooltip,arrow:o.Arrow,...O},slotProps:{arrow:D.arrow??s.arrow,popper:{...E,...Ne??s.popper},tooltip:D.tooltip??s.tooltip,transition:{...A,...Ie??s.transition}}},[Re,ze]=Wu(`popper`,{elementType:Yg,externalForwardedProps:Le,ownerState:Me,className:z(Fe.popper,E?.className)}),[Be,Ve]=Wu(`transition`,{elementType:pg,externalForwardedProps:Le,ownerState:Me}),[He,Ue]=Wu(`tooltip`,{elementType:Xg,className:Fe.tooltip,externalForwardedProps:Le,ownerState:Me}),[We,Ge]=Wu(`arrow`,{elementType:Zg,className:Fe.arrow,externalForwardedProps:Le,ownerState:Me,ref:re});return(0,L.jsxs)(x.Fragment,{children:[x.cloneElement(M,Ae),(0,L.jsx)(Re,{as:T??dm,placement:w,anchorEl:g?{getBoundingClientRect:()=>({top:e_.y,left:e_.x,right:e_.x,bottom:e_.y,width:0,height:0})}:P,popperRef:Ee,open:P?de:!1,id:fe,transition:!0,...je,...ze,popperOptions:Pe,children:({TransitionProps:e})=>(0,L.jsx)(Be,{timeout:te.transitions.duration.shorter,...e,...Ve,children:(0,L.jsxs)(He,{...Ue,children:[k,r?(0,L.jsx)(We,{...Ge}):null]})})})]})}),r_=x.createContext();function i_(e){return Ro(`MuiTable`,e)}zo(`MuiTable`,[`root`,`stickyHeader`]);var a_=e=>{let{classes:t,stickyHeader:n}=e;return uc({root:[`root`,n&&`stickyHeader`]},i_,t)},o_=V(`table`,{name:`MuiTable`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.stickyHeader&&t.stickyHeader]}})(jl(({theme:e})=>({display:`table`,width:`100%`,borderCollapse:`collapse`,borderSpacing:0,"& caption":{...e.typography.body2,padding:e.spacing(2),color:(e.vars||e).palette.text.secondary,textAlign:`left`,captionSide:`bottom`},variants:[{props:({ownerState:e})=>e.stickyHeader,style:{borderCollapse:`separate`}}]}))),s_=`table`,c_=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiTable`}),{className:r,component:i=s_,padding:a=`normal`,size:o=`medium`,stickyHeader:s=!1,...c}=n,l={...n,component:i,padding:a,size:o,stickyHeader:s},u=a_(l),d=x.useMemo(()=>({padding:a,size:o,stickyHeader:s}),[a,o,s]);return(0,L.jsx)(r_.Provider,{value:d,children:(0,L.jsx)(o_,{as:i,role:i===s_?null:`table`,ref:t,className:z(u.root,r),ownerState:l,...c})})}),l_=x.createContext();function u_(e){return Ro(`MuiTableBody`,e)}zo(`MuiTableBody`,[`root`]);var d_=e=>{let{classes:t}=e;return uc({root:[`root`]},u_,t)},f_=V(`tbody`,{name:`MuiTableBody`,slot:`Root`})({display:`table-row-group`}),p_={variant:`body`},m_=`tbody`,h_=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiTableBody`}),{className:r,component:i=m_,...a}=n,o={...n,component:i},s=d_(o);return(0,L.jsx)(l_.Provider,{value:p_,children:(0,L.jsx)(f_,{className:z(s.root,r),as:i,ref:t,role:i===m_?null:`rowgroup`,ownerState:o,...a})})});function g_(e){return Ro(`MuiTableCell`,e)}var __=zo(`MuiTableCell`,[`root`,`head`,`body`,`footer`,`sizeSmall`,`sizeMedium`,`paddingCheckbox`,`paddingNone`,`alignLeft`,`alignCenter`,`alignRight`,`alignJustify`,`stickyHeader`]),v_=e=>{let{classes:t,variant:n,align:r,padding:i,size:a,stickyHeader:o}=e;return uc({root:[`root`,n,o&&`stickyHeader`,r!==`inherit`&&`align${H(r)}`,i!==`normal`&&`padding${H(i)}`,`size${H(a)}`]},g_,t)},y_=V(`td`,{name:`MuiTableCell`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`size${H(n.size)}`],n.padding!==`normal`&&t[`padding${H(n.padding)}`],n.align!==`inherit`&&t[`align${H(n.align)}`],n.stickyHeader&&t.stickyHeader]}})(jl(({theme:e})=>({...e.typography.body2,display:`table-cell`,verticalAlign:`inherit`,borderBottom:e.vars?`1px solid ${e.vars.palette.TableCell.border}`:`1px solid
    ${e.palette.mode===`light`?e.lighten(e.alpha(e.palette.divider,1),.88):e.darken(e.alpha(e.palette.divider,1),.68)}`,textAlign:`left`,padding:16,variants:[{props:{variant:`head`},style:{color:(e.vars||e).palette.text.primary,lineHeight:e.typography.pxToRem(24),fontWeight:e.typography.fontWeightMedium}},{props:{variant:`body`},style:{color:(e.vars||e).palette.text.primary}},{props:{variant:`footer`},style:{color:(e.vars||e).palette.text.secondary,lineHeight:e.typography.pxToRem(21),fontSize:e.typography.pxToRem(12)}},{props:{size:`small`},style:{padding:`6px 16px`,[`&.${__.paddingCheckbox}`]:{width:24,padding:`0 12px 0 16px`,"& > *":{padding:0}}}},{props:{padding:`checkbox`},style:{width:48,padding:`0 0 0 4px`}},{props:{padding:`none`},style:{padding:0}},{props:{align:`left`},style:{textAlign:`left`}},{props:{align:`center`},style:{textAlign:`center`}},{props:{align:`right`},style:{textAlign:`right`,flexDirection:`row-reverse`}},{props:{align:`justify`},style:{textAlign:`justify`}},{props:({ownerState:e})=>e.stickyHeader,style:{position:`sticky`,top:0,zIndex:2,backgroundColor:(e.vars||e).palette.background.default}}]}))),J=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiTableCell`}),{align:r=`inherit`,className:i,component:a,padding:o,scope:s,size:c,sortDirection:l,variant:u,...d}=n,f=x.useContext(r_),p=x.useContext(l_),m=p&&p.variant===`head`,h;h=a||(m?`th`:`td`);let g=s;h===`td`?g=void 0:!g&&m&&(g=`col`);let _=u||p&&p.variant,v={...n,align:r,component:h,padding:o||(f&&f.padding?f.padding:`normal`),size:c||(f&&f.size?f.size:`medium`),sortDirection:l,stickyHeader:_===`head`&&f&&f.stickyHeader,variant:_},y=v_(v),b=null;return l&&(b=l===`asc`?`ascending`:`descending`),(0,L.jsx)(y_,{as:h,ref:t,className:z(y.root,i),"aria-sort":b,scope:g,ownerState:v,...d})});function b_(e){return Ro(`MuiTableContainer`,e)}zo(`MuiTableContainer`,[`root`]);var x_=e=>{let{classes:t}=e;return uc({root:[`root`]},b_,t)},S_=V(`div`,{name:`MuiTableContainer`,slot:`Root`})({width:`100%`,overflowX:`auto`}),C_=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiTableContainer`}),{className:r,component:i=`div`,...a}=n,o={...n,component:i};return(0,L.jsx)(S_,{ref:t,as:i,className:z(x_(o).root,r),ownerState:o,...a})});function w_(e){return Ro(`MuiTableHead`,e)}zo(`MuiTableHead`,[`root`]);var T_=e=>{let{classes:t}=e;return uc({root:[`root`]},w_,t)},E_=V(`thead`,{name:`MuiTableHead`,slot:`Root`})({display:`table-header-group`}),D_={variant:`head`},O_=`thead`,k_=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiTableHead`}),{className:r,component:i=O_,...a}=n,o={...n,component:i},s=T_(o);return(0,L.jsx)(l_.Provider,{value:D_,children:(0,L.jsx)(E_,{as:i,className:z(s.root,r),ref:t,role:i===O_?null:`rowgroup`,ownerState:o,...a})})});function A_(e){return Ro(`MuiToolbar`,e)}zo(`MuiToolbar`,[`root`,`gutters`,`regular`,`dense`]);var j_=e=>{let{classes:t,disableGutters:n,variant:r}=e;return uc({root:[`root`,!n&&`gutters`,r]},A_,t)},M_=V(`div`,{name:`MuiToolbar`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.disableGutters&&t.gutters,t[n.variant]]}})(jl(({theme:e})=>({position:`relative`,display:`flex`,alignItems:`center`,variants:[{props:({ownerState:e})=>!e.disableGutters,style:{paddingLeft:e.spacing(2),paddingRight:e.spacing(2),[e.breakpoints.up(`sm`)]:{paddingLeft:e.spacing(3),paddingRight:e.spacing(3)}}},{props:{variant:`dense`},style:{minHeight:48}},{props:{variant:`regular`},style:e.mixins.toolbar}]}))),N_=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiToolbar`}),{className:r,component:i=`div`,disableGutters:a=!1,variant:o=`regular`,...s}=n,c={...n,component:i,disableGutters:a,variant:o};return(0,L.jsx)(M_,{as:i,className:z(j_(c).root,r),ref:t,ownerState:c,...s})});function P_(e){return Ro(`MuiTableRow`,e)}var F_=zo(`MuiTableRow`,[`root`,`selected`,`hover`,`head`,`footer`]),I_=e=>{let{classes:t,selected:n,hover:r,head:i,footer:a}=e;return uc({root:[`root`,n&&`selected`,r&&`hover`,i&&`head`,a&&`footer`]},P_,t)},L_=V(`tr`,{name:`MuiTableRow`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.head&&t.head,n.footer&&t.footer]}})(jl(({theme:e})=>({color:`inherit`,display:`table-row`,verticalAlign:`middle`,outline:0,[`&.${F_.hover}:hover`]:{backgroundColor:(e.vars||e).palette.action.hover},[`&.${F_.selected}`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,(e.vars||e).palette.action.selectedOpacity),"&:hover":{backgroundColor:e.alpha((e.vars||e).palette.primary.main,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.hoverOpacity}`)}}}))),R_=`tr`,Y=x.forwardRef(function(e,t){let n=Ml({props:e,name:`MuiTableRow`}),{className:r,component:i=R_,hover:a=!1,selected:o=!1,...s}=n,c=x.useContext(l_),l={...n,component:i,hover:a,selected:o,head:c&&c.variant===`head`,footer:c&&c.variant===`footer`};return(0,L.jsx)(L_,{as:i,ref:t,className:z(I_(l).root,r),role:i===R_?null:`row`,ownerState:l,...s})}),z_={root:{}};const B_=()=>(0,L.jsxs)(W,{sx:z_.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ReactiveUIToolKit`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit brings a React-like component model to Unity UI Toolkit using a virtual node tree, typed props, and reconciliation logic that runs in C#. You build your UI from`,` `,(0,L.jsx)(`code`,{children:`V.*`}),` helpers and function components, and the reconciler updates the underlying`,(0,L.jsx)(`code`,{children:`VisualElement`}),` hierarchy for you.`]}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`The toolkit is designed to work both in the Unity Editor and at runtime, and to feel familiar if you have used React, while still fitting naturally into Unity's component model and UI Toolkit controls.`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[(0,L.jsx)(`strong`,{children:`P.S.`}),` ReactiveUIToolKit runs entirely in C# on top of Unity UI Toolkit. There is no JavaScript engine or bridge layer involved.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Highlights`}),(0,L.jsxs)(G,{children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`VirtualNode diffing and batched updates for UI Toolkit trees`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Typed props and adapters for most built-in UI Toolkit controls`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Router and Signals utilities for navigation and shared state`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Editor-only elements are UNITY_EDITOR guarded`})})]})]});var V_=Object.create,H_=Object.defineProperty,U_=Object.defineProperties,W_=Object.getOwnPropertyDescriptor,G_=Object.getOwnPropertyDescriptors,K_=Object.getOwnPropertyNames,q_=Object.getOwnPropertySymbols,J_=Object.getPrototypeOf,Y_=Object.prototype.hasOwnProperty,X_=Object.prototype.propertyIsEnumerable,Z_=(e,t,n)=>t in e?H_(e,t,{enumerable:!0,configurable:!0,writable:!0,value:n}):e[t]=n,Q_=(e,t)=>{for(var n in t||={})Y_.call(t,n)&&Z_(e,n,t[n]);if(q_)for(var n of q_(t))X_.call(t,n)&&Z_(e,n,t[n]);return e},$_=(e,t)=>U_(e,G_(t)),ev=(e,t)=>{var n={};for(var r in e)Y_.call(e,r)&&t.indexOf(r)<0&&(n[r]=e[r]);if(e!=null&&q_)for(var r of q_(e))t.indexOf(r)<0&&X_.call(e,r)&&(n[r]=e[r]);return n},tv=(e,t)=>function(){return t||(0,e[K_(e)[0]])((t={exports:{}}).exports,t),t.exports},nv=(e,t)=>{for(var n in t)H_(e,n,{get:t[n],enumerable:!0})},rv=(e,t,n,r)=>{if(t&&typeof t==`object`||typeof t==`function`)for(let i of K_(t))!Y_.call(e,i)&&i!==n&&H_(e,i,{get:()=>t[i],enumerable:!(r=W_(t,i))||r.enumerable});return e},X=((e,t,n)=>(n=e==null?{}:V_(J_(e)),rv(t||!e||!e.__esModule?H_(n,`default`,{value:e,enumerable:!0}):n,e)))(tv({"../../node_modules/.pnpm/prismjs@1.29.0_patch_hash=vrxx3pzkik6jpmgpayxfjunetu/node_modules/prismjs/prism.js"(e,t){var n=function(){var e=/(?:^|\s)lang(?:uage)?-([\w-]+)(?=\s|$)/i,t=0,n={},r={util:{encode:function e(t){return t instanceof i?new i(t.type,e(t.content),t.alias):Array.isArray(t)?t.map(e):t.replace(/&/g,`&amp;`).replace(/</g,`&lt;`).replace(/\u00a0/g,` `)},type:function(e){return Object.prototype.toString.call(e).slice(8,-1)},objId:function(e){return e.__id||Object.defineProperty(e,`__id`,{value:++t}),e.__id},clone:function e(t,n){n||={};var i,a;switch(r.util.type(t)){case`Object`:if(a=r.util.objId(t),n[a])return n[a];for(var o in i={},n[a]=i,t)t.hasOwnProperty(o)&&(i[o]=e(t[o],n));return i;case`Array`:return a=r.util.objId(t),n[a]?n[a]:(i=[],n[a]=i,t.forEach(function(t,r){i[r]=e(t,n)}),i);default:return t}},getLanguage:function(t){for(;t;){var n=e.exec(t.className);if(n)return n[1].toLowerCase();t=t.parentElement}return`none`},setLanguage:function(t,n){t.className=t.className.replace(RegExp(e,`gi`),``),t.classList.add(`language-`+n)},isActive:function(e,t,n){for(var r=`no-`+t;e;){var i=e.classList;if(i.contains(t))return!0;if(i.contains(r))return!1;e=e.parentElement}return!!n}},languages:{plain:n,plaintext:n,text:n,txt:n,extend:function(e,t){var n=r.util.clone(r.languages[e]);for(var i in t)n[i]=t[i];return n},insertBefore:function(e,t,n,i){i||=r.languages;var a=i[e],o={};for(var s in a)if(a.hasOwnProperty(s)){if(s==t)for(var c in n)n.hasOwnProperty(c)&&(o[c]=n[c]);n.hasOwnProperty(s)||(o[s]=a[s])}var l=i[e];return i[e]=o,r.languages.DFS(r.languages,function(t,n){n===l&&t!=e&&(this[t]=o)}),o},DFS:function e(t,n,i,a){a||={};var o=r.util.objId;for(var s in t)if(t.hasOwnProperty(s)){n.call(t,s,t[s],i||s);var c=t[s],l=r.util.type(c);l===`Object`&&!a[o(c)]?(a[o(c)]=!0,e(c,n,null,a)):l===`Array`&&!a[o(c)]&&(a[o(c)]=!0,e(c,n,s,a))}}},plugins:{},highlight:function(e,t,n){var a={code:e,grammar:t,language:n};if(r.hooks.run(`before-tokenize`,a),!a.grammar)throw Error(`The language "`+a.language+`" has no grammar.`);return a.tokens=r.tokenize(a.code,a.grammar),r.hooks.run(`after-tokenize`,a),i.stringify(r.util.encode(a.tokens),a.language)},tokenize:function(e,t){var n=t.rest;if(n){for(var r in n)t[r]=n[r];delete t.rest}var i=new s;return c(i,i.head,e),o(e,i,t,i.head,0),u(i)},hooks:{all:{},add:function(e,t){var n=r.hooks.all;n[e]=n[e]||[],n[e].push(t)},run:function(e,t){var n=r.hooks.all[e];if(!(!n||!n.length))for(var i=0,a;a=n[i++];)a(t)}},Token:i};function i(e,t,n,r){this.type=e,this.content=t,this.alias=n,this.length=(r||``).length|0}i.stringify=function e(t,n){if(typeof t==`string`)return t;if(Array.isArray(t)){var i=``;return t.forEach(function(t){i+=e(t,n)}),i}var a={type:t.type,content:e(t.content,n),tag:`span`,classes:[`token`,t.type],attributes:{},language:n},o=t.alias;o&&(Array.isArray(o)?Array.prototype.push.apply(a.classes,o):a.classes.push(o)),r.hooks.run(`wrap`,a);var s=``;for(var c in a.attributes)s+=` `+c+`="`+(a.attributes[c]||``).replace(/"/g,`&quot;`)+`"`;return`<`+a.tag+` class="`+a.classes.join(` `)+`"`+s+`>`+a.content+`</`+a.tag+`>`};function a(e,t,n,r){e.lastIndex=t;var i=e.exec(n);if(i&&r&&i[1]){var a=i[1].length;i.index+=a,i[0]=i[0].slice(a)}return i}function o(e,t,n,s,u,d){for(var f in n)if(!(!n.hasOwnProperty(f)||!n[f])){var p=n[f];p=Array.isArray(p)?p:[p];for(var m=0;m<p.length;++m){if(d&&d.cause==f+`,`+m)return;var h=p[m],g=h.inside,_=!!h.lookbehind,v=!!h.greedy,y=h.alias;if(v&&!h.pattern.global){var b=h.pattern.toString().match(/[imsuy]*$/)[0];h.pattern=RegExp(h.pattern.source,b+`g`)}for(var x=h.pattern||h,S=s.next,C=u;S!==t.tail&&!(d&&C>=d.reach);C+=S.value.length,S=S.next){var w=S.value;if(t.length>e.length)return;if(!(w instanceof i)){var T=1,E;if(v){if(E=a(x,C,e,_),!E||E.index>=e.length)break;var D=E.index,O=E.index+E[0].length,k=C;for(k+=S.value.length;D>=k;)S=S.next,k+=S.value.length;if(k-=S.value.length,C=k,S.value instanceof i)continue;for(var ee=S;ee!==t.tail&&(k<O||typeof ee.value==`string`);ee=ee.next)T++,k+=ee.value.length;T--,w=e.slice(C,k),E.index-=C}else if(E=a(x,0,w,_),!E)continue;var D=E.index,A=E[0],j=w.slice(0,D),M=w.slice(D+A.length),te=C+w.length;d&&te>d.reach&&(d.reach=te);var N=S.prev;j&&(N=c(t,N,j),C+=j.length),l(t,N,T);var P=new i(f,g?r.tokenize(A,g):A,y,A);if(S=c(t,N,P),M&&c(t,S,M),T>1){var F={cause:f+`,`+m,reach:te};o(e,t,n,S.prev,C,F),d&&F.reach>d.reach&&(d.reach=F.reach)}}}}}}function s(){var e={value:null,prev:null,next:null},t={value:null,prev:e,next:null};e.next=t,this.head=e,this.tail=t,this.length=0}function c(e,t,n){var r=t.next,i={value:n,prev:t,next:r};return t.next=i,r.prev=i,e.length++,i}function l(e,t,n){for(var r=t.next,i=0;i<n&&r!==e.tail;i++)r=r.next;t.next=r,r.prev=t,e.length-=i}function u(e){for(var t=[],n=e.head.next;n!==e.tail;)t.push(n.value),n=n.next;return t}return r}();t.exports=n,n.default=n}})());X.languages.markup={comment:{pattern:/<!--(?:(?!<!--)[\s\S])*?-->/,greedy:!0},prolog:{pattern:/<\?[\s\S]+?\?>/,greedy:!0},doctype:{pattern:/<!DOCTYPE(?:[^>"'[\]]|"[^"]*"|'[^']*')+(?:\[(?:[^<"'\]]|"[^"]*"|'[^']*'|<(?!!--)|<!--(?:[^-]|-(?!->))*-->)*\]\s*)?>/i,greedy:!0,inside:{"internal-subset":{pattern:/(^[^\[]*\[)[\s\S]+(?=\]>$)/,lookbehind:!0,greedy:!0,inside:null},string:{pattern:/"[^"]*"|'[^']*'/,greedy:!0},punctuation:/^<!|>$|[[\]]/,"doctype-tag":/^DOCTYPE/i,name:/[^\s<>'"]+/}},cdata:{pattern:/<!\[CDATA\[[\s\S]*?\]\]>/i,greedy:!0},tag:{pattern:/<\/?(?!\d)[^\s>\/=$<%]+(?:\s(?:\s*[^\s>\/=]+(?:\s*=\s*(?:"[^"]*"|'[^']*'|[^\s'">=]+(?=[\s>]))|(?=[\s/>])))+)?\s*\/?>/,greedy:!0,inside:{tag:{pattern:/^<\/?[^\s>\/]+/,inside:{punctuation:/^<\/?/,namespace:/^[^\s>\/:]+:/}},"special-attr":[],"attr-value":{pattern:/=\s*(?:"[^"]*"|'[^']*'|[^\s'">=]+)/,inside:{punctuation:[{pattern:/^=/,alias:`attr-equals`},{pattern:/^(\s*)["']|["']$/,lookbehind:!0}]}},punctuation:/\/?>/,"attr-name":{pattern:/[^\s>\/]+/,inside:{namespace:/^[^\s>\/:]+:/}}}},entity:[{pattern:/&[\da-z]{1,8};/i,alias:`named-entity`},/&#x?[\da-f]{1,8};/i]},X.languages.markup.tag.inside[`attr-value`].inside.entity=X.languages.markup.entity,X.languages.markup.doctype.inside[`internal-subset`].inside=X.languages.markup,X.hooks.add(`wrap`,function(e){e.type===`entity`&&(e.attributes.title=e.content.replace(/&amp;/,`&`))}),Object.defineProperty(X.languages.markup.tag,`addInlined`,{value:function(e,t){var n={},n=(n[`language-`+t]={pattern:/(^<!\[CDATA\[)[\s\S]+?(?=\]\]>$)/i,lookbehind:!0,inside:X.languages[t]},n.cdata=/^<!\[CDATA\[|\]\]>$/i,{"included-cdata":{pattern:/<!\[CDATA\[[\s\S]*?\]\]>/i,inside:n}}),t=(n[`language-`+t]={pattern:/[\s\S]+/,inside:X.languages[t]},{});t[e]={pattern:RegExp(`(<__[^>]*>)(?:<!\\[CDATA\\[(?:[^\\]]|\\](?!\\]>))*\\]\\]>|(?!<!\\[CDATA\\[)[\\s\\S])*?(?=<\\/__>)`.replace(/__/g,function(){return e}),`i`),lookbehind:!0,greedy:!0,inside:n},X.languages.insertBefore(`markup`,`cdata`,t)}}),Object.defineProperty(X.languages.markup.tag,`addAttribute`,{value:function(e,t){X.languages.markup.tag.inside[`special-attr`].push({pattern:RegExp(`(^|["'\\s])(?:`+e+`)\\s*=\\s*(?:"[^"]*"|'[^']*'|[^\\s'">=]+(?=[\\s>]))`,`i`),lookbehind:!0,inside:{"attr-name":/^[^\s=]+/,"attr-value":{pattern:/=[\s\S]+/,inside:{value:{pattern:/(^=\s*(["']|(?!["'])))\S[\s\S]*(?=\2$)/,lookbehind:!0,alias:[t,`language-`+t],inside:X.languages[t]},punctuation:[{pattern:/^=/,alias:`attr-equals`},/"|'/]}}}})}}),X.languages.html=X.languages.markup,X.languages.mathml=X.languages.markup,X.languages.svg=X.languages.markup,X.languages.xml=X.languages.extend(`markup`,{}),X.languages.ssml=X.languages.xml,X.languages.atom=X.languages.xml,X.languages.rss=X.languages.xml,function(e){var t={pattern:/\\[\\(){}[\]^$+*?|.]/,alias:`escape`},n=/\\(?:x[\da-fA-F]{2}|u[\da-fA-F]{4}|u\{[\da-fA-F]+\}|0[0-7]{0,2}|[123][0-7]{2}|c[a-zA-Z]|.)/,r=`(?:[^\\\\-]|`+n.source+`)`,r=RegExp(r+`-`+r),i={pattern:/(<|')[^<>']+(?=[>']$)/,lookbehind:!0,alias:`variable`};e.languages.regex={"char-class":{pattern:/((?:^|[^\\])(?:\\\\)*)\[(?:[^\\\]]|\\[\s\S])*\]/,lookbehind:!0,inside:{"char-class-negation":{pattern:/(^\[)\^/,lookbehind:!0,alias:`operator`},"char-class-punctuation":{pattern:/^\[|\]$/,alias:`punctuation`},range:{pattern:r,inside:{escape:n,"range-punctuation":{pattern:/-/,alias:`operator`}}},"special-escape":t,"char-set":{pattern:/\\[wsd]|\\p\{[^{}]+\}/i,alias:`class-name`},escape:n}},"special-escape":t,"char-set":{pattern:/\.|\\[wsd]|\\p\{[^{}]+\}/i,alias:`class-name`},backreference:[{pattern:/\\(?![123][0-7]{2})[1-9]/,alias:`keyword`},{pattern:/\\k<[^<>']+>/,alias:`keyword`,inside:{"group-name":i}}],anchor:{pattern:/[$^]|\\[ABbGZz]/,alias:`function`},escape:n,group:[{pattern:/\((?:\?(?:<[^<>']+>|'[^<>']+'|[>:]|<?[=!]|[idmnsuxU]+(?:-[idmnsuxU]+)?:?))?/,alias:`punctuation`,inside:{"group-name":i}},{pattern:/\)/,alias:`punctuation`}],quantifier:{pattern:/(?:[+*?]|\{\d+(?:,\d*)?\})[?+]?/,alias:`number`},alternation:{pattern:/\|/,alias:`keyword`}}}(X),X.languages.clike={comment:[{pattern:/(^|[^\\])\/\*[\s\S]*?(?:\*\/|$)/,lookbehind:!0,greedy:!0},{pattern:/(^|[^\\:])\/\/.*/,lookbehind:!0,greedy:!0}],string:{pattern:/(["'])(?:\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,greedy:!0},"class-name":{pattern:/(\b(?:class|extends|implements|instanceof|interface|new|trait)\s+|\bcatch\s+\()[\w.\\]+/i,lookbehind:!0,inside:{punctuation:/[.\\]/}},keyword:/\b(?:break|catch|continue|do|else|finally|for|function|if|in|instanceof|new|null|return|throw|try|while)\b/,boolean:/\b(?:false|true)\b/,function:/\b\w+(?=\()/,number:/\b0x[\da-f]+\b|(?:\b\d+(?:\.\d*)?|\B\.\d+)(?:e[+-]?\d+)?/i,operator:/[<>]=?|[!=]=?=?|--?|\+\+?|&&?|\|\|?|[?*/~^%]/,punctuation:/[{}[\];(),.:]/},X.languages.javascript=X.languages.extend(`clike`,{"class-name":[X.languages.clike[`class-name`],{pattern:/(^|[^$\w\xA0-\uFFFF])(?!\s)[_$A-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\.(?:constructor|prototype))/,lookbehind:!0}],keyword:[{pattern:/((?:^|\})\s*)catch\b/,lookbehind:!0},{pattern:/(^|[^.]|\.\.\.\s*)\b(?:as|assert(?=\s*\{)|async(?=\s*(?:function\b|\(|[$\w\xA0-\uFFFF]|$))|await|break|case|class|const|continue|debugger|default|delete|do|else|enum|export|extends|finally(?=\s*(?:\{|$))|for|from(?=\s*(?:['"]|$))|function|(?:get|set)(?=\s*(?:[#\[$\w\xA0-\uFFFF]|$))|if|implements|import|in|instanceof|interface|let|new|null|of|package|private|protected|public|return|static|super|switch|this|throw|try|typeof|undefined|var|void|while|with|yield)\b/,lookbehind:!0}],function:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*(?:\.\s*(?:apply|bind|call)\s*)?\()/,number:{pattern:RegExp(`(^|[^\\w$])(?:NaN|Infinity|0[bB][01]+(?:_[01]+)*n?|0[oO][0-7]+(?:_[0-7]+)*n?|0[xX][\\dA-Fa-f]+(?:_[\\dA-Fa-f]+)*n?|\\d+(?:_\\d+)*n|(?:\\d+(?:_\\d+)*(?:\\.(?:\\d+(?:_\\d+)*)?)?|\\.\\d+(?:_\\d+)*)(?:[Ee][+-]?\\d+(?:_\\d+)*)?)(?![\\w$])`),lookbehind:!0},operator:/--|\+\+|\*\*=?|=>|&&=?|\|\|=?|[!=]==|<<=?|>>>?=?|[-+*/%&|^!=<>]=?|\.{3}|\?\?=?|\?\.?|[~:]/}),X.languages.javascript[`class-name`][0].pattern=/(\b(?:class|extends|implements|instanceof|interface|new)\s+)[\w.\\]+/,X.languages.insertBefore(`javascript`,`keyword`,{regex:{pattern:RegExp(`((?:^|[^$\\w\\xA0-\\uFFFF."'\\])\\s]|\\b(?:return|yield))\\s*)\\/(?:(?:\\[(?:[^\\]\\\\\\r\\n]|\\\\.)*\\]|\\\\.|[^/\\\\\\[\\r\\n])+\\/[dgimyus]{0,7}|(?:\\[(?:[^[\\]\\\\\\r\\n]|\\\\.|\\[(?:[^[\\]\\\\\\r\\n]|\\\\.|\\[(?:[^[\\]\\\\\\r\\n]|\\\\.)*\\])*\\])*\\]|\\\\.|[^/\\\\\\[\\r\\n])+\\/[dgimyus]{0,7}v[dgimyus]{0,7})(?=(?:\\s|\\/\\*(?:[^*]|\\*(?!\\/))*\\*\\/)*(?:$|[\\r\\n,.;:})\\]]|\\/\\/))`),lookbehind:!0,greedy:!0,inside:{"regex-source":{pattern:/^(\/)[\s\S]+(?=\/[a-z]*$)/,lookbehind:!0,alias:`language-regex`,inside:X.languages.regex},"regex-delimiter":/^\/|\/$/,"regex-flags":/^[a-z]+$/}},"function-variable":{pattern:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*[=:]\s*(?:async\s*)?(?:\bfunction\b|(?:\((?:[^()]|\([^()]*\))*\)|(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)\s*=>))/,alias:`function`},parameter:[{pattern:/(function(?:\s+(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)?\s*\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\))/,lookbehind:!0,inside:X.languages.javascript},{pattern:/(^|[^$\w\xA0-\uFFFF])(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*=>)/i,lookbehind:!0,inside:X.languages.javascript},{pattern:/(\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\)\s*=>)/,lookbehind:!0,inside:X.languages.javascript},{pattern:/((?:\b|\s|^)(?!(?:as|async|await|break|case|catch|class|const|continue|debugger|default|delete|do|else|enum|export|extends|finally|for|from|function|get|if|implements|import|in|instanceof|interface|let|new|null|of|package|private|protected|public|return|set|static|super|switch|this|throw|try|typeof|undefined|var|void|while|with|yield)(?![$\w\xA0-\uFFFF]))(?:(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*\s*)\(\s*|\]\s*\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\)\s*\{)/,lookbehind:!0,inside:X.languages.javascript}],constant:/\b[A-Z](?:[A-Z_]|\dx?)*\b/}),X.languages.insertBefore(`javascript`,`string`,{hashbang:{pattern:/^#!.*/,greedy:!0,alias:`comment`},"template-string":{pattern:/`(?:\\[\s\S]|\$\{(?:[^{}]|\{(?:[^{}]|\{[^}]*\})*\})+\}|(?!\$\{)[^\\`])*`/,greedy:!0,inside:{"template-punctuation":{pattern:/^`|`$/,alias:`string`},interpolation:{pattern:/((?:^|[^\\])(?:\\{2})*)\$\{(?:[^{}]|\{(?:[^{}]|\{[^}]*\})*\})+\}/,lookbehind:!0,inside:{"interpolation-punctuation":{pattern:/^\$\{|\}$/,alias:`punctuation`},rest:X.languages.javascript}},string:/[\s\S]+/}},"string-property":{pattern:/((?:^|[,{])[ \t]*)(["'])(?:\\(?:\r\n|[\s\S])|(?!\2)[^\\\r\n])*\2(?=\s*:)/m,lookbehind:!0,greedy:!0,alias:`property`}}),X.languages.insertBefore(`javascript`,`operator`,{"literal-property":{pattern:/((?:^|[,{])[ \t]*)(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*:)/m,lookbehind:!0,alias:`property`}}),X.languages.markup&&(X.languages.markup.tag.addInlined(`script`,`javascript`),X.languages.markup.tag.addAttribute(`on(?:abort|blur|change|click|composition(?:end|start|update)|dblclick|error|focus(?:in|out)?|key(?:down|up)|load|mouse(?:down|enter|leave|move|out|over|up)|reset|resize|scroll|select|slotchange|submit|unload|wheel)`,`javascript`)),X.languages.js=X.languages.javascript,X.languages.actionscript=X.languages.extend(`javascript`,{keyword:/\b(?:as|break|case|catch|class|const|default|delete|do|dynamic|each|else|extends|final|finally|for|function|get|if|implements|import|in|include|instanceof|interface|internal|is|namespace|native|new|null|override|package|private|protected|public|return|set|static|super|switch|this|throw|try|typeof|use|var|void|while|with)\b/,operator:/\+\+|--|(?:[+\-*\/%^]|&&?|\|\|?|<<?|>>?>?|[!=]=?)=?|[~?@]/}),X.languages.actionscript[`class-name`].alias=`function`,delete X.languages.actionscript.parameter,delete X.languages.actionscript[`literal-property`],X.languages.markup&&X.languages.insertBefore(`actionscript`,`string`,{xml:{pattern:/(^|[^.])<\/?\w+(?:\s+[^\s>\/=]+=("|')(?:\\[\s\S]|(?!\2)[^\\])*\2)*\s*\/?>/,lookbehind:!0,inside:X.languages.markup}}),function(e){var t=/#(?!\{).+/,n={pattern:/#\{[^}]+\}/,alias:`variable`};e.languages.coffeescript=e.languages.extend(`javascript`,{comment:t,string:[{pattern:/'(?:\\[\s\S]|[^\\'])*'/,greedy:!0},{pattern:/"(?:\\[\s\S]|[^\\"])*"/,greedy:!0,inside:{interpolation:n}}],keyword:/\b(?:and|break|by|catch|class|continue|debugger|delete|do|each|else|extend|extends|false|finally|for|if|in|instanceof|is|isnt|let|loop|namespace|new|no|not|null|of|off|on|or|own|return|super|switch|then|this|throw|true|try|typeof|undefined|unless|until|when|while|window|with|yes|yield)\b/,"class-member":{pattern:/@(?!\d)\w+/,alias:`variable`}}),e.languages.insertBefore(`coffeescript`,`comment`,{"multiline-comment":{pattern:/###[\s\S]+?###/,alias:`comment`},"block-regex":{pattern:/\/{3}[\s\S]*?\/{3}/,alias:`regex`,inside:{comment:t,interpolation:n}}}),e.languages.insertBefore(`coffeescript`,`string`,{"inline-javascript":{pattern:/`(?:\\[\s\S]|[^\\`])*`/,inside:{delimiter:{pattern:/^`|`$/,alias:`punctuation`},script:{pattern:/[\s\S]+/,alias:`language-javascript`,inside:e.languages.javascript}}},"multiline-string":[{pattern:/'''[\s\S]*?'''/,greedy:!0,alias:`string`},{pattern:/"""[\s\S]*?"""/,greedy:!0,alias:`string`,inside:{interpolation:n}}]}),e.languages.insertBefore(`coffeescript`,`keyword`,{property:/(?!\d)\w+(?=\s*:(?!:))/}),delete e.languages.coffeescript[`template-string`],e.languages.coffee=e.languages.coffeescript}(X),function(e){var t=e.languages.javadoclike={parameter:{pattern:/(^[\t ]*(?:\/{3}|\*|\/\*\*)\s*@(?:arg|arguments|param)\s+)\w+/m,lookbehind:!0},keyword:{pattern:/(^[\t ]*(?:\/{3}|\*|\/\*\*)\s*|\{)@[a-z][a-zA-Z-]+\b/m,lookbehind:!0},punctuation:/[{}]/};Object.defineProperty(t,`addSupport`,{value:function(t,n){(t=typeof t==`string`?[t]:t).forEach(function(t){var r=function(e){e.inside||={},e.inside.rest=n},i=`doc-comment`;if(a=e.languages[t]){var a,o=a[i];if((o||=(a=e.languages.insertBefore(t,`comment`,{"doc-comment":{pattern:/(^|[^\\])\/\*\*[^/][\s\S]*?(?:\*\/|$)/,lookbehind:!0,alias:`comment`}}))[i])instanceof RegExp&&(o=a[i]={pattern:o}),Array.isArray(o))for(var s=0,c=o.length;s<c;s++)o[s]instanceof RegExp&&(o[s]={pattern:o[s]}),r(o[s]);else r(o)}})}}),t.addSupport([`java`,`javascript`,`php`],t)}(X),function(e){var t=/(?:"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"|'(?:\\(?:\r\n|[\s\S])|[^'\\\r\n])*')/,t=(e.languages.css={comment:/\/\*[\s\S]*?\*\//,atrule:{pattern:RegExp(`@[\\w-](?:[^;{\\s"']|\\s+(?!\\s)|`+t.source+`)*?(?:;|(?=\\s*\\{))`),inside:{rule:/^@[\w-]+/,"selector-function-argument":{pattern:/(\bselector\s*\(\s*(?![\s)]))(?:[^()\s]|\s+(?![\s)])|\((?:[^()]|\([^()]*\))*\))+(?=\s*\))/,lookbehind:!0,alias:`selector`},keyword:{pattern:/(^|[^\w-])(?:and|not|only|or)(?![\w-])/,lookbehind:!0}}},url:{pattern:RegExp(`\\burl\\((?:`+t.source+`|(?:[^\\\\\\r\\n()"']|\\\\[\\s\\S])*)\\)`,`i`),greedy:!0,inside:{function:/^url/i,punctuation:/^\(|\)$/,string:{pattern:RegExp(`^`+t.source+`$`),alias:`url`}}},selector:{pattern:RegExp(`(^|[{}\\s])[^{}\\s](?:[^{};"'\\s]|\\s+(?![\\s{])|`+t.source+`)*(?=\\s*\\{)`),lookbehind:!0},string:{pattern:t,greedy:!0},property:{pattern:/(^|[^-\w\xA0-\uFFFF])(?!\s)[-_a-z\xA0-\uFFFF](?:(?!\s)[-\w\xA0-\uFFFF])*(?=\s*:)/i,lookbehind:!0},important:/!important\b/i,function:{pattern:/(^|[^-a-z0-9])[-a-z0-9]+(?=\()/i,lookbehind:!0},punctuation:/[(){};:,]/},e.languages.css.atrule.inside.rest=e.languages.css,e.languages.markup);t&&(t.tag.addInlined(`style`,`css`),t.tag.addAttribute(`style`,`css`))}(X),function(e){var t=/("|')(?:\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,t=(e.languages.css.selector={pattern:e.languages.css.selector.pattern,lookbehind:!0,inside:t={"pseudo-element":/:(?:after|before|first-letter|first-line|selection)|::[-\w]+/,"pseudo-class":/:[-\w]+/,class:/\.[-\w]+/,id:/#[-\w]+/,attribute:{pattern:RegExp(`\\[(?:[^[\\]"']|`+t.source+`)*\\]`),greedy:!0,inside:{punctuation:/^\[|\]$/,"case-sensitivity":{pattern:/(\s)[si]$/i,lookbehind:!0,alias:`keyword`},namespace:{pattern:/^(\s*)(?:(?!\s)[-*\w\xA0-\uFFFF])*\|(?!=)/,lookbehind:!0,inside:{punctuation:/\|$/}},"attr-name":{pattern:/^(\s*)(?:(?!\s)[-\w\xA0-\uFFFF])+/,lookbehind:!0},"attr-value":[t,{pattern:/(=\s*)(?:(?!\s)[-\w\xA0-\uFFFF])+(?=\s*$)/,lookbehind:!0}],operator:/[|~*^$]?=/}},"n-th":[{pattern:/(\(\s*)[+-]?\d*[\dn](?:\s*[+-]\s*\d+)?(?=\s*\))/,lookbehind:!0,inside:{number:/[\dn]+/,operator:/[+-]/}},{pattern:/(\(\s*)(?:even|odd)(?=\s*\))/i,lookbehind:!0}],combinator:/>|\+|~|\|\|/,punctuation:/[(),]/}},e.languages.css.atrule.inside[`selector-function-argument`].inside=t,e.languages.insertBefore(`css`,`property`,{variable:{pattern:/(^|[^-\w\xA0-\uFFFF])--(?!\s)[-_a-z\xA0-\uFFFF](?:(?!\s)[-\w\xA0-\uFFFF])*/i,lookbehind:!0}}),{pattern:/(\b\d+)(?:%|[a-z]+(?![\w-]))/,lookbehind:!0}),n={pattern:/(^|[^\w.-])-?(?:\d+(?:\.\d+)?|\.\d+)/,lookbehind:!0};e.languages.insertBefore(`css`,`function`,{operator:{pattern:/(\s)[+\-*\/](?=\s)/,lookbehind:!0},hexcode:{pattern:/\B#[\da-f]{3,8}\b/i,alias:`color`},color:[{pattern:/(^|[^\w-])(?:AliceBlue|AntiqueWhite|Aqua|Aquamarine|Azure|Beige|Bisque|Black|BlanchedAlmond|Blue|BlueViolet|Brown|BurlyWood|CadetBlue|Chartreuse|Chocolate|Coral|CornflowerBlue|Cornsilk|Crimson|Cyan|DarkBlue|DarkCyan|DarkGoldenRod|DarkGr[ae]y|DarkGreen|DarkKhaki|DarkMagenta|DarkOliveGreen|DarkOrange|DarkOrchid|DarkRed|DarkSalmon|DarkSeaGreen|DarkSlateBlue|DarkSlateGr[ae]y|DarkTurquoise|DarkViolet|DeepPink|DeepSkyBlue|DimGr[ae]y|DodgerBlue|FireBrick|FloralWhite|ForestGreen|Fuchsia|Gainsboro|GhostWhite|Gold|GoldenRod|Gr[ae]y|Green|GreenYellow|HoneyDew|HotPink|IndianRed|Indigo|Ivory|Khaki|Lavender|LavenderBlush|LawnGreen|LemonChiffon|LightBlue|LightCoral|LightCyan|LightGoldenRodYellow|LightGr[ae]y|LightGreen|LightPink|LightSalmon|LightSeaGreen|LightSkyBlue|LightSlateGr[ae]y|LightSteelBlue|LightYellow|Lime|LimeGreen|Linen|Magenta|Maroon|MediumAquaMarine|MediumBlue|MediumOrchid|MediumPurple|MediumSeaGreen|MediumSlateBlue|MediumSpringGreen|MediumTurquoise|MediumVioletRed|MidnightBlue|MintCream|MistyRose|Moccasin|NavajoWhite|Navy|OldLace|Olive|OliveDrab|Orange|OrangeRed|Orchid|PaleGoldenRod|PaleGreen|PaleTurquoise|PaleVioletRed|PapayaWhip|PeachPuff|Peru|Pink|Plum|PowderBlue|Purple|RebeccaPurple|Red|RosyBrown|RoyalBlue|SaddleBrown|Salmon|SandyBrown|SeaGreen|SeaShell|Sienna|Silver|SkyBlue|SlateBlue|SlateGr[ae]y|Snow|SpringGreen|SteelBlue|Tan|Teal|Thistle|Tomato|Transparent|Turquoise|Violet|Wheat|White|WhiteSmoke|Yellow|YellowGreen)(?![\w-])/i,lookbehind:!0},{pattern:/\b(?:hsl|rgb)\(\s*\d{1,3}\s*,\s*\d{1,3}%?\s*,\s*\d{1,3}%?\s*\)\B|\b(?:hsl|rgb)a\(\s*\d{1,3}\s*,\s*\d{1,3}%?\s*,\s*\d{1,3}%?\s*,\s*(?:0|0?\.\d+|1)\s*\)\B/i,inside:{unit:t,number:n,function:/[\w-]+(?=\()/,punctuation:/[(),]/}}],entity:/\\[\da-f]{1,8}/i,unit:t,number:n})}(X),function(e){var t=/[*&][^\s[\]{},]+/,n=/!(?:<[\w\-%#;/?:@&=+$,.!~*'()[\]]+>|(?:[a-zA-Z\d-]*!)?[\w\-%#;/?:@&=+$.~*'()]+)?/,r=`(?:`+n.source+`(?:[ 	]+`+t.source+`)?|`+t.source+`(?:[ 	]+`+n.source+`)?)`,i=`(?:[^\\s\\x00-\\x08\\x0e-\\x1f!"#%&'*,\\-:>?@[\\]\`{|}\\x7f-\\x84\\x86-\\x9f\\ud800-\\udfff\\ufffe\\uffff]|[?:-]<PLAIN>)(?:[ \\t]*(?:(?![#:])<PLAIN>|:<PLAIN>))*`.replace(/<PLAIN>/g,function(){return`[^\\s\\x00-\\x08\\x0e-\\x1f,[\\]{}\\x7f-\\x84\\x86-\\x9f\\ud800-\\udfff\\ufffe\\uffff]`}),a=`"(?:[^"\\\\\\r\\n]|\\\\.)*"|'(?:[^'\\\\\\r\\n]|\\\\.)*'`;function o(e,t){t=(t||``).replace(/m/g,``)+`m`;var n=`([:\\-,[{]\\s*(?:\\s<<prop>>[ \\t]+)?)(?:<<value>>)(?=[ \\t]*(?:$|,|\\]|\\}|(?:[\\r\\n]\\s*)?#))`.replace(/<<prop>>/g,function(){return r}).replace(/<<value>>/g,function(){return e});return RegExp(n,t)}e.languages.yaml={scalar:{pattern:RegExp(`([\\-:]\\s*(?:\\s<<prop>>[ \\t]+)?[|>])[ \\t]*(?:((?:\\r?\\n|\\r)[ \\t]+)\\S[^\\r\\n]*(?:\\2[^\\r\\n]+)*)`.replace(/<<prop>>/g,function(){return r})),lookbehind:!0,alias:`string`},comment:/#.*/,key:{pattern:RegExp(`((?:^|[:\\-,[{\\r\\n?])[ \\t]*(?:<<prop>>[ \\t]+)?)<<key>>(?=\\s*:\\s)`.replace(/<<prop>>/g,function(){return r}).replace(/<<key>>/g,function(){return`(?:`+i+`|`+a+`)`})),lookbehind:!0,greedy:!0,alias:`atrule`},directive:{pattern:/(^[ \t]*)%.+/m,lookbehind:!0,alias:`important`},datetime:{pattern:o(`\\d{4}-\\d\\d?-\\d\\d?(?:[tT]|[ \\t]+)\\d\\d?:\\d{2}:\\d{2}(?:\\.\\d*)?(?:[ \\t]*(?:Z|[-+]\\d\\d?(?::\\d{2})?))?|\\d{4}-\\d{2}-\\d{2}|\\d\\d?:\\d{2}(?::\\d{2}(?:\\.\\d*)?)?`),lookbehind:!0,alias:`number`},boolean:{pattern:o(`false|true`,`i`),lookbehind:!0,alias:`important`},null:{pattern:o(`null|~`,`i`),lookbehind:!0,alias:`important`},string:{pattern:o(a),lookbehind:!0,greedy:!0},number:{pattern:o(`[+-]?(?:0x[\\da-f]+|0o[0-7]+|(?:\\d+(?:\\.\\d*)?|\\.\\d+)(?:e[+-]?\\d+)?|\\.inf|\\.nan)`,`i`),lookbehind:!0},tag:n,important:t,punctuation:/---|[:[\]{}\-,|>?]|\.\.\./},e.languages.yml=e.languages.yaml}(X),function(e){var t=`(?:\\\\.|[^\\\\\\n\\r]|(?:\\n|\\r\\n?)(?![\\r\\n]))`;function n(e){return e=e.replace(/<inner>/g,function(){return t}),RegExp(`((?:^|[^\\\\])(?:\\\\{2})*)(?:`+e+`)`)}var r="(?:\\\\.|``(?:[^`\\r\\n]|`(?!`))+``|`[^`\\r\\n]+`|[^\\\\|\\r\\n`])+",i=`\\|?__(?:\\|__)+\\|?(?:(?:\\n|\\r\\n?)|(?![\\s\\S]))`.replace(/__/g,function(){return r}),a=`\\|?[ \\t]*:?-{3,}:?[ \\t]*(?:\\|[ \\t]*:?-{3,}:?[ \\t]*)+\\|?(?:\\n|\\r\\n?)`,o=(e.languages.markdown=e.languages.extend(`markup`,{}),e.languages.insertBefore(`markdown`,`prolog`,{"front-matter-block":{pattern:/(^(?:\s*[\r\n])?)---(?!.)[\s\S]*?[\r\n]---(?!.)/,lookbehind:!0,greedy:!0,inside:{punctuation:/^---|---$/,"front-matter":{pattern:/\S+(?:\s+\S+)*/,alias:[`yaml`,`language-yaml`],inside:e.languages.yaml}}},blockquote:{pattern:/^>(?:[\t ]*>)*/m,alias:`punctuation`},table:{pattern:RegExp(`^`+i+a+`(?:`+i+`)*`,`m`),inside:{"table-data-rows":{pattern:RegExp(`^(`+i+a+`)(?:`+i+`)*$`),lookbehind:!0,inside:{"table-data":{pattern:RegExp(r),inside:e.languages.markdown},punctuation:/\|/}},"table-line":{pattern:RegExp(`^(`+i+`)`+a+`$`),lookbehind:!0,inside:{punctuation:/\||:?-{3,}:?/}},"table-header-row":{pattern:RegExp(`^`+i+`$`),inside:{"table-header":{pattern:RegExp(r),alias:`important`,inside:e.languages.markdown},punctuation:/\|/}}}},code:[{pattern:/((?:^|\n)[ \t]*\n|(?:^|\r\n?)[ \t]*\r\n?)(?: {4}|\t).+(?:(?:\n|\r\n?)(?: {4}|\t).+)*/,lookbehind:!0,alias:`keyword`},{pattern:/^```[\s\S]*?^```$/m,greedy:!0,inside:{"code-block":{pattern:/^(```.*(?:\n|\r\n?))[\s\S]+?(?=(?:\n|\r\n?)^```$)/m,lookbehind:!0},"code-language":{pattern:/^(```).+/,lookbehind:!0},punctuation:/```/}}],title:[{pattern:/\S.*(?:\n|\r\n?)(?:==+|--+)(?=[ \t]*$)/m,alias:`important`,inside:{punctuation:/==+$|--+$/}},{pattern:/(^\s*)#.+/m,lookbehind:!0,alias:`important`,inside:{punctuation:/^#+|#+$/}}],hr:{pattern:/(^\s*)([*-])(?:[\t ]*\2){2,}(?=\s*$)/m,lookbehind:!0,alias:`punctuation`},list:{pattern:/(^\s*)(?:[*+-]|\d+\.)(?=[\t ].)/m,lookbehind:!0,alias:`punctuation`},"url-reference":{pattern:/!?\[[^\]]+\]:[\t ]+(?:\S+|<(?:\\.|[^>\\])+>)(?:[\t ]+(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\)))?/,inside:{variable:{pattern:/^(!?\[)[^\]]+/,lookbehind:!0},string:/(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\))$/,punctuation:/^[\[\]!:]|[<>]/},alias:`url`},bold:{pattern:n(`\\b__(?:(?!_)<inner>|_(?:(?!_)<inner>)+_)+__\\b|\\*\\*(?:(?!\\*)<inner>|\\*(?:(?!\\*)<inner>)+\\*)+\\*\\*`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^..)[\s\S]+(?=..$)/,lookbehind:!0,inside:{}},punctuation:/\*\*|__/}},italic:{pattern:n(`\\b_(?:(?!_)<inner>|__(?:(?!_)<inner>)+__)+_\\b|\\*(?:(?!\\*)<inner>|\\*\\*(?:(?!\\*)<inner>)+\\*\\*)+\\*`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^.)[\s\S]+(?=.$)/,lookbehind:!0,inside:{}},punctuation:/[*_]/}},strike:{pattern:n(`(~~?)(?:(?!~)<inner>)+\\2`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^~~?)[\s\S]+(?=\1$)/,lookbehind:!0,inside:{}},punctuation:/~~?/}},"code-snippet":{pattern:/(^|[^\\`])(?:``[^`\r\n]+(?:`[^`\r\n]+)*``(?!`)|`[^`\r\n]+`(?!`))/,lookbehind:!0,greedy:!0,alias:[`code`,`keyword`]},url:{pattern:n(`!?\\[(?:(?!\\])<inner>)+\\](?:\\([^\\s)]+(?:[\\t ]+"(?:\\\\.|[^"\\\\])*")?\\)|[ \\t]?\\[(?:(?!\\])<inner>)+\\])`),lookbehind:!0,greedy:!0,inside:{operator:/^!/,content:{pattern:/(^\[)[^\]]+(?=\])/,lookbehind:!0,inside:{}},variable:{pattern:/(^\][ \t]?\[)[^\]]+(?=\]$)/,lookbehind:!0},url:{pattern:/(^\]\()[^\s)]+/,lookbehind:!0},string:{pattern:/(^[ \t]+)"(?:\\.|[^"\\])*"(?=\)$)/,lookbehind:!0}}}}),[`url`,`bold`,`italic`,`strike`].forEach(function(t){[`url`,`bold`,`italic`,`strike`,`code-snippet`].forEach(function(n){t!==n&&(e.languages.markdown[t].inside.content.inside[n]=e.languages.markdown[n])})}),e.hooks.add(`after-tokenize`,function(e){e.language!==`markdown`&&e.language!==`md`||function e(t){if(t&&typeof t!=`string`)for(var n=0,r=t.length;n<r;n++){var i,a=t[n];a.type===`code`?(i=a.content[1],a=a.content[3],i&&a&&i.type===`code-language`&&a.type===`code-block`&&typeof i.content==`string`&&(i=i.content.replace(/\b#/g,`sharp`).replace(/\b\+\+/g,`pp`),i=`language-`+(i=(/[a-z][\w-]*/i.exec(i)||[``])[0].toLowerCase()),a.alias?typeof a.alias==`string`?a.alias=[a.alias,i]:a.alias.push(i):a.alias=[i])):e(a.content)}}(e.tokens)}),e.hooks.add(`wrap`,function(t){if(t.type===`code-block`){for(var n=``,r=0,i=t.classes.length;r<i;r++){var a=t.classes[r],a=/language-(.+)/.exec(a);if(a){n=a[1];break}}var l,u=e.languages[n];u?t.content=e.highlight(function(e){return e=e.replace(o,``),e=e.replace(/&(\w{1,8}|#x?[\da-f]{1,8});/gi,function(e,t){var n;return(t=t.toLowerCase())[0]===`#`?(n=t[1]===`x`?parseInt(t.slice(2),16):Number(t.slice(1)),c(n)):s[t]||e})}(t.content),u,n):n&&n!==`none`&&e.plugins.autoloader&&(l=`md-`+new Date().valueOf()+`-`+Math.floor(0x2386f26fc10000*Math.random()),t.attributes.id=l,e.plugins.autoloader.loadLanguages(n,function(){var t=document.getElementById(l);t&&(t.innerHTML=e.highlight(t.textContent,e.languages[n],n))}))}}),RegExp(e.languages.markup.tag.pattern.source,`gi`)),s={amp:`&`,lt:`<`,gt:`>`,quot:`"`},c=String.fromCodePoint||String.fromCharCode;e.languages.md=e.languages.markdown}(X),X.languages.graphql={comment:/#.*/,description:{pattern:/(?:"""(?:[^"]|(?!""")")*"""|"(?:\\.|[^\\"\r\n])*")(?=\s*[a-z_])/i,greedy:!0,alias:`string`,inside:{"language-markdown":{pattern:/(^"(?:"")?)(?!\1)[\s\S]+(?=\1$)/,lookbehind:!0,inside:X.languages.markdown}}},string:{pattern:/"""(?:[^"]|(?!""")")*"""|"(?:\\.|[^\\"\r\n])*"/,greedy:!0},number:/(?:\B-|\b)\d+(?:\.\d+)?(?:e[+-]?\d+)?\b/i,boolean:/\b(?:false|true)\b/,variable:/\$[a-z_]\w*/i,directive:{pattern:/@[a-z_]\w*/i,alias:`function`},"attr-name":{pattern:/\b[a-z_]\w*(?=\s*(?:\((?:[^()"]|"(?:\\.|[^\\"\r\n])*")*\))?:)/i,greedy:!0},"atom-input":{pattern:/\b[A-Z]\w*Input\b/,alias:`class-name`},scalar:/\b(?:Boolean|Float|ID|Int|String)\b/,constant:/\b[A-Z][A-Z_\d]*\b/,"class-name":{pattern:/(\b(?:enum|implements|interface|on|scalar|type|union)\s+|&\s*|:\s*|\[)[A-Z_]\w*/,lookbehind:!0},fragment:{pattern:/(\bfragment\s+|\.{3}\s*(?!on\b))[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},"definition-mutation":{pattern:/(\bmutation\s+)[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},"definition-query":{pattern:/(\bquery\s+)[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},keyword:/\b(?:directive|enum|extend|fragment|implements|input|interface|mutation|on|query|repeatable|scalar|schema|subscription|type|union)\b/,operator:/[!=|&]|\.{3}/,"property-query":/\w+(?=\s*\()/,object:/\w+(?=\s*\{)/,punctuation:/[!(){}\[\]:=,]/,property:/\w+/},X.hooks.add(`after-tokenize`,function(e){if(e.language===`graphql`)for(var t=e.tokens.filter(function(e){return typeof e!=`string`&&e.type!==`comment`&&e.type!==`scalar`}),n=0;n<t.length;){var r=t[n++];if(r.type===`keyword`&&r.content===`mutation`){var i=[];if(d([`definition-mutation`,`punctuation`])&&u(1).content===`(`){n+=2;var a=f(/^\($/,/^\)$/);if(a===-1)continue;for(;n<a;n++){var o=u(0);o.type===`variable`&&(p(o,`variable-input`),i.push(o.content))}n=a+1}if(d([`punctuation`,`property-query`])&&u(0).content===`{`&&(n++,p(u(0),`property-mutation`),0<i.length)){var s=f(/^\{$/,/^\}$/);if(s!==-1)for(var c=n;c<s;c++){var l=t[c];l.type===`variable`&&0<=i.indexOf(l.content)&&p(l,`variable-input`)}}}}function u(e){return t[n+e]}function d(e,t){t||=0;for(var n=0;n<e.length;n++){var r=u(n+t);if(!r||r.type!==e[n])return}return 1}function f(e,r){for(var i=1,a=n;a<t.length;a++){var o=t[a],s=o.content;if(o.type===`punctuation`&&typeof s==`string`){if(e.test(s))i++;else if(r.test(s)&&--i===0)return a}}return-1}function p(e,t){var n=e.alias;n?Array.isArray(n)||(e.alias=n=[n]):e.alias=n=[],n.push(t)}}),X.languages.sql={comment:{pattern:/(^|[^\\])(?:\/\*[\s\S]*?\*\/|(?:--|\/\/|#).*)/,lookbehind:!0},variable:[{pattern:/@(["'`])(?:\\[\s\S]|(?!\1)[^\\])+\1/,greedy:!0},/@[\w.$]+/],string:{pattern:/(^|[^@\\])("|')(?:\\[\s\S]|(?!\2)[^\\]|\2\2)*\2/,greedy:!0,lookbehind:!0},identifier:{pattern:/(^|[^@\\])`(?:\\[\s\S]|[^`\\]|``)*`/,greedy:!0,lookbehind:!0,inside:{punctuation:/^`|`$/}},function:/\b(?:AVG|COUNT|FIRST|FORMAT|LAST|LCASE|LEN|MAX|MID|MIN|MOD|NOW|ROUND|SUM|UCASE)(?=\s*\()/i,keyword:/\b(?:ACTION|ADD|AFTER|ALGORITHM|ALL|ALTER|ANALYZE|ANY|APPLY|AS|ASC|AUTHORIZATION|AUTO_INCREMENT|BACKUP|BDB|BEGIN|BERKELEYDB|BIGINT|BINARY|BIT|BLOB|BOOL|BOOLEAN|BREAK|BROWSE|BTREE|BULK|BY|CALL|CASCADED?|CASE|CHAIN|CHAR(?:ACTER|SET)?|CHECK(?:POINT)?|CLOSE|CLUSTERED|COALESCE|COLLATE|COLUMNS?|COMMENT|COMMIT(?:TED)?|COMPUTE|CONNECT|CONSISTENT|CONSTRAINT|CONTAINS(?:TABLE)?|CONTINUE|CONVERT|CREATE|CROSS|CURRENT(?:_DATE|_TIME|_TIMESTAMP|_USER)?|CURSOR|CYCLE|DATA(?:BASES?)?|DATE(?:TIME)?|DAY|DBCC|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFINER|DELAYED|DELETE|DELIMITERS?|DENY|DESC|DESCRIBE|DETERMINISTIC|DISABLE|DISCARD|DISK|DISTINCT|DISTINCTROW|DISTRIBUTED|DO|DOUBLE|DROP|DUMMY|DUMP(?:FILE)?|DUPLICATE|ELSE(?:IF)?|ENABLE|ENCLOSED|END|ENGINE|ENUM|ERRLVL|ERRORS|ESCAPED?|EXCEPT|EXEC(?:UTE)?|EXISTS|EXIT|EXPLAIN|EXTENDED|FETCH|FIELDS|FILE|FILLFACTOR|FIRST|FIXED|FLOAT|FOLLOWING|FOR(?: EACH ROW)?|FORCE|FOREIGN|FREETEXT(?:TABLE)?|FROM|FULL|FUNCTION|GEOMETRY(?:COLLECTION)?|GLOBAL|GOTO|GRANT|GROUP|HANDLER|HASH|HAVING|HOLDLOCK|HOUR|IDENTITY(?:COL|_INSERT)?|IF|IGNORE|IMPORT|INDEX|INFILE|INNER|INNODB|INOUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|INVOKER|ISOLATION|ITERATE|JOIN|KEYS?|KILL|LANGUAGE|LAST|LEAVE|LEFT|LEVEL|LIMIT|LINENO|LINES|LINESTRING|LOAD|LOCAL|LOCK|LONG(?:BLOB|TEXT)|LOOP|MATCH(?:ED)?|MEDIUM(?:BLOB|INT|TEXT)|MERGE|MIDDLEINT|MINUTE|MODE|MODIFIES|MODIFY|MONTH|MULTI(?:LINESTRING|POINT|POLYGON)|NATIONAL|NATURAL|NCHAR|NEXT|NO|NONCLUSTERED|NULLIF|NUMERIC|OFF?|OFFSETS?|ON|OPEN(?:DATASOURCE|QUERY|ROWSET)?|OPTIMIZE|OPTION(?:ALLY)?|ORDER|OUT(?:ER|FILE)?|OVER|PARTIAL|PARTITION|PERCENT|PIVOT|PLAN|POINT|POLYGON|PRECEDING|PRECISION|PREPARE|PREV|PRIMARY|PRINT|PRIVILEGES|PROC(?:EDURE)?|PUBLIC|PURGE|QUICK|RAISERROR|READS?|REAL|RECONFIGURE|REFERENCES|RELEASE|RENAME|REPEAT(?:ABLE)?|REPLACE|REPLICATION|REQUIRE|RESIGNAL|RESTORE|RESTRICT|RETURN(?:ING|S)?|REVOKE|RIGHT|ROLLBACK|ROUTINE|ROW(?:COUNT|GUIDCOL|S)?|RTREE|RULE|SAVE(?:POINT)?|SCHEMA|SECOND|SELECT|SERIAL(?:IZABLE)?|SESSION(?:_USER)?|SET(?:USER)?|SHARE|SHOW|SHUTDOWN|SIMPLE|SMALLINT|SNAPSHOT|SOME|SONAME|SQL|START(?:ING)?|STATISTICS|STATUS|STRIPED|SYSTEM_USER|TABLES?|TABLESPACE|TEMP(?:ORARY|TABLE)?|TERMINATED|TEXT(?:SIZE)?|THEN|TIME(?:STAMP)?|TINY(?:BLOB|INT|TEXT)|TOP?|TRAN(?:SACTIONS?)?|TRIGGER|TRUNCATE|TSEQUAL|TYPES?|UNBOUNDED|UNCOMMITTED|UNDEFINED|UNION|UNIQUE|UNLOCK|UNPIVOT|UNSIGNED|UPDATE(?:TEXT)?|USAGE|USE|USER|USING|VALUES?|VAR(?:BINARY|CHAR|CHARACTER|YING)|VIEW|WAITFOR|WARNINGS|WHEN|WHERE|WHILE|WITH(?: ROLLUP|IN)?|WORK|WRITE(?:TEXT)?|YEAR)\b/i,boolean:/\b(?:FALSE|NULL|TRUE)\b/i,number:/\b0x[\da-f]+\b|\b\d+(?:\.\d*)?|\B\.\d+\b/i,operator:/[-+*\/=%^~]|&&?|\|\|?|!=?|<(?:=>?|<|>)?|>[>=]?|\b(?:AND|BETWEEN|DIV|ILIKE|IN|IS|LIKE|NOT|OR|REGEXP|RLIKE|SOUNDS LIKE|XOR)\b/i,punctuation:/[;[\]()`,.]/},function(e){var t=e.languages.javascript[`template-string`],n=t.pattern.source,r=t.inside.interpolation,i=r.inside[`interpolation-punctuation`],a=r.pattern.source;function o(t,r){if(e.languages[t])return{pattern:RegExp(`((?:`+r+`)\\s*)`+n),lookbehind:!0,greedy:!0,inside:{"template-punctuation":{pattern:/^`|`$/,alias:`string`},"embedded-code":{pattern:/[\s\S]+/,alias:t}}}}function s(t,n,r){return t={code:t,grammar:n,language:r},e.hooks.run(`before-tokenize`,t),t.tokens=e.tokenize(t.code,t.grammar),e.hooks.run(`after-tokenize`,t),t.tokens}function c(t,n,o){var c=e.tokenize(t,{interpolation:{pattern:RegExp(a),lookbehind:!0}}),l=0,u={},c=s(c.map(function(e){if(typeof e==`string`)return e;for(var n,r,e=e.content;t.indexOf((r=l++,n=`___`+o.toUpperCase()+`_`+r+`___`))!==-1;);return u[n]=e,n}).join(``),n,o),d=Object.keys(u);return l=0,function t(n){for(var a=0;a<n.length;a++){if(l>=d.length)return;var o,c,f,p,m,h,g,_=n[a];typeof _==`string`||typeof _.content==`string`?(o=d[l],(g=(h=typeof _==`string`?_:_.content).indexOf(o))!==-1&&(++l,c=h.substring(0,g),m=u[o],f=void 0,(p={})[`interpolation-punctuation`]=i,(p=e.tokenize(m,p)).length===3&&((f=[1,1]).push.apply(f,s(p[1],e.languages.javascript,`javascript`)),p.splice.apply(p,f)),f=new e.Token(`interpolation`,p,r.alias,m),p=h.substring(g+o.length),m=[],c&&m.push(c),m.push(f),p&&(t(h=[p]),m.push.apply(m,h)),typeof _==`string`?(n.splice.apply(n,[a,1].concat(m)),a+=m.length-1):_.content=m)):(g=_.content,t(Array.isArray(g)?g:[g]))}}(c),new e.Token(o,c,`language-`+o,t)}e.languages.javascript[`template-string`]=[o(`css`,`\\b(?:styled(?:\\([^)]*\\))?(?:\\s*\\.\\s*\\w+(?:\\([^)]*\\))*)*|css(?:\\s*\\.\\s*(?:global|resolve))?|createGlobalStyle|keyframes)`),o(`html`,`\\bhtml|\\.\\s*(?:inner|outer)HTML\\s*\\+?=`),o(`svg`,`\\bsvg`),o(`markdown`,`\\b(?:markdown|md)`),o(`graphql`,`\\b(?:gql|graphql(?:\\s*\\.\\s*experimental)?)`),o(`sql`,`\\bsql`),t].filter(Boolean);var l={javascript:!0,js:!0,typescript:!0,ts:!0,jsx:!0,tsx:!0};function u(e){return typeof e==`string`?e:Array.isArray(e)?e.map(u).join(``):u(e.content)}e.hooks.add(`after-tokenize`,function(t){t.language in l&&function t(n){for(var r=0,i=n.length;r<i;r++){var a,o,s,l=n[r];typeof l!=`string`&&(a=l.content,Array.isArray(a)?l.type===`template-string`?(l=a[1],a.length===3&&typeof l!=`string`&&l.type===`embedded-code`&&(o=u(l),l=l.alias,l=Array.isArray(l)?l[0]:l,s=e.languages[l])&&(a[1]=c(o,s,l))):t(a):typeof a!=`string`&&t([a]))}}(t.tokens)})}(X),function(e){e.languages.typescript=e.languages.extend(`javascript`,{"class-name":{pattern:/(\b(?:class|extends|implements|instanceof|interface|new|type)\s+)(?!keyof\b)(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?:\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)?/,lookbehind:!0,greedy:!0,inside:null},builtin:/\b(?:Array|Function|Promise|any|boolean|console|never|number|string|symbol|unknown)\b/}),e.languages.typescript.keyword.push(/\b(?:abstract|declare|is|keyof|readonly|require)\b/,/\b(?:asserts|infer|interface|module|namespace|type)\b(?=\s*(?:[{_$a-zA-Z\xA0-\uFFFF]|$))/,/\btype\b(?=\s*(?:[\{*]|$))/),delete e.languages.typescript.parameter,delete e.languages.typescript[`literal-property`];var t=e.languages.extend(`typescript`,{});delete t[`class-name`],e.languages.typescript[`class-name`].inside=t,e.languages.insertBefore(`typescript`,`function`,{decorator:{pattern:/@[$\w\xA0-\uFFFF]+/,inside:{at:{pattern:/^@/,alias:`operator`},function:/^[\s\S]+/}},"generic-function":{pattern:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>(?=\s*\()/,greedy:!0,inside:{function:/^#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*/,generic:{pattern:/<[\s\S]+/,alias:`class-name`,inside:t}}}}),e.languages.ts=e.languages.typescript}(X),function(e){var t=e.languages.javascript,n=`\\{(?:[^{}]|\\{(?:[^{}]|\\{[^{}]*\\})*\\})+\\}`,r=`(@(?:arg|argument|param|property)\\s+(?:`+n+`\\s+)?)`;e.languages.jsdoc=e.languages.extend(`javadoclike`,{parameter:{pattern:RegExp(r+`(?:(?!\\s)[$\\w\\xA0-\\uFFFF.])+(?=\\s|$)`),lookbehind:!0,inside:{punctuation:/\./}}}),e.languages.insertBefore(`jsdoc`,`keyword`,{"optional-parameter":{pattern:RegExp(r+`\\[(?:(?!\\s)[$\\w\\xA0-\\uFFFF.])+(?:=[^[\\]]+)?\\](?=\\s|$)`),lookbehind:!0,inside:{parameter:{pattern:/(^\[)[$\w\xA0-\uFFFF\.]+/,lookbehind:!0,inside:{punctuation:/\./}},code:{pattern:/(=)[\s\S]*(?=\]$)/,lookbehind:!0,inside:t,alias:`language-javascript`},punctuation:/[=[\]]/}},"class-name":[{pattern:RegExp(`(@(?:augments|class|extends|interface|memberof!?|template|this|typedef)\\s+(?:<TYPE>\\s+)?)[A-Z]\\w*(?:\\.[A-Z]\\w*)*`.replace(/<TYPE>/g,function(){return n})),lookbehind:!0,inside:{punctuation:/\./}},{pattern:RegExp(`(@[a-z]+\\s+)`+n),lookbehind:!0,inside:{string:t.string,number:t.number,boolean:t.boolean,keyword:e.languages.typescript.keyword,operator:/=>|\.\.\.|[&|?:*]/,punctuation:/[.,;=<>{}()[\]]/}}],example:{pattern:/(@example\s+(?!\s))(?:[^@\s]|\s+(?!\s))+?(?=\s*(?:\*\s*)?(?:@\w|\*\/))/,lookbehind:!0,inside:{code:{pattern:/^([\t ]*(?:\*\s*)?)\S.*$/m,lookbehind:!0,inside:t,alias:`language-javascript`}}}}),e.languages.javadoclike.addSupport(`javascript`,e.languages.jsdoc)}(X),function(e){e.languages.flow=e.languages.extend(`javascript`,{}),e.languages.insertBefore(`flow`,`keyword`,{type:[{pattern:/\b(?:[Bb]oolean|Function|[Nn]umber|[Ss]tring|[Ss]ymbol|any|mixed|null|void)\b/,alias:`class-name`}]}),e.languages.flow[`function-variable`].pattern=/(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*=\s*(?:function\b|(?:\([^()]*\)(?:\s*:\s*\w+)?|(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)\s*=>))/i,delete e.languages.flow.parameter,e.languages.insertBefore(`flow`,`operator`,{"flow-punctuation":{pattern:/\{\||\|\}/,alias:`punctuation`}}),Array.isArray(e.languages.flow.keyword)||(e.languages.flow.keyword=[e.languages.flow.keyword]),e.languages.flow.keyword.unshift({pattern:/(^|[^$]\b)(?:Class|declare|opaque|type)\b(?!\$)/,lookbehind:!0},{pattern:/(^|[^$]\B)\$(?:Diff|Enum|Exact|Keys|ObjMap|PropertyType|Record|Shape|Subtype|Supertype|await)\b(?!\$)/,lookbehind:!0})}(X),X.languages.n4js=X.languages.extend(`javascript`,{keyword:/\b(?:Array|any|boolean|break|case|catch|class|const|constructor|continue|debugger|declare|default|delete|do|else|enum|export|extends|false|finally|for|from|function|get|if|implements|import|in|instanceof|interface|let|module|new|null|number|package|private|protected|public|return|set|static|string|super|switch|this|throw|true|try|typeof|var|void|while|with|yield)\b/}),X.languages.insertBefore(`n4js`,`constant`,{annotation:{pattern:/@+\w+/,alias:`operator`}}),X.languages.n4jsd=X.languages.n4js,function(e){function t(e,t){return RegExp(e.replace(/<ID>/g,function(){return`(?!\\s)[_$a-zA-Z\\xA0-\\uFFFF](?:(?!\\s)[$\\w\\xA0-\\uFFFF])*`}),t)}e.languages.insertBefore(`javascript`,`function-variable`,{"method-variable":{pattern:RegExp(`(\\.\\s*)`+e.languages.javascript[`function-variable`].pattern.source),lookbehind:!0,alias:[`function-variable`,`method`,`function`,`property-access`]}}),e.languages.insertBefore(`javascript`,`function`,{method:{pattern:RegExp(`(\\.\\s*)`+e.languages.javascript.function.source),lookbehind:!0,alias:[`function`,`property-access`]}}),e.languages.insertBefore(`javascript`,`constant`,{"known-class-name":[{pattern:/\b(?:(?:Float(?:32|64)|(?:Int|Uint)(?:8|16|32)|Uint8Clamped)?Array|ArrayBuffer|BigInt|Boolean|DataView|Date|Error|Function|Intl|JSON|(?:Weak)?(?:Map|Set)|Math|Number|Object|Promise|Proxy|Reflect|RegExp|String|Symbol|WebAssembly)\b/,alias:`class-name`},{pattern:/\b(?:[A-Z]\w*)Error\b/,alias:`class-name`}]}),e.languages.insertBefore(`javascript`,`keyword`,{imports:{pattern:t(`(\\bimport\\b\\s*)(?:<ID>(?:\\s*,\\s*(?:\\*\\s*as\\s+<ID>|\\{[^{}]*\\}))?|\\*\\s*as\\s+<ID>|\\{[^{}]*\\})(?=\\s*\\bfrom\\b)`),lookbehind:!0,inside:e.languages.javascript},exports:{pattern:t(`(\\bexport\\b\\s*)(?:\\*(?:\\s*as\\s+<ID>)?(?=\\s*\\bfrom\\b)|\\{[^{}]*\\})`),lookbehind:!0,inside:e.languages.javascript}}),e.languages.javascript.keyword.unshift({pattern:/\b(?:as|default|export|from|import)\b/,alias:`module`},{pattern:/\b(?:await|break|catch|continue|do|else|finally|for|if|return|switch|throw|try|while|yield)\b/,alias:`control-flow`},{pattern:/\bnull\b/,alias:[`null`,`nil`]},{pattern:/\bundefined\b/,alias:`nil`}),e.languages.insertBefore(`javascript`,`operator`,{spread:{pattern:/\.{3}/,alias:`operator`},arrow:{pattern:/=>/,alias:`operator`}}),e.languages.insertBefore(`javascript`,`punctuation`,{"property-access":{pattern:t(`(\\.\\s*)#?<ID>`),lookbehind:!0},"maybe-class-name":{pattern:/(^|[^$\w\xA0-\uFFFF])[A-Z][$\w\xA0-\uFFFF]+/,lookbehind:!0},dom:{pattern:/\b(?:document|(?:local|session)Storage|location|navigator|performance|window)\b/,alias:`variable`},console:{pattern:/\bconsole(?=\s*\.)/,alias:`class-name`}});for(var n=[`function`,`function-variable`,`method`,`method-variable`,`property-access`],r=0;r<n.length;r++){var i=n[r],a=e.languages.javascript[i],i=(a=e.util.type(a)===`RegExp`?e.languages.javascript[i]={pattern:a}:a).inside||{};(a.inside=i)[`maybe-class-name`]=/^[A-Z][\s\S]*/}}(X),function(e){var t=e.util.clone(e.languages.javascript),n=`(?:\\s|\\/\\/.*(?!.)|\\/\\*(?:[^*]|\\*(?!\\/))\\*\\/)`,r=`(?:\\{(?:\\{(?:\\{[^{}]*\\}|[^{}])*\\}|[^{}])*\\})`,i=`(?:\\{<S>*\\.{3}(?:[^{}]|<BRACES>)*\\})`;function a(e,t){return e=e.replace(/<S>/g,function(){return n}).replace(/<BRACES>/g,function(){return r}).replace(/<SPREAD>/g,function(){return i}),RegExp(e,t)}i=a(i).source,e.languages.jsx=e.languages.extend(`markup`,t),e.languages.jsx.tag.pattern=a(`<\\/?(?:[\\w.:-]+(?:<S>+(?:[\\w.:$-]+(?:=(?:"(?:\\\\[\\s\\S]|[^\\\\"])*"|'(?:\\\\[\\s\\S]|[^\\\\'])*'|[^\\s{'"/>=]+|<BRACES>))?|<SPREAD>))*<S>*\\/?)?>`),e.languages.jsx.tag.inside.tag.pattern=/^<\/?[^\s>\/]*/,e.languages.jsx.tag.inside[`attr-value`].pattern=/=(?!\{)(?:"(?:\\[\s\S]|[^\\"])*"|'(?:\\[\s\S]|[^\\'])*'|[^\s'">]+)/,e.languages.jsx.tag.inside.tag.inside[`class-name`]=/^[A-Z]\w*(?:\.[A-Z]\w*)*$/,e.languages.jsx.tag.inside.comment=t.comment,e.languages.insertBefore(`inside`,`attr-name`,{spread:{pattern:a(`<SPREAD>`),inside:e.languages.jsx}},e.languages.jsx.tag),e.languages.insertBefore(`inside`,`special-attr`,{script:{pattern:a(`=<BRACES>`),alias:`language-javascript`,inside:{"script-punctuation":{pattern:/^=(?=\{)/,alias:`punctuation`},rest:e.languages.jsx}}},e.languages.jsx.tag);function o(t){for(var n=[],r=0;r<t.length;r++){var i=t[r],a=!1;typeof i!=`string`&&(i.type===`tag`&&i.content[0]&&i.content[0].type===`tag`?i.content[0].content[0].content===`</`?0<n.length&&n[n.length-1].tagName===s(i.content[0].content[1])&&n.pop():i.content[i.content.length-1].content!==`/>`&&n.push({tagName:s(i.content[0].content[1]),openedBraces:0}):0<n.length&&i.type===`punctuation`&&i.content===`{`?n[n.length-1].openedBraces++:0<n.length&&0<n[n.length-1].openedBraces&&i.type===`punctuation`&&i.content===`}`?n[n.length-1].openedBraces--:a=!0),(a||typeof i==`string`)&&0<n.length&&n[n.length-1].openedBraces===0&&(a=s(i),r<t.length-1&&(typeof t[r+1]==`string`||t[r+1].type===`plain-text`)&&(a+=s(t[r+1]),t.splice(r+1,1)),0<r&&(typeof t[r-1]==`string`||t[r-1].type===`plain-text`)&&(a=s(t[r-1])+a,t.splice(r-1,1),r--),t[r]=new e.Token(`plain-text`,a,null,a)),i.content&&typeof i.content!=`string`&&o(i.content)}}var s=function(e){return e?typeof e==`string`?e:typeof e.content==`string`?e.content:e.content.map(s).join(``):``};e.hooks.add(`after-tokenize`,function(e){e.language!==`jsx`&&e.language!==`tsx`||o(e.tokens)})}(X),function(e){var t=e.util.clone(e.languages.typescript),t=(e.languages.tsx=e.languages.extend(`jsx`,t),delete e.languages.tsx.parameter,delete e.languages.tsx[`literal-property`],e.languages.tsx.tag);t.pattern=RegExp(`(^|[^\\w$]|(?=<\\/))(?:`+t.pattern.source+`)`,t.pattern.flags),t.lookbehind=!0}(X),X.languages.swift={comment:{pattern:/(^|[^\\:])(?:\/\/.*|\/\*(?:[^/*]|\/(?!\*)|\*(?!\/)|\/\*(?:[^*]|\*(?!\/))*\*\/)*\*\/)/,lookbehind:!0,greedy:!0},"string-literal":[{pattern:RegExp(`(^|[^"#])(?:"(?:\\\\(?:\\((?:[^()]|\\([^()]*\\))*\\)|\\r\\n|[^(])|[^\\\\\\r\\n"])*"|"""(?:\\\\(?:\\((?:[^()]|\\([^()]*\\))*\\)|[^(])|[^\\\\"]|"(?!""))*""")(?!["#])`),lookbehind:!0,greedy:!0,inside:{interpolation:{pattern:/(\\\()(?:[^()]|\([^()]*\))*(?=\))/,lookbehind:!0,inside:null},"interpolation-punctuation":{pattern:/^\)|\\\($/,alias:`punctuation`},punctuation:/\\(?=[\r\n])/,string:/[\s\S]+/}},{pattern:RegExp(`(^|[^"#])(#+)(?:"(?:\\\\(?:#+\\((?:[^()]|\\([^()]*\\))*\\)|\\r\\n|[^#])|[^\\\\\\r\\n])*?"|"""(?:\\\\(?:#+\\((?:[^()]|\\([^()]*\\))*\\)|[^#])|[^\\\\])*?""")\\2`),lookbehind:!0,greedy:!0,inside:{interpolation:{pattern:/(\\#+\()(?:[^()]|\([^()]*\))*(?=\))/,lookbehind:!0,inside:null},"interpolation-punctuation":{pattern:/^\)|\\#+\($/,alias:`punctuation`},string:/[\s\S]+/}}],directive:{pattern:RegExp(`#(?:(?:elseif|if)\\b(?:[ 	]*(?:![ \\t]*)?(?:\\b\\w+\\b(?:[ \\t]*\\((?:[^()]|\\([^()]*\\))*\\))?|\\((?:[^()]|\\([^()]*\\))*\\))(?:[ \\t]*(?:&&|\\|\\|))?)+|(?:else|endif)\\b)`),alias:`property`,inside:{"directive-name":/^#\w+/,boolean:/\b(?:false|true)\b/,number:/\b\d+(?:\.\d+)*\b/,operator:/!|&&|\|\||[<>]=?/,punctuation:/[(),]/}},literal:{pattern:/#(?:colorLiteral|column|dsohandle|file(?:ID|Literal|Path)?|function|imageLiteral|line)\b/,alias:`constant`},"other-directive":{pattern:/#\w+\b/,alias:`property`},attribute:{pattern:/@\w+/,alias:`atrule`},"function-definition":{pattern:/(\bfunc\s+)\w+/,lookbehind:!0,alias:`function`},label:{pattern:/\b(break|continue)\s+\w+|\b[a-zA-Z_]\w*(?=\s*:\s*(?:for|repeat|while)\b)/,lookbehind:!0,alias:`important`},keyword:/\b(?:Any|Protocol|Self|Type|actor|as|assignment|associatedtype|associativity|async|await|break|case|catch|class|continue|convenience|default|defer|deinit|didSet|do|dynamic|else|enum|extension|fallthrough|fileprivate|final|for|func|get|guard|higherThan|if|import|in|indirect|infix|init|inout|internal|is|isolated|lazy|left|let|lowerThan|mutating|none|nonisolated|nonmutating|open|operator|optional|override|postfix|precedencegroup|prefix|private|protocol|public|repeat|required|rethrows|return|right|safe|self|set|some|static|struct|subscript|super|switch|throw|throws|try|typealias|unowned|unsafe|var|weak|where|while|willSet)\b/,boolean:/\b(?:false|true)\b/,nil:{pattern:/\bnil\b/,alias:`constant`},"short-argument":/\$\d+\b/,omit:{pattern:/\b_\b/,alias:`keyword`},number:/\b(?:[\d_]+(?:\.[\de_]+)?|0x[a-f0-9_]+(?:\.[a-f0-9p_]+)?|0b[01_]+|0o[0-7_]+)\b/i,"class-name":/\b[A-Z](?:[A-Z_\d]*[a-z]\w*)?\b/,function:/\b[a-z_]\w*(?=\s*\()/i,constant:/\b(?:[A-Z_]{2,}|k[A-Z][A-Za-z_]+)\b/,operator:/[-+*/%=!<>&|^~?]+|\.[.\-+*/%=!<>&|^~?]+/,punctuation:/[{}[\]();,.:\\]/},X.languages.swift[`string-literal`].forEach(function(e){e.inside.interpolation.inside=X.languages.swift}),function(e){e.languages.kotlin=e.languages.extend(`clike`,{keyword:{pattern:/(^|[^.])\b(?:abstract|actual|annotation|as|break|by|catch|class|companion|const|constructor|continue|crossinline|data|do|dynamic|else|enum|expect|external|final|finally|for|fun|get|if|import|in|infix|init|inline|inner|interface|internal|is|lateinit|noinline|null|object|open|operator|out|override|package|private|protected|public|reified|return|sealed|set|super|suspend|tailrec|this|throw|to|try|typealias|val|var|vararg|when|where|while)\b/,lookbehind:!0},function:[{pattern:/(?:`[^\r\n`]+`|\b\w+)(?=\s*\()/,greedy:!0},{pattern:/(\.)(?:`[^\r\n`]+`|\w+)(?=\s*\{)/,lookbehind:!0,greedy:!0}],number:/\b(?:0[xX][\da-fA-F]+(?:_[\da-fA-F]+)*|0[bB][01]+(?:_[01]+)*|\d+(?:_\d+)*(?:\.\d+(?:_\d+)*)?(?:[eE][+-]?\d+(?:_\d+)*)?[fFL]?)\b/,operator:/\+[+=]?|-[-=>]?|==?=?|!(?:!|==?)?|[\/*%<>]=?|[?:]:?|\.\.|&&|\|\||\b(?:and|inv|or|shl|shr|ushr|xor)\b/}),delete e.languages.kotlin[`class-name`];var t={"interpolation-punctuation":{pattern:/^\$\{?|\}$/,alias:`punctuation`},expression:{pattern:/[\s\S]+/,inside:e.languages.kotlin}};e.languages.insertBefore(`kotlin`,`string`,{"string-literal":[{pattern:/"""(?:[^$]|\$(?:(?!\{)|\{[^{}]*\}))*?"""/,alias:`multiline`,inside:{interpolation:{pattern:/\$(?:[a-z_]\w*|\{[^{}]*\})/i,inside:t},string:/[\s\S]+/}},{pattern:/"(?:[^"\\\r\n$]|\\.|\$(?:(?!\{)|\{[^{}]*\}))*"/,alias:`singleline`,inside:{interpolation:{pattern:/((?:^|[^\\])(?:\\{2})*)\$(?:[a-z_]\w*|\{[^{}]*\})/i,lookbehind:!0,inside:t},string:/[\s\S]+/}}],char:{pattern:/'(?:[^'\\\r\n]|\\(?:.|u[a-fA-F0-9]{0,4}))'/,greedy:!0}}),delete e.languages.kotlin.string,e.languages.insertBefore(`kotlin`,`keyword`,{annotation:{pattern:/\B@(?:\w+:)?(?:[A-Z]\w*|\[[^\]]+\])/,alias:`builtin`}}),e.languages.insertBefore(`kotlin`,`function`,{label:{pattern:/\b\w+@|@\w+\b/,alias:`symbol`}}),e.languages.kt=e.languages.kotlin,e.languages.kts=e.languages.kotlin}(X),X.languages.c=X.languages.extend(`clike`,{comment:{pattern:/\/\/(?:[^\r\n\\]|\\(?:\r\n?|\n|(?![\r\n])))*|\/\*[\s\S]*?(?:\*\/|$)/,greedy:!0},string:{pattern:/"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"/,greedy:!0},"class-name":{pattern:/(\b(?:enum|struct)\s+(?:__attribute__\s*\(\([\s\S]*?\)\)\s*)?)\w+|\b[a-z]\w*_t\b/,lookbehind:!0},keyword:/\b(?:_Alignas|_Alignof|_Atomic|_Bool|_Complex|_Generic|_Imaginary|_Noreturn|_Static_assert|_Thread_local|__attribute__|asm|auto|break|case|char|const|continue|default|do|double|else|enum|extern|float|for|goto|if|inline|int|long|register|return|short|signed|sizeof|static|struct|switch|typedef|typeof|union|unsigned|void|volatile|while)\b/,function:/\b[a-z_]\w*(?=\s*\()/i,number:/(?:\b0x(?:[\da-f]+(?:\.[\da-f]*)?|\.[\da-f]+)(?:p[+-]?\d+)?|(?:\b\d+(?:\.\d*)?|\B\.\d+)(?:e[+-]?\d+)?)[ful]{0,4}/i,operator:/>>=?|<<=?|->|([-+&|:])\1|[?:~]|[-+*/%&|^!=<>]=?/}),X.languages.insertBefore(`c`,`string`,{char:{pattern:/'(?:\\(?:\r\n|[\s\S])|[^'\\\r\n]){0,32}'/,greedy:!0}}),X.languages.insertBefore(`c`,`string`,{macro:{pattern:/(^[\t ]*)#\s*[a-z](?:[^\r\n\\/]|\/(?!\*)|\/\*(?:[^*]|\*(?!\/))*\*\/|\\(?:\r\n|[\s\S]))*/im,lookbehind:!0,greedy:!0,alias:`property`,inside:{string:[{pattern:/^(#\s*include\s*)<[^>]+>/,lookbehind:!0},X.languages.c.string],char:X.languages.c.char,comment:X.languages.c.comment,"macro-name":[{pattern:/(^#\s*define\s+)\w+\b(?!\()/i,lookbehind:!0},{pattern:/(^#\s*define\s+)\w+\b(?=\()/i,lookbehind:!0,alias:`function`}],directive:{pattern:/^(#\s*)[a-z]+/,lookbehind:!0,alias:`keyword`},"directive-hash":/^#/,punctuation:/##|\\(?=[\r\n])/,expression:{pattern:/\S[\s\S]*/,inside:X.languages.c}}}}),X.languages.insertBefore(`c`,`function`,{constant:/\b(?:EOF|NULL|SEEK_CUR|SEEK_END|SEEK_SET|__DATE__|__FILE__|__LINE__|__TIMESTAMP__|__TIME__|__func__|stderr|stdin|stdout)\b/}),delete X.languages.c.boolean,X.languages.objectivec=X.languages.extend(`c`,{string:{pattern:/@?"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"/,greedy:!0},keyword:/\b(?:asm|auto|break|case|char|const|continue|default|do|double|else|enum|extern|float|for|goto|if|in|inline|int|long|register|return|self|short|signed|sizeof|static|struct|super|switch|typedef|typeof|union|unsigned|void|volatile|while)\b|(?:@interface|@end|@implementation|@protocol|@class|@public|@protected|@private|@property|@try|@catch|@finally|@throw|@synthesize|@dynamic|@selector)\b/,operator:/-[->]?|\+\+?|!=?|<<?=?|>>?=?|==?|&&?|\|\|?|[~^%?*\/@]/}),delete X.languages.objectivec[`class-name`],X.languages.objc=X.languages.objectivec,X.languages.reason=X.languages.extend(`clike`,{string:{pattern:/"(?:\\(?:\r\n|[\s\S])|[^\\\r\n"])*"/,greedy:!0},"class-name":/\b[A-Z]\w*/,keyword:/\b(?:and|as|assert|begin|class|constraint|do|done|downto|else|end|exception|external|for|fun|function|functor|if|in|include|inherit|initializer|lazy|let|method|module|mutable|new|nonrec|object|of|open|or|private|rec|sig|struct|switch|then|to|try|type|val|virtual|when|while|with)\b/,operator:/\.{3}|:[:=]|\|>|->|=(?:==?|>)?|<=?|>=?|[|^?'#!~`]|[+\-*\/]\.?|\b(?:asr|land|lor|lsl|lsr|lxor|mod)\b/}),X.languages.insertBefore(`reason`,`class-name`,{char:{pattern:/'(?:\\x[\da-f]{2}|\\o[0-3][0-7][0-7]|\\\d{3}|\\.|[^'\\\r\n])'/,greedy:!0},constructor:/\b[A-Z]\w*\b(?!\s*\.)/,label:{pattern:/\b[a-z]\w*(?=::)/,alias:`symbol`}}),delete X.languages.reason.function,function(e){for(var t=`\\/\\*(?:[^*/]|\\*(?!\\/)|\\/(?!\\*)|<self>)*\\*\\/`,n=0;n<2;n++)t=t.replace(/<self>/g,function(){return t});t=t.replace(/<self>/g,function(){return`[^\\s\\S]`}),e.languages.rust={comment:[{pattern:RegExp(`(^|[^\\\\])`+t),lookbehind:!0,greedy:!0},{pattern:/(^|[^\\:])\/\/.*/,lookbehind:!0,greedy:!0}],string:{pattern:/b?"(?:\\[\s\S]|[^\\"])*"|b?r(#*)"(?:[^"]|"(?!\1))*"\1/,greedy:!0},char:{pattern:/b?'(?:\\(?:x[0-7][\da-fA-F]|u\{(?:[\da-fA-F]_*){1,6}\}|.)|[^\\\r\n\t'])'/,greedy:!0},attribute:{pattern:/#!?\[(?:[^\[\]"]|"(?:\\[\s\S]|[^\\"])*")*\]/,greedy:!0,alias:`attr-name`,inside:{string:null}},"closure-params":{pattern:/([=(,:]\s*|\bmove\s*)\|[^|]*\||\|[^|]*\|(?=\s*(?:\{|->))/,lookbehind:!0,greedy:!0,inside:{"closure-punctuation":{pattern:/^\||\|$/,alias:`punctuation`},rest:null}},"lifetime-annotation":{pattern:/'\w+/,alias:`symbol`},"fragment-specifier":{pattern:/(\$\w+:)[a-z]+/,lookbehind:!0,alias:`punctuation`},variable:/\$\w+/,"function-definition":{pattern:/(\bfn\s+)\w+/,lookbehind:!0,alias:`function`},"type-definition":{pattern:/(\b(?:enum|struct|trait|type|union)\s+)\w+/,lookbehind:!0,alias:`class-name`},"module-declaration":[{pattern:/(\b(?:crate|mod)\s+)[a-z][a-z_\d]*/,lookbehind:!0,alias:`namespace`},{pattern:/(\b(?:crate|self|super)\s*)::\s*[a-z][a-z_\d]*\b(?:\s*::(?:\s*[a-z][a-z_\d]*\s*::)*)?/,lookbehind:!0,alias:`namespace`,inside:{punctuation:/::/}}],keyword:[/\b(?:Self|abstract|as|async|await|become|box|break|const|continue|crate|do|dyn|else|enum|extern|final|fn|for|if|impl|in|let|loop|macro|match|mod|move|mut|override|priv|pub|ref|return|self|static|struct|super|trait|try|type|typeof|union|unsafe|unsized|use|virtual|where|while|yield)\b/,/\b(?:bool|char|f(?:32|64)|[ui](?:8|16|32|64|128|size)|str)\b/],function:/\b[a-z_]\w*(?=\s*(?:::\s*<|\())/,macro:{pattern:/\b\w+!/,alias:`property`},constant:/\b[A-Z_][A-Z_\d]+\b/,"class-name":/\b[A-Z]\w*\b/,namespace:{pattern:/(?:\b[a-z][a-z_\d]*\s*::\s*)*\b[a-z][a-z_\d]*\s*::(?!\s*<)/,inside:{punctuation:/::/}},number:/\b(?:0x[\dA-Fa-f](?:_?[\dA-Fa-f])*|0o[0-7](?:_?[0-7])*|0b[01](?:_?[01])*|(?:(?:\d(?:_?\d)*)?\.)?\d(?:_?\d)*(?:[Ee][+-]?\d+)?)(?:_?(?:f32|f64|[iu](?:8|16|32|64|size)?))?\b/,boolean:/\b(?:false|true)\b/,punctuation:/->|\.\.=|\.{1,3}|::|[{}[\];(),:]/,operator:/[-+*\/%!^]=?|=[=>]?|&[&=]?|\|[|=]?|<<?=?|>>?=?|[@?]/},e.languages.rust[`closure-params`].inside.rest=e.languages.rust,e.languages.rust.attribute.inside.string=e.languages.rust.string}(X),X.languages.go=X.languages.extend(`clike`,{string:{pattern:/(^|[^\\])"(?:\\.|[^"\\\r\n])*"|`[^`]*`/,lookbehind:!0,greedy:!0},keyword:/\b(?:break|case|chan|const|continue|default|defer|else|fallthrough|for|func|go(?:to)?|if|import|interface|map|package|range|return|select|struct|switch|type|var)\b/,boolean:/\b(?:_|false|iota|nil|true)\b/,number:[/\b0(?:b[01_]+|o[0-7_]+)i?\b/i,/\b0x(?:[a-f\d_]+(?:\.[a-f\d_]*)?|\.[a-f\d_]+)(?:p[+-]?\d+(?:_\d+)*)?i?(?!\w)/i,/(?:\b\d[\d_]*(?:\.[\d_]*)?|\B\.\d[\d_]*)(?:e[+-]?[\d_]+)?i?(?!\w)/i],operator:/[*\/%^!=]=?|\+[=+]?|-[=-]?|\|[=|]?|&(?:=|&|\^=?)?|>(?:>=?|=)?|<(?:<=?|=|-)?|:=|\.\.\./,builtin:/\b(?:append|bool|byte|cap|close|complex|complex(?:64|128)|copy|delete|error|float(?:32|64)|u?int(?:8|16|32|64)?|imag|len|make|new|panic|print(?:ln)?|real|recover|rune|string|uintptr)\b/}),X.languages.insertBefore(`go`,`string`,{char:{pattern:/'(?:\\.|[^'\\\r\n]){0,10}'/,greedy:!0}}),delete X.languages.go[`class-name`],function(e){var t=/\b(?:alignas|alignof|asm|auto|bool|break|case|catch|char|char16_t|char32_t|char8_t|class|co_await|co_return|co_yield|compl|concept|const|const_cast|consteval|constexpr|constinit|continue|decltype|default|delete|do|double|dynamic_cast|else|enum|explicit|export|extern|final|float|for|friend|goto|if|import|inline|int|int16_t|int32_t|int64_t|int8_t|long|module|mutable|namespace|new|noexcept|nullptr|operator|override|private|protected|public|register|reinterpret_cast|requires|return|short|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|throw|try|typedef|typeid|typename|uint16_t|uint32_t|uint64_t|uint8_t|union|unsigned|using|virtual|void|volatile|wchar_t|while)\b/,n=`\\b(?!<keyword>)\\w+(?:\\s*\\.\\s*\\w+)*\\b`.replace(/<keyword>/g,function(){return t.source});e.languages.cpp=e.languages.extend(`c`,{"class-name":[{pattern:RegExp(`(\\b(?:class|concept|enum|struct|typename)\\s+)(?!<keyword>)\\w+`.replace(/<keyword>/g,function(){return t.source})),lookbehind:!0},/\b[A-Z]\w*(?=\s*::\s*\w+\s*\()/,/\b[A-Z_]\w*(?=\s*::\s*~\w+\s*\()/i,/\b\w+(?=\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>\s*::\s*\w+\s*\()/],keyword:t,number:{pattern:/(?:\b0b[01']+|\b0x(?:[\da-f']+(?:\.[\da-f']*)?|\.[\da-f']+)(?:p[+-]?[\d']+)?|(?:\b[\d']+(?:\.[\d']*)?|\B\.[\d']+)(?:e[+-]?[\d']+)?)[ful]{0,4}/i,greedy:!0},operator:/>>=?|<<=?|->|--|\+\+|&&|\|\||[?:~]|<=>|[-+*/%&|^!=<>]=?|\b(?:and|and_eq|bitand|bitor|not|not_eq|or|or_eq|xor|xor_eq)\b/,boolean:/\b(?:false|true)\b/}),e.languages.insertBefore(`cpp`,`string`,{module:{pattern:RegExp(`(\\b(?:import|module)\\s+)(?:"(?:\\\\(?:\\r\\n|[\\s\\S])|[^"\\\\\\r\\n])*"|<[^<>\\r\\n]*>|`+`<mod-name>(?:\\s*:\\s*<mod-name>)?|:\\s*<mod-name>`.replace(/<mod-name>/g,function(){return n})+`)`),lookbehind:!0,greedy:!0,inside:{string:/^[<"][\s\S]+/,operator:/:/,punctuation:/\./}},"raw-string":{pattern:/R"([^()\\ ]{0,16})\([\s\S]*?\)\1"/,alias:`string`,greedy:!0}}),e.languages.insertBefore(`cpp`,`keyword`,{"generic-function":{pattern:/\b(?!operator\b)[a-z_]\w*\s*<(?:[^<>]|<[^<>]*>)*>(?=\s*\()/i,inside:{function:/^\w+/,generic:{pattern:/<[\s\S]+/,alias:`class-name`,inside:e.languages.cpp}}}}),e.languages.insertBefore(`cpp`,`operator`,{"double-colon":{pattern:/::/,alias:`punctuation`}}),e.languages.insertBefore(`cpp`,`class-name`,{"base-clause":{pattern:/(\b(?:class|struct)\s+\w+\s*:\s*)[^;{}"'\s]+(?:\s+[^;{}"'\s]+)*(?=\s*[;{])/,lookbehind:!0,greedy:!0,inside:e.languages.extend(`cpp`,{})}}),e.languages.insertBefore(`inside`,`double-colon`,{"class-name":/\b[a-z_]\w*\b(?!\s*::)/i},e.languages.cpp[`base-clause`])}(X),X.languages.python={comment:{pattern:/(^|[^\\])#.*/,lookbehind:!0,greedy:!0},"string-interpolation":{pattern:/(?:f|fr|rf)(?:("""|''')[\s\S]*?\1|("|')(?:\\.|(?!\2)[^\\\r\n])*\2)/i,greedy:!0,inside:{interpolation:{pattern:/((?:^|[^{])(?:\{\{)*)\{(?!\{)(?:[^{}]|\{(?!\{)(?:[^{}]|\{(?!\{)(?:[^{}])+\})+\})+\}/,lookbehind:!0,inside:{"format-spec":{pattern:/(:)[^:(){}]+(?=\}$)/,lookbehind:!0},"conversion-option":{pattern:/![sra](?=[:}]$)/,alias:`punctuation`},rest:null}},string:/[\s\S]+/}},"triple-quoted-string":{pattern:/(?:[rub]|br|rb)?("""|''')[\s\S]*?\1/i,greedy:!0,alias:`string`},string:{pattern:/(?:[rub]|br|rb)?("|')(?:\\.|(?!\1)[^\\\r\n])*\1/i,greedy:!0},function:{pattern:/((?:^|\s)def[ \t]+)[a-zA-Z_]\w*(?=\s*\()/g,lookbehind:!0},"class-name":{pattern:/(\bclass\s+)\w+/i,lookbehind:!0},decorator:{pattern:/(^[\t ]*)@\w+(?:\.\w+)*/m,lookbehind:!0,alias:[`annotation`,`punctuation`],inside:{punctuation:/\./}},keyword:/\b(?:_(?=\s*:)|and|as|assert|async|await|break|case|class|continue|def|del|elif|else|except|exec|finally|for|from|global|if|import|in|is|lambda|match|nonlocal|not|or|pass|print|raise|return|try|while|with|yield)\b/,builtin:/\b(?:__import__|abs|all|any|apply|ascii|basestring|bin|bool|buffer|bytearray|bytes|callable|chr|classmethod|cmp|coerce|compile|complex|delattr|dict|dir|divmod|enumerate|eval|execfile|file|filter|float|format|frozenset|getattr|globals|hasattr|hash|help|hex|id|input|int|intern|isinstance|issubclass|iter|len|list|locals|long|map|max|memoryview|min|next|object|oct|open|ord|pow|property|range|raw_input|reduce|reload|repr|reversed|round|set|setattr|slice|sorted|staticmethod|str|sum|super|tuple|type|unichr|unicode|vars|xrange|zip)\b/,boolean:/\b(?:False|None|True)\b/,number:/\b0(?:b(?:_?[01])+|o(?:_?[0-7])+|x(?:_?[a-f0-9])+)\b|(?:\b\d+(?:_\d+)*(?:\.(?:\d+(?:_\d+)*)?)?|\B\.\d+(?:_\d+)*)(?:e[+-]?\d+(?:_\d+)*)?j?(?!\w)/i,operator:/[-+%=]=?|!=|:=|\*\*?=?|\/\/?=?|<[<=>]?|>[=>]?|[&|^~]/,punctuation:/[{}[\];(),.:]/},X.languages.python[`string-interpolation`].inside.interpolation.inside.rest=X.languages.python,X.languages.py=X.languages.python,X.languages.json={property:{pattern:/(^|[^\\])"(?:\\.|[^\\"\r\n])*"(?=\s*:)/,lookbehind:!0,greedy:!0},string:{pattern:/(^|[^\\])"(?:\\.|[^\\"\r\n])*"(?!\s*:)/,lookbehind:!0,greedy:!0},comment:{pattern:/\/\/.*|\/\*[\s\S]*?(?:\*\/|$)/,greedy:!0},number:/-?\b\d+(?:\.\d+)?(?:e[+-]?\d+)?\b/i,punctuation:/[{}[\],]/,operator:/:/,boolean:/\b(?:false|true)\b/,null:{pattern:/\bnull\b/,alias:`keyword`}},X.languages.webmanifest=X.languages.json;var iv={};nv(iv,{dracula:()=>av,duotoneDark:()=>ov,duotoneLight:()=>sv,github:()=>cv,gruvboxMaterialDark:()=>wv,gruvboxMaterialLight:()=>Tv,jettwaveDark:()=>bv,jettwaveLight:()=>xv,nightOwl:()=>lv,nightOwlLight:()=>uv,oceanicNext:()=>fv,okaidia:()=>pv,oneDark:()=>Sv,oneLight:()=>Cv,palenight:()=>mv,shadesOfPurple:()=>hv,synthwave84:()=>gv,ultramin:()=>_v,vsDark:()=>vv,vsLight:()=>yv});var av={plain:{color:`#F8F8F2`,backgroundColor:`#282A36`},styles:[{types:[`prolog`,`constant`,`builtin`],style:{color:`rgb(189, 147, 249)`}},{types:[`inserted`,`function`],style:{color:`rgb(80, 250, 123)`}},{types:[`deleted`],style:{color:`rgb(255, 85, 85)`}},{types:[`changed`],style:{color:`rgb(255, 184, 108)`}},{types:[`punctuation`,`symbol`],style:{color:`rgb(248, 248, 242)`}},{types:[`string`,`char`,`tag`,`selector`],style:{color:`rgb(255, 121, 198)`}},{types:[`keyword`,`variable`],style:{color:`rgb(189, 147, 249)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(98, 114, 164)`}},{types:[`attr-name`],style:{color:`rgb(241, 250, 140)`}}]},ov={plain:{backgroundColor:`#2a2734`,color:`#9a86fd`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`,`punctuation`],style:{color:`#6c6783`}},{types:[`namespace`],style:{opacity:.7}},{types:[`tag`,`operator`,`number`],style:{color:`#e09142`}},{types:[`property`,`function`],style:{color:`#9a86fd`}},{types:[`tag-id`,`selector`,`atrule-id`],style:{color:`#eeebff`}},{types:[`attr-name`],style:{color:`#c4b9fe`}},{types:[`boolean`,`string`,`entity`,`url`,`attr-value`,`keyword`,`control`,`directive`,`unit`,`statement`,`regex`,`atrule`,`placeholder`,`variable`],style:{color:`#ffcc99`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`#c4b9fe`}}]},sv={plain:{backgroundColor:`#faf8f5`,color:`#728fcb`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`,`punctuation`],style:{color:`#b6ad9a`}},{types:[`namespace`],style:{opacity:.7}},{types:[`tag`,`operator`,`number`],style:{color:`#063289`}},{types:[`property`,`function`],style:{color:`#b29762`}},{types:[`tag-id`,`selector`,`atrule-id`],style:{color:`#2d2006`}},{types:[`attr-name`],style:{color:`#896724`}},{types:[`boolean`,`string`,`entity`,`url`,`attr-value`,`keyword`,`control`,`directive`,`unit`,`statement`,`regex`,`atrule`],style:{color:`#728fcb`}},{types:[`placeholder`,`variable`],style:{color:`#93abdc`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`#896724`}}]},cv={plain:{color:`#393A34`,backgroundColor:`#f6f8fa`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`],style:{color:`#999988`,fontStyle:`italic`}},{types:[`namespace`],style:{opacity:.7}},{types:[`string`,`attr-value`],style:{color:`#e3116c`}},{types:[`punctuation`,`operator`],style:{color:`#393A34`}},{types:[`entity`,`url`,`symbol`,`number`,`boolean`,`variable`,`constant`,`property`,`regex`,`inserted`],style:{color:`#36acaa`}},{types:[`atrule`,`keyword`,`attr-name`,`selector`],style:{color:`#00a4db`}},{types:[`function`,`deleted`,`tag`],style:{color:`#d73a49`}},{types:[`function-variable`],style:{color:`#6f42c1`}},{types:[`tag`,`selector`,`keyword`],style:{color:`#00009f`}}]},lv={plain:{color:`#d6deeb`,backgroundColor:`#011627`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`,fontStyle:`italic`}},{types:[`inserted`,`attr-name`],style:{color:`rgb(173, 219, 103)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(99, 119, 119)`,fontStyle:`italic`}},{types:[`string`,`url`],style:{color:`rgb(173, 219, 103)`}},{types:[`variable`],style:{color:`rgb(214, 222, 235)`}},{types:[`number`],style:{color:`rgb(247, 140, 108)`}},{types:[`builtin`,`char`,`constant`,`function`],style:{color:`rgb(130, 170, 255)`}},{types:[`punctuation`],style:{color:`rgb(199, 146, 234)`}},{types:[`selector`,`doctype`],style:{color:`rgb(199, 146, 234)`,fontStyle:`italic`}},{types:[`class-name`],style:{color:`rgb(255, 203, 139)`}},{types:[`tag`,`operator`,`keyword`],style:{color:`rgb(127, 219, 202)`}},{types:[`boolean`],style:{color:`rgb(255, 88, 116)`}},{types:[`property`],style:{color:`rgb(128, 203, 196)`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`}}]},uv={plain:{color:`#403f53`,backgroundColor:`#FBFBFB`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`,fontStyle:`italic`}},{types:[`inserted`,`attr-name`],style:{color:`rgb(72, 118, 214)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(152, 159, 177)`,fontStyle:`italic`}},{types:[`string`,`builtin`,`char`,`constant`,`url`],style:{color:`rgb(72, 118, 214)`}},{types:[`variable`],style:{color:`rgb(201, 103, 101)`}},{types:[`number`],style:{color:`rgb(170, 9, 130)`}},{types:[`punctuation`],style:{color:`rgb(153, 76, 195)`}},{types:[`function`,`selector`,`doctype`],style:{color:`rgb(153, 76, 195)`,fontStyle:`italic`}},{types:[`class-name`],style:{color:`rgb(17, 17, 17)`}},{types:[`tag`],style:{color:`rgb(153, 76, 195)`}},{types:[`operator`,`property`,`keyword`,`namespace`],style:{color:`rgb(12, 150, 155)`}},{types:[`boolean`],style:{color:`rgb(188, 84, 84)`}}]},dv={char:`#D8DEE9`,comment:`#999999`,keyword:`#c5a5c5`,primitive:`#5a9bcf`,string:`#8dc891`,variable:`#d7deea`,boolean:`#ff8b50`,punctuation:`#5FB3B3`,tag:`#fc929e`,function:`#79b6f2`,className:`#FAC863`,method:`#6699CC`,operator:`#fc929e`},fv={plain:{backgroundColor:`#282c34`,color:`#ffffff`},styles:[{types:[`attr-name`],style:{color:dv.keyword}},{types:[`attr-value`],style:{color:dv.string}},{types:[`comment`,`block-comment`,`prolog`,`doctype`,`cdata`,`shebang`],style:{color:dv.comment}},{types:[`property`,`number`,`function-name`,`constant`,`symbol`,`deleted`],style:{color:dv.primitive}},{types:[`boolean`],style:{color:dv.boolean}},{types:[`tag`],style:{color:dv.tag}},{types:[`string`],style:{color:dv.string}},{types:[`punctuation`],style:{color:dv.string}},{types:[`selector`,`char`,`builtin`,`inserted`],style:{color:dv.char}},{types:[`function`],style:{color:dv.function}},{types:[`operator`,`entity`,`url`,`variable`],style:{color:dv.variable}},{types:[`keyword`],style:{color:dv.keyword}},{types:[`atrule`,`class-name`],style:{color:dv.className}},{types:[`important`],style:{fontWeight:`400`}},{types:[`bold`],style:{fontWeight:`bold`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`namespace`],style:{opacity:.7}}]},pv={plain:{color:`#f8f8f2`,backgroundColor:`#272822`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`#f92672`,fontStyle:`italic`}},{types:[`inserted`],style:{color:`rgb(173, 219, 103)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`#8292a2`,fontStyle:`italic`}},{types:[`string`,`url`],style:{color:`#a6e22e`}},{types:[`variable`],style:{color:`#f8f8f2`}},{types:[`number`],style:{color:`#ae81ff`}},{types:[`builtin`,`char`,`constant`,`function`,`class-name`],style:{color:`#e6db74`}},{types:[`punctuation`],style:{color:`#f8f8f2`}},{types:[`selector`,`doctype`],style:{color:`#a6e22e`,fontStyle:`italic`}},{types:[`tag`,`operator`,`keyword`],style:{color:`#66d9ef`}},{types:[`boolean`],style:{color:`#ae81ff`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`,opacity:.7}},{types:[`tag`,`property`],style:{color:`#f92672`}},{types:[`attr-name`],style:{color:`#a6e22e !important`}},{types:[`doctype`],style:{color:`#8292a2`}},{types:[`rule`],style:{color:`#e6db74`}}]},mv={plain:{color:`#bfc7d5`,backgroundColor:`#292d3e`},styles:[{types:[`comment`],style:{color:`rgb(105, 112, 152)`,fontStyle:`italic`}},{types:[`string`,`inserted`],style:{color:`rgb(195, 232, 141)`}},{types:[`number`],style:{color:`rgb(247, 140, 108)`}},{types:[`builtin`,`char`,`constant`,`function`],style:{color:`rgb(130, 170, 255)`}},{types:[`punctuation`,`selector`],style:{color:`rgb(199, 146, 234)`}},{types:[`variable`],style:{color:`rgb(191, 199, 213)`}},{types:[`class-name`,`attr-name`],style:{color:`rgb(255, 203, 107)`}},{types:[`tag`,`deleted`],style:{color:`rgb(255, 85, 114)`}},{types:[`operator`],style:{color:`rgb(137, 221, 255)`}},{types:[`boolean`],style:{color:`rgb(255, 88, 116)`}},{types:[`keyword`],style:{fontStyle:`italic`}},{types:[`doctype`],style:{color:`rgb(199, 146, 234)`,fontStyle:`italic`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`}},{types:[`url`],style:{color:`rgb(221, 221, 221)`}}]},hv={plain:{color:`#9EFEFF`,backgroundColor:`#2D2A55`},styles:[{types:[`changed`],style:{color:`rgb(255, 238, 128)`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`}},{types:[`inserted`],style:{color:`rgb(173, 219, 103)`}},{types:[`comment`],style:{color:`rgb(179, 98, 255)`,fontStyle:`italic`}},{types:[`punctuation`],style:{color:`rgb(255, 255, 255)`}},{types:[`constant`],style:{color:`rgb(255, 98, 140)`}},{types:[`string`,`url`],style:{color:`rgb(165, 255, 144)`}},{types:[`variable`],style:{color:`rgb(255, 238, 128)`}},{types:[`number`,`boolean`],style:{color:`rgb(255, 98, 140)`}},{types:[`attr-name`],style:{color:`rgb(255, 180, 84)`}},{types:[`keyword`,`operator`,`property`,`namespace`,`tag`,`selector`,`doctype`],style:{color:`rgb(255, 157, 0)`}},{types:[`builtin`,`char`,`constant`,`function`,`class-name`],style:{color:`rgb(250, 208, 0)`}}]},gv={plain:{backgroundColor:`linear-gradient(to bottom, #2a2139 75%, #34294f)`,backgroundImage:`#34294f`,color:`#f92aad`,textShadow:`0 0 2px #100c0f, 0 0 5px #dc078e33, 0 0 10px #fff3`},styles:[{types:[`comment`,`block-comment`,`prolog`,`doctype`,`cdata`],style:{color:`#495495`,fontStyle:`italic`}},{types:[`punctuation`],style:{color:`#ccc`}},{types:[`tag`,`attr-name`,`namespace`,`number`,`unit`,`hexcode`,`deleted`],style:{color:`#e2777a`}},{types:[`property`,`selector`],style:{color:`#72f1b8`,textShadow:`0 0 2px #100c0f, 0 0 10px #257c5575, 0 0 35px #21272475`}},{types:[`function-name`],style:{color:`#6196cc`}},{types:[`boolean`,`selector-id`,`function`],style:{color:`#fdfdfd`,textShadow:`0 0 2px #001716, 0 0 3px #03edf975, 0 0 5px #03edf975, 0 0 8px #03edf975`}},{types:[`class-name`,`maybe-class-name`,`builtin`],style:{color:`#fff5f6`,textShadow:`0 0 2px #000, 0 0 10px #fc1f2c75, 0 0 5px #fc1f2c75, 0 0 25px #fc1f2c75`}},{types:[`constant`,`symbol`],style:{color:`#f92aad`,textShadow:`0 0 2px #100c0f, 0 0 5px #dc078e33, 0 0 10px #fff3`}},{types:[`important`,`atrule`,`keyword`,`selector-class`],style:{color:`#f4eee4`,textShadow:`0 0 2px #393a33, 0 0 8px #f39f0575, 0 0 2px #f39f0575`}},{types:[`string`,`char`,`attr-value`,`regex`,`variable`],style:{color:`#f87c32`}},{types:[`parameter`],style:{fontStyle:`italic`}},{types:[`entity`,`url`],style:{color:`#67cdcc`}},{types:[`operator`],style:{color:`ffffffee`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`entity`],style:{cursor:`help`}},{types:[`inserted`],style:{color:`green`}}]},_v={plain:{color:`#282a2e`,backgroundColor:`#ffffff`},styles:[{types:[`comment`],style:{color:`rgb(197, 200, 198)`}},{types:[`string`,`number`,`builtin`,`variable`],style:{color:`rgb(150, 152, 150)`}},{types:[`class-name`,`function`,`tag`,`attr-name`],style:{color:`rgb(40, 42, 46)`}}]},vv={plain:{color:`#9CDCFE`,backgroundColor:`#1E1E1E`},styles:[{types:[`prolog`],style:{color:`rgb(0, 0, 128)`}},{types:[`comment`],style:{color:`rgb(106, 153, 85)`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`rgb(86, 156, 214)`}},{types:[`number`,`inserted`],style:{color:`rgb(181, 206, 168)`}},{types:[`constant`],style:{color:`rgb(100, 102, 149)`}},{types:[`attr-name`,`variable`],style:{color:`rgb(156, 220, 254)`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`rgb(206, 145, 120)`}},{types:[`selector`],style:{color:`rgb(215, 186, 125)`}},{types:[`tag`],style:{color:`rgb(78, 201, 176)`}},{types:[`tag`],languages:[`markup`],style:{color:`rgb(86, 156, 214)`}},{types:[`punctuation`,`operator`],style:{color:`rgb(212, 212, 212)`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`rgb(220, 220, 170)`}},{types:[`class-name`],style:{color:`rgb(78, 201, 176)`}},{types:[`char`],style:{color:`rgb(209, 105, 105)`}}]},yv={plain:{color:`#000000`,backgroundColor:`#ffffff`},styles:[{types:[`comment`],style:{color:`rgb(0, 128, 0)`}},{types:[`builtin`],style:{color:`rgb(0, 112, 193)`}},{types:[`number`,`variable`,`inserted`],style:{color:`rgb(9, 134, 88)`}},{types:[`operator`],style:{color:`rgb(0, 0, 0)`}},{types:[`constant`,`char`],style:{color:`rgb(129, 31, 63)`}},{types:[`tag`],style:{color:`rgb(128, 0, 0)`}},{types:[`attr-name`],style:{color:`rgb(255, 0, 0)`}},{types:[`deleted`,`string`],style:{color:`rgb(163, 21, 21)`}},{types:[`changed`,`punctuation`],style:{color:`rgb(4, 81, 165)`}},{types:[`function`,`keyword`],style:{color:`rgb(0, 0, 255)`}},{types:[`class-name`],style:{color:`rgb(38, 127, 153)`}}]},bv={plain:{color:`#f8fafc`,backgroundColor:`#011627`},styles:[{types:[`prolog`],style:{color:`#000080`}},{types:[`comment`],style:{color:`#6A9955`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`#569CD6`}},{types:[`number`,`inserted`],style:{color:`#B5CEA8`}},{types:[`constant`],style:{color:`#f8fafc`}},{types:[`attr-name`,`variable`],style:{color:`#9CDCFE`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`#cbd5e1`}},{types:[`selector`],style:{color:`#D7BA7D`}},{types:[`tag`],style:{color:`#0ea5e9`}},{types:[`tag`],languages:[`markup`],style:{color:`#0ea5e9`}},{types:[`punctuation`,`operator`],style:{color:`#D4D4D4`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`#7dd3fc`}},{types:[`class-name`],style:{color:`#0ea5e9`}},{types:[`char`],style:{color:`#D16969`}}]},xv={plain:{color:`#0f172a`,backgroundColor:`#f1f5f9`},styles:[{types:[`prolog`],style:{color:`#000080`}},{types:[`comment`],style:{color:`#6A9955`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`#0c4a6e`}},{types:[`number`,`inserted`],style:{color:`#B5CEA8`}},{types:[`constant`],style:{color:`#0f172a`}},{types:[`attr-name`,`variable`],style:{color:`#0c4a6e`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`#64748b`}},{types:[`selector`],style:{color:`#D7BA7D`}},{types:[`tag`],style:{color:`#0ea5e9`}},{types:[`tag`],languages:[`markup`],style:{color:`#0ea5e9`}},{types:[`punctuation`,`operator`],style:{color:`#475569`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`#0e7490`}},{types:[`class-name`],style:{color:`#0ea5e9`}},{types:[`char`],style:{color:`#D16969`}}]},Sv={plain:{backgroundColor:`hsl(220, 13%, 18%)`,color:`hsl(220, 14%, 71%)`,textShadow:`0 1px rgba(0, 0, 0, 0.3)`},styles:[{types:[`comment`,`prolog`,`cdata`],style:{color:`hsl(220, 10%, 40%)`}},{types:[`doctype`,`punctuation`,`entity`],style:{color:`hsl(220, 14%, 71%)`}},{types:[`attr-name`,`class-name`,`maybe-class-name`,`boolean`,`constant`,`number`,`atrule`],style:{color:`hsl(29, 54%, 61%)`}},{types:[`keyword`],style:{color:`hsl(286, 60%, 67%)`}},{types:[`property`,`tag`,`symbol`,`deleted`,`important`],style:{color:`hsl(355, 65%, 65%)`}},{types:[`selector`,`string`,`char`,`builtin`,`inserted`,`regex`,`attr-value`],style:{color:`hsl(95, 38%, 62%)`}},{types:[`variable`,`operator`,`function`],style:{color:`hsl(207, 82%, 66%)`}},{types:[`url`],style:{color:`hsl(187, 47%, 55%)`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`hsl(220, 14%, 71%)`}}]},Cv={plain:{backgroundColor:`hsl(230, 1%, 98%)`,color:`hsl(230, 8%, 24%)`},styles:[{types:[`comment`,`prolog`,`cdata`],style:{color:`hsl(230, 4%, 64%)`}},{types:[`doctype`,`punctuation`,`entity`],style:{color:`hsl(230, 8%, 24%)`}},{types:[`attr-name`,`class-name`,`boolean`,`constant`,`number`,`atrule`],style:{color:`hsl(35, 99%, 36%)`}},{types:[`keyword`],style:{color:`hsl(301, 63%, 40%)`}},{types:[`property`,`tag`,`symbol`,`deleted`,`important`],style:{color:`hsl(5, 74%, 59%)`}},{types:[`selector`,`string`,`char`,`builtin`,`inserted`,`regex`,`attr-value`,`punctuation`],style:{color:`hsl(119, 34%, 47%)`}},{types:[`variable`,`operator`,`function`],style:{color:`hsl(221, 87%, 60%)`}},{types:[`url`],style:{color:`hsl(198, 99%, 37%)`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`hsl(230, 8%, 24%)`}}]},wv={plain:{color:`#ebdbb2`,backgroundColor:`#292828`},styles:[{types:[`imports`,`class-name`,`maybe-class-name`,`constant`,`doctype`,`builtin`,`function`],style:{color:`#d8a657`}},{types:[`property-access`],style:{color:`#7daea3`}},{types:[`tag`],style:{color:`#e78a4e`}},{types:[`attr-name`,`char`,`url`,`regex`],style:{color:`#a9b665`}},{types:[`attr-value`,`string`],style:{color:`#89b482`}},{types:[`comment`,`prolog`,`cdata`,`operator`,`inserted`],style:{color:`#a89984`}},{types:[`delimiter`,`boolean`,`keyword`,`selector`,`important`,`atrule`,`property`,`variable`,`deleted`],style:{color:`#ea6962`}},{types:[`entity`,`number`,`symbol`],style:{color:`#d3869b`}}]},Tv={plain:{color:`#654735`,backgroundColor:`#f9f5d7`},styles:[{types:[`delimiter`,`boolean`,`keyword`,`selector`,`important`,`atrule`,`property`,`variable`,`deleted`],style:{color:`#af2528`}},{types:[`imports`,`class-name`,`maybe-class-name`,`constant`,`doctype`,`builtin`],style:{color:`#b4730e`}},{types:[`string`,`attr-value`],style:{color:`#477a5b`}},{types:[`property-access`],style:{color:`#266b79`}},{types:[`function`,`attr-name`,`char`,`url`],style:{color:`#72761e`}},{types:[`tag`],style:{color:`#b94c07`}},{types:[`comment`,`prolog`,`cdata`,`operator`,`inserted`],style:{color:`#a89984`}},{types:[`entity`,`number`,`symbol`],style:{color:`#924f79`}}]},Ev=e=>(0,x.useCallback)(t=>{var n=t,{className:r,style:i,line:a}=n;let o=$_(Q_({},ev(n,[`className`,`style`,`line`])),{className:z(`token-line`,r)});return typeof e==`object`&&`plain`in e&&(o.style=e.plain),typeof i==`object`&&(o.style=Q_(Q_({},o.style||{}),i)),o},[e]),Dv=e=>{let t=(0,x.useCallback)(({types:t,empty:n})=>{if(e!=null)return t.length===1&&t[0]===`plain`?n==null?void 0:{display:`inline-block`}:t.length===1&&n!=null?e[t[0]]:Object.assign(n==null?{}:{display:`inline-block`},...t.map(t=>e[t]))},[e]);return(0,x.useCallback)(e=>{var n=e,{token:r,className:i,style:a}=n;let o=$_(Q_({},ev(n,[`token`,`className`,`style`])),{className:z(`token`,...r.types,i),children:r.content,style:t(r)});return a!=null&&(o.style=Q_(Q_({},o.style||{}),a)),o},[t])},Ov=/\r\n|\r|\n/,kv=e=>{e.length===0?e.push({types:[`plain`],content:`
`,empty:!0}):e.length===1&&e[0].content===``&&(e[0].content=`
`,e[0].empty=!0)},Av=(e,t)=>{let n=e.length;return n>0&&e[n-1]===t?e:e.concat(t)},jv=e=>{let t=[[]],n=[e],r=[0],i=[e.length],a=0,o=0,s=[],c=[s];for(;o>-1;){for(;(a=r[o]++)<i[o];){let e,l=t[o],u=n[o][a];if(typeof u==`string`?(l=o>0?l:[`plain`],e=u):(l=Av(l,u.type),u.alias&&(l=Av(l,u.alias)),e=u.content),typeof e!=`string`){o++,t.push(l),n.push(e),r.push(0),i.push(e.length);continue}let d=e.split(Ov),f=d.length;s.push({types:l,content:d[0]});for(let e=1;e<f;e++)kv(s),c.push(s=[]),s.push({types:l,content:d[e]})}o--,t.pop(),n.pop(),r.pop(),i.pop()}return kv(s),c},Mv=({prism:e,code:t,grammar:n,language:r})=>(0,x.useMemo)(()=>{if(n==null)return jv([t]);let i={code:t,grammar:n,language:r,tokens:[]};return e.hooks.run(`before-tokenize`,i),i.tokens=e.tokenize(t,n),e.hooks.run(`after-tokenize`,i),jv(i.tokens)},[t,n,r,e]),Nv=(e,t)=>{let{plain:n}=e,r=e.styles.reduce((e,n)=>{let{languages:r,style:i}=n;return r&&!r.includes(t)||n.types.forEach(t=>{e[t]=Q_(Q_({},e[t]),i)}),e},{});return r.root=n,r.plain=$_(Q_({},n),{backgroundColor:void 0}),r},Pv=({children:e,language:t,code:n,theme:r,prism:i})=>{let a=t.toLowerCase(),o=Nv(r,a),s=Ev(o),c=Dv(o),l=i.languages[a];return e({tokens:Mv({prism:i,language:a,code:n,grammar:l}),className:`prism-code language-${a}`,style:o==null?{}:o.root,getLineProps:s,getTokenProps:c})},Fv=e=>(0,x.createElement)(Pv,$_(Q_({},e),{prism:e.prism||X,theme:e.theme||vv,code:e.code,language:e.language})),Iv=Rl((0,L.jsx)(`path`,{d:`M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2m0 16H8V7h11z`}),`ContentCopy`),Lv={wrapper:{borderRadius:1,overflow:`hidden`,border:1,borderColor:`divider`,bgcolor:`background.paper`},header:{display:`flex`,alignItems:`center`,justifyContent:`space-between`,px:1.5,py:.5,borderBottom:1,borderColor:`divider`,bgcolor:`rgba(255,255,255,0.02)`},headerNoTabs:{display:`flex`,alignItems:`center`,justifyContent:`flex-end`,px:1.5,py:.5,borderBottom:1,borderColor:`divider`,bgcolor:`rgba(255,255,255,0.02)`},tabs:{display:`flex`,alignItems:`center`,gap:.5},tab:{px:1,py:.25,borderRadius:999,fontSize:12,color:`text.secondary`,cursor:`pointer`},tabActive:{px:1,py:.25,borderRadius:999,fontSize:12,bgcolor:`primary.main`,color:`#0b0f1a`,cursor:`pointer`},copyBtn:{color:`text.secondary`},body:{p:0,bgcolor:`#21252e`},preCustom:{margin:0,padding:10}};const Z=({code:e,codeRuntime:t,codeEditor:n,language:r=`tsx`})=>{let[i,a]=(0,x.useState)(!1),o=(0,x.useCallback)(()=>{let r=(t??n??e??``).trim();navigator.clipboard.writeText(r).then(()=>{a(!0),setTimeout(()=>a(!1),1200)})},[e,t,n]),s=!!(t||n),[c,l]=(0,x.useState)(t?`runtime`:`editor`),u=(()=>s?c===`runtime`?(t??n??``).trim():(n??t??``).trim():(e??``).trim())();return(0,L.jsxs)(W,{sx:Lv.wrapper,children:[(0,L.jsxs)(W,{sx:s?Lv.header:Lv.headerNoTabs,children:[s&&(0,L.jsxs)(W,{sx:Lv.tabs,children:[t&&(0,L.jsx)(W,{sx:c===`runtime`?Lv.tabActive:Lv.tab,onClick:()=>l(`runtime`),children:(0,L.jsx)(U,{variant:`caption`,children:`Runtime`})}),n&&(0,L.jsx)(W,{sx:c===`editor`?Lv.tabActive:Lv.tab,onClick:()=>l(`editor`),children:(0,L.jsx)(U,{variant:`caption`,children:`Editor`})})]}),(0,L.jsx)(n_,{title:i?`Copied`:`Copy`,children:(0,L.jsx)(Ud,{size:`small`,onClick:o,sx:Lv.copyBtn,children:(0,L.jsx)(Iv,{fontSize:`inherit`})})})]}),(0,L.jsx)(W,{sx:Lv.body,children:(0,L.jsx)(Fv,{theme:iv.oneDark,code:u,language:r,children:({className:e,style:t,tokens:n,getLineProps:r,getTokenProps:i})=>(0,L.jsx)(`pre`,{className:e,style:{...t,...Lv.preCustom},children:n.map((e,t)=>(0,L.jsx)(`div`,{...r({line:e}),children:e.map((e,t)=>(0,L.jsx)(`span`,{...i({token:e})},t))},t))})})})]})};var Rv={root:{}};const zv=()=>(0,L.jsxs)(W,{sx:Rv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Getting Started`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Supported Unity versions: `,(0,L.jsx)(`strong`,{children:`Unity 6.2+`})]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Install via Unity Package Manager`}),(0,L.jsxs)(G,{children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Open Package Manager in Unity.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Add package from Git URL:`})})]}),(0,L.jsx)(Z,{language:`tsx`,code:`https://github.com/yanivkalfa/ReactiveUIToolKit.git#dist`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Hello World (Editor)`}),(0,L.jsx)(Z,{language:`tsx`,codeEditor:`using UnityEditor;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Props.Typed;

// EditorWindow sample (C#)
[MenuItem("Window/ReactiveUITK/Hello World")]
static void Open() {
  var w = GetWindow<EditorWindow>("Hello");
  EditorRootRendererUtility.Render(
    w.rootVisualElement,
    V.VisualElement(null, null,
      V.Label(new LabelProps { Text = "Hello ReactiveUITK" })
    )
  );
}`,codeRuntime:`// Runtime MonoBehaviour with RootRenderer (C#)
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public sealed class HelloRuntime : MonoBehaviour
{
  [SerializeField] private UIDocument uiDocument;

  private RootRenderer _rootRenderer;

  private void Awake()
  {
    if (uiDocument == null)
    {
      Debug.LogError("Assign UIDocument on HelloRuntime");
      return;
    }

    // Create / reuse a RootRenderer in the scene
    _rootRenderer = FindObjectOfType<RootRenderer>();
    if (_rootRenderer == null)
    {
      _rootRenderer = new GameObject("ReactiveUIRoot").AddComponent<RootRenderer>();
    }

    _rootRenderer.Initialize(uiDocument.rootVisualElement);

    // Render a simple VNode tree
    var vnode = V.VisualElement(
      null,
      null,
      V.Label(new LabelProps { Text = "Hello ReactiveUITK (Runtime)" })
    );

    _rootRenderer.Render(vnode);
  }
}`})]});var Bv={root:{display:`flex`,flexDirection:`column`,gap:2},list:{pl:2}};const Vv=()=>(0,L.jsxs)(W,{sx:Bv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Router`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit includes a lightweight, in-memory router inspired by React Router. It routes based on the current path and lets you nest routes and links inside your `,(0,L.jsx)(`code`,{children:`VirtualNode`}),` `,`tree.`]}),(0,L.jsxs)(W,{children:[(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Core concepts`}),(0,L.jsxs)(G,{sx:Bv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Use `,(0,L.jsx)(`code`,{children:`V.Router(...)`}),` at the root of a subtree to set up routing context and history.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Use `,(0,L.jsx)(`code`,{children:`V.Route(path, exact, element, children)`}),` to match the current path and decide what to render.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Use `,(0,L.jsx)(`code`,{children:`V.Link`}),` and `,(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigate(replace)`}),` to perform navigation from code or UI.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Use `,(0,L.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),`, `,(0,L.jsx)(`code`,{children:`RouterHooks.UseParams()`}),`, and`,` `,(0,L.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` to access path, parameters, and query-string values.`]})})})]})]}),(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Basic example`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The example below shows the same router tree hosted in an editor window and in a runtime function component. Inside the matched routes you can use `,(0,L.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),` `,`and `,(0,L.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` to read the active path and parameters.`]}),(0,L.jsx)(Z,{language:`tsx`,codeEditor:`using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Elements;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Props.Typed.EditorRootRendererUtility;
using ReactiveUITK.Router;
using ReactiveUITK.EditorSupport;
using UnityEditor;
using UnityEngine.UIElements;

// EditorWindow with Router
[MenuItem("Window/ReactiveUITK/Router Demo")]
public static void Open()
{
  var window = GetWindow<EditorWindow>("Router Demo");

  Render(
    window.rootVisualElement,
    V.Router(
      children: new[]
      {
        V.VisualElement(
          new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.MarginBottom, 6f) },
          null,
          V.Link("/", "Home"),
          V.Link("/about", "About"),
          V.Link("/users/42", "User 42")
        ),
        V.Route(path: "/", exact: true, element: V.Text("Home route")),
        V.Route(path: "/about", element: V.Text("About route")),
        V.Route(
          path: "/users/:id",
          children: new[] { V.Func(UserProfileFunc.Render) }
        ),
        V.Route(path: "*", element: V.Text("Not found")),
      }
    )
  );
}`,codeRuntime:`using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Elements;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;

// Function component using Router in runtime
public static class RouterDemoFunc
{
  private static readonly Style LinkBarStyle = new Style
  {
    (StyleKeys.FlexDirection, "row"),
    (StyleKeys.MarginBottom, 6f),
  };

  // Function component entrypoint �?" pass RouterDemoFunc.Render
  // directly to V.Func when mounting.
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Router(
      children: new[]
      {
        V.VisualElement(
          LinkBarStyle,
          null,
          V.Link("/", "Home"),
          V.Link("/about", "About"),
          V.Link("/users/42", "User 42")
        ),
        V.Route(path: "/", exact: true, element: V.Text("Home route")),
        V.Route(path: "/about", element: V.Text("About route")),
        V.Route(
          path: "/users/:id",
          children: new[] { V.Func(UserProfile) }
        ),
        V.Route(path: "*", element: V.Text("Not found")),
      }
    );
  }
}

// Mounted through RootRenderer elsewhere:
// rootRenderer.Render(V.Func(RouterDemoFunc.Example));`}),(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Navigation and history`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`By default `,(0,L.jsx)(`code`,{children:`V.Router`}),` uses an in-memory history implementation. You can provide a custom `,(0,L.jsx)(`code`,{children:`IRouterHistory`}),` instance if you want to control how locations are stored or synchronized. Inside components, use `,(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),` to push or replace locations, and `,(0,L.jsx)(`code`,{children:`RouterHooks.UseGo()`}),` / `,(0,L.jsx)(`code`,{children:`RouterHooks.UseCanGo()`}),` to implement back/forward UI. You can also use `,(0,L.jsx)(`code`,{children:`RouterHooks.UseBlocker()`}),` to prevent navigation while a confirmation dialog is open.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Links and route data`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use `,(0,L.jsx)(`code`,{children:`V.Link`}),` to render navigation buttons bound to specific paths. Inside routed components, use `,(0,L.jsx)(`code`,{children:`RouterHooks.UseLocationInfo()`}),` for the full location payload,`,(0,L.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` for path parameters, `,(0,L.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` `,`for query-string values, and `,(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` for any state object passed when navigating.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Links, params, query, and state (example)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The example below demonstrates how to combine `,(0,L.jsx)(`code`,{children:`V.Link`}),`,`,` `,(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),`, `,(0,L.jsx)(`code`,{children:`RouterHooks.UseGo()`}),`,`,` `,(0,L.jsx)(`code`,{children:`RouterHooks.UseParams()`}),`, `,(0,L.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),`, and`,` `,(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` to build a small navigation bar that can move back and forth and read route data.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;

// Demonstrates links, programmatic navigation, params, query, and state.
public static class RouterLinksFunc
{
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var navigate = RouterHooks.UseNavigate();
    var go = RouterHooks.UseGo();
    bool canBack = RouterHooks.UseCanGo(-1);

    var location = RouterHooks.UseLocationInfo();
    var routeMatch = RouterHooks.UseRouteMatch();
    var parameters = RouterHooks.UseParams();
    var query = RouterHooks.UseQuery();
    var navState = RouterHooks.UseNavigationState();

    void ToUser42()
    {
      // Push a new location and attach a small state payload
      navigate("/users/42?tab=details", new { from = "nav-button" });
    }

    void GoBack()
    {
      go(-1);
    }

    string userId = parameters.TryGetValue("id", out var id) ? id : "(none)";

    return V.Column(
      key: null,
      V.Row(
        key: "links",
        V.Link("/", "Home"),
        V.Link("/about", "About"),
        V.Link("/users/42?tab=details", "User 42 (details)")
      ),
      V.Row(
        key: "actions",
        V.Button(new ButtonProps { Text = "To User 42 (code)", OnClick = ToUser42 }),
        V.Button(new ButtonProps { Text = "Back", Enabled = canBack, OnClick = GoBack })
      ),
      V.Label(new LabelProps { Text = $"Path: {location?.Path}" }),
      V.Label(new LabelProps { Text = $"User id param: {userId}" }),
      V.Label(new LabelProps { Text = $"Query keys: {string.Join(\\", \\", query.Keys)}" }),
      V.Label(new LabelProps { Text = $"Nav state type: {navState?.GetType().Name ?? \\"(none)\\"}" })
    );
  }
}`}),(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Split layouts with nested routes`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`You can keep a single router history while nesting routes to act like “outlets”. Child routes may use relative paths (for example “profile”), and we automatically resolve them against the parent match. When you use a relative route, it is prefixed with the parent route’s path before matching, so patterns like `,(0,L.jsx)(`code`,{children:`:id/edit`}),` work the same way they do in React Router—no need to repeat the parent prefix.`]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The example below matches `,(0,L.jsx)(`code`,{children:`/mainMenu/*`}),`, renders a sidebar, and nests additional`,` `,(0,L.jsx)(`code`,{children:`V.Route`}),` elements so the right-hand panel switches content as the path changes. The sidebar buttons simply call `,(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),` with relative targets, and the router keeps everything in sync without spinning up another router.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;

public static class SplitShellDemo
{
  private static readonly Style Shell = new()
  {
    (StyleKeys.FlexGrow, 1f),
    (StyleKeys.FlexDirection, "column"),
    (StyleKeys.Padding, 12f),
  };

  private static readonly Style ContentRow = new()
  {
    (StyleKeys.FlexGrow, 1f),
    (StyleKeys.FlexDirection, "row"),
    (StyleKeys.MarginTop, 8f),
  };

  private static readonly Style Sidebar = new()
  {
    (StyleKeys.Width, 220f),
    (StyleKeys.FlexDirection, "column"),
    (StyleKeys.Padding, 10f),
    (StyleKeys.BorderWidth, 1f),
    (StyleKeys.BorderRadius, 6f),
  };

  private static readonly Style Outlet = new()
  {
    (StyleKeys.FlexGrow, 1f),
    (StyleKeys.MarginLeft, 12f),
    (StyleKeys.Padding, 12f),
    (StyleKeys.BorderWidth, 1f),
    (StyleKeys.BorderRadius, 6f),
  };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Router(
      children: new[]
      {
        BuildNavRow(),
        V.Route(path: "/", exact: true, element: V.Text("Landing route")),
        V.Route(path: "/mainMenu/*", children: new[] { V.Func(MainMenuLayout) }),
        V.Route(path: "*", element: V.Text("Not found")),
      }
    );
  }

  private static VirtualNode BuildNavRow()
  {
    var navigate = RouterHooks.UseNavigate();
    return V.VisualElement(
      new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.MarginBottom, 4f) },
      null,
      V.Button(new ButtonProps { Text = "Home (/)", OnClick = () => navigate("/") }),
      V.Button(new ButtonProps { Text = "Open Main Menu", OnClick = () => navigate("/mainMenu") })
    );
  }

  private static VirtualNode MainMenuLayout(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var location = RouterHooks.UseLocationInfo();
    var navigate = RouterHooks.UseNavigate();
    return V.VisualElement(
      ContentRow,
      null,
      V.VisualElement(
        Sidebar,
        null,
        V.Text("Sidebar"),
        V.Button(new ButtonProps { Text = "Home", OnClick = () => navigate(string.Empty) }),
        V.Button(new ButtonProps { Text = "Profile", OnClick = () => navigate("profile") }),
        V.Button(new ButtonProps { Text = "Store", OnClick = () => navigate("store") }),
        V.Button(new ButtonProps { Text = "Settings", OnClick = () => navigate("settings") })
      ),
      V.VisualElement(
        Outlet,
        null,
        V.Text($"Outlet (current path: {location?.Path ?? "/"})"),
        V.Route(path: string.Empty, exact: true, element: V.Text("Pick a submenu from the left.")),
        V.Route(path: ":id/edit", element: V.Text("Editing view with route params")),
        V.Route(path: "profile", element: V.Text("Profile content")),
        V.Route(path: "store", element: V.Text("Store content")),
        V.Route(path: "settings", element: V.Text("Settings content"))
      )
    );
  }
}
`})]});var Hv={root:{display:`flex`,flexDirection:`column`,gap:2},list:{pl:2}};const Uv=()=>(0,L.jsxs)(W,{sx:Hv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Signals`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`Signals`}),` are lightweight, named reactive values that live in a process-wide registry. They behave like a small observable store with a simple API and are ideal whenever you want a single source of truth with a single point of entry for reading and updating state (for example: selection, filters, or global preferences).`]}),(0,L.jsxs)(W,{children:[(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Concepts`}),(0,L.jsxs)(G,{sx:Hv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`Signals`}),` live in a global registry keyed by `,(0,L.jsx)(`code`,{children:`string`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Call `,(0,L.jsx)(`code`,{children:`Signals.Get<T>(key, initialValue)`}),` to create or return a`,` `,(0,L.jsx)(`code`,{children:`Signal<T>`}),` instance.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Call `,(0,L.jsx)(`code`,{children:`signal.Subscribe(...)`}),` to watch changes outside of components; use`,` `,(0,L.jsx)(`code`,{children:`Hooks.UseSignal(...)`}),` inside function components.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Use `,(0,L.jsx)(`code`,{children:`Dispatch(prev => next)`}),` or `,(0,L.jsx)(`code`,{children:`Dispatch(value)`}),` to update the value and notify listeners.`]})})})]})]}),(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Runtime usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`using System;
using ReactiveUITK.Signals;
using UnityEngine;

// Runtime: global signal and subscription

public sealed class SignalsDemo : MonoBehaviour
{
  private IDisposable _subscription;

  private void Start()
  {
    // Ensure the runtime host exists
    SignalsRuntime.EnsureInitialized();

    var counter = Signals.Get<int>("demo-counter", 0);
    _subscription = counter.Subscribe(v => Debug.Log($"Counter changed to {v}"));

    // Update via functional Dispatch using previous value
    counter.Dispatch(previous => previous + 1);

    // Or assign a value directly
    counter.Dispatch(42);
  }

  private void OnDestroy()
  {
    _subscription?.Dispose();
  }
}`}),(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Using signals from components`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Inside function components, use `,(0,L.jsx)(`code`,{children:`Hooks.UseSignal`}),` or the selector overload`,` `,(0,L.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(...)`}),` to read a signal and re-render when it changes. The example below shows a simple counter bound to the global `,(0,L.jsx)(`code`,{children:`demo-counter`}),` `,`signal, but you can also project a slice of a more complex signal value and compare with a custom equality comparer for performance.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Signals;

// Function component bound to a signal
public static class SignalCounterFunc
{
  // Function component – pass SignalCounterFunc.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    // Reads and subscribes to the signal by key
    int value = Hooks.UseSignal<int>("demo-counter", initialValue: 0);

    void Increment()
    {
      var signal = Signals.Get<int>("demo-counter", 0);
      signal.Dispatch(previous => previous + 1);
    }

    return V.Row(
      key: null,
      V.Label(new LabelProps { Text = $"Value: {value}" }),
      V.Button(new ButtonProps { Text = "Increment", OnClick = Increment })
    );
  }
}`})]});var Wv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const Gv=()=>(0,L.jsxs)(W,{sx:Wv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Concepts & Environment`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit aims to feel familiar if you know React, while still fitting naturally into Unity's UI Toolkit and C# ecosystem. You build trees from `,(0,L.jsx)(`code`,{children:`V.*`}),` helpers and function components, use hooks to manage state, and let the reconciler diff and update the underlying `,(0,L.jsx)(`code`,{children:`VisualElement`}),` hierarchy for you.`]}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`Where Unity or UI Toolkit impose different constraints (for example: layout system, event model, or platform concerns), the library deliberately diverges from React to provide a more idiomatic Unity experience. The routing, signals, and safe-area helpers are examples of features that don't exist in core React but are important here.`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The package also ships with a rich demo set under `,(0,L.jsx)(`code`,{children:`Assets/ReactiveUIToolKit/Samples`}),` `,`(editor windows and runtime scenes) that you can import into your project. These demos show real-world usage of components, hooks, routing, signals, and more, and are a great way to see the concepts on this page in action.`]}),(0,L.jsxs)(W,{sx:Wv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Scripting define symbols (environment & tracing)`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Set these in `,(0,L.jsx)(`strong`,{children:`Project Settings → Player → Scripting Define Symbols`}),`. They control environment labels and diagnostics at compile time.`]}),(0,L.jsxs)(G,{sx:Wv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ENV_DEV`}),` — development environment. Enables dev-oriented defaults such as Basic trace level and compiles editor diagnostics helpers.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ENV_STAGING`}),` — staging environment label (no implicit tracing changes).`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ENV_PROD`}),` — production environment label. This is the implied default if no `,(0,L.jsx)(`code`,{children:`ENV_*`}),` symbol is defined.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RUITK_TRACE_VERBOSE`}),` — force reconciler trace level to`,` `,(0,L.jsx)(`strong`,{children:`Verbose`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RUITK_TRACE_BASIC`}),` — force reconciler trace level to`,` `,(0,L.jsx)(`strong`,{children:`Basic`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RUITK_DIFF_TRACING`}),` — force`,` `,(0,L.jsx)(`code`,{children:`DiagnosticsConfig.EnableDiffTracing`}),` to `,(0,L.jsx)(`code`,{children:`true`}),` for detailed Fiber diff diagnostics.`]})})})]}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,sx:Wv.section,children:(0,L.jsx)(`strong`,{children:`Behavior summary`})}),(0,L.jsxs)(G,{sx:Wv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Environment is resolved to `,(0,L.jsx)(`code`,{children:`development`}),`, `,(0,L.jsx)(`code`,{children:`staging`}),`, or`,` `,(0,L.jsx)(`code`,{children:`production`}),` via the `,(0,L.jsx)(`code`,{children:`ENV_*`}),` defines and is exposed at runtime as `,(0,L.jsx)(`code`,{children:`HostContext.Environment["env"]`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Trace level resolution priority:`,` `,(0,L.jsx)(`code`,{children:`RUITK_TRACE_VERBOSE`}),` > `,(0,L.jsx)(`code`,{children:`RUITK_TRACE_BASIC`}),` >`,` `,(0,L.jsx)(`code`,{children:`ENV_DEV`}),` (Basic) > none.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Editor-only diagnostic utilities are compiled only when ENV_DEV is defined.`})})]})]})]});var Kv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const qv=()=>(0,L.jsxs)(W,{sx:Kv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Different from React`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`ReactiveUIToolKit feels familiar if you know React, but there are important differences in how rendering and scheduling behave when you are working in C# and Unity instead of JavaScript and the browser. This section focuses on the places where your mental model should be adjusted rather than re-explaining core concepts.`}),(0,L.jsxs)(W,{sx:Kv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`State updates with UseState (parity)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`Hooks.UseState`}),` matches React's mental model: you get a value and a setter, and you can call the setter with either a value or a function of the previous value (for example `,(0,L.jsx)(`code`,{children:`set(value)`}),` or `,(0,L.jsx)(`code`,{children:`set(prev => next)`}),`).`]}),(0,L.jsxs)(G,{sx:Kv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`The setter is a delegate (`,(0,L.jsx)(`code`,{children:`StateSetter<T>`}),`), not an instance method, but you call it just like a normal function.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`You can either call `,(0,L.jsx)(`code`,{children:`set(value)`}),` / `,(0,L.jsx)(`code`,{children:`set(prev => next)`}),` `,`(React-style) or use the optional extension helpers`,` `,(0,L.jsx)(`code`,{children:`StateSetterExtensions.Set(value)`}),` /`,` `,(0,L.jsx)(`code`,{children:`StateSetterExtensions.Set(prev => next)`}),` if you prefer a fluent style.`]})})})]}),(0,L.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

// Function component with UseState
public static VirtualNode CounterFunc(
  Dictionary<string, object> props,
  IReadOnlyList<VirtualNode> children
)
{
  var (count, setCount) = Hooks.UseState(0);

  // Direct value update
  void Reset() => setCount(0);

  // Functional update using previous value
  void Increment() => setCount(previous => previous + 1);

  return V.Column(
    key: null,
    V.Label(new LabelProps { Text = $"Count: {count}" }),
    V.Button(new ButtonProps { Text = "Increment", OnClick = Increment }),
    V.Button(new ButtonProps { Text = "Reset", OnClick = Reset })
  );
}`})]}),(0,L.jsxs)(W,{sx:Kv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Sync rendering vs React concurrent mode`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit's Fiber reconciler currently runs in a single, synchronous mode per Unity frame. There is no React 18-style concurrent rendering yet: no`,` `,(0,L.jsx)(`code`,{children:`startTransition`}),`, no transition priorities, and no cooperative time-slicing of large trees.`]}),(0,L.jsxs)(G,{sx:Kv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsx)(L.Fragment,{children:`All updates scheduled in a frame are processed synchronously; there is no partial rendering or preemption between high- and low-priority updates.`})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsx)(L.Fragment,{children:`This behaves like legacy React (pre-18) "sync mode": your components and hooks logic are the same, but you should not expect concurrent features such as transitions or suspenseful background rendering.`})})})]})]})]});var Jv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const Yv=()=>(0,L.jsxs)(W,{sx:Jv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`API Reference`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`This section gives a high-level map of the main namespaces and types you will use when working with ReactiveUIToolKit. Use it as a guide when you are looking for where a particular class (for example `,(0,L.jsx)(`code`,{children:`ButtonProps`}),`) lives.`]}),(0,L.jsxs)(W,{sx:Jv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Core`}),(0,L.jsxs)(G,{sx:Jv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Core.V`}),` – static factory for building`,` `,(0,L.jsx)(`code`,{children:`VirtualNode`}),` trees (for example `,(0,L.jsx)(`code`,{children:`V.VisualElement`}),`,`,` `,(0,L.jsx)(`code`,{children:`V.VisualElementSafe`}),`, `,(0,L.jsx)(`code`,{children:`V.Label`}),`, `,(0,L.jsx)(`code`,{children:`V.Button`}),`,`,` `,(0,L.jsx)(`code`,{children:`V.Router`}),`, `,(0,L.jsx)(`code`,{children:`V.TabView`}),`).`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Core.Hooks`}),` – hook functions for function components, such as `,(0,L.jsx)(`code`,{children:`UseState`}),`, `,(0,L.jsx)(`code`,{children:`UseReducer`}),`, `,(0,L.jsx)(`code`,{children:`UseEffect`}),`,`,` `,(0,L.jsx)(`code`,{children:`UseMemo`}),`, `,(0,L.jsx)(`code`,{children:`UseSignal`}),`, and context helpers.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Core.StateSetterExtensions`}),` – helpers for working with state setters (for example `,(0,L.jsx)(`code`,{children:`set.Set(value)`}),` /`,` `,(0,L.jsx)(`code`,{children:`set.Set(prev => next)`}),`).`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Core.RootRenderer`}),` – runtime component that mounts a`,` `,(0,L.jsx)(`code`,{children:`VirtualNode`}),` tree into a `,(0,L.jsx)(`code`,{children:`UIDocument`}),` root.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Core.RenderScheduler`}),` – runtime scheduler used by the reconciler to batch updates per frame.`]})})})]})]}),(0,L.jsxs)(W,{sx:Jv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props & Styles`}),(0,L.jsxs)(G,{sx:Jv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Props.Typed`}),` – typed props for UI Toolkit controls. Each control has a corresponding `,(0,L.jsx)(`code`,{children:`*Props`}),` class (for example`,` `,(0,L.jsx)(`code`,{children:`ButtonProps`}),`, `,(0,L.jsx)(`code`,{children:`LabelProps`}),`, `,(0,L.jsx)(`code`,{children:`ListViewProps`}),`,`,` `,(0,L.jsx)(`code`,{children:`ScrollViewProps`}),`).`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Props.Typed.Style`}),` – strongly typed wrapper around a style dictionary used by many props (`,(0,L.jsx)(`code`,{children:`Style`}),` is often passed as`,` `,(0,L.jsx)(`code`,{children:`props.Style`}),`).`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Props.Typed.StyleKeys`}),` – constants used as keys inside`,` `,(0,L.jsx)(`code`,{children:`Style`}),` (for example `,(0,L.jsx)(`code`,{children:`StyleKeys.MarginTop`}),`,`,` `,(0,L.jsx)(`code`,{children:`StyleKeys.FlexDirection`}),`).`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Most field and layout controls follow the same pattern:`,(0,L.jsx)(`code`,{children:`V.FloatField(new FloatFieldProps { ... })`}),`,`,` `,(0,L.jsx)(`code`,{children:`V.ListView(new ListViewProps { ... })`}),`, and so on.`]})})})]})]}),(0,L.jsxs)(W,{sx:Jv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Router`}),(0,L.jsxs)(G,{sx:Jv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Router.RouterHooks`}),` – hook helpers for routing:`,` `,(0,L.jsx)(`code`,{children:`UseRouter()`}),`, `,(0,L.jsx)(`code`,{children:`UseLocation()`}),`, `,(0,L.jsx)(`code`,{children:`UseLocationInfo()`}),`, `,(0,L.jsx)(`code`,{children:`UseParams()`}),`, `,(0,L.jsx)(`code`,{children:`UseQuery()`}),`,`,` `,(0,L.jsx)(`code`,{children:`UseNavigationState()`}),`, `,(0,L.jsx)(`code`,{children:`UseNavigate()`}),`, `,(0,L.jsx)(`code`,{children:`UseGo()`}),`, `,(0,L.jsx)(`code`,{children:`UseCanGo()`}),`, `,(0,L.jsx)(`code`,{children:`UseBlocker()`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Router.IRouterHistory`}),`, `,(0,L.jsx)(`code`,{children:`MemoryHistory`}),` – the history abstraction used by `,(0,L.jsx)(`code`,{children:`V.Router`}),`. You can supply your own history implementation by passing an `,(0,L.jsx)(`code`,{children:`IRouterHistory`}),` instance to`,` `,(0,L.jsx)(`code`,{children:`V.Router`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Router.RouterLocation`}),`, `,(0,L.jsx)(`code`,{children:`RouterPath`}),`,`,` `,(0,L.jsx)(`code`,{children:`RouteMatch`}),` – types that describe the current location, parsed path, and the result of route matching.`]})})})]})]}),(0,L.jsxs)(W,{sx:Jv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signals`}),(0,L.jsxs)(G,{sx:Jv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Signals.Signals`}),` – entry point for working with signals via`,` `,(0,L.jsx)(`code`,{children:`Signals.Get<T>(key, initialValue)`}),` and`,` `,(0,L.jsx)(`code`,{children:`Signals.TryGet<T>(key, out signal)`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Signals.Signal<T>`}),` – concrete signal type with`,` `,(0,L.jsx)(`code`,{children:`Value`}),`, `,(0,L.jsx)(`code`,{children:`Subscribe(...)`}),`, `,(0,L.jsx)(`code`,{children:`Set(value)`}),`, and`,` `,(0,L.jsx)(`code`,{children:`Dispatch(update)`}),` methods.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Signals.SignalsRuntime`}),` – bootstraps the runtime registry and hidden host GameObject. Call `,(0,L.jsx)(`code`,{children:`SignalsRuntime.EnsureInitialized()`}),` at startup if you are using signals outside of components.`]})})})]})]}),(0,L.jsxs)(W,{sx:Jv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Editor support`}),(0,L.jsxs)(G,{sx:Jv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.EditorSupport.EditorRootRendererUtility`}),` – helper for mounting a `,(0,L.jsx)(`code`,{children:`VirtualNode`}),` tree into an EditorWindow`,` `,(0,L.jsx)(`code`,{children:`VisualElement`}),`. Used from editor samples and your own tools.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.EditorSupport.EditorRenderScheduler`}),` – scheduler used in the editor for batched updates.`]})})})]})]}),(0,L.jsxs)(W,{sx:Jv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Elements & registry`}),(0,L.jsxs)(G,{sx:Jv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Elements.ElementRegistry`}),` – maps element names (for example`,` `,(0,L.jsx)(`code`,{children:`"Button"`}),`, `,(0,L.jsx)(`code`,{children:`"ListView"`}),`) to concrete adapters and is used by the reconciler when creating and updating UI Toolkit elements.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ReactiveUITK.Elements.ElementRegistryProvider`}),` – static helpers for obtaining the default registry used by both runtime and editor hosts.`]})})})]})]})]}),Xv={AnimateProps:`using System.Collections.Generic;
using ReactiveUITK.Core.Animation;

namespace ReactiveUITK.Props.Typed
{
    public sealed class AnimateProps : BaseProps
    {
        public List<AnimateTrack> Tracks { get; set; }
        public bool Autoplay { get; set; } = true;
    }
}`,BaseProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Base class for all Props that wrap a VisualElement.
    /// Provides the common set of properties that every VisualElement supports.
    /// </summary>
    public abstract class BaseProps : global::ReactiveUITK.Core.IProps
    {
        // --- Identity / structure ---
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }
        public Dictionary<string, object> ContentContainer { get; set; }

        // --- Visibility / enabled ---
        public bool? Visible { get; set; }
        public bool? Enabled { get; set; }

        // --- Tooltip / persistence ---
        public string Tooltip { get; set; }
        public string ViewDataKey { get; set; }

        // --- Focus / interaction ---
        public PickingMode? PickingMode { get; set; }
        public bool? Focusable { get; set; }
        public int? TabIndex { get; set; }
        public bool? DelegatesFocus { get; set; }

        // --- Locale ---
        public LanguageDirection? LanguageDirection { get; set; }

        // --- Pointer events ---
        public PointerEventHandler OnClick { get; set; }
        public PointerEventHandler OnPointerDown { get; set; }
        public PointerEventHandler OnPointerUp { get; set; }
        public PointerEventHandler OnPointerMove { get; set; }
        public PointerEventHandler OnPointerEnter { get; set; }
        public PointerEventHandler OnPointerLeave { get; set; }
        public WheelEventHandler OnWheel { get; set; }
        public WheelEventHandler OnScroll { get; set; }

        // --- Drag events (editor-only) ---
#if UNITY_EDITOR
        public DragEventHandler OnDragEnter { get; set; }
        public DragEventHandler OnDragLeave { get; set; }
        public DragEventHandler OnDragUpdated { get; set; }
        public DragEventHandler OnDragPerform { get; set; }
        public DragEventHandler OnDragExited { get; set; }
#endif

        // --- Focus events ---
        public FocusEventHandler OnFocus { get; set; }
        public FocusEventHandler OnBlur { get; set; }
        public FocusEventHandler OnFocusIn { get; set; }
        public FocusEventHandler OnFocusOut { get; set; }

        // --- Keyboard events ---
        public KeyboardEventHandler OnKeyDown { get; set; }
        public KeyboardEventHandler OnKeyUp { get; set; }

        // --- Input event (fires on every keystroke) ---
        public InputEventHandler OnInput { get; set; }

        // --- Lifecycle events ---
        public GeometryChangedEventHandler OnGeometryChanged { get; set; }
        public PanelLifecycleEventHandler OnAttachToPanel { get; set; }
        public PanelLifecycleEventHandler OnDetachFromPanel { get; set; }

        // --- Escape hatch for non-standard / custom prop keys ---
        /// <summary>
        /// Optional dictionary of arbitrary extra props to be merged into the final
        /// serialized dictionary. Use this for custom event types or non-standard keys
        /// that are not covered by the typed properties above.
        /// </summary>
        public Dictionary<string, object> ExtraProps { get; set; }

        public virtual Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                dict["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                dict["className"] = ClassName;
            if (Style != null)
                dict["style"] = Style;
            if (Ref != null)
                dict["ref"] = Ref;
            if (ContentContainer != null)
                dict["contentContainer"] = ContentContainer;
            if (Visible.HasValue)
                dict["visible"] = Visible.Value;
            if (Enabled.HasValue)
                dict["enabled"] = Enabled.Value;
            if (!string.IsNullOrEmpty(Tooltip))
                dict["tooltip"] = Tooltip;
            if (!string.IsNullOrEmpty(ViewDataKey))
                dict["viewDataKey"] = ViewDataKey;
            if (PickingMode.HasValue)
                dict["pickingMode"] = PickingMode.Value;
            if (Focusable.HasValue)
                dict["focusable"] = Focusable.Value;
            if (TabIndex.HasValue)
                dict["tabIndex"] = TabIndex.Value;
            if (DelegatesFocus.HasValue)
                dict["delegatesFocus"] = DelegatesFocus.Value;
            if (LanguageDirection.HasValue)
                dict["languageDirection"] = LanguageDirection.Value;
            if (OnClick != null)
                dict["onClick"] = OnClick;
            if (OnPointerDown != null)
                dict["onPointerDown"] = OnPointerDown;
            if (OnPointerUp != null)
                dict["onPointerUp"] = OnPointerUp;
            if (OnPointerMove != null)
                dict["onPointerMove"] = OnPointerMove;
            if (OnPointerEnter != null)
                dict["onPointerEnter"] = OnPointerEnter;
            if (OnPointerLeave != null)
                dict["onPointerLeave"] = OnPointerLeave;
            if (OnWheel != null)
                dict["onWheel"] = OnWheel;
            if (OnScroll != null)
                dict["onScroll"] = OnScroll;
#if UNITY_EDITOR
            if (OnDragEnter != null)
                dict["onDragEnter"] = OnDragEnter;
            if (OnDragLeave != null)
                dict["onDragLeave"] = OnDragLeave;
            if (OnDragUpdated != null)
                dict["onDragUpdated"] = OnDragUpdated;
            if (OnDragPerform != null)
                dict["onDragPerform"] = OnDragPerform;
            if (OnDragExited != null)
                dict["onDragExited"] = OnDragExited;
#endif
            if (OnFocus != null)
                dict["onFocus"] = OnFocus;
            if (OnBlur != null)
                dict["onBlur"] = OnBlur;
            if (OnFocusIn != null)
                dict["onFocusIn"] = OnFocusIn;
            if (OnFocusOut != null)
                dict["onFocusOut"] = OnFocusOut;
            if (OnKeyDown != null)
                dict["onKeyDown"] = OnKeyDown;
            if (OnKeyUp != null)
                dict["onKeyUp"] = OnKeyUp;
            if (OnInput != null)
                dict["onInput"] = OnInput;
            if (OnGeometryChanged != null)
                dict["onGeometryChanged"] = OnGeometryChanged;
            if (OnAttachToPanel != null)
                dict["onAttachToPanel"] = OnAttachToPanel;
            if (OnDetachFromPanel != null)
                dict["onDetachFromPanel"] = OnDetachFromPanel;
            if (ExtraProps != null)
            {
                foreach (var kv in ExtraProps)
                    dict[kv.Key] = kv.Value;
            }
            return dict;
        }
    }
}`,BoundsFieldProps:`using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class BoundsFieldProps : BaseProps
    {
        public Bounds? Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,BoundsIntFieldProps:`using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class BoundsIntFieldProps : BaseProps
    {
        public BoundsInt? Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,BoxProps:`using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class BoxProps : BaseProps
    {
        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();
    }
}`,ButtonProps:`using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ButtonProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                dict["text"] = Text;
            }
            return dict;
        }
    }
}`,ColorFieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ColorFieldProps : BaseProps
    {
        public Color? Value { get; set; }
        public ChangeEventHandler<Color> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,DoubleFieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class DoubleFieldProps : BaseProps
    {
        public double? Value { get; set; }
        public ChangeEventHandler<double> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,DropdownFieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class DropdownFieldProps : BaseProps
    {
        public List<string> Choices { get; set; }
        public string Value { get; set; }
        public int? SelectedIndex { get; set; }

        public ChangeEventHandler<string> OnChange { get; set; }

        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Choices != null)
            {
                dict["choices"] = Choices;
            }
            if (Value != null)
            {
                dict["value"] = Value;
            }
            if (SelectedIndex.HasValue)
            {
                dict["selectedIndex"] = SelectedIndex.Value;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (Label != null)
            {
                dict["label"] = Label;
            }
            if (VisualInput != null)
            {
                dict["visualInput"] = VisualInput;
            }
            return dict;
        }
    }
}`,EnumFieldProps:`using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class EnumFieldProps : BaseProps
    {
        public Enum Value { get; set; }
        public string EnumType { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (!string.IsNullOrEmpty(EnumType))
                map["enumType"] = EnumType;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,EnumFlagsFieldProps:`using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class EnumFlagsFieldProps : BaseProps
    {
        public Enum Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,ErrorBoundaryProps:`using System;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ErrorBoundaryProps : global::ReactiveUITK.Core.IProps
    {
        public VirtualNode Fallback { get; set; }
        public ErrorEventHandler OnError { get; set; }
        public string ResetKey { get; set; }
    }
}`,FloatFieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class FloatFieldProps : BaseProps
    {
        public float? Value { get; set; }
        public ChangeEventHandler<float> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,FoldoutProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class FoldoutProps : BaseProps
    {
        public string Text { get; set; }
        public bool? Value { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }
        public Dictionary<string, object> Header { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Text != null)
            {
                dict["text"] = Text;
            }
            if (Value.HasValue)
            {
                dict["value"] = Value.Value;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (Header != null)
            {
                dict["header"] = Header;
            }
            return dict;
        }
    }
}`,GroupBoxProps:`using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class GroupBoxProps : BaseProps
    {
        public string Text { get; set; }
        public Dictionary<string, object> Label { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            if (Label != null)
            {
                map["label"] = Label;
            }
            return map;
        }
    }
}`,Hash128FieldProps:`using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class Hash128FieldProps : BaseProps
    {
        public Hash128? Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,HelpBoxProps:`using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class HelpBoxProps : BaseProps
    {
        public string Text { get; set; }
        public string MessageType { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Text != null)
            {
                dict["text"] = Text;
            }
            if (!string.IsNullOrEmpty(MessageType))
            {
                dict["messageType"] = MessageType;
            }
            return dict;
        }
    }
}`,IMGUIContainerProps:`using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class IMGUIContainerProps : BaseProps
    {
        public Action OnGUI { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (OnGUI != null)
                map["onGUI"] = OnGUI;
            return map;
        }
    }
}`,ImageProps:`using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ImageProps : BaseProps
    {
        public Texture2D Texture { get; set; }
        public Sprite Sprite { get; set; }
        public string ScaleMode { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Texture != null)
            {
                dict["texture"] = Texture;
            }
            if (Sprite != null)
            {
                dict["sprite"] = Sprite;
            }
            if (!string.IsNullOrEmpty(ScaleMode))
            {
                dict["scaleMode"] = ScaleMode;
            }
            return dict;
        }
    }
}`,IntegerFieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class IntegerFieldProps : BaseProps
    {
        public int? Value { get; set; }
        public ChangeEventHandler<int> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,LabelProps:`using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class LabelProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Text != null)
            {
                map["text"] = Text;
            }
            return map;
        }
    }
}`,ListViewProps:`using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ListViewProps : BaseProps
    {
        public IList Items { get; set; }
        public int? SelectedIndex { get; set; }
        public float? FixedItemHeight { get; set; }
        public ItemFactory MakeItem { get; set; }
        public ItemBinder BindItem { get; set; }
        public ItemBinder UnbindItem { get; set; }

        public RowRenderer Row { get; set; }
        public SelectionType? Selection { get; set; }

        public Dictionary<string, object> ScrollView { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Items != null)
            {
                dict["items"] = Items;
            }
            if (SelectedIndex.HasValue)
            {
                dict["selectedIndex"] = SelectedIndex.Value;
            }
            if (FixedItemHeight.HasValue)
            {
                dict["fixedItemHeight"] = FixedItemHeight.Value;
            }
            if (MakeItem != null)
            {
                dict["makeItem"] = MakeItem;
            }
            if (BindItem != null)
            {
                dict["bindItem"] = BindItem;
            }
            if (UnbindItem != null)
            {
                dict["unbindItem"] = UnbindItem;
            }
            if (Row != null)
            {
                dict["row"] = Row;
            }
            if (Selection.HasValue)
            {
                dict["selectionType"] = Selection.Value;
            }
            if (ScrollView != null)
            {
                dict["scrollView"] = ScrollView;
            }
            return dict;
        }
    }
}`,LongFieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class LongFieldProps : BaseProps
    {
        public long? Value { get; set; }
        public ChangeEventHandler<long> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,MinMaxSliderProps:`using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MinMaxSliderProps : BaseProps
    {
        public float? MinValue { get; set; }
        public float? MaxValue { get; set; }
        public float? LowLimit { get; set; }
        public float? HighLimit { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (MinValue.HasValue)
                map["minValue"] = MinValue.Value;
            if (MaxValue.HasValue)
                map["maxValue"] = MaxValue.Value;
            if (LowLimit.HasValue)
                map["lowLimit"] = LowLimit.Value;
            if (HighLimit.HasValue)
                map["highLimit"] = HighLimit.Value;
            return map;
        }
    }
}`,MultiColumnListViewProps:`using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MultiColumnListViewProps : BaseProps
    {
        public IList Items { get; set; }
        public int? SelectedIndex { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }

        public List<ColumnDef> Columns { get; set; }

        public List<SortedColumnDef> SortedColumns { get; set; }
        public object SortingMode { get; set; }
        public ColumnSortEventHandler ColumnSortingChanged { get; set; }
        public Dictionary<string, float> ColumnWidths { get; set; }
        public Dictionary<string, bool> ColumnVisibility { get; set; }
        public Dictionary<string, int> ColumnDisplayIndex { get; set; }
        public ColumnLayoutEventHandler ColumnLayoutChanged { get; set; }

        public sealed class ColumnDef : global::ReactiveUITK.Core.IProps
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public float? Width { get; set; }
            public float? MinWidth { get; set; }
            public float? MaxWidth { get; set; }
            public bool? Resizable { get; set; }
            public bool? Stretchable { get; set; }
            public bool? Sortable { get; set; }
            public RowRenderer Cell { get; set; }


        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Items != null)
            {
                dict["items"] = Items;
            }
            if (SelectedIndex.HasValue)
            {
                dict["selectedIndex"] = SelectedIndex.Value;
            }
            if (FixedItemHeight.HasValue)
            {
                dict["fixedItemHeight"] = FixedItemHeight.Value;
            }
            if (Selection.HasValue)
            {
                dict["selectionType"] = Selection.Value;
            }
            if (Columns != null)
            {
                var cols = new List<Dictionary<string, object>>(Columns.Count);
                foreach (var c in Columns)
                {
                    cols.Add(c?.ToDictionary());
                }
                dict["columns"] = cols;
            }
            if (SortedColumns != null)
            {
                var arr = new List<Dictionary<string, object>>(SortedColumns.Count);
                foreach (var s in SortedColumns)
                {
                    arr.Add(s?.ToDictionary());
                }
                dict["sortedColumns"] = arr;
            }
            if (SortingMode != null)
            {
                dict["sortingMode"] = SortingMode;
            }
            if (ColumnSortingChanged != null)
            {
                dict["columnSortingChanged"] = ColumnSortingChanged;
            }
            if (ColumnWidths != null)
            {
                dict["columnWidths"] = ColumnWidths;
            }
            if (ColumnVisibility != null)
            {
                dict["columnVisibility"] = ColumnVisibility;
            }
            if (ColumnDisplayIndex != null)
            {
                dict["columnDisplayIndex"] = ColumnDisplayIndex;
            }
            if (ColumnLayoutChanged != null)
            {
                dict["columnLayoutChanged"] = ColumnLayoutChanged;
            }
            return dict;
        }
    }
}`,MultiColumnTreeViewProps:`using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MultiColumnTreeViewProps : BaseProps
    {
        public IList RootItems { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }
        public int? SelectedIndex { get; set; }
        public List<ColumnDef> Columns { get; set; }
        public IList<int> ExpandedItemIds { get; set; }
        public bool? StopTrackingUserChange { get; set; }
        public Dictionary<string, float> ColumnWidths { get; set; }
        public Dictionary<string, bool> ColumnVisibility { get; set; }
        public Dictionary<string, int> ColumnDisplayIndex { get; set; }
        public List<SortedColumnDef> SortedColumns { get; set; }
        public object SortingMode { get; set; }
        public ColumnSortEventHandler ColumnSortingChanged { get; set; }
        public ColumnLayoutEventHandler ColumnLayoutChanged { get; set; }

        public sealed class ColumnDef : global::ReactiveUITK.Core.IProps
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public float? Width { get; set; }
            public float? MinWidth { get; set; }
            public float? MaxWidth { get; set; }
            public bool? Resizable { get; set; }
            public bool? Stretchable { get; set; }
            public bool? Sortable { get; set; }
            public RowRenderer Cell { get; set; }


        }

        public override Dictionary<string, object> ToDictionary()
        {
            var d = base.ToDictionary();
            if (RootItems != null)
            {
                d["rootItems"] = RootItems;
            }
            if (FixedItemHeight.HasValue)
            {
                d["fixedItemHeight"] = FixedItemHeight.Value;
            }
            if (Selection.HasValue)
            {
                d["selectionType"] = Selection.Value;
            }
            if (SelectedIndex.HasValue)
            {
                d["selectedIndex"] = SelectedIndex.Value;
            }
            if (Columns != null)
            {
                var cols = new List<Dictionary<string, object>>(Columns.Count);
                foreach (var c in Columns)
                {
                    cols.Add(c?.ToDictionary());
                }
                d["columns"] = cols;
            }
            if (ExpandedItemIds != null)
            {
                d["expandedItemIds"] = ExpandedItemIds;
            }
            if (StopTrackingUserChange.HasValue)
            {
                d["stopTrackingUserChange"] = StopTrackingUserChange.Value;
            }
            if (ColumnWidths != null)
            {
                d["columnWidths"] = ColumnWidths;
            }
            if (ColumnVisibility != null)
            {
                d["columnVisibility"] = ColumnVisibility;
            }
            if (ColumnDisplayIndex != null)
            {
                d["columnDisplayIndex"] = ColumnDisplayIndex;
            }
            if (SortedColumns != null)
            {
                var arr = new List<Dictionary<string, object>>(SortedColumns.Count);
                foreach (var s in SortedColumns)
                {
                    arr.Add(s?.ToDictionary());
                }
                d["sortedColumns"] = arr;
            }
            if (SortingMode != null)
            {
                d["sortingMode"] = SortingMode;
            }
            if (ColumnSortingChanged != null)
            {
                d["columnSortingChanged"] = ColumnSortingChanged;
            }
            if (ColumnLayoutChanged != null)
            {
                d["columnLayoutChanged"] = ColumnLayoutChanged;
            }
            return d;
        }
    }
}`,ObjectFieldProps:`using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ObjectFieldProps : BaseProps
    {
        public UnityEngine.Object Value { get; set; }
        public string ObjectType { get; set; }
        public bool? AllowSceneObjects { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (!string.IsNullOrEmpty(ObjectType))
                map["objectType"] = ObjectType;
            if (AllowSceneObjects.HasValue)
                map["allowSceneObjects"] = AllowSceneObjects.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,ProgressBarProps:`using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ProgressBarProps : BaseProps
    {
        public float? Value { get; set; }
        public string Title { get; set; }
        public Dictionary<string, object> Progress { get; set; }
        public Dictionary<string, object> TitleElement { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Value.HasValue)
            {
                map["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Title))
            {
                map["title"] = Title;
            }
            if (Progress != null)
            {
                map["progress"] = Progress;
            }
            if (TitleElement != null)
            {
                map["titleElement"] = TitleElement;
            }
            return map;
        }
    }
}`,PropertyInspectorProps:`using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class PropertyFieldProps : BaseProps
    {
        public Object Target { get; set; }
        public string BindingPath { get; set; }
        public string Label { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Target != null)
                map["target"] = Target;
            if (!string.IsNullOrEmpty(BindingPath))
                map["bindingPath"] = BindingPath;
            if (!string.IsNullOrEmpty(Label))
                map["label"] = Label;
            return map;
        }
    }

    public sealed class InspectorElementProps : BaseProps
    {
        public Object Target { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Target != null)
                map["target"] = Target;
            return map;
        }
    }
}`,RadioButtonGroupProps:`using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RadioButtonGroupProps : BaseProps
    {
        public IList<string> Choices { get; set; }
        public string Value { get; set; }
        public int? Index { get; set; }
        public ChangeEventHandler<int> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Choices != null)
            {
                map["choices"] = Choices;
            }
            if (!string.IsNullOrEmpty(Value))
            {
                map["value"] = Value;
            }
            if (Index.HasValue)
            {
                map["index"] = Index.Value;
            }
            if (OnChange != null)
            {
                map["onChange"] = OnChange;
            }
            return map;
        }
    }
}`,RadioButtonProps:`using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RadioButtonProps : BaseProps
    {
        public bool? Value { get; set; }
        public string Text { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Value.HasValue)
            {
                map["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            if (OnChange != null)
            {
                map["onChange"] = OnChange;
            }
            if (Label != null)
            {
                map["label"] = Label;
            }
            return map;
        }
    }
}`,RectFieldProps:`using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RectFieldProps : BaseProps
    {
        public Rect? Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,RectIntFieldProps:`using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RectIntFieldProps : BaseProps
    {
        public RectInt? Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,RepeatButtonProps:`using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RepeatButtonProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            return map;
        }
    }
}`,ScrollViewProps:`using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ScrollViewProps : BaseProps
    {
        public string Mode { get; set; }
        public ScrollerVisibility? VerticalScrollerVisibility { get; set; }
        public ScrollerVisibility? HorizontalScrollerVisibility { get; set; }
        public Vector2? ScrollOffset { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (!string.IsNullOrEmpty(Mode))
            {
                dict["mode"] = Mode;
            }
            if (VerticalScrollerVisibility.HasValue)
            {
                dict["verticalScrollerVisibility"] = VerticalScrollerVisibility.Value;
            }
            if (HorizontalScrollerVisibility.HasValue)
            {
                dict["horizontalScrollerVisibility"] = HorizontalScrollerVisibility.Value;
            }
            if (ScrollOffset.HasValue)
            {
                dict["scrollOffset"] = ScrollOffset.Value;
            }
            return dict;
        }
    }
}`,ScrollerProps:`using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ScrollerProps : BaseProps
    {
        public float? LowValue { get; set; }
        public float? HighValue { get; set; }
        public float? Value { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (LowValue.HasValue)
                map["lowValue"] = LowValue.Value;
            if (HighValue.HasValue)
                map["highValue"] = HighValue.Value;
            if (Value.HasValue)
                map["value"] = Value.Value;
            return map;
        }
    }
}`,SliderIntProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class SliderIntProps : BaseProps
    {
        public int? LowValue { get; set; }
        public int? HighValue { get; set; }
        public int? Value { get; set; }
        public string Direction { get; set; }

        public ChangeEventHandler<int> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (LowValue.HasValue)
            {
                dict["lowValue"] = LowValue.Value;
            }
            if (HighValue.HasValue)
            {
                dict["highValue"] = HighValue.Value;
            }
            if (Value.HasValue)
            {
                dict["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Direction))
            {
                dict["direction"] = Direction;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            return dict;
        }
    }
}`,SliderProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class SliderProps : BaseProps
    {
        public float? LowValue { get; set; }
        public float? HighValue { get; set; }
        public float? Value { get; set; }
        public string Direction { get; set; }

        // Optional slot-style props for inner parts of the slider.
        // These maps can contain "style", "className", etc., which are
        // applied directly to the corresponding UI Toolkit elements.
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> Track { get; set; }
        public Dictionary<string, object> DragContainer { get; set; }
        public Dictionary<string, object> Handle { get; set; }
        public Dictionary<string, object> HandleBorder { get; set; }

        public ChangeEventHandler<float> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (LowValue.HasValue)
            {
                dict["lowValue"] = LowValue.Value;
            }
            if (HighValue.HasValue)
            {
                dict["highValue"] = HighValue.Value;
            }
            if (Value.HasValue)
            {
                dict["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Direction))
            {
                dict["direction"] = Direction;
            }
            if (Input != null)
            {
                dict["input"] = Input;
            }
            if (Track != null)
            {
                dict["track"] = Track;
            }
            if (DragContainer != null)
            {
                dict["dragContainer"] = DragContainer;
            }
            if (Handle != null)
            {
                dict["handle"] = Handle;
            }
            if (HandleBorder != null)
            {
                dict["handleBorder"] = HandleBorder;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            return dict;
        }
    }
}`,TabProps:`using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var d = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                d["text"] = Text;
            }
            return d;
        }
    }
}`,TabViewProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabViewProps : BaseProps
    {
        public int? SelectedIndex { get; set; }
        public int? SelectedTabIndex { get; set; }
        public List<TabDef> Tabs { get; set; }
        public TabIndexEventHandler SelectedIndexChanged { get; set; }
        public TabChangedEventHandler ActiveTabChanged { get; set; }

        public sealed class TabDef : global::ReactiveUITK.Core.IProps
        {
            public string Title { get; set; }
            public ContentRenderer Content { get; set; }
            public VirtualNode StaticContent { get; set; }


        }

        public override Dictionary<string, object> ToDictionary()
        {
            var d = base.ToDictionary();
            int? selected = SelectedTabIndex ?? SelectedIndex;
            if (selected.HasValue)
            {
                d["selectedTabIndex"] = selected.Value;
            }
            if (Tabs != null)
            {
                var list = new List<Dictionary<string, object>>(Tabs.Count);
                foreach (var t in Tabs)
                {
                    list.Add(t?.ToDictionary());
                }
                d["tabs"] = list;
            }
            if (SelectedIndexChanged != null)
            {
                d["selectedIndexChanged"] = SelectedIndexChanged;
            }
            if (ActiveTabChanged != null)
            {
                d["activeTabChanged"] = ActiveTabChanged;
            }
            return d;
        }
    }
}`,TemplateContainerProps:`using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TemplateContainerProps : BaseProps
    {
        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();
    }
}`,TextElementProps:`using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TextElementProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            return map;
        }
    }
}`,TextFieldProps:`using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TextFieldProps : BaseProps
    {
        public string Value { get; set; }
        public bool? Multiline { get; set; }
        public bool? Password { get; set; }
        public bool? ReadOnly { get; set; }
        public int? MaxLength { get; set; }
        public string Placeholder { get; set; }
        public bool? HidePlaceholderOnFocus { get; set; }

        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> TextElement { get; set; }

        public ChangeEventHandler<string> OnChange { get; set; }

        public string LabelText { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Value != null)
            {
                dict["value"] = Value;
            }
            if (Multiline.HasValue)
            {
                dict["multiline"] = Multiline.Value;
            }
            if (Password.HasValue)
            {
                dict["password"] = Password.Value;
            }
            if (ReadOnly.HasValue)
            {
                dict["readOnly"] = ReadOnly.Value;
            }
            if (MaxLength.HasValue)
            {
                dict["maxLength"] = MaxLength.Value;
            }
            if (!string.IsNullOrEmpty(Placeholder))
            {
                dict["placeholder"] = Placeholder;
            }
            if (HidePlaceholderOnFocus.HasValue)
            {
                dict["hidePlaceholderOnFocus"] = HidePlaceholderOnFocus.Value;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (!string.IsNullOrEmpty(LabelText))
            {
                dict["label"] = LabelText;
            }
            if (Label != null)
            {
                dict["label"] = Label;
            }
            if (Input != null)
            {
                dict["input"] = Input;
            }
            if (TextElement != null)
            {
                dict["textElement"] = TextElement;
            }
            return dict;
        }
    }
}`,ToggleButtonGroupProps:`using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToggleButtonGroupProps : BaseProps
    {
        public int? Value { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            return map;
        }
    }
}`,ToggleProps:`using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToggleProps : BaseProps
    {
        public bool? Value { get; set; }
        public string Text { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> Checkmark { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Value.HasValue)
            {
                map["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            if (OnChange != null)
            {
                map["onChange"] = OnChange;
            }
            if (Label != null)
            {
                map["label"] = Label;
            }
            if (Input != null)
            {
                map["input"] = Input;
            }
            if (Checkmark != null)
            {
                map["checkmark"] = Checkmark;
            }
            return map;
        }
    }
}`,ToolbarProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToolbarProps : BaseProps
    {
        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();
    }

    public sealed class ToolbarButtonProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            return map;
        }
    }

    public sealed class ToolbarToggleProps : BaseProps
    {
        public string Text { get; set; }
        public bool? Value { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            return map;
        }
    }

    public sealed class ToolbarMenuProps : BaseProps
    {
        public string Text { get; set; }
        public MenuBuilderHandler PopulateMenu { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (PopulateMenu != null)
                map["populateMenu"] = PopulateMenu;
            return map;
        }
    }

    public sealed class ToolbarBreadcrumbsProps : BaseProps
    {
        public IEnumerable<string> Items { get; set; }
        public Action<int> OnItem { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Items != null)
                map["items"] = Items;
            if (OnItem != null)
                map["onItem"] = OnItem;
            return map;
        }
    }

    public sealed class ToolbarPopupSearchFieldProps : BaseProps
    {
        public string Value { get; set; }
        public ChangeEventHandler<string> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            return map;
        }
    }

    public sealed class ToolbarSearchFieldProps : BaseProps
    {
        public string Value { get; set; }
        public ChangeEventHandler<string> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            return map;
        }
    }

    public sealed class ToolbarSpacerProps : BaseProps
    {
        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();
    }
}`,TreeViewProps:`using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TreeViewProps : BaseProps
    {
        public IList RootItems { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }
        public int? SelectedIndex { get; set; }
        public RowRenderer Row { get; set; }
        public IList<int> ExpandedItemIds { get; set; }
        public bool? StopTrackingUserChange { get; set; }
        public TreeExpansionEventHandler ItemExpandedChanged { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var d = base.ToDictionary();
            if (RootItems != null)
            {
                d["rootItems"] = RootItems;
            }
            if (FixedItemHeight.HasValue)
            {
                d["fixedItemHeight"] = FixedItemHeight.Value;
            }
            if (Selection.HasValue)
            {
                d["selectionType"] = Selection.Value;
            }
            if (SelectedIndex.HasValue)
            {
                d["selectedIndex"] = SelectedIndex.Value;
            }
            if (Row != null)
            {
                d["row"] = Row;
            }
            if (ExpandedItemIds != null)
            {
                d["expandedItemIds"] = ExpandedItemIds;
            }
            if (StopTrackingUserChange.HasValue)
            {
                d["stopTrackingUserChange"] = StopTrackingUserChange.Value;
            }
            if (ItemExpandedChanged != null)
            {
                d["itemExpandedChanged"] = ItemExpandedChanged;
            }
            return d;
        }
    }
}`,TwoPaneSplitViewProps:`#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TwoPaneSplitViewProps : BaseProps
    {
        public string Orientation { get; set; } // "horizontal" | "vertical"
        public int? FixedPaneIndex { get; set; }
        public float? FixedPaneInitialDimension { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Orientation))
                map["orientation"] = Orientation;
            if (FixedPaneIndex.HasValue)
                map["fixedPaneIndex"] = FixedPaneIndex.Value;
            if (FixedPaneInitialDimension.HasValue)
                map["fixedPaneInitialDimension"] = FixedPaneInitialDimension.Value;
            return map;
        }
    }
}
#endif`,UnsignedIntegerFieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class UnsignedIntegerFieldProps : BaseProps
    {
        public uint? Value { get; set; }
        public ChangeEventHandler<uint> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,UnsignedLongFieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class UnsignedLongFieldProps : BaseProps
    {
        public ulong? Value { get; set; }
        public ChangeEventHandler<ulong> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,Vector2FieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class Vector2FieldProps : BaseProps
    {
        public Vector2? Value { get; set; }
        public ChangeEventHandler<Vector2> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,Vector2IntFieldProps:`using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class Vector2IntFieldProps : BaseProps
    {
        public Vector2Int? Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,Vector3FieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class Vector3FieldProps : BaseProps
    {
        public Vector3? Value { get; set; }
        public ChangeEventHandler<Vector3> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,Vector3IntFieldProps:`using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class Vector3IntFieldProps : BaseProps
    {
        public Vector3Int? Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,Vector4FieldProps:`using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class Vector4FieldProps : BaseProps
    {
        public Vector4? Value { get; set; }
        public ChangeEventHandler<Vector4> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}`,VisualElementProps:`using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Props for a plain VisualElement. All shared element properties are inherited from BaseProps.
    /// </summary>
    public sealed class VisualElementProps : BaseProps
    {
        public override Dictionary<string, object> ToDictionary()
        {
            return base.ToDictionary();
        }
    }
}`},Q=e=>Xv[e]??``;var Zv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Qv={BoundsField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-BoundsField.html`},BoundsIntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-BoundsIntField.html`},Box:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Box.html`},Button:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Button.html`},ColorField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ColorField.html`},DoubleField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-DoubleField.html`},DropdownField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-DropdownField.html`},EnumField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-EnumField.html`},EnumFlagsField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-EnumFlagsField.html`},FloatField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-FloatField.html`},Foldout:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Foldout.html`},GroupBox:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-GroupBox.html`},Hash128Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Hash128Field.html`},HelpBox:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-HelpBox.html`},IMGUIContainer:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-IMGUIContainer.html`},Image:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Image.html`},IntegerField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-IntegerField.html`},Label:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Label.html`},ListView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ListView.html`},LongField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-LongField.html`},MinMaxSlider:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MinMaxSlider.html`},MultiColumnListView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MultiColumnListView.html`},MultiColumnTreeView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MultiColumnTreeView.html`},ObjectField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ObjectField.html`},ProgressBar:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ProgressBar.html`},PropertyInspector:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-InspectorElement.html`,label:`InspectorElement entry`,note:`ReactiveUITK.PropertyInspector wraps Unity’s InspectorElement to embed serialized-object inspectors.`},RadioButton:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RadioButton.html`},RadioButtonGroup:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RadioButtonGroup.html`},RectField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RectField.html`},RectIntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RectIntField.html`},RepeatButton:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RepeatButton.html`},ScrollView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ScrollView.html`},Scroller:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Scroller.html`},Slider:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Slider.html`},SliderInt:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-SliderInt.html`},Tab:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Tab.html`},TabView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TabView.html`},TemplateContainer:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TemplateContainer.html`},TextElement:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TextElement.html`},TextField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TextField.html`},Toggle:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Toggle.html`},ToggleButtonGroup:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ToggleButtonGroup.html`},Toolbar:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Toolbar.html`},TreeView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TreeView.html`},TwoPaneSplitView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TwoPaneSplitView.html`},UnsignedIntegerField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-UnsignedIntegerField.html`},UnsignedLongField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-UnsignedLongField.html`},Vector2Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector2Field.html`},Vector2IntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector2IntField.html`},Vector3Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector3Field.html`},Vector3IntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector3IntField.html`},Vector4Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector4Field.html`},VisualElement:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-VisualElement.html`}},$=({componentName:e})=>{let t=Qv[e];if(!t)return null;let n=t.label??`${e} entry`;return(0,L.jsxs)(W,{sx:{mt:2},children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Unity docs`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Review the`,` `,(0,L.jsx)(bg,{href:t.href,target:`_blank`,rel:`noreferrer`,children:n}),` `,`in the Unity manual for the official UI Toolkit reference.`]}),t.note&&(0,L.jsx)(U,{variant:`body2`,color:`text.secondary`,paragraph:!0,children:t.note})]})},$v=()=>(0,L.jsxs)(W,{sx:Zv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`BoundsField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.BoundsField`}),` wraps the Unity `,(0,L.jsx)(`code`,{children:`BoundsField`}),` control using`,` `,(0,L.jsx)(`code`,{children:`BoundsFieldProps`}),`. It is useful for editing `,(0,L.jsx)(`code`,{children:`Bounds`}),` values in both runtime UI and editor tools.`]}),(0,L.jsxs)(W,{sx:Zv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`BoundsFieldProps`)})]}),(0,L.jsxs)(W,{sx:Zv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,L.jsx)(`code`,{children:`BoundsFieldProps`}),` instance to `,(0,L.jsx)(`code`,{children:`V.BoundsField`}),`. The`,` `,(0,L.jsx)(`code`,{children:`Value`}),` property controls the current bounds.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class BoundsFieldExamples
{
  private static readonly Style VisualInputStyle = new Style
  {
    (StyleKeys.PaddingLeft, 4f),
  };

  // Function component – pass BoundsFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (bounds, setBounds) = Hooks.UseState(new Bounds(Vector3.zero, new Vector3(1, 1, 1)));

    void OnChange(ChangeEvent<Bounds> evt)
    {
      setBounds(evt.newValue);
    }

    return V.BoundsField(
      new BoundsFieldProps
      {
        Value = bounds,
        Label = new LabelProps { Text = "Bounds" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", VisualInputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:Zv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,L.jsxs)(U,{variant:`body1`,children:[(0,L.jsx)(`code`,{children:`BoundsField`}),` does not accept child nodes; all configuration is done through`,` `,(0,L.jsx)(`code`,{children:`BoundsFieldProps`}),`.`]})]}),(0,L.jsxs)(W,{sx:Zv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,L.jsx)(`code`,{children:`Label`}),` and `,(0,L.jsx)(`code`,{children:`VisualInput`}),` properties to style the label and the internal input container. Both expect dictionaries – you can compose them using other typed props (for example `,(0,L.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),`) or by building a`,` `,(0,L.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,L.jsxs)(W,{sx:Zv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Controlled value`}),(0,L.jsxs)(U,{variant:`body1`,children:[`Use `,(0,L.jsx)(`code`,{children:`Hooks.UseState`}),` (or a signal) to hold the current `,(0,L.jsx)(`code`,{children:`Bounds`}),` and update it from a change handler. The example above uses a local state tuple and updates the value via `,(0,L.jsx)(`code`,{children:`setBounds(evt.newValue)`}),` (you can also use the optional`,` `,(0,L.jsx)(`code`,{children:`StateSetterExtensions.Set`}),` helper if you prefer method syntax).`]})]}),(0,L.jsx)($,{componentName:`BoundsField`})]});var ey={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ty=()=>(0,L.jsxs)(W,{sx:ey.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`BoundsIntField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.BoundsIntField`}),` wraps the Unity `,(0,L.jsx)(`code`,{children:`BoundsIntField`}),` control using`,` `,(0,L.jsx)(`code`,{children:`BoundsIntFieldProps`}),` for working with integer bounds in both runtime UI and editor tools.`]}),(0,L.jsxs)(W,{sx:ey.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`BoundsIntFieldProps`)})]}),(0,L.jsxs)(W,{sx:ey.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,L.jsx)(`code`,{children:`BoundsIntFieldProps`}),` with an initial `,(0,L.jsx)(`code`,{children:`BoundsInt`}),` to render the field. Combine it with `,(0,L.jsx)(`code`,{children:`Hooks.UseState`}),` or signals to keep the value controlled.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class BoundsIntFieldExamples
{
  private static readonly Style VisualInputStyle = new Style
  {
    (StyleKeys.PaddingLeft, 4f),
  };

  // Function component – pass BoundsIntFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new BoundsInt(1, 2, 3, 4, 5, 6));

    void OnChange(ChangeEvent<BoundsInt> evt)
    {
      setValue(evt.newValue);
    }

    return V.BoundsIntField(
      new BoundsIntFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "BoundsInt" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", VisualInputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:ey.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,L.jsxs)(U,{variant:`body1`,children:[(0,L.jsx)(`code`,{children:`BoundsIntField`}),` does not support child nodes. Use the label slot to add context.`]})]}),(0,L.jsxs)(W,{sx:ey.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,L.jsx)(`code`,{children:`Label`}),` and `,(0,L.jsx)(`code`,{children:`VisualInput`}),` properties on`,` `,(0,L.jsx)(`code`,{children:`BoundsIntFieldProps`}),` to configure the label and the internal input container. Both expect dictionaries; for example, you can build a label with`,` `,(0,L.jsx)(`code`,{children:`new LabelProps { Text = "BoundsInt" }.ToDictionary()`}),` or provide a`,(0,L.jsx)(`code`,{children:`VisualInput`}),` dictionary that contains a nested `,(0,L.jsx)(`code`,{children:`Style`}),`.`]})]}),(0,L.jsx)($,{componentName:`BoundsIntField`})]});var ny={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ry=()=>(0,L.jsxs)(W,{sx:ny.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Box`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Box`}),` renders a boxed container element with optional content. It is useful for grouping related controls with a background and padding.`]}),(0,L.jsxs)(W,{sx:ny.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`BoxProps`)})]}),(0,L.jsxs)(W,{sx:ny.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,L.jsx)(`code`,{children:`BoxProps`}),` instance to `,(0,L.jsx)(`code`,{children:`V.Box`}),` and supply children as additional arguments.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class BoxExamples
{
  private static readonly Style OuterStyle = new Style
  {
    (StyleKeys.Padding, 8f),
    (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.2f, 1f)),
    (StyleKeys.BorderRadius, 4f),
  };

  private static readonly Style ContentContainerStyle = new Style
  {
    (StyleKeys.MarginTop, 4f),
  };

  // Function component – pass BoxExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentContainerStyle },
    };

    return V.Box(
      new BoxProps
      {
        Style = OuterStyle,
        ContentContainer = contentContainerProps,
      },
      key: null,
      V.Label(new LabelProps { Text = "Inside Box" })
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:ny.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,L.jsx)(U,{variant:`body1`,children:`Children are rendered inside the box's content container. Use this to create sections of your UI that share common styling.`})]}),(0,L.jsxs)(W,{sx:ny.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (contentContainer)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,L.jsx)(`code`,{children:`ContentContainer`}),` property on `,(0,L.jsx)(`code`,{children:`BoxProps`}),` to style or configure the box's `,(0,L.jsx)(`code`,{children:`contentContainer`}),`. This property expects a dictionary, allowing you to pass a nested `,(0,L.jsx)(`code`,{children:`Style`}),` or additional props that should be applied to the content container element.`]})]}),(0,L.jsx)($,{componentName:`Box`})]});var iy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ay=()=>(0,L.jsxs)(W,{sx:iy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Button`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Button`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Button`}),` element with`,` `,(0,L.jsx)(`code`,{children:`ButtonProps`}),`. Use it for clickable actions.`]}),(0,L.jsxs)(W,{sx:iy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ButtonProps`)})]}),(0,L.jsxs)(W,{sx:iy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,L.jsx)(`code`,{children:`Text`}),`, optional `,(0,L.jsx)(`code`,{children:`Style`}),`, and an `,(0,L.jsx)(`code`,{children:`OnClick`}),` handler in `,(0,L.jsx)(`code`,{children:`ButtonProps`}),`. Combine with `,(0,L.jsx)(`code`,{children:`Hooks.UseState`}),` to build controlled buttons.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ButtonExamples
{
  private static readonly Style ButtonStyle = new Style { (StyleKeys.MarginTop, 4f) };

  // Function component �?" no Render method wrapper; pass this
  // function to V.Func when mounting.
  public static VirtualNode BasicButton(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (count, setCount) = Hooks.UseState(0);

    void OnClick()
    {
      setCount(previous => previous + 1);
      Debug.Log($"Clicked {count + 1} times");
    }

    return V.Button(
      new ButtonProps
      {
        Text = $"Click me ({count})",
        OnClick = OnClick,
        Style = ButtonStyle,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Button`})]});var oy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const sy=()=>(0,L.jsxs)(W,{sx:oy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ColorField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.ColorField`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`ColorField`}),` element using`,` `,(0,L.jsx)(`code`,{children:`ColorFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:oy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ColorFieldProps`)})]}),(0,L.jsxs)(W,{sx:oy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class ColorFieldExamples
{
  private static readonly Style InputStyle = new Style
  {
    (StyleKeys.PaddingLeft, 4f),
  };

  // Function component – pass ColorFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (color, setColor) = Hooks.UseState(new Color(0.2f, 0.6f, 0.9f, 1f));

    void OnChange(ChangeEvent<Color> evt)
    {
      setColor(evt.newValue);
    }

    return V.ColorField(
      new ColorFieldProps
      {
        Value = color,
        Label = new LabelProps { Text = "Tint" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:oy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use `,(0,L.jsx)(`code`,{children:`ColorFieldProps.Label`}),` to configure the label element, and`,` `,(0,L.jsx)(`code`,{children:`ColorFieldProps.VisualInput`}),` to style the input container (for example, padding or background). Both properties accept dictionaries; in most cases you construct them from other typed props or by nesting a `,(0,L.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,L.jsx)($,{componentName:`ColorField`})]});var cy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ly=()=>(0,L.jsxs)(W,{sx:cy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`DoubleField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.DoubleField`}),` exposes a double-precision numeric field via`,` `,(0,L.jsx)(`code`,{children:`DoubleFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:cy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`DoubleFieldProps`)})]}),(0,L.jsxs)(W,{sx:cy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class DoubleFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass DoubleFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(3.14159);

    void OnChange(ChangeEvent<double> evt)
  {
      setValue(evt.newValue);
    }

    return V.DoubleField(
      new DoubleFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Double" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:cy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`DoubleFieldProps.Label`}),` and `,(0,L.jsx)(`code`,{children:`DoubleFieldProps.VisualInput`}),` follow the same pattern as other numeric fields. Use a label dictionary (often built from`,` `,(0,L.jsx)(`code`,{children:`LabelProps`}),`) and a visual input dictionary that can contain a nested`,` `,(0,L.jsx)(`code`,{children:`Style`}),` for the inner input container.`]})]}),(0,L.jsx)($,{componentName:`DoubleField`})]});var uy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const dy=()=>(0,L.jsxs)(W,{sx:uy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`DropdownField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.DropdownField`}),` renders a text-based dropdown using `,(0,L.jsx)(`code`,{children:`DropdownFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:uy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`DropdownFieldProps`)})]}),(0,L.jsxs)(W,{sx:uy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class DropdownFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass DropdownFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (index, setIndex) = Hooks.UseState(0);

    IList choices = new[] { "Red", "Green", "Blue" };

    void OnChange(ChangeEvent<string> evt)
    {
      setIndex(previous => choices.IndexOf(evt.newValue));
    }

    return V.DropdownField(
      new DropdownFieldProps
      {
        Choices = choices,
        SelectedIndex = index,
        Label = new LabelProps { Text = "Color" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:uy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`DropdownFieldProps.Label`}),` and `,(0,L.jsx)(`code`,{children:`DropdownFieldProps.VisualInput`}),` mirror the slots on the underlying UI Toolkit control. Use `,(0,L.jsx)(`code`,{children:`Label`}),` to configure the label element, and `,(0,L.jsx)(`code`,{children:`VisualInput`}),` to style the internal input area via a dictionary that can contain a nested `,(0,L.jsx)(`code`,{children:`Style`}),`.`]})]}),(0,L.jsx)($,{componentName:`DropdownField`})]});var fy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const py=()=>(0,L.jsxs)(W,{sx:fy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`EnumField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.EnumField`}),` binds to any enum type via `,(0,L.jsx)(`code`,{children:`EnumFieldProps`}),`. Provide the enum's assembly-qualified type name and an initial `,(0,L.jsx)(`code`,{children:`Value`}),`.`]}),(0,L.jsxs)(W,{sx:fy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`EnumFieldProps`)})]}),(0,L.jsxs)(W,{sx:fy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public enum ExampleEnum
{
  A,
  B,
  C,
}

public static class EnumFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass EnumFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(ExampleEnum.B);

    void OnChange(ChangeEvent<System.Enum> evt)
    {
      setValue((ExampleEnum)evt.newValue);
    }

    return V.EnumField(
      new EnumFieldProps
      {
        EnumType = typeof(ExampleEnum).AssemblyQualifiedName,
        Value = value,
        Label = new LabelProps { Text = "Example enum" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:fy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`EnumFieldProps.Label`}),` and `,(0,L.jsx)(`code`,{children:`EnumFieldProps.VisualInput`}),` configure the label and input slots respectively. As with other fields, both expect dictionaries; label dictionaries are often created from `,(0,L.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),`, while visual input dictionaries typically wrap a `,(0,L.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,L.jsx)($,{componentName:`EnumField`})]});var my={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const hy=()=>(0,L.jsxs)(W,{sx:my.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`EnumFlagsField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.EnumFlagsField`}),` is similar to `,(0,L.jsx)(`code`,{children:`V.EnumField`}),` but supports`,` `,(0,L.jsx)(`code`,{children:`[Flags]`}),` enums.`]}),(0,L.jsxs)(W,{sx:my.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`EnumFlagsFieldProps`)})]}),(0,L.jsxs)(W,{sx:my.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

[Flags]
public enum ExampleFlags
{
  None = 0,
  A = 1 << 0,
  B = 1 << 1,
  C = 1 << 2,
}

public static class EnumFlagsFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass EnumFlagsFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(ExampleFlags.A | ExampleFlags.C);

    void OnChange(ChangeEvent<System.Enum> evt)
    {
      setValue((ExampleFlags)evt.newValue);
    }

    return V.EnumFlagsField(
      new EnumFlagsFieldProps
      {
        EnumType = typeof(ExampleFlags).AssemblyQualifiedName,
        Value = value,
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:my.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`EnumFlagsFieldProps.Label`}),` and `,(0,L.jsx)(`code`,{children:`EnumFlagsFieldProps.VisualInput`}),`behave the same as on `,(0,L.jsx)(`code`,{children:`EnumFieldProps`}),`, allowing you to style the label element and the embedded input area via dictionaries that can contain nested `,(0,L.jsx)(`code`,{children:`Style`}),` `,`objects.`]})]}),(0,L.jsx)($,{componentName:`EnumFlagsField`})]});var gy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const _y=()=>(0,L.jsxs)(W,{sx:gy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`FloatField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.FloatField`}),` represents a single-precision numeric field, backed by`,` `,(0,L.jsx)(`code`,{children:`FloatFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:gy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`FloatFieldProps`)})]}),(0,L.jsxs)(W,{sx:gy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class FloatFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass FloatFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(1.23f);

    void OnChange(ChangeEvent<float> evt)
    {
      setValue(evt.newValue);
    }

    return V.FloatField(
      new FloatFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Float" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:gy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`FloatFieldProps.Label`}),` and `,(0,L.jsx)(`code`,{children:`FloatFieldProps.VisualInput`}),` let you customize the label element and the inner input container. Both accept dictionaries: build a label via `,(0,L.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),` and pass a dictionary with a nested`,` `,(0,L.jsx)(`code`,{children:`Style`}),` object to `,(0,L.jsx)(`code`,{children:`VisualInput`}),` to style the input.`]})]}),(0,L.jsx)($,{componentName:`FloatField`})]});var vy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const yy=()=>(0,L.jsxs)(W,{sx:vy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Foldout`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Foldout`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Foldout`}),` element using`,` `,(0,L.jsx)(`code`,{children:`FoldoutProps`}),`. It is useful for expandable sections of UI that reveal more content when open.`]}),(0,L.jsxs)(W,{sx:vy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`FoldoutProps`)})]}),(0,L.jsxs)(W,{sx:vy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,L.jsx)(`code`,{children:`Text`}),`, an optional initial `,(0,L.jsx)(`code`,{children:`Value`}),`, and an`,` `,(0,L.jsx)(`code`,{children:`OnChange`}),` handler. The example below also shows children rendered inside the foldout when it is expanded.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class FoldoutExamples
{
  private static readonly Style HeaderStyle = new Style { (StyleKeys.FontSize, 14f) };

  private static readonly Style ContentContainerStyle = new Style { (StyleKeys.PaddingLeft, 12f) };

  // Function component – pass FoldoutExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (isOpen, setIsOpen) = Hooks.UseState(true);

    void OnChange(ChangeEvent<bool> evt)
    {
      setIsOpen(evt.newValue);
    }

    var headerProps = new Dictionary<string, object>
    {
      { "style", HeaderStyle },
    };

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentContainerStyle },
    };

    return V.Foldout(
      new FoldoutProps
      {
        Text = "Foldout title",
        Value = isOpen,
        OnChange = OnChange,
        Header = headerProps,
        ContentContainer = contentContainerProps,
      },
      key: null,
      V.Label(new LabelProps { Text = "Child 1" }),
      V.Label(new LabelProps { Text = "Child 2" })
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:vy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Children passed to `,(0,L.jsx)(`code`,{children:`V.Foldout`}),` are rendered inside the foldout's content area and are shown or hidden based on the current `,(0,L.jsx)(`code`,{children:`Value`}),`.`]})]}),(0,L.jsxs)(W,{sx:vy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (header / contentContainer)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use `,(0,L.jsx)(`code`,{children:`FoldoutProps.Header`}),` and `,(0,L.jsx)(`code`,{children:`FoldoutProps.ContentContainer`}),` to style the header bar and inner content container. Both accept dictionaries; commonly a nested`,` `,(0,L.jsx)(`code`,{children:`Style`}),` is provided under the `,(0,L.jsx)(`code`,{children:`"style"`}),` key.`]})]}),(0,L.jsxs)(W,{sx:vy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Controlled value`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`For controlled foldouts, track a `,(0,L.jsx)(`code`,{children:`bool`}),` with `,(0,L.jsx)(`code`,{children:`Hooks.UseState`}),` (or a signal) and update it in `,(0,L.jsx)(`code`,{children:`OnChange`}),`. The `,(0,L.jsx)(`code`,{children:`Value`}),` property will then always reflect your source of truth.`]})]}),(0,L.jsx)($,{componentName:`Foldout`})]});var by={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const xy=()=>(0,L.jsxs)(W,{sx:by.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`GroupBox`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.GroupBox`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`GroupBox`}),` element using`,` `,(0,L.jsx)(`code`,{children:`GroupBoxProps`}),`. It is useful for grouping related controls under a titled header.`]}),(0,L.jsxs)(W,{sx:by.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`GroupBoxProps`)})]}),(0,L.jsxs)(W,{sx:by.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,L.jsx)(`code`,{children:`Text`}),` for the group title, a `,(0,L.jsx)(`code`,{children:`Style`}),` for layout, and add children that will appear inside the group.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class GroupBoxExamples
{
  private static readonly Style OuterStyle = new Style
  {
    (StyleKeys.MarginTop, 8f),
    (StyleKeys.Padding, 6f),
  };

  private static readonly Style ContentContainerStyle = new Style
  {
    (StyleKeys.PaddingTop, 4f),
  };

  private static readonly Style LabelStyle = new Style
  {
    (StyleKeys.FontSize, 14f),
  };

  // Function component – pass GroupBoxExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentContainerStyle },
    };

    var labelProps = new Dictionary<string, object>
    {
      { "style", LabelStyle },
    };

    return V.GroupBox(
      new GroupBoxProps
      {
        Text = "Group title",
        Style = OuterStyle,
        ContentContainer = contentContainerProps,
        Label = labelProps,
      },
      key: null,
      V.Label(new LabelProps { Text = "Content item 1" }),
      V.Label(new LabelProps { Text = "Content item 2" })
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:by.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Children passed to `,(0,L.jsx)(`code`,{children:`V.GroupBox`}),` are rendered inside the group's content container, below the labeled header.`]})]}),(0,L.jsxs)(W,{sx:by.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / contentContainer)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use `,(0,L.jsx)(`code`,{children:`GroupBoxProps.Label`}),` and `,(0,L.jsx)(`code`,{children:`GroupBoxProps.ContentContainer`}),` to style the header label and the inner content container. Both properties accept dictionaries, often containing nested `,(0,L.jsx)(`code`,{children:`Style`}),` objects.`]})]}),(0,L.jsx)($,{componentName:`GroupBox`})]});var Sy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Cy=()=>(0,L.jsxs)(W,{sx:Sy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Hash128Field`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Hash128Field`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Hash128Field`}),` for editing`,` `,(0,L.jsx)(`code`,{children:`Hash128`}),` values.`]}),(0,L.jsxs)(W,{sx:Sy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`Hash128FieldProps`)})]}),(0,L.jsxs)(W,{sx:Sy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Hash128FieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass Hash128FieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Hash128(1, 2, 3, 4));

    void OnChange(ChangeEvent<Hash128> evt)
    {
      setValue(evt.newValue);
    }

    return V.Hash128Field(
      new Hash128FieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Hash128" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Hash128Field`})]});var wy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ty=()=>(0,L.jsxs)(W,{sx:wy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`HelpBox`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.HelpBox`}),` wraps the standard UI Toolkit `,(0,L.jsx)(`code`,{children:`HelpBox`}),` for displaying informational, warning, or error messages.`]}),(0,L.jsxs)(W,{sx:wy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`HelpBoxProps`)})]}),(0,L.jsxs)(W,{sx:wy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class HelpBoxExamples
{
  // Function component – pass HelpBoxExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.HelpBox(
      new HelpBoxProps
      {
        Text = "Something went wrong.",
        MessageType = "Error",
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`HelpBox`})]});var Ey={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Dy=()=>(0,L.jsxs)(W,{sx:Ey.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`IMGUIContainer`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.IMGUIContainer`}),` lets you embed IMGUI content inside a UI Toolkit layout by providing an `,(0,L.jsx)(`code`,{children:`OnGUI`}),` callback in `,(0,L.jsx)(`code`,{children:`IMGUIContainerProps`}),`. This is primarily an editor-only pattern.`]}),(0,L.jsxs)(W,{sx:Ey.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`IMGUIContainerProps`)})]}),(0,L.jsxs)(W,{sx:Ey.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class IMGUIContainerExamples
{
  // Function component – pass IMGUIContainerExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    void OnGUI()
    {
      EditorGUILayout.LabelField("IMGUI content inside UI Toolkit");
    }

    return V.IMGUIContainer(
      new IMGUIContainerProps
      {
        OnGUI = OnGUI,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`IMGUIContainer`})]});var Oy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ky=()=>(0,L.jsxs)(W,{sx:Oy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Image`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Image`}),` renders a UI Toolkit `,(0,L.jsx)(`code`,{children:`Image`}),` using `,(0,L.jsx)(`code`,{children:`ImageProps`}),`. It supports both `,(0,L.jsx)(`code`,{children:`Texture2D`}),` and `,(0,L.jsx)(`code`,{children:`Sprite`}),` sources.`]}),(0,L.jsxs)(W,{sx:Oy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ImageProps`)})]}),(0,L.jsxs)(W,{sx:Oy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ImageExamples
{
  private static readonly Style ImageStyle = new Style { (StyleKeys.Width, 128f), (StyleKeys.Height, 128f) };

  // Function component – pass ImageExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var texture = props != null && props.TryGetValue("texture", out var t) ? t as Texture2D : null;

    return V.Image(
      new ImageProps
      {
        Texture = texture,
        ScaleMode = "ScaleToFit",
        Style = ImageStyle,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Image`})]});var Ay={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const jy=()=>(0,L.jsxs)(W,{sx:Ay.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`IntegerField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.IntegerField`}),` represents an integer numeric field using `,(0,L.jsx)(`code`,{children:`IntegerFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:Ay.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`IntegerFieldProps`)})]}),(0,L.jsxs)(W,{sx:Ay.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class IntegerFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass IntegerFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(42);

    void OnChange(ChangeEvent<int> evt)
    {
      setValue(evt.newValue);
    }

    return V.IntegerField(
      new IntegerFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Integer" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`IntegerField`})]});var My={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ny=()=>(0,L.jsxs)(W,{sx:My.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Label`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Label`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Label`}),` element via `,(0,L.jsx)(`code`,{children:`LabelProps`}),`. It is the primary way to render text in your component trees.`]}),(0,L.jsxs)(W,{sx:My.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`LabelProps`)})]}),(0,L.jsxs)(W,{sx:My.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class LabelExamples
{
  private static readonly Style LabelStyle = new Style { (StyleKeys.FontSize, 16f) };

  // Function component – pass LabelExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Label(
      new LabelProps
      {
        Text = "Hello label",
        Style = LabelStyle,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Label`})]});var Py={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Fy=()=>(0,L.jsxs)(W,{sx:Py.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`LongField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.LongField`}),` represents a 64-bit integer field using `,(0,L.jsx)(`code`,{children:`LongFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:Py.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`LongFieldProps`)})]}),(0,L.jsxs)(W,{sx:Py.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class LongFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass LongFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(123456789L);

    void OnChange(ChangeEvent<long> evt)
    {
      setValue(evt.newValue);
    }

    return V.LongField(
      new LongFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Long" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`LongField`})]});var Iy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ly=()=>(0,L.jsxs)(W,{sx:Iy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ProgressBar`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.ProgressBar`}),` renders a UI Toolkit `,(0,L.jsx)(`code`,{children:`ProgressBar`}),` using`,` `,(0,L.jsx)(`code`,{children:`ProgressBarProps`}),`. It is typically driven by state changes elsewhere in your UI.`]}),(0,L.jsxs)(W,{sx:Iy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ProgressBarProps`)})]}),(0,L.jsxs)(W,{sx:Iy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ProgressBarExamples
{
  private static readonly Style TrackStyle = new Style
  {
    (StyleKeys.BackgroundColor, new Color(0.02f, 0.2f, 0.02f, 0.7f)),
    (StyleKeys.BorderColor, new Color(0.07f, 0.9f, 0.22f, 1f)),
    (StyleKeys.BorderWidth, 2f),
    (StyleKeys.BorderRadius, 6f),
    (StyleKeys.Height, 30f),
  };

  private static readonly Style ProgressFillStyle = new Style
  {
    (StyleKeys.BackgroundColor, new Color(0.4f, 0.95f, 0.4f, 0.7f)),
    (StyleKeys.BorderRadius, 4f),
    (StyleKeys.MarginLeft, 2f),
    (StyleKeys.MarginRight, 2f),
    (StyleKeys.MarginTop, 2f),
    (StyleKeys.MarginBottom, 2f),
  };

  private static readonly Style TitleStyle = new Style
  {
    (StyleKeys.FontSize, 13f),
    (StyleKeys.TextAlign, "center"),
  };

  // Function component – pass ProgressBarExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0.65f);

    var progressProps = new Dictionary<string, object>
    {
      { "style", ProgressFillStyle },
    };

    var titleElementProps = new Dictionary<string, object>
    {
      { "style", TitleStyle },
    };

    return V.ProgressBar(
      new ProgressBarProps
      {
        Title = $"Downloading - {(value * 100f):0}%",
        Value = value,
        Style = TrackStyle,
        Progress = progressProps,
        TitleElement = titleElementProps,
      }
    );
  }
}`})]}),(0,L.jsxs)(W,{sx:Iy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Styling track and fill`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The`,` `,(0,L.jsx)(`a`,{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ProgressBar.html`,target:`_blank`,rel:`noreferrer`,children:`Unity ProgressBar documentation`}),` `,`highlights that the root element is the visible track, while the inner`,` `,(0,L.jsx)(`code`,{children:`.unity-progress-bar__progress`}),` child renders the filled portion.`]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Assign styles to the track via `,(0,L.jsx)(`code`,{children:`ProgressBarProps.Style`}),` (for border, unfilled background, size, etc.) and target the fill through the `,(0,L.jsx)(`code`,{children:`Progress`}),` slot. You can also style the caption by populating `,(0,L.jsx)(`code`,{children:`TitleElement`}),`. The example above uses this pattern to create a progress bar with a dark green track, a lighter fill, and centered text.`]})]}),(0,L.jsx)($,{componentName:`ProgressBar`})]});var Ry={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const zy=()=>(0,L.jsxs)(W,{sx:Ry.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ListView`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.ListView`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`ListView`}),` control using`,` `,(0,L.jsx)(`code`,{children:`ListViewProps`}),`. It can use either the standard `,(0,L.jsx)(`code`,{children:`makeItem/bindItem`}),` `,`properties or the higher-level `,(0,L.jsx)(`code`,{children:`Row`}),` function that returns a `,(0,L.jsx)(`code`,{children:`VirtualNode`}),`.`]}),(0,L.jsxs)(W,{sx:Ry.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ListViewProps`)})]}),(0,L.jsxs)(W,{sx:Ry.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ListViewExamples
{
  private static readonly Style ScrollViewStyle = new Style { (StyleKeys.MaxHeight, 200f) };

  private static readonly Style ListStyle = new Style { (StyleKeys.FlexGrow, 1f) };

  // Function component – pass ListViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    IList items = new[] { "One", "Two", "Three" };

    VirtualNode Row(int index, object item)
    {
      return V.Label(
        new LabelProps { Text = $"{index}: {item}" },
        key: $"row-{index}"
      );
    }

    var scrollViewProps = new Dictionary<string, object>
    {
      { "style", ScrollViewStyle },
    };

    var listProps = new ListViewProps
    {
      Items = items,
      FixedItemHeight = 20f,
      Row = Row,
      Selection = SelectionType.None,
      ScrollView = scrollViewProps,
      Style = ListStyle,
    };

    return V.ListView(listProps);
  }
}`})]}),(0,L.jsx)($,{componentName:`ListView`})]});var By={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Vy=()=>(0,L.jsxs)(W,{sx:By.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MinMaxSlider`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.MinMaxSlider`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`MinMaxSlider`}),` element using`,` `,(0,L.jsx)(`code`,{children:`MinMaxSliderProps`}),` for selecting a range between two limits.`]}),(0,L.jsxs)(W,{sx:By.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`MinMaxSliderProps`)})]}),(0,L.jsxs)(W,{sx:By.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class MinMaxSliderExamples
{
  private static readonly Style SliderStyle = new Style { (StyleKeys.Width, 200f) };

  // Function component – pass MinMaxSliderExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (range, setRange) = Hooks.UseState((min: 20f, max: 80f));

    void Update(float min, float max)
    {
      setRange(_ => (min, max));
    }

    return V.MinMaxSlider(
      new MinMaxSliderProps
      {
        MinValue = range.min,
        MaxValue = range.max,
        LowLimit = 0f,
        HighLimit = 100f,
        Style = SliderStyle,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`MinMaxSlider`})]});var Hy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Uy=()=>(0,L.jsxs)(W,{sx:Hy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ObjectField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.ObjectField`}),` wraps the editor-only UI Toolkit `,(0,L.jsx)(`code`,{children:`ObjectField`}),` element using `,(0,L.jsx)(`code`,{children:`ObjectFieldProps`}),`. It is typically used in custom inspectors and tools.`]}),(0,L.jsxs)(W,{sx:Hy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ObjectFieldProps`)})]}),(0,L.jsxs)(W,{sx:Hy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ObjectFieldExamples
{
  // Function component – pass ObjectFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState<Object>(null);

    void OnChange(ChangeEvent<Object> evt)
    {
      setValue(evt.newValue);
    }

    return V.ObjectField(
      new ObjectFieldProps
      {
        ObjectType = typeof(Texture2D).AssemblyQualifiedName,
        AllowSceneObjects = false,
        Value = value,
        Label = new LabelProps { Text = "Texture" }.ToDictionary(),
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`ObjectField`})]});var Wy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Gy=()=>(0,L.jsxs)(W,{sx:Wy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RadioButton`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.RadioButton`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`RadioButton`}),` element using`,` `,(0,L.jsx)(`code`,{children:`RadioButtonProps`}),`. It is usually used within a `,(0,L.jsx)(`code`,{children:`RadioButtonGroup`}),`.`]}),(0,L.jsxs)(W,{sx:Wy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`RadioButtonProps`)})]}),(0,L.jsxs)(W,{sx:Wy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class RadioButtonExamples
{
  // Function component – pass RadioButtonExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(false);

    void OnChange(ChangeEvent<bool> evt)
    {
      setValue(evt.newValue);
    }

    return V.RadioButton(
      new RadioButtonProps
      {
        Text = "Option",
        Value = value,
        Label = new LabelProps { Text = "Option" }.ToDictionary(),
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`RadioButton`})]});var Ky={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const qy=()=>(0,L.jsxs)(W,{sx:Ky.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RadioButtonGroup`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.RadioButtonGroup`}),` wraps UI Toolkit's `,(0,L.jsx)(`code`,{children:`RadioButtonGroup`}),` using`,` `,(0,L.jsx)(`code`,{children:`RadioButtonGroupProps`}),`. It manages a set of mutually exclusive choices.`]}),(0,L.jsxs)(W,{sx:Ky.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`RadioButtonGroupProps`)})]}),(0,L.jsxs)(W,{sx:Ky.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class RadioButtonGroupExamples
{
  private static readonly Style ContentContainerStyle = new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.Gap, 8f) };

  // Function component – pass RadioButtonGroupExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (index, setIndex) = Hooks.UseState(0);

    void OnChange(ChangeEvent<int> evt)
    {
      setIndex(evt.newValue);
    }

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentContainerStyle },
    };

    return V.RadioButtonGroup(
      new RadioButtonGroupProps
      {
        Choices = new[] { "Option A", "Option B", "Option C" },
        Index = index,
        ContentContainer = contentContainerProps,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`RadioButtonGroup`})]});var Jy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Yy=()=>(0,L.jsxs)(W,{sx:Jy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RepeatButton`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.RepeatButton`}),` wraps UI Toolkit's `,(0,L.jsx)(`code`,{children:`RepeatButton`}),`, invoking`,` `,(0,L.jsx)(`code`,{children:`OnClick`}),` repeatedly while the button is held.`]}),(0,L.jsxs)(W,{sx:Jy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`RepeatButtonProps`)})]}),(0,L.jsxs)(W,{sx:Jy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class RepeatButtonExamples
{
  // Function component – pass RepeatButtonExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (count, setCount) = Hooks.UseState(0);

    void OnClick()
    {
      setCount(prev => prev + 1);
    }

    return V.RepeatButton(
      new RepeatButtonProps
      {
        Text = $"Hold to repeat ({count})",
        OnClick = OnClick,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`RepeatButton`})]});var Xy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Zy=()=>(0,L.jsxs)(W,{sx:Xy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ScrollView`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.ScrollView`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`ScrollView`}),` element using`,` `,(0,L.jsx)(`code`,{children:`ScrollViewProps`}),`. It is the primary way to add scrolling regions to your layouts.`]}),(0,L.jsxs)(W,{sx:Xy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ScrollViewProps`)})]}),(0,L.jsxs)(W,{sx:Xy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ScrollViewExamples
{
  private static readonly Style ScrollContentStyle = new Style { (StyleKeys.Padding, 6f), (StyleKeys.RowGap, 4f) };

  private static readonly Style ScrollViewStyle = new Style { (StyleKeys.Height, 200f) };

  // Function component – pass ScrollViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var contentProps = new Dictionary<string, object>
    {
      { "style", ScrollContentStyle },
    };

    var scrollViewProps = new ScrollViewProps
    {
      Mode = "Vertical",
      ContentContainer = contentProps,
      Style = ScrollViewStyle,
    };

    return V.ScrollView(
      scrollViewProps,
      key: null,
      V.Label(new LabelProps { Text = "Row 1" }),
      V.Label(new LabelProps { Text = "Row 2" }),
      V.Label(new LabelProps { Text = "Row 3" })
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`ScrollView`})]});var Qy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const $y=()=>(0,L.jsxs)(W,{sx:Qy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Slider`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Slider`}),` renders a float slider using `,(0,L.jsx)(`code`,{children:`SliderProps`}),`. In addition to basic value and range, you can optionally style the inner parts of the UI Toolkit slider through slot dictionaries (`,(0,L.jsx)(`code`,{children:`Input`}),`, `,(0,L.jsx)(`code`,{children:`Track`}),`, `,(0,L.jsx)(`code`,{children:`DragContainer`}),`,`,` `,(0,L.jsx)(`code`,{children:`Handle`}),`, and `,(0,L.jsx)(`code`,{children:`HandleBorder`}),`), which map to the corresponding visual elements inside the control.`]}),(0,L.jsxs)(W,{sx:Qy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`SliderProps`)})]}),(0,L.jsxs)(W,{sx:Qy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class SliderExamples
{
  // Function component – pass SliderExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0.5f);

    void OnChange(ChangeEvent<float> evt)
    {
      setValue(evt.newValue);
    }

    var trackStyle = new Style
    {
      { StyleKeys.BackgroundColor, new Color(0.2f, 0.2f, 0.2f, 1f) },
      { StyleKeys.Height, 3f },
    };

    var handleStyle = new Style
    {
      { StyleKeys.Width, 12f },
      { StyleKeys.Height, 12f },
      { StyleKeys.BorderRadius, 6f },
      { StyleKeys.BackgroundColor, Color.white },
    };

    return V.Slider(
      new SliderProps
      {
        LowValue = 0f,
        HighValue = 1f,
        Value = value,
        Direction = "horizontal",
        Style = new Style { { StyleKeys.Width, 220f } },
        Track = new Dictionary<string, object>
        {
          { "style", trackStyle },
        },
        Handle = new Dictionary<string, object>
        {
          { "style", handleStyle },
        },
        OnChange = OnChange,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Slider`})]});var eb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const tb=()=>(0,L.jsxs)(W,{sx:eb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`SliderInt`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.SliderInt`}),` renders an integer slider using `,(0,L.jsx)(`code`,{children:`SliderIntProps`}),`.`]}),(0,L.jsxs)(W,{sx:eb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`SliderIntProps`)})]}),(0,L.jsxs)(W,{sx:eb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class SliderIntExamples
{
  // Function component – pass SliderIntExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(5);

    void OnChange(ChangeEvent<int> evt)
    {
      setValue(evt.newValue);
    }

    return V.SliderInt(
      new SliderIntProps
      {
        LowValue = 0,
        HighValue = 10,
        Value = value,
        Direction = "Horizontal",
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`SliderInt`})]});var nb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const rb=()=>(0,L.jsxs)(W,{sx:nb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Toggle`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Toggle`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Toggle`}),` control using `,(0,L.jsx)(`code`,{children:`ToggleProps`}),`.`]}),(0,L.jsxs)(W,{sx:nb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ToggleProps`)})]}),(0,L.jsxs)(W,{sx:nb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class ToggleExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.MarginRight, 4f) };

  // Function component – pass ToggleExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(true);

    void OnChange(ChangeEvent<bool> evt)
    {
      setValue(evt.newValue);
    }

    var inputProps = new Dictionary<string, object>
    {
      { "style", InputStyle },
    };

    return V.Toggle(
      new ToggleProps
      {
        Text = "Enabled",
        Value = value,
        Input = inputProps,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Toggle`})]});var ib={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ab=()=>(0,L.jsxs)(W,{sx:ib.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TreeView`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.TreeView`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`TreeView`}),` control using`,` `,(0,L.jsx)(`code`,{children:`TreeViewProps`}),`, allowing you to render hierarchical data with a`,` `,(0,L.jsx)(`code`,{children:`Row`}),` function that returns `,(0,L.jsx)(`code`,{children:`VirtualNode`}),` instances.`]}),(0,L.jsxs)(W,{sx:ib.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`TreeViewProps`)})]}),(0,L.jsxs)(W,{sx:ib.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public sealed class TreeItem
{
  public string Label;
  public int Id;
}

public static class TreeViewExamples
{
  private static readonly Style TreeViewStyle = new Style { (StyleKeys.FlexGrow, 1f) };

  // Function component – pass TreeViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var items = new List<TreeItem>
    {
      new TreeItem { Id = 1, Label = "Root 1" },
      new TreeItem { Id = 2, Label = "Root 2" },
    };

    VirtualNode Row(int index, object obj)
    {
      var item = obj as TreeItem;
      return V.Label(
        new LabelProps { Text = item?.Label ?? "<null>" },
        key: $"tree-{item?.Id ?? index}"
      );
    }

    var propsTree = new TreeViewProps
    {
      RootItems = items,
      FixedItemHeight = 20f,
      Selection = SelectionType.Single,
      Row = Row,
      Style = TreeViewStyle,
    };

    return V.TreeView(propsTree);
  }
}`})]}),(0,L.jsx)($,{componentName:`TreeView`})]});var ob={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const sb=()=>(0,L.jsxs)(W,{sx:ob.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Tab`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Tab`}),` renders an individual tab using `,(0,L.jsx)(`code`,{children:`TabProps`}),`. In most cases you will use it indirectly via `,(0,L.jsx)(`code`,{children:`TabView`}),`, but you can also construct tab strips manually.`]}),(0,L.jsxs)(W,{sx:ob.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`TabProps`)})]}),(0,L.jsxs)(W,{sx:ob.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class TabExamples
{
  // Function component – pass TabExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Tab(
      new TabProps
      {
        Text = "Tab title",
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Tab`})]});var cb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const lb=()=>(0,L.jsxs)(W,{sx:cb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TabView`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.TabView`}),` renders a tab strip and tab content using `,(0,L.jsx)(`code`,{children:`TabViewProps`}),`. Each tab is defined by a `,(0,L.jsx)(`code`,{children:`TabViewProps.TabDef`}),`, which can provide either static content or a factory function.`]}),(0,L.jsxs)(W,{sx:cb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`TabViewProps`)})]}),(0,L.jsxs)(W,{sx:cb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class TabViewExamples
{
  // Function component – pass TabViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (index, setIndex) = Hooks.UseState(0);

    var tabs = new List<TabViewProps.TabDef>
    {
      new TabViewProps.TabDef
      {
        Title = "Tab A",
        StaticContent = V.Label(new LabelProps { Text = "Content A" }),
      },
      new TabViewProps.TabDef
      {
        Title = "Tab B",
        StaticContent = V.Label(new LabelProps { Text = "Content B" }),
      },
    };

    return V.TabView(
      new TabViewProps
      {
        SelectedIndex = index,
        Tabs = tabs,
        SelectedIndexChanged = setIndex,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`TabView`})]});var ub={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const db=()=>(0,L.jsxs)(W,{sx:ub.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ToggleButtonGroup`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.ToggleButtonGroup`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`ToggleButtonGroup`}),` element using `,(0,L.jsx)(`code`,{children:`ToggleButtonGroupProps`}),`. Provide a zero-based `,(0,L.jsx)(`code`,{children:`Value`}),` index and add regular `,(0,L.jsx)(`code`,{children:`V.Button`}),` children, handling each button's `,(0,L.jsx)(`code`,{children:`OnClick`}),`to drive your own selection state.`]}),(0,L.jsxs)(W,{sx:ub.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ToggleButtonGroupProps`)})]}),(0,L.jsxs)(W,{sx:ub.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;

public static class ToggleButtonGroupExamples
{
  private static readonly Style ContainerStyle = new()
  {
    (StyleKeys.FlexDirection, "column"),
    (StyleKeys.Padding, 16f),
    (StyleKeys.BackgroundColor, new Color(0.11f, 0.11f, 0.11f, 0.85f)),
  };

  private static readonly Style StatusStyle = new()
  {
    (StyleKeys.FontSize, 13f),
    (StyleKeys.TextColor, new Color(0.85f, 0.95f, 1f, 1f)),
  };

  private static readonly string[] Options = new[] { "Alpha", "Beta", "Gamma" };

  // Function component: pass ToggleButtonGroupExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (selected, setSelected) = Hooks.UseState(0);

    return V.VisualElement(
      ContainerStyle,
      null,
      V.Label(new LabelProps { Text = "ToggleButtonGroup (Buttons inside)" }),
      V.ToggleButtonGroup(
        new ToggleButtonGroupProps { Value = selected },
        null,
        V.Button(new ButtonProps { Text = "0", OnClick = () => setSelected.Set(0) }),
        V.Button(new ButtonProps { Text = "1", OnClick = () => setSelected.Set(1) }),
        V.Button(new ButtonProps { Text = "2", OnClick = () => setSelected.Set(2) })
      ),
      V.Label(
        new LabelProps
        {
          Text = $"Selected option: {Options[selected]}",
          Style = StatusStyle,
        }
      )
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`ToggleButtonGroup`})]});var fb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const pb=()=>(0,L.jsxs)(W,{sx:fb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TextField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.TextField`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`TextField`}),` using`,` `,(0,L.jsx)(`code`,{children:`TextFieldProps`}),`, with support for slots like `,(0,L.jsx)(`code`,{children:`Label`}),`,`,` `,(0,L.jsx)(`code`,{children:`Input`}),`, and `,(0,L.jsx)(`code`,{children:`TextElement`}),`.`]}),(0,L.jsxs)(W,{sx:fb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`TextFieldProps`)})]}),(0,L.jsxs)(W,{sx:fb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class TextFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass TextFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState("Hello");

    void OnChange(ChangeEvent<string> evt)
    {
      setValue(evt.newValue);
    }

    var inputProps = new Dictionary<string, object>
    {
      { "style", InputStyle },
    };

    return V.TextField(
      new TextFieldProps
      {
        Value = value,
        Placeholder = "Type here...",
        Input = inputProps,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`TextField`})]});var mb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const hb=()=>(0,L.jsxs)(W,{sx:mb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Toolbar`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Toolbar`}),` and related helpers (`,(0,L.jsx)(`code`,{children:`V.ToolbarButton`}),`,`,` `,(0,L.jsx)(`code`,{children:`V.ToolbarToggle`}),`, `,(0,L.jsx)(`code`,{children:`V.ToolbarMenu`}),`, etc.) wrap the UI Toolkit editor toolbar elements using the `,(0,L.jsx)(`code`,{children:`ToolbarProps`}),` family.`]}),(0,L.jsxs)(W,{sx:mb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ToolbarProps`)})]}),(0,L.jsxs)(W,{sx:mb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class ToolbarExamples
{
  private static readonly Style ToolbarStyle = new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.Gap, 4f) };

  // Function component – pass ToolbarExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Toolbar(
      new ToolbarProps
      {
        Style = ToolbarStyle,
      },
      key: null,
      V.ToolbarButton(new ToolbarButtonProps { Text = "Action" }),
      V.ToolbarToggle(new ToolbarToggleProps { Text = "Toggle", Value = true }),
      V.ToolbarSpacer(new ToolbarSpacerProps()),
      V.ToolbarSearchField(new ToolbarSearchFieldProps { Value = "", }),
      V.ToolbarMenu(new ToolbarMenuProps { Text = "Menu" })
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Toolbar`})]});var gb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const _b=()=>(0,L.jsxs)(W,{sx:gb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RectField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.RectField`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`RectField`}),` control using`,` `,(0,L.jsx)(`code`,{children:`RectFieldProps`}),`. It is available in both runtime and editor UIs.`]}),(0,L.jsxs)(W,{sx:gb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`RectFieldProps`)})]}),(0,L.jsxs)(W,{sx:gb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class RectFieldExamples
{
  private static readonly Style VisualInputStyle = new Style
  {
    (StyleKeys.PaddingLeft, 4f),
  };

  // Function component – pass RectFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (rect, setRect) = Hooks.UseState(new Rect(0, 0, 128, 64));

    return V.RectField(
      new RectFieldProps
      {
        Value = rect,
        Label = new LabelProps { Text = "Rect" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", VisualInputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`RectField`})]});var vb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const yb=()=>(0,L.jsxs)(W,{sx:vb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RectIntField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.RectIntField`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`RectIntField`}),` control using`,` `,(0,L.jsx)(`code`,{children:`RectIntFieldProps`}),`. It is available in both runtime and editor UIs.`]}),(0,L.jsxs)(W,{sx:vb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`RectIntFieldProps`)})]}),(0,L.jsxs)(W,{sx:vb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class RectIntFieldExamples
{
  private static readonly Style VisualInputStyle = new Style
  {
    (StyleKeys.PaddingLeft, 4f),
  };

  // Function component – pass RectIntFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (rect, setRect) = Hooks.UseState(new RectInt(0, 0, 16, 16));

    return V.RectIntField(
      new RectIntFieldProps
      {
        Value = rect,
        Label = new LabelProps { Text = "RectInt" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", VisualInputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`RectIntField`})]});var bb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const xb=()=>(0,L.jsxs)(W,{sx:bb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UnsignedIntegerField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.UnsignedIntegerField`}),` represents a `,(0,L.jsx)(`code`,{children:`uint`}),` numeric field using`,` `,(0,L.jsx)(`code`,{children:`UnsignedIntegerFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:bb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`UnsignedIntegerFieldProps`)})]}),(0,L.jsxs)(W,{sx:bb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class UnsignedIntegerFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass UnsignedIntegerFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState<uint>(0u);

    void OnChange(ChangeEvent<uint> evt)
    {
      setValue(evt.newValue);
    }

    return V.UnsignedIntegerField(
      new UnsignedIntegerFieldProps
      {
        Value = value,
        OnChange = OnChange,
        Label = new LabelProps { Text = "Unsigned Int" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`UnsignedIntegerField`})]});var Sb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Cb=()=>(0,L.jsxs)(W,{sx:Sb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UnsignedLongField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.UnsignedLongField`}),` represents a `,(0,L.jsx)(`code`,{children:`ulong`}),` numeric field using`,` `,(0,L.jsx)(`code`,{children:`UnsignedLongFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:Sb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`UnsignedLongFieldProps`)})]}),(0,L.jsxs)(W,{sx:Sb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class UnsignedLongFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass UnsignedLongFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState<ulong>(0ul);

    void OnChange(ChangeEvent<ulong> evt)
    {
      setValue(evt.newValue);
    }

    return V.UnsignedLongField(
      new UnsignedLongFieldProps
      {
        Value = value,
        OnChange = OnChange,
        Label = new LabelProps { Text = "Unsigned Long" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`UnsignedLongField`})]});var wb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Tb=()=>(0,L.jsxs)(W,{sx:wb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector2Field`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Vector2Field`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Vector2Field`}),` control using`,` `,(0,L.jsx)(`code`,{children:`Vector2FieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:wb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`Vector2FieldProps`)})]}),(0,L.jsxs)(W,{sx:wb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector2FieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass Vector2FieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector2(1f, 2f));

    void OnChange(ChangeEvent<Vector2> evt)
    {
      setValue(evt.newValue);
    }

    return V.Vector2Field(
      new Vector2FieldProps
      {
        Value = value,
        OnChange = OnChange,
        Label = new LabelProps { Text = "Vector2" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Vector2Field`})]});var Eb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Db=()=>(0,L.jsxs)(W,{sx:Eb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector2IntField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Vector2IntField`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Vector2IntField`}),` control using`,` `,(0,L.jsx)(`code`,{children:`Vector2IntFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:Eb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`Vector2IntFieldProps`)})]}),(0,L.jsxs)(W,{sx:Eb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector2IntFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass Vector2IntFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector2Int(1, 2));

    return V.Vector2IntField(
      new Vector2IntFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Vector2Int" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Vector2IntField`})]});var Ob={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const kb=()=>(0,L.jsxs)(W,{sx:Ob.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector3Field`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Vector3Field`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Vector3Field`}),` control using`,` `,(0,L.jsx)(`code`,{children:`Vector3FieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:Ob.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`Vector3FieldProps`)})]}),(0,L.jsxs)(W,{sx:Ob.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector3FieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass Vector3FieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector3(1f, 2f, 3f));

    void OnChange(ChangeEvent<Vector3> evt)
    {
      setValue(evt.newValue);
    }

    return V.Vector3Field(
      new Vector3FieldProps
      {
        Value = value,
        OnChange = OnChange,
        Label = new LabelProps { Text = "Vector3" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Vector3Field`})]});var Ab={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const jb=()=>(0,L.jsxs)(W,{sx:Ab.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector3IntField`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Vector3IntField`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Vector3IntField`}),` control using`,` `,(0,L.jsx)(`code`,{children:`Vector3IntFieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:Ab.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`Vector3IntFieldProps`)})]}),(0,L.jsxs)(W,{sx:Ab.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector3IntFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass Vector3IntFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector3Int(1, 2, 3));

    return V.Vector3IntField(
      new Vector3IntFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Vector3Int" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Vector3IntField`})]});var Mb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Nb=()=>(0,L.jsxs)(W,{sx:Mb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector4Field`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Vector4Field`}),` wraps the UI Toolkit `,(0,L.jsx)(`code`,{children:`Vector4Field`}),` control using`,` `,(0,L.jsx)(`code`,{children:`Vector4FieldProps`}),`.`]}),(0,L.jsxs)(W,{sx:Mb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`Vector4FieldProps`)})]}),(0,L.jsxs)(W,{sx:Mb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector4FieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass Vector4FieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector4(1f, 2f, 3f, 4f));

    void OnChange(ChangeEvent<Vector4> evt)
    {
      setValue(evt.newValue);
    }

    return V.Vector4Field(
      new Vector4FieldProps
      {
        Value = value,
        OnChange = OnChange,
        Label = new LabelProps { Text = "Vector4" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Vector4Field`})]});var Pb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Fb=()=>(0,L.jsxs)(W,{sx:Pb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TemplateContainer`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.TemplateContainer`}),` wraps UI Toolkit `,(0,L.jsx)(`code`,{children:`TemplateContainer`}),` and exposes a`,` `,(0,L.jsx)(`code`,{children:`ContentContainer`}),` slot through `,(0,L.jsx)(`code`,{children:`TemplateContainerProps`}),`.`]}),(0,L.jsxs)(W,{sx:Pb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`TemplateContainerProps`)})]}),(0,L.jsxs)(W,{sx:Pb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class TemplateContainerExamples
{
  private static readonly Style ContentStyle = new Style
  {
    (StyleKeys.PaddingTop, 4f),
    (StyleKeys.PaddingBottom, 4f),
  };

  // Function component – pass TemplateContainerExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentStyle },
    };

    return V.TemplateContainer(
      new TemplateContainerProps
      {
        ContentContainer = contentContainerProps,
      },
      children
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`TemplateContainer`})]});var Ib={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Lb=()=>(0,L.jsxs)(W,{sx:Ib.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`VisualElement`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.VisualElement`}),` creates a generic container element styled via a `,(0,L.jsx)(`code`,{children:`Style`}),` `,`instance, and is often used as the top-level layout node for your component trees.`]}),(0,L.jsxs)(W,{sx:Ib.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signature`}),(0,L.jsx)(Z,{language:`tsx`,code:`public static VirtualNode VisualElement(
  Style style,
  string key = null,
  params VirtualNode[] children
);

public static VirtualNode VisualElement(
  IReadOnlyDictionary<string, object> elementProperties = null,
  string key = null,
  params VirtualNode[] children
);`})]}),(0,L.jsxs)(W,{sx:Ib.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic container`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;
using UnityEngine;

public static class VisualElementExamples
{
  private static readonly Style ContainerStyle = new Style
  {
    (StyleKeys.FlexDirection, FlexDirection.Column),
    (StyleKeys.PaddingLeft, 8f),
    (StyleKeys.PaddingTop, 4f),
    (StyleKeys.Gap, 4f),
  };

  // Function component – pass VisualElementExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.VisualElement(
      ContainerStyle,
      null,
      V.Label(new LabelProps { Text = "VisualElement container" }),
      V.Button(new ButtonProps { Text = "Click me" })
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`VisualElement`})]});var Rb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const zb=()=>(0,L.jsxs)(W,{sx:Rb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`VisualElementSafe`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.VisualElementSafe`}),` is a safe-area-aware variant of `,(0,L.jsx)(`code`,{children:`V.VisualElement`}),` `,`that merges its padding with safe-area insets from `,(0,L.jsx)(`code`,{children:`SafeAreaUtility`}),`. Use it as a top-level container on devices with notches or system UI overlays.`]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Pass either a `,(0,L.jsx)(`code`,{children:`Style`}),` or the same props dictionary you would send to`,` `,(0,L.jsx)(`code`,{children:`V.VisualElement`}),` (e.g., `,(0,L.jsx)(`code`,{children:`pickingMode`}),`, `,(0,L.jsx)(`code`,{children:`name`}),`, refs, event handlers). The helper clones those props, replaces/merges the `,(0,L.jsx)(`code`,{children:`style`}),` entry, and leaves everything else untouched.`]}),(0,L.jsxs)(W,{sx:Rb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signature`}),(0,L.jsx)(Z,{language:`tsx`,code:`public static VirtualNode VisualElementSafe(
  object elementPropsOrStyle = null,
  string key = null,
  params VirtualNode[] children
);`})]}),(0,L.jsxs)(W,{sx:Rb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Safe-area aware container`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;
using UnityEngine;

public static class VisualElementSafeExamples
{
  private static readonly Style SafeStyle = new Style
  {
    (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.15f, 1f)),
  };

  // Function component – pass VisualElementSafeExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    // VisualElementSafe merges user padding with safe-area insets.
    // You can pass either a Style or a props dictionary.
    return V.VisualElementSafe(new Dictionary<string, object>
      {
        { "pickingMode", PickingMode.Ignore },
        { "style", SafeStyle },
      },
      null,
      V.Label(new LabelProps { Text = "Safe-area aware root" })
    );
  }
}`})]})]});var Bb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Vb=()=>(0,L.jsxs)(W,{sx:Bb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Animate`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Animate`}),` wraps a child subtree and drives one or more animation tracks on its root `,(0,L.jsx)(`code`,{children:`VisualElement`}),`. It is a thin, declarative wrapper around`,` `,(0,L.jsx)(`code`,{children:`Hooks.UseAnimate`}),` and the underlying `,(0,L.jsx)(`code`,{children:`Animator`}),` helpers.`]}),(0,L.jsxs)(W,{sx:Bb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`AnimateProps`)})]}),(0,L.jsxs)(W,{sx:Bb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,L.jsxs)(G,{sx:Bb.section,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Tracks are defined via AnimateTrack helpers and target individual style properties (for example, backgroundColor, opacity, size).`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Each track specifies from/to values, duration, easing, and optional delay.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`When the Animate node mounts or its dependencies change, tracks are played; they are stopped and cleaned up automatically when unmounting.`})})]})]}),(0,L.jsxs)(W,{sx:Bb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class AnimateExamples
{
  private static readonly Style AnimatedBoxStyle = new Style
  {
    (StyleKeys.Width, 120f),
    (StyleKeys.Height, 32f),
    (StyleKeys.AlignItems, Align.Center),
    (StyleKeys.JustifyContent, Justify.Center),
  };

  // Function component – pass AnimateExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var tracks = new List<AnimateTrack>
    {
      AnimateTrack.Property(
        property: StyleKeys.BackgroundColor,
        from: Color.gray,
        to: Color.cyan,
        durationSeconds: 0.75f,
        easing: Easing.InOutQuad
      ),
    };

    return V.Animate(
      new AnimateProps
      {
        Tracks = tracks,
      },
      null,
      V.Box(
        new BoxProps
        {
          Style = AnimatedBoxStyle,
        },
        V.Label(new LabelProps { Text = "Animated box" })
      )
    );
  }
}`})]})]});var Hb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ub=()=>(0,L.jsxs)(W,{sx:Hb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ErrorBoundary`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.ErrorBoundary`}),` catches exceptions from its descendants and renders the`,` `,(0,L.jsx)(`code`,{children:`Fallback`}),` `,(0,L.jsx)(`code`,{children:`VirtualNode`}),` from `,(0,L.jsx)(`code`,{children:`ErrorBoundaryProps`}),`.`]}),(0,L.jsxs)(W,{sx:Hb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ErrorBoundaryProps`)})]}),(0,L.jsxs)(W,{sx:Hb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class ErrorBoundaryExamples
{
  private static readonly Style FallbackBoxStyle = new Style
  {
    (StyleKeys.PaddingLeft, 8f),
    (StyleKeys.PaddingTop, 4f),
  };

  // Function component – pass ErrorBoundaryExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var fallback = V.Box(
      new BoxProps
      {
        Style = FallbackBoxStyle,
      },
      V.Label(new LabelProps { Text = "Something went wrong." })
    );

    void OnError(Exception ex)
    {
      UnityEngine.Debug.LogException(ex);
    }

    return V.ErrorBoundary(
      new ErrorBoundaryProps
      {
        Fallback = fallback,
        OnError = OnError,
      },
      V.Func(
        (p, c) =>
        {
          var (value, setValue) = Hooks.UseState(0);
          if (value > 3)
          {
            throw new InvalidOperationException("Demo error");
          }
          return V.Button(
            new ButtonProps
            {
              Text = $"Clicks: {value}",
              OnClick = _ => setValue(prev => prev + 1),
            }
          );
        }
      )
    );
  }
}`})]})]});var Wb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Gb=()=>(0,L.jsxs)(W,{sx:Wb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MultiColumnListView`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.MultiColumnListView`}),` displays tabular data with columns configured via`,` `,(0,L.jsx)(`code`,{children:`MultiColumnListViewProps`}),`. It is backed by Unity's`,` `,(0,L.jsx)(`code`,{children:`MultiColumnListView`}),` control and supports large, virtualized data sets.`]}),(0,L.jsxs)(W,{sx:Wb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`MultiColumnListViewProps`)})]}),(0,L.jsxs)(W,{sx:Wb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,L.jsxs)(G,{sx:Wb.section,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Items are provided as an IList; rows are virtualized by the underlying control for performance.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Columns are defined via MultiColumnListViewColumn objects, each with a name, width, and Cell callback.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`The Cell callback receives the strongly-typed row item and index so you can render arbitrary content per column.`})})]})]}),(0,L.jsxs)(W,{sx:Wb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class MultiColumnListViewExamples
{
  private sealed class Row
  {
    public string Name;
    public int Value;
  }

  // Function component – pass MultiColumnListViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (rows, setRows) = Hooks.UseState(new List<Row>
    {
      new Row { Name = "One", Value = 1 },
      new Row { Name = "Two", Value = 2 },
      new Row { Name = "Three", Value = 3 },
    });

    var columns = new List<MultiColumnListViewColumn>
    {
      new MultiColumnListViewColumn
      {
        Name = "Name",
        Width = 160f,
        Cell = (item, index) => V.Label(new LabelProps { Text = item.Name }),
      },
      new MultiColumnListViewColumn
      {
        Name = "Value",
        Width = 80f,
        Cell = (item, index) => V.Label(new LabelProps { Text = item.Value.ToString() }),
      },
    };

    return V.MultiColumnListView(
      new MultiColumnListViewProps
      {
        Items = rows,
        Columns = columns,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`MultiColumnListView`})]});var Kb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const qb=()=>(0,L.jsxs)(W,{sx:Kb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MultiColumnTreeView`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.MultiColumnTreeView`}),` renders hierarchical data across multiple columns via`,` `,(0,L.jsx)(`code`,{children:`MultiColumnTreeViewProps`}),`. It is backed by Unity's`,` `,(0,L.jsx)(`code`,{children:`MultiColumnTreeView`}),` control and is suitable for project browser–style views.`]}),(0,L.jsxs)(W,{sx:Kb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`MultiColumnTreeViewProps`)})]}),(0,L.jsxs)(W,{sx:Kb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,L.jsxs)(G,{sx:Kb.section,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Items are provided as a tree of nodes; the adapter flattens and expands them based on TreeView state.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Columns are defined via MultiColumnTreeViewColumn objects, just like MultiColumnListView.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Each Cell callback receives the node item and index so you can render per-column content (labels, badges, icons).`})})]})]}),(0,L.jsxs)(W,{sx:Kb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class MultiColumnTreeViewExamples
{
  private sealed class Node
  {
    public string Name;
    public int Depth;
    public IList<Node> Children;
  }

  // Function component – pass MultiColumnTreeViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var root = new Node
    {
      Name = "Root",
      Depth = 0,
      Children = new List<Node>
      {
        new Node { Name = "Child A", Depth = 1 },
        new Node { Name = "Child B", Depth = 1 },
      },
    };

    var nodes = new List<Node> { root };

    var columns = new List<MultiColumnTreeViewColumn>
    {
      new MultiColumnTreeViewColumn
      {
        Name = "Name",
        Width = 200f,
        Cell = (item, index) => V.Label(new LabelProps { Text = item.Name }),
      },
      new MultiColumnTreeViewColumn
      {
        Name = "Depth",
        Width = 80f,
        Cell = (item, index) => V.Label(new LabelProps { Text = item.Depth.ToString() }),
      },
    };

    return V.MultiColumnTreeView(
      new MultiColumnTreeViewProps
      {
        Items = nodes,
        Columns = columns,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`MultiColumnTreeView`})]});var Jb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Yb=()=>(0,L.jsxs)(W,{sx:Jb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Scroller`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.Scroller`}),` wraps the low-level UI Toolkit `,(0,L.jsx)(`code`,{children:`Scroller`}),` element using`,` `,(0,L.jsx)(`code`,{children:`ScrollerProps`}),`.`]}),(0,L.jsxs)(W,{sx:Jb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`ScrollerProps`)})]}),(0,L.jsxs)(W,{sx:Jb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class ScrollerExamples
{
  private static readonly Style ScrollerStyle = new Style
  {
    (StyleKeys.Width, 12f),
    (StyleKeys.Height, 120f),
  };

  // Function component – pass ScrollerExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0f);

    void OnChange(ChangeEvent<float> evt)
    {
      setValue(evt.newValue);
    }

    return V.Scroller(
      new ScrollerProps
      {
        LowValue = 0f,
        HighValue = 100f,
        Value = value,
        Style = ScrollerStyle,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`Scroller`})]});var Xb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Zb=()=>(0,L.jsxs)(W,{sx:Xb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TextElement`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`V.TextElement`}),` is a low-level text node wrapper using `,(0,L.jsx)(`code`,{children:`TextElementProps`}),`.`]}),(0,L.jsxs)(W,{sx:Xb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`TextElementProps`)})]}),(0,L.jsxs)(W,{sx:Xb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class TextElementExamples
{
  private static readonly Style BoldTextStyle = new Style
  {
    (StyleKeys.UnityFontStyleAndWeight, FontStyle.Bold),
  };

  // Function component – pass TextElementExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.TextElement(
      new TextElementProps
      {
        Text = "Inline text element",
        Style = BoldTextStyle,
      }
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`TextElement`})]});var Qb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const $b=()=>(0,L.jsxs)(W,{sx:Qb.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`PropertyField & InspectorElement`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Editor-only helpers that wrap Unity's `,(0,L.jsx)(`code`,{children:`PropertyField`}),` and `,(0,L.jsx)(`code`,{children:`InspectorElement`}),` `,`via `,(0,L.jsx)(`code`,{children:`PropertyFieldProps`}),` and `,(0,L.jsx)(`code`,{children:`InspectorElementProps`}),`.`]}),(0,L.jsxs)(W,{sx:Qb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`PropertyInspectorProps`)})]}),(0,L.jsxs)(W,{sx:Qb.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

// Editor-only usage
public static class PropertyInspectorExamples
{
  private static readonly Style InspectorBoxStyle = new Style
  {
    (StyleKeys.FlexDirection, FlexDirection.Row),
    (StyleKeys.Gap, 4f),
  };

  // Function component – pass PropertyInspectorExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (target, setTarget) = Hooks.UseState<Object>(null);

    return V.Box(
      new BoxProps
      {
        Style = InspectorBoxStyle,
      },
      V.PropertyField(
        new PropertyFieldProps
        {
          Target = target,
          BindingPath = "m_Name",
          Label = "Name",
        }
      ),
      V.InspectorElement(
        new InspectorElementProps
        {
          Target = target,
        }
      )
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`PropertyInspector`})]});var ex={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const tx=()=>(0,L.jsxs)(W,{sx:ex.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TwoPaneSplitView`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Editor-only splitter layout wrapping Unity's `,(0,L.jsx)(`code`,{children:`TwoPaneSplitView`}),` via`,` `,(0,L.jsx)(`code`,{children:`TwoPaneSplitViewProps`}),`.`]}),(0,L.jsxs)(W,{sx:ex.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,L.jsx)(Z,{language:`tsx`,code:Q(`TwoPaneSplitViewProps`)})]}),(0,L.jsxs)(W,{sx:ex.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

// Editor-only usage
public static class TwoPaneSplitViewExamples
{
  // Function component – pass TwoPaneSplitViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.TwoPaneSplitView(
      new TwoPaneSplitViewProps
      {
        FixedPaneIndex = 0,
        FixedPaneInitialDimension = 220f,
        Orientation = "horizontal",
      },
      V.Box(new BoxProps(), V.Label(new LabelProps { Text = "Pane 1" })),
      V.Box(new BoxProps(), V.Label(new LabelProps { Text = "Pane 2" }))
    );
  }
}`})]}),(0,L.jsx)($,{componentName:`TwoPaneSplitView`})]});var nx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const rx=()=>(0,L.jsxs)(W,{sx:nx.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Known Issues`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:nx.section,children:`Runtime`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`There is a known issue where `,(0,L.jsx)(`code`,{children:`MultiColumnListView`}),` can briefly jump or snap when scrolling large data sets; this will be addressed in a future update.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:nx.section,children:`Burst AOT & Assembly Resolution`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`If you encounter the error:`}),(0,L.jsx)(Z,{language:`text`,code:`Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: Assembly-CSharp-Editor`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Go to `,(0,L.jsx)(`strong`,{children:`Edit → Project Settings → Burst AOT Settings`}),` and add `,(0,L.jsx)(`code`,{children:`Assembly-CSharp-Editor`}),` to the exclusion list. This prevents Burst from trying to AOT-compile editor-only assemblies that reference UITKX types.`]})]});var ix={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const ax=()=>(0,L.jsxs)(W,{sx:ix.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Roadmap`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`The roadmap will be documented here in a future update.`})]});var ox={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},sx=`using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class AnimateWithHook
{
  // Function component – pass AnimateWithHook.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var tracks = new[]
    {
      AnimateTrack.Property(
        property: StyleKeys.BackgroundColor,
        from: Color.gray,
        to: Color.cyan,
        durationSeconds: 0.75f,
        easing: Easing.InOutQuad
      ),
    };

    Hooks.UseAnimate(tracks);

    return V.Box(
      new BoxProps
      {
        Style = new Style { (StyleKeys.Width, 120f), (StyleKeys.Height, 32f) },
      },
      V.Label(new LabelProps { Text = "Animated box (UseAnimate)" })
    );
  }
}`,cx=`using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class TweenFloatExamples
{
  // Function component – pass TweenFloatExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    float current = 0f;

    Hooks.UseTweenFloat(
      from: 0f,
      to: 1f,
      duration: 1.0f,
      ease: Ease.InOutQuad,
      delay: 0f,
      onUpdate: value => current = value,
      onComplete: () => Debug.Log($"Tween finished at {current:0.00}")
    );

    return V.Label(new LabelProps { Text = $"Tween value: {current:0.00}" });
  }
}`;const lx=()=>(0,L.jsxs)(W,{sx:ox.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special animation hooks`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`ReactiveUIToolKit exposes animation-specific hooks that do not exist in React's core API. These hooks are designed to drive UI Toolkit animations in a frame-accurate way while still fitting into the normal function component lifecycle.`}),(0,L.jsxs)(W,{sx:ox.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,L.jsx)(`code`,{children:`Hooks.UseAnimate`})}),(0,L.jsxs)(G,{sx:ox.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Starts one or more AnimateTrack definitions on the component's VisualElement container.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Tracks are created with ReactiveUITK.Core.Animation.AnimateTrack helpers (for example, animating background color or size).`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Plays animations when dependencies change, and stops/cleans them up when the component unmounts or the effect is re-run.`})})]}),(0,L.jsx)(Z,{language:`tsx`,code:sx})]}),(0,L.jsxs)(W,{sx:ox.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,L.jsx)(`code`,{children:`Hooks.UseTweenFloat`})}),(0,L.jsxs)(G,{sx:ox.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Tweens a single float value over time with easing and an optional delay.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Calls an onUpdate callback every frame with the eased value, and an onComplete callback when finished.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Uses UI Toolkit's scheduler and integrates with the component's lifecycle; cancelling on unmount.`})})]}),(0,L.jsx)(Z,{language:`tsx`,code:cx})]}),(0,L.jsxs)(U,{variant:`body2`,sx:ox.section,children:[`For a higher-level API, see the `,(0,L.jsx)(`code`,{children:`Animate`}),` component documented under Components → Common/Uncommon Components. It builds on top of these hooks and the underlying`,` `,(0,L.jsx)(`code`,{children:`Animator`}),` utilities.`]})]});var ux={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},dx=`using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;

// Demonstrates RouterHooks.UseNavigate, UseParams, and UseQuery.
public static class RouterHooksDemoFunc
{
  // Function component – pass RouterHooksDemoFunc.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var navigate = RouterHooks.UseNavigate();
    var parameters = RouterHooks.UseParams();
    var query = RouterHooks.UseQuery();

    string userId = parameters.TryGetValue("id", out var id) ? id : "(none)";

    void ToUser42()
    {
      navigate("/users/42?tab=details");
    }

    return V.Column(
      key: null,
      V.Row(
        key: "actions",
        V.Button(new ButtonProps { Text = "Go to User 42", OnClick = ToUser42 })
      ),
      V.Label(new LabelProps { Text = $"User id param: {userId}" }),
      V.Label(new LabelProps { Text = $"Query keys: {string.Join(", ", query.Keys)}" })
    );
  }
}`;const fx=()=>(0,L.jsxs)(W,{sx:ux.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special router hooks`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`The router in ReactiveUIToolKit ships with a set of hooks that mirror React Router's ergonomics but are implemented entirely in C# for Unity UI Toolkit.`}),(0,L.jsxs)(W,{sx:ux.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Reading router state`}),(0,L.jsxs)(G,{sx:ux.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),` / `,(0,L.jsx)(`code`,{children:`UseLocationInfo()`}),` – current path, query, and optional navigation state.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` – path parameters extracted from the active route template.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` – parsed query-string key/value pairs.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` – arbitrary state object provided when navigating.`]})})})]})]}),(0,L.jsxs)(W,{sx:ux.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Navigation helpers`}),(0,L.jsxs)(G,{sx:ux.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigate(replace = false)`}),` – imperative navigation, similar to React Router's `,(0,L.jsx)(`code`,{children:`useNavigate`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseGo()`}),` – navigate relative to the history stack (for example, `,(0,L.jsx)(`code`,{children:`go(-1)`}),`, `,(0,L.jsx)(`code`,{children:`go(1)`}),`).`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseCanGo(delta)`}),` – returns whether a given delta is available for back/forward UI.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseBlocker(blocker, enabled)`}),` – intercepts transitions to implement confirmation prompts.`]})})})]}),(0,L.jsx)(Z,{language:`tsx`,code:dx})]}),(0,L.jsxs)(U,{variant:`body2`,sx:ux.section,children:[`See the main Router documentation for complete examples of composing `,(0,L.jsx)(`code`,{children:`V.Router`}),`,`,` `,(0,L.jsx)(`code`,{children:`V.Route`}),`, `,(0,L.jsx)(`code`,{children:`V.Link`}),`, and these hooks in editor and runtime apps.`]})]});var px={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},mx=`using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Signals;

public static class SignalHooksDemoFunc
{
  private static readonly Signal<int> CounterSignal =
    Signals.Get<int>("demo.counter", 0);

  // Function component – pass SignalHooksDemoFunc.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    int value = Hooks.UseSignal(CounterSignal);

    void Increment()
    {
      CounterSignal.Dispatch(v => v + 1);
    }

    return V.Column(
      key: null,
      V.Label(new LabelProps { Text = $"Count from signal: {value}" }),
      V.Button(new ButtonProps { Text = "Increment", OnClick = Increment })
    );
  }
}`;const hx=()=>(0,L.jsxs)(W,{sx:px.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special signal hooks`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Signals provide a small, global, observable state primitive. The`,` `,(0,L.jsx)(`code`,{children:`Hooks.UseSignal`}),` family gives you fine-grained reactivity from function components, something React does not have out of the box.`]}),(0,L.jsxs)(W,{sx:px.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,L.jsx)(`code`,{children:`Hooks.UseSignal`})}),(0,L.jsxs)(G,{sx:px.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`Hooks.UseSignal(Signal<T>)`}),` – subscribe to a`,` `,(0,L.jsx)(`code`,{children:`Signal<T>`}),` and re-render when it changes.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`Hooks.UseSignal<T>(key, initialValue)`}),` – shorthand that resolves a`,` `,(0,L.jsx)(`code`,{children:`Signal<T>`}),` from the global registry by key.`]})})})]}),(0,L.jsx)(Z,{language:`tsx`,code:mx})]}),(0,L.jsxs)(W,{sx:px.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Selector overloads`}),(0,L.jsxs)(G,{sx:px.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(signal, selector, comparer)`}),` – project a slice of a signal value and control equality.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(key, selector, comparer, initialValue)`}),` `,`– keyed variant that creates/resolves the signal for you.`]})})})]})]}),(0,L.jsxs)(U,{variant:`body2`,sx:px.section,children:[`For an end-to-end walkthrough, see the Signals page, which shows how to combine`,` `,(0,L.jsx)(`code`,{children:`Signals.Get`}),`, `,(0,L.jsx)(`code`,{children:`Hooks.UseSignal`}),`, and dispatch helpers in real UIs.`]})]});var gx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},_x=`using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Core.Util;
using UnityEngine;

public static class SafeAreaHooksDemoFunc
{
  // Function component – pass SafeAreaHooksDemoFunc.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    // Read current insets (top, bottom, left, right)
    SafeAreaInsets insets = Hooks.UseSafeArea();

    var style = new Style
    {
      (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.15f, 1f)),
    };

    return V.VisualElementSafe(
      style,
      key: null,
      V.Label(new LabelProps { Text = $"Safe area: top={insets.Top:0}, bottom={insets.Bottom:0}" })
    );
  }
}`;const vx=()=>(0,L.jsxs)(W,{sx:gx.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Safe area hooks`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`When targeting mobile or platforms with notches and system insets, the`,` `,(0,L.jsx)(`code`,{children:`Hooks.UseSafeArea`}),` hook and `,(0,L.jsx)(`code`,{children:`V.VisualElementSafe`}),` helper work together to keep your layout inside the safe region.`]}),(0,L.jsxs)(W,{sx:gx.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,L.jsx)(`code`,{children:`Hooks.UseSafeArea`})}),(0,L.jsxs)(G,{sx:gx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Returns SafeAreaInsets (top, bottom, left, right) based on Unity's Screen.safeArea.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Records a hook usage so that changes to the safe area can trigger re-rendering.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Accepts an optional tolerance parameter to avoid flicker when the reported insets change only slightly.`})})]}),(0,L.jsx)(Z,{language:`tsx`,code:_x})]}),(0,L.jsxs)(W,{sx:gx.section,children:[(0,L.jsxs)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:[(0,L.jsx)(`code`,{children:`V.VisualElementSafe`}),` helper`]}),(0,L.jsxs)(G,{sx:gx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`V.VisualElementSafe(propsOrStyle, key, children) – takes either a Style or a props dictionary, wraps a VisualElement, and automatically applies padding based on SafeAreaInsets.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Merges your own padding with the safe-area padding so you keep control over layout while staying visible on all devices.`})})]})]}),(0,L.jsxs)(U,{variant:`body2`,sx:gx.section,children:[`Combine `,(0,L.jsx)(`code`,{children:`Hooks.UseSafeArea`}),` when you need direct access to inset values with`,` `,(0,L.jsx)(`code`,{children:`V.VisualElementSafe`}),` when you want a drop-in, safe-area-aware container.`]})]}),yx=[{id:`intro`,title:`Introduction`,pages:[{id:`introduction`,title:`Introduction`,path:`/`,keywords:[`overview`,`unity 6.2`,`reactive`,`ui toolkit`],searchContent:`reactiveuitoolkit react-like component model unity hooks state effects reconciliation v.func v.memo visualelement diffing scheduler rendering fiber dom-like`,element:()=>(0,L.jsx)(B_,{})}]},{id:`getting-started`,title:`Getting Started`,pages:[{id:`install`,title:`Install & Setup`,path:`/getting-started`,keywords:[`install`,`setup`,`unity package manager`,`dist`],searchContent:`install setup unity package manager git url create component mount rootrenderer uidocument rootvisualelement editorrootrendererutility partial class render method v.func v.memo`,element:()=>(0,L.jsx)(zv,{})}]},{id:`concepts`,title:`Concepts & Environment`,pages:[{id:`concepts-and-environment`,title:`Concepts & Environment`,path:`/concepts`,keywords:[`concepts`,`environment`,`defines`,`trace`,`react differences`],searchContent:`concepts environment defines tracings symbols env_dev env_staging env_prod ruitk_trace_verbose ruitk_trace_basic runtime diagnostics intrinsic tags hooks markup reconciliation scheduling companion partial classes`,element:()=>(0,L.jsx)(Gv,{})}]},{id:`differences`,title:`Different from React`,pages:[{id:`different-from-react`,title:`Different from React`,path:`/differences`,keywords:[`react`,`usestate`,`signals`,`differences`],searchContent:`different from react usestate setter value updater function fiber schedule asynchronously scheduler sliced render deferred passive effects concurrent jsx-like syntax visualelement interop unity controls styles events`,element:()=>(0,L.jsx)(qv,{})}]},{id:`tooling`,title:`Tooling`,pages:[{id:`router`,title:`Router`,path:`/tooling/router`,keywords:[`navigation`,`routes`],searchContent:`router routing route links routerhooks usenavigate usego useparams usequery uselocationinfo usenavigationstate useblocker declarative route composition navigation routernavlink`,element:()=>(0,L.jsx)(Vv,{})},{id:`signals`,title:`Signals`,path:`/tooling/signals`,keywords:[`state`,`observable`],searchContent:`signals shared-state primitive signal registry usesignal dispatch signalfactory.get signalsruntime.ensureinitialized signals.get counter increment`,element:()=>(0,L.jsx)(Uv,{})}]},{id:`components`,title:`Components`,pages:[{id:`component-bounds-field`,title:`BoundsField`,path:`/components/bounds-field`,keywords:[`bounds`,`field`,`BoundsField`],group:`advanced`,element:()=>(0,L.jsx)($v,{})},{id:`component-bounds-int-field`,title:`BoundsIntField`,path:`/components/bounds-int-field`,keywords:[`boundsint`,`field`,`BoundsIntField`],element:()=>(0,L.jsx)(ty,{})},{id:`component-box`,title:`Box`,path:`/components/box`,keywords:[`box`,`container`],group:`basic`,element:()=>(0,L.jsx)(ry,{})},{id:`component-button`,title:`Button`,path:`/components/button`,keywords:[`button`,`click`],group:`basic`,element:()=>(0,L.jsx)(ay,{})},{id:`component-color-field`,title:`ColorField`,path:`/components/color-field`,keywords:[`color`,`field`,`ColorField`],group:`advanced`,element:()=>(0,L.jsx)(sy,{})},{id:`component-double-field`,title:`DoubleField`,path:`/components/double-field`,keywords:[`double`,`field`,`DoubleField`],group:`advanced`,element:()=>(0,L.jsx)(ly,{})},{id:`component-dropdown-field`,title:`DropdownField`,path:`/components/dropdown-field`,keywords:[`dropdown`,`field`,`choices`],group:`basic`,element:()=>(0,L.jsx)(dy,{})},{id:`component-enum-field`,title:`EnumField`,path:`/components/enum-field`,keywords:[`enum`,`field`,`EnumField`],group:`basic`,element:()=>(0,L.jsx)(py,{})},{id:`component-enum-flags-field`,title:`EnumFlagsField`,path:`/components/enum-flags-field`,keywords:[`enum`,`flags`,`EnumFlagsField`],group:`advanced`,element:()=>(0,L.jsx)(hy,{})},{id:`component-float-field`,title:`FloatField`,path:`/components/float-field`,keywords:[`float`,`field`,`FloatField`],group:`basic`,element:()=>(0,L.jsx)(_y,{})},{id:`component-foldout`,title:`Foldout`,path:`/components/foldout`,keywords:[`foldout`,`toggle`,`collapsible`],group:`basic`,element:()=>(0,L.jsx)(yy,{})},{id:`component-group-box`,title:`GroupBox`,path:`/components/group-box`,keywords:[`group`,`groupbox`],group:`basic`,element:()=>(0,L.jsx)(xy,{})},{id:`component-hash128-field`,title:`Hash128Field`,path:`/components/hash128-field`,keywords:[`hash128`,`field`],group:`advanced`,element:()=>(0,L.jsx)(Cy,{})},{id:`component-help-box`,title:`HelpBox`,path:`/components/help-box`,keywords:[`helpbox`,`message`],group:`basic`,element:()=>(0,L.jsx)(Ty,{})},{id:`component-imgui-container`,title:`IMGUIContainer`,path:`/components/imgui-container`,keywords:[`imgui`,`editor`],group:`advanced`,element:()=>(0,L.jsx)(Dy,{})},{id:`component-image`,title:`Image`,path:`/components/image`,keywords:[`image`,`texture`,`sprite`],group:`basic`,element:()=>(0,L.jsx)(ky,{})},{id:`component-integer-field`,title:`IntegerField`,path:`/components/integer-field`,keywords:[`integer`,`field`,`int`],group:`basic`,element:()=>(0,L.jsx)(jy,{})},{id:`component-label`,title:`Label`,path:`/components/label`,keywords:[`label`,`text`],group:`basic`,element:()=>(0,L.jsx)(Ny,{})},{id:`component-long-field`,title:`LongField`,path:`/components/long-field`,keywords:[`long`,`field`,`LongField`],group:`advanced`,element:()=>(0,L.jsx)(Fy,{})},{id:`component-progress-bar`,title:`ProgressBar`,path:`/components/progress-bar`,keywords:[`progress`,`bar`],group:`basic`,element:()=>(0,L.jsx)(Ly,{})},{id:`component-list-view`,title:`ListView`,path:`/components/list-view`,keywords:[`list`,`ListView`],group:`basic`,element:()=>(0,L.jsx)(zy,{})},{id:`component-minmax-slider`,title:`MinMaxSlider`,path:`/components/minmax-slider`,keywords:[`minmax`,`slider`],group:`advanced`,element:()=>(0,L.jsx)(Vy,{})},{id:`component-object-field`,title:`ObjectField`,path:`/components/object-field`,keywords:[`object`,`field`],group:`advanced`,element:()=>(0,L.jsx)(Uy,{})},{id:`component-radio-button`,title:`RadioButton`,path:`/components/radio-button`,keywords:[`radio`,`button`],group:`basic`,element:()=>(0,L.jsx)(Gy,{})},{id:`component-radio-button-group`,title:`RadioButtonGroup`,path:`/components/radio-button-group`,keywords:[`radio`,`group`],group:`basic`,element:()=>(0,L.jsx)(qy,{})},{id:`component-rect-field`,title:`RectField`,path:`/components/rect-field`,keywords:[`rect`,`field`],group:`advanced`,element:()=>(0,L.jsx)(_b,{})},{id:`component-rect-int-field`,title:`RectIntField`,path:`/components/rect-int-field`,keywords:[`rectint`,`field`],group:`advanced`,element:()=>(0,L.jsx)(yb,{})},{id:`component-repeat-button`,title:`RepeatButton`,path:`/components/repeat-button`,keywords:[`repeat`,`button`],group:`basic`,element:()=>(0,L.jsx)(Yy,{})},{id:`component-scroll-view`,title:`ScrollView`,path:`/components/scroll-view`,keywords:[`scroll`,`view`],group:`basic`,element:()=>(0,L.jsx)(Zy,{})},{id:`component-slider`,title:`Slider`,path:`/components/slider`,keywords:[`slider`,`float`],group:`basic`,element:()=>(0,L.jsx)($y,{})},{id:`component-slider-int`,title:`SliderInt`,path:`/components/slider-int`,keywords:[`slider`,`int`],group:`basic`,element:()=>(0,L.jsx)(tb,{})},{id:`component-toggle`,title:`Toggle`,path:`/components/toggle`,keywords:[`toggle`,`checkbox`],group:`basic`,element:()=>(0,L.jsx)(rb,{})},{id:`component-tree-view`,title:`TreeView`,path:`/components/tree-view`,keywords:[`tree`,`TreeView`],group:`basic`,element:()=>(0,L.jsx)(ab,{})},{id:`component-tab`,title:`Tab`,path:`/components/tab`,keywords:[`tab`],group:`basic`,element:()=>(0,L.jsx)(sb,{})},{id:`component-tab-view`,title:`TabView`,path:`/components/tab-view`,keywords:[`tab`,`TabView`],group:`basic`,element:()=>(0,L.jsx)(lb,{})},{id:`component-toggle-button-group`,title:`ToggleButtonGroup`,path:`/components/toggle-button-group`,keywords:[`toggle`,`buttons`,`group`],group:`advanced`,element:()=>(0,L.jsx)(db,{})},{id:`component-text-field`,title:`TextField`,path:`/components/text-field`,keywords:[`text`,`field`],group:`basic`,element:()=>(0,L.jsx)(pb,{})},{id:`component-toolbar`,title:`Toolbar`,path:`/components/toolbar`,keywords:[`toolbar`,`editor`],group:`advanced`,element:()=>(0,L.jsx)(hb,{})},{id:`component-template-container`,title:`TemplateContainer`,path:`/components/template-container`,keywords:[`template`,`container`],group:`advanced`,element:()=>(0,L.jsx)(Fb,{})},{id:`component-visual-element`,title:`VisualElement`,path:`/components/visual-element`,keywords:[`visualelement`,`container`,`safe`],group:`basic`,element:()=>(0,L.jsx)(Lb,{})},{id:`component-visual-element-safe`,title:`VisualElementSafe`,path:`/components/visual-element-safe`,keywords:[`visualelementsafe`,`safe-area`,`container`],group:`basic`,element:()=>(0,L.jsx)(zb,{})},{id:`component-unsigned-integer-field`,title:`UnsignedIntegerField`,path:`/components/unsigned-integer-field`,keywords:[`uint`,`field`],group:`advanced`,element:()=>(0,L.jsx)(xb,{})},{id:`component-unsigned-long-field`,title:`UnsignedLongField`,path:`/components/unsigned-long-field`,keywords:[`ulong`,`field`],group:`advanced`,element:()=>(0,L.jsx)(Cb,{})},{id:`component-vector2-field`,title:`Vector2Field`,path:`/components/vector2-field`,keywords:[`vector2`,`field`],group:`advanced`,element:()=>(0,L.jsx)(Tb,{})},{id:`component-vector2-int-field`,title:`Vector2IntField`,path:`/components/vector2-int-field`,keywords:[`vector2int`,`field`],group:`advanced`,element:()=>(0,L.jsx)(Db,{})},{id:`component-vector3-field`,title:`Vector3Field`,path:`/components/vector3-field`,keywords:[`vector3`,`field`],group:`advanced`,element:()=>(0,L.jsx)(kb,{})},{id:`component-vector3-int-field`,title:`Vector3IntField`,path:`/components/vector3-int-field`,keywords:[`vector3int`,`field`],group:`advanced`,element:()=>(0,L.jsx)(jb,{})},{id:`component-vector4-field`,title:`Vector4Field`,path:`/components/vector4-field`,keywords:[`vector4`,`field`],group:`advanced`,element:()=>(0,L.jsx)(Nb,{})},{id:`component-animate`,title:`Animate`,path:`/components/animate`,keywords:[`animate`,`animation`],group:`basic`,element:()=>(0,L.jsx)(Vb,{})},{id:`component-error-boundary`,title:`ErrorBoundary`,path:`/components/error-boundary`,keywords:[`error`,`boundary`],group:`advanced`,element:()=>(0,L.jsx)(Ub,{})},{id:`component-multi-column-list-view`,title:`MultiColumnListView`,path:`/components/multi-column-list-view`,keywords:[`list`,`multi`,`columns`],group:`basic`,element:()=>(0,L.jsx)(Gb,{})},{id:`component-multi-column-tree-view`,title:`MultiColumnTreeView`,path:`/components/multi-column-tree-view`,keywords:[`tree`,`multi`,`columns`],group:`basic`,element:()=>(0,L.jsx)(qb,{})},{id:`component-scroller`,title:`Scroller`,path:`/components/scroller`,keywords:[`scroller`],group:`advanced`,element:()=>(0,L.jsx)(Yb,{})},{id:`component-text-element`,title:`TextElement`,path:`/components/text-element`,keywords:[`text`,`TextElement`],group:`advanced`,element:()=>(0,L.jsx)(Zb,{})},{id:`component-property-inspector`,title:`PropertyField & InspectorElement`,path:`/components/property-inspector`,keywords:[`propertyfield`,`inspectorelement`,`editor`],group:`advanced`,element:()=>(0,L.jsx)($b,{})},{id:`component-two-pane-split-view`,title:`TwoPaneSplitView`,path:`/components/two-pane-split-view`,keywords:[`split`,`editor`],group:`advanced`,element:()=>(0,L.jsx)(tx,{})}]},{id:`special-hooks`,title:`Special Hooks`,pages:[{id:`special-hooks-animation`,title:`Animation hooks`,path:`/special-hooks/animation`,keywords:[`hooks`,`animation`,`UseAnimate`,`UseTweenFloat`],searchContent:`animation hooks useanimate usetweenfloat animate tween float interpolation easing duration delay loop repeat playback`,element:()=>(0,L.jsx)(lx,{})},{id:`special-hooks-router`,title:`Router hooks`,path:`/special-hooks/router`,keywords:[`hooks`,`router`,`RouterHooks`],searchContent:`router hooks routerhooks usenavigate useparams usequery uselocationinfo usenavigationstate useblocker usego usepath imperative navigation`,element:()=>(0,L.jsx)(fx,{})},{id:`special-hooks-signals`,title:`Signal hooks`,path:`/special-hooks/signals`,keywords:[`hooks`,`signals`,`UseSignal`],searchContent:`signal hooks usesignal dispatch signalfactory signalfactory.get signals.get shared-state reactive`,element:()=>(0,L.jsx)(hx,{})},{id:`special-hooks-safe-area`,title:`Safe area hooks`,path:`/special-hooks/safe-area`,keywords:[`hooks`,`safe area`,`UseSafeArea`,`VisualElementSafe`],searchContent:`safe area hooks usesafearea visualelementsafe notch insets screen safe zone padding margins mobile`,element:()=>(0,L.jsx)(vx,{})}]},{id:`api`,title:`API`,pages:[{id:`api-reference`,title:`API Reference`,path:`/api`,keywords:[`api`,`namespace`,`props`,`hooks`,`router`,`signals`],searchContent:`api reference namespace props hooks usestate useeffect usememo useref usecallback usecontext router signals runtime types v vnode virtualnode rootrenderer editorrootrendererutility`,element:()=>(0,L.jsx)(Yv,{})}]},{id:`known-issues`,title:`Known Issues`,pages:[{id:`known-issues-page`,title:`Known Issues`,path:`/known-issues`,keywords:[`issues`,`limitations`,`known issues`],element:()=>(0,L.jsx)(rx,{})}]},{id:`roadmap`,title:`Roadmap`,pages:[{id:`roadmap-page`,title:`Roadmap`,path:`/roadmap`,keywords:[`roadmap`,`future`,`plans`],element:()=>(0,L.jsx)(ax,{})}]}];yx.flatMap(e=>{if(e.id===`components`){let t=e.pages.filter(e=>e.group===`basic`),n=e.pages.filter(e=>e.group===`advanced`||!e.group);return[...t,...n]}return e.pages});const bx=()=>(0,L.jsxs)(W,{sx:Jv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UITKX API Map`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`For UITKX users, the important API split is: author in markup, use hooks in setup code, and understand the runtime types only when you need to mount, integrate, or debug.`}),(0,L.jsxs)(W,{sx:Jv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Authoring surface`}),(0,L.jsxs)(G,{sx:Jv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`.uitkx`}),` function-style components are the primary source format.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`useState`}),`, `,(0,L.jsx)(`code`,{children:`useEffect`}),`, `,(0,L.jsx)(`code`,{children:`useMemo`}),`, and `,(0,L.jsx)(`code`,{children:`useSignal`}),` are the normal UITKX setup-code hooks, alongside router/context helpers.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Intrinsic tags map onto built-in ReactiveUITK/UI Toolkit elements.`})})]})]}),(0,L.jsxs)(W,{sx:Jv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Runtime layer underneath`}),(0,L.jsxs)(G,{sx:Jv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`V`}),` and `,(0,L.jsx)(`code`,{children:`VirtualNode`}),` still exist as the underlying runtime representation.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RootRenderer`}),` and `,(0,L.jsx)(`code`,{children:`EditorRootRendererUtility`}),` are still how UITKX output is mounted.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Props classes and typed styles still matter for low-level integration, custom emit targets, and debugging generated output.`})})]})]})]});var xx=e=>e===`VisualElementSafe`?``:`${e}Props`,Sx=e=>{if(e.endsWith(`Field`))return`Use <${e}> in UITKX for a controlled UI Toolkit ${e} input. Keep the current value in useState(...) or a signal and feed user edits back through onChange.`;switch(e){case`Button`:case`RepeatButton`:return`Use <${e}> in UITKX for clickable actions. Pass text, event handlers, and styling directly on the tag.`;case`Toggle`:case`RadioButton`:return`Use <${e}> in UITKX for boolean-style selection controls. The usual pattern is value + onChange, backed by local state or a signal.`;case`DropdownField`:case`EnumField`:case`EnumFlagsField`:case`RadioButtonGroup`:case`ToggleButtonGroup`:return`Use <${e}> in UITKX when the user is choosing from a predefined set of options.`;case`Slider`:case`SliderInt`:case`Scroller`:case`MinMaxSlider`:return`Use <${e}> in UITKX for range-style numeric input.`;case`Label`:case`TextElement`:return`Use <${e}> in UITKX to render text content directly in markup.`;case`VisualElement`:case`VisualElementSafe`:case`Box`:case`GroupBox`:case`ScrollView`:case`TemplateContainer`:case`TwoPaneSplitView`:return`Use <${e}> in UITKX as a structural layout primitive. It composes naturally with child tags and style props.`;case`HelpBox`:case`Image`:case`ProgressBar`:return`Use <${e}> in UITKX as a presentational control inside your returned markup tree.`;case`ListView`:case`TreeView`:case`MultiColumnListView`:case`MultiColumnTreeView`:return`Use <${e}> in UITKX for data-driven collection UIs. These components usually combine declarative markup with row, cell, or binding delegates configured through props.`;case`Tab`:case`TabView`:case`Toolbar`:return`Use <${e}> in UITKX for higher-level navigation and editor-style composition.`;case`Animate`:case`ErrorBoundary`:case`IMGUIContainer`:case`PropertyInspector`:return`Use <${e}> in UITKX when you need this higher-level ReactiveUITK runtime feature directly in markup.`;default:return`Use <${e}> directly in UITKX markup. The runtime still exposes the underlying props type, but the normal authoring surface is the tag itself.`}},Cx=e=>{switch(e){case`VisualElementSafe`:return[`VisualElementSafe is the safe-area-aware variant of VisualElement.`,`Use it when your layout should automatically respect device insets.`];case`IMGUIContainer`:return[`IMGUIContainer remains callback-driven even in UITKX.`,`It is mainly useful for editor tooling or legacy IMGUI interop.`];case`PropertyInspector`:return[`PropertyInspector is especially useful in editor tooling and inspector-like UIs.`,`It is still backed by runtime props underneath, even when authored in UITKX.`];case`ListView`:case`TreeView`:case`MultiColumnListView`:case`MultiColumnTreeView`:return[`Collection components often rely on renderer delegates in addition to plain tag props.`,`The props section below matters more than usual for these components.`];default:return[`The props section below shows the underlying runtime API that UITKX lowers into.`]}},wx=e=>{switch(e){case`Button`:return`component ButtonExample {
  var (count, setCount) = useState(0);

  return (
    <Button
      text={$"Click me ({count})"}
      onClick={_ => setCount(previous => previous + 1)}
    />
  );
}`;case`RepeatButton`:return`component RepeatButtonExample {
  var (count, setCount) = useState(0);

  return (
    <RepeatButton
      text={$"Hold to increment ({count})"}
      onClick={_ => setCount(previous => previous + 1)}
    />
  );
}`;case`Toggle`:return`component ToggleExample {
  var (value, setValue) = useState(true);

  return (
    <Toggle
      text="Enabled"
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`RadioButton`:return`component RadioButtonExample {
  var (value, setValue) = useState(false);

  return (
    <RadioButton
      text="Option"
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`DropdownField`:return`component DropdownFieldExample {
  var choices = new[] { "Red", "Green", "Blue" };
  var (selectedIndex, setSelectedIndex) = useState(0);

  return (
    <DropdownField
      choices={choices}
      selectedIndex={selectedIndex}
      onChange={evt => setSelectedIndex(Array.IndexOf(choices, evt.newValue))}
    />
  );
}`;case`TextField`:return`component TextFieldExample {
  var (value, setValue) = useState("Hello");

  return (
    <TextField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`IntegerField`:return`component IntegerFieldExample {
  var (value, setValue) = useState(42);

  return (
    <IntegerField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`FloatField`:return`component FloatFieldExample {
  var (value, setValue) = useState(1.23f);

  return (
    <FloatField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`DoubleField`:return`component DoubleFieldExample {
  var (value, setValue) = useState(3.14159);

  return (
    <DoubleField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`LongField`:return`component LongFieldExample {
  var (value, setValue) = useState(123456789L);

  return (
    <LongField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`UnsignedIntegerField`:return`component UnsignedIntegerFieldExample {
  var (value, setValue) = useState<uint>(0u);

  return (
    <UnsignedIntegerField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`UnsignedLongField`:return`component UnsignedLongFieldExample {
  var (value, setValue) = useState<ulong>(0ul);

  return (
    <UnsignedLongField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Slider`:return`component SliderExample {
  var (value, setValue) = useState(0.5f);

  return (
    <Slider
      lowValue={0f}
      highValue={1f}
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`SliderInt`:return`component SliderIntExample {
  var (value, setValue) = useState(5);

  return (
    <SliderInt
      lowValue={0}
      highValue={10}
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Scroller`:return`component ScrollerExample {
  var (value, setValue) = useState(0f);

  return (
    <Scroller
      lowValue={0f}
      highValue={100f}
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`MinMaxSlider`:return`component MinMaxSliderExample {
  var (range, setRange) = useState((min: 20f, max: 80f));

  return (
    <MinMaxSlider
      minValue={0f}
      maxValue={100f}
      value={range}
      onChange={evt => setRange(evt.newValue)}
    />
  );
}`;case`BoundsField`:return`component BoundsFieldExample {
  var (value, setValue) = useState(new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f)));

  return (
    <BoundsField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`BoundsIntField`:return`component BoundsIntFieldExample {
  var (value, setValue) = useState(new BoundsInt(1, 2, 3, 4, 5, 6));

  return (
    <BoundsIntField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`ColorField`:return`component ColorFieldExample {
  var (value, setValue) = useState(new Color(0.2f, 0.6f, 0.9f, 1f));

  return (
    <ColorField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`EnumField`:return`component EnumFieldExample {
  var (value, setValue) = useState(ExampleEnum.B);

  return (
    <EnumField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`EnumFlagsField`:return`component EnumFlagsFieldExample {
  var (value, setValue) = useState(ExampleFlags.A | ExampleFlags.C);

  return (
    <EnumFlagsField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Hash128Field`:return`component Hash128FieldExample {
  var (value, setValue) = useState(new Hash128(1, 2, 3, 4));

  return (
    <Hash128Field
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`ObjectField`:return`component ObjectFieldExample {
  var (value, setValue) = useState<Object>(null);

  return (
    <ObjectField
      objectType={typeof(Texture2D)}
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`RectField`:return`component RectFieldExample {
  var (value, setValue) = useState(new Rect(0f, 0f, 128f, 64f));

  return (
    <RectField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`RectIntField`:return`component RectIntFieldExample {
  var (value, setValue) = useState(new RectInt(0, 0, 16, 16));

  return (
    <RectIntField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Vector2Field`:return`component Vector2FieldExample {
  var (value, setValue) = useState(new Vector2(1f, 2f));

  return (
    <Vector2Field
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Vector2IntField`:return`component Vector2IntFieldExample {
  var (value, setValue) = useState(new Vector2Int(1, 2));

  return (
    <Vector2IntField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Vector3Field`:return`component Vector3FieldExample {
  var (value, setValue) = useState(new Vector3(1f, 2f, 3f));

  return (
    <Vector3Field
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Vector3IntField`:return`component Vector3IntFieldExample {
  var (value, setValue) = useState(new Vector3Int(1, 2, 3));

  return (
    <Vector3IntField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Vector4Field`:return`component Vector4FieldExample {
  var (value, setValue) = useState(new Vector4(1f, 2f, 3f, 4f));

  return (
    <Vector4Field
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`;case`Label`:return`component LabelExample {
  return (
    <Label text="Hello from UITKX" />
  );
}`;case`TextElement`:return`component TextElementExample {
  return (
    <TextElement text="TextElement authored directly in UITKX" />
  );
}`;case`HelpBox`:return`component HelpBoxExample {
  return (
    <HelpBox
      text="Remember to save before entering play mode."
      helpBoxMessageType="warning"
    />
  );
}`;case`ProgressBar`:return`component ProgressBarExample {
  return (
    <ProgressBar
      value={65f}
      title="Build progress"
    />
  );
}`;case`Image`:return`component ImageExample(Texture2D texture) {
  return (
    <Image image={texture} />
  );
}`;case`VisualElement`:return`component VisualElementExample {
  return (
    <VisualElement>
      <Text text="Child content inside a VisualElement" />
    </VisualElement>
  );
}`;case`VisualElementSafe`:return`component VisualElementSafeExample {
  return (
    <VisualElementSafe>
      <Text text="Safe-area aware content" />
    </VisualElementSafe>
  );
}`;case`Box`:return`component BoxExample {
  return (
    <Box>
      <Text text="Inside Box" />
    </Box>
  );
}`;case`GroupBox`:return`component GroupBoxExample {
  return (
    <GroupBox text="Example group">
      <Text text="Content item 1" />
      <Text text="Content item 2" />
    </GroupBox>
  );
}`;case`ScrollView`:return`component ScrollViewExample {
  return (
    <ScrollView mode="vertical">
      <Text text="Row 1" />
      <Text text="Row 2" />
      <Text text="Row 3" />
    </ScrollView>
  );
}`;case`TwoPaneSplitView`:return`component TwoPaneSplitViewExample {
  return (
    <TwoPaneSplitView
      orientation="horizontal"
      fixedPaneIndex={0}
      fixedPaneInitialDimension={220f}
    >
      <VisualElement>
        <Text text="Pane 1" />
      </VisualElement>
      <VisualElement>
        <Text text="Pane 2" />
      </VisualElement>
    </TwoPaneSplitView>
  );
}`;case`Toolbar`:return`component ToolbarExample {
  return (
    <Toolbar>
      <ToolbarButton text="Ping" onClick={_ => UnityEngine.Debug.Log("Ping")} />
      <ToolbarToggle text="Toggle" value={false} />
      <ToolbarSearchField />
    </Toolbar>
  );
}`;case`Foldout`:return`component FoldoutExample {
  var (open, setOpen) = useState(true);

  return (
    <Foldout
      text="Advanced options"
      value={open}
      onChange={evt => setOpen(evt.newValue)}
    >
      <Text text="Nested content" />
    </Foldout>
  );
}`;case`Tab`:return`component TabExample {
  return (
    <Tab text="General" />
  );
}`;case`TabView`:return`component TabViewExample {
  return (
    <TabView>
      <Tab text="General">
        <Text text="General settings" />
      </Tab>
      <Tab text="Audio">
        <Text text="Audio settings" />
      </Tab>
    </TabView>
  );
}`;case`ListView`:return`component ListViewExample {
  var items = new[] { "One", "Two", "Three" };

  return (
    <ListView
      items={items}
      fixedItemHeight={20f}
      row={(index, item) => <Label text={$"{index}: {item}"} />}
    />
  );
}`;case`TreeView`:return`component TreeViewExample {
  return (
    <TreeView
      items={treeItems}
      row={(index, item) => <Label text={item.ToString()} />}
    />
  );
}`;case`MultiColumnListView`:return`component MultiColumnListViewExample {
  return (
    <MultiColumnListView
      items={rows}
      columns={columns}
    />
  );
}`;case`MultiColumnTreeView`:return`component MultiColumnTreeViewExample {
  return (
    <MultiColumnTreeView
      items={treeItems}
      columns={columns}
    />
  );
}`;case`Animate`:return`component AnimateExample {
  return (
    <Animate>
      <Box>
        <Text text="Animated box" />
      </Box>
    </Animate>
  );
}`;case`ErrorBoundary`:return`component ErrorBoundaryExample {
  return (
    <ErrorBoundary fallback={<Text text="Something went wrong." />}>
      <UnstableChild />
    </ErrorBoundary>
  );
}`;case`IMGUIContainer`:return`component IMGUIContainerExample {
  void DrawGui()
  {
    GUILayout.Label("Hello from IMGUI");
  }

  return (
    <IMGUIContainer onGUI={DrawGui} />
  );
}`;case`PropertyInspector`:return`component PropertyInspectorExample {
  var (target, setTarget) = useState<Object>(null);

  return (
    <PropertyInspector target={target} />
  );
}`;default:return`component ${e}Example {
  return (
    <${e} />
  );
}`}};const Tx=({title:e})=>{let t=Q(xx(e)),n=Cx(e);return(0,L.jsxs)(W,{sx:iy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:e}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:Sx(e)}),(0,L.jsxs)(W,{sx:iy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`UITKX Example`}),(0,L.jsx)(Z,{language:`tsx`,code:wx(e)})]}),(0,L.jsxs)(W,{sx:iy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Notes`}),(0,L.jsx)(G,{children:n.map(e=>(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:e})},e))})]}),t?(0,L.jsxs)(W,{sx:iy.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Underlying Props`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`UITKX authors normally use the tag directly, but the runtime still exposes the underlying props contract shown below.`}),(0,L.jsx)(Z,{language:`tsx`,code:t})]}):null]})};var Ex=`component ButtonShowcase {
  var (enabled, setEnabled) = useState(true);

  return (
    <VisualElement>
      <Text text={$"Enabled: {enabled}"} />
      <Button
        text={enabled ? "Disable" : "Enable"}
        enabled={true}
        onClick={_ => setEnabled(previous => !previous)}
      />
      <Button
        text="Secondary action"
        enabled={enabled}
        onClick={_ => UnityEngine.Debug.Log("Clicked")}
      />
    </VisualElement>
  );
}`;const Dx=()=>(0,L.jsxs)(W,{sx:iy.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Components in UITKX`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`In the UITKX track, components are authored as markup using intrinsic tags like`,` `,(0,L.jsx)(`code`,{children:`<VisualElement>`}),`, `,(0,L.jsx)(`code`,{children:`<Button>`}),`, `,(0,L.jsx)(`code`,{children:`<Text>`}),`, router tags, and your own custom components.`]}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`The practical rule is simple: use intrinsic tags for built-in elements, and use PascalCase names for your own components. If you wrap a native element, consumers should use your custom component name, not the native one.`}),(0,L.jsx)(Z,{language:`tsx`,code:Ex}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Authoring guidelines`}),(0,L.jsxs)(G,{children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Prefer direct tag props over hand-building props objects when authoring UITKX.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Keep setup code small and close to the returned markup tree.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Use custom component names whenever a native tag name would collide.`})})]})]}),Ox=()=>(0,L.jsxs)(W,{sx:Rv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Companion Files`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The source generator produces a `,(0,L.jsx)(`strong`,{children:`complete C# class`}),` from every`,` `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file — namespace, partial class, `,(0,L.jsx)(`code`,{children:`Render()`}),` method, and everything else. You do `,(0,L.jsx)(`strong`,{children:`not`}),` need to create any `,(0,L.jsx)(`code`,{children:`.cs`}),` file for a component to work.`]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Companion files are `,(0,L.jsx)(`strong`,{children:`optional`}),` `,(0,L.jsx)(`code`,{children:`.cs`}),` files that live next to a`,` `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file. Use them when you want to share styles, type definitions, or utility functions with your component.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`The UITKX component`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`Here is a component that uses styles, types, and utility functions defined in companion files:`}),(0,L.jsx)(Z,{language:`tsx`,code:`@namespace MyGame.UI
@using UnityEngine.UIElements

component PlayerCard {
  @props { PlayerInfo player }

  var healthColor = player.Health > player.MaxHealth / 2
    ? PlayerCardStyles.HealthGreen
    : PlayerCardStyles.DamageRed;

  return (
    <VisualElement>
      <Label text={player.Name} />
      <Label text={PlayerCardUtils.FormatHealth(player.Health, player.MaxHealth)}
             style:color={healthColor} />
      <Label text={PlayerCardUtils.RankLabel(player.Rank)} />
    </VisualElement>
  );
}`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Generated namespace & class name`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The source generator creates a C# class from the `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file. Two things determine its identity:`]}),(0,L.jsxs)(G,{children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`strong`,{children:`Namespace`}),` — comes from the `,(0,L.jsx)(`code`,{children:`@namespace`}),` directive at the top of the `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`strong`,{children:`Class name`}),` — comes from the `,(0,L.jsx)(`code`,{children:`component`}),` name (the identifier after the `,(0,L.jsx)(`code`,{children:`component`}),` keyword).`]})})})]}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`For the example above, the generator produces:`}),(0,L.jsx)(Z,{language:`tsx`,code:`// Auto-generated by the source generator (simplified):
namespace MyGame.UI                    // ← from @namespace
{
    public partial class PlayerCard    // ← from component name
    {
        public static VisualElement Render(PlayerInfo player) { ... }
    }
}`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Companion `,(0,L.jsx)(`code`,{children:`.cs`}),` files that need to reference or extend the generated class must use the `,(0,L.jsx)(`strong`,{children:`same namespace`}),` and `,(0,L.jsx)(`strong`,{children:`same class name`}),`.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Directory layout`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Place companion files in the `,(0,L.jsx)(`strong`,{children:`same directory`}),` as the `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file:`]}),(0,L.jsx)(Z,{language:`text`,code:`Assets/
  UI/
    PlayerCard/
      PlayerCard.uitkx          ← component template
      PlayerCard.styles.cs      ← optional: style constants & helpers
      PlayerCard.types.cs       ← optional: enums, structs, DTOs
      PlayerCard.utils.cs       ← optional: pure helper functions`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`These names are conventions, not enforced rules. Any `,(0,L.jsx)(`code`,{children:`.cs`}),` file (except`,` `,(0,L.jsx)(`code`,{children:`.g.cs`}),`) in the same directory is automatically picked up during compilation.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Naming conventions`}),(0,L.jsx)(C_,{component:td,variant:`outlined`,sx:{mb:2},children:(0,L.jsxs)(c_,{size:`small`,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`File`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`Purpose`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`Required?`})})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`MyComponent.styles.cs`})}),(0,L.jsx)(J,{children:`Style constants, helper methods, colours, sizes`}),(0,L.jsx)(J,{children:`No`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`MyComponent.types.cs`})}),(0,L.jsx)(J,{children:`Enums, structs, DTOs used by the component`}),(0,L.jsx)(J,{children:`No`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`MyComponent.utils.cs`})}),(0,L.jsx)(J,{children:`Pure helper / formatting functions`}),(0,L.jsx)(J,{children:`No`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`MyComponent.extra.cs`})}),(0,L.jsx)(J,{children:`Partial class extension (same namespace + class name)`}),(0,L.jsx)(J,{children:`No`})]})]})]})}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Example: style helpers`}),(0,L.jsx)(Z,{language:`tsx`,code:`// PlayerCard.styles.cs
using UnityEngine.UIElements;

namespace MyGame.UI
{
    public static class PlayerCardStyles
    {
        public static readonly StyleColor HealthGreen = new(new Color(0.2f, 0.8f, 0.3f));
        public static readonly StyleColor DamageRed   = new(new Color(0.9f, 0.2f, 0.2f));
        public static readonly StyleLength AvatarSize  = new(64);
    }
}`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Example: type definitions`}),(0,L.jsx)(Z,{language:`tsx`,code:`// PlayerCard.types.cs
namespace MyGame.UI
{
    public enum PlayerRank { Bronze, Silver, Gold, Diamond }

    public readonly struct PlayerInfo
    {
        public string Name { get; init; }
        public int Health { get; init; }
        public int MaxHealth { get; init; }
        public PlayerRank Rank { get; init; }
    }
}`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Example: utility functions`}),(0,L.jsx)(Z,{language:`tsx`,code:`// PlayerCard.utils.cs
namespace MyGame.UI
{
    public static class PlayerCardUtils
    {
        public static string FormatHealth(int current, int max)
            => $"{current} / {max} HP";

        public static string RankLabel(PlayerRank rank) => rank switch
        {
            PlayerRank.Diamond => "★ Diamond",
            PlayerRank.Gold    => "● Gold",
            PlayerRank.Silver  => "○ Silver",
            _                  => "· Bronze",
        };
    }
}`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Extending the generated partial class`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Because the generated class is `,(0,L.jsx)(`code`,{children:`partial`}),`, you can extend it with additional fields or methods. The namespace and class name `,(0,L.jsx)(`strong`,{children:`must match`}),` the generated ones:`]}),(0,L.jsx)(Z,{language:`tsx`,code:`// PlayerCard.extra.cs — extending the generated partial class
namespace MyGame.UI                    // ← must match @namespace
{
    public partial class PlayerCard    // ← must match component name
    {
        // Add fields, methods, or interfaces to the generated class
        private static readonly Color GoldColor = new(1f, 0.84f, 0f);
    }
}`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`HMR support`}),(0,L.jsxs)(G,{children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Editing a companion .cs file automatically triggers HMR for the associated .uitkx.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Creating a new companion file is detected instantly — the file watcher picks up new files.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`All .cs files in the directory (except .g.cs) are included in compilation.`})})]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`When not to use companion files`}),(0,L.jsxs)(G,{children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Simple components — if a component has no shared styles or types, it doesn't need any companion files.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Small helpers — for code that only the component uses, prefer`,` `,(0,L.jsx)(`code`,{children:`@code`}),` blocks inside the `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file itself.`]})})})]})]}),kx=()=>(0,L.jsxs)(W,{sx:Wv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Concepts & Environment`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`UITKX is the authoring layer. ReactiveUITK is the runtime layer underneath it. In practice, that means you think in terms of components, intrinsic tags, hooks, and markup structure, while the runtime handles reconciliation, scheduling, and adapter application.`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`The key mental model is: write UI as UITKX, keep your setup code local to the component, and let the generator and runtime bridge that into Unity UI Toolkit.`}),(0,L.jsxs)(W,{sx:Wv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Core authoring rules`}),(0,L.jsxs)(G,{sx:Wv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Intrinsic UITKX/native tag names are reserved; custom components should use distinct names.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Function-style components are the default form: setup code first, then a single returned markup tree.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`State setters are called directly like functions, for example setCount(count + 1).`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Companion .cs files are optional — use them to share styles, types, or utilities. The source generator produces the full class from the .uitkx file alone.`})})]})]}),(0,L.jsxs)(W,{sx:Wv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Environment defines`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`Compile-time environment and tracing symbols still work the same way in UITKX projects, because the generated output runs on the same ReactiveUITK runtime.`}),(0,L.jsxs)(G,{sx:Wv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`ENV_DEV`}),`, `,(0,L.jsx)(`code`,{children:`ENV_STAGING`}),`, `,(0,L.jsx)(`code`,{children:`ENV_PROD`}),` control environment labeling.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RUITK_TRACE_VERBOSE`}),` and `,(0,L.jsx)(`code`,{children:`RUITK_TRACE_BASIC`}),` control runtime diagnostics.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Editor-only diagnostic helpers still compile behind the same development symbols.`})})]})]})]});var Ax={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},table:{"& th":{fontWeight:600},"& td, & th":{px:1.5,py:.75,fontSize:`0.875rem`},"& code":{fontSize:`0.8125rem`,backgroundColor:`rgba(255,255,255,0.06)`,px:.5,borderRadius:.5}}},jx=`{
  // Path to a custom UitkxLanguageServer.dll (leave empty for bundled server)
  "uitkx.server.path": "",

  // Path to the dotnet executable used to run the LSP server
  "uitkx.server.dotnetPath": "dotnet",

  // Trace LSP communication (off | messages | verbose)
  "uitkx.trace.server": "off"
}`;const Mx=()=>(0,L.jsxs)(W,{sx:Ax.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Configuration Reference`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`All configuration options for the UITKX editor extensions and formatter.`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`VS Code Extension Settings`}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Setting`}),(0,L.jsx)(J,{children:`Type`}),(0,L.jsx)(J,{children:`Default`}),(0,L.jsx)(J,{children:`Description`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`uitkx.server.path`})}),(0,L.jsx)(J,{children:`string`}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`""`})}),(0,L.jsxs)(J,{children:[`Absolute path to a custom `,(0,L.jsx)(`code`,{children:`UitkxLanguageServer.dll`}),`. Leave empty to use the server bundled with the extension.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`uitkx.server.dotnetPath`})}),(0,L.jsx)(J,{children:`string`}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`"dotnet"`})}),(0,L.jsxs)(J,{children:[`Path to the `,(0,L.jsx)(`code`,{children:`dotnet`}),` executable. Override this if your .NET 8+ SDK is installed in a non-standard location.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`uitkx.trace.server`})}),(0,L.jsx)(J,{children:`enum`}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`"off"`})}),(0,L.jsxs)(J,{children:[`Controls LSP trace output. Set to `,(0,L.jsx)(`code`,{children:`"messages"`}),` or`,` `,(0,L.jsx)(`code`,{children:`"verbose"`}),` to see JSON-RPC traffic in the Output panel (select "UITKX Language Server" channel).`]})]})]})]})}),(0,L.jsx)(Z,{language:`json`,code:jx}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Editor Defaults`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`The extension automatically configures these editor settings for`,` `,(0,L.jsx)(`code`,{children:`.uitkx`}),` files:`]}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Setting`}),(0,L.jsx)(J,{children:`Value`}),(0,L.jsx)(J,{children:`Reason`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`editor.defaultFormatter`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`ReactiveUITK.uitkx`})}),(0,L.jsxs)(J,{children:[`Uses the UITKX formatter for `,(0,L.jsx)(`code`,{children:`.uitkx`}),` files`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`editor.formatOnSave`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`true`})}),(0,L.jsx)(J,{children:`Auto-format on save (recommended)`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`editor.tabSize`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`2`})}),(0,L.jsx)(J,{children:`UITKX uses 2-space indentation`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`editor.insertSpaces`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`true`})}),(0,L.jsx)(J,{children:`Spaces, not tabs`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`editor.bracketPairColorization`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`false`})}),(0,L.jsx)(J,{children:`Disabled — conflicting colors with UITKX semantic tokens`})]})]})]})})]});var Nx=`// Generated files are at:
// Library/PackageCache/com.reactiveuitk/Analyzers~
//   or under your project's SourceGenerator~ output folder.
// Look for files ending in .uitkx.g.cs`,Px=`{
  "uitkx.trace.server": "verbose"
}`;const Fx=()=>(0,L.jsxs)(W,{sx:Ax.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Debugging Guide`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`How to diagnose and fix common issues when working with UITKX.`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Inspecting Generated Code`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Every `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file produces a corresponding `,(0,L.jsx)(`code`,{children:`.uitkx.g.cs`}),` file via the Roslyn source generator. To inspect it:`]}),(0,L.jsxs)(U,{component:`ol`,variant:`body2`,children:[(0,L.jsxs)(`li`,{children:[`In VS Code, go to `,(0,L.jsx)(`strong`,{children:`Definition`}),` (F12) on any generated symbol.`]}),(0,L.jsxs)(`li`,{children:[`Or navigate to the `,(0,L.jsx)(`code`,{children:`GeneratedFiles`}),` folder under your project's Analyzers output directory.`]}),(0,L.jsxs)(`li`,{children:[`The generated file contains `,(0,L.jsx)(`code`,{children:`#line`}),` directives that map errors back to the original `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file and line number.`]})]}),(0,L.jsx)(Z,{language:`csharp`,code:Nx}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Understanding #line Directives`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`When the C# compiler reports an error in generated code, the`,` `,(0,L.jsx)(`code`,{children:`#line`}),` directive maps it back to your `,(0,L.jsx)(`code`,{children:`.uitkx`}),` `,`source. For example:`]}),(0,L.jsxs)(U,{variant:`body2`,component:`ul`,children:[(0,L.jsxs)(`li`,{children:[(0,L.jsx)(`code`,{children:`#line 42 "MyComponent.uitkx"`}),` means the C# code that follows was generated from line 42 of your UITKX file.`]}),(0,L.jsx)(`li`,{children:`Clicking on the error in VS Code or Visual Studio will jump directly to the UITKX source line.`})]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`LSP Server Logs`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`To see detailed LSP communication, set the trace level in your VS Code settings:`}),(0,L.jsx)(Z,{language:`json`,code:Px}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Then open the `,(0,L.jsx)(`strong`,{children:`Output`}),` panel (Ctrl+Shift+U) and select the`,` `,(0,L.jsx)(`strong`,{children:`UITKX Language Server`}),` channel. This shows all JSON-RPC requests and responses, which is useful for diagnosing:`]}),(0,L.jsxs)(U,{component:`ul`,variant:`body2`,children:[(0,L.jsx)(`li`,{children:`Missing completions — check if the completion request/response is present`}),(0,L.jsxs)(`li`,{children:[`Stale diagnostics — look for `,(0,L.jsx)(`code`,{children:`textDocument/publishDiagnostics`}),` messages`]}),(0,L.jsx)(`li`,{children:`Server crashes — look for error messages in the trace output`})]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Formatter Issues`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`If formatting produces unexpected results:`}),(0,L.jsxs)(U,{component:`ol`,variant:`body2`,children:[(0,L.jsxs)(`li`,{children:[(0,L.jsx)(`strong`,{children:`Check for syntax errors first`}),` — the formatter requires valid UITKX syntax. Fix any red squiggles before formatting.`]}),(0,L.jsxs)(`li`,{children:[(0,L.jsx)(`strong`,{children:`Ensure format-on-save is using the UITKX formatter`}),` — check that `,(0,L.jsx)(`code`,{children:`editor.defaultFormatter`}),` is set to`,` `,(0,L.jsx)(`code`,{children:`"ReactiveUITK.uitkx"`}),` for `,(0,L.jsx)(`code`,{children:`[uitkx]`}),` files.`]}),(0,L.jsxs)(`li`,{children:[(0,L.jsx)(`strong`,{children:`Try formatting manually`}),` — press Shift+Alt+F to rule out format-on-save timing issues.`]})]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Reporting Bugs`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`When reporting an issue, include:`}),(0,L.jsxs)(U,{component:`ol`,variant:`body2`,children:[(0,L.jsxs)(`li`,{children:[`The minimal `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file that reproduces the problem.`]}),(0,L.jsx)(`li`,{children:`The exact error message or diagnostic code (if any).`}),(0,L.jsx)(`li`,{children:`Your editor (VS Code / Visual Studio / Rider) and extension version.`}),(0,L.jsx)(`li`,{children:`LSP trace output if relevant (see above).`})]})]}),Ix=()=>(0,L.jsxs)(W,{sx:Ax.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Diagnostics Reference`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`Every diagnostic code emitted by the UITKX source generator and language server, with severity, meaning, and how to fix it.`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Source Generator Diagnostics`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Emitted at compile time by the Roslyn source generator when processing`,(0,L.jsx)(`code`,{children:`.uitkx`}),` files.`]}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Code`}),(0,L.jsx)(J,{children:`Severity`}),(0,L.jsx)(J,{children:`Title`}),(0,L.jsx)(J,{children:`How to fix`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0001`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`Unknown built-in element`}),(0,L.jsxs)(J,{children:[`Check the tag name — built-in elements use PascalCase (e.g. `,(0,L.jsx)(`code`,{children:`<Button>`}),`, `,(0,L.jsx)(`code`,{children:`<Label>`}),`).`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0002`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`Unknown attribute on element`}),(0,L.jsx)(J,{children:`Verify the attribute name matches a property on the element's props type.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0005`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Missing required directive`}),(0,L.jsxs)(J,{children:[`Add the missing `,(0,L.jsx)(`code`,{children:`@namespace`}),` or `,(0,L.jsx)(`code`,{children:`@component`}),` directive.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0006`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`@component name mismatch`}),(0,L.jsxs)(J,{children:[`Rename `,(0,L.jsx)(`code`,{children:`@component`}),` to match the file name, or rename the file.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0008`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`Unknown function component`}),(0,L.jsxs)(J,{children:[`Ensure the component type exists and has a public static `,(0,L.jsx)(`code`,{children:`Render`}),` method.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0009`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`@foreach child missing key`}),(0,L.jsxs)(J,{children:[`Add a `,(0,L.jsx)(`code`,{children:`key`}),` attribute with a stable unique identifier from the item.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0010`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`Duplicate sibling key`}),(0,L.jsxs)(J,{children:[`Ensure each sibling element has a unique `,(0,L.jsx)(`code`,{children:`key`}),` value.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0012`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Directive order error`}),(0,L.jsxs)(J,{children:[`Move `,(0,L.jsx)(`code`,{children:`@namespace`}),` above `,(0,L.jsx)(`code`,{children:`@component`}),`.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0013`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Hook in conditional`}),(0,L.jsxs)(J,{children:[`Move the hook call to the component top level, outside any `,(0,L.jsx)(`code`,{children:`@if`}),` branch.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0014`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Hook in loop`}),(0,L.jsxs)(J,{children:[`Move the hook call to the component top level, outside any `,(0,L.jsx)(`code`,{children:`@foreach`}),` loop.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0015`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Hook in switch case`}),(0,L.jsxs)(J,{children:[`Move the hook call to the component top level, outside the `,(0,L.jsx)(`code`,{children:`@switch`}),`.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0016`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Hook in event handler`}),(0,L.jsx)(J,{children:`Move the hook call to the component top level — hooks cannot be called inside attribute expressions.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0017`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Multiple root elements`}),(0,L.jsxs)(J,{children:[`Wrap all root elements in a single container element (e.g. `,(0,L.jsx)(`code`,{children:`<VisualElement>`}),`).`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0018`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`UseEffect missing dependency array`}),(0,L.jsxs)(J,{children:[`Pass an explicit dependency array as the second argument, or `,(0,L.jsx)(`code`,{children:`Array.Empty<object>()`}),` for run-once.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0019`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`Loop variable used as key`}),(0,L.jsx)(J,{children:`Use a stable unique identifier from the item instead of the loop index.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0020`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`ref on component without Ref<T> param`}),(0,L.jsxs)(J,{children:[`Add a `,(0,L.jsx)(`code`,{children:`Ref<T>?`}),` parameter to the component, or remove the `,(0,L.jsx)(`code`,{children:`ref`}),` attribute.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0021`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`ref ambiguous — multiple Ref<T> params`}),(0,L.jsxs)(J,{children:[`Use an explicit prop name (e.g. `,(0,L.jsxs)(`code`,{children:[`inputRef=`,`{x}`]}),`) instead of `,(0,L.jsx)(`code`,{children:`ref`}),`.`]})]})]})]})}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Structural Diagnostics (Language Server)`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`Emitted in real time by the language server as you type. These appear as squiggly underlines in your editor.`}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Code`}),(0,L.jsx)(J,{children:`Severity`}),(0,L.jsx)(J,{children:`Message`}),(0,L.jsx)(J,{children:`How to fix`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0101`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsxs)(J,{children:[`Missing required `,(0,L.jsx)(`code`,{children:`@namespace`}),` directive`]}),(0,L.jsxs)(J,{children:[`Add `,(0,L.jsx)(`code`,{children:`@namespace Your.Namespace`}),` at the top of the file.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0102`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsxs)(J,{children:[`Missing required `,(0,L.jsx)(`code`,{children:`@component`}),` directive`]}),(0,L.jsxs)(J,{children:[`Add `,(0,L.jsx)(`code`,{children:`@component YourComponentName`}),` or use function-style syntax.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0103`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`@component name does not match filename`}),(0,L.jsxs)(J,{children:[`Rename `,(0,L.jsx)(`code`,{children:`@component`}),` to match the file name.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0104`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Duplicate sibling key`}),(0,L.jsxs)(J,{children:[`Ensure each sibling has a unique `,(0,L.jsx)(`code`,{children:`key`}),`.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0105`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Unknown element — no component found`}),(0,L.jsx)(J,{children:`Check the tag name or add the missing component to your project.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0106`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Element inside @foreach missing key`}),(0,L.jsxs)(J,{children:[`Add a `,(0,L.jsx)(`code`,{children:`key`}),` attribute for reconciliation.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0107`})}),(0,L.jsx)(J,{children:`Hint`}),(0,L.jsx)(J,{children:`Unreachable code after return / @break / @continue`}),(0,L.jsx)(J,{children:`Remove the unreachable code, or restructure control flow.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0108`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Multiple root elements`}),(0,L.jsx)(J,{children:`Wrap all root elements in a single container.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0109`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Unknown attribute on element`}),(0,L.jsx)(J,{children:`Check the attribute name against the element's props type.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0111`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Unused component parameter`}),(0,L.jsx)(J,{children:`Remove the unused parameter or use it in the component body.`})]})]})]})}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Parser Diagnostics`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`Emitted when the parser encounters malformed syntax.`}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Code`}),(0,L.jsx)(J,{children:`Severity`}),(0,L.jsx)(J,{children:`Title`}),(0,L.jsx)(J,{children:`How to fix`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0300`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Unexpected token`}),(0,L.jsx)(J,{children:`Check for typos or misplaced syntax near the reported line.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0301`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Unclosed tag`}),(0,L.jsxs)(J,{children:[`Add a matching closing tag or use self-closing syntax (`,(0,L.jsx)(`code`,{children:`/>`}),`).`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0302`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Mismatched closing tag`}),(0,L.jsx)(J,{children:`Ensure the closing tag matches the opening tag name.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0303`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Unexpected end of file`}),(0,L.jsx)(J,{children:`Close any open tags, braces, or expressions.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0304`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`Unclosed expression or block`}),(0,L.jsx)(J,{children:`Close the unclosed brace or parenthesis.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0305`})}),(0,L.jsx)(J,{children:`Warning`}),(0,L.jsx)(J,{children:`Unknown markup directive`}),(0,L.jsxs)(J,{children:[`Valid directives: `,(0,L.jsx)(`code`,{children:`@if`}),`, `,(0,L.jsx)(`code`,{children:`@else`}),`, `,(0,L.jsx)(`code`,{children:`@for`}),`, `,(0,L.jsx)(`code`,{children:`@foreach`}),`, `,(0,L.jsx)(`code`,{children:`@while`}),`, `,(0,L.jsx)(`code`,{children:`@switch`}),`, `,(0,L.jsx)(`code`,{children:`@case`}),`, `,(0,L.jsx)(`code`,{children:`@default`}),`, `,(0,L.jsx)(`code`,{children:`@break`}),`, `,(0,L.jsx)(`code`,{children:`@continue`}),`, `,(0,L.jsx)(`code`,{children:`@code`}),`.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`UITKX0306`})}),(0,L.jsx)(J,{children:`Error`}),(0,L.jsx)(J,{children:`@(expr) in setup code`}),(0,L.jsxs)(J,{children:[`Inline expressions `,(0,L.jsx)(`code`,{children:`@(...)`}),` are only valid inside markup, not in `,(0,L.jsx)(`code`,{children:`@code`}),` blocks.`]})]})]})]})})]}),Lx=()=>(0,L.jsxs)(W,{sx:Kv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Different from React`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`UITKX borrows React’s component-and-hooks mental model, but it runs on Unity UI Toolkit and a C# runtime. The biggest difference is that your authored code is markup-first, while the underlying runtime is still constrained by Unity’s VisualElement system, scheduling model, and C# semantics.`}),(0,L.jsxs)(W,{sx:Kv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`State updates`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`code`,{children:`useState`}),` behaves like React’s `,(0,L.jsx)(`code`,{children:`useState`}),`. You call the setter directly with either a value or an updater function, and UITKX lowers that into the runtime hook implementation for you.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`component StateCounterExample {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text={$"Count: {count}"} />
      <Button text="Increment" onClick={_ => setCount(previous => previous + 1)} />
      <Button text="Reset" onClick={_ => setCount(0)} />
    </VisualElement>
  );
}`})]}),(0,L.jsxs)(W,{sx:Kv.section,children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Rendering model`}),(0,L.jsxs)(G,{sx:Kv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`ReactiveUITK’s fiber can schedule work asynchronously when a scheduler is present, including sliced render work and deferred passive effects.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`That does not mean UITKX promises a one-to-one clone of React’s full concurrent feature surface; the scheduler still operates inside Unity’s runtime constraints.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`The authored syntax is JSX-like, but it lowers into ReactiveUITK’s own runtime representation instead of a browser DOM model.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Interop with Unity controls, styles, and events is a first-class constraint, so some APIs deliberately differ from browser React conventions.`})})]})]})]}),Rx=()=>(0,L.jsxs)(W,{sx:Rv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UITKX Getting Started`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`UITKX is the primary authoring model for ReactiveUIToolKit. You write function-style`,` `,(0,L.jsx)(`code`,{children:`.uitkx`}),` components and the source generator produces a complete C# class automatically — no boilerplate needed.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Install via Unity Package Manager`}),(0,L.jsxs)(G,{children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Open Package Manager in Unity.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Add package from Git URL:`})})]}),(0,L.jsx)(Z,{language:`tsx`,code:`https://github.com/yanivkalfa/ReactiveUIToolKit.git#dist`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`1. Create a UITKX component`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`A function-style UITKX component contains setup code at the top and returns markup. This is the default shape new users should learn.`}),(0,L.jsx)(Z,{language:`tsx`,code:`@namespace MyGame.UI

component HelloWorld {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text="Hello ReactiveUITK" />
      <Text text={$"Count: {count}"} />
      <Button text="Increment" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`On the next Unity compile the source generator emits a complete C# class (`,(0,L.jsx)(`code`,{children:`HelloWorld.uitkx.g.cs`}),`) with `,(0,L.jsx)(`code`,{children:`namespace`}),`,`,` `,(0,L.jsx)(`code`,{children:`public partial class`}),`, and a full `,(0,L.jsx)(`code`,{children:`Render()`}),` method. You don't need to create any companion file for this to work.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`2. Mount it`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Runtime mounting uses `,(0,L.jsx)(`code`,{children:`RootRenderer`}),` and `,(0,L.jsx)(`code`,{children:`V.Func(...)`}),`, but the authored UI stays in UITKX.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK;
using ReactiveUITK.Core;

public sealed class HelloRuntime : MonoBehaviour
{
  [SerializeField] private UIDocument uiDocument;

  private RootRenderer rootRenderer;

  private void Awake()
  {
    rootRenderer = FindObjectOfType<RootRenderer>();
    if (rootRenderer == null)
    {
      rootRenderer = new GameObject("ReactiveUIRoot").AddComponent<RootRenderer>();
    }

    rootRenderer.Initialize(uiDocument.rootVisualElement);
    rootRenderer.Render(V.Func(HelloWorld.Render));
  }
}`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Companion files (optional)`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The generator produces everything needed, but you can optionally add `,(0,L.jsx)(`code`,{children:`.cs`}),` files next to your `,(0,L.jsx)(`code`,{children:`.uitkx`}),` to share styles, types, or utilities across components. See the `,(0,L.jsx)(`strong`,{children:`Companion Files`}),` page for naming conventions and examples.`]})]});var zx=`component CounterCard {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text={$"Count: {count}"} />
      <Button text="+" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`;const Bx=()=>(0,L.jsxs)(W,{sx:z_.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ReactiveUIToolKit`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit is a React-like UI Toolkit runtime for Unity, and UITKX is its primary authoring language. You write function-style components in `,(0,L.jsx)(`code`,{children:`.uitkx`}),`, use hooks for state and effects, and let the toolkit reconcile the resulting tree onto Unity`,(0,L.jsx)(`code`,{children:`VisualElement`}),`s.`]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The authored experience should feel markup-first. The underlying runtime is still C#, but the normal way to build UI is UITKX, not hand-built `,(0,L.jsx)(`code`,{children:`V.*`}),` trees.`]}),(0,L.jsx)(Z,{language:`tsx`,code:zx}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Highlights`}),(0,L.jsxs)(G,{children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Function-style UITKX components with hooks and typed props`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Reactive diffing and batched updates on top of UI Toolkit`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Router and Signals utilities that work naturally inside UITKX`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Generated C# output for production builds with no runtime codegen`})})]})]});var Vx=`@namespace My.Game.UI
@using System.Collections.Generic
@component MyButton
@props MyButtonProps
@key "root-key"
@inject ILogger logger`,Hx=`@using UnityEngine

component Counter(string label = "Count") {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Label text={$"{label}: {count}"} />
      <Button text="+" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`,Ux=`<VisualElement>
  @if (isLoggedIn) {
    <Label text="Welcome back!" />
  } @else {
    <Button text="Log in" onClick={_ => login()} />
  }

  @foreach (var item in items) {
    <Label key={item.Id} text={item.Name} />
  }

  @switch (mode) {
    @case "dark":
      <Label text="Dark mode" />
    @default:
      <Label text="Light mode" />
  }
</VisualElement>`,Wx=`<Label text={$"Count: {count}"} />
<Button onClick={_ => setCount(count + 1)} />
<VisualElement>
  @(MyCustomComponent)
  {/* This is a JSX comment */}
</VisualElement>`;const Gx=()=>(0,L.jsxs)(W,{sx:Ax.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UITKX Language Reference`}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`Complete reference for the UITKX markup language — directives, syntax, control flow, and expressions.`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Header Directives`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Header directives appear at the top of a `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file, before any markup. They configure the generated C# class.`]}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Directive`}),(0,L.jsx)(J,{children:`Syntax`}),(0,L.jsx)(J,{children:`Description`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@namespace`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@namespace My.Game.UI`})}),(0,L.jsx)(J,{children:`C# namespace for the generated class`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@component`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@component MyButton`})}),(0,L.jsx)(J,{children:`Component class name (must match filename)`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@using`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@using System.Collections.Generic`})}),(0,L.jsx)(J,{children:`Adds a using directive to the generated file`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@props`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@props MyButtonProps`})}),(0,L.jsx)(J,{children:`Props type consumed by the component`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@key`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@key "root-key"`})}),(0,L.jsx)(J,{children:`Static key on the root element`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@inject`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@inject ILogger logger`})}),(0,L.jsx)(J,{children:`Dependency-injected field`})]})]})]})}),(0,L.jsx)(Z,{language:`tsx`,code:Vx}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Function-Style Components`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Function-style components use a `,(0,L.jsxs)(`code`,{children:[`component Name `,`{ ... }`]}),` `,`syntax with optional typed parameters. They replace the directive-header form for most use cases.`]}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Feature`}),(0,L.jsx)(J,{children:`Syntax`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Declaration`}),(0,L.jsx)(J,{children:(0,L.jsxs)(`code`,{children:[`component Name `,`{ ... }`]})})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`With parameters`}),(0,L.jsx)(J,{children:(0,L.jsxs)(`code`,{children:[`component Name(string text = "default") `,`{ ... }`]})})]}),(0,L.jsxs)(Y,{children:[(0,L.jsxs)(J,{children:[`Preamble `,(0,L.jsx)(`code`,{children:`@using`})]}),(0,L.jsxs)(J,{children:[`Before the `,(0,L.jsx)(`code`,{children:`component`}),` keyword`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsxs)(J,{children:[`Preamble `,(0,L.jsx)(`code`,{children:`@namespace`})]}),(0,L.jsx)(J,{children:`Optional explicit namespace override`})]})]})]})}),(0,L.jsx)(Z,{language:`tsx`,code:Hx}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Markup Control Flow`}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Directive`}),(0,L.jsx)(J,{children:`Syntax`}),(0,L.jsx)(J,{children:`Notes`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@if / @else if / @else`})}),(0,L.jsx)(J,{children:(0,L.jsxs)(`code`,{children:[`@if (cond) `,`{ ... }`,` @else `,`{ ... }`]})}),(0,L.jsx)(J,{children:`Conditional rendering`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@foreach`})}),(0,L.jsx)(J,{children:(0,L.jsxs)(`code`,{children:[`@foreach (var item in list) `,`{ ... }`]})}),(0,L.jsxs)(J,{children:[`Loop — direct children must have `,(0,L.jsx)(`code`,{children:`key`})]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@for`})}),(0,L.jsx)(J,{children:(0,L.jsxs)(`code`,{children:[`@for (int i = 0; i < n; i++) `,`{ ... }`]})}),(0,L.jsx)(J,{children:`C-style for loop`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@while`})}),(0,L.jsx)(J,{children:(0,L.jsxs)(`code`,{children:[`@while (cond) `,`{ ... }`]})}),(0,L.jsx)(J,{children:`While loop`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@switch / @case / @default`})}),(0,L.jsx)(J,{children:(0,L.jsxs)(`code`,{children:[`@switch (val) `,`{ @case "a": ... @default: ... }`]})}),(0,L.jsx)(J,{children:`Switch expression`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@break`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@break;`})}),(0,L.jsxs)(J,{children:[`Exit a `,(0,L.jsx)(`code`,{children:`@for`}),` or `,(0,L.jsx)(`code`,{children:`@while`}),` loop`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@continue`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@continue;`})}),(0,L.jsx)(J,{children:`Skip to the next iteration`})]})]})]})}),(0,L.jsx)(Z,{language:`tsx`,code:Ux}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Expressions & Values`}),(0,L.jsx)(C_,{children:(0,L.jsxs)(c_,{size:`small`,sx:Ax.table,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Syntax`}),(0,L.jsx)(J,{children:`Example`}),(0,L.jsx)(J,{children:`Description`})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@(expr)`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`@(MyCustomComponent)`})}),(0,L.jsx)(J,{children:`Render a component or expression inline in markup children`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`{expr}`})}),(0,L.jsx)(J,{children:(0,L.jsxs)(`code`,{children:[`text=`,`{$"Count: {count}"}`]})}),(0,L.jsx)(J,{children:`C# expression as attribute value`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`"literal"`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`text="hello"`})}),(0,L.jsx)(J,{children:`Plain string attribute`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`{/* comment */}`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`code`,{children:`{/* TODO */}`})}),(0,L.jsx)(J,{children:`JSX-style block comment`})]})]})]})}),(0,L.jsx)(Z,{language:`tsx`,code:Wx}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Ax.section,children:`Rules & Gotchas`}),(0,L.jsxs)(U,{component:`ul`,variant:`body2`,children:[(0,L.jsxs)(`li`,{children:[(0,L.jsx)(`code`,{children:`@namespace`}),` must appear before `,(0,L.jsx)(`code`,{children:`@component`}),` in directive-header form.`]}),(0,L.jsxs)(`li`,{children:[`Hook calls must be unconditional at component top level — not inside `,(0,L.jsx)(`code`,{children:`@if`}),`, `,(0,L.jsx)(`code`,{children:`@foreach`}),`, etc.`]}),(0,L.jsxs)(`li`,{children:[(0,L.jsx)(`code`,{children:`@break`}),` / `,(0,L.jsx)(`code`,{children:`@continue`}),` are only valid inside `,(0,L.jsx)(`code`,{children:`@for`}),` and `,(0,L.jsx)(`code`,{children:`@while`}),`.`]}),(0,L.jsxs)(`li`,{children:[`Direct children of `,(0,L.jsx)(`code`,{children:`@foreach`}),` need a `,(0,L.jsx)(`code`,{children:`key`}),` attribute for stable reconciliation.`]}),(0,L.jsx)(`li`,{children:`Components must have a single root element.`}),(0,L.jsxs)(`li`,{children:[`Component names must match the filename (e.g. `,(0,L.jsx)(`code`,{children:`MyButton.uitkx`}),` defines `,(0,L.jsx)(`code`,{children:`component MyButton`}),`).`]})]})]}),Kx=()=>(0,L.jsxs)(W,{sx:Bv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Router`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`In UITKX, routing is authored directly in markup. You compose `,(0,L.jsx)(`code`,{children:`<Router>`}),`,`,(0,L.jsx)(`code`,{children:`<Route>`}),`, links, and routed child components as part of the same returned UI tree.`]}),(0,L.jsxs)(G,{sx:Bv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`<Router>`}),` establishes routing context for the subtree.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`<Route>`}),` matches paths and can render elements or child component trees.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks`}),` stay in setup code for imperative navigation, history control, params, query values, and navigation state.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),` pushes or replaces locations, while `,(0,L.jsx)(`code`,{children:`UseGo()`}),` and `,(0,L.jsx)(`code`,{children:`UseCanGo()`}),` drive back/forward UI.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseLocationInfo()`}),`, `,(0,L.jsx)(`code`,{children:`UseParams()`}),`, `,(0,L.jsx)(`code`,{children:`UseQuery()`}),`, and `,(0,L.jsx)(`code`,{children:`UseNavigationState()`}),` expose the active routed data.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`RouterHooks.UseBlocker()`}),` lets you intercept transitions when a screen has unsaved or guarded state.`]})})})]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The example below shows both styles together: declarative route composition in markup, and imperative setup-code helpers through `,(0,L.jsx)(`code`,{children:`RouterHooks`}),` for navigation and route data.`]}),(0,L.jsx)(Z,{language:`tsx`,code:`@using ReactiveUITK.Router

component RouterDemo {
  var navigate = RouterHooks.UseNavigate();
  var parameters = RouterHooks.UseParams();
  var query = RouterHooks.UseQuery();

  return (
    <Router>
      <VisualElement>
        <RouterNavLink path="/" label="Home" exact={true} />
        <RouterNavLink path="/about" label="About" />
        <RouterNavLink path="/users" label="Users" />
        <Button
          text="Open profile 42"
          onClick={_ => navigate("/users/42?tab=profile")}
        />

        <Route
          path="/"
          exact={true}
          element={<Text text="Landing route" />}
        />
        <Route
          path="/about"
          element={<Text text="About route" />}
        />
        <Route path="/users/:id">
          <VisualElement>
            <Text text={$"User id: {parameters["id"]}"} />
            <Text text={$"Tab: {query["tab"] ?? "summary"}"} />
            <RouterUserDetails />
          </VisualElement>
        </Route>
        <Route
          path="*"
          element={<Text text="Not found" />}
        />
      </VisualElement>
    </Router>
  );
}`})]}),qx=()=>(0,L.jsxs)(W,{sx:Hv.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Signals`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Signals remain the shared-state primitive underneath the UITKX authoring model. Outside components you work with the signal registry directly; inside UITKX components you typically read them with `,(0,L.jsx)(`code`,{children:`useSignal(...)`}),` and dispatch updates from event handlers.`]}),(0,L.jsxs)(W,{children:[(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Runtime access`}),(0,L.jsx)(Z,{language:`tsx`,code:`using ReactiveUITK.Signals;

SignalsRuntime.EnsureInitialized();
var counter = Signals.Get<int>("demo.counter", 0);
counter.Dispatch(previous => previous + 1);`})]}),(0,L.jsxs)(W,{children:[(0,L.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Using signals inside UITKX`}),(0,L.jsxs)(G,{sx:Hv.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Use a signal factory or registry lookup in setup code.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Read the current value with useSignal(...).`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Dispatch updates directly from UITKX event handlers.`})})]}),(0,L.jsx)(Z,{language:`tsx`,code:`@using ReactiveUITK.Signals
@using System

component SignalCounterDemo {
  var counterSignal = useMemo(() => SignalFactory.Get<int>("demo.counter", 0), Array.Empty<object>());
  var count = useSignal(counterSignal);

  return (
    <VisualElement>
      <Text text="Signal Counter" />
      <Text text={$"Count: {count}"} />
      <VisualElement style={new Style { (StyleKeys.FlexDirection, "row") }}>
        <Button text="Increment" onClick={_ => counterSignal.Dispatch(v => v + 1)} />
        <Button text="Reset" onClick={_ => counterSignal.Dispatch(0)} />
      </VisualElement>
    </VisualElement>
  );
}`})]})]});var Jx={root:{display:`flex`,flexDirection:`column`,gap:2},list:{pl:2},table:{my:1}},Yx=({title:e,children:t})=>(0,L.jsxs)(W,{children:[(0,L.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:e}),t]});const Xx=()=>(0,L.jsxs)(W,{sx:Jx.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Hot Module Replacement`}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Hot Module Replacement lets you edit `,(0,L.jsx)(`code`,{children:`.uitkx`}),` files and see changes instantly in the Unity Editor — without domain reload, without losing component state.`]}),(0,L.jsx)(Yx,{title:`Quick Start`,children:(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Open `,(0,L.jsx)(`strong`,{children:`ReactiveUITK → HMR Mode`}),` from the Unity menu bar.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Click `,(0,L.jsx)(`strong`,{children:`Start HMR`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Edit and save any `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`The component updates in-place — hook state (counters, refs, effects) is preserved.`})})]})}),(0,L.jsxs)(Yx,{title:`How It Works`,children:[(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`When HMR is active:`}),(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Assembly reloads are locked — no domain reload occurs on file saves.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`A `,(0,L.jsx)(`code`,{children:`FileSystemWatcher`}),` detects `,(0,L.jsx)(`code`,{children:`.uitkx`}),` changes under `,(0,L.jsx)(`code`,{children:`Assets/`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`The file is parsed and emitted to C# using `,(0,L.jsx)(`code`,{children:`ReactiveUITK.Language.dll`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`C# is compiled in-process via Roslyn (`,(0,L.jsx)(`code`,{children:`Microsoft.CodeAnalysis.CSharp`}),` 4.3.1), with automatic fallback to external `,(0,L.jsx)(`code`,{children:`csc.dll`}),` if Roslyn DLLs aren't available.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`The compiled assembly is loaded via `,(0,L.jsx)(`code`,{children:`Assembly.Load(byte[])`}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`The new `,(0,L.jsx)(`code`,{children:`Render`}),` delegate is swapped into all active `,(0,L.jsx)(`code`,{children:`RootRenderer`}),` instances.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`A re-render is triggered — hooks run against preserved state.`})})]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Total time: typically `,(0,L.jsx)(`strong`,{children:`25–100 ms`}),` compile + emit from save to visual update (first compile per session is ~1–1.5s due to Roslyn JIT warmup).`]})]}),(0,L.jsxs)(Yx,{title:`State Preservation`,children:[(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`HMR preserves all hook state across swaps:`}),(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`useState`}),` — current values retained.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`useRef`}),` — ref objects preserved.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`useEffect`}),` — cleanup runs, effect re-runs with new closure.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`useMemo`}),` / `,(0,L.jsx)(`code`,{children:`useCallback`}),` — recomputed with new function body.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(`code`,{children:`useContext`}),` — context values preserved.`]})})})]}),(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`If the number or order of hooks changes between edits, HMR detects the mismatch, resets state for that component, and logs a warning.`})]}),(0,L.jsxs)(Yx,{title:`Companion Files`,children:[(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Companion `,(0,L.jsx)(`code`,{children:`.cs`}),` files are `,(0,L.jsx)(`strong`,{children:`optional`}),`. The source generator produces a complete class from the `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file alone. However, you can add`,(0,L.jsx)(`code`,{children:`.cs`}),` files in the same directory to share styles, types, or utilities. When a`,` `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file changes, HMR automatically includes all `,(0,L.jsx)(`code`,{children:`.cs`}),` files in the same directory (excluding `,(0,L.jsx)(`code`,{children:`.g.cs`}),`) in the compilation:`]}),(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Style helpers (e.g. `,(0,L.jsx)(`code`,{children:`MyComponent.styles.cs`}),`)`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Type / prop definitions (e.g. `,(0,L.jsx)(`code`,{children:`MyComponent.types.cs`}),`)`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Shared utilities (e.g. `,(0,L.jsx)(`code`,{children:`MyComponent.utils.cs`}),`)`]})})})]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Companion `,(0,L.jsx)(`code`,{children:`.cs`}),` file changes also trigger HMR — saving a`,` `,(0,L.jsx)(`code`,{children:`.styles.cs`}),` or `,(0,L.jsx)(`code`,{children:`.utils.cs`}),` file automatically detects the associated `,(0,L.jsx)(`code`,{children:`.uitkx`}),` in the same directory, recompiles everything, and swaps the result in-place.`]}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,L.jsx)(`strong`,{children:`Creating new companion files`}),` works too — simply create a `,(0,L.jsx)(`code`,{children:`.cs`}),` `,`file in the same directory as your `,(0,L.jsx)(`code`,{children:`.uitkx`}),`. The file watcher detects new files and includes them in the next compilation.`]})]}),(0,L.jsxs)(Yx,{title:`New Component Support`,children:[(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`HMR can compile and load `,(0,L.jsx)(`strong`,{children:`new`}),` `,(0,L.jsx)(`code`,{children:`.uitkx`}),` files that don't exist in any pre-compiled assembly:`]}),(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`When a parent component references an unknown child, CS0103 errors are caught.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`HMR scans the project for matching `,(0,L.jsx)(`code`,{children:`.uitkx`}),` files and compiles them first.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`The parent is automatically retried after the dependency resolves.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Cross-component references are managed via an assembly registry.`})})]})]}),(0,L.jsxs)(Yx,{title:`HMR Window`,children:[(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The HMR window (`,(0,L.jsx)(`strong`,{children:`ReactiveUITK → HMR Mode`}),`) shows:`]}),(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Start / Stop button with status indicator.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Stats: swap count, error count, last component name and timing.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Timing breakdown: Parse, Emit, Compile, and Swap durations per step.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Settings: auto-stop on play mode, swap notifications.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Keyboard Shortcuts: configurable bindings (see below).`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Recent Errors: last 10 compilation errors (scrollable, copyable).`})})]})]}),(0,L.jsxs)(Yx,{title:`Keyboard Shortcuts`,children:[(0,L.jsx)(U,{variant:`body1`,paragraph:!0,children:`Shortcuts are not bound by default — configure them in the HMR window to avoid conflicting with your existing keybindings.`}),(0,L.jsx)(C_,{component:td,variant:`outlined`,sx:Jx.table,children:(0,L.jsxs)(c_,{size:`small`,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`Action`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`Description`})})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Toggle HMR`}),(0,L.jsx)(J,{children:`Start or stop the HMR session`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Open / Close Window`}),(0,L.jsx)(J,{children:`Show or hide the HMR window`})]})]})]})}),(0,L.jsx)(U,{variant:`body2`,sx:{mt:1},children:`Requirements: at least one modifier key (Ctrl, Alt, or Shift) plus one regular key.`})]}),(0,L.jsxs)(Yx,{title:`Lifecycle`,children:[(0,L.jsx)(C_,{component:td,variant:`outlined`,sx:Jx.table,children:(0,L.jsxs)(c_,{size:`small`,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`Event`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`Behavior`})})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Start HMR`}),(0,L.jsx)(J,{children:`Assembly reload locked, file watcher started`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Stop HMR`}),(0,L.jsx)(J,{children:`Assembly reload unlocked, pending changes compile normally`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Enter / Exit Play Mode`}),(0,L.jsx)(J,{children:`Auto-stops HMR (configurable)`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Build (Player)`}),(0,L.jsx)(J,{children:`Auto-stops HMR`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Editor quit`}),(0,L.jsx)(J,{children:`Auto-stops HMR`})]})]})]})}),(0,L.jsxs)(U,{variant:`body1`,paragraph:!0,sx:{mt:1},children:[`While HMR is active, `,(0,L.jsx)(`strong`,{children:`all compilation is deferred`}),` — not just`,` `,(0,L.jsx)(`code`,{children:`.uitkx`}),` changes. Any `,(0,L.jsx)(`code`,{children:`.cs`}),` edits accumulate and compile in one batch when HMR is stopped.`]})]}),(0,L.jsx)(Yx,{title:`Limitations`,children:(0,L.jsx)(C_,{component:td,variant:`outlined`,sx:Jx.table,children:(0,L.jsxs)(c_,{size:`small`,children:[(0,L.jsx)(k_,{children:(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`Limitation`})}),(0,L.jsx)(J,{children:(0,L.jsx)(`strong`,{children:`Details`})})]})}),(0,L.jsxs)(h_,{children:[(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Old assemblies stay in memory`}),(0,L.jsx)(J,{children:`Mono cannot unload assemblies. ~10–30 KB per swap, cleared on domain reload.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`All compilation deferred`}),(0,L.jsxs)(J,{children:[`Non-UITKX `,(0,L.jsx)(`code`,{children:`.cs`}),` changes don't take effect until HMR stops. UX warning shown.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`First compile is slow`}),(0,L.jsx)(J,{children:`~1–1.5s on first HMR compile per session (Roslyn JIT warmup). Subsequent compiles are 25–100ms.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Requires NuGet cache`}),(0,L.jsxs)(J,{children:[`In-process Roslyn loads DLLs from `,(0,L.jsx)(`code`,{children:`~/.nuget/packages/`}),`. Falls back to external `,(0,L.jsx)(`code`,{children:`csc.dll`}),` if unavailable.`]})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Static field changes ignored`}),(0,L.jsx)(J,{children:`Statics live on the old assembly's type.`})]}),(0,L.jsxs)(Y,{children:[(0,L.jsx)(J,{children:`Cross-assembly props`}),(0,L.jsx)(J,{children:`Props are read via reflection to handle type mismatches across assemblies.`})]})]})]})})}),(0,L.jsxs)(Yx,{title:`Troubleshooting`,children:[(0,L.jsx)(U,{variant:`h6`,component:`h3`,gutterBottom:!0,children:`HMR doesn't start`}),(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Check the Console for initialization errors.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Ensure `,(0,L.jsx)(`code`,{children:`ReactiveUITK.Language.dll`}),` exists in the `,(0,L.jsx)(`code`,{children:`Analyzers/`}),` folder.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Verify Unity's Roslyn compiler is present at `,(0,L.jsxs)(`code`,{children:["${EditorPath}",`/Data/DotNetSdkRoslyn/csc.dll`]}),`.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Check for `,(0,L.jsx)(`code`,{children:`[HMR] In-process Roslyn compiler loaded successfully`}),` in Console.`]})})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`If Roslyn fails to load, verify that `,(0,L.jsx)(`code`,{children:`~/.nuget/packages/microsoft.codeanalysis.csharp/4.3.1/`}),` exists.`]})})})]}),(0,L.jsx)(U,{variant:`h6`,component:`h3`,gutterBottom:!0,sx:{mt:2},children:`Changes don't appear`}),(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Confirm the file is saved (not just modified).`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Check the HMR window for compilation errors.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Verify the file is under `,(0,L.jsx)(`code`,{children:`Assets/`}),` (the watched directory).`]})})})]}),(0,L.jsx)(U,{variant:`h6`,component:`h3`,gutterBottom:!0,sx:{mt:2},children:`State is lost after edit`}),(0,L.jsxs)(G,{sx:Jx.list,children:[(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:`Hook order or count may have changed — this triggers automatic state reset.`})}),(0,L.jsx)(K,{disablePadding:!0,children:(0,L.jsx)(q,{primary:(0,L.jsxs)(L.Fragment,{children:[`Check Console for "`,(0,L.jsx)(`code`,{children:`[HMR] Hook mismatch`}),`" messages.`]})})})]})]})]});var Zx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:3},question:{mt:2,fontWeight:600}};const Qx=()=>(0,L.jsxs)(W,{sx:Zx.root,children:[(0,L.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Frequently Asked Questions`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Zx.section,children:`General`}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`What is UITKX?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`UITKX is a markup language for authoring Unity UI Toolkit components using a React-like model. You write `,(0,L.jsx)(`code`,{children:`.uitkx`}),` files with JSX-style markup, hooks, and control flow. A Roslyn source generator compiles them into standard C# that runs on the ReactiveUIToolKit runtime.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Which Unity versions are supported?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Unity `,(0,L.jsx)(`strong`,{children:`6.2`}),` and above. The framework relies on UI Toolkit APIs available from Unity 6.2 onward.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Does UITKX work with existing UI Toolkit code?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Yes. UITKX components render into the same `,(0,L.jsx)(`code`,{children:`VisualElement`}),` tree as hand-written UI Toolkit code. You can mount UITKX components alongside existing UI Toolkit panels, mix UITKX components with native elements, and interop through standard `,(0,L.jsx)(`code`,{children:`VisualElement`}),` references.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Does UITKX add runtime overhead?`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`The reconciliation scheduler adds a small per-frame cost similar to other retained-mode UI frameworks. In practice, the overhead is negligible for typical UI workloads. All generated code is standard C# — there is no runtime code generation or reflection.`}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Can I use UITKX in production builds?`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`Yes. The source generator produces plain C# at compile time. The generated output is included in your build like any other script. There is no interpreter or runtime codegen — UITKX is fully AOT-compatible.`}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Zx.section,children:`IDE & Editor Extensions`}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Which editors are supported?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[(0,L.jsx)(`strong`,{children:`VS Code`}),` and `,(0,L.jsx)(`strong`,{children:`Visual Studio 2022`}),` have full, officially supported extensions with syntax highlighting, completions, hover documentation, diagnostics, and formatting. A`,` `,(0,L.jsx)(`strong`,{children:`JetBrains Rider`}),` plugin exists as a stub — source generation and `,(0,L.jsx)(`code`,{children:`#line`}),` mapping work via standard Roslyn support, but the full editing experience has not been fully verified. Rider is not officially supported in V1.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Do I need the VS Code extension to use UITKX?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`No. The source generator runs inside Unity regardless of your editor. The extension provides the editing experience — syntax highlighting, completions, error squiggles, and formatting. Without it you can still write `,(0,L.jsx)(`code`,{children:`.uitkx`}),` files, but you won't have language support.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`VS Code shows wrong colours briefly when I open a file — is that a bug?`}),(0,L.jsx)(U,{variant:`body2`,paragraph:!0,children:`This is expected. VS Code layers TextMate grammar colours first, then overrides them with LSP semantic tokens after ~200 ms. The brief flash (e.g. PascalCase names appearing green) is inherent to how VS Code works and resolves automatically.`}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`What .NET version does the language server need?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`The LSP server requires `,(0,L.jsx)(`strong`,{children:`.NET 8`}),` or later. Run`,` `,(0,L.jsx)(`code`,{children:`dotnet --version`}),` to verify. If you have a non-standard install location, set `,(0,L.jsx)(`code`,{children:`uitkx.server.dotnetPath`}),` in VS Code settings.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Zx.section,children:`Authoring`}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Can I use standard C# inside UITKX files?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Yes. The setup code section (before the `,(0,L.jsx)(`code`,{children:`return`}),`) is standard C#. You can declare variables, call methods, use LINQ, and access any type available via `,(0,L.jsx)(`code`,{children:`@using`}),` directives. Attribute values inside markup also accept C# expressions via the `,(0,L.jsx)(`code`,{children:`{expr}`}),` syntax.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Does HMR (Hot Module Replacement) affect build times?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`No. HMR bypasses Unity's normal compilation pipeline — it compiles only the changed `,(0,L.jsx)(`code`,{children:`.uitkx`}),` file using Roslyn directly, loads the result via `,(0,L.jsx)(`code`,{children:`Assembly.Load`}),`, and swaps the render delegate. Typical save-to-visual-update time is 50–200 ms. When HMR is stopped, Unity compiles normally.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Why do hooks need to be at the top level?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`UITKX follows the same rules as React hooks — they must be called unconditionally at the component top level, never inside`,` `,(0,L.jsx)(`code`,{children:`@if`}),`, `,(0,L.jsx)(`code`,{children:`@foreach`}),`, or event handlers. This ensures hooks are called in the same order on every render, which is required for the reconciler to track state correctly.`]}),(0,L.jsx)(U,{variant:`h5`,component:`h2`,sx:Zx.section,children:`Troubleshooting`}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`I see "Failed to resolve assembly: Assembly-CSharp-Editor" from Burst — what do I do?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Go to `,(0,L.jsx)(`strong`,{children:`Edit → Project Settings → Burst AOT Settings`}),` and add `,(0,L.jsx)(`code`,{children:`Assembly-CSharp-Editor`}),` to the exclusion list. This prevents Burst from trying to AOT-compile editor-only assemblies.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`Completions or hover stopped working — how do I debug?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Set `,(0,L.jsx)(`code`,{children:`uitkx.trace.server`}),` to `,(0,L.jsx)(`code`,{children:`"verbose"`}),` in VS Code settings, then open the Output panel (Ctrl+Shift+U) and select the "UITKX Language Server" channel. Check for error messages or missing responses. See the `,(0,L.jsx)(`em`,{children:`Debugging Guide`}),` page for more details.`]}),(0,L.jsx)(U,{variant:`body1`,sx:Zx.question,children:`My component has red squiggles but the code looks correct — what's wrong?`}),(0,L.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Make sure the file is saved — the language server works on the last saved content. Also check that the `,(0,L.jsx)(`code`,{children:`component`}),` keyword is present and that any `,(0,L.jsx)(`code`,{children:`@using`}),` directives are correct.`]})]});var $x=yx.find(e=>e.id===`components`)?.pages??[],eS=(e,t)=>t===`/`?e:`${e}${t}`,tS=(e,t,n)=>t.map(t=>({id:`${e}-${t.id}`,title:t.title,track:e,pages:t.pages.map(t=>({id:`${e}-${t.id}`,canonicalId:t.id,title:t.title,path:eS(n,t.path),keywords:t.keywords,searchContent:t.searchContent,group:t.group,track:e,element:t.element}))}));const nS=[{id:`uitkx-intro`,title:`Introduction`,track:`uitkx`,pages:[{id:`uitkx-introduction`,canonicalId:`introduction`,title:`Introduction`,path:`/`,keywords:[`uitkx`,`introduction`,`markup`,`unity ui toolkit`],searchContent:`reactiveuitoolkit react-like ui toolkit runtime unity uitkx primary authoring language function-style components .uitkx hooks state effects reconcile visualelement markup-first c# v.* trees component countercard usestate return text button onclick highlights reactive diffing batched updates router signals utilities generated c# output production builds no runtime codegen var count setcount`,track:`uitkx`,element:()=>(0,L.jsx)(Bx,{})}]},{id:`uitkx-getting-started`,title:`Getting Started`,track:`uitkx`,pages:[{id:`uitkx-getting-started-page`,canonicalId:`install`,title:`Install & Setup`,path:`/getting-started`,keywords:[`uitkx`,`install`,`setup`,`component`,`partial`],searchContent:`uitkx getting started primary authoring model reactiveuitoolkit function-style .uitkx components source generator produces complete class no boilerplate install via unity package manager open package manager add package from git url create a uitkx component setup code returned markup generator emits render mount rootrenderer v.func @namespace MyGame.UI component HelloWorld var count setCount useState return VisualElement Text Hello ReactiveUITK Button Increment onClick setCount count + 1 companion files optional styles types utils`,track:`uitkx`,element:()=>(0,L.jsx)(Rx,{})}]},{id:`uitkx-companion-files`,title:`Companion Files`,track:`uitkx`,pages:[{id:`uitkx-companion-files-page`,canonicalId:`companion-files`,title:`Companion Files`,path:`/companion-files`,keywords:[`uitkx`,`companion`,`styles`,`types`,`utils`,`partial class`],searchContent:`companion files optional .cs file styles types utils naming conventions directory layout source generator produces complete class no boilerplate needed MyComponent.styles.cs style constants helpers colours sizes MyComponent.types.cs enums structs DTOs MyComponent.utils.cs pure helper formatting functions hmr support editing companion triggers hmr creating new file detected instantly @code blocks when not to use simple components small helpers`,track:`uitkx`,element:()=>(0,L.jsx)(Ox,{})}]},{id:`uitkx-components`,title:`Components Overview`,track:`uitkx`,pages:[{id:`uitkx-components-overview`,canonicalId:`uitkx-components-overview`,title:`Components Overview`,path:`/components`,keywords:[`uitkx`,`components`,`intrinsic tags`,`custom components`],searchContent:`components in uitkx intrinsic tags visualelement button text router tags custom components pascalcase names native element consumers custom component name authoring guidelines prefer direct tag props hand-building props objects keep setup code small close to returned markup tree custom component names must not collide with native tag name`,track:`uitkx`,element:()=>(0,L.jsx)(Dx,{})}]},{id:`uitkx-component-reference`,title:`Components`,track:`uitkx`,pages:$x.map(e=>({id:`uitkx-${e.id}`,canonicalId:e.id,title:e.title,path:e.path,keywords:[`uitkx`,...e.keywords??[]],group:e.group,track:`uitkx`,element:()=>(0,L.jsx)(Tx,{title:e.title})}))},{id:`uitkx-concepts`,title:`Concepts & Environment`,track:`uitkx`,pages:[{id:`uitkx-concepts-page`,canonicalId:`concepts-and-environment`,title:`Concepts & Environment`,path:`/concepts`,keywords:[`uitkx`,`concepts`,`environment`,`defines`],searchContent:`concepts and environment uitkx authoring layer reactiveuitk runtime layer components intrinsic tags hooks markup structure reconciliation scheduling adapter application mental model write ui uitkx setup code local component generator runtime bridge unity ui toolkit core authoring rules intrinsic uitkx native tag names reserved custom components distinct names function-style components default form setup code first single returned markup tree state setters called directly as functions setcount(count + 1) companion partial classes host generated output environment defines compile-time environment tracing symbols env_dev env_staging env_prod environment labeling ruitk_trace_verbose ruitk_trace_basic runtime diagnostics editor-only diagnostic helpers development symbols`,track:`uitkx`,element:()=>(0,L.jsx)(kx,{})}]},{id:`uitkx-differences`,title:`Different from React`,track:`uitkx`,pages:[{id:`uitkx-differences-page`,canonicalId:`different-from-react`,title:`Different from React`,path:`/differences`,keywords:[`uitkx`,`react`,`hooks`,`rendering`],searchContent:`different from react uitkx borrows react component-and-hooks mental model unity ui toolkit c# runtime markup-first visualelement system scheduling model c# semantics state updates usestate behaves like react usestate setter directly value updater function uitkx lowers runtime hook implementation component rendering model reactiveuitk fiber schedule work asynchronously scheduler sliced render work deferred passive effects concurrent feature surface scheduler unity runtime constraints jsx-like syntax runtime representation browser dom model interop unity controls styles events first-class constraint apis differ from browser react conventions`,track:`uitkx`,element:()=>(0,L.jsx)(Lx,{})}]},{id:`uitkx-tooling`,title:`Tooling`,track:`uitkx`,pages:[{id:`uitkx-router-page`,canonicalId:`router`,title:`Router`,path:`/tooling/router`,keywords:[`uitkx`,`router`,`routes`,`navigation`],searchContent:`router uitkx routing authored directly in markup Router Route links routed child components returned UI tree Router establishes routing context subtree Route matches paths render elements RouterHooks setup code imperative navigation history control params query values navigation state RouterHooks.UseNavigate() pushes replaces locations UseGo() UseCanGo() back forward RouterHooks.UseLocationInfo() UseParams() UseQuery() UseNavigationState() expose active routed data RouterHooks.UseBlocker() intercept transitions unsaved guarded state declarative route composition imperative helpers @using ReactiveUITK.Router component RouterDemo var navigate RouterHooks.UseNavigate var parameters RouterHooks.UseParams var query RouterHooks.UseQuery RouterNavLink path label exact Route path /users/:id VisualElement Text User Not found`,track:`uitkx`,element:()=>(0,L.jsx)(Kx,{})},{id:`uitkx-signals-page`,canonicalId:`signals`,title:`Signals`,path:`/tooling/signals`,keywords:[`uitkx`,`signals`,`shared state`],searchContent:`signals shared-state primitive uitkx authoring model signal registry useSignal dispatch updates event handlers SignalsRuntime.EnsureInitialized Signals.Get SignalFactory.Get useMemo @using ReactiveUITK.Signals System component SignalCounterDemo var counterSignal SignalFactory.Get int demo.counter var count useSignal counterSignal Text Signal Counter Count Button Increment onClick counterSignal.Dispatch v + 1 Reset Dispatch 0 Style StyleKeys.FlexDirection row`,track:`uitkx`,element:()=>(0,L.jsx)(qx,{})},{id:`uitkx-hmr-page`,canonicalId:`hmr`,title:`Hot Module Replacement`,path:`/tooling/hmr`,keywords:[`uitkx`,`hmr`,`hot reload`,`live editing`,`instant preview`],searchContent:`hot module replacement hmr edit .uitkx files changes instantly unity editor without domain reload component state quick start open reactiveuitk hmr mode start edit save updates in-place hook state counters refs effects preserved assembly reloads filesystemwatcher detects parsed emitted compiled in-process roslyn microsoft.codeanalysis.csharp csc.dll fallback assembly.load render delegate swapped rootrenderer instances re-render hooks state preservation usestate useref useeffect usememo usecallback usecontext companion files .cs partial class style types create new companion auto-detected new component support cs0103 dependency auto-discovery cross-component assembly registry hmr window stats swap count error timing parse emit compile keyboard shortcuts toggle start stop lifecycle limitations old assemblies memory mono unload 10-30 kb per swap first compile jit warmup nuget cache troubleshooting console errors`,track:`uitkx`,element:()=>(0,L.jsx)(Xx,{})}]},{id:`uitkx-api`,title:`API`,track:`uitkx`,pages:[{id:`uitkx-api-page`,canonicalId:`api-reference`,title:`API Map`,path:`/api`,keywords:[`uitkx`,`api`,`hooks`,`runtime`],searchContent:`uitkx api map author markup hooks setup code runtime types mount integrate debug authoring surface .uitkx function-style components usestate useeffect usememo usesignal router context helpers intrinsic tags built-in reactiveuitk ui toolkit elements runtime layer virtualnode rootrenderer editorrootrendererutility props classes typed styles`,track:`uitkx`,element:()=>(0,L.jsx)(bx,{})}]},{id:`uitkx-reference-guides`,title:`Reference & Guides`,track:`uitkx`,pages:[{id:`uitkx-language-reference`,canonicalId:`language-reference`,title:`Language Reference`,path:`/reference`,keywords:[`uitkx`,`directives`,`syntax`,`control flow`,`expressions`],searchContent:`uitkx language reference directives syntax control flow expressions header directives @namespace My.Game.UI c# namespace generated class @component MyButton component class name must match filename @using System.Collections.Generic adds using directive generated file @props MyButtonProps props type consumed by the component @key root-key static key root element @inject ILogger logger dependency-injected field function-style components component keyword preamble declaration parameters typed optional default @using UnityEngine component Counter string label Count var count setCount useState return VisualElement Label text Button onClick setCount conditional rendering @if (isLoggedIn) Label Welcome back @else Button Log in login @foreach (var item in items) Label key item.Id text item.Name @switch (mode) @case dark Label Dark mode @default Label Light mode @for c-style for loop @while while loop @break exit loop @continue skip next iteration @(expr) @(MyCustomComponent) render component expression inline markup children {expr} c# expression attribute value literal plain string attribute {/* comment */} jsx-style block comment rules gotchas hook calls must be unconditional component top level single root element component names must match filename reconciliation`,track:`uitkx`,element:()=>(0,L.jsx)(Gx,{})},{id:`uitkx-diagnostics`,canonicalId:`diagnostics`,title:`Diagnostics`,path:`/diagnostics`,keywords:[`uitkx`,`diagnostics`,`errors`,`warnings`,`codes`],searchContent:`diagnostics reference diagnostic code uitkx source generator language server severity meaning fix source generator diagnostics compile time roslyn processing .uitkx files uitkx0001 warning unknown built-in element tag name pascalcase button label uitkx0002 warning unknown attribute element attribute name property props type uitkx0005 error missing required directive @namespace @component uitkx0006 warning @component name mismatch rename file uitkx0008 warning unknown function component type exists public static render method uitkx0009 warning @foreach child missing key stable unique identifier uitkx0010 warning duplicate sibling key unique key value uitkx0012 error directive order error @namespace above @component uitkx0013 error hook in conditional hook call must be at component top level not inside @if branch uitkx0014 error hook in loop must be at component top level not inside @foreach loop uitkx0015 error hook in switch case must be at component top level uitkx0016 error hook in event handler must be at component top level uitkx0017 error multiple root elements wrap in single container element visualelement uitkx0018 warning useeffect missing dependency array explicit dependency array.empty uitkx0019 warning loop variable used as key stable unique identifier not loop index uitkx0020 error ref on component without ref param parameter remove ref attribute uitkx0021 error ref ambiguous multiple ref params explicit prop name structural diagnostics language server real time squiggly underlines editor uitkx0101 uitkx0102 uitkx0103 uitkx0104 uitkx0105 uitkx0106 uitkx0107 hint unreachable code uitkx0108 uitkx0109 uitkx0111 unused parameter parser diagnostics malformed syntax uitkx0300 unexpected token uitkx0301 unclosed tag uitkx0302 mismatched closing tag uitkx0303 unexpected end of file uitkx0304 unclosed expression or block uitkx0305 unknown markup directive uitkx0306 @(expr) in setup code inline expressions only valid inside markup`,track:`uitkx`,element:()=>(0,L.jsx)(Ix,{})},{id:`uitkx-config`,canonicalId:`configuration`,title:`Configuration`,path:`/config`,keywords:[`uitkx`,`config`,`settings`,`vscode`,`extension`],searchContent:`configuration reference configuration options uitkx editor extensions formatter vs code extension settings uitkx.server.path string absolute path to custom uitkxlanguageserver.dll leave empty to use bundled server uitkx.server.dotnetpath string path to dotnet executable .net 8+ sdk non-standard location uitkx.trace.server enum off controls lsp trace output messages verbose json-rpc traffic output panel uitkx language server channel editor defaults extension automatically configures editor settings for .uitkx files editor.defaultformatter reactiveuitk.uitkx uitkx formatter editor.formatonsave true auto-format on save recommended editor.tabsize 2 uitkx 2-space indentation editor.insertspaces true spaces not tabs editor.bracketpaircolorization false disabled conflicting colors uitkx semantic tokens`,track:`uitkx`,element:()=>(0,L.jsx)(Mx,{})},{id:`uitkx-debugging`,canonicalId:`debugging`,title:`Debugging Guide`,path:`/debugging`,keywords:[`uitkx`,`debugging`,`troubleshooting`,`logs`,`generated code`],searchContent:`debugging guide diagnose fix common issues uitkx inspecting generated code .uitkx .uitkx.g.cs roslyn source generator vs code definition f12 generated symbol generatedfiles folder analyzers output directory #line directives map errors original .uitkx file line number library packagecache com.reactiveuitk understanding #line directives c# compiler error generated code lsp server logs detailed lsp communication trace level vs code settings uitkx.trace.server verbose output panel ctrl+shift+u uitkx language server channel json-rpc requests responses missing completions stale diagnostics textdocument/publishdiagnostics server crashes formatter issues formatting unexpected results syntax errors red squiggles format-on-save editor.defaultformatter reactiveuitk.uitkx shift+alt+f reporting bugs minimal .uitkx file reproduces problem exact error message diagnostic code editor extension version lsp trace output`,track:`uitkx`,element:()=>(0,L.jsx)(Fx,{})}]},{id:`uitkx-faq`,title:`FAQ`,track:`uitkx`,pages:[{id:`uitkx-faq-page`,canonicalId:`faq-page`,title:`FAQ`,path:`/faq`,keywords:[`faq`,`frequently asked questions`,`help`],searchContent:`frequently asked questions what is uitkx markup language authoring unity ui toolkit components react-like model .uitkx jsx-style hooks control flow roslyn source generator which unity versions supported unity 6.2 does uitkx work with existing ui toolkit code visualelement does uitkx add runtime overhead reconciliation scheduler per-frame cost aot-compatible production builds plain c# no interpreter no runtime codegen which editors supported vs code visual studio 2022 full extensions syntax highlighting completions hover diagnostics formatting jetbrains rider stub not officially supported v1 do i need the vs code extension source generator runs inside unity wrong colours briefly textmate grammar lsp semantic tokens 200ms what .net version language server .net 8 dotnet directive-header form function-style components @namespace @component @props setup code c# @using hmr hot module replacement build times bypasses unity compilation roslyn assembly.load 50-200ms hooks top level unconditional @if @foreach reconciler burst assembly-csharp-editor project settings burst aot exclusion list completions hover stopped working uitkx.trace.server verbose output panel debugging guide red squiggles saved @namespace before @component`,track:`uitkx`,element:()=>(0,L.jsx)(Qx,{})}]},{id:`uitkx-known-issues`,title:`Known Issues`,track:`uitkx`,pages:[{id:`uitkx-known-issues-page`,canonicalId:`known-issues-page`,title:`Known Issues`,path:`/known-issues`,keywords:[`issues`,`limitations`,`known issues`],searchContent:`known issues runtime multicolumnlistview briefly jump snap scrolling large data sets burst aot assembly resolution mono.cecil.assemblyresolutionexception failed resolve assembly assembly-csharp-editor project settings burst aot exclusion list editor-only assemblies uitkx types`,track:`uitkx`,element:()=>(0,L.jsx)(rx,{})}]},{id:`uitkx-roadmap`,title:`Roadmap`,track:`uitkx`,pages:[{id:`uitkx-roadmap-page`,canonicalId:`roadmap-page`,title:`Roadmap`,path:`/roadmap`,keywords:[`roadmap`,`future`,`plans`],searchContent:`roadmap documented future update planned features`,track:`uitkx`,element:()=>(0,L.jsx)(ax,{})}]}],rS=tS(`csharp`,yx,`/csharp`),iS={uitkx:nS,csharp:rS};[...nS,...rS];const aS=e=>e===`/csharp`||e.startsWith(`/csharp/`)?`csharp`:`uitkx`,oS=e=>iS[e],sS=e=>iS[e].flatMap(e=>{if(e.title===`Components`){let t=e.pages.filter(e=>e.group===`basic`),n=e.pages.filter(e=>e.group===`advanced`||!e.group);return[...t,...n]}return e.pages}),cS=[...sS(`uitkx`),...sS(`csharp`)],lS=e=>e===`uitkx`?`/`:`/csharp`,uS=(e,t)=>cS.find(n=>n.track===e&&n.canonicalId===t)?.path??lS(e),dS=yx.flatMap(e=>e.pages).filter(e=>e.path!==`/`).map(e=>({from:e.path,to:eS(`/csharp`,e.path)}));var fS=Rl((0,L.jsx)(`path`,{d:`M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14`}),`Search`),pS=Rl((0,L.jsx)(`path`,{d:`M12 1.27a11 11 0 00-3.48 21.46c.55.09.73-.28.73-.55v-1.84c-3.03.64-3.67-1.46-3.67-1.46-.55-1.29-1.28-1.65-1.28-1.65-.92-.65.1-.65.1-.65 1.1 0 1.73 1.1 1.73 1.1.92 1.65 2.57 1.2 3.21.92a2 2 0 01.64-1.47c-2.47-.27-5.04-1.19-5.04-5.5 0-1.1.46-2.1 1.2-2.84a3.76 3.76 0 010-2.93s.91-.28 3.11 1.1c1.8-.49 3.7-.49 5.5 0 2.1-1.38 3.02-1.1 3.02-1.1a3.76 3.76 0 010 2.93c.83.74 1.2 1.74 1.2 2.94 0 4.21-2.57 5.13-5.04 5.4.45.37.82.92.82 2.02v3.03c0 .27.1.64.73.55A11 11 0 0012 1.27`}),`GitHub`),mS={appBar:{borderBottom:1,borderColor:`divider`},toolbar:{display:`flex`,alignItems:`center`,gap:2},left:{display:`flex`,alignItems:`center`,gap:1.25},logo:{width:28,height:28,borderRadius:1},titleLink:{display:`flex`,alignItems:`center`,gap:.75,color:`inherit`,textDecoration:`none`},title:{fontWeight:600,letterSpacing:.3},center:{flex:1,display:`flex`,justifyContent:`center`},searchPaper:{p:`2px 8px`,display:`flex`,alignItems:`center`,gap:1,width:360,cursor:`text`},inputFlex:{flex:1},right:{ml:1,display:`flex`,alignItems:`center`,gap:1}};const hS=({onOpenSearch:e})=>{let{pathname:t}=Be(),n=aS(t),r=cS.find(e=>e.path===t)?.canonicalId??`introduction`,i=uS(`uitkx`,r),a=uS(`csharp`,r);return(0,L.jsx)(tf,{position:`sticky`,color:`default`,elevation:0,sx:mS.appBar,children:(0,L.jsxs)(N_,{sx:mS.toolbar,children:[(0,L.jsxs)(W,{sx:mS.left,children:[(0,L.jsxs)(bg,{component:tn,to:`/`,underline:`none`,sx:mS.titleLink,children:[(0,L.jsx)(W,{component:`img`,src:`/logo.png`,alt:`ReactiveUIToolKit logo`,sx:mS.logo}),(0,L.jsx)(U,{variant:`h6`,sx:mS.title,children:`ReactiveUIToolKit`})]}),(0,L.jsx)(ym,{label:`v0.2.26`,size:`small`}),(0,L.jsx)(ym,{label:`UITKX`,size:`small`,color:n===`uitkx`?`primary`:`default`,component:tn,to:i,clickable:!0}),(0,L.jsx)(ym,{label:`C#`,size:`small`,color:n===`csharp`?`primary`:`default`,component:tn,to:a,clickable:!0})]}),(0,L.jsx)(W,{sx:mS.center,children:(0,L.jsxs)(td,{sx:mS.searchPaper,variant:`outlined`,onClick:e,children:[(0,L.jsx)(fS,{fontSize:`small`}),(0,L.jsx)(zm,{placeholder:`Search ${n===`uitkx`?`UITKX`:`C#`} docs…`,sx:mS.inputFlex,readOnly:!0,autoFocus:!0})]})}),(0,L.jsxs)(W,{sx:mS.right,children:[(0,L.jsx)(ym,{label:`Unity 6.2+`,size:`small`}),(0,L.jsx)(Ud,{component:bg,href:`https://github.com/yanivkalfa/ReactiveUIToolKit.git`,target:`_blank`,rel:`noreferrer`,children:(0,L.jsx)(pS,{})})]})]})})};var gS=Rl((0,L.jsx)(`path`,{d:`m12 8-6 6 1.41 1.41L12 10.83l4.59 4.58L18 14z`}),`ExpandLess`),_S=Rl((0,L.jsx)(`path`,{d:`M16.59 8.59 12 13.17 7.41 8.59 6 10l6 6 6-6z`}),`ExpandMore`),vS={root:{width:280,borderRight:1,borderColor:`divider`,height:`100%`,overflow:`auto`,"&::-webkit-scrollbar":{width:8},"&::-webkit-scrollbar-track":{backgroundColor:`transparent`},"&::-webkit-scrollbar-thumb":{backgroundColor:`rgba(25,118,210,0.4)`,borderRadius:999,border:`2px solid transparent`,backgroundClip:`padding-box`},"&::-webkit-scrollbar-thumb:hover":{backgroundColor:`rgba(25,118,210,0.7)`},scrollbarWidth:`thin`,scrollbarColor:`rgba(25,118,210,0.6) transparent`},childItem:{pl:4},sectionTitle:{fontWeight:700},subgroupHeader:{pl:4,pt:1,pb:.5,fontSize:11,textTransform:`uppercase`,letterSpacing:.5,color:`text.secondary`},subgroupDivider:{mt:.5,mb:.5,opacity:.4}};const yS=()=>{let e=Be(),t=aS(e.pathname),n=oS(t).flatMap(e=>e.title===`Components`&&e.pages.some(e=>e.group)?[{...e,id:`components-common`,title:`Common Components`,pages:e.pages.filter(e=>e.group===`basic`)},{...e,id:`components-uncommon`,title:`Uncommon Components`,pages:e.pages.filter(e=>e.group===`advanced`||!e.group)}]:[e]),[r,i]=(0,x.useState)(()=>{let e={};return n.forEach((t,n)=>e[t.id]=n===0),e});return(0,L.jsxs)(W,{sx:vS.root,children:[(0,L.jsx)(W,{sx:{px:2,py:1.5},children:(0,L.jsx)(U,{variant:`overline`,color:`text.secondary`,children:t===`uitkx`?`UITKX Docs`:`C# Docs`})}),(0,L.jsx)(G,{disablePadding:!0,children:n.map(t=>{let n=!!r[t.id],a=t.pages.length===1,o=t.pages[0];return a?(0,L.jsxs)(W,{children:[(0,L.jsx)(jg,{component:tn,to:o.path,selected:e.pathname===o.path,children:(0,L.jsx)(q,{primary:(0,L.jsx)(U,{sx:vS.sectionTitle,children:t.title})})}),(0,L.jsx)(cg,{})]},t.id):(0,L.jsxs)(W,{children:[(0,L.jsxs)(jg,{onClick:()=>i({...r,[t.id]:!r[t.id]}),children:[(0,L.jsx)(q,{primary:(0,L.jsx)(U,{sx:vS.sectionTitle,children:t.title})}),n?(0,L.jsx)(gS,{}):(0,L.jsx)(_S,{})]}),(0,L.jsx)(Zu,{in:n,timeout:`auto`,unmountOnExit:!0,children:(0,L.jsx)(G,{disablePadding:!0,children:t.pages.map(t=>(0,L.jsx)(jg,{component:tn,to:t.path,selected:e.pathname===t.path,sx:vS.childItem,children:(0,L.jsx)(q,{primary:t.title})},t.id))})}),(0,L.jsx)(cg,{})]},t.id)})})]})};var bS={root:{display:`flex`,justifyContent:`space-between`,borderTop:1,borderColor:`divider`,mt:4,pt:2}};const xS=()=>{let e=Ue(),{pathname:t}=Be(),n=aS(t),r=(0,x.useMemo)(()=>sS(n),[n]),i=(0,x.useMemo)(()=>r.findIndex(e=>e.path===t),[r,t]),a=i>0?r[i-1]:void 0,o=i>=0&&i<r.length-1?r[i+1]:void 0;return(0,L.jsxs)(W,{sx:bS.root,children:[(0,L.jsx)(`span`,{children:a&&(0,L.jsxs)(ih,{onClick:()=>e(a.path),variant:`text`,children:[`← `,a.title]})}),(0,L.jsx)(`span`,{children:o&&(0,L.jsxs)(ih,{onClick:()=>e(o.path),variant:`text`,children:[o.title,` →`]})})]})};var SS=Rl((0,L.jsx)(`path`,{d:`M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z`}),`Close`),CS=new Map,wS=[`children`,`code`,`codeRuntime`,`codeEditor`,`text`,`primary`,`secondary`,`label`,`title`,`placeholder`,`description`];function TS(e,t,n){if(e==null||typeof e==`boolean`)return;if(typeof e==`string`){t.push(e);return}if(typeof e==`number`){t.push(String(e));return}if(Array.isArray(e)){for(let r of e)TS(r,t,n);return}if(typeof e!=`object`)return;let r=e;if(r.props){if(typeof r.type==`function`&&n<8)try{TS(r.type(r.props),t,n+1);return}catch{}for(let e of wS){let i=r.props[e];i!=null&&TS(i,t,n)}}}function ES(e){let t=CS.get(e.id);if(t!==void 0)return t;let n=[];try{TS(e.element(),n,0)}catch{}let r=n.join(` `).toLowerCase();return CS.set(e.id,r),r}var DS={header:{display:`flex`,alignItems:`center`,gap:1,mb:1},inputPaper:{p:1,display:`flex`,alignItems:`center`,gap:1,flex:1},noResults:{p:2},content:{pt:1}};const OS=({open:e,onClose:t})=>{let n=Ue(),{pathname:r}=Be(),i=aS(r),a=(0,x.useMemo)(()=>sS(i),[i]),[o,s]=(0,x.useState)(``),[c,l]=(0,x.useState)(0),u=(0,x.useRef)(null);(0,x.useEffect)(()=>{if(e){let e=setTimeout(()=>u.current?.focus(),50);return()=>clearTimeout(e)}},[e]);let d=()=>{s(``),l(0),t()},f=(0,x.useMemo)(()=>{let e=o.trim().toLowerCase();if(!e)return[];let t=e.split(/\s+/).filter(Boolean);return a.filter(e=>{let n=[e.title,(e.keywords||[]).join(` `),e.searchContent||``,ES(e)].join(` `).toLowerCase();return t.every(e=>n.includes(e))})},[a,o]);return(0,x.useEffect)(()=>l(0),[f]),(0,L.jsx)(Zh,{open:e,onClose:d,fullWidth:!0,maxWidth:`md`,children:(0,L.jsxs)(ng,{sx:DS.content,children:[(0,L.jsxs)(W,{sx:DS.header,children:[(0,L.jsxs)(td,{sx:DS.inputPaper,variant:`outlined`,children:[(0,L.jsx)(fS,{}),(0,L.jsx)(zm,{inputRef:u,placeholder:`Search ${i===`uitkx`?`UITKX`:`C#`} docs…`,value:o,onChange:e=>s(e.target.value),onKeyDown:e=>{e.key===`Escape`&&d(),e.key===`ArrowDown`&&(e.preventDefault(),l(e=>Math.min(e+1,f.length-1))),e.key===`ArrowUp`&&(e.preventDefault(),l(e=>Math.max(e-1,0))),e.key===`Enter`&&f[c]&&(d(),n(f[c].path))},sx:{flex:1}})]}),(0,L.jsx)(Ud,{onClick:d,"aria-label":`Close search`,children:(0,L.jsx)(SS,{})})]}),(0,L.jsxs)(G,{children:[f.map((e,t)=>(0,L.jsx)(jg,{selected:t===c,onClick:()=>{d(),n(e.path)},children:(0,L.jsx)(q,{primary:e.title,secondary:(e.keywords||[]).join(`, `)})},e.id)),o&&f.length===0&&(0,L.jsx)(U,{sx:DS.noResults,color:`text.secondary`,children:`No results`})]})]})})};var kS={shell:{display:`grid`,gridTemplateRows:`auto 1fr`,height:`100vh`},grid:{display:`grid`,gridTemplateColumns:`280px 1fr`,minHeight:0},content:{p:3,overflow:`auto`},main:{maxWidth:980}};const AS=pl({palette:{mode:`dark`,background:{default:`#181c26`,paper:`#202532`},divider:`#343a4c`,primary:{main:`#4cc2ff`},text:{primary:`#e5e9f5`,secondary:`#a0a8c0`}},shape:{borderRadius:8},typography:{fontSize:14,body1:{lineHeight:1.3,color:`#a0a8c0`},body2:{lineHeight:1.3,color:`#a0a8c0`},h4:{fontSize:28,fontWeight:600,letterSpacing:.2,color:`#e5e9f5`},h5:{fontSize:20,fontWeight:600,letterSpacing:.15,marginTop:16,color:`#e5e9f5`}},components:{MuiCssBaseline:{styleOverrides:{code:{fontFamily:`ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace`,backgroundColor:`#202532`,borderRadius:4,padding:`2px 6px`,border:`1px solid #343a4c`,fontSize:`0.85em`}}}}});(0,No.createRoot)(document.getElementById(`root`)).render((0,L.jsx)(x.StrictMode,{children:(0,L.jsx)(Qt,{children:(0,L.jsx)(()=>{let[e,t]=(0,x.useState)(!1);return(0,L.jsxs)(Tl,{theme:AS,children:[(0,L.jsx)(ph,{}),(0,L.jsxs)(W,{sx:kS.shell,children:[(0,L.jsx)(hS,{onOpenSearch:()=>t(!0)}),(0,L.jsxs)(W,{sx:kS.grid,children:[(0,L.jsx)(yS,{}),(0,L.jsx)(W,{sx:kS.content,children:(0,L.jsxs)(ht,{children:[cS.map(e=>(0,L.jsx)(pt,{path:e.path,element:(0,L.jsxs)(W,{component:`main`,sx:kS.main,children:[e.element(),(0,L.jsx)(xS,{})]})},e.id)),dS.map(e=>(0,L.jsx)(pt,{path:e.from,element:(0,L.jsx)(ft,{to:e.to,replace:!0})},`legacy-${e.from}`)),(0,L.jsx)(pt,{path:`*`,element:(0,L.jsxs)(L.Fragment,{children:[(0,L.jsx)(U,{variant:`h5`,gutterBottom:!0,children:`Not Found`}),(0,L.jsx)(bg,{component:tn,to:`/`,children:`Go to UITKX Introduction`})]})})]})})]})]}),(0,L.jsx)(OS,{open:e,onClose:()=>t(!1)})]})},{})})}));
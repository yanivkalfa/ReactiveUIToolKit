var e=Object.create,t=Object.defineProperty,n=Object.getOwnPropertyDescriptor,r=Object.getOwnPropertyNames,i=Object.getPrototypeOf,a=Object.prototype.hasOwnProperty,o=(e,t)=>()=>(t||e((t={exports:{}}).exports,t),t.exports),s=(e,i,o,s)=>{if(i&&typeof i==`object`||typeof i==`function`)for(var c=r(i),l=0,u=c.length,d;l<u;l++)d=c[l],!a.call(e,d)&&d!==o&&t(e,d,{get:(e=>i[e]).bind(null,d),enumerable:!(s=n(i,d))||s.enumerable});return e},c=(n,r,a)=>(a=n==null?{}:e(i(n)),s(r||!n||!n.__esModule?t(a,`default`,{value:n,enumerable:!0}):a,n));(function(){let e=document.createElement(`link`).relList;if(e&&e.supports&&e.supports(`modulepreload`))return;for(let e of document.querySelectorAll(`link[rel="modulepreload"]`))n(e);new MutationObserver(e=>{for(let t of e)if(t.type===`childList`)for(let e of t.addedNodes)e.tagName===`LINK`&&e.rel===`modulepreload`&&n(e)}).observe(document,{childList:!0,subtree:!0});function t(e){let t={};return e.integrity&&(t.integrity=e.integrity),e.referrerPolicy&&(t.referrerPolicy=e.referrerPolicy),e.crossOrigin===`use-credentials`?t.credentials=`include`:e.crossOrigin===`anonymous`?t.credentials=`omit`:t.credentials=`same-origin`,t}function n(e){if(e.ep)return;e.ep=!0;let n=t(e);fetch(e.href,n)}})();var l=o((e=>{var t=Symbol.for(`react.transitional.element`),n=Symbol.for(`react.portal`),r=Symbol.for(`react.fragment`),i=Symbol.for(`react.strict_mode`),a=Symbol.for(`react.profiler`),o=Symbol.for(`react.consumer`),s=Symbol.for(`react.context`),c=Symbol.for(`react.forward_ref`),l=Symbol.for(`react.suspense`),u=Symbol.for(`react.memo`),d=Symbol.for(`react.lazy`),f=Symbol.for(`react.activity`),p=Symbol.iterator;function m(e){return typeof e!=`object`||!e?null:(e=p&&e[p]||e[`@@iterator`],typeof e==`function`?e:null)}var h={isMounted:function(){return!1},enqueueForceUpdate:function(){},enqueueReplaceState:function(){},enqueueSetState:function(){}},g=Object.assign,_={};function v(e,t,n){this.props=e,this.context=t,this.refs=_,this.updater=n||h}v.prototype.isReactComponent={},v.prototype.setState=function(e,t){if(typeof e!=`object`&&typeof e!=`function`&&e!=null)throw Error(`takes an object of state variables to update or a function which returns an object of state variables.`);this.updater.enqueueSetState(this,e,t,`setState`)},v.prototype.forceUpdate=function(e){this.updater.enqueueForceUpdate(this,e,`forceUpdate`)};function y(){}y.prototype=v.prototype;function b(e,t,n){this.props=e,this.context=t,this.refs=_,this.updater=n||h}var x=b.prototype=new y;x.constructor=b,g(x,v.prototype),x.isPureReactComponent=!0;var S=Array.isArray;function C(){}var w={H:null,A:null,T:null,S:null},T=Object.prototype.hasOwnProperty;function E(e,n,r){var i=r.ref;return{$$typeof:t,type:e,key:n,ref:i===void 0?null:i,props:r}}function D(e,t){return E(e.type,t,e.props)}function O(e){return typeof e==`object`&&!!e&&e.$$typeof===t}function k(e){var t={"=":`=0`,":":`=2`};return`$`+e.replace(/[=:]/g,function(e){return t[e]})}var ee=/\/+/g;function A(e,t){return typeof e==`object`&&e&&e.key!=null?k(``+e.key):t.toString(36)}function j(e){switch(e.status){case`fulfilled`:return e.value;case`rejected`:throw e.reason;default:switch(typeof e.status==`string`?e.then(C,C):(e.status=`pending`,e.then(function(t){e.status===`pending`&&(e.status=`fulfilled`,e.value=t)},function(t){e.status===`pending`&&(e.status=`rejected`,e.reason=t)})),e.status){case`fulfilled`:return e.value;case`rejected`:throw e.reason}}throw e}function M(e,r,i,a,o){var s=typeof e;(s===`undefined`||s===`boolean`)&&(e=null);var c=!1;if(e===null)c=!0;else switch(s){case`bigint`:case`string`:case`number`:c=!0;break;case`object`:switch(e.$$typeof){case t:case n:c=!0;break;case d:return c=e._init,M(c(e._payload),r,i,a,o)}}if(c)return o=o(e),c=a===``?`.`+A(e,0):a,S(o)?(i=``,c!=null&&(i=c.replace(ee,`$&/`)+`/`),M(o,r,i,``,function(e){return e})):o!=null&&(O(o)&&(o=D(o,i+(o.key==null||e&&e.key===o.key?``:(``+o.key).replace(ee,`$&/`)+`/`)+c)),r.push(o)),1;c=0;var l=a===``?`.`:a+`:`;if(S(e))for(var u=0;u<e.length;u++)a=e[u],s=l+A(a,u),c+=M(a,r,i,s,o);else if(u=m(e),typeof u==`function`)for(e=u.call(e),u=0;!(a=e.next()).done;)a=a.value,s=l+A(a,u++),c+=M(a,r,i,s,o);else if(s===`object`){if(typeof e.then==`function`)return M(j(e),r,i,a,o);throw r=String(e),Error(`Objects are not valid as a React child (found: `+(r===`[object Object]`?`object with keys {`+Object.keys(e).join(`, `)+`}`:r)+`). If you meant to render a collection of children, use an array instead.`)}return c}function te(e,t,n){if(e==null)return e;var r=[],i=0;return M(e,r,``,``,function(e){return t.call(n,e,i++)}),r}function N(e){if(e._status===-1){var t=e._result;t=t(),t.then(function(t){(e._status===0||e._status===-1)&&(e._status=1,e._result=t)},function(t){(e._status===0||e._status===-1)&&(e._status=2,e._result=t)}),e._status===-1&&(e._status=0,e._result=t)}if(e._status===1)return e._result.default;throw e._result}var P=typeof reportError==`function`?reportError:function(e){if(typeof window==`object`&&typeof window.ErrorEvent==`function`){var t=new window.ErrorEvent(`error`,{bubbles:!0,cancelable:!0,message:typeof e==`object`&&e&&typeof e.message==`string`?String(e.message):String(e),error:e});if(!window.dispatchEvent(t))return}else if(typeof process==`object`&&typeof process.emit==`function`){process.emit(`uncaughtException`,e);return}console.error(e)},F={map:te,forEach:function(e,t,n){te(e,function(){t.apply(this,arguments)},n)},count:function(e){var t=0;return te(e,function(){t++}),t},toArray:function(e){return te(e,function(e){return e})||[]},only:function(e){if(!O(e))throw Error(`React.Children.only expected to receive a single React element child.`);return e}};e.Activity=f,e.Children=F,e.Component=v,e.Fragment=r,e.Profiler=a,e.PureComponent=b,e.StrictMode=i,e.Suspense=l,e.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE=w,e.__COMPILER_RUNTIME={__proto__:null,c:function(e){return w.H.useMemoCache(e)}},e.cache=function(e){return function(){return e.apply(null,arguments)}},e.cacheSignal=function(){return null},e.cloneElement=function(e,t,n){if(e==null)throw Error(`The argument must be a React element, but you passed `+e+`.`);var r=g({},e.props),i=e.key;if(t!=null)for(a in t.key!==void 0&&(i=``+t.key),t)!T.call(t,a)||a===`key`||a===`__self`||a===`__source`||a===`ref`&&t.ref===void 0||(r[a]=t[a]);var a=arguments.length-2;if(a===1)r.children=n;else if(1<a){for(var o=Array(a),s=0;s<a;s++)o[s]=arguments[s+2];r.children=o}return E(e.type,i,r)},e.createContext=function(e){return e={$$typeof:s,_currentValue:e,_currentValue2:e,_threadCount:0,Provider:null,Consumer:null},e.Provider=e,e.Consumer={$$typeof:o,_context:e},e},e.createElement=function(e,t,n){var r,i={},a=null;if(t!=null)for(r in t.key!==void 0&&(a=``+t.key),t)T.call(t,r)&&r!==`key`&&r!==`__self`&&r!==`__source`&&(i[r]=t[r]);var o=arguments.length-2;if(o===1)i.children=n;else if(1<o){for(var s=Array(o),c=0;c<o;c++)s[c]=arguments[c+2];i.children=s}if(e&&e.defaultProps)for(r in o=e.defaultProps,o)i[r]===void 0&&(i[r]=o[r]);return E(e,a,i)},e.createRef=function(){return{current:null}},e.forwardRef=function(e){return{$$typeof:c,render:e}},e.isValidElement=O,e.lazy=function(e){return{$$typeof:d,_payload:{_status:-1,_result:e},_init:N}},e.memo=function(e,t){return{$$typeof:u,type:e,compare:t===void 0?null:t}},e.startTransition=function(e){var t=w.T,n={};w.T=n;try{var r=e(),i=w.S;i!==null&&i(n,r),typeof r==`object`&&r&&typeof r.then==`function`&&r.then(C,P)}catch(e){P(e)}finally{t!==null&&n.types!==null&&(t.types=n.types),w.T=t}},e.unstable_useCacheRefresh=function(){return w.H.useCacheRefresh()},e.use=function(e){return w.H.use(e)},e.useActionState=function(e,t,n){return w.H.useActionState(e,t,n)},e.useCallback=function(e,t){return w.H.useCallback(e,t)},e.useContext=function(e){return w.H.useContext(e)},e.useDebugValue=function(){},e.useDeferredValue=function(e,t){return w.H.useDeferredValue(e,t)},e.useEffect=function(e,t){return w.H.useEffect(e,t)},e.useEffectEvent=function(e){return w.H.useEffectEvent(e)},e.useId=function(){return w.H.useId()},e.useImperativeHandle=function(e,t,n){return w.H.useImperativeHandle(e,t,n)},e.useInsertionEffect=function(e,t){return w.H.useInsertionEffect(e,t)},e.useLayoutEffect=function(e,t){return w.H.useLayoutEffect(e,t)},e.useMemo=function(e,t){return w.H.useMemo(e,t)},e.useOptimistic=function(e,t){return w.H.useOptimistic(e,t)},e.useReducer=function(e,t,n){return w.H.useReducer(e,t,n)},e.useRef=function(e){return w.H.useRef(e)},e.useState=function(e){return w.H.useState(e)},e.useSyncExternalStore=function(e,t,n){return w.H.useSyncExternalStore(e,t,n)},e.useTransition=function(){return w.H.useTransition()},e.version=`19.2.0`})),u=o(((e,t)=>{t.exports=l()})),d=o((e=>{function t(e,t){var n=e.length;e.push(t);a:for(;0<n;){var r=n-1>>>1,a=e[r];if(0<i(a,t))e[r]=t,e[n]=a,n=r;else break a}}function n(e){return e.length===0?null:e[0]}function r(e){if(e.length===0)return null;var t=e[0],n=e.pop();if(n!==t){e[0]=n;a:for(var r=0,a=e.length,o=a>>>1;r<o;){var s=2*(r+1)-1,c=e[s],l=s+1,u=e[l];if(0>i(c,n))l<a&&0>i(u,c)?(e[r]=u,e[l]=n,r=l):(e[r]=c,e[s]=n,r=s);else if(l<a&&0>i(u,n))e[r]=u,e[l]=n,r=l;else break a}}return t}function i(e,t){var n=e.sortIndex-t.sortIndex;return n===0?e.id-t.id:n}if(e.unstable_now=void 0,typeof performance==`object`&&typeof performance.now==`function`){var a=performance;e.unstable_now=function(){return a.now()}}else{var o=Date,s=o.now();e.unstable_now=function(){return o.now()-s}}var c=[],l=[],u=1,d=null,f=3,p=!1,m=!1,h=!1,g=!1,_=typeof setTimeout==`function`?setTimeout:null,v=typeof clearTimeout==`function`?clearTimeout:null,y=typeof setImmediate<`u`?setImmediate:null;function b(e){for(var i=n(l);i!==null;){if(i.callback===null)r(l);else if(i.startTime<=e)r(l),i.sortIndex=i.expirationTime,t(c,i);else break;i=n(l)}}function x(e){if(h=!1,b(e),!m)if(n(c)!==null)m=!0,S||(S=!0,O());else{var t=n(l);t!==null&&A(x,t.startTime-e)}}var S=!1,C=-1,w=5,T=-1;function E(){return g?!0:!(e.unstable_now()-T<w)}function D(){if(g=!1,S){var t=e.unstable_now();T=t;var i=!0;try{a:{m=!1,h&&(h=!1,v(C),C=-1),p=!0;var a=f;try{b:{for(b(t),d=n(c);d!==null&&!(d.expirationTime>t&&E());){var o=d.callback;if(typeof o==`function`){d.callback=null,f=d.priorityLevel;var s=o(d.expirationTime<=t);if(t=e.unstable_now(),typeof s==`function`){d.callback=s,b(t),i=!0;break b}d===n(c)&&r(c),b(t)}else r(c);d=n(c)}if(d!==null)i=!0;else{var u=n(l);u!==null&&A(x,u.startTime-t),i=!1}}break a}finally{d=null,f=a,p=!1}i=void 0}}finally{i?O():S=!1}}}var O;if(typeof y==`function`)O=function(){y(D)};else if(typeof MessageChannel<`u`){var k=new MessageChannel,ee=k.port2;k.port1.onmessage=D,O=function(){ee.postMessage(null)}}else O=function(){_(D,0)};function A(t,n){C=_(function(){t(e.unstable_now())},n)}e.unstable_IdlePriority=5,e.unstable_ImmediatePriority=1,e.unstable_LowPriority=4,e.unstable_NormalPriority=3,e.unstable_Profiling=null,e.unstable_UserBlockingPriority=2,e.unstable_cancelCallback=function(e){e.callback=null},e.unstable_forceFrameRate=function(e){0>e||125<e?console.error(`forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported`):w=0<e?Math.floor(1e3/e):5},e.unstable_getCurrentPriorityLevel=function(){return f},e.unstable_next=function(e){switch(f){case 1:case 2:case 3:var t=3;break;default:t=f}var n=f;f=t;try{return e()}finally{f=n}},e.unstable_requestPaint=function(){g=!0},e.unstable_runWithPriority=function(e,t){switch(e){case 1:case 2:case 3:case 4:case 5:break;default:e=3}var n=f;f=e;try{return t()}finally{f=n}},e.unstable_scheduleCallback=function(r,i,a){var o=e.unstable_now();switch(typeof a==`object`&&a?(a=a.delay,a=typeof a==`number`&&0<a?o+a:o):a=o,r){case 1:var s=-1;break;case 2:s=250;break;case 5:s=1073741823;break;case 4:s=1e4;break;default:s=5e3}return s=a+s,r={id:u++,callback:i,priorityLevel:r,startTime:a,expirationTime:s,sortIndex:-1},a>o?(r.sortIndex=a,t(l,r),n(c)===null&&r===n(l)&&(h?(v(C),C=-1):h=!0,A(x,a-o))):(r.sortIndex=s,t(c,r),m||p||(m=!0,S||(S=!0,O()))),r},e.unstable_shouldYield=E,e.unstable_wrapCallback=function(e){var t=f;return function(){var n=f;f=t;try{return e.apply(this,arguments)}finally{f=n}}}})),f=o(((e,t)=>{t.exports=d()})),p=o((e=>{var t=u();function n(e){var t=`https://react.dev/errors/`+e;if(1<arguments.length){t+=`?args[]=`+encodeURIComponent(arguments[1]);for(var n=2;n<arguments.length;n++)t+=`&args[]=`+encodeURIComponent(arguments[n])}return`Minified React error #`+e+`; visit `+t+` for the full message or use the non-minified dev environment for full errors and additional helpful warnings.`}function r(){}var i={d:{f:r,r:function(){throw Error(n(522))},D:r,C:r,L:r,m:r,X:r,S:r,M:r},p:0,findDOMNode:null},a=Symbol.for(`react.portal`);function o(e,t,n){var r=3<arguments.length&&arguments[3]!==void 0?arguments[3]:null;return{$$typeof:a,key:r==null?null:``+r,children:e,containerInfo:t,implementation:n}}var s=t.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE;function c(e,t){if(e===`font`)return``;if(typeof t==`string`)return t===`use-credentials`?t:``}e.__DOM_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE=i,e.createPortal=function(e,t){var r=2<arguments.length&&arguments[2]!==void 0?arguments[2]:null;if(!t||t.nodeType!==1&&t.nodeType!==9&&t.nodeType!==11)throw Error(n(299));return o(e,t,null,r)},e.flushSync=function(e){var t=s.T,n=i.p;try{if(s.T=null,i.p=2,e)return e()}finally{s.T=t,i.p=n,i.d.f()}},e.preconnect=function(e,t){typeof e==`string`&&(t?(t=t.crossOrigin,t=typeof t==`string`?t===`use-credentials`?t:``:void 0):t=null,i.d.C(e,t))},e.prefetchDNS=function(e){typeof e==`string`&&i.d.D(e)},e.preinit=function(e,t){if(typeof e==`string`&&t&&typeof t.as==`string`){var n=t.as,r=c(n,t.crossOrigin),a=typeof t.integrity==`string`?t.integrity:void 0,o=typeof t.fetchPriority==`string`?t.fetchPriority:void 0;n===`style`?i.d.S(e,typeof t.precedence==`string`?t.precedence:void 0,{crossOrigin:r,integrity:a,fetchPriority:o}):n===`script`&&i.d.X(e,{crossOrigin:r,integrity:a,fetchPriority:o,nonce:typeof t.nonce==`string`?t.nonce:void 0})}},e.preinitModule=function(e,t){if(typeof e==`string`)if(typeof t==`object`&&t){if(t.as==null||t.as===`script`){var n=c(t.as,t.crossOrigin);i.d.M(e,{crossOrigin:n,integrity:typeof t.integrity==`string`?t.integrity:void 0,nonce:typeof t.nonce==`string`?t.nonce:void 0})}}else t??i.d.M(e)},e.preload=function(e,t){if(typeof e==`string`&&typeof t==`object`&&t&&typeof t.as==`string`){var n=t.as,r=c(n,t.crossOrigin);i.d.L(e,n,{crossOrigin:r,integrity:typeof t.integrity==`string`?t.integrity:void 0,nonce:typeof t.nonce==`string`?t.nonce:void 0,type:typeof t.type==`string`?t.type:void 0,fetchPriority:typeof t.fetchPriority==`string`?t.fetchPriority:void 0,referrerPolicy:typeof t.referrerPolicy==`string`?t.referrerPolicy:void 0,imageSrcSet:typeof t.imageSrcSet==`string`?t.imageSrcSet:void 0,imageSizes:typeof t.imageSizes==`string`?t.imageSizes:void 0,media:typeof t.media==`string`?t.media:void 0})}},e.preloadModule=function(e,t){if(typeof e==`string`)if(t){var n=c(t.as,t.crossOrigin);i.d.m(e,{as:typeof t.as==`string`&&t.as!==`script`?t.as:void 0,crossOrigin:n,integrity:typeof t.integrity==`string`?t.integrity:void 0})}else i.d.m(e)},e.requestFormReset=function(e){i.d.r(e)},e.unstable_batchedUpdates=function(e,t){return e(t)},e.useFormState=function(e,t,n){return s.H.useFormState(e,t,n)},e.useFormStatus=function(){return s.H.useHostTransitionStatus()},e.version=`19.2.0`})),m=o(((e,t)=>{function n(){if(!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__>`u`||typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE!=`function`))try{__REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(n)}catch(e){console.error(e)}}n(),t.exports=p()})),h=o((e=>{var t=f(),n=u(),r=m();function i(e){var t=`https://react.dev/errors/`+e;if(1<arguments.length){t+=`?args[]=`+encodeURIComponent(arguments[1]);for(var n=2;n<arguments.length;n++)t+=`&args[]=`+encodeURIComponent(arguments[n])}return`Minified React error #`+e+`; visit `+t+` for the full message or use the non-minified dev environment for full errors and additional helpful warnings.`}function a(e){return!(!e||e.nodeType!==1&&e.nodeType!==9&&e.nodeType!==11)}function o(e){var t=e,n=e;if(e.alternate)for(;t.return;)t=t.return;else{e=t;do t=e,t.flags&4098&&(n=t.return),e=t.return;while(e)}return t.tag===3?n:null}function s(e){if(e.tag===13){var t=e.memoizedState;if(t===null&&(e=e.alternate,e!==null&&(t=e.memoizedState)),t!==null)return t.dehydrated}return null}function c(e){if(e.tag===31){var t=e.memoizedState;if(t===null&&(e=e.alternate,e!==null&&(t=e.memoizedState)),t!==null)return t.dehydrated}return null}function l(e){if(o(e)!==e)throw Error(i(188))}function d(e){var t=e.alternate;if(!t){if(t=o(e),t===null)throw Error(i(188));return t===e?e:null}for(var n=e,r=t;;){var a=n.return;if(a===null)break;var s=a.alternate;if(s===null){if(r=a.return,r!==null){n=r;continue}break}if(a.child===s.child){for(s=a.child;s;){if(s===n)return l(a),e;if(s===r)return l(a),t;s=s.sibling}throw Error(i(188))}if(n.return!==r.return)n=a,r=s;else{for(var c=!1,u=a.child;u;){if(u===n){c=!0,n=a,r=s;break}if(u===r){c=!0,r=a,n=s;break}u=u.sibling}if(!c){for(u=s.child;u;){if(u===n){c=!0,n=s,r=a;break}if(u===r){c=!0,r=s,n=a;break}u=u.sibling}if(!c)throw Error(i(189))}}if(n.alternate!==r)throw Error(i(190))}if(n.tag!==3)throw Error(i(188));return n.stateNode.current===n?e:t}function p(e){var t=e.tag;if(t===5||t===26||t===27||t===6)return e;for(e=e.child;e!==null;){if(t=p(e),t!==null)return t;e=e.sibling}return null}var h=Object.assign,g=Symbol.for(`react.element`),_=Symbol.for(`react.transitional.element`),v=Symbol.for(`react.portal`),y=Symbol.for(`react.fragment`),b=Symbol.for(`react.strict_mode`),x=Symbol.for(`react.profiler`),S=Symbol.for(`react.consumer`),C=Symbol.for(`react.context`),w=Symbol.for(`react.forward_ref`),T=Symbol.for(`react.suspense`),E=Symbol.for(`react.suspense_list`),D=Symbol.for(`react.memo`),O=Symbol.for(`react.lazy`),k=Symbol.for(`react.activity`),ee=Symbol.for(`react.memo_cache_sentinel`),A=Symbol.iterator;function j(e){return typeof e!=`object`||!e?null:(e=A&&e[A]||e[`@@iterator`],typeof e==`function`?e:null)}var M=Symbol.for(`react.client.reference`);function te(e){if(e==null)return null;if(typeof e==`function`)return e.$$typeof===M?null:e.displayName||e.name||null;if(typeof e==`string`)return e;switch(e){case y:return`Fragment`;case x:return`Profiler`;case b:return`StrictMode`;case T:return`Suspense`;case E:return`SuspenseList`;case k:return`Activity`}if(typeof e==`object`)switch(e.$$typeof){case v:return`Portal`;case C:return e.displayName||`Context`;case S:return(e._context.displayName||`Context`)+`.Consumer`;case w:var t=e.render;return e=e.displayName,e||=(e=t.displayName||t.name||``,e===``?`ForwardRef`:`ForwardRef(`+e+`)`),e;case D:return t=e.displayName||null,t===null?te(e.type)||`Memo`:t;case O:t=e._payload,e=e._init;try{return te(e(t))}catch{}}return null}var N=Array.isArray,P=n.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE,F=r.__DOM_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE,ne={pending:!1,data:null,method:null,action:null},re=[],ie=-1;function ae(e){return{current:e}}function oe(e){0>ie||(e.current=re[ie],re[ie]=null,ie--)}function se(e,t){ie++,re[ie]=e.current,e.current=t}var ce=ae(null),le=ae(null),ue=ae(null),de=ae(null);function fe(e,t){switch(se(ue,t),se(le,e),se(ce,null),t.nodeType){case 9:case 11:e=(e=t.documentElement)&&(e=e.namespaceURI)?Qd(e):0;break;default:if(e=t.tagName,t=t.namespaceURI)t=Qd(t),e=$d(t,e);else switch(e){case`svg`:e=1;break;case`math`:e=2;break;default:e=0}}oe(ce),se(ce,e)}function pe(){oe(ce),oe(le),oe(ue)}function me(e){e.memoizedState!==null&&se(de,e);var t=ce.current,n=$d(t,e.type);t!==n&&(se(le,e),se(ce,n))}function he(e){le.current===e&&(oe(ce),oe(le)),de.current===e&&(oe(de),cp._currentValue=ne)}var ge,_e;function ve(e){if(ge===void 0)try{throw Error()}catch(e){var t=e.stack.trim().match(/\n( *(at )?)/);ge=t&&t[1]||``,_e=-1<e.stack.indexOf(`
    at`)?` (<anonymous>)`:-1<e.stack.indexOf(`@`)?`@unknown:0:0`:``}return`
`+ge+e+_e}var ye=!1;function be(e,t){if(!e||ye)return``;ye=!0;var n=Error.prepareStackTrace;Error.prepareStackTrace=void 0;try{var r={DetermineComponentFrameRoot:function(){try{if(t){var n=function(){throw Error()};if(Object.defineProperty(n.prototype,`props`,{set:function(){throw Error()}}),typeof Reflect==`object`&&Reflect.construct){try{Reflect.construct(n,[])}catch(e){var r=e}Reflect.construct(e,[],n)}else{try{n.call()}catch(e){r=e}e.call(n.prototype)}}else{try{throw Error()}catch(e){r=e}(n=e())&&typeof n.catch==`function`&&n.catch(function(){})}}catch(e){if(e&&r&&typeof e.stack==`string`)return[e.stack,r.stack]}return[null,null]}};r.DetermineComponentFrameRoot.displayName=`DetermineComponentFrameRoot`;var i=Object.getOwnPropertyDescriptor(r.DetermineComponentFrameRoot,`name`);i&&i.configurable&&Object.defineProperty(r.DetermineComponentFrameRoot,`name`,{value:`DetermineComponentFrameRoot`});var a=r.DetermineComponentFrameRoot(),o=a[0],s=a[1];if(o&&s){var c=o.split(`
`),l=s.split(`
`);for(i=r=0;r<c.length&&!c[r].includes(`DetermineComponentFrameRoot`);)r++;for(;i<l.length&&!l[i].includes(`DetermineComponentFrameRoot`);)i++;if(r===c.length||i===l.length)for(r=c.length-1,i=l.length-1;1<=r&&0<=i&&c[r]!==l[i];)i--;for(;1<=r&&0<=i;r--,i--)if(c[r]!==l[i]){if(r!==1||i!==1)do if(r--,i--,0>i||c[r]!==l[i]){var u=`
`+c[r].replace(` at new `,` at `);return e.displayName&&u.includes(`<anonymous>`)&&(u=u.replace(`<anonymous>`,e.displayName)),u}while(1<=r&&0<=i);break}}}finally{ye=!1,Error.prepareStackTrace=n}return(n=e?e.displayName||e.name:``)?ve(n):``}function xe(e,t){switch(e.tag){case 26:case 27:case 5:return ve(e.type);case 16:return ve(`Lazy`);case 13:return e.child!==t&&t!==null?ve(`Suspense Fallback`):ve(`Suspense`);case 19:return ve(`SuspenseList`);case 0:case 15:return be(e.type,!1);case 11:return be(e.type.render,!1);case 1:return be(e.type,!0);case 31:return ve(`Activity`);default:return``}}function Se(e){try{var t=``,n=null;do t+=xe(e,n),n=e,e=e.return;while(e);return t}catch(e){return`
Error generating stack: `+e.message+`
`+e.stack}}var Ce=Object.prototype.hasOwnProperty,we=t.unstable_scheduleCallback,Te=t.unstable_cancelCallback,Ee=t.unstable_shouldYield,De=t.unstable_requestPaint,Oe=t.unstable_now,ke=t.unstable_getCurrentPriorityLevel,Ae=t.unstable_ImmediatePriority,je=t.unstable_UserBlockingPriority,Me=t.unstable_NormalPriority,Ne=t.unstable_LowPriority,Pe=t.unstable_IdlePriority,Fe=t.log,Ie=t.unstable_setDisableYieldValue,Le=null,Re=null;function ze(e){if(typeof Fe==`function`&&Ie(e),Re&&typeof Re.setStrictMode==`function`)try{Re.setStrictMode(Le,e)}catch{}}var Be=Math.clz32?Math.clz32:Ue,Ve=Math.log,He=Math.LN2;function Ue(e){return e>>>=0,e===0?32:31-(Ve(e)/He|0)|0}var We=256,Ge=262144,Ke=4194304;function qe(e){var t=e&42;if(t!==0)return t;switch(e&-e){case 1:return 1;case 2:return 2;case 4:return 4;case 8:return 8;case 16:return 16;case 32:return 32;case 64:return 64;case 128:return 128;case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:return e&261888;case 262144:case 524288:case 1048576:case 2097152:return e&3932160;case 4194304:case 8388608:case 16777216:case 33554432:return e&62914560;case 67108864:return 67108864;case 134217728:return 134217728;case 268435456:return 268435456;case 536870912:return 536870912;case 1073741824:return 0;default:return e}}function Je(e,t,n){var r=e.pendingLanes;if(r===0)return 0;var i=0,a=e.suspendedLanes,o=e.pingedLanes;e=e.warmLanes;var s=r&134217727;return s===0?(s=r&~a,s===0?o===0?n||(n=r&~e,n!==0&&(i=qe(n))):i=qe(o):i=qe(s)):(r=s&~a,r===0?(o&=s,o===0?n||(n=s&~e,n!==0&&(i=qe(n))):i=qe(o)):i=qe(r)),i===0?0:t!==0&&t!==i&&(t&a)===0&&(a=i&-i,n=t&-t,a>=n||a===32&&n&4194048)?t:i}function Ye(e,t){return(e.pendingLanes&~(e.suspendedLanes&~e.pingedLanes)&t)===0}function Xe(e,t){switch(e){case 1:case 2:case 4:case 8:case 64:return t+250;case 16:case 32:case 128:case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:case 262144:case 524288:case 1048576:case 2097152:return t+5e3;case 4194304:case 8388608:case 16777216:case 33554432:return-1;case 67108864:case 134217728:case 268435456:case 536870912:case 1073741824:return-1;default:return-1}}function Ze(){var e=Ke;return Ke<<=1,!(Ke&62914560)&&(Ke=4194304),e}function Qe(e){for(var t=[],n=0;31>n;n++)t.push(e);return t}function $e(e,t){e.pendingLanes|=t,t!==268435456&&(e.suspendedLanes=0,e.pingedLanes=0,e.warmLanes=0)}function et(e,t,n,r,i,a){var o=e.pendingLanes;e.pendingLanes=n,e.suspendedLanes=0,e.pingedLanes=0,e.warmLanes=0,e.expiredLanes&=n,e.entangledLanes&=n,e.errorRecoveryDisabledLanes&=n,e.shellSuspendCounter=0;var s=e.entanglements,c=e.expirationTimes,l=e.hiddenUpdates;for(n=o&~n;0<n;){var u=31-Be(n),d=1<<u;s[u]=0,c[u]=-1;var f=l[u];if(f!==null)for(l[u]=null,u=0;u<f.length;u++){var p=f[u];p!==null&&(p.lane&=-536870913)}n&=~d}r!==0&&tt(e,r,0),a!==0&&i===0&&e.tag!==0&&(e.suspendedLanes|=a&~(o&~t))}function tt(e,t,n){e.pendingLanes|=t,e.suspendedLanes&=~t;var r=31-Be(t);e.entangledLanes|=t,e.entanglements[r]=e.entanglements[r]|1073741824|n&261930}function nt(e,t){var n=e.entangledLanes|=t;for(e=e.entanglements;n;){var r=31-Be(n),i=1<<r;i&t|e[r]&t&&(e[r]|=t),n&=~i}}function rt(e,t){var n=t&-t;return n=n&42?1:it(n),(n&(e.suspendedLanes|t))===0?n:0}function it(e){switch(e){case 2:e=1;break;case 8:e=4;break;case 32:e=16;break;case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:case 262144:case 524288:case 1048576:case 2097152:case 4194304:case 8388608:case 16777216:case 33554432:e=128;break;case 268435456:e=134217728;break;default:e=0}return e}function at(e){return e&=-e,2<e?8<e?e&134217727?32:268435456:8:2}function ot(){var e=F.p;return e===0?(e=window.event,e===void 0?32:wp(e.type)):e}function st(e,t){var n=F.p;try{return F.p=e,t()}finally{F.p=n}}var ct=Math.random().toString(36).slice(2),lt=`__reactFiber$`+ct,ut=`__reactProps$`+ct,dt=`__reactContainer$`+ct,ft=`__reactEvents$`+ct,pt=`__reactListeners$`+ct,mt=`__reactHandles$`+ct,ht=`__reactResources$`+ct,gt=`__reactMarker$`+ct;function _t(e){delete e[lt],delete e[ut],delete e[ft],delete e[pt],delete e[mt]}function vt(e){var t=e[lt];if(t)return t;for(var n=e.parentNode;n;){if(t=n[dt]||n[lt]){if(n=t.alternate,t.child!==null||n!==null&&n.child!==null)for(e=xf(e);e!==null;){if(n=e[lt])return n;e=xf(e)}return t}e=n,n=e.parentNode}return null}function yt(e){if(e=e[lt]||e[dt]){var t=e.tag;if(t===5||t===6||t===13||t===31||t===26||t===27||t===3)return e}return null}function bt(e){var t=e.tag;if(t===5||t===26||t===27||t===6)return e.stateNode;throw Error(i(33))}function xt(e){var t=e[ht];return t||=e[ht]={hoistableStyles:new Map,hoistableScripts:new Map},t}function St(e){e[gt]=!0}var Ct=new Set,wt={};function Tt(e,t){Et(e,t),Et(e+`Capture`,t)}function Et(e,t){for(wt[e]=t,e=0;e<t.length;e++)Ct.add(t[e])}var Dt=RegExp(`^[:A-Z_a-z\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD][:A-Z_a-z\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD\\-.0-9\\u00B7\\u0300-\\u036F\\u203F-\\u2040]*$`),Ot={},kt={};function At(e){return Ce.call(kt,e)?!0:Ce.call(Ot,e)?!1:Dt.test(e)?kt[e]=!0:(Ot[e]=!0,!1)}function jt(e,t,n){if(At(t))if(n===null)e.removeAttribute(t);else{switch(typeof n){case`undefined`:case`function`:case`symbol`:e.removeAttribute(t);return;case`boolean`:var r=t.toLowerCase().slice(0,5);if(r!==`data-`&&r!==`aria-`){e.removeAttribute(t);return}}e.setAttribute(t,``+n)}}function Mt(e,t,n){if(n===null)e.removeAttribute(t);else{switch(typeof n){case`undefined`:case`function`:case`symbol`:case`boolean`:e.removeAttribute(t);return}e.setAttribute(t,``+n)}}function Nt(e,t,n,r){if(r===null)e.removeAttribute(n);else{switch(typeof r){case`undefined`:case`function`:case`symbol`:case`boolean`:e.removeAttribute(n);return}e.setAttributeNS(t,n,``+r)}}function Pt(e){switch(typeof e){case`bigint`:case`boolean`:case`number`:case`string`:case`undefined`:return e;case`object`:return e;default:return``}}function Ft(e){var t=e.type;return(e=e.nodeName)&&e.toLowerCase()===`input`&&(t===`checkbox`||t===`radio`)}function It(e,t,n){var r=Object.getOwnPropertyDescriptor(e.constructor.prototype,t);if(!e.hasOwnProperty(t)&&r!==void 0&&typeof r.get==`function`&&typeof r.set==`function`){var i=r.get,a=r.set;return Object.defineProperty(e,t,{configurable:!0,get:function(){return i.call(this)},set:function(e){n=``+e,a.call(this,e)}}),Object.defineProperty(e,t,{enumerable:r.enumerable}),{getValue:function(){return n},setValue:function(e){n=``+e},stopTracking:function(){e._valueTracker=null,delete e[t]}}}}function Lt(e){if(!e._valueTracker){var t=Ft(e)?`checked`:`value`;e._valueTracker=It(e,t,``+e[t])}}function Rt(e){if(!e)return!1;var t=e._valueTracker;if(!t)return!0;var n=t.getValue(),r=``;return e&&(r=Ft(e)?e.checked?`true`:`false`:e.value),e=r,e===n?!1:(t.setValue(e),!0)}function zt(e){if(e||=typeof document<`u`?document:void 0,e===void 0)return null;try{return e.activeElement||e.body}catch{return e.body}}var Bt=/[\n"\\]/g;function Vt(e){return e.replace(Bt,function(e){return`\\`+e.charCodeAt(0).toString(16)+` `})}function Ht(e,t,n,r,i,a,o,s){e.name=``,o!=null&&typeof o!=`function`&&typeof o!=`symbol`&&typeof o!=`boolean`?e.type=o:e.removeAttribute(`type`),t==null?o!==`submit`&&o!==`reset`||e.removeAttribute(`value`):o===`number`?(t===0&&e.value===``||e.value!=t)&&(e.value=``+Pt(t)):e.value!==``+Pt(t)&&(e.value=``+Pt(t)),t==null?n==null?r!=null&&e.removeAttribute(`value`):Wt(e,o,Pt(n)):Wt(e,o,Pt(t)),i==null&&a!=null&&(e.defaultChecked=!!a),i!=null&&(e.checked=i&&typeof i!=`function`&&typeof i!=`symbol`),s!=null&&typeof s!=`function`&&typeof s!=`symbol`&&typeof s!=`boolean`?e.name=``+Pt(s):e.removeAttribute(`name`)}function Ut(e,t,n,r,i,a,o,s){if(a!=null&&typeof a!=`function`&&typeof a!=`symbol`&&typeof a!=`boolean`&&(e.type=a),t!=null||n!=null){if(!(a!==`submit`&&a!==`reset`||t!=null)){Lt(e);return}n=n==null?``:``+Pt(n),t=t==null?n:``+Pt(t),s||t===e.value||(e.value=t),e.defaultValue=t}r??=i,r=typeof r!=`function`&&typeof r!=`symbol`&&!!r,e.checked=s?e.checked:!!r,e.defaultChecked=!!r,o!=null&&typeof o!=`function`&&typeof o!=`symbol`&&typeof o!=`boolean`&&(e.name=o),Lt(e)}function Wt(e,t,n){t===`number`&&zt(e.ownerDocument)===e||e.defaultValue===``+n||(e.defaultValue=``+n)}function Gt(e,t,n,r){if(e=e.options,t){t={};for(var i=0;i<n.length;i++)t[`$`+n[i]]=!0;for(n=0;n<e.length;n++)i=t.hasOwnProperty(`$`+e[n].value),e[n].selected!==i&&(e[n].selected=i),i&&r&&(e[n].defaultSelected=!0)}else{for(n=``+Pt(n),t=null,i=0;i<e.length;i++){if(e[i].value===n){e[i].selected=!0,r&&(e[i].defaultSelected=!0);return}t!==null||e[i].disabled||(t=e[i])}t!==null&&(t.selected=!0)}}function Kt(e,t,n){if(t!=null&&(t=``+Pt(t),t!==e.value&&(e.value=t),n==null)){e.defaultValue!==t&&(e.defaultValue=t);return}e.defaultValue=n==null?``:``+Pt(n)}function qt(e,t,n,r){if(t==null){if(r!=null){if(n!=null)throw Error(i(92));if(N(r)){if(1<r.length)throw Error(i(93));r=r[0]}n=r}n??=``,t=n}n=Pt(t),e.defaultValue=n,r=e.textContent,r===n&&r!==``&&r!==null&&(e.value=r),Lt(e)}function Jt(e,t){if(t){var n=e.firstChild;if(n&&n===e.lastChild&&n.nodeType===3){n.nodeValue=t;return}}e.textContent=t}var Yt=new Set(`animationIterationCount aspectRatio borderImageOutset borderImageSlice borderImageWidth boxFlex boxFlexGroup boxOrdinalGroup columnCount columns flex flexGrow flexPositive flexShrink flexNegative flexOrder gridArea gridRow gridRowEnd gridRowSpan gridRowStart gridColumn gridColumnEnd gridColumnSpan gridColumnStart fontWeight lineClamp lineHeight opacity order orphans scale tabSize widows zIndex zoom fillOpacity floodOpacity stopOpacity strokeDasharray strokeDashoffset strokeMiterlimit strokeOpacity strokeWidth MozAnimationIterationCount MozBoxFlex MozBoxFlexGroup MozLineClamp msAnimationIterationCount msFlex msZoom msFlexGrow msFlexNegative msFlexOrder msFlexPositive msFlexShrink msGridColumn msGridColumnSpan msGridRow msGridRowSpan WebkitAnimationIterationCount WebkitBoxFlex WebKitBoxFlexGroup WebkitBoxOrdinalGroup WebkitColumnCount WebkitColumns WebkitFlex WebkitFlexGrow WebkitFlexPositive WebkitFlexShrink WebkitLineClamp`.split(` `));function Xt(e,t,n){var r=t.indexOf(`--`)===0;n==null||typeof n==`boolean`||n===``?r?e.setProperty(t,``):t===`float`?e.cssFloat=``:e[t]=``:r?e.setProperty(t,n):typeof n!=`number`||n===0||Yt.has(t)?t===`float`?e.cssFloat=n:e[t]=(``+n).trim():e[t]=n+`px`}function Zt(e,t,n){if(t!=null&&typeof t!=`object`)throw Error(i(62));if(e=e.style,n!=null){for(var r in n)!n.hasOwnProperty(r)||t!=null&&t.hasOwnProperty(r)||(r.indexOf(`--`)===0?e.setProperty(r,``):r===`float`?e.cssFloat=``:e[r]=``);for(var a in t)r=t[a],t.hasOwnProperty(a)&&n[a]!==r&&Xt(e,a,r)}else for(var o in t)t.hasOwnProperty(o)&&Xt(e,o,t[o])}function Qt(e){if(e.indexOf(`-`)===-1)return!1;switch(e){case`annotation-xml`:case`color-profile`:case`font-face`:case`font-face-src`:case`font-face-uri`:case`font-face-format`:case`font-face-name`:case`missing-glyph`:return!1;default:return!0}}var $t=new Map([[`acceptCharset`,`accept-charset`],[`htmlFor`,`for`],[`httpEquiv`,`http-equiv`],[`crossOrigin`,`crossorigin`],[`accentHeight`,`accent-height`],[`alignmentBaseline`,`alignment-baseline`],[`arabicForm`,`arabic-form`],[`baselineShift`,`baseline-shift`],[`capHeight`,`cap-height`],[`clipPath`,`clip-path`],[`clipRule`,`clip-rule`],[`colorInterpolation`,`color-interpolation`],[`colorInterpolationFilters`,`color-interpolation-filters`],[`colorProfile`,`color-profile`],[`colorRendering`,`color-rendering`],[`dominantBaseline`,`dominant-baseline`],[`enableBackground`,`enable-background`],[`fillOpacity`,`fill-opacity`],[`fillRule`,`fill-rule`],[`floodColor`,`flood-color`],[`floodOpacity`,`flood-opacity`],[`fontFamily`,`font-family`],[`fontSize`,`font-size`],[`fontSizeAdjust`,`font-size-adjust`],[`fontStretch`,`font-stretch`],[`fontStyle`,`font-style`],[`fontVariant`,`font-variant`],[`fontWeight`,`font-weight`],[`glyphName`,`glyph-name`],[`glyphOrientationHorizontal`,`glyph-orientation-horizontal`],[`glyphOrientationVertical`,`glyph-orientation-vertical`],[`horizAdvX`,`horiz-adv-x`],[`horizOriginX`,`horiz-origin-x`],[`imageRendering`,`image-rendering`],[`letterSpacing`,`letter-spacing`],[`lightingColor`,`lighting-color`],[`markerEnd`,`marker-end`],[`markerMid`,`marker-mid`],[`markerStart`,`marker-start`],[`overlinePosition`,`overline-position`],[`overlineThickness`,`overline-thickness`],[`paintOrder`,`paint-order`],[`panose-1`,`panose-1`],[`pointerEvents`,`pointer-events`],[`renderingIntent`,`rendering-intent`],[`shapeRendering`,`shape-rendering`],[`stopColor`,`stop-color`],[`stopOpacity`,`stop-opacity`],[`strikethroughPosition`,`strikethrough-position`],[`strikethroughThickness`,`strikethrough-thickness`],[`strokeDasharray`,`stroke-dasharray`],[`strokeDashoffset`,`stroke-dashoffset`],[`strokeLinecap`,`stroke-linecap`],[`strokeLinejoin`,`stroke-linejoin`],[`strokeMiterlimit`,`stroke-miterlimit`],[`strokeOpacity`,`stroke-opacity`],[`strokeWidth`,`stroke-width`],[`textAnchor`,`text-anchor`],[`textDecoration`,`text-decoration`],[`textRendering`,`text-rendering`],[`transformOrigin`,`transform-origin`],[`underlinePosition`,`underline-position`],[`underlineThickness`,`underline-thickness`],[`unicodeBidi`,`unicode-bidi`],[`unicodeRange`,`unicode-range`],[`unitsPerEm`,`units-per-em`],[`vAlphabetic`,`v-alphabetic`],[`vHanging`,`v-hanging`],[`vIdeographic`,`v-ideographic`],[`vMathematical`,`v-mathematical`],[`vectorEffect`,`vector-effect`],[`vertAdvY`,`vert-adv-y`],[`vertOriginX`,`vert-origin-x`],[`vertOriginY`,`vert-origin-y`],[`wordSpacing`,`word-spacing`],[`writingMode`,`writing-mode`],[`xmlnsXlink`,`xmlns:xlink`],[`xHeight`,`x-height`]]),en=/^[\u0000-\u001F ]*j[\r\n\t]*a[\r\n\t]*v[\r\n\t]*a[\r\n\t]*s[\r\n\t]*c[\r\n\t]*r[\r\n\t]*i[\r\n\t]*p[\r\n\t]*t[\r\n\t]*:/i;function tn(e){return en.test(``+e)?`javascript:throw new Error('React has blocked a javascript: URL as a security precaution.')`:e}function nn(){}var rn=null;function an(e){return e=e.target||e.srcElement||window,e.correspondingUseElement&&(e=e.correspondingUseElement),e.nodeType===3?e.parentNode:e}var on=null,sn=null;function cn(e){var t=yt(e);if(t&&(e=t.stateNode)){var n=e[ut]||null;a:switch(e=t.stateNode,t.type){case`input`:if(Ht(e,n.value,n.defaultValue,n.defaultValue,n.checked,n.defaultChecked,n.type,n.name),t=n.name,n.type===`radio`&&t!=null){for(n=e;n.parentNode;)n=n.parentNode;for(n=n.querySelectorAll(`input[name="`+Vt(``+t)+`"][type="radio"]`),t=0;t<n.length;t++){var r=n[t];if(r!==e&&r.form===e.form){var a=r[ut]||null;if(!a)throw Error(i(90));Ht(r,a.value,a.defaultValue,a.defaultValue,a.checked,a.defaultChecked,a.type,a.name)}}for(t=0;t<n.length;t++)r=n[t],r.form===e.form&&Rt(r)}break a;case`textarea`:Kt(e,n.value,n.defaultValue);break a;case`select`:t=n.value,t!=null&&Gt(e,!!n.multiple,t,!1)}}}var ln=!1;function un(e,t,n){if(ln)return e(t,n);ln=!0;try{return e(t)}finally{if(ln=!1,(on!==null||sn!==null)&&(ku(),on&&(t=on,e=sn,sn=on=null,cn(t),e)))for(t=0;t<e.length;t++)cn(e[t])}}function dn(e,t){var n=e.stateNode;if(n===null)return null;var r=n[ut]||null;if(r===null)return null;n=r[t];a:switch(t){case`onClick`:case`onClickCapture`:case`onDoubleClick`:case`onDoubleClickCapture`:case`onMouseDown`:case`onMouseDownCapture`:case`onMouseMove`:case`onMouseMoveCapture`:case`onMouseUp`:case`onMouseUpCapture`:case`onMouseEnter`:(r=!r.disabled)||(e=e.type,r=!(e===`button`||e===`input`||e===`select`||e===`textarea`)),e=!r;break a;default:e=!1}if(e)return null;if(n&&typeof n!=`function`)throw Error(i(231,t,typeof n));return n}var fn=!(typeof window>`u`||window.document===void 0||window.document.createElement===void 0),pn=!1;if(fn)try{var mn={};Object.defineProperty(mn,`passive`,{get:function(){pn=!0}}),window.addEventListener(`test`,mn,mn),window.removeEventListener(`test`,mn,mn)}catch{pn=!1}var hn=null,gn=null,_n=null;function vn(){if(_n)return _n;var e,t=gn,n=t.length,r,i=`value`in hn?hn.value:hn.textContent,a=i.length;for(e=0;e<n&&t[e]===i[e];e++);var o=n-e;for(r=1;r<=o&&t[n-r]===i[a-r];r++);return _n=i.slice(e,1<r?1-r:void 0)}function yn(e){var t=e.keyCode;return`charCode`in e?(e=e.charCode,e===0&&t===13&&(e=13)):e=t,e===10&&(e=13),32<=e||e===13?e:0}function bn(){return!0}function xn(){return!1}function Sn(e){function t(t,n,r,i,a){for(var o in this._reactName=t,this._targetInst=r,this.type=n,this.nativeEvent=i,this.target=a,this.currentTarget=null,e)e.hasOwnProperty(o)&&(t=e[o],this[o]=t?t(i):i[o]);return this.isDefaultPrevented=(i.defaultPrevented==null?!1===i.returnValue:i.defaultPrevented)?bn:xn,this.isPropagationStopped=xn,this}return h(t.prototype,{preventDefault:function(){this.defaultPrevented=!0;var e=this.nativeEvent;e&&(e.preventDefault?e.preventDefault():typeof e.returnValue!=`unknown`&&(e.returnValue=!1),this.isDefaultPrevented=bn)},stopPropagation:function(){var e=this.nativeEvent;e&&(e.stopPropagation?e.stopPropagation():typeof e.cancelBubble!=`unknown`&&(e.cancelBubble=!0),this.isPropagationStopped=bn)},persist:function(){},isPersistent:bn}),t}var Cn={eventPhase:0,bubbles:0,cancelable:0,timeStamp:function(e){return e.timeStamp||Date.now()},defaultPrevented:0,isTrusted:0},wn=Sn(Cn),Tn=h({},Cn,{view:0,detail:0}),En=Sn(Tn),Dn,On,kn,An=h({},Tn,{screenX:0,screenY:0,clientX:0,clientY:0,pageX:0,pageY:0,ctrlKey:0,shiftKey:0,altKey:0,metaKey:0,getModifierState:Vn,button:0,buttons:0,relatedTarget:function(e){return e.relatedTarget===void 0?e.fromElement===e.srcElement?e.toElement:e.fromElement:e.relatedTarget},movementX:function(e){return`movementX`in e?e.movementX:(e!==kn&&(kn&&e.type===`mousemove`?(Dn=e.screenX-kn.screenX,On=e.screenY-kn.screenY):On=Dn=0,kn=e),Dn)},movementY:function(e){return`movementY`in e?e.movementY:On}}),jn=Sn(An),Mn=Sn(h({},An,{dataTransfer:0})),Nn=Sn(h({},Tn,{relatedTarget:0})),Pn=Sn(h({},Cn,{animationName:0,elapsedTime:0,pseudoElement:0})),Fn=Sn(h({},Cn,{clipboardData:function(e){return`clipboardData`in e?e.clipboardData:window.clipboardData}})),In=Sn(h({},Cn,{data:0})),Ln={Esc:`Escape`,Spacebar:` `,Left:`ArrowLeft`,Up:`ArrowUp`,Right:`ArrowRight`,Down:`ArrowDown`,Del:`Delete`,Win:`OS`,Menu:`ContextMenu`,Apps:`ContextMenu`,Scroll:`ScrollLock`,MozPrintableKey:`Unidentified`},Rn={8:`Backspace`,9:`Tab`,12:`Clear`,13:`Enter`,16:`Shift`,17:`Control`,18:`Alt`,19:`Pause`,20:`CapsLock`,27:`Escape`,32:` `,33:`PageUp`,34:`PageDown`,35:`End`,36:`Home`,37:`ArrowLeft`,38:`ArrowUp`,39:`ArrowRight`,40:`ArrowDown`,45:`Insert`,46:`Delete`,112:`F1`,113:`F2`,114:`F3`,115:`F4`,116:`F5`,117:`F6`,118:`F7`,119:`F8`,120:`F9`,121:`F10`,122:`F11`,123:`F12`,144:`NumLock`,145:`ScrollLock`,224:`Meta`},zn={Alt:`altKey`,Control:`ctrlKey`,Meta:`metaKey`,Shift:`shiftKey`};function Bn(e){var t=this.nativeEvent;return t.getModifierState?t.getModifierState(e):(e=zn[e])?!!t[e]:!1}function Vn(){return Bn}var Hn=Sn(h({},Tn,{key:function(e){if(e.key){var t=Ln[e.key]||e.key;if(t!==`Unidentified`)return t}return e.type===`keypress`?(e=yn(e),e===13?`Enter`:String.fromCharCode(e)):e.type===`keydown`||e.type===`keyup`?Rn[e.keyCode]||`Unidentified`:``},code:0,location:0,ctrlKey:0,shiftKey:0,altKey:0,metaKey:0,repeat:0,locale:0,getModifierState:Vn,charCode:function(e){return e.type===`keypress`?yn(e):0},keyCode:function(e){return e.type===`keydown`||e.type===`keyup`?e.keyCode:0},which:function(e){return e.type===`keypress`?yn(e):e.type===`keydown`||e.type===`keyup`?e.keyCode:0}})),Un=Sn(h({},An,{pointerId:0,width:0,height:0,pressure:0,tangentialPressure:0,tiltX:0,tiltY:0,twist:0,pointerType:0,isPrimary:0})),Wn=Sn(h({},Tn,{touches:0,targetTouches:0,changedTouches:0,altKey:0,metaKey:0,ctrlKey:0,shiftKey:0,getModifierState:Vn})),Gn=Sn(h({},Cn,{propertyName:0,elapsedTime:0,pseudoElement:0})),Kn=Sn(h({},An,{deltaX:function(e){return`deltaX`in e?e.deltaX:`wheelDeltaX`in e?-e.wheelDeltaX:0},deltaY:function(e){return`deltaY`in e?e.deltaY:`wheelDeltaY`in e?-e.wheelDeltaY:`wheelDelta`in e?-e.wheelDelta:0},deltaZ:0,deltaMode:0})),qn=Sn(h({},Cn,{newState:0,oldState:0})),Jn=[9,13,27,32],Yn=fn&&`CompositionEvent`in window,Xn=null;fn&&`documentMode`in document&&(Xn=document.documentMode);var Zn=fn&&`TextEvent`in window&&!Xn,Qn=fn&&(!Yn||Xn&&8<Xn&&11>=Xn),$n=` `,er=!1;function tr(e,t){switch(e){case`keyup`:return Jn.indexOf(t.keyCode)!==-1;case`keydown`:return t.keyCode!==229;case`keypress`:case`mousedown`:case`focusout`:return!0;default:return!1}}function nr(e){return e=e.detail,typeof e==`object`&&`data`in e?e.data:null}var rr=!1;function ir(e,t){switch(e){case`compositionend`:return nr(t);case`keypress`:return t.which===32?(er=!0,$n):null;case`textInput`:return e=t.data,e===$n&&er?null:e;default:return null}}function ar(e,t){if(rr)return e===`compositionend`||!Yn&&tr(e,t)?(e=vn(),_n=gn=hn=null,rr=!1,e):null;switch(e){case`paste`:return null;case`keypress`:if(!(t.ctrlKey||t.altKey||t.metaKey)||t.ctrlKey&&t.altKey){if(t.char&&1<t.char.length)return t.char;if(t.which)return String.fromCharCode(t.which)}return null;case`compositionend`:return Qn&&t.locale!==`ko`?null:t.data;default:return null}}var or={color:!0,date:!0,datetime:!0,"datetime-local":!0,email:!0,month:!0,number:!0,password:!0,range:!0,search:!0,tel:!0,text:!0,time:!0,url:!0,week:!0};function sr(e){var t=e&&e.nodeName&&e.nodeName.toLowerCase();return t===`input`?!!or[e.type]:t===`textarea`}function cr(e,t,n,r){on?sn?sn.push(r):sn=[r]:on=r,t=Id(t,`onChange`),0<t.length&&(n=new wn(`onChange`,`change`,null,n,r),e.push({event:n,listeners:t}))}var lr=null,ur=null;function dr(e){kd(e,0)}function fr(e){if(Rt(bt(e)))return e}function pr(e,t){if(e===`change`)return t}var mr=!1;if(fn){var hr;if(fn){var gr=`oninput`in document;if(!gr){var _r=document.createElement(`div`);_r.setAttribute(`oninput`,`return;`),gr=typeof _r.oninput==`function`}hr=gr}else hr=!1;mr=hr&&(!document.documentMode||9<document.documentMode)}function vr(){lr&&(lr.detachEvent(`onpropertychange`,yr),ur=lr=null)}function yr(e){if(e.propertyName===`value`&&fr(ur)){var t=[];cr(t,ur,e,an(e)),un(dr,t)}}function br(e,t,n){e===`focusin`?(vr(),lr=t,ur=n,lr.attachEvent(`onpropertychange`,yr)):e===`focusout`&&vr()}function xr(e){if(e===`selectionchange`||e===`keyup`||e===`keydown`)return fr(ur)}function Sr(e,t){if(e===`click`)return fr(t)}function Cr(e,t){if(e===`input`||e===`change`)return fr(t)}function wr(e,t){return e===t&&(e!==0||1/e==1/t)||e!==e&&t!==t}var Tr=typeof Object.is==`function`?Object.is:wr;function Er(e,t){if(Tr(e,t))return!0;if(typeof e!=`object`||!e||typeof t!=`object`||!t)return!1;var n=Object.keys(e),r=Object.keys(t);if(n.length!==r.length)return!1;for(r=0;r<n.length;r++){var i=n[r];if(!Ce.call(t,i)||!Tr(e[i],t[i]))return!1}return!0}function Dr(e){for(;e&&e.firstChild;)e=e.firstChild;return e}function Or(e,t){var n=Dr(e);e=0;for(var r;n;){if(n.nodeType===3){if(r=e+n.textContent.length,e<=t&&r>=t)return{node:n,offset:t-e};e=r}a:{for(;n;){if(n.nextSibling){n=n.nextSibling;break a}n=n.parentNode}n=void 0}n=Dr(n)}}function kr(e,t){return e&&t?e===t?!0:e&&e.nodeType===3?!1:t&&t.nodeType===3?kr(e,t.parentNode):`contains`in e?e.contains(t):e.compareDocumentPosition?!!(e.compareDocumentPosition(t)&16):!1:!1}function Ar(e){e=e!=null&&e.ownerDocument!=null&&e.ownerDocument.defaultView!=null?e.ownerDocument.defaultView:window;for(var t=zt(e.document);t instanceof e.HTMLIFrameElement;){try{var n=typeof t.contentWindow.location.href==`string`}catch{n=!1}if(n)e=t.contentWindow;else break;t=zt(e.document)}return t}function jr(e){var t=e&&e.nodeName&&e.nodeName.toLowerCase();return t&&(t===`input`&&(e.type===`text`||e.type===`search`||e.type===`tel`||e.type===`url`||e.type===`password`)||t===`textarea`||e.contentEditable===`true`)}var Mr=fn&&`documentMode`in document&&11>=document.documentMode,Nr=null,Pr=null,Fr=null,Ir=!1;function Lr(e,t,n){var r=n.window===n?n.document:n.nodeType===9?n:n.ownerDocument;Ir||Nr==null||Nr!==zt(r)||(r=Nr,`selectionStart`in r&&jr(r)?r={start:r.selectionStart,end:r.selectionEnd}:(r=(r.ownerDocument&&r.ownerDocument.defaultView||window).getSelection(),r={anchorNode:r.anchorNode,anchorOffset:r.anchorOffset,focusNode:r.focusNode,focusOffset:r.focusOffset}),Fr&&Er(Fr,r)||(Fr=r,r=Id(Pr,`onSelect`),0<r.length&&(t=new wn(`onSelect`,`select`,null,t,n),e.push({event:t,listeners:r}),t.target=Nr)))}function Rr(e,t){var n={};return n[e.toLowerCase()]=t.toLowerCase(),n[`Webkit`+e]=`webkit`+t,n[`Moz`+e]=`moz`+t,n}var zr={animationend:Rr(`Animation`,`AnimationEnd`),animationiteration:Rr(`Animation`,`AnimationIteration`),animationstart:Rr(`Animation`,`AnimationStart`),transitionrun:Rr(`Transition`,`TransitionRun`),transitionstart:Rr(`Transition`,`TransitionStart`),transitioncancel:Rr(`Transition`,`TransitionCancel`),transitionend:Rr(`Transition`,`TransitionEnd`)},Br={},Vr={};fn&&(Vr=document.createElement(`div`).style,`AnimationEvent`in window||(delete zr.animationend.animation,delete zr.animationiteration.animation,delete zr.animationstart.animation),`TransitionEvent`in window||delete zr.transitionend.transition);function Hr(e){if(Br[e])return Br[e];if(!zr[e])return e;var t=zr[e],n;for(n in t)if(t.hasOwnProperty(n)&&n in Vr)return Br[e]=t[n];return e}var Ur=Hr(`animationend`),Wr=Hr(`animationiteration`),Gr=Hr(`animationstart`),Kr=Hr(`transitionrun`),qr=Hr(`transitionstart`),Jr=Hr(`transitioncancel`),Yr=Hr(`transitionend`),Xr=new Map,Zr=`abort auxClick beforeToggle cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel`.split(` `);Zr.push(`scrollEnd`);function Qr(e,t){Xr.set(e,t),Tt(t,[e])}var $r=typeof reportError==`function`?reportError:function(e){if(typeof window==`object`&&typeof window.ErrorEvent==`function`){var t=new window.ErrorEvent(`error`,{bubbles:!0,cancelable:!0,message:typeof e==`object`&&e&&typeof e.message==`string`?String(e.message):String(e),error:e});if(!window.dispatchEvent(t))return}else if(typeof process==`object`&&typeof process.emit==`function`){process.emit(`uncaughtException`,e);return}console.error(e)},ei=[],ti=0,ni=0;function ri(){for(var e=ti,t=ni=ti=0;t<e;){var n=ei[t];ei[t++]=null;var r=ei[t];ei[t++]=null;var i=ei[t];ei[t++]=null;var a=ei[t];if(ei[t++]=null,r!==null&&i!==null){var o=r.pending;o===null?i.next=i:(i.next=o.next,o.next=i),r.pending=i}a!==0&&si(n,i,a)}}function ii(e,t,n,r){ei[ti++]=e,ei[ti++]=t,ei[ti++]=n,ei[ti++]=r,ni|=r,e.lanes|=r,e=e.alternate,e!==null&&(e.lanes|=r)}function ai(e,t,n,r){return ii(e,t,n,r),ci(e)}function oi(e,t){return ii(e,null,null,t),ci(e)}function si(e,t,n){e.lanes|=n;var r=e.alternate;r!==null&&(r.lanes|=n);for(var i=!1,a=e.return;a!==null;)a.childLanes|=n,r=a.alternate,r!==null&&(r.childLanes|=n),a.tag===22&&(e=a.stateNode,e===null||e._visibility&1||(i=!0)),e=a,a=a.return;return e.tag===3?(a=e.stateNode,i&&t!==null&&(i=31-Be(n),e=a.hiddenUpdates,r=e[i],r===null?e[i]=[t]:r.push(t),t.lane=n|536870912),a):null}function ci(e){if(50<bu)throw bu=0,xu=null,Error(i(185));for(var t=e.return;t!==null;)e=t,t=e.return;return e.tag===3?e.stateNode:null}var li={};function ui(e,t,n,r){this.tag=e,this.key=n,this.sibling=this.child=this.return=this.stateNode=this.type=this.elementType=null,this.index=0,this.refCleanup=this.ref=null,this.pendingProps=t,this.dependencies=this.memoizedState=this.updateQueue=this.memoizedProps=null,this.mode=r,this.subtreeFlags=this.flags=0,this.deletions=null,this.childLanes=this.lanes=0,this.alternate=null}function di(e,t,n,r){return new ui(e,t,n,r)}function fi(e){return e=e.prototype,!(!e||!e.isReactComponent)}function pi(e,t){var n=e.alternate;return n===null?(n=di(e.tag,t,e.key,e.mode),n.elementType=e.elementType,n.type=e.type,n.stateNode=e.stateNode,n.alternate=e,e.alternate=n):(n.pendingProps=t,n.type=e.type,n.flags=0,n.subtreeFlags=0,n.deletions=null),n.flags=e.flags&65011712,n.childLanes=e.childLanes,n.lanes=e.lanes,n.child=e.child,n.memoizedProps=e.memoizedProps,n.memoizedState=e.memoizedState,n.updateQueue=e.updateQueue,t=e.dependencies,n.dependencies=t===null?null:{lanes:t.lanes,firstContext:t.firstContext},n.sibling=e.sibling,n.index=e.index,n.ref=e.ref,n.refCleanup=e.refCleanup,n}function mi(e,t){e.flags&=65011714;var n=e.alternate;return n===null?(e.childLanes=0,e.lanes=t,e.child=null,e.subtreeFlags=0,e.memoizedProps=null,e.memoizedState=null,e.updateQueue=null,e.dependencies=null,e.stateNode=null):(e.childLanes=n.childLanes,e.lanes=n.lanes,e.child=n.child,e.subtreeFlags=0,e.deletions=null,e.memoizedProps=n.memoizedProps,e.memoizedState=n.memoizedState,e.updateQueue=n.updateQueue,e.type=n.type,t=n.dependencies,e.dependencies=t===null?null:{lanes:t.lanes,firstContext:t.firstContext}),e}function hi(e,t,n,r,a,o){var s=0;if(r=e,typeof e==`function`)fi(e)&&(s=1);else if(typeof e==`string`)s=$f(e,n,ce.current)?26:e===`html`||e===`head`||e===`body`?27:5;else a:switch(e){case k:return e=di(31,n,t,a),e.elementType=k,e.lanes=o,e;case y:return gi(n.children,a,o,t);case b:s=8,a|=24;break;case x:return e=di(12,n,t,a|2),e.elementType=x,e.lanes=o,e;case T:return e=di(13,n,t,a),e.elementType=T,e.lanes=o,e;case E:return e=di(19,n,t,a),e.elementType=E,e.lanes=o,e;default:if(typeof e==`object`&&e)switch(e.$$typeof){case C:s=10;break a;case S:s=9;break a;case w:s=11;break a;case D:s=14;break a;case O:s=16,r=null;break a}s=29,n=Error(i(130,e===null?`null`:typeof e,``)),r=null}return t=di(s,n,t,a),t.elementType=e,t.type=r,t.lanes=o,t}function gi(e,t,n,r){return e=di(7,e,r,t),e.lanes=n,e}function _i(e,t,n){return e=di(6,e,null,t),e.lanes=n,e}function vi(e){var t=di(18,null,null,0);return t.stateNode=e,t}function yi(e,t,n){return t=di(4,e.children===null?[]:e.children,e.key,t),t.lanes=n,t.stateNode={containerInfo:e.containerInfo,pendingChildren:null,implementation:e.implementation},t}var bi=new WeakMap;function xi(e,t){if(typeof e==`object`&&e){var n=bi.get(e);return n===void 0?(t={value:e,source:t,stack:Se(t)},bi.set(e,t),t):n}return{value:e,source:t,stack:Se(t)}}var Si=[],Ci=0,wi=null,Ti=0,Ei=[],Di=0,Oi=null,ki=1,Ai=``;function ji(e,t){Si[Ci++]=Ti,Si[Ci++]=wi,wi=e,Ti=t}function Mi(e,t,n){Ei[Di++]=ki,Ei[Di++]=Ai,Ei[Di++]=Oi,Oi=e;var r=ki;e=Ai;var i=32-Be(r)-1;r&=~(1<<i),n+=1;var a=32-Be(t)+i;if(30<a){var o=i-i%5;a=(r&(1<<o)-1).toString(32),r>>=o,i-=o,ki=1<<32-Be(t)+i|n<<i|r,Ai=a+e}else ki=1<<a|n<<i|r,Ai=e}function Ni(e){e.return!==null&&(ji(e,1),Mi(e,1,0))}function Pi(e){for(;e===wi;)wi=Si[--Ci],Si[Ci]=null,Ti=Si[--Ci],Si[Ci]=null;for(;e===Oi;)Oi=Ei[--Di],Ei[Di]=null,Ai=Ei[--Di],Ei[Di]=null,ki=Ei[--Di],Ei[Di]=null}function Fi(e,t){Ei[Di++]=ki,Ei[Di++]=Ai,Ei[Di++]=Oi,ki=t.id,Ai=t.overflow,Oi=e}var Ii=null,Li=null,Ri=!1,zi=null,Bi=!1,Vi=Error(i(519));function Hi(e){throw qi(xi(Error(i(418,1<arguments.length&&arguments[1]!==void 0&&arguments[1]?`text`:`HTML`,``)),e)),Vi}function I(e){var t=e.stateNode,n=e.type,r=e.memoizedProps;switch(t[lt]=e,t[ut]=r,n){case`dialog`:H(`cancel`,t),H(`close`,t);break;case`iframe`:case`object`:case`embed`:H(`load`,t);break;case`video`:case`audio`:for(n=0;n<Dd.length;n++)H(Dd[n],t);break;case`source`:H(`error`,t);break;case`img`:case`image`:case`link`:H(`error`,t),H(`load`,t);break;case`details`:H(`toggle`,t);break;case`input`:H(`invalid`,t),Ut(t,r.value,r.defaultValue,r.checked,r.defaultChecked,r.type,r.name,!0);break;case`select`:H(`invalid`,t);break;case`textarea`:H(`invalid`,t),qt(t,r.value,r.defaultValue,r.children)}n=r.children,typeof n!=`string`&&typeof n!=`number`&&typeof n!=`bigint`||t.textContent===``+n||!0===r.suppressHydrationWarning||Hd(t.textContent,n)?(r.popover!=null&&(H(`beforetoggle`,t),H(`toggle`,t)),r.onScroll!=null&&H(`scroll`,t),r.onScrollEnd!=null&&H(`scrollend`,t),r.onClick!=null&&(t.onclick=nn),t=!0):t=!1,t||Hi(e,!0)}function Ui(e){for(Ii=e.return;Ii;)switch(Ii.tag){case 5:case 31:case 13:Bi=!1;return;case 27:case 3:Bi=!0;return;default:Ii=Ii.return}}function Wi(e){if(e!==Ii)return!1;if(!Ri)return Ui(e),Ri=!0,!1;var t=e.tag,n;if((n=t!==3&&t!==27)&&((n=t===5)&&(n=e.type,n=!(n!==`form`&&n!==`button`)||ef(e.type,e.memoizedProps)),n=!n),n&&Li&&Hi(e),Ui(e),t===13){if(e=e.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(317));Li=bf(e)}else if(t===31){if(e=e.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(317));Li=bf(e)}else t===27?(t=Li,lf(e.type)?(e=yf,yf=null,Li=e):Li=t):Li=Ii?vf(e.stateNode.nextSibling):null;return!0}function Gi(){Li=Ii=null,Ri=!1}function Ki(){var e=zi;return e!==null&&(ou===null?ou=e:ou.push.apply(ou,e),zi=null),e}function qi(e){zi===null?zi=[e]:zi.push(e)}var Ji=ae(null),Yi=null,Xi=null;function Zi(e,t,n){se(Ji,t._currentValue),t._currentValue=n}function Qi(e){e._currentValue=Ji.current,oe(Ji)}function $i(e,t,n){for(;e!==null;){var r=e.alternate;if((e.childLanes&t)===t?r!==null&&(r.childLanes&t)!==t&&(r.childLanes|=t):(e.childLanes|=t,r!==null&&(r.childLanes|=t)),e===n)break;e=e.return}}function ea(e,t,n,r){var a=e.child;for(a!==null&&(a.return=e);a!==null;){var o=a.dependencies;if(o!==null){var s=a.child;o=o.firstContext;a:for(;o!==null;){var c=o;o=a;for(var l=0;l<t.length;l++)if(c.context===t[l]){o.lanes|=n,c=o.alternate,c!==null&&(c.lanes|=n),$i(o.return,n,e),r||(s=null);break a}o=c.next}}else if(a.tag===18){if(s=a.return,s===null)throw Error(i(341));s.lanes|=n,o=s.alternate,o!==null&&(o.lanes|=n),$i(s,n,e),s=null}else s=a.child;if(s!==null)s.return=a;else for(s=a;s!==null;){if(s===e){s=null;break}if(a=s.sibling,a!==null){a.return=s.return,s=a;break}s=s.return}a=s}}function ta(e,t,n,r){e=null;for(var a=t,o=!1;a!==null;){if(!o){if(a.flags&524288)o=!0;else if(a.flags&262144)break}if(a.tag===10){var s=a.alternate;if(s===null)throw Error(i(387));if(s=s.memoizedProps,s!==null){var c=a.type;Tr(a.pendingProps.value,s.value)||(e===null?e=[c]:e.push(c))}}else if(a===de.current){if(s=a.alternate,s===null)throw Error(i(387));s.memoizedState.memoizedState!==a.memoizedState.memoizedState&&(e===null?e=[cp]:e.push(cp))}a=a.return}e!==null&&ea(t,e,n,r),t.flags|=262144}function na(e){for(e=e.firstContext;e!==null;){if(!Tr(e.context._currentValue,e.memoizedValue))return!0;e=e.next}return!1}function ra(e){Yi=e,Xi=null,e=e.dependencies,e!==null&&(e.firstContext=null)}function ia(e){return oa(Yi,e)}function aa(e,t){return Yi===null&&ra(e),oa(e,t)}function oa(e,t){var n=t._currentValue;if(t={context:t,memoizedValue:n,next:null},Xi===null){if(e===null)throw Error(i(308));Xi=t,e.dependencies={lanes:0,firstContext:t},e.flags|=524288}else Xi=Xi.next=t;return n}var sa=typeof AbortController<`u`?AbortController:function(){var e=[],t=this.signal={aborted:!1,addEventListener:function(t,n){e.push(n)}};this.abort=function(){t.aborted=!0,e.forEach(function(e){return e()})}},ca=t.unstable_scheduleCallback,la=t.unstable_NormalPriority,ua={$$typeof:C,Consumer:null,Provider:null,_currentValue:null,_currentValue2:null,_threadCount:0};function da(){return{controller:new sa,data:new Map,refCount:0}}function fa(e){e.refCount--,e.refCount===0&&ca(la,function(){e.controller.abort()})}var pa=null,ma=0,ha=0,ga=null;function _a(e,t){if(pa===null){var n=pa=[];ma=0,ha=xd(),ga={status:`pending`,value:void 0,then:function(e){n.push(e)}}}return ma++,t.then(va,va),t}function va(){if(--ma===0&&pa!==null){ga!==null&&(ga.status=`fulfilled`);var e=pa;pa=null,ha=0,ga=null;for(var t=0;t<e.length;t++)(0,e[t])()}}function ya(e,t){var n=[],r={status:`pending`,value:null,reason:null,then:function(e){n.push(e)}};return e.then(function(){r.status=`fulfilled`,r.value=t;for(var e=0;e<n.length;e++)(0,n[e])(t)},function(e){for(r.status=`rejected`,r.reason=e,e=0;e<n.length;e++)(0,n[e])(void 0)}),r}var ba=P.S;P.S=function(e,t){lu=Oe(),typeof t==`object`&&t&&typeof t.then==`function`&&_a(e,t),ba!==null&&ba(e,t)};var xa=ae(null);function Sa(){var e=xa.current;return e===null?Wl.pooledCache:e}function Ca(e,t){t===null?se(xa,xa.current):se(xa,t.pool)}function wa(){var e=Sa();return e===null?null:{parent:ua._currentValue,pool:e}}var Ta=Error(i(460)),Ea=Error(i(474)),Da=Error(i(542)),Oa={then:function(){}};function ka(e){return e=e.status,e===`fulfilled`||e===`rejected`}function Aa(e,t,n){switch(n=e[n],n===void 0?e.push(t):n!==t&&(t.then(nn,nn),t=n),t.status){case`fulfilled`:return t.value;case`rejected`:throw e=t.reason,Pa(e),e;default:if(typeof t.status==`string`)t.then(nn,nn);else{if(e=Wl,e!==null&&100<e.shellSuspendCounter)throw Error(i(482));e=t,e.status=`pending`,e.then(function(e){if(t.status===`pending`){var n=t;n.status=`fulfilled`,n.value=e}},function(e){if(t.status===`pending`){var n=t;n.status=`rejected`,n.reason=e}})}switch(t.status){case`fulfilled`:return t.value;case`rejected`:throw e=t.reason,Pa(e),e}throw Ma=t,Ta}}function ja(e){try{var t=e._init;return t(e._payload)}catch(e){throw typeof e==`object`&&e&&typeof e.then==`function`?(Ma=e,Ta):e}}var Ma=null;function Na(){if(Ma===null)throw Error(i(459));var e=Ma;return Ma=null,e}function Pa(e){if(e===Ta||e===Da)throw Error(i(483))}var Fa=null,Ia=0;function La(e){var t=Ia;return Ia+=1,Fa===null&&(Fa=[]),Aa(Fa,e,t)}function Ra(e,t){t=t.props.ref,e.ref=t===void 0?null:t}function za(e,t){throw t.$$typeof===g?Error(i(525)):(e=Object.prototype.toString.call(t),Error(i(31,e===`[object Object]`?`object with keys {`+Object.keys(t).join(`, `)+`}`:e)))}function Ba(e){function t(t,n){if(e){var r=t.deletions;r===null?(t.deletions=[n],t.flags|=16):r.push(n)}}function n(n,r){if(!e)return null;for(;r!==null;)t(n,r),r=r.sibling;return null}function r(e){for(var t=new Map;e!==null;)e.key===null?t.set(e.index,e):t.set(e.key,e),e=e.sibling;return t}function a(e,t){return e=pi(e,t),e.index=0,e.sibling=null,e}function o(t,n,r){return t.index=r,e?(r=t.alternate,r===null?(t.flags|=67108866,n):(r=r.index,r<n?(t.flags|=67108866,n):r)):(t.flags|=1048576,n)}function s(t){return e&&t.alternate===null&&(t.flags|=67108866),t}function c(e,t,n,r){return t===null||t.tag!==6?(t=_i(n,e.mode,r),t.return=e,t):(t=a(t,n),t.return=e,t)}function l(e,t,n,r){var i=n.type;return i===y?d(e,t,n.props.children,r,n.key):t!==null&&(t.elementType===i||typeof i==`object`&&i&&i.$$typeof===O&&ja(i)===t.type)?(t=a(t,n.props),Ra(t,n),t.return=e,t):(t=hi(n.type,n.key,n.props,null,e.mode,r),Ra(t,n),t.return=e,t)}function u(e,t,n,r){return t===null||t.tag!==4||t.stateNode.containerInfo!==n.containerInfo||t.stateNode.implementation!==n.implementation?(t=yi(n,e.mode,r),t.return=e,t):(t=a(t,n.children||[]),t.return=e,t)}function d(e,t,n,r,i){return t===null||t.tag!==7?(t=gi(n,e.mode,r,i),t.return=e,t):(t=a(t,n),t.return=e,t)}function f(e,t,n){if(typeof t==`string`&&t!==``||typeof t==`number`||typeof t==`bigint`)return t=_i(``+t,e.mode,n),t.return=e,t;if(typeof t==`object`&&t){switch(t.$$typeof){case _:return n=hi(t.type,t.key,t.props,null,e.mode,n),Ra(n,t),n.return=e,n;case v:return t=yi(t,e.mode,n),t.return=e,t;case O:return t=ja(t),f(e,t,n)}if(N(t)||j(t))return t=gi(t,e.mode,n,null),t.return=e,t;if(typeof t.then==`function`)return f(e,La(t),n);if(t.$$typeof===C)return f(e,aa(e,t),n);za(e,t)}return null}function p(e,t,n,r){var i=t===null?null:t.key;if(typeof n==`string`&&n!==``||typeof n==`number`||typeof n==`bigint`)return i===null?c(e,t,``+n,r):null;if(typeof n==`object`&&n){switch(n.$$typeof){case _:return n.key===i?l(e,t,n,r):null;case v:return n.key===i?u(e,t,n,r):null;case O:return n=ja(n),p(e,t,n,r)}if(N(n)||j(n))return i===null?d(e,t,n,r,null):null;if(typeof n.then==`function`)return p(e,t,La(n),r);if(n.$$typeof===C)return p(e,t,aa(e,n),r);za(e,n)}return null}function m(e,t,n,r,i){if(typeof r==`string`&&r!==``||typeof r==`number`||typeof r==`bigint`)return e=e.get(n)||null,c(t,e,``+r,i);if(typeof r==`object`&&r){switch(r.$$typeof){case _:return e=e.get(r.key===null?n:r.key)||null,l(t,e,r,i);case v:return e=e.get(r.key===null?n:r.key)||null,u(t,e,r,i);case O:return r=ja(r),m(e,t,n,r,i)}if(N(r)||j(r))return e=e.get(n)||null,d(t,e,r,i,null);if(typeof r.then==`function`)return m(e,t,n,La(r),i);if(r.$$typeof===C)return m(e,t,n,aa(t,r),i);za(t,r)}return null}function h(i,a,s,c){for(var l=null,u=null,d=a,h=a=0,g=null;d!==null&&h<s.length;h++){d.index>h?(g=d,d=null):g=d.sibling;var _=p(i,d,s[h],c);if(_===null){d===null&&(d=g);break}e&&d&&_.alternate===null&&t(i,d),a=o(_,a,h),u===null?l=_:u.sibling=_,u=_,d=g}if(h===s.length)return n(i,d),Ri&&ji(i,h),l;if(d===null){for(;h<s.length;h++)d=f(i,s[h],c),d!==null&&(a=o(d,a,h),u===null?l=d:u.sibling=d,u=d);return Ri&&ji(i,h),l}for(d=r(d);h<s.length;h++)g=m(d,i,h,s[h],c),g!==null&&(e&&g.alternate!==null&&d.delete(g.key===null?h:g.key),a=o(g,a,h),u===null?l=g:u.sibling=g,u=g);return e&&d.forEach(function(e){return t(i,e)}),Ri&&ji(i,h),l}function g(a,s,c,l){if(c==null)throw Error(i(151));for(var u=null,d=null,h=s,g=s=0,_=null,v=c.next();h!==null&&!v.done;g++,v=c.next()){h.index>g?(_=h,h=null):_=h.sibling;var y=p(a,h,v.value,l);if(y===null){h===null&&(h=_);break}e&&h&&y.alternate===null&&t(a,h),s=o(y,s,g),d===null?u=y:d.sibling=y,d=y,h=_}if(v.done)return n(a,h),Ri&&ji(a,g),u;if(h===null){for(;!v.done;g++,v=c.next())v=f(a,v.value,l),v!==null&&(s=o(v,s,g),d===null?u=v:d.sibling=v,d=v);return Ri&&ji(a,g),u}for(h=r(h);!v.done;g++,v=c.next())v=m(h,a,g,v.value,l),v!==null&&(e&&v.alternate!==null&&h.delete(v.key===null?g:v.key),s=o(v,s,g),d===null?u=v:d.sibling=v,d=v);return e&&h.forEach(function(e){return t(a,e)}),Ri&&ji(a,g),u}function b(e,r,o,c){if(typeof o==`object`&&o&&o.type===y&&o.key===null&&(o=o.props.children),typeof o==`object`&&o){switch(o.$$typeof){case _:a:{for(var l=o.key;r!==null;){if(r.key===l){if(l=o.type,l===y){if(r.tag===7){n(e,r.sibling),c=a(r,o.props.children),c.return=e,e=c;break a}}else if(r.elementType===l||typeof l==`object`&&l&&l.$$typeof===O&&ja(l)===r.type){n(e,r.sibling),c=a(r,o.props),Ra(c,o),c.return=e,e=c;break a}n(e,r);break}else t(e,r);r=r.sibling}o.type===y?(c=gi(o.props.children,e.mode,c,o.key),c.return=e,e=c):(c=hi(o.type,o.key,o.props,null,e.mode,c),Ra(c,o),c.return=e,e=c)}return s(e);case v:a:{for(l=o.key;r!==null;){if(r.key===l)if(r.tag===4&&r.stateNode.containerInfo===o.containerInfo&&r.stateNode.implementation===o.implementation){n(e,r.sibling),c=a(r,o.children||[]),c.return=e,e=c;break a}else{n(e,r);break}else t(e,r);r=r.sibling}c=yi(o,e.mode,c),c.return=e,e=c}return s(e);case O:return o=ja(o),b(e,r,o,c)}if(N(o))return h(e,r,o,c);if(j(o)){if(l=j(o),typeof l!=`function`)throw Error(i(150));return o=l.call(o),g(e,r,o,c)}if(typeof o.then==`function`)return b(e,r,La(o),c);if(o.$$typeof===C)return b(e,r,aa(e,o),c);za(e,o)}return typeof o==`string`&&o!==``||typeof o==`number`||typeof o==`bigint`?(o=``+o,r!==null&&r.tag===6?(n(e,r.sibling),c=a(r,o),c.return=e,e=c):(n(e,r),c=_i(o,e.mode,c),c.return=e,e=c),s(e)):n(e,r)}return function(e,t,n,r){try{Ia=0;var i=b(e,t,n,r);return Fa=null,i}catch(t){if(t===Ta||t===Da)throw t;var a=di(29,t,null,e.mode);return a.lanes=r,a.return=e,a}}}var Va=Ba(!0),Ha=Ba(!1),Ua=!1;function Wa(e){e.updateQueue={baseState:e.memoizedState,firstBaseUpdate:null,lastBaseUpdate:null,shared:{pending:null,lanes:0,hiddenCallbacks:null},callbacks:null}}function Ga(e,t){e=e.updateQueue,t.updateQueue===e&&(t.updateQueue={baseState:e.baseState,firstBaseUpdate:e.firstBaseUpdate,lastBaseUpdate:e.lastBaseUpdate,shared:e.shared,callbacks:null})}function Ka(e){return{lane:e,tag:0,payload:null,callback:null,next:null}}function qa(e,t,n){var r=e.updateQueue;if(r===null)return null;if(r=r.shared,Ul&2){var i=r.pending;return i===null?t.next=t:(t.next=i.next,i.next=t),r.pending=t,t=ci(e),si(e,null,n),t}return ii(e,r,t,n),ci(e)}function Ja(e,t,n){if(t=t.updateQueue,t!==null&&(t=t.shared,n&4194048)){var r=t.lanes;r&=e.pendingLanes,n|=r,t.lanes=n,nt(e,n)}}function Ya(e,t){var n=e.updateQueue,r=e.alternate;if(r!==null&&(r=r.updateQueue,n===r)){var i=null,a=null;if(n=n.firstBaseUpdate,n!==null){do{var o={lane:n.lane,tag:n.tag,payload:n.payload,callback:null,next:null};a===null?i=a=o:a=a.next=o,n=n.next}while(n!==null);a===null?i=a=t:a=a.next=t}else i=a=t;n={baseState:r.baseState,firstBaseUpdate:i,lastBaseUpdate:a,shared:r.shared,callbacks:r.callbacks},e.updateQueue=n;return}e=n.lastBaseUpdate,e===null?n.firstBaseUpdate=t:e.next=t,n.lastBaseUpdate=t}var Xa=!1;function Za(){if(Xa){var e=ga;if(e!==null)throw e}}function Qa(e,t,n,r){Xa=!1;var i=e.updateQueue;Ua=!1;var a=i.firstBaseUpdate,o=i.lastBaseUpdate,s=i.shared.pending;if(s!==null){i.shared.pending=null;var c=s,l=c.next;c.next=null,o===null?a=l:o.next=l,o=c;var u=e.alternate;u!==null&&(u=u.updateQueue,s=u.lastBaseUpdate,s!==o&&(s===null?u.firstBaseUpdate=l:s.next=l,u.lastBaseUpdate=c))}if(a!==null){var d=i.baseState;o=0,u=l=c=null,s=a;do{var f=s.lane&-536870913,p=f!==s.lane;if(p?(Kl&f)===f:(r&f)===f){f!==0&&f===ha&&(Xa=!0),u!==null&&(u=u.next={lane:0,tag:s.tag,payload:s.payload,callback:null,next:null});a:{var m=e,g=s;f=t;var _=n;switch(g.tag){case 1:if(m=g.payload,typeof m==`function`){d=m.call(_,d,f);break a}d=m;break a;case 3:m.flags=m.flags&-65537|128;case 0:if(m=g.payload,f=typeof m==`function`?m.call(_,d,f):m,f==null)break a;d=h({},d,f);break a;case 2:Ua=!0}}f=s.callback,f!==null&&(e.flags|=64,p&&(e.flags|=8192),p=i.callbacks,p===null?i.callbacks=[f]:p.push(f))}else p={lane:f,tag:s.tag,payload:s.payload,callback:s.callback,next:null},u===null?(l=u=p,c=d):u=u.next=p,o|=f;if(s=s.next,s===null){if(s=i.shared.pending,s===null)break;p=s,s=p.next,p.next=null,i.lastBaseUpdate=p,i.shared.pending=null}}while(1);u===null&&(c=d),i.baseState=c,i.firstBaseUpdate=l,i.lastBaseUpdate=u,a===null&&(i.shared.lanes=0),eu|=o,e.lanes=o,e.memoizedState=d}}function $a(e,t){if(typeof e!=`function`)throw Error(i(191,e));e.call(t)}function eo(e,t){var n=e.callbacks;if(n!==null)for(e.callbacks=null,e=0;e<n.length;e++)$a(n[e],t)}var to=ae(null),no=ae(0);function ro(e,t){e=Ql,se(no,e),se(to,t),Ql=e|t.baseLanes}function io(){se(no,Ql),se(to,to.current)}function ao(){Ql=no.current,oe(to),oe(no)}var oo=ae(null),so=null;function co(e){var t=e.alternate;se(mo,mo.current&1),se(oo,e),so===null&&(t===null||to.current!==null||t.memoizedState!==null)&&(so=e)}function lo(e){se(mo,mo.current),se(oo,e),so===null&&(so=e)}function uo(e){e.tag===22?(se(mo,mo.current),se(oo,e),so===null&&(so=e)):fo(e)}function fo(){se(mo,mo.current),se(oo,oo.current)}function po(e){oe(oo),so===e&&(so=null),oe(mo)}var mo=ae(0);function ho(e){for(var t=e;t!==null;){if(t.tag===13){var n=t.memoizedState;if(n!==null&&(n=n.dehydrated,n===null||hf(n)||gf(n)))return t}else if(t.tag===19&&(t.memoizedProps.revealOrder===`forwards`||t.memoizedProps.revealOrder===`backwards`||t.memoizedProps.revealOrder===`unstable_legacy-backwards`||t.memoizedProps.revealOrder===`together`)){if(t.flags&128)return t}else if(t.child!==null){t.child.return=t,t=t.child;continue}if(t===e)break;for(;t.sibling===null;){if(t.return===null||t.return===e)return null;t=t.return}t.sibling.return=t.return,t=t.sibling}return null}var go=0,L=null,_o=null,vo=null,yo=!1,bo=!1,xo=!1,So=0,Co=0,wo=null,To=0;function Eo(){throw Error(i(321))}function Do(e,t){if(t===null)return!1;for(var n=0;n<t.length&&n<e.length;n++)if(!Tr(e[n],t[n]))return!1;return!0}function Oo(e,t,n,r,i,a){return go=a,L=t,t.memoizedState=null,t.updateQueue=null,t.lanes=0,P.H=e===null||e.memoizedState===null?Ws:Gs,xo=!1,a=n(r,i),xo=!1,bo&&(a=Ao(t,n,r,i)),ko(e),a}function ko(e){P.H=Us;var t=_o!==null&&_o.next!==null;if(go=0,vo=_o=L=null,yo=!1,Co=0,wo=null,t)throw Error(i(300));e===null||cc||(e=e.dependencies,e!==null&&na(e)&&(cc=!0))}function Ao(e,t,n,r){L=e;var a=0;do{if(bo&&(wo=null),Co=0,bo=!1,25<=a)throw Error(i(301));if(a+=1,vo=_o=null,e.updateQueue!=null){var o=e.updateQueue;o.lastEffect=null,o.events=null,o.stores=null,o.memoCache!=null&&(o.memoCache.index=0)}P.H=Ks,o=t(n,r)}while(bo);return o}function jo(){var e=P.H,t=e.useState()[0];return t=typeof t.then==`function`?Lo(t):t,e=e.useState()[0],(_o===null?null:_o.memoizedState)!==e&&(L.flags|=1024),t}function Mo(){var e=So!==0;return So=0,e}function No(e,t,n){t.updateQueue=e.updateQueue,t.flags&=-2053,e.lanes&=~n}function Po(e){if(yo){for(e=e.memoizedState;e!==null;){var t=e.queue;t!==null&&(t.pending=null),e=e.next}yo=!1}go=0,vo=_o=L=null,bo=!1,Co=So=0,wo=null}function Fo(){var e={memoizedState:null,baseState:null,baseQueue:null,queue:null,next:null};return vo===null?L.memoizedState=vo=e:vo=vo.next=e,vo}function Io(){if(_o===null){var e=L.alternate;e=e===null?null:e.memoizedState}else e=_o.next;var t=vo===null?L.memoizedState:vo.next;if(t!==null)vo=t,_o=e;else{if(e===null)throw L.alternate===null?Error(i(467)):Error(i(310));_o=e,e={memoizedState:_o.memoizedState,baseState:_o.baseState,baseQueue:_o.baseQueue,queue:_o.queue,next:null},vo===null?L.memoizedState=vo=e:vo=vo.next=e}return vo}function R(){return{lastEffect:null,events:null,stores:null,memoCache:null}}function Lo(e){var t=Co;return Co+=1,wo===null&&(wo=[]),e=Aa(wo,e,t),t=L,(vo===null?t.memoizedState:vo.next)===null&&(t=t.alternate,P.H=t===null||t.memoizedState===null?Ws:Gs),e}function Ro(e){if(typeof e==`object`&&e){if(typeof e.then==`function`)return Lo(e);if(e.$$typeof===C)return ia(e)}throw Error(i(438,String(e)))}function zo(e){var t=null,n=L.updateQueue;if(n!==null&&(t=n.memoCache),t==null){var r=L.alternate;r!==null&&(r=r.updateQueue,r!==null&&(r=r.memoCache,r!=null&&(t={data:r.data.map(function(e){return e.slice()}),index:0})))}if(t??={data:[],index:0},n===null&&(n=R(),L.updateQueue=n),n.memoCache=t,n=t.data[t.index],n===void 0)for(n=t.data[t.index]=Array(e),r=0;r<e;r++)n[r]=ee;return t.index++,n}function Bo(e,t){return typeof t==`function`?t(e):t}function Vo(e){return Ho(Io(),_o,e)}function Ho(e,t,n){var r=e.queue;if(r===null)throw Error(i(311));r.lastRenderedReducer=n;var a=e.baseQueue,o=r.pending;if(o!==null){if(a!==null){var s=a.next;a.next=o.next,o.next=s}t.baseQueue=a=o,r.pending=null}if(o=e.baseState,a===null)e.memoizedState=o;else{t=a.next;var c=s=null,l=null,u=t,d=!1;do{var f=u.lane&-536870913;if(f===u.lane?(go&f)===f:(Kl&f)===f){var p=u.revertLane;if(p===0)l!==null&&(l=l.next={lane:0,revertLane:0,gesture:null,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null}),f===ha&&(d=!0);else if((go&p)===p){u=u.next,p===ha&&(d=!0);continue}else f={lane:0,revertLane:u.revertLane,gesture:null,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null},l===null?(c=l=f,s=o):l=l.next=f,L.lanes|=p,eu|=p;f=u.action,xo&&n(o,f),o=u.hasEagerState?u.eagerState:n(o,f)}else p={lane:f,revertLane:u.revertLane,gesture:u.gesture,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null},l===null?(c=l=p,s=o):l=l.next=p,L.lanes|=f,eu|=f;u=u.next}while(u!==null&&u!==t);if(l===null?s=o:l.next=c,!Tr(o,e.memoizedState)&&(cc=!0,d&&(n=ga,n!==null)))throw n;e.memoizedState=o,e.baseState=s,e.baseQueue=l,r.lastRenderedState=o}return a===null&&(r.lanes=0),[e.memoizedState,r.dispatch]}function Uo(e){var t=Io(),n=t.queue;if(n===null)throw Error(i(311));n.lastRenderedReducer=e;var r=n.dispatch,a=n.pending,o=t.memoizedState;if(a!==null){n.pending=null;var s=a=a.next;do o=e(o,s.action),s=s.next;while(s!==a);Tr(o,t.memoizedState)||(cc=!0),t.memoizedState=o,t.baseQueue===null&&(t.baseState=o),n.lastRenderedState=o}return[o,r]}function Wo(e,t,n){var r=L,a=Io(),o=Ri;if(o){if(n===void 0)throw Error(i(407));n=n()}else n=t();var s=!Tr((_o||a).memoizedState,n);if(s&&(a.memoizedState=n,cc=!0),a=a.queue,hs(qo.bind(null,r,a,e),[e]),a.getSnapshot!==t||s||vo!==null&&vo.memoizedState.tag&1){if(r.flags|=2048,us(9,{destroy:void 0},Ko.bind(null,r,a,n,t),null),Wl===null)throw Error(i(349));o||go&127||Go(r,t,n)}return n}function Go(e,t,n){e.flags|=16384,e={getSnapshot:t,value:n},t=L.updateQueue,t===null?(t=R(),L.updateQueue=t,t.stores=[e]):(n=t.stores,n===null?t.stores=[e]:n.push(e))}function Ko(e,t,n,r){t.value=n,t.getSnapshot=r,Jo(t)&&Yo(e)}function qo(e,t,n){return n(function(){Jo(t)&&Yo(e)})}function Jo(e){var t=e.getSnapshot;e=e.value;try{var n=t();return!Tr(e,n)}catch{return!0}}function Yo(e){var t=oi(e,2);t!==null&&wu(t,e,2)}function Xo(e){var t=Fo();if(typeof e==`function`){var n=e;if(e=n(),xo){ze(!0);try{n()}finally{ze(!1)}}}return t.memoizedState=t.baseState=e,t.queue={pending:null,lanes:0,dispatch:null,lastRenderedReducer:Bo,lastRenderedState:e},t}function Zo(e,t,n,r){return e.baseState=n,Ho(e,_o,typeof r==`function`?r:Bo)}function Qo(e,t,n,r,a){if(Bs(e))throw Error(i(485));if(e=t.action,e!==null){var o={payload:a,action:e,next:null,isTransition:!0,status:`pending`,value:null,reason:null,listeners:[],then:function(e){o.listeners.push(e)}};P.T===null?o.isTransition=!1:n(!0),r(o),n=t.pending,n===null?(o.next=t.pending=o,$o(t,o)):(o.next=n.next,t.pending=n.next=o)}}function $o(e,t){var n=t.action,r=t.payload,i=e.state;if(t.isTransition){var a=P.T,o={};P.T=o;try{var s=n(i,r),c=P.S;c!==null&&c(o,s),es(e,t,s)}catch(n){ns(e,t,n)}finally{a!==null&&o.types!==null&&(a.types=o.types),P.T=a}}else try{a=n(i,r),es(e,t,a)}catch(n){ns(e,t,n)}}function es(e,t,n){typeof n==`object`&&n&&typeof n.then==`function`?n.then(function(n){ts(e,t,n)},function(n){return ns(e,t,n)}):ts(e,t,n)}function ts(e,t,n){t.status=`fulfilled`,t.value=n,rs(t),e.state=n,t=e.pending,t!==null&&(n=t.next,n===t?e.pending=null:(n=n.next,t.next=n,$o(e,n)))}function ns(e,t,n){var r=e.pending;if(e.pending=null,r!==null){r=r.next;do t.status=`rejected`,t.reason=n,rs(t),t=t.next;while(t!==r)}e.action=null}function rs(e){e=e.listeners;for(var t=0;t<e.length;t++)(0,e[t])()}function is(e,t){return t}function as(e,t){if(Ri){var n=Wl.formState;if(n!==null){a:{var r=L;if(Ri){if(Li){b:{for(var i=Li,a=Bi;i.nodeType!==8;){if(!a){i=null;break b}if(i=vf(i.nextSibling),i===null){i=null;break b}}a=i.data,i=a===`F!`||a===`F`?i:null}if(i){Li=vf(i.nextSibling),r=i.data===`F!`;break a}}Hi(r)}r=!1}r&&(t=n[0])}}return n=Fo(),n.memoizedState=n.baseState=t,r={pending:null,lanes:0,dispatch:null,lastRenderedReducer:is,lastRenderedState:t},n.queue=r,n=Ls.bind(null,L,r),r.dispatch=n,r=Xo(!1),a=zs.bind(null,L,!1,r.queue),r=Fo(),i={state:t,dispatch:null,action:e,pending:null},r.queue=i,n=Qo.bind(null,L,i,a,n),i.dispatch=n,r.memoizedState=e,[t,n,!1]}function os(e){return ss(Io(),_o,e)}function ss(e,t,n){if(t=Ho(e,t,is)[0],e=Vo(Bo)[0],typeof t==`object`&&t&&typeof t.then==`function`)try{var r=Lo(t)}catch(e){throw e===Ta?Da:e}else r=t;t=Io();var i=t.queue,a=i.dispatch;return n!==t.memoizedState&&(L.flags|=2048,us(9,{destroy:void 0},cs.bind(null,i,n),null)),[r,a,e]}function cs(e,t){e.action=t}function ls(e){var t=Io(),n=_o;if(n!==null)return ss(t,n,e);Io(),t=t.memoizedState,n=Io();var r=n.queue.dispatch;return n.memoizedState=e,[t,r,!1]}function us(e,t,n,r){return e={tag:e,create:n,deps:r,inst:t,next:null},t=L.updateQueue,t===null&&(t=R(),L.updateQueue=t),n=t.lastEffect,n===null?t.lastEffect=e.next=e:(r=n.next,n.next=e,e.next=r,t.lastEffect=e),e}function ds(){return Io().memoizedState}function fs(e,t,n,r){var i=Fo();L.flags|=e,i.memoizedState=us(1|t,{destroy:void 0},n,r===void 0?null:r)}function ps(e,t,n,r){var i=Io();r=r===void 0?null:r;var a=i.memoizedState.inst;_o!==null&&r!==null&&Do(r,_o.memoizedState.deps)?i.memoizedState=us(t,a,n,r):(L.flags|=e,i.memoizedState=us(1|t,a,n,r))}function ms(e,t){fs(8390656,8,e,t)}function hs(e,t){ps(2048,8,e,t)}function gs(e){L.flags|=4;var t=L.updateQueue;if(t===null)t=R(),L.updateQueue=t,t.events=[e];else{var n=t.events;n===null?t.events=[e]:n.push(e)}}function _s(e){var t=Io().memoizedState;return gs({ref:t,nextImpl:e}),function(){if(Ul&2)throw Error(i(440));return t.impl.apply(void 0,arguments)}}function vs(e,t){return ps(4,2,e,t)}function ys(e,t){return ps(4,4,e,t)}function bs(e,t){if(typeof t==`function`){e=e();var n=t(e);return function(){typeof n==`function`?n():t(null)}}if(t!=null)return e=e(),t.current=e,function(){t.current=null}}function xs(e,t,n){n=n==null?null:n.concat([e]),ps(4,4,bs.bind(null,t,e),n)}function Ss(){}function Cs(e,t){var n=Io();t=t===void 0?null:t;var r=n.memoizedState;return t!==null&&Do(t,r[1])?r[0]:(n.memoizedState=[e,t],e)}function ws(e,t){var n=Io();t=t===void 0?null:t;var r=n.memoizedState;if(t!==null&&Do(t,r[1]))return r[0];if(r=e(),xo){ze(!0);try{e()}finally{ze(!1)}}return n.memoizedState=[r,t],r}function Ts(e,t,n){return n===void 0||go&1073741824&&!(Kl&261930)?e.memoizedState=t:(e.memoizedState=n,e=Cu(),L.lanes|=e,eu|=e,n)}function Es(e,t,n,r){return Tr(n,t)?n:to.current===null?!(go&42)||go&1073741824&&!(Kl&261930)?(cc=!0,e.memoizedState=n):(e=Cu(),L.lanes|=e,eu|=e,t):(e=Ts(e,n,r),Tr(e,t)||(cc=!0),e)}function Ds(e,t,n,r,i){var a=F.p;F.p=a!==0&&8>a?a:8;var o=P.T,s={};P.T=s,zs(e,!1,t,n);try{var c=i(),l=P.S;l!==null&&l(s,c),typeof c==`object`&&c&&typeof c.then==`function`?Rs(e,t,ya(c,r),Su(e)):Rs(e,t,r,Su(e))}catch(n){Rs(e,t,{then:function(){},status:`rejected`,reason:n},Su())}finally{F.p=a,o!==null&&s.types!==null&&(o.types=s.types),P.T=o}}function Os(){}function ks(e,t,n,r){if(e.tag!==5)throw Error(i(476));var a=As(e).queue;Ds(e,a,t,ne,n===null?Os:function(){return js(e),n(r)})}function As(e){var t=e.memoizedState;if(t!==null)return t;t={memoizedState:ne,baseState:ne,baseQueue:null,queue:{pending:null,lanes:0,dispatch:null,lastRenderedReducer:Bo,lastRenderedState:ne},next:null};var n={};return t.next={memoizedState:n,baseState:n,baseQueue:null,queue:{pending:null,lanes:0,dispatch:null,lastRenderedReducer:Bo,lastRenderedState:n},next:null},e.memoizedState=t,e=e.alternate,e!==null&&(e.memoizedState=t),t}function js(e){var t=As(e);t.next===null&&(t=e.alternate.memoizedState),Rs(e,t.next.queue,{},Su())}function Ms(){return ia(cp)}function Ns(){return Io().memoizedState}function Ps(){return Io().memoizedState}function Fs(e){for(var t=e.return;t!==null;){switch(t.tag){case 24:case 3:var n=Su();e=Ka(n);var r=qa(t,e,n);r!==null&&(wu(r,t,n),Ja(r,t,n)),t={cache:da()},e.payload=t;return}t=t.return}}function Is(e,t,n){var r=Su();n={lane:r,revertLane:0,gesture:null,action:n,hasEagerState:!1,eagerState:null,next:null},Bs(e)?Vs(t,n):(n=ai(e,t,n,r),n!==null&&(wu(n,e,r),Hs(n,t,r)))}function Ls(e,t,n){Rs(e,t,n,Su())}function Rs(e,t,n,r){var i={lane:r,revertLane:0,gesture:null,action:n,hasEagerState:!1,eagerState:null,next:null};if(Bs(e))Vs(t,i);else{var a=e.alternate;if(e.lanes===0&&(a===null||a.lanes===0)&&(a=t.lastRenderedReducer,a!==null))try{var o=t.lastRenderedState,s=a(o,n);if(i.hasEagerState=!0,i.eagerState=s,Tr(s,o))return ii(e,t,i,0),Wl===null&&ri(),!1}catch{}if(n=ai(e,t,i,r),n!==null)return wu(n,e,r),Hs(n,t,r),!0}return!1}function zs(e,t,n,r){if(r={lane:2,revertLane:xd(),gesture:null,action:r,hasEagerState:!1,eagerState:null,next:null},Bs(e)){if(t)throw Error(i(479))}else t=ai(e,n,r,2),t!==null&&wu(t,e,2)}function Bs(e){var t=e.alternate;return e===L||t!==null&&t===L}function Vs(e,t){bo=yo=!0;var n=e.pending;n===null?t.next=t:(t.next=n.next,n.next=t),e.pending=t}function Hs(e,t,n){if(n&4194048){var r=t.lanes;r&=e.pendingLanes,n|=r,t.lanes=n,nt(e,n)}}var Us={readContext:ia,use:Ro,useCallback:Eo,useContext:Eo,useEffect:Eo,useImperativeHandle:Eo,useLayoutEffect:Eo,useInsertionEffect:Eo,useMemo:Eo,useReducer:Eo,useRef:Eo,useState:Eo,useDebugValue:Eo,useDeferredValue:Eo,useTransition:Eo,useSyncExternalStore:Eo,useId:Eo,useHostTransitionStatus:Eo,useFormState:Eo,useActionState:Eo,useOptimistic:Eo,useMemoCache:Eo,useCacheRefresh:Eo};Us.useEffectEvent=Eo;var Ws={readContext:ia,use:Ro,useCallback:function(e,t){return Fo().memoizedState=[e,t===void 0?null:t],e},useContext:ia,useEffect:ms,useImperativeHandle:function(e,t,n){n=n==null?null:n.concat([e]),fs(4194308,4,bs.bind(null,t,e),n)},useLayoutEffect:function(e,t){return fs(4194308,4,e,t)},useInsertionEffect:function(e,t){fs(4,2,e,t)},useMemo:function(e,t){var n=Fo();t=t===void 0?null:t;var r=e();if(xo){ze(!0);try{e()}finally{ze(!1)}}return n.memoizedState=[r,t],r},useReducer:function(e,t,n){var r=Fo();if(n!==void 0){var i=n(t);if(xo){ze(!0);try{n(t)}finally{ze(!1)}}}else i=t;return r.memoizedState=r.baseState=i,e={pending:null,lanes:0,dispatch:null,lastRenderedReducer:e,lastRenderedState:i},r.queue=e,e=e.dispatch=Is.bind(null,L,e),[r.memoizedState,e]},useRef:function(e){var t=Fo();return e={current:e},t.memoizedState=e},useState:function(e){e=Xo(e);var t=e.queue,n=Ls.bind(null,L,t);return t.dispatch=n,[e.memoizedState,n]},useDebugValue:Ss,useDeferredValue:function(e,t){return Ts(Fo(),e,t)},useTransition:function(){var e=Xo(!1);return e=Ds.bind(null,L,e.queue,!0,!1),Fo().memoizedState=e,[!1,e]},useSyncExternalStore:function(e,t,n){var r=L,a=Fo();if(Ri){if(n===void 0)throw Error(i(407));n=n()}else{if(n=t(),Wl===null)throw Error(i(349));Kl&127||Go(r,t,n)}a.memoizedState=n;var o={value:n,getSnapshot:t};return a.queue=o,ms(qo.bind(null,r,o,e),[e]),r.flags|=2048,us(9,{destroy:void 0},Ko.bind(null,r,o,n,t),null),n},useId:function(){var e=Fo(),t=Wl.identifierPrefix;if(Ri){var n=Ai,r=ki;n=(r&~(1<<32-Be(r)-1)).toString(32)+n,t=`_`+t+`R_`+n,n=So++,0<n&&(t+=`H`+n.toString(32)),t+=`_`}else n=To++,t=`_`+t+`r_`+n.toString(32)+`_`;return e.memoizedState=t},useHostTransitionStatus:Ms,useFormState:as,useActionState:as,useOptimistic:function(e){var t=Fo();t.memoizedState=t.baseState=e;var n={pending:null,lanes:0,dispatch:null,lastRenderedReducer:null,lastRenderedState:null};return t.queue=n,t=zs.bind(null,L,!0,n),n.dispatch=t,[e,t]},useMemoCache:zo,useCacheRefresh:function(){return Fo().memoizedState=Fs.bind(null,L)},useEffectEvent:function(e){var t=Fo(),n={impl:e};return t.memoizedState=n,function(){if(Ul&2)throw Error(i(440));return n.impl.apply(void 0,arguments)}}},Gs={readContext:ia,use:Ro,useCallback:Cs,useContext:ia,useEffect:hs,useImperativeHandle:xs,useInsertionEffect:vs,useLayoutEffect:ys,useMemo:ws,useReducer:Vo,useRef:ds,useState:function(){return Vo(Bo)},useDebugValue:Ss,useDeferredValue:function(e,t){return Es(Io(),_o.memoizedState,e,t)},useTransition:function(){var e=Vo(Bo)[0],t=Io().memoizedState;return[typeof e==`boolean`?e:Lo(e),t]},useSyncExternalStore:Wo,useId:Ns,useHostTransitionStatus:Ms,useFormState:os,useActionState:os,useOptimistic:function(e,t){return Zo(Io(),_o,e,t)},useMemoCache:zo,useCacheRefresh:Ps};Gs.useEffectEvent=_s;var Ks={readContext:ia,use:Ro,useCallback:Cs,useContext:ia,useEffect:hs,useImperativeHandle:xs,useInsertionEffect:vs,useLayoutEffect:ys,useMemo:ws,useReducer:Uo,useRef:ds,useState:function(){return Uo(Bo)},useDebugValue:Ss,useDeferredValue:function(e,t){var n=Io();return _o===null?Ts(n,e,t):Es(n,_o.memoizedState,e,t)},useTransition:function(){var e=Uo(Bo)[0],t=Io().memoizedState;return[typeof e==`boolean`?e:Lo(e),t]},useSyncExternalStore:Wo,useId:Ns,useHostTransitionStatus:Ms,useFormState:ls,useActionState:ls,useOptimistic:function(e,t){var n=Io();return _o===null?(n.baseState=e,[e,n.queue.dispatch]):Zo(n,_o,e,t)},useMemoCache:zo,useCacheRefresh:Ps};Ks.useEffectEvent=_s;function qs(e,t,n,r){t=e.memoizedState,n=n(r,t),n=n==null?t:h({},t,n),e.memoizedState=n,e.lanes===0&&(e.updateQueue.baseState=n)}var Js={enqueueSetState:function(e,t,n){e=e._reactInternals;var r=Su(),i=Ka(r);i.payload=t,n!=null&&(i.callback=n),t=qa(e,i,r),t!==null&&(wu(t,e,r),Ja(t,e,r))},enqueueReplaceState:function(e,t,n){e=e._reactInternals;var r=Su(),i=Ka(r);i.tag=1,i.payload=t,n!=null&&(i.callback=n),t=qa(e,i,r),t!==null&&(wu(t,e,r),Ja(t,e,r))},enqueueForceUpdate:function(e,t){e=e._reactInternals;var n=Su(),r=Ka(n);r.tag=2,t!=null&&(r.callback=t),t=qa(e,r,n),t!==null&&(wu(t,e,n),Ja(t,e,n))}};function Ys(e,t,n,r,i,a,o){return e=e.stateNode,typeof e.shouldComponentUpdate==`function`?e.shouldComponentUpdate(r,a,o):t.prototype&&t.prototype.isPureReactComponent?!Er(n,r)||!Er(i,a):!0}function Xs(e,t,n,r){e=t.state,typeof t.componentWillReceiveProps==`function`&&t.componentWillReceiveProps(n,r),typeof t.UNSAFE_componentWillReceiveProps==`function`&&t.UNSAFE_componentWillReceiveProps(n,r),t.state!==e&&Js.enqueueReplaceState(t,t.state,null)}function Zs(e,t){var n=t;if(`ref`in t)for(var r in n={},t)r!==`ref`&&(n[r]=t[r]);if(e=e.defaultProps)for(var i in n===t&&(n=h({},n)),e)n[i]===void 0&&(n[i]=e[i]);return n}function Qs(e){$r(e)}function $s(e){console.error(e)}function ec(e){$r(e)}function tc(e,t){try{var n=e.onUncaughtError;n(t.value,{componentStack:t.stack})}catch(e){setTimeout(function(){throw e})}}function nc(e,t,n){try{var r=e.onCaughtError;r(n.value,{componentStack:n.stack,errorBoundary:t.tag===1?t.stateNode:null})}catch(e){setTimeout(function(){throw e})}}function rc(e,t,n){return n=Ka(n),n.tag=3,n.payload={element:null},n.callback=function(){tc(e,t)},n}function ic(e){return e=Ka(e),e.tag=3,e}function ac(e,t,n,r){var i=n.type.getDerivedStateFromError;if(typeof i==`function`){var a=r.value;e.payload=function(){return i(a)},e.callback=function(){nc(t,n,r)}}var o=n.stateNode;o!==null&&typeof o.componentDidCatch==`function`&&(e.callback=function(){nc(t,n,r),typeof i!=`function`&&(fu===null?fu=new Set([this]):fu.add(this));var e=r.stack;this.componentDidCatch(r.value,{componentStack:e===null?``:e})})}function oc(e,t,n,r,a){if(n.flags|=32768,typeof r==`object`&&r&&typeof r.then==`function`){if(t=n.alternate,t!==null&&ta(t,n,a,!0),n=oo.current,n!==null){switch(n.tag){case 31:case 13:return so===null?Iu():n.alternate===null&&$l===0&&($l=3),n.flags&=-257,n.flags|=65536,n.lanes=a,r===Oa?n.flags|=16384:(t=n.updateQueue,t===null?n.updateQueue=new Set([r]):t.add(r),td(e,r,a)),!1;case 22:return n.flags|=65536,r===Oa?n.flags|=16384:(t=n.updateQueue,t===null?(t={transitions:null,markerInstances:null,retryQueue:new Set([r])},n.updateQueue=t):(n=t.retryQueue,n===null?t.retryQueue=new Set([r]):n.add(r)),td(e,r,a)),!1}throw Error(i(435,n.tag))}return td(e,r,a),Iu(),!1}if(Ri)return t=oo.current,t===null?(r!==Vi&&(t=Error(i(423),{cause:r}),qi(xi(t,n))),e=e.current.alternate,e.flags|=65536,a&=-a,e.lanes|=a,r=xi(r,n),a=rc(e.stateNode,r,a),Ya(e,a),$l!==4&&($l=2)):(!(t.flags&65536)&&(t.flags|=256),t.flags|=65536,t.lanes=a,r!==Vi&&(e=Error(i(422),{cause:r}),qi(xi(e,n)))),!1;var o=Error(i(520),{cause:r});if(o=xi(o,n),au===null?au=[o]:au.push(o),$l!==4&&($l=2),t===null)return!0;r=xi(r,n),n=t;do{switch(n.tag){case 3:return n.flags|=65536,e=a&-a,n.lanes|=e,e=rc(n.stateNode,r,e),Ya(n,e),!1;case 1:if(t=n.type,o=n.stateNode,!(n.flags&128)&&(typeof t.getDerivedStateFromError==`function`||o!==null&&typeof o.componentDidCatch==`function`&&(fu===null||!fu.has(o))))return n.flags|=65536,a&=-a,n.lanes|=a,a=ic(a),ac(a,e,n,r),Ya(n,a),!1}n=n.return}while(n!==null);return!1}var sc=Error(i(461)),cc=!1;function lc(e,t,n,r){t.child=e===null?Ha(t,null,n,r):Va(t,e.child,n,r)}function uc(e,t,n,r,i){n=n.render;var a=t.ref;if(`ref`in r){var o={};for(var s in r)s!==`ref`&&(o[s]=r[s])}else o=r;return ra(t),r=Oo(e,t,n,o,a,i),s=Mo(),e!==null&&!cc?(No(e,t,i),Pc(e,t,i)):(Ri&&s&&Ni(t),t.flags|=1,lc(e,t,r,i),t.child)}function dc(e,t,n,r,i){if(e===null){var a=n.type;return typeof a==`function`&&!fi(a)&&a.defaultProps===void 0&&n.compare===null?(t.tag=15,t.type=a,fc(e,t,a,r,i)):(e=hi(n.type,null,r,t,t.mode,i),e.ref=t.ref,e.return=t,t.child=e)}if(a=e.child,!Fc(e,i)){var o=a.memoizedProps;if(n=n.compare,n=n===null?Er:n,n(o,r)&&e.ref===t.ref)return Pc(e,t,i)}return t.flags|=1,e=pi(a,r),e.ref=t.ref,e.return=t,t.child=e}function fc(e,t,n,r,i){if(e!==null){var a=e.memoizedProps;if(Er(a,r)&&e.ref===t.ref)if(cc=!1,t.pendingProps=r=a,Fc(e,i))e.flags&131072&&(cc=!0);else return t.lanes=e.lanes,Pc(e,t,i)}return bc(e,t,n,r,i)}function pc(e,t,n,r){var i=r.children,a=e===null?null:e.memoizedState;if(e===null&&t.stateNode===null&&(t.stateNode={_visibility:1,_pendingMarkers:null,_retryCache:null,_transitions:null}),r.mode===`hidden`){if(t.flags&128){if(a=a===null?n:a.baseLanes|n,e!==null){for(r=t.child=e.child,i=0;r!==null;)i=i|r.lanes|r.childLanes,r=r.sibling;r=i&~a}else r=0,t.child=null;return hc(e,t,a,n,r)}if(n&536870912)t.memoizedState={baseLanes:0,cachePool:null},e!==null&&Ca(t,a===null?null:a.cachePool),a===null?io():ro(t,a),uo(t);else return r=t.lanes=536870912,hc(e,t,a===null?n:a.baseLanes|n,n,r)}else a===null?(e!==null&&Ca(t,null),io(),fo(t)):(Ca(t,a.cachePool),ro(t,a),fo(t),t.memoizedState=null);return lc(e,t,i,n),t.child}function mc(e,t){return e!==null&&e.tag===22||t.stateNode!==null||(t.stateNode={_visibility:1,_pendingMarkers:null,_retryCache:null,_transitions:null}),t.sibling}function hc(e,t,n,r,i){var a=Sa();return a=a===null?null:{parent:ua._currentValue,pool:a},t.memoizedState={baseLanes:n,cachePool:a},e!==null&&Ca(t,null),io(),uo(t),e!==null&&ta(e,t,r,!0),t.childLanes=i,null}function gc(e,t){return t=kc({mode:t.mode,children:t.children},e.mode),t.ref=e.ref,e.child=t,t.return=e,t}function _c(e,t,n){return Va(t,e.child,null,n),e=gc(t,t.pendingProps),e.flags|=2,po(t),t.memoizedState=null,e}function vc(e,t,n){var r=t.pendingProps,a=(t.flags&128)!=0;if(t.flags&=-129,e===null){if(Ri){if(r.mode===`hidden`)return e=gc(t,r),t.lanes=536870912,mc(null,e);if(lo(t),(e=Li)?(e=mf(e,Bi),e=e!==null&&e.data===`&`?e:null,e!==null&&(t.memoizedState={dehydrated:e,treeContext:Oi===null?null:{id:ki,overflow:Ai},retryLane:536870912,hydrationErrors:null},n=vi(e),n.return=t,t.child=n,Ii=t,Li=null)):e=null,e===null)throw Hi(t);return t.lanes=536870912,null}return gc(t,r)}var o=e.memoizedState;if(o!==null){var s=o.dehydrated;if(lo(t),a)if(t.flags&256)t.flags&=-257,t=_c(e,t,n);else if(t.memoizedState!==null)t.child=e.child,t.flags|=128,t=null;else throw Error(i(558));else if(cc||ta(e,t,n,!1),a=(n&e.childLanes)!==0,cc||a){if(r=Wl,r!==null&&(s=rt(r,n),s!==0&&s!==o.retryLane))throw o.retryLane=s,oi(e,s),wu(r,e,s),sc;Iu(),t=_c(e,t,n)}else e=o.treeContext,Li=vf(s.nextSibling),Ii=t,Ri=!0,zi=null,Bi=!1,e!==null&&Fi(t,e),t=gc(t,r),t.flags|=4096;return t}return e=pi(e.child,{mode:r.mode,children:r.children}),e.ref=t.ref,t.child=e,e.return=t,e}function yc(e,t){var n=t.ref;if(n===null)e!==null&&e.ref!==null&&(t.flags|=4194816);else{if(typeof n!=`function`&&typeof n!=`object`)throw Error(i(284));(e===null||e.ref!==n)&&(t.flags|=4194816)}}function bc(e,t,n,r,i){return ra(t),n=Oo(e,t,n,r,void 0,i),r=Mo(),e!==null&&!cc?(No(e,t,i),Pc(e,t,i)):(Ri&&r&&Ni(t),t.flags|=1,lc(e,t,n,i),t.child)}function xc(e,t,n,r,i,a){return ra(t),t.updateQueue=null,n=Ao(t,r,n,i),ko(e),r=Mo(),e!==null&&!cc?(No(e,t,a),Pc(e,t,a)):(Ri&&r&&Ni(t),t.flags|=1,lc(e,t,n,a),t.child)}function Sc(e,t,n,r,i){if(ra(t),t.stateNode===null){var a=li,o=n.contextType;typeof o==`object`&&o&&(a=ia(o)),a=new n(r,a),t.memoizedState=a.state!==null&&a.state!==void 0?a.state:null,a.updater=Js,t.stateNode=a,a._reactInternals=t,a=t.stateNode,a.props=r,a.state=t.memoizedState,a.refs={},Wa(t),o=n.contextType,a.context=typeof o==`object`&&o?ia(o):li,a.state=t.memoizedState,o=n.getDerivedStateFromProps,typeof o==`function`&&(qs(t,n,o,r),a.state=t.memoizedState),typeof n.getDerivedStateFromProps==`function`||typeof a.getSnapshotBeforeUpdate==`function`||typeof a.UNSAFE_componentWillMount!=`function`&&typeof a.componentWillMount!=`function`||(o=a.state,typeof a.componentWillMount==`function`&&a.componentWillMount(),typeof a.UNSAFE_componentWillMount==`function`&&a.UNSAFE_componentWillMount(),o!==a.state&&Js.enqueueReplaceState(a,a.state,null),Qa(t,r,a,i),Za(),a.state=t.memoizedState),typeof a.componentDidMount==`function`&&(t.flags|=4194308),r=!0}else if(e===null){a=t.stateNode;var s=t.memoizedProps,c=Zs(n,s);a.props=c;var l=a.context,u=n.contextType;o=li,typeof u==`object`&&u&&(o=ia(u));var d=n.getDerivedStateFromProps;u=typeof d==`function`||typeof a.getSnapshotBeforeUpdate==`function`,s=t.pendingProps!==s,u||typeof a.UNSAFE_componentWillReceiveProps!=`function`&&typeof a.componentWillReceiveProps!=`function`||(s||l!==o)&&Xs(t,a,r,o),Ua=!1;var f=t.memoizedState;a.state=f,Qa(t,r,a,i),Za(),l=t.memoizedState,s||f!==l||Ua?(typeof d==`function`&&(qs(t,n,d,r),l=t.memoizedState),(c=Ua||Ys(t,n,c,r,f,l,o))?(u||typeof a.UNSAFE_componentWillMount!=`function`&&typeof a.componentWillMount!=`function`||(typeof a.componentWillMount==`function`&&a.componentWillMount(),typeof a.UNSAFE_componentWillMount==`function`&&a.UNSAFE_componentWillMount()),typeof a.componentDidMount==`function`&&(t.flags|=4194308)):(typeof a.componentDidMount==`function`&&(t.flags|=4194308),t.memoizedProps=r,t.memoizedState=l),a.props=r,a.state=l,a.context=o,r=c):(typeof a.componentDidMount==`function`&&(t.flags|=4194308),r=!1)}else{a=t.stateNode,Ga(e,t),o=t.memoizedProps,u=Zs(n,o),a.props=u,d=t.pendingProps,f=a.context,l=n.contextType,c=li,typeof l==`object`&&l&&(c=ia(l)),s=n.getDerivedStateFromProps,(l=typeof s==`function`||typeof a.getSnapshotBeforeUpdate==`function`)||typeof a.UNSAFE_componentWillReceiveProps!=`function`&&typeof a.componentWillReceiveProps!=`function`||(o!==d||f!==c)&&Xs(t,a,r,c),Ua=!1,f=t.memoizedState,a.state=f,Qa(t,r,a,i),Za();var p=t.memoizedState;o!==d||f!==p||Ua||e!==null&&e.dependencies!==null&&na(e.dependencies)?(typeof s==`function`&&(qs(t,n,s,r),p=t.memoizedState),(u=Ua||Ys(t,n,u,r,f,p,c)||e!==null&&e.dependencies!==null&&na(e.dependencies))?(l||typeof a.UNSAFE_componentWillUpdate!=`function`&&typeof a.componentWillUpdate!=`function`||(typeof a.componentWillUpdate==`function`&&a.componentWillUpdate(r,p,c),typeof a.UNSAFE_componentWillUpdate==`function`&&a.UNSAFE_componentWillUpdate(r,p,c)),typeof a.componentDidUpdate==`function`&&(t.flags|=4),typeof a.getSnapshotBeforeUpdate==`function`&&(t.flags|=1024)):(typeof a.componentDidUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=4),typeof a.getSnapshotBeforeUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=1024),t.memoizedProps=r,t.memoizedState=p),a.props=r,a.state=p,a.context=c,r=u):(typeof a.componentDidUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=4),typeof a.getSnapshotBeforeUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=1024),r=!1)}return a=r,yc(e,t),r=(t.flags&128)!=0,a||r?(a=t.stateNode,n=r&&typeof n.getDerivedStateFromError!=`function`?null:a.render(),t.flags|=1,e!==null&&r?(t.child=Va(t,e.child,null,i),t.child=Va(t,null,n,i)):lc(e,t,n,i),t.memoizedState=a.state,e=t.child):e=Pc(e,t,i),e}function Cc(e,t,n,r){return Gi(),t.flags|=256,lc(e,t,n,r),t.child}var wc={dehydrated:null,treeContext:null,retryLane:0,hydrationErrors:null};function Tc(e){return{baseLanes:e,cachePool:wa()}}function Ec(e,t,n){return e=e===null?0:e.childLanes&~n,t&&(e|=ru),e}function Dc(e,t,n){var r=t.pendingProps,a=!1,o=(t.flags&128)!=0,s;if((s=o)||(s=e!==null&&e.memoizedState===null?!1:(mo.current&2)!=0),s&&(a=!0,t.flags&=-129),s=(t.flags&32)!=0,t.flags&=-33,e===null){if(Ri){if(a?co(t):fo(t),(e=Li)?(e=mf(e,Bi),e=e!==null&&e.data!==`&`?e:null,e!==null&&(t.memoizedState={dehydrated:e,treeContext:Oi===null?null:{id:ki,overflow:Ai},retryLane:536870912,hydrationErrors:null},n=vi(e),n.return=t,t.child=n,Ii=t,Li=null)):e=null,e===null)throw Hi(t);return gf(e)?t.lanes=32:t.lanes=536870912,null}var c=r.children;return r=r.fallback,a?(fo(t),a=t.mode,c=kc({mode:`hidden`,children:c},a),r=gi(r,a,n,null),c.return=t,r.return=t,c.sibling=r,t.child=c,r=t.child,r.memoizedState=Tc(n),r.childLanes=Ec(e,s,n),t.memoizedState=wc,mc(null,r)):(co(t),Oc(t,c))}var l=e.memoizedState;if(l!==null&&(c=l.dehydrated,c!==null)){if(o)t.flags&256?(co(t),t.flags&=-257,t=Ac(e,t,n)):t.memoizedState===null?(fo(t),c=r.fallback,a=t.mode,r=kc({mode:`visible`,children:r.children},a),c=gi(c,a,n,null),c.flags|=2,r.return=t,c.return=t,r.sibling=c,t.child=r,Va(t,e.child,null,n),r=t.child,r.memoizedState=Tc(n),r.childLanes=Ec(e,s,n),t.memoizedState=wc,t=mc(null,r)):(fo(t),t.child=e.child,t.flags|=128,t=null);else if(co(t),gf(c)){if(s=c.nextSibling&&c.nextSibling.dataset,s)var u=s.dgst;s=u,r=Error(i(419)),r.stack=``,r.digest=s,qi({value:r,source:null,stack:null}),t=Ac(e,t,n)}else if(cc||ta(e,t,n,!1),s=(n&e.childLanes)!==0,cc||s){if(s=Wl,s!==null&&(r=rt(s,n),r!==0&&r!==l.retryLane))throw l.retryLane=r,oi(e,r),wu(s,e,r),sc;hf(c)||Iu(),t=Ac(e,t,n)}else hf(c)?(t.flags|=192,t.child=e.child,t=null):(e=l.treeContext,Li=vf(c.nextSibling),Ii=t,Ri=!0,zi=null,Bi=!1,e!==null&&Fi(t,e),t=Oc(t,r.children),t.flags|=4096);return t}return a?(fo(t),c=r.fallback,a=t.mode,l=e.child,u=l.sibling,r=pi(l,{mode:`hidden`,children:r.children}),r.subtreeFlags=l.subtreeFlags&65011712,u===null?(c=gi(c,a,n,null),c.flags|=2):c=pi(u,c),c.return=t,r.return=t,r.sibling=c,t.child=r,mc(null,r),r=t.child,c=e.child.memoizedState,c===null?c=Tc(n):(a=c.cachePool,a===null?a=wa():(l=ua._currentValue,a=a.parent===l?a:{parent:l,pool:l}),c={baseLanes:c.baseLanes|n,cachePool:a}),r.memoizedState=c,r.childLanes=Ec(e,s,n),t.memoizedState=wc,mc(e.child,r)):(co(t),n=e.child,e=n.sibling,n=pi(n,{mode:`visible`,children:r.children}),n.return=t,n.sibling=null,e!==null&&(s=t.deletions,s===null?(t.deletions=[e],t.flags|=16):s.push(e)),t.child=n,t.memoizedState=null,n)}function Oc(e,t){return t=kc({mode:`visible`,children:t},e.mode),t.return=e,e.child=t}function kc(e,t){return e=di(22,e,null,t),e.lanes=0,e}function Ac(e,t,n){return Va(t,e.child,null,n),e=Oc(t,t.pendingProps.children),e.flags|=2,t.memoizedState=null,e}function jc(e,t,n){e.lanes|=t;var r=e.alternate;r!==null&&(r.lanes|=t),$i(e.return,t,n)}function Mc(e,t,n,r,i,a){var o=e.memoizedState;o===null?e.memoizedState={isBackwards:t,rendering:null,renderingStartTime:0,last:r,tail:n,tailMode:i,treeForkCount:a}:(o.isBackwards=t,o.rendering=null,o.renderingStartTime=0,o.last=r,o.tail=n,o.tailMode=i,o.treeForkCount=a)}function Nc(e,t,n){var r=t.pendingProps,i=r.revealOrder,a=r.tail;r=r.children;var o=mo.current,s=(o&2)!=0;if(s?(o=o&1|2,t.flags|=128):o&=1,se(mo,o),lc(e,t,r,n),r=Ri?Ti:0,!s&&e!==null&&e.flags&128)a:for(e=t.child;e!==null;){if(e.tag===13)e.memoizedState!==null&&jc(e,n,t);else if(e.tag===19)jc(e,n,t);else if(e.child!==null){e.child.return=e,e=e.child;continue}if(e===t)break a;for(;e.sibling===null;){if(e.return===null||e.return===t)break a;e=e.return}e.sibling.return=e.return,e=e.sibling}switch(i){case`forwards`:for(n=t.child,i=null;n!==null;)e=n.alternate,e!==null&&ho(e)===null&&(i=n),n=n.sibling;n=i,n===null?(i=t.child,t.child=null):(i=n.sibling,n.sibling=null),Mc(t,!1,i,n,a,r);break;case`backwards`:case`unstable_legacy-backwards`:for(n=null,i=t.child,t.child=null;i!==null;){if(e=i.alternate,e!==null&&ho(e)===null){t.child=i;break}e=i.sibling,i.sibling=n,n=i,i=e}Mc(t,!0,n,null,a,r);break;case`together`:Mc(t,!1,null,null,void 0,r);break;default:t.memoizedState=null}return t.child}function Pc(e,t,n){if(e!==null&&(t.dependencies=e.dependencies),eu|=t.lanes,(n&t.childLanes)===0)if(e!==null){if(ta(e,t,n,!1),(n&t.childLanes)===0)return null}else return null;if(e!==null&&t.child!==e.child)throw Error(i(153));if(t.child!==null){for(e=t.child,n=pi(e,e.pendingProps),t.child=n,n.return=t;e.sibling!==null;)e=e.sibling,n=n.sibling=pi(e,e.pendingProps),n.return=t;n.sibling=null}return t.child}function Fc(e,t){return(e.lanes&t)===0?(e=e.dependencies,!!(e!==null&&na(e))):!0}function Ic(e,t,n){switch(t.tag){case 3:fe(t,t.stateNode.containerInfo),Zi(t,ua,e.memoizedState.cache),Gi();break;case 27:case 5:me(t);break;case 4:fe(t,t.stateNode.containerInfo);break;case 10:Zi(t,t.type,t.memoizedProps.value);break;case 31:if(t.memoizedState!==null)return t.flags|=128,lo(t),null;break;case 13:var r=t.memoizedState;if(r!==null)return r.dehydrated===null?(n&t.child.childLanes)===0?(co(t),e=Pc(e,t,n),e===null?null:e.sibling):Dc(e,t,n):(co(t),t.flags|=128,null);co(t);break;case 19:var i=(e.flags&128)!=0;if(r=(n&t.childLanes)!==0,r||=(ta(e,t,n,!1),(n&t.childLanes)!==0),i){if(r)return Nc(e,t,n);t.flags|=128}if(i=t.memoizedState,i!==null&&(i.rendering=null,i.tail=null,i.lastEffect=null),se(mo,mo.current),r)break;return null;case 22:return t.lanes=0,pc(e,t,n,t.pendingProps);case 24:Zi(t,ua,e.memoizedState.cache)}return Pc(e,t,n)}function Lc(e,t,n){if(e!==null)if(e.memoizedProps!==t.pendingProps)cc=!0;else{if(!Fc(e,n)&&!(t.flags&128))return cc=!1,Ic(e,t,n);cc=!!(e.flags&131072)}else cc=!1,Ri&&t.flags&1048576&&Mi(t,Ti,t.index);switch(t.lanes=0,t.tag){case 16:a:{var r=t.pendingProps;if(e=ja(t.elementType),t.type=e,typeof e==`function`)fi(e)?(r=Zs(e,r),t.tag=1,t=Sc(null,t,e,r,n)):(t.tag=0,t=bc(null,t,e,r,n));else{if(e!=null){var a=e.$$typeof;if(a===w){t.tag=11,t=uc(null,t,e,r,n);break a}else if(a===D){t.tag=14,t=dc(null,t,e,r,n);break a}}throw t=te(e)||e,Error(i(306,t,``))}}return t;case 0:return bc(e,t,t.type,t.pendingProps,n);case 1:return r=t.type,a=Zs(r,t.pendingProps),Sc(e,t,r,a,n);case 3:a:{if(fe(t,t.stateNode.containerInfo),e===null)throw Error(i(387));r=t.pendingProps;var o=t.memoizedState;a=o.element,Ga(e,t),Qa(t,r,null,n);var s=t.memoizedState;if(r=s.cache,Zi(t,ua,r),r!==o.cache&&ea(t,[ua],n,!0),Za(),r=s.element,o.isDehydrated)if(o={element:r,isDehydrated:!1,cache:s.cache},t.updateQueue.baseState=o,t.memoizedState=o,t.flags&256){t=Cc(e,t,r,n);break a}else if(r!==a){a=xi(Error(i(424)),t),qi(a),t=Cc(e,t,r,n);break a}else{switch(e=t.stateNode.containerInfo,e.nodeType){case 9:e=e.body;break;default:e=e.nodeName===`HTML`?e.ownerDocument.body:e}for(Li=vf(e.firstChild),Ii=t,Ri=!0,zi=null,Bi=!0,n=Ha(t,null,r,n),t.child=n;n;)n.flags=n.flags&-3|4096,n=n.sibling}else{if(Gi(),r===a){t=Pc(e,t,n);break a}lc(e,t,r,n)}t=t.child}return t;case 26:return yc(e,t),e===null?(n=zf(t.type,null,t.pendingProps,null))?t.memoizedState=n:Ri||(n=t.type,e=t.pendingProps,r=Zd(ue.current).createElement(n),r[lt]=t,r[ut]=e,Gd(r,n,e),St(r),t.stateNode=r):t.memoizedState=zf(t.type,e.memoizedProps,t.pendingProps,e.memoizedState),null;case 27:return me(t),e===null&&Ri&&(r=t.stateNode=Sf(t.type,t.pendingProps,ue.current),Ii=t,Bi=!0,a=Li,lf(t.type)?(yf=a,Li=vf(r.firstChild)):Li=a),lc(e,t,t.pendingProps.children,n),yc(e,t),e===null&&(t.flags|=4194304),t.child;case 5:return e===null&&Ri&&((a=r=Li)&&(r=ff(r,t.type,t.pendingProps,Bi),r===null?a=!1:(t.stateNode=r,Ii=t,Li=vf(r.firstChild),Bi=!1,a=!0)),a||Hi(t)),me(t),a=t.type,o=t.pendingProps,s=e===null?null:e.memoizedProps,r=o.children,ef(a,o)?r=null:s!==null&&ef(a,s)&&(t.flags|=32),t.memoizedState!==null&&(a=Oo(e,t,jo,null,null,n),cp._currentValue=a),yc(e,t),lc(e,t,r,n),t.child;case 6:return e===null&&Ri&&((e=n=Li)&&(n=pf(n,t.pendingProps,Bi),n===null?e=!1:(t.stateNode=n,Ii=t,Li=null,e=!0)),e||Hi(t)),null;case 13:return Dc(e,t,n);case 4:return fe(t,t.stateNode.containerInfo),r=t.pendingProps,e===null?t.child=Va(t,null,r,n):lc(e,t,r,n),t.child;case 11:return uc(e,t,t.type,t.pendingProps,n);case 7:return lc(e,t,t.pendingProps,n),t.child;case 8:return lc(e,t,t.pendingProps.children,n),t.child;case 12:return lc(e,t,t.pendingProps.children,n),t.child;case 10:return r=t.pendingProps,Zi(t,t.type,r.value),lc(e,t,r.children,n),t.child;case 9:return a=t.type._context,r=t.pendingProps.children,ra(t),a=ia(a),r=r(a),t.flags|=1,lc(e,t,r,n),t.child;case 14:return dc(e,t,t.type,t.pendingProps,n);case 15:return fc(e,t,t.type,t.pendingProps,n);case 19:return Nc(e,t,n);case 31:return vc(e,t,n);case 22:return pc(e,t,n,t.pendingProps);case 24:return ra(t),r=ia(ua),e===null?(a=Sa(),a===null&&(a=Wl,o=da(),a.pooledCache=o,o.refCount++,o!==null&&(a.pooledCacheLanes|=n),a=o),t.memoizedState={parent:r,cache:a},Wa(t),Zi(t,ua,a)):((e.lanes&n)!==0&&(Ga(e,t),Qa(t,null,null,n),Za()),a=e.memoizedState,o=t.memoizedState,a.parent===r?(r=o.cache,Zi(t,ua,r),r!==a.cache&&ea(t,[ua],n,!0)):(a={parent:r,cache:r},t.memoizedState=a,t.lanes===0&&(t.memoizedState=t.updateQueue.baseState=a),Zi(t,ua,r))),lc(e,t,t.pendingProps.children,n),t.child;case 29:throw t.pendingProps}throw Error(i(156,t.tag))}function Rc(e){e.flags|=4}function zc(e,t,n,r,i){if((t=(e.mode&32)!=0)&&(t=!1),t){if(e.flags|=16777216,(i&335544128)===i)if(e.stateNode.complete)e.flags|=8192;else if(Nu())e.flags|=8192;else throw Ma=Oa,Ea}else e.flags&=-16777217}function Bc(e,t){if(t.type!==`stylesheet`||t.state.loading&4)e.flags&=-16777217;else if(e.flags|=16777216,!ep(t))if(Nu())e.flags|=8192;else throw Ma=Oa,Ea}function Vc(e,t){t!==null&&(e.flags|=4),e.flags&16384&&(t=e.tag===22?536870912:Ze(),e.lanes|=t,iu|=t)}function Hc(e,t){if(!Ri)switch(e.tailMode){case`hidden`:t=e.tail;for(var n=null;t!==null;)t.alternate!==null&&(n=t),t=t.sibling;n===null?e.tail=null:n.sibling=null;break;case`collapsed`:n=e.tail;for(var r=null;n!==null;)n.alternate!==null&&(r=n),n=n.sibling;r===null?t||e.tail===null?e.tail=null:e.tail.sibling=null:r.sibling=null}}function Uc(e){var t=e.alternate!==null&&e.alternate.child===e.child,n=0,r=0;if(t)for(var i=e.child;i!==null;)n|=i.lanes|i.childLanes,r|=i.subtreeFlags&65011712,r|=i.flags&65011712,i.return=e,i=i.sibling;else for(i=e.child;i!==null;)n|=i.lanes|i.childLanes,r|=i.subtreeFlags,r|=i.flags,i.return=e,i=i.sibling;return e.subtreeFlags|=r,e.childLanes=n,t}function Wc(e,t,n){var r=t.pendingProps;switch(Pi(t),t.tag){case 16:case 15:case 0:case 11:case 7:case 8:case 12:case 9:case 14:return Uc(t),null;case 1:return Uc(t),null;case 3:return n=t.stateNode,r=null,e!==null&&(r=e.memoizedState.cache),t.memoizedState.cache!==r&&(t.flags|=2048),Qi(ua),pe(),n.pendingContext&&=(n.context=n.pendingContext,null),(e===null||e.child===null)&&(Wi(t)?Rc(t):e===null||e.memoizedState.isDehydrated&&!(t.flags&256)||(t.flags|=1024,Ki())),Uc(t),null;case 26:var a=t.type,o=t.memoizedState;return e===null?(Rc(t),o===null?(Uc(t),zc(t,a,null,r,n)):(Uc(t),Bc(t,o))):o?o===e.memoizedState?(Uc(t),t.flags&=-16777217):(Rc(t),Uc(t),Bc(t,o)):(e=e.memoizedProps,e!==r&&Rc(t),Uc(t),zc(t,a,e,r,n)),null;case 27:if(he(t),n=ue.current,a=t.type,e!==null&&t.stateNode!=null)e.memoizedProps!==r&&Rc(t);else{if(!r){if(t.stateNode===null)throw Error(i(166));return Uc(t),null}e=ce.current,Wi(t)?I(t,e):(e=Sf(a,r,n),t.stateNode=e,Rc(t))}return Uc(t),null;case 5:if(he(t),a=t.type,e!==null&&t.stateNode!=null)e.memoizedProps!==r&&Rc(t);else{if(!r){if(t.stateNode===null)throw Error(i(166));return Uc(t),null}if(o=ce.current,Wi(t))I(t,o);else{var s=Zd(ue.current);switch(o){case 1:o=s.createElementNS(`http://www.w3.org/2000/svg`,a);break;case 2:o=s.createElementNS(`http://www.w3.org/1998/Math/MathML`,a);break;default:switch(a){case`svg`:o=s.createElementNS(`http://www.w3.org/2000/svg`,a);break;case`math`:o=s.createElementNS(`http://www.w3.org/1998/Math/MathML`,a);break;case`script`:o=s.createElement(`div`),o.innerHTML=`<script><\/script>`,o=o.removeChild(o.firstChild);break;case`select`:o=typeof r.is==`string`?s.createElement(`select`,{is:r.is}):s.createElement(`select`),r.multiple?o.multiple=!0:r.size&&(o.size=r.size);break;default:o=typeof r.is==`string`?s.createElement(a,{is:r.is}):s.createElement(a)}}o[lt]=t,o[ut]=r;a:for(s=t.child;s!==null;){if(s.tag===5||s.tag===6)o.appendChild(s.stateNode);else if(s.tag!==4&&s.tag!==27&&s.child!==null){s.child.return=s,s=s.child;continue}if(s===t)break a;for(;s.sibling===null;){if(s.return===null||s.return===t)break a;s=s.return}s.sibling.return=s.return,s=s.sibling}t.stateNode=o;a:switch(Gd(o,a,r),a){case`button`:case`input`:case`select`:case`textarea`:r=!!r.autoFocus;break a;case`img`:r=!0;break a;default:r=!1}r&&Rc(t)}}return Uc(t),zc(t,t.type,e===null?null:e.memoizedProps,t.pendingProps,n),null;case 6:if(e&&t.stateNode!=null)e.memoizedProps!==r&&Rc(t);else{if(typeof r!=`string`&&t.stateNode===null)throw Error(i(166));if(e=ue.current,Wi(t)){if(e=t.stateNode,n=t.memoizedProps,r=null,a=Ii,a!==null)switch(a.tag){case 27:case 5:r=a.memoizedProps}e[lt]=t,e=!!(e.nodeValue===n||r!==null&&!0===r.suppressHydrationWarning||Hd(e.nodeValue,n)),e||Hi(t,!0)}else e=Zd(e).createTextNode(r),e[lt]=t,t.stateNode=e}return Uc(t),null;case 31:if(n=t.memoizedState,e===null||e.memoizedState!==null){if(r=Wi(t),n!==null){if(e===null){if(!r)throw Error(i(318));if(e=t.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(557));e[lt]=t}else Gi(),!(t.flags&128)&&(t.memoizedState=null),t.flags|=4;Uc(t),e=!1}else n=Ki(),e!==null&&e.memoizedState!==null&&(e.memoizedState.hydrationErrors=n),e=!0;if(!e)return t.flags&256?(po(t),t):(po(t),null);if(t.flags&128)throw Error(i(558))}return Uc(t),null;case 13:if(r=t.memoizedState,e===null||e.memoizedState!==null&&e.memoizedState.dehydrated!==null){if(a=Wi(t),r!==null&&r.dehydrated!==null){if(e===null){if(!a)throw Error(i(318));if(a=t.memoizedState,a=a===null?null:a.dehydrated,!a)throw Error(i(317));a[lt]=t}else Gi(),!(t.flags&128)&&(t.memoizedState=null),t.flags|=4;Uc(t),a=!1}else a=Ki(),e!==null&&e.memoizedState!==null&&(e.memoizedState.hydrationErrors=a),a=!0;if(!a)return t.flags&256?(po(t),t):(po(t),null)}return po(t),t.flags&128?(t.lanes=n,t):(n=r!==null,e=e!==null&&e.memoizedState!==null,n&&(r=t.child,a=null,r.alternate!==null&&r.alternate.memoizedState!==null&&r.alternate.memoizedState.cachePool!==null&&(a=r.alternate.memoizedState.cachePool.pool),o=null,r.memoizedState!==null&&r.memoizedState.cachePool!==null&&(o=r.memoizedState.cachePool.pool),o!==a&&(r.flags|=2048)),n!==e&&n&&(t.child.flags|=8192),Vc(t,t.updateQueue),Uc(t),null);case 4:return pe(),e===null&&Md(t.stateNode.containerInfo),Uc(t),null;case 10:return Qi(t.type),Uc(t),null;case 19:if(oe(mo),r=t.memoizedState,r===null)return Uc(t),null;if(a=(t.flags&128)!=0,o=r.rendering,o===null)if(a)Hc(r,!1);else{if($l!==0||e!==null&&e.flags&128)for(e=t.child;e!==null;){if(o=ho(e),o!==null){for(t.flags|=128,Hc(r,!1),e=o.updateQueue,t.updateQueue=e,Vc(t,e),t.subtreeFlags=0,e=n,n=t.child;n!==null;)mi(n,e),n=n.sibling;return se(mo,mo.current&1|2),Ri&&ji(t,r.treeForkCount),t.child}e=e.sibling}r.tail!==null&&Oe()>uu&&(t.flags|=128,a=!0,Hc(r,!1),t.lanes=4194304)}else{if(!a)if(e=ho(o),e!==null){if(t.flags|=128,a=!0,e=e.updateQueue,t.updateQueue=e,Vc(t,e),Hc(r,!0),r.tail===null&&r.tailMode===`hidden`&&!o.alternate&&!Ri)return Uc(t),null}else 2*Oe()-r.renderingStartTime>uu&&n!==536870912&&(t.flags|=128,a=!0,Hc(r,!1),t.lanes=4194304);r.isBackwards?(o.sibling=t.child,t.child=o):(e=r.last,e===null?t.child=o:e.sibling=o,r.last=o)}return r.tail===null?(Uc(t),null):(e=r.tail,r.rendering=e,r.tail=e.sibling,r.renderingStartTime=Oe(),e.sibling=null,n=mo.current,se(mo,a?n&1|2:n&1),Ri&&ji(t,r.treeForkCount),e);case 22:case 23:return po(t),ao(),r=t.memoizedState!==null,e===null?r&&(t.flags|=8192):e.memoizedState!==null!==r&&(t.flags|=8192),r?n&536870912&&!(t.flags&128)&&(Uc(t),t.subtreeFlags&6&&(t.flags|=8192)):Uc(t),n=t.updateQueue,n!==null&&Vc(t,n.retryQueue),n=null,e!==null&&e.memoizedState!==null&&e.memoizedState.cachePool!==null&&(n=e.memoizedState.cachePool.pool),r=null,t.memoizedState!==null&&t.memoizedState.cachePool!==null&&(r=t.memoizedState.cachePool.pool),r!==n&&(t.flags|=2048),e!==null&&oe(xa),null;case 24:return n=null,e!==null&&(n=e.memoizedState.cache),t.memoizedState.cache!==n&&(t.flags|=2048),Qi(ua),Uc(t),null;case 25:return null;case 30:return null}throw Error(i(156,t.tag))}function Gc(e,t){switch(Pi(t),t.tag){case 1:return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 3:return Qi(ua),pe(),e=t.flags,e&65536&&!(e&128)?(t.flags=e&-65537|128,t):null;case 26:case 27:case 5:return he(t),null;case 31:if(t.memoizedState!==null){if(po(t),t.alternate===null)throw Error(i(340));Gi()}return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 13:if(po(t),e=t.memoizedState,e!==null&&e.dehydrated!==null){if(t.alternate===null)throw Error(i(340));Gi()}return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 19:return oe(mo),null;case 4:return pe(),null;case 10:return Qi(t.type),null;case 22:case 23:return po(t),ao(),e!==null&&oe(xa),e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 24:return Qi(ua),null;case 25:return null;default:return null}}function Kc(e,t){switch(Pi(t),t.tag){case 3:Qi(ua),pe();break;case 26:case 27:case 5:he(t);break;case 4:pe();break;case 31:t.memoizedState!==null&&po(t);break;case 13:po(t);break;case 19:oe(mo);break;case 10:Qi(t.type);break;case 22:case 23:po(t),ao(),e!==null&&oe(xa);break;case 24:Qi(ua)}}function qc(e,t){try{var n=t.updateQueue,r=n===null?null:n.lastEffect;if(r!==null){var i=r.next;n=i;do{if((n.tag&e)===e){r=void 0;var a=n.create,o=n.inst;r=a(),o.destroy=r}n=n.next}while(n!==i)}}catch(e){ed(t,t.return,e)}}function Jc(e,t,n){try{var r=t.updateQueue,i=r===null?null:r.lastEffect;if(i!==null){var a=i.next;r=a;do{if((r.tag&e)===e){var o=r.inst,s=o.destroy;if(s!==void 0){o.destroy=void 0,i=t;var c=n,l=s;try{l()}catch(e){ed(i,c,e)}}}r=r.next}while(r!==a)}}catch(e){ed(t,t.return,e)}}function Yc(e){var t=e.updateQueue;if(t!==null){var n=e.stateNode;try{eo(t,n)}catch(t){ed(e,e.return,t)}}}function Xc(e,t,n){n.props=Zs(e.type,e.memoizedProps),n.state=e.memoizedState;try{n.componentWillUnmount()}catch(n){ed(e,t,n)}}function Zc(e,t){try{var n=e.ref;if(n!==null){switch(e.tag){case 26:case 27:case 5:var r=e.stateNode;break;case 30:r=e.stateNode;break;default:r=e.stateNode}typeof n==`function`?e.refCleanup=n(r):n.current=r}}catch(n){ed(e,t,n)}}function Qc(e,t){var n=e.ref,r=e.refCleanup;if(n!==null)if(typeof r==`function`)try{r()}catch(n){ed(e,t,n)}finally{e.refCleanup=null,e=e.alternate,e!=null&&(e.refCleanup=null)}else if(typeof n==`function`)try{n(null)}catch(n){ed(e,t,n)}else n.current=null}function $c(e){var t=e.type,n=e.memoizedProps,r=e.stateNode;try{a:switch(t){case`button`:case`input`:case`select`:case`textarea`:n.autoFocus&&r.focus();break a;case`img`:n.src?r.src=n.src:n.srcSet&&(r.srcset=n.srcSet)}}catch(t){ed(e,e.return,t)}}function el(e,t,n){try{var r=e.stateNode;Kd(r,e.type,n,t),r[ut]=t}catch(t){ed(e,e.return,t)}}function tl(e){return e.tag===5||e.tag===3||e.tag===26||e.tag===27&&lf(e.type)||e.tag===4}function nl(e){a:for(;;){for(;e.sibling===null;){if(e.return===null||tl(e.return))return null;e=e.return}for(e.sibling.return=e.return,e=e.sibling;e.tag!==5&&e.tag!==6&&e.tag!==18;){if(e.tag===27&&lf(e.type)||e.flags&2||e.child===null||e.tag===4)continue a;e.child.return=e,e=e.child}if(!(e.flags&2))return e.stateNode}}function rl(e,t,n){var r=e.tag;if(r===5||r===6)e=e.stateNode,t?(n.nodeType===9?n.body:n.nodeName===`HTML`?n.ownerDocument.body:n).insertBefore(e,t):(t=n.nodeType===9?n.body:n.nodeName===`HTML`?n.ownerDocument.body:n,t.appendChild(e),n=n._reactRootContainer,n!=null||t.onclick!==null||(t.onclick=nn));else if(r!==4&&(r===27&&lf(e.type)&&(n=e.stateNode,t=null),e=e.child,e!==null))for(rl(e,t,n),e=e.sibling;e!==null;)rl(e,t,n),e=e.sibling}function il(e,t,n){var r=e.tag;if(r===5||r===6)e=e.stateNode,t?n.insertBefore(e,t):n.appendChild(e);else if(r!==4&&(r===27&&lf(e.type)&&(n=e.stateNode),e=e.child,e!==null))for(il(e,t,n),e=e.sibling;e!==null;)il(e,t,n),e=e.sibling}function al(e){var t=e.stateNode,n=e.memoizedProps;try{for(var r=e.type,i=t.attributes;i.length;)t.removeAttributeNode(i[0]);Gd(t,r,n),t[lt]=e,t[ut]=n}catch(t){ed(e,e.return,t)}}var z=!1,ol=!1,sl=!1,cl=typeof WeakSet==`function`?WeakSet:Set,ll=null;function ul(e,t){if(e=e.containerInfo,Yd=_p,e=Ar(e),jr(e)){if(`selectionStart`in e)var n={start:e.selectionStart,end:e.selectionEnd};else a:{n=(n=e.ownerDocument)&&n.defaultView||window;var r=n.getSelection&&n.getSelection();if(r&&r.rangeCount!==0){n=r.anchorNode;var a=r.anchorOffset,o=r.focusNode;r=r.focusOffset;try{n.nodeType,o.nodeType}catch{n=null;break a}var s=0,c=-1,l=-1,u=0,d=0,f=e,p=null;b:for(;;){for(var m;f!==n||a!==0&&f.nodeType!==3||(c=s+a),f!==o||r!==0&&f.nodeType!==3||(l=s+r),f.nodeType===3&&(s+=f.nodeValue.length),(m=f.firstChild)!==null;)p=f,f=m;for(;;){if(f===e)break b;if(p===n&&++u===a&&(c=s),p===o&&++d===r&&(l=s),(m=f.nextSibling)!==null)break;f=p,p=f.parentNode}f=m}n=c===-1||l===-1?null:{start:c,end:l}}else n=null}n||={start:0,end:0}}else n=null;for(Xd={focusedElem:e,selectionRange:n},_p=!1,ll=t;ll!==null;)if(t=ll,e=t.child,t.subtreeFlags&1028&&e!==null)e.return=t,ll=e;else for(;ll!==null;){switch(t=ll,o=t.alternate,e=t.flags,t.tag){case 0:if(e&4&&(e=t.updateQueue,e=e===null?null:e.events,e!==null))for(n=0;n<e.length;n++)a=e[n],a.ref.impl=a.nextImpl;break;case 11:case 15:break;case 1:if(e&1024&&o!==null){e=void 0,n=t,a=o.memoizedProps,o=o.memoizedState,r=n.stateNode;try{var h=Zs(n.type,a);e=r.getSnapshotBeforeUpdate(h,o),r.__reactInternalSnapshotBeforeUpdate=e}catch(e){ed(n,n.return,e)}}break;case 3:if(e&1024){if(e=t.stateNode.containerInfo,n=e.nodeType,n===9)U(e);else if(n===1)switch(e.nodeName){case`HEAD`:case`HTML`:case`BODY`:U(e);break;default:e.textContent=``}}break;case 5:case 26:case 27:case 6:case 4:case 17:break;default:if(e&1024)throw Error(i(163))}if(e=t.sibling,e!==null){e.return=t.return,ll=e;break}ll=t.return}}function dl(e,t,n){var r=n.flags;switch(n.tag){case 0:case 11:case 15:Tl(e,n),r&4&&qc(5,n);break;case 1:if(Tl(e,n),r&4)if(e=n.stateNode,t===null)try{e.componentDidMount()}catch(e){ed(n,n.return,e)}else{var i=Zs(n.type,t.memoizedProps);t=t.memoizedState;try{e.componentDidUpdate(i,t,e.__reactInternalSnapshotBeforeUpdate)}catch(e){ed(n,n.return,e)}}r&64&&Yc(n),r&512&&Zc(n,n.return);break;case 3:if(Tl(e,n),r&64&&(e=n.updateQueue,e!==null)){if(t=null,n.child!==null)switch(n.child.tag){case 27:case 5:t=n.child.stateNode;break;case 1:t=n.child.stateNode}try{eo(e,t)}catch(e){ed(n,n.return,e)}}break;case 27:t===null&&r&4&&al(n);case 26:case 5:Tl(e,n),t===null&&r&4&&$c(n),r&512&&Zc(n,n.return);break;case 12:Tl(e,n);break;case 31:Tl(e,n),r&4&&_l(e,n);break;case 13:Tl(e,n),r&4&&vl(e,n),r&64&&(e=n.memoizedState,e!==null&&(e=e.dehydrated,e!==null&&(n=id.bind(null,n),_f(e,n))));break;case 22:if(r=n.memoizedState!==null||z,!r){t=t!==null&&t.memoizedState!==null||ol,i=z;var a=ol;z=r,(ol=t)&&!a?V(e,n,(n.subtreeFlags&8772)!=0):Tl(e,n),z=i,ol=a}break;case 30:break;default:Tl(e,n)}}function fl(e){var t=e.alternate;t!==null&&(e.alternate=null,fl(t)),e.child=null,e.deletions=null,e.sibling=null,e.tag===5&&(t=e.stateNode,t!==null&&_t(t)),e.stateNode=null,e.return=null,e.dependencies=null,e.memoizedProps=null,e.memoizedState=null,e.pendingProps=null,e.stateNode=null,e.updateQueue=null}var pl=null,ml=!1;function hl(e,t,n){for(n=n.child;n!==null;)gl(e,t,n),n=n.sibling}function gl(e,t,n){if(Re&&typeof Re.onCommitFiberUnmount==`function`)try{Re.onCommitFiberUnmount(Le,n)}catch{}switch(n.tag){case 26:ol||Qc(n,t),hl(e,t,n),n.memoizedState?n.memoizedState.count--:n.stateNode&&(n=n.stateNode,n.parentNode.removeChild(n));break;case 27:ol||Qc(n,t);var r=pl,i=ml;lf(n.type)&&(pl=n.stateNode,ml=!1),hl(e,t,n),Cf(n.stateNode),pl=r,ml=i;break;case 5:ol||Qc(n,t);case 6:if(r=pl,i=ml,pl=null,hl(e,t,n),pl=r,ml=i,pl!==null)if(ml)try{(pl.nodeType===9?pl.body:pl.nodeName===`HTML`?pl.ownerDocument.body:pl).removeChild(n.stateNode)}catch(e){ed(n,t,e)}else try{pl.removeChild(n.stateNode)}catch(e){ed(n,t,e)}break;case 18:pl!==null&&(ml?(e=pl,uf(e.nodeType===9?e.body:e.nodeName===`HTML`?e.ownerDocument.body:e,n.stateNode),Up(e)):uf(pl,n.stateNode));break;case 4:r=pl,i=ml,pl=n.stateNode.containerInfo,ml=!0,hl(e,t,n),pl=r,ml=i;break;case 0:case 11:case 14:case 15:Jc(2,n,t),ol||Jc(4,n,t),hl(e,t,n);break;case 1:ol||(Qc(n,t),r=n.stateNode,typeof r.componentWillUnmount==`function`&&Xc(n,t,r)),hl(e,t,n);break;case 21:hl(e,t,n);break;case 22:ol=(r=ol)||n.memoizedState!==null,hl(e,t,n),ol=r;break;default:hl(e,t,n)}}function _l(e,t){if(t.memoizedState===null&&(e=t.alternate,e!==null&&(e=e.memoizedState,e!==null))){e=e.dehydrated;try{Up(e)}catch(e){ed(t,t.return,e)}}}function vl(e,t){if(t.memoizedState===null&&(e=t.alternate,e!==null&&(e=e.memoizedState,e!==null&&(e=e.dehydrated,e!==null))))try{Up(e)}catch(e){ed(t,t.return,e)}}function yl(e){switch(e.tag){case 31:case 13:case 19:var t=e.stateNode;return t===null&&(t=e.stateNode=new cl),t;case 22:return e=e.stateNode,t=e._retryCache,t===null&&(t=e._retryCache=new cl),t;default:throw Error(i(435,e.tag))}}function B(e,t){var n=yl(e);t.forEach(function(t){if(!n.has(t)){n.add(t);var r=ad.bind(null,e,t);t.then(r,r)}})}function bl(e,t){var n=t.deletions;if(n!==null)for(var r=0;r<n.length;r++){var a=n[r],o=e,s=t,c=s;a:for(;c!==null;){switch(c.tag){case 27:if(lf(c.type)){pl=c.stateNode,ml=!1;break a}break;case 5:pl=c.stateNode,ml=!1;break a;case 3:case 4:pl=c.stateNode.containerInfo,ml=!0;break a}c=c.return}if(pl===null)throw Error(i(160));gl(o,s,a),pl=null,ml=!1,o=a.alternate,o!==null&&(o.return=null),a.return=null}if(t.subtreeFlags&13886)for(t=t.child;t!==null;)Sl(t,e),t=t.sibling}var xl=null;function Sl(e,t){var n=e.alternate,r=e.flags;switch(e.tag){case 0:case 11:case 14:case 15:bl(t,e),Cl(e),r&4&&(Jc(3,e,e.return),qc(3,e),Jc(5,e,e.return));break;case 1:bl(t,e),Cl(e),r&512&&(ol||n===null||Qc(n,n.return)),r&64&&z&&(e=e.updateQueue,e!==null&&(r=e.callbacks,r!==null&&(n=e.shared.hiddenCallbacks,e.shared.hiddenCallbacks=n===null?r:n.concat(r))));break;case 26:var a=xl;if(bl(t,e),Cl(e),r&512&&(ol||n===null||Qc(n,n.return)),r&4){var o=n===null?null:n.memoizedState;if(r=e.memoizedState,n===null)if(r===null)if(e.stateNode===null){a:{r=e.type,n=e.memoizedProps,a=a.ownerDocument||a;b:switch(r){case`title`:o=a.getElementsByTagName(`title`)[0],(!o||o[gt]||o[lt]||o.namespaceURI===`http://www.w3.org/2000/svg`||o.hasAttribute(`itemprop`))&&(o=a.createElement(r),a.head.insertBefore(o,a.querySelector(`head > title`))),Gd(o,r,n),o[lt]=e,St(o),r=o;break a;case`link`:var s=Zf(`link`,`href`,a).get(r+(n.href||``));if(s){for(var c=0;c<s.length;c++)if(o=s[c],o.getAttribute(`href`)===(n.href==null||n.href===``?null:n.href)&&o.getAttribute(`rel`)===(n.rel==null?null:n.rel)&&o.getAttribute(`title`)===(n.title==null?null:n.title)&&o.getAttribute(`crossorigin`)===(n.crossOrigin==null?null:n.crossOrigin)){s.splice(c,1);break b}}o=a.createElement(r),Gd(o,r,n),a.head.appendChild(o);break;case`meta`:if(s=Zf(`meta`,`content`,a).get(r+(n.content||``))){for(c=0;c<s.length;c++)if(o=s[c],o.getAttribute(`content`)===(n.content==null?null:``+n.content)&&o.getAttribute(`name`)===(n.name==null?null:n.name)&&o.getAttribute(`property`)===(n.property==null?null:n.property)&&o.getAttribute(`http-equiv`)===(n.httpEquiv==null?null:n.httpEquiv)&&o.getAttribute(`charset`)===(n.charSet==null?null:n.charSet)){s.splice(c,1);break b}}o=a.createElement(r),Gd(o,r,n),a.head.appendChild(o);break;default:throw Error(i(468,r))}o[lt]=e,St(o),r=o}e.stateNode=r}else Qf(a,e.type,e.stateNode);else e.stateNode=Kf(a,r,e.memoizedProps);else o===r?r===null&&e.stateNode!==null&&el(e,e.memoizedProps,n.memoizedProps):(o===null?n.stateNode!==null&&(n=n.stateNode,n.parentNode.removeChild(n)):o.count--,r===null?Qf(a,e.type,e.stateNode):Kf(a,r,e.memoizedProps))}break;case 27:bl(t,e),Cl(e),r&512&&(ol||n===null||Qc(n,n.return)),n!==null&&r&4&&el(e,e.memoizedProps,n.memoizedProps);break;case 5:if(bl(t,e),Cl(e),r&512&&(ol||n===null||Qc(n,n.return)),e.flags&32){a=e.stateNode;try{Jt(a,``)}catch(t){ed(e,e.return,t)}}r&4&&e.stateNode!=null&&(a=e.memoizedProps,el(e,a,n===null?a:n.memoizedProps)),r&1024&&(sl=!0);break;case 6:if(bl(t,e),Cl(e),r&4){if(e.stateNode===null)throw Error(i(162));r=e.memoizedProps,n=e.stateNode;try{n.nodeValue=r}catch(t){ed(e,e.return,t)}}break;case 3:if(Xf=null,a=xl,xl=Ef(t.containerInfo),bl(t,e),xl=a,Cl(e),r&4&&n!==null&&n.memoizedState.isDehydrated)try{Up(t.containerInfo)}catch(t){ed(e,e.return,t)}sl&&(sl=!1,wl(e));break;case 4:r=xl,xl=Ef(e.stateNode.containerInfo),bl(t,e),Cl(e),xl=r;break;case 12:bl(t,e),Cl(e);break;case 31:bl(t,e),Cl(e),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,B(e,r)));break;case 13:bl(t,e),Cl(e),e.child.flags&8192&&e.memoizedState!==null!=(n!==null&&n.memoizedState!==null)&&(cu=Oe()),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,B(e,r)));break;case 22:a=e.memoizedState!==null;var l=n!==null&&n.memoizedState!==null,u=z,d=ol;if(z=u||a,ol=d||l,bl(t,e),ol=d,z=u,Cl(e),r&8192)a:for(t=e.stateNode,t._visibility=a?t._visibility&-2:t._visibility|1,a&&(n===null||l||z||ol||El(e)),n=null,t=e;;){if(t.tag===5||t.tag===26){if(n===null){l=n=t;try{if(o=l.stateNode,a)s=o.style,typeof s.setProperty==`function`?s.setProperty(`display`,`none`,`important`):s.display=`none`;else{c=l.stateNode;var f=l.memoizedProps.style,p=f!=null&&f.hasOwnProperty(`display`)?f.display:null;c.style.display=p==null||typeof p==`boolean`?``:(``+p).trim()}}catch(e){ed(l,l.return,e)}}}else if(t.tag===6){if(n===null){l=t;try{l.stateNode.nodeValue=a?``:l.memoizedProps}catch(e){ed(l,l.return,e)}}}else if(t.tag===18){if(n===null){l=t;try{var m=l.stateNode;a?df(m,!0):df(l.stateNode,!1)}catch(e){ed(l,l.return,e)}}}else if((t.tag!==22&&t.tag!==23||t.memoizedState===null||t===e)&&t.child!==null){t.child.return=t,t=t.child;continue}if(t===e)break a;for(;t.sibling===null;){if(t.return===null||t.return===e)break a;n===t&&(n=null),t=t.return}n===t&&(n=null),t.sibling.return=t.return,t=t.sibling}r&4&&(r=e.updateQueue,r!==null&&(n=r.retryQueue,n!==null&&(r.retryQueue=null,B(e,n))));break;case 19:bl(t,e),Cl(e),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,B(e,r)));break;case 30:break;case 21:break;default:bl(t,e),Cl(e)}}function Cl(e){var t=e.flags;if(t&2){try{for(var n,r=e.return;r!==null;){if(tl(r)){n=r;break}r=r.return}if(n==null)throw Error(i(160));switch(n.tag){case 27:var a=n.stateNode;il(e,nl(e),a);break;case 5:var o=n.stateNode;n.flags&32&&(Jt(o,``),n.flags&=-33),il(e,nl(e),o);break;case 3:case 4:var s=n.stateNode.containerInfo;rl(e,nl(e),s);break;default:throw Error(i(161))}}catch(t){ed(e,e.return,t)}e.flags&=-3}t&4096&&(e.flags&=-4097)}function wl(e){if(e.subtreeFlags&1024)for(e=e.child;e!==null;){var t=e;wl(t),t.tag===5&&t.flags&1024&&t.stateNode.reset(),e=e.sibling}}function Tl(e,t){if(t.subtreeFlags&8772)for(t=t.child;t!==null;)dl(e,t.alternate,t),t=t.sibling}function El(e){for(e=e.child;e!==null;){var t=e;switch(t.tag){case 0:case 11:case 14:case 15:Jc(4,t,t.return),El(t);break;case 1:Qc(t,t.return);var n=t.stateNode;typeof n.componentWillUnmount==`function`&&Xc(t,t.return,n),El(t);break;case 27:Cf(t.stateNode);case 26:case 5:Qc(t,t.return),El(t);break;case 22:t.memoizedState===null&&El(t);break;case 30:El(t);break;default:El(t)}e=e.sibling}}function V(e,t,n){for(n&&=(t.subtreeFlags&8772)!=0,t=t.child;t!==null;){var r=t.alternate,i=e,a=t,o=a.flags;switch(a.tag){case 0:case 11:case 15:V(i,a,n),qc(4,a);break;case 1:if(V(i,a,n),r=a,i=r.stateNode,typeof i.componentDidMount==`function`)try{i.componentDidMount()}catch(e){ed(r,r.return,e)}if(r=a,i=r.updateQueue,i!==null){var s=r.stateNode;try{var c=i.shared.hiddenCallbacks;if(c!==null)for(i.shared.hiddenCallbacks=null,i=0;i<c.length;i++)$a(c[i],s)}catch(e){ed(r,r.return,e)}}n&&o&64&&Yc(a),Zc(a,a.return);break;case 27:al(a);case 26:case 5:V(i,a,n),n&&r===null&&o&4&&$c(a),Zc(a,a.return);break;case 12:V(i,a,n);break;case 31:V(i,a,n),n&&o&4&&_l(i,a);break;case 13:V(i,a,n),n&&o&4&&vl(i,a);break;case 22:a.memoizedState===null&&V(i,a,n),Zc(a,a.return);break;case 30:break;default:V(i,a,n)}t=t.sibling}}function Dl(e,t){var n=null;e!==null&&e.memoizedState!==null&&e.memoizedState.cachePool!==null&&(n=e.memoizedState.cachePool.pool),e=null,t.memoizedState!==null&&t.memoizedState.cachePool!==null&&(e=t.memoizedState.cachePool.pool),e!==n&&(e!=null&&e.refCount++,n!=null&&fa(n))}function Ol(e,t){e=null,t.alternate!==null&&(e=t.alternate.memoizedState.cache),t=t.memoizedState.cache,t!==e&&(t.refCount++,e!=null&&fa(e))}function kl(e,t,n,r){if(t.subtreeFlags&10256)for(t=t.child;t!==null;)Al(e,t,n,r),t=t.sibling}function Al(e,t,n,r){var i=t.flags;switch(t.tag){case 0:case 11:case 15:kl(e,t,n,r),i&2048&&qc(9,t);break;case 1:kl(e,t,n,r);break;case 3:kl(e,t,n,r),i&2048&&(e=null,t.alternate!==null&&(e=t.alternate.memoizedState.cache),t=t.memoizedState.cache,t!==e&&(t.refCount++,e!=null&&fa(e)));break;case 12:if(i&2048){kl(e,t,n,r),e=t.stateNode;try{var a=t.memoizedProps,o=a.id,s=a.onPostCommit;typeof s==`function`&&s(o,t.alternate===null?`mount`:`update`,e.passiveEffectDuration,-0)}catch(e){ed(t,t.return,e)}}else kl(e,t,n,r);break;case 31:kl(e,t,n,r);break;case 13:kl(e,t,n,r);break;case 23:break;case 22:a=t.stateNode,o=t.alternate,t.memoizedState===null?a._visibility&2?kl(e,t,n,r):(a._visibility|=2,jl(e,t,n,r,(t.subtreeFlags&10256)!=0||!1)):a._visibility&2?kl(e,t,n,r):Ml(e,t),i&2048&&Dl(o,t);break;case 24:kl(e,t,n,r),i&2048&&Ol(t.alternate,t);break;default:kl(e,t,n,r)}}function jl(e,t,n,r,i){for(i&&=(t.subtreeFlags&10256)!=0||!1,t=t.child;t!==null;){var a=e,o=t,s=n,c=r,l=o.flags;switch(o.tag){case 0:case 11:case 15:jl(a,o,s,c,i),qc(8,o);break;case 23:break;case 22:var u=o.stateNode;o.memoizedState===null?(u._visibility|=2,jl(a,o,s,c,i)):u._visibility&2?jl(a,o,s,c,i):Ml(a,o),i&&l&2048&&Dl(o.alternate,o);break;case 24:jl(a,o,s,c,i),i&&l&2048&&Ol(o.alternate,o);break;default:jl(a,o,s,c,i)}t=t.sibling}}function Ml(e,t){if(t.subtreeFlags&10256)for(t=t.child;t!==null;){var n=e,r=t,i=r.flags;switch(r.tag){case 22:Ml(n,r),i&2048&&Dl(r.alternate,r);break;case 24:Ml(n,r),i&2048&&Ol(r.alternate,r);break;default:Ml(n,r)}t=t.sibling}}var Nl=8192;function Pl(e,t,n){if(e.subtreeFlags&Nl)for(e=e.child;e!==null;)Fl(e,t,n),e=e.sibling}function Fl(e,t,n){switch(e.tag){case 26:Pl(e,t,n),e.flags&Nl&&e.memoizedState!==null&&tp(n,xl,e.memoizedState,e.memoizedProps);break;case 5:Pl(e,t,n);break;case 3:case 4:var r=xl;xl=Ef(e.stateNode.containerInfo),Pl(e,t,n),xl=r;break;case 22:e.memoizedState===null&&(r=e.alternate,r!==null&&r.memoizedState!==null?(r=Nl,Nl=16777216,Pl(e,t,n),Nl=r):Pl(e,t,n));break;default:Pl(e,t,n)}}function Il(e){var t=e.alternate;if(t!==null&&(e=t.child,e!==null)){t.child=null;do t=e.sibling,e.sibling=null,e=t;while(e!==null)}}function Ll(e){var t=e.deletions;if(e.flags&16){if(t!==null)for(var n=0;n<t.length;n++){var r=t[n];ll=r,Bl(r,e)}Il(e)}if(e.subtreeFlags&10256)for(e=e.child;e!==null;)Rl(e),e=e.sibling}function Rl(e){switch(e.tag){case 0:case 11:case 15:Ll(e),e.flags&2048&&Jc(9,e,e.return);break;case 3:Ll(e);break;case 12:Ll(e);break;case 22:var t=e.stateNode;e.memoizedState!==null&&t._visibility&2&&(e.return===null||e.return.tag!==13)?(t._visibility&=-3,zl(e)):Ll(e);break;default:Ll(e)}}function zl(e){var t=e.deletions;if(e.flags&16){if(t!==null)for(var n=0;n<t.length;n++){var r=t[n];ll=r,Bl(r,e)}Il(e)}for(e=e.child;e!==null;){switch(t=e,t.tag){case 0:case 11:case 15:Jc(8,t,t.return),zl(t);break;case 22:n=t.stateNode,n._visibility&2&&(n._visibility&=-3,zl(t));break;default:zl(t)}e=e.sibling}}function Bl(e,t){for(;ll!==null;){var n=ll;switch(n.tag){case 0:case 11:case 15:Jc(8,n,t);break;case 23:case 22:if(n.memoizedState!==null&&n.memoizedState.cachePool!==null){var r=n.memoizedState.cachePool.pool;r!=null&&r.refCount++}break;case 24:fa(n.memoizedState.cache)}if(r=n.child,r!==null)r.return=n,ll=r;else a:for(n=e;ll!==null;){r=ll;var i=r.sibling,a=r.return;if(fl(r),r===n){ll=null;break a}if(i!==null){i.return=a,ll=i;break a}ll=a}}}var Vl={getCacheForType:function(e){var t=ia(ua),n=t.data.get(e);return n===void 0&&(n=e(),t.data.set(e,n)),n},cacheSignal:function(){return ia(ua).controller.signal}},Hl=typeof WeakMap==`function`?WeakMap:Map,Ul=0,Wl=null,Gl=null,Kl=0,ql=0,Jl=null,Yl=!1,Xl=!1,Zl=!1,Ql=0,$l=0,eu=0,tu=0,nu=0,ru=0,iu=0,au=null,ou=null,su=!1,cu=0,lu=0,uu=1/0,du=null,fu=null,pu=0,mu=null,hu=null,gu=0,_u=0,vu=null,yu=null,bu=0,xu=null;function Su(){return Ul&2&&Kl!==0?Kl&-Kl:P.T===null?ot():xd()}function Cu(){if(ru===0)if(!(Kl&536870912)||Ri){var e=Ge;Ge<<=1,!(Ge&3932160)&&(Ge=262144),ru=e}else ru=536870912;return e=oo.current,e!==null&&(e.flags|=32),ru}function wu(e,t,n){(e===Wl&&(ql===2||ql===9)||e.cancelPendingCommit!==null)&&(ju(e,0),Ou(e,Kl,ru,!1)),$e(e,n),(!(Ul&2)||e!==Wl)&&(e===Wl&&(!(Ul&2)&&(tu|=n),$l===4&&Ou(e,Kl,ru,!1)),pd(e))}function Tu(e,t,n){if(Ul&6)throw Error(i(327));var r=!n&&(t&127)==0&&(t&e.expiredLanes)===0||Ye(e,t),a=r?zu(e,t):Lu(e,t,!0),o=r;do{if(a===0){Xl&&!r&&Ou(e,t,0,!1);break}else{if(n=e.current.alternate,o&&!Du(n)){a=Lu(e,t,!1),o=!1;continue}if(a===2){if(o=t,e.errorRecoveryDisabledLanes&o)var s=0;else s=e.pendingLanes&-536870913,s=s===0?s&536870912?536870912:0:s;if(s!==0){t=s;a:{var c=e;a=au;var l=c.current.memoizedState.isDehydrated;if(l&&(ju(c,s).flags|=256),s=Lu(c,s,!1),s!==2){if(Zl&&!l){c.errorRecoveryDisabledLanes|=o,tu|=o,a=4;break a}o=ou,ou=a,o!==null&&(ou===null?ou=o:ou.push.apply(ou,o))}a=s}if(o=!1,a!==2)continue}}if(a===1){ju(e,0),Ou(e,t,0,!0);break}a:{switch(r=e,o=a,o){case 0:case 1:throw Error(i(345));case 4:if((t&4194048)!==t)break;case 6:Ou(r,t,ru,!Yl);break a;case 2:ou=null;break;case 3:case 5:break;default:throw Error(i(329))}if((t&62914560)===t&&(a=cu+300-Oe(),10<a)){if(Ou(r,t,ru,!Yl),Je(r,0,!0)!==0)break a;gu=t,r.timeoutHandle=rf(Eu.bind(null,r,n,ou,du,su,t,ru,tu,iu,Yl,o,`Throttled`,-0,0),a);break a}Eu(r,n,ou,du,su,t,ru,tu,iu,Yl,o,null,-0,0)}}break}while(1);pd(e)}function Eu(e,t,n,r,i,a,o,s,c,l,u,d,f,p){if(e.timeoutHandle=-1,d=t.subtreeFlags,d&8192||(d&16785408)==16785408){d={stylesheets:null,count:0,imgCount:0,imgBytes:0,suspenseyImages:[],waitingForImages:!0,waitingForViewTransition:!1,unsuspend:nn},Fl(t,a,d);var m=(a&62914560)===a?cu-Oe():(a&4194048)===a?lu-Oe():0;if(m=rp(d,m),m!==null){gu=a,e.cancelPendingCommit=m(Ku.bind(null,e,t,a,n,r,i,o,s,c,u,d,null,f,p)),Ou(e,a,o,!l);return}}Ku(e,t,a,n,r,i,o,s,c)}function Du(e){for(var t=e;;){var n=t.tag;if((n===0||n===11||n===15)&&t.flags&16384&&(n=t.updateQueue,n!==null&&(n=n.stores,n!==null)))for(var r=0;r<n.length;r++){var i=n[r],a=i.getSnapshot;i=i.value;try{if(!Tr(a(),i))return!1}catch{return!1}}if(n=t.child,t.subtreeFlags&16384&&n!==null)n.return=t,t=n;else{if(t===e)break;for(;t.sibling===null;){if(t.return===null||t.return===e)return!0;t=t.return}t.sibling.return=t.return,t=t.sibling}}return!0}function Ou(e,t,n,r){t&=~nu,t&=~tu,e.suspendedLanes|=t,e.pingedLanes&=~t,r&&(e.warmLanes|=t),r=e.expirationTimes;for(var i=t;0<i;){var a=31-Be(i),o=1<<a;r[a]=-1,i&=~o}n!==0&&tt(e,n,t)}function ku(){return Ul&6?!0:(md(0,!1),!1)}function Au(){if(Gl!==null){if(ql===0)var e=Gl.return;else e=Gl,Xi=Yi=null,Po(e),Fa=null,Ia=0,e=Gl;for(;e!==null;)Kc(e.alternate,e),e=e.return;Gl=null}}function ju(e,t){var n=e.timeoutHandle;n!==-1&&(e.timeoutHandle=-1,af(n)),n=e.cancelPendingCommit,n!==null&&(e.cancelPendingCommit=null,n()),gu=0,Au(),Wl=e,Gl=n=pi(e.current,null),Kl=t,ql=0,Jl=null,Yl=!1,Xl=Ye(e,t),Zl=!1,iu=ru=nu=tu=eu=$l=0,ou=au=null,su=!1,t&8&&(t|=t&32);var r=e.entangledLanes;if(r!==0)for(e=e.entanglements,r&=t;0<r;){var i=31-Be(r),a=1<<i;t|=e[i],r&=~a}return Ql=t,ri(),n}function Mu(e,t){L=null,P.H=Us,t===Ta||t===Da?(t=Na(),ql=3):t===Ea?(t=Na(),ql=4):ql=t===sc?8:typeof t==`object`&&t&&typeof t.then==`function`?6:1,Jl=t,Gl===null&&($l=1,tc(e,xi(t,e.current)))}function Nu(){var e=oo.current;return e===null?!0:(Kl&4194048)===Kl?so===null:(Kl&62914560)===Kl||Kl&536870912?e===so:!1}function Pu(){var e=P.H;return P.H=Us,e===null?Us:e}function Fu(){var e=P.A;return P.A=Vl,e}function Iu(){$l=4,Yl||(Kl&4194048)!==Kl&&oo.current!==null||(Xl=!0),!(eu&134217727)&&!(tu&134217727)||Wl===null||Ou(Wl,Kl,ru,!1)}function Lu(e,t,n){var r=Ul;Ul|=2;var i=Pu(),a=Fu();(Wl!==e||Kl!==t)&&(du=null,ju(e,t)),t=!1;var o=$l;a:do try{if(ql!==0&&Gl!==null){var s=Gl,c=Jl;switch(ql){case 8:Au(),o=6;break a;case 3:case 2:case 9:case 6:oo.current===null&&(t=!0);var l=ql;if(ql=0,Jl=null,Uu(e,s,c,l),n&&Xl){o=0;break a}break;default:l=ql,ql=0,Jl=null,Uu(e,s,c,l)}}Ru(),o=$l;break}catch(t){Mu(e,t)}while(1);return t&&e.shellSuspendCounter++,Xi=Yi=null,Ul=r,P.H=i,P.A=a,Gl===null&&(Wl=null,Kl=0,ri()),o}function Ru(){for(;Gl!==null;)Vu(Gl)}function zu(e,t){var n=Ul;Ul|=2;var r=Pu(),a=Fu();Wl!==e||Kl!==t?(du=null,uu=Oe()+500,ju(e,t)):Xl=Ye(e,t);a:do try{if(ql!==0&&Gl!==null){t=Gl;var o=Jl;b:switch(ql){case 1:ql=0,Jl=null,Uu(e,t,o,1);break;case 2:case 9:if(ka(o)){ql=0,Jl=null,Hu(t);break}t=function(){ql!==2&&ql!==9||Wl!==e||(ql=7),pd(e)},o.then(t,t);break a;case 3:ql=7;break a;case 4:ql=5;break a;case 7:ka(o)?(ql=0,Jl=null,Hu(t)):(ql=0,Jl=null,Uu(e,t,o,7));break;case 5:var s=null;switch(Gl.tag){case 26:s=Gl.memoizedState;case 5:case 27:var c=Gl;if(s?ep(s):c.stateNode.complete){ql=0,Jl=null;var l=c.sibling;if(l!==null)Gl=l;else{var u=c.return;u===null?Gl=null:(Gl=u,Wu(u))}break b}}ql=0,Jl=null,Uu(e,t,o,5);break;case 6:ql=0,Jl=null,Uu(e,t,o,6);break;case 8:Au(),$l=6;break a;default:throw Error(i(462))}}Bu();break}catch(t){Mu(e,t)}while(1);return Xi=Yi=null,P.H=r,P.A=a,Ul=n,Gl===null?(Wl=null,Kl=0,ri(),$l):0}function Bu(){for(;Gl!==null&&!Ee();)Vu(Gl)}function Vu(e){var t=Lc(e.alternate,e,Ql);e.memoizedProps=e.pendingProps,t===null?Wu(e):Gl=t}function Hu(e){var t=e,n=t.alternate;switch(t.tag){case 15:case 0:t=xc(n,t,t.pendingProps,t.type,void 0,Kl);break;case 11:t=xc(n,t,t.pendingProps,t.type.render,t.ref,Kl);break;case 5:Po(t);default:Kc(n,t),t=Gl=mi(t,Ql),t=Lc(n,t,Ql)}e.memoizedProps=e.pendingProps,t===null?Wu(e):Gl=t}function Uu(e,t,n,r){Xi=Yi=null,Po(t),Fa=null,Ia=0;var i=t.return;try{if(oc(e,i,t,n,Kl)){$l=1,tc(e,xi(n,e.current)),Gl=null;return}}catch(t){if(i!==null)throw Gl=i,t;$l=1,tc(e,xi(n,e.current)),Gl=null;return}t.flags&32768?(Ri||r===1?e=!0:Xl||Kl&536870912?e=!1:(Yl=e=!0,(r===2||r===9||r===3||r===6)&&(r=oo.current,r!==null&&r.tag===13&&(r.flags|=16384))),Gu(t,e)):Wu(t)}function Wu(e){var t=e;do{if(t.flags&32768){Gu(t,Yl);return}e=t.return;var n=Wc(t.alternate,t,Ql);if(n!==null){Gl=n;return}if(t=t.sibling,t!==null){Gl=t;return}Gl=t=e}while(t!==null);$l===0&&($l=5)}function Gu(e,t){do{var n=Gc(e.alternate,e);if(n!==null){n.flags&=32767,Gl=n;return}if(n=e.return,n!==null&&(n.flags|=32768,n.subtreeFlags=0,n.deletions=null),!t&&(e=e.sibling,e!==null)){Gl=e;return}Gl=e=n}while(e!==null);$l=6,Gl=null}function Ku(e,t,n,r,a,o,s,c,l){e.cancelPendingCommit=null;do Zu();while(pu!==0);if(Ul&6)throw Error(i(327));if(t!==null){if(t===e.current)throw Error(i(177));if(o=t.lanes|t.childLanes,o|=ni,et(e,n,o,s,c,l),e===Wl&&(Gl=Wl=null,Kl=0),hu=t,mu=e,gu=n,_u=o,vu=a,yu=r,t.subtreeFlags&10256||t.flags&10256?(e.callbackNode=null,e.callbackPriority=0,od(Me,function(){return Qu(),null})):(e.callbackNode=null,e.callbackPriority=0),r=(t.flags&13878)!=0,t.subtreeFlags&13878||r){r=P.T,P.T=null,a=F.p,F.p=2,s=Ul,Ul|=4;try{ul(e,t,n)}finally{Ul=s,F.p=a,P.T=r}}pu=1,qu(),Ju(),Yu()}}function qu(){if(pu===1){pu=0;var e=mu,t=hu,n=(t.flags&13878)!=0;if(t.subtreeFlags&13878||n){n=P.T,P.T=null;var r=F.p;F.p=2;var i=Ul;Ul|=4;try{Sl(t,e);var a=Xd,o=Ar(e.containerInfo),s=a.focusedElem,c=a.selectionRange;if(o!==s&&s&&s.ownerDocument&&kr(s.ownerDocument.documentElement,s)){if(c!==null&&jr(s)){var l=c.start,u=c.end;if(u===void 0&&(u=l),`selectionStart`in s)s.selectionStart=l,s.selectionEnd=Math.min(u,s.value.length);else{var d=s.ownerDocument||document,f=d&&d.defaultView||window;if(f.getSelection){var p=f.getSelection(),m=s.textContent.length,h=Math.min(c.start,m),g=c.end===void 0?h:Math.min(c.end,m);!p.extend&&h>g&&(o=g,g=h,h=o);var _=Or(s,h),v=Or(s,g);if(_&&v&&(p.rangeCount!==1||p.anchorNode!==_.node||p.anchorOffset!==_.offset||p.focusNode!==v.node||p.focusOffset!==v.offset)){var y=d.createRange();y.setStart(_.node,_.offset),p.removeAllRanges(),h>g?(p.addRange(y),p.extend(v.node,v.offset)):(y.setEnd(v.node,v.offset),p.addRange(y))}}}}for(d=[],p=s;p=p.parentNode;)p.nodeType===1&&d.push({element:p,left:p.scrollLeft,top:p.scrollTop});for(typeof s.focus==`function`&&s.focus(),s=0;s<d.length;s++){var b=d[s];b.element.scrollLeft=b.left,b.element.scrollTop=b.top}}_p=!!Yd,Xd=Yd=null}finally{Ul=i,F.p=r,P.T=n}}e.current=t,pu=2}}function Ju(){if(pu===2){pu=0;var e=mu,t=hu,n=(t.flags&8772)!=0;if(t.subtreeFlags&8772||n){n=P.T,P.T=null;var r=F.p;F.p=2;var i=Ul;Ul|=4;try{dl(e,t.alternate,t)}finally{Ul=i,F.p=r,P.T=n}}pu=3}}function Yu(){if(pu===4||pu===3){pu=0,De();var e=mu,t=hu,n=gu,r=yu;t.subtreeFlags&10256||t.flags&10256?pu=5:(pu=0,hu=mu=null,Xu(e,e.pendingLanes));var i=e.pendingLanes;if(i===0&&(fu=null),at(n),t=t.stateNode,Re&&typeof Re.onCommitFiberRoot==`function`)try{Re.onCommitFiberRoot(Le,t,void 0,(t.current.flags&128)==128)}catch{}if(r!==null){t=P.T,i=F.p,F.p=2,P.T=null;try{for(var a=e.onRecoverableError,o=0;o<r.length;o++){var s=r[o];a(s.value,{componentStack:s.stack})}}finally{P.T=t,F.p=i}}gu&3&&Zu(),pd(e),i=e.pendingLanes,n&261930&&i&42?e===xu?bu++:(bu=0,xu=e):bu=0,md(0,!1)}}function Xu(e,t){(e.pooledCacheLanes&=t)===0&&(t=e.pooledCache,t!=null&&(e.pooledCache=null,fa(t)))}function Zu(){return qu(),Ju(),Yu(),Qu()}function Qu(){if(pu!==5)return!1;var e=mu,t=_u;_u=0;var n=at(gu),r=P.T,a=F.p;try{F.p=32>n?32:n,P.T=null,n=vu,vu=null;var o=mu,s=gu;if(pu=0,hu=mu=null,gu=0,Ul&6)throw Error(i(331));var c=Ul;if(Ul|=4,Rl(o.current),Al(o,o.current,s,n),Ul=c,md(0,!1),Re&&typeof Re.onPostCommitFiberRoot==`function`)try{Re.onPostCommitFiberRoot(Le,o)}catch{}return!0}finally{F.p=a,P.T=r,Xu(e,t)}}function $u(e,t,n){t=xi(n,t),t=rc(e.stateNode,t,2),e=qa(e,t,2),e!==null&&($e(e,2),pd(e))}function ed(e,t,n){if(e.tag===3)$u(e,e,n);else for(;t!==null;){if(t.tag===3){$u(t,e,n);break}else if(t.tag===1){var r=t.stateNode;if(typeof t.type.getDerivedStateFromError==`function`||typeof r.componentDidCatch==`function`&&(fu===null||!fu.has(r))){e=xi(n,e),n=ic(2),r=qa(t,n,2),r!==null&&(ac(n,r,t,e),$e(r,2),pd(r));break}}t=t.return}}function td(e,t,n){var r=e.pingCache;if(r===null){r=e.pingCache=new Hl;var i=new Set;r.set(t,i)}else i=r.get(t),i===void 0&&(i=new Set,r.set(t,i));i.has(n)||(Zl=!0,i.add(n),e=nd.bind(null,e,t,n),t.then(e,e))}function nd(e,t,n){var r=e.pingCache;r!==null&&r.delete(t),e.pingedLanes|=e.suspendedLanes&n,e.warmLanes&=~n,Wl===e&&(Kl&n)===n&&($l===4||$l===3&&(Kl&62914560)===Kl&&300>Oe()-cu?!(Ul&2)&&ju(e,0):nu|=n,iu===Kl&&(iu=0)),pd(e)}function rd(e,t){t===0&&(t=Ze()),e=oi(e,t),e!==null&&($e(e,t),pd(e))}function id(e){var t=e.memoizedState,n=0;t!==null&&(n=t.retryLane),rd(e,n)}function ad(e,t){var n=0;switch(e.tag){case 31:case 13:var r=e.stateNode,a=e.memoizedState;a!==null&&(n=a.retryLane);break;case 19:r=e.stateNode;break;case 22:r=e.stateNode._retryCache;break;default:throw Error(i(314))}r!==null&&r.delete(t),rd(e,n)}function od(e,t){return we(e,t)}var sd=null,cd=null,ld=!1,ud=!1,dd=!1,fd=0;function pd(e){e!==cd&&e.next===null&&(cd===null?sd=cd=e:cd=cd.next=e),ud=!0,ld||(ld=!0,bd())}function md(e,t){if(!dd&&ud){dd=!0;do for(var n=!1,r=sd;r!==null;){if(!t)if(e!==0){var i=r.pendingLanes;if(i===0)var a=0;else{var o=r.suspendedLanes,s=r.pingedLanes;a=(1<<31-Be(42|e)+1)-1,a&=i&~(o&~s),a=a&201326741?a&201326741|1:a?a|2:0}a!==0&&(n=!0,yd(r,a))}else a=Kl,a=Je(r,r===Wl?a:0,r.cancelPendingCommit!==null||r.timeoutHandle!==-1),!(a&3)||Ye(r,a)||(n=!0,yd(r,a));r=r.next}while(n);dd=!1}}function hd(){gd()}function gd(){ud=ld=!1;var e=0;fd!==0&&nf()&&(e=fd);for(var t=Oe(),n=null,r=sd;r!==null;){var i=r.next,a=_d(r,t);a===0?(r.next=null,n===null?sd=i:n.next=i,i===null&&(cd=n)):(n=r,(e!==0||a&3)&&(ud=!0)),r=i}pu!==0&&pu!==5||md(e,!1),fd!==0&&(fd=0)}function _d(e,t){for(var n=e.suspendedLanes,r=e.pingedLanes,i=e.expirationTimes,a=e.pendingLanes&-62914561;0<a;){var o=31-Be(a),s=1<<o,c=i[o];c===-1?((s&n)===0||(s&r)!==0)&&(i[o]=Xe(s,t)):c<=t&&(e.expiredLanes|=s),a&=~s}if(t=Wl,n=Kl,n=Je(e,e===t?n:0,e.cancelPendingCommit!==null||e.timeoutHandle!==-1),r=e.callbackNode,n===0||e===t&&(ql===2||ql===9)||e.cancelPendingCommit!==null)return r!==null&&r!==null&&Te(r),e.callbackNode=null,e.callbackPriority=0;if(!(n&3)||Ye(e,n)){if(t=n&-n,t===e.callbackPriority)return t;switch(r!==null&&Te(r),at(n)){case 2:case 8:n=je;break;case 32:n=Me;break;case 268435456:n=Pe;break;default:n=Me}return r=vd.bind(null,e),n=we(n,r),e.callbackPriority=t,e.callbackNode=n,t}return r!==null&&r!==null&&Te(r),e.callbackPriority=2,e.callbackNode=null,2}function vd(e,t){if(pu!==0&&pu!==5)return e.callbackNode=null,e.callbackPriority=0,null;var n=e.callbackNode;if(Zu()&&e.callbackNode!==n)return null;var r=Kl;return r=Je(e,e===Wl?r:0,e.cancelPendingCommit!==null||e.timeoutHandle!==-1),r===0?null:(Tu(e,r,t),_d(e,Oe()),e.callbackNode!=null&&e.callbackNode===n?vd.bind(null,e):null)}function yd(e,t){if(Zu())return null;Tu(e,t,!0)}function bd(){sf(function(){Ul&6?we(Ae,hd):gd()})}function xd(){if(fd===0){var e=ha;e===0&&(e=We,We<<=1,!(We&261888)&&(We=256)),fd=e}return fd}function Sd(e){return e==null||typeof e==`symbol`||typeof e==`boolean`?null:typeof e==`function`?e:tn(``+e)}function Cd(e,t){var n=t.ownerDocument.createElement(`input`);return n.name=t.name,n.value=t.value,e.id&&n.setAttribute(`form`,e.id),t.parentNode.insertBefore(n,t),e=new FormData(e),n.parentNode.removeChild(n),e}function wd(e,t,n,r,i){if(t===`submit`&&n&&n.stateNode===i){var a=Sd((i[ut]||null).action),o=r.submitter;o&&(t=(t=o[ut]||null)?Sd(t.formAction):o.getAttribute(`formAction`),t!==null&&(a=t,o=null));var s=new wn(`action`,`action`,null,r,i);e.push({event:s,listeners:[{instance:null,listener:function(){if(r.defaultPrevented){if(fd!==0){var e=o?Cd(i,o):new FormData(i);ks(n,{pending:!0,data:e,method:i.method,action:a},null,e)}}else typeof a==`function`&&(s.preventDefault(),e=o?Cd(i,o):new FormData(i),ks(n,{pending:!0,data:e,method:i.method,action:a},a,e))},currentTarget:i}]})}}for(var Td=0;Td<Zr.length;Td++){var Ed=Zr[Td];Qr(Ed.toLowerCase(),`on`+(Ed[0].toUpperCase()+Ed.slice(1)))}Qr(Ur,`onAnimationEnd`),Qr(Wr,`onAnimationIteration`),Qr(Gr,`onAnimationStart`),Qr(`dblclick`,`onDoubleClick`),Qr(`focusin`,`onFocus`),Qr(`focusout`,`onBlur`),Qr(Kr,`onTransitionRun`),Qr(qr,`onTransitionStart`),Qr(Jr,`onTransitionCancel`),Qr(Yr,`onTransitionEnd`),Et(`onMouseEnter`,[`mouseout`,`mouseover`]),Et(`onMouseLeave`,[`mouseout`,`mouseover`]),Et(`onPointerEnter`,[`pointerout`,`pointerover`]),Et(`onPointerLeave`,[`pointerout`,`pointerover`]),Tt(`onChange`,`change click focusin focusout input keydown keyup selectionchange`.split(` `)),Tt(`onSelect`,`focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange`.split(` `)),Tt(`onBeforeInput`,[`compositionend`,`keypress`,`textInput`,`paste`]),Tt(`onCompositionEnd`,`compositionend focusout keydown keypress keyup mousedown`.split(` `)),Tt(`onCompositionStart`,`compositionstart focusout keydown keypress keyup mousedown`.split(` `)),Tt(`onCompositionUpdate`,`compositionupdate focusout keydown keypress keyup mousedown`.split(` `));var Dd=`abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting`.split(` `),Od=new Set(`beforetoggle cancel close invalid load scroll scrollend toggle`.split(` `).concat(Dd));function kd(e,t){t=(t&4)!=0;for(var n=0;n<e.length;n++){var r=e[n],i=r.event;r=r.listeners;a:{var a=void 0;if(t)for(var o=r.length-1;0<=o;o--){var s=r[o],c=s.instance,l=s.currentTarget;if(s=s.listener,c!==a&&i.isPropagationStopped())break a;a=s,i.currentTarget=l;try{a(i)}catch(e){$r(e)}i.currentTarget=null,a=c}else for(o=0;o<r.length;o++){if(s=r[o],c=s.instance,l=s.currentTarget,s=s.listener,c!==a&&i.isPropagationStopped())break a;a=s,i.currentTarget=l;try{a(i)}catch(e){$r(e)}i.currentTarget=null,a=c}}}}function H(e,t){var n=t[ft];n===void 0&&(n=t[ft]=new Set);var r=e+`__bubble`;n.has(r)||(Nd(t,e,2,!1),n.add(r))}function Ad(e,t,n){var r=0;t&&(r|=4),Nd(n,e,r,t)}var jd=`_reactListening`+Math.random().toString(36).slice(2);function Md(e){if(!e[jd]){e[jd]=!0,Ct.forEach(function(t){t!==`selectionchange`&&(Od.has(t)||Ad(t,!1,e),Ad(t,!0,e))});var t=e.nodeType===9?e:e.ownerDocument;t===null||t[jd]||(t[jd]=!0,Ad(`selectionchange`,!1,t))}}function Nd(e,t,n,r){switch(wp(t)){case 2:var i=vp;break;case 8:i=yp;break;default:i=bp}n=i.bind(null,t,n,e),i=void 0,!pn||t!==`touchstart`&&t!==`touchmove`&&t!==`wheel`||(i=!0),r?i===void 0?e.addEventListener(t,n,!0):e.addEventListener(t,n,{capture:!0,passive:i}):i===void 0?e.addEventListener(t,n,!1):e.addEventListener(t,n,{passive:i})}function Pd(e,t,n,r,i){var a=r;if(!(t&1)&&!(t&2)&&r!==null)a:for(;;){if(r===null)return;var s=r.tag;if(s===3||s===4){var c=r.stateNode.containerInfo;if(c===i)break;if(s===4)for(s=r.return;s!==null;){var l=s.tag;if((l===3||l===4)&&s.stateNode.containerInfo===i)return;s=s.return}for(;c!==null;){if(s=vt(c),s===null)return;if(l=s.tag,l===5||l===6||l===26||l===27){r=a=s;continue a}c=c.parentNode}}r=r.return}un(function(){var r=a,i=an(n),s=[];a:{var c=Xr.get(e);if(c!==void 0){var l=wn,u=e;switch(e){case`keypress`:if(yn(n)===0)break a;case`keydown`:case`keyup`:l=Hn;break;case`focusin`:u=`focus`,l=Nn;break;case`focusout`:u=`blur`,l=Nn;break;case`beforeblur`:case`afterblur`:l=Nn;break;case`click`:if(n.button===2)break a;case`auxclick`:case`dblclick`:case`mousedown`:case`mousemove`:case`mouseup`:case`mouseout`:case`mouseover`:case`contextmenu`:l=jn;break;case`drag`:case`dragend`:case`dragenter`:case`dragexit`:case`dragleave`:case`dragover`:case`dragstart`:case`drop`:l=Mn;break;case`touchcancel`:case`touchend`:case`touchmove`:case`touchstart`:l=Wn;break;case Ur:case Wr:case Gr:l=Pn;break;case Yr:l=Gn;break;case`scroll`:case`scrollend`:l=En;break;case`wheel`:l=Kn;break;case`copy`:case`cut`:case`paste`:l=Fn;break;case`gotpointercapture`:case`lostpointercapture`:case`pointercancel`:case`pointerdown`:case`pointermove`:case`pointerout`:case`pointerover`:case`pointerup`:l=Un;break;case`toggle`:case`beforetoggle`:l=qn}var d=(t&4)!=0,f=!d&&(e===`scroll`||e===`scrollend`),p=d?c===null?null:c+`Capture`:c;d=[];for(var m=r,h;m!==null;){var g=m;if(h=g.stateNode,g=g.tag,g!==5&&g!==26&&g!==27||h===null||p===null||(g=dn(m,p),g!=null&&d.push(Fd(m,g,h))),f)break;m=m.return}0<d.length&&(c=new l(c,u,null,n,i),s.push({event:c,listeners:d}))}}if(!(t&7)){a:{if(c=e===`mouseover`||e===`pointerover`,l=e===`mouseout`||e===`pointerout`,c&&n!==rn&&(u=n.relatedTarget||n.fromElement)&&(vt(u)||u[dt]))break a;if((l||c)&&(c=i.window===i?i:(c=i.ownerDocument)?c.defaultView||c.parentWindow:window,l?(u=n.relatedTarget||n.toElement,l=r,u=u?vt(u):null,u!==null&&(f=o(u),d=u.tag,u!==f||d!==5&&d!==27&&d!==6)&&(u=null)):(l=null,u=r),l!==u)){if(d=jn,g=`onMouseLeave`,p=`onMouseEnter`,m=`mouse`,(e===`pointerout`||e===`pointerover`)&&(d=Un,g=`onPointerLeave`,p=`onPointerEnter`,m=`pointer`),f=l==null?c:bt(l),h=u==null?c:bt(u),c=new d(g,m+`leave`,l,n,i),c.target=f,c.relatedTarget=h,g=null,vt(i)===r&&(d=new d(p,m+`enter`,u,n,i),d.target=h,d.relatedTarget=f,g=d),f=g,l&&u)b:{for(d=Ld,p=l,m=u,h=0,g=p;g;g=d(g))h++;g=0;for(var _=m;_;_=d(_))g++;for(;0<h-g;)p=d(p),h--;for(;0<g-h;)m=d(m),g--;for(;h--;){if(p===m||m!==null&&p===m.alternate){d=p;break b}p=d(p),m=d(m)}d=null}else d=null;l!==null&&Rd(s,c,l,d,!1),u!==null&&f!==null&&Rd(s,f,u,d,!0)}}a:{if(c=r?bt(r):window,l=c.nodeName&&c.nodeName.toLowerCase(),l===`select`||l===`input`&&c.type===`file`)var v=pr;else if(sr(c))if(mr)v=Cr;else{v=xr;var y=br}else l=c.nodeName,!l||l.toLowerCase()!==`input`||c.type!==`checkbox`&&c.type!==`radio`?r&&Qt(r.elementType)&&(v=pr):v=Sr;if(v&&=v(e,r)){cr(s,v,n,i);break a}y&&y(e,c,r),e===`focusout`&&r&&c.type===`number`&&r.memoizedProps.value!=null&&Wt(c,`number`,c.value)}switch(y=r?bt(r):window,e){case`focusin`:(sr(y)||y.contentEditable===`true`)&&(Nr=y,Pr=r,Fr=null);break;case`focusout`:Fr=Pr=Nr=null;break;case`mousedown`:Ir=!0;break;case`contextmenu`:case`mouseup`:case`dragend`:Ir=!1,Lr(s,n,i);break;case`selectionchange`:if(Mr)break;case`keydown`:case`keyup`:Lr(s,n,i)}var b;if(Yn)b:{switch(e){case`compositionstart`:var x=`onCompositionStart`;break b;case`compositionend`:x=`onCompositionEnd`;break b;case`compositionupdate`:x=`onCompositionUpdate`;break b}x=void 0}else rr?tr(e,n)&&(x=`onCompositionEnd`):e===`keydown`&&n.keyCode===229&&(x=`onCompositionStart`);x&&(Qn&&n.locale!==`ko`&&(rr||x!==`onCompositionStart`?x===`onCompositionEnd`&&rr&&(b=vn()):(hn=i,gn=`value`in hn?hn.value:hn.textContent,rr=!0)),y=Id(r,x),0<y.length&&(x=new In(x,e,null,n,i),s.push({event:x,listeners:y}),b?x.data=b:(b=nr(n),b!==null&&(x.data=b)))),(b=Zn?ir(e,n):ar(e,n))&&(x=Id(r,`onBeforeInput`),0<x.length&&(y=new In(`onBeforeInput`,`beforeinput`,null,n,i),s.push({event:y,listeners:x}),y.data=b)),wd(s,e,r,n,i)}kd(s,t)})}function Fd(e,t,n){return{instance:e,listener:t,currentTarget:n}}function Id(e,t){for(var n=t+`Capture`,r=[];e!==null;){var i=e,a=i.stateNode;if(i=i.tag,i!==5&&i!==26&&i!==27||a===null||(i=dn(e,n),i!=null&&r.unshift(Fd(e,i,a)),i=dn(e,t),i!=null&&r.push(Fd(e,i,a))),e.tag===3)return r;e=e.return}return[]}function Ld(e){if(e===null)return null;do e=e.return;while(e&&e.tag!==5&&e.tag!==27);return e||null}function Rd(e,t,n,r,i){for(var a=t._reactName,o=[];n!==null&&n!==r;){var s=n,c=s.alternate,l=s.stateNode;if(s=s.tag,c!==null&&c===r)break;s!==5&&s!==26&&s!==27||l===null||(c=l,i?(l=dn(n,a),l!=null&&o.unshift(Fd(n,l,c))):i||(l=dn(n,a),l!=null&&o.push(Fd(n,l,c)))),n=n.return}o.length!==0&&e.push({event:t,listeners:o})}var zd=/\r\n?/g,Bd=/\u0000|\uFFFD/g;function Vd(e){return(typeof e==`string`?e:``+e).replace(zd,`
`).replace(Bd,``)}function Hd(e,t){return t=Vd(t),Vd(e)===t}function Ud(e,t,n,r,a,o){switch(n){case`children`:typeof r==`string`?t===`body`||t===`textarea`&&r===``||Jt(e,r):(typeof r==`number`||typeof r==`bigint`)&&t!==`body`&&Jt(e,``+r);break;case`className`:Mt(e,`class`,r);break;case`tabIndex`:Mt(e,`tabindex`,r);break;case`dir`:case`role`:case`viewBox`:case`width`:case`height`:Mt(e,n,r);break;case`style`:Zt(e,r,o);break;case`data`:if(t!==`object`){Mt(e,`data`,r);break}case`src`:case`href`:if(r===``&&(t!==`a`||n!==`href`)){e.removeAttribute(n);break}if(r==null||typeof r==`function`||typeof r==`symbol`||typeof r==`boolean`){e.removeAttribute(n);break}r=tn(``+r),e.setAttribute(n,r);break;case`action`:case`formAction`:if(typeof r==`function`){e.setAttribute(n,`javascript:throw new Error('A React form was unexpectedly submitted. If you called form.submit() manually, consider using form.requestSubmit() instead. If you\\'re trying to use event.stopPropagation() in a submit event handler, consider also calling event.preventDefault().')`);break}else typeof o==`function`&&(n===`formAction`?(t!==`input`&&Ud(e,t,`name`,a.name,a,null),Ud(e,t,`formEncType`,a.formEncType,a,null),Ud(e,t,`formMethod`,a.formMethod,a,null),Ud(e,t,`formTarget`,a.formTarget,a,null)):(Ud(e,t,`encType`,a.encType,a,null),Ud(e,t,`method`,a.method,a,null),Ud(e,t,`target`,a.target,a,null)));if(r==null||typeof r==`symbol`||typeof r==`boolean`){e.removeAttribute(n);break}r=tn(``+r),e.setAttribute(n,r);break;case`onClick`:r!=null&&(e.onclick=nn);break;case`onScroll`:r!=null&&H(`scroll`,e);break;case`onScrollEnd`:r!=null&&H(`scrollend`,e);break;case`dangerouslySetInnerHTML`:if(r!=null){if(typeof r!=`object`||!(`__html`in r))throw Error(i(61));if(n=r.__html,n!=null){if(a.children!=null)throw Error(i(60));e.innerHTML=n}}break;case`multiple`:e.multiple=r&&typeof r!=`function`&&typeof r!=`symbol`;break;case`muted`:e.muted=r&&typeof r!=`function`&&typeof r!=`symbol`;break;case`suppressContentEditableWarning`:case`suppressHydrationWarning`:case`defaultValue`:case`defaultChecked`:case`innerHTML`:case`ref`:break;case`autoFocus`:break;case`xlinkHref`:if(r==null||typeof r==`function`||typeof r==`boolean`||typeof r==`symbol`){e.removeAttribute(`xlink:href`);break}n=tn(``+r),e.setAttributeNS(`http://www.w3.org/1999/xlink`,`xlink:href`,n);break;case`contentEditable`:case`spellCheck`:case`draggable`:case`value`:case`autoReverse`:case`externalResourcesRequired`:case`focusable`:case`preserveAlpha`:r!=null&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,``+r):e.removeAttribute(n);break;case`inert`:case`allowFullScreen`:case`async`:case`autoPlay`:case`controls`:case`default`:case`defer`:case`disabled`:case`disablePictureInPicture`:case`disableRemotePlayback`:case`formNoValidate`:case`hidden`:case`loop`:case`noModule`:case`noValidate`:case`open`:case`playsInline`:case`readOnly`:case`required`:case`reversed`:case`scoped`:case`seamless`:case`itemScope`:r&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,``):e.removeAttribute(n);break;case`capture`:case`download`:!0===r?e.setAttribute(n,``):!1!==r&&r!=null&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,r):e.removeAttribute(n);break;case`cols`:case`rows`:case`size`:case`span`:r!=null&&typeof r!=`function`&&typeof r!=`symbol`&&!isNaN(r)&&1<=r?e.setAttribute(n,r):e.removeAttribute(n);break;case`rowSpan`:case`start`:r==null||typeof r==`function`||typeof r==`symbol`||isNaN(r)?e.removeAttribute(n):e.setAttribute(n,r);break;case`popover`:H(`beforetoggle`,e),H(`toggle`,e),jt(e,`popover`,r);break;case`xlinkActuate`:Nt(e,`http://www.w3.org/1999/xlink`,`xlink:actuate`,r);break;case`xlinkArcrole`:Nt(e,`http://www.w3.org/1999/xlink`,`xlink:arcrole`,r);break;case`xlinkRole`:Nt(e,`http://www.w3.org/1999/xlink`,`xlink:role`,r);break;case`xlinkShow`:Nt(e,`http://www.w3.org/1999/xlink`,`xlink:show`,r);break;case`xlinkTitle`:Nt(e,`http://www.w3.org/1999/xlink`,`xlink:title`,r);break;case`xlinkType`:Nt(e,`http://www.w3.org/1999/xlink`,`xlink:type`,r);break;case`xmlBase`:Nt(e,`http://www.w3.org/XML/1998/namespace`,`xml:base`,r);break;case`xmlLang`:Nt(e,`http://www.w3.org/XML/1998/namespace`,`xml:lang`,r);break;case`xmlSpace`:Nt(e,`http://www.w3.org/XML/1998/namespace`,`xml:space`,r);break;case`is`:jt(e,`is`,r);break;case`innerText`:case`textContent`:break;default:(!(2<n.length)||n[0]!==`o`&&n[0]!==`O`||n[1]!==`n`&&n[1]!==`N`)&&(n=$t.get(n)||n,jt(e,n,r))}}function Wd(e,t,n,r,a,o){switch(n){case`style`:Zt(e,r,o);break;case`dangerouslySetInnerHTML`:if(r!=null){if(typeof r!=`object`||!(`__html`in r))throw Error(i(61));if(n=r.__html,n!=null){if(a.children!=null)throw Error(i(60));e.innerHTML=n}}break;case`children`:typeof r==`string`?Jt(e,r):(typeof r==`number`||typeof r==`bigint`)&&Jt(e,``+r);break;case`onScroll`:r!=null&&H(`scroll`,e);break;case`onScrollEnd`:r!=null&&H(`scrollend`,e);break;case`onClick`:r!=null&&(e.onclick=nn);break;case`suppressContentEditableWarning`:case`suppressHydrationWarning`:case`innerHTML`:case`ref`:break;case`innerText`:case`textContent`:break;default:if(!wt.hasOwnProperty(n))a:{if(n[0]===`o`&&n[1]===`n`&&(a=n.endsWith(`Capture`),t=n.slice(2,a?n.length-7:void 0),o=e[ut]||null,o=o==null?null:o[n],typeof o==`function`&&e.removeEventListener(t,o,a),typeof r==`function`)){typeof o!=`function`&&o!==null&&(n in e?e[n]=null:e.hasAttribute(n)&&e.removeAttribute(n)),e.addEventListener(t,r,a);break a}n in e?e[n]=r:!0===r?e.setAttribute(n,``):jt(e,n,r)}}}function Gd(e,t,n){switch(t){case`div`:case`span`:case`svg`:case`path`:case`a`:case`g`:case`p`:case`li`:break;case`img`:H(`error`,e),H(`load`,e);var r=!1,a=!1,o;for(o in n)if(n.hasOwnProperty(o)){var s=n[o];if(s!=null)switch(o){case`src`:r=!0;break;case`srcSet`:a=!0;break;case`children`:case`dangerouslySetInnerHTML`:throw Error(i(137,t));default:Ud(e,t,o,s,n,null)}}a&&Ud(e,t,`srcSet`,n.srcSet,n,null),r&&Ud(e,t,`src`,n.src,n,null);return;case`input`:H(`invalid`,e);var c=o=s=a=null,l=null,u=null;for(r in n)if(n.hasOwnProperty(r)){var d=n[r];if(d!=null)switch(r){case`name`:a=d;break;case`type`:s=d;break;case`checked`:l=d;break;case`defaultChecked`:u=d;break;case`value`:o=d;break;case`defaultValue`:c=d;break;case`children`:case`dangerouslySetInnerHTML`:if(d!=null)throw Error(i(137,t));break;default:Ud(e,t,r,d,n,null)}}Ut(e,o,c,l,u,s,a,!1);return;case`select`:for(a in H(`invalid`,e),r=s=o=null,n)if(n.hasOwnProperty(a)&&(c=n[a],c!=null))switch(a){case`value`:o=c;break;case`defaultValue`:s=c;break;case`multiple`:r=c;default:Ud(e,t,a,c,n,null)}t=o,n=s,e.multiple=!!r,t==null?n!=null&&Gt(e,!!r,n,!0):Gt(e,!!r,t,!1);return;case`textarea`:for(s in H(`invalid`,e),o=a=r=null,n)if(n.hasOwnProperty(s)&&(c=n[s],c!=null))switch(s){case`value`:r=c;break;case`defaultValue`:a=c;break;case`children`:o=c;break;case`dangerouslySetInnerHTML`:if(c!=null)throw Error(i(91));break;default:Ud(e,t,s,c,n,null)}qt(e,r,a,o);return;case`option`:for(l in n)if(n.hasOwnProperty(l)&&(r=n[l],r!=null))switch(l){case`selected`:e.selected=r&&typeof r!=`function`&&typeof r!=`symbol`;break;default:Ud(e,t,l,r,n,null)}return;case`dialog`:H(`beforetoggle`,e),H(`toggle`,e),H(`cancel`,e),H(`close`,e);break;case`iframe`:case`object`:H(`load`,e);break;case`video`:case`audio`:for(r=0;r<Dd.length;r++)H(Dd[r],e);break;case`image`:H(`error`,e),H(`load`,e);break;case`details`:H(`toggle`,e);break;case`embed`:case`source`:case`link`:H(`error`,e),H(`load`,e);case`area`:case`base`:case`br`:case`col`:case`hr`:case`keygen`:case`meta`:case`param`:case`track`:case`wbr`:case`menuitem`:for(u in n)if(n.hasOwnProperty(u)&&(r=n[u],r!=null))switch(u){case`children`:case`dangerouslySetInnerHTML`:throw Error(i(137,t));default:Ud(e,t,u,r,n,null)}return;default:if(Qt(t)){for(d in n)n.hasOwnProperty(d)&&(r=n[d],r!==void 0&&Wd(e,t,d,r,n,void 0));return}}for(c in n)n.hasOwnProperty(c)&&(r=n[c],r!=null&&Ud(e,t,c,r,n,null))}function Kd(e,t,n,r){switch(t){case`div`:case`span`:case`svg`:case`path`:case`a`:case`g`:case`p`:case`li`:break;case`input`:var a=null,o=null,s=null,c=null,l=null,u=null,d=null;for(m in n){var f=n[m];if(n.hasOwnProperty(m)&&f!=null)switch(m){case`checked`:break;case`value`:break;case`defaultValue`:l=f;default:r.hasOwnProperty(m)||Ud(e,t,m,null,r,f)}}for(var p in r){var m=r[p];if(f=n[p],r.hasOwnProperty(p)&&(m!=null||f!=null))switch(p){case`type`:o=m;break;case`name`:a=m;break;case`checked`:u=m;break;case`defaultChecked`:d=m;break;case`value`:s=m;break;case`defaultValue`:c=m;break;case`children`:case`dangerouslySetInnerHTML`:if(m!=null)throw Error(i(137,t));break;default:m!==f&&Ud(e,t,p,m,r,f)}}Ht(e,s,c,l,u,d,o,a);return;case`select`:for(o in m=s=c=p=null,n)if(l=n[o],n.hasOwnProperty(o)&&l!=null)switch(o){case`value`:break;case`multiple`:m=l;default:r.hasOwnProperty(o)||Ud(e,t,o,null,r,l)}for(a in r)if(o=r[a],l=n[a],r.hasOwnProperty(a)&&(o!=null||l!=null))switch(a){case`value`:p=o;break;case`defaultValue`:c=o;break;case`multiple`:s=o;default:o!==l&&Ud(e,t,a,o,r,l)}t=c,n=s,r=m,p==null?!!r!=!!n&&(t==null?Gt(e,!!n,n?[]:``,!1):Gt(e,!!n,t,!0)):Gt(e,!!n,p,!1);return;case`textarea`:for(c in m=p=null,n)if(a=n[c],n.hasOwnProperty(c)&&a!=null&&!r.hasOwnProperty(c))switch(c){case`value`:break;case`children`:break;default:Ud(e,t,c,null,r,a)}for(s in r)if(a=r[s],o=n[s],r.hasOwnProperty(s)&&(a!=null||o!=null))switch(s){case`value`:p=a;break;case`defaultValue`:m=a;break;case`children`:break;case`dangerouslySetInnerHTML`:if(a!=null)throw Error(i(91));break;default:a!==o&&Ud(e,t,s,a,r,o)}Kt(e,p,m);return;case`option`:for(var h in n)if(p=n[h],n.hasOwnProperty(h)&&p!=null&&!r.hasOwnProperty(h))switch(h){case`selected`:e.selected=!1;break;default:Ud(e,t,h,null,r,p)}for(l in r)if(p=r[l],m=n[l],r.hasOwnProperty(l)&&p!==m&&(p!=null||m!=null))switch(l){case`selected`:e.selected=p&&typeof p!=`function`&&typeof p!=`symbol`;break;default:Ud(e,t,l,p,r,m)}return;case`img`:case`link`:case`area`:case`base`:case`br`:case`col`:case`embed`:case`hr`:case`keygen`:case`meta`:case`param`:case`source`:case`track`:case`wbr`:case`menuitem`:for(var g in n)p=n[g],n.hasOwnProperty(g)&&p!=null&&!r.hasOwnProperty(g)&&Ud(e,t,g,null,r,p);for(u in r)if(p=r[u],m=n[u],r.hasOwnProperty(u)&&p!==m&&(p!=null||m!=null))switch(u){case`children`:case`dangerouslySetInnerHTML`:if(p!=null)throw Error(i(137,t));break;default:Ud(e,t,u,p,r,m)}return;default:if(Qt(t)){for(var _ in n)p=n[_],n.hasOwnProperty(_)&&p!==void 0&&!r.hasOwnProperty(_)&&Wd(e,t,_,void 0,r,p);for(d in r)p=r[d],m=n[d],!r.hasOwnProperty(d)||p===m||p===void 0&&m===void 0||Wd(e,t,d,p,r,m);return}}for(var v in n)p=n[v],n.hasOwnProperty(v)&&p!=null&&!r.hasOwnProperty(v)&&Ud(e,t,v,null,r,p);for(f in r)p=r[f],m=n[f],!r.hasOwnProperty(f)||p===m||p==null&&m==null||Ud(e,t,f,p,r,m)}function qd(e){switch(e){case`css`:case`script`:case`font`:case`img`:case`image`:case`input`:case`link`:return!0;default:return!1}}function Jd(){if(typeof performance.getEntriesByType==`function`){for(var e=0,t=0,n=performance.getEntriesByType(`resource`),r=0;r<n.length;r++){var i=n[r],a=i.transferSize,o=i.initiatorType,s=i.duration;if(a&&s&&qd(o)){for(o=0,s=i.responseEnd,r+=1;r<n.length;r++){var c=n[r],l=c.startTime;if(l>s)break;var u=c.transferSize,d=c.initiatorType;u&&qd(d)&&(c=c.responseEnd,o+=u*(c<s?1:(s-l)/(c-l)))}if(--r,t+=8*(a+o)/(i.duration/1e3),e++,10<e)break}}if(0<e)return t/e/1e6}return navigator.connection&&(e=navigator.connection.downlink,typeof e==`number`)?e:5}var Yd=null,Xd=null;function Zd(e){return e.nodeType===9?e:e.ownerDocument}function Qd(e){switch(e){case`http://www.w3.org/2000/svg`:return 1;case`http://www.w3.org/1998/Math/MathML`:return 2;default:return 0}}function $d(e,t){if(e===0)switch(t){case`svg`:return 1;case`math`:return 2;default:return 0}return e===1&&t===`foreignObject`?0:e}function ef(e,t){return e===`textarea`||e===`noscript`||typeof t.children==`string`||typeof t.children==`number`||typeof t.children==`bigint`||typeof t.dangerouslySetInnerHTML==`object`&&t.dangerouslySetInnerHTML!==null&&t.dangerouslySetInnerHTML.__html!=null}var tf=null;function nf(){var e=window.event;return e&&e.type===`popstate`?e===tf?!1:(tf=e,!0):(tf=null,!1)}var rf=typeof setTimeout==`function`?setTimeout:void 0,af=typeof clearTimeout==`function`?clearTimeout:void 0,of=typeof Promise==`function`?Promise:void 0,sf=typeof queueMicrotask==`function`?queueMicrotask:of===void 0?rf:function(e){return of.resolve(null).then(e).catch(cf)};function cf(e){setTimeout(function(){throw e})}function lf(e){return e===`head`}function uf(e,t){var n=t,r=0;do{var i=n.nextSibling;if(e.removeChild(n),i&&i.nodeType===8)if(n=i.data,n===`/$`||n===`/&`){if(r===0){e.removeChild(i),Up(t);return}r--}else if(n===`$`||n===`$?`||n===`$~`||n===`$!`||n===`&`)r++;else if(n===`html`)Cf(e.ownerDocument.documentElement);else if(n===`head`){n=e.ownerDocument.head,Cf(n);for(var a=n.firstChild;a;){var o=a.nextSibling,s=a.nodeName;a[gt]||s===`SCRIPT`||s===`STYLE`||s===`LINK`&&a.rel.toLowerCase()===`stylesheet`||n.removeChild(a),a=o}}else n===`body`&&Cf(e.ownerDocument.body);n=i}while(n);Up(t)}function df(e,t){var n=e;e=0;do{var r=n.nextSibling;if(n.nodeType===1?t?(n._stashedDisplay=n.style.display,n.style.display=`none`):(n.style.display=n._stashedDisplay||``,n.getAttribute(`style`)===``&&n.removeAttribute(`style`)):n.nodeType===3&&(t?(n._stashedText=n.nodeValue,n.nodeValue=``):n.nodeValue=n._stashedText||``),r&&r.nodeType===8)if(n=r.data,n===`/$`){if(e===0)break;e--}else n!==`$`&&n!==`$?`&&n!==`$~`&&n!==`$!`||e++;n=r}while(n)}function U(e){var t=e.firstChild;for(t&&t.nodeType===10&&(t=t.nextSibling);t;){var n=t;switch(t=t.nextSibling,n.nodeName){case`HTML`:case`HEAD`:case`BODY`:U(n),_t(n);continue;case`SCRIPT`:case`STYLE`:continue;case`LINK`:if(n.rel.toLowerCase()===`stylesheet`)continue}e.removeChild(n)}}function ff(e,t,n,r){for(;e.nodeType===1;){var i=n;if(e.nodeName.toLowerCase()!==t.toLowerCase()){if(!r&&(e.nodeName!==`INPUT`||e.type!==`hidden`))break}else if(r){if(!e[gt])switch(t){case`meta`:if(!e.hasAttribute(`itemprop`))break;return e;case`link`:if(a=e.getAttribute(`rel`),a===`stylesheet`&&e.hasAttribute(`data-precedence`)||a!==i.rel||e.getAttribute(`href`)!==(i.href==null||i.href===``?null:i.href)||e.getAttribute(`crossorigin`)!==(i.crossOrigin==null?null:i.crossOrigin)||e.getAttribute(`title`)!==(i.title==null?null:i.title))break;return e;case`style`:if(e.hasAttribute(`data-precedence`))break;return e;case`script`:if(a=e.getAttribute(`src`),(a!==(i.src==null?null:i.src)||e.getAttribute(`type`)!==(i.type==null?null:i.type)||e.getAttribute(`crossorigin`)!==(i.crossOrigin==null?null:i.crossOrigin))&&a&&e.hasAttribute(`async`)&&!e.hasAttribute(`itemprop`))break;return e;default:return e}}else if(t===`input`&&e.type===`hidden`){var a=i.name==null?null:``+i.name;if(i.type===`hidden`&&e.getAttribute(`name`)===a)return e}else return e;if(e=vf(e.nextSibling),e===null)break}return null}function pf(e,t,n){if(t===``)return null;for(;e.nodeType!==3;)if((e.nodeType!==1||e.nodeName!==`INPUT`||e.type!==`hidden`)&&!n||(e=vf(e.nextSibling),e===null))return null;return e}function mf(e,t){for(;e.nodeType!==8;)if((e.nodeType!==1||e.nodeName!==`INPUT`||e.type!==`hidden`)&&!t||(e=vf(e.nextSibling),e===null))return null;return e}function hf(e){return e.data===`$?`||e.data===`$~`}function gf(e){return e.data===`$!`||e.data===`$?`&&e.ownerDocument.readyState!==`loading`}function _f(e,t){var n=e.ownerDocument;if(e.data===`$~`)e._reactRetry=t;else if(e.data!==`$?`||n.readyState!==`loading`)t();else{var r=function(){t(),n.removeEventListener(`DOMContentLoaded`,r)};n.addEventListener(`DOMContentLoaded`,r),e._reactRetry=r}}function vf(e){for(;e!=null;e=e.nextSibling){var t=e.nodeType;if(t===1||t===3)break;if(t===8){if(t=e.data,t===`$`||t===`$!`||t===`$?`||t===`$~`||t===`&`||t===`F!`||t===`F`)break;if(t===`/$`||t===`/&`)return null}}return e}var yf=null;function bf(e){e=e.nextSibling;for(var t=0;e;){if(e.nodeType===8){var n=e.data;if(n===`/$`||n===`/&`){if(t===0)return vf(e.nextSibling);t--}else n!==`$`&&n!==`$!`&&n!==`$?`&&n!==`$~`&&n!==`&`||t++}e=e.nextSibling}return null}function xf(e){e=e.previousSibling;for(var t=0;e;){if(e.nodeType===8){var n=e.data;if(n===`$`||n===`$!`||n===`$?`||n===`$~`||n===`&`){if(t===0)return e;t--}else n!==`/$`&&n!==`/&`||t++}e=e.previousSibling}return null}function Sf(e,t,n){switch(t=Zd(n),e){case`html`:if(e=t.documentElement,!e)throw Error(i(452));return e;case`head`:if(e=t.head,!e)throw Error(i(453));return e;case`body`:if(e=t.body,!e)throw Error(i(454));return e;default:throw Error(i(451))}}function Cf(e){for(var t=e.attributes;t.length;)e.removeAttributeNode(t[0]);_t(e)}var wf=new Map,Tf=new Set;function Ef(e){return typeof e.getRootNode==`function`?e.getRootNode():e.nodeType===9?e:e.ownerDocument}var Df=F.d;F.d={f:Of,r:kf,D:Mf,C:Nf,L:Pf,m:Ff,X:Lf,S:If,M:Rf};function Of(){var e=Df.f(),t=ku();return e||t}function kf(e){var t=yt(e);t!==null&&t.tag===5&&t.type===`form`?js(t):Df.r(e)}var Af=typeof document>`u`?null:document;function jf(e,t,n){var r=Af;if(r&&typeof t==`string`&&t){var i=Vt(t);i=`link[rel="`+e+`"][href="`+i+`"]`,typeof n==`string`&&(i+=`[crossorigin="`+n+`"]`),Tf.has(i)||(Tf.add(i),e={rel:e,crossOrigin:n,href:t},r.querySelector(i)===null&&(t=r.createElement(`link`),Gd(t,`link`,e),St(t),r.head.appendChild(t)))}}function Mf(e){Df.D(e),jf(`dns-prefetch`,e,null)}function Nf(e,t){Df.C(e,t),jf(`preconnect`,e,t)}function Pf(e,t,n){Df.L(e,t,n);var r=Af;if(r&&e&&t){var i=`link[rel="preload"][as="`+Vt(t)+`"]`;t===`image`&&n&&n.imageSrcSet?(i+=`[imagesrcset="`+Vt(n.imageSrcSet)+`"]`,typeof n.imageSizes==`string`&&(i+=`[imagesizes="`+Vt(n.imageSizes)+`"]`)):i+=`[href="`+Vt(e)+`"]`;var a=i;switch(t){case`style`:a=Bf(e);break;case`script`:a=Wf(e)}wf.has(a)||(e=h({rel:`preload`,href:t===`image`&&n&&n.imageSrcSet?void 0:e,as:t},n),wf.set(a,e),r.querySelector(i)!==null||t===`style`&&r.querySelector(Vf(a))||t===`script`&&r.querySelector(Gf(a))||(t=r.createElement(`link`),Gd(t,`link`,e),St(t),r.head.appendChild(t)))}}function Ff(e,t){Df.m(e,t);var n=Af;if(n&&e){var r=t&&typeof t.as==`string`?t.as:`script`,i=`link[rel="modulepreload"][as="`+Vt(r)+`"][href="`+Vt(e)+`"]`,a=i;switch(r){case`audioworklet`:case`paintworklet`:case`serviceworker`:case`sharedworker`:case`worker`:case`script`:a=Wf(e)}if(!wf.has(a)&&(e=h({rel:`modulepreload`,href:e},t),wf.set(a,e),n.querySelector(i)===null)){switch(r){case`audioworklet`:case`paintworklet`:case`serviceworker`:case`sharedworker`:case`worker`:case`script`:if(n.querySelector(Gf(a)))return}r=n.createElement(`link`),Gd(r,`link`,e),St(r),n.head.appendChild(r)}}}function If(e,t,n){Df.S(e,t,n);var r=Af;if(r&&e){var i=xt(r).hoistableStyles,a=Bf(e);t||=`default`;var o=i.get(a);if(!o){var s={loading:0,preload:null};if(o=r.querySelector(Vf(a)))s.loading=5;else{e=h({rel:`stylesheet`,href:e,"data-precedence":t},n),(n=wf.get(a))&&Jf(e,n);var c=o=r.createElement(`link`);St(c),Gd(c,`link`,e),c._p=new Promise(function(e,t){c.onload=e,c.onerror=t}),c.addEventListener(`load`,function(){s.loading|=1}),c.addEventListener(`error`,function(){s.loading|=2}),s.loading|=4,qf(o,t,r)}o={type:`stylesheet`,instance:o,count:1,state:s},i.set(a,o)}}}function Lf(e,t){Df.X(e,t);var n=Af;if(n&&e){var r=xt(n).hoistableScripts,i=Wf(e),a=r.get(i);a||(a=n.querySelector(Gf(i)),a||(e=h({src:e,async:!0},t),(t=wf.get(i))&&Yf(e,t),a=n.createElement(`script`),St(a),Gd(a,`link`,e),n.head.appendChild(a)),a={type:`script`,instance:a,count:1,state:null},r.set(i,a))}}function Rf(e,t){Df.M(e,t);var n=Af;if(n&&e){var r=xt(n).hoistableScripts,i=Wf(e),a=r.get(i);a||(a=n.querySelector(Gf(i)),a||(e=h({src:e,async:!0,type:`module`},t),(t=wf.get(i))&&Yf(e,t),a=n.createElement(`script`),St(a),Gd(a,`link`,e),n.head.appendChild(a)),a={type:`script`,instance:a,count:1,state:null},r.set(i,a))}}function zf(e,t,n,r){var a=(a=ue.current)?Ef(a):null;if(!a)throw Error(i(446));switch(e){case`meta`:case`title`:return null;case`style`:return typeof n.precedence==`string`&&typeof n.href==`string`?(t=Bf(n.href),n=xt(a).hoistableStyles,r=n.get(t),r||(r={type:`style`,instance:null,count:0,state:null},n.set(t,r)),r):{type:`void`,instance:null,count:0,state:null};case`link`:if(n.rel===`stylesheet`&&typeof n.href==`string`&&typeof n.precedence==`string`){e=Bf(n.href);var o=xt(a).hoistableStyles,s=o.get(e);if(s||(a=a.ownerDocument||a,s={type:`stylesheet`,instance:null,count:0,state:{loading:0,preload:null}},o.set(e,s),(o=a.querySelector(Vf(e)))&&!o._p&&(s.instance=o,s.state.loading=5),wf.has(e)||(n={rel:`preload`,as:`style`,href:n.href,crossOrigin:n.crossOrigin,integrity:n.integrity,media:n.media,hrefLang:n.hrefLang,referrerPolicy:n.referrerPolicy},wf.set(e,n),o||Uf(a,e,n,s.state))),t&&r===null)throw Error(i(528,``));return s}if(t&&r!==null)throw Error(i(529,``));return null;case`script`:return t=n.async,n=n.src,typeof n==`string`&&t&&typeof t!=`function`&&typeof t!=`symbol`?(t=Wf(n),n=xt(a).hoistableScripts,r=n.get(t),r||(r={type:`script`,instance:null,count:0,state:null},n.set(t,r)),r):{type:`void`,instance:null,count:0,state:null};default:throw Error(i(444,e))}}function Bf(e){return`href="`+Vt(e)+`"`}function Vf(e){return`link[rel="stylesheet"][`+e+`]`}function Hf(e){return h({},e,{"data-precedence":e.precedence,precedence:null})}function Uf(e,t,n,r){e.querySelector(`link[rel="preload"][as="style"][`+t+`]`)?r.loading=1:(t=e.createElement(`link`),r.preload=t,t.addEventListener(`load`,function(){return r.loading|=1}),t.addEventListener(`error`,function(){return r.loading|=2}),Gd(t,`link`,n),St(t),e.head.appendChild(t))}function Wf(e){return`[src="`+Vt(e)+`"]`}function Gf(e){return`script[async]`+e}function Kf(e,t,n){if(t.count++,t.instance===null)switch(t.type){case`style`:var r=e.querySelector(`style[data-href~="`+Vt(n.href)+`"]`);if(r)return t.instance=r,St(r),r;var a=h({},n,{"data-href":n.href,"data-precedence":n.precedence,href:null,precedence:null});return r=(e.ownerDocument||e).createElement(`style`),St(r),Gd(r,`style`,a),qf(r,n.precedence,e),t.instance=r;case`stylesheet`:a=Bf(n.href);var o=e.querySelector(Vf(a));if(o)return t.state.loading|=4,t.instance=o,St(o),o;r=Hf(n),(a=wf.get(a))&&Jf(r,a),o=(e.ownerDocument||e).createElement(`link`),St(o);var s=o;return s._p=new Promise(function(e,t){s.onload=e,s.onerror=t}),Gd(o,`link`,r),t.state.loading|=4,qf(o,n.precedence,e),t.instance=o;case`script`:return o=Wf(n.src),(a=e.querySelector(Gf(o)))?(t.instance=a,St(a),a):(r=n,(a=wf.get(o))&&(r=h({},n),Yf(r,a)),e=e.ownerDocument||e,a=e.createElement(`script`),St(a),Gd(a,`link`,r),e.head.appendChild(a),t.instance=a);case`void`:return null;default:throw Error(i(443,t.type))}else t.type===`stylesheet`&&!(t.state.loading&4)&&(r=t.instance,t.state.loading|=4,qf(r,n.precedence,e));return t.instance}function qf(e,t,n){for(var r=n.querySelectorAll(`link[rel="stylesheet"][data-precedence],style[data-precedence]`),i=r.length?r[r.length-1]:null,a=i,o=0;o<r.length;o++){var s=r[o];if(s.dataset.precedence===t)a=s;else if(a!==i)break}a?a.parentNode.insertBefore(e,a.nextSibling):(t=n.nodeType===9?n.head:n,t.insertBefore(e,t.firstChild))}function Jf(e,t){e.crossOrigin??=t.crossOrigin,e.referrerPolicy??=t.referrerPolicy,e.title??=t.title}function Yf(e,t){e.crossOrigin??=t.crossOrigin,e.referrerPolicy??=t.referrerPolicy,e.integrity??=t.integrity}var Xf=null;function Zf(e,t,n){if(Xf===null){var r=new Map,i=Xf=new Map;i.set(n,r)}else i=Xf,r=i.get(n),r||(r=new Map,i.set(n,r));if(r.has(e))return r;for(r.set(e,null),n=n.getElementsByTagName(e),i=0;i<n.length;i++){var a=n[i];if(!(a[gt]||a[lt]||e===`link`&&a.getAttribute(`rel`)===`stylesheet`)&&a.namespaceURI!==`http://www.w3.org/2000/svg`){var o=a.getAttribute(t)||``;o=e+o;var s=r.get(o);s?s.push(a):r.set(o,[a])}}return r}function Qf(e,t,n){e=e.ownerDocument||e,e.head.insertBefore(n,t===`title`?e.querySelector(`head > title`):null)}function $f(e,t,n){if(n===1||t.itemProp!=null)return!1;switch(e){case`meta`:case`title`:return!0;case`style`:if(typeof t.precedence!=`string`||typeof t.href!=`string`||t.href===``)break;return!0;case`link`:if(typeof t.rel!=`string`||typeof t.href!=`string`||t.href===``||t.onLoad||t.onError)break;switch(t.rel){case`stylesheet`:return e=t.disabled,typeof t.precedence==`string`&&e==null;default:return!0}case`script`:if(t.async&&typeof t.async!=`function`&&typeof t.async!=`symbol`&&!t.onLoad&&!t.onError&&t.src&&typeof t.src==`string`)return!0}return!1}function ep(e){return!(e.type===`stylesheet`&&!(e.state.loading&3))}function tp(e,t,n,r){if(n.type===`stylesheet`&&(typeof r.media!=`string`||!1!==matchMedia(r.media).matches)&&!(n.state.loading&4)){if(n.instance===null){var i=Bf(r.href),a=t.querySelector(Vf(i));if(a){t=a._p,typeof t==`object`&&t&&typeof t.then==`function`&&(e.count++,e=ip.bind(e),t.then(e,e)),n.state.loading|=4,n.instance=a,St(a);return}a=t.ownerDocument||t,r=Hf(r),(i=wf.get(i))&&Jf(r,i),a=a.createElement(`link`),St(a);var o=a;o._p=new Promise(function(e,t){o.onload=e,o.onerror=t}),Gd(a,`link`,r),n.instance=a}e.stylesheets===null&&(e.stylesheets=new Map),e.stylesheets.set(n,t),(t=n.state.preload)&&!(n.state.loading&3)&&(e.count++,n=ip.bind(e),t.addEventListener(`load`,n),t.addEventListener(`error`,n))}}var np=0;function rp(e,t){return e.stylesheets&&e.count===0&&op(e,e.stylesheets),0<e.count||0<e.imgCount?function(n){var r=setTimeout(function(){if(e.stylesheets&&op(e,e.stylesheets),e.unsuspend){var t=e.unsuspend;e.unsuspend=null,t()}},6e4+t);0<e.imgBytes&&np===0&&(np=62500*Jd());var i=setTimeout(function(){if(e.waitingForImages=!1,e.count===0&&(e.stylesheets&&op(e,e.stylesheets),e.unsuspend)){var t=e.unsuspend;e.unsuspend=null,t()}},(e.imgBytes>np?50:800)+t);return e.unsuspend=n,function(){e.unsuspend=null,clearTimeout(r),clearTimeout(i)}}:null}function ip(){if(this.count--,this.count===0&&(this.imgCount===0||!this.waitingForImages)){if(this.stylesheets)op(this,this.stylesheets);else if(this.unsuspend){var e=this.unsuspend;this.unsuspend=null,e()}}}var ap=null;function op(e,t){e.stylesheets=null,e.unsuspend!==null&&(e.count++,ap=new Map,t.forEach(sp,e),ap=null,ip.call(e))}function sp(e,t){if(!(t.state.loading&4)){var n=ap.get(e);if(n)var r=n.get(null);else{n=new Map,ap.set(e,n);for(var i=e.querySelectorAll(`link[data-precedence],style[data-precedence]`),a=0;a<i.length;a++){var o=i[a];(o.nodeName===`LINK`||o.getAttribute(`media`)!==`not all`)&&(n.set(o.dataset.precedence,o),r=o)}r&&n.set(null,r)}i=t.instance,o=i.getAttribute(`data-precedence`),a=n.get(o)||r,a===r&&n.set(null,i),n.set(o,i),this.count++,r=ip.bind(this),i.addEventListener(`load`,r),i.addEventListener(`error`,r),a?a.parentNode.insertBefore(i,a.nextSibling):(e=e.nodeType===9?e.head:e,e.insertBefore(i,e.firstChild)),t.state.loading|=4}}var cp={$$typeof:C,Provider:null,Consumer:null,_currentValue:ne,_currentValue2:ne,_threadCount:0};function lp(e,t,n,r,i,a,o,s,c){this.tag=1,this.containerInfo=e,this.pingCache=this.current=this.pendingChildren=null,this.timeoutHandle=-1,this.callbackNode=this.next=this.pendingContext=this.context=this.cancelPendingCommit=null,this.callbackPriority=0,this.expirationTimes=Qe(-1),this.entangledLanes=this.shellSuspendCounter=this.errorRecoveryDisabledLanes=this.expiredLanes=this.warmLanes=this.pingedLanes=this.suspendedLanes=this.pendingLanes=0,this.entanglements=Qe(0),this.hiddenUpdates=Qe(null),this.identifierPrefix=r,this.onUncaughtError=i,this.onCaughtError=a,this.onRecoverableError=o,this.pooledCache=null,this.pooledCacheLanes=0,this.formState=c,this.incompleteTransitions=new Map}function up(e,t,n,r,i,a,o,s,c,l,u,d){return e=new lp(e,t,n,o,c,l,u,d,s),t=1,!0===a&&(t|=24),a=di(3,null,null,t),e.current=a,a.stateNode=e,t=da(),t.refCount++,e.pooledCache=t,t.refCount++,a.memoizedState={element:r,isDehydrated:n,cache:t},Wa(a),e}function dp(e){return e?(e=li,e):li}function fp(e,t,n,r,i,a){i=dp(i),r.context===null?r.context=i:r.pendingContext=i,r=Ka(t),r.payload={element:n},a=a===void 0?null:a,a!==null&&(r.callback=a),n=qa(e,r,t),n!==null&&(wu(n,e,t),Ja(n,e,t))}function pp(e,t){if(e=e.memoizedState,e!==null&&e.dehydrated!==null){var n=e.retryLane;e.retryLane=n!==0&&n<t?n:t}}function mp(e,t){pp(e,t),(e=e.alternate)&&pp(e,t)}function hp(e){if(e.tag===13||e.tag===31){var t=oi(e,67108864);t!==null&&wu(t,e,67108864),mp(e,67108864)}}function gp(e){if(e.tag===13||e.tag===31){var t=Su();t=it(t);var n=oi(e,t);n!==null&&wu(n,e,t),mp(e,t)}}var _p=!0;function vp(e,t,n,r){var i=P.T;P.T=null;var a=F.p;try{F.p=2,bp(e,t,n,r)}finally{F.p=a,P.T=i}}function yp(e,t,n,r){var i=P.T;P.T=null;var a=F.p;try{F.p=8,bp(e,t,n,r)}finally{F.p=a,P.T=i}}function bp(e,t,n,r){if(_p){var i=xp(r);if(i===null)Pd(e,t,r,Sp,n),Np(e,r);else if(Fp(i,e,t,n,r))r.stopPropagation();else if(Np(e,r),t&4&&-1<Mp.indexOf(e)){for(;i!==null;){var a=yt(i);if(a!==null)switch(a.tag){case 3:if(a=a.stateNode,a.current.memoizedState.isDehydrated){var o=qe(a.pendingLanes);if(o!==0){var s=a;for(s.pendingLanes|=2,s.entangledLanes|=2;o;){var c=1<<31-Be(o);s.entanglements[1]|=c,o&=~c}pd(a),!(Ul&6)&&(uu=Oe()+500,md(0,!1))}}break;case 31:case 13:s=oi(a,2),s!==null&&wu(s,a,2),ku(),mp(a,2)}if(a=xp(r),a===null&&Pd(e,t,r,Sp,n),a===i)break;i=a}i!==null&&r.stopPropagation()}else Pd(e,t,r,null,n)}}function xp(e){return e=an(e),Cp(e)}var Sp=null;function Cp(e){if(Sp=null,e=vt(e),e!==null){var t=o(e);if(t===null)e=null;else{var n=t.tag;if(n===13){if(e=s(t),e!==null)return e;e=null}else if(n===31){if(e=c(t),e!==null)return e;e=null}else if(n===3){if(t.stateNode.current.memoizedState.isDehydrated)return t.tag===3?t.stateNode.containerInfo:null;e=null}else t!==e&&(e=null)}}return Sp=e,null}function wp(e){switch(e){case`beforetoggle`:case`cancel`:case`click`:case`close`:case`contextmenu`:case`copy`:case`cut`:case`auxclick`:case`dblclick`:case`dragend`:case`dragstart`:case`drop`:case`focusin`:case`focusout`:case`input`:case`invalid`:case`keydown`:case`keypress`:case`keyup`:case`mousedown`:case`mouseup`:case`paste`:case`pause`:case`play`:case`pointercancel`:case`pointerdown`:case`pointerup`:case`ratechange`:case`reset`:case`resize`:case`seeked`:case`submit`:case`toggle`:case`touchcancel`:case`touchend`:case`touchstart`:case`volumechange`:case`change`:case`selectionchange`:case`textInput`:case`compositionstart`:case`compositionend`:case`compositionupdate`:case`beforeblur`:case`afterblur`:case`beforeinput`:case`blur`:case`fullscreenchange`:case`focus`:case`hashchange`:case`popstate`:case`select`:case`selectstart`:return 2;case`drag`:case`dragenter`:case`dragexit`:case`dragleave`:case`dragover`:case`mousemove`:case`mouseout`:case`mouseover`:case`pointermove`:case`pointerout`:case`pointerover`:case`scroll`:case`touchmove`:case`wheel`:case`mouseenter`:case`mouseleave`:case`pointerenter`:case`pointerleave`:return 8;case`message`:switch(ke()){case Ae:return 2;case je:return 8;case Me:case Ne:return 32;case Pe:return 268435456;default:return 32}default:return 32}}var Tp=!1,Ep=null,Dp=null,Op=null,kp=new Map,Ap=new Map,jp=[],Mp=`mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset`.split(` `);function Np(e,t){switch(e){case`focusin`:case`focusout`:Ep=null;break;case`dragenter`:case`dragleave`:Dp=null;break;case`mouseover`:case`mouseout`:Op=null;break;case`pointerover`:case`pointerout`:kp.delete(t.pointerId);break;case`gotpointercapture`:case`lostpointercapture`:Ap.delete(t.pointerId)}}function Pp(e,t,n,r,i,a){return e===null||e.nativeEvent!==a?(e={blockedOn:t,domEventName:n,eventSystemFlags:r,nativeEvent:a,targetContainers:[i]},t!==null&&(t=yt(t),t!==null&&hp(t)),e):(e.eventSystemFlags|=r,t=e.targetContainers,i!==null&&t.indexOf(i)===-1&&t.push(i),e)}function Fp(e,t,n,r,i){switch(t){case`focusin`:return Ep=Pp(Ep,e,t,n,r,i),!0;case`dragenter`:return Dp=Pp(Dp,e,t,n,r,i),!0;case`mouseover`:return Op=Pp(Op,e,t,n,r,i),!0;case`pointerover`:var a=i.pointerId;return kp.set(a,Pp(kp.get(a)||null,e,t,n,r,i)),!0;case`gotpointercapture`:return a=i.pointerId,Ap.set(a,Pp(Ap.get(a)||null,e,t,n,r,i)),!0}return!1}function Ip(e){var t=vt(e.target);if(t!==null){var n=o(t);if(n!==null){if(t=n.tag,t===13){if(t=s(n),t!==null){e.blockedOn=t,st(e.priority,function(){gp(n)});return}}else if(t===31){if(t=c(n),t!==null){e.blockedOn=t,st(e.priority,function(){gp(n)});return}}else if(t===3&&n.stateNode.current.memoizedState.isDehydrated){e.blockedOn=n.tag===3?n.stateNode.containerInfo:null;return}}}e.blockedOn=null}function Lp(e){if(e.blockedOn!==null)return!1;for(var t=e.targetContainers;0<t.length;){var n=xp(e.nativeEvent);if(n===null){n=e.nativeEvent;var r=new n.constructor(n.type,n);rn=r,n.target.dispatchEvent(r),rn=null}else return t=yt(n),t!==null&&hp(t),e.blockedOn=n,!1;t.shift()}return!0}function Rp(e,t,n){Lp(e)&&n.delete(t)}function zp(){Tp=!1,Ep!==null&&Lp(Ep)&&(Ep=null),Dp!==null&&Lp(Dp)&&(Dp=null),Op!==null&&Lp(Op)&&(Op=null),kp.forEach(Rp),Ap.forEach(Rp)}function Bp(e,n){e.blockedOn===n&&(e.blockedOn=null,Tp||(Tp=!0,t.unstable_scheduleCallback(t.unstable_NormalPriority,zp)))}var Vp=null;function Hp(e){Vp!==e&&(Vp=e,t.unstable_scheduleCallback(t.unstable_NormalPriority,function(){Vp===e&&(Vp=null);for(var t=0;t<e.length;t+=3){var n=e[t],r=e[t+1],i=e[t+2];if(typeof r!=`function`){if(Cp(r||n)===null)continue;break}var a=yt(n);a!==null&&(e.splice(t,3),t-=3,ks(a,{pending:!0,data:i,method:n.method,action:r},r,i))}}))}function Up(e){function t(t){return Bp(t,e)}Ep!==null&&Bp(Ep,e),Dp!==null&&Bp(Dp,e),Op!==null&&Bp(Op,e),kp.forEach(t),Ap.forEach(t);for(var n=0;n<jp.length;n++){var r=jp[n];r.blockedOn===e&&(r.blockedOn=null)}for(;0<jp.length&&(n=jp[0],n.blockedOn===null);)Ip(n),n.blockedOn===null&&jp.shift();if(n=(e.ownerDocument||e).$$reactFormReplay,n!=null)for(r=0;r<n.length;r+=3){var i=n[r],a=n[r+1],o=i[ut]||null;if(typeof a==`function`)o||Hp(n);else if(o){var s=null;if(a&&a.hasAttribute(`formAction`)){if(i=a,o=a[ut]||null)s=o.formAction;else if(Cp(i)!==null)continue}else s=o.action;typeof s==`function`?n[r+1]=s:(n.splice(r,3),r-=3),Hp(n)}}}function Wp(){function e(e){e.canIntercept&&e.info===`react-transition`&&e.intercept({handler:function(){return new Promise(function(e){return i=e})},focusReset:`manual`,scroll:`manual`})}function t(){i!==null&&(i(),i=null),r||setTimeout(n,20)}function n(){if(!r&&!navigation.transition){var e=navigation.currentEntry;e&&e.url!=null&&navigation.navigate(e.url,{state:e.getState(),info:`react-transition`,history:`replace`})}}if(typeof navigation==`object`){var r=!1,i=null;return navigation.addEventListener(`navigate`,e),navigation.addEventListener(`navigatesuccess`,t),navigation.addEventListener(`navigateerror`,t),setTimeout(n,100),function(){r=!0,navigation.removeEventListener(`navigate`,e),navigation.removeEventListener(`navigatesuccess`,t),navigation.removeEventListener(`navigateerror`,t),i!==null&&(i(),i=null)}}}function Gp(e){this._internalRoot=e}Kp.prototype.render=Gp.prototype.render=function(e){var t=this._internalRoot;if(t===null)throw Error(i(409));var n=t.current;fp(n,Su(),e,t,null,null)},Kp.prototype.unmount=Gp.prototype.unmount=function(){var e=this._internalRoot;if(e!==null){this._internalRoot=null;var t=e.containerInfo;fp(e.current,2,null,e,null,null),ku(),t[dt]=null}};function Kp(e){this._internalRoot=e}Kp.prototype.unstable_scheduleHydration=function(e){if(e){var t=ot();e={blockedOn:null,target:e,priority:t};for(var n=0;n<jp.length&&t!==0&&t<jp[n].priority;n++);jp.splice(n,0,e),n===0&&Ip(e)}};var qp=n.version;if(qp!==`19.2.0`)throw Error(i(527,qp,`19.2.0`));F.findDOMNode=function(e){var t=e._reactInternals;if(t===void 0)throw typeof e.render==`function`?Error(i(188)):(e=Object.keys(e).join(`,`),Error(i(268,e)));return e=d(t),e=e===null?null:p(e),e=e===null?null:e.stateNode,e};var Jp={bundleType:0,version:`19.2.0`,rendererPackageName:`react-dom`,currentDispatcherRef:P,reconcilerVersion:`19.2.0`};if(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__<`u`){var Yp=__REACT_DEVTOOLS_GLOBAL_HOOK__;if(!Yp.isDisabled&&Yp.supportsFiber)try{Le=Yp.inject(Jp),Re=Yp}catch{}}e.createRoot=function(e,t){if(!a(e))throw Error(i(299));var n=!1,r=``,o=Qs,s=$s,c=ec;return t!=null&&(!0===t.unstable_strictMode&&(n=!0),t.identifierPrefix!==void 0&&(r=t.identifierPrefix),t.onUncaughtError!==void 0&&(o=t.onUncaughtError),t.onCaughtError!==void 0&&(s=t.onCaughtError),t.onRecoverableError!==void 0&&(c=t.onRecoverableError)),t=up(e,1,!1,null,null,n,r,null,o,s,c,Wp),e[dt]=t.current,Md(e),new Gp(t)}})),g=o(((e,t)=>{function n(){if(!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__>`u`||typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE!=`function`))try{__REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(n)}catch(e){console.error(e)}}n(),t.exports=h()})),_=`modulepreload`,v=function(e){return`/`+e},y={};const b=function(e,t,n){let r=Promise.resolve();if(t&&t.length>0){let e=document.getElementsByTagName(`link`),i=document.querySelector(`meta[property=csp-nonce]`),a=i?.nonce||i?.getAttribute(`nonce`);function o(e){return Promise.all(e.map(e=>Promise.resolve(e).then(e=>({status:`fulfilled`,value:e}),e=>({status:`rejected`,reason:e}))))}r=o(t.map(t=>{if(t=v(t,n),t in y)return;y[t]=!0;let r=t.endsWith(`.css`),i=r?`[rel="stylesheet"]`:``;if(n)for(let n=e.length-1;n>=0;n--){let i=e[n];if(i.href===t&&(!r||i.rel===`stylesheet`))return}else if(document.querySelector(`link[href="${t}"]${i}`))return;let o=document.createElement(`link`);if(o.rel=r?`stylesheet`:_,r||(o.as=`script`),o.crossOrigin=``,o.href=t,a&&o.setAttribute(`nonce`,a),document.head.appendChild(o),r)return new Promise((e,n)=>{o.addEventListener(`load`,e),o.addEventListener(`error`,()=>n(Error(`Unable to preload CSS for ${t}`)))})}))}function i(e){let t=new Event(`vite:preloadError`,{cancelable:!0});if(t.payload=e,window.dispatchEvent(t),!t.defaultPrevented)throw e}return r.then(t=>{for(let e of t||[])e.status===`rejected`&&i(e.reason);return e().catch(i)})};var x=c(u(),1),S=`popstate`;function C(e={}){function t(e,t){let{pathname:n,search:r,hash:i}=e.location;return O(``,{pathname:n,search:r,hash:i},t.state&&t.state.usr||null,t.state&&t.state.key||`default`)}function n(e,t){return typeof t==`string`?t:k(t)}return A(t,n,null,e)}function w(e,t){if(e===!1||e==null)throw Error(t)}function T(e,t){if(!e){typeof console<`u`&&console.warn(t);try{throw Error(t)}catch{}}}function E(){return Math.random().toString(36).substring(2,10)}function D(e,t){return{usr:e.state,key:e.key,idx:t}}function O(e,t,n=null,r){return{pathname:typeof e==`string`?e:e.pathname,search:``,hash:``,...typeof t==`string`?ee(t):t,state:n,key:t&&t.key||r||E()}}function k({pathname:e=`/`,search:t=``,hash:n=``}){return t&&t!==`?`&&(e+=t.charAt(0)===`?`?t:`?`+t),n&&n!==`#`&&(e+=n.charAt(0)===`#`?n:`#`+n),e}function ee(e){let t={};if(e){let n=e.indexOf(`#`);n>=0&&(t.hash=e.substring(n),e=e.substring(0,n));let r=e.indexOf(`?`);r>=0&&(t.search=e.substring(r),e=e.substring(0,r)),e&&(t.pathname=e)}return t}function A(e,t,n,r={}){let{window:i=document.defaultView,v5Compat:a=!1}=r,o=i.history,s=`POP`,c=null,l=u();l??(l=0,o.replaceState({...o.state,idx:l},``));function u(){return(o.state||{idx:null}).idx}function d(){s=`POP`;let e=u(),t=e==null?null:e-l;l=e,c&&c({action:s,location:h.location,delta:t})}function f(e,t){s=`PUSH`;let r=O(h.location,e,t);n&&n(r,e),l=u()+1;let d=D(r,l),f=h.createHref(r);try{o.pushState(d,``,f)}catch(e){if(e instanceof DOMException&&e.name===`DataCloneError`)throw e;i.location.assign(f)}a&&c&&c({action:s,location:h.location,delta:1})}function p(e,t){s=`REPLACE`;let r=O(h.location,e,t);n&&n(r,e),l=u();let i=D(r,l),d=h.createHref(r);o.replaceState(i,``,d),a&&c&&c({action:s,location:h.location,delta:0})}function m(e){return j(e)}let h={get action(){return s},get location(){return e(i,o)},listen(e){if(c)throw Error(`A history only accepts one active listener`);return i.addEventListener(S,d),c=e,()=>{i.removeEventListener(S,d),c=null}},createHref(e){return t(i,e)},createURL:m,encodeLocation(e){let t=m(e);return{pathname:t.pathname,search:t.search,hash:t.hash}},push:f,replace:p,go(e){return o.go(e)}};return h}function j(e,t=!1){let n=`http://localhost`;typeof window<`u`&&(n=window.location.origin===`null`?window.location.href:window.location.origin),w(n,`No window.location.(origin|href) available to create URL`);let r=typeof e==`string`?e:k(e);return r=r.replace(/ $/,`%20`),!t&&r.startsWith(`//`)&&(r=n+r),new URL(r,n)}function M(e,t,n=`/`){return te(e,t,n,!1)}function te(e,t,n,r){let i=ge((typeof t==`string`?ee(t):t).pathname||`/`,n);if(i==null)return null;let a=P(e);ne(a);let o=null;for(let e=0;o==null&&e<a.length;++e){let t=he(i);o=fe(a[e],t,r)}return o}function N(e,t){let{route:n,pathname:r,params:i}=e;return{id:n.id,pathname:r,params:i,data:t[n.id],loaderData:t[n.id],handle:n.handle}}function P(e,t=[],n=[],r=``,i=!1){let a=(e,a,o=i,s)=>{let c={relativePath:s===void 0?e.path||``:s,caseSensitive:e.caseSensitive===!0,childrenIndex:a,route:e};if(c.relativePath.startsWith(`/`)){if(!c.relativePath.startsWith(r)&&o)return;w(c.relativePath.startsWith(r),`Absolute route path "${c.relativePath}" nested under path "${r}" is not valid. An absolute child route path must start with the combined path of all its parent routes.`),c.relativePath=c.relativePath.slice(r.length)}let l=Te([r,c.relativePath]),u=n.concat(c);e.children&&e.children.length>0&&(w(e.index!==!0,`Index routes must not have child routes. Please remove all child routes from route path "${l}".`),P(e.children,t,u,l,o)),!(e.path==null&&!e.index)&&t.push({path:l,score:ue(l,e.index),routesMeta:u})};return e.forEach((e,t)=>{if(e.path===``||!e.path?.includes(`?`))a(e,t);else for(let n of F(e.path))a(e,t,!0,n)}),t}function F(e){let t=e.split(`/`);if(t.length===0)return[];let[n,...r]=t,i=n.endsWith(`?`),a=n.replace(/\?$/,``);if(r.length===0)return i?[a,``]:[a];let o=F(r.join(`/`)),s=[];return s.push(...o.map(e=>e===``?a:[a,e].join(`/`))),i&&s.push(...o),s.map(t=>e.startsWith(`/`)&&t===``?`/`:t)}function ne(e){e.sort((e,t)=>e.score===t.score?de(e.routesMeta.map(e=>e.childrenIndex),t.routesMeta.map(e=>e.childrenIndex)):t.score-e.score)}var re=/^:[\w-]+$/,ie=3,ae=2,oe=1,se=10,ce=-2,le=e=>e===`*`;function ue(e,t){let n=e.split(`/`),r=n.length;return n.some(le)&&(r+=ce),t&&(r+=ae),n.filter(e=>!le(e)).reduce((e,t)=>e+(re.test(t)?ie:t===``?oe:se),r)}function de(e,t){return e.length===t.length&&e.slice(0,-1).every((e,n)=>e===t[n])?e[e.length-1]-t[t.length-1]:0}function fe(e,t,n=!1){let{routesMeta:r}=e,i={},a=`/`,o=[];for(let e=0;e<r.length;++e){let s=r[e],c=e===r.length-1,l=a===`/`?t:t.slice(a.length)||`/`,u=pe({path:s.relativePath,caseSensitive:s.caseSensitive,end:c},l),d=s.route;if(!u&&c&&n&&!r[r.length-1].route.index&&(u=pe({path:s.relativePath,caseSensitive:s.caseSensitive,end:!1},l)),!u)return null;Object.assign(i,u.params),o.push({params:i,pathname:Te([a,u.pathname]),pathnameBase:Ee(Te([a,u.pathnameBase])),route:d}),u.pathnameBase!==`/`&&(a=Te([a,u.pathnameBase]))}return o}function pe(e,t){typeof e==`string`&&(e={path:e,caseSensitive:!1,end:!0});let[n,r]=me(e.path,e.caseSensitive,e.end),i=t.match(n);if(!i)return null;let a=i[0],o=a.replace(/(.)\/+$/,`$1`),s=i.slice(1);return{params:r.reduce((e,{paramName:t,isOptional:n},r)=>{if(t===`*`){let e=s[r]||``;o=a.slice(0,a.length-e.length).replace(/(.)\/+$/,`$1`)}let i=s[r];return n&&!i?e[t]=void 0:e[t]=(i||``).replace(/%2F/g,`/`),e},{}),pathname:a,pathnameBase:o,pattern:e}}function me(e,t=!1,n=!0){T(e===`*`||!e.endsWith(`*`)||e.endsWith(`/*`),`Route path "${e}" will be treated as if it were "${e.replace(/\*$/,`/*`)}" because the \`*\` character must always follow a \`/\` in the pattern. To get rid of this warning, please change the route path to "${e.replace(/\*$/,`/*`)}".`);let r=[],i=`^`+e.replace(/\/*\*?$/,``).replace(/^\/*/,`/`).replace(/[\\.*+^${}|()[\]]/g,`\\$&`).replace(/\/:([\w-]+)(\?)?/g,(e,t,n)=>(r.push({paramName:t,isOptional:n!=null}),n?`/?([^\\/]+)?`:`/([^\\/]+)`)).replace(/\/([\w-]+)\?(\/|$)/g,`(/$1)?$2`);return e.endsWith(`*`)?(r.push({paramName:`*`}),i+=e===`*`||e===`/*`?`(.*)$`:`(?:\\/(.+)|\\/*)$`):n?i+=`\\/*$`:e!==``&&e!==`/`&&(i+=`(?:(?=\\/|$))`),[new RegExp(i,t?void 0:`i`),r]}function he(e){try{return e.split(`/`).map(e=>decodeURIComponent(e).replace(/\//g,`%2F`)).join(`/`)}catch(t){return T(!1,`The URL path "${e}" could not be decoded because it is a malformed URL segment. This is probably due to a bad percent encoding (${t}).`),e}}function ge(e,t){if(t===`/`)return e;if(!e.toLowerCase().startsWith(t.toLowerCase()))return null;let n=t.endsWith(`/`)?t.length-1:t.length,r=e.charAt(n);return r&&r!==`/`?null:e.slice(n)||`/`}var _e=/^(?:[a-z][a-z0-9+.-]*:|\/\/)/i,ve=e=>_e.test(e);function ye(e,t=`/`){let{pathname:n,search:r=``,hash:i=``}=typeof e==`string`?ee(e):e,a;if(n)if(ve(n))a=n;else{if(n.includes(`//`)){let e=n;n=n.replace(/\/\/+/g,`/`),T(!1,`Pathnames cannot have embedded double slashes - normalizing ${e} -> ${n}`)}a=n.startsWith(`/`)?be(n.substring(1),`/`):be(n,t)}else a=t;return{pathname:a,search:De(r),hash:Oe(i)}}function be(e,t){let n=t.replace(/\/+$/,``).split(`/`);return e.split(`/`).forEach(e=>{e===`..`?n.length>1&&n.pop():e!==`.`&&n.push(e)}),n.length>1?n.join(`/`):`/`}function xe(e,t,n,r){return`Cannot include a '${e}' character in a manually specified \`to.${t}\` field [${JSON.stringify(r)}].  Please separate it out to the \`to.${n}\` field. Alternatively you may provide the full path as a string in <Link to="..."> and the router will parse it for you.`}function Se(e){return e.filter((e,t)=>t===0||e.route.path&&e.route.path.length>0)}function Ce(e){let t=Se(e);return t.map((e,n)=>n===t.length-1?e.pathname:e.pathnameBase)}function we(e,t,n,r=!1){let i;typeof e==`string`?i=ee(e):(i={...e},w(!i.pathname||!i.pathname.includes(`?`),xe(`?`,`pathname`,`search`,i)),w(!i.pathname||!i.pathname.includes(`#`),xe(`#`,`pathname`,`hash`,i)),w(!i.search||!i.search.includes(`#`),xe(`#`,`search`,`hash`,i)));let a=e===``||i.pathname===``,o=a?`/`:i.pathname,s;if(o==null)s=n;else{let e=t.length-1;if(!r&&o.startsWith(`..`)){let t=o.split(`/`);for(;t[0]===`..`;)t.shift(),--e;i.pathname=t.join(`/`)}s=e>=0?t[e]:`/`}let c=ye(i,s),l=o&&o!==`/`&&o.endsWith(`/`),u=(a||o===`.`)&&n.endsWith(`/`);return!c.pathname.endsWith(`/`)&&(l||u)&&(c.pathname+=`/`),c}var Te=e=>e.join(`/`).replace(/\/\/+/g,`/`),Ee=e=>e.replace(/\/+$/,``).replace(/^\/*/,`/`),De=e=>!e||e===`?`?``:e.startsWith(`?`)?e:`?`+e,Oe=e=>!e||e===`#`?``:e.startsWith(`#`)?e:`#`+e;function ke(e){return e!=null&&typeof e.status==`number`&&typeof e.statusText==`string`&&typeof e.internal==`boolean`&&`data`in e}Object.getOwnPropertyNames(Object.prototype).sort().join(`\0`);var Ae=x.createContext(null);Ae.displayName=`DataRouter`;var je=x.createContext(null);je.displayName=`DataRouterState`,x.createContext(!1);var Me=x.createContext({isTransitioning:!1});Me.displayName=`ViewTransition`;var Ne=x.createContext(new Map);Ne.displayName=`Fetchers`;var Pe=x.createContext(null);Pe.displayName=`Await`;var Fe=x.createContext(null);Fe.displayName=`Navigation`;var Ie=x.createContext(null);Ie.displayName=`Location`;var Le=x.createContext({outlet:null,matches:[],isDataRoute:!1});Le.displayName=`Route`;var Re=x.createContext(null);Re.displayName=`RouteError`;function ze(e,{relative:t}={}){w(Be(),`useHref() may be used only in the context of a <Router> component.`);let{basename:n,navigator:r}=x.useContext(Fe),{hash:i,pathname:a,search:o}=Ke(e,{relative:t}),s=a;return n!==`/`&&(s=a===`/`?n:Te([n,a])),r.createHref({pathname:s,search:o,hash:i})}function Be(){return x.useContext(Ie)!=null}function Ve(){return w(Be(),`useLocation() may be used only in the context of a <Router> component.`),x.useContext(Ie).location}var He=`You should call navigate() in a React.useEffect(), not when your component is first rendered.`;function Ue(e){x.useContext(Fe).static||x.useLayoutEffect(e)}function We(){let{isDataRoute:e}=x.useContext(Le);return e?lt():Ge()}function Ge(){w(Be(),`useNavigate() may be used only in the context of a <Router> component.`);let e=x.useContext(Ae),{basename:t,navigator:n}=x.useContext(Fe),{matches:r}=x.useContext(Le),{pathname:i}=Ve(),a=JSON.stringify(Ce(r)),o=x.useRef(!1);return Ue(()=>{o.current=!0}),x.useCallback((r,s={})=>{if(T(o.current,He),!o.current)return;if(typeof r==`number`){n.go(r);return}let c=we(r,JSON.parse(a),i,s.relative===`path`);e==null&&t!==`/`&&(c.pathname=c.pathname===`/`?t:Te([t,c.pathname])),(s.replace?n.replace:n.push)(c,s.state,s)},[t,n,a,i,e])}x.createContext(null);function Ke(e,{relative:t}={}){let{matches:n}=x.useContext(Le),{pathname:r}=Ve(),i=JSON.stringify(Ce(n));return x.useMemo(()=>we(e,JSON.parse(i),r,t===`path`),[e,i,r,t])}function qe(e,t){return Je(e,t)}function Je(e,t,n,r,i){w(Be(),`useRoutes() may be used only in the context of a <Router> component.`);let{navigator:a}=x.useContext(Fe),{matches:o}=x.useContext(Le),s=o[o.length-1],c=s?s.params:{},l=s?s.pathname:`/`,u=s?s.pathnameBase:`/`,d=s&&s.route;{let e=d&&d.path||``;dt(l,!d||e.endsWith(`*`)||e.endsWith(`*?`),`You rendered descendant <Routes> (or called \`useRoutes()\`) at "${l}" (under <Route path="${e}">) but the parent route path has no trailing "*". This means if you navigate deeper, the parent won't match anymore and therefore the child routes will never render.

Please change the parent <Route path="${e}"> to <Route path="${e===`/`?`*`:`${e}/*`}">.`)}let f=Ve(),p;if(t){let e=typeof t==`string`?ee(t):t;w(u===`/`||e.pathname?.startsWith(u),`When overriding the location using \`<Routes location>\` or \`useRoutes(routes, location)\`, the location pathname must begin with the portion of the URL pathname that was matched by all parent routes. The current pathname base is "${u}" but pathname "${e.pathname}" was given in the \`location\` prop.`),p=e}else p=f;let m=p.pathname||`/`,h=m;if(u!==`/`){let e=u.replace(/^\//,``).split(`/`);h=`/`+m.replace(/^\//,``).split(`/`).slice(e.length).join(`/`)}let g=M(e,{pathname:h});T(d||g!=null,`No routes matched location "${p.pathname}${p.search}${p.hash}" `),T(g==null||g[g.length-1].route.element!==void 0||g[g.length-1].route.Component!==void 0||g[g.length-1].route.lazy!==void 0,`Matched leaf route at location "${p.pathname}${p.search}${p.hash}" does not have an element or Component. This means it will render an <Outlet /> with a null value by default resulting in an "empty" page.`);let _=$e(g&&g.map(e=>Object.assign({},e,{params:Object.assign({},c,e.params),pathname:Te([u,a.encodeLocation?a.encodeLocation(e.pathname.replace(/\?/g,`%3F`).replace(/#/g,`%23`)).pathname:e.pathname]),pathnameBase:e.pathnameBase===`/`?u:Te([u,a.encodeLocation?a.encodeLocation(e.pathnameBase.replace(/\?/g,`%3F`).replace(/#/g,`%23`)).pathname:e.pathnameBase])})),o,n,r,i);return t&&_?x.createElement(Ie.Provider,{value:{location:{pathname:`/`,search:``,hash:``,state:null,key:`default`,...p},navigationType:`POP`}},_):_}function Ye(){let e=ct(),t=ke(e)?`${e.status} ${e.statusText}`:e instanceof Error?e.message:JSON.stringify(e),n=e instanceof Error?e.stack:null,r=`rgba(200,200,200, 0.5)`,i={padding:`0.5rem`,backgroundColor:r},a={padding:`2px 4px`,backgroundColor:r},o=null;return console.error(`Error handled by React Router default ErrorBoundary:`,e),o=x.createElement(x.Fragment,null,x.createElement(`p`,null,`💿 Hey developer 👋`),x.createElement(`p`,null,`You can provide a way better UX than this when your app throws errors by providing your own `,x.createElement(`code`,{style:a},`ErrorBoundary`),` or`,` `,x.createElement(`code`,{style:a},`errorElement`),` prop on your route.`)),x.createElement(x.Fragment,null,x.createElement(`h2`,null,`Unexpected Application Error!`),x.createElement(`h3`,{style:{fontStyle:`italic`}},t),n?x.createElement(`pre`,{style:i},n):null,o)}var Xe=x.createElement(Ye,null),Ze=class extends x.Component{constructor(e){super(e),this.state={location:e.location,revalidation:e.revalidation,error:e.error}}static getDerivedStateFromError(e){return{error:e}}static getDerivedStateFromProps(e,t){return t.location!==e.location||t.revalidation!==`idle`&&e.revalidation===`idle`?{error:e.error,location:e.location,revalidation:e.revalidation}:{error:e.error===void 0?t.error:e.error,location:t.location,revalidation:e.revalidation||t.revalidation}}componentDidCatch(e,t){this.props.onError?this.props.onError(e,t):console.error(`React Router caught the following error during render`,e)}render(){return this.state.error===void 0?this.props.children:x.createElement(Le.Provider,{value:this.props.routeContext},x.createElement(Re.Provider,{value:this.state.error,children:this.props.component}))}};function Qe({routeContext:e,match:t,children:n}){let r=x.useContext(Ae);return r&&r.static&&r.staticContext&&(t.route.errorElement||t.route.ErrorBoundary)&&(r.staticContext._deepestRenderedBoundaryId=t.route.id),x.createElement(Le.Provider,{value:e},n)}function $e(e,t=[],n=null,r=null,i=null){if(e==null){if(!n)return null;if(n.errors)e=n.matches;else if(t.length===0&&!n.initialized&&n.matches.length>0)e=n.matches;else return null}let a=e,o=n?.errors;if(o!=null){let e=a.findIndex(e=>e.route.id&&o?.[e.route.id]!==void 0);w(e>=0,`Could not find a matching route for errors on route IDs: ${Object.keys(o).join(`,`)}`),a=a.slice(0,Math.min(a.length,e+1))}let s=!1,c=-1;if(n)for(let e=0;e<a.length;e++){let t=a[e];if((t.route.HydrateFallback||t.route.hydrateFallbackElement)&&(c=e),t.route.id){let{loaderData:e,errors:r}=n,i=t.route.loader&&!e.hasOwnProperty(t.route.id)&&(!r||r[t.route.id]===void 0);if(t.route.lazy||i){s=!0,a=c>=0?a.slice(0,c+1):[a[0]];break}}}let l=n&&r?(e,t)=>{r(e,{location:n.location,params:n.matches?.[0]?.params??{},errorInfo:t})}:void 0;return a.reduceRight((e,r,i)=>{let u,d=!1,f=null,p=null;n&&(u=o&&r.route.id?o[r.route.id]:void 0,f=r.route.errorElement||Xe,s&&(c<0&&i===0?(dt(`route-fallback`,!1,"No `HydrateFallback` element provided to render during initial hydration"),d=!0,p=null):c===i&&(d=!0,p=r.route.hydrateFallbackElement||null)));let m=t.concat(a.slice(0,i+1)),h=()=>{let t;return t=u?f:d?p:r.route.Component?x.createElement(r.route.Component,null):r.route.element?r.route.element:e,x.createElement(Qe,{match:r,routeContext:{outlet:e,matches:m,isDataRoute:n!=null},children:t})};return n&&(r.route.ErrorBoundary||r.route.errorElement||i===0)?x.createElement(Ze,{location:n.location,revalidation:n.revalidation,component:f,error:u,children:h(),routeContext:{outlet:null,matches:m,isDataRoute:!0},onError:l}):h()},null)}function et(e){return`${e} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`}function tt(e){let t=x.useContext(Ae);return w(t,et(e)),t}function nt(e){let t=x.useContext(je);return w(t,et(e)),t}function rt(e){let t=x.useContext(Le);return w(t,et(e)),t}function it(e){let t=rt(e),n=t.matches[t.matches.length-1];return w(n.route.id,`${e} can only be used on routes that contain a unique "id"`),n.route.id}function at(){return it(`useRouteId`)}function ot(){return nt(`useNavigation`).navigation}function st(){let{matches:e,loaderData:t}=nt(`useMatches`);return x.useMemo(()=>e.map(e=>N(e,t)),[e,t])}function ct(){let e=x.useContext(Re),t=nt(`useRouteError`),n=it(`useRouteError`);return e===void 0?t.errors?.[n]:e}function lt(){let{router:e}=tt(`useNavigate`),t=it(`useNavigate`),n=x.useRef(!1);return Ue(()=>{n.current=!0}),x.useCallback(async(r,i={})=>{T(n.current,He),n.current&&(typeof r==`number`?e.navigate(r):await e.navigate(r,{fromRouteId:t,...i}))},[e,t])}var ut={};function dt(e,t,n){!t&&!ut[e]&&(ut[e]=!0,T(!1,n))}x.memo(ft);function ft({routes:e,future:t,state:n,unstable_onError:r}){return Je(e,void 0,n,r,t)}function pt({to:e,replace:t,state:n,relative:r}){w(Be(),`<Navigate> may be used only in the context of a <Router> component.`);let{static:i}=x.useContext(Fe);T(!i,`<Navigate> must not be used on the initial render in a <StaticRouter>. This is a no-op, but you should modify your code so the <Navigate> is only ever rendered in response to some user interaction or state change.`);let{matches:a}=x.useContext(Le),{pathname:o}=Ve(),s=We(),c=we(e,Ce(a),o,r===`path`),l=JSON.stringify(c);return x.useEffect(()=>{s(JSON.parse(l),{replace:t,state:n,relative:r})},[s,l,r,t,n]),null}function mt(e){w(!1,`A <Route> is only ever to be used as the child of <Routes> element, never rendered directly. Please wrap your <Route> in a <Routes>.`)}function ht({basename:e=`/`,children:t=null,location:n,navigationType:r=`POP`,navigator:i,static:a=!1}){w(!Be(),`You cannot render a <Router> inside another <Router>. You should never have more than one in your app.`);let o=e.replace(/^\/*/,`/`),s=x.useMemo(()=>({basename:o,navigator:i,static:a,future:{}}),[o,i,a]);typeof n==`string`&&(n=ee(n));let{pathname:c=`/`,search:l=``,hash:u=``,state:d=null,key:f=`default`}=n,p=x.useMemo(()=>{let e=ge(c,o);return e==null?null:{location:{pathname:e,search:l,hash:u,state:d,key:f},navigationType:r}},[o,c,l,u,d,f,r]);return T(p!=null,`<Router basename="${o}"> is not able to match the URL "${c}${l}${u}" because it does not start with the basename, so the <Router> won't render anything.`),p==null?null:x.createElement(Fe.Provider,{value:s},x.createElement(Ie.Provider,{children:t,value:p}))}function gt({children:e,location:t}){return qe(_t(e),t)}function _t(e,t=[]){let n=[];return x.Children.forEach(e,(e,r)=>{if(!x.isValidElement(e))return;let i=[...t,r];if(e.type===x.Fragment){n.push.apply(n,_t(e.props.children,i));return}w(e.type===mt,`[${typeof e.type==`string`?e.type:e.type.name}] is not a <Route> component. All component children of <Routes> must be a <Route> or <React.Fragment>`),w(!e.props.index||!e.props.children,`An index route cannot have child routes.`);let a={id:e.props.id||i.join(`-`),caseSensitive:e.props.caseSensitive,element:e.props.element,Component:e.props.Component,index:e.props.index,path:e.props.path,middleware:e.props.middleware,loader:e.props.loader,action:e.props.action,hydrateFallbackElement:e.props.hydrateFallbackElement,HydrateFallback:e.props.HydrateFallback,errorElement:e.props.errorElement,ErrorBoundary:e.props.ErrorBoundary,hasErrorBoundary:e.props.hasErrorBoundary===!0||e.props.ErrorBoundary!=null||e.props.errorElement!=null,shouldRevalidate:e.props.shouldRevalidate,handle:e.props.handle,lazy:e.props.lazy};e.props.children&&(a.children=_t(e.props.children,i)),n.push(a)}),n}var vt=`get`,yt=`application/x-www-form-urlencoded`;function bt(e){return e!=null&&typeof e.tagName==`string`}function xt(e){return bt(e)&&e.tagName.toLowerCase()===`button`}function St(e){return bt(e)&&e.tagName.toLowerCase()===`form`}function Ct(e){return bt(e)&&e.tagName.toLowerCase()===`input`}function wt(e){return!!(e.metaKey||e.altKey||e.ctrlKey||e.shiftKey)}function Tt(e,t){return e.button===0&&(!t||t===`_self`)&&!wt(e)}var Et=null;function Dt(){if(Et===null)try{new FormData(document.createElement(`form`),0),Et=!1}catch{Et=!0}return Et}var Ot=new Set([`application/x-www-form-urlencoded`,`multipart/form-data`,`text/plain`]);function kt(e){return e!=null&&!Ot.has(e)?(T(!1,`"${e}" is not a valid \`encType\` for \`<Form>\`/\`<fetcher.Form>\` and will default to "${yt}"`),null):e}function At(e,t){let n,r,i,a,o;if(St(e)){let o=e.getAttribute(`action`);r=o?ge(o,t):null,n=e.getAttribute(`method`)||vt,i=kt(e.getAttribute(`enctype`))||yt,a=new FormData(e)}else if(xt(e)||Ct(e)&&(e.type===`submit`||e.type===`image`)){let o=e.form;if(o==null)throw Error(`Cannot submit a <button> or <input type="submit"> without a <form>`);let s=e.getAttribute(`formaction`)||o.getAttribute(`action`);if(r=s?ge(s,t):null,n=e.getAttribute(`formmethod`)||o.getAttribute(`method`)||vt,i=kt(e.getAttribute(`formenctype`))||kt(o.getAttribute(`enctype`))||yt,a=new FormData(o,e),!Dt()){let{name:t,type:n,value:r}=e;if(n===`image`){let e=t?`${t}.`:``;a.append(`${e}x`,`0`),a.append(`${e}y`,`0`)}else t&&a.append(t,r)}}else if(bt(e))throw Error(`Cannot submit element that is not <form>, <button>, or <input type="submit|image">`);else n=vt,r=null,i=yt,o=e;return a&&i===`text/plain`&&(o=a,a=void 0),{action:r,method:n.toLowerCase(),encType:i,formData:a,body:o}}Object.getOwnPropertyNames(Object.prototype).sort().join(`\0`);function jt(e,t){if(e===!1||e==null)throw Error(t)}function Mt(e,t,n){let r=typeof e==`string`?new URL(e,typeof window>`u`?`server://singlefetch/`:window.location.origin):e;return r.pathname===`/`?r.pathname=`_root.${n}`:t&&ge(r.pathname,t)===`/`?r.pathname=`${t.replace(/\/$/,``)}/_root.${n}`:r.pathname=`${r.pathname.replace(/\/$/,``)}.${n}`,r}async function Nt(e,t){if(e.id in t)return t[e.id];try{let n=await b(()=>import(e.module),[]);return t[e.id]=n,n}catch(t){return console.error(`Error loading route module \`${e.module}\`, reloading page...`),console.error(t),window.__reactRouterContext&&window.__reactRouterContext.isSpaMode,window.location.reload(),new Promise(()=>{})}}function Pt(e){return e!=null&&typeof e.page==`string`}function Ft(e){return e==null?!1:e.href==null?e.rel===`preload`&&typeof e.imageSrcSet==`string`&&typeof e.imageSizes==`string`:typeof e.rel==`string`&&typeof e.href==`string`}async function It(e,t,n){return Vt((await Promise.all(e.map(async e=>{let r=t.routes[e.route.id];if(r){let e=await Nt(r,n);return e.links?e.links():[]}return[]}))).flat(1).filter(Ft).filter(e=>e.rel===`stylesheet`||e.rel===`preload`).map(e=>e.rel===`stylesheet`?{...e,rel:`prefetch`,as:`style`}:{...e,rel:`prefetch`}))}function Lt(e,t,n,r,i,a){let o=(e,t)=>n[t]?e.route.id!==n[t].route.id:!0,s=(e,t)=>n[t].pathname!==e.pathname||n[t].route.path?.endsWith(`*`)&&n[t].params[`*`]!==e.params[`*`];return a===`assets`?t.filter((e,t)=>o(e,t)||s(e,t)):a===`data`?t.filter((t,a)=>{let c=r.routes[t.route.id];if(!c||!c.hasLoader)return!1;if(o(t,a)||s(t,a))return!0;if(t.route.shouldRevalidate){let r=t.route.shouldRevalidate({currentUrl:new URL(i.pathname+i.search+i.hash,window.origin),currentParams:n[0]?.params||{},nextUrl:new URL(e,window.origin),nextParams:t.params,defaultShouldRevalidate:!0});if(typeof r==`boolean`)return r}return!0}):[]}function Rt(e,t,{includeHydrateFallback:n}={}){return zt(e.map(e=>{let r=t.routes[e.route.id];if(!r)return[];let i=[r.module];return r.clientActionModule&&(i=i.concat(r.clientActionModule)),r.clientLoaderModule&&(i=i.concat(r.clientLoaderModule)),n&&r.hydrateFallbackModule&&(i=i.concat(r.hydrateFallbackModule)),r.imports&&(i=i.concat(r.imports)),i}).flat(1))}function zt(e){return[...new Set(e)]}function Bt(e){let t={},n=Object.keys(e).sort();for(let r of n)t[r]=e[r];return t}function Vt(e,t){let n=new Set,r=new Set(t);return e.reduce((e,i)=>{if(t&&!Pt(i)&&i.as===`script`&&i.href&&r.has(i.href))return e;let a=JSON.stringify(Bt(i));return n.has(a)||(n.add(a),e.push({key:a,link:i})),e},[])}function Ht(){let e=x.useContext(Ae);return jt(e,`You must render this element inside a <DataRouterContext.Provider> element`),e}function Ut(){let e=x.useContext(je);return jt(e,`You must render this element inside a <DataRouterStateContext.Provider> element`),e}var Wt=x.createContext(void 0);Wt.displayName=`FrameworkContext`;function Gt(){let e=x.useContext(Wt);return jt(e,`You must render this element inside a <HydratedRouter> element`),e}function Kt(e,t){let n=x.useContext(Wt),[r,i]=x.useState(!1),[a,o]=x.useState(!1),{onFocus:s,onBlur:c,onMouseEnter:l,onMouseLeave:u,onTouchStart:d}=t,f=x.useRef(null);x.useEffect(()=>{if(e===`render`&&o(!0),e===`viewport`){let e=new IntersectionObserver(e=>{e.forEach(e=>{o(e.isIntersecting)})},{threshold:.5});return f.current&&e.observe(f.current),()=>{e.disconnect()}}},[e]),x.useEffect(()=>{if(r){let e=setTimeout(()=>{o(!0)},100);return()=>{clearTimeout(e)}}},[r]);let p=()=>{i(!0)},m=()=>{i(!1),o(!1)};return n?e===`intent`?[a,f,{onFocus:qt(s,p),onBlur:qt(c,m),onMouseEnter:qt(l,p),onMouseLeave:qt(u,m),onTouchStart:qt(d,p)}]:[a,f,{}]:[!1,f,{}]}function qt(e,t){return n=>{e&&e(n),n.defaultPrevented||t(n)}}function Jt({page:e,...t}){let{router:n}=Ht(),r=x.useMemo(()=>M(n.routes,e,n.basename),[n.routes,e,n.basename]);return r?x.createElement(Xt,{page:e,matches:r,...t}):null}function Yt(e){let{manifest:t,routeModules:n}=Gt(),[r,i]=x.useState([]);return x.useEffect(()=>{let r=!1;return It(e,t,n).then(e=>{r||i(e)}),()=>{r=!0}},[e,t,n]),r}function Xt({page:e,matches:t,...n}){let r=Ve(),{manifest:i,routeModules:a}=Gt(),{basename:o}=Ht(),{loaderData:s,matches:c}=Ut(),l=x.useMemo(()=>Lt(e,t,c,i,r,`data`),[e,t,c,i,r]),u=x.useMemo(()=>Lt(e,t,c,i,r,`assets`),[e,t,c,i,r]),d=x.useMemo(()=>{if(e===r.pathname+r.search+r.hash)return[];let n=new Set,c=!1;if(t.forEach(e=>{let t=i.routes[e.route.id];!t||!t.hasLoader||(!l.some(t=>t.route.id===e.route.id)&&e.route.id in s&&a[e.route.id]?.shouldRevalidate||t.hasClientLoader?c=!0:n.add(e.route.id))}),n.size===0)return[];let u=Mt(e,o,`data`);return c&&n.size>0&&u.searchParams.set(`_routes`,t.filter(e=>n.has(e.route.id)).map(e=>e.route.id).join(`,`)),[u.pathname+u.search]},[o,s,r,i,l,t,e,a]),f=x.useMemo(()=>Rt(u,i),[u,i]),p=Yt(u);return x.createElement(x.Fragment,null,d.map(e=>x.createElement(`link`,{key:e,rel:`prefetch`,as:`fetch`,href:e,...n})),f.map(e=>x.createElement(`link`,{key:e,rel:`modulepreload`,href:e,...n})),p.map(({key:e,link:t})=>x.createElement(`link`,{key:e,nonce:n.nonce,...t})))}function Zt(...e){return t=>{e.forEach(e=>{typeof e==`function`?e(t):e!=null&&(e.current=t)})}}var Qt=typeof window<`u`&&window.document!==void 0&&window.document.createElement!==void 0;try{Qt&&(window.__reactRouterVersion=`7.9.6`)}catch{}function $t({basename:e,children:t,window:n}){let r=x.useRef();r.current??=C({window:n,v5Compat:!0});let i=r.current,[a,o]=x.useState({action:i.action,location:i.location}),s=x.useCallback(e=>{x.startTransition(()=>o(e))},[o]);return x.useLayoutEffect(()=>i.listen(s),[i,s]),x.createElement(ht,{basename:e,children:t,location:a.location,navigationType:a.action,navigator:i})}function en({basename:e,children:t,history:n}){let[r,i]=x.useState({action:n.action,location:n.location}),a=x.useCallback(e=>{x.startTransition(()=>i(e))},[i]);return x.useLayoutEffect(()=>n.listen(a),[n,a]),x.createElement(ht,{basename:e,children:t,location:r.location,navigationType:r.action,navigator:n})}en.displayName=`unstable_HistoryRouter`;var tn=/^(?:[a-z][a-z0-9+.-]*:|\/\/)/i,nn=x.forwardRef(function({onClick:e,discover:t=`render`,prefetch:n=`none`,relative:r,reloadDocument:i,replace:a,state:o,target:s,to:c,preventScrollReset:l,viewTransition:u,...d},f){let{basename:p}=x.useContext(Fe),m=typeof c==`string`&&tn.test(c),h,g=!1;if(typeof c==`string`&&m&&(h=c,Qt))try{let e=new URL(window.location.href),t=c.startsWith(`//`)?new URL(e.protocol+c):new URL(c),n=ge(t.pathname,p);t.origin===e.origin&&n!=null?c=n+t.search+t.hash:g=!0}catch{T(!1,`<Link to="${c}"> contains an invalid URL which will probably break when clicked - please update to a valid URL path.`)}let _=ze(c,{relative:r}),[v,y,b]=Kt(n,d),S=un(c,{replace:a,state:o,target:s,preventScrollReset:l,relative:r,viewTransition:u});function C(t){e&&e(t),t.defaultPrevented||S(t)}let w=x.createElement(`a`,{...d,...b,href:h||_,onClick:g||i?e:C,ref:Zt(f,y),target:s,"data-discover":!m&&t===`render`?`true`:void 0});return v&&!m?x.createElement(x.Fragment,null,w,x.createElement(Jt,{page:_})):w});nn.displayName=`Link`;var rn=x.forwardRef(function({"aria-current":e=`page`,caseSensitive:t=!1,className:n=``,end:r=!1,style:i,to:a,viewTransition:o,children:s,...c},l){let u=Ke(a,{relative:c.relative}),d=Ve(),f=x.useContext(je),{navigator:p,basename:m}=x.useContext(Fe),h=f!=null&&bn(u)&&o===!0,g=p.encodeLocation?p.encodeLocation(u).pathname:u.pathname,_=d.pathname,v=f&&f.navigation&&f.navigation.location?f.navigation.location.pathname:null;t||(_=_.toLowerCase(),v=v?v.toLowerCase():null,g=g.toLowerCase()),v&&m&&(v=ge(v,m)||v);let y=g!==`/`&&g.endsWith(`/`)?g.length-1:g.length,b=_===g||!r&&_.startsWith(g)&&_.charAt(y)===`/`,S=v!=null&&(v===g||!r&&v.startsWith(g)&&v.charAt(g.length)===`/`),C={isActive:b,isPending:S,isTransitioning:h},w=b?e:void 0,T;T=typeof n==`function`?n(C):[n,b?`active`:null,S?`pending`:null,h?`transitioning`:null].filter(Boolean).join(` `);let E=typeof i==`function`?i(C):i;return x.createElement(nn,{...c,"aria-current":w,className:T,ref:l,style:E,to:a,viewTransition:o},typeof s==`function`?s(C):s)});rn.displayName=`NavLink`;var an=x.forwardRef(({discover:e=`render`,fetcherKey:t,navigate:n,reloadDocument:r,replace:i,state:a,method:o=vt,action:s,onSubmit:c,relative:l,preventScrollReset:u,viewTransition:d,...f},p)=>{let m=pn(),h=mn(s,{relative:l}),g=o.toLowerCase()===`get`?`get`:`post`,_=typeof s==`string`&&tn.test(s);return x.createElement(`form`,{ref:p,method:g,action:h,onSubmit:r?c:e=>{if(c&&c(e),e.defaultPrevented)return;e.preventDefault();let r=e.nativeEvent.submitter,s=r?.getAttribute(`formmethod`)||o;m(r||e.currentTarget,{fetcherKey:t,method:s,navigate:n,replace:i,state:a,relative:l,preventScrollReset:u,viewTransition:d})},...f,"data-discover":!_&&e===`render`?`true`:void 0})});an.displayName=`Form`;function on({getKey:e,storageKey:t,...n}){let r=x.useContext(Wt),{basename:i}=x.useContext(Fe),a=Ve(),o=st();vn({getKey:e,storageKey:t});let s=x.useMemo(()=>{if(!r||!e)return null;let t=_n(a,o,i,e);return t===a.key?null:t},[]);if(!r||r.isSpaMode)return null;let c=((e,t)=>{if(!window.history.state||!window.history.state.key){let e=Math.random().toString(32).slice(2);window.history.replaceState({key:e},``)}try{let n=JSON.parse(sessionStorage.getItem(e)||`{}`)[t||window.history.state.key];typeof n==`number`&&window.scrollTo(0,n)}catch(t){console.error(t),sessionStorage.removeItem(e)}}).toString();return x.createElement(`script`,{...n,suppressHydrationWarning:!0,dangerouslySetInnerHTML:{__html:`(${c})(${JSON.stringify(t||hn)}, ${JSON.stringify(s)})`}})}on.displayName=`ScrollRestoration`;function sn(e){return`${e} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`}function cn(e){let t=x.useContext(Ae);return w(t,sn(e)),t}function ln(e){let t=x.useContext(je);return w(t,sn(e)),t}function un(e,{target:t,replace:n,state:r,preventScrollReset:i,relative:a,viewTransition:o}={}){let s=We(),c=Ve(),l=Ke(e,{relative:a});return x.useCallback(u=>{Tt(u,t)&&(u.preventDefault(),s(e,{replace:n===void 0?k(c)===k(l):n,state:r,preventScrollReset:i,relative:a,viewTransition:o}))},[c,s,l,n,r,t,e,i,a,o])}var dn=0,fn=()=>`__${String(++dn)}__`;function pn(){let{router:e}=cn(`useSubmit`),{basename:t}=x.useContext(Fe),n=at();return x.useCallback(async(r,i={})=>{let{action:a,method:o,encType:s,formData:c,body:l}=At(r,t);if(i.navigate===!1){let t=i.fetcherKey||fn();await e.fetch(t,n,i.action||a,{preventScrollReset:i.preventScrollReset,formData:c,body:l,formMethod:i.method||o,formEncType:i.encType||s,flushSync:i.flushSync})}else await e.navigate(i.action||a,{preventScrollReset:i.preventScrollReset,formData:c,body:l,formMethod:i.method||o,formEncType:i.encType||s,replace:i.replace,state:i.state,fromRouteId:n,flushSync:i.flushSync,viewTransition:i.viewTransition})},[e,t,n])}function mn(e,{relative:t}={}){let{basename:n}=x.useContext(Fe),r=x.useContext(Le);w(r,`useFormAction must be used inside a RouteContext`);let[i]=r.matches.slice(-1),a={...Ke(e||`.`,{relative:t})},o=Ve();if(e==null){a.search=o.search;let e=new URLSearchParams(a.search),t=e.getAll(`index`);if(t.some(e=>e===``)){e.delete(`index`),t.filter(e=>e).forEach(t=>e.append(`index`,t));let n=e.toString();a.search=n?`?${n}`:``}}return(!e||e===`.`)&&i.route.index&&(a.search=a.search?a.search.replace(/^\?/,`?index&`):`?index`),n!==`/`&&(a.pathname=a.pathname===`/`?n:Te([n,a.pathname])),k(a)}var hn=`react-router-scroll-positions`,gn={};function _n(e,t,n,r){let i=null;return r&&(i=r(n===`/`?e:{...e,pathname:ge(e.pathname,n)||e.pathname},t)),i??=e.key,i}function vn({getKey:e,storageKey:t}={}){let{router:n}=cn(`useScrollRestoration`),{restoreScrollPosition:r,preventScrollReset:i}=ln(`useScrollRestoration`),{basename:a}=x.useContext(Fe),o=Ve(),s=st(),c=ot();x.useEffect(()=>(window.history.scrollRestoration=`manual`,()=>{window.history.scrollRestoration=`auto`}),[]),yn(x.useCallback(()=>{if(c.state===`idle`){let t=_n(o,s,a,e);gn[t]=window.scrollY}try{sessionStorage.setItem(t||hn,JSON.stringify(gn))}catch(e){T(!1,`Failed to save scroll positions in sessionStorage, <ScrollRestoration /> will not work properly (${e}).`)}window.history.scrollRestoration=`auto`},[c.state,e,a,o,s,t])),typeof document<`u`&&(x.useLayoutEffect(()=>{try{let e=sessionStorage.getItem(t||hn);e&&(gn=JSON.parse(e))}catch{}},[t]),x.useLayoutEffect(()=>{let t=n?.enableScrollRestoration(gn,()=>window.scrollY,e?(t,n)=>_n(t,n,a,e):void 0);return()=>t&&t()},[n,a,e]),x.useLayoutEffect(()=>{if(r!==!1){if(typeof r==`number`){window.scrollTo(0,r);return}try{if(o.hash){let e=document.getElementById(decodeURIComponent(o.hash.slice(1)));if(e){e.scrollIntoView();return}}}catch{T(!1,`"${o.hash.slice(1)}" is not a decodable element ID. The view will not scroll to it.`)}i!==!0&&window.scrollTo(0,0)}},[o,r,i]))}function yn(e,t){let{capture:n}=t||{};x.useEffect(()=>{let t=n==null?void 0:{capture:n};return window.addEventListener(`pagehide`,e,t),()=>{window.removeEventListener(`pagehide`,e,t)}},[e,n])}function bn(e,{relative:t}={}){let n=x.useContext(Me);w(n!=null,"`useViewTransitionState` must be used within `react-router-dom`'s `RouterProvider`.  Did you accidentally import `RouterProvider` from `react-router`?");let{basename:r}=cn(`useViewTransitionState`),i=Ke(e,{relative:t});if(!n.isTransitioning)return!1;let a=ge(n.currentLocation.pathname,r)||n.currentLocation.pathname,o=ge(n.nextLocation.pathname,r)||n.nextLocation.pathname;return pe(i.pathname,o)!=null||pe(i.pathname,a)!=null}var xn={black:`#000`,white:`#fff`},Sn={50:`#ffebee`,100:`#ffcdd2`,200:`#ef9a9a`,300:`#e57373`,400:`#ef5350`,500:`#f44336`,600:`#e53935`,700:`#d32f2f`,800:`#c62828`,900:`#b71c1c`,A100:`#ff8a80`,A200:`#ff5252`,A400:`#ff1744`,A700:`#d50000`},Cn={50:`#f3e5f5`,100:`#e1bee7`,200:`#ce93d8`,300:`#ba68c8`,400:`#ab47bc`,500:`#9c27b0`,600:`#8e24aa`,700:`#7b1fa2`,800:`#6a1b9a`,900:`#4a148c`,A100:`#ea80fc`,A200:`#e040fb`,A400:`#d500f9`,A700:`#aa00ff`},wn={50:`#e3f2fd`,100:`#bbdefb`,200:`#90caf9`,300:`#64b5f6`,400:`#42a5f5`,500:`#2196f3`,600:`#1e88e5`,700:`#1976d2`,800:`#1565c0`,900:`#0d47a1`,A100:`#82b1ff`,A200:`#448aff`,A400:`#2979ff`,A700:`#2962ff`},Tn={50:`#e1f5fe`,100:`#b3e5fc`,200:`#81d4fa`,300:`#4fc3f7`,400:`#29b6f6`,500:`#03a9f4`,600:`#039be5`,700:`#0288d1`,800:`#0277bd`,900:`#01579b`,A100:`#80d8ff`,A200:`#40c4ff`,A400:`#00b0ff`,A700:`#0091ea`},En={50:`#e8f5e9`,100:`#c8e6c9`,200:`#a5d6a7`,300:`#81c784`,400:`#66bb6a`,500:`#4caf50`,600:`#43a047`,700:`#388e3c`,800:`#2e7d32`,900:`#1b5e20`,A100:`#b9f6ca`,A200:`#69f0ae`,A400:`#00e676`,A700:`#00c853`},Dn={50:`#fff3e0`,100:`#ffe0b2`,200:`#ffcc80`,300:`#ffb74d`,400:`#ffa726`,500:`#ff9800`,600:`#fb8c00`,700:`#f57c00`,800:`#ef6c00`,900:`#e65100`,A100:`#ffd180`,A200:`#ffab40`,A400:`#ff9100`,A700:`#ff6d00`},On={50:`#fafafa`,100:`#f5f5f5`,200:`#eeeeee`,300:`#e0e0e0`,400:`#bdbdbd`,500:`#9e9e9e`,600:`#757575`,700:`#616161`,800:`#424242`,900:`#212121`,A100:`#f5f5f5`,A200:`#eeeeee`,A400:`#bdbdbd`,A700:`#616161`};function kn(e,...t){let n=new URL(`https://mui.com/production-error/?code=${e}`);return t.forEach(e=>n.searchParams.append(`args[]`,e)),`Minified MUI error #${e}; visit ${n} for the full message.`}var An=`$$material`;function jn(){return jn=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)({}).hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e},jn.apply(null,arguments)}var Mn=!1;function Nn(e){if(e.sheet)return e.sheet;for(var t=0;t<document.styleSheets.length;t++)if(document.styleSheets[t].ownerNode===e)return document.styleSheets[t]}function Pn(e){var t=document.createElement(`style`);return t.setAttribute(`data-emotion`,e.key),e.nonce!==void 0&&t.setAttribute(`nonce`,e.nonce),t.appendChild(document.createTextNode(``)),t.setAttribute(`data-s`,``),t}var Fn=function(){function e(e){var t=this;this._insertTag=function(e){var n=t.tags.length===0?t.insertionPoint?t.insertionPoint.nextSibling:t.prepend?t.container.firstChild:t.before:t.tags[t.tags.length-1].nextSibling;t.container.insertBefore(e,n),t.tags.push(e)},this.isSpeedy=e.speedy===void 0?!Mn:e.speedy,this.tags=[],this.ctr=0,this.nonce=e.nonce,this.key=e.key,this.container=e.container,this.prepend=e.prepend,this.insertionPoint=e.insertionPoint,this.before=null}var t=e.prototype;return t.hydrate=function(e){e.forEach(this._insertTag)},t.insert=function(e){this.ctr%(this.isSpeedy?65e3:1)==0&&this._insertTag(Pn(this));var t=this.tags[this.tags.length-1];if(this.isSpeedy){var n=Nn(t);try{n.insertRule(e,n.cssRules.length)}catch{}}else t.appendChild(document.createTextNode(e));this.ctr++},t.flush=function(){this.tags.forEach(function(e){return e.parentNode?.removeChild(e)}),this.tags=[],this.ctr=0},e}(),In=`-ms-`,Ln=`-moz-`,Rn=`-webkit-`,zn=`comm`,Bn=`rule`,Vn=`decl`,Hn=`@import`,Un=`@keyframes`,Wn=`@layer`,Gn=Math.abs,Kn=String.fromCharCode,qn=Object.assign;function Jn(e,t){return $n(e,0)^45?(((t<<2^$n(e,0))<<2^$n(e,1))<<2^$n(e,2))<<2^$n(e,3):0}function Yn(e){return e.trim()}function Xn(e,t){return(e=t.exec(e))?e[0]:e}function Zn(e,t,n){return e.replace(t,n)}function Qn(e,t){return e.indexOf(t)}function $n(e,t){return e.charCodeAt(t)|0}function er(e,t,n){return e.slice(t,n)}function tr(e){return e.length}function nr(e){return e.length}function rr(e,t){return t.push(e),e}function ir(e,t){return e.map(t).join(``)}var ar=1,or=1,sr=0,cr=0,lr=0,ur=``;function dr(e,t,n,r,i,a,o){return{value:e,root:t,parent:n,type:r,props:i,children:a,line:ar,column:or,length:o,return:``}}function fr(e,t){return qn(dr(``,null,null,``,null,null,0),e,{length:-e.length},t)}function pr(){return lr}function mr(){return lr=cr>0?$n(ur,--cr):0,or--,lr===10&&(or=1,ar--),lr}function hr(){return lr=cr<sr?$n(ur,cr++):0,or++,lr===10&&(or=1,ar++),lr}function gr(){return $n(ur,cr)}function _r(){return cr}function vr(e,t){return er(ur,e,t)}function yr(e){switch(e){case 0:case 9:case 10:case 13:case 32:return 5;case 33:case 43:case 44:case 47:case 62:case 64:case 126:case 59:case 123:case 125:return 4;case 58:return 3;case 34:case 39:case 40:case 91:return 2;case 41:case 93:return 1}return 0}function br(e){return ar=or=1,sr=tr(ur=e),cr=0,[]}function xr(e){return ur=``,e}function Sr(e){return Yn(vr(cr-1,Tr(e===91?e+2:e===40?e+1:e)))}function Cr(e){for(;(lr=gr())&&lr<33;)hr();return yr(e)>2||yr(lr)>3?``:` `}function wr(e,t){for(;--t&&hr()&&!(lr<48||lr>102||lr>57&&lr<65||lr>70&&lr<97););return vr(e,_r()+(t<6&&gr()==32&&hr()==32))}function Tr(e){for(;hr();)switch(lr){case e:return cr;case 34:case 39:e!==34&&e!==39&&Tr(lr);break;case 40:e===41&&Tr(e);break;case 92:hr();break}return cr}function Er(e,t){for(;hr()&&e+lr!==57&&!(e+lr===84&&gr()===47););return`/*`+vr(t,cr-1)+`*`+Kn(e===47?e:hr())}function Dr(e){for(;!yr(gr());)hr();return vr(e,cr)}function Or(e){return xr(kr(``,null,null,null,[``],e=br(e),0,[0],e))}function kr(e,t,n,r,i,a,o,s,c){for(var l=0,u=0,d=o,f=0,p=0,m=0,h=1,g=1,_=1,v=0,y=``,b=i,x=a,S=r,C=y;g;)switch(m=v,v=hr()){case 40:if(m!=108&&$n(C,d-1)==58){Qn(C+=Zn(Sr(v),`&`,`&\f`),`&\f`)!=-1&&(_=-1);break}case 34:case 39:case 91:C+=Sr(v);break;case 9:case 10:case 13:case 32:C+=Cr(m);break;case 92:C+=wr(_r()-1,7);continue;case 47:switch(gr()){case 42:case 47:rr(jr(Er(hr(),_r()),t,n),c);break;default:C+=`/`}break;case 123*h:s[l++]=tr(C)*_;case 125*h:case 59:case 0:switch(v){case 0:case 125:g=0;case 59+u:_==-1&&(C=Zn(C,/\f/g,``)),p>0&&tr(C)-d&&rr(p>32?Mr(C+`;`,r,n,d-1):Mr(Zn(C,` `,``)+`;`,r,n,d-2),c);break;case 59:C+=`;`;default:if(rr(S=Ar(C,t,n,l,u,i,s,y,b=[],x=[],d),a),v===123)if(u===0)kr(C,t,S,S,b,a,d,s,x);else switch(f===99&&$n(C,3)===110?100:f){case 100:case 108:case 109:case 115:kr(e,S,S,r&&rr(Ar(e,S,S,0,0,i,s,y,i,b=[],d),x),i,x,d,s,r?b:x);break;default:kr(C,S,S,S,[``],x,0,s,x)}}l=u=p=0,h=_=1,y=C=``,d=o;break;case 58:d=1+tr(C),p=m;default:if(h<1){if(v==123)--h;else if(v==125&&h++==0&&mr()==125)continue}switch(C+=Kn(v),v*h){case 38:_=u>0?1:(C+=`\f`,-1);break;case 44:s[l++]=(tr(C)-1)*_,_=1;break;case 64:gr()===45&&(C+=Sr(hr())),f=gr(),u=d=tr(y=C+=Dr(_r())),v++;break;case 45:m===45&&tr(C)==2&&(h=0)}}return a}function Ar(e,t,n,r,i,a,o,s,c,l,u){for(var d=i-1,f=i===0?a:[``],p=nr(f),m=0,h=0,g=0;m<r;++m)for(var _=0,v=er(e,d+1,d=Gn(h=o[m])),y=e;_<p;++_)(y=Yn(h>0?f[_]+` `+v:Zn(v,/&\f/g,f[_])))&&(c[g++]=y);return dr(e,t,n,i===0?Bn:s,c,l,u)}function jr(e,t,n){return dr(e,t,n,zn,Kn(pr()),er(e,2,-2),0)}function Mr(e,t,n,r){return dr(e,t,n,Vn,er(e,0,r),er(e,r+1,-1),r)}function Nr(e,t){for(var n=``,r=nr(e),i=0;i<r;i++)n+=t(e[i],i,e,t)||``;return n}function Pr(e,t,n,r){switch(e.type){case Wn:if(e.children.length)break;case Hn:case Vn:return e.return=e.return||e.value;case zn:return``;case Un:return e.return=e.value+`{`+Nr(e.children,r)+`}`;case Bn:e.value=e.props.join(`,`)}return tr(n=Nr(e.children,r))?e.return=e.value+`{`+n+`}`:``}function Fr(e){var t=nr(e);return function(n,r,i,a){for(var o=``,s=0;s<t;s++)o+=e[s](n,r,i,a)||``;return o}}function Ir(e){return function(t){t.root||(t=t.return)&&e(t)}}function Lr(e){var t=Object.create(null);return function(n){return t[n]===void 0&&(t[n]=e(n)),t[n]}}var Rr=function(e,t,n){for(var r=0,i=0;r=i,i=gr(),r===38&&i===12&&(t[n]=1),!yr(i);)hr();return vr(e,cr)},zr=function(e,t){var n=-1,r=44;do switch(yr(r)){case 0:r===38&&gr()===12&&(t[n]=1),e[n]+=Rr(cr-1,t,n);break;case 2:e[n]+=Sr(r);break;case 4:if(r===44){e[++n]=gr()===58?`&\f`:``,t[n]=e[n].length;break}default:e[n]+=Kn(r)}while(r=hr());return e},Br=function(e,t){return xr(zr(br(e),t))},Vr=new WeakMap,Hr=function(e){if(!(e.type!==`rule`||!e.parent||e.length<1)){for(var t=e.value,n=e.parent,r=e.column===n.column&&e.line===n.line;n.type!==`rule`;)if(n=n.parent,!n)return;if(!(e.props.length===1&&t.charCodeAt(0)!==58&&!Vr.get(n))&&!r){Vr.set(e,!0);for(var i=[],a=Br(t,i),o=n.props,s=0,c=0;s<a.length;s++)for(var l=0;l<o.length;l++,c++)e.props[c]=i[s]?a[s].replace(/&\f/g,o[l]):o[l]+` `+a[s]}}},Ur=function(e){if(e.type===`decl`){var t=e.value;t.charCodeAt(0)===108&&t.charCodeAt(2)===98&&(e.return=``,e.value=``)}};function Wr(e,t){switch(Jn(e,t)){case 5103:return Rn+`print-`+e+e;case 5737:case 4201:case 3177:case 3433:case 1641:case 4457:case 2921:case 5572:case 6356:case 5844:case 3191:case 6645:case 3005:case 6391:case 5879:case 5623:case 6135:case 4599:case 4855:case 4215:case 6389:case 5109:case 5365:case 5621:case 3829:return Rn+e+e;case 5349:case 4246:case 4810:case 6968:case 2756:return Rn+e+Ln+e+In+e+e;case 6828:case 4268:return Rn+e+In+e+e;case 6165:return Rn+e+In+`flex-`+e+e;case 5187:return Rn+e+Zn(e,/(\w+).+(:[^]+)/,Rn+`box-$1$2`+In+`flex-$1$2`)+e;case 5443:return Rn+e+In+`flex-item-`+Zn(e,/flex-|-self/,``)+e;case 4675:return Rn+e+In+`flex-line-pack`+Zn(e,/align-content|flex-|-self/,``)+e;case 5548:return Rn+e+In+Zn(e,`shrink`,`negative`)+e;case 5292:return Rn+e+In+Zn(e,`basis`,`preferred-size`)+e;case 6060:return Rn+`box-`+Zn(e,`-grow`,``)+Rn+e+In+Zn(e,`grow`,`positive`)+e;case 4554:return Rn+Zn(e,/([^-])(transform)/g,`$1`+Rn+`$2`)+e;case 6187:return Zn(Zn(Zn(e,/(zoom-|grab)/,Rn+`$1`),/(image-set)/,Rn+`$1`),e,``)+e;case 5495:case 3959:return Zn(e,/(image-set\([^]*)/,Rn+"$1$`$1");case 4968:return Zn(Zn(e,/(.+:)(flex-)?(.*)/,Rn+`box-pack:$3`+In+`flex-pack:$3`),/s.+-b[^;]+/,`justify`)+Rn+e+e;case 4095:case 3583:case 4068:case 2532:return Zn(e,/(.+)-inline(.+)/,Rn+`$1$2`)+e;case 8116:case 7059:case 5753:case 5535:case 5445:case 5701:case 4933:case 4677:case 5533:case 5789:case 5021:case 4765:if(tr(e)-1-t>6)switch($n(e,t+1)){case 109:if($n(e,t+4)!==45)break;case 102:return Zn(e,/(.+:)(.+)-([^]+)/,`$1`+Rn+`$2-$3$1`+Ln+($n(e,t+3)==108?`$3`:`$2-$3`))+e;case 115:return~Qn(e,`stretch`)?Wr(Zn(e,`stretch`,`fill-available`),t)+e:e}break;case 4949:if($n(e,t+1)!==115)break;case 6444:switch($n(e,tr(e)-3-(~Qn(e,`!important`)&&10))){case 107:return Zn(e,`:`,`:`+Rn)+e;case 101:return Zn(e,/(.+:)([^;!]+)(;|!.+)?/,`$1`+Rn+($n(e,14)===45?`inline-`:``)+`box$3$1`+Rn+`$2$3$1`+In+`$2box$3`)+e}break;case 5936:switch($n(e,t+11)){case 114:return Rn+e+In+Zn(e,/[svh]\w+-[tblr]{2}/,`tb`)+e;case 108:return Rn+e+In+Zn(e,/[svh]\w+-[tblr]{2}/,`tb-rl`)+e;case 45:return Rn+e+In+Zn(e,/[svh]\w+-[tblr]{2}/,`lr`)+e}return Rn+e+In+e+e}return e}var Gr=[function(e,t,n,r){if(e.length>-1&&!e.return)switch(e.type){case Vn:e.return=Wr(e.value,e.length);break;case Un:return Nr([fr(e,{value:Zn(e.value,`@`,`@`+Rn)})],r);case Bn:if(e.length)return ir(e.props,function(t){switch(Xn(t,/(::plac\w+|:read-\w+)/)){case`:read-only`:case`:read-write`:return Nr([fr(e,{props:[Zn(t,/:(read-\w+)/,`:`+Ln+`$1`)]})],r);case`::placeholder`:return Nr([fr(e,{props:[Zn(t,/:(plac\w+)/,`:`+Rn+`input-$1`)]}),fr(e,{props:[Zn(t,/:(plac\w+)/,`:`+Ln+`$1`)]}),fr(e,{props:[Zn(t,/:(plac\w+)/,In+`input-$1`)]})],r)}return``})}}],Kr=function(e){var t=e.key;if(t===`css`){var n=document.querySelectorAll(`style[data-emotion]:not([data-s])`);Array.prototype.forEach.call(n,function(e){e.getAttribute(`data-emotion`).indexOf(` `)!==-1&&(document.head.appendChild(e),e.setAttribute(`data-s`,``))})}var r=e.stylisPlugins||Gr,i={},a,o=[];a=e.container||document.head,Array.prototype.forEach.call(document.querySelectorAll(`style[data-emotion^="`+t+` "]`),function(e){for(var t=e.getAttribute(`data-emotion`).split(` `),n=1;n<t.length;n++)i[t[n]]=!0;o.push(e)});var s,c=[Hr,Ur],l,u=[Pr,Ir(function(e){l.insert(e)})],d=Fr(c.concat(r,u)),f=function(e){return Nr(Or(e),d)};s=function(e,t,n,r){l=n,f(e?e+`{`+t.styles+`}`:t.styles),r&&(p.inserted[t.name]=!0)};var p={key:t,sheet:new Fn({key:t,container:a,nonce:e.nonce,speedy:e.speedy,prepend:e.prepend,insertionPoint:e.insertionPoint}),nonce:e.nonce,inserted:i,registered:{},insert:s};return p.sheet.hydrate(o),p},qr=o((e=>{var t=typeof Symbol==`function`&&Symbol.for,n=t?Symbol.for(`react.element`):60103,r=t?Symbol.for(`react.portal`):60106,i=t?Symbol.for(`react.fragment`):60107,a=t?Symbol.for(`react.strict_mode`):60108,o=t?Symbol.for(`react.profiler`):60114,s=t?Symbol.for(`react.provider`):60109,c=t?Symbol.for(`react.context`):60110,l=t?Symbol.for(`react.async_mode`):60111,u=t?Symbol.for(`react.concurrent_mode`):60111,d=t?Symbol.for(`react.forward_ref`):60112,f=t?Symbol.for(`react.suspense`):60113,p=t?Symbol.for(`react.suspense_list`):60120,m=t?Symbol.for(`react.memo`):60115,h=t?Symbol.for(`react.lazy`):60116,g=t?Symbol.for(`react.block`):60121,_=t?Symbol.for(`react.fundamental`):60117,v=t?Symbol.for(`react.responder`):60118,y=t?Symbol.for(`react.scope`):60119;function b(e){if(typeof e==`object`&&e){var t=e.$$typeof;switch(t){case n:switch(e=e.type,e){case l:case u:case i:case o:case a:case f:return e;default:switch(e&&=e.$$typeof,e){case c:case d:case h:case m:case s:return e;default:return t}}case r:return t}}}function x(e){return b(e)===u}e.AsyncMode=l,e.ConcurrentMode=u,e.ContextConsumer=c,e.ContextProvider=s,e.Element=n,e.ForwardRef=d,e.Fragment=i,e.Lazy=h,e.Memo=m,e.Portal=r,e.Profiler=o,e.StrictMode=a,e.Suspense=f,e.isAsyncMode=function(e){return x(e)||b(e)===l},e.isConcurrentMode=x,e.isContextConsumer=function(e){return b(e)===c},e.isContextProvider=function(e){return b(e)===s},e.isElement=function(e){return typeof e==`object`&&!!e&&e.$$typeof===n},e.isForwardRef=function(e){return b(e)===d},e.isFragment=function(e){return b(e)===i},e.isLazy=function(e){return b(e)===h},e.isMemo=function(e){return b(e)===m},e.isPortal=function(e){return b(e)===r},e.isProfiler=function(e){return b(e)===o},e.isStrictMode=function(e){return b(e)===a},e.isSuspense=function(e){return b(e)===f},e.isValidElementType=function(e){return typeof e==`string`||typeof e==`function`||e===i||e===u||e===o||e===a||e===f||e===p||typeof e==`object`&&!!e&&(e.$$typeof===h||e.$$typeof===m||e.$$typeof===s||e.$$typeof===c||e.$$typeof===d||e.$$typeof===_||e.$$typeof===v||e.$$typeof===y||e.$$typeof===g)},e.typeOf=b})),Jr=o(((e,t)=>{t.exports=qr()})),Yr=o(((e,t)=>{var n=Jr(),r={childContextTypes:!0,contextType:!0,contextTypes:!0,defaultProps:!0,displayName:!0,getDefaultProps:!0,getDerivedStateFromError:!0,getDerivedStateFromProps:!0,mixins:!0,propTypes:!0,type:!0},i={name:!0,length:!0,prototype:!0,caller:!0,callee:!0,arguments:!0,arity:!0},a={$$typeof:!0,render:!0,defaultProps:!0,displayName:!0,propTypes:!0},o={$$typeof:!0,compare:!0,defaultProps:!0,displayName:!0,propTypes:!0,type:!0},s={};s[n.ForwardRef]=a,s[n.Memo]=o;function c(e){return n.isMemo(e)?o:s[e.$$typeof]||r}var l=Object.defineProperty,u=Object.getOwnPropertyNames,d=Object.getOwnPropertySymbols,f=Object.getOwnPropertyDescriptor,p=Object.getPrototypeOf,m=Object.prototype;function h(e,t,n){if(typeof t!=`string`){if(m){var r=p(t);r&&r!==m&&h(e,r,n)}var a=u(t);d&&(a=a.concat(d(t)));for(var o=c(e),s=c(t),g=0;g<a.length;++g){var _=a[g];if(!i[_]&&!(n&&n[_])&&!(s&&s[_])&&!(o&&o[_])){var v=f(t,_);try{l(e,_,v)}catch{}}}}return e}t.exports=h})),Xr=!0;function Zr(e,t,n){var r=``;return n.split(` `).forEach(function(n){e[n]===void 0?n&&(r+=n+` `):t.push(e[n]+`;`)}),r}var Qr=function(e,t,n){var r=e.key+`-`+t.name;(n===!1||Xr===!1)&&e.registered[r]===void 0&&(e.registered[r]=t.styles)},$r=function(e,t,n){Qr(e,t,n);var r=e.key+`-`+t.name;if(e.inserted[t.name]===void 0){var i=t;do e.insert(t===i?`.`+r:``,i,e.sheet,!0),i=i.next;while(i!==void 0)}};function ei(e){for(var t=0,n,r=0,i=e.length;i>=4;++r,i-=4)n=e.charCodeAt(r)&255|(e.charCodeAt(++r)&255)<<8|(e.charCodeAt(++r)&255)<<16|(e.charCodeAt(++r)&255)<<24,n=(n&65535)*1540483477+((n>>>16)*59797<<16),n^=n>>>24,t=(n&65535)*1540483477+((n>>>16)*59797<<16)^(t&65535)*1540483477+((t>>>16)*59797<<16);switch(i){case 3:t^=(e.charCodeAt(r+2)&255)<<16;case 2:t^=(e.charCodeAt(r+1)&255)<<8;case 1:t^=e.charCodeAt(r)&255,t=(t&65535)*1540483477+((t>>>16)*59797<<16)}return t^=t>>>13,t=(t&65535)*1540483477+((t>>>16)*59797<<16),((t^t>>>15)>>>0).toString(36)}var ti={animationIterationCount:1,aspectRatio:1,borderImageOutset:1,borderImageSlice:1,borderImageWidth:1,boxFlex:1,boxFlexGroup:1,boxOrdinalGroup:1,columnCount:1,columns:1,flex:1,flexGrow:1,flexPositive:1,flexShrink:1,flexNegative:1,flexOrder:1,gridRow:1,gridRowEnd:1,gridRowSpan:1,gridRowStart:1,gridColumn:1,gridColumnEnd:1,gridColumnSpan:1,gridColumnStart:1,msGridRow:1,msGridRowSpan:1,msGridColumn:1,msGridColumnSpan:1,fontWeight:1,lineHeight:1,opacity:1,order:1,orphans:1,scale:1,tabSize:1,widows:1,zIndex:1,zoom:1,WebkitLineClamp:1,fillOpacity:1,floodOpacity:1,stopOpacity:1,strokeDasharray:1,strokeDashoffset:1,strokeMiterlimit:1,strokeOpacity:1,strokeWidth:1},ni=!1,ri=/[A-Z]|^ms/g,ii=/_EMO_([^_]+?)_([^]*?)_EMO_/g,ai=function(e){return e.charCodeAt(1)===45},oi=function(e){return e!=null&&typeof e!=`boolean`},si=Lr(function(e){return ai(e)?e:e.replace(ri,`-$&`).toLowerCase()}),ci=function(e,t){switch(e){case`animation`:case`animationName`:if(typeof t==`string`)return t.replace(ii,function(e,t,n){return pi={name:t,styles:n,next:pi},t})}return ti[e]!==1&&!ai(e)&&typeof t==`number`&&t!==0?t+`px`:t},li=`Component selectors can only be used in conjunction with @emotion/babel-plugin, the swc Emotion plugin, or another Emotion-aware compiler transform.`;function ui(e,t,n){if(n==null)return``;var r=n;if(r.__emotion_styles!==void 0)return r;switch(typeof n){case`boolean`:return``;case`object`:var i=n;if(i.anim===1)return pi={name:i.name,styles:i.styles,next:pi},i.name;var a=n;if(a.styles!==void 0){var o=a.next;if(o!==void 0)for(;o!==void 0;)pi={name:o.name,styles:o.styles,next:pi},o=o.next;return a.styles+`;`}return di(e,t,n);case`function`:if(e!==void 0){var s=pi,c=n(e);return pi=s,ui(e,t,c)}break}var l=n;if(t==null)return l;var u=t[l];return u===void 0?l:u}function di(e,t,n){var r=``;if(Array.isArray(n))for(var i=0;i<n.length;i++)r+=ui(e,t,n[i])+`;`;else for(var a in n){var o=n[a];if(typeof o!=`object`){var s=o;t!=null&&t[s]!==void 0?r+=a+`{`+t[s]+`}`:oi(s)&&(r+=si(a)+`:`+ci(a,s)+`;`)}else{if(a===`NO_COMPONENT_SELECTOR`&&ni)throw Error(li);if(Array.isArray(o)&&typeof o[0]==`string`&&(t==null||t[o[0]]===void 0))for(var c=0;c<o.length;c++)oi(o[c])&&(r+=si(a)+`:`+ci(a,o[c])+`;`);else{var l=ui(e,t,o);switch(a){case`animation`:case`animationName`:r+=si(a)+`:`+l+`;`;break;default:r+=a+`{`+l+`}`}}}}return r}var fi=/label:\s*([^\s;{]+)\s*(;|$)/g,pi;function mi(e,t,n){if(e.length===1&&typeof e[0]==`object`&&e[0]!==null&&e[0].styles!==void 0)return e[0];var r=!0,i=``;pi=void 0;var a=e[0];a==null||a.raw===void 0?(r=!1,i+=ui(n,t,a)):i+=a[0];for(var o=1;o<e.length;o++)i+=ui(n,t,e[o]),r&&(i+=a[o]);fi.lastIndex=0;for(var s=``,c;(c=fi.exec(i))!==null;)s+=`-`+c[1];return{name:ei(i)+s,styles:i,next:pi}}var hi=function(e){return e()},gi=x.useInsertionEffect?x.useInsertionEffect:!1,_i=gi||hi,vi=gi||x.useLayoutEffect,yi=x.createContext(typeof HTMLElement<`u`?Kr({key:`css`}):null);yi.Provider;var bi=function(e){return(0,x.forwardRef)(function(t,n){return e(t,(0,x.useContext)(yi),n)})},xi=x.createContext({}),Si={}.hasOwnProperty,Ci=`__EMOTION_TYPE_PLEASE_DO_NOT_USE__`,wi=function(e,t){var n={};for(var r in t)Si.call(t,r)&&(n[r]=t[r]);return n[Ci]=e,n},Ti=function(e){var t=e.cache,n=e.serialized,r=e.isStringTag;return Qr(t,n,r),_i(function(){return $r(t,n,r)}),null},Ei=bi(function(e,t,n){var r=e.css;typeof r==`string`&&t.registered[r]!==void 0&&(r=t.registered[r]);var i=e[Ci],a=[r],o=``;typeof e.className==`string`?o=Zr(t.registered,a,e.className):e.className!=null&&(o=e.className+` `);var s=mi(a,void 0,x.useContext(xi));o+=t.key+`-`+s.name;var c={};for(var l in e)Si.call(e,l)&&l!==`css`&&l!==Ci&&(c[l]=e[l]);return c.className=o,n&&(c.ref=n),x.createElement(x.Fragment,null,x.createElement(Ti,{cache:t,serialized:s,isStringTag:typeof i==`string`}),x.createElement(i,c))});Yr();var Di=function(e,t){var n=arguments;if(t==null||!Si.call(t,`css`))return x.createElement.apply(void 0,n);var r=n.length,i=Array(r);i[0]=Ei,i[1]=wi(e,t);for(var a=2;a<r;a++)i[a]=n[a];return x.createElement.apply(null,i)};(function(e){var t;(function(e){})(t||=e.JSX||={})})(Di||={});var Oi=bi(function(e,t){var n=e.styles,r=mi([n],void 0,x.useContext(xi)),i=x.useRef();return vi(function(){var e=t.key+`-global`,n=new t.sheet.constructor({key:e,nonce:t.sheet.nonce,container:t.sheet.container,speedy:t.sheet.isSpeedy}),a=!1,o=document.querySelector(`style[data-emotion="`+e+` `+r.name+`"]`);return t.sheet.tags.length&&(n.before=t.sheet.tags[0]),o!==null&&(a=!0,o.setAttribute(`data-emotion`,e),n.hydrate([o])),i.current=[n,a],function(){n.flush()}},[t]),vi(function(){var e=i.current,n=e[0];if(e[1]){e[1]=!1;return}r.next!==void 0&&$r(t,r.next,!0),n.tags.length&&(n.before=n.tags[n.tags.length-1].nextElementSibling,n.flush()),t.insert(``,r,n,!1)},[t,r.name]),null});function ki(){return mi([...arguments])}function Ai(){var e=ki.apply(void 0,arguments),t=`animation-`+e.name;return{name:t,styles:`@keyframes `+t+`{`+e.styles+`}`,anim:1,toString:function(){return`_EMO_`+this.name+`_`+this.styles+`_EMO_`}}}var ji=/^((children|dangerouslySetInnerHTML|key|ref|autoFocus|defaultValue|defaultChecked|innerHTML|suppressContentEditableWarning|suppressHydrationWarning|valueLink|abbr|accept|acceptCharset|accessKey|action|allow|allowUserMedia|allowPaymentRequest|allowFullScreen|allowTransparency|alt|async|autoComplete|autoPlay|capture|cellPadding|cellSpacing|challenge|charSet|checked|cite|classID|className|cols|colSpan|content|contentEditable|contextMenu|controls|controlsList|coords|crossOrigin|data|dateTime|decoding|default|defer|dir|disabled|disablePictureInPicture|disableRemotePlayback|download|draggable|encType|enterKeyHint|fetchpriority|fetchPriority|form|formAction|formEncType|formMethod|formNoValidate|formTarget|frameBorder|headers|height|hidden|high|href|hrefLang|htmlFor|httpEquiv|id|inputMode|integrity|is|keyParams|keyType|kind|label|lang|list|loading|loop|low|marginHeight|marginWidth|max|maxLength|media|mediaGroup|method|min|minLength|multiple|muted|name|nonce|noValidate|open|optimum|pattern|placeholder|playsInline|popover|popoverTarget|popoverTargetAction|poster|preload|profile|radioGroup|readOnly|referrerPolicy|rel|required|reversed|role|rows|rowSpan|sandbox|scope|scoped|scrolling|seamless|selected|shape|size|sizes|slot|span|spellCheck|src|srcDoc|srcLang|srcSet|start|step|style|summary|tabIndex|target|title|translate|type|useMap|value|width|wmode|wrap|about|datatype|inlist|prefix|property|resource|typeof|vocab|autoCapitalize|autoCorrect|autoSave|color|incremental|fallback|inert|itemProp|itemScope|itemType|itemID|itemRef|on|option|results|security|unselectable|accentHeight|accumulate|additive|alignmentBaseline|allowReorder|alphabetic|amplitude|arabicForm|ascent|attributeName|attributeType|autoReverse|azimuth|baseFrequency|baselineShift|baseProfile|bbox|begin|bias|by|calcMode|capHeight|clip|clipPathUnits|clipPath|clipRule|colorInterpolation|colorInterpolationFilters|colorProfile|colorRendering|contentScriptType|contentStyleType|cursor|cx|cy|d|decelerate|descent|diffuseConstant|direction|display|divisor|dominantBaseline|dur|dx|dy|edgeMode|elevation|enableBackground|end|exponent|externalResourcesRequired|fill|fillOpacity|fillRule|filter|filterRes|filterUnits|floodColor|floodOpacity|focusable|fontFamily|fontSize|fontSizeAdjust|fontStretch|fontStyle|fontVariant|fontWeight|format|from|fr|fx|fy|g1|g2|glyphName|glyphOrientationHorizontal|glyphOrientationVertical|glyphRef|gradientTransform|gradientUnits|hanging|horizAdvX|horizOriginX|ideographic|imageRendering|in|in2|intercept|k|k1|k2|k3|k4|kernelMatrix|kernelUnitLength|kerning|keyPoints|keySplines|keyTimes|lengthAdjust|letterSpacing|lightingColor|limitingConeAngle|local|markerEnd|markerMid|markerStart|markerHeight|markerUnits|markerWidth|mask|maskContentUnits|maskUnits|mathematical|mode|numOctaves|offset|opacity|operator|order|orient|orientation|origin|overflow|overlinePosition|overlineThickness|panose1|paintOrder|pathLength|patternContentUnits|patternTransform|patternUnits|pointerEvents|points|pointsAtX|pointsAtY|pointsAtZ|preserveAlpha|preserveAspectRatio|primitiveUnits|r|radius|refX|refY|renderingIntent|repeatCount|repeatDur|requiredExtensions|requiredFeatures|restart|result|rotate|rx|ry|scale|seed|shapeRendering|slope|spacing|specularConstant|specularExponent|speed|spreadMethod|startOffset|stdDeviation|stemh|stemv|stitchTiles|stopColor|stopOpacity|strikethroughPosition|strikethroughThickness|string|stroke|strokeDasharray|strokeDashoffset|strokeLinecap|strokeLinejoin|strokeMiterlimit|strokeOpacity|strokeWidth|surfaceScale|systemLanguage|tableValues|targetX|targetY|textAnchor|textDecoration|textRendering|textLength|to|transform|u1|u2|underlinePosition|underlineThickness|unicode|unicodeBidi|unicodeRange|unitsPerEm|vAlphabetic|vHanging|vIdeographic|vMathematical|values|vectorEffect|version|vertAdvY|vertOriginX|vertOriginY|viewBox|viewTarget|visibility|widths|wordSpacing|writingMode|x|xHeight|x1|x2|xChannelSelector|xlinkActuate|xlinkArcrole|xlinkHref|xlinkRole|xlinkShow|xlinkTitle|xlinkType|xmlBase|xmlns|xmlnsXlink|xmlLang|xmlSpace|y|y1|y2|yChannelSelector|z|zoomAndPan|for|class|autofocus)|(([Dd][Aa][Tt][Aa]|[Aa][Rr][Ii][Aa]|x)-.*))$/,Mi=Lr(function(e){return ji.test(e)||e.charCodeAt(0)===111&&e.charCodeAt(1)===110&&e.charCodeAt(2)<91}),Ni=!1,Pi=Mi,Fi=function(e){return e!==`theme`},Ii=function(e){return typeof e==`string`&&e.charCodeAt(0)>96?Pi:Fi},Li=function(e,t,n){var r;if(t){var i=t.shouldForwardProp;r=e.__emotion_forwardProp&&i?function(t){return e.__emotion_forwardProp(t)&&i(t)}:i}return typeof r!=`function`&&n&&(r=e.__emotion_forwardProp),r},Ri=function(e){var t=e.cache,n=e.serialized,r=e.isStringTag;return Qr(t,n,r),_i(function(){return $r(t,n,r)}),null},zi=function e(t,n){var r=t.__emotion_real===t,i=r&&t.__emotion_base||t,a,o;n!==void 0&&(a=n.label,o=n.target);var s=Li(t,n,r),c=s||Ii(i),l=!c(`as`);return function(){var u=arguments,d=r&&t.__emotion_styles!==void 0?t.__emotion_styles.slice(0):[];if(a!==void 0&&d.push(`label:`+a+`;`),u[0]==null||u[0].raw===void 0)d.push.apply(d,u);else{var f=u[0];d.push(f[0]);for(var p=u.length,m=1;m<p;m++)d.push(u[m],f[m])}var h=bi(function(e,t,n){var r=l&&e.as||i,a=``,u=[],f=e;if(e.theme==null){for(var p in f={},e)f[p]=e[p];f.theme=x.useContext(xi)}typeof e.className==`string`?a=Zr(t.registered,u,e.className):e.className!=null&&(a=e.className+` `);var m=mi(d.concat(u),t.registered,f);a+=t.key+`-`+m.name,o!==void 0&&(a+=` `+o);var h=l&&s===void 0?Ii(r):c,g={};for(var _ in e)l&&_===`as`||h(_)&&(g[_]=e[_]);return g.className=a,n&&(g.ref=n),x.createElement(x.Fragment,null,x.createElement(Ri,{cache:t,serialized:m,isStringTag:typeof r==`string`}),x.createElement(r,g))});return h.displayName=a===void 0?`Styled(`+(typeof i==`string`?i:i.displayName||i.name||`Component`)+`)`:a,h.defaultProps=t.defaultProps,h.__emotion_real=h,h.__emotion_base=i,h.__emotion_styles=d,h.__emotion_forwardProp=s,Object.defineProperty(h,`toString`,{value:function(){return o===void 0&&Ni?`NO_COMPONENT_SELECTOR`:`.`+o}}),h.withComponent=function(t,r){return e(t,jn({},n,r,{shouldForwardProp:Li(h,r,!0)})).apply(void 0,d)},h}},Bi=`a.abbr.address.area.article.aside.audio.b.base.bdi.bdo.big.blockquote.body.br.button.canvas.caption.cite.code.col.colgroup.data.datalist.dd.del.details.dfn.dialog.div.dl.dt.em.embed.fieldset.figcaption.figure.footer.form.h1.h2.h3.h4.h5.h6.head.header.hgroup.hr.html.i.iframe.img.input.ins.kbd.keygen.label.legend.li.link.main.map.mark.marquee.menu.menuitem.meta.meter.nav.noscript.object.ol.optgroup.option.output.p.param.picture.pre.progress.q.rp.rt.ruby.s.samp.script.section.select.small.source.span.strong.style.sub.summary.sup.table.tbody.td.textarea.tfoot.th.thead.time.title.tr.track.u.ul.var.video.wbr.circle.clipPath.defs.ellipse.foreignObject.g.image.line.linearGradient.mask.path.pattern.polygon.polyline.radialGradient.rect.stop.svg.text.tspan`.split(`.`),Vi=zi.bind(null);Bi.forEach(function(e){Vi[e]=Vi(e)});var Hi=o((e=>{var t=Symbol.for(`react.transitional.element`),n=Symbol.for(`react.fragment`);function r(e,n,r){var i=null;if(r!==void 0&&(i=``+r),n.key!==void 0&&(i=``+n.key),`key`in n)for(var a in r={},n)a!==`key`&&(r[a]=n[a]);else r=n;return n=r.ref,{$$typeof:t,type:e,key:i,ref:n===void 0?null:n,props:r}}e.Fragment=n,e.jsx=r,e.jsxs=r})),I=o(((e,t)=>{t.exports=Hi()}))();function Ui(e){return e==null||Object.keys(e).length===0}function Wi(e){let{styles:t,defaultTheme:n={}}=e;return(0,I.jsx)(Oi,{styles:typeof t==`function`?e=>t(Ui(e)?n:e):t})}function Gi(e,t){return Vi(e,t)}function Ki(e,t){Array.isArray(e.__emotion_styles)&&(e.__emotion_styles=t(e.__emotion_styles))}var qi=[];function Ji(e){return qi[0]=e,mi(qi)}var Yi=o((e=>{var t=Symbol.for(`react.fragment`),n=Symbol.for(`react.strict_mode`),r=Symbol.for(`react.profiler`),i=Symbol.for(`react.consumer`),a=Symbol.for(`react.context`),o=Symbol.for(`react.forward_ref`),s=Symbol.for(`react.suspense`),c=Symbol.for(`react.suspense_list`),l=Symbol.for(`react.memo`),u=Symbol.for(`react.lazy`),d=Symbol.for(`react.client.reference`);e.isValidElementType=function(e){return!!(typeof e==`string`||typeof e==`function`||e===t||e===r||e===n||e===s||e===c||typeof e==`object`&&e&&(e.$$typeof===u||e.$$typeof===l||e.$$typeof===a||e.$$typeof===i||e.$$typeof===o||e.$$typeof===d||e.getModuleId!==void 0))}})),Xi=o(((e,t)=>{t.exports=Yi()}))();function Zi(e){if(typeof e!=`object`||!e)return!1;let t=Object.getPrototypeOf(e);return(t===null||t===Object.prototype||Object.getPrototypeOf(t)===null)&&!(Symbol.toStringTag in e)&&!(Symbol.iterator in e)}function Qi(e){if(x.isValidElement(e)||(0,Xi.isValidElementType)(e)||!Zi(e))return e;let t={};return Object.keys(e).forEach(n=>{t[n]=Qi(e[n])}),t}function $i(e,t,n={clone:!0}){let r=n.clone?{...e}:e;return Zi(e)&&Zi(t)&&Object.keys(t).forEach(i=>{x.isValidElement(t[i])||(0,Xi.isValidElementType)(t[i])?r[i]=t[i]:Zi(t[i])&&Object.prototype.hasOwnProperty.call(e,i)&&Zi(e[i])?r[i]=$i(e[i],t[i],n):n.clone?r[i]=Zi(t[i])?Qi(t[i]):t[i]:r[i]=t[i]}),r}var ea=e=>{let t=Object.keys(e).map(t=>({key:t,val:e[t]}))||[];return t.sort((e,t)=>e.val-t.val),t.reduce((e,t)=>({...e,[t.key]:t.val}),{})};function ta(e){let{values:t={xs:0,sm:600,md:900,lg:1200,xl:1536},unit:n=`px`,step:r=5,...i}=e,a=ea(t),o=Object.keys(a);function s(e){return`@media (min-width:${typeof t[e]==`number`?t[e]:e}${n})`}function c(e){return`@media (max-width:${(typeof t[e]==`number`?t[e]:e)-r/100}${n})`}function l(e,i){let a=o.indexOf(i);return`@media (min-width:${typeof t[e]==`number`?t[e]:e}${n}) and (max-width:${(a!==-1&&typeof t[o[a]]==`number`?t[o[a]]:i)-r/100}${n})`}function u(e){return o.indexOf(e)+1<o.length?l(e,o[o.indexOf(e)+1]):s(e)}function d(e){let t=o.indexOf(e);return t===0?s(o[1]):t===o.length-1?c(o[t]):l(e,o[o.indexOf(e)+1]).replace(`@media`,`@media not all and`)}return{keys:o,values:a,up:s,down:c,between:l,only:u,not:d,unit:n,...i}}function na(e,t){if(!e.containerQueries)return t;let n=Object.keys(t).filter(e=>e.startsWith(`@container`)).sort((e,t)=>{let n=/min-width:\s*([0-9.]+)/;return(e.match(n)?.[1]||0)-+(t.match(n)?.[1]||0)});return n.length?n.reduce((e,n)=>{let r=t[n];return delete e[n],e[n]=r,e},{...t}):t}function ra(e,t){return t===`@`||t.startsWith(`@`)&&(e.some(e=>t.startsWith(`@${e}`))||!!t.match(/^@\d/))}function ia(e,t){let n=t.match(/^@([^/]+)?\/?(.+)?$/);if(!n)return null;let[,r,i]=n,a=Number.isNaN(+r)?r||0:+r;return e.containerQueries(i).up(a)}function aa(e){let t=(e,t)=>e.replace(`@media`,t?`@container ${t}`:`@container`);function n(n,r){n.up=(...n)=>t(e.breakpoints.up(...n),r),n.down=(...n)=>t(e.breakpoints.down(...n),r),n.between=(...n)=>t(e.breakpoints.between(...n),r),n.only=(...n)=>t(e.breakpoints.only(...n),r),n.not=(...n)=>{let i=t(e.breakpoints.not(...n),r);return i.includes(`not all and`)?i.replace(`not all and `,``).replace(`min-width:`,`width<`).replace(`max-width:`,`width>`).replace(`and`,`or`):i}}let r={},i=e=>(n(r,e),r);return n(i),{...e,containerQueries:i}}var oa={borderRadius:4};function sa(e,t){return t?$i(e,t,{clone:!1}):e}var ca=sa;const la={xs:0,sm:600,md:900,lg:1200,xl:1536};var ua={keys:[`xs`,`sm`,`md`,`lg`,`xl`],up:e=>`@media (min-width:${la[e]}px)`},da={containerQueries:e=>({up:t=>{let n=typeof t==`number`?t:la[t]||t;return typeof n==`number`&&(n=`${n}px`),e?`@container ${e} (min-width:${n})`:`@container (min-width:${n})`}})};function fa(e,t,n){let r=e.theme||{};if(Array.isArray(t)){let e=r.breakpoints||ua;return t.reduce((r,i,a)=>(r[e.up(e.keys[a])]=n(t[a]),r),{})}if(typeof t==`object`){let e=r.breakpoints||ua;return Object.keys(t).reduce((i,a)=>{if(ra(e.keys,a)){let e=ia(r.containerQueries?r:da,a);e&&(i[e]=n(t[a],a))}else if(Object.keys(e.values||la).includes(a)){let r=e.up(a);i[r]=n(t[a],a)}else{let e=a;i[e]=t[e]}return i},{})}return n(t)}function pa(e={}){return e.keys?.reduce((t,n)=>{let r=e.up(n);return t[r]={},t},{})||{}}function ma(e,t){return e.reduce((e,t)=>{let n=e[t];return(!n||Object.keys(n).length===0)&&delete e[t],e},t)}function ha(e){if(typeof e!=`string`)throw Error(kn(7));return e.charAt(0).toUpperCase()+e.slice(1)}function ga(e,t,n=!0){if(!t||typeof t!=`string`)return null;if(e&&e.vars&&n){let n=`vars.${t}`.split(`.`).reduce((e,t)=>e&&e[t]?e[t]:null,e);if(n!=null)return n}return t.split(`.`).reduce((e,t)=>e&&e[t]!=null?e[t]:null,e)}function _a(e,t,n,r=n){let i;return i=typeof e==`function`?e(n):Array.isArray(e)?e[n]||r:ga(e,n)||r,t&&(i=t(i,r,e)),i}function va(e){let{prop:t,cssProperty:n=e.prop,themeKey:r,transform:i}=e,a=e=>{if(e[t]==null)return null;let a=e[t],o=e.theme,s=ga(o,r)||{};return fa(e,a,e=>{let r=_a(s,i,e);return e===r&&typeof e==`string`&&(r=_a(s,i,`${t}${e===`default`?``:ha(e)}`,e)),n===!1?r:{[n]:r}})};return a.propTypes={},a.filterProps=[t],a}var ya=va;function ba(e){let t={};return n=>(t[n]===void 0&&(t[n]=e(n)),t[n])}var xa={m:`margin`,p:`padding`},Sa={t:`Top`,r:`Right`,b:`Bottom`,l:`Left`,x:[`Left`,`Right`],y:[`Top`,`Bottom`]},Ca={marginX:`mx`,marginY:`my`,paddingX:`px`,paddingY:`py`},wa=ba(e=>{if(e.length>2)if(Ca[e])e=Ca[e];else return[e];let[t,n]=e.split(``),r=xa[t],i=Sa[n]||``;return Array.isArray(i)?i.map(e=>r+e):[r+i]});const Ta=[`m`,`mt`,`mr`,`mb`,`ml`,`mx`,`my`,`margin`,`marginTop`,`marginRight`,`marginBottom`,`marginLeft`,`marginX`,`marginY`,`marginInline`,`marginInlineStart`,`marginInlineEnd`,`marginBlock`,`marginBlockStart`,`marginBlockEnd`],Ea=[`p`,`pt`,`pr`,`pb`,`pl`,`px`,`py`,`padding`,`paddingTop`,`paddingRight`,`paddingBottom`,`paddingLeft`,`paddingX`,`paddingY`,`paddingInline`,`paddingInlineStart`,`paddingInlineEnd`,`paddingBlock`,`paddingBlockStart`,`paddingBlockEnd`];var Da=[...Ta,...Ea];function Oa(e,t,n,r){let i=ga(e,t,!0)??n;return typeof i==`number`||typeof i==`string`?e=>typeof e==`string`?e:typeof i==`string`?i.startsWith(`var(`)&&e===0?0:i.startsWith(`var(`)&&e===1?i:`calc(${e} * ${i})`:i*e:Array.isArray(i)?e=>{if(typeof e==`string`)return e;let t=i[Math.abs(e)];return e>=0?t:typeof t==`number`?-t:typeof t==`string`&&t.startsWith(`var(`)?`calc(-1 * ${t})`:`-${t}`}:typeof i==`function`?i:()=>void 0}function ka(e){return Oa(e,`spacing`,8,`spacing`)}function Aa(e,t){return typeof t==`string`||t==null?t:e(t)}function ja(e,t){return n=>e.reduce((e,r)=>(e[r]=Aa(t,n),e),{})}function Ma(e,t,n,r){if(!t.includes(n))return null;let i=ja(wa(n),r),a=e[n];return fa(e,a,i)}function Na(e,t){let n=ka(e.theme);return Object.keys(e).map(r=>Ma(e,t,r,n)).reduce(ca,{})}function Pa(e){return Na(e,Ta)}Pa.propTypes={},Pa.filterProps=Ta;function Fa(e){return Na(e,Ea)}Fa.propTypes={},Fa.filterProps=Ea;function Ia(e){return Na(e,Da)}Ia.propTypes={},Ia.filterProps=Da;function La(e=8,t=ka({spacing:e})){if(e.mui)return e;let n=(...e)=>(e.length===0?[1]:e).map(e=>{let n=t(e);return typeof n==`number`?`${n}px`:n}).join(` `);return n.mui=!0,n}function Ra(...e){let t=e.reduce((e,t)=>(t.filterProps.forEach(n=>{e[n]=t}),e),{}),n=e=>Object.keys(e).reduce((n,r)=>t[r]?ca(n,t[r](e)):n,{});return n.propTypes={},n.filterProps=e.reduce((e,t)=>e.concat(t.filterProps),[]),n}var za=Ra;function Ba(e){return typeof e==`number`?`${e}px solid`:e}function Va(e,t){return ya({prop:e,themeKey:`borders`,transform:t})}const Ha=Va(`border`,Ba),Ua=Va(`borderTop`,Ba),Wa=Va(`borderRight`,Ba),Ga=Va(`borderBottom`,Ba),Ka=Va(`borderLeft`,Ba),qa=Va(`borderColor`),Ja=Va(`borderTopColor`),Ya=Va(`borderRightColor`),Xa=Va(`borderBottomColor`),Za=Va(`borderLeftColor`),Qa=Va(`outline`,Ba),$a=Va(`outlineColor`),eo=e=>{if(e.borderRadius!==void 0&&e.borderRadius!==null){let t=Oa(e.theme,`shape.borderRadius`,4,`borderRadius`);return fa(e,e.borderRadius,e=>({borderRadius:Aa(t,e)}))}return null};eo.propTypes={},eo.filterProps=[`borderRadius`],za(Ha,Ua,Wa,Ga,Ka,qa,Ja,Ya,Xa,Za,eo,Qa,$a);const to=e=>{if(e.gap!==void 0&&e.gap!==null){let t=Oa(e.theme,`spacing`,8,`gap`);return fa(e,e.gap,e=>({gap:Aa(t,e)}))}return null};to.propTypes={},to.filterProps=[`gap`];const no=e=>{if(e.columnGap!==void 0&&e.columnGap!==null){let t=Oa(e.theme,`spacing`,8,`columnGap`);return fa(e,e.columnGap,e=>({columnGap:Aa(t,e)}))}return null};no.propTypes={},no.filterProps=[`columnGap`];const ro=e=>{if(e.rowGap!==void 0&&e.rowGap!==null){let t=Oa(e.theme,`spacing`,8,`rowGap`);return fa(e,e.rowGap,e=>({rowGap:Aa(t,e)}))}return null};ro.propTypes={},ro.filterProps=[`rowGap`],za(to,no,ro,ya({prop:`gridColumn`}),ya({prop:`gridRow`}),ya({prop:`gridAutoFlow`}),ya({prop:`gridAutoColumns`}),ya({prop:`gridAutoRows`}),ya({prop:`gridTemplateColumns`}),ya({prop:`gridTemplateRows`}),ya({prop:`gridTemplateAreas`}),ya({prop:`gridArea`}));function io(e,t){return t===`grey`?t:e}za(ya({prop:`color`,themeKey:`palette`,transform:io}),ya({prop:`bgcolor`,cssProperty:`backgroundColor`,themeKey:`palette`,transform:io}),ya({prop:`backgroundColor`,themeKey:`palette`,transform:io}));function ao(e){return e<=1&&e!==0?`${e*100}%`:e}const oo=ya({prop:`width`,transform:ao}),so=e=>e.maxWidth!==void 0&&e.maxWidth!==null?fa(e,e.maxWidth,t=>{let n=e.theme?.breakpoints?.values?.[t]||la[t];return n?e.theme?.breakpoints?.unit===`px`?{maxWidth:n}:{maxWidth:`${n}${e.theme.breakpoints.unit}`}:{maxWidth:ao(t)}}):null;so.filterProps=[`maxWidth`];const co=ya({prop:`minWidth`,transform:ao}),lo=ya({prop:`height`,transform:ao}),uo=ya({prop:`maxHeight`,transform:ao}),fo=ya({prop:`minHeight`,transform:ao});ya({prop:`size`,cssProperty:`width`,transform:ao}),ya({prop:`size`,cssProperty:`height`,transform:ao}),za(oo,so,co,lo,uo,fo,ya({prop:`boxSizing`}));var po={border:{themeKey:`borders`,transform:Ba},borderTop:{themeKey:`borders`,transform:Ba},borderRight:{themeKey:`borders`,transform:Ba},borderBottom:{themeKey:`borders`,transform:Ba},borderLeft:{themeKey:`borders`,transform:Ba},borderColor:{themeKey:`palette`},borderTopColor:{themeKey:`palette`},borderRightColor:{themeKey:`palette`},borderBottomColor:{themeKey:`palette`},borderLeftColor:{themeKey:`palette`},outline:{themeKey:`borders`,transform:Ba},outlineColor:{themeKey:`palette`},borderRadius:{themeKey:`shape.borderRadius`,style:eo},color:{themeKey:`palette`,transform:io},bgcolor:{themeKey:`palette`,cssProperty:`backgroundColor`,transform:io},backgroundColor:{themeKey:`palette`,transform:io},p:{style:Fa},pt:{style:Fa},pr:{style:Fa},pb:{style:Fa},pl:{style:Fa},px:{style:Fa},py:{style:Fa},padding:{style:Fa},paddingTop:{style:Fa},paddingRight:{style:Fa},paddingBottom:{style:Fa},paddingLeft:{style:Fa},paddingX:{style:Fa},paddingY:{style:Fa},paddingInline:{style:Fa},paddingInlineStart:{style:Fa},paddingInlineEnd:{style:Fa},paddingBlock:{style:Fa},paddingBlockStart:{style:Fa},paddingBlockEnd:{style:Fa},m:{style:Pa},mt:{style:Pa},mr:{style:Pa},mb:{style:Pa},ml:{style:Pa},mx:{style:Pa},my:{style:Pa},margin:{style:Pa},marginTop:{style:Pa},marginRight:{style:Pa},marginBottom:{style:Pa},marginLeft:{style:Pa},marginX:{style:Pa},marginY:{style:Pa},marginInline:{style:Pa},marginInlineStart:{style:Pa},marginInlineEnd:{style:Pa},marginBlock:{style:Pa},marginBlockStart:{style:Pa},marginBlockEnd:{style:Pa},displayPrint:{cssProperty:!1,transform:e=>({"@media print":{display:e}})},display:{},overflow:{},textOverflow:{},visibility:{},whiteSpace:{},flexBasis:{},flexDirection:{},flexWrap:{},justifyContent:{},alignItems:{},alignContent:{},order:{},flex:{},flexGrow:{},flexShrink:{},alignSelf:{},justifyItems:{},justifySelf:{},gap:{style:to},rowGap:{style:ro},columnGap:{style:no},gridColumn:{},gridRow:{},gridAutoFlow:{},gridAutoColumns:{},gridAutoRows:{},gridTemplateColumns:{},gridTemplateRows:{},gridTemplateAreas:{},gridArea:{},position:{},zIndex:{themeKey:`zIndex`},top:{},right:{},bottom:{},left:{},boxShadow:{themeKey:`shadows`},width:{transform:ao},maxWidth:{style:so},minWidth:{transform:ao},height:{transform:ao},maxHeight:{transform:ao},minHeight:{transform:ao},boxSizing:{},font:{themeKey:`font`},fontFamily:{themeKey:`typography`},fontSize:{themeKey:`typography`},fontStyle:{themeKey:`typography`},fontWeight:{themeKey:`typography`},letterSpacing:{},textTransform:{},lineHeight:{},textAlign:{},typography:{cssProperty:!1,themeKey:`typography`}};function mo(...e){let t=e.reduce((e,t)=>e.concat(Object.keys(t)),[]),n=new Set(t);return e.every(e=>n.size===Object.keys(e).length)}function ho(e,t){return typeof e==`function`?e(t):e}function go(){function e(e,t,n,r){let i={[e]:t,theme:n},a=r[e];if(!a)return{[e]:t};let{cssProperty:o=e,themeKey:s,transform:c,style:l}=a;if(t==null)return null;if(s===`typography`&&t===`inherit`)return{[e]:t};let u=ga(n,s)||{};return l?l(i):fa(i,t,t=>{let n=_a(u,c,t);return t===n&&typeof t==`string`&&(n=_a(u,c,`${e}${t===`default`?``:ha(t)}`,t)),o===!1?n:{[o]:n}})}function t(n){let{sx:r,theme:i={},nested:a}=n||{};if(!r)return null;let o=i.unstable_sxConfig??po;function s(n){let r=n;if(typeof n==`function`)r=n(i);else if(typeof n!=`object`)return n;if(!r)return null;let s=pa(i.breakpoints),c=Object.keys(s),l=s;return Object.keys(r).forEach(n=>{let a=ho(r[n],i);if(a!=null)if(typeof a==`object`)if(o[n])l=ca(l,e(n,a,i,o));else{let e=fa({theme:i},a,e=>({[n]:e}));mo(e,a)?l[n]=t({sx:a,theme:i,nested:!0}):l=ca(l,e)}else l=ca(l,e(n,a,i,o))}),!a&&i.modularCssLayers?{"@layer sx":na(i,ma(c,l))}:na(i,ma(c,l))}return Array.isArray(r)?r.map(s):s(r)}return t}var L=go();L.filterProps=[`sx`];var _o=L;function vo(e,t){let n=this;if(n.vars){if(!n.colorSchemes?.[e]||typeof n.getColorSchemeSelector!=`function`)return{};let r=n.getColorSchemeSelector(e);return r===`&`?t:((r.includes(`data-`)||r.includes(`.`))&&(r=`*:where(${r.replace(/\s*&$/,``)}) &`),{[r]:t})}return n.palette.mode===e?t:{}}function yo(e={},...t){let{breakpoints:n={},palette:r={},spacing:i,shape:a={},...o}=e,s=ta(n),c=La(i),l=$i({breakpoints:s,direction:`ltr`,components:{},palette:{mode:`light`,...r},spacing:c,shape:{...oa,...a}},o);return l=aa(l),l.applyStyles=vo,l=t.reduce((e,t)=>$i(e,t),l),l.unstable_sxConfig={...po,...o?.unstable_sxConfig},l.unstable_sx=function(e){return _o({sx:e,theme:this})},l}var bo=yo;function xo(e){return Object.keys(e).length===0}function So(e=null){let t=x.useContext(xi);return!t||xo(t)?e:t}var Co=So;const wo=bo();function To(e=wo){return Co(e)}var Eo=To;function Do(e){let t=Ji(e);return e!==t&&t.styles?(t.styles.match(/^@layer\s+[^{]*$/)||(t.styles=`@layer global{${t.styles}}`),t):e}function Oo({styles:e,themeId:t,defaultTheme:n={}}){let r=Eo(n),i=t&&r[t]||r,a=typeof e==`function`?e(i):e;return i.modularCssLayers&&(a=Array.isArray(a)?a.map(e=>Do(typeof e==`function`?e(i):e)):Do(a)),(0,I.jsx)(Wi,{styles:a})}var ko=Oo,Ao=e=>{let t={systemProps:{},otherProps:{}},n=e?.theme?.unstable_sxConfig??po;return Object.keys(e).forEach(r=>{n[r]?t.systemProps[r]=e[r]:t.otherProps[r]=e[r]}),t};function jo(e){let{sx:t,...n}=e,{systemProps:r,otherProps:i}=Ao(n),a;return a=Array.isArray(t)?[r,...t]:typeof t==`function`?(...e)=>{let n=t(...e);return Zi(n)?{...r,...n}:r}:{...r,...t},{...i,sx:a}}var Mo=e=>e,No=(()=>{let e=Mo;return{configure(t){e=t},generate(t){return e(t)},reset(){e=Mo}}})(),Po=g();function Fo(e){var t,n,r=``;if(typeof e==`string`||typeof e==`number`)r+=e;else if(typeof e==`object`)if(Array.isArray(e)){var i=e.length;for(t=0;t<i;t++)e[t]&&(n=Fo(e[t]))&&(r&&(r+=` `),r+=n)}else for(n in e)e[n]&&(r&&(r+=` `),r+=n);return r}function Io(){for(var e,t,n=0,r=``,i=arguments.length;n<i;n++)(e=arguments[n])&&(t=Fo(e))&&(r&&(r+=` `),r+=t);return r}var R=Io;function Lo(e={}){let{themeId:t,defaultTheme:n,defaultClassName:r=`MuiBox-root`,generateClassName:i}=e,a=Gi(`div`,{shouldForwardProp:e=>e!==`theme`&&e!==`sx`&&e!==`as`})(_o);return x.forwardRef(function(e,o){let s=Eo(n),{className:c,component:l=`div`,...u}=jo(e);return(0,I.jsx)(a,{as:l,ref:o,className:R(c,i?i(r):r),theme:t&&s[t]||s,...u})})}const Ro={active:`active`,checked:`checked`,completed:`completed`,disabled:`disabled`,error:`error`,expanded:`expanded`,focused:`focused`,focusVisible:`focusVisible`,open:`open`,readOnly:`readOnly`,required:`required`,selected:`selected`};function zo(e,t,n=`Mui`){let r=Ro[t];return r?`${n}-${r}`:`${No.generate(e)}-${t}`}function Bo(e,t,n=`Mui`){let r={};return t.forEach(t=>{r[t]=zo(e,t,n)}),r}function Vo(e){let{variants:t,...n}=e,r={variants:t,style:Ji(n),isProcessed:!0};return r.style===n||t&&t.forEach(e=>{typeof e.style!=`function`&&(e.style=Ji(e.style))}),r}const Ho=bo();function Uo(e){return e!==`ownerState`&&e!==`theme`&&e!==`sx`&&e!==`as`}function Wo(e,t){return t&&e&&typeof e==`object`&&e.styles&&!e.styles.startsWith(`@layer`)&&(e.styles=`@layer ${t}{${String(e.styles)}}`),e}function Go(e){return e?(t,n)=>n[e]:null}function Ko(e,t,n){e.theme=Xo(e.theme)?n:e.theme[t]||e.theme}function qo(e,t,n){let r=typeof t==`function`?t(e):t;if(Array.isArray(r))return r.flatMap(t=>qo(e,t,n));if(Array.isArray(r?.variants)){let t;if(r.isProcessed)t=n?Wo(r.style,n):r.style;else{let{variants:e,...i}=r;t=n?Wo(Ji(i),n):i}return Jo(e,r.variants,[t],n)}return r?.isProcessed?n?Wo(Ji(r.style),n):r.style:n?Wo(Ji(r),n):r}function Jo(e,t,n=[],r=void 0){let i;variantLoop:for(let a=0;a<t.length;a+=1){let o=t[a];if(typeof o.props==`function`){if(i??={...e,...e.ownerState,ownerState:e.ownerState},!o.props(i))continue}else for(let t in o.props)if(e[t]!==o.props[t]&&e.ownerState?.[t]!==o.props[t])continue variantLoop;typeof o.style==`function`?(i??={...e,...e.ownerState,ownerState:e.ownerState},n.push(r?Wo(Ji(o.style(i)),r):o.style(i))):n.push(r?Wo(Ji(o.style),r):o.style)}return n}function Yo(e={}){let{themeId:t,defaultTheme:n=Ho,rootShouldForwardProp:r=Uo,slotShouldForwardProp:i=Uo}=e;function a(e){Ko(e,t,n)}return(e,t={})=>{Ki(e,e=>e.filter(e=>e!==_o));let{name:n,slot:o,skipVariantsResolver:s,skipSx:c,overridesResolver:l=Go(Qo(o)),...u}=t,d=n&&n.startsWith(`Mui`)||o?`components`:`custom`,f=s===void 0?o&&o!==`Root`&&o!==`root`||!1:s,p=c||!1,m=Uo;o===`Root`||o===`root`?m=r:o?m=i:Zo(e)&&(m=void 0);let h=Gi(e,{shouldForwardProp:m,label:void 0,...u}),g=e=>{if(e.__emotion_real===e)return e;if(typeof e==`function`)return function(t){return qo(t,e,t.theme.modularCssLayers?d:void 0)};if(Zi(e)){let t=Vo(e);return function(e){return t.variants?qo(e,t,e.theme.modularCssLayers?d:void 0):e.theme.modularCssLayers?Wo(t.style,d):t.style}}return e},_=(...t)=>{let r=[],i=t.map(g),o=[];if(r.push(a),n&&l&&o.push(function(e){let t=e.theme.components?.[n]?.styleOverrides;if(!t)return null;let r={};for(let n in t)r[n]=qo(e,t[n],e.theme.modularCssLayers?`theme`:void 0);return l(e,r)}),n&&!f&&o.push(function(e){let t=e.theme?.components?.[n]?.variants;return t?Jo(e,t,[],e.theme.modularCssLayers?`theme`:void 0):null}),p||o.push(_o),Array.isArray(i[0])){let e=i.shift(),t=Array(r.length).fill(``),n=Array(o.length).fill(``),a;a=[...t,...e,...n],a.raw=[...t,...e.raw,...n],r.unshift(a)}let s=h(...r,...i,...o);return e.muiName&&(s.muiName=e.muiName),s};return h.withConfig&&(_.withConfig=h.withConfig),_}}function Xo(e){for(let t in e)return!1;return!0}function Zo(e){return typeof e==`string`&&e.charCodeAt(0)>96}function Qo(e){return e&&e.charAt(0).toLowerCase()+e.slice(1)}function $o(e,t,n=!1){let r={...t};for(let i in e)if(Object.prototype.hasOwnProperty.call(e,i)){let a=i;if(a===`components`||a===`slots`)r[a]={...e[a],...r[a]};else if(a===`componentsProps`||a===`slotProps`){let i=e[a],o=t[a];if(!o)r[a]=i||{};else if(!i)r[a]=o;else for(let e in r[a]={...o},i)if(Object.prototype.hasOwnProperty.call(i,e)){let t=e;r[a][t]=$o(i[t],o[t],n)}}else a===`className`&&n&&t.className?r.className=R(e?.className,t?.className):a===`style`&&n&&t.style?r.style={...e?.style,...t?.style}:r[a]===void 0&&(r[a]=e[a])}return r}var es=typeof window<`u`?x.useLayoutEffect:x.useEffect;function ts(e,t=-(2**53-1),n=2**53-1){return Math.max(t,Math.min(e,n))}var ns=ts;function rs(e,t=0,n=1){return ns(e,t,n)}function is(e){e=e.slice(1);let t=RegExp(`.{1,${e.length>=6?2:1}}`,`g`),n=e.match(t);return n&&n[0].length===1&&(n=n.map(e=>e+e)),n?`rgb${n.length===4?`a`:``}(${n.map((e,t)=>t<3?parseInt(e,16):Math.round(parseInt(e,16)/255*1e3)/1e3).join(`, `)})`:``}function as(e){if(e.type)return e;if(e.charAt(0)===`#`)return as(is(e));let t=e.indexOf(`(`),n=e.substring(0,t);if(![`rgb`,`rgba`,`hsl`,`hsla`,`color`].includes(n))throw Error(kn(9,e));let r=e.substring(t+1,e.length-1),i;if(n===`color`){if(r=r.split(` `),i=r.shift(),r.length===4&&r[3].charAt(0)===`/`&&(r[3]=r[3].slice(1)),![`srgb`,`display-p3`,`a98-rgb`,`prophoto-rgb`,`rec-2020`].includes(i))throw Error(kn(10,i))}else r=r.split(`,`);return r=r.map(e=>parseFloat(e)),{type:n,values:r,colorSpace:i}}const os=e=>{let t=as(e);return t.values.slice(0,3).map((e,n)=>t.type.includes(`hsl`)&&n!==0?`${e}%`:e).join(` `)},ss=(e,t)=>{try{return os(e)}catch{return e}};function cs(e){let{type:t,colorSpace:n}=e,{values:r}=e;return t.includes(`rgb`)?r=r.map((e,t)=>t<3?parseInt(e,10):e):t.includes(`hsl`)&&(r[1]=`${r[1]}%`,r[2]=`${r[2]}%`),r=t.includes(`color`)?`${n} ${r.join(` `)}`:`${r.join(`, `)}`,`${t}(${r})`}function ls(e){e=as(e);let{values:t}=e,n=t[0],r=t[1]/100,i=t[2]/100,a=r*Math.min(i,1-i),o=(e,t=(e+n/30)%12)=>i-a*Math.max(Math.min(t-3,9-t,1),-1),s=`rgb`,c=[Math.round(o(0)*255),Math.round(o(8)*255),Math.round(o(4)*255)];return e.type===`hsla`&&(s+=`a`,c.push(t[3])),cs({type:s,values:c})}function us(e){e=as(e);let t=e.type===`hsl`||e.type===`hsla`?as(ls(e)).values:e.values;return t=t.map(t=>(e.type!==`color`&&(t/=255),t<=.03928?t/12.92:((t+.055)/1.055)**2.4)),Number((.2126*t[0]+.7152*t[1]+.0722*t[2]).toFixed(3))}function ds(e,t){let n=us(e),r=us(t);return(Math.max(n,r)+.05)/(Math.min(n,r)+.05)}function fs(e,t){return e=as(e),t=rs(t),(e.type===`rgb`||e.type===`hsl`)&&(e.type+=`a`),e.type===`color`?e.values[3]=`/${t}`:e.values[3]=t,cs(e)}function ps(e,t,n){try{return fs(e,t)}catch{return e}}function ms(e,t){if(e=as(e),t=rs(t),e.type.includes(`hsl`))e.values[2]*=1-t;else if(e.type.includes(`rgb`)||e.type.includes(`color`))for(let n=0;n<3;n+=1)e.values[n]*=1-t;return cs(e)}function hs(e,t,n){try{return ms(e,t)}catch{return e}}function gs(e,t){if(e=as(e),t=rs(t),e.type.includes(`hsl`))e.values[2]+=(100-e.values[2])*t;else if(e.type.includes(`rgb`))for(let n=0;n<3;n+=1)e.values[n]+=(255-e.values[n])*t;else if(e.type.includes(`color`))for(let n=0;n<3;n+=1)e.values[n]+=(1-e.values[n])*t;return cs(e)}function _s(e,t,n){try{return gs(e,t)}catch{return e}}function vs(e,t=.15){return us(e)>.5?ms(e,t):gs(e,t)}function ys(e,t,n){try{return vs(e,t)}catch{return e}}var bs=x.createContext(null);function xs(){return x.useContext(bs)}var Ss=typeof Symbol==`function`&&Symbol.for?Symbol.for(`mui.nested`):`__THEME_NESTED__`;function Cs(e,t){return typeof t==`function`?t(e):{...e,...t}}function ws(e){let{children:t,theme:n}=e,r=xs(),i=x.useMemo(()=>{let e=r===null?{...n}:Cs(r,n);return e!=null&&(e[Ss]=r!==null),e},[n,r]);return(0,I.jsx)(bs.Provider,{value:i,children:t})}var Ts=ws,Es=x.createContext();function Ds({value:e,...t}){return(0,I.jsx)(Es.Provider,{value:e??!0,...t})}const Os=()=>x.useContext(Es)??!1;var ks=Ds,As=x.createContext(void 0);function js({value:e,children:t}){return(0,I.jsx)(As.Provider,{value:e,children:t})}function Ms(e){let{theme:t,name:n,props:r}=e;if(!t||!t.components||!t.components[n])return r;let i=t.components[n];return i.defaultProps?$o(i.defaultProps,r,t.components.mergeClassNameAndStyle):!i.styleOverrides&&!i.variants?$o(i,r,t.components.mergeClassNameAndStyle):r}function Ns({props:e,name:t}){return Ms({props:e,name:t,theme:{components:x.useContext(As)}})}var Ps=js,Fs=0;function Is(e){let[t,n]=x.useState(e),r=e||t;return x.useEffect(()=>{t??(Fs+=1,n(`mui-${Fs}`))},[t]),r}var Ls={...x}.useId;function Rs(e){if(Ls!==void 0){let t=Ls();return e??t}return Is(e)}function zs(e){let t=Co(),n=Rs()||``,{modularCssLayers:r}=e,i=`mui.global, mui.components, mui.theme, mui.custom, mui.sx`;return i=!r||t!==null?``:typeof r==`string`?r.replace(/mui(?!\.)/g,i):`@layer ${i};`,es(()=>{let e=document.querySelector(`head`);if(!e)return;let t=e.firstChild;if(i){if(t&&t.hasAttribute?.(`data-mui-layer-order`)&&t.getAttribute(`data-mui-layer-order`)===n)return;let r=document.createElement(`style`);r.setAttribute(`data-mui-layer-order`,n),r.textContent=i,e.prepend(r)}else e.querySelector(`style[data-mui-layer-order="${n}"]`)?.remove()},[i,n]),i?(0,I.jsx)(ko,{styles:i}):null}var Bs={};function Vs(e,t,n,r=!1){return x.useMemo(()=>{let i=e&&t[e]||t;if(typeof n==`function`){let a=n(i),o=e?{...t,[e]:a}:a;return r?()=>o:o}return e?{...t,[e]:n}:{...t,...n}},[e,t,n,r])}function Hs(e){let{children:t,theme:n,themeId:r}=e,i=Co(Bs),a=xs()||Bs,o=Vs(r,i,n),s=Vs(r,a,n,!0),c=(r?o[r]:o).direction===`rtl`,l=zs(o);return(0,I.jsx)(Ts,{theme:s,children:(0,I.jsx)(xi.Provider,{value:o,children:(0,I.jsx)(ks,{value:c,children:(0,I.jsxs)(Ps,{value:r?o[r].components:o.components,children:[l,t]})})})})}var Us=Hs,Ws={theme:void 0};function Gs(e){let t,n;return function(r){let i=t;return(i===void 0||r.theme!==n)&&(Ws.theme=r.theme,i=Vo(e(Ws)),t=i,n=r.theme),i}}const Ks=`mode`,qs=`color-scheme`;function Js(e){let{defaultMode:t=`system`,defaultLightColorScheme:n=`light`,defaultDarkColorScheme:r=`dark`,modeStorageKey:i=Ks,colorSchemeStorageKey:a=qs,attribute:o=`data-color-scheme`,colorSchemeNode:s=`document.documentElement`,nonce:c}=e||{},l=``,u=o;if(o===`class`&&(u=`.%s`),o===`data`&&(u=`[data-%s]`),u.startsWith(`.`)){let e=u.substring(1);l+=`${s}.classList.remove('${e}'.replace('%s', light), '${e}'.replace('%s', dark));
      ${s}.classList.add('${e}'.replace('%s', colorScheme));`}let d=u.match(/\[([^[\]]+)\]/);if(d){let[e,t]=d[1].split(`=`);t||(l+=`${s}.removeAttribute('${e}'.replace('%s', light));
      ${s}.removeAttribute('${e}'.replace('%s', dark));`),l+=`
      ${s}.setAttribute('${e}'.replace('%s', colorScheme), ${t?`${t}.replace('%s', colorScheme)`:`""`});`}else l+=`${s}.setAttribute('${u}', colorScheme);`;return(0,I.jsx)(`script`,{suppressHydrationWarning:!0,nonce:typeof window>`u`?c:``,dangerouslySetInnerHTML:{__html:`(function() {
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
} catch(e){}})();`}},`mui-color-scheme-init`)}function Ys(){}var Xs=({key:e,storageWindow:t})=>(!t&&typeof window<`u`&&(t=window),{get(n){if(typeof window>`u`)return;if(!t)return n;let r;try{r=t.localStorage.getItem(e)}catch{}return r||n},set:n=>{if(t)try{t.localStorage.setItem(e,n)}catch{}},subscribe:n=>{if(!t)return Ys;let r=t=>{let r=t.newValue;t.key===e&&n(r)};return t.addEventListener(`storage`,r),()=>{t.removeEventListener(`storage`,r)}}});function Zs(){}function Qs(e){if(typeof window<`u`&&typeof window.matchMedia==`function`&&e===`system`)return window.matchMedia(`(prefers-color-scheme: dark)`).matches?`dark`:`light`}function $s(e,t){if(e.mode===`light`||e.mode===`system`&&e.systemMode===`light`)return t(`light`);if(e.mode===`dark`||e.mode===`system`&&e.systemMode===`dark`)return t(`dark`)}function ec(e){return $s(e,t=>{if(t===`light`)return e.lightColorScheme;if(t===`dark`)return e.darkColorScheme})}function tc(e){let{defaultMode:t=`light`,defaultLightColorScheme:n,defaultDarkColorScheme:r,supportedColorSchemes:i=[],modeStorageKey:a=Ks,colorSchemeStorageKey:o=qs,storageWindow:s=typeof window>`u`?void 0:window,storageManager:c=Xs,noSsr:l=!1}=e,u=i.join(`,`),d=i.length>1,f=x.useMemo(()=>c?.({key:a,storageWindow:s}),[c,a,s]),p=x.useMemo(()=>c?.({key:`${o}-light`,storageWindow:s}),[c,o,s]),m=x.useMemo(()=>c?.({key:`${o}-dark`,storageWindow:s}),[c,o,s]),[h,g]=x.useState(()=>{let e=f?.get(t)||t,i=p?.get(n)||n,a=m?.get(r)||r;return{mode:e,systemMode:Qs(e),lightColorScheme:i,darkColorScheme:a}}),[_,v]=x.useState(l||!d);x.useEffect(()=>{v(!0)},[]);let y=ec(h),b=x.useCallback(e=>{g(n=>{if(e===n.mode)return n;let r=e??t;return f?.set(r),{...n,mode:r,systemMode:Qs(r)}})},[f,t]),S=x.useCallback(e=>{e?typeof e==`string`?e&&!u.includes(e)?console.error(`\`${e}\` does not exist in \`theme.colorSchemes\`.`):g(t=>{let n={...t};return $s(t,t=>{t===`light`&&(p?.set(e),n.lightColorScheme=e),t===`dark`&&(m?.set(e),n.darkColorScheme=e)}),n}):g(t=>{let i={...t},a=e.light===null?n:e.light,o=e.dark===null?r:e.dark;return a&&(u.includes(a)?(i.lightColorScheme=a,p?.set(a)):console.error(`\`${a}\` does not exist in \`theme.colorSchemes\`.`)),o&&(u.includes(o)?(i.darkColorScheme=o,m?.set(o)):console.error(`\`${o}\` does not exist in \`theme.colorSchemes\`.`)),i}):g(e=>(p?.set(n),m?.set(r),{...e,lightColorScheme:n,darkColorScheme:r}))},[u,p,m,n,r]),C=x.useCallback(e=>{h.mode===`system`&&g(t=>{let n=e?.matches?`dark`:`light`;return t.systemMode===n?t:{...t,systemMode:n}})},[h.mode]),w=x.useRef(C);return w.current=C,x.useEffect(()=>{if(typeof window.matchMedia!=`function`||!d)return;let e=(...e)=>w.current(...e),t=window.matchMedia(`(prefers-color-scheme: dark)`);return t.addListener(e),e(t),()=>{t.removeListener(e)}},[d]),x.useEffect(()=>{if(d){let e=f?.subscribe(e=>{(!e||[`light`,`dark`,`system`].includes(e))&&b(e||t)})||Zs,n=p?.subscribe(e=>{(!e||u.match(e))&&S({light:e})})||Zs,r=m?.subscribe(e=>{(!e||u.match(e))&&S({dark:e})})||Zs;return()=>{e(),n(),r()}}},[S,b,u,t,s,d,f,p,m]),{...h,mode:_?h.mode:void 0,systemMode:_?h.systemMode:void 0,colorScheme:_?y:void 0,setMode:b,setColorScheme:S}}function nc(e){let{themeId:t,theme:n={},modeStorageKey:r=Ks,colorSchemeStorageKey:i=qs,disableTransitionOnChange:a=!1,defaultColorScheme:o,resolveTheme:s}=e,c={allColorSchemes:[],colorScheme:void 0,darkColorScheme:void 0,lightColorScheme:void 0,mode:void 0,setColorScheme:()=>{},setMode:()=>{},systemMode:void 0},l=x.createContext(void 0),u=()=>x.useContext(l)||c,d={},f={};function p(e){let{children:c,theme:u,modeStorageKey:p=r,colorSchemeStorageKey:m=i,disableTransitionOnChange:h=a,storageManager:g,storageWindow:_=typeof window>`u`?void 0:window,documentNode:v=typeof document>`u`?void 0:document,colorSchemeNode:y=typeof document>`u`?void 0:document.documentElement,disableNestedContext:b=!1,disableStyleSheetGeneration:S=!1,defaultMode:C=`system`,forceThemeRerender:w=!1,noSsr:T}=e,E=x.useRef(!1),D=xs(),O=x.useContext(l),k=!!O&&!b,ee=x.useMemo(()=>u||(typeof n==`function`?n():n),[u]),A=ee[t],j=A||ee,{colorSchemes:M=d,components:te=f,cssVarPrefix:N}=j,P=Object.keys(M).filter(e=>!!M[e]).join(`,`),F=x.useMemo(()=>P.split(`,`),[P]),ne=typeof o==`string`?o:o.light,re=typeof o==`string`?o:o.dark,{mode:ie,setMode:ae,systemMode:oe,lightColorScheme:se,darkColorScheme:ce,colorScheme:le,setColorScheme:ue}=tc({supportedColorSchemes:F,defaultLightColorScheme:ne,defaultDarkColorScheme:re,modeStorageKey:p,colorSchemeStorageKey:m,defaultMode:M[ne]&&M[re]?C:M[j.defaultColorScheme]?.palette?.mode||j.palette?.mode,storageManager:g,storageWindow:_,noSsr:T}),de=ie,fe=le;k&&(de=O.mode,fe=O.colorScheme);let pe=fe||j.defaultColorScheme;j.vars&&!w&&(pe=j.defaultColorScheme);let me=x.useMemo(()=>{let e=j.generateThemeVars?.()||j.vars,t={...j,components:te,colorSchemes:M,cssVarPrefix:N,vars:e};if(typeof t.generateSpacing==`function`&&(t.spacing=t.generateSpacing()),pe){let e=M[pe];e&&typeof e==`object`&&Object.keys(e).forEach(n=>{e[n]&&typeof e[n]==`object`?t[n]={...t[n],...e[n]}:t[n]=e[n]})}return s?s(t):t},[j,pe,te,M,N]),he=j.colorSchemeSelector;es(()=>{if(fe&&y&&he&&he!==`media`){let e=he,t=he;if(e===`class`&&(t=`.%s`),e===`data`&&(t=`[data-%s]`),e?.startsWith(`data-`)&&!e.includes(`%s`)&&(t=`[${e}="%s"]`),t.startsWith(`.`))y.classList.remove(...F.map(e=>t.substring(1).replace(`%s`,e))),y.classList.add(t.substring(1).replace(`%s`,fe));else{let e=t.replace(`%s`,fe).match(/\[([^\]]+)\]/);if(e){let[t,n]=e[1].split(`=`);n||F.forEach(e=>{y.removeAttribute(t.replace(fe,e))}),y.setAttribute(t,n?n.replace(/"|'/g,``):``)}else y.setAttribute(t,fe)}}},[fe,he,y,F]),x.useEffect(()=>{let e;if(h&&E.current&&v){let t=v.createElement(`style`);t.appendChild(v.createTextNode(`*{-webkit-transition:none!important;-moz-transition:none!important;-o-transition:none!important;-ms-transition:none!important;transition:none!important}`)),v.head.appendChild(t),window.getComputedStyle(v.body),e=setTimeout(()=>{v.head.removeChild(t)},1)}return()=>{clearTimeout(e)}},[fe,h,v]),x.useEffect(()=>(E.current=!0,()=>{E.current=!1}),[]);let ge=x.useMemo(()=>({allColorSchemes:F,colorScheme:fe,darkColorScheme:ce,lightColorScheme:se,mode:de,setColorScheme:ue,setMode:ae,systemMode:oe}),[F,fe,ce,se,de,ue,ae,oe,me.colorSchemeSelector]),_e=!0;(S||j.cssVariables===!1||k&&D?.cssVarPrefix===N)&&(_e=!1);let ve=(0,I.jsxs)(x.Fragment,{children:[(0,I.jsx)(Us,{themeId:A?t:void 0,theme:me,children:c}),_e&&(0,I.jsx)(Wi,{styles:me.generateStyleSheets?.()||[]})]});return k?ve:(0,I.jsx)(l.Provider,{value:ge,children:ve})}let m=typeof o==`string`?o:o.light,h=typeof o==`string`?o:o.dark;return{CssVarsProvider:p,useColorScheme:u,getInitColorSchemeScript:e=>Js({colorSchemeStorageKey:i,defaultLightColorScheme:m,defaultDarkColorScheme:h,modeStorageKey:r,...e})}}function rc(e=``){function t(...n){if(!n.length)return``;let r=n[0];return typeof r==`string`&&!r.match(/(#|\(|\)|(-?(\d*\.)?\d+)(px|em|%|ex|ch|rem|vw|vh|vmin|vmax|cm|mm|in|pt|pc))|^(-?(\d*\.)?\d+)$|(\d+ \d+ \d+)/)?`, var(--${e?`${e}-`:``}${r}${t(...n.slice(1))})`:`, ${r}`}return(n,...r)=>`var(--${e?`${e}-`:``}${n}${t(...r)})`}const ic=(e,t,n,r=[])=>{let i=e;t.forEach((e,a)=>{a===t.length-1?Array.isArray(i)?i[Number(e)]=n:i&&typeof i==`object`&&(i[e]=n):i&&typeof i==`object`&&(i[e]||(i[e]=r.includes(e)?[]:{}),i=i[e])})},ac=(e,t,n)=>{function r(e,i=[],a=[]){Object.entries(e).forEach(([e,o])=>{(!n||n&&!n([...i,e]))&&o!=null&&(typeof o==`object`&&Object.keys(o).length>0?r(o,[...i,e],Array.isArray(o)?[...a,e]:a):t([...i,e],o,a))})}r(e)};var oc=(e,t)=>typeof t==`number`?[`lineHeight`,`fontWeight`,`opacity`,`zIndex`].some(t=>e.includes(t))||e[e.length-1].toLowerCase().includes(`opacity`)?t:`${t}px`:t;function sc(e,t){let{prefix:n,shouldSkipGeneratingVar:r}=t||{},i={},a={},o={};return ac(e,(e,t,s)=>{if((typeof t==`string`||typeof t==`number`)&&(!r||!r(e,t))){let r=`--${n?`${n}-`:``}${e.join(`-`)}`,c=oc(e,t);Object.assign(i,{[r]:c}),ic(a,e,`var(${r})`,s),ic(o,e,`var(${r}, ${c})`,s)}},e=>e[0]===`vars`),{css:i,vars:a,varsWithDefaults:o}}function cc(e,t={}){let{getSelector:n=_,disableCssColorScheme:r,colorSchemeSelector:i,enableContrastVars:a}=t,{colorSchemes:o={},components:s,defaultColorScheme:c=`light`,...l}=e,{vars:u,css:d,varsWithDefaults:f}=sc(l,t),p=f,m={},{[c]:h,...g}=o;if(Object.entries(g||{}).forEach(([e,n])=>{let{vars:r,css:i,varsWithDefaults:a}=sc(n,t);p=$i(p,a),m[e]={css:i,vars:r}}),h){let{css:e,vars:n,varsWithDefaults:r}=sc(h,t);p=$i(p,r),m[c]={css:e,vars:n}}function _(t,n){let r=i;if(i===`class`&&(r=`.%s`),i===`data`&&(r=`[data-%s]`),i?.startsWith(`data-`)&&!i.includes(`%s`)&&(r=`[${i}="%s"]`),t){if(r===`media`)return e.defaultColorScheme===t?`:root`:{[`@media (prefers-color-scheme: ${o[t]?.palette?.mode||t})`]:{":root":n}};if(r)return e.defaultColorScheme===t?`:root, ${r.replace(`%s`,String(t))}`:r.replace(`%s`,String(t))}return`:root`}return{vars:p,generateThemeVars:()=>{let e={...u};return Object.entries(m).forEach(([,{vars:t}])=>{e=$i(e,t)}),e},generateStyleSheets:()=>{let t=[],i=e.defaultColorScheme||`light`;function s(e,n){Object.keys(n).length&&t.push(typeof e==`string`?{[e]:{...n}}:e)}s(n(void 0,{...d}),d);let{[i]:c,...l}=m;if(c){let{css:e}=c,t=o[i]?.palette?.mode,a=!r&&t?{colorScheme:t,...e}:{...e};s(n(i,{...a}),a)}return Object.entries(l).forEach(([e,{css:t}])=>{let i=o[e]?.palette?.mode,a=!r&&i?{colorScheme:i,...t}:{...t};s(n(e,{...a}),a)}),a&&t.push({":root":{"--__l-threshold":`0.7`,"--__l":`clamp(0, (l / var(--__l-threshold) - 1) * -infinity, 1)`,"--__a":`clamp(0.87, (l / var(--__l-threshold) - 1) * -infinity, 1)`}}),t}}}var lc=cc;function uc(e){return function(t){return e===`media`?`@media (prefers-color-scheme: ${t})`:e?e.startsWith(`data-`)&&!e.includes(`%s`)?`[${e}="${t}"] &`:e===`class`?`.${t} &`:e===`data`?`[data-${t}] &`:`${e.replace(`%s`,t)} &`:`&`}}function dc(e,t,n=void 0){let r={};for(let i in e){let a=e[i],o=``,s=!0;for(let e=0;e<a.length;e+=1){let r=a[e];r&&(o+=(s===!0?``:` `)+t(r),s=!1,n&&n[r]&&(o+=` `+n[r]))}r[i]=o}return r}function fc(e,t){return x.isValidElement(e)&&t.indexOf(e.type.muiName??e.type?._payload?.value?.muiName)!==-1}function pc(){return{text:{primary:`rgba(0, 0, 0, 0.87)`,secondary:`rgba(0, 0, 0, 0.6)`,disabled:`rgba(0, 0, 0, 0.38)`},divider:`rgba(0, 0, 0, 0.12)`,background:{paper:xn.white,default:xn.white},action:{active:`rgba(0, 0, 0, 0.54)`,hover:`rgba(0, 0, 0, 0.04)`,hoverOpacity:.04,selected:`rgba(0, 0, 0, 0.08)`,selectedOpacity:.08,disabled:`rgba(0, 0, 0, 0.26)`,disabledBackground:`rgba(0, 0, 0, 0.12)`,disabledOpacity:.38,focus:`rgba(0, 0, 0, 0.12)`,focusOpacity:.12,activatedOpacity:.12}}}const mc=pc();function hc(){return{text:{primary:xn.white,secondary:`rgba(255, 255, 255, 0.7)`,disabled:`rgba(255, 255, 255, 0.5)`,icon:`rgba(255, 255, 255, 0.5)`},divider:`rgba(255, 255, 255, 0.12)`,background:{paper:`#121212`,default:`#121212`},action:{active:xn.white,hover:`rgba(255, 255, 255, 0.08)`,hoverOpacity:.08,selected:`rgba(255, 255, 255, 0.16)`,selectedOpacity:.16,disabled:`rgba(255, 255, 255, 0.3)`,disabledBackground:`rgba(255, 255, 255, 0.12)`,disabledOpacity:.38,focus:`rgba(255, 255, 255, 0.12)`,focusOpacity:.12,activatedOpacity:.24}}}const gc=hc();function _c(e,t,n,r){let i=r.light||r,a=r.dark||r*1.5;e[t]||(e.hasOwnProperty(n)?e[t]=e[n]:t===`light`?e.light=gs(e.main,i):t===`dark`&&(e.dark=ms(e.main,a)))}function vc(e,t,n,r,i){let a=i.light||i,o=i.dark||i*1.5;t[n]||(t.hasOwnProperty(r)?t[n]=t[r]:n===`light`?t.light=`color-mix(in ${e}, ${t.main}, #fff ${(a*100).toFixed(0)}%)`:n===`dark`&&(t.dark=`color-mix(in ${e}, ${t.main}, #000 ${(o*100).toFixed(0)}%)`))}function yc(e=`light`){return e===`dark`?{main:wn[200],light:wn[50],dark:wn[400]}:{main:wn[700],light:wn[400],dark:wn[800]}}function bc(e=`light`){return e===`dark`?{main:Cn[200],light:Cn[50],dark:Cn[400]}:{main:Cn[500],light:Cn[300],dark:Cn[700]}}function xc(e=`light`){return e===`dark`?{main:Sn[500],light:Sn[300],dark:Sn[700]}:{main:Sn[700],light:Sn[400],dark:Sn[800]}}function Sc(e=`light`){return e===`dark`?{main:Tn[400],light:Tn[300],dark:Tn[700]}:{main:Tn[700],light:Tn[500],dark:Tn[900]}}function Cc(e=`light`){return e===`dark`?{main:En[400],light:En[300],dark:En[700]}:{main:En[800],light:En[500],dark:En[900]}}function wc(e=`light`){return e===`dark`?{main:Dn[400],light:Dn[300],dark:Dn[700]}:{main:`#ed6c02`,light:Dn[500],dark:Dn[900]}}function Tc(e){return`oklch(from ${e} var(--__l) 0 h / var(--__a))`}function Ec(e){let{mode:t=`light`,contrastThreshold:n=3,tonalOffset:r=.2,colorSpace:i,...a}=e,o=e.primary||yc(t),s=e.secondary||bc(t),c=e.error||xc(t),l=e.info||Sc(t),u=e.success||Cc(t),d=e.warning||wc(t);function f(e){return i?Tc(e):ds(e,gc.text.primary)>=n?gc.text.primary:mc.text.primary}let p=({color:e,name:t,mainShade:n=500,lightShade:a=300,darkShade:o=700})=>{if(e={...e},!e.main&&e[n]&&(e.main=e[n]),!e.hasOwnProperty(`main`))throw Error(kn(11,t?` (${t})`:``,n));if(typeof e.main!=`string`)throw Error(kn(12,t?` (${t})`:``,JSON.stringify(e.main)));return i?(vc(i,e,`light`,a,r),vc(i,e,`dark`,o,r)):(_c(e,`light`,a,r),_c(e,`dark`,o,r)),e.contrastText||=f(e.main),e},m;return t===`light`?m=pc():t===`dark`&&(m=hc()),$i({common:{...xn},mode:t,primary:p({color:o,name:`primary`}),secondary:p({color:s,name:`secondary`,mainShade:`A400`,lightShade:`A200`,darkShade:`A700`}),error:p({color:c,name:`error`}),warning:p({color:d,name:`warning`}),info:p({color:l,name:`info`}),success:p({color:u,name:`success`}),grey:On,contrastThreshold:n,getContrastText:f,augmentColor:p,tonalOffset:r,...m},a)}function Dc(e){let t={};return Object.entries(e).forEach(e=>{let[n,r]=e;typeof r==`object`&&(t[n]=`${r.fontStyle?`${r.fontStyle} `:``}${r.fontVariant?`${r.fontVariant} `:``}${r.fontWeight?`${r.fontWeight} `:``}${r.fontStretch?`${r.fontStretch} `:``}${r.fontSize||``}${r.lineHeight?`/${r.lineHeight} `:``}${r.fontFamily||``}`)}),t}function Oc(e,t){return{toolbar:{minHeight:56,[e.up(`xs`)]:{"@media (orientation: landscape)":{minHeight:48}},[e.up(`sm`)]:{minHeight:64}},...t}}function kc(e){return Math.round(e*1e5)/1e5}var Ac={textTransform:`uppercase`},jc=`"Roboto", "Helvetica", "Arial", sans-serif`;function Mc(e,t){let{fontFamily:n=jc,fontSize:r=14,fontWeightLight:i=300,fontWeightRegular:a=400,fontWeightMedium:o=500,fontWeightBold:s=700,htmlFontSize:c=16,allVariants:l,pxToRem:u,...d}=typeof t==`function`?t(e):t,f=r/14,p=u||(e=>`${e/c*f}rem`),m=(e,t,r,i,a)=>({fontFamily:n,fontWeight:e,fontSize:p(t),lineHeight:r,...n===jc?{letterSpacing:`${kc(i/t)}em`}:{},...a,...l});return $i({htmlFontSize:c,pxToRem:p,fontFamily:n,fontSize:r,fontWeightLight:i,fontWeightRegular:a,fontWeightMedium:o,fontWeightBold:s,h1:m(i,96,1.167,-1.5),h2:m(i,60,1.2,-.5),h3:m(a,48,1.167,0),h4:m(a,34,1.235,.25),h5:m(a,24,1.334,0),h6:m(o,20,1.6,.15),subtitle1:m(a,16,1.75,.15),subtitle2:m(o,14,1.57,.1),body1:m(a,16,1.5,.15),body2:m(a,14,1.43,.15),button:m(o,14,1.75,.4,Ac),caption:m(a,12,1.66,.4),overline:m(a,12,2.66,1,Ac),inherit:{fontFamily:`inherit`,fontWeight:`inherit`,fontSize:`inherit`,lineHeight:`inherit`,letterSpacing:`inherit`}},d,{clone:!1})}var Nc=.2,Pc=.14,Fc=.12;function Ic(...e){return[`${e[0]}px ${e[1]}px ${e[2]}px ${e[3]}px rgba(0,0,0,${Nc})`,`${e[4]}px ${e[5]}px ${e[6]}px ${e[7]}px rgba(0,0,0,${Pc})`,`${e[8]}px ${e[9]}px ${e[10]}px ${e[11]}px rgba(0,0,0,${Fc})`].join(`,`)}var Lc=[`none`,Ic(0,2,1,-1,0,1,1,0,0,1,3,0),Ic(0,3,1,-2,0,2,2,0,0,1,5,0),Ic(0,3,3,-2,0,3,4,0,0,1,8,0),Ic(0,2,4,-1,0,4,5,0,0,1,10,0),Ic(0,3,5,-1,0,5,8,0,0,1,14,0),Ic(0,3,5,-1,0,6,10,0,0,1,18,0),Ic(0,4,5,-2,0,7,10,1,0,2,16,1),Ic(0,5,5,-3,0,8,10,1,0,3,14,2),Ic(0,5,6,-3,0,9,12,1,0,3,16,2),Ic(0,6,6,-3,0,10,14,1,0,4,18,3),Ic(0,6,7,-4,0,11,15,1,0,4,20,3),Ic(0,7,8,-4,0,12,17,2,0,5,22,4),Ic(0,7,8,-4,0,13,19,2,0,5,24,4),Ic(0,7,9,-4,0,14,21,2,0,5,26,4),Ic(0,8,9,-5,0,15,22,2,0,6,28,5),Ic(0,8,10,-5,0,16,24,2,0,6,30,5),Ic(0,8,11,-5,0,17,26,2,0,6,32,5),Ic(0,9,11,-5,0,18,28,2,0,7,34,6),Ic(0,9,12,-6,0,19,29,2,0,7,36,6),Ic(0,10,13,-6,0,20,31,3,0,8,38,7),Ic(0,10,13,-6,0,21,33,3,0,8,40,7),Ic(0,10,14,-6,0,22,35,3,0,8,42,7),Ic(0,11,14,-7,0,23,36,3,0,9,44,8),Ic(0,11,15,-7,0,24,38,3,0,9,46,8)];const Rc={easeInOut:`cubic-bezier(0.4, 0, 0.2, 1)`,easeOut:`cubic-bezier(0.0, 0, 0.2, 1)`,easeIn:`cubic-bezier(0.4, 0, 1, 1)`,sharp:`cubic-bezier(0.4, 0, 0.6, 1)`},zc={shortest:150,shorter:200,short:250,standard:300,complex:375,enteringScreen:225,leavingScreen:195};function Bc(e){return`${Math.round(e)}ms`}function Vc(e){if(!e)return 0;let t=e/36;return Math.min(Math.round((4+15*t**.25+t/5)*10),3e3)}function Hc(e){let t={...Rc,...e.easing},n={...zc,...e.duration};return{getAutoHeightDuration:Vc,create:(e=[`all`],r={})=>{let{duration:i=n.standard,easing:a=t.easeInOut,delay:o=0,...s}=r;return(Array.isArray(e)?e:[e]).map(e=>`${e} ${typeof i==`string`?i:Bc(i)} ${a} ${typeof o==`string`?o:Bc(o)}`).join(`,`)},...e,easing:t,duration:n}}var Uc={mobileStepper:1e3,fab:1050,speedDial:1050,appBar:1100,drawer:1200,modal:1300,snackbar:1400,tooltip:1500};function Wc(e){return Zi(e)||e===void 0||typeof e==`string`||typeof e==`boolean`||typeof e==`number`||Array.isArray(e)}function Gc(e={}){let t={...e};function n(e){let t=Object.entries(e);for(let r=0;r<t.length;r++){let[i,a]=t[r];!Wc(a)||i.startsWith(`unstable_`)?delete e[i]:Zi(a)&&(e[i]={...a},n(e[i]))}}return n(t),`import { unstable_createBreakpoints as createBreakpoints, createTransitions } from '@mui/material/styles';

const theme = ${JSON.stringify(t,null,2)};

theme.breakpoints = createBreakpoints(theme.breakpoints || {});
theme.transitions = createTransitions(theme.transitions || {});

export default theme;`}function Kc(e){return typeof e==`number`?`${(e*100).toFixed(0)}%`:`calc((${e}) * 100%)`}var qc=e=>{if(!Number.isNaN(+e))return+e;let t=e.match(/\d*\.?\d+/g);if(!t)return 0;let n=0;for(let e=0;e<t.length;e+=1)n+=+t[e];return n};function Jc(e){Object.assign(e,{alpha(t,n){let r=this||e;return r.colorSpace?`oklch(from ${t} l c h / ${typeof n==`string`?`calc(${n})`:n})`:r.vars?`rgba(${t.replace(/var\(--([^,\s)]+)(?:,[^)]+)?\)+/g,`var(--$1Channel)`)} / ${typeof n==`string`?`calc(${n})`:n})`:fs(t,qc(n))},lighten(t,n){let r=this||e;return r.colorSpace?`color-mix(in ${r.colorSpace}, ${t}, #fff ${Kc(n)})`:gs(t,n)},darken(t,n){let r=this||e;return r.colorSpace?`color-mix(in ${r.colorSpace}, ${t}, #000 ${Kc(n)})`:ms(t,n)}})}function Yc(e={},...t){let{breakpoints:n,mixins:r={},spacing:i,palette:a={},transitions:o={},typography:s={},shape:c,colorSpace:l,...u}=e;if(e.vars&&e.generateThemeVars===void 0)throw Error(kn(20));let d=Ec({...a,colorSpace:l}),f=bo(e),p=$i(f,{mixins:Oc(f.breakpoints,r),palette:d,shadows:Lc.slice(),typography:Mc(d,s),transitions:Hc(o),zIndex:{...Uc}});return p=$i(p,u),p=t.reduce((e,t)=>$i(e,t),p),p.unstable_sxConfig={...po,...u?.unstable_sxConfig},p.unstable_sx=function(e){return _o({sx:e,theme:this})},p.toRuntimeSource=Gc,Jc(p),p}var Xc=Yc;function Zc(e){let t;return t=e<1?5.11916*e**2:4.5*Math.log(e+1)+2,Math.round(t*10)/1e3}var Qc=[...Array(25)].map((e,t)=>{if(t===0)return`none`;let n=Zc(t);return`linear-gradient(rgba(255 255 255 / ${n}), rgba(255 255 255 / ${n}))`});function $c(e){return{inputPlaceholder:e===`dark`?.5:.42,inputUnderline:e===`dark`?.7:.42,switchTrackDisabled:e===`dark`?.2:.12,switchTrack:e===`dark`?.3:.38}}function el(e){return e===`dark`?Qc:[]}function tl(e){let{palette:t={mode:`light`},opacity:n,overlays:r,colorSpace:i,...a}=e,o=Ec({...t,colorSpace:i});return{palette:o,opacity:{...$c(o.mode),...n},overlays:r||el(o.mode),...a}}function nl(e){return!!e[0].match(/(cssVarPrefix|colorSchemeSelector|modularCssLayers|rootSelector|typography|mixins|breakpoints|direction|transitions)/)||!!e[0].match(/sxConfig$/)||e[0]===`palette`&&!!e[1]?.match(/(mode|contrastThreshold|tonalOffset)/)}var rl=e=>[...[...Array(25)].map((t,n)=>`--${e?`${e}-`:``}overlays-${n}`),`--${e?`${e}-`:``}palette-AppBar-darkBg`,`--${e?`${e}-`:``}palette-AppBar-darkColor`],il=e=>(t,n)=>{let r=e.rootSelector||`:root`,i=e.colorSchemeSelector,a=i;if(i===`class`&&(a=`.%s`),i===`data`&&(a=`[data-%s]`),i?.startsWith(`data-`)&&!i.includes(`%s`)&&(a=`[${i}="%s"]`),e.defaultColorScheme===t){if(t===`dark`){let i={};return rl(e.cssVarPrefix).forEach(e=>{i[e]=n[e],delete n[e]}),a===`media`?{[r]:n,"@media (prefers-color-scheme: dark)":{[r]:i}}:a?{[a.replace(`%s`,t)]:i,[`${r}, ${a.replace(`%s`,t)}`]:n}:{[r]:{...n,...i}}}if(a&&a!==`media`)return`${r}, ${a.replace(`%s`,String(t))}`}else if(t){if(a===`media`)return{[`@media (prefers-color-scheme: ${String(t)})`]:{[r]:n}};if(a)return a.replace(`%s`,String(t))}return r};function al(e,t){t.forEach(t=>{e[t]||(e[t]={})})}function z(e,t,n){!e[t]&&n&&(e[t]=n)}function ol(e){return typeof e!=`string`||!e.startsWith(`hsl`)?e:ls(e)}function sl(e,t){`${t}Channel`in e||(e[`${t}Channel`]=ss(ol(e[t]),`MUI: Can't create \`palette.${t}Channel\` because \`palette.${t}\` is not one of these formats: #nnn, #nnnnnn, rgb(), rgba(), hsl(), hsla(), color().
To suppress this warning, you need to explicitly provide the \`palette.${t}Channel\` as a string (in rgb format, for example "12 12 12") or undefined if you want to remove the channel token.`))}function cl(e){return typeof e==`number`?`${e}px`:typeof e==`string`||typeof e==`function`||Array.isArray(e)?e:`8px`}var ll=e=>{try{return e()}catch{}};const ul=(e=`mui`)=>rc(e);function dl(e,t,n,r,i){if(!n)return;n=n===!0?{}:n;let a=i===`dark`?`dark`:`light`;if(!r){t[i]=tl({...n,palette:{mode:a,...n?.palette},colorSpace:e});return}let{palette:o,...s}=Xc({...r,palette:{mode:a,...n?.palette},colorSpace:e});return t[i]={...n,palette:o,opacity:{...$c(a),...n?.opacity},overlays:n?.overlays||el(a)},s}function fl(e={},...t){let{colorSchemes:n={light:!0},defaultColorScheme:r,disableCssColorScheme:i=!1,cssVarPrefix:a=`mui`,nativeColor:o=!1,shouldSkipGeneratingVar:s=nl,colorSchemeSelector:c=n.light&&n.dark?`media`:void 0,rootSelector:l=`:root`,...u}=e,d=Object.keys(n)[0],f=r||(n.light&&d!==`light`?`light`:d),p=ul(a),{[f]:m,light:h,dark:g,..._}=n,v={..._},y=m;if((f===`dark`&&!(`dark`in n)||f===`light`&&!(`light`in n))&&(y=!0),!y)throw Error(kn(21,f));let b;o&&(b=`oklch`);let x=dl(b,v,y,u,f);h&&!v.light&&dl(b,v,h,void 0,`light`),g&&!v.dark&&dl(b,v,g,void 0,`dark`);let S={defaultColorScheme:f,...x,cssVarPrefix:a,colorSchemeSelector:c,rootSelector:l,getCssVar:p,colorSchemes:v,font:{...Dc(x.typography),...x.font},spacing:cl(u.spacing)};Object.keys(S.colorSchemes).forEach(e=>{let t=S.colorSchemes[e].palette,n=e=>{let n=e.split(`-`),r=n[1],i=n[2];return p(e,t[r][i])};t.mode===`light`&&(z(t.common,`background`,`#fff`),z(t.common,`onBackground`,`#000`)),t.mode===`dark`&&(z(t.common,`background`,`#000`),z(t.common,`onBackground`,`#fff`));function r(e,t,n){if(b){let r;return e===ps&&(r=`transparent ${((1-n)*100).toFixed(0)}%`),e===hs&&(r=`#000 ${(n*100).toFixed(0)}%`),e===_s&&(r=`#fff ${(n*100).toFixed(0)}%`),`color-mix(in ${b}, ${t}, ${r})`}return e(t,n)}if(al(t,[`Alert`,`AppBar`,`Avatar`,`Button`,`Chip`,`FilledInput`,`LinearProgress`,`Skeleton`,`Slider`,`SnackbarContent`,`SpeedDialAction`,`StepConnector`,`StepContent`,`Switch`,`TableCell`,`Tooltip`]),t.mode===`light`){z(t.Alert,`errorColor`,r(hs,t.error.light,.6)),z(t.Alert,`infoColor`,r(hs,t.info.light,.6)),z(t.Alert,`successColor`,r(hs,t.success.light,.6)),z(t.Alert,`warningColor`,r(hs,t.warning.light,.6)),z(t.Alert,`errorFilledBg`,n(`palette-error-main`)),z(t.Alert,`infoFilledBg`,n(`palette-info-main`)),z(t.Alert,`successFilledBg`,n(`palette-success-main`)),z(t.Alert,`warningFilledBg`,n(`palette-warning-main`)),z(t.Alert,`errorFilledColor`,ll(()=>t.getContrastText(t.error.main))),z(t.Alert,`infoFilledColor`,ll(()=>t.getContrastText(t.info.main))),z(t.Alert,`successFilledColor`,ll(()=>t.getContrastText(t.success.main))),z(t.Alert,`warningFilledColor`,ll(()=>t.getContrastText(t.warning.main))),z(t.Alert,`errorStandardBg`,r(_s,t.error.light,.9)),z(t.Alert,`infoStandardBg`,r(_s,t.info.light,.9)),z(t.Alert,`successStandardBg`,r(_s,t.success.light,.9)),z(t.Alert,`warningStandardBg`,r(_s,t.warning.light,.9)),z(t.Alert,`errorIconColor`,n(`palette-error-main`)),z(t.Alert,`infoIconColor`,n(`palette-info-main`)),z(t.Alert,`successIconColor`,n(`palette-success-main`)),z(t.Alert,`warningIconColor`,n(`palette-warning-main`)),z(t.AppBar,`defaultBg`,n(`palette-grey-100`)),z(t.Avatar,`defaultBg`,n(`palette-grey-400`)),z(t.Button,`inheritContainedBg`,n(`palette-grey-300`)),z(t.Button,`inheritContainedHoverBg`,n(`palette-grey-A100`)),z(t.Chip,`defaultBorder`,n(`palette-grey-400`)),z(t.Chip,`defaultAvatarColor`,n(`palette-grey-700`)),z(t.Chip,`defaultIconColor`,n(`palette-grey-700`)),z(t.FilledInput,`bg`,`rgba(0, 0, 0, 0.06)`),z(t.FilledInput,`hoverBg`,`rgba(0, 0, 0, 0.09)`),z(t.FilledInput,`disabledBg`,`rgba(0, 0, 0, 0.12)`),z(t.LinearProgress,`primaryBg`,r(_s,t.primary.main,.62)),z(t.LinearProgress,`secondaryBg`,r(_s,t.secondary.main,.62)),z(t.LinearProgress,`errorBg`,r(_s,t.error.main,.62)),z(t.LinearProgress,`infoBg`,r(_s,t.info.main,.62)),z(t.LinearProgress,`successBg`,r(_s,t.success.main,.62)),z(t.LinearProgress,`warningBg`,r(_s,t.warning.main,.62)),z(t.Skeleton,`bg`,b?r(ps,t.text.primary,.11):`rgba(${n(`palette-text-primaryChannel`)} / 0.11)`),z(t.Slider,`primaryTrack`,r(_s,t.primary.main,.62)),z(t.Slider,`secondaryTrack`,r(_s,t.secondary.main,.62)),z(t.Slider,`errorTrack`,r(_s,t.error.main,.62)),z(t.Slider,`infoTrack`,r(_s,t.info.main,.62)),z(t.Slider,`successTrack`,r(_s,t.success.main,.62)),z(t.Slider,`warningTrack`,r(_s,t.warning.main,.62));let e=b?r(hs,t.background.default,.6825):ys(t.background.default,.8);z(t.SnackbarContent,`bg`,e),z(t.SnackbarContent,`color`,ll(()=>b?gc.text.primary:t.getContrastText(e))),z(t.SpeedDialAction,`fabHoverBg`,ys(t.background.paper,.15)),z(t.StepConnector,`border`,n(`palette-grey-400`)),z(t.StepContent,`border`,n(`palette-grey-400`)),z(t.Switch,`defaultColor`,n(`palette-common-white`)),z(t.Switch,`defaultDisabledColor`,n(`palette-grey-100`)),z(t.Switch,`primaryDisabledColor`,r(_s,t.primary.main,.62)),z(t.Switch,`secondaryDisabledColor`,r(_s,t.secondary.main,.62)),z(t.Switch,`errorDisabledColor`,r(_s,t.error.main,.62)),z(t.Switch,`infoDisabledColor`,r(_s,t.info.main,.62)),z(t.Switch,`successDisabledColor`,r(_s,t.success.main,.62)),z(t.Switch,`warningDisabledColor`,r(_s,t.warning.main,.62)),z(t.TableCell,`border`,r(_s,r(ps,t.divider,1),.88)),z(t.Tooltip,`bg`,r(ps,t.grey[700],.92))}if(t.mode===`dark`){z(t.Alert,`errorColor`,r(_s,t.error.light,.6)),z(t.Alert,`infoColor`,r(_s,t.info.light,.6)),z(t.Alert,`successColor`,r(_s,t.success.light,.6)),z(t.Alert,`warningColor`,r(_s,t.warning.light,.6)),z(t.Alert,`errorFilledBg`,n(`palette-error-dark`)),z(t.Alert,`infoFilledBg`,n(`palette-info-dark`)),z(t.Alert,`successFilledBg`,n(`palette-success-dark`)),z(t.Alert,`warningFilledBg`,n(`palette-warning-dark`)),z(t.Alert,`errorFilledColor`,ll(()=>t.getContrastText(t.error.dark))),z(t.Alert,`infoFilledColor`,ll(()=>t.getContrastText(t.info.dark))),z(t.Alert,`successFilledColor`,ll(()=>t.getContrastText(t.success.dark))),z(t.Alert,`warningFilledColor`,ll(()=>t.getContrastText(t.warning.dark))),z(t.Alert,`errorStandardBg`,r(hs,t.error.light,.9)),z(t.Alert,`infoStandardBg`,r(hs,t.info.light,.9)),z(t.Alert,`successStandardBg`,r(hs,t.success.light,.9)),z(t.Alert,`warningStandardBg`,r(hs,t.warning.light,.9)),z(t.Alert,`errorIconColor`,n(`palette-error-main`)),z(t.Alert,`infoIconColor`,n(`palette-info-main`)),z(t.Alert,`successIconColor`,n(`palette-success-main`)),z(t.Alert,`warningIconColor`,n(`palette-warning-main`)),z(t.AppBar,`defaultBg`,n(`palette-grey-900`)),z(t.AppBar,`darkBg`,n(`palette-background-paper`)),z(t.AppBar,`darkColor`,n(`palette-text-primary`)),z(t.Avatar,`defaultBg`,n(`palette-grey-600`)),z(t.Button,`inheritContainedBg`,n(`palette-grey-800`)),z(t.Button,`inheritContainedHoverBg`,n(`palette-grey-700`)),z(t.Chip,`defaultBorder`,n(`palette-grey-700`)),z(t.Chip,`defaultAvatarColor`,n(`palette-grey-300`)),z(t.Chip,`defaultIconColor`,n(`palette-grey-300`)),z(t.FilledInput,`bg`,`rgba(255, 255, 255, 0.09)`),z(t.FilledInput,`hoverBg`,`rgba(255, 255, 255, 0.13)`),z(t.FilledInput,`disabledBg`,`rgba(255, 255, 255, 0.12)`),z(t.LinearProgress,`primaryBg`,r(hs,t.primary.main,.5)),z(t.LinearProgress,`secondaryBg`,r(hs,t.secondary.main,.5)),z(t.LinearProgress,`errorBg`,r(hs,t.error.main,.5)),z(t.LinearProgress,`infoBg`,r(hs,t.info.main,.5)),z(t.LinearProgress,`successBg`,r(hs,t.success.main,.5)),z(t.LinearProgress,`warningBg`,r(hs,t.warning.main,.5)),z(t.Skeleton,`bg`,b?r(ps,t.text.primary,.13):`rgba(${n(`palette-text-primaryChannel`)} / 0.13)`),z(t.Slider,`primaryTrack`,r(hs,t.primary.main,.5)),z(t.Slider,`secondaryTrack`,r(hs,t.secondary.main,.5)),z(t.Slider,`errorTrack`,r(hs,t.error.main,.5)),z(t.Slider,`infoTrack`,r(hs,t.info.main,.5)),z(t.Slider,`successTrack`,r(hs,t.success.main,.5)),z(t.Slider,`warningTrack`,r(hs,t.warning.main,.5));let e=b?r(_s,t.background.default,.985):ys(t.background.default,.98);z(t.SnackbarContent,`bg`,e),z(t.SnackbarContent,`color`,ll(()=>b?mc.text.primary:t.getContrastText(e))),z(t.SpeedDialAction,`fabHoverBg`,ys(t.background.paper,.15)),z(t.StepConnector,`border`,n(`palette-grey-600`)),z(t.StepContent,`border`,n(`palette-grey-600`)),z(t.Switch,`defaultColor`,n(`palette-grey-300`)),z(t.Switch,`defaultDisabledColor`,n(`palette-grey-600`)),z(t.Switch,`primaryDisabledColor`,r(hs,t.primary.main,.55)),z(t.Switch,`secondaryDisabledColor`,r(hs,t.secondary.main,.55)),z(t.Switch,`errorDisabledColor`,r(hs,t.error.main,.55)),z(t.Switch,`infoDisabledColor`,r(hs,t.info.main,.55)),z(t.Switch,`successDisabledColor`,r(hs,t.success.main,.55)),z(t.Switch,`warningDisabledColor`,r(hs,t.warning.main,.55)),z(t.TableCell,`border`,r(hs,r(ps,t.divider,1),.68)),z(t.Tooltip,`bg`,r(ps,t.grey[700],.92))}sl(t.background,`default`),sl(t.background,`paper`),sl(t.common,`background`),sl(t.common,`onBackground`),sl(t,`divider`),Object.keys(t).forEach(e=>{let n=t[e];e!==`tonalOffset`&&n&&typeof n==`object`&&(n.main&&z(t[e],`mainChannel`,ss(ol(n.main))),n.light&&z(t[e],`lightChannel`,ss(ol(n.light))),n.dark&&z(t[e],`darkChannel`,ss(ol(n.dark))),n.contrastText&&z(t[e],`contrastTextChannel`,ss(ol(n.contrastText))),e===`text`&&(sl(t[e],`primary`),sl(t[e],`secondary`)),e===`action`&&(n.active&&sl(t[e],`active`),n.selected&&sl(t[e],`selected`)))})}),S=t.reduce((e,t)=>$i(e,t),S);let C={prefix:a,disableCssColorScheme:i,shouldSkipGeneratingVar:s,getSelector:il(S),enableContrastVars:o},{vars:w,generateThemeVars:T,generateStyleSheets:E}=lc(S,C);return S.vars=w,Object.entries(S.colorSchemes[S.defaultColorScheme]).forEach(([e,t])=>{S[e]=t}),S.generateThemeVars=T,S.generateStyleSheets=E,S.generateSpacing=function(){return La(u.spacing,ka(this))},S.getColorSchemeSelector=uc(c),S.spacing=S.generateSpacing(),S.shouldSkipGeneratingVar=s,S.unstable_sxConfig={...po,...u?.unstable_sxConfig},S.unstable_sx=function(e){return _o({sx:e,theme:this})},S.toRuntimeSource=Gc,S}function pl(e,t,n){e.colorSchemes&&n&&(e.colorSchemes[t]={...n!==!0&&n,palette:Ec({...n===!0?{}:n.palette,mode:t})})}function ml(e={},...t){let{palette:n,cssVariables:r=!1,colorSchemes:i=n?void 0:{light:!0},defaultColorScheme:a=n?.mode,...o}=e,s=a||`light`,c=i?.[s],l={...i,...n?{[s]:{...typeof c!=`boolean`&&c,palette:n}}:void 0};if(r===!1){if(!(`colorSchemes`in e))return Xc(e,...t);let r=n;`palette`in e||l[s]&&(l[s]===!0?s===`dark`&&(r={mode:`dark`}):r=l[s].palette);let i=Xc({...e,palette:r},...t);return i.defaultColorScheme=s,i.colorSchemes=l,i.palette.mode===`light`&&(i.colorSchemes.light={...l.light!==!0&&l.light,palette:i.palette},pl(i,`dark`,l.dark)),i.palette.mode===`dark`&&(i.colorSchemes.dark={...l.dark!==!0&&l.dark,palette:i.palette},pl(i,`light`,l.light)),i}return!n&&!(`light`in l)&&s===`light`&&(l.light=!0),fl({...o,colorSchemes:l,defaultColorScheme:s,...typeof r!=`boolean`&&r},...t)}var hl=ml();function gl(){let e=Eo(hl);return e.$$material||e}function _l(e){return e!==`ownerState`&&e!==`theme`&&e!==`sx`&&e!==`as`}var vl=_l,yl=e=>vl(e)&&e!==`classes`,B=Yo({themeId:An,defaultTheme:hl,rootShouldForwardProp:yl});function bl({theme:e,...t}){let n=`$$material`in e?e[An]:void 0;return(0,I.jsx)(Us,{...t,themeId:n?An:void 0,theme:n||e})}const xl={attribute:`data-mui-color-scheme`,colorSchemeStorageKey:`mui-color-scheme`,defaultLightColorScheme:`light`,defaultDarkColorScheme:`dark`,modeStorageKey:`mui-mode`};var{CssVarsProvider:Sl,useColorScheme:Cl,getInitColorSchemeScript:wl}=nc({themeId:An,theme:()=>ml({cssVariables:!0}),colorSchemeStorageKey:xl.colorSchemeStorageKey,modeStorageKey:xl.modeStorageKey,defaultColorScheme:{light:xl.defaultLightColorScheme,dark:xl.defaultDarkColorScheme},resolveTheme:e=>{let t={...e,typography:Mc(e.palette,e.typography)};return t.unstable_sx=function(e){return _o({sx:e,theme:this})},t}});const Tl=Sl;function El({theme:e,...t}){let n=x.useMemo(()=>{if(typeof e==`function`)return e;let t=`$$material`in e?e[An]:e;return`colorSchemes`in t?null:`vars`in t?e:{...e,vars:null}},[e]);return n?(0,I.jsx)(bl,{theme:n,...t}):(0,I.jsx)(Tl,{theme:e,...t})}var V=ha;function Dl(...e){return e.reduce((e,t)=>t==null?e:function(...n){e.apply(this,n),t.apply(this,n)},()=>{})}function Ol(e){return(0,I.jsx)(ko,{...e,defaultTheme:hl,themeId:An})}var kl=Ol;function Al(e){return function(t){return(0,I.jsx)(kl,{styles:typeof e==`function`?n=>e({theme:n,...t}):e})}}function jl(){return jo}var Ml=Gs;function Nl(e){return Ns(e)}function Pl(e){return zo(`MuiSvgIcon`,e)}Bo(`MuiSvgIcon`,[`root`,`colorPrimary`,`colorSecondary`,`colorAction`,`colorError`,`colorDisabled`,`fontSizeInherit`,`fontSizeSmall`,`fontSizeMedium`,`fontSizeLarge`]);var Fl=e=>{let{color:t,fontSize:n,classes:r}=e;return dc({root:[`root`,t!==`inherit`&&`color${V(t)}`,`fontSize${V(n)}`]},Pl,r)},Il=B(`svg`,{name:`MuiSvgIcon`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.color!==`inherit`&&t[`color${V(n.color)}`],t[`fontSize${V(n.fontSize)}`]]}})(Ml(({theme:e})=>({userSelect:`none`,width:`1em`,height:`1em`,display:`inline-block`,flexShrink:0,transition:e.transitions?.create?.(`fill`,{duration:(e.vars??e).transitions?.duration?.shorter}),variants:[{props:e=>!e.hasSvgAsChild,style:{fill:`currentColor`}},{props:{fontSize:`inherit`},style:{fontSize:`inherit`}},{props:{fontSize:`small`},style:{fontSize:e.typography?.pxToRem?.(20)||`1.25rem`}},{props:{fontSize:`medium`},style:{fontSize:e.typography?.pxToRem?.(24)||`1.5rem`}},{props:{fontSize:`large`},style:{fontSize:e.typography?.pxToRem?.(35)||`2.1875rem`}},...Object.entries((e.vars??e).palette).filter(([,e])=>e&&e.main).map(([t])=>({props:{color:t},style:{color:(e.vars??e).palette?.[t]?.main}})),{props:{color:`action`},style:{color:(e.vars??e).palette?.action?.active}},{props:{color:`disabled`},style:{color:(e.vars??e).palette?.action?.disabled}},{props:{color:`inherit`},style:{color:void 0}}]}))),Ll=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiSvgIcon`}),{children:r,className:i,color:a=`inherit`,component:o=`svg`,fontSize:s=`medium`,htmlColor:c,inheritViewBox:l=!1,titleAccess:u,viewBox:d=`0 0 24 24`,...f}=n,p=x.isValidElement(r)&&r.type===`svg`,m={...n,color:a,component:o,fontSize:s,instanceFontSize:e.fontSize,inheritViewBox:l,viewBox:d,hasSvgAsChild:p},h={};return l||(h.viewBox=d),(0,I.jsxs)(Il,{as:o,className:R(Fl(m).root,i),focusable:`false`,color:c,"aria-hidden":u?void 0:!0,role:u?`img`:void 0,ref:t,...h,...f,...p&&r.props,ownerState:m,children:[p?r.props.children:r,u?(0,I.jsx)(`title`,{children:u}):null]})});Ll.muiName=`SvgIcon`;var Rl=Ll;function zl(e,t){function n(t,n){return(0,I.jsx)(Rl,{"data-testid":void 0,ref:n,...t,children:e})}return n.muiName=Rl.muiName,x.memo(x.forwardRef(n))}function Bl(e,t=166){let n;function r(...r){clearTimeout(n),n=setTimeout(()=>{e.apply(this,r)},t)}return r.clear=()=>{clearTimeout(n)},r}var Vl=fc;function Hl(e){return e&&e.ownerDocument||document}function Ul(e){return Hl(e).defaultView||window}function Wl(e,t){typeof e==`function`?e(t):e&&(e.current=t)}var Gl=es,Kl=Rs;function ql(e){let{controlled:t,default:n,name:r,state:i=`value`}=e,{current:a}=x.useRef(t!==void 0),[o,s]=x.useState(n);return[a?t:o,x.useCallback(e=>{a||s(e)},[])]}var Jl=ql;function Yl(e){let t=x.useRef(e);return es(()=>{t.current=e}),x.useRef((...e)=>(0,t.current)(...e)).current}var Xl=Yl,Zl=Xl;function Ql(...e){let t=x.useRef(void 0),n=x.useCallback(t=>{let n=e.map(e=>{if(e==null)return null;if(typeof e==`function`){let n=e,r=n(t);return typeof r==`function`?r:()=>{n(null)}}return e.current=t,()=>{e.current=null}});return()=>{n.forEach(e=>e?.())}},e);return x.useMemo(()=>e.every(e=>e==null)?null:e=>{t.current&&=(t.current(),void 0),e!=null&&(t.current=n(e))},e)}var $l=Ql;function eu(e,t){if(e==null)return{};var n={};for(var r in e)if({}.hasOwnProperty.call(e,r)){if(t.indexOf(r)!==-1)continue;n[r]=e[r]}return n}function tu(e,t){return tu=Object.setPrototypeOf?Object.setPrototypeOf.bind():function(e,t){return e.__proto__=t,e},tu(e,t)}function nu(e,t){e.prototype=Object.create(t.prototype),e.prototype.constructor=e,tu(e,t)}var ru={disabled:!1},iu=x.createContext(null),au=function(e){return e.scrollTop},ou=c(m()),su=`unmounted`,cu=`exited`,lu=`entering`,uu=`entered`,du=`exiting`,fu=function(e){nu(t,e);function t(t,n){var r=e.call(this,t,n)||this,i=n,a=i&&!i.isMounting?t.enter:t.appear,o;return r.appearStatus=null,t.in?a?(o=cu,r.appearStatus=lu):o=uu:o=t.unmountOnExit||t.mountOnEnter?su:cu,r.state={status:o},r.nextCallback=null,r}t.getDerivedStateFromProps=function(e,t){return e.in&&t.status===`unmounted`?{status:cu}:null};var n=t.prototype;return n.componentDidMount=function(){this.updateStatus(!0,this.appearStatus)},n.componentDidUpdate=function(e){var t=null;if(e!==this.props){var n=this.state.status;this.props.in?n!==`entering`&&n!==`entered`&&(t=lu):(n===`entering`||n===`entered`)&&(t=du)}this.updateStatus(!1,t)},n.componentWillUnmount=function(){this.cancelNextCallback()},n.getTimeouts=function(){var e=this.props.timeout,t=n=r=e,n,r;return e!=null&&typeof e!=`number`&&(t=e.exit,n=e.enter,r=e.appear===void 0?n:e.appear),{exit:t,enter:n,appear:r}},n.updateStatus=function(e,t){if(e===void 0&&(e=!1),t!==null)if(this.cancelNextCallback(),t===`entering`){if(this.props.unmountOnExit||this.props.mountOnEnter){var n=this.props.nodeRef?this.props.nodeRef.current:ou.default.findDOMNode(this);n&&au(n)}this.performEnter(e)}else this.performExit();else this.props.unmountOnExit&&this.state.status===`exited`&&this.setState({status:su})},n.performEnter=function(e){var t=this,n=this.props.enter,r=this.context?this.context.isMounting:e,i=this.props.nodeRef?[r]:[ou.default.findDOMNode(this),r],a=i[0],o=i[1],s=this.getTimeouts(),c=r?s.appear:s.enter;if(!e&&!n||ru.disabled){this.safeSetState({status:uu},function(){t.props.onEntered(a)});return}this.props.onEnter(a,o),this.safeSetState({status:lu},function(){t.props.onEntering(a,o),t.onTransitionEnd(c,function(){t.safeSetState({status:uu},function(){t.props.onEntered(a,o)})})})},n.performExit=function(){var e=this,t=this.props.exit,n=this.getTimeouts(),r=this.props.nodeRef?void 0:ou.default.findDOMNode(this);if(!t||ru.disabled){this.safeSetState({status:cu},function(){e.props.onExited(r)});return}this.props.onExit(r),this.safeSetState({status:du},function(){e.props.onExiting(r),e.onTransitionEnd(n.exit,function(){e.safeSetState({status:cu},function(){e.props.onExited(r)})})})},n.cancelNextCallback=function(){this.nextCallback!==null&&(this.nextCallback.cancel(),this.nextCallback=null)},n.safeSetState=function(e,t){t=this.setNextCallback(t),this.setState(e,t)},n.setNextCallback=function(e){var t=this,n=!0;return this.nextCallback=function(r){n&&(n=!1,t.nextCallback=null,e(r))},this.nextCallback.cancel=function(){n=!1},this.nextCallback},n.onTransitionEnd=function(e,t){this.setNextCallback(t);var n=this.props.nodeRef?this.props.nodeRef.current:ou.default.findDOMNode(this),r=e==null&&!this.props.addEndListener;if(!n||r){setTimeout(this.nextCallback,0);return}if(this.props.addEndListener){var i=this.props.nodeRef?[this.nextCallback]:[n,this.nextCallback],a=i[0],o=i[1];this.props.addEndListener(a,o)}e!=null&&setTimeout(this.nextCallback,e)},n.render=function(){var e=this.state.status;if(e===`unmounted`)return null;var t=this.props,n=t.children;t.in,t.mountOnEnter,t.unmountOnExit,t.appear,t.enter,t.exit,t.timeout,t.addEndListener,t.onEnter,t.onEntering,t.onEntered,t.onExit,t.onExiting,t.onExited,t.nodeRef;var r=eu(t,[`children`,`in`,`mountOnEnter`,`unmountOnExit`,`appear`,`enter`,`exit`,`timeout`,`addEndListener`,`onEnter`,`onEntering`,`onEntered`,`onExit`,`onExiting`,`onExited`,`nodeRef`]);return x.createElement(iu.Provider,{value:null},typeof n==`function`?n(e,r):x.cloneElement(x.Children.only(n),r))},t}(x.Component);fu.contextType=iu,fu.propTypes={};function pu(){}fu.defaultProps={in:!1,mountOnEnter:!1,unmountOnExit:!1,appear:!1,enter:!0,exit:!0,onEnter:pu,onEntering:pu,onEntered:pu,onExit:pu,onExiting:pu,onExited:pu},fu.UNMOUNTED=su,fu.EXITED=cu,fu.ENTERING=lu,fu.ENTERED=uu,fu.EXITING=du;var mu=fu;function hu(e){if(e===void 0)throw ReferenceError(`this hasn't been initialised - super() hasn't been called`);return e}function gu(e,t){var n=function(e){return t&&(0,x.isValidElement)(e)?t(e):e},r=Object.create(null);return e&&x.Children.map(e,function(e){return e}).forEach(function(e){r[e.key]=n(e)}),r}function _u(e,t){e||={},t||={};function n(n){return n in t?t[n]:e[n]}var r=Object.create(null),i=[];for(var a in e)a in t?i.length&&(r[a]=i,i=[]):i.push(a);var o,s={};for(var c in t){if(r[c])for(o=0;o<r[c].length;o++){var l=r[c][o];s[r[c][o]]=n(l)}s[c]=n(c)}for(o=0;o<i.length;o++)s[i[o]]=n(i[o]);return s}function vu(e,t,n){return n[t]==null?e.props[t]:n[t]}function yu(e,t){return gu(e.children,function(n){return(0,x.cloneElement)(n,{onExited:t.bind(null,n),in:!0,appear:vu(n,`appear`,e),enter:vu(n,`enter`,e),exit:vu(n,`exit`,e)})})}function bu(e,t,n){var r=gu(e.children),i=_u(t,r);return Object.keys(i).forEach(function(a){var o=i[a];if((0,x.isValidElement)(o)){var s=a in t,c=a in r,l=t[a],u=(0,x.isValidElement)(l)&&!l.props.in;c&&(!s||u)?i[a]=(0,x.cloneElement)(o,{onExited:n.bind(null,o),in:!0,exit:vu(o,`exit`,e),enter:vu(o,`enter`,e)}):!c&&s&&!u?i[a]=(0,x.cloneElement)(o,{in:!1}):c&&s&&(0,x.isValidElement)(l)&&(i[a]=(0,x.cloneElement)(o,{onExited:n.bind(null,o),in:l.props.in,exit:vu(o,`exit`,e),enter:vu(o,`enter`,e)}))}}),i}var xu=Object.values||function(e){return Object.keys(e).map(function(t){return e[t]})},Su={component:`div`,childFactory:function(e){return e}},Cu=function(e){nu(t,e);function t(t,n){var r=e.call(this,t,n)||this;return r.state={contextValue:{isMounting:!0},handleExited:r.handleExited.bind(hu(r)),firstRender:!0},r}var n=t.prototype;return n.componentDidMount=function(){this.mounted=!0,this.setState({contextValue:{isMounting:!1}})},n.componentWillUnmount=function(){this.mounted=!1},t.getDerivedStateFromProps=function(e,t){var n=t.children,r=t.handleExited;return{children:t.firstRender?yu(e,r):bu(e,n,r),firstRender:!1}},n.handleExited=function(e,t){var n=gu(this.props.children);e.key in n||(e.props.onExited&&e.props.onExited(t),this.mounted&&this.setState(function(t){var n=jn({},t.children);return delete n[e.key],{children:n}}))},n.render=function(){var e=this.props,t=e.component,n=e.childFactory,r=eu(e,[`component`,`childFactory`]),i=this.state.contextValue,a=xu(this.state.children).map(n);return delete r.appear,delete r.enter,delete r.exit,t===null?x.createElement(iu.Provider,{value:i},a):x.createElement(iu.Provider,{value:i},x.createElement(t,r,a))},t}(x.Component);Cu.propTypes={},Cu.defaultProps=Su;var wu=Cu,Tu={};function Eu(e,t){let n=x.useRef(Tu);return n.current===Tu&&(n.current=e(t)),n}var Du=[];function Ou(e){x.useEffect(e,Du)}var ku=class e{static create(){return new e}currentId=null;start(e,t){this.clear(),this.currentId=setTimeout(()=>{this.currentId=null,t()},e)}clear=()=>{this.currentId!==null&&(clearTimeout(this.currentId),this.currentId=null)};disposeEffect=()=>this.clear};function Au(){let e=Eu(ku.create).current;return Ou(e.disposeEffect),e}const ju=e=>e.scrollTop;function Mu(e,t){let{timeout:n,easing:r,style:i={}}=e;return{duration:i.transitionDuration??(typeof n==`number`?n:n[t.mode]||0),easing:i.transitionTimingFunction??(typeof r==`object`?r[t.mode]:r),delay:i.transitionDelay}}function Nu(e){return typeof e==`string`}var Pu=Nu;function Fu(e,t,n){return e===void 0||Pu(e)?t:{...t,ownerState:{...t.ownerState,...n}}}var Iu=Fu;function Lu(e,t,n){return typeof e==`function`?e(t,n):e}var Ru=Lu;function zu(e,t=[]){if(e===void 0)return{};let n={};return Object.keys(e).filter(n=>n.match(/^on[A-Z]/)&&typeof e[n]==`function`&&!t.includes(n)).forEach(t=>{n[t]=e[t]}),n}var Bu=zu;function Vu(e){if(e===void 0)return{};let t={};return Object.keys(e).filter(t=>!(t.match(/^on[A-Z]/)&&typeof e[t]==`function`)).forEach(n=>{t[n]=e[n]}),t}var Hu=Vu;function Uu(e){let{getSlotProps:t,additionalProps:n,externalSlotProps:r,externalForwardedProps:i,className:a}=e;if(!t){let e=R(n?.className,a,i?.className,r?.className),t={...n?.style,...i?.style,...r?.style},o={...n,...i,...r};return e.length>0&&(o.className=e),Object.keys(t).length>0&&(o.style=t),{props:o,internalRef:void 0}}let o=Bu({...i,...r}),s=Hu(r),c=Hu(i),l=t(o),u=R(l?.className,n?.className,a,i?.className,r?.className),d={...l?.style,...n?.style,...i?.style,...r?.style},f={...l,...n,...c,...s};return u.length>0&&(f.className=u),Object.keys(d).length>0&&(f.style=d),{props:f,internalRef:l.ref}}var Wu=Uu;function Gu(e,t){let{className:n,elementType:r,ownerState:i,externalForwardedProps:a,internalForwardedProps:o,shouldForwardComponentProp:s=!1,...c}=t,{component:l,slots:u={[e]:void 0},slotProps:d={[e]:void 0},...f}=a,p=u[e]||r,m=Ru(d[e],i),{props:{component:h,...g},internalRef:_}=Wu({className:n,...c,externalForwardedProps:e===`root`?f:void 0,externalSlotProps:m}),v=Ql(_,m?.ref,t.ref),y=e===`root`?h||l:h;return[p,Iu(p,{...e===`root`&&!l&&!u[e]&&o,...e!==`root`&&!u[e]&&o,...g,...y&&!s&&{as:y},...y&&s&&{component:y},ref:v},i)]}function Ku(e){return zo(`MuiCollapse`,e)}Bo(`MuiCollapse`,[`root`,`horizontal`,`vertical`,`entered`,`hidden`,`wrapper`,`wrapperInner`]);var qu=e=>{let{orientation:t,classes:n}=e;return dc({root:[`root`,`${t}`],entered:[`entered`],hidden:[`hidden`],wrapper:[`wrapper`,`${t}`],wrapperInner:[`wrapperInner`,`${t}`]},Ku,n)},Ju=B(`div`,{name:`MuiCollapse`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.orientation],n.state===`entered`&&t.entered,n.state===`exited`&&!n.in&&n.collapsedSize===`0px`&&t.hidden]}})(Ml(({theme:e})=>({height:0,overflow:`hidden`,transition:e.transitions.create(`height`),variants:[{props:{orientation:`horizontal`},style:{height:`auto`,width:0,transition:e.transitions.create(`width`)}},{props:{state:`entered`},style:{height:`auto`,overflow:`visible`}},{props:{state:`entered`,orientation:`horizontal`},style:{width:`auto`}},{props:({ownerState:e})=>e.state===`exited`&&!e.in&&e.collapsedSize===`0px`,style:{visibility:`hidden`}}]}))),Yu=B(`div`,{name:`MuiCollapse`,slot:`Wrapper`})({display:`flex`,width:`100%`,variants:[{props:{orientation:`horizontal`},style:{width:`auto`,height:`100%`}}]}),Xu=B(`div`,{name:`MuiCollapse`,slot:`WrapperInner`})({width:`100%`,variants:[{props:{orientation:`horizontal`},style:{width:`auto`,height:`100%`}}]}),Zu=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiCollapse`}),{addEndListener:r,children:i,className:a,collapsedSize:o=`0px`,component:s,easing:c,in:l,onEnter:u,onEntered:d,onEntering:f,onExit:p,onExited:m,onExiting:h,orientation:g=`vertical`,slots:_={},slotProps:v={},style:y,timeout:b=zc.standard,TransitionComponent:S=mu,...C}=n,w={...n,orientation:g,collapsedSize:o},T=qu(w),E=gl(),D=Au(),O=x.useRef(null),k=x.useRef(),ee=typeof o==`number`?`${o}px`:o,A=g===`horizontal`,j=A?`width`:`height`,M=x.useRef(null),te=$l(t,M),N=e=>t=>{if(e){let n=M.current;t===void 0?e(n):e(n,t)}},P=()=>O.current?O.current[A?`clientWidth`:`clientHeight`]:0,F=N((e,t)=>{O.current&&A&&(O.current.style.position=`absolute`),e.style[j]=ee,u&&u(e,t)}),ne=N((e,t)=>{let n=P();O.current&&A&&(O.current.style.position=``);let{duration:r,easing:i}=Mu({style:y,timeout:b,easing:c},{mode:`enter`});if(b===`auto`){let t=E.transitions.getAutoHeightDuration(n);e.style.transitionDuration=`${t}ms`,k.current=t}else e.style.transitionDuration=typeof r==`string`?r:`${r}ms`;e.style[j]=`${n}px`,e.style.transitionTimingFunction=i,f&&f(e,t)}),re=N((e,t)=>{e.style[j]=`auto`,d&&d(e,t)}),ie=N(e=>{e.style[j]=`${P()}px`,p&&p(e)}),ae=N(m),oe=N(e=>{let t=P(),{duration:n,easing:r}=Mu({style:y,timeout:b,easing:c},{mode:`exit`});if(b===`auto`){let n=E.transitions.getAutoHeightDuration(t);e.style.transitionDuration=`${n}ms`,k.current=n}else e.style.transitionDuration=typeof n==`string`?n:`${n}ms`;e.style[j]=ee,e.style.transitionTimingFunction=r,h&&h(e)}),se=e=>{b===`auto`&&D.start(k.current||0,e),r&&r(M.current,e)},ce={slots:_,slotProps:v,component:s},[le,ue]=Gu(`root`,{ref:te,className:R(T.root,a),elementType:Ju,externalForwardedProps:ce,ownerState:w,additionalProps:{style:{[A?`minWidth`:`minHeight`]:ee,...y}}}),[de,fe]=Gu(`wrapper`,{ref:O,className:T.wrapper,elementType:Yu,externalForwardedProps:ce,ownerState:w}),[pe,me]=Gu(`wrapperInner`,{className:T.wrapperInner,elementType:Xu,externalForwardedProps:ce,ownerState:w});return(0,I.jsx)(S,{in:l,onEnter:F,onEntered:re,onEntering:ne,onExit:ie,onExited:ae,onExiting:oe,addEndListener:se,nodeRef:M,timeout:b===`auto`?null:b,...C,children:(e,{ownerState:t,...n})=>{let r={...w,state:e};return(0,I.jsx)(le,{...ue,className:R(ue.className,{entered:T.entered,exited:!l&&ee===`0px`&&T.hidden}[e]),ownerState:r,...n,children:(0,I.jsx)(de,{...fe,ownerState:r,children:(0,I.jsx)(pe,{...me,ownerState:r,children:i})})})}})});Zu&&(Zu.muiSupportAuto=!0);var Qu=Zu;function $u(e){return zo(`MuiPaper`,e)}Bo(`MuiPaper`,`root.rounded.outlined.elevation.elevation0.elevation1.elevation2.elevation3.elevation4.elevation5.elevation6.elevation7.elevation8.elevation9.elevation10.elevation11.elevation12.elevation13.elevation14.elevation15.elevation16.elevation17.elevation18.elevation19.elevation20.elevation21.elevation22.elevation23.elevation24`.split(`.`));var ed=e=>{let{square:t,elevation:n,variant:r,classes:i}=e;return dc({root:[`root`,r,!t&&`rounded`,r===`elevation`&&`elevation${n}`]},$u,i)},td=B(`div`,{name:`MuiPaper`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],!n.square&&t.rounded,n.variant===`elevation`&&t[`elevation${n.elevation}`]]}})(Ml(({theme:e})=>({backgroundColor:(e.vars||e).palette.background.paper,color:(e.vars||e).palette.text.primary,transition:e.transitions.create(`box-shadow`),variants:[{props:({ownerState:e})=>!e.square,style:{borderRadius:e.shape.borderRadius}},{props:{variant:`outlined`},style:{border:`1px solid ${(e.vars||e).palette.divider}`}},{props:{variant:`elevation`},style:{boxShadow:`var(--Paper-shadow)`,backgroundImage:`var(--Paper-overlay)`}}]}))),nd=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiPaper`}),r=gl(),{className:i,component:a=`div`,elevation:o=1,square:s=!1,variant:c=`elevation`,...l}=n,u={...n,component:a,elevation:o,square:s,variant:c};return(0,I.jsx)(td,{as:a,ownerState:u,className:R(ed(u).root,i),ref:t,...l,style:{...c===`elevation`&&{"--Paper-shadow":(r.vars||r).shadows[o],...r.vars&&{"--Paper-overlay":r.vars.overlays?.[o]},...!r.vars&&r.palette.mode===`dark`&&{"--Paper-overlay":`linear-gradient(${fs(`#fff`,Zc(o))}, ${fs(`#fff`,Zc(o))})`}},...l.style}})});function rd(e){try{return e.matches(`:focus-visible`)}catch{}return!1}var id=class e{static create(){return new e}static use(){let t=Eu(e.create).current,[n,r]=x.useState(!1);return t.shouldMount=n,t.setShouldMount=r,x.useEffect(t.mountEffect,[n]),t}constructor(){this.ref={current:null},this.mounted=null,this.didMount=!1,this.shouldMount=!1,this.setShouldMount=null}mount(){return this.mounted||(this.mounted=od(),this.shouldMount=!0,this.setShouldMount(this.shouldMount)),this.mounted}mountEffect=()=>{this.shouldMount&&!this.didMount&&this.ref.current!==null&&(this.didMount=!0,this.mounted.resolve())};start(...e){this.mount().then(()=>this.ref.current?.start(...e))}stop(...e){this.mount().then(()=>this.ref.current?.stop(...e))}pulsate(...e){this.mount().then(()=>this.ref.current?.pulsate(...e))}};function ad(){return id.use()}function od(){let e,t,n=new Promise((n,r)=>{e=n,t=r});return n.resolve=e,n.reject=t,n}function sd(e){let{className:t,classes:n,pulsate:r=!1,rippleX:i,rippleY:a,rippleSize:o,in:s,onExited:c,timeout:l}=e,[u,d]=x.useState(!1),f=R(t,n.ripple,n.rippleVisible,r&&n.ripplePulsate),p={width:o,height:o,top:-(o/2)+a,left:-(o/2)+i},m=R(n.child,u&&n.childLeaving,r&&n.childPulsate);return!s&&!u&&d(!0),x.useEffect(()=>{if(!s&&c!=null){let e=setTimeout(c,l);return()=>{clearTimeout(e)}}},[c,s,l]),(0,I.jsx)(`span`,{className:f,style:p,children:(0,I.jsx)(`span`,{className:m})})}var cd=sd,ld=Bo(`MuiTouchRipple`,[`root`,`ripple`,`rippleVisible`,`ripplePulsate`,`child`,`childLeaving`,`childPulsate`]),ud=550,dd=Ai`
  0% {
    transform: scale(0);
    opacity: 0.1;
  }

  100% {
    transform: scale(1);
    opacity: 0.3;
  }
`,fd=Ai`
  0% {
    opacity: 1;
  }

  100% {
    opacity: 0;
  }
`,pd=Ai`
  0% {
    transform: scale(1);
  }

  50% {
    transform: scale(0.92);
  }

  100% {
    transform: scale(1);
  }
`;const md=B(`span`,{name:`MuiTouchRipple`,slot:`Root`})({overflow:`hidden`,pointerEvents:`none`,position:`absolute`,zIndex:0,top:0,right:0,bottom:0,left:0,borderRadius:`inherit`}),hd=B(cd,{name:`MuiTouchRipple`,slot:`Ripple`})`
  opacity: 0;
  position: absolute;

  &.${ld.rippleVisible} {
    opacity: 0.3;
    transform: scale(1);
    animation-name: ${dd};
    animation-duration: ${ud}ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
  }

  &.${ld.ripplePulsate} {
    animation-duration: ${({theme:e})=>e.transitions.duration.shorter}ms;
  }

  & .${ld.child} {
    opacity: 1;
    display: block;
    width: 100%;
    height: 100%;
    border-radius: 50%;
    background-color: currentColor;
  }

  & .${ld.childLeaving} {
    opacity: 0;
    animation-name: ${fd};
    animation-duration: ${ud}ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
  }

  & .${ld.childPulsate} {
    position: absolute;
    /* @noflip */
    left: 0px;
    top: 0;
    animation-name: ${pd};
    animation-duration: 2500ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
    animation-iteration-count: infinite;
    animation-delay: 200ms;
  }
`;var gd=x.forwardRef(function(e,t){let{center:n=!1,classes:r={},className:i,...a}=Nl({props:e,name:`MuiTouchRipple`}),[o,s]=x.useState([]),c=x.useRef(0),l=x.useRef(null);x.useEffect(()=>{l.current&&=(l.current(),null)},[o]);let u=x.useRef(!1),d=Au(),f=x.useRef(null),p=x.useRef(null),m=x.useCallback(e=>{let{pulsate:t,rippleX:n,rippleY:i,rippleSize:a,cb:o}=e;s(e=>[...e,(0,I.jsx)(hd,{classes:{ripple:R(r.ripple,ld.ripple),rippleVisible:R(r.rippleVisible,ld.rippleVisible),ripplePulsate:R(r.ripplePulsate,ld.ripplePulsate),child:R(r.child,ld.child),childLeaving:R(r.childLeaving,ld.childLeaving),childPulsate:R(r.childPulsate,ld.childPulsate)},timeout:ud,pulsate:t,rippleX:n,rippleY:i,rippleSize:a},c.current)]),c.current+=1,l.current=o},[r]),h=x.useCallback((e={},t={},r=()=>{})=>{let{pulsate:i=!1,center:a=n||t.pulsate,fakeElement:o=!1}=t;if(e?.type===`mousedown`&&u.current){u.current=!1;return}e?.type===`touchstart`&&(u.current=!0);let s=o?null:p.current,c=s?s.getBoundingClientRect():{width:0,height:0,left:0,top:0},l,h,g;if(a||e===void 0||e.clientX===0&&e.clientY===0||!e.clientX&&!e.touches)l=Math.round(c.width/2),h=Math.round(c.height/2);else{let{clientX:t,clientY:n}=e.touches&&e.touches.length>0?e.touches[0]:e;l=Math.round(t-c.left),h=Math.round(n-c.top)}if(a)g=Math.sqrt((2*c.width**2+c.height**2)/3),g%2==0&&(g+=1);else{let e=Math.max(Math.abs((s?s.clientWidth:0)-l),l)*2+2,t=Math.max(Math.abs((s?s.clientHeight:0)-h),h)*2+2;g=Math.sqrt(e**2+t**2)}e?.touches?f.current===null&&(f.current=()=>{m({pulsate:i,rippleX:l,rippleY:h,rippleSize:g,cb:r})},d.start(80,()=>{f.current&&=(f.current(),null)})):m({pulsate:i,rippleX:l,rippleY:h,rippleSize:g,cb:r})},[n,m,d]),g=x.useCallback(()=>{h({},{pulsate:!0})},[h]),_=x.useCallback((e,t)=>{if(d.clear(),e?.type===`touchend`&&f.current){f.current(),f.current=null,d.start(0,()=>{_(e,t)});return}f.current=null,s(e=>e.length>0?e.slice(1):e),l.current=t},[d]);return x.useImperativeHandle(t,()=>({pulsate:g,start:h,stop:_}),[g,h,_]),(0,I.jsx)(md,{className:R(ld.root,r.root,i),ref:p,...a,children:(0,I.jsx)(wu,{component:null,exit:!0,children:o})})});function _d(e){return zo(`MuiButtonBase`,e)}var vd=Bo(`MuiButtonBase`,[`root`,`disabled`,`focusVisible`]),yd=e=>{let{disabled:t,focusVisible:n,focusVisibleClassName:r,classes:i}=e,a=dc({root:[`root`,t&&`disabled`,n&&`focusVisible`]},_d,i);return n&&r&&(a.root+=` ${r}`),a};const bd=B(`button`,{name:`MuiButtonBase`,slot:`Root`})({display:`inline-flex`,alignItems:`center`,justifyContent:`center`,position:`relative`,boxSizing:`border-box`,WebkitTapHighlightColor:`transparent`,backgroundColor:`transparent`,outline:0,border:0,margin:0,borderRadius:0,padding:0,cursor:`pointer`,userSelect:`none`,verticalAlign:`middle`,MozAppearance:`none`,WebkitAppearance:`none`,textDecoration:`none`,color:`inherit`,"&::-moz-focus-inner":{borderStyle:`none`},[`&.${vd.disabled}`]:{pointerEvents:`none`,cursor:`default`},"@media print":{colorAdjust:`exact`}});var xd=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiButtonBase`}),{action:r,centerRipple:i=!1,children:a,className:o,component:s=`button`,disabled:c=!1,disableRipple:l=!1,disableTouchRipple:u=!1,focusRipple:d=!1,focusVisibleClassName:f,LinkComponent:p=`a`,onBlur:m,onClick:h,onContextMenu:g,onDragLeave:_,onFocus:v,onFocusVisible:y,onKeyDown:b,onKeyUp:S,onMouseDown:C,onMouseLeave:w,onMouseUp:T,onTouchEnd:E,onTouchMove:D,onTouchStart:O,tabIndex:k=0,TouchRippleProps:ee,touchRippleRef:A,type:j,...M}=n,te=x.useRef(null),N=ad(),P=$l(N.ref,A),[F,ne]=x.useState(!1);c&&F&&ne(!1),x.useImperativeHandle(r,()=>({focusVisible:()=>{ne(!0),te.current.focus()}}),[]);let re=N.shouldMount&&!l&&!c;x.useEffect(()=>{F&&d&&!l&&N.pulsate()},[l,d,F,N]);let ie=Sd(N,`start`,C,u),ae=Sd(N,`stop`,g,u),oe=Sd(N,`stop`,_,u),se=Sd(N,`stop`,T,u),ce=Sd(N,`stop`,e=>{F&&e.preventDefault(),w&&w(e)},u),le=Sd(N,`start`,O,u),ue=Sd(N,`stop`,E,u),de=Sd(N,`stop`,D,u),fe=Sd(N,`stop`,e=>{rd(e.target)||ne(!1),m&&m(e)},!1),pe=Zl(e=>{te.current||=e.currentTarget,rd(e.target)&&(ne(!0),y&&y(e)),v&&v(e)}),me=()=>{let e=te.current;return s&&s!==`button`&&!(e.tagName===`A`&&e.href)},he=Zl(e=>{d&&!e.repeat&&F&&e.key===` `&&N.stop(e,()=>{N.start(e)}),e.target===e.currentTarget&&me()&&e.key===` `&&e.preventDefault(),b&&b(e),e.target===e.currentTarget&&me()&&e.key===`Enter`&&!c&&(e.preventDefault(),h&&h(e))}),ge=Zl(e=>{d&&e.key===` `&&F&&!e.defaultPrevented&&N.stop(e,()=>{N.pulsate(e)}),S&&S(e),h&&e.target===e.currentTarget&&me()&&e.key===` `&&!e.defaultPrevented&&h(e)}),_e=s;_e===`button`&&(M.href||M.to)&&(_e=p);let ve={};_e===`button`?(ve.type=j===void 0?`button`:j,ve.disabled=c):(!M.href&&!M.to&&(ve.role=`button`),c&&(ve[`aria-disabled`]=c));let ye=$l(t,te),be={...n,centerRipple:i,component:s,disabled:c,disableRipple:l,disableTouchRipple:u,focusRipple:d,tabIndex:k,focusVisible:F},xe=yd(be);return(0,I.jsxs)(bd,{as:_e,className:R(xe.root,o),ownerState:be,onBlur:fe,onClick:h,onContextMenu:ae,onFocus:pe,onKeyDown:he,onKeyUp:ge,onMouseDown:ie,onMouseLeave:ce,onMouseUp:se,onDragLeave:oe,onTouchEnd:ue,onTouchMove:de,onTouchStart:le,ref:ye,tabIndex:c?-1:k,type:j,...ve,...M,children:[a,re?(0,I.jsx)(gd,{ref:P,center:i,...ee}):null]})});function Sd(e,t,n,r=!1){return Zl(i=>(n&&n(i),r||e[t](i),!0))}var Cd=xd;function wd(e){return typeof e.main==`string`}function Td(e,t=[]){if(!wd(e))return!1;for(let n of t)if(!e.hasOwnProperty(n)||typeof e[n]!=`string`)return!1;return!0}function Ed(e=[]){return([,t])=>t&&Td(t,e)}function Dd(e){return zo(`MuiAlert`,e)}var Od=Bo(`MuiAlert`,[`root`,`action`,`icon`,`message`,`filled`,`colorSuccess`,`colorInfo`,`colorWarning`,`colorError`,`filledSuccess`,`filledInfo`,`filledWarning`,`filledError`,`outlined`,`outlinedSuccess`,`outlinedInfo`,`outlinedWarning`,`outlinedError`,`standard`,`standardSuccess`,`standardInfo`,`standardWarning`,`standardError`]);function kd(e){return zo(`MuiCircularProgress`,e)}Bo(`MuiCircularProgress`,[`root`,`determinate`,`indeterminate`,`colorPrimary`,`colorSecondary`,`svg`,`track`,`circle`,`circleDeterminate`,`circleIndeterminate`,`circleDisableShrink`]);var H=44,Ad=Ai`
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
`,jd=Ai`
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
`,Md=typeof Ad==`string`?null:ki`
        animation: ${Ad} 1.4s linear infinite;
      `,Nd=typeof jd==`string`?null:ki`
        animation: ${jd} 1.4s ease-in-out infinite;
      `,Pd=e=>{let{classes:t,variant:n,color:r,disableShrink:i}=e;return dc({root:[`root`,n,`color${V(r)}`],svg:[`svg`],track:[`track`],circle:[`circle`,`circle${V(n)}`,i&&`circleDisableShrink`]},kd,t)},Fd=B(`span`,{name:`MuiCircularProgress`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`color${V(n.color)}`]]}})(Ml(({theme:e})=>({display:`inline-block`,variants:[{props:{variant:`determinate`},style:{transition:e.transitions.create(`transform`)}},{props:{variant:`indeterminate`},style:Md||{animation:`${Ad} 1.4s linear infinite`}},...Object.entries(e.palette).filter(Ed()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}}))]}))),Id=B(`svg`,{name:`MuiCircularProgress`,slot:`Svg`})({display:`block`}),Ld=B(`circle`,{name:`MuiCircularProgress`,slot:`Circle`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.circle,t[`circle${V(n.variant)}`],n.disableShrink&&t.circleDisableShrink]}})(Ml(({theme:e})=>({stroke:`currentColor`,variants:[{props:{variant:`determinate`},style:{transition:e.transitions.create(`stroke-dashoffset`)}},{props:{variant:`indeterminate`},style:{strokeDasharray:`80px, 200px`,strokeDashoffset:0}},{props:({ownerState:e})=>e.variant===`indeterminate`&&!e.disableShrink,style:Nd||{animation:`${jd} 1.4s ease-in-out infinite`}}]}))),Rd=B(`circle`,{name:`MuiCircularProgress`,slot:`Track`})(Ml(({theme:e})=>({stroke:`currentColor`,opacity:(e.vars||e).palette.action.activatedOpacity}))),zd=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiCircularProgress`}),{className:r,color:i=`primary`,disableShrink:a=!1,enableTrackSlot:o=!1,size:s=40,style:c,thickness:l=3.6,value:u=0,variant:d=`indeterminate`,...f}=n,p={...n,color:i,disableShrink:a,size:s,thickness:l,value:u,variant:d,enableTrackSlot:o},m=Pd(p),h={},g={},_={};if(d===`determinate`){let e=2*Math.PI*((H-l)/2);h.strokeDasharray=e.toFixed(3),_[`aria-valuenow`]=Math.round(u),h.strokeDashoffset=`${((100-u)/100*e).toFixed(3)}px`,g.transform=`rotate(-90deg)`}return(0,I.jsx)(Fd,{className:R(m.root,r),style:{width:s,height:s,...g,...c},ownerState:p,ref:t,role:`progressbar`,..._,...f,children:(0,I.jsxs)(Id,{className:m.svg,ownerState:p,viewBox:`${H/2} ${H/2} ${H} ${H}`,children:[o?(0,I.jsx)(Rd,{className:m.track,ownerState:p,cx:H,cy:H,r:(H-l)/2,fill:`none`,strokeWidth:l,"aria-hidden":`true`}):null,(0,I.jsx)(Ld,{className:m.circle,style:h,ownerState:p,cx:H,cy:H,r:(H-l)/2,fill:`none`,strokeWidth:l})]})})});function Bd(e){return zo(`MuiIconButton`,e)}var Vd=Bo(`MuiIconButton`,[`root`,`disabled`,`colorInherit`,`colorPrimary`,`colorSecondary`,`colorError`,`colorInfo`,`colorSuccess`,`colorWarning`,`edgeStart`,`edgeEnd`,`sizeSmall`,`sizeMedium`,`sizeLarge`,`loading`,`loadingIndicator`,`loadingWrapper`]),Hd=e=>{let{classes:t,disabled:n,color:r,edge:i,size:a,loading:o}=e;return dc({root:[`root`,o&&`loading`,n&&`disabled`,r!==`default`&&`color${V(r)}`,i&&`edge${V(i)}`,`size${V(a)}`],loadingIndicator:[`loadingIndicator`],loadingWrapper:[`loadingWrapper`]},Bd,t)},Ud=B(Cd,{name:`MuiIconButton`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.loading&&t.loading,n.color!==`default`&&t[`color${V(n.color)}`],n.edge&&t[`edge${V(n.edge)}`],t[`size${V(n.size)}`]]}})(Ml(({theme:e})=>({textAlign:`center`,flex:`0 0 auto`,fontSize:e.typography.pxToRem(24),padding:8,borderRadius:`50%`,color:(e.vars||e).palette.action.active,transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),variants:[{props:e=>!e.disableRipple,style:{"--IconButton-hoverBg":e.alpha((e.vars||e).palette.action.active,(e.vars||e).palette.action.hoverOpacity),"&:hover":{backgroundColor:`var(--IconButton-hoverBg)`,"@media (hover: none)":{backgroundColor:`transparent`}}}},{props:{edge:`start`},style:{marginLeft:-12}},{props:{edge:`start`,size:`small`},style:{marginLeft:-3}},{props:{edge:`end`},style:{marginRight:-12}},{props:{edge:`end`,size:`small`},style:{marginRight:-3}}]})),Ml(({theme:e})=>({variants:[{props:{color:`inherit`},style:{color:`inherit`}},...Object.entries(e.palette).filter(Ed()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}})),...Object.entries(e.palette).filter(Ed()).map(([t])=>({props:{color:t},style:{"--IconButton-hoverBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)}})),{props:{size:`small`},style:{padding:5,fontSize:e.typography.pxToRem(18)}},{props:{size:`large`},style:{padding:12,fontSize:e.typography.pxToRem(28)}}],[`&.${Vd.disabled}`]:{backgroundColor:`transparent`,color:(e.vars||e).palette.action.disabled},[`&.${Vd.loading}`]:{color:`transparent`}}))),Wd=B(`span`,{name:`MuiIconButton`,slot:`LoadingIndicator`})(({theme:e})=>({display:`none`,position:`absolute`,visibility:`visible`,top:`50%`,left:`50%`,transform:`translate(-50%, -50%)`,color:(e.vars||e).palette.action.disabled,variants:[{props:{loading:!0},style:{display:`flex`}}]})),Gd=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiIconButton`}),{edge:r=!1,children:i,className:a,color:o=`default`,disabled:s=!1,disableFocusRipple:c=!1,size:l=`medium`,id:u,loading:d=null,loadingIndicator:f,...p}=n,m=Kl(u),h=f??(0,I.jsx)(zd,{"aria-labelledby":m,color:`inherit`,size:16}),g={...n,edge:r,color:o,disabled:s,disableFocusRipple:c,loading:d,loadingIndicator:h,size:l},_=Hd(g);return(0,I.jsxs)(Ud,{id:d?m:u,className:R(_.root,a),centerRipple:!0,focusRipple:!c,disabled:s||d,ref:t,...p,ownerState:g,children:[typeof d==`boolean`&&(0,I.jsx)(`span`,{className:_.loadingWrapper,style:{display:`contents`},children:(0,I.jsx)(Wd,{className:_.loadingIndicator,ownerState:g,children:d&&h})}),i]})}),Kd=zl((0,I.jsx)(`path`,{d:`M20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4C12.76,4 13.5,4.11 14.2, 4.31L15.77,2.74C14.61,2.26 13.34,2 12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0, 0 22,12M7.91,10.08L6.5,11.5L11,16L21,6L19.59,4.58L11,13.17L7.91,10.08Z`}),`SuccessOutlined`),qd=zl((0,I.jsx)(`path`,{d:`M12 5.99L19.53 19H4.47L12 5.99M12 2L1 21h22L12 2zm1 14h-2v2h2v-2zm0-6h-2v4h2v-4z`}),`ReportProblemOutlined`),Jd=zl((0,I.jsx)(`path`,{d:`M11 15h2v2h-2zm0-8h2v6h-2zm.99-5C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z`}),`ErrorOutline`),Yd=zl((0,I.jsx)(`path`,{d:`M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20, 12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10, 10 0 0,0 12,2M11,17H13V11H11V17Z`}),`InfoOutlined`),Xd=zl((0,I.jsx)(`path`,{d:`M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z`}),`Close`),Zd=e=>{let{variant:t,color:n,severity:r,classes:i}=e;return dc({root:[`root`,`color${V(n||r)}`,`${t}${V(n||r)}`,`${t}`],icon:[`icon`],message:[`message`],action:[`action`]},Dd,i)},Qd=B(nd,{name:`MuiAlert`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`${n.variant}${V(n.color||n.severity)}`]]}})(Ml(({theme:e})=>{let t=e.palette.mode===`light`?e.darken:e.lighten,n=e.palette.mode===`light`?e.lighten:e.darken;return{...e.typography.body2,backgroundColor:`transparent`,display:`flex`,padding:`6px 16px`,variants:[...Object.entries(e.palette).filter(Ed([`light`])).map(([r])=>({props:{colorSeverity:r,variant:`standard`},style:{color:e.vars?e.vars.palette.Alert[`${r}Color`]:t(e.palette[r].light,.6),backgroundColor:e.vars?e.vars.palette.Alert[`${r}StandardBg`]:n(e.palette[r].light,.9),[`& .${Od.icon}`]:e.vars?{color:e.vars.palette.Alert[`${r}IconColor`]}:{color:e.palette[r].main}}})),...Object.entries(e.palette).filter(Ed([`light`])).map(([n])=>({props:{colorSeverity:n,variant:`outlined`},style:{color:e.vars?e.vars.palette.Alert[`${n}Color`]:t(e.palette[n].light,.6),border:`1px solid ${(e.vars||e).palette[n].light}`,[`& .${Od.icon}`]:e.vars?{color:e.vars.palette.Alert[`${n}IconColor`]}:{color:e.palette[n].main}}})),...Object.entries(e.palette).filter(Ed([`dark`])).map(([t])=>({props:{colorSeverity:t,variant:`filled`},style:{fontWeight:e.typography.fontWeightMedium,...e.vars?{color:e.vars.palette.Alert[`${t}FilledColor`],backgroundColor:e.vars.palette.Alert[`${t}FilledBg`]}:{backgroundColor:e.palette.mode===`dark`?e.palette[t].dark:e.palette[t].main,color:e.palette.getContrastText(e.palette[t].main)}}}))]}})),$d=B(`div`,{name:`MuiAlert`,slot:`Icon`})({marginRight:12,padding:`7px 0`,display:`flex`,fontSize:22,opacity:.9}),ef=B(`div`,{name:`MuiAlert`,slot:`Message`})({padding:`8px 0`,minWidth:0,overflow:`auto`}),tf=B(`div`,{name:`MuiAlert`,slot:`Action`})({display:`flex`,alignItems:`flex-start`,padding:`4px 0 0 16px`,marginLeft:`auto`,marginRight:-8}),nf={success:(0,I.jsx)(Kd,{fontSize:`inherit`}),warning:(0,I.jsx)(qd,{fontSize:`inherit`}),error:(0,I.jsx)(Jd,{fontSize:`inherit`}),info:(0,I.jsx)(Yd,{fontSize:`inherit`})},rf=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiAlert`}),{action:r,children:i,className:a,closeText:o=`Close`,color:s,components:c={},componentsProps:l={},icon:u,iconMapping:d=nf,onClose:f,role:p=`alert`,severity:m=`success`,slotProps:h={},slots:g={},variant:_=`standard`,...v}=n,y={...n,color:s,severity:m,variant:_,colorSeverity:s||m},b=Zd(y),x={slots:{closeButton:c.CloseButton,closeIcon:c.CloseIcon,...g},slotProps:{...l,...h}},[S,C]=Gu(`root`,{ref:t,shouldForwardComponentProp:!0,className:R(b.root,a),elementType:Qd,externalForwardedProps:{...x,...v},ownerState:y,additionalProps:{role:p,elevation:0}}),[w,T]=Gu(`icon`,{className:b.icon,elementType:$d,externalForwardedProps:x,ownerState:y}),[E,D]=Gu(`message`,{className:b.message,elementType:ef,externalForwardedProps:x,ownerState:y}),[O,k]=Gu(`action`,{className:b.action,elementType:tf,externalForwardedProps:x,ownerState:y}),[ee,A]=Gu(`closeButton`,{elementType:Gd,externalForwardedProps:x,ownerState:y}),[j,M]=Gu(`closeIcon`,{elementType:Xd,externalForwardedProps:x,ownerState:y});return(0,I.jsxs)(S,{...C,children:[u===!1?null:(0,I.jsx)(w,{...T,children:u||d[m]||nf[m]}),(0,I.jsx)(E,{...D,children:i}),r==null?null:(0,I.jsx)(O,{...k,children:r}),r==null&&f?(0,I.jsx)(O,{...k,children:(0,I.jsx)(ee,{size:`small`,"aria-label":o,title:o,color:`inherit`,onClick:f,...A,children:(0,I.jsx)(j,{fontSize:`small`,...M})})}):null]})});function af(e){return zo(`MuiTypography`,e)}var of=Bo(`MuiTypography`,[`root`,`h1`,`h2`,`h3`,`h4`,`h5`,`h6`,`subtitle1`,`subtitle2`,`body1`,`body2`,`inherit`,`button`,`caption`,`overline`,`alignLeft`,`alignRight`,`alignCenter`,`alignJustify`,`noWrap`,`gutterBottom`,`paragraph`]),sf={primary:!0,secondary:!0,error:!0,info:!0,success:!0,warning:!0,textPrimary:!0,textSecondary:!0,textDisabled:!0},cf=jl(),lf=e=>{let{align:t,gutterBottom:n,noWrap:r,paragraph:i,variant:a,classes:o}=e;return dc({root:[`root`,a,e.align!==`inherit`&&`align${V(t)}`,n&&`gutterBottom`,r&&`noWrap`,i&&`paragraph`]},af,o)};const uf=B(`span`,{name:`MuiTypography`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.variant&&t[n.variant],n.align!==`inherit`&&t[`align${V(n.align)}`],n.noWrap&&t.noWrap,n.gutterBottom&&t.gutterBottom,n.paragraph&&t.paragraph]}})(Ml(({theme:e})=>({margin:0,variants:[{props:{variant:`inherit`},style:{font:`inherit`,lineHeight:`inherit`,letterSpacing:`inherit`}},...Object.entries(e.typography).filter(([e,t])=>e!==`inherit`&&t&&typeof t==`object`).map(([e,t])=>({props:{variant:e},style:t})),...Object.entries(e.palette).filter(Ed()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}})),...Object.entries(e.palette?.text||{}).filter(([,e])=>typeof e==`string`).map(([t])=>({props:{color:`text${V(t)}`},style:{color:(e.vars||e).palette.text[t]}})),{props:({ownerState:e})=>e.align!==`inherit`,style:{textAlign:`var(--Typography-textAlign)`}},{props:({ownerState:e})=>e.noWrap,style:{overflow:`hidden`,textOverflow:`ellipsis`,whiteSpace:`nowrap`}},{props:({ownerState:e})=>e.gutterBottom,style:{marginBottom:`0.35em`}},{props:({ownerState:e})=>e.paragraph,style:{marginBottom:16}}]})));var df={h1:`h1`,h2:`h2`,h3:`h3`,h4:`h4`,h5:`h5`,h6:`h6`,subtitle1:`h6`,subtitle2:`h6`,body1:`p`,body2:`p`,inherit:`p`},U=x.forwardRef(function(e,t){let{color:n,...r}=Nl({props:e,name:`MuiTypography`}),i=!sf[n],a=cf({...r,...i&&{color:n}}),{align:o=`inherit`,className:s,component:c,gutterBottom:l=!1,noWrap:u=!1,paragraph:d=!1,variant:f=`body1`,variantMapping:p=df,...m}=a,h={...a,align:o,color:n,className:s,component:c,gutterBottom:l,noWrap:u,paragraph:d,variant:f,variantMapping:p};return(0,I.jsx)(uf,{as:c||(d?`p`:p[f]||df[f])||`span`,ref:t,className:R(lf(h).root,s),...m,ownerState:h,style:{...o!==`inherit`&&{"--Typography-textAlign":o},...m.style}})});function ff(e){return zo(`MuiAppBar`,e)}Bo(`MuiAppBar`,[`root`,`positionFixed`,`positionAbsolute`,`positionSticky`,`positionStatic`,`positionRelative`,`colorDefault`,`colorPrimary`,`colorSecondary`,`colorInherit`,`colorTransparent`,`colorError`,`colorInfo`,`colorSuccess`,`colorWarning`]);var pf=e=>{let{color:t,position:n,classes:r}=e;return dc({root:[`root`,`color${V(t)}`,`position${V(n)}`]},ff,r)},mf=(e,t)=>e?`${e?.replace(`)`,``)}, ${t})`:t,hf=B(nd,{name:`MuiAppBar`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[`position${V(n.position)}`],t[`color${V(n.color)}`]]}})(Ml(({theme:e})=>({display:`flex`,flexDirection:`column`,width:`100%`,boxSizing:`border-box`,flexShrink:0,variants:[{props:{position:`fixed`},style:{position:`fixed`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0,"@media print":{position:`absolute`}}},{props:{position:`absolute`},style:{position:`absolute`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0}},{props:{position:`sticky`},style:{position:`sticky`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0}},{props:{position:`static`},style:{position:`static`}},{props:{position:`relative`},style:{position:`relative`}},{props:{color:`inherit`},style:{"--AppBar-color":`inherit`}},{props:{color:`default`},style:{"--AppBar-background":e.vars?e.vars.palette.AppBar.defaultBg:e.palette.grey[100],"--AppBar-color":e.vars?e.vars.palette.text.primary:e.palette.getContrastText(e.palette.grey[100]),...e.applyStyles(`dark`,{"--AppBar-background":e.vars?e.vars.palette.AppBar.defaultBg:e.palette.grey[900],"--AppBar-color":e.vars?e.vars.palette.text.primary:e.palette.getContrastText(e.palette.grey[900])})}},...Object.entries(e.palette).filter(Ed([`contrastText`])).map(([t])=>({props:{color:t},style:{"--AppBar-background":(e.vars??e).palette[t].main,"--AppBar-color":(e.vars??e).palette[t].contrastText}})),{props:e=>e.enableColorOnDark===!0&&![`inherit`,`transparent`].includes(e.color),style:{backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`}},{props:e=>e.enableColorOnDark===!1&&![`inherit`,`transparent`].includes(e.color),style:{backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`,...e.applyStyles(`dark`,{backgroundColor:e.vars?mf(e.vars.palette.AppBar.darkBg,`var(--AppBar-background)`):null,color:e.vars?mf(e.vars.palette.AppBar.darkColor,`var(--AppBar-color)`):null})}},{props:{color:`transparent`},style:{"--AppBar-background":`transparent`,"--AppBar-color":`inherit`,backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`,...e.applyStyles(`dark`,{backgroundImage:`none`})}}]}))),gf=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiAppBar`}),{className:r,color:i=`primary`,enableColorOnDark:a=!1,position:o=`fixed`,...s}=n,c={...n,color:i,position:o,enableColorOnDark:a};return(0,I.jsx)(hf,{square:!0,component:`header`,ownerState:c,elevation:4,className:R(pf(c).root,r,o===`fixed`&&`mui-fixed`),ref:t,...s})}),_f=`bottom`,vf=`right`,yf=`left`,bf=`auto`,xf=[`top`,_f,vf,yf],Sf=`start`,Cf=`clippingParents`,wf=`viewport`,Tf=`popper`,Ef=`reference`,Df=xf.reduce(function(e,t){return e.concat([t+`-`+Sf,t+`-end`])},[]),Of=[].concat(xf,[bf]).reduce(function(e,t){return e.concat([t,t+`-`+Sf,t+`-end`])},[]),kf=[`beforeRead`,`read`,`afterRead`,`beforeMain`,`main`,`afterMain`,`beforeWrite`,`write`,`afterWrite`];function Af(e){return e?(e.nodeName||``).toLowerCase():null}function jf(e){if(e==null)return window;if(e.toString()!==`[object Window]`){var t=e.ownerDocument;return t&&t.defaultView||window}return e}function Mf(e){return e instanceof jf(e).Element||e instanceof Element}function Nf(e){return e instanceof jf(e).HTMLElement||e instanceof HTMLElement}function Pf(e){return typeof ShadowRoot>`u`?!1:e instanceof jf(e).ShadowRoot||e instanceof ShadowRoot}function Ff(e){var t=e.state;Object.keys(t.elements).forEach(function(e){var n=t.styles[e]||{},r=t.attributes[e]||{},i=t.elements[e];!Nf(i)||!Af(i)||(Object.assign(i.style,n),Object.keys(r).forEach(function(e){var t=r[e];t===!1?i.removeAttribute(e):i.setAttribute(e,t===!0?``:t)}))})}function If(e){var t=e.state,n={popper:{position:t.options.strategy,left:`0`,top:`0`,margin:`0`},arrow:{position:`absolute`},reference:{}};return Object.assign(t.elements.popper.style,n.popper),t.styles=n,t.elements.arrow&&Object.assign(t.elements.arrow.style,n.arrow),function(){Object.keys(t.elements).forEach(function(e){var r=t.elements[e],i=t.attributes[e]||{},a=Object.keys(t.styles.hasOwnProperty(e)?t.styles[e]:n[e]).reduce(function(e,t){return e[t]=``,e},{});!Nf(r)||!Af(r)||(Object.assign(r.style,a),Object.keys(i).forEach(function(e){r.removeAttribute(e)}))})}}var Lf={name:`applyStyles`,enabled:!0,phase:`write`,fn:Ff,effect:If,requires:[`computeStyles`]};function Rf(e){return e.split(`-`)[0]}var zf=Math.max,Bf=Math.min,Vf=Math.round;function Hf(){var e=navigator.userAgentData;return e!=null&&e.brands&&Array.isArray(e.brands)?e.brands.map(function(e){return e.brand+`/`+e.version}).join(` `):navigator.userAgent}function Uf(){return!/^((?!chrome|android).)*safari/i.test(Hf())}function Wf(e,t,n){t===void 0&&(t=!1),n===void 0&&(n=!1);var r=e.getBoundingClientRect(),i=1,a=1;t&&Nf(e)&&(i=e.offsetWidth>0&&Vf(r.width)/e.offsetWidth||1,a=e.offsetHeight>0&&Vf(r.height)/e.offsetHeight||1);var o=(Mf(e)?jf(e):window).visualViewport,s=!Uf()&&n,c=(r.left+(s&&o?o.offsetLeft:0))/i,l=(r.top+(s&&o?o.offsetTop:0))/a,u=r.width/i,d=r.height/a;return{width:u,height:d,top:l,right:c+u,bottom:l+d,left:c,x:c,y:l}}function Gf(e){var t=Wf(e),n=e.offsetWidth,r=e.offsetHeight;return Math.abs(t.width-n)<=1&&(n=t.width),Math.abs(t.height-r)<=1&&(r=t.height),{x:e.offsetLeft,y:e.offsetTop,width:n,height:r}}function Kf(e,t){var n=t.getRootNode&&t.getRootNode();if(e.contains(t))return!0;if(n&&Pf(n)){var r=t;do{if(r&&e.isSameNode(r))return!0;r=r.parentNode||r.host}while(r)}return!1}function qf(e){return jf(e).getComputedStyle(e)}function Jf(e){return[`table`,`td`,`th`].indexOf(Af(e))>=0}function Yf(e){return((Mf(e)?e.ownerDocument:e.document)||window.document).documentElement}function Xf(e){return Af(e)===`html`?e:e.assignedSlot||e.parentNode||(Pf(e)?e.host:null)||Yf(e)}function Zf(e){return!Nf(e)||qf(e).position===`fixed`?null:e.offsetParent}function Qf(e){var t=/firefox/i.test(Hf());if(/Trident/i.test(Hf())&&Nf(e)&&qf(e).position===`fixed`)return null;var n=Xf(e);for(Pf(n)&&(n=n.host);Nf(n)&&[`html`,`body`].indexOf(Af(n))<0;){var r=qf(n);if(r.transform!==`none`||r.perspective!==`none`||r.contain===`paint`||[`transform`,`perspective`].indexOf(r.willChange)!==-1||t&&r.willChange===`filter`||t&&r.filter&&r.filter!==`none`)return n;n=n.parentNode}return null}function $f(e){for(var t=jf(e),n=Zf(e);n&&Jf(n)&&qf(n).position===`static`;)n=Zf(n);return n&&(Af(n)===`html`||Af(n)===`body`&&qf(n).position===`static`)?t:n||Qf(e)||t}function ep(e){return[`top`,`bottom`].indexOf(e)>=0?`x`:`y`}function tp(e,t,n){return zf(e,Bf(t,n))}function np(e,t,n){var r=tp(e,t,n);return r>n?n:r}function rp(){return{top:0,right:0,bottom:0,left:0}}function ip(e){return Object.assign({},rp(),e)}function ap(e,t){return t.reduce(function(t,n){return t[n]=e,t},{})}var op=function(e,t){return e=typeof e==`function`?e(Object.assign({},t.rects,{placement:t.placement})):e,ip(typeof e==`number`?ap(e,xf):e)};function sp(e){var t,n=e.state,r=e.name,i=e.options,a=n.elements.arrow,o=n.modifiersData.popperOffsets,s=Rf(n.placement),c=ep(s),l=[`left`,`right`].indexOf(s)>=0?`height`:`width`;if(!(!a||!o)){var u=op(i.padding,n),d=Gf(a),f=c===`y`?`top`:yf,p=c===`y`?_f:vf,m=n.rects.reference[l]+n.rects.reference[c]-o[c]-n.rects.popper[l],h=o[c]-n.rects.reference[c],g=$f(a),_=g?c===`y`?g.clientHeight||0:g.clientWidth||0:0,v=m/2-h/2,y=u[f],b=_-d[l]-u[p],x=_/2-d[l]/2+v,S=tp(y,x,b),C=c;n.modifiersData[r]=(t={},t[C]=S,t.centerOffset=S-x,t)}}function cp(e){var t=e.state,n=e.options.element,r=n===void 0?`[data-popper-arrow]`:n;r!=null&&(typeof r==`string`&&(r=t.elements.popper.querySelector(r),!r)||Kf(t.elements.popper,r)&&(t.elements.arrow=r))}var lp={name:`arrow`,enabled:!0,phase:`main`,fn:sp,effect:cp,requires:[`popperOffsets`],requiresIfExists:[`preventOverflow`]};function up(e){return e.split(`-`)[1]}var dp={top:`auto`,right:`auto`,bottom:`auto`,left:`auto`};function fp(e,t){var n=e.x,r=e.y,i=t.devicePixelRatio||1;return{x:Vf(n*i)/i||0,y:Vf(r*i)/i||0}}function pp(e){var t,n=e.popper,r=e.popperRect,i=e.placement,a=e.variation,o=e.offsets,s=e.position,c=e.gpuAcceleration,l=e.adaptive,u=e.roundOffsets,d=e.isFixed,f=o.x,p=f===void 0?0:f,m=o.y,h=m===void 0?0:m,g=typeof u==`function`?u({x:p,y:h}):{x:p,y:h};p=g.x,h=g.y;var _=o.hasOwnProperty(`x`),v=o.hasOwnProperty(`y`),y=yf,b=`top`,x=window;if(l){var S=$f(n),C=`clientHeight`,w=`clientWidth`;if(S===jf(n)&&(S=Yf(n),qf(S).position!==`static`&&s===`absolute`&&(C=`scrollHeight`,w=`scrollWidth`)),S=S,i===`top`||(i===`left`||i===`right`)&&a===`end`){b=_f;var T=d&&S===x&&x.visualViewport?x.visualViewport.height:S[C];h-=T-r.height,h*=c?1:-1}if(i===`left`||(i===`top`||i===`bottom`)&&a===`end`){y=vf;var E=d&&S===x&&x.visualViewport?x.visualViewport.width:S[w];p-=E-r.width,p*=c?1:-1}}var D=Object.assign({position:s},l&&dp),O=u===!0?fp({x:p,y:h},jf(n)):{x:p,y:h};if(p=O.x,h=O.y,c){var k;return Object.assign({},D,(k={},k[b]=v?`0`:``,k[y]=_?`0`:``,k.transform=(x.devicePixelRatio||1)<=1?`translate(`+p+`px, `+h+`px)`:`translate3d(`+p+`px, `+h+`px, 0)`,k))}return Object.assign({},D,(t={},t[b]=v?h+`px`:``,t[y]=_?p+`px`:``,t.transform=``,t))}function mp(e){var t=e.state,n=e.options,r=n.gpuAcceleration,i=r===void 0?!0:r,a=n.adaptive,o=a===void 0?!0:a,s=n.roundOffsets,c=s===void 0?!0:s,l={placement:Rf(t.placement),variation:up(t.placement),popper:t.elements.popper,popperRect:t.rects.popper,gpuAcceleration:i,isFixed:t.options.strategy===`fixed`};t.modifiersData.popperOffsets!=null&&(t.styles.popper=Object.assign({},t.styles.popper,pp(Object.assign({},l,{offsets:t.modifiersData.popperOffsets,position:t.options.strategy,adaptive:o,roundOffsets:c})))),t.modifiersData.arrow!=null&&(t.styles.arrow=Object.assign({},t.styles.arrow,pp(Object.assign({},l,{offsets:t.modifiersData.arrow,position:`absolute`,adaptive:!1,roundOffsets:c})))),t.attributes.popper=Object.assign({},t.attributes.popper,{"data-popper-placement":t.placement})}var hp={name:`computeStyles`,enabled:!0,phase:`beforeWrite`,fn:mp,data:{}},gp={passive:!0};function _p(e){var t=e.state,n=e.instance,r=e.options,i=r.scroll,a=i===void 0?!0:i,o=r.resize,s=o===void 0?!0:o,c=jf(t.elements.popper),l=[].concat(t.scrollParents.reference,t.scrollParents.popper);return a&&l.forEach(function(e){e.addEventListener(`scroll`,n.update,gp)}),s&&c.addEventListener(`resize`,n.update,gp),function(){a&&l.forEach(function(e){e.removeEventListener(`scroll`,n.update,gp)}),s&&c.removeEventListener(`resize`,n.update,gp)}}var vp={name:`eventListeners`,enabled:!0,phase:`write`,fn:function(){},effect:_p,data:{}},yp={left:`right`,right:`left`,bottom:`top`,top:`bottom`};function bp(e){return e.replace(/left|right|bottom|top/g,function(e){return yp[e]})}var xp={start:`end`,end:`start`};function Sp(e){return e.replace(/start|end/g,function(e){return xp[e]})}function Cp(e){var t=jf(e);return{scrollLeft:t.pageXOffset,scrollTop:t.pageYOffset}}function wp(e){return Wf(Yf(e)).left+Cp(e).scrollLeft}function Tp(e,t){var n=jf(e),r=Yf(e),i=n.visualViewport,a=r.clientWidth,o=r.clientHeight,s=0,c=0;if(i){a=i.width,o=i.height;var l=Uf();(l||!l&&t===`fixed`)&&(s=i.offsetLeft,c=i.offsetTop)}return{width:a,height:o,x:s+wp(e),y:c}}function Ep(e){var t=Yf(e),n=Cp(e),r=e.ownerDocument?.body,i=zf(t.scrollWidth,t.clientWidth,r?r.scrollWidth:0,r?r.clientWidth:0),a=zf(t.scrollHeight,t.clientHeight,r?r.scrollHeight:0,r?r.clientHeight:0),o=-n.scrollLeft+wp(e),s=-n.scrollTop;return qf(r||t).direction===`rtl`&&(o+=zf(t.clientWidth,r?r.clientWidth:0)-i),{width:i,height:a,x:o,y:s}}function Dp(e){var t=qf(e),n=t.overflow,r=t.overflowX,i=t.overflowY;return/auto|scroll|overlay|hidden/.test(n+i+r)}function Op(e){return[`html`,`body`,`#document`].indexOf(Af(e))>=0?e.ownerDocument.body:Nf(e)&&Dp(e)?e:Op(Xf(e))}function kp(e,t){t===void 0&&(t=[]);var n=Op(e),r=n===e.ownerDocument?.body,i=jf(n),a=r?[i].concat(i.visualViewport||[],Dp(n)?n:[]):n,o=t.concat(a);return r?o:o.concat(kp(Xf(a)))}function Ap(e){return Object.assign({},e,{left:e.x,top:e.y,right:e.x+e.width,bottom:e.y+e.height})}function jp(e,t){var n=Wf(e,!1,t===`fixed`);return n.top+=e.clientTop,n.left+=e.clientLeft,n.bottom=n.top+e.clientHeight,n.right=n.left+e.clientWidth,n.width=e.clientWidth,n.height=e.clientHeight,n.x=n.left,n.y=n.top,n}function Mp(e,t,n){return t===`viewport`?Ap(Tp(e,n)):Mf(t)?jp(t,n):Ap(Ep(Yf(e)))}function Np(e){var t=kp(Xf(e)),n=[`absolute`,`fixed`].indexOf(qf(e).position)>=0&&Nf(e)?$f(e):e;return Mf(n)?t.filter(function(e){return Mf(e)&&Kf(e,n)&&Af(e)!==`body`}):[]}function Pp(e,t,n,r){var i=t===`clippingParents`?Np(e):[].concat(t),a=[].concat(i,[n]),o=a[0],s=a.reduce(function(t,n){var i=Mp(e,n,r);return t.top=zf(i.top,t.top),t.right=Bf(i.right,t.right),t.bottom=Bf(i.bottom,t.bottom),t.left=zf(i.left,t.left),t},Mp(e,o,r));return s.width=s.right-s.left,s.height=s.bottom-s.top,s.x=s.left,s.y=s.top,s}function Fp(e){var t=e.reference,n=e.element,r=e.placement,i=r?Rf(r):null,a=r?up(r):null,o=t.x+t.width/2-n.width/2,s=t.y+t.height/2-n.height/2,c;switch(i){case`top`:c={x:o,y:t.y-n.height};break;case _f:c={x:o,y:t.y+t.height};break;case vf:c={x:t.x+t.width,y:s};break;case yf:c={x:t.x-n.width,y:s};break;default:c={x:t.x,y:t.y}}var l=i?ep(i):null;if(l!=null){var u=l===`y`?`height`:`width`;switch(a){case Sf:c[l]=c[l]-(t[u]/2-n[u]/2);break;case`end`:c[l]=c[l]+(t[u]/2-n[u]/2);break;default:}}return c}function Ip(e,t){t===void 0&&(t={});var n=t,r=n.placement,i=r===void 0?e.placement:r,a=n.strategy,o=a===void 0?e.strategy:a,s=n.boundary,c=s===void 0?Cf:s,l=n.rootBoundary,u=l===void 0?wf:l,d=n.elementContext,f=d===void 0?Tf:d,p=n.altBoundary,m=p===void 0?!1:p,h=n.padding,g=h===void 0?0:h,_=ip(typeof g==`number`?ap(g,xf):g),v=f===`popper`?Ef:Tf,y=e.rects.popper,b=e.elements[m?v:f],x=Pp(Mf(b)?b:b.contextElement||Yf(e.elements.popper),c,u,o),S=Wf(e.elements.reference),C=Fp({reference:S,element:y,strategy:`absolute`,placement:i}),w=Ap(Object.assign({},y,C)),T=f===`popper`?w:S,E={top:x.top-T.top+_.top,bottom:T.bottom-x.bottom+_.bottom,left:x.left-T.left+_.left,right:T.right-x.right+_.right},D=e.modifiersData.offset;if(f===`popper`&&D){var O=D[i];Object.keys(E).forEach(function(e){var t=[`right`,`bottom`].indexOf(e)>=0?1:-1,n=[`top`,`bottom`].indexOf(e)>=0?`y`:`x`;E[e]+=O[n]*t})}return E}function Lp(e,t){t===void 0&&(t={});var n=t,r=n.placement,i=n.boundary,a=n.rootBoundary,o=n.padding,s=n.flipVariations,c=n.allowedAutoPlacements,l=c===void 0?Of:c,u=up(r),d=u?s?Df:Df.filter(function(e){return up(e)===u}):xf,f=d.filter(function(e){return l.indexOf(e)>=0});f.length===0&&(f=d);var p=f.reduce(function(t,n){return t[n]=Ip(e,{placement:n,boundary:i,rootBoundary:a,padding:o})[Rf(n)],t},{});return Object.keys(p).sort(function(e,t){return p[e]-p[t]})}function Rp(e){if(Rf(e)===`auto`)return[];var t=bp(e);return[Sp(e),t,Sp(t)]}function zp(e){var t=e.state,n=e.options,r=e.name;if(!t.modifiersData[r]._skip){for(var i=n.mainAxis,a=i===void 0?!0:i,o=n.altAxis,s=o===void 0?!0:o,c=n.fallbackPlacements,l=n.padding,u=n.boundary,d=n.rootBoundary,f=n.altBoundary,p=n.flipVariations,m=p===void 0?!0:p,h=n.allowedAutoPlacements,g=t.options.placement,_=Rf(g)===g,v=c||(_||!m?[bp(g)]:Rp(g)),y=[g].concat(v).reduce(function(e,n){return e.concat(Rf(n)===`auto`?Lp(t,{placement:n,boundary:u,rootBoundary:d,padding:l,flipVariations:m,allowedAutoPlacements:h}):n)},[]),b=t.rects.reference,x=t.rects.popper,S=new Map,C=!0,w=y[0],T=0;T<y.length;T++){var E=y[T],D=Rf(E),O=up(E)===Sf,k=[`top`,_f].indexOf(D)>=0,ee=k?`width`:`height`,A=Ip(t,{placement:E,boundary:u,rootBoundary:d,altBoundary:f,padding:l}),j=k?O?vf:yf:O?_f:`top`;b[ee]>x[ee]&&(j=bp(j));var M=bp(j),te=[];if(a&&te.push(A[D]<=0),s&&te.push(A[j]<=0,A[M]<=0),te.every(function(e){return e})){w=E,C=!1;break}S.set(E,te)}if(C)for(var N=m?3:1,P=function(e){var t=y.find(function(t){var n=S.get(t);if(n)return n.slice(0,e).every(function(e){return e})});if(t)return w=t,`break`},F=N;F>0&&P(F)!==`break`;F--);t.placement!==w&&(t.modifiersData[r]._skip=!0,t.placement=w,t.reset=!0)}}var Bp={name:`flip`,enabled:!0,phase:`main`,fn:zp,requiresIfExists:[`offset`],data:{_skip:!1}};function Vp(e,t,n){return n===void 0&&(n={x:0,y:0}),{top:e.top-t.height-n.y,right:e.right-t.width+n.x,bottom:e.bottom-t.height+n.y,left:e.left-t.width-n.x}}function Hp(e){return[`top`,vf,_f,yf].some(function(t){return e[t]>=0})}function Up(e){var t=e.state,n=e.name,r=t.rects.reference,i=t.rects.popper,a=t.modifiersData.preventOverflow,o=Ip(t,{elementContext:`reference`}),s=Ip(t,{altBoundary:!0}),c=Vp(o,r),l=Vp(s,i,a),u=Hp(c),d=Hp(l);t.modifiersData[n]={referenceClippingOffsets:c,popperEscapeOffsets:l,isReferenceHidden:u,hasPopperEscaped:d},t.attributes.popper=Object.assign({},t.attributes.popper,{"data-popper-reference-hidden":u,"data-popper-escaped":d})}var Wp={name:`hide`,enabled:!0,phase:`main`,requiresIfExists:[`preventOverflow`],fn:Up};function Gp(e,t,n){var r=Rf(e),i=[`left`,`top`].indexOf(r)>=0?-1:1,a=typeof n==`function`?n(Object.assign({},t,{placement:e})):n,o=a[0],s=a[1];return o||=0,s=(s||0)*i,[`left`,`right`].indexOf(r)>=0?{x:s,y:o}:{x:o,y:s}}function Kp(e){var t=e.state,n=e.options,r=e.name,i=n.offset,a=i===void 0?[0,0]:i,o=Of.reduce(function(e,n){return e[n]=Gp(n,t.rects,a),e},{}),s=o[t.placement],c=s.x,l=s.y;t.modifiersData.popperOffsets!=null&&(t.modifiersData.popperOffsets.x+=c,t.modifiersData.popperOffsets.y+=l),t.modifiersData[r]=o}var qp={name:`offset`,enabled:!0,phase:`main`,requires:[`popperOffsets`],fn:Kp};function Jp(e){var t=e.state,n=e.name;t.modifiersData[n]=Fp({reference:t.rects.reference,element:t.rects.popper,strategy:`absolute`,placement:t.placement})}var Yp={name:`popperOffsets`,enabled:!0,phase:`read`,fn:Jp,data:{}};function Xp(e){return e===`x`?`y`:`x`}function Zp(e){var t=e.state,n=e.options,r=e.name,i=n.mainAxis,a=i===void 0?!0:i,o=n.altAxis,s=o===void 0?!1:o,c=n.boundary,l=n.rootBoundary,u=n.altBoundary,d=n.padding,f=n.tether,p=f===void 0?!0:f,m=n.tetherOffset,h=m===void 0?0:m,g=Ip(t,{boundary:c,rootBoundary:l,padding:d,altBoundary:u}),_=Rf(t.placement),v=up(t.placement),y=!v,b=ep(_),x=Xp(b),S=t.modifiersData.popperOffsets,C=t.rects.reference,w=t.rects.popper,T=typeof h==`function`?h(Object.assign({},t.rects,{placement:t.placement})):h,E=typeof T==`number`?{mainAxis:T,altAxis:T}:Object.assign({mainAxis:0,altAxis:0},T),D=t.modifiersData.offset?t.modifiersData.offset[t.placement]:null,O={x:0,y:0};if(S){if(a){var k=b===`y`?`top`:yf,ee=b===`y`?_f:vf,A=b===`y`?`height`:`width`,j=S[b],M=j+g[k],te=j-g[ee],N=p?-w[A]/2:0,P=v===`start`?C[A]:w[A],F=v===`start`?-w[A]:-C[A],ne=t.elements.arrow,re=p&&ne?Gf(ne):{width:0,height:0},ie=t.modifiersData[`arrow#persistent`]?t.modifiersData[`arrow#persistent`].padding:rp(),ae=ie[k],oe=ie[ee],se=tp(0,C[A],re[A]),ce=y?C[A]/2-N-se-ae-E.mainAxis:P-se-ae-E.mainAxis,le=y?-C[A]/2+N+se+oe+E.mainAxis:F+se+oe+E.mainAxis,ue=t.elements.arrow&&$f(t.elements.arrow),de=ue?b===`y`?ue.clientTop||0:ue.clientLeft||0:0,fe=D?.[b]??0,pe=j+ce-fe-de,me=j+le-fe,he=tp(p?Bf(M,pe):M,j,p?zf(te,me):te);S[b]=he,O[b]=he-j}if(s){var ge=b===`x`?`top`:yf,_e=b===`x`?_f:vf,ve=S[x],ye=x===`y`?`height`:`width`,be=ve+g[ge],xe=ve-g[_e],Se=[`top`,yf].indexOf(_)!==-1,Ce=D?.[x]??0,we=Se?be:ve-C[ye]-w[ye]-Ce+E.altAxis,Te=Se?ve+C[ye]+w[ye]-Ce-E.altAxis:xe,Ee=p&&Se?np(we,ve,Te):tp(p?we:be,ve,p?Te:xe);S[x]=Ee,O[x]=Ee-ve}t.modifiersData[r]=O}}var Qp={name:`preventOverflow`,enabled:!0,phase:`main`,fn:Zp,requiresIfExists:[`offset`]};function $p(e){return{scrollLeft:e.scrollLeft,scrollTop:e.scrollTop}}function em(e){return e===jf(e)||!Nf(e)?Cp(e):$p(e)}function tm(e){var t=e.getBoundingClientRect(),n=Vf(t.width)/e.offsetWidth||1,r=Vf(t.height)/e.offsetHeight||1;return n!==1||r!==1}function nm(e,t,n){n===void 0&&(n=!1);var r=Nf(t),i=Nf(t)&&tm(t),a=Yf(t),o=Wf(e,i,n),s={scrollLeft:0,scrollTop:0},c={x:0,y:0};return(r||!r&&!n)&&((Af(t)!==`body`||Dp(a))&&(s=em(t)),Nf(t)?(c=Wf(t,!0),c.x+=t.clientLeft,c.y+=t.clientTop):a&&(c.x=wp(a))),{x:o.left+s.scrollLeft-c.x,y:o.top+s.scrollTop-c.y,width:o.width,height:o.height}}function rm(e){var t=new Map,n=new Set,r=[];e.forEach(function(e){t.set(e.name,e)});function i(e){n.add(e.name),[].concat(e.requires||[],e.requiresIfExists||[]).forEach(function(e){if(!n.has(e)){var r=t.get(e);r&&i(r)}}),r.push(e)}return e.forEach(function(e){n.has(e.name)||i(e)}),r}function im(e){var t=rm(e);return kf.reduce(function(e,n){return e.concat(t.filter(function(e){return e.phase===n}))},[])}function am(e){var t;return function(){return t||=new Promise(function(n){Promise.resolve().then(function(){t=void 0,n(e())})}),t}}function om(e){var t=e.reduce(function(e,t){var n=e[t.name];return e[t.name]=n?Object.assign({},n,t,{options:Object.assign({},n.options,t.options),data:Object.assign({},n.data,t.data)}):t,e},{});return Object.keys(t).map(function(e){return t[e]})}var sm={placement:`bottom`,modifiers:[],strategy:`absolute`};function cm(){return![...arguments].some(function(e){return!(e&&typeof e.getBoundingClientRect==`function`)})}function lm(e){e===void 0&&(e={});var t=e,n=t.defaultModifiers,r=n===void 0?[]:n,i=t.defaultOptions,a=i===void 0?sm:i;return function(e,t,n){n===void 0&&(n=a);var i={placement:`bottom`,orderedModifiers:[],options:Object.assign({},sm,a),modifiersData:{},elements:{reference:e,popper:t},attributes:{},styles:{}},o=[],s=!1,c={state:i,setOptions:function(n){var o=typeof n==`function`?n(i.options):n;return u(),i.options=Object.assign({},a,i.options,o),i.scrollParents={reference:Mf(e)?kp(e):e.contextElement?kp(e.contextElement):[],popper:kp(t)},i.orderedModifiers=im(om([].concat(r,i.options.modifiers))).filter(function(e){return e.enabled}),l(),c.update()},forceUpdate:function(){if(!s){var e=i.elements,t=e.reference,n=e.popper;if(cm(t,n)){i.rects={reference:nm(t,$f(n),i.options.strategy===`fixed`),popper:Gf(n)},i.reset=!1,i.placement=i.options.placement,i.orderedModifiers.forEach(function(e){return i.modifiersData[e.name]=Object.assign({},e.data)});for(var r=0;r<i.orderedModifiers.length;r++){if(i.reset===!0){i.reset=!1,r=-1;continue}var a=i.orderedModifiers[r],o=a.fn,l=a.options,u=l===void 0?{}:l,d=a.name;typeof o==`function`&&(i=o({state:i,options:u,name:d,instance:c})||i)}}}},update:am(function(){return new Promise(function(e){c.forceUpdate(),e(i)})}),destroy:function(){u(),s=!0}};if(!cm(e,t))return c;c.setOptions(n).then(function(e){!s&&n.onFirstUpdate&&n.onFirstUpdate(e)});function l(){i.orderedModifiers.forEach(function(e){var t=e.name,n=e.options,r=n===void 0?{}:n,a=e.effect;if(typeof a==`function`){var s=a({state:i,name:t,instance:c,options:r});o.push(s||function(){})}})}function u(){o.forEach(function(e){return e()}),o=[]}return c}}var um=lm({defaultModifiers:[vp,Yp,hp,Lf,qp,Bp,Qp,lp,Wp]});function dm(e){let{elementType:t,externalSlotProps:n,ownerState:r,skipResolvingSlotProps:i=!1,...a}=e,o=i?{}:Ru(n,r),{props:s,internalRef:c}=Wu({...a,externalSlotProps:o}),l=Ql(c,o?.ref,e.additionalProps?.ref);return Iu(t,{...s,ref:l},r)}var fm=dm;function pm(e){return e?.props?.ref||null}var mm=c(m());function hm(e){return typeof e==`function`?e():e}var gm=x.forwardRef(function(e,t){let{children:n,container:r,disablePortal:i=!1}=e,[a,o]=x.useState(null),s=Ql(x.isValidElement(n)?pm(n):null,t);if(es(()=>{i||o(hm(r)||document.body)},[r,i]),es(()=>{if(a&&!i)return Wl(t,a),()=>{Wl(t,null)}},[t,a,i]),i){if(x.isValidElement(n)){let e={ref:s};return x.cloneElement(n,e)}return n}return a&&mm.createPortal(n,a)});function _m(e){return zo(`MuiPopper`,e)}Bo(`MuiPopper`,[`root`]);function vm(e,t){if(t===`ltr`)return e;switch(e){case`bottom-end`:return`bottom-start`;case`bottom-start`:return`bottom-end`;case`top-end`:return`top-start`;case`top-start`:return`top-end`;default:return e}}function ym(e){return typeof e==`function`?e():e}function bm(e){return e.nodeType!==void 0}var xm=e=>{let{classes:t}=e;return dc({root:[`root`]},_m,t)},Sm={},Cm=x.forwardRef(function(e,t){let{anchorEl:n,children:r,direction:i,disablePortal:a,modifiers:o,open:s,placement:c,popperOptions:l,popperRef:u,slotProps:d={},slots:f={},TransitionProps:p,ownerState:m,...h}=e,g=x.useRef(null),_=Ql(g,t),v=x.useRef(null),y=Ql(v,u),b=x.useRef(y);es(()=>{b.current=y},[y]),x.useImperativeHandle(u,()=>v.current,[]);let S=vm(c,i),[C,w]=x.useState(S),[T,E]=x.useState(ym(n));x.useEffect(()=>{v.current&&v.current.forceUpdate()}),x.useEffect(()=>{n&&E(ym(n))},[n]),es(()=>{if(!T||!s)return;let e=e=>{w(e.placement)},t=[{name:`preventOverflow`,options:{altBoundary:a}},{name:`flip`,options:{altBoundary:a}},{name:`onUpdate`,enabled:!0,phase:`afterWrite`,fn:({state:t})=>{e(t)}}];o!=null&&(t=t.concat(o)),l&&l.modifiers!=null&&(t=t.concat(l.modifiers));let n=um(T,g.current,{placement:S,...l,modifiers:t});return b.current(n),()=>{n.destroy(),b.current(null)}},[T,a,o,s,l,S]);let D={placement:C};p!==null&&(D.TransitionProps=p);let O=xm(e),k=f.root??`div`;return(0,I.jsx)(k,{...fm({elementType:k,externalSlotProps:d.root,externalForwardedProps:h,additionalProps:{role:`tooltip`,ref:_},ownerState:e,className:O.root}),children:typeof r==`function`?r(D):r})}),wm=B(x.forwardRef(function(e,t){let{anchorEl:n,children:r,container:i,direction:a=`ltr`,disablePortal:o=!1,keepMounted:s=!1,modifiers:c,open:l,placement:u=`bottom`,popperOptions:d=Sm,popperRef:f,style:p,transition:m=!1,slotProps:h={},slots:g={},..._}=e,[v,y]=x.useState(!0),b=()=>{y(!1)},S=()=>{y(!0)};if(!s&&!l&&(!m||v))return null;let C;if(i)C=i;else if(n){let e=ym(n);C=e&&bm(e)?Hl(e).body:Hl(null).body}let w=!l&&s&&(!m||v)?`none`:void 0,T=m?{in:l,onEnter:b,onExited:S}:void 0;return(0,I.jsx)(gm,{disablePortal:o,container:C,children:(0,I.jsx)(Cm,{anchorEl:n,direction:a,disablePortal:o,modifiers:c,ref:t,open:m?!v:l,placement:u,popperOptions:d,popperRef:f,slotProps:h,slots:g,..._,style:{position:`fixed`,top:0,left:0,display:w,...p},TransitionProps:T,children:r})})}),{name:`MuiPopper`,slot:`Root`})({}),Tm=x.forwardRef(function(e,t){let n=Os(),{anchorEl:r,component:i,components:a,componentsProps:o,container:s,disablePortal:c,keepMounted:l,modifiers:u,open:d,placement:f,popperOptions:p,popperRef:m,transition:h,slots:g,slotProps:_,...v}=Nl({props:e,name:`MuiPopper`}),y=g?.root??a?.Root,b={anchorEl:r,container:s,disablePortal:c,keepMounted:l,modifiers:u,open:d,placement:f,popperOptions:p,popperRef:m,transition:h,...v};return(0,I.jsx)(wm,{as:i,direction:n?`rtl`:`ltr`,slots:{root:y},slotProps:_??o,...b,ref:t})}),Em=zl((0,I.jsx)(`path`,{d:`M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z`}),`Cancel`);function Dm(e){return zo(`MuiChip`,e)}var Om=Bo(`MuiChip`,`root.sizeSmall.sizeMedium.colorDefault.colorError.colorInfo.colorPrimary.colorSecondary.colorSuccess.colorWarning.disabled.clickable.clickableColorPrimary.clickableColorSecondary.deletable.deletableColorPrimary.deletableColorSecondary.outlined.filled.outlinedPrimary.outlinedSecondary.filledPrimary.filledSecondary.avatar.avatarSmall.avatarMedium.avatarColorPrimary.avatarColorSecondary.icon.iconSmall.iconMedium.iconColorPrimary.iconColorSecondary.label.labelSmall.labelMedium.deleteIcon.deleteIconSmall.deleteIconMedium.deleteIconColorPrimary.deleteIconColorSecondary.deleteIconOutlinedColorPrimary.deleteIconOutlinedColorSecondary.deleteIconFilledColorPrimary.deleteIconFilledColorSecondary.focusVisible`.split(`.`)),km=e=>{let{classes:t,disabled:n,size:r,color:i,iconColor:a,onDelete:o,clickable:s,variant:c}=e;return dc({root:[`root`,c,n&&`disabled`,`size${V(r)}`,`color${V(i)}`,s&&`clickable`,s&&`clickableColor${V(i)}`,o&&`deletable`,o&&`deletableColor${V(i)}`,`${c}${V(i)}`],label:[`label`,`label${V(r)}`],avatar:[`avatar`,`avatar${V(r)}`,`avatarColor${V(i)}`],icon:[`icon`,`icon${V(r)}`,`iconColor${V(a)}`],deleteIcon:[`deleteIcon`,`deleteIcon${V(r)}`,`deleteIconColor${V(i)}`,`deleteIcon${V(c)}Color${V(i)}`]},Dm,t)},Am=B(`div`,{name:`MuiChip`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e,{color:r,iconColor:i,clickable:a,onDelete:o,size:s,variant:c}=n;return[{[`& .${Om.avatar}`]:t.avatar},{[`& .${Om.avatar}`]:t[`avatar${V(s)}`]},{[`& .${Om.avatar}`]:t[`avatarColor${V(r)}`]},{[`& .${Om.icon}`]:t.icon},{[`& .${Om.icon}`]:t[`icon${V(s)}`]},{[`& .${Om.icon}`]:t[`iconColor${V(i)}`]},{[`& .${Om.deleteIcon}`]:t.deleteIcon},{[`& .${Om.deleteIcon}`]:t[`deleteIcon${V(s)}`]},{[`& .${Om.deleteIcon}`]:t[`deleteIconColor${V(r)}`]},{[`& .${Om.deleteIcon}`]:t[`deleteIcon${V(c)}Color${V(r)}`]},t.root,t[`size${V(s)}`],t[`color${V(r)}`],a&&t.clickable,a&&r!==`default`&&t[`clickableColor${V(r)})`],o&&t.deletable,o&&r!==`default`&&t[`deletableColor${V(r)}`],t[c],t[`${c}${V(r)}`]]}})(Ml(({theme:e})=>{let t=e.palette.mode===`light`?e.palette.grey[700]:e.palette.grey[300];return{maxWidth:`100%`,fontFamily:e.typography.fontFamily,fontSize:e.typography.pxToRem(13),display:`inline-flex`,alignItems:`center`,justifyContent:`center`,height:32,lineHeight:1.5,color:(e.vars||e).palette.text.primary,backgroundColor:(e.vars||e).palette.action.selected,borderRadius:32/2,whiteSpace:`nowrap`,transition:e.transitions.create([`background-color`,`box-shadow`]),cursor:`unset`,outline:0,textDecoration:`none`,border:0,padding:0,verticalAlign:`middle`,boxSizing:`border-box`,[`&.${Om.disabled}`]:{opacity:(e.vars||e).palette.action.disabledOpacity,pointerEvents:`none`},[`& .${Om.avatar}`]:{marginLeft:5,marginRight:-6,width:24,height:24,color:e.vars?e.vars.palette.Chip.defaultAvatarColor:t,fontSize:e.typography.pxToRem(12)},[`& .${Om.avatarColorPrimary}`]:{color:(e.vars||e).palette.primary.contrastText,backgroundColor:(e.vars||e).palette.primary.dark},[`& .${Om.avatarColorSecondary}`]:{color:(e.vars||e).palette.secondary.contrastText,backgroundColor:(e.vars||e).palette.secondary.dark},[`& .${Om.avatarSmall}`]:{marginLeft:4,marginRight:-4,width:18,height:18,fontSize:e.typography.pxToRem(10)},[`& .${Om.icon}`]:{marginLeft:5,marginRight:-6},[`& .${Om.deleteIcon}`]:{WebkitTapHighlightColor:`transparent`,color:e.alpha((e.vars||e).palette.text.primary,.26),fontSize:22,cursor:`pointer`,margin:`0 5px 0 -6px`,"&:hover":{color:e.alpha((e.vars||e).palette.text.primary,.4)}},variants:[{props:{size:`small`},style:{height:24,[`& .${Om.icon}`]:{fontSize:18,marginLeft:4,marginRight:-4},[`& .${Om.deleteIcon}`]:{fontSize:16,marginRight:4,marginLeft:-4}}},...Object.entries(e.palette).filter(Ed([`contrastText`])).map(([t])=>({props:{color:t},style:{backgroundColor:(e.vars||e).palette[t].main,color:(e.vars||e).palette[t].contrastText,[`& .${Om.deleteIcon}`]:{color:e.alpha((e.vars||e).palette[t].contrastText,.7),"&:hover, &:active":{color:(e.vars||e).palette[t].contrastText}}}})),{props:e=>e.iconColor===e.color,style:{[`& .${Om.icon}`]:{color:e.vars?e.vars.palette.Chip.defaultIconColor:t}}},{props:e=>e.iconColor===e.color&&e.color!==`default`,style:{[`& .${Om.icon}`]:{color:`inherit`}}},{props:{onDelete:!0},style:{[`&.${Om.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)}}},...Object.entries(e.palette).filter(Ed([`dark`])).map(([t])=>({props:{color:t,onDelete:!0},style:{[`&.${Om.focusVisible}`]:{background:(e.vars||e).palette[t].dark}}})),{props:{clickable:!0},style:{userSelect:`none`,WebkitTapHighlightColor:`transparent`,cursor:`pointer`,"&:hover":{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.hoverOpacity}`)},[`&.${Om.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)},"&:active":{boxShadow:(e.vars||e).shadows[1]}}},...Object.entries(e.palette).filter(Ed([`dark`])).map(([t])=>({props:{color:t,clickable:!0},style:{[`&:hover, &.${Om.focusVisible}`]:{backgroundColor:(e.vars||e).palette[t].dark}}})),{props:{variant:`outlined`},style:{backgroundColor:`transparent`,border:e.vars?`1px solid ${e.vars.palette.Chip.defaultBorder}`:`1px solid ${e.palette.mode===`light`?e.palette.grey[400]:e.palette.grey[700]}`,[`&.${Om.clickable}:hover`]:{backgroundColor:(e.vars||e).palette.action.hover},[`&.${Om.focusVisible}`]:{backgroundColor:(e.vars||e).palette.action.focus},[`& .${Om.avatar}`]:{marginLeft:4},[`& .${Om.avatarSmall}`]:{marginLeft:2},[`& .${Om.icon}`]:{marginLeft:4},[`& .${Om.iconSmall}`]:{marginLeft:2},[`& .${Om.deleteIcon}`]:{marginRight:5},[`& .${Om.deleteIconSmall}`]:{marginRight:3}}},...Object.entries(e.palette).filter(Ed()).map(([t])=>({props:{variant:`outlined`,color:t},style:{color:(e.vars||e).palette[t].main,border:`1px solid ${e.alpha((e.vars||e).palette[t].main,.7)}`,[`&.${Om.clickable}:hover`]:{backgroundColor:e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)},[`&.${Om.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.focusOpacity)},[`& .${Om.deleteIcon}`]:{color:e.alpha((e.vars||e).palette[t].main,.7),"&:hover, &:active":{color:(e.vars||e).palette[t].main}}}}))]}})),jm=B(`span`,{name:`MuiChip`,slot:`Label`,overridesResolver:(e,t)=>{let{ownerState:n}=e,{size:r}=n;return[t.label,t[`label${V(r)}`]]}})({overflow:`hidden`,textOverflow:`ellipsis`,paddingLeft:12,paddingRight:12,whiteSpace:`nowrap`,variants:[{props:{variant:`outlined`},style:{paddingLeft:11,paddingRight:11}},{props:{size:`small`},style:{paddingLeft:8,paddingRight:8}},{props:{size:`small`,variant:`outlined`},style:{paddingLeft:7,paddingRight:7}}]});function Mm(e){return e.key===`Backspace`||e.key===`Delete`}var Nm=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiChip`}),{avatar:r,className:i,clickable:a,color:o=`default`,component:s,deleteIcon:c,disabled:l=!1,icon:u,label:d,onClick:f,onDelete:p,onKeyDown:m,onKeyUp:h,size:g=`medium`,variant:_=`filled`,tabIndex:v,skipFocusWhenDisabled:y=!1,slots:b={},slotProps:S={},...C}=n,w=$l(x.useRef(null),t),T=e=>{e.stopPropagation(),p&&p(e)},E=e=>{e.currentTarget===e.target&&Mm(e)&&e.preventDefault(),m&&m(e)},D=e=>{e.currentTarget===e.target&&p&&Mm(e)&&p(e),h&&h(e)},O=a!==!1&&f?!0:a,k=O||p?Cd:s||`div`,ee={...n,component:k,disabled:l,size:g,color:o,iconColor:x.isValidElement(u)&&u.props.color||o,onDelete:!!p,clickable:O,variant:_},A=km(ee),j=k===Cd?{component:s||`div`,focusVisibleClassName:A.focusVisible,...p&&{disableRipple:!0}}:{},M=null;p&&(M=c&&x.isValidElement(c)?x.cloneElement(c,{className:R(c.props.className,A.deleteIcon),onClick:T}):(0,I.jsx)(Em,{className:A.deleteIcon,onClick:T}));let te=null;r&&x.isValidElement(r)&&(te=x.cloneElement(r,{className:R(A.avatar,r.props.className)}));let N=null;u&&x.isValidElement(u)&&(N=x.cloneElement(u,{className:R(A.icon,u.props.className)}));let P={slots:b,slotProps:S},[F,ne]=Gu(`root`,{elementType:Am,externalForwardedProps:{...P,...C},ownerState:ee,shouldForwardComponentProp:!0,ref:w,className:R(A.root,i),additionalProps:{disabled:O&&l?!0:void 0,tabIndex:y&&l?-1:v,...j},getSlotProps:e=>({...e,onClick:t=>{e.onClick?.(t),f?.(t)},onKeyDown:t=>{e.onKeyDown?.(t),E(t)},onKeyUp:t=>{e.onKeyUp?.(t),D(t)}})}),[re,ie]=Gu(`label`,{elementType:jm,externalForwardedProps:P,ownerState:ee,className:A.label});return(0,I.jsxs)(F,{as:k,...ne,children:[te||N,(0,I.jsx)(re,{...ie,children:d}),M]})});function Pm(e){return parseInt(e,10)||0}var Fm={shadow:{visibility:`hidden`,position:`absolute`,overflow:`hidden`,height:0,top:0,left:0,transform:`translateZ(0)`}};function Im(e){for(let t in e)return!1;return!0}function Lm(e){return Im(e)||e.outerHeightStyle===0&&!e.overflowing}var Rm=x.forwardRef(function(e,t){let{onChange:n,maxRows:r,minRows:i=1,style:a,value:o,...s}=e,{current:c}=x.useRef(o!=null),l=x.useRef(null),u=Ql(t,l),d=x.useRef(null),f=x.useRef(null),p=x.useCallback(()=>{let t=l.current,n=f.current;if(!t||!n)return;let a=Ul(t).getComputedStyle(t);if(a.width===`0px`)return{outerHeightStyle:0,overflowing:!1};n.style.width=a.width,n.value=t.value||e.placeholder||`x`,n.value.slice(-1)===`
`&&(n.value+=` `);let o=a.boxSizing,s=Pm(a.paddingBottom)+Pm(a.paddingTop),c=Pm(a.borderBottomWidth)+Pm(a.borderTopWidth),u=n.scrollHeight;n.value=`x`;let d=n.scrollHeight,p=u;return i&&(p=Math.max(Number(i)*d,p)),r&&(p=Math.min(Number(r)*d,p)),p=Math.max(p,d),{outerHeightStyle:p+(o===`border-box`?s+c:0),overflowing:Math.abs(p-u)<=1}},[r,i,e.placeholder]),m=Xl(()=>{let e=l.current,t=p();if(!e||!t||Lm(t))return!1;let n=t.outerHeightStyle;return d.current!=null&&d.current!==n}),h=x.useCallback(()=>{let e=l.current,t=p();if(!e||!t||Lm(t))return;let n=t.outerHeightStyle;d.current!==n&&(d.current=n,e.style.height=`${n}px`),e.style.overflow=t.overflowing?`hidden`:``},[p]),g=x.useRef(-1);return es(()=>{let e=Bl(h),t=l?.current;if(!t)return;let n=Ul(t);n.addEventListener(`resize`,e);let r;return typeof ResizeObserver<`u`&&(r=new ResizeObserver(()=>{m()&&(r.unobserve(t),cancelAnimationFrame(g.current),h(),g.current=requestAnimationFrame(()=>{r.observe(t)}))}),r.observe(t)),()=>{e.clear(),cancelAnimationFrame(g.current),n.removeEventListener(`resize`,e),r&&r.disconnect()}},[p,h,m]),es(()=>{h()}),(0,I.jsxs)(x.Fragment,{children:[(0,I.jsx)(`textarea`,{value:o,onChange:e=>{c||h();let t=e.target,r=t.value.length,i=t.value.endsWith(`
`),a=t.selectionStart===r;i&&a&&t.setSelectionRange(r,r),n&&n(e)},ref:u,rows:i,style:a,...s}),(0,I.jsx)(`textarea`,{"aria-hidden":!0,className:e.className,readOnly:!0,ref:f,tabIndex:-1,style:{...Fm.shadow,...a,paddingTop:0,paddingBottom:0}})]})});function zm({props:e,states:t,muiFormControl:n}){return t.reduce((t,r)=>(t[r]=e[r],n&&e[r]===void 0&&(t[r]=n[r]),t),{})}var Bm=x.createContext(void 0);function Vm(){return x.useContext(Bm)}function Hm(e){return e!=null&&!(Array.isArray(e)&&e.length===0)}function Um(e,t=!1){return e&&(Hm(e.value)&&e.value!==``||t&&Hm(e.defaultValue)&&e.defaultValue!==``)}function Wm(e){return zo(`MuiInputBase`,e)}var Gm=Bo(`MuiInputBase`,[`root`,`formControl`,`focused`,`disabled`,`adornedStart`,`adornedEnd`,`error`,`sizeSmall`,`multiline`,`colorSecondary`,`fullWidth`,`hiddenLabel`,`readOnly`,`input`,`inputSizeSmall`,`inputMultiline`,`inputTypeSearch`,`inputAdornedStart`,`inputAdornedEnd`,`inputHiddenLabel`]),Km;const qm=(e,t)=>{let{ownerState:n}=e;return[t.root,n.formControl&&t.formControl,n.startAdornment&&t.adornedStart,n.endAdornment&&t.adornedEnd,n.error&&t.error,n.size===`small`&&t.sizeSmall,n.multiline&&t.multiline,n.color&&t[`color${V(n.color)}`],n.fullWidth&&t.fullWidth,n.hiddenLabel&&t.hiddenLabel]},Jm=(e,t)=>{let{ownerState:n}=e;return[t.input,n.size===`small`&&t.inputSizeSmall,n.multiline&&t.inputMultiline,n.type===`search`&&t.inputTypeSearch,n.startAdornment&&t.inputAdornedStart,n.endAdornment&&t.inputAdornedEnd,n.hiddenLabel&&t.inputHiddenLabel]};var Ym=e=>{let{classes:t,color:n,disabled:r,error:i,endAdornment:a,focused:o,formControl:s,fullWidth:c,hiddenLabel:l,multiline:u,readOnly:d,size:f,startAdornment:p,type:m}=e;return dc({root:[`root`,`color${V(n)}`,r&&`disabled`,i&&`error`,c&&`fullWidth`,o&&`focused`,s&&`formControl`,f&&f!==`medium`&&`size${V(f)}`,u&&`multiline`,p&&`adornedStart`,a&&`adornedEnd`,l&&`hiddenLabel`,d&&`readOnly`],input:[`input`,r&&`disabled`,m===`search`&&`inputTypeSearch`,u&&`inputMultiline`,f===`small`&&`inputSizeSmall`,l&&`inputHiddenLabel`,p&&`inputAdornedStart`,a&&`inputAdornedEnd`,d&&`readOnly`]},Wm,t)};const Xm=B(`div`,{name:`MuiInputBase`,slot:`Root`,overridesResolver:qm})(Ml(({theme:e})=>({...e.typography.body1,color:(e.vars||e).palette.text.primary,lineHeight:`1.4375em`,boxSizing:`border-box`,position:`relative`,cursor:`text`,display:`inline-flex`,alignItems:`center`,[`&.${Gm.disabled}`]:{color:(e.vars||e).palette.text.disabled,cursor:`default`},variants:[{props:({ownerState:e})=>e.multiline,style:{padding:`4px 0 5px`}},{props:({ownerState:e,size:t})=>e.multiline&&t===`small`,style:{paddingTop:1}},{props:({ownerState:e})=>e.fullWidth,style:{width:`100%`}}]}))),Zm=B(`input`,{name:`MuiInputBase`,slot:`Input`,overridesResolver:Jm})(Ml(({theme:e})=>{let t=e.palette.mode===`light`,n={color:`currentColor`,...e.vars?{opacity:e.vars.opacity.inputPlaceholder}:{opacity:t?.42:.5},transition:e.transitions.create(`opacity`,{duration:e.transitions.duration.shorter})},r={opacity:`0 !important`},i=e.vars?{opacity:e.vars.opacity.inputPlaceholder}:{opacity:t?.42:.5};return{font:`inherit`,letterSpacing:`inherit`,color:`currentColor`,padding:`4px 0 5px`,border:0,boxSizing:`content-box`,background:`none`,height:`1.4375em`,margin:0,WebkitTapHighlightColor:`transparent`,display:`block`,minWidth:0,width:`100%`,"&::-webkit-input-placeholder":n,"&::-moz-placeholder":n,"&::-ms-input-placeholder":n,"&:focus":{outline:0},"&:invalid":{boxShadow:`none`},"&::-webkit-search-decoration":{WebkitAppearance:`none`},[`label[data-shrink=false] + .${Gm.formControl} &`]:{"&::-webkit-input-placeholder":r,"&::-moz-placeholder":r,"&::-ms-input-placeholder":r,"&:focus::-webkit-input-placeholder":i,"&:focus::-moz-placeholder":i,"&:focus::-ms-input-placeholder":i},[`&.${Gm.disabled}`]:{opacity:1,WebkitTextFillColor:(e.vars||e).palette.text.disabled},variants:[{props:({ownerState:e})=>!e.disableInjectingGlobalStyles,style:{animationName:`mui-auto-fill-cancel`,animationDuration:`10ms`,"&:-webkit-autofill":{animationDuration:`5000s`,animationName:`mui-auto-fill`}}},{props:{size:`small`},style:{paddingTop:1}},{props:({ownerState:e})=>e.multiline,style:{height:`auto`,resize:`none`,padding:0,paddingTop:0}},{props:{type:`search`},style:{MozAppearance:`textfield`}}]}}));var Qm=Al({"@keyframes mui-auto-fill":{from:{display:`block`}},"@keyframes mui-auto-fill-cancel":{from:{display:`block`}}}),$m=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiInputBase`}),{"aria-describedby":r,autoComplete:i,autoFocus:a,className:o,color:s,components:c={},componentsProps:l={},defaultValue:u,disabled:d,disableInjectingGlobalStyles:f,endAdornment:p,error:m,fullWidth:h=!1,id:g,inputComponent:_=`input`,inputProps:v={},inputRef:y,margin:b,maxRows:S,minRows:C,multiline:w=!1,name:T,onBlur:E,onChange:D,onClick:O,onFocus:k,onKeyDown:ee,onKeyUp:A,placeholder:j,readOnly:M,renderSuffix:te,rows:N,size:P,slotProps:F={},slots:ne={},startAdornment:re,type:ie=`text`,value:ae,...oe}=n,se=v.value==null?ae:v.value,{current:ce}=x.useRef(se!=null),le=x.useRef(),ue=x.useCallback(e=>{},[]),de=$l(le,y,v.ref,ue),[fe,pe]=x.useState(!1),me=Vm(),he=zm({props:n,muiFormControl:me,states:[`color`,`disabled`,`error`,`hiddenLabel`,`size`,`required`,`filled`]});he.focused=me?me.focused:fe,x.useEffect(()=>{!me&&d&&fe&&(pe(!1),E&&E())},[me,d,fe,E]);let ge=me&&me.onFilled,_e=me&&me.onEmpty,ve=x.useCallback(e=>{Um(e)?ge&&ge():_e&&_e()},[ge,_e]);Gl(()=>{ce&&ve({value:se})},[se,ve,ce]);let ye=e=>{k&&k(e),v.onFocus&&v.onFocus(e),me&&me.onFocus?me.onFocus(e):pe(!0)},be=e=>{E&&E(e),v.onBlur&&v.onBlur(e),me&&me.onBlur?me.onBlur(e):pe(!1)},xe=(e,...t)=>{if(!ce){let t=e.target||le.current;if(t==null)throw Error(kn(1));ve({value:t.value})}v.onChange&&v.onChange(e,...t),D&&D(e,...t)};x.useEffect(()=>{ve(le.current)},[]);let Se=e=>{le.current&&e.currentTarget===e.target&&le.current.focus(),O&&O(e)},Ce=_,we=v;w&&Ce===`input`&&(we=N?{type:void 0,minRows:N,maxRows:N,...we}:{type:void 0,maxRows:S,minRows:C,...we},Ce=Rm);let Te=e=>{ve(e.animationName===`mui-auto-fill-cancel`?le.current:{value:`x`})};x.useEffect(()=>{me&&me.setAdornedStart(!!re)},[me,re]);let Ee={...n,color:he.color||`primary`,disabled:he.disabled,endAdornment:p,error:he.error,focused:he.focused,formControl:me,fullWidth:h,hiddenLabel:he.hiddenLabel,multiline:w,size:he.size,startAdornment:re,type:ie},De=Ym(Ee),Oe=ne.root||c.Root||Xm,ke=F.root||l.root||{},Ae=ne.input||c.Input||Zm;return we={...we,...F.input??l.input},(0,I.jsxs)(x.Fragment,{children:[!f&&typeof Qm==`function`&&(Km||=(0,I.jsx)(Qm,{})),(0,I.jsxs)(Oe,{...ke,ref:t,onClick:Se,...oe,...!Pu(Oe)&&{ownerState:{...Ee,...ke.ownerState}},className:R(De.root,ke.className,o,M&&`MuiInputBase-readOnly`),children:[re,(0,I.jsx)(Bm.Provider,{value:null,children:(0,I.jsx)(Ae,{"aria-invalid":he.error,"aria-describedby":r,autoComplete:i,autoFocus:a,defaultValue:u,disabled:he.disabled,id:g,onAnimationStart:Te,name:T,placeholder:j,readOnly:M,required:he.required,rows:N,value:se,onKeyDown:ee,onKeyUp:A,type:ie,...we,...!Pu(Ae)&&{as:Ce,ownerState:{...Ee,...we.ownerState}},ref:de,className:R(De.input,we.className,M&&`MuiInputBase-readOnly`),onBlur:be,onChange:xe,onFocus:ye})}),p,te?te({...he,startAdornment:re}):null]})]})}),eh={entering:{opacity:1},entered:{opacity:1}},th=x.forwardRef(function(e,t){let n=gl(),r={enter:n.transitions.duration.enteringScreen,exit:n.transitions.duration.leavingScreen},{addEndListener:i,appear:a=!0,children:o,easing:s,in:c,onEnter:l,onEntered:u,onEntering:d,onExit:f,onExited:p,onExiting:m,style:h,timeout:g=r,TransitionComponent:_=mu,...v}=e,y=x.useRef(null),b=$l(y,pm(o),t),S=e=>t=>{if(e){let n=y.current;t===void 0?e(n):e(n,t)}},C=S(d),w=S((e,t)=>{ju(e);let r=Mu({style:h,timeout:g,easing:s},{mode:`enter`});e.style.webkitTransition=n.transitions.create(`opacity`,r),e.style.transition=n.transitions.create(`opacity`,r),l&&l(e,t)}),T=S(u),E=S(m),D=S(e=>{let t=Mu({style:h,timeout:g,easing:s},{mode:`exit`});e.style.webkitTransition=n.transitions.create(`opacity`,t),e.style.transition=n.transitions.create(`opacity`,t),f&&f(e)}),O=S(p);return(0,I.jsx)(_,{appear:a,in:c,nodeRef:y,onEnter:w,onEntered:T,onEntering:C,onExit:D,onExited:O,onExiting:E,addEndListener:e=>{i&&i(y.current,e)},timeout:g,...v,children:(e,{ownerState:t,...n})=>x.cloneElement(o,{style:{opacity:0,visibility:e===`exited`&&!c?`hidden`:void 0,...eh[e],...h,...o.props.style},ref:b,...n})})});function nh(e){return zo(`MuiBackdrop`,e)}Bo(`MuiBackdrop`,[`root`,`invisible`]);var rh=e=>{let{classes:t,invisible:n}=e;return dc({root:[`root`,n&&`invisible`]},nh,t)},ih=B(`div`,{name:`MuiBackdrop`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.invisible&&t.invisible]}})({position:`fixed`,display:`flex`,alignItems:`center`,justifyContent:`center`,right:0,bottom:0,top:0,left:0,backgroundColor:`rgba(0, 0, 0, 0.5)`,WebkitTapHighlightColor:`transparent`,variants:[{props:{invisible:!0},style:{backgroundColor:`transparent`}}]}),ah=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiBackdrop`}),{children:r,className:i,component:a=`div`,invisible:o=!1,open:s,components:c={},componentsProps:l={},slotProps:u={},slots:d={},TransitionComponent:f,transitionDuration:p,...m}=n,h={...n,component:a,invisible:o},g=rh(h),_={component:a,slots:{transition:f,root:c.Root,...d},slotProps:{...l,...u}},[v,y]=Gu(`root`,{elementType:ih,externalForwardedProps:_,className:R(g.root,i),ownerState:h}),[b,x]=Gu(`transition`,{elementType:th,externalForwardedProps:_,ownerState:h});return(0,I.jsx)(b,{in:s,timeout:p,...m,...x,children:(0,I.jsx)(v,{"aria-hidden":!0,...y,classes:g,ref:t,children:r})})}),oh=Bo(`MuiBox`,[`root`]),W=Lo({themeId:An,defaultTheme:ml(),defaultClassName:oh.root,generateClassName:No.generate});function sh(e){return zo(`MuiButton`,e)}var ch=Bo(`MuiButton`,`root.text.textInherit.textPrimary.textSecondary.textSuccess.textError.textInfo.textWarning.outlined.outlinedInherit.outlinedPrimary.outlinedSecondary.outlinedSuccess.outlinedError.outlinedInfo.outlinedWarning.contained.containedInherit.containedPrimary.containedSecondary.containedSuccess.containedError.containedInfo.containedWarning.disableElevation.focusVisible.disabled.colorInherit.colorPrimary.colorSecondary.colorSuccess.colorError.colorInfo.colorWarning.textSizeSmall.textSizeMedium.textSizeLarge.outlinedSizeSmall.outlinedSizeMedium.outlinedSizeLarge.containedSizeSmall.containedSizeMedium.containedSizeLarge.sizeMedium.sizeSmall.sizeLarge.fullWidth.startIcon.endIcon.icon.iconSizeSmall.iconSizeMedium.iconSizeLarge.loading.loadingWrapper.loadingIconPlaceholder.loadingIndicator.loadingPositionCenter.loadingPositionStart.loadingPositionEnd`.split(`.`)),lh=x.createContext({}),uh=x.createContext(void 0),dh=e=>{let{color:t,disableElevation:n,fullWidth:r,size:i,variant:a,loading:o,loadingPosition:s,classes:c}=e,l=dc({root:[`root`,o&&`loading`,a,`${a}${V(t)}`,`size${V(i)}`,`${a}Size${V(i)}`,`color${V(t)}`,n&&`disableElevation`,r&&`fullWidth`,o&&`loadingPosition${V(s)}`],startIcon:[`icon`,`startIcon`,`iconSize${V(i)}`],endIcon:[`icon`,`endIcon`,`iconSize${V(i)}`],loadingIndicator:[`loadingIndicator`],loadingWrapper:[`loadingWrapper`]},sh,c);return{...c,...l}},fh=[{props:{size:`small`},style:{"& > *:nth-of-type(1)":{fontSize:18}}},{props:{size:`medium`},style:{"& > *:nth-of-type(1)":{fontSize:20}}},{props:{size:`large`},style:{"& > *:nth-of-type(1)":{fontSize:22}}}],ph=B(Cd,{shouldForwardProp:e=>yl(e)||e===`classes`,name:`MuiButton`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`${n.variant}${V(n.color)}`],t[`size${V(n.size)}`],t[`${n.variant}Size${V(n.size)}`],n.color===`inherit`&&t.colorInherit,n.disableElevation&&t.disableElevation,n.fullWidth&&t.fullWidth,n.loading&&t.loading]}})(Ml(({theme:e})=>{let t=e.palette.mode===`light`?e.palette.grey[300]:e.palette.grey[800],n=e.palette.mode===`light`?e.palette.grey.A100:e.palette.grey[700];return{...e.typography.button,minWidth:64,padding:`6px 16px`,border:0,borderRadius:(e.vars||e).shape.borderRadius,transition:e.transitions.create([`background-color`,`box-shadow`,`border-color`,`color`],{duration:e.transitions.duration.short}),"&:hover":{textDecoration:`none`},[`&.${ch.disabled}`]:{color:(e.vars||e).palette.action.disabled},variants:[{props:{variant:`contained`},style:{color:`var(--variant-containedColor)`,backgroundColor:`var(--variant-containedBg)`,boxShadow:(e.vars||e).shadows[2],"&:hover":{boxShadow:(e.vars||e).shadows[4],"@media (hover: none)":{boxShadow:(e.vars||e).shadows[2]}},"&:active":{boxShadow:(e.vars||e).shadows[8]},[`&.${ch.focusVisible}`]:{boxShadow:(e.vars||e).shadows[6]},[`&.${ch.disabled}`]:{color:(e.vars||e).palette.action.disabled,boxShadow:(e.vars||e).shadows[0],backgroundColor:(e.vars||e).palette.action.disabledBackground}}},{props:{variant:`outlined`},style:{padding:`5px 15px`,border:`1px solid currentColor`,borderColor:`var(--variant-outlinedBorder, currentColor)`,backgroundColor:`var(--variant-outlinedBg)`,color:`var(--variant-outlinedColor)`,[`&.${ch.disabled}`]:{border:`1px solid ${(e.vars||e).palette.action.disabledBackground}`}}},{props:{variant:`text`},style:{padding:`6px 8px`,color:`var(--variant-textColor)`,backgroundColor:`var(--variant-textBg)`}},...Object.entries(e.palette).filter(Ed()).map(([t])=>({props:{color:t},style:{"--variant-textColor":(e.vars||e).palette[t].main,"--variant-outlinedColor":(e.vars||e).palette[t].main,"--variant-outlinedBorder":e.alpha((e.vars||e).palette[t].main,.5),"--variant-containedColor":(e.vars||e).palette[t].contrastText,"--variant-containedBg":(e.vars||e).palette[t].main,"@media (hover: hover)":{"&:hover":{"--variant-containedBg":(e.vars||e).palette[t].dark,"--variant-textBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity),"--variant-outlinedBorder":(e.vars||e).palette[t].main,"--variant-outlinedBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)}}}})),{props:{color:`inherit`},style:{color:`inherit`,borderColor:`currentColor`,"--variant-containedBg":e.vars?e.vars.palette.Button.inheritContainedBg:t,"@media (hover: hover)":{"&:hover":{"--variant-containedBg":e.vars?e.vars.palette.Button.inheritContainedHoverBg:n,"--variant-textBg":e.alpha((e.vars||e).palette.text.primary,(e.vars||e).palette.action.hoverOpacity),"--variant-outlinedBg":e.alpha((e.vars||e).palette.text.primary,(e.vars||e).palette.action.hoverOpacity)}}}},{props:{size:`small`,variant:`text`},style:{padding:`4px 5px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`text`},style:{padding:`8px 11px`,fontSize:e.typography.pxToRem(15)}},{props:{size:`small`,variant:`outlined`},style:{padding:`3px 9px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`outlined`},style:{padding:`7px 21px`,fontSize:e.typography.pxToRem(15)}},{props:{size:`small`,variant:`contained`},style:{padding:`4px 10px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`contained`},style:{padding:`8px 22px`,fontSize:e.typography.pxToRem(15)}},{props:{disableElevation:!0},style:{boxShadow:`none`,"&:hover":{boxShadow:`none`},[`&.${ch.focusVisible}`]:{boxShadow:`none`},"&:active":{boxShadow:`none`},[`&.${ch.disabled}`]:{boxShadow:`none`}}},{props:{fullWidth:!0},style:{width:`100%`}},{props:{loadingPosition:`center`},style:{transition:e.transitions.create([`background-color`,`box-shadow`,`border-color`],{duration:e.transitions.duration.short}),[`&.${ch.loading}`]:{color:`transparent`}}}]}})),mh=B(`span`,{name:`MuiButton`,slot:`StartIcon`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.startIcon,n.loading&&t.startIconLoadingStart,t[`iconSize${V(n.size)}`]]}})(({theme:e})=>({display:`inherit`,marginRight:8,marginLeft:-4,variants:[{props:{size:`small`},style:{marginLeft:-2}},{props:{loadingPosition:`start`,loading:!0},style:{transition:e.transitions.create([`opacity`],{duration:e.transitions.duration.short}),opacity:0}},{props:{loadingPosition:`start`,loading:!0,fullWidth:!0},style:{marginRight:-8}},...fh]})),hh=B(`span`,{name:`MuiButton`,slot:`EndIcon`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.endIcon,n.loading&&t.endIconLoadingEnd,t[`iconSize${V(n.size)}`]]}})(({theme:e})=>({display:`inherit`,marginRight:-4,marginLeft:8,variants:[{props:{size:`small`},style:{marginRight:-2}},{props:{loadingPosition:`end`,loading:!0},style:{transition:e.transitions.create([`opacity`],{duration:e.transitions.duration.short}),opacity:0}},{props:{loadingPosition:`end`,loading:!0,fullWidth:!0},style:{marginLeft:-8}},...fh]})),gh=B(`span`,{name:`MuiButton`,slot:`LoadingIndicator`})(({theme:e})=>({display:`none`,position:`absolute`,visibility:`visible`,variants:[{props:{loading:!0},style:{display:`flex`}},{props:{loadingPosition:`start`},style:{left:14}},{props:{loadingPosition:`start`,size:`small`},style:{left:10}},{props:{variant:`text`,loadingPosition:`start`},style:{left:6}},{props:{loadingPosition:`center`},style:{left:`50%`,transform:`translate(-50%)`,color:(e.vars||e).palette.action.disabled}},{props:{loadingPosition:`end`},style:{right:14}},{props:{loadingPosition:`end`,size:`small`},style:{right:10}},{props:{variant:`text`,loadingPosition:`end`},style:{right:6}},{props:{loadingPosition:`start`,fullWidth:!0},style:{position:`relative`,left:-10}},{props:{loadingPosition:`end`,fullWidth:!0},style:{position:`relative`,right:-10}}]})),_h=B(`span`,{name:`MuiButton`,slot:`LoadingIconPlaceholder`})({display:`inline-block`,width:`1em`,height:`1em`}),vh=x.forwardRef(function(e,t){let n=x.useContext(lh),r=x.useContext(uh),i=Nl({props:$o(n,e),name:`MuiButton`}),{children:a,color:o=`primary`,component:s=`button`,className:c,disabled:l=!1,disableElevation:u=!1,disableFocusRipple:d=!1,endIcon:f,focusVisibleClassName:p,fullWidth:m=!1,id:h,loading:g=null,loadingIndicator:_,loadingPosition:v=`center`,size:y=`medium`,startIcon:b,type:S,variant:C=`text`,...w}=i,T=Kl(h),E=_??(0,I.jsx)(zd,{"aria-labelledby":T,color:`inherit`,size:16}),D={...i,color:o,component:s,disabled:l,disableElevation:u,disableFocusRipple:d,fullWidth:m,loading:g,loadingIndicator:E,loadingPosition:v,size:y,type:S,variant:C},O=dh(D),k=(b||g&&v===`start`)&&(0,I.jsx)(mh,{className:O.startIcon,ownerState:D,children:b||(0,I.jsx)(_h,{className:O.loadingIconPlaceholder,ownerState:D})}),ee=(f||g&&v===`end`)&&(0,I.jsx)(hh,{className:O.endIcon,ownerState:D,children:f||(0,I.jsx)(_h,{className:O.loadingIconPlaceholder,ownerState:D})}),A=r||``,j=typeof g==`boolean`?(0,I.jsx)(`span`,{className:O.loadingWrapper,style:{display:`contents`},children:g&&(0,I.jsx)(gh,{className:O.loadingIndicator,ownerState:D,children:E})}):null;return(0,I.jsxs)(ph,{ownerState:D,className:R(n.className,O.root,c,A),component:s,disabled:l||g,focusRipple:!d,focusVisibleClassName:R(O.focusVisible,p),ref:t,type:S,id:g?T:h,...w,classes:O,children:[k,v!==`end`&&j,a,v===`end`&&j,ee]})}),yh=typeof Al({})==`function`;const bh=(e,t)=>({WebkitFontSmoothing:`antialiased`,MozOsxFontSmoothing:`grayscale`,boxSizing:`border-box`,WebkitTextSizeAdjust:`100%`,...t&&!e.vars&&{colorScheme:e.palette.mode}}),xh=e=>({color:(e.vars||e).palette.text.primary,...e.typography.body1,backgroundColor:(e.vars||e).palette.background.default,"@media print":{backgroundColor:(e.vars||e).palette.common.white}}),Sh=(e,t=!1)=>{let n={};t&&e.colorSchemes&&typeof e.getColorSchemeSelector==`function`&&Object.entries(e.colorSchemes).forEach(([t,r])=>{let i=e.getColorSchemeSelector(t);i.startsWith(`@`)?n[i]={":root":{colorScheme:r.palette?.mode}}:n[i.replace(/\s*&/,``)]={colorScheme:r.palette?.mode}});let r={html:bh(e,t),"*, *::before, *::after":{boxSizing:`inherit`},"strong, b":{fontWeight:e.typography.fontWeightBold},body:{margin:0,...xh(e),"&::backdrop":{backgroundColor:(e.vars||e).palette.background.default}},...n},i=e.components?.MuiCssBaseline?.styleOverrides;return i&&(r=[r,i]),r};var Ch=`mui-ecs`,wh=e=>{let t=Sh(e,!1),n=Array.isArray(t)?t[0]:t;return!e.vars&&n&&(n.html[`:root:has(${Ch})`]={colorScheme:e.palette.mode}),e.colorSchemes&&Object.entries(e.colorSchemes).forEach(([t,r])=>{let i=e.getColorSchemeSelector(t);i.startsWith(`@`)?n[i]={[`:root:not(:has(.${Ch}))`]:{colorScheme:r.palette?.mode}}:n[i.replace(/\s*&/,``)]={[`&:not(:has(.${Ch}))`]:{colorScheme:r.palette?.mode}}}),t},Th=Al(yh?({theme:e,enableColorScheme:t})=>Sh(e,t):({theme:e})=>wh(e));function Eh(e){let{children:t,enableColorScheme:n=!1}=Nl({props:e,name:`MuiCssBaseline`});return(0,I.jsxs)(x.Fragment,{children:[yh&&(0,I.jsx)(Th,{enableColorScheme:n}),!yh&&!n&&(0,I.jsx)(`span`,{className:Ch,style:{display:`none`}}),t]})}var Dh=Eh;function Oh(e=window){let t=e.document.documentElement.clientWidth;return e.innerWidth-t}function kh(e){let t=Hl(e);return t.body===e?Ul(e).innerWidth>t.documentElement.clientWidth:e.scrollHeight>e.clientHeight}function Ah(e,t){t?e.setAttribute(`aria-hidden`,`true`):e.removeAttribute(`aria-hidden`)}function jh(e){return parseInt(Ul(e).getComputedStyle(e).paddingRight,10)||0}function Mh(e){let t=[`TEMPLATE`,`SCRIPT`,`STYLE`,`LINK`,`MAP`,`META`,`NOSCRIPT`,`PICTURE`,`COL`,`COLGROUP`,`PARAM`,`SLOT`,`SOURCE`,`TRACK`].includes(e.tagName),n=e.tagName===`INPUT`&&e.getAttribute(`type`)===`hidden`;return t||n}function Nh(e,t,n,r,i){let a=[t,n,...r];[].forEach.call(e.children,e=>{let t=!a.includes(e),n=!Mh(e);t&&n&&Ah(e,i)})}function Ph(e,t){let n=-1;return e.some((e,r)=>t(e)?(n=r,!0):!1),n}function Fh(e,t){let n=[],r=e.container;if(!t.disableScrollLock){if(kh(r)){let e=Oh(Ul(r));n.push({value:r.style.paddingRight,property:`padding-right`,el:r}),r.style.paddingRight=`${jh(r)+e}px`;let t=Hl(r).querySelectorAll(`.mui-fixed`);[].forEach.call(t,t=>{n.push({value:t.style.paddingRight,property:`padding-right`,el:t}),t.style.paddingRight=`${jh(t)+e}px`})}let e;if(r.parentNode instanceof DocumentFragment)e=Hl(r).body;else{let t=r.parentElement,n=Ul(r);e=t?.nodeName===`HTML`&&n.getComputedStyle(t).overflowY===`scroll`?t:r}n.push({value:e.style.overflow,property:`overflow`,el:e},{value:e.style.overflowX,property:`overflow-x`,el:e},{value:e.style.overflowY,property:`overflow-y`,el:e}),e.style.overflow=`hidden`}return()=>{n.forEach(({value:e,el:t,property:n})=>{e?t.style.setProperty(n,e):t.style.removeProperty(n)})}}function Ih(e){let t=[];return[].forEach.call(e.children,e=>{e.getAttribute(`aria-hidden`)===`true`&&t.push(e)}),t}var Lh=class{constructor(){this.modals=[],this.containers=[]}add(e,t){let n=this.modals.indexOf(e);if(n!==-1)return n;n=this.modals.length,this.modals.push(e),e.modalRef&&Ah(e.modalRef,!1);let r=Ih(t);Nh(t,e.mount,e.modalRef,r,!0);let i=Ph(this.containers,e=>e.container===t);return i===-1?(this.containers.push({modals:[e],container:t,restore:null,hiddenSiblings:r}),n):(this.containers[i].modals.push(e),n)}mount(e,t){let n=Ph(this.containers,t=>t.modals.includes(e)),r=this.containers[n];r.restore||=Fh(r,t)}remove(e,t=!0){let n=this.modals.indexOf(e);if(n===-1)return n;let r=Ph(this.containers,t=>t.modals.includes(e)),i=this.containers[r];if(i.modals.splice(i.modals.indexOf(e),1),this.modals.splice(n,1),i.modals.length===0)i.restore&&i.restore(),e.modalRef&&Ah(e.modalRef,t),Nh(i.container,e.mount,e.modalRef,i.hiddenSiblings,!1),this.containers.splice(r,1);else{let e=i.modals[i.modals.length-1];e.modalRef&&Ah(e.modalRef,!1)}return n}isTopModal(e){return this.modals.length>0&&this.modals[this.modals.length-1]===e}},Rh=[`input`,`select`,`textarea`,`a[href]`,`button`,`[tabindex]`,`audio[controls]`,`video[controls]`,`[contenteditable]:not([contenteditable="false"])`].join(`,`);function zh(e){let t=parseInt(e.getAttribute(`tabindex`)||``,10);return Number.isNaN(t)?e.contentEditable===`true`||(e.nodeName===`AUDIO`||e.nodeName===`VIDEO`||e.nodeName===`DETAILS`)&&e.getAttribute(`tabindex`)===null?0:e.tabIndex:t}function Bh(e){if(e.tagName!==`INPUT`||e.type!==`radio`||!e.name)return!1;let t=t=>e.ownerDocument.querySelector(`input[type="radio"]${t}`),n=t(`[name="${e.name}"]:checked`);return n||=t(`[name="${e.name}"]`),n!==e}function Vh(e){return!(e.disabled||e.tagName===`INPUT`&&e.type===`hidden`||Bh(e))}function Hh(e){let t=[],n=[];return Array.from(e.querySelectorAll(Rh)).forEach((e,r)=>{let i=zh(e);i===-1||!Vh(e)||(i===0?t.push(e):n.push({documentOrder:r,tabIndex:i,node:e}))}),n.sort((e,t)=>e.tabIndex===t.tabIndex?e.documentOrder-t.documentOrder:e.tabIndex-t.tabIndex).map(e=>e.node).concat(t)}function Uh(){return!0}function Wh(e){let{children:t,disableAutoFocus:n=!1,disableEnforceFocus:r=!1,disableRestoreFocus:i=!1,getTabbable:a=Hh,isEnabled:o=Uh,open:s}=e,c=x.useRef(!1),l=x.useRef(null),u=x.useRef(null),d=x.useRef(null),f=x.useRef(null),p=x.useRef(!1),m=x.useRef(null),h=Ql(pm(t),m),g=x.useRef(null);x.useEffect(()=>{!s||!m.current||(p.current=!n)},[n,s]),x.useEffect(()=>{if(!s||!m.current)return;let e=Hl(m.current);return m.current.contains(e.activeElement)||(m.current.hasAttribute(`tabIndex`)||m.current.setAttribute(`tabIndex`,`-1`),p.current&&m.current.focus()),()=>{i||(d.current&&d.current.focus&&(c.current=!0,d.current.focus()),d.current=null)}},[s]),x.useEffect(()=>{if(!s||!m.current)return;let e=Hl(m.current),t=t=>{g.current=t,!(r||!o()||t.key!==`Tab`)&&e.activeElement===m.current&&t.shiftKey&&(c.current=!0,u.current&&u.current.focus())},n=()=>{let t=m.current;if(t===null)return;if(!e.hasFocus()||!o()||c.current){c.current=!1;return}if(t.contains(e.activeElement)||r&&e.activeElement!==l.current&&e.activeElement!==u.current)return;if(e.activeElement!==f.current)f.current=null;else if(f.current!==null)return;if(!p.current)return;let n=[];if((e.activeElement===l.current||e.activeElement===u.current)&&(n=a(m.current)),n.length>0){let e=!!(g.current?.shiftKey&&g.current?.key===`Tab`),t=n[0],r=n[n.length-1];typeof t!=`string`&&typeof r!=`string`&&(e?r.focus():t.focus())}else t.focus()};e.addEventListener(`focusin`,n),e.addEventListener(`keydown`,t,!0);let i=setInterval(()=>{e.activeElement&&e.activeElement.tagName===`BODY`&&n()},50);return()=>{clearInterval(i),e.removeEventListener(`focusin`,n),e.removeEventListener(`keydown`,t,!0)}},[n,r,i,o,s,a]);let _=e=>{d.current===null&&(d.current=e.relatedTarget),p.current=!0,f.current=e.target;let n=t.props.onFocus;n&&n(e)},v=e=>{d.current===null&&(d.current=e.relatedTarget),p.current=!0};return(0,I.jsxs)(x.Fragment,{children:[(0,I.jsx)(`div`,{tabIndex:s?0:-1,onFocus:v,ref:l,"data-testid":`sentinelStart`}),x.cloneElement(t,{ref:h,onFocus:_}),(0,I.jsx)(`div`,{tabIndex:s?0:-1,onFocus:v,ref:u,"data-testid":`sentinelEnd`})]})}var Gh=Wh;function Kh(e){return typeof e==`function`?e():e}function qh(e){return e?e.props.hasOwnProperty(`in`):!1}var Jh=()=>{},Yh=new Lh;function Xh(e){let{container:t,disableEscapeKeyDown:n=!1,disableScrollLock:r=!1,closeAfterTransition:i=!1,onTransitionEnter:a,onTransitionExited:o,children:s,onClose:c,open:l,rootRef:u}=e,d=x.useRef({}),f=x.useRef(null),p=x.useRef(null),m=Ql(p,u),[h,g]=x.useState(!l),_=qh(s),v=!0;(e[`aria-hidden`]===`false`||e[`aria-hidden`]===!1)&&(v=!1);let y=()=>Hl(f.current),b=()=>(d.current.modalRef=p.current,d.current.mount=f.current,d.current),S=()=>{Yh.mount(b(),{disableScrollLock:r}),p.current&&(p.current.scrollTop=0)},C=Xl(()=>{let e=Kh(t)||y().body;Yh.add(b(),e),p.current&&S()}),w=()=>Yh.isTopModal(b()),T=Xl(e=>{f.current=e,e&&(l&&w()?S():p.current&&Ah(p.current,v))}),E=x.useCallback(()=>{Yh.remove(b(),v)},[v]);x.useEffect(()=>()=>{E()},[E]),x.useEffect(()=>{l?C():(!_||!i)&&E()},[l,E,_,i,C]);let D=e=>t=>{e.onKeyDown?.(t),!(t.key!==`Escape`||t.which===229||!w())&&(n||(t.stopPropagation(),c&&c(t,`escapeKeyDown`)))},O=e=>t=>{e.onClick?.(t),t.target===t.currentTarget&&c&&c(t,`backdropClick`)};return{getRootProps:(t={})=>{let n=Bu(e);delete n.onTransitionEnter,delete n.onTransitionExited;let r={...n,...t};return{role:`presentation`,...r,onKeyDown:D(r),ref:m}},getBackdropProps:(e={})=>{let t=e;return{"aria-hidden":!0,...t,onClick:O(t),open:l}},getTransitionProps:()=>({onEnter:Dl(()=>{g(!1),a&&a()},s?.props.onEnter??Jh),onExited:Dl(()=>{g(!0),o&&o(),i&&E()},s?.props.onExited??Jh)}),rootRef:m,portalRef:T,isTopModal:w,exited:h,hasTransition:_}}var Zh=Xh;function Qh(e){return zo(`MuiModal`,e)}Bo(`MuiModal`,[`root`,`hidden`,`backdrop`]);var $h=e=>{let{open:t,exited:n,classes:r}=e;return dc({root:[`root`,!t&&n&&`hidden`],backdrop:[`backdrop`]},Qh,r)},eg=B(`div`,{name:`MuiModal`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.open&&n.exited&&t.hidden]}})(Ml(({theme:e})=>({position:`fixed`,zIndex:(e.vars||e).zIndex.modal,right:0,bottom:0,top:0,left:0,variants:[{props:({ownerState:e})=>!e.open&&e.exited,style:{visibility:`hidden`}}]}))),tg=B(ah,{name:`MuiModal`,slot:`Backdrop`})({zIndex:-1}),ng=x.forwardRef(function(e,t){let n=Nl({name:`MuiModal`,props:e}),{BackdropComponent:r=tg,BackdropProps:i,classes:a,className:o,closeAfterTransition:s=!1,children:c,container:l,component:u,components:d={},componentsProps:f={},disableAutoFocus:p=!1,disableEnforceFocus:m=!1,disableEscapeKeyDown:h=!1,disablePortal:g=!1,disableRestoreFocus:_=!1,disableScrollLock:v=!1,hideBackdrop:y=!1,keepMounted:b=!1,onClose:S,onTransitionEnter:C,onTransitionExited:w,open:T,slotProps:E={},slots:D={},theme:O,...k}=n,ee={...n,closeAfterTransition:s,disableAutoFocus:p,disableEnforceFocus:m,disableEscapeKeyDown:h,disablePortal:g,disableRestoreFocus:_,disableScrollLock:v,hideBackdrop:y,keepMounted:b},{getRootProps:A,getBackdropProps:j,getTransitionProps:M,portalRef:te,isTopModal:N,exited:P,hasTransition:F}=Zh({...ee,rootRef:t}),ne={...ee,exited:P},re=$h(ne),ie={};if(c.props.tabIndex===void 0&&(ie.tabIndex=`-1`),F){let{onEnter:e,onExited:t}=M();ie.onEnter=e,ie.onExited=t}let ae={slots:{root:d.Root,backdrop:d.Backdrop,...D},slotProps:{...f,...E}},[oe,se]=Gu(`root`,{ref:t,elementType:eg,externalForwardedProps:{...ae,...k,component:u},getSlotProps:A,ownerState:ne,className:R(o,re?.root,!ne.open&&ne.exited&&re?.hidden)}),[ce,le]=Gu(`backdrop`,{ref:i?.ref,elementType:r,externalForwardedProps:ae,shouldForwardComponentProp:!0,additionalProps:i,getSlotProps:e=>j({...e,onClick:t=>{e?.onClick&&e.onClick(t)}}),className:R(i?.className,re?.backdrop),ownerState:ne});return!b&&!T&&(!F||P)?null:(0,I.jsx)(gm,{ref:te,container:l,disablePortal:g,children:(0,I.jsxs)(oe,{...se,children:[!y&&r?(0,I.jsx)(ce,{...le}):null,(0,I.jsx)(Gh,{disableEnforceFocus:m,disableAutoFocus:p,disableRestoreFocus:_,isEnabled:N,open:T,children:x.cloneElement(c,ie)})]})})});function rg(e){return zo(`MuiDialog`,e)}var ig=Bo(`MuiDialog`,[`root`,`scrollPaper`,`scrollBody`,`container`,`paper`,`paperScrollPaper`,`paperScrollBody`,`paperWidthFalse`,`paperWidthXs`,`paperWidthSm`,`paperWidthMd`,`paperWidthLg`,`paperWidthXl`,`paperFullWidth`,`paperFullScreen`]),ag=x.createContext({}),og=B(ah,{name:`MuiDialog`,slot:`Backdrop`,overrides:(e,t)=>t.backdrop})({zIndex:-1}),sg=e=>{let{classes:t,scroll:n,maxWidth:r,fullWidth:i,fullScreen:a}=e;return dc({root:[`root`],container:[`container`,`scroll${V(n)}`],paper:[`paper`,`paperScroll${V(n)}`,`paperWidth${V(String(r))}`,i&&`paperFullWidth`,a&&`paperFullScreen`]},rg,t)},cg=B(ng,{name:`MuiDialog`,slot:`Root`})({"@media print":{position:`absolute !important`}}),lg=B(`div`,{name:`MuiDialog`,slot:`Container`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.container,t[`scroll${V(n.scroll)}`]]}})({height:`100%`,"@media print":{height:`auto`},outline:0,variants:[{props:{scroll:`paper`},style:{display:`flex`,justifyContent:`center`,alignItems:`center`}},{props:{scroll:`body`},style:{overflowY:`auto`,overflowX:`hidden`,textAlign:`center`,"&::after":{content:`""`,display:`inline-block`,verticalAlign:`middle`,height:`100%`,width:`0`}}}]}),ug=B(nd,{name:`MuiDialog`,slot:`Paper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.paper,t[`scrollPaper${V(n.scroll)}`],t[`paperWidth${V(String(n.maxWidth))}`],n.fullWidth&&t.paperFullWidth,n.fullScreen&&t.paperFullScreen]}})(Ml(({theme:e})=>({margin:32,position:`relative`,overflowY:`auto`,"@media print":{overflowY:`visible`,boxShadow:`none`},variants:[{props:{scroll:`paper`},style:{display:`flex`,flexDirection:`column`,maxHeight:`calc(100% - 64px)`}},{props:{scroll:`body`},style:{display:`inline-block`,verticalAlign:`middle`,textAlign:`initial`}},{props:({ownerState:e})=>!e.maxWidth,style:{maxWidth:`calc(100% - 64px)`}},{props:{maxWidth:`xs`},style:{maxWidth:e.breakpoints.unit===`px`?Math.max(e.breakpoints.values.xs,444):`max(${e.breakpoints.values.xs}${e.breakpoints.unit}, 444px)`,[`&.${ig.paperScrollBody}`]:{[e.breakpoints.down(Math.max(e.breakpoints.values.xs,444)+64)]:{maxWidth:`calc(100% - 64px)`}}}},...Object.keys(e.breakpoints.values).filter(e=>e!==`xs`).map(t=>({props:{maxWidth:t},style:{maxWidth:`${e.breakpoints.values[t]}${e.breakpoints.unit}`,[`&.${ig.paperScrollBody}`]:{[e.breakpoints.down(e.breakpoints.values[t]+64)]:{maxWidth:`calc(100% - 64px)`}}}})),{props:({ownerState:e})=>e.fullWidth,style:{width:`calc(100% - 64px)`}},{props:({ownerState:e})=>e.fullScreen,style:{margin:0,width:`100%`,maxWidth:`100%`,height:`100%`,maxHeight:`none`,borderRadius:0,[`&.${ig.paperScrollBody}`]:{margin:0,maxWidth:`100%`}}}]}))),dg=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiDialog`}),r=gl(),i={enter:r.transitions.duration.enteringScreen,exit:r.transitions.duration.leavingScreen},{"aria-describedby":a,"aria-labelledby":o,"aria-modal":s=!0,BackdropComponent:c,BackdropProps:l,children:u,className:d,disableEscapeKeyDown:f=!1,fullScreen:p=!1,fullWidth:m=!1,maxWidth:h=`sm`,onClick:g,onClose:_,open:v,PaperComponent:y=nd,PaperProps:b={},scroll:S=`paper`,slots:C={},slotProps:w={},TransitionComponent:T=th,transitionDuration:E=i,TransitionProps:D,...O}=n,k={...n,disableEscapeKeyDown:f,fullScreen:p,fullWidth:m,maxWidth:h,scroll:S},ee=sg(k),A=x.useRef(),j=e=>{A.current=e.target===e.currentTarget},M=e=>{g&&g(e),A.current&&(A.current=null,_&&_(e,`backdropClick`))},te=Rs(o),N=x.useMemo(()=>({titleId:te}),[te]),P={slots:{transition:T,...C},slotProps:{transition:D,paper:b,backdrop:l,...w}},[F,ne]=Gu(`root`,{elementType:cg,shouldForwardComponentProp:!0,externalForwardedProps:P,ownerState:k,className:R(ee.root,d),ref:t}),[re,ie]=Gu(`backdrop`,{elementType:og,shouldForwardComponentProp:!0,externalForwardedProps:P,ownerState:k}),[ae,oe]=Gu(`paper`,{elementType:ug,shouldForwardComponentProp:!0,externalForwardedProps:P,ownerState:k,className:R(ee.paper,b.className)}),[se,ce]=Gu(`container`,{elementType:lg,externalForwardedProps:P,ownerState:k,className:ee.container}),[le,ue]=Gu(`transition`,{elementType:th,externalForwardedProps:P,ownerState:k,additionalProps:{appear:!0,in:v,timeout:E,role:`presentation`}});return(0,I.jsx)(F,{closeAfterTransition:!0,slots:{backdrop:re},slotProps:{backdrop:{transitionDuration:E,as:c,...ie}},disableEscapeKeyDown:f,onClose:_,open:v,onClick:M,...ne,...O,children:(0,I.jsx)(le,{...ue,children:(0,I.jsx)(se,{onMouseDown:j,...ce,children:(0,I.jsx)(ae,{as:y,elevation:24,role:`dialog`,"aria-describedby":a,"aria-labelledby":te,"aria-modal":s,...oe,children:(0,I.jsx)(ag.Provider,{value:N,children:u})})})})})});function fg(e){return zo(`MuiDialogContent`,e)}Bo(`MuiDialogContent`,[`root`,`dividers`]);var pg=Bo(`MuiDialogTitle`,[`root`]),mg=e=>{let{classes:t,dividers:n}=e;return dc({root:[`root`,n&&`dividers`]},fg,t)},hg=B(`div`,{name:`MuiDialogContent`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.dividers&&t.dividers]}})(Ml(({theme:e})=>({flex:`1 1 auto`,WebkitOverflowScrolling:`touch`,overflowY:`auto`,padding:`20px 24px`,variants:[{props:({ownerState:e})=>e.dividers,style:{padding:`16px 24px`,borderTop:`1px solid ${(e.vars||e).palette.divider}`,borderBottom:`1px solid ${(e.vars||e).palette.divider}`}},{props:({ownerState:e})=>!e.dividers,style:{[`.${pg.root} + &`]:{paddingTop:0}}}]}))),gg=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiDialogContent`}),{className:r,dividers:i=!1,...a}=n,o={...n,dividers:i};return(0,I.jsx)(hg,{className:R(mg(o).root,r),ownerState:o,ref:t,...a})});function _g(e){return zo(`MuiDivider`,e)}Bo(`MuiDivider`,[`root`,`absolute`,`fullWidth`,`inset`,`middle`,`flexItem`,`light`,`vertical`,`withChildren`,`withChildrenVertical`,`textAlignRight`,`textAlignLeft`,`wrapper`,`wrapperVertical`]);var vg=e=>{let{absolute:t,children:n,classes:r,flexItem:i,light:a,orientation:o,textAlign:s,variant:c}=e;return dc({root:[`root`,t&&`absolute`,c,a&&`light`,o===`vertical`&&`vertical`,i&&`flexItem`,n&&`withChildren`,n&&o===`vertical`&&`withChildrenVertical`,s===`right`&&o!==`vertical`&&`textAlignRight`,s===`left`&&o!==`vertical`&&`textAlignLeft`],wrapper:[`wrapper`,o===`vertical`&&`wrapperVertical`]},_g,r)},yg=B(`div`,{name:`MuiDivider`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.absolute&&t.absolute,t[n.variant],n.light&&t.light,n.orientation===`vertical`&&t.vertical,n.flexItem&&t.flexItem,n.children&&t.withChildren,n.children&&n.orientation===`vertical`&&t.withChildrenVertical,n.textAlign===`right`&&n.orientation!==`vertical`&&t.textAlignRight,n.textAlign===`left`&&n.orientation!==`vertical`&&t.textAlignLeft]}})(Ml(({theme:e})=>({margin:0,flexShrink:0,borderWidth:0,borderStyle:`solid`,borderColor:(e.vars||e).palette.divider,borderBottomWidth:`thin`,variants:[{props:{absolute:!0},style:{position:`absolute`,bottom:0,left:0,width:`100%`}},{props:{light:!0},style:{borderColor:e.alpha((e.vars||e).palette.divider,.08)}},{props:{variant:`inset`},style:{marginLeft:72}},{props:{variant:`middle`,orientation:`horizontal`},style:{marginLeft:e.spacing(2),marginRight:e.spacing(2)}},{props:{variant:`middle`,orientation:`vertical`},style:{marginTop:e.spacing(1),marginBottom:e.spacing(1)}},{props:{orientation:`vertical`},style:{height:`100%`,borderBottomWidth:0,borderRightWidth:`thin`}},{props:{flexItem:!0},style:{alignSelf:`stretch`,height:`auto`}},{props:({ownerState:e})=>!!e.children,style:{display:`flex`,textAlign:`center`,border:0,borderTopStyle:`solid`,borderLeftStyle:`solid`,"&::before, &::after":{content:`""`,alignSelf:`center`}}},{props:({ownerState:e})=>e.children&&e.orientation!==`vertical`,style:{"&::before, &::after":{width:`100%`,borderTop:`thin solid ${(e.vars||e).palette.divider}`,borderTopStyle:`inherit`}}},{props:({ownerState:e})=>e.orientation===`vertical`&&e.children,style:{flexDirection:`column`,"&::before, &::after":{height:`100%`,borderLeft:`thin solid ${(e.vars||e).palette.divider}`,borderLeftStyle:`inherit`}}},{props:({ownerState:e})=>e.textAlign===`right`&&e.orientation!==`vertical`,style:{"&::before":{width:`90%`},"&::after":{width:`10%`}}},{props:({ownerState:e})=>e.textAlign===`left`&&e.orientation!==`vertical`,style:{"&::before":{width:`10%`},"&::after":{width:`90%`}}}]}))),bg=B(`span`,{name:`MuiDivider`,slot:`Wrapper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.wrapper,n.orientation===`vertical`&&t.wrapperVertical]}})(Ml(({theme:e})=>({display:`inline-block`,paddingLeft:`calc(${e.spacing(1)} * 1.2)`,paddingRight:`calc(${e.spacing(1)} * 1.2)`,whiteSpace:`nowrap`,variants:[{props:{orientation:`vertical`},style:{paddingTop:`calc(${e.spacing(1)} * 1.2)`,paddingBottom:`calc(${e.spacing(1)} * 1.2)`}}]}))),xg=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiDivider`}),{absolute:r=!1,children:i,className:a,orientation:o=`horizontal`,component:s=i||o===`vertical`?`div`:`hr`,flexItem:c=!1,light:l=!1,role:u=s===`hr`?void 0:`separator`,textAlign:d=`center`,variant:f=`fullWidth`,...p}=n,m={...n,absolute:r,component:s,flexItem:c,light:l,orientation:o,role:u,textAlign:d,variant:f},h=vg(m);return(0,I.jsx)(yg,{as:s,className:R(h.root,a),role:u,ref:t,ownerState:m,"aria-orientation":u===`separator`&&(s!==`hr`||o===`vertical`)?o:void 0,...p,children:i?(0,I.jsx)(bg,{className:h.wrapper,ownerState:m,children:i}):null})});xg&&(xg.muiSkipListHighlight=!0);var Sg=xg;function Cg(e){return`scale(${e}, ${e**2})`}var wg={entering:{opacity:1,transform:Cg(1)},entered:{opacity:1,transform:`none`}},Tg=typeof navigator<`u`&&/^((?!chrome|android).)*(safari|mobile)/i.test(navigator.userAgent)&&/(os |version\/)15(.|_)4/i.test(navigator.userAgent),Eg=x.forwardRef(function(e,t){let{addEndListener:n,appear:r=!0,children:i,easing:a,in:o,onEnter:s,onEntered:c,onEntering:l,onExit:u,onExited:d,onExiting:f,style:p,timeout:m=`auto`,TransitionComponent:h=mu,...g}=e,_=Au(),v=x.useRef(),y=gl(),b=x.useRef(null),S=$l(b,pm(i),t),C=e=>t=>{if(e){let n=b.current;t===void 0?e(n):e(n,t)}},w=C(l),T=C((e,t)=>{ju(e);let{duration:n,delay:r,easing:i}=Mu({style:p,timeout:m,easing:a},{mode:`enter`}),o;m===`auto`?(o=y.transitions.getAutoHeightDuration(e.clientHeight),v.current=o):o=n,e.style.transition=[y.transitions.create(`opacity`,{duration:o,delay:r}),y.transitions.create(`transform`,{duration:Tg?o:o*.666,delay:r,easing:i})].join(`,`),s&&s(e,t)}),E=C(c),D=C(f),O=C(e=>{let{duration:t,delay:n,easing:r}=Mu({style:p,timeout:m,easing:a},{mode:`exit`}),i;m===`auto`?(i=y.transitions.getAutoHeightDuration(e.clientHeight),v.current=i):i=t,e.style.transition=[y.transitions.create(`opacity`,{duration:i,delay:n}),y.transitions.create(`transform`,{duration:Tg?i:i*.666,delay:Tg?n:n||i*.333,easing:r})].join(`,`),e.style.opacity=0,e.style.transform=Cg(.75),u&&u(e)}),k=C(d);return(0,I.jsx)(h,{appear:r,in:o,nodeRef:b,onEnter:T,onEntered:E,onEntering:w,onExit:O,onExited:k,onExiting:D,addEndListener:e=>{m===`auto`&&_.start(v.current||0,e),n&&n(b.current,e)},timeout:m===`auto`?null:m,...g,children:(e,{ownerState:t,...n})=>x.cloneElement(i,{style:{opacity:0,transform:Cg(.75),visibility:e===`exited`&&!o?`hidden`:void 0,...wg[e],...p,...i.props.style},ref:S,...n})})});Eg&&(Eg.muiSupportAuto=!0);var Dg=Eg;function Og(e){return zo(`MuiLink`,e)}var kg=Bo(`MuiLink`,[`root`,`underlineNone`,`underlineHover`,`underlineAlways`,`button`,`focusVisible`]),Ag=({theme:e,ownerState:t})=>{let n=t.color;if(`colorSpace`in e&&e.colorSpace){let r=ga(e,`palette.${n}.main`)||ga(e,`palette.${n}`)||t.color;return e.alpha(r,.4)}let r=ga(e,`palette.${n}.main`,!1)||ga(e,`palette.${n}`,!1)||t.color,i=ga(e,`palette.${n}.mainChannel`)||ga(e,`palette.${n}Channel`);return`vars`in e&&i?`rgba(${i} / 0.4)`:fs(r,.4)},jg={primary:!0,secondary:!0,error:!0,info:!0,success:!0,warning:!0,textPrimary:!0,textSecondary:!0,textDisabled:!0},Mg=e=>{let{classes:t,component:n,focusVisible:r,underline:i}=e;return dc({root:[`root`,`underline${V(i)}`,n===`button`&&`button`,r&&`focusVisible`]},Og,t)},Ng=B(U,{name:`MuiLink`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[`underline${V(n.underline)}`],n.component===`button`&&t.button]}})(Ml(({theme:e})=>({variants:[{props:{underline:`none`},style:{textDecoration:`none`}},{props:{underline:`hover`},style:{textDecoration:`none`,"&:hover":{textDecoration:`underline`}}},{props:{underline:`always`},style:{textDecoration:`underline`,"&:hover":{textDecorationColor:`inherit`}}},{props:({underline:e,ownerState:t})=>e===`always`&&t.color!==`inherit`,style:{textDecorationColor:`var(--Link-underlineColor)`}},{props:({underline:e,ownerState:t})=>e===`always`&&t.color===`inherit`,style:e.colorSpace?{textDecorationColor:e.alpha(`currentColor`,.4)}:null},...Object.entries(e.palette).filter(Ed()).map(([t])=>({props:{underline:`always`,color:t},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette[t].main,.4)}})),{props:{underline:`always`,color:`textPrimary`},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette.text.primary,.4)}},{props:{underline:`always`,color:`textSecondary`},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette.text.secondary,.4)}},{props:{underline:`always`,color:`textDisabled`},style:{"--Link-underlineColor":(e.vars||e).palette.text.disabled}},{props:{component:`button`},style:{position:`relative`,WebkitTapHighlightColor:`transparent`,backgroundColor:`transparent`,outline:0,border:0,margin:0,borderRadius:0,padding:0,cursor:`pointer`,userSelect:`none`,verticalAlign:`middle`,MozAppearance:`none`,WebkitAppearance:`none`,"&::-moz-focus-inner":{borderStyle:`none`},[`&.${kg.focusVisible}`]:{outline:`auto`}}}]}))),Pg=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiLink`}),r=gl(),{className:i,color:a=`primary`,component:o=`a`,onBlur:s,onFocus:c,TypographyClasses:l,underline:u=`always`,variant:d=`inherit`,sx:f,...p}=n,[m,h]=x.useState(!1),g=e=>{rd(e.target)||h(!1),s&&s(e)},_=e=>{rd(e.target)&&h(!0),c&&c(e)},v={...n,color:a,component:o,focusVisible:m,underline:u,variant:d};return(0,I.jsx)(Ng,{color:a,className:R(Mg(v).root,i),classes:l,component:o,onBlur:g,onFocus:_,ref:t,ownerState:v,variant:d,...p,sx:[...jg[a]===void 0?[{color:a}]:[],...Array.isArray(f)?f:[f]],style:{...p.style,...u===`always`&&a!==`inherit`&&!jg[a]&&{"--Link-underlineColor":Ag({theme:r,ownerState:v})}}})}),Fg=x.createContext({});function Ig(e){return zo(`MuiList`,e)}Bo(`MuiList`,[`root`,`padding`,`dense`,`subheader`]);var Lg=e=>{let{classes:t,disablePadding:n,dense:r,subheader:i}=e;return dc({root:[`root`,!n&&`padding`,r&&`dense`,i&&`subheader`]},Ig,t)},Rg=B(`ul`,{name:`MuiList`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.disablePadding&&t.padding,n.dense&&t.dense,n.subheader&&t.subheader]}})({listStyle:`none`,margin:0,padding:0,position:`relative`,variants:[{props:({ownerState:e})=>!e.disablePadding,style:{paddingTop:8,paddingBottom:8}},{props:({ownerState:e})=>e.subheader,style:{paddingTop:0}}]}),G=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiList`}),{children:r,className:i,component:a=`ul`,dense:o=!1,disablePadding:s=!1,subheader:c,...l}=n,u=x.useMemo(()=>({dense:o}),[o]),d={...n,component:a,dense:o,disablePadding:s},f=Lg(d);return(0,I.jsx)(Fg.Provider,{value:u,children:(0,I.jsxs)(Rg,{as:a,className:R(f.root,i),ref:t,ownerState:d,...l,children:[c,r]})})});function zg(e){return zo(`MuiListItem`,e)}Bo(`MuiListItem`,[`root`,`container`,`dense`,`alignItemsFlexStart`,`divider`,`gutters`,`padding`,`secondaryAction`]);function Bg(e){return zo(`MuiListItemButton`,e)}var Vg=Bo(`MuiListItemButton`,[`root`,`focusVisible`,`dense`,`alignItemsFlexStart`,`disabled`,`divider`,`gutters`,`selected`]);const Hg=(e,t)=>{let{ownerState:n}=e;return[t.root,n.dense&&t.dense,n.alignItems===`flex-start`&&t.alignItemsFlexStart,n.divider&&t.divider,!n.disableGutters&&t.gutters]};var Ug=e=>{let{alignItems:t,classes:n,dense:r,disabled:i,disableGutters:a,divider:o,selected:s}=e,c=dc({root:[`root`,r&&`dense`,!a&&`gutters`,o&&`divider`,i&&`disabled`,t===`flex-start`&&`alignItemsFlexStart`,s&&`selected`]},Bg,n);return{...n,...c}},Wg=B(Cd,{shouldForwardProp:e=>yl(e)||e===`classes`,name:`MuiListItemButton`,slot:`Root`,overridesResolver:Hg})(Ml(({theme:e})=>({display:`flex`,flexGrow:1,justifyContent:`flex-start`,alignItems:`center`,position:`relative`,textDecoration:`none`,minWidth:0,boxSizing:`border-box`,textAlign:`left`,paddingTop:8,paddingBottom:8,transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),"&:hover":{textDecoration:`none`,backgroundColor:(e.vars||e).palette.action.hover,"@media (hover: none)":{backgroundColor:`transparent`}},[`&.${Vg.selected}`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,(e.vars||e).palette.action.selectedOpacity),[`&.${Vg.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)}},[`&.${Vg.selected}:hover`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.hoverOpacity}`),"@media (hover: none)":{backgroundColor:e.alpha((e.vars||e).palette.primary.main,(e.vars||e).palette.action.selectedOpacity)}},[`&.${Vg.focusVisible}`]:{backgroundColor:(e.vars||e).palette.action.focus},[`&.${Vg.disabled}`]:{opacity:(e.vars||e).palette.action.disabledOpacity},variants:[{props:({ownerState:e})=>e.divider,style:{borderBottom:`1px solid ${(e.vars||e).palette.divider}`,backgroundClip:`padding-box`}},{props:{alignItems:`flex-start`},style:{alignItems:`flex-start`}},{props:({ownerState:e})=>!e.disableGutters,style:{paddingLeft:16,paddingRight:16}},{props:({ownerState:e})=>e.dense,style:{paddingTop:4,paddingBottom:4}}]}))),Gg=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiListItemButton`}),{alignItems:r=`center`,autoFocus:i=!1,component:a=`div`,children:o,dense:s=!1,disableGutters:c=!1,divider:l=!1,focusVisibleClassName:u,selected:d=!1,className:f,...p}=n,m=x.useContext(Fg),h=x.useMemo(()=>({dense:s||m.dense||!1,alignItems:r,disableGutters:c}),[r,m.dense,s,c]),g=x.useRef(null);Gl(()=>{i&&g.current&&g.current.focus()},[i]);let _={...n,alignItems:r,dense:h.dense,disableGutters:c,divider:l,selected:d},v=Ug(_),y=$l(g,t);return(0,I.jsx)(Fg.Provider,{value:h,children:(0,I.jsx)(Wg,{ref:y,href:p.href||p.to,component:(p.href||p.to)&&a===`div`?`button`:a,focusVisibleClassName:R(v.focusVisible,u),ownerState:_,className:R(v.root,f),...p,classes:v,children:o})})});function Kg(e){return zo(`MuiListItemSecondaryAction`,e)}Bo(`MuiListItemSecondaryAction`,[`root`,`disableGutters`]);var qg=e=>{let{disableGutters:t,classes:n}=e;return dc({root:[`root`,t&&`disableGutters`]},Kg,n)},Jg=B(`div`,{name:`MuiListItemSecondaryAction`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.disableGutters&&t.disableGutters]}})({position:`absolute`,right:16,top:`50%`,transform:`translateY(-50%)`,variants:[{props:({ownerState:e})=>e.disableGutters,style:{right:0}}]}),Yg=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiListItemSecondaryAction`}),{className:r,...i}=n,a=x.useContext(Fg),o={...n,disableGutters:a.disableGutters};return(0,I.jsx)(Jg,{className:R(qg(o).root,r),ownerState:o,ref:t,...i})});Yg.muiName=`ListItemSecondaryAction`;var Xg=Yg;const Zg=(e,t)=>{let{ownerState:n}=e;return[t.root,n.dense&&t.dense,n.alignItems===`flex-start`&&t.alignItemsFlexStart,n.divider&&t.divider,!n.disableGutters&&t.gutters,!n.disablePadding&&t.padding,n.hasSecondaryAction&&t.secondaryAction]};var Qg=e=>{let{alignItems:t,classes:n,dense:r,disableGutters:i,disablePadding:a,divider:o,hasSecondaryAction:s}=e;return dc({root:[`root`,r&&`dense`,!i&&`gutters`,!a&&`padding`,o&&`divider`,t===`flex-start`&&`alignItemsFlexStart`,s&&`secondaryAction`],container:[`container`]},zg,n)};const $g=B(`div`,{name:`MuiListItem`,slot:`Root`,overridesResolver:Zg})(Ml(({theme:e})=>({display:`flex`,justifyContent:`flex-start`,alignItems:`center`,position:`relative`,textDecoration:`none`,width:`100%`,boxSizing:`border-box`,textAlign:`left`,variants:[{props:({ownerState:e})=>!e.disablePadding,style:{paddingTop:8,paddingBottom:8}},{props:({ownerState:e})=>!e.disablePadding&&e.dense,style:{paddingTop:4,paddingBottom:4}},{props:({ownerState:e})=>!e.disablePadding&&!e.disableGutters,style:{paddingLeft:16,paddingRight:16}},{props:({ownerState:e})=>!e.disablePadding&&!!e.secondaryAction,style:{paddingRight:48}},{props:({ownerState:e})=>!!e.secondaryAction,style:{[`& > .${Vg.root}`]:{paddingRight:48}}},{props:{alignItems:`flex-start`},style:{alignItems:`flex-start`}},{props:({ownerState:e})=>e.divider,style:{borderBottom:`1px solid ${(e.vars||e).palette.divider}`,backgroundClip:`padding-box`}},{props:({ownerState:e})=>e.button,style:{transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),"&:hover":{textDecoration:`none`,backgroundColor:(e.vars||e).palette.action.hover,"@media (hover: none)":{backgroundColor:`transparent`}}}},{props:({ownerState:e})=>e.hasSecondaryAction,style:{paddingRight:48}}]})));var e_=B(`li`,{name:`MuiListItem`,slot:`Container`})({position:`relative`}),K=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiListItem`}),{alignItems:r=`center`,children:i,className:a,component:o,components:s={},componentsProps:c={},ContainerComponent:l=`li`,ContainerProps:{className:u,...d}={},dense:f=!1,disableGutters:p=!1,disablePadding:m=!1,divider:h=!1,secondaryAction:g,slotProps:_={},slots:v={},...y}=n,b=x.useContext(Fg),S=x.useMemo(()=>({dense:f||b.dense||!1,alignItems:r,disableGutters:p}),[r,b.dense,f,p]),C=x.useRef(null),w=x.Children.toArray(i),T=w.length&&Vl(w[w.length-1],[`ListItemSecondaryAction`]),E={...n,alignItems:r,dense:S.dense,disableGutters:p,disablePadding:m,divider:h,hasSecondaryAction:T},D=Qg(E),O=$l(C,t),k=v.root||s.Root||$g,ee=_.root||c.root||{},A={className:R(D.root,ee.className,a),...y},j=o||`li`;return T?(j=!A.component&&!o?`div`:j,l===`li`&&(j===`li`?j=`div`:A.component===`li`&&(A.component=`div`)),(0,I.jsx)(Fg.Provider,{value:S,children:(0,I.jsxs)(e_,{as:l,className:R(D.container,u),ref:O,ownerState:E,...d,children:[(0,I.jsx)(k,{...ee,...!Pu(k)&&{as:j,ownerState:{...E,...ee.ownerState}},...A,children:w}),w.pop()]})})):(0,I.jsx)(Fg.Provider,{value:S,children:(0,I.jsxs)(k,{...ee,as:j,ref:O,...!Pu(k)&&{ownerState:{...E,...ee.ownerState}},...A,children:[w,g&&(0,I.jsx)(Xg,{children:g})]})})});function t_(e){return zo(`MuiListItemText`,e)}var n_=Bo(`MuiListItemText`,[`root`,`multiline`,`dense`,`inset`,`primary`,`secondary`]),r_=e=>{let{classes:t,inset:n,primary:r,secondary:i,dense:a}=e;return dc({root:[`root`,n&&`inset`,a&&`dense`,r&&i&&`multiline`],primary:[`primary`],secondary:[`secondary`]},t_,t)},i_=B(`div`,{name:`MuiListItemText`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[{[`& .${n_.primary}`]:t.primary},{[`& .${n_.secondary}`]:t.secondary},t.root,n.inset&&t.inset,n.primary&&n.secondary&&t.multiline,n.dense&&t.dense]}})({flex:`1 1 auto`,minWidth:0,marginTop:4,marginBottom:4,[`.${of.root}:where(& .${n_.primary})`]:{display:`block`},[`.${of.root}:where(& .${n_.secondary})`]:{display:`block`},variants:[{props:({ownerState:e})=>e.primary&&e.secondary,style:{marginTop:6,marginBottom:6}},{props:({ownerState:e})=>e.inset,style:{paddingLeft:56}}]}),q=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiListItemText`}),{children:r,className:i,disableTypography:a=!1,inset:o=!1,primary:s,primaryTypographyProps:c,secondary:l,secondaryTypographyProps:u,slots:d={},slotProps:f={},...p}=n,{dense:m}=x.useContext(Fg),h=s??r,g=l,_={...n,disableTypography:a,inset:o,primary:!!h,secondary:!!g,dense:m},v=r_(_),y={slots:d,slotProps:{primary:c,secondary:u,...f}},[b,S]=Gu(`root`,{className:R(v.root,i),elementType:i_,externalForwardedProps:{...y,...p},ownerState:_,ref:t}),[C,w]=Gu(`primary`,{className:v.primary,elementType:U,externalForwardedProps:y,ownerState:_}),[T,E]=Gu(`secondary`,{className:v.secondary,elementType:U,externalForwardedProps:y,ownerState:_});return h!=null&&h.type!==U&&!a&&(h=(0,I.jsx)(C,{variant:m?`body2`:`body1`,component:w?.variant?void 0:`span`,...w,children:h})),g!=null&&g.type!==U&&!a&&(g=(0,I.jsx)(T,{variant:`body2`,color:`textSecondary`,...E,children:g})),(0,I.jsxs)(b,{...S,children:[h,g]})});function a_(e){return zo(`MuiTooltip`,e)}var o_=Bo(`MuiTooltip`,[`popper`,`popperInteractive`,`popperArrow`,`popperClose`,`tooltip`,`tooltipArrow`,`touch`,`tooltipPlacementLeft`,`tooltipPlacementRight`,`tooltipPlacementTop`,`tooltipPlacementBottom`,`arrow`]);function s_(e){return Math.round(e*1e5)/1e5}var c_=e=>{let{classes:t,disableInteractive:n,arrow:r,touch:i,placement:a}=e;return dc({popper:[`popper`,!n&&`popperInteractive`,r&&`popperArrow`],tooltip:[`tooltip`,r&&`tooltipArrow`,i&&`touch`,`tooltipPlacement${V(a.split(`-`)[0])}`],arrow:[`arrow`]},a_,t)},l_=B(Tm,{name:`MuiTooltip`,slot:`Popper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.popper,!n.disableInteractive&&t.popperInteractive,n.arrow&&t.popperArrow,!n.open&&t.popperClose]}})(Ml(({theme:e})=>({zIndex:(e.vars||e).zIndex.tooltip,pointerEvents:`none`,variants:[{props:({ownerState:e})=>!e.disableInteractive,style:{pointerEvents:`auto`}},{props:({open:e})=>!e,style:{pointerEvents:`none`}},{props:({ownerState:e})=>e.arrow,style:{[`&[data-popper-placement*="bottom"] .${o_.arrow}`]:{top:0,marginTop:`-0.71em`,"&::before":{transformOrigin:`0 100%`}},[`&[data-popper-placement*="top"] .${o_.arrow}`]:{bottom:0,marginBottom:`-0.71em`,"&::before":{transformOrigin:`100% 0`}},[`&[data-popper-placement*="right"] .${o_.arrow}`]:{height:`1em`,width:`0.71em`,"&::before":{transformOrigin:`100% 100%`}},[`&[data-popper-placement*="left"] .${o_.arrow}`]:{height:`1em`,width:`0.71em`,"&::before":{transformOrigin:`0 0`}}}},{props:({ownerState:e})=>e.arrow&&!e.isRtl,style:{[`&[data-popper-placement*="right"] .${o_.arrow}`]:{left:0,marginLeft:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!!e.isRtl,style:{[`&[data-popper-placement*="right"] .${o_.arrow}`]:{right:0,marginRight:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!e.isRtl,style:{[`&[data-popper-placement*="left"] .${o_.arrow}`]:{right:0,marginRight:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!!e.isRtl,style:{[`&[data-popper-placement*="left"] .${o_.arrow}`]:{left:0,marginLeft:`-0.71em`}}}]}))),u_=B(`div`,{name:`MuiTooltip`,slot:`Tooltip`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.tooltip,n.touch&&t.touch,n.arrow&&t.tooltipArrow,t[`tooltipPlacement${V(n.placement.split(`-`)[0])}`]]}})(Ml(({theme:e})=>({backgroundColor:e.vars?e.vars.palette.Tooltip.bg:e.alpha(e.palette.grey[700],.92),borderRadius:(e.vars||e).shape.borderRadius,color:(e.vars||e).palette.common.white,fontFamily:e.typography.fontFamily,padding:`4px 8px`,fontSize:e.typography.pxToRem(11),maxWidth:300,margin:2,wordWrap:`break-word`,fontWeight:e.typography.fontWeightMedium,[`.${o_.popper}[data-popper-placement*="left"] &`]:{transformOrigin:`right center`},[`.${o_.popper}[data-popper-placement*="right"] &`]:{transformOrigin:`left center`},[`.${o_.popper}[data-popper-placement*="top"] &`]:{transformOrigin:`center bottom`,marginBottom:`14px`},[`.${o_.popper}[data-popper-placement*="bottom"] &`]:{transformOrigin:`center top`,marginTop:`14px`},variants:[{props:({ownerState:e})=>e.arrow,style:{position:`relative`,margin:0}},{props:({ownerState:e})=>e.touch,style:{padding:`8px 16px`,fontSize:e.typography.pxToRem(14),lineHeight:`${s_(16/14)}em`,fontWeight:e.typography.fontWeightRegular}},{props:({ownerState:e})=>!e.isRtl,style:{[`.${o_.popper}[data-popper-placement*="left"] &`]:{marginRight:`14px`},[`.${o_.popper}[data-popper-placement*="right"] &`]:{marginLeft:`14px`}}},{props:({ownerState:e})=>!e.isRtl&&e.touch,style:{[`.${o_.popper}[data-popper-placement*="left"] &`]:{marginRight:`24px`},[`.${o_.popper}[data-popper-placement*="right"] &`]:{marginLeft:`24px`}}},{props:({ownerState:e})=>!!e.isRtl,style:{[`.${o_.popper}[data-popper-placement*="left"] &`]:{marginLeft:`14px`},[`.${o_.popper}[data-popper-placement*="right"] &`]:{marginRight:`14px`}}},{props:({ownerState:e})=>!!e.isRtl&&e.touch,style:{[`.${o_.popper}[data-popper-placement*="left"] &`]:{marginLeft:`24px`},[`.${o_.popper}[data-popper-placement*="right"] &`]:{marginRight:`24px`}}},{props:({ownerState:e})=>e.touch,style:{[`.${o_.popper}[data-popper-placement*="top"] &`]:{marginBottom:`24px`}}},{props:({ownerState:e})=>e.touch,style:{[`.${o_.popper}[data-popper-placement*="bottom"] &`]:{marginTop:`24px`}}}]}))),d_=B(`span`,{name:`MuiTooltip`,slot:`Arrow`})(Ml(({theme:e})=>({overflow:`hidden`,position:`absolute`,width:`1em`,height:`0.71em`,boxSizing:`border-box`,color:e.vars?e.vars.palette.Tooltip.bg:e.alpha(e.palette.grey[700],.9),"&::before":{content:`""`,margin:`auto`,display:`block`,width:`100%`,height:`100%`,backgroundColor:`currentColor`,transform:`rotate(45deg)`}}))),f_=!1,p_=new ku,m_={x:0,y:0};function h_(e,t){return(n,...r)=>{t&&t(n,...r),e(n,...r)}}var g_=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiTooltip`}),{arrow:r=!1,children:i,classes:a,components:o={},componentsProps:s={},describeChild:c=!1,disableFocusListener:l=!1,disableHoverListener:u=!1,disableInteractive:d=!1,disableTouchListener:f=!1,enterDelay:p=100,enterNextDelay:m=0,enterTouchDelay:h=700,followCursor:g=!1,id:_,leaveDelay:v=0,leaveTouchDelay:y=1500,onClose:b,onOpen:S,open:C,placement:w=`bottom`,PopperComponent:T,PopperProps:E={},slotProps:D={},slots:O={},title:k,TransitionComponent:ee,TransitionProps:A,...j}=n,M=x.isValidElement(i)?i:(0,I.jsx)(`span`,{children:i}),te=gl(),N=Os(),[P,F]=x.useState(),[ne,re]=x.useState(null),ie=x.useRef(!1),ae=d||g,oe=Au(),se=Au(),ce=Au(),le=Au(),[ue,de]=Jl({controlled:C,default:!1,name:`Tooltip`,state:`open`}),fe=ue,pe=Kl(_),me=x.useRef(),he=Zl(()=>{me.current!==void 0&&(document.body.style.WebkitUserSelect=me.current,me.current=void 0),le.clear()});x.useEffect(()=>he,[he]);let ge=e=>{p_.clear(),f_=!0,de(!0),S&&!fe&&S(e)},_e=Zl(e=>{p_.start(800+v,()=>{f_=!1}),de(!1),b&&fe&&b(e),oe.start(te.transitions.duration.shortest,()=>{ie.current=!1})}),ve=e=>{ie.current&&e.type!==`touchstart`||(P&&P.removeAttribute(`title`),se.clear(),ce.clear(),p||f_&&m?se.start(f_?m:p,()=>{ge(e)}):ge(e))},ye=e=>{se.clear(),ce.start(v,()=>{_e(e)})},[,be]=x.useState(!1),xe=e=>{rd(e.target)||(be(!1),ye(e))},Se=e=>{P||F(e.currentTarget),rd(e.target)&&(be(!0),ve(e))},Ce=e=>{ie.current=!0;let t=M.props;t.onTouchStart&&t.onTouchStart(e)},we=e=>{Ce(e),ce.clear(),oe.clear(),he(),me.current=document.body.style.WebkitUserSelect,document.body.style.WebkitUserSelect=`none`,le.start(h,()=>{document.body.style.WebkitUserSelect=me.current,ve(e)})},Te=e=>{M.props.onTouchEnd&&M.props.onTouchEnd(e),he(),ce.start(y,()=>{_e(e)})};x.useEffect(()=>{if(!fe)return;function e(e){e.key===`Escape`&&_e(e)}return document.addEventListener(`keydown`,e),()=>{document.removeEventListener(`keydown`,e)}},[_e,fe]);let Ee=$l(pm(M),F,t);!k&&k!==0&&(fe=!1);let De=x.useRef(),Oe=e=>{let t=M.props;t.onMouseMove&&t.onMouseMove(e),m_={x:e.clientX,y:e.clientY},De.current&&De.current.update()},ke={},Ae=typeof k==`string`;c?(ke.title=!fe&&Ae&&!u?k:null,ke[`aria-describedby`]=fe?pe:null):(ke[`aria-label`]=Ae?k:null,ke[`aria-labelledby`]=fe&&!Ae?pe:null);let je={...ke,...j,...M.props,className:R(j.className,M.props.className),onTouchStart:Ce,ref:Ee,...g?{onMouseMove:Oe}:{}},Me={};f||(je.onTouchStart=we,je.onTouchEnd=Te),u||(je.onMouseOver=h_(ve,je.onMouseOver),je.onMouseLeave=h_(ye,je.onMouseLeave),ae||(Me.onMouseOver=ve,Me.onMouseLeave=ye)),l||(je.onFocus=h_(Se,je.onFocus),je.onBlur=h_(xe,je.onBlur),ae||(Me.onFocus=Se,Me.onBlur=xe));let Ne={...n,isRtl:N,arrow:r,disableInteractive:ae,placement:w,PopperComponentProp:T,touch:ie.current},Pe=typeof D.popper==`function`?D.popper(Ne):D.popper,Fe=x.useMemo(()=>{let e=[{name:`arrow`,enabled:!!ne,options:{element:ne,padding:4}}];return E.popperOptions?.modifiers&&(e=e.concat(E.popperOptions.modifiers)),Pe?.popperOptions?.modifiers&&(e=e.concat(Pe.popperOptions.modifiers)),{...E.popperOptions,...Pe?.popperOptions,modifiers:e}},[ne,E.popperOptions,Pe?.popperOptions]),Ie=c_(Ne),Le=typeof D.transition==`function`?D.transition(Ne):D.transition,Re={slots:{popper:o.Popper,transition:o.Transition??ee,tooltip:o.Tooltip,arrow:o.Arrow,...O},slotProps:{arrow:D.arrow??s.arrow,popper:{...E,...Pe??s.popper},tooltip:D.tooltip??s.tooltip,transition:{...A,...Le??s.transition}}},[ze,Be]=Gu(`popper`,{elementType:l_,externalForwardedProps:Re,ownerState:Ne,className:R(Ie.popper,E?.className)}),[Ve,He]=Gu(`transition`,{elementType:Dg,externalForwardedProps:Re,ownerState:Ne}),[Ue,We]=Gu(`tooltip`,{elementType:u_,className:Ie.tooltip,externalForwardedProps:Re,ownerState:Ne}),[Ge,Ke]=Gu(`arrow`,{elementType:d_,className:Ie.arrow,externalForwardedProps:Re,ownerState:Ne,ref:re});return(0,I.jsxs)(x.Fragment,{children:[x.cloneElement(M,je),(0,I.jsx)(ze,{as:T??Tm,placement:w,anchorEl:g?{getBoundingClientRect:()=>({top:m_.y,left:m_.x,right:m_.x,bottom:m_.y,width:0,height:0})}:P,popperRef:De,open:P?fe:!1,id:pe,transition:!0,...Me,...Be,popperOptions:Fe,children:({TransitionProps:e})=>(0,I.jsx)(Ve,{timeout:te.transitions.duration.shorter,...e,...He,children:(0,I.jsxs)(Ue,{...We,children:[k,r?(0,I.jsx)(Ge,{...Ke}):null]})})})]})}),__=x.createContext();function v_(e){return zo(`MuiTable`,e)}Bo(`MuiTable`,[`root`,`stickyHeader`]);var y_=e=>{let{classes:t,stickyHeader:n}=e;return dc({root:[`root`,n&&`stickyHeader`]},v_,t)},b_=B(`table`,{name:`MuiTable`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.stickyHeader&&t.stickyHeader]}})(Ml(({theme:e})=>({display:`table`,width:`100%`,borderCollapse:`collapse`,borderSpacing:0,"& caption":{...e.typography.body2,padding:e.spacing(2),color:(e.vars||e).palette.text.secondary,textAlign:`left`,captionSide:`bottom`},variants:[{props:({ownerState:e})=>e.stickyHeader,style:{borderCollapse:`separate`}}]}))),x_=`table`,S_=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiTable`}),{className:r,component:i=x_,padding:a=`normal`,size:o=`medium`,stickyHeader:s=!1,...c}=n,l={...n,component:i,padding:a,size:o,stickyHeader:s},u=y_(l),d=x.useMemo(()=>({padding:a,size:o,stickyHeader:s}),[a,o,s]);return(0,I.jsx)(__.Provider,{value:d,children:(0,I.jsx)(b_,{as:i,role:i===x_?null:`table`,ref:t,className:R(u.root,r),ownerState:l,...c})})}),C_=x.createContext();function w_(e){return zo(`MuiTableBody`,e)}Bo(`MuiTableBody`,[`root`]);var T_=e=>{let{classes:t}=e;return dc({root:[`root`]},w_,t)},E_=B(`tbody`,{name:`MuiTableBody`,slot:`Root`})({display:`table-row-group`}),D_={variant:`body`},O_=`tbody`,k_=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiTableBody`}),{className:r,component:i=O_,...a}=n,o={...n,component:i},s=T_(o);return(0,I.jsx)(C_.Provider,{value:D_,children:(0,I.jsx)(E_,{className:R(s.root,r),as:i,ref:t,role:i===O_?null:`rowgroup`,ownerState:o,...a})})});function A_(e){return zo(`MuiTableCell`,e)}var j_=Bo(`MuiTableCell`,[`root`,`head`,`body`,`footer`,`sizeSmall`,`sizeMedium`,`paddingCheckbox`,`paddingNone`,`alignLeft`,`alignCenter`,`alignRight`,`alignJustify`,`stickyHeader`]),M_=e=>{let{classes:t,variant:n,align:r,padding:i,size:a,stickyHeader:o}=e;return dc({root:[`root`,n,o&&`stickyHeader`,r!==`inherit`&&`align${V(r)}`,i!==`normal`&&`padding${V(i)}`,`size${V(a)}`]},A_,t)},N_=B(`td`,{name:`MuiTableCell`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`size${V(n.size)}`],n.padding!==`normal`&&t[`padding${V(n.padding)}`],n.align!==`inherit`&&t[`align${V(n.align)}`],n.stickyHeader&&t.stickyHeader]}})(Ml(({theme:e})=>({...e.typography.body2,display:`table-cell`,verticalAlign:`inherit`,borderBottom:e.vars?`1px solid ${e.vars.palette.TableCell.border}`:`1px solid
    ${e.palette.mode===`light`?e.lighten(e.alpha(e.palette.divider,1),.88):e.darken(e.alpha(e.palette.divider,1),.68)}`,textAlign:`left`,padding:16,variants:[{props:{variant:`head`},style:{color:(e.vars||e).palette.text.primary,lineHeight:e.typography.pxToRem(24),fontWeight:e.typography.fontWeightMedium}},{props:{variant:`body`},style:{color:(e.vars||e).palette.text.primary}},{props:{variant:`footer`},style:{color:(e.vars||e).palette.text.secondary,lineHeight:e.typography.pxToRem(21),fontSize:e.typography.pxToRem(12)}},{props:{size:`small`},style:{padding:`6px 16px`,[`&.${j_.paddingCheckbox}`]:{width:24,padding:`0 12px 0 16px`,"& > *":{padding:0}}}},{props:{padding:`checkbox`},style:{width:48,padding:`0 0 0 4px`}},{props:{padding:`none`},style:{padding:0}},{props:{align:`left`},style:{textAlign:`left`}},{props:{align:`center`},style:{textAlign:`center`}},{props:{align:`right`},style:{textAlign:`right`,flexDirection:`row-reverse`}},{props:{align:`justify`},style:{textAlign:`justify`}},{props:({ownerState:e})=>e.stickyHeader,style:{position:`sticky`,top:0,zIndex:2,backgroundColor:(e.vars||e).palette.background.default}}]}))),J=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiTableCell`}),{align:r=`inherit`,className:i,component:a,padding:o,scope:s,size:c,sortDirection:l,variant:u,...d}=n,f=x.useContext(__),p=x.useContext(C_),m=p&&p.variant===`head`,h;h=a||(m?`th`:`td`);let g=s;h===`td`?g=void 0:!g&&m&&(g=`col`);let _=u||p&&p.variant,v={...n,align:r,component:h,padding:o||(f&&f.padding?f.padding:`normal`),size:c||(f&&f.size?f.size:`medium`),sortDirection:l,stickyHeader:_===`head`&&f&&f.stickyHeader,variant:_},y=M_(v),b=null;return l&&(b=l===`asc`?`ascending`:`descending`),(0,I.jsx)(N_,{as:h,ref:t,className:R(y.root,i),"aria-sort":b,scope:g,ownerState:v,...d})});function P_(e){return zo(`MuiTableContainer`,e)}Bo(`MuiTableContainer`,[`root`]);var F_=e=>{let{classes:t}=e;return dc({root:[`root`]},P_,t)},I_=B(`div`,{name:`MuiTableContainer`,slot:`Root`})({width:`100%`,overflowX:`auto`}),L_=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiTableContainer`}),{className:r,component:i=`div`,...a}=n,o={...n,component:i};return(0,I.jsx)(I_,{ref:t,as:i,className:R(F_(o).root,r),ownerState:o,...a})});function R_(e){return zo(`MuiTableHead`,e)}Bo(`MuiTableHead`,[`root`]);var z_=e=>{let{classes:t}=e;return dc({root:[`root`]},R_,t)},B_=B(`thead`,{name:`MuiTableHead`,slot:`Root`})({display:`table-header-group`}),V_={variant:`head`},H_=`thead`,U_=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiTableHead`}),{className:r,component:i=H_,...a}=n,o={...n,component:i},s=z_(o);return(0,I.jsx)(C_.Provider,{value:V_,children:(0,I.jsx)(B_,{as:i,className:R(s.root,r),ref:t,role:i===H_?null:`rowgroup`,ownerState:o,...a})})});function W_(e){return zo(`MuiToolbar`,e)}Bo(`MuiToolbar`,[`root`,`gutters`,`regular`,`dense`]);var G_=e=>{let{classes:t,disableGutters:n,variant:r}=e;return dc({root:[`root`,!n&&`gutters`,r]},W_,t)},K_=B(`div`,{name:`MuiToolbar`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.disableGutters&&t.gutters,t[n.variant]]}})(Ml(({theme:e})=>({position:`relative`,display:`flex`,alignItems:`center`,variants:[{props:({ownerState:e})=>!e.disableGutters,style:{paddingLeft:e.spacing(2),paddingRight:e.spacing(2),[e.breakpoints.up(`sm`)]:{paddingLeft:e.spacing(3),paddingRight:e.spacing(3)}}},{props:{variant:`dense`},style:{minHeight:48}},{props:{variant:`regular`},style:e.mixins.toolbar}]}))),q_=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiToolbar`}),{className:r,component:i=`div`,disableGutters:a=!1,variant:o=`regular`,...s}=n,c={...n,component:i,disableGutters:a,variant:o};return(0,I.jsx)(K_,{as:i,className:R(G_(c).root,r),ref:t,ownerState:c,...s})});function J_(e){return zo(`MuiTableRow`,e)}var Y_=Bo(`MuiTableRow`,[`root`,`selected`,`hover`,`head`,`footer`]),X_=e=>{let{classes:t,selected:n,hover:r,head:i,footer:a}=e;return dc({root:[`root`,n&&`selected`,r&&`hover`,i&&`head`,a&&`footer`]},J_,t)},Z_=B(`tr`,{name:`MuiTableRow`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.head&&t.head,n.footer&&t.footer]}})(Ml(({theme:e})=>({color:`inherit`,display:`table-row`,verticalAlign:`middle`,outline:0,[`&.${Y_.hover}:hover`]:{backgroundColor:(e.vars||e).palette.action.hover},[`&.${Y_.selected}`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,(e.vars||e).palette.action.selectedOpacity),"&:hover":{backgroundColor:e.alpha((e.vars||e).palette.primary.main,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.hoverOpacity}`)}}}))),Q_=`tr`,Y=x.forwardRef(function(e,t){let n=Nl({props:e,name:`MuiTableRow`}),{className:r,component:i=Q_,hover:a=!1,selected:o=!1,...s}=n,c=x.useContext(C_),l={...n,component:i,hover:a,selected:o,head:c&&c.variant===`head`,footer:c&&c.variant===`footer`};return(0,I.jsx)(Z_,{as:i,ref:t,className:R(X_(l).root,r),role:i===Q_?null:`row`,ownerState:l,...s})}),$_={root:{}};const ev=()=>(0,I.jsxs)(W,{sx:$_.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ReactiveUIToolKit`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit brings a React-like component model to Unity UI Toolkit using a virtual node tree, typed props, and reconciliation logic that runs in C#. You build your UI from`,` `,(0,I.jsx)(`code`,{children:`V.*`}),` helpers and function components, and the reconciler updates the underlying`,(0,I.jsx)(`code`,{children:`VisualElement`}),` hierarchy for you.`]}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`The toolkit is designed to work both in the Unity Editor and at runtime, and to feel familiar if you have used React, while still fitting naturally into Unity's component model and UI Toolkit controls.`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[(0,I.jsx)(`strong`,{children:`P.S.`}),` ReactiveUIToolKit runs entirely in C# on top of Unity UI Toolkit. There is no JavaScript engine or bridge layer involved.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Highlights`}),(0,I.jsxs)(G,{children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`VirtualNode diffing and batched updates for UI Toolkit trees`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Typed props and adapters for most built-in UI Toolkit controls`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Router and Signals utilities for navigation and shared state`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Editor-only elements are UNITY_EDITOR guarded`})})]})]});var tv=Object.create,nv=Object.defineProperty,rv=Object.defineProperties,iv=Object.getOwnPropertyDescriptor,av=Object.getOwnPropertyDescriptors,ov=Object.getOwnPropertyNames,sv=Object.getOwnPropertySymbols,cv=Object.getPrototypeOf,lv=Object.prototype.hasOwnProperty,uv=Object.prototype.propertyIsEnumerable,dv=(e,t,n)=>t in e?nv(e,t,{enumerable:!0,configurable:!0,writable:!0,value:n}):e[t]=n,fv=(e,t)=>{for(var n in t||={})lv.call(t,n)&&dv(e,n,t[n]);if(sv)for(var n of sv(t))uv.call(t,n)&&dv(e,n,t[n]);return e},pv=(e,t)=>rv(e,av(t)),mv=(e,t)=>{var n={};for(var r in e)lv.call(e,r)&&t.indexOf(r)<0&&(n[r]=e[r]);if(e!=null&&sv)for(var r of sv(e))t.indexOf(r)<0&&uv.call(e,r)&&(n[r]=e[r]);return n},hv=(e,t)=>function(){return t||(0,e[ov(e)[0]])((t={exports:{}}).exports,t),t.exports},gv=(e,t)=>{for(var n in t)nv(e,n,{get:t[n],enumerable:!0})},_v=(e,t,n,r)=>{if(t&&typeof t==`object`||typeof t==`function`)for(let i of ov(t))!lv.call(e,i)&&i!==n&&nv(e,i,{get:()=>t[i],enumerable:!(r=iv(t,i))||r.enumerable});return e},X=((e,t,n)=>(n=e==null?{}:tv(cv(e)),_v(t||!e||!e.__esModule?nv(n,`default`,{value:e,enumerable:!0}):n,e)))(hv({"../../node_modules/.pnpm/prismjs@1.29.0_patch_hash=vrxx3pzkik6jpmgpayxfjunetu/node_modules/prismjs/prism.js"(e,t){var n=function(){var e=/(?:^|\s)lang(?:uage)?-([\w-]+)(?=\s|$)/i,t=0,n={},r={util:{encode:function e(t){return t instanceof i?new i(t.type,e(t.content),t.alias):Array.isArray(t)?t.map(e):t.replace(/&/g,`&amp;`).replace(/</g,`&lt;`).replace(/\u00a0/g,` `)},type:function(e){return Object.prototype.toString.call(e).slice(8,-1)},objId:function(e){return e.__id||Object.defineProperty(e,`__id`,{value:++t}),e.__id},clone:function e(t,n){n||={};var i,a;switch(r.util.type(t)){case`Object`:if(a=r.util.objId(t),n[a])return n[a];for(var o in i={},n[a]=i,t)t.hasOwnProperty(o)&&(i[o]=e(t[o],n));return i;case`Array`:return a=r.util.objId(t),n[a]?n[a]:(i=[],n[a]=i,t.forEach(function(t,r){i[r]=e(t,n)}),i);default:return t}},getLanguage:function(t){for(;t;){var n=e.exec(t.className);if(n)return n[1].toLowerCase();t=t.parentElement}return`none`},setLanguage:function(t,n){t.className=t.className.replace(RegExp(e,`gi`),``),t.classList.add(`language-`+n)},isActive:function(e,t,n){for(var r=`no-`+t;e;){var i=e.classList;if(i.contains(t))return!0;if(i.contains(r))return!1;e=e.parentElement}return!!n}},languages:{plain:n,plaintext:n,text:n,txt:n,extend:function(e,t){var n=r.util.clone(r.languages[e]);for(var i in t)n[i]=t[i];return n},insertBefore:function(e,t,n,i){i||=r.languages;var a=i[e],o={};for(var s in a)if(a.hasOwnProperty(s)){if(s==t)for(var c in n)n.hasOwnProperty(c)&&(o[c]=n[c]);n.hasOwnProperty(s)||(o[s]=a[s])}var l=i[e];return i[e]=o,r.languages.DFS(r.languages,function(t,n){n===l&&t!=e&&(this[t]=o)}),o},DFS:function e(t,n,i,a){a||={};var o=r.util.objId;for(var s in t)if(t.hasOwnProperty(s)){n.call(t,s,t[s],i||s);var c=t[s],l=r.util.type(c);l===`Object`&&!a[o(c)]?(a[o(c)]=!0,e(c,n,null,a)):l===`Array`&&!a[o(c)]&&(a[o(c)]=!0,e(c,n,s,a))}}},plugins:{},highlight:function(e,t,n){var a={code:e,grammar:t,language:n};if(r.hooks.run(`before-tokenize`,a),!a.grammar)throw Error(`The language "`+a.language+`" has no grammar.`);return a.tokens=r.tokenize(a.code,a.grammar),r.hooks.run(`after-tokenize`,a),i.stringify(r.util.encode(a.tokens),a.language)},tokenize:function(e,t){var n=t.rest;if(n){for(var r in n)t[r]=n[r];delete t.rest}var i=new s;return c(i,i.head,e),o(e,i,t,i.head,0),u(i)},hooks:{all:{},add:function(e,t){var n=r.hooks.all;n[e]=n[e]||[],n[e].push(t)},run:function(e,t){var n=r.hooks.all[e];if(!(!n||!n.length))for(var i=0,a;a=n[i++];)a(t)}},Token:i};function i(e,t,n,r){this.type=e,this.content=t,this.alias=n,this.length=(r||``).length|0}i.stringify=function e(t,n){if(typeof t==`string`)return t;if(Array.isArray(t)){var i=``;return t.forEach(function(t){i+=e(t,n)}),i}var a={type:t.type,content:e(t.content,n),tag:`span`,classes:[`token`,t.type],attributes:{},language:n},o=t.alias;o&&(Array.isArray(o)?Array.prototype.push.apply(a.classes,o):a.classes.push(o)),r.hooks.run(`wrap`,a);var s=``;for(var c in a.attributes)s+=` `+c+`="`+(a.attributes[c]||``).replace(/"/g,`&quot;`)+`"`;return`<`+a.tag+` class="`+a.classes.join(` `)+`"`+s+`>`+a.content+`</`+a.tag+`>`};function a(e,t,n,r){e.lastIndex=t;var i=e.exec(n);if(i&&r&&i[1]){var a=i[1].length;i.index+=a,i[0]=i[0].slice(a)}return i}function o(e,t,n,s,u,d){for(var f in n)if(!(!n.hasOwnProperty(f)||!n[f])){var p=n[f];p=Array.isArray(p)?p:[p];for(var m=0;m<p.length;++m){if(d&&d.cause==f+`,`+m)return;var h=p[m],g=h.inside,_=!!h.lookbehind,v=!!h.greedy,y=h.alias;if(v&&!h.pattern.global){var b=h.pattern.toString().match(/[imsuy]*$/)[0];h.pattern=RegExp(h.pattern.source,b+`g`)}for(var x=h.pattern||h,S=s.next,C=u;S!==t.tail&&!(d&&C>=d.reach);C+=S.value.length,S=S.next){var w=S.value;if(t.length>e.length)return;if(!(w instanceof i)){var T=1,E;if(v){if(E=a(x,C,e,_),!E||E.index>=e.length)break;var D=E.index,O=E.index+E[0].length,k=C;for(k+=S.value.length;D>=k;)S=S.next,k+=S.value.length;if(k-=S.value.length,C=k,S.value instanceof i)continue;for(var ee=S;ee!==t.tail&&(k<O||typeof ee.value==`string`);ee=ee.next)T++,k+=ee.value.length;T--,w=e.slice(C,k),E.index-=C}else if(E=a(x,0,w,_),!E)continue;var D=E.index,A=E[0],j=w.slice(0,D),M=w.slice(D+A.length),te=C+w.length;d&&te>d.reach&&(d.reach=te);var N=S.prev;j&&(N=c(t,N,j),C+=j.length),l(t,N,T);var P=new i(f,g?r.tokenize(A,g):A,y,A);if(S=c(t,N,P),M&&c(t,S,M),T>1){var F={cause:f+`,`+m,reach:te};o(e,t,n,S.prev,C,F),d&&F.reach>d.reach&&(d.reach=F.reach)}}}}}}function s(){var e={value:null,prev:null,next:null},t={value:null,prev:e,next:null};e.next=t,this.head=e,this.tail=t,this.length=0}function c(e,t,n){var r=t.next,i={value:n,prev:t,next:r};return t.next=i,r.prev=i,e.length++,i}function l(e,t,n){for(var r=t.next,i=0;i<n&&r!==e.tail;i++)r=r.next;t.next=r,r.prev=t,e.length-=i}function u(e){for(var t=[],n=e.head.next;n!==e.tail;)t.push(n.value),n=n.next;return t}return r}();t.exports=n,n.default=n}})());X.languages.markup={comment:{pattern:/<!--(?:(?!<!--)[\s\S])*?-->/,greedy:!0},prolog:{pattern:/<\?[\s\S]+?\?>/,greedy:!0},doctype:{pattern:/<!DOCTYPE(?:[^>"'[\]]|"[^"]*"|'[^']*')+(?:\[(?:[^<"'\]]|"[^"]*"|'[^']*'|<(?!!--)|<!--(?:[^-]|-(?!->))*-->)*\]\s*)?>/i,greedy:!0,inside:{"internal-subset":{pattern:/(^[^\[]*\[)[\s\S]+(?=\]>$)/,lookbehind:!0,greedy:!0,inside:null},string:{pattern:/"[^"]*"|'[^']*'/,greedy:!0},punctuation:/^<!|>$|[[\]]/,"doctype-tag":/^DOCTYPE/i,name:/[^\s<>'"]+/}},cdata:{pattern:/<!\[CDATA\[[\s\S]*?\]\]>/i,greedy:!0},tag:{pattern:/<\/?(?!\d)[^\s>\/=$<%]+(?:\s(?:\s*[^\s>\/=]+(?:\s*=\s*(?:"[^"]*"|'[^']*'|[^\s'">=]+(?=[\s>]))|(?=[\s/>])))+)?\s*\/?>/,greedy:!0,inside:{tag:{pattern:/^<\/?[^\s>\/]+/,inside:{punctuation:/^<\/?/,namespace:/^[^\s>\/:]+:/}},"special-attr":[],"attr-value":{pattern:/=\s*(?:"[^"]*"|'[^']*'|[^\s'">=]+)/,inside:{punctuation:[{pattern:/^=/,alias:`attr-equals`},{pattern:/^(\s*)["']|["']$/,lookbehind:!0}]}},punctuation:/\/?>/,"attr-name":{pattern:/[^\s>\/]+/,inside:{namespace:/^[^\s>\/:]+:/}}}},entity:[{pattern:/&[\da-z]{1,8};/i,alias:`named-entity`},/&#x?[\da-f]{1,8};/i]},X.languages.markup.tag.inside[`attr-value`].inside.entity=X.languages.markup.entity,X.languages.markup.doctype.inside[`internal-subset`].inside=X.languages.markup,X.hooks.add(`wrap`,function(e){e.type===`entity`&&(e.attributes.title=e.content.replace(/&amp;/,`&`))}),Object.defineProperty(X.languages.markup.tag,`addInlined`,{value:function(e,t){var n={},n=(n[`language-`+t]={pattern:/(^<!\[CDATA\[)[\s\S]+?(?=\]\]>$)/i,lookbehind:!0,inside:X.languages[t]},n.cdata=/^<!\[CDATA\[|\]\]>$/i,{"included-cdata":{pattern:/<!\[CDATA\[[\s\S]*?\]\]>/i,inside:n}}),t=(n[`language-`+t]={pattern:/[\s\S]+/,inside:X.languages[t]},{});t[e]={pattern:RegExp(`(<__[^>]*>)(?:<!\\[CDATA\\[(?:[^\\]]|\\](?!\\]>))*\\]\\]>|(?!<!\\[CDATA\\[)[\\s\\S])*?(?=<\\/__>)`.replace(/__/g,function(){return e}),`i`),lookbehind:!0,greedy:!0,inside:n},X.languages.insertBefore(`markup`,`cdata`,t)}}),Object.defineProperty(X.languages.markup.tag,`addAttribute`,{value:function(e,t){X.languages.markup.tag.inside[`special-attr`].push({pattern:RegExp(`(^|["'\\s])(?:`+e+`)\\s*=\\s*(?:"[^"]*"|'[^']*'|[^\\s'">=]+(?=[\\s>]))`,`i`),lookbehind:!0,inside:{"attr-name":/^[^\s=]+/,"attr-value":{pattern:/=[\s\S]+/,inside:{value:{pattern:/(^=\s*(["']|(?!["'])))\S[\s\S]*(?=\2$)/,lookbehind:!0,alias:[t,`language-`+t],inside:X.languages[t]},punctuation:[{pattern:/^=/,alias:`attr-equals`},/"|'/]}}}})}}),X.languages.html=X.languages.markup,X.languages.mathml=X.languages.markup,X.languages.svg=X.languages.markup,X.languages.xml=X.languages.extend(`markup`,{}),X.languages.ssml=X.languages.xml,X.languages.atom=X.languages.xml,X.languages.rss=X.languages.xml,function(e){var t={pattern:/\\[\\(){}[\]^$+*?|.]/,alias:`escape`},n=/\\(?:x[\da-fA-F]{2}|u[\da-fA-F]{4}|u\{[\da-fA-F]+\}|0[0-7]{0,2}|[123][0-7]{2}|c[a-zA-Z]|.)/,r=`(?:[^\\\\-]|`+n.source+`)`,r=RegExp(r+`-`+r),i={pattern:/(<|')[^<>']+(?=[>']$)/,lookbehind:!0,alias:`variable`};e.languages.regex={"char-class":{pattern:/((?:^|[^\\])(?:\\\\)*)\[(?:[^\\\]]|\\[\s\S])*\]/,lookbehind:!0,inside:{"char-class-negation":{pattern:/(^\[)\^/,lookbehind:!0,alias:`operator`},"char-class-punctuation":{pattern:/^\[|\]$/,alias:`punctuation`},range:{pattern:r,inside:{escape:n,"range-punctuation":{pattern:/-/,alias:`operator`}}},"special-escape":t,"char-set":{pattern:/\\[wsd]|\\p\{[^{}]+\}/i,alias:`class-name`},escape:n}},"special-escape":t,"char-set":{pattern:/\.|\\[wsd]|\\p\{[^{}]+\}/i,alias:`class-name`},backreference:[{pattern:/\\(?![123][0-7]{2})[1-9]/,alias:`keyword`},{pattern:/\\k<[^<>']+>/,alias:`keyword`,inside:{"group-name":i}}],anchor:{pattern:/[$^]|\\[ABbGZz]/,alias:`function`},escape:n,group:[{pattern:/\((?:\?(?:<[^<>']+>|'[^<>']+'|[>:]|<?[=!]|[idmnsuxU]+(?:-[idmnsuxU]+)?:?))?/,alias:`punctuation`,inside:{"group-name":i}},{pattern:/\)/,alias:`punctuation`}],quantifier:{pattern:/(?:[+*?]|\{\d+(?:,\d*)?\})[?+]?/,alias:`number`},alternation:{pattern:/\|/,alias:`keyword`}}}(X),X.languages.clike={comment:[{pattern:/(^|[^\\])\/\*[\s\S]*?(?:\*\/|$)/,lookbehind:!0,greedy:!0},{pattern:/(^|[^\\:])\/\/.*/,lookbehind:!0,greedy:!0}],string:{pattern:/(["'])(?:\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,greedy:!0},"class-name":{pattern:/(\b(?:class|extends|implements|instanceof|interface|new|trait)\s+|\bcatch\s+\()[\w.\\]+/i,lookbehind:!0,inside:{punctuation:/[.\\]/}},keyword:/\b(?:break|catch|continue|do|else|finally|for|function|if|in|instanceof|new|null|return|throw|try|while)\b/,boolean:/\b(?:false|true)\b/,function:/\b\w+(?=\()/,number:/\b0x[\da-f]+\b|(?:\b\d+(?:\.\d*)?|\B\.\d+)(?:e[+-]?\d+)?/i,operator:/[<>]=?|[!=]=?=?|--?|\+\+?|&&?|\|\|?|[?*/~^%]/,punctuation:/[{}[\];(),.:]/},X.languages.javascript=X.languages.extend(`clike`,{"class-name":[X.languages.clike[`class-name`],{pattern:/(^|[^$\w\xA0-\uFFFF])(?!\s)[_$A-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\.(?:constructor|prototype))/,lookbehind:!0}],keyword:[{pattern:/((?:^|\})\s*)catch\b/,lookbehind:!0},{pattern:/(^|[^.]|\.\.\.\s*)\b(?:as|assert(?=\s*\{)|async(?=\s*(?:function\b|\(|[$\w\xA0-\uFFFF]|$))|await|break|case|class|const|continue|debugger|default|delete|do|else|enum|export|extends|finally(?=\s*(?:\{|$))|for|from(?=\s*(?:['"]|$))|function|(?:get|set)(?=\s*(?:[#\[$\w\xA0-\uFFFF]|$))|if|implements|import|in|instanceof|interface|let|new|null|of|package|private|protected|public|return|static|super|switch|this|throw|try|typeof|undefined|var|void|while|with|yield)\b/,lookbehind:!0}],function:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*(?:\.\s*(?:apply|bind|call)\s*)?\()/,number:{pattern:RegExp(`(^|[^\\w$])(?:NaN|Infinity|0[bB][01]+(?:_[01]+)*n?|0[oO][0-7]+(?:_[0-7]+)*n?|0[xX][\\dA-Fa-f]+(?:_[\\dA-Fa-f]+)*n?|\\d+(?:_\\d+)*n|(?:\\d+(?:_\\d+)*(?:\\.(?:\\d+(?:_\\d+)*)?)?|\\.\\d+(?:_\\d+)*)(?:[Ee][+-]?\\d+(?:_\\d+)*)?)(?![\\w$])`),lookbehind:!0},operator:/--|\+\+|\*\*=?|=>|&&=?|\|\|=?|[!=]==|<<=?|>>>?=?|[-+*/%&|^!=<>]=?|\.{3}|\?\?=?|\?\.?|[~:]/}),X.languages.javascript[`class-name`][0].pattern=/(\b(?:class|extends|implements|instanceof|interface|new)\s+)[\w.\\]+/,X.languages.insertBefore(`javascript`,`keyword`,{regex:{pattern:RegExp(`((?:^|[^$\\w\\xA0-\\uFFFF."'\\])\\s]|\\b(?:return|yield))\\s*)\\/(?:(?:\\[(?:[^\\]\\\\\\r\\n]|\\\\.)*\\]|\\\\.|[^/\\\\\\[\\r\\n])+\\/[dgimyus]{0,7}|(?:\\[(?:[^[\\]\\\\\\r\\n]|\\\\.|\\[(?:[^[\\]\\\\\\r\\n]|\\\\.|\\[(?:[^[\\]\\\\\\r\\n]|\\\\.)*\\])*\\])*\\]|\\\\.|[^/\\\\\\[\\r\\n])+\\/[dgimyus]{0,7}v[dgimyus]{0,7})(?=(?:\\s|\\/\\*(?:[^*]|\\*(?!\\/))*\\*\\/)*(?:$|[\\r\\n,.;:})\\]]|\\/\\/))`),lookbehind:!0,greedy:!0,inside:{"regex-source":{pattern:/^(\/)[\s\S]+(?=\/[a-z]*$)/,lookbehind:!0,alias:`language-regex`,inside:X.languages.regex},"regex-delimiter":/^\/|\/$/,"regex-flags":/^[a-z]+$/}},"function-variable":{pattern:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*[=:]\s*(?:async\s*)?(?:\bfunction\b|(?:\((?:[^()]|\([^()]*\))*\)|(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)\s*=>))/,alias:`function`},parameter:[{pattern:/(function(?:\s+(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)?\s*\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\))/,lookbehind:!0,inside:X.languages.javascript},{pattern:/(^|[^$\w\xA0-\uFFFF])(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*=>)/i,lookbehind:!0,inside:X.languages.javascript},{pattern:/(\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\)\s*=>)/,lookbehind:!0,inside:X.languages.javascript},{pattern:/((?:\b|\s|^)(?!(?:as|async|await|break|case|catch|class|const|continue|debugger|default|delete|do|else|enum|export|extends|finally|for|from|function|get|if|implements|import|in|instanceof|interface|let|new|null|of|package|private|protected|public|return|set|static|super|switch|this|throw|try|typeof|undefined|var|void|while|with|yield)(?![$\w\xA0-\uFFFF]))(?:(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*\s*)\(\s*|\]\s*\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\)\s*\{)/,lookbehind:!0,inside:X.languages.javascript}],constant:/\b[A-Z](?:[A-Z_]|\dx?)*\b/}),X.languages.insertBefore(`javascript`,`string`,{hashbang:{pattern:/^#!.*/,greedy:!0,alias:`comment`},"template-string":{pattern:/`(?:\\[\s\S]|\$\{(?:[^{}]|\{(?:[^{}]|\{[^}]*\})*\})+\}|(?!\$\{)[^\\`])*`/,greedy:!0,inside:{"template-punctuation":{pattern:/^`|`$/,alias:`string`},interpolation:{pattern:/((?:^|[^\\])(?:\\{2})*)\$\{(?:[^{}]|\{(?:[^{}]|\{[^}]*\})*\})+\}/,lookbehind:!0,inside:{"interpolation-punctuation":{pattern:/^\$\{|\}$/,alias:`punctuation`},rest:X.languages.javascript}},string:/[\s\S]+/}},"string-property":{pattern:/((?:^|[,{])[ \t]*)(["'])(?:\\(?:\r\n|[\s\S])|(?!\2)[^\\\r\n])*\2(?=\s*:)/m,lookbehind:!0,greedy:!0,alias:`property`}}),X.languages.insertBefore(`javascript`,`operator`,{"literal-property":{pattern:/((?:^|[,{])[ \t]*)(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*:)/m,lookbehind:!0,alias:`property`}}),X.languages.markup&&(X.languages.markup.tag.addInlined(`script`,`javascript`),X.languages.markup.tag.addAttribute(`on(?:abort|blur|change|click|composition(?:end|start|update)|dblclick|error|focus(?:in|out)?|key(?:down|up)|load|mouse(?:down|enter|leave|move|out|over|up)|reset|resize|scroll|select|slotchange|submit|unload|wheel)`,`javascript`)),X.languages.js=X.languages.javascript,X.languages.actionscript=X.languages.extend(`javascript`,{keyword:/\b(?:as|break|case|catch|class|const|default|delete|do|dynamic|each|else|extends|final|finally|for|function|get|if|implements|import|in|include|instanceof|interface|internal|is|namespace|native|new|null|override|package|private|protected|public|return|set|static|super|switch|this|throw|try|typeof|use|var|void|while|with)\b/,operator:/\+\+|--|(?:[+\-*\/%^]|&&?|\|\|?|<<?|>>?>?|[!=]=?)=?|[~?@]/}),X.languages.actionscript[`class-name`].alias=`function`,delete X.languages.actionscript.parameter,delete X.languages.actionscript[`literal-property`],X.languages.markup&&X.languages.insertBefore(`actionscript`,`string`,{xml:{pattern:/(^|[^.])<\/?\w+(?:\s+[^\s>\/=]+=("|')(?:\\[\s\S]|(?!\2)[^\\])*\2)*\s*\/?>/,lookbehind:!0,inside:X.languages.markup}}),function(e){var t=/#(?!\{).+/,n={pattern:/#\{[^}]+\}/,alias:`variable`};e.languages.coffeescript=e.languages.extend(`javascript`,{comment:t,string:[{pattern:/'(?:\\[\s\S]|[^\\'])*'/,greedy:!0},{pattern:/"(?:\\[\s\S]|[^\\"])*"/,greedy:!0,inside:{interpolation:n}}],keyword:/\b(?:and|break|by|catch|class|continue|debugger|delete|do|each|else|extend|extends|false|finally|for|if|in|instanceof|is|isnt|let|loop|namespace|new|no|not|null|of|off|on|or|own|return|super|switch|then|this|throw|true|try|typeof|undefined|unless|until|when|while|window|with|yes|yield)\b/,"class-member":{pattern:/@(?!\d)\w+/,alias:`variable`}}),e.languages.insertBefore(`coffeescript`,`comment`,{"multiline-comment":{pattern:/###[\s\S]+?###/,alias:`comment`},"block-regex":{pattern:/\/{3}[\s\S]*?\/{3}/,alias:`regex`,inside:{comment:t,interpolation:n}}}),e.languages.insertBefore(`coffeescript`,`string`,{"inline-javascript":{pattern:/`(?:\\[\s\S]|[^\\`])*`/,inside:{delimiter:{pattern:/^`|`$/,alias:`punctuation`},script:{pattern:/[\s\S]+/,alias:`language-javascript`,inside:e.languages.javascript}}},"multiline-string":[{pattern:/'''[\s\S]*?'''/,greedy:!0,alias:`string`},{pattern:/"""[\s\S]*?"""/,greedy:!0,alias:`string`,inside:{interpolation:n}}]}),e.languages.insertBefore(`coffeescript`,`keyword`,{property:/(?!\d)\w+(?=\s*:(?!:))/}),delete e.languages.coffeescript[`template-string`],e.languages.coffee=e.languages.coffeescript}(X),function(e){var t=e.languages.javadoclike={parameter:{pattern:/(^[\t ]*(?:\/{3}|\*|\/\*\*)\s*@(?:arg|arguments|param)\s+)\w+/m,lookbehind:!0},keyword:{pattern:/(^[\t ]*(?:\/{3}|\*|\/\*\*)\s*|\{)@[a-z][a-zA-Z-]+\b/m,lookbehind:!0},punctuation:/[{}]/};Object.defineProperty(t,`addSupport`,{value:function(t,n){(t=typeof t==`string`?[t]:t).forEach(function(t){var r=function(e){e.inside||={},e.inside.rest=n},i=`doc-comment`;if(a=e.languages[t]){var a,o=a[i];if((o||=(a=e.languages.insertBefore(t,`comment`,{"doc-comment":{pattern:/(^|[^\\])\/\*\*[^/][\s\S]*?(?:\*\/|$)/,lookbehind:!0,alias:`comment`}}))[i])instanceof RegExp&&(o=a[i]={pattern:o}),Array.isArray(o))for(var s=0,c=o.length;s<c;s++)o[s]instanceof RegExp&&(o[s]={pattern:o[s]}),r(o[s]);else r(o)}})}}),t.addSupport([`java`,`javascript`,`php`],t)}(X),function(e){var t=/(?:"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"|'(?:\\(?:\r\n|[\s\S])|[^'\\\r\n])*')/,t=(e.languages.css={comment:/\/\*[\s\S]*?\*\//,atrule:{pattern:RegExp(`@[\\w-](?:[^;{\\s"']|\\s+(?!\\s)|`+t.source+`)*?(?:;|(?=\\s*\\{))`),inside:{rule:/^@[\w-]+/,"selector-function-argument":{pattern:/(\bselector\s*\(\s*(?![\s)]))(?:[^()\s]|\s+(?![\s)])|\((?:[^()]|\([^()]*\))*\))+(?=\s*\))/,lookbehind:!0,alias:`selector`},keyword:{pattern:/(^|[^\w-])(?:and|not|only|or)(?![\w-])/,lookbehind:!0}}},url:{pattern:RegExp(`\\burl\\((?:`+t.source+`|(?:[^\\\\\\r\\n()"']|\\\\[\\s\\S])*)\\)`,`i`),greedy:!0,inside:{function:/^url/i,punctuation:/^\(|\)$/,string:{pattern:RegExp(`^`+t.source+`$`),alias:`url`}}},selector:{pattern:RegExp(`(^|[{}\\s])[^{}\\s](?:[^{};"'\\s]|\\s+(?![\\s{])|`+t.source+`)*(?=\\s*\\{)`),lookbehind:!0},string:{pattern:t,greedy:!0},property:{pattern:/(^|[^-\w\xA0-\uFFFF])(?!\s)[-_a-z\xA0-\uFFFF](?:(?!\s)[-\w\xA0-\uFFFF])*(?=\s*:)/i,lookbehind:!0},important:/!important\b/i,function:{pattern:/(^|[^-a-z0-9])[-a-z0-9]+(?=\()/i,lookbehind:!0},punctuation:/[(){};:,]/},e.languages.css.atrule.inside.rest=e.languages.css,e.languages.markup);t&&(t.tag.addInlined(`style`,`css`),t.tag.addAttribute(`style`,`css`))}(X),function(e){var t=/("|')(?:\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,t=(e.languages.css.selector={pattern:e.languages.css.selector.pattern,lookbehind:!0,inside:t={"pseudo-element":/:(?:after|before|first-letter|first-line|selection)|::[-\w]+/,"pseudo-class":/:[-\w]+/,class:/\.[-\w]+/,id:/#[-\w]+/,attribute:{pattern:RegExp(`\\[(?:[^[\\]"']|`+t.source+`)*\\]`),greedy:!0,inside:{punctuation:/^\[|\]$/,"case-sensitivity":{pattern:/(\s)[si]$/i,lookbehind:!0,alias:`keyword`},namespace:{pattern:/^(\s*)(?:(?!\s)[-*\w\xA0-\uFFFF])*\|(?!=)/,lookbehind:!0,inside:{punctuation:/\|$/}},"attr-name":{pattern:/^(\s*)(?:(?!\s)[-\w\xA0-\uFFFF])+/,lookbehind:!0},"attr-value":[t,{pattern:/(=\s*)(?:(?!\s)[-\w\xA0-\uFFFF])+(?=\s*$)/,lookbehind:!0}],operator:/[|~*^$]?=/}},"n-th":[{pattern:/(\(\s*)[+-]?\d*[\dn](?:\s*[+-]\s*\d+)?(?=\s*\))/,lookbehind:!0,inside:{number:/[\dn]+/,operator:/[+-]/}},{pattern:/(\(\s*)(?:even|odd)(?=\s*\))/i,lookbehind:!0}],combinator:/>|\+|~|\|\|/,punctuation:/[(),]/}},e.languages.css.atrule.inside[`selector-function-argument`].inside=t,e.languages.insertBefore(`css`,`property`,{variable:{pattern:/(^|[^-\w\xA0-\uFFFF])--(?!\s)[-_a-z\xA0-\uFFFF](?:(?!\s)[-\w\xA0-\uFFFF])*/i,lookbehind:!0}}),{pattern:/(\b\d+)(?:%|[a-z]+(?![\w-]))/,lookbehind:!0}),n={pattern:/(^|[^\w.-])-?(?:\d+(?:\.\d+)?|\.\d+)/,lookbehind:!0};e.languages.insertBefore(`css`,`function`,{operator:{pattern:/(\s)[+\-*\/](?=\s)/,lookbehind:!0},hexcode:{pattern:/\B#[\da-f]{3,8}\b/i,alias:`color`},color:[{pattern:/(^|[^\w-])(?:AliceBlue|AntiqueWhite|Aqua|Aquamarine|Azure|Beige|Bisque|Black|BlanchedAlmond|Blue|BlueViolet|Brown|BurlyWood|CadetBlue|Chartreuse|Chocolate|Coral|CornflowerBlue|Cornsilk|Crimson|Cyan|DarkBlue|DarkCyan|DarkGoldenRod|DarkGr[ae]y|DarkGreen|DarkKhaki|DarkMagenta|DarkOliveGreen|DarkOrange|DarkOrchid|DarkRed|DarkSalmon|DarkSeaGreen|DarkSlateBlue|DarkSlateGr[ae]y|DarkTurquoise|DarkViolet|DeepPink|DeepSkyBlue|DimGr[ae]y|DodgerBlue|FireBrick|FloralWhite|ForestGreen|Fuchsia|Gainsboro|GhostWhite|Gold|GoldenRod|Gr[ae]y|Green|GreenYellow|HoneyDew|HotPink|IndianRed|Indigo|Ivory|Khaki|Lavender|LavenderBlush|LawnGreen|LemonChiffon|LightBlue|LightCoral|LightCyan|LightGoldenRodYellow|LightGr[ae]y|LightGreen|LightPink|LightSalmon|LightSeaGreen|LightSkyBlue|LightSlateGr[ae]y|LightSteelBlue|LightYellow|Lime|LimeGreen|Linen|Magenta|Maroon|MediumAquaMarine|MediumBlue|MediumOrchid|MediumPurple|MediumSeaGreen|MediumSlateBlue|MediumSpringGreen|MediumTurquoise|MediumVioletRed|MidnightBlue|MintCream|MistyRose|Moccasin|NavajoWhite|Navy|OldLace|Olive|OliveDrab|Orange|OrangeRed|Orchid|PaleGoldenRod|PaleGreen|PaleTurquoise|PaleVioletRed|PapayaWhip|PeachPuff|Peru|Pink|Plum|PowderBlue|Purple|RebeccaPurple|Red|RosyBrown|RoyalBlue|SaddleBrown|Salmon|SandyBrown|SeaGreen|SeaShell|Sienna|Silver|SkyBlue|SlateBlue|SlateGr[ae]y|Snow|SpringGreen|SteelBlue|Tan|Teal|Thistle|Tomato|Transparent|Turquoise|Violet|Wheat|White|WhiteSmoke|Yellow|YellowGreen)(?![\w-])/i,lookbehind:!0},{pattern:/\b(?:hsl|rgb)\(\s*\d{1,3}\s*,\s*\d{1,3}%?\s*,\s*\d{1,3}%?\s*\)\B|\b(?:hsl|rgb)a\(\s*\d{1,3}\s*,\s*\d{1,3}%?\s*,\s*\d{1,3}%?\s*,\s*(?:0|0?\.\d+|1)\s*\)\B/i,inside:{unit:t,number:n,function:/[\w-]+(?=\()/,punctuation:/[(),]/}}],entity:/\\[\da-f]{1,8}/i,unit:t,number:n})}(X),function(e){var t=/[*&][^\s[\]{},]+/,n=/!(?:<[\w\-%#;/?:@&=+$,.!~*'()[\]]+>|(?:[a-zA-Z\d-]*!)?[\w\-%#;/?:@&=+$.~*'()]+)?/,r=`(?:`+n.source+`(?:[ 	]+`+t.source+`)?|`+t.source+`(?:[ 	]+`+n.source+`)?)`,i=`(?:[^\\s\\x00-\\x08\\x0e-\\x1f!"#%&'*,\\-:>?@[\\]\`{|}\\x7f-\\x84\\x86-\\x9f\\ud800-\\udfff\\ufffe\\uffff]|[?:-]<PLAIN>)(?:[ \\t]*(?:(?![#:])<PLAIN>|:<PLAIN>))*`.replace(/<PLAIN>/g,function(){return`[^\\s\\x00-\\x08\\x0e-\\x1f,[\\]{}\\x7f-\\x84\\x86-\\x9f\\ud800-\\udfff\\ufffe\\uffff]`}),a=`"(?:[^"\\\\\\r\\n]|\\\\.)*"|'(?:[^'\\\\\\r\\n]|\\\\.)*'`;function o(e,t){t=(t||``).replace(/m/g,``)+`m`;var n=`([:\\-,[{]\\s*(?:\\s<<prop>>[ \\t]+)?)(?:<<value>>)(?=[ \\t]*(?:$|,|\\]|\\}|(?:[\\r\\n]\\s*)?#))`.replace(/<<prop>>/g,function(){return r}).replace(/<<value>>/g,function(){return e});return RegExp(n,t)}e.languages.yaml={scalar:{pattern:RegExp(`([\\-:]\\s*(?:\\s<<prop>>[ \\t]+)?[|>])[ \\t]*(?:((?:\\r?\\n|\\r)[ \\t]+)\\S[^\\r\\n]*(?:\\2[^\\r\\n]+)*)`.replace(/<<prop>>/g,function(){return r})),lookbehind:!0,alias:`string`},comment:/#.*/,key:{pattern:RegExp(`((?:^|[:\\-,[{\\r\\n?])[ \\t]*(?:<<prop>>[ \\t]+)?)<<key>>(?=\\s*:\\s)`.replace(/<<prop>>/g,function(){return r}).replace(/<<key>>/g,function(){return`(?:`+i+`|`+a+`)`})),lookbehind:!0,greedy:!0,alias:`atrule`},directive:{pattern:/(^[ \t]*)%.+/m,lookbehind:!0,alias:`important`},datetime:{pattern:o(`\\d{4}-\\d\\d?-\\d\\d?(?:[tT]|[ \\t]+)\\d\\d?:\\d{2}:\\d{2}(?:\\.\\d*)?(?:[ \\t]*(?:Z|[-+]\\d\\d?(?::\\d{2})?))?|\\d{4}-\\d{2}-\\d{2}|\\d\\d?:\\d{2}(?::\\d{2}(?:\\.\\d*)?)?`),lookbehind:!0,alias:`number`},boolean:{pattern:o(`false|true`,`i`),lookbehind:!0,alias:`important`},null:{pattern:o(`null|~`,`i`),lookbehind:!0,alias:`important`},string:{pattern:o(a),lookbehind:!0,greedy:!0},number:{pattern:o(`[+-]?(?:0x[\\da-f]+|0o[0-7]+|(?:\\d+(?:\\.\\d*)?|\\.\\d+)(?:e[+-]?\\d+)?|\\.inf|\\.nan)`,`i`),lookbehind:!0},tag:n,important:t,punctuation:/---|[:[\]{}\-,|>?]|\.\.\./},e.languages.yml=e.languages.yaml}(X),function(e){var t=`(?:\\\\.|[^\\\\\\n\\r]|(?:\\n|\\r\\n?)(?![\\r\\n]))`;function n(e){return e=e.replace(/<inner>/g,function(){return t}),RegExp(`((?:^|[^\\\\])(?:\\\\{2})*)(?:`+e+`)`)}var r="(?:\\\\.|``(?:[^`\\r\\n]|`(?!`))+``|`[^`\\r\\n]+`|[^\\\\|\\r\\n`])+",i=`\\|?__(?:\\|__)+\\|?(?:(?:\\n|\\r\\n?)|(?![\\s\\S]))`.replace(/__/g,function(){return r}),a=`\\|?[ \\t]*:?-{3,}:?[ \\t]*(?:\\|[ \\t]*:?-{3,}:?[ \\t]*)+\\|?(?:\\n|\\r\\n?)`,o=(e.languages.markdown=e.languages.extend(`markup`,{}),e.languages.insertBefore(`markdown`,`prolog`,{"front-matter-block":{pattern:/(^(?:\s*[\r\n])?)---(?!.)[\s\S]*?[\r\n]---(?!.)/,lookbehind:!0,greedy:!0,inside:{punctuation:/^---|---$/,"front-matter":{pattern:/\S+(?:\s+\S+)*/,alias:[`yaml`,`language-yaml`],inside:e.languages.yaml}}},blockquote:{pattern:/^>(?:[\t ]*>)*/m,alias:`punctuation`},table:{pattern:RegExp(`^`+i+a+`(?:`+i+`)*`,`m`),inside:{"table-data-rows":{pattern:RegExp(`^(`+i+a+`)(?:`+i+`)*$`),lookbehind:!0,inside:{"table-data":{pattern:RegExp(r),inside:e.languages.markdown},punctuation:/\|/}},"table-line":{pattern:RegExp(`^(`+i+`)`+a+`$`),lookbehind:!0,inside:{punctuation:/\||:?-{3,}:?/}},"table-header-row":{pattern:RegExp(`^`+i+`$`),inside:{"table-header":{pattern:RegExp(r),alias:`important`,inside:e.languages.markdown},punctuation:/\|/}}}},code:[{pattern:/((?:^|\n)[ \t]*\n|(?:^|\r\n?)[ \t]*\r\n?)(?: {4}|\t).+(?:(?:\n|\r\n?)(?: {4}|\t).+)*/,lookbehind:!0,alias:`keyword`},{pattern:/^```[\s\S]*?^```$/m,greedy:!0,inside:{"code-block":{pattern:/^(```.*(?:\n|\r\n?))[\s\S]+?(?=(?:\n|\r\n?)^```$)/m,lookbehind:!0},"code-language":{pattern:/^(```).+/,lookbehind:!0},punctuation:/```/}}],title:[{pattern:/\S.*(?:\n|\r\n?)(?:==+|--+)(?=[ \t]*$)/m,alias:`important`,inside:{punctuation:/==+$|--+$/}},{pattern:/(^\s*)#.+/m,lookbehind:!0,alias:`important`,inside:{punctuation:/^#+|#+$/}}],hr:{pattern:/(^\s*)([*-])(?:[\t ]*\2){2,}(?=\s*$)/m,lookbehind:!0,alias:`punctuation`},list:{pattern:/(^\s*)(?:[*+-]|\d+\.)(?=[\t ].)/m,lookbehind:!0,alias:`punctuation`},"url-reference":{pattern:/!?\[[^\]]+\]:[\t ]+(?:\S+|<(?:\\.|[^>\\])+>)(?:[\t ]+(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\)))?/,inside:{variable:{pattern:/^(!?\[)[^\]]+/,lookbehind:!0},string:/(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\))$/,punctuation:/^[\[\]!:]|[<>]/},alias:`url`},bold:{pattern:n(`\\b__(?:(?!_)<inner>|_(?:(?!_)<inner>)+_)+__\\b|\\*\\*(?:(?!\\*)<inner>|\\*(?:(?!\\*)<inner>)+\\*)+\\*\\*`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^..)[\s\S]+(?=..$)/,lookbehind:!0,inside:{}},punctuation:/\*\*|__/}},italic:{pattern:n(`\\b_(?:(?!_)<inner>|__(?:(?!_)<inner>)+__)+_\\b|\\*(?:(?!\\*)<inner>|\\*\\*(?:(?!\\*)<inner>)+\\*\\*)+\\*`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^.)[\s\S]+(?=.$)/,lookbehind:!0,inside:{}},punctuation:/[*_]/}},strike:{pattern:n(`(~~?)(?:(?!~)<inner>)+\\2`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^~~?)[\s\S]+(?=\1$)/,lookbehind:!0,inside:{}},punctuation:/~~?/}},"code-snippet":{pattern:/(^|[^\\`])(?:``[^`\r\n]+(?:`[^`\r\n]+)*``(?!`)|`[^`\r\n]+`(?!`))/,lookbehind:!0,greedy:!0,alias:[`code`,`keyword`]},url:{pattern:n(`!?\\[(?:(?!\\])<inner>)+\\](?:\\([^\\s)]+(?:[\\t ]+"(?:\\\\.|[^"\\\\])*")?\\)|[ \\t]?\\[(?:(?!\\])<inner>)+\\])`),lookbehind:!0,greedy:!0,inside:{operator:/^!/,content:{pattern:/(^\[)[^\]]+(?=\])/,lookbehind:!0,inside:{}},variable:{pattern:/(^\][ \t]?\[)[^\]]+(?=\]$)/,lookbehind:!0},url:{pattern:/(^\]\()[^\s)]+/,lookbehind:!0},string:{pattern:/(^[ \t]+)"(?:\\.|[^"\\])*"(?=\)$)/,lookbehind:!0}}}}),[`url`,`bold`,`italic`,`strike`].forEach(function(t){[`url`,`bold`,`italic`,`strike`,`code-snippet`].forEach(function(n){t!==n&&(e.languages.markdown[t].inside.content.inside[n]=e.languages.markdown[n])})}),e.hooks.add(`after-tokenize`,function(e){e.language!==`markdown`&&e.language!==`md`||function e(t){if(t&&typeof t!=`string`)for(var n=0,r=t.length;n<r;n++){var i,a=t[n];a.type===`code`?(i=a.content[1],a=a.content[3],i&&a&&i.type===`code-language`&&a.type===`code-block`&&typeof i.content==`string`&&(i=i.content.replace(/\b#/g,`sharp`).replace(/\b\+\+/g,`pp`),i=`language-`+(i=(/[a-z][\w-]*/i.exec(i)||[``])[0].toLowerCase()),a.alias?typeof a.alias==`string`?a.alias=[a.alias,i]:a.alias.push(i):a.alias=[i])):e(a.content)}}(e.tokens)}),e.hooks.add(`wrap`,function(t){if(t.type===`code-block`){for(var n=``,r=0,i=t.classes.length;r<i;r++){var a=t.classes[r],a=/language-(.+)/.exec(a);if(a){n=a[1];break}}var l,u=e.languages[n];u?t.content=e.highlight(function(e){return e=e.replace(o,``),e=e.replace(/&(\w{1,8}|#x?[\da-f]{1,8});/gi,function(e,t){var n;return(t=t.toLowerCase())[0]===`#`?(n=t[1]===`x`?parseInt(t.slice(2),16):Number(t.slice(1)),c(n)):s[t]||e})}(t.content),u,n):n&&n!==`none`&&e.plugins.autoloader&&(l=`md-`+new Date().valueOf()+`-`+Math.floor(0x2386f26fc10000*Math.random()),t.attributes.id=l,e.plugins.autoloader.loadLanguages(n,function(){var t=document.getElementById(l);t&&(t.innerHTML=e.highlight(t.textContent,e.languages[n],n))}))}}),RegExp(e.languages.markup.tag.pattern.source,`gi`)),s={amp:`&`,lt:`<`,gt:`>`,quot:`"`},c=String.fromCodePoint||String.fromCharCode;e.languages.md=e.languages.markdown}(X),X.languages.graphql={comment:/#.*/,description:{pattern:/(?:"""(?:[^"]|(?!""")")*"""|"(?:\\.|[^\\"\r\n])*")(?=\s*[a-z_])/i,greedy:!0,alias:`string`,inside:{"language-markdown":{pattern:/(^"(?:"")?)(?!\1)[\s\S]+(?=\1$)/,lookbehind:!0,inside:X.languages.markdown}}},string:{pattern:/"""(?:[^"]|(?!""")")*"""|"(?:\\.|[^\\"\r\n])*"/,greedy:!0},number:/(?:\B-|\b)\d+(?:\.\d+)?(?:e[+-]?\d+)?\b/i,boolean:/\b(?:false|true)\b/,variable:/\$[a-z_]\w*/i,directive:{pattern:/@[a-z_]\w*/i,alias:`function`},"attr-name":{pattern:/\b[a-z_]\w*(?=\s*(?:\((?:[^()"]|"(?:\\.|[^\\"\r\n])*")*\))?:)/i,greedy:!0},"atom-input":{pattern:/\b[A-Z]\w*Input\b/,alias:`class-name`},scalar:/\b(?:Boolean|Float|ID|Int|String)\b/,constant:/\b[A-Z][A-Z_\d]*\b/,"class-name":{pattern:/(\b(?:enum|implements|interface|on|scalar|type|union)\s+|&\s*|:\s*|\[)[A-Z_]\w*/,lookbehind:!0},fragment:{pattern:/(\bfragment\s+|\.{3}\s*(?!on\b))[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},"definition-mutation":{pattern:/(\bmutation\s+)[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},"definition-query":{pattern:/(\bquery\s+)[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},keyword:/\b(?:directive|enum|extend|fragment|implements|input|interface|mutation|on|query|repeatable|scalar|schema|subscription|type|union)\b/,operator:/[!=|&]|\.{3}/,"property-query":/\w+(?=\s*\()/,object:/\w+(?=\s*\{)/,punctuation:/[!(){}\[\]:=,]/,property:/\w+/},X.hooks.add(`after-tokenize`,function(e){if(e.language===`graphql`)for(var t=e.tokens.filter(function(e){return typeof e!=`string`&&e.type!==`comment`&&e.type!==`scalar`}),n=0;n<t.length;){var r=t[n++];if(r.type===`keyword`&&r.content===`mutation`){var i=[];if(d([`definition-mutation`,`punctuation`])&&u(1).content===`(`){n+=2;var a=f(/^\($/,/^\)$/);if(a===-1)continue;for(;n<a;n++){var o=u(0);o.type===`variable`&&(p(o,`variable-input`),i.push(o.content))}n=a+1}if(d([`punctuation`,`property-query`])&&u(0).content===`{`&&(n++,p(u(0),`property-mutation`),0<i.length)){var s=f(/^\{$/,/^\}$/);if(s!==-1)for(var c=n;c<s;c++){var l=t[c];l.type===`variable`&&0<=i.indexOf(l.content)&&p(l,`variable-input`)}}}}function u(e){return t[n+e]}function d(e,t){t||=0;for(var n=0;n<e.length;n++){var r=u(n+t);if(!r||r.type!==e[n])return}return 1}function f(e,r){for(var i=1,a=n;a<t.length;a++){var o=t[a],s=o.content;if(o.type===`punctuation`&&typeof s==`string`){if(e.test(s))i++;else if(r.test(s)&&--i===0)return a}}return-1}function p(e,t){var n=e.alias;n?Array.isArray(n)||(e.alias=n=[n]):e.alias=n=[],n.push(t)}}),X.languages.sql={comment:{pattern:/(^|[^\\])(?:\/\*[\s\S]*?\*\/|(?:--|\/\/|#).*)/,lookbehind:!0},variable:[{pattern:/@(["'`])(?:\\[\s\S]|(?!\1)[^\\])+\1/,greedy:!0},/@[\w.$]+/],string:{pattern:/(^|[^@\\])("|')(?:\\[\s\S]|(?!\2)[^\\]|\2\2)*\2/,greedy:!0,lookbehind:!0},identifier:{pattern:/(^|[^@\\])`(?:\\[\s\S]|[^`\\]|``)*`/,greedy:!0,lookbehind:!0,inside:{punctuation:/^`|`$/}},function:/\b(?:AVG|COUNT|FIRST|FORMAT|LAST|LCASE|LEN|MAX|MID|MIN|MOD|NOW|ROUND|SUM|UCASE)(?=\s*\()/i,keyword:/\b(?:ACTION|ADD|AFTER|ALGORITHM|ALL|ALTER|ANALYZE|ANY|APPLY|AS|ASC|AUTHORIZATION|AUTO_INCREMENT|BACKUP|BDB|BEGIN|BERKELEYDB|BIGINT|BINARY|BIT|BLOB|BOOL|BOOLEAN|BREAK|BROWSE|BTREE|BULK|BY|CALL|CASCADED?|CASE|CHAIN|CHAR(?:ACTER|SET)?|CHECK(?:POINT)?|CLOSE|CLUSTERED|COALESCE|COLLATE|COLUMNS?|COMMENT|COMMIT(?:TED)?|COMPUTE|CONNECT|CONSISTENT|CONSTRAINT|CONTAINS(?:TABLE)?|CONTINUE|CONVERT|CREATE|CROSS|CURRENT(?:_DATE|_TIME|_TIMESTAMP|_USER)?|CURSOR|CYCLE|DATA(?:BASES?)?|DATE(?:TIME)?|DAY|DBCC|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFINER|DELAYED|DELETE|DELIMITERS?|DENY|DESC|DESCRIBE|DETERMINISTIC|DISABLE|DISCARD|DISK|DISTINCT|DISTINCTROW|DISTRIBUTED|DO|DOUBLE|DROP|DUMMY|DUMP(?:FILE)?|DUPLICATE|ELSE(?:IF)?|ENABLE|ENCLOSED|END|ENGINE|ENUM|ERRLVL|ERRORS|ESCAPED?|EXCEPT|EXEC(?:UTE)?|EXISTS|EXIT|EXPLAIN|EXTENDED|FETCH|FIELDS|FILE|FILLFACTOR|FIRST|FIXED|FLOAT|FOLLOWING|FOR(?: EACH ROW)?|FORCE|FOREIGN|FREETEXT(?:TABLE)?|FROM|FULL|FUNCTION|GEOMETRY(?:COLLECTION)?|GLOBAL|GOTO|GRANT|GROUP|HANDLER|HASH|HAVING|HOLDLOCK|HOUR|IDENTITY(?:COL|_INSERT)?|IF|IGNORE|IMPORT|INDEX|INFILE|INNER|INNODB|INOUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|INVOKER|ISOLATION|ITERATE|JOIN|KEYS?|KILL|LANGUAGE|LAST|LEAVE|LEFT|LEVEL|LIMIT|LINENO|LINES|LINESTRING|LOAD|LOCAL|LOCK|LONG(?:BLOB|TEXT)|LOOP|MATCH(?:ED)?|MEDIUM(?:BLOB|INT|TEXT)|MERGE|MIDDLEINT|MINUTE|MODE|MODIFIES|MODIFY|MONTH|MULTI(?:LINESTRING|POINT|POLYGON)|NATIONAL|NATURAL|NCHAR|NEXT|NO|NONCLUSTERED|NULLIF|NUMERIC|OFF?|OFFSETS?|ON|OPEN(?:DATASOURCE|QUERY|ROWSET)?|OPTIMIZE|OPTION(?:ALLY)?|ORDER|OUT(?:ER|FILE)?|OVER|PARTIAL|PARTITION|PERCENT|PIVOT|PLAN|POINT|POLYGON|PRECEDING|PRECISION|PREPARE|PREV|PRIMARY|PRINT|PRIVILEGES|PROC(?:EDURE)?|PUBLIC|PURGE|QUICK|RAISERROR|READS?|REAL|RECONFIGURE|REFERENCES|RELEASE|RENAME|REPEAT(?:ABLE)?|REPLACE|REPLICATION|REQUIRE|RESIGNAL|RESTORE|RESTRICT|RETURN(?:ING|S)?|REVOKE|RIGHT|ROLLBACK|ROUTINE|ROW(?:COUNT|GUIDCOL|S)?|RTREE|RULE|SAVE(?:POINT)?|SCHEMA|SECOND|SELECT|SERIAL(?:IZABLE)?|SESSION(?:_USER)?|SET(?:USER)?|SHARE|SHOW|SHUTDOWN|SIMPLE|SMALLINT|SNAPSHOT|SOME|SONAME|SQL|START(?:ING)?|STATISTICS|STATUS|STRIPED|SYSTEM_USER|TABLES?|TABLESPACE|TEMP(?:ORARY|TABLE)?|TERMINATED|TEXT(?:SIZE)?|THEN|TIME(?:STAMP)?|TINY(?:BLOB|INT|TEXT)|TOP?|TRAN(?:SACTIONS?)?|TRIGGER|TRUNCATE|TSEQUAL|TYPES?|UNBOUNDED|UNCOMMITTED|UNDEFINED|UNION|UNIQUE|UNLOCK|UNPIVOT|UNSIGNED|UPDATE(?:TEXT)?|USAGE|USE|USER|USING|VALUES?|VAR(?:BINARY|CHAR|CHARACTER|YING)|VIEW|WAITFOR|WARNINGS|WHEN|WHERE|WHILE|WITH(?: ROLLUP|IN)?|WORK|WRITE(?:TEXT)?|YEAR)\b/i,boolean:/\b(?:FALSE|NULL|TRUE)\b/i,number:/\b0x[\da-f]+\b|\b\d+(?:\.\d*)?|\B\.\d+\b/i,operator:/[-+*\/=%^~]|&&?|\|\|?|!=?|<(?:=>?|<|>)?|>[>=]?|\b(?:AND|BETWEEN|DIV|ILIKE|IN|IS|LIKE|NOT|OR|REGEXP|RLIKE|SOUNDS LIKE|XOR)\b/i,punctuation:/[;[\]()`,.]/},function(e){var t=e.languages.javascript[`template-string`],n=t.pattern.source,r=t.inside.interpolation,i=r.inside[`interpolation-punctuation`],a=r.pattern.source;function o(t,r){if(e.languages[t])return{pattern:RegExp(`((?:`+r+`)\\s*)`+n),lookbehind:!0,greedy:!0,inside:{"template-punctuation":{pattern:/^`|`$/,alias:`string`},"embedded-code":{pattern:/[\s\S]+/,alias:t}}}}function s(t,n,r){return t={code:t,grammar:n,language:r},e.hooks.run(`before-tokenize`,t),t.tokens=e.tokenize(t.code,t.grammar),e.hooks.run(`after-tokenize`,t),t.tokens}function c(t,n,o){var c=e.tokenize(t,{interpolation:{pattern:RegExp(a),lookbehind:!0}}),l=0,u={},c=s(c.map(function(e){if(typeof e==`string`)return e;for(var n,r,e=e.content;t.indexOf((r=l++,n=`___`+o.toUpperCase()+`_`+r+`___`))!==-1;);return u[n]=e,n}).join(``),n,o),d=Object.keys(u);return l=0,function t(n){for(var a=0;a<n.length;a++){if(l>=d.length)return;var o,c,f,p,m,h,g,_=n[a];typeof _==`string`||typeof _.content==`string`?(o=d[l],(g=(h=typeof _==`string`?_:_.content).indexOf(o))!==-1&&(++l,c=h.substring(0,g),m=u[o],f=void 0,(p={})[`interpolation-punctuation`]=i,(p=e.tokenize(m,p)).length===3&&((f=[1,1]).push.apply(f,s(p[1],e.languages.javascript,`javascript`)),p.splice.apply(p,f)),f=new e.Token(`interpolation`,p,r.alias,m),p=h.substring(g+o.length),m=[],c&&m.push(c),m.push(f),p&&(t(h=[p]),m.push.apply(m,h)),typeof _==`string`?(n.splice.apply(n,[a,1].concat(m)),a+=m.length-1):_.content=m)):(g=_.content,t(Array.isArray(g)?g:[g]))}}(c),new e.Token(o,c,`language-`+o,t)}e.languages.javascript[`template-string`]=[o(`css`,`\\b(?:styled(?:\\([^)]*\\))?(?:\\s*\\.\\s*\\w+(?:\\([^)]*\\))*)*|css(?:\\s*\\.\\s*(?:global|resolve))?|createGlobalStyle|keyframes)`),o(`html`,`\\bhtml|\\.\\s*(?:inner|outer)HTML\\s*\\+?=`),o(`svg`,`\\bsvg`),o(`markdown`,`\\b(?:markdown|md)`),o(`graphql`,`\\b(?:gql|graphql(?:\\s*\\.\\s*experimental)?)`),o(`sql`,`\\bsql`),t].filter(Boolean);var l={javascript:!0,js:!0,typescript:!0,ts:!0,jsx:!0,tsx:!0};function u(e){return typeof e==`string`?e:Array.isArray(e)?e.map(u).join(``):u(e.content)}e.hooks.add(`after-tokenize`,function(t){t.language in l&&function t(n){for(var r=0,i=n.length;r<i;r++){var a,o,s,l=n[r];typeof l!=`string`&&(a=l.content,Array.isArray(a)?l.type===`template-string`?(l=a[1],a.length===3&&typeof l!=`string`&&l.type===`embedded-code`&&(o=u(l),l=l.alias,l=Array.isArray(l)?l[0]:l,s=e.languages[l])&&(a[1]=c(o,s,l))):t(a):typeof a!=`string`&&t([a]))}}(t.tokens)})}(X),function(e){e.languages.typescript=e.languages.extend(`javascript`,{"class-name":{pattern:/(\b(?:class|extends|implements|instanceof|interface|new|type)\s+)(?!keyof\b)(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?:\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)?/,lookbehind:!0,greedy:!0,inside:null},builtin:/\b(?:Array|Function|Promise|any|boolean|console|never|number|string|symbol|unknown)\b/}),e.languages.typescript.keyword.push(/\b(?:abstract|declare|is|keyof|readonly|require)\b/,/\b(?:asserts|infer|interface|module|namespace|type)\b(?=\s*(?:[{_$a-zA-Z\xA0-\uFFFF]|$))/,/\btype\b(?=\s*(?:[\{*]|$))/),delete e.languages.typescript.parameter,delete e.languages.typescript[`literal-property`];var t=e.languages.extend(`typescript`,{});delete t[`class-name`],e.languages.typescript[`class-name`].inside=t,e.languages.insertBefore(`typescript`,`function`,{decorator:{pattern:/@[$\w\xA0-\uFFFF]+/,inside:{at:{pattern:/^@/,alias:`operator`},function:/^[\s\S]+/}},"generic-function":{pattern:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>(?=\s*\()/,greedy:!0,inside:{function:/^#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*/,generic:{pattern:/<[\s\S]+/,alias:`class-name`,inside:t}}}}),e.languages.ts=e.languages.typescript}(X),function(e){var t=e.languages.javascript,n=`\\{(?:[^{}]|\\{(?:[^{}]|\\{[^{}]*\\})*\\})+\\}`,r=`(@(?:arg|argument|param|property)\\s+(?:`+n+`\\s+)?)`;e.languages.jsdoc=e.languages.extend(`javadoclike`,{parameter:{pattern:RegExp(r+`(?:(?!\\s)[$\\w\\xA0-\\uFFFF.])+(?=\\s|$)`),lookbehind:!0,inside:{punctuation:/\./}}}),e.languages.insertBefore(`jsdoc`,`keyword`,{"optional-parameter":{pattern:RegExp(r+`\\[(?:(?!\\s)[$\\w\\xA0-\\uFFFF.])+(?:=[^[\\]]+)?\\](?=\\s|$)`),lookbehind:!0,inside:{parameter:{pattern:/(^\[)[$\w\xA0-\uFFFF\.]+/,lookbehind:!0,inside:{punctuation:/\./}},code:{pattern:/(=)[\s\S]*(?=\]$)/,lookbehind:!0,inside:t,alias:`language-javascript`},punctuation:/[=[\]]/}},"class-name":[{pattern:RegExp(`(@(?:augments|class|extends|interface|memberof!?|template|this|typedef)\\s+(?:<TYPE>\\s+)?)[A-Z]\\w*(?:\\.[A-Z]\\w*)*`.replace(/<TYPE>/g,function(){return n})),lookbehind:!0,inside:{punctuation:/\./}},{pattern:RegExp(`(@[a-z]+\\s+)`+n),lookbehind:!0,inside:{string:t.string,number:t.number,boolean:t.boolean,keyword:e.languages.typescript.keyword,operator:/=>|\.\.\.|[&|?:*]/,punctuation:/[.,;=<>{}()[\]]/}}],example:{pattern:/(@example\s+(?!\s))(?:[^@\s]|\s+(?!\s))+?(?=\s*(?:\*\s*)?(?:@\w|\*\/))/,lookbehind:!0,inside:{code:{pattern:/^([\t ]*(?:\*\s*)?)\S.*$/m,lookbehind:!0,inside:t,alias:`language-javascript`}}}}),e.languages.javadoclike.addSupport(`javascript`,e.languages.jsdoc)}(X),function(e){e.languages.flow=e.languages.extend(`javascript`,{}),e.languages.insertBefore(`flow`,`keyword`,{type:[{pattern:/\b(?:[Bb]oolean|Function|[Nn]umber|[Ss]tring|[Ss]ymbol|any|mixed|null|void)\b/,alias:`class-name`}]}),e.languages.flow[`function-variable`].pattern=/(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*=\s*(?:function\b|(?:\([^()]*\)(?:\s*:\s*\w+)?|(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)\s*=>))/i,delete e.languages.flow.parameter,e.languages.insertBefore(`flow`,`operator`,{"flow-punctuation":{pattern:/\{\||\|\}/,alias:`punctuation`}}),Array.isArray(e.languages.flow.keyword)||(e.languages.flow.keyword=[e.languages.flow.keyword]),e.languages.flow.keyword.unshift({pattern:/(^|[^$]\b)(?:Class|declare|opaque|type)\b(?!\$)/,lookbehind:!0},{pattern:/(^|[^$]\B)\$(?:Diff|Enum|Exact|Keys|ObjMap|PropertyType|Record|Shape|Subtype|Supertype|await)\b(?!\$)/,lookbehind:!0})}(X),X.languages.n4js=X.languages.extend(`javascript`,{keyword:/\b(?:Array|any|boolean|break|case|catch|class|const|constructor|continue|debugger|declare|default|delete|do|else|enum|export|extends|false|finally|for|from|function|get|if|implements|import|in|instanceof|interface|let|module|new|null|number|package|private|protected|public|return|set|static|string|super|switch|this|throw|true|try|typeof|var|void|while|with|yield)\b/}),X.languages.insertBefore(`n4js`,`constant`,{annotation:{pattern:/@+\w+/,alias:`operator`}}),X.languages.n4jsd=X.languages.n4js,function(e){function t(e,t){return RegExp(e.replace(/<ID>/g,function(){return`(?!\\s)[_$a-zA-Z\\xA0-\\uFFFF](?:(?!\\s)[$\\w\\xA0-\\uFFFF])*`}),t)}e.languages.insertBefore(`javascript`,`function-variable`,{"method-variable":{pattern:RegExp(`(\\.\\s*)`+e.languages.javascript[`function-variable`].pattern.source),lookbehind:!0,alias:[`function-variable`,`method`,`function`,`property-access`]}}),e.languages.insertBefore(`javascript`,`function`,{method:{pattern:RegExp(`(\\.\\s*)`+e.languages.javascript.function.source),lookbehind:!0,alias:[`function`,`property-access`]}}),e.languages.insertBefore(`javascript`,`constant`,{"known-class-name":[{pattern:/\b(?:(?:Float(?:32|64)|(?:Int|Uint)(?:8|16|32)|Uint8Clamped)?Array|ArrayBuffer|BigInt|Boolean|DataView|Date|Error|Function|Intl|JSON|(?:Weak)?(?:Map|Set)|Math|Number|Object|Promise|Proxy|Reflect|RegExp|String|Symbol|WebAssembly)\b/,alias:`class-name`},{pattern:/\b(?:[A-Z]\w*)Error\b/,alias:`class-name`}]}),e.languages.insertBefore(`javascript`,`keyword`,{imports:{pattern:t(`(\\bimport\\b\\s*)(?:<ID>(?:\\s*,\\s*(?:\\*\\s*as\\s+<ID>|\\{[^{}]*\\}))?|\\*\\s*as\\s+<ID>|\\{[^{}]*\\})(?=\\s*\\bfrom\\b)`),lookbehind:!0,inside:e.languages.javascript},exports:{pattern:t(`(\\bexport\\b\\s*)(?:\\*(?:\\s*as\\s+<ID>)?(?=\\s*\\bfrom\\b)|\\{[^{}]*\\})`),lookbehind:!0,inside:e.languages.javascript}}),e.languages.javascript.keyword.unshift({pattern:/\b(?:as|default|export|from|import)\b/,alias:`module`},{pattern:/\b(?:await|break|catch|continue|do|else|finally|for|if|return|switch|throw|try|while|yield)\b/,alias:`control-flow`},{pattern:/\bnull\b/,alias:[`null`,`nil`]},{pattern:/\bundefined\b/,alias:`nil`}),e.languages.insertBefore(`javascript`,`operator`,{spread:{pattern:/\.{3}/,alias:`operator`},arrow:{pattern:/=>/,alias:`operator`}}),e.languages.insertBefore(`javascript`,`punctuation`,{"property-access":{pattern:t(`(\\.\\s*)#?<ID>`),lookbehind:!0},"maybe-class-name":{pattern:/(^|[^$\w\xA0-\uFFFF])[A-Z][$\w\xA0-\uFFFF]+/,lookbehind:!0},dom:{pattern:/\b(?:document|(?:local|session)Storage|location|navigator|performance|window)\b/,alias:`variable`},console:{pattern:/\bconsole(?=\s*\.)/,alias:`class-name`}});for(var n=[`function`,`function-variable`,`method`,`method-variable`,`property-access`],r=0;r<n.length;r++){var i=n[r],a=e.languages.javascript[i],i=(a=e.util.type(a)===`RegExp`?e.languages.javascript[i]={pattern:a}:a).inside||{};(a.inside=i)[`maybe-class-name`]=/^[A-Z][\s\S]*/}}(X),function(e){var t=e.util.clone(e.languages.javascript),n=`(?:\\s|\\/\\/.*(?!.)|\\/\\*(?:[^*]|\\*(?!\\/))\\*\\/)`,r=`(?:\\{(?:\\{(?:\\{[^{}]*\\}|[^{}])*\\}|[^{}])*\\})`,i=`(?:\\{<S>*\\.{3}(?:[^{}]|<BRACES>)*\\})`;function a(e,t){return e=e.replace(/<S>/g,function(){return n}).replace(/<BRACES>/g,function(){return r}).replace(/<SPREAD>/g,function(){return i}),RegExp(e,t)}i=a(i).source,e.languages.jsx=e.languages.extend(`markup`,t),e.languages.jsx.tag.pattern=a(`<\\/?(?:[\\w.:-]+(?:<S>+(?:[\\w.:$-]+(?:=(?:"(?:\\\\[\\s\\S]|[^\\\\"])*"|'(?:\\\\[\\s\\S]|[^\\\\'])*'|[^\\s{'"/>=]+|<BRACES>))?|<SPREAD>))*<S>*\\/?)?>`),e.languages.jsx.tag.inside.tag.pattern=/^<\/?[^\s>\/]*/,e.languages.jsx.tag.inside[`attr-value`].pattern=/=(?!\{)(?:"(?:\\[\s\S]|[^\\"])*"|'(?:\\[\s\S]|[^\\'])*'|[^\s'">]+)/,e.languages.jsx.tag.inside.tag.inside[`class-name`]=/^[A-Z]\w*(?:\.[A-Z]\w*)*$/,e.languages.jsx.tag.inside.comment=t.comment,e.languages.insertBefore(`inside`,`attr-name`,{spread:{pattern:a(`<SPREAD>`),inside:e.languages.jsx}},e.languages.jsx.tag),e.languages.insertBefore(`inside`,`special-attr`,{script:{pattern:a(`=<BRACES>`),alias:`language-javascript`,inside:{"script-punctuation":{pattern:/^=(?=\{)/,alias:`punctuation`},rest:e.languages.jsx}}},e.languages.jsx.tag);function o(t){for(var n=[],r=0;r<t.length;r++){var i=t[r],a=!1;typeof i!=`string`&&(i.type===`tag`&&i.content[0]&&i.content[0].type===`tag`?i.content[0].content[0].content===`</`?0<n.length&&n[n.length-1].tagName===s(i.content[0].content[1])&&n.pop():i.content[i.content.length-1].content!==`/>`&&n.push({tagName:s(i.content[0].content[1]),openedBraces:0}):0<n.length&&i.type===`punctuation`&&i.content===`{`?n[n.length-1].openedBraces++:0<n.length&&0<n[n.length-1].openedBraces&&i.type===`punctuation`&&i.content===`}`?n[n.length-1].openedBraces--:a=!0),(a||typeof i==`string`)&&0<n.length&&n[n.length-1].openedBraces===0&&(a=s(i),r<t.length-1&&(typeof t[r+1]==`string`||t[r+1].type===`plain-text`)&&(a+=s(t[r+1]),t.splice(r+1,1)),0<r&&(typeof t[r-1]==`string`||t[r-1].type===`plain-text`)&&(a=s(t[r-1])+a,t.splice(r-1,1),r--),t[r]=new e.Token(`plain-text`,a,null,a)),i.content&&typeof i.content!=`string`&&o(i.content)}}var s=function(e){return e?typeof e==`string`?e:typeof e.content==`string`?e.content:e.content.map(s).join(``):``};e.hooks.add(`after-tokenize`,function(e){e.language!==`jsx`&&e.language!==`tsx`||o(e.tokens)})}(X),function(e){var t=e.util.clone(e.languages.typescript),t=(e.languages.tsx=e.languages.extend(`jsx`,t),delete e.languages.tsx.parameter,delete e.languages.tsx[`literal-property`],e.languages.tsx.tag);t.pattern=RegExp(`(^|[^\\w$]|(?=<\\/))(?:`+t.pattern.source+`)`,t.pattern.flags),t.lookbehind=!0}(X),X.languages.swift={comment:{pattern:/(^|[^\\:])(?:\/\/.*|\/\*(?:[^/*]|\/(?!\*)|\*(?!\/)|\/\*(?:[^*]|\*(?!\/))*\*\/)*\*\/)/,lookbehind:!0,greedy:!0},"string-literal":[{pattern:RegExp(`(^|[^"#])(?:"(?:\\\\(?:\\((?:[^()]|\\([^()]*\\))*\\)|\\r\\n|[^(])|[^\\\\\\r\\n"])*"|"""(?:\\\\(?:\\((?:[^()]|\\([^()]*\\))*\\)|[^(])|[^\\\\"]|"(?!""))*""")(?!["#])`),lookbehind:!0,greedy:!0,inside:{interpolation:{pattern:/(\\\()(?:[^()]|\([^()]*\))*(?=\))/,lookbehind:!0,inside:null},"interpolation-punctuation":{pattern:/^\)|\\\($/,alias:`punctuation`},punctuation:/\\(?=[\r\n])/,string:/[\s\S]+/}},{pattern:RegExp(`(^|[^"#])(#+)(?:"(?:\\\\(?:#+\\((?:[^()]|\\([^()]*\\))*\\)|\\r\\n|[^#])|[^\\\\\\r\\n])*?"|"""(?:\\\\(?:#+\\((?:[^()]|\\([^()]*\\))*\\)|[^#])|[^\\\\])*?""")\\2`),lookbehind:!0,greedy:!0,inside:{interpolation:{pattern:/(\\#+\()(?:[^()]|\([^()]*\))*(?=\))/,lookbehind:!0,inside:null},"interpolation-punctuation":{pattern:/^\)|\\#+\($/,alias:`punctuation`},string:/[\s\S]+/}}],directive:{pattern:RegExp(`#(?:(?:elseif|if)\\b(?:[ 	]*(?:![ \\t]*)?(?:\\b\\w+\\b(?:[ \\t]*\\((?:[^()]|\\([^()]*\\))*\\))?|\\((?:[^()]|\\([^()]*\\))*\\))(?:[ \\t]*(?:&&|\\|\\|))?)+|(?:else|endif)\\b)`),alias:`property`,inside:{"directive-name":/^#\w+/,boolean:/\b(?:false|true)\b/,number:/\b\d+(?:\.\d+)*\b/,operator:/!|&&|\|\||[<>]=?/,punctuation:/[(),]/}},literal:{pattern:/#(?:colorLiteral|column|dsohandle|file(?:ID|Literal|Path)?|function|imageLiteral|line)\b/,alias:`constant`},"other-directive":{pattern:/#\w+\b/,alias:`property`},attribute:{pattern:/@\w+/,alias:`atrule`},"function-definition":{pattern:/(\bfunc\s+)\w+/,lookbehind:!0,alias:`function`},label:{pattern:/\b(break|continue)\s+\w+|\b[a-zA-Z_]\w*(?=\s*:\s*(?:for|repeat|while)\b)/,lookbehind:!0,alias:`important`},keyword:/\b(?:Any|Protocol|Self|Type|actor|as|assignment|associatedtype|associativity|async|await|break|case|catch|class|continue|convenience|default|defer|deinit|didSet|do|dynamic|else|enum|extension|fallthrough|fileprivate|final|for|func|get|guard|higherThan|if|import|in|indirect|infix|init|inout|internal|is|isolated|lazy|left|let|lowerThan|mutating|none|nonisolated|nonmutating|open|operator|optional|override|postfix|precedencegroup|prefix|private|protocol|public|repeat|required|rethrows|return|right|safe|self|set|some|static|struct|subscript|super|switch|throw|throws|try|typealias|unowned|unsafe|var|weak|where|while|willSet)\b/,boolean:/\b(?:false|true)\b/,nil:{pattern:/\bnil\b/,alias:`constant`},"short-argument":/\$\d+\b/,omit:{pattern:/\b_\b/,alias:`keyword`},number:/\b(?:[\d_]+(?:\.[\de_]+)?|0x[a-f0-9_]+(?:\.[a-f0-9p_]+)?|0b[01_]+|0o[0-7_]+)\b/i,"class-name":/\b[A-Z](?:[A-Z_\d]*[a-z]\w*)?\b/,function:/\b[a-z_]\w*(?=\s*\()/i,constant:/\b(?:[A-Z_]{2,}|k[A-Z][A-Za-z_]+)\b/,operator:/[-+*/%=!<>&|^~?]+|\.[.\-+*/%=!<>&|^~?]+/,punctuation:/[{}[\]();,.:\\]/},X.languages.swift[`string-literal`].forEach(function(e){e.inside.interpolation.inside=X.languages.swift}),function(e){e.languages.kotlin=e.languages.extend(`clike`,{keyword:{pattern:/(^|[^.])\b(?:abstract|actual|annotation|as|break|by|catch|class|companion|const|constructor|continue|crossinline|data|do|dynamic|else|enum|expect|external|final|finally|for|fun|get|if|import|in|infix|init|inline|inner|interface|internal|is|lateinit|noinline|null|object|open|operator|out|override|package|private|protected|public|reified|return|sealed|set|super|suspend|tailrec|this|throw|to|try|typealias|val|var|vararg|when|where|while)\b/,lookbehind:!0},function:[{pattern:/(?:`[^\r\n`]+`|\b\w+)(?=\s*\()/,greedy:!0},{pattern:/(\.)(?:`[^\r\n`]+`|\w+)(?=\s*\{)/,lookbehind:!0,greedy:!0}],number:/\b(?:0[xX][\da-fA-F]+(?:_[\da-fA-F]+)*|0[bB][01]+(?:_[01]+)*|\d+(?:_\d+)*(?:\.\d+(?:_\d+)*)?(?:[eE][+-]?\d+(?:_\d+)*)?[fFL]?)\b/,operator:/\+[+=]?|-[-=>]?|==?=?|!(?:!|==?)?|[\/*%<>]=?|[?:]:?|\.\.|&&|\|\||\b(?:and|inv|or|shl|shr|ushr|xor)\b/}),delete e.languages.kotlin[`class-name`];var t={"interpolation-punctuation":{pattern:/^\$\{?|\}$/,alias:`punctuation`},expression:{pattern:/[\s\S]+/,inside:e.languages.kotlin}};e.languages.insertBefore(`kotlin`,`string`,{"string-literal":[{pattern:/"""(?:[^$]|\$(?:(?!\{)|\{[^{}]*\}))*?"""/,alias:`multiline`,inside:{interpolation:{pattern:/\$(?:[a-z_]\w*|\{[^{}]*\})/i,inside:t},string:/[\s\S]+/}},{pattern:/"(?:[^"\\\r\n$]|\\.|\$(?:(?!\{)|\{[^{}]*\}))*"/,alias:`singleline`,inside:{interpolation:{pattern:/((?:^|[^\\])(?:\\{2})*)\$(?:[a-z_]\w*|\{[^{}]*\})/i,lookbehind:!0,inside:t},string:/[\s\S]+/}}],char:{pattern:/'(?:[^'\\\r\n]|\\(?:.|u[a-fA-F0-9]{0,4}))'/,greedy:!0}}),delete e.languages.kotlin.string,e.languages.insertBefore(`kotlin`,`keyword`,{annotation:{pattern:/\B@(?:\w+:)?(?:[A-Z]\w*|\[[^\]]+\])/,alias:`builtin`}}),e.languages.insertBefore(`kotlin`,`function`,{label:{pattern:/\b\w+@|@\w+\b/,alias:`symbol`}}),e.languages.kt=e.languages.kotlin,e.languages.kts=e.languages.kotlin}(X),X.languages.c=X.languages.extend(`clike`,{comment:{pattern:/\/\/(?:[^\r\n\\]|\\(?:\r\n?|\n|(?![\r\n])))*|\/\*[\s\S]*?(?:\*\/|$)/,greedy:!0},string:{pattern:/"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"/,greedy:!0},"class-name":{pattern:/(\b(?:enum|struct)\s+(?:__attribute__\s*\(\([\s\S]*?\)\)\s*)?)\w+|\b[a-z]\w*_t\b/,lookbehind:!0},keyword:/\b(?:_Alignas|_Alignof|_Atomic|_Bool|_Complex|_Generic|_Imaginary|_Noreturn|_Static_assert|_Thread_local|__attribute__|asm|auto|break|case|char|const|continue|default|do|double|else|enum|extern|float|for|goto|if|inline|int|long|register|return|short|signed|sizeof|static|struct|switch|typedef|typeof|union|unsigned|void|volatile|while)\b/,function:/\b[a-z_]\w*(?=\s*\()/i,number:/(?:\b0x(?:[\da-f]+(?:\.[\da-f]*)?|\.[\da-f]+)(?:p[+-]?\d+)?|(?:\b\d+(?:\.\d*)?|\B\.\d+)(?:e[+-]?\d+)?)[ful]{0,4}/i,operator:/>>=?|<<=?|->|([-+&|:])\1|[?:~]|[-+*/%&|^!=<>]=?/}),X.languages.insertBefore(`c`,`string`,{char:{pattern:/'(?:\\(?:\r\n|[\s\S])|[^'\\\r\n]){0,32}'/,greedy:!0}}),X.languages.insertBefore(`c`,`string`,{macro:{pattern:/(^[\t ]*)#\s*[a-z](?:[^\r\n\\/]|\/(?!\*)|\/\*(?:[^*]|\*(?!\/))*\*\/|\\(?:\r\n|[\s\S]))*/im,lookbehind:!0,greedy:!0,alias:`property`,inside:{string:[{pattern:/^(#\s*include\s*)<[^>]+>/,lookbehind:!0},X.languages.c.string],char:X.languages.c.char,comment:X.languages.c.comment,"macro-name":[{pattern:/(^#\s*define\s+)\w+\b(?!\()/i,lookbehind:!0},{pattern:/(^#\s*define\s+)\w+\b(?=\()/i,lookbehind:!0,alias:`function`}],directive:{pattern:/^(#\s*)[a-z]+/,lookbehind:!0,alias:`keyword`},"directive-hash":/^#/,punctuation:/##|\\(?=[\r\n])/,expression:{pattern:/\S[\s\S]*/,inside:X.languages.c}}}}),X.languages.insertBefore(`c`,`function`,{constant:/\b(?:EOF|NULL|SEEK_CUR|SEEK_END|SEEK_SET|__DATE__|__FILE__|__LINE__|__TIMESTAMP__|__TIME__|__func__|stderr|stdin|stdout)\b/}),delete X.languages.c.boolean,X.languages.objectivec=X.languages.extend(`c`,{string:{pattern:/@?"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"/,greedy:!0},keyword:/\b(?:asm|auto|break|case|char|const|continue|default|do|double|else|enum|extern|float|for|goto|if|in|inline|int|long|register|return|self|short|signed|sizeof|static|struct|super|switch|typedef|typeof|union|unsigned|void|volatile|while)\b|(?:@interface|@end|@implementation|@protocol|@class|@public|@protected|@private|@property|@try|@catch|@finally|@throw|@synthesize|@dynamic|@selector)\b/,operator:/-[->]?|\+\+?|!=?|<<?=?|>>?=?|==?|&&?|\|\|?|[~^%?*\/@]/}),delete X.languages.objectivec[`class-name`],X.languages.objc=X.languages.objectivec,X.languages.reason=X.languages.extend(`clike`,{string:{pattern:/"(?:\\(?:\r\n|[\s\S])|[^\\\r\n"])*"/,greedy:!0},"class-name":/\b[A-Z]\w*/,keyword:/\b(?:and|as|assert|begin|class|constraint|do|done|downto|else|end|exception|external|for|fun|function|functor|if|in|include|inherit|initializer|lazy|let|method|module|mutable|new|nonrec|object|of|open|or|private|rec|sig|struct|switch|then|to|try|type|val|virtual|when|while|with)\b/,operator:/\.{3}|:[:=]|\|>|->|=(?:==?|>)?|<=?|>=?|[|^?'#!~`]|[+\-*\/]\.?|\b(?:asr|land|lor|lsl|lsr|lxor|mod)\b/}),X.languages.insertBefore(`reason`,`class-name`,{char:{pattern:/'(?:\\x[\da-f]{2}|\\o[0-3][0-7][0-7]|\\\d{3}|\\.|[^'\\\r\n])'/,greedy:!0},constructor:/\b[A-Z]\w*\b(?!\s*\.)/,label:{pattern:/\b[a-z]\w*(?=::)/,alias:`symbol`}}),delete X.languages.reason.function,function(e){for(var t=`\\/\\*(?:[^*/]|\\*(?!\\/)|\\/(?!\\*)|<self>)*\\*\\/`,n=0;n<2;n++)t=t.replace(/<self>/g,function(){return t});t=t.replace(/<self>/g,function(){return`[^\\s\\S]`}),e.languages.rust={comment:[{pattern:RegExp(`(^|[^\\\\])`+t),lookbehind:!0,greedy:!0},{pattern:/(^|[^\\:])\/\/.*/,lookbehind:!0,greedy:!0}],string:{pattern:/b?"(?:\\[\s\S]|[^\\"])*"|b?r(#*)"(?:[^"]|"(?!\1))*"\1/,greedy:!0},char:{pattern:/b?'(?:\\(?:x[0-7][\da-fA-F]|u\{(?:[\da-fA-F]_*){1,6}\}|.)|[^\\\r\n\t'])'/,greedy:!0},attribute:{pattern:/#!?\[(?:[^\[\]"]|"(?:\\[\s\S]|[^\\"])*")*\]/,greedy:!0,alias:`attr-name`,inside:{string:null}},"closure-params":{pattern:/([=(,:]\s*|\bmove\s*)\|[^|]*\||\|[^|]*\|(?=\s*(?:\{|->))/,lookbehind:!0,greedy:!0,inside:{"closure-punctuation":{pattern:/^\||\|$/,alias:`punctuation`},rest:null}},"lifetime-annotation":{pattern:/'\w+/,alias:`symbol`},"fragment-specifier":{pattern:/(\$\w+:)[a-z]+/,lookbehind:!0,alias:`punctuation`},variable:/\$\w+/,"function-definition":{pattern:/(\bfn\s+)\w+/,lookbehind:!0,alias:`function`},"type-definition":{pattern:/(\b(?:enum|struct|trait|type|union)\s+)\w+/,lookbehind:!0,alias:`class-name`},"module-declaration":[{pattern:/(\b(?:crate|mod)\s+)[a-z][a-z_\d]*/,lookbehind:!0,alias:`namespace`},{pattern:/(\b(?:crate|self|super)\s*)::\s*[a-z][a-z_\d]*\b(?:\s*::(?:\s*[a-z][a-z_\d]*\s*::)*)?/,lookbehind:!0,alias:`namespace`,inside:{punctuation:/::/}}],keyword:[/\b(?:Self|abstract|as|async|await|become|box|break|const|continue|crate|do|dyn|else|enum|extern|final|fn|for|if|impl|in|let|loop|macro|match|mod|move|mut|override|priv|pub|ref|return|self|static|struct|super|trait|try|type|typeof|union|unsafe|unsized|use|virtual|where|while|yield)\b/,/\b(?:bool|char|f(?:32|64)|[ui](?:8|16|32|64|128|size)|str)\b/],function:/\b[a-z_]\w*(?=\s*(?:::\s*<|\())/,macro:{pattern:/\b\w+!/,alias:`property`},constant:/\b[A-Z_][A-Z_\d]+\b/,"class-name":/\b[A-Z]\w*\b/,namespace:{pattern:/(?:\b[a-z][a-z_\d]*\s*::\s*)*\b[a-z][a-z_\d]*\s*::(?!\s*<)/,inside:{punctuation:/::/}},number:/\b(?:0x[\dA-Fa-f](?:_?[\dA-Fa-f])*|0o[0-7](?:_?[0-7])*|0b[01](?:_?[01])*|(?:(?:\d(?:_?\d)*)?\.)?\d(?:_?\d)*(?:[Ee][+-]?\d+)?)(?:_?(?:f32|f64|[iu](?:8|16|32|64|size)?))?\b/,boolean:/\b(?:false|true)\b/,punctuation:/->|\.\.=|\.{1,3}|::|[{}[\];(),:]/,operator:/[-+*\/%!^]=?|=[=>]?|&[&=]?|\|[|=]?|<<?=?|>>?=?|[@?]/},e.languages.rust[`closure-params`].inside.rest=e.languages.rust,e.languages.rust.attribute.inside.string=e.languages.rust.string}(X),X.languages.go=X.languages.extend(`clike`,{string:{pattern:/(^|[^\\])"(?:\\.|[^"\\\r\n])*"|`[^`]*`/,lookbehind:!0,greedy:!0},keyword:/\b(?:break|case|chan|const|continue|default|defer|else|fallthrough|for|func|go(?:to)?|if|import|interface|map|package|range|return|select|struct|switch|type|var)\b/,boolean:/\b(?:_|false|iota|nil|true)\b/,number:[/\b0(?:b[01_]+|o[0-7_]+)i?\b/i,/\b0x(?:[a-f\d_]+(?:\.[a-f\d_]*)?|\.[a-f\d_]+)(?:p[+-]?\d+(?:_\d+)*)?i?(?!\w)/i,/(?:\b\d[\d_]*(?:\.[\d_]*)?|\B\.\d[\d_]*)(?:e[+-]?[\d_]+)?i?(?!\w)/i],operator:/[*\/%^!=]=?|\+[=+]?|-[=-]?|\|[=|]?|&(?:=|&|\^=?)?|>(?:>=?|=)?|<(?:<=?|=|-)?|:=|\.\.\./,builtin:/\b(?:append|bool|byte|cap|close|complex|complex(?:64|128)|copy|delete|error|float(?:32|64)|u?int(?:8|16|32|64)?|imag|len|make|new|panic|print(?:ln)?|real|recover|rune|string|uintptr)\b/}),X.languages.insertBefore(`go`,`string`,{char:{pattern:/'(?:\\.|[^'\\\r\n]){0,10}'/,greedy:!0}}),delete X.languages.go[`class-name`],function(e){var t=/\b(?:alignas|alignof|asm|auto|bool|break|case|catch|char|char16_t|char32_t|char8_t|class|co_await|co_return|co_yield|compl|concept|const|const_cast|consteval|constexpr|constinit|continue|decltype|default|delete|do|double|dynamic_cast|else|enum|explicit|export|extern|final|float|for|friend|goto|if|import|inline|int|int16_t|int32_t|int64_t|int8_t|long|module|mutable|namespace|new|noexcept|nullptr|operator|override|private|protected|public|register|reinterpret_cast|requires|return|short|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|throw|try|typedef|typeid|typename|uint16_t|uint32_t|uint64_t|uint8_t|union|unsigned|using|virtual|void|volatile|wchar_t|while)\b/,n=`\\b(?!<keyword>)\\w+(?:\\s*\\.\\s*\\w+)*\\b`.replace(/<keyword>/g,function(){return t.source});e.languages.cpp=e.languages.extend(`c`,{"class-name":[{pattern:RegExp(`(\\b(?:class|concept|enum|struct|typename)\\s+)(?!<keyword>)\\w+`.replace(/<keyword>/g,function(){return t.source})),lookbehind:!0},/\b[A-Z]\w*(?=\s*::\s*\w+\s*\()/,/\b[A-Z_]\w*(?=\s*::\s*~\w+\s*\()/i,/\b\w+(?=\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>\s*::\s*\w+\s*\()/],keyword:t,number:{pattern:/(?:\b0b[01']+|\b0x(?:[\da-f']+(?:\.[\da-f']*)?|\.[\da-f']+)(?:p[+-]?[\d']+)?|(?:\b[\d']+(?:\.[\d']*)?|\B\.[\d']+)(?:e[+-]?[\d']+)?)[ful]{0,4}/i,greedy:!0},operator:/>>=?|<<=?|->|--|\+\+|&&|\|\||[?:~]|<=>|[-+*/%&|^!=<>]=?|\b(?:and|and_eq|bitand|bitor|not|not_eq|or|or_eq|xor|xor_eq)\b/,boolean:/\b(?:false|true)\b/}),e.languages.insertBefore(`cpp`,`string`,{module:{pattern:RegExp(`(\\b(?:import|module)\\s+)(?:"(?:\\\\(?:\\r\\n|[\\s\\S])|[^"\\\\\\r\\n])*"|<[^<>\\r\\n]*>|`+`<mod-name>(?:\\s*:\\s*<mod-name>)?|:\\s*<mod-name>`.replace(/<mod-name>/g,function(){return n})+`)`),lookbehind:!0,greedy:!0,inside:{string:/^[<"][\s\S]+/,operator:/:/,punctuation:/\./}},"raw-string":{pattern:/R"([^()\\ ]{0,16})\([\s\S]*?\)\1"/,alias:`string`,greedy:!0}}),e.languages.insertBefore(`cpp`,`keyword`,{"generic-function":{pattern:/\b(?!operator\b)[a-z_]\w*\s*<(?:[^<>]|<[^<>]*>)*>(?=\s*\()/i,inside:{function:/^\w+/,generic:{pattern:/<[\s\S]+/,alias:`class-name`,inside:e.languages.cpp}}}}),e.languages.insertBefore(`cpp`,`operator`,{"double-colon":{pattern:/::/,alias:`punctuation`}}),e.languages.insertBefore(`cpp`,`class-name`,{"base-clause":{pattern:/(\b(?:class|struct)\s+\w+\s*:\s*)[^;{}"'\s]+(?:\s+[^;{}"'\s]+)*(?=\s*[;{])/,lookbehind:!0,greedy:!0,inside:e.languages.extend(`cpp`,{})}}),e.languages.insertBefore(`inside`,`double-colon`,{"class-name":/\b[a-z_]\w*\b(?!\s*::)/i},e.languages.cpp[`base-clause`])}(X),X.languages.python={comment:{pattern:/(^|[^\\])#.*/,lookbehind:!0,greedy:!0},"string-interpolation":{pattern:/(?:f|fr|rf)(?:("""|''')[\s\S]*?\1|("|')(?:\\.|(?!\2)[^\\\r\n])*\2)/i,greedy:!0,inside:{interpolation:{pattern:/((?:^|[^{])(?:\{\{)*)\{(?!\{)(?:[^{}]|\{(?!\{)(?:[^{}]|\{(?!\{)(?:[^{}])+\})+\})+\}/,lookbehind:!0,inside:{"format-spec":{pattern:/(:)[^:(){}]+(?=\}$)/,lookbehind:!0},"conversion-option":{pattern:/![sra](?=[:}]$)/,alias:`punctuation`},rest:null}},string:/[\s\S]+/}},"triple-quoted-string":{pattern:/(?:[rub]|br|rb)?("""|''')[\s\S]*?\1/i,greedy:!0,alias:`string`},string:{pattern:/(?:[rub]|br|rb)?("|')(?:\\.|(?!\1)[^\\\r\n])*\1/i,greedy:!0},function:{pattern:/((?:^|\s)def[ \t]+)[a-zA-Z_]\w*(?=\s*\()/g,lookbehind:!0},"class-name":{pattern:/(\bclass\s+)\w+/i,lookbehind:!0},decorator:{pattern:/(^[\t ]*)@\w+(?:\.\w+)*/m,lookbehind:!0,alias:[`annotation`,`punctuation`],inside:{punctuation:/\./}},keyword:/\b(?:_(?=\s*:)|and|as|assert|async|await|break|case|class|continue|def|del|elif|else|except|exec|finally|for|from|global|if|import|in|is|lambda|match|nonlocal|not|or|pass|print|raise|return|try|while|with|yield)\b/,builtin:/\b(?:__import__|abs|all|any|apply|ascii|basestring|bin|bool|buffer|bytearray|bytes|callable|chr|classmethod|cmp|coerce|compile|complex|delattr|dict|dir|divmod|enumerate|eval|execfile|file|filter|float|format|frozenset|getattr|globals|hasattr|hash|help|hex|id|input|int|intern|isinstance|issubclass|iter|len|list|locals|long|map|max|memoryview|min|next|object|oct|open|ord|pow|property|range|raw_input|reduce|reload|repr|reversed|round|set|setattr|slice|sorted|staticmethod|str|sum|super|tuple|type|unichr|unicode|vars|xrange|zip)\b/,boolean:/\b(?:False|None|True)\b/,number:/\b0(?:b(?:_?[01])+|o(?:_?[0-7])+|x(?:_?[a-f0-9])+)\b|(?:\b\d+(?:_\d+)*(?:\.(?:\d+(?:_\d+)*)?)?|\B\.\d+(?:_\d+)*)(?:e[+-]?\d+(?:_\d+)*)?j?(?!\w)/i,operator:/[-+%=]=?|!=|:=|\*\*?=?|\/\/?=?|<[<=>]?|>[=>]?|[&|^~]/,punctuation:/[{}[\];(),.:]/},X.languages.python[`string-interpolation`].inside.interpolation.inside.rest=X.languages.python,X.languages.py=X.languages.python,X.languages.json={property:{pattern:/(^|[^\\])"(?:\\.|[^\\"\r\n])*"(?=\s*:)/,lookbehind:!0,greedy:!0},string:{pattern:/(^|[^\\])"(?:\\.|[^\\"\r\n])*"(?!\s*:)/,lookbehind:!0,greedy:!0},comment:{pattern:/\/\/.*|\/\*[\s\S]*?(?:\*\/|$)/,greedy:!0},number:/-?\b\d+(?:\.\d+)?(?:e[+-]?\d+)?\b/i,punctuation:/[{}[\],]/,operator:/:/,boolean:/\b(?:false|true)\b/,null:{pattern:/\bnull\b/,alias:`keyword`}},X.languages.webmanifest=X.languages.json;var vv={};gv(vv,{dracula:()=>yv,duotoneDark:()=>bv,duotoneLight:()=>xv,github:()=>Sv,gruvboxMaterialDark:()=>Rv,gruvboxMaterialLight:()=>zv,jettwaveDark:()=>Pv,jettwaveLight:()=>Fv,nightOwl:()=>Cv,nightOwlLight:()=>wv,oceanicNext:()=>Ev,okaidia:()=>Dv,oneDark:()=>Iv,oneLight:()=>Lv,palenight:()=>Ov,shadesOfPurple:()=>kv,synthwave84:()=>Av,ultramin:()=>jv,vsDark:()=>Mv,vsLight:()=>Nv});var yv={plain:{color:`#F8F8F2`,backgroundColor:`#282A36`},styles:[{types:[`prolog`,`constant`,`builtin`],style:{color:`rgb(189, 147, 249)`}},{types:[`inserted`,`function`],style:{color:`rgb(80, 250, 123)`}},{types:[`deleted`],style:{color:`rgb(255, 85, 85)`}},{types:[`changed`],style:{color:`rgb(255, 184, 108)`}},{types:[`punctuation`,`symbol`],style:{color:`rgb(248, 248, 242)`}},{types:[`string`,`char`,`tag`,`selector`],style:{color:`rgb(255, 121, 198)`}},{types:[`keyword`,`variable`],style:{color:`rgb(189, 147, 249)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(98, 114, 164)`}},{types:[`attr-name`],style:{color:`rgb(241, 250, 140)`}}]},bv={plain:{backgroundColor:`#2a2734`,color:`#9a86fd`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`,`punctuation`],style:{color:`#6c6783`}},{types:[`namespace`],style:{opacity:.7}},{types:[`tag`,`operator`,`number`],style:{color:`#e09142`}},{types:[`property`,`function`],style:{color:`#9a86fd`}},{types:[`tag-id`,`selector`,`atrule-id`],style:{color:`#eeebff`}},{types:[`attr-name`],style:{color:`#c4b9fe`}},{types:[`boolean`,`string`,`entity`,`url`,`attr-value`,`keyword`,`control`,`directive`,`unit`,`statement`,`regex`,`atrule`,`placeholder`,`variable`],style:{color:`#ffcc99`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`#c4b9fe`}}]},xv={plain:{backgroundColor:`#faf8f5`,color:`#728fcb`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`,`punctuation`],style:{color:`#b6ad9a`}},{types:[`namespace`],style:{opacity:.7}},{types:[`tag`,`operator`,`number`],style:{color:`#063289`}},{types:[`property`,`function`],style:{color:`#b29762`}},{types:[`tag-id`,`selector`,`atrule-id`],style:{color:`#2d2006`}},{types:[`attr-name`],style:{color:`#896724`}},{types:[`boolean`,`string`,`entity`,`url`,`attr-value`,`keyword`,`control`,`directive`,`unit`,`statement`,`regex`,`atrule`],style:{color:`#728fcb`}},{types:[`placeholder`,`variable`],style:{color:`#93abdc`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`#896724`}}]},Sv={plain:{color:`#393A34`,backgroundColor:`#f6f8fa`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`],style:{color:`#999988`,fontStyle:`italic`}},{types:[`namespace`],style:{opacity:.7}},{types:[`string`,`attr-value`],style:{color:`#e3116c`}},{types:[`punctuation`,`operator`],style:{color:`#393A34`}},{types:[`entity`,`url`,`symbol`,`number`,`boolean`,`variable`,`constant`,`property`,`regex`,`inserted`],style:{color:`#36acaa`}},{types:[`atrule`,`keyword`,`attr-name`,`selector`],style:{color:`#00a4db`}},{types:[`function`,`deleted`,`tag`],style:{color:`#d73a49`}},{types:[`function-variable`],style:{color:`#6f42c1`}},{types:[`tag`,`selector`,`keyword`],style:{color:`#00009f`}}]},Cv={plain:{color:`#d6deeb`,backgroundColor:`#011627`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`,fontStyle:`italic`}},{types:[`inserted`,`attr-name`],style:{color:`rgb(173, 219, 103)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(99, 119, 119)`,fontStyle:`italic`}},{types:[`string`,`url`],style:{color:`rgb(173, 219, 103)`}},{types:[`variable`],style:{color:`rgb(214, 222, 235)`}},{types:[`number`],style:{color:`rgb(247, 140, 108)`}},{types:[`builtin`,`char`,`constant`,`function`],style:{color:`rgb(130, 170, 255)`}},{types:[`punctuation`],style:{color:`rgb(199, 146, 234)`}},{types:[`selector`,`doctype`],style:{color:`rgb(199, 146, 234)`,fontStyle:`italic`}},{types:[`class-name`],style:{color:`rgb(255, 203, 139)`}},{types:[`tag`,`operator`,`keyword`],style:{color:`rgb(127, 219, 202)`}},{types:[`boolean`],style:{color:`rgb(255, 88, 116)`}},{types:[`property`],style:{color:`rgb(128, 203, 196)`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`}}]},wv={plain:{color:`#403f53`,backgroundColor:`#FBFBFB`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`,fontStyle:`italic`}},{types:[`inserted`,`attr-name`],style:{color:`rgb(72, 118, 214)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(152, 159, 177)`,fontStyle:`italic`}},{types:[`string`,`builtin`,`char`,`constant`,`url`],style:{color:`rgb(72, 118, 214)`}},{types:[`variable`],style:{color:`rgb(201, 103, 101)`}},{types:[`number`],style:{color:`rgb(170, 9, 130)`}},{types:[`punctuation`],style:{color:`rgb(153, 76, 195)`}},{types:[`function`,`selector`,`doctype`],style:{color:`rgb(153, 76, 195)`,fontStyle:`italic`}},{types:[`class-name`],style:{color:`rgb(17, 17, 17)`}},{types:[`tag`],style:{color:`rgb(153, 76, 195)`}},{types:[`operator`,`property`,`keyword`,`namespace`],style:{color:`rgb(12, 150, 155)`}},{types:[`boolean`],style:{color:`rgb(188, 84, 84)`}}]},Tv={char:`#D8DEE9`,comment:`#999999`,keyword:`#c5a5c5`,primitive:`#5a9bcf`,string:`#8dc891`,variable:`#d7deea`,boolean:`#ff8b50`,punctuation:`#5FB3B3`,tag:`#fc929e`,function:`#79b6f2`,className:`#FAC863`,method:`#6699CC`,operator:`#fc929e`},Ev={plain:{backgroundColor:`#282c34`,color:`#ffffff`},styles:[{types:[`attr-name`],style:{color:Tv.keyword}},{types:[`attr-value`],style:{color:Tv.string}},{types:[`comment`,`block-comment`,`prolog`,`doctype`,`cdata`,`shebang`],style:{color:Tv.comment}},{types:[`property`,`number`,`function-name`,`constant`,`symbol`,`deleted`],style:{color:Tv.primitive}},{types:[`boolean`],style:{color:Tv.boolean}},{types:[`tag`],style:{color:Tv.tag}},{types:[`string`],style:{color:Tv.string}},{types:[`punctuation`],style:{color:Tv.string}},{types:[`selector`,`char`,`builtin`,`inserted`],style:{color:Tv.char}},{types:[`function`],style:{color:Tv.function}},{types:[`operator`,`entity`,`url`,`variable`],style:{color:Tv.variable}},{types:[`keyword`],style:{color:Tv.keyword}},{types:[`atrule`,`class-name`],style:{color:Tv.className}},{types:[`important`],style:{fontWeight:`400`}},{types:[`bold`],style:{fontWeight:`bold`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`namespace`],style:{opacity:.7}}]},Dv={plain:{color:`#f8f8f2`,backgroundColor:`#272822`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`#f92672`,fontStyle:`italic`}},{types:[`inserted`],style:{color:`rgb(173, 219, 103)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`#8292a2`,fontStyle:`italic`}},{types:[`string`,`url`],style:{color:`#a6e22e`}},{types:[`variable`],style:{color:`#f8f8f2`}},{types:[`number`],style:{color:`#ae81ff`}},{types:[`builtin`,`char`,`constant`,`function`,`class-name`],style:{color:`#e6db74`}},{types:[`punctuation`],style:{color:`#f8f8f2`}},{types:[`selector`,`doctype`],style:{color:`#a6e22e`,fontStyle:`italic`}},{types:[`tag`,`operator`,`keyword`],style:{color:`#66d9ef`}},{types:[`boolean`],style:{color:`#ae81ff`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`,opacity:.7}},{types:[`tag`,`property`],style:{color:`#f92672`}},{types:[`attr-name`],style:{color:`#a6e22e !important`}},{types:[`doctype`],style:{color:`#8292a2`}},{types:[`rule`],style:{color:`#e6db74`}}]},Ov={plain:{color:`#bfc7d5`,backgroundColor:`#292d3e`},styles:[{types:[`comment`],style:{color:`rgb(105, 112, 152)`,fontStyle:`italic`}},{types:[`string`,`inserted`],style:{color:`rgb(195, 232, 141)`}},{types:[`number`],style:{color:`rgb(247, 140, 108)`}},{types:[`builtin`,`char`,`constant`,`function`],style:{color:`rgb(130, 170, 255)`}},{types:[`punctuation`,`selector`],style:{color:`rgb(199, 146, 234)`}},{types:[`variable`],style:{color:`rgb(191, 199, 213)`}},{types:[`class-name`,`attr-name`],style:{color:`rgb(255, 203, 107)`}},{types:[`tag`,`deleted`],style:{color:`rgb(255, 85, 114)`}},{types:[`operator`],style:{color:`rgb(137, 221, 255)`}},{types:[`boolean`],style:{color:`rgb(255, 88, 116)`}},{types:[`keyword`],style:{fontStyle:`italic`}},{types:[`doctype`],style:{color:`rgb(199, 146, 234)`,fontStyle:`italic`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`}},{types:[`url`],style:{color:`rgb(221, 221, 221)`}}]},kv={plain:{color:`#9EFEFF`,backgroundColor:`#2D2A55`},styles:[{types:[`changed`],style:{color:`rgb(255, 238, 128)`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`}},{types:[`inserted`],style:{color:`rgb(173, 219, 103)`}},{types:[`comment`],style:{color:`rgb(179, 98, 255)`,fontStyle:`italic`}},{types:[`punctuation`],style:{color:`rgb(255, 255, 255)`}},{types:[`constant`],style:{color:`rgb(255, 98, 140)`}},{types:[`string`,`url`],style:{color:`rgb(165, 255, 144)`}},{types:[`variable`],style:{color:`rgb(255, 238, 128)`}},{types:[`number`,`boolean`],style:{color:`rgb(255, 98, 140)`}},{types:[`attr-name`],style:{color:`rgb(255, 180, 84)`}},{types:[`keyword`,`operator`,`property`,`namespace`,`tag`,`selector`,`doctype`],style:{color:`rgb(255, 157, 0)`}},{types:[`builtin`,`char`,`constant`,`function`,`class-name`],style:{color:`rgb(250, 208, 0)`}}]},Av={plain:{backgroundColor:`linear-gradient(to bottom, #2a2139 75%, #34294f)`,backgroundImage:`#34294f`,color:`#f92aad`,textShadow:`0 0 2px #100c0f, 0 0 5px #dc078e33, 0 0 10px #fff3`},styles:[{types:[`comment`,`block-comment`,`prolog`,`doctype`,`cdata`],style:{color:`#495495`,fontStyle:`italic`}},{types:[`punctuation`],style:{color:`#ccc`}},{types:[`tag`,`attr-name`,`namespace`,`number`,`unit`,`hexcode`,`deleted`],style:{color:`#e2777a`}},{types:[`property`,`selector`],style:{color:`#72f1b8`,textShadow:`0 0 2px #100c0f, 0 0 10px #257c5575, 0 0 35px #21272475`}},{types:[`function-name`],style:{color:`#6196cc`}},{types:[`boolean`,`selector-id`,`function`],style:{color:`#fdfdfd`,textShadow:`0 0 2px #001716, 0 0 3px #03edf975, 0 0 5px #03edf975, 0 0 8px #03edf975`}},{types:[`class-name`,`maybe-class-name`,`builtin`],style:{color:`#fff5f6`,textShadow:`0 0 2px #000, 0 0 10px #fc1f2c75, 0 0 5px #fc1f2c75, 0 0 25px #fc1f2c75`}},{types:[`constant`,`symbol`],style:{color:`#f92aad`,textShadow:`0 0 2px #100c0f, 0 0 5px #dc078e33, 0 0 10px #fff3`}},{types:[`important`,`atrule`,`keyword`,`selector-class`],style:{color:`#f4eee4`,textShadow:`0 0 2px #393a33, 0 0 8px #f39f0575, 0 0 2px #f39f0575`}},{types:[`string`,`char`,`attr-value`,`regex`,`variable`],style:{color:`#f87c32`}},{types:[`parameter`],style:{fontStyle:`italic`}},{types:[`entity`,`url`],style:{color:`#67cdcc`}},{types:[`operator`],style:{color:`ffffffee`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`entity`],style:{cursor:`help`}},{types:[`inserted`],style:{color:`green`}}]},jv={plain:{color:`#282a2e`,backgroundColor:`#ffffff`},styles:[{types:[`comment`],style:{color:`rgb(197, 200, 198)`}},{types:[`string`,`number`,`builtin`,`variable`],style:{color:`rgb(150, 152, 150)`}},{types:[`class-name`,`function`,`tag`,`attr-name`],style:{color:`rgb(40, 42, 46)`}}]},Mv={plain:{color:`#9CDCFE`,backgroundColor:`#1E1E1E`},styles:[{types:[`prolog`],style:{color:`rgb(0, 0, 128)`}},{types:[`comment`],style:{color:`rgb(106, 153, 85)`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`rgb(86, 156, 214)`}},{types:[`number`,`inserted`],style:{color:`rgb(181, 206, 168)`}},{types:[`constant`],style:{color:`rgb(100, 102, 149)`}},{types:[`attr-name`,`variable`],style:{color:`rgb(156, 220, 254)`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`rgb(206, 145, 120)`}},{types:[`selector`],style:{color:`rgb(215, 186, 125)`}},{types:[`tag`],style:{color:`rgb(78, 201, 176)`}},{types:[`tag`],languages:[`markup`],style:{color:`rgb(86, 156, 214)`}},{types:[`punctuation`,`operator`],style:{color:`rgb(212, 212, 212)`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`rgb(220, 220, 170)`}},{types:[`class-name`],style:{color:`rgb(78, 201, 176)`}},{types:[`char`],style:{color:`rgb(209, 105, 105)`}}]},Nv={plain:{color:`#000000`,backgroundColor:`#ffffff`},styles:[{types:[`comment`],style:{color:`rgb(0, 128, 0)`}},{types:[`builtin`],style:{color:`rgb(0, 112, 193)`}},{types:[`number`,`variable`,`inserted`],style:{color:`rgb(9, 134, 88)`}},{types:[`operator`],style:{color:`rgb(0, 0, 0)`}},{types:[`constant`,`char`],style:{color:`rgb(129, 31, 63)`}},{types:[`tag`],style:{color:`rgb(128, 0, 0)`}},{types:[`attr-name`],style:{color:`rgb(255, 0, 0)`}},{types:[`deleted`,`string`],style:{color:`rgb(163, 21, 21)`}},{types:[`changed`,`punctuation`],style:{color:`rgb(4, 81, 165)`}},{types:[`function`,`keyword`],style:{color:`rgb(0, 0, 255)`}},{types:[`class-name`],style:{color:`rgb(38, 127, 153)`}}]},Pv={plain:{color:`#f8fafc`,backgroundColor:`#011627`},styles:[{types:[`prolog`],style:{color:`#000080`}},{types:[`comment`],style:{color:`#6A9955`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`#569CD6`}},{types:[`number`,`inserted`],style:{color:`#B5CEA8`}},{types:[`constant`],style:{color:`#f8fafc`}},{types:[`attr-name`,`variable`],style:{color:`#9CDCFE`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`#cbd5e1`}},{types:[`selector`],style:{color:`#D7BA7D`}},{types:[`tag`],style:{color:`#0ea5e9`}},{types:[`tag`],languages:[`markup`],style:{color:`#0ea5e9`}},{types:[`punctuation`,`operator`],style:{color:`#D4D4D4`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`#7dd3fc`}},{types:[`class-name`],style:{color:`#0ea5e9`}},{types:[`char`],style:{color:`#D16969`}}]},Fv={plain:{color:`#0f172a`,backgroundColor:`#f1f5f9`},styles:[{types:[`prolog`],style:{color:`#000080`}},{types:[`comment`],style:{color:`#6A9955`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`#0c4a6e`}},{types:[`number`,`inserted`],style:{color:`#B5CEA8`}},{types:[`constant`],style:{color:`#0f172a`}},{types:[`attr-name`,`variable`],style:{color:`#0c4a6e`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`#64748b`}},{types:[`selector`],style:{color:`#D7BA7D`}},{types:[`tag`],style:{color:`#0ea5e9`}},{types:[`tag`],languages:[`markup`],style:{color:`#0ea5e9`}},{types:[`punctuation`,`operator`],style:{color:`#475569`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`#0e7490`}},{types:[`class-name`],style:{color:`#0ea5e9`}},{types:[`char`],style:{color:`#D16969`}}]},Iv={plain:{backgroundColor:`hsl(220, 13%, 18%)`,color:`hsl(220, 14%, 71%)`,textShadow:`0 1px rgba(0, 0, 0, 0.3)`},styles:[{types:[`comment`,`prolog`,`cdata`],style:{color:`hsl(220, 10%, 40%)`}},{types:[`doctype`,`punctuation`,`entity`],style:{color:`hsl(220, 14%, 71%)`}},{types:[`attr-name`,`class-name`,`maybe-class-name`,`boolean`,`constant`,`number`,`atrule`],style:{color:`hsl(29, 54%, 61%)`}},{types:[`keyword`],style:{color:`hsl(286, 60%, 67%)`}},{types:[`property`,`tag`,`symbol`,`deleted`,`important`],style:{color:`hsl(355, 65%, 65%)`}},{types:[`selector`,`string`,`char`,`builtin`,`inserted`,`regex`,`attr-value`],style:{color:`hsl(95, 38%, 62%)`}},{types:[`variable`,`operator`,`function`],style:{color:`hsl(207, 82%, 66%)`}},{types:[`url`],style:{color:`hsl(187, 47%, 55%)`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`hsl(220, 14%, 71%)`}}]},Lv={plain:{backgroundColor:`hsl(230, 1%, 98%)`,color:`hsl(230, 8%, 24%)`},styles:[{types:[`comment`,`prolog`,`cdata`],style:{color:`hsl(230, 4%, 64%)`}},{types:[`doctype`,`punctuation`,`entity`],style:{color:`hsl(230, 8%, 24%)`}},{types:[`attr-name`,`class-name`,`boolean`,`constant`,`number`,`atrule`],style:{color:`hsl(35, 99%, 36%)`}},{types:[`keyword`],style:{color:`hsl(301, 63%, 40%)`}},{types:[`property`,`tag`,`symbol`,`deleted`,`important`],style:{color:`hsl(5, 74%, 59%)`}},{types:[`selector`,`string`,`char`,`builtin`,`inserted`,`regex`,`attr-value`,`punctuation`],style:{color:`hsl(119, 34%, 47%)`}},{types:[`variable`,`operator`,`function`],style:{color:`hsl(221, 87%, 60%)`}},{types:[`url`],style:{color:`hsl(198, 99%, 37%)`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`hsl(230, 8%, 24%)`}}]},Rv={plain:{color:`#ebdbb2`,backgroundColor:`#292828`},styles:[{types:[`imports`,`class-name`,`maybe-class-name`,`constant`,`doctype`,`builtin`,`function`],style:{color:`#d8a657`}},{types:[`property-access`],style:{color:`#7daea3`}},{types:[`tag`],style:{color:`#e78a4e`}},{types:[`attr-name`,`char`,`url`,`regex`],style:{color:`#a9b665`}},{types:[`attr-value`,`string`],style:{color:`#89b482`}},{types:[`comment`,`prolog`,`cdata`,`operator`,`inserted`],style:{color:`#a89984`}},{types:[`delimiter`,`boolean`,`keyword`,`selector`,`important`,`atrule`,`property`,`variable`,`deleted`],style:{color:`#ea6962`}},{types:[`entity`,`number`,`symbol`],style:{color:`#d3869b`}}]},zv={plain:{color:`#654735`,backgroundColor:`#f9f5d7`},styles:[{types:[`delimiter`,`boolean`,`keyword`,`selector`,`important`,`atrule`,`property`,`variable`,`deleted`],style:{color:`#af2528`}},{types:[`imports`,`class-name`,`maybe-class-name`,`constant`,`doctype`,`builtin`],style:{color:`#b4730e`}},{types:[`string`,`attr-value`],style:{color:`#477a5b`}},{types:[`property-access`],style:{color:`#266b79`}},{types:[`function`,`attr-name`,`char`,`url`],style:{color:`#72761e`}},{types:[`tag`],style:{color:`#b94c07`}},{types:[`comment`,`prolog`,`cdata`,`operator`,`inserted`],style:{color:`#a89984`}},{types:[`entity`,`number`,`symbol`],style:{color:`#924f79`}}]},Bv=e=>(0,x.useCallback)(t=>{var n=t,{className:r,style:i,line:a}=n;let o=pv(fv({},mv(n,[`className`,`style`,`line`])),{className:R(`token-line`,r)});return typeof e==`object`&&`plain`in e&&(o.style=e.plain),typeof i==`object`&&(o.style=fv(fv({},o.style||{}),i)),o},[e]),Vv=e=>{let t=(0,x.useCallback)(({types:t,empty:n})=>{if(e!=null)return t.length===1&&t[0]===`plain`?n==null?void 0:{display:`inline-block`}:t.length===1&&n!=null?e[t[0]]:Object.assign(n==null?{}:{display:`inline-block`},...t.map(t=>e[t]))},[e]);return(0,x.useCallback)(e=>{var n=e,{token:r,className:i,style:a}=n;let o=pv(fv({},mv(n,[`token`,`className`,`style`])),{className:R(`token`,...r.types,i),children:r.content,style:t(r)});return a!=null&&(o.style=fv(fv({},o.style||{}),a)),o},[t])},Hv=/\r\n|\r|\n/,Uv=e=>{e.length===0?e.push({types:[`plain`],content:`
`,empty:!0}):e.length===1&&e[0].content===``&&(e[0].content=`
`,e[0].empty=!0)},Wv=(e,t)=>{let n=e.length;return n>0&&e[n-1]===t?e:e.concat(t)},Gv=e=>{let t=[[]],n=[e],r=[0],i=[e.length],a=0,o=0,s=[],c=[s];for(;o>-1;){for(;(a=r[o]++)<i[o];){let e,l=t[o],u=n[o][a];if(typeof u==`string`?(l=o>0?l:[`plain`],e=u):(l=Wv(l,u.type),u.alias&&(l=Wv(l,u.alias)),e=u.content),typeof e!=`string`){o++,t.push(l),n.push(e),r.push(0),i.push(e.length);continue}let d=e.split(Hv),f=d.length;s.push({types:l,content:d[0]});for(let e=1;e<f;e++)Uv(s),c.push(s=[]),s.push({types:l,content:d[e]})}o--,t.pop(),n.pop(),r.pop(),i.pop()}return Uv(s),c},Kv=({prism:e,code:t,grammar:n,language:r})=>(0,x.useMemo)(()=>{if(n==null)return Gv([t]);let i={code:t,grammar:n,language:r,tokens:[]};return e.hooks.run(`before-tokenize`,i),i.tokens=e.tokenize(t,n),e.hooks.run(`after-tokenize`,i),Gv(i.tokens)},[t,n,r,e]),qv=(e,t)=>{let{plain:n}=e,r=e.styles.reduce((e,n)=>{let{languages:r,style:i}=n;return r&&!r.includes(t)||n.types.forEach(t=>{e[t]=fv(fv({},e[t]),i)}),e},{});return r.root=n,r.plain=pv(fv({},n),{backgroundColor:void 0}),r},Jv=({children:e,language:t,code:n,theme:r,prism:i})=>{let a=t.toLowerCase(),o=qv(r,a),s=Bv(o),c=Vv(o),l=i.languages[a];return e({tokens:Kv({prism:i,language:a,code:n,grammar:l}),className:`prism-code language-${a}`,style:o==null?{}:o.root,getLineProps:s,getTokenProps:c})},Yv=e=>(0,x.createElement)(Jv,pv(fv({},e),{prism:e.prism||X,theme:e.theme||Mv,code:e.code,language:e.language})),Xv=zl((0,I.jsx)(`path`,{d:`M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2m0 16H8V7h11z`}),`ContentCopy`),Zv={wrapper:{borderRadius:1,overflow:`hidden`,border:1,borderColor:`divider`,bgcolor:`background.paper`},header:{display:`flex`,alignItems:`center`,justifyContent:`space-between`,px:1.5,py:.5,borderBottom:1,borderColor:`divider`,bgcolor:`rgba(255,255,255,0.02)`},headerNoTabs:{display:`flex`,alignItems:`center`,justifyContent:`flex-end`,px:1.5,py:.5,borderBottom:1,borderColor:`divider`,bgcolor:`rgba(255,255,255,0.02)`},tabs:{display:`flex`,alignItems:`center`,gap:.5},tab:{px:1,py:.25,borderRadius:999,fontSize:12,color:`text.secondary`,cursor:`pointer`},tabActive:{px:1,py:.25,borderRadius:999,fontSize:12,bgcolor:`primary.main`,color:`#0b0f1a`,cursor:`pointer`},copyBtn:{color:`text.secondary`},body:{p:0,bgcolor:`#21252e`},preCustom:{margin:0,padding:10}};const Z=({code:e,codeRuntime:t,codeEditor:n,language:r=`tsx`})=>{let[i,a]=(0,x.useState)(!1),o=(0,x.useCallback)(()=>{let r=(t??n??e??``).trim();navigator.clipboard.writeText(r).then(()=>{a(!0),setTimeout(()=>a(!1),1200)})},[e,t,n]),s=!!(t||n),[c,l]=(0,x.useState)(t?`runtime`:`editor`),u=(()=>s?c===`runtime`?(t??n??``).trim():(n??t??``).trim():(e??``).trim())();return(0,I.jsxs)(W,{sx:Zv.wrapper,children:[(0,I.jsxs)(W,{sx:s?Zv.header:Zv.headerNoTabs,children:[s&&(0,I.jsxs)(W,{sx:Zv.tabs,children:[t&&(0,I.jsx)(W,{sx:c===`runtime`?Zv.tabActive:Zv.tab,onClick:()=>l(`runtime`),children:(0,I.jsx)(U,{variant:`caption`,children:`Runtime`})}),n&&(0,I.jsx)(W,{sx:c===`editor`?Zv.tabActive:Zv.tab,onClick:()=>l(`editor`),children:(0,I.jsx)(U,{variant:`caption`,children:`Editor`})})]}),(0,I.jsx)(g_,{title:i?`Copied`:`Copy`,children:(0,I.jsx)(Gd,{size:`small`,onClick:o,sx:Zv.copyBtn,children:(0,I.jsx)(Xv,{fontSize:`inherit`})})})]}),(0,I.jsx)(W,{sx:Zv.body,children:(0,I.jsx)(Yv,{theme:vv.oneDark,code:u,language:r,children:({className:e,style:t,tokens:n,getLineProps:r,getTokenProps:i})=>(0,I.jsx)(`pre`,{className:e,style:{...t,...Zv.preCustom},children:n.map((e,t)=>(0,I.jsx)(`div`,{...r({line:e}),children:e.map((e,t)=>(0,I.jsx)(`span`,{...i({token:e})},t))},t))})})})]})};var Qv={root:{}};const $v=()=>(0,I.jsxs)(W,{sx:Qv.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Getting Started`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Supported Unity versions: `,(0,I.jsx)(`strong`,{children:`Unity 6.2+`})]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Install via Unity Package Manager`}),(0,I.jsxs)(G,{children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Open Package Manager in Unity.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Add package from Git URL:`})})]}),(0,I.jsx)(Z,{language:`tsx`,code:`https://github.com/yanivkalfa/ReactiveUIToolKit.git#dist`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Hello World (Editor)`}),(0,I.jsx)(Z,{language:`tsx`,codeEditor:`using UnityEditor;
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
}`})]});var ey={root:{display:`flex`,flexDirection:`column`,gap:2},list:{pl:2}};const ty=()=>(0,I.jsxs)(W,{sx:ey.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Router`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit includes a lightweight, in-memory router inspired by React Router. It routes based on the current path and lets you nest routes and links inside your `,(0,I.jsx)(`code`,{children:`VirtualNode`}),` `,`tree.`]}),(0,I.jsxs)(W,{children:[(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Core concepts`}),(0,I.jsxs)(G,{sx:ey.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Use `,(0,I.jsx)(`code`,{children:`V.Router(...)`}),` at the root of a subtree to set up routing context and history.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Use `,(0,I.jsx)(`code`,{children:`V.Route(path, exact, element, children)`}),` to match the current path and decide what to render.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Use `,(0,I.jsx)(`code`,{children:`V.Link`}),` and `,(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigate(replace)`}),` to perform navigation from code or UI.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Use `,(0,I.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),`, `,(0,I.jsx)(`code`,{children:`RouterHooks.UseParams()`}),`, and`,` `,(0,I.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` to access path, parameters, and query-string values.`]})})})]})]}),(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Basic example`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The example below shows the same router tree hosted in an editor window and in a runtime function component. Inside the matched routes you can use `,(0,I.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),` `,`and `,(0,I.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` to read the active path and parameters.`]}),(0,I.jsx)(Z,{language:`tsx`,codeEditor:`using System.Collections.Generic;
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
// rootRenderer.Render(V.Func(RouterDemoFunc.Example));`}),(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Navigation and history`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`By default `,(0,I.jsx)(`code`,{children:`V.Router`}),` uses an in-memory history implementation. You can provide a custom `,(0,I.jsx)(`code`,{children:`IRouterHistory`}),` instance if you want to control how locations are stored or synchronized. Inside components, use `,(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),` to push or replace locations, and `,(0,I.jsx)(`code`,{children:`RouterHooks.UseGo()`}),` / `,(0,I.jsx)(`code`,{children:`RouterHooks.UseCanGo()`}),` to implement back/forward UI. You can also use `,(0,I.jsx)(`code`,{children:`RouterHooks.UseBlocker()`}),` to prevent navigation while a confirmation dialog is open.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Links and route data`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use `,(0,I.jsx)(`code`,{children:`V.Link`}),` to render navigation buttons bound to specific paths. Inside routed components, use `,(0,I.jsx)(`code`,{children:`RouterHooks.UseLocationInfo()`}),` for the full location payload,`,(0,I.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` for path parameters, `,(0,I.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` `,`for query-string values, and `,(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` for any state object passed when navigating.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Links, params, query, and state (example)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The example below demonstrates how to combine `,(0,I.jsx)(`code`,{children:`V.Link`}),`,`,` `,(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),`, `,(0,I.jsx)(`code`,{children:`RouterHooks.UseGo()`}),`,`,` `,(0,I.jsx)(`code`,{children:`RouterHooks.UseParams()`}),`, `,(0,I.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),`, and`,` `,(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` to build a small navigation bar that can move back and forth and read route data.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
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
}`}),(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Split layouts with nested routes`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`You can keep a single router history while nesting routes to act like “outlets”. Child routes may use relative paths (for example “profile”), and we automatically resolve them against the parent match. When you use a relative route, it is prefixed with the parent route’s path before matching, so patterns like `,(0,I.jsx)(`code`,{children:`:id/edit`}),` work the same way they do in React Router—no need to repeat the parent prefix.`]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The example below matches `,(0,I.jsx)(`code`,{children:`/mainMenu/*`}),`, renders a sidebar, and nests additional`,` `,(0,I.jsx)(`code`,{children:`V.Route`}),` elements so the right-hand panel switches content as the path changes. The sidebar buttons simply call `,(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),` with relative targets, and the router keeps everything in sync without spinning up another router.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
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
`})]});var ny={root:{display:`flex`,flexDirection:`column`,gap:2},list:{pl:2}};const ry=()=>(0,I.jsxs)(W,{sx:ny.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Signals`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`Signals`}),` are lightweight, named reactive values that live in a process-wide registry. They behave like a small observable store with a simple API and are ideal whenever you want a single source of truth with a single point of entry for reading and updating state (for example: selection, filters, or global preferences).`]}),(0,I.jsxs)(W,{children:[(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Concepts`}),(0,I.jsxs)(G,{sx:ny.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`Signals`}),` live in a global registry keyed by `,(0,I.jsx)(`code`,{children:`string`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Call `,(0,I.jsx)(`code`,{children:`Signals.Get<T>(key, initialValue)`}),` to create or return a`,` `,(0,I.jsx)(`code`,{children:`Signal<T>`}),` instance.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Call `,(0,I.jsx)(`code`,{children:`signal.Subscribe(...)`}),` to watch changes outside of components; use`,` `,(0,I.jsx)(`code`,{children:`Hooks.UseSignal(...)`}),` inside function components.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Use `,(0,I.jsx)(`code`,{children:`Dispatch(prev => next)`}),` or `,(0,I.jsx)(`code`,{children:`Dispatch(value)`}),` to update the value and notify listeners.`]})})})]})]}),(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Runtime usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`using System;
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
}`}),(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Using signals from components`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Inside function components, use `,(0,I.jsx)(`code`,{children:`Hooks.UseSignal`}),` or the selector overload`,` `,(0,I.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(...)`}),` to read a signal and re-render when it changes. The example below shows a simple counter bound to the global `,(0,I.jsx)(`code`,{children:`demo-counter`}),` `,`signal, but you can also project a slice of a more complex signal value and compare with a custom equality comparer for performance.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
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
}`})]});var iy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const ay=()=>(0,I.jsxs)(W,{sx:iy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Concepts & Environment`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit aims to feel familiar if you know React, while still fitting naturally into Unity's UI Toolkit and C# ecosystem. You build trees from `,(0,I.jsx)(`code`,{children:`V.*`}),` helpers and function components, use hooks to manage state, and let the reconciler diff and update the underlying `,(0,I.jsx)(`code`,{children:`VisualElement`}),` hierarchy for you.`]}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`Where Unity or UI Toolkit impose different constraints (for example: layout system, event model, or platform concerns), the library deliberately diverges from React to provide a more idiomatic Unity experience. The routing, signals, and safe-area helpers are examples of features that don't exist in core React but are important here.`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The package also ships with a rich demo set under `,(0,I.jsx)(`code`,{children:`Assets/ReactiveUIToolKit/Samples`}),` `,`(editor windows and runtime scenes) that you can import into your project. These demos show real-world usage of components, hooks, routing, signals, and more, and are a great way to see the concepts on this page in action.`]}),(0,I.jsxs)(W,{sx:iy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Scripting define symbols (environment & tracing)`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Set these in `,(0,I.jsx)(`strong`,{children:`Project Settings → Player → Scripting Define Symbols`}),`. They control environment labels and diagnostics at compile time.`]}),(0,I.jsxs)(G,{sx:iy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ENV_DEV`}),` — development environment. Enables dev-oriented defaults such as Basic trace level and compiles editor diagnostics helpers.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ENV_STAGING`}),` — staging environment label (no implicit tracing changes).`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ENV_PROD`}),` — production environment label. This is the implied default if no `,(0,I.jsx)(`code`,{children:`ENV_*`}),` symbol is defined.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RUITK_TRACE_VERBOSE`}),` — force reconciler trace level to`,` `,(0,I.jsx)(`strong`,{children:`Verbose`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RUITK_TRACE_BASIC`}),` — force reconciler trace level to`,` `,(0,I.jsx)(`strong`,{children:`Basic`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RUITK_DIFF_TRACING`}),` — force`,` `,(0,I.jsx)(`code`,{children:`DiagnosticsConfig.EnableDiffTracing`}),` to `,(0,I.jsx)(`code`,{children:`true`}),` for detailed Fiber diff diagnostics.`]})})})]}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,sx:iy.section,children:(0,I.jsx)(`strong`,{children:`Behavior summary`})}),(0,I.jsxs)(G,{sx:iy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Environment is resolved to `,(0,I.jsx)(`code`,{children:`development`}),`, `,(0,I.jsx)(`code`,{children:`staging`}),`, or`,` `,(0,I.jsx)(`code`,{children:`production`}),` via the `,(0,I.jsx)(`code`,{children:`ENV_*`}),` defines and is exposed at runtime as `,(0,I.jsx)(`code`,{children:`HostContext.Environment["env"]`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Trace level resolution priority:`,` `,(0,I.jsx)(`code`,{children:`RUITK_TRACE_VERBOSE`}),` > `,(0,I.jsx)(`code`,{children:`RUITK_TRACE_BASIC`}),` >`,` `,(0,I.jsx)(`code`,{children:`ENV_DEV`}),` (Basic) > none.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Editor-only diagnostic utilities are compiled only when ENV_DEV is defined.`})})]})]})]});var oy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const sy=()=>(0,I.jsxs)(W,{sx:oy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Different from React`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`ReactiveUIToolKit feels familiar if you know React, but there are important differences in how rendering and scheduling behave when you are working in C# and Unity instead of JavaScript and the browser. This section focuses on the places where your mental model should be adjusted rather than re-explaining core concepts.`}),(0,I.jsxs)(W,{sx:oy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`State updates with UseState (parity)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`Hooks.UseState`}),` matches React's mental model: you get a value and a setter, and you can call the setter with either a value or a function of the previous value (for example `,(0,I.jsx)(`code`,{children:`set(value)`}),` or `,(0,I.jsx)(`code`,{children:`set(prev => next)`}),`).`]}),(0,I.jsxs)(G,{sx:oy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`The setter is a delegate (`,(0,I.jsx)(`code`,{children:`StateSetter<T>`}),`), not an instance method, but you call it just like a normal function.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`You can either call `,(0,I.jsx)(`code`,{children:`set(value)`}),` / `,(0,I.jsx)(`code`,{children:`set(prev => next)`}),` `,`(React-style) or use the optional extension helpers`,` `,(0,I.jsx)(`code`,{children:`StateSetterExtensions.Set(value)`}),` /`,` `,(0,I.jsx)(`code`,{children:`StateSetterExtensions.Set(prev => next)`}),` if you prefer a fluent style.`]})})})]}),(0,I.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
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
}`})]}),(0,I.jsxs)(W,{sx:oy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Sync rendering vs React concurrent mode`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit's Fiber reconciler currently runs in a single, synchronous mode per Unity frame. There is no React 18-style concurrent rendering yet: no`,` `,(0,I.jsx)(`code`,{children:`startTransition`}),`, no transition priorities, and no cooperative time-slicing of large trees.`]}),(0,I.jsxs)(G,{sx:oy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsx)(I.Fragment,{children:`All updates scheduled in a frame are processed synchronously; there is no partial rendering or preemption between high- and low-priority updates.`})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsx)(I.Fragment,{children:`This behaves like legacy React (pre-18) "sync mode": your components and hooks logic are the same, but you should not expect concurrent features such as transitions or suspenseful background rendering.`})})})]})]})]});var cy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const ly=()=>(0,I.jsxs)(W,{sx:cy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`API Reference`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`This section gives a high-level map of the main namespaces and types you will use when working with ReactiveUIToolKit. Use it as a guide when you are looking for where a particular class (for example `,(0,I.jsx)(`code`,{children:`ButtonProps`}),`) lives.`]}),(0,I.jsxs)(W,{sx:cy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Core`}),(0,I.jsxs)(G,{sx:cy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Core.V`}),` – static factory for building`,` `,(0,I.jsx)(`code`,{children:`VirtualNode`}),` trees (for example `,(0,I.jsx)(`code`,{children:`V.VisualElement`}),`,`,` `,(0,I.jsx)(`code`,{children:`V.VisualElementSafe`}),`, `,(0,I.jsx)(`code`,{children:`V.Label`}),`, `,(0,I.jsx)(`code`,{children:`V.Button`}),`,`,` `,(0,I.jsx)(`code`,{children:`V.Router`}),`, `,(0,I.jsx)(`code`,{children:`V.TabView`}),`).`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Core.Hooks`}),` – hook functions for function components, such as `,(0,I.jsx)(`code`,{children:`UseState`}),`, `,(0,I.jsx)(`code`,{children:`UseReducer`}),`, `,(0,I.jsx)(`code`,{children:`UseEffect`}),`,`,` `,(0,I.jsx)(`code`,{children:`UseMemo`}),`, `,(0,I.jsx)(`code`,{children:`UseSignal`}),`, and context helpers.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Core.StateSetterExtensions`}),` – helpers for working with state setters (for example `,(0,I.jsx)(`code`,{children:`set.Set(value)`}),` /`,` `,(0,I.jsx)(`code`,{children:`set.Set(prev => next)`}),`).`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Core.RootRenderer`}),` – runtime component that mounts a`,` `,(0,I.jsx)(`code`,{children:`VirtualNode`}),` tree into a `,(0,I.jsx)(`code`,{children:`UIDocument`}),` root.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Core.RenderScheduler`}),` – runtime scheduler used by the reconciler to batch updates per frame.`]})})})]})]}),(0,I.jsxs)(W,{sx:cy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props & Styles`}),(0,I.jsxs)(G,{sx:cy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Props.Typed`}),` – typed props for UI Toolkit controls. Each control has a corresponding `,(0,I.jsx)(`code`,{children:`*Props`}),` class (for example`,` `,(0,I.jsx)(`code`,{children:`ButtonProps`}),`, `,(0,I.jsx)(`code`,{children:`LabelProps`}),`, `,(0,I.jsx)(`code`,{children:`ListViewProps`}),`,`,` `,(0,I.jsx)(`code`,{children:`ScrollViewProps`}),`).`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Props.Typed.Style`}),` – strongly typed wrapper around a style dictionary used by many props (`,(0,I.jsx)(`code`,{children:`Style`}),` is often passed as`,` `,(0,I.jsx)(`code`,{children:`props.Style`}),`).`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Props.Typed.StyleKeys`}),` – constants used as keys inside`,` `,(0,I.jsx)(`code`,{children:`Style`}),` (for example `,(0,I.jsx)(`code`,{children:`StyleKeys.MarginTop`}),`,`,` `,(0,I.jsx)(`code`,{children:`StyleKeys.FlexDirection`}),`).`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Most field and layout controls follow the same pattern:`,(0,I.jsx)(`code`,{children:`V.FloatField(new FloatFieldProps { ... })`}),`,`,` `,(0,I.jsx)(`code`,{children:`V.ListView(new ListViewProps { ... })`}),`, and so on.`]})})})]})]}),(0,I.jsxs)(W,{sx:cy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Router`}),(0,I.jsxs)(G,{sx:cy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Router.RouterHooks`}),` – hook helpers for routing:`,` `,(0,I.jsx)(`code`,{children:`UseRouter()`}),`, `,(0,I.jsx)(`code`,{children:`UseLocation()`}),`, `,(0,I.jsx)(`code`,{children:`UseLocationInfo()`}),`, `,(0,I.jsx)(`code`,{children:`UseParams()`}),`, `,(0,I.jsx)(`code`,{children:`UseQuery()`}),`,`,` `,(0,I.jsx)(`code`,{children:`UseNavigationState()`}),`, `,(0,I.jsx)(`code`,{children:`UseNavigate()`}),`, `,(0,I.jsx)(`code`,{children:`UseGo()`}),`, `,(0,I.jsx)(`code`,{children:`UseCanGo()`}),`, `,(0,I.jsx)(`code`,{children:`UseBlocker()`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Router.IRouterHistory`}),`, `,(0,I.jsx)(`code`,{children:`MemoryHistory`}),` – the history abstraction used by `,(0,I.jsx)(`code`,{children:`V.Router`}),`. You can supply your own history implementation by passing an `,(0,I.jsx)(`code`,{children:`IRouterHistory`}),` instance to`,` `,(0,I.jsx)(`code`,{children:`V.Router`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Router.RouterLocation`}),`, `,(0,I.jsx)(`code`,{children:`RouterPath`}),`,`,` `,(0,I.jsx)(`code`,{children:`RouteMatch`}),` – types that describe the current location, parsed path, and the result of route matching.`]})})})]})]}),(0,I.jsxs)(W,{sx:cy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signals`}),(0,I.jsxs)(G,{sx:cy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Signals.Signals`}),` – entry point for working with signals via`,` `,(0,I.jsx)(`code`,{children:`Signals.Get<T>(key, initialValue)`}),` and`,` `,(0,I.jsx)(`code`,{children:`Signals.TryGet<T>(key, out signal)`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Signals.Signal<T>`}),` – concrete signal type with`,` `,(0,I.jsx)(`code`,{children:`Value`}),`, `,(0,I.jsx)(`code`,{children:`Subscribe(...)`}),`, `,(0,I.jsx)(`code`,{children:`Set(value)`}),`, and`,` `,(0,I.jsx)(`code`,{children:`Dispatch(update)`}),` methods.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Signals.SignalsRuntime`}),` – bootstraps the runtime registry and hidden host GameObject. Call `,(0,I.jsx)(`code`,{children:`SignalsRuntime.EnsureInitialized()`}),` at startup if you are using signals outside of components.`]})})})]})]}),(0,I.jsxs)(W,{sx:cy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Editor support`}),(0,I.jsxs)(G,{sx:cy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.EditorSupport.EditorRootRendererUtility`}),` – helper for mounting a `,(0,I.jsx)(`code`,{children:`VirtualNode`}),` tree into an EditorWindow`,` `,(0,I.jsx)(`code`,{children:`VisualElement`}),`. Used from editor samples and your own tools.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.EditorSupport.EditorRenderScheduler`}),` – scheduler used in the editor for batched updates.`]})})})]})]}),(0,I.jsxs)(W,{sx:cy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Elements & registry`}),(0,I.jsxs)(G,{sx:cy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Elements.ElementRegistry`}),` – maps element names (for example`,` `,(0,I.jsx)(`code`,{children:`"Button"`}),`, `,(0,I.jsx)(`code`,{children:`"ListView"`}),`) to concrete adapters and is used by the reconciler when creating and updating UI Toolkit elements.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ReactiveUITK.Elements.ElementRegistryProvider`}),` – static helpers for obtaining the default registry used by both runtime and editor hosts.`]})})})]})]})]}),uy={AnimateProps:`using System.Collections.Generic;
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
}`},Q=e=>uy[e]??``;var dy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const fy={BoundsField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-BoundsField.html`},BoundsIntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-BoundsIntField.html`},Box:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Box.html`},Button:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Button.html`},ColorField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ColorField.html`},DoubleField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-DoubleField.html`},DropdownField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-DropdownField.html`},EnumField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-EnumField.html`},EnumFlagsField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-EnumFlagsField.html`},FloatField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-FloatField.html`},Foldout:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Foldout.html`},GroupBox:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-GroupBox.html`},Hash128Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Hash128Field.html`},HelpBox:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-HelpBox.html`},IMGUIContainer:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-IMGUIContainer.html`},Image:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Image.html`},IntegerField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-IntegerField.html`},Label:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Label.html`},ListView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ListView.html`},LongField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-LongField.html`},MinMaxSlider:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MinMaxSlider.html`},MultiColumnListView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MultiColumnListView.html`},MultiColumnTreeView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MultiColumnTreeView.html`},ObjectField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ObjectField.html`},ProgressBar:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ProgressBar.html`},PropertyInspector:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-InspectorElement.html`,label:`InspectorElement entry`,note:`ReactiveUITK.PropertyInspector wraps Unity’s InspectorElement to embed serialized-object inspectors.`},RadioButton:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RadioButton.html`},RadioButtonGroup:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RadioButtonGroup.html`},RectField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RectField.html`},RectIntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RectIntField.html`},RepeatButton:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RepeatButton.html`},ScrollView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ScrollView.html`},Scroller:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Scroller.html`},Slider:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Slider.html`},SliderInt:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-SliderInt.html`},Tab:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Tab.html`},TabView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TabView.html`},TemplateContainer:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TemplateContainer.html`},TextElement:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TextElement.html`},TextField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TextField.html`},Toggle:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Toggle.html`},ToggleButtonGroup:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ToggleButtonGroup.html`},Toolbar:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Toolbar.html`},TreeView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TreeView.html`},TwoPaneSplitView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TwoPaneSplitView.html`},UnsignedIntegerField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-UnsignedIntegerField.html`},UnsignedLongField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-UnsignedLongField.html`},Vector2Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector2Field.html`},Vector2IntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector2IntField.html`},Vector3Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector3Field.html`},Vector3IntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector3IntField.html`},Vector4Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector4Field.html`},VisualElement:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-VisualElement.html`}},$=({componentName:e})=>{let t=fy[e];if(!t)return null;let n=t.label??`${e} entry`;return(0,I.jsxs)(W,{sx:{mt:2},children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Unity docs`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Review the`,` `,(0,I.jsx)(Pg,{href:t.href,target:`_blank`,rel:`noreferrer`,children:n}),` `,`in the Unity manual for the official UI Toolkit reference.`]}),t.note&&(0,I.jsx)(U,{variant:`body2`,color:`text.secondary`,paragraph:!0,children:t.note})]})},py=()=>(0,I.jsxs)(W,{sx:dy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`BoundsField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.BoundsField`}),` wraps the Unity `,(0,I.jsx)(`code`,{children:`BoundsField`}),` control using`,` `,(0,I.jsx)(`code`,{children:`BoundsFieldProps`}),`. It is useful for editing `,(0,I.jsx)(`code`,{children:`Bounds`}),` values in both runtime UI and editor tools.`]}),(0,I.jsxs)(W,{sx:dy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`BoundsFieldProps`)})]}),(0,I.jsxs)(W,{sx:dy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,I.jsx)(`code`,{children:`BoundsFieldProps`}),` instance to `,(0,I.jsx)(`code`,{children:`V.BoundsField`}),`. The`,` `,(0,I.jsx)(`code`,{children:`Value`}),` property controls the current bounds.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:dy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,I.jsxs)(U,{variant:`body1`,children:[(0,I.jsx)(`code`,{children:`BoundsField`}),` does not accept child nodes; all configuration is done through`,` `,(0,I.jsx)(`code`,{children:`BoundsFieldProps`}),`.`]})]}),(0,I.jsxs)(W,{sx:dy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,I.jsx)(`code`,{children:`Label`}),` and `,(0,I.jsx)(`code`,{children:`VisualInput`}),` properties to style the label and the internal input container. Both expect dictionaries – you can compose them using other typed props (for example `,(0,I.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),`) or by building a`,` `,(0,I.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,I.jsxs)(W,{sx:dy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Controlled value`}),(0,I.jsxs)(U,{variant:`body1`,children:[`Use `,(0,I.jsx)(`code`,{children:`Hooks.UseState`}),` (or a signal) to hold the current `,(0,I.jsx)(`code`,{children:`Bounds`}),` and update it from a change handler. The example above uses a local state tuple and updates the value via `,(0,I.jsx)(`code`,{children:`setBounds(evt.newValue)`}),` (you can also use the optional`,` `,(0,I.jsx)(`code`,{children:`StateSetterExtensions.Set`}),` helper if you prefer method syntax).`]})]}),(0,I.jsx)($,{componentName:`BoundsField`})]});var my={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const hy=()=>(0,I.jsxs)(W,{sx:my.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`BoundsIntField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.BoundsIntField`}),` wraps the Unity `,(0,I.jsx)(`code`,{children:`BoundsIntField`}),` control using`,` `,(0,I.jsx)(`code`,{children:`BoundsIntFieldProps`}),` for working with integer bounds in both runtime UI and editor tools.`]}),(0,I.jsxs)(W,{sx:my.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`BoundsIntFieldProps`)})]}),(0,I.jsxs)(W,{sx:my.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,I.jsx)(`code`,{children:`BoundsIntFieldProps`}),` with an initial `,(0,I.jsx)(`code`,{children:`BoundsInt`}),` to render the field. Combine it with `,(0,I.jsx)(`code`,{children:`Hooks.UseState`}),` or signals to keep the value controlled.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:my.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,I.jsxs)(U,{variant:`body1`,children:[(0,I.jsx)(`code`,{children:`BoundsIntField`}),` does not support child nodes. Use the label slot to add context.`]})]}),(0,I.jsxs)(W,{sx:my.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,I.jsx)(`code`,{children:`Label`}),` and `,(0,I.jsx)(`code`,{children:`VisualInput`}),` properties on`,` `,(0,I.jsx)(`code`,{children:`BoundsIntFieldProps`}),` to configure the label and the internal input container. Both expect dictionaries; for example, you can build a label with`,` `,(0,I.jsx)(`code`,{children:`new LabelProps { Text = "BoundsInt" }.ToDictionary()`}),` or provide a`,(0,I.jsx)(`code`,{children:`VisualInput`}),` dictionary that contains a nested `,(0,I.jsx)(`code`,{children:`Style`}),`.`]})]}),(0,I.jsx)($,{componentName:`BoundsIntField`})]});var gy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const _y=()=>(0,I.jsxs)(W,{sx:gy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Box`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Box`}),` renders a boxed container element with optional content. It is useful for grouping related controls with a background and padding.`]}),(0,I.jsxs)(W,{sx:gy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`BoxProps`)})]}),(0,I.jsxs)(W,{sx:gy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,I.jsx)(`code`,{children:`BoxProps`}),` instance to `,(0,I.jsx)(`code`,{children:`V.Box`}),` and supply children as additional arguments.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:gy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,I.jsx)(U,{variant:`body1`,children:`Children are rendered inside the box's content container. Use this to create sections of your UI that share common styling.`})]}),(0,I.jsxs)(W,{sx:gy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (contentContainer)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,I.jsx)(`code`,{children:`ContentContainer`}),` property on `,(0,I.jsx)(`code`,{children:`BoxProps`}),` to style or configure the box's `,(0,I.jsx)(`code`,{children:`contentContainer`}),`. This property expects a dictionary, allowing you to pass a nested `,(0,I.jsx)(`code`,{children:`Style`}),` or additional props that should be applied to the content container element.`]})]}),(0,I.jsx)($,{componentName:`Box`})]});var vy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const yy=()=>(0,I.jsxs)(W,{sx:vy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Button`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Button`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Button`}),` element with`,` `,(0,I.jsx)(`code`,{children:`ButtonProps`}),`. Use it for clickable actions.`]}),(0,I.jsxs)(W,{sx:vy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ButtonProps`)})]}),(0,I.jsxs)(W,{sx:vy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,I.jsx)(`code`,{children:`Text`}),`, optional `,(0,I.jsx)(`code`,{children:`Style`}),`, and an `,(0,I.jsx)(`code`,{children:`OnClick`}),` handler in `,(0,I.jsx)(`code`,{children:`ButtonProps`}),`. Combine with `,(0,I.jsx)(`code`,{children:`Hooks.UseState`}),` to build controlled buttons.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Button`})]});var by={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const xy=()=>(0,I.jsxs)(W,{sx:by.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ColorField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.ColorField`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`ColorField`}),` element using`,` `,(0,I.jsx)(`code`,{children:`ColorFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:by.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ColorFieldProps`)})]}),(0,I.jsxs)(W,{sx:by.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:by.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use `,(0,I.jsx)(`code`,{children:`ColorFieldProps.Label`}),` to configure the label element, and`,` `,(0,I.jsx)(`code`,{children:`ColorFieldProps.VisualInput`}),` to style the input container (for example, padding or background). Both properties accept dictionaries; in most cases you construct them from other typed props or by nesting a `,(0,I.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,I.jsx)($,{componentName:`ColorField`})]});var Sy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Cy=()=>(0,I.jsxs)(W,{sx:Sy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`DoubleField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.DoubleField`}),` exposes a double-precision numeric field via`,` `,(0,I.jsx)(`code`,{children:`DoubleFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Sy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`DoubleFieldProps`)})]}),(0,I.jsxs)(W,{sx:Sy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:Sy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`DoubleFieldProps.Label`}),` and `,(0,I.jsx)(`code`,{children:`DoubleFieldProps.VisualInput`}),` follow the same pattern as other numeric fields. Use a label dictionary (often built from`,` `,(0,I.jsx)(`code`,{children:`LabelProps`}),`) and a visual input dictionary that can contain a nested`,` `,(0,I.jsx)(`code`,{children:`Style`}),` for the inner input container.`]})]}),(0,I.jsx)($,{componentName:`DoubleField`})]});var wy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ty=()=>(0,I.jsxs)(W,{sx:wy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`DropdownField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.DropdownField`}),` renders a text-based dropdown using `,(0,I.jsx)(`code`,{children:`DropdownFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:wy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`DropdownFieldProps`)})]}),(0,I.jsxs)(W,{sx:wy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:wy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`DropdownFieldProps.Label`}),` and `,(0,I.jsx)(`code`,{children:`DropdownFieldProps.VisualInput`}),` mirror the slots on the underlying UI Toolkit control. Use `,(0,I.jsx)(`code`,{children:`Label`}),` to configure the label element, and `,(0,I.jsx)(`code`,{children:`VisualInput`}),` to style the internal input area via a dictionary that can contain a nested `,(0,I.jsx)(`code`,{children:`Style`}),`.`]})]}),(0,I.jsx)($,{componentName:`DropdownField`})]});var Ey={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Dy=()=>(0,I.jsxs)(W,{sx:Ey.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`EnumField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.EnumField`}),` binds to any enum type via `,(0,I.jsx)(`code`,{children:`EnumFieldProps`}),`. Provide the enum's assembly-qualified type name and an initial `,(0,I.jsx)(`code`,{children:`Value`}),`.`]}),(0,I.jsxs)(W,{sx:Ey.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`EnumFieldProps`)})]}),(0,I.jsxs)(W,{sx:Ey.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:Ey.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`EnumFieldProps.Label`}),` and `,(0,I.jsx)(`code`,{children:`EnumFieldProps.VisualInput`}),` configure the label and input slots respectively. As with other fields, both expect dictionaries; label dictionaries are often created from `,(0,I.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),`, while visual input dictionaries typically wrap a `,(0,I.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,I.jsx)($,{componentName:`EnumField`})]});var Oy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ky=()=>(0,I.jsxs)(W,{sx:Oy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`EnumFlagsField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.EnumFlagsField`}),` is similar to `,(0,I.jsx)(`code`,{children:`V.EnumField`}),` but supports`,` `,(0,I.jsx)(`code`,{children:`[Flags]`}),` enums.`]}),(0,I.jsxs)(W,{sx:Oy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`EnumFlagsFieldProps`)})]}),(0,I.jsxs)(W,{sx:Oy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:Oy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`EnumFlagsFieldProps.Label`}),` and `,(0,I.jsx)(`code`,{children:`EnumFlagsFieldProps.VisualInput`}),`behave the same as on `,(0,I.jsx)(`code`,{children:`EnumFieldProps`}),`, allowing you to style the label element and the embedded input area via dictionaries that can contain nested `,(0,I.jsx)(`code`,{children:`Style`}),` `,`objects.`]})]}),(0,I.jsx)($,{componentName:`EnumFlagsField`})]});var Ay={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const jy=()=>(0,I.jsxs)(W,{sx:Ay.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`FloatField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.FloatField`}),` represents a single-precision numeric field, backed by`,` `,(0,I.jsx)(`code`,{children:`FloatFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Ay.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`FloatFieldProps`)})]}),(0,I.jsxs)(W,{sx:Ay.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:Ay.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`FloatFieldProps.Label`}),` and `,(0,I.jsx)(`code`,{children:`FloatFieldProps.VisualInput`}),` let you customize the label element and the inner input container. Both accept dictionaries: build a label via `,(0,I.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),` and pass a dictionary with a nested`,` `,(0,I.jsx)(`code`,{children:`Style`}),` object to `,(0,I.jsx)(`code`,{children:`VisualInput`}),` to style the input.`]})]}),(0,I.jsx)($,{componentName:`FloatField`})]});var My={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ny=()=>(0,I.jsxs)(W,{sx:My.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Foldout`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Foldout`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Foldout`}),` element using`,` `,(0,I.jsx)(`code`,{children:`FoldoutProps`}),`. It is useful for expandable sections of UI that reveal more content when open.`]}),(0,I.jsxs)(W,{sx:My.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`FoldoutProps`)})]}),(0,I.jsxs)(W,{sx:My.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,I.jsx)(`code`,{children:`Text`}),`, an optional initial `,(0,I.jsx)(`code`,{children:`Value`}),`, and an`,` `,(0,I.jsx)(`code`,{children:`OnChange`}),` handler. The example below also shows children rendered inside the foldout when it is expanded.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:My.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Children passed to `,(0,I.jsx)(`code`,{children:`V.Foldout`}),` are rendered inside the foldout's content area and are shown or hidden based on the current `,(0,I.jsx)(`code`,{children:`Value`}),`.`]})]}),(0,I.jsxs)(W,{sx:My.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (header / contentContainer)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use `,(0,I.jsx)(`code`,{children:`FoldoutProps.Header`}),` and `,(0,I.jsx)(`code`,{children:`FoldoutProps.ContentContainer`}),` to style the header bar and inner content container. Both accept dictionaries; commonly a nested`,` `,(0,I.jsx)(`code`,{children:`Style`}),` is provided under the `,(0,I.jsx)(`code`,{children:`"style"`}),` key.`]})]}),(0,I.jsxs)(W,{sx:My.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Controlled value`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`For controlled foldouts, track a `,(0,I.jsx)(`code`,{children:`bool`}),` with `,(0,I.jsx)(`code`,{children:`Hooks.UseState`}),` (or a signal) and update it in `,(0,I.jsx)(`code`,{children:`OnChange`}),`. The `,(0,I.jsx)(`code`,{children:`Value`}),` property will then always reflect your source of truth.`]})]}),(0,I.jsx)($,{componentName:`Foldout`})]});var Py={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Fy=()=>(0,I.jsxs)(W,{sx:Py.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`GroupBox`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.GroupBox`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`GroupBox`}),` element using`,` `,(0,I.jsx)(`code`,{children:`GroupBoxProps`}),`. It is useful for grouping related controls under a titled header.`]}),(0,I.jsxs)(W,{sx:Py.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`GroupBoxProps`)})]}),(0,I.jsxs)(W,{sx:Py.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,I.jsx)(`code`,{children:`Text`}),` for the group title, a `,(0,I.jsx)(`code`,{children:`Style`}),` for layout, and add children that will appear inside the group.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:Py.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Children passed to `,(0,I.jsx)(`code`,{children:`V.GroupBox`}),` are rendered inside the group's content container, below the labeled header.`]})]}),(0,I.jsxs)(W,{sx:Py.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / contentContainer)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Use `,(0,I.jsx)(`code`,{children:`GroupBoxProps.Label`}),` and `,(0,I.jsx)(`code`,{children:`GroupBoxProps.ContentContainer`}),` to style the header label and the inner content container. Both properties accept dictionaries, often containing nested `,(0,I.jsx)(`code`,{children:`Style`}),` objects.`]})]}),(0,I.jsx)($,{componentName:`GroupBox`})]});var Iy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ly=()=>(0,I.jsxs)(W,{sx:Iy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Hash128Field`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Hash128Field`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Hash128Field`}),` for editing`,` `,(0,I.jsx)(`code`,{children:`Hash128`}),` values.`]}),(0,I.jsxs)(W,{sx:Iy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`Hash128FieldProps`)})]}),(0,I.jsxs)(W,{sx:Iy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Hash128Field`})]});var Ry={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const zy=()=>(0,I.jsxs)(W,{sx:Ry.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`HelpBox`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.HelpBox`}),` wraps the standard UI Toolkit `,(0,I.jsx)(`code`,{children:`HelpBox`}),` for displaying informational, warning, or error messages.`]}),(0,I.jsxs)(W,{sx:Ry.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`HelpBoxProps`)})]}),(0,I.jsxs)(W,{sx:Ry.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`HelpBox`})]});var By={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Vy=()=>(0,I.jsxs)(W,{sx:By.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`IMGUIContainer`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.IMGUIContainer`}),` lets you embed IMGUI content inside a UI Toolkit layout by providing an `,(0,I.jsx)(`code`,{children:`OnGUI`}),` callback in `,(0,I.jsx)(`code`,{children:`IMGUIContainerProps`}),`. This is primarily an editor-only pattern.`]}),(0,I.jsxs)(W,{sx:By.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`IMGUIContainerProps`)})]}),(0,I.jsxs)(W,{sx:By.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

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
}`})]}),(0,I.jsx)($,{componentName:`IMGUIContainer`})]});var Hy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Uy=()=>(0,I.jsxs)(W,{sx:Hy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Image`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Image`}),` renders a UI Toolkit `,(0,I.jsx)(`code`,{children:`Image`}),` using `,(0,I.jsx)(`code`,{children:`ImageProps`}),`. It supports both `,(0,I.jsx)(`code`,{children:`Texture2D`}),` and `,(0,I.jsx)(`code`,{children:`Sprite`}),` sources.`]}),(0,I.jsxs)(W,{sx:Hy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ImageProps`)})]}),(0,I.jsxs)(W,{sx:Hy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Image`})]});var Wy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Gy=()=>(0,I.jsxs)(W,{sx:Wy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`IntegerField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.IntegerField`}),` represents an integer numeric field using `,(0,I.jsx)(`code`,{children:`IntegerFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Wy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`IntegerFieldProps`)})]}),(0,I.jsxs)(W,{sx:Wy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`IntegerField`})]});var Ky={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const qy=()=>(0,I.jsxs)(W,{sx:Ky.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Label`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Label`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Label`}),` element via `,(0,I.jsx)(`code`,{children:`LabelProps`}),`. It is the primary way to render text in your component trees.`]}),(0,I.jsxs)(W,{sx:Ky.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`LabelProps`)})]}),(0,I.jsxs)(W,{sx:Ky.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Label`})]});var Jy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Yy=()=>(0,I.jsxs)(W,{sx:Jy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`LongField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.LongField`}),` represents a 64-bit integer field using `,(0,I.jsx)(`code`,{children:`LongFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Jy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`LongFieldProps`)})]}),(0,I.jsxs)(W,{sx:Jy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`LongField`})]});var Xy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Zy=()=>(0,I.jsxs)(W,{sx:Xy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ProgressBar`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.ProgressBar`}),` renders a UI Toolkit `,(0,I.jsx)(`code`,{children:`ProgressBar`}),` using`,` `,(0,I.jsx)(`code`,{children:`ProgressBarProps`}),`. It is typically driven by state changes elsewhere in your UI.`]}),(0,I.jsxs)(W,{sx:Xy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ProgressBarProps`)})]}),(0,I.jsxs)(W,{sx:Xy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsxs)(W,{sx:Xy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Styling track and fill`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The`,` `,(0,I.jsx)(`a`,{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ProgressBar.html`,target:`_blank`,rel:`noreferrer`,children:`Unity ProgressBar documentation`}),` `,`highlights that the root element is the visible track, while the inner`,` `,(0,I.jsx)(`code`,{children:`.unity-progress-bar__progress`}),` child renders the filled portion.`]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Assign styles to the track via `,(0,I.jsx)(`code`,{children:`ProgressBarProps.Style`}),` (for border, unfilled background, size, etc.) and target the fill through the `,(0,I.jsx)(`code`,{children:`Progress`}),` slot. You can also style the caption by populating `,(0,I.jsx)(`code`,{children:`TitleElement`}),`. The example above uses this pattern to create a progress bar with a dark green track, a lighter fill, and centered text.`]})]}),(0,I.jsx)($,{componentName:`ProgressBar`})]});var Qy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const $y=()=>(0,I.jsxs)(W,{sx:Qy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ListView`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.ListView`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`ListView`}),` control using`,` `,(0,I.jsx)(`code`,{children:`ListViewProps`}),`. It can use either the standard `,(0,I.jsx)(`code`,{children:`makeItem/bindItem`}),` `,`properties or the higher-level `,(0,I.jsx)(`code`,{children:`Row`}),` function that returns a `,(0,I.jsx)(`code`,{children:`VirtualNode`}),`.`]}),(0,I.jsxs)(W,{sx:Qy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ListViewProps`)})]}),(0,I.jsxs)(W,{sx:Qy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`ListView`})]});var eb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const tb=()=>(0,I.jsxs)(W,{sx:eb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MinMaxSlider`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.MinMaxSlider`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`MinMaxSlider`}),` element using`,` `,(0,I.jsx)(`code`,{children:`MinMaxSliderProps`}),` for selecting a range between two limits.`]}),(0,I.jsxs)(W,{sx:eb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`MinMaxSliderProps`)})]}),(0,I.jsxs)(W,{sx:eb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`MinMaxSlider`})]});var nb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const rb=()=>(0,I.jsxs)(W,{sx:nb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ObjectField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.ObjectField`}),` wraps the editor-only UI Toolkit `,(0,I.jsx)(`code`,{children:`ObjectField`}),` element using `,(0,I.jsx)(`code`,{children:`ObjectFieldProps`}),`. It is typically used in custom inspectors and tools.`]}),(0,I.jsxs)(W,{sx:nb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ObjectFieldProps`)})]}),(0,I.jsxs)(W,{sx:nb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

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
}`})]}),(0,I.jsx)($,{componentName:`ObjectField`})]});var ib={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ab=()=>(0,I.jsxs)(W,{sx:ib.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RadioButton`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.RadioButton`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`RadioButton`}),` element using`,` `,(0,I.jsx)(`code`,{children:`RadioButtonProps`}),`. It is usually used within a `,(0,I.jsx)(`code`,{children:`RadioButtonGroup`}),`.`]}),(0,I.jsxs)(W,{sx:ib.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`RadioButtonProps`)})]}),(0,I.jsxs)(W,{sx:ib.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`RadioButton`})]});var ob={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const sb=()=>(0,I.jsxs)(W,{sx:ob.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RadioButtonGroup`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.RadioButtonGroup`}),` wraps UI Toolkit's `,(0,I.jsx)(`code`,{children:`RadioButtonGroup`}),` using`,` `,(0,I.jsx)(`code`,{children:`RadioButtonGroupProps`}),`. It manages a set of mutually exclusive choices.`]}),(0,I.jsxs)(W,{sx:ob.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`RadioButtonGroupProps`)})]}),(0,I.jsxs)(W,{sx:ob.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`RadioButtonGroup`})]});var cb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const lb=()=>(0,I.jsxs)(W,{sx:cb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RepeatButton`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.RepeatButton`}),` wraps UI Toolkit's `,(0,I.jsx)(`code`,{children:`RepeatButton`}),`, invoking`,` `,(0,I.jsx)(`code`,{children:`OnClick`}),` repeatedly while the button is held.`]}),(0,I.jsxs)(W,{sx:cb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`RepeatButtonProps`)})]}),(0,I.jsxs)(W,{sx:cb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`RepeatButton`})]});var ub={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const db=()=>(0,I.jsxs)(W,{sx:ub.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ScrollView`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.ScrollView`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`ScrollView`}),` element using`,` `,(0,I.jsx)(`code`,{children:`ScrollViewProps`}),`. It is the primary way to add scrolling regions to your layouts.`]}),(0,I.jsxs)(W,{sx:ub.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ScrollViewProps`)})]}),(0,I.jsxs)(W,{sx:ub.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`ScrollView`})]});var fb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const pb=()=>(0,I.jsxs)(W,{sx:fb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Slider`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Slider`}),` renders a float slider using `,(0,I.jsx)(`code`,{children:`SliderProps`}),`. In addition to basic value and range, you can optionally style the inner parts of the UI Toolkit slider through slot dictionaries (`,(0,I.jsx)(`code`,{children:`Input`}),`, `,(0,I.jsx)(`code`,{children:`Track`}),`, `,(0,I.jsx)(`code`,{children:`DragContainer`}),`,`,` `,(0,I.jsx)(`code`,{children:`Handle`}),`, and `,(0,I.jsx)(`code`,{children:`HandleBorder`}),`), which map to the corresponding visual elements inside the control.`]}),(0,I.jsxs)(W,{sx:fb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`SliderProps`)})]}),(0,I.jsxs)(W,{sx:fb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Slider`})]});var mb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const hb=()=>(0,I.jsxs)(W,{sx:mb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`SliderInt`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.SliderInt`}),` renders an integer slider using `,(0,I.jsx)(`code`,{children:`SliderIntProps`}),`.`]}),(0,I.jsxs)(W,{sx:mb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`SliderIntProps`)})]}),(0,I.jsxs)(W,{sx:mb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`SliderInt`})]});var gb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const _b=()=>(0,I.jsxs)(W,{sx:gb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Toggle`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Toggle`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Toggle`}),` control using `,(0,I.jsx)(`code`,{children:`ToggleProps`}),`.`]}),(0,I.jsxs)(W,{sx:gb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ToggleProps`)})]}),(0,I.jsxs)(W,{sx:gb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Toggle`})]});var vb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const yb=()=>(0,I.jsxs)(W,{sx:vb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TreeView`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.TreeView`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`TreeView`}),` control using`,` `,(0,I.jsx)(`code`,{children:`TreeViewProps`}),`, allowing you to render hierarchical data with a`,` `,(0,I.jsx)(`code`,{children:`Row`}),` function that returns `,(0,I.jsx)(`code`,{children:`VirtualNode`}),` instances.`]}),(0,I.jsxs)(W,{sx:vb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`TreeViewProps`)})]}),(0,I.jsxs)(W,{sx:vb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`TreeView`})]});var bb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const xb=()=>(0,I.jsxs)(W,{sx:bb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Tab`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Tab`}),` renders an individual tab using `,(0,I.jsx)(`code`,{children:`TabProps`}),`. In most cases you will use it indirectly via `,(0,I.jsx)(`code`,{children:`TabView`}),`, but you can also construct tab strips manually.`]}),(0,I.jsxs)(W,{sx:bb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`TabProps`)})]}),(0,I.jsxs)(W,{sx:bb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Tab`})]});var Sb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Cb=()=>(0,I.jsxs)(W,{sx:Sb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TabView`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.TabView`}),` renders a tab strip and tab content using `,(0,I.jsx)(`code`,{children:`TabViewProps`}),`. Each tab is defined by a `,(0,I.jsx)(`code`,{children:`TabViewProps.TabDef`}),`, which can provide either static content or a factory function.`]}),(0,I.jsxs)(W,{sx:Sb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`TabViewProps`)})]}),(0,I.jsxs)(W,{sx:Sb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`TabView`})]});var wb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Tb=()=>(0,I.jsxs)(W,{sx:wb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ToggleButtonGroup`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.ToggleButtonGroup`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`ToggleButtonGroup`}),` element using `,(0,I.jsx)(`code`,{children:`ToggleButtonGroupProps`}),`. Provide a zero-based `,(0,I.jsx)(`code`,{children:`Value`}),` index and add regular `,(0,I.jsx)(`code`,{children:`V.Button`}),` children, handling each button's `,(0,I.jsx)(`code`,{children:`OnClick`}),`to drive your own selection state.`]}),(0,I.jsxs)(W,{sx:wb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ToggleButtonGroupProps`)})]}),(0,I.jsxs)(W,{sx:wb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`ToggleButtonGroup`})]});var Eb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Db=()=>(0,I.jsxs)(W,{sx:Eb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TextField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.TextField`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`TextField`}),` using`,` `,(0,I.jsx)(`code`,{children:`TextFieldProps`}),`, with support for slots like `,(0,I.jsx)(`code`,{children:`Label`}),`,`,` `,(0,I.jsx)(`code`,{children:`Input`}),`, and `,(0,I.jsx)(`code`,{children:`TextElement`}),`.`]}),(0,I.jsxs)(W,{sx:Eb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`TextFieldProps`)})]}),(0,I.jsxs)(W,{sx:Eb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`TextField`})]});var Ob={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const kb=()=>(0,I.jsxs)(W,{sx:Ob.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Toolbar`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Toolbar`}),` and related helpers (`,(0,I.jsx)(`code`,{children:`V.ToolbarButton`}),`,`,` `,(0,I.jsx)(`code`,{children:`V.ToolbarToggle`}),`, `,(0,I.jsx)(`code`,{children:`V.ToolbarMenu`}),`, etc.) wrap the UI Toolkit editor toolbar elements using the `,(0,I.jsx)(`code`,{children:`ToolbarProps`}),` family.`]}),(0,I.jsxs)(W,{sx:Ob.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ToolbarProps`)})]}),(0,I.jsxs)(W,{sx:Ob.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

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
}`})]}),(0,I.jsx)($,{componentName:`Toolbar`})]});var Ab={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const jb=()=>(0,I.jsxs)(W,{sx:Ab.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RectField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.RectField`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`RectField`}),` control using`,` `,(0,I.jsx)(`code`,{children:`RectFieldProps`}),`. It is available in both runtime and editor UIs.`]}),(0,I.jsxs)(W,{sx:Ab.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`RectFieldProps`)})]}),(0,I.jsxs)(W,{sx:Ab.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`RectField`})]});var Mb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Nb=()=>(0,I.jsxs)(W,{sx:Mb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RectIntField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.RectIntField`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`RectIntField`}),` control using`,` `,(0,I.jsx)(`code`,{children:`RectIntFieldProps`}),`. It is available in both runtime and editor UIs.`]}),(0,I.jsxs)(W,{sx:Mb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`RectIntFieldProps`)})]}),(0,I.jsxs)(W,{sx:Mb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`RectIntField`})]});var Pb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Fb=()=>(0,I.jsxs)(W,{sx:Pb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UnsignedIntegerField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.UnsignedIntegerField`}),` represents a `,(0,I.jsx)(`code`,{children:`uint`}),` numeric field using`,` `,(0,I.jsx)(`code`,{children:`UnsignedIntegerFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Pb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`UnsignedIntegerFieldProps`)})]}),(0,I.jsxs)(W,{sx:Pb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`UnsignedIntegerField`})]});var Ib={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Lb=()=>(0,I.jsxs)(W,{sx:Ib.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UnsignedLongField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.UnsignedLongField`}),` represents a `,(0,I.jsx)(`code`,{children:`ulong`}),` numeric field using`,` `,(0,I.jsx)(`code`,{children:`UnsignedLongFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Ib.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`UnsignedLongFieldProps`)})]}),(0,I.jsxs)(W,{sx:Ib.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`UnsignedLongField`})]});var Rb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const zb=()=>(0,I.jsxs)(W,{sx:Rb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector2Field`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Vector2Field`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Vector2Field`}),` control using`,` `,(0,I.jsx)(`code`,{children:`Vector2FieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Rb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`Vector2FieldProps`)})]}),(0,I.jsxs)(W,{sx:Rb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Vector2Field`})]});var Bb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Vb=()=>(0,I.jsxs)(W,{sx:Bb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector2IntField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Vector2IntField`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Vector2IntField`}),` control using`,` `,(0,I.jsx)(`code`,{children:`Vector2IntFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Bb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`Vector2IntFieldProps`)})]}),(0,I.jsxs)(W,{sx:Bb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Vector2IntField`})]});var Hb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ub=()=>(0,I.jsxs)(W,{sx:Hb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector3Field`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Vector3Field`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Vector3Field`}),` control using`,` `,(0,I.jsx)(`code`,{children:`Vector3FieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Hb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`Vector3FieldProps`)})]}),(0,I.jsxs)(W,{sx:Hb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Vector3Field`})]});var Wb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Gb=()=>(0,I.jsxs)(W,{sx:Wb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector3IntField`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Vector3IntField`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Vector3IntField`}),` control using`,` `,(0,I.jsx)(`code`,{children:`Vector3IntFieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Wb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`Vector3IntFieldProps`)})]}),(0,I.jsxs)(W,{sx:Wb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Vector3IntField`})]});var Kb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const qb=()=>(0,I.jsxs)(W,{sx:Kb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector4Field`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Vector4Field`}),` wraps the UI Toolkit `,(0,I.jsx)(`code`,{children:`Vector4Field`}),` control using`,` `,(0,I.jsx)(`code`,{children:`Vector4FieldProps`}),`.`]}),(0,I.jsxs)(W,{sx:Kb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`Vector4FieldProps`)})]}),(0,I.jsxs)(W,{sx:Kb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Vector4Field`})]});var Jb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Yb=()=>(0,I.jsxs)(W,{sx:Jb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TemplateContainer`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.TemplateContainer`}),` wraps UI Toolkit `,(0,I.jsx)(`code`,{children:`TemplateContainer`}),` and exposes a`,` `,(0,I.jsx)(`code`,{children:`ContentContainer`}),` slot through `,(0,I.jsx)(`code`,{children:`TemplateContainerProps`}),`.`]}),(0,I.jsxs)(W,{sx:Jb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`TemplateContainerProps`)})]}),(0,I.jsxs)(W,{sx:Jb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`TemplateContainer`})]});var Xb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Zb=()=>(0,I.jsxs)(W,{sx:Xb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`VisualElement`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.VisualElement`}),` creates a generic container element styled via a `,(0,I.jsx)(`code`,{children:`Style`}),` `,`instance, and is often used as the top-level layout node for your component trees.`]}),(0,I.jsxs)(W,{sx:Xb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signature`}),(0,I.jsx)(Z,{language:`tsx`,code:`public static VirtualNode VisualElement(
  Style style,
  string key = null,
  params VirtualNode[] children
);

public static VirtualNode VisualElement(
  IReadOnlyDictionary<string, object> elementProperties = null,
  string key = null,
  params VirtualNode[] children
);`})]}),(0,I.jsxs)(W,{sx:Xb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic container`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`VisualElement`})]});var Qb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const $b=()=>(0,I.jsxs)(W,{sx:Qb.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`VisualElementSafe`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.VisualElementSafe`}),` is a safe-area-aware variant of `,(0,I.jsx)(`code`,{children:`V.VisualElement`}),` `,`that merges its padding with safe-area insets from `,(0,I.jsx)(`code`,{children:`SafeAreaUtility`}),`. Use it as a top-level container on devices with notches or system UI overlays.`]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Pass either a `,(0,I.jsx)(`code`,{children:`Style`}),` or the same props dictionary you would send to`,` `,(0,I.jsx)(`code`,{children:`V.VisualElement`}),` (e.g., `,(0,I.jsx)(`code`,{children:`pickingMode`}),`, `,(0,I.jsx)(`code`,{children:`name`}),`, refs, event handlers). The helper clones those props, replaces/merges the `,(0,I.jsx)(`code`,{children:`style`}),` entry, and leaves everything else untouched.`]}),(0,I.jsxs)(W,{sx:Qb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signature`}),(0,I.jsx)(Z,{language:`tsx`,code:`public static VirtualNode VisualElementSafe(
  object elementPropsOrStyle = null,
  string key = null,
  params VirtualNode[] children
);`})]}),(0,I.jsxs)(W,{sx:Qb.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Safe-area aware container`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]})]});var ex={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const tx=()=>(0,I.jsxs)(W,{sx:ex.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Animate`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Animate`}),` wraps a child subtree and drives one or more animation tracks on its root `,(0,I.jsx)(`code`,{children:`VisualElement`}),`. It is a thin, declarative wrapper around`,` `,(0,I.jsx)(`code`,{children:`Hooks.UseAnimate`}),` and the underlying `,(0,I.jsx)(`code`,{children:`Animator`}),` helpers.`]}),(0,I.jsxs)(W,{sx:ex.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`AnimateProps`)})]}),(0,I.jsxs)(W,{sx:ex.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,I.jsxs)(G,{sx:ex.section,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Tracks are defined via AnimateTrack helpers and target individual style properties (for example, backgroundColor, opacity, size).`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Each track specifies from/to values, duration, easing, and optional delay.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`When the Animate node mounts or its dependencies change, tracks are played; they are stopped and cleaned up automatically when unmounting.`})})]})]}),(0,I.jsxs)(W,{sx:ex.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]})]});var nx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const rx=()=>(0,I.jsxs)(W,{sx:nx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ErrorBoundary`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.ErrorBoundary`}),` catches exceptions from its descendants and renders the`,` `,(0,I.jsx)(`code`,{children:`Fallback`}),` `,(0,I.jsx)(`code`,{children:`VirtualNode`}),` from `,(0,I.jsx)(`code`,{children:`ErrorBoundaryProps`}),`.`]}),(0,I.jsxs)(W,{sx:nx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ErrorBoundaryProps`)})]}),(0,I.jsxs)(W,{sx:nx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]})]});var ix={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ax=()=>(0,I.jsxs)(W,{sx:ix.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MultiColumnListView`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.MultiColumnListView`}),` displays tabular data with columns configured via`,` `,(0,I.jsx)(`code`,{children:`MultiColumnListViewProps`}),`. It is backed by Unity's`,` `,(0,I.jsx)(`code`,{children:`MultiColumnListView`}),` control and supports large, virtualized data sets.`]}),(0,I.jsxs)(W,{sx:ix.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`MultiColumnListViewProps`)})]}),(0,I.jsxs)(W,{sx:ix.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,I.jsxs)(G,{sx:ix.section,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Items are provided as an IList; rows are virtualized by the underlying control for performance.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Columns are defined via MultiColumnListViewColumn objects, each with a name, width, and Cell callback.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`The Cell callback receives the strongly-typed row item and index so you can render arbitrary content per column.`})})]})]}),(0,I.jsxs)(W,{sx:ix.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`MultiColumnListView`})]});var ox={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const sx=()=>(0,I.jsxs)(W,{sx:ox.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MultiColumnTreeView`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.MultiColumnTreeView`}),` renders hierarchical data across multiple columns via`,` `,(0,I.jsx)(`code`,{children:`MultiColumnTreeViewProps`}),`. It is backed by Unity's`,` `,(0,I.jsx)(`code`,{children:`MultiColumnTreeView`}),` control and is suitable for project browser–style views.`]}),(0,I.jsxs)(W,{sx:ox.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`MultiColumnTreeViewProps`)})]}),(0,I.jsxs)(W,{sx:ox.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,I.jsxs)(G,{sx:ox.section,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Items are provided as a tree of nodes; the adapter flattens and expands them based on TreeView state.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Columns are defined via MultiColumnTreeViewColumn objects, just like MultiColumnListView.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Each Cell callback receives the node item and index so you can render per-column content (labels, badges, icons).`})})]})]}),(0,I.jsxs)(W,{sx:ox.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`MultiColumnTreeView`})]});var cx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const lx=()=>(0,I.jsxs)(W,{sx:cx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Scroller`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.Scroller`}),` wraps the low-level UI Toolkit `,(0,I.jsx)(`code`,{children:`Scroller`}),` element using`,` `,(0,I.jsx)(`code`,{children:`ScrollerProps`}),`.`]}),(0,I.jsxs)(W,{sx:cx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`ScrollerProps`)})]}),(0,I.jsxs)(W,{sx:cx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`Scroller`})]});var ux={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const dx=()=>(0,I.jsxs)(W,{sx:ux.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TextElement`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`V.TextElement`}),` is a low-level text node wrapper using `,(0,I.jsx)(`code`,{children:`TextElementProps`}),`.`]}),(0,I.jsxs)(W,{sx:ux.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`TextElementProps`)})]}),(0,I.jsxs)(W,{sx:ux.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`TextElement`})]});var fx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const px=()=>(0,I.jsxs)(W,{sx:fx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`PropertyField & InspectorElement`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Editor-only helpers that wrap Unity's `,(0,I.jsx)(`code`,{children:`PropertyField`}),` and `,(0,I.jsx)(`code`,{children:`InspectorElement`}),` `,`via `,(0,I.jsx)(`code`,{children:`PropertyFieldProps`}),` and `,(0,I.jsx)(`code`,{children:`InspectorElementProps`}),`.`]}),(0,I.jsxs)(W,{sx:fx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`PropertyInspectorProps`)})]}),(0,I.jsxs)(W,{sx:fx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`PropertyInspector`})]});var mx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const hx=()=>(0,I.jsxs)(W,{sx:mx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TwoPaneSplitView`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Editor-only splitter layout wrapping Unity's `,(0,I.jsx)(`code`,{children:`TwoPaneSplitView`}),` via`,` `,(0,I.jsx)(`code`,{children:`TwoPaneSplitViewProps`}),`.`]}),(0,I.jsxs)(W,{sx:mx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,I.jsx)(Z,{language:`tsx`,code:Q(`TwoPaneSplitViewProps`)})]}),(0,I.jsxs)(W,{sx:mx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,I.jsx)($,{componentName:`TwoPaneSplitView`})]});var gx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const _x=()=>(0,I.jsxs)(W,{sx:gx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Known Issues`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:gx.section,children:`Runtime`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`There is a known issue where `,(0,I.jsx)(`code`,{children:`MultiColumnListView`}),` can briefly jump or snap when scrolling large data sets; this will be addressed in a future update.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:gx.section,children:`Burst AOT & Assembly Resolution`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`If you encounter the error:`}),(0,I.jsx)(Z,{language:`text`,code:`Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: Assembly-CSharp-Editor`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Go to `,(0,I.jsx)(`strong`,{children:`Edit → Project Settings → Burst AOT Settings`}),` and add `,(0,I.jsx)(`code`,{children:`Assembly-CSharp-Editor`}),` to the exclusion list. This prevents Burst from trying to AOT-compile editor-only assemblies that reference UITKX types.`]})]});var vx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const yx=()=>(0,I.jsxs)(W,{sx:vx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Roadmap`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`The roadmap will be documented here in a future update.`})]});var bx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},xx=`using System.Collections.Generic;
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
}`,Sx=`using System.Collections.Generic;
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
}`;const Cx=()=>(0,I.jsxs)(W,{sx:bx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special animation hooks`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`ReactiveUIToolKit exposes animation-specific hooks that do not exist in React's core API. These hooks are designed to drive UI Toolkit animations in a frame-accurate way while still fitting into the normal function component lifecycle.`}),(0,I.jsxs)(W,{sx:bx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,I.jsx)(`code`,{children:`Hooks.UseAnimate`})}),(0,I.jsxs)(G,{sx:bx.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Starts one or more AnimateTrack definitions on the component's VisualElement container.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Tracks are created with ReactiveUITK.Core.Animation.AnimateTrack helpers (for example, animating background color or size).`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Plays animations when dependencies change, and stops/cleans them up when the component unmounts or the effect is re-run.`})})]}),(0,I.jsx)(Z,{language:`tsx`,code:xx})]}),(0,I.jsxs)(W,{sx:bx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,I.jsx)(`code`,{children:`Hooks.UseTweenFloat`})}),(0,I.jsxs)(G,{sx:bx.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Tweens a single float value over time with easing and an optional delay.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Calls an onUpdate callback every frame with the eased value, and an onComplete callback when finished.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Uses UI Toolkit's scheduler and integrates with the component's lifecycle; cancelling on unmount.`})})]}),(0,I.jsx)(Z,{language:`tsx`,code:Sx})]}),(0,I.jsxs)(U,{variant:`body2`,sx:bx.section,children:[`For a higher-level API, see the `,(0,I.jsx)(`code`,{children:`Animate`}),` component documented under Components → Common/Uncommon Components. It builds on top of these hooks and the underlying`,` `,(0,I.jsx)(`code`,{children:`Animator`}),` utilities.`]})]});var wx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},Tx=`using System.Collections.Generic;
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
}`;const Ex=()=>(0,I.jsxs)(W,{sx:wx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special router hooks`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`The router in ReactiveUIToolKit ships with a set of hooks that mirror React Router's ergonomics but are implemented entirely in C# for Unity UI Toolkit.`}),(0,I.jsxs)(W,{sx:wx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Reading router state`}),(0,I.jsxs)(G,{sx:wx.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),` / `,(0,I.jsx)(`code`,{children:`UseLocationInfo()`}),` – current path, query, and optional navigation state.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` – path parameters extracted from the active route template.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` – parsed query-string key/value pairs.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` – arbitrary state object provided when navigating.`]})})})]})]}),(0,I.jsxs)(W,{sx:wx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Navigation helpers`}),(0,I.jsxs)(G,{sx:wx.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigate(replace = false)`}),` – imperative navigation, similar to React Router's `,(0,I.jsx)(`code`,{children:`useNavigate`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseGo()`}),` – navigate relative to the history stack (for example, `,(0,I.jsx)(`code`,{children:`go(-1)`}),`, `,(0,I.jsx)(`code`,{children:`go(1)`}),`).`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseCanGo(delta)`}),` – returns whether a given delta is available for back/forward UI.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseBlocker(blocker, enabled)`}),` – intercepts transitions to implement confirmation prompts.`]})})})]}),(0,I.jsx)(Z,{language:`tsx`,code:Tx})]}),(0,I.jsxs)(U,{variant:`body2`,sx:wx.section,children:[`See the main Router documentation for complete examples of composing `,(0,I.jsx)(`code`,{children:`V.Router`}),`,`,` `,(0,I.jsx)(`code`,{children:`V.Route`}),`, `,(0,I.jsx)(`code`,{children:`V.Link`}),`, and these hooks in editor and runtime apps.`]})]});var Dx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},Ox=`using System.Collections.Generic;
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
}`;const kx=()=>(0,I.jsxs)(W,{sx:Dx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special signal hooks`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Signals provide a small, global, observable state primitive. The`,` `,(0,I.jsx)(`code`,{children:`Hooks.UseSignal`}),` family gives you fine-grained reactivity from function components, something React does not have out of the box.`]}),(0,I.jsxs)(W,{sx:Dx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,I.jsx)(`code`,{children:`Hooks.UseSignal`})}),(0,I.jsxs)(G,{sx:Dx.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`Hooks.UseSignal(Signal<T>)`}),` – subscribe to a`,` `,(0,I.jsx)(`code`,{children:`Signal<T>`}),` and re-render when it changes.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`Hooks.UseSignal<T>(key, initialValue)`}),` – shorthand that resolves a`,` `,(0,I.jsx)(`code`,{children:`Signal<T>`}),` from the global registry by key.`]})})})]}),(0,I.jsx)(Z,{language:`tsx`,code:Ox})]}),(0,I.jsxs)(W,{sx:Dx.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Selector overloads`}),(0,I.jsxs)(G,{sx:Dx.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(signal, selector, comparer)`}),` – project a slice of a signal value and control equality.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(key, selector, comparer, initialValue)`}),` `,`– keyed variant that creates/resolves the signal for you.`]})})})]})]}),(0,I.jsxs)(U,{variant:`body2`,sx:Dx.section,children:[`For an end-to-end walkthrough, see the Signals page, which shows how to combine`,` `,(0,I.jsx)(`code`,{children:`Signals.Get`}),`, `,(0,I.jsx)(`code`,{children:`Hooks.UseSignal`}),`, and dispatch helpers in real UIs.`]})]});var Ax={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},jx=`using System.Collections.Generic;
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
}`;const Mx=()=>(0,I.jsxs)(W,{sx:Ax.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Safe area hooks`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`When targeting mobile or platforms with notches and system insets, the`,` `,(0,I.jsx)(`code`,{children:`Hooks.UseSafeArea`}),` hook and `,(0,I.jsx)(`code`,{children:`V.VisualElementSafe`}),` helper work together to keep your layout inside the safe region.`]}),(0,I.jsxs)(W,{sx:Ax.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,I.jsx)(`code`,{children:`Hooks.UseSafeArea`})}),(0,I.jsxs)(G,{sx:Ax.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Returns SafeAreaInsets (top, bottom, left, right) based on Unity's Screen.safeArea.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Records a hook usage so that changes to the safe area can trigger re-rendering.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Accepts an optional tolerance parameter to avoid flicker when the reported insets change only slightly.`})})]}),(0,I.jsx)(Z,{language:`tsx`,code:jx})]}),(0,I.jsxs)(W,{sx:Ax.section,children:[(0,I.jsxs)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:[(0,I.jsx)(`code`,{children:`V.VisualElementSafe`}),` helper`]}),(0,I.jsxs)(G,{sx:Ax.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`V.VisualElementSafe(propsOrStyle, key, children) – takes either a Style or a props dictionary, wraps a VisualElement, and automatically applies padding based on SafeAreaInsets.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Merges your own padding with the safe-area padding so you keep control over layout while staying visible on all devices.`})})]})]}),(0,I.jsxs)(U,{variant:`body2`,sx:Ax.section,children:[`Combine `,(0,I.jsx)(`code`,{children:`Hooks.UseSafeArea`}),` when you need direct access to inset values with`,` `,(0,I.jsx)(`code`,{children:`V.VisualElementSafe`}),` when you want a drop-in, safe-area-aware container.`]})]}),Nx=[{id:`intro`,title:`Introduction`,pages:[{id:`introduction`,title:`Introduction`,path:`/`,keywords:[`overview`,`unity 6.2`,`reactive`,`ui toolkit`],searchContent:`reactiveuitoolkit react-like component model unity hooks state effects reconciliation v.func v.memo visualelement diffing scheduler rendering fiber dom-like`,element:()=>(0,I.jsx)(ev,{})}]},{id:`getting-started`,title:`Getting Started`,pages:[{id:`install`,title:`Install & Setup`,path:`/getting-started`,keywords:[`install`,`setup`,`unity package manager`,`dist`],searchContent:`install setup unity package manager git url create component mount rootrenderer uidocument rootvisualelement editorrootrendererutility partial class render method v.func v.memo`,element:()=>(0,I.jsx)($v,{})}]},{id:`concepts`,title:`Concepts & Environment`,pages:[{id:`concepts-and-environment`,title:`Concepts & Environment`,path:`/concepts`,keywords:[`concepts`,`environment`,`defines`,`trace`,`react differences`],searchContent:`concepts environment defines tracings symbols env_dev env_staging env_prod ruitk_trace_verbose ruitk_trace_basic runtime diagnostics intrinsic tags hooks markup reconciliation scheduling companion partial classes`,element:()=>(0,I.jsx)(ay,{})}]},{id:`differences`,title:`Different from React`,pages:[{id:`different-from-react`,title:`Different from React`,path:`/differences`,keywords:[`react`,`usestate`,`signals`,`differences`],searchContent:`different from react usestate setter value updater function fiber schedule asynchronously scheduler sliced render deferred passive effects concurrent jsx-like syntax visualelement interop unity controls styles events`,element:()=>(0,I.jsx)(sy,{})}]},{id:`tooling`,title:`Tooling`,pages:[{id:`router`,title:`Router`,path:`/tooling/router`,keywords:[`navigation`,`routes`],searchContent:`router routing route links routerhooks usenavigate usego useparams usequery uselocationinfo usenavigationstate useblocker declarative route composition navigation routernavlink`,element:()=>(0,I.jsx)(ty,{})},{id:`signals`,title:`Signals`,path:`/tooling/signals`,keywords:[`state`,`observable`],searchContent:`signals shared-state primitive signal registry usesignal dispatch signalfactory.get signalsruntime.ensureinitialized signals.get counter increment`,element:()=>(0,I.jsx)(ry,{})}]},{id:`components`,title:`Components`,pages:[{id:`component-bounds-field`,title:`BoundsField`,path:`/components/bounds-field`,keywords:[`bounds`,`field`,`BoundsField`],group:`advanced`,element:()=>(0,I.jsx)(py,{})},{id:`component-bounds-int-field`,title:`BoundsIntField`,path:`/components/bounds-int-field`,keywords:[`boundsint`,`field`,`BoundsIntField`],element:()=>(0,I.jsx)(hy,{})},{id:`component-box`,title:`Box`,path:`/components/box`,keywords:[`box`,`container`],group:`basic`,element:()=>(0,I.jsx)(_y,{})},{id:`component-button`,title:`Button`,path:`/components/button`,keywords:[`button`,`click`],group:`basic`,element:()=>(0,I.jsx)(yy,{})},{id:`component-color-field`,title:`ColorField`,path:`/components/color-field`,keywords:[`color`,`field`,`ColorField`],group:`advanced`,element:()=>(0,I.jsx)(xy,{})},{id:`component-double-field`,title:`DoubleField`,path:`/components/double-field`,keywords:[`double`,`field`,`DoubleField`],group:`advanced`,element:()=>(0,I.jsx)(Cy,{})},{id:`component-dropdown-field`,title:`DropdownField`,path:`/components/dropdown-field`,keywords:[`dropdown`,`field`,`choices`],group:`basic`,element:()=>(0,I.jsx)(Ty,{})},{id:`component-enum-field`,title:`EnumField`,path:`/components/enum-field`,keywords:[`enum`,`field`,`EnumField`],group:`basic`,element:()=>(0,I.jsx)(Dy,{})},{id:`component-enum-flags-field`,title:`EnumFlagsField`,path:`/components/enum-flags-field`,keywords:[`enum`,`flags`,`EnumFlagsField`],group:`advanced`,element:()=>(0,I.jsx)(ky,{})},{id:`component-float-field`,title:`FloatField`,path:`/components/float-field`,keywords:[`float`,`field`,`FloatField`],group:`basic`,element:()=>(0,I.jsx)(jy,{})},{id:`component-foldout`,title:`Foldout`,path:`/components/foldout`,keywords:[`foldout`,`toggle`,`collapsible`],group:`basic`,element:()=>(0,I.jsx)(Ny,{})},{id:`component-group-box`,title:`GroupBox`,path:`/components/group-box`,keywords:[`group`,`groupbox`],group:`basic`,element:()=>(0,I.jsx)(Fy,{})},{id:`component-hash128-field`,title:`Hash128Field`,path:`/components/hash128-field`,keywords:[`hash128`,`field`],group:`advanced`,element:()=>(0,I.jsx)(Ly,{})},{id:`component-help-box`,title:`HelpBox`,path:`/components/help-box`,keywords:[`helpbox`,`message`],group:`basic`,element:()=>(0,I.jsx)(zy,{})},{id:`component-imgui-container`,title:`IMGUIContainer`,path:`/components/imgui-container`,keywords:[`imgui`,`editor`],group:`advanced`,element:()=>(0,I.jsx)(Vy,{})},{id:`component-image`,title:`Image`,path:`/components/image`,keywords:[`image`,`texture`,`sprite`],group:`basic`,element:()=>(0,I.jsx)(Uy,{})},{id:`component-integer-field`,title:`IntegerField`,path:`/components/integer-field`,keywords:[`integer`,`field`,`int`],group:`basic`,element:()=>(0,I.jsx)(Gy,{})},{id:`component-label`,title:`Label`,path:`/components/label`,keywords:[`label`,`text`],group:`basic`,element:()=>(0,I.jsx)(qy,{})},{id:`component-long-field`,title:`LongField`,path:`/components/long-field`,keywords:[`long`,`field`,`LongField`],group:`advanced`,element:()=>(0,I.jsx)(Yy,{})},{id:`component-progress-bar`,title:`ProgressBar`,path:`/components/progress-bar`,keywords:[`progress`,`bar`],group:`basic`,element:()=>(0,I.jsx)(Zy,{})},{id:`component-list-view`,title:`ListView`,path:`/components/list-view`,keywords:[`list`,`ListView`],group:`basic`,element:()=>(0,I.jsx)($y,{})},{id:`component-minmax-slider`,title:`MinMaxSlider`,path:`/components/minmax-slider`,keywords:[`minmax`,`slider`],group:`advanced`,element:()=>(0,I.jsx)(tb,{})},{id:`component-object-field`,title:`ObjectField`,path:`/components/object-field`,keywords:[`object`,`field`],group:`advanced`,element:()=>(0,I.jsx)(rb,{})},{id:`component-radio-button`,title:`RadioButton`,path:`/components/radio-button`,keywords:[`radio`,`button`],group:`basic`,element:()=>(0,I.jsx)(ab,{})},{id:`component-radio-button-group`,title:`RadioButtonGroup`,path:`/components/radio-button-group`,keywords:[`radio`,`group`],group:`basic`,element:()=>(0,I.jsx)(sb,{})},{id:`component-rect-field`,title:`RectField`,path:`/components/rect-field`,keywords:[`rect`,`field`],group:`advanced`,element:()=>(0,I.jsx)(jb,{})},{id:`component-rect-int-field`,title:`RectIntField`,path:`/components/rect-int-field`,keywords:[`rectint`,`field`],group:`advanced`,element:()=>(0,I.jsx)(Nb,{})},{id:`component-repeat-button`,title:`RepeatButton`,path:`/components/repeat-button`,keywords:[`repeat`,`button`],group:`basic`,element:()=>(0,I.jsx)(lb,{})},{id:`component-scroll-view`,title:`ScrollView`,path:`/components/scroll-view`,keywords:[`scroll`,`view`],group:`basic`,element:()=>(0,I.jsx)(db,{})},{id:`component-slider`,title:`Slider`,path:`/components/slider`,keywords:[`slider`,`float`],group:`basic`,element:()=>(0,I.jsx)(pb,{})},{id:`component-slider-int`,title:`SliderInt`,path:`/components/slider-int`,keywords:[`slider`,`int`],group:`basic`,element:()=>(0,I.jsx)(hb,{})},{id:`component-toggle`,title:`Toggle`,path:`/components/toggle`,keywords:[`toggle`,`checkbox`],group:`basic`,element:()=>(0,I.jsx)(_b,{})},{id:`component-tree-view`,title:`TreeView`,path:`/components/tree-view`,keywords:[`tree`,`TreeView`],group:`basic`,element:()=>(0,I.jsx)(yb,{})},{id:`component-tab`,title:`Tab`,path:`/components/tab`,keywords:[`tab`],group:`basic`,element:()=>(0,I.jsx)(xb,{})},{id:`component-tab-view`,title:`TabView`,path:`/components/tab-view`,keywords:[`tab`,`TabView`],group:`basic`,element:()=>(0,I.jsx)(Cb,{})},{id:`component-toggle-button-group`,title:`ToggleButtonGroup`,path:`/components/toggle-button-group`,keywords:[`toggle`,`buttons`,`group`],group:`advanced`,element:()=>(0,I.jsx)(Tb,{})},{id:`component-text-field`,title:`TextField`,path:`/components/text-field`,keywords:[`text`,`field`],group:`basic`,element:()=>(0,I.jsx)(Db,{})},{id:`component-toolbar`,title:`Toolbar`,path:`/components/toolbar`,keywords:[`toolbar`,`editor`],group:`advanced`,element:()=>(0,I.jsx)(kb,{})},{id:`component-template-container`,title:`TemplateContainer`,path:`/components/template-container`,keywords:[`template`,`container`],group:`advanced`,element:()=>(0,I.jsx)(Yb,{})},{id:`component-visual-element`,title:`VisualElement`,path:`/components/visual-element`,keywords:[`visualelement`,`container`,`safe`],group:`basic`,element:()=>(0,I.jsx)(Zb,{})},{id:`component-visual-element-safe`,title:`VisualElementSafe`,path:`/components/visual-element-safe`,keywords:[`visualelementsafe`,`safe-area`,`container`],group:`basic`,element:()=>(0,I.jsx)($b,{})},{id:`component-unsigned-integer-field`,title:`UnsignedIntegerField`,path:`/components/unsigned-integer-field`,keywords:[`uint`,`field`],group:`advanced`,element:()=>(0,I.jsx)(Fb,{})},{id:`component-unsigned-long-field`,title:`UnsignedLongField`,path:`/components/unsigned-long-field`,keywords:[`ulong`,`field`],group:`advanced`,element:()=>(0,I.jsx)(Lb,{})},{id:`component-vector2-field`,title:`Vector2Field`,path:`/components/vector2-field`,keywords:[`vector2`,`field`],group:`advanced`,element:()=>(0,I.jsx)(zb,{})},{id:`component-vector2-int-field`,title:`Vector2IntField`,path:`/components/vector2-int-field`,keywords:[`vector2int`,`field`],group:`advanced`,element:()=>(0,I.jsx)(Vb,{})},{id:`component-vector3-field`,title:`Vector3Field`,path:`/components/vector3-field`,keywords:[`vector3`,`field`],group:`advanced`,element:()=>(0,I.jsx)(Ub,{})},{id:`component-vector3-int-field`,title:`Vector3IntField`,path:`/components/vector3-int-field`,keywords:[`vector3int`,`field`],group:`advanced`,element:()=>(0,I.jsx)(Gb,{})},{id:`component-vector4-field`,title:`Vector4Field`,path:`/components/vector4-field`,keywords:[`vector4`,`field`],group:`advanced`,element:()=>(0,I.jsx)(qb,{})},{id:`component-animate`,title:`Animate`,path:`/components/animate`,keywords:[`animate`,`animation`],group:`basic`,element:()=>(0,I.jsx)(tx,{})},{id:`component-error-boundary`,title:`ErrorBoundary`,path:`/components/error-boundary`,keywords:[`error`,`boundary`],group:`advanced`,element:()=>(0,I.jsx)(rx,{})},{id:`component-multi-column-list-view`,title:`MultiColumnListView`,path:`/components/multi-column-list-view`,keywords:[`list`,`multi`,`columns`],group:`basic`,element:()=>(0,I.jsx)(ax,{})},{id:`component-multi-column-tree-view`,title:`MultiColumnTreeView`,path:`/components/multi-column-tree-view`,keywords:[`tree`,`multi`,`columns`],group:`basic`,element:()=>(0,I.jsx)(sx,{})},{id:`component-scroller`,title:`Scroller`,path:`/components/scroller`,keywords:[`scroller`],group:`advanced`,element:()=>(0,I.jsx)(lx,{})},{id:`component-text-element`,title:`TextElement`,path:`/components/text-element`,keywords:[`text`,`TextElement`],group:`advanced`,element:()=>(0,I.jsx)(dx,{})},{id:`component-property-inspector`,title:`PropertyField & InspectorElement`,path:`/components/property-inspector`,keywords:[`propertyfield`,`inspectorelement`,`editor`],group:`advanced`,element:()=>(0,I.jsx)(px,{})},{id:`component-two-pane-split-view`,title:`TwoPaneSplitView`,path:`/components/two-pane-split-view`,keywords:[`split`,`editor`],group:`advanced`,element:()=>(0,I.jsx)(hx,{})}]},{id:`special-hooks`,title:`Special Hooks`,pages:[{id:`special-hooks-animation`,title:`Animation hooks`,path:`/special-hooks/animation`,keywords:[`hooks`,`animation`,`UseAnimate`,`UseTweenFloat`],searchContent:`animation hooks useanimate usetweenfloat animate tween float interpolation easing duration delay loop repeat playback`,element:()=>(0,I.jsx)(Cx,{})},{id:`special-hooks-router`,title:`Router hooks`,path:`/special-hooks/router`,keywords:[`hooks`,`router`,`RouterHooks`],searchContent:`router hooks routerhooks usenavigate useparams usequery uselocationinfo usenavigationstate useblocker usego usepath imperative navigation`,element:()=>(0,I.jsx)(Ex,{})},{id:`special-hooks-signals`,title:`Signal hooks`,path:`/special-hooks/signals`,keywords:[`hooks`,`signals`,`UseSignal`],searchContent:`signal hooks usesignal dispatch signalfactory signalfactory.get signals.get shared-state reactive`,element:()=>(0,I.jsx)(kx,{})},{id:`special-hooks-safe-area`,title:`Safe area hooks`,path:`/special-hooks/safe-area`,keywords:[`hooks`,`safe area`,`UseSafeArea`,`VisualElementSafe`],searchContent:`safe area hooks usesafearea visualelementsafe notch insets screen safe zone padding margins mobile`,element:()=>(0,I.jsx)(Mx,{})}]},{id:`api`,title:`API`,pages:[{id:`api-reference`,title:`API Reference`,path:`/api`,keywords:[`api`,`namespace`,`props`,`hooks`,`router`,`signals`],searchContent:`api reference namespace props hooks usestate useeffect usememo useref usecallback usecontext router signals runtime types v vnode virtualnode rootrenderer editorrootrendererutility`,element:()=>(0,I.jsx)(ly,{})}]},{id:`known-issues`,title:`Known Issues`,pages:[{id:`known-issues-page`,title:`Known Issues`,path:`/known-issues`,keywords:[`issues`,`limitations`,`known issues`],element:()=>(0,I.jsx)(_x,{})}]},{id:`roadmap`,title:`Roadmap`,pages:[{id:`roadmap-page`,title:`Roadmap`,path:`/roadmap`,keywords:[`roadmap`,`future`,`plans`],element:()=>(0,I.jsx)(yx,{})}]}];Nx.flatMap(e=>{if(e.id===`components`){let t=e.pages.filter(e=>e.group===`basic`),n=e.pages.filter(e=>e.group===`advanced`||!e.group);return[...t,...n]}return e.pages});const Px=()=>(0,I.jsxs)(W,{sx:cy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UITKX API Map`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`For UITKX users, the important API split is: author in markup, use hooks in setup code, and understand the runtime types only when you need to mount, integrate, or debug.`}),(0,I.jsxs)(W,{sx:cy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Authoring surface`}),(0,I.jsxs)(G,{sx:cy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`.uitkx`}),` function-style components are the primary source format.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`useState`}),`, `,(0,I.jsx)(`code`,{children:`useEffect`}),`, `,(0,I.jsx)(`code`,{children:`useMemo`}),`, and `,(0,I.jsx)(`code`,{children:`useSignal`}),` are the normal UITKX setup-code hooks, alongside router/context helpers.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Intrinsic tags map onto built-in ReactiveUITK/UI Toolkit elements.`})})]})]}),(0,I.jsxs)(W,{sx:cy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Runtime layer underneath`}),(0,I.jsxs)(G,{sx:cy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`V`}),` and `,(0,I.jsx)(`code`,{children:`VirtualNode`}),` still exist as the underlying runtime representation.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RootRenderer`}),` and `,(0,I.jsx)(`code`,{children:`EditorRootRendererUtility`}),` are still how UITKX output is mounted.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Props classes and typed styles still matter for low-level integration, custom emit targets, and debugging generated output.`})})]})]})]});var Fx=e=>e===`VisualElementSafe`?``:`${e}Props`,Ix=e=>{if(e.endsWith(`Field`))return`Use <${e}> in UITKX for a controlled UI Toolkit ${e} input. Keep the current value in useState(...) or a signal and feed user edits back through onChange.`;switch(e){case`Button`:case`RepeatButton`:return`Use <${e}> in UITKX for clickable actions. Pass text, event handlers, and styling directly on the tag.`;case`Toggle`:case`RadioButton`:return`Use <${e}> in UITKX for boolean-style selection controls. The usual pattern is value + onChange, backed by local state or a signal.`;case`DropdownField`:case`EnumField`:case`EnumFlagsField`:case`RadioButtonGroup`:case`ToggleButtonGroup`:return`Use <${e}> in UITKX when the user is choosing from a predefined set of options.`;case`Slider`:case`SliderInt`:case`Scroller`:case`MinMaxSlider`:return`Use <${e}> in UITKX for range-style numeric input.`;case`Label`:case`TextElement`:return`Use <${e}> in UITKX to render text content directly in markup.`;case`VisualElement`:case`VisualElementSafe`:case`Box`:case`GroupBox`:case`ScrollView`:case`TemplateContainer`:case`TwoPaneSplitView`:return`Use <${e}> in UITKX as a structural layout primitive. It composes naturally with child tags and style props.`;case`HelpBox`:case`Image`:case`ProgressBar`:return`Use <${e}> in UITKX as a presentational control inside your returned markup tree.`;case`ListView`:case`TreeView`:case`MultiColumnListView`:case`MultiColumnTreeView`:return`Use <${e}> in UITKX for data-driven collection UIs. These components usually combine declarative markup with row, cell, or binding delegates configured through props.`;case`Tab`:case`TabView`:case`Toolbar`:return`Use <${e}> in UITKX for higher-level navigation and editor-style composition.`;case`Animate`:case`ErrorBoundary`:case`IMGUIContainer`:case`PropertyInspector`:return`Use <${e}> in UITKX when you need this higher-level ReactiveUITK runtime feature directly in markup.`;default:return`Use <${e}> directly in UITKX markup. The runtime still exposes the underlying props type, but the normal authoring surface is the tag itself.`}},Lx=e=>{switch(e){case`VisualElementSafe`:return[`VisualElementSafe is the safe-area-aware variant of VisualElement.`,`Use it when your layout should automatically respect device insets.`];case`IMGUIContainer`:return[`IMGUIContainer remains callback-driven even in UITKX.`,`It is mainly useful for editor tooling or legacy IMGUI interop.`];case`PropertyInspector`:return[`PropertyInspector is especially useful in editor tooling and inspector-like UIs.`,`It is still backed by runtime props underneath, even when authored in UITKX.`];case`ListView`:case`TreeView`:case`MultiColumnListView`:case`MultiColumnTreeView`:return[`Collection components often rely on renderer delegates in addition to plain tag props.`,`The props section below matters more than usual for these components.`];default:return[`The props section below shows the underlying runtime API that UITKX lowers into.`]}},Rx=e=>{switch(e){case`Button`:return`component ButtonExample {
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
}`}};const zx=({title:e})=>{let t=Q(Fx(e)),n=Lx(e);return(0,I.jsxs)(W,{sx:vy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:e}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:Ix(e)}),(0,I.jsxs)(W,{sx:vy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`UITKX Example`}),(0,I.jsx)(Z,{language:`tsx`,code:Rx(e)})]}),(0,I.jsxs)(W,{sx:vy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Notes`}),(0,I.jsx)(G,{children:n.map(e=>(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:e})},e))})]}),t?(0,I.jsxs)(W,{sx:vy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Underlying Props`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`UITKX authors normally use the tag directly, but the runtime still exposes the underlying props contract shown below.`}),(0,I.jsx)(Z,{language:`tsx`,code:t})]}):null]})};var Bx=`component ButtonShowcase {
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
}`;const Vx=()=>(0,I.jsxs)(W,{sx:vy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Components in UITKX`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`In the UITKX track, components are authored as markup using intrinsic tags like`,` `,(0,I.jsx)(`code`,{children:`<VisualElement>`}),`, `,(0,I.jsx)(`code`,{children:`<Button>`}),`, `,(0,I.jsx)(`code`,{children:`<Text>`}),`, router tags, and your own custom components.`]}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`The practical rule is simple: use intrinsic tags for built-in elements, and use PascalCase names for your own components. If you wrap a native element, consumers should use your custom component name, not the native one.`}),(0,I.jsx)(Z,{language:`tsx`,code:Bx}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Authoring guidelines`}),(0,I.jsxs)(G,{children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Prefer direct tag props over hand-building props objects when authoring UITKX.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Keep setup code small and close to the returned markup tree.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Use custom component names whenever a native tag name would collide.`})})]})]}),Hx=()=>(0,I.jsxs)(W,{sx:Qv.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Companion Files`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The source generator produces a `,(0,I.jsx)(`strong`,{children:`complete C# class`}),` from every`,` `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file — namespace, partial class, `,(0,I.jsx)(`code`,{children:`Render()`}),` method, and everything else. You do `,(0,I.jsx)(`strong`,{children:`not`}),` need to create any `,(0,I.jsx)(`code`,{children:`.cs`}),` file for a component to work.`]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Companion files are `,(0,I.jsx)(`strong`,{children:`optional`}),` `,(0,I.jsx)(`code`,{children:`.cs`}),` files that live next to a`,` `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file. Use them when you want to share styles, type definitions, or utility functions with your component.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`The UITKX component`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`Here is a component that uses styles, types, and utility functions defined in companion files:`}),(0,I.jsx)(Z,{language:`tsx`,code:`@namespace MyGame.UI
@using UnityEngine
@using static ReactiveUITK.Props.Typed.StyleKeys

component PlayerCard(PlayerInfo player) {
  var healthColor = player.Health > player.MaxHealth / 2
    ? HealthGreen
    : DamageRed;

  return (
    <VisualElement>
      <Label text={player.Name} />
      <Label text={FormatHealth(player.Health, player.MaxHealth)}
             style={new Style { (Color, healthColor) }} />
      <Label text={RankLabel(player.Rank)} />
    </VisualElement>
  );
}`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Generated namespace & class name`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The source generator creates a C# class from the `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file. Two things determine its identity:`]}),(0,I.jsxs)(G,{children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`strong`,{children:`Namespace`}),` — comes from the `,(0,I.jsx)(`code`,{children:`@namespace`}),` directive at the top of the `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file. If omitted, the generator looks for a companion`,` `,(0,I.jsx)(`code`,{children:`.cs`}),` file with the same name (e.g. `,(0,I.jsx)(`code`,{children:`PlayerCard.cs`}),` next to`,` `,(0,I.jsx)(`code`,{children:`PlayerCard.uitkx`}),`) and uses its namespace declaration. If neither exists, it falls back to `,(0,I.jsx)(`code`,{children:`ReactiveUITK.FunctionStyle`}),`. Declaring`,` `,(0,I.jsx)(`code`,{children:`@namespace`}),` explicitly is recommended.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`strong`,{children:`Class name`}),` — comes from the `,(0,I.jsx)(`code`,{children:`component`}),` name (the identifier after the `,(0,I.jsx)(`code`,{children:`component`}),` keyword).`]})})})]}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`For the example above, the generator produces:`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Auto-generated by the source generator (simplified):
namespace MyGame.UI                    // ← from @namespace
{
    public partial class PlayerCard    // ← from component name
    {
        public static VisualElement Render(PlayerInfo player) { ... }
    }
}`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Companion `,(0,I.jsx)(`code`,{children:`.cs`}),` files that need to reference or extend the generated class must use the `,(0,I.jsx)(`strong`,{children:`same namespace`}),` and `,(0,I.jsx)(`strong`,{children:`same class name`}),`.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Directory layout`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Place companion files in the `,(0,I.jsx)(`strong`,{children:`same directory`}),` as the `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file:`]}),(0,I.jsx)(Z,{language:`text`,code:`Assets/
  UI/
    PlayerCard/
      PlayerCard.uitkx          ← component template
      PlayerCard.styles.cs      ← optional: style constants & helpers
      PlayerCard.types.cs       ← optional: enums, structs, DTOs
      PlayerCard.utils.cs       ← optional: pure helper functions`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`These names are conventions, not enforced rules. Any `,(0,I.jsx)(`code`,{children:`.cs`}),` file (except`,` `,(0,I.jsx)(`code`,{children:`.g.cs`}),`) in the same directory is automatically picked up during compilation.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Naming conventions`}),(0,I.jsx)(L_,{component:nd,variant:`outlined`,sx:{mb:2},children:(0,I.jsxs)(S_,{size:`small`,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`File`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Purpose`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Required?`})})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`MyComponent.styles.cs`})}),(0,I.jsx)(J,{children:`Style constants, helper methods, colours, sizes`}),(0,I.jsx)(J,{children:`No`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`MyComponent.types.cs`})}),(0,I.jsx)(J,{children:`Enums, structs, DTOs used by the component`}),(0,I.jsx)(J,{children:`No`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`MyComponent.utils.cs`})}),(0,I.jsx)(J,{children:`Pure helper / formatting functions`}),(0,I.jsx)(J,{children:`No`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`MyComponent.extra.cs`})}),(0,I.jsx)(J,{children:`Partial class extension (same namespace + class name)`}),(0,I.jsx)(J,{children:`No`})]})]})]})}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Example: style helpers`}),(0,I.jsx)(Z,{language:`tsx`,code:`// PlayerCard.styles.cs
using UnityEngine;

namespace MyGame.UI
{
    public partial class PlayerCard
    {
        public static readonly Color HealthGreen = new(0.2f, 0.8f, 0.3f);
        public static readonly Color DamageRed   = new(0.9f, 0.2f, 0.2f);
        public static readonly float AvatarSize  = 64f;
    }
}`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Example: type definitions`}),(0,I.jsx)(Z,{language:`tsx`,code:`// PlayerCard.types.cs
namespace MyGame.UI
{
    public partial class PlayerCard
    {
        public enum PlayerRank { Bronze, Silver, Gold, Diamond }

        public readonly struct PlayerInfo
        {
            public string Name { get; init; }
            public int Health { get; init; }
            public int MaxHealth { get; init; }
            public PlayerRank Rank { get; init; }
        }
    }
}`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Example: utility functions`}),(0,I.jsx)(Z,{language:`tsx`,code:`// PlayerCard.utils.cs
namespace MyGame.UI
{
    public partial class PlayerCard
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
}`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Standalone classes`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`Not everything has to go in the partial class. Standalone classes under the same namespace are useful for types shared across multiple components:`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Alternatively, standalone classes under the same namespace also work.
// This is useful for types shared across multiple components.
namespace MyGame.UI
{
    public static class SharedColors
    {
        public static readonly Color Gold = new(1f, 0.84f, 0f);
    }
}`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`HMR support`}),(0,I.jsxs)(G,{children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Editing a companion .cs file automatically triggers HMR for the associated .uitkx.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Creating a new companion file is detected instantly — the file watcher picks up new files.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`All .cs files in the directory (except .g.cs) are included in compilation.`})})]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`When not to use companion files`}),(0,I.jsxs)(G,{children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Simple components — if a component has no shared styles or types, it doesn't need any companion files.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Small helpers — for code that only the component uses, prefer`,` `,(0,I.jsx)(`code`,{children:`@code`}),` blocks inside the `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file itself.`]})})})]})]}),Ux=()=>(0,I.jsxs)(W,{sx:iy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Concepts & Environment`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`UITKX is the authoring layer. ReactiveUITK is the runtime layer underneath it. In practice, that means you think in terms of components, intrinsic tags, hooks, and markup structure, while the runtime handles reconciliation, scheduling, and adapter application.`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`The key mental model is: write UI as UITKX, keep your setup code local to the component, and let the generator and runtime bridge that into Unity UI Toolkit.`}),(0,I.jsxs)(W,{sx:iy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Core authoring rules`}),(0,I.jsxs)(G,{sx:iy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Intrinsic UITKX/native tag names are reserved; custom components should use distinct names.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Function-style components are the default form: setup code first, then a single returned markup tree.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`State setters are called directly like functions, for example setCount(count + 1).`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Companion .cs files are optional — use them to share styles, types, or utilities. The source generator produces the full class from the .uitkx file alone.`})})]})]}),(0,I.jsxs)(W,{sx:iy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Environment defines`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`Compile-time environment and tracing symbols still work the same way in UITKX projects, because the generated output runs on the same ReactiveUITK runtime.`}),(0,I.jsxs)(G,{sx:iy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`ENV_DEV`}),`, `,(0,I.jsx)(`code`,{children:`ENV_STAGING`}),`, `,(0,I.jsx)(`code`,{children:`ENV_PROD`}),` control environment labeling.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RUITK_TRACE_VERBOSE`}),` and `,(0,I.jsx)(`code`,{children:`RUITK_TRACE_BASIC`}),` control runtime diagnostics.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Editor-only diagnostic helpers still compile behind the same development symbols.`})})]})]})]});var Wx={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},table:{"& th":{fontWeight:600},"& td, & th":{px:1.5,py:.75,fontSize:`0.875rem`},"& code":{fontSize:`0.8125rem`,backgroundColor:`rgba(255,255,255,0.06)`,px:.5,borderRadius:.5}}},Gx=`{
  // Path to a custom UitkxLanguageServer.dll (leave empty for bundled server)
  "uitkx.server.path": "",

  // Path to the dotnet executable used to run the LSP server
  "uitkx.server.dotnetPath": "dotnet",

  // Trace LSP communication (off | messages | verbose)
  "uitkx.trace.server": "off"
}`;const Kx=()=>(0,I.jsxs)(W,{sx:Wx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Configuration Reference`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`All configuration options for the UITKX editor extensions and formatter.`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`VS Code Extension Settings`}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Setting`}),(0,I.jsx)(J,{children:`Type`}),(0,I.jsx)(J,{children:`Default`}),(0,I.jsx)(J,{children:`Description`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`uitkx.server.path`})}),(0,I.jsx)(J,{children:`string`}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`""`})}),(0,I.jsxs)(J,{children:[`Absolute path to a custom `,(0,I.jsx)(`code`,{children:`UitkxLanguageServer.dll`}),`. Leave empty to use the server bundled with the extension.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`uitkx.server.dotnetPath`})}),(0,I.jsx)(J,{children:`string`}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`"dotnet"`})}),(0,I.jsxs)(J,{children:[`Path to the `,(0,I.jsx)(`code`,{children:`dotnet`}),` executable. Override this if your .NET 8+ SDK is installed in a non-standard location.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`uitkx.trace.server`})}),(0,I.jsx)(J,{children:`enum`}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`"off"`})}),(0,I.jsxs)(J,{children:[`Controls LSP trace output. Set to `,(0,I.jsx)(`code`,{children:`"messages"`}),` or`,` `,(0,I.jsx)(`code`,{children:`"verbose"`}),` to see JSON-RPC traffic in the Output panel (select "UITKX Language Server" channel).`]})]})]})]})}),(0,I.jsx)(Z,{language:`json`,code:Gx}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Editor Defaults`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`The extension automatically configures these editor settings for`,` `,(0,I.jsx)(`code`,{children:`.uitkx`}),` files:`]}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Setting`}),(0,I.jsx)(J,{children:`Value`}),(0,I.jsx)(J,{children:`Reason`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`editor.defaultFormatter`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`ReactiveUITK.uitkx`})}),(0,I.jsxs)(J,{children:[`Uses the UITKX formatter for `,(0,I.jsx)(`code`,{children:`.uitkx`}),` files`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`editor.formatOnSave`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`true`})}),(0,I.jsx)(J,{children:`Auto-format on save (recommended)`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`editor.tabSize`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`2`})}),(0,I.jsx)(J,{children:`UITKX uses 2-space indentation`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`editor.insertSpaces`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`true`})}),(0,I.jsx)(J,{children:`Spaces, not tabs`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`editor.bracketPairColorization`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`false`})}),(0,I.jsx)(J,{children:`Disabled — conflicting colors with UITKX semantic tokens`})]})]})]})})]});var qx=`// Generated files are at:
// Library/PackageCache/com.reactiveuitk/Analyzers~
//   or under your project's SourceGenerator~ output folder.
// Look for files ending in .uitkx.g.cs`,Jx=`{
  "uitkx.trace.server": "verbose"
}`;const Yx=()=>(0,I.jsxs)(W,{sx:Wx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Debugging Guide`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`How to diagnose and fix common issues when working with UITKX.`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Inspecting Generated Code`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Every `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file produces a corresponding `,(0,I.jsx)(`code`,{children:`.uitkx.g.cs`}),` file via the Roslyn source generator. To inspect it:`]}),(0,I.jsxs)(U,{component:`ol`,variant:`body2`,children:[(0,I.jsxs)(`li`,{children:[`In VS Code, go to `,(0,I.jsx)(`strong`,{children:`Definition`}),` (F12) on any generated symbol.`]}),(0,I.jsxs)(`li`,{children:[`Or navigate to the `,(0,I.jsx)(`code`,{children:`GeneratedFiles`}),` folder under your project's Analyzers output directory.`]}),(0,I.jsxs)(`li`,{children:[`The generated file contains `,(0,I.jsx)(`code`,{children:`#line`}),` directives that map errors back to the original `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file and line number.`]})]}),(0,I.jsx)(Z,{language:`csharp`,code:qx}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Understanding #line Directives`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`When the C# compiler reports an error in generated code, the`,` `,(0,I.jsx)(`code`,{children:`#line`}),` directive maps it back to your `,(0,I.jsx)(`code`,{children:`.uitkx`}),` `,`source. For example:`]}),(0,I.jsxs)(U,{variant:`body2`,component:`ul`,children:[(0,I.jsxs)(`li`,{children:[(0,I.jsx)(`code`,{children:`#line 42 "MyComponent.uitkx"`}),` means the C# code that follows was generated from line 42 of your UITKX file.`]}),(0,I.jsx)(`li`,{children:`Clicking on the error in VS Code or Visual Studio will jump directly to the UITKX source line.`})]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`LSP Server Logs`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`To see detailed LSP communication, set the trace level in your VS Code settings:`}),(0,I.jsx)(Z,{language:`json`,code:Jx}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Then open the `,(0,I.jsx)(`strong`,{children:`Output`}),` panel (Ctrl+Shift+U) and select the`,` `,(0,I.jsx)(`strong`,{children:`UITKX Language Server`}),` channel. This shows all JSON-RPC requests and responses, which is useful for diagnosing:`]}),(0,I.jsxs)(U,{component:`ul`,variant:`body2`,children:[(0,I.jsx)(`li`,{children:`Missing completions — check if the completion request/response is present`}),(0,I.jsxs)(`li`,{children:[`Stale diagnostics — look for `,(0,I.jsx)(`code`,{children:`textDocument/publishDiagnostics`}),` messages`]}),(0,I.jsx)(`li`,{children:`Server crashes — look for error messages in the trace output`})]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Formatter Issues`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`If formatting produces unexpected results:`}),(0,I.jsxs)(U,{component:`ol`,variant:`body2`,children:[(0,I.jsxs)(`li`,{children:[(0,I.jsx)(`strong`,{children:`Check for syntax errors first`}),` — the formatter requires valid UITKX syntax. Fix any red squiggles before formatting.`]}),(0,I.jsxs)(`li`,{children:[(0,I.jsx)(`strong`,{children:`Ensure format-on-save is using the UITKX formatter`}),` — check that `,(0,I.jsx)(`code`,{children:`editor.defaultFormatter`}),` is set to`,` `,(0,I.jsx)(`code`,{children:`"ReactiveUITK.uitkx"`}),` for `,(0,I.jsx)(`code`,{children:`[uitkx]`}),` files.`]}),(0,I.jsxs)(`li`,{children:[(0,I.jsx)(`strong`,{children:`Try formatting manually`}),` — press Shift+Alt+F to rule out format-on-save timing issues.`]})]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Reporting Bugs`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`When reporting an issue, include:`}),(0,I.jsxs)(U,{component:`ol`,variant:`body2`,children:[(0,I.jsxs)(`li`,{children:[`The minimal `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file that reproduces the problem.`]}),(0,I.jsx)(`li`,{children:`The exact error message or diagnostic code (if any).`}),(0,I.jsx)(`li`,{children:`Your editor (VS Code / Visual Studio / Rider) and extension version.`}),(0,I.jsx)(`li`,{children:`LSP trace output if relevant (see above).`})]})]}),Xx=()=>(0,I.jsxs)(W,{sx:Wx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Diagnostics Reference`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`Every diagnostic code emitted by the UITKX source generator and language server, with severity, meaning, and how to fix it.`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Source Generator Diagnostics`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Emitted at compile time by the Roslyn source generator when processing`,(0,I.jsx)(`code`,{children:`.uitkx`}),` files.`]}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Code`}),(0,I.jsx)(J,{children:`Severity`}),(0,I.jsx)(J,{children:`Title`}),(0,I.jsx)(J,{children:`How to fix`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0001`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`Unknown built-in element`}),(0,I.jsxs)(J,{children:[`Check the tag name — built-in elements use PascalCase (e.g. `,(0,I.jsx)(`code`,{children:`<Button>`}),`, `,(0,I.jsx)(`code`,{children:`<Label>`}),`).`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0002`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`Unknown attribute on element`}),(0,I.jsx)(J,{children:`Verify the attribute name matches a property on the element's props type.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0005`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Missing required directive`}),(0,I.jsxs)(J,{children:[`Add the missing `,(0,I.jsx)(`code`,{children:`@namespace`}),` or `,(0,I.jsx)(`code`,{children:`@component`}),` directive.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0006`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`@component name mismatch`}),(0,I.jsxs)(J,{children:[`Rename `,(0,I.jsx)(`code`,{children:`@component`}),` to match the file name, or rename the file.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0008`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`Unknown function component`}),(0,I.jsxs)(J,{children:[`Ensure the component type exists and has a public static `,(0,I.jsx)(`code`,{children:`Render`}),` method.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0009`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`@foreach child missing key`}),(0,I.jsxs)(J,{children:[`Add a `,(0,I.jsx)(`code`,{children:`key`}),` attribute with a stable unique identifier from the item.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0010`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`Duplicate sibling key`}),(0,I.jsxs)(J,{children:[`Ensure each sibling element has a unique `,(0,I.jsx)(`code`,{children:`key`}),` value.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0012`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Directive order error`}),(0,I.jsxs)(J,{children:[`Move `,(0,I.jsx)(`code`,{children:`@namespace`}),` above `,(0,I.jsx)(`code`,{children:`@component`}),`.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0013`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Hook in conditional`}),(0,I.jsxs)(J,{children:[`Move the hook call to the component top level, outside any `,(0,I.jsx)(`code`,{children:`@if`}),` branch.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0014`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Hook in loop`}),(0,I.jsxs)(J,{children:[`Move the hook call to the component top level, outside any `,(0,I.jsx)(`code`,{children:`@foreach`}),` loop.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0015`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Hook in switch case`}),(0,I.jsxs)(J,{children:[`Move the hook call to the component top level, outside the `,(0,I.jsx)(`code`,{children:`@switch`}),`.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0016`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Hook in event handler`}),(0,I.jsx)(J,{children:`Move the hook call to the component top level — hooks cannot be called inside attribute expressions.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0017`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Multiple root elements`}),(0,I.jsxs)(J,{children:[`Wrap all root elements in a single container element (e.g. `,(0,I.jsx)(`code`,{children:`<VisualElement>`}),`).`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0018`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`UseEffect missing dependency array`}),(0,I.jsxs)(J,{children:[`Pass an explicit dependency array as the second argument, or `,(0,I.jsx)(`code`,{children:`Array.Empty<object>()`}),` for run-once.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0019`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`Loop variable used as key`}),(0,I.jsx)(J,{children:`Use a stable unique identifier from the item instead of the loop index.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0020`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`ref on component without Ref<T> param`}),(0,I.jsxs)(J,{children:[`Add a `,(0,I.jsx)(`code`,{children:`Ref<T>?`}),` parameter to the component, or remove the `,(0,I.jsx)(`code`,{children:`ref`}),` attribute.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0021`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`ref ambiguous — multiple Ref<T> params`}),(0,I.jsxs)(J,{children:[`Use an explicit prop name (e.g. `,(0,I.jsxs)(`code`,{children:[`inputRef=`,`{x}`]}),`) instead of `,(0,I.jsx)(`code`,{children:`ref`}),`.`]})]})]})]})}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Structural Diagnostics (Language Server)`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`Emitted in real time by the language server as you type. These appear as squiggly underlines in your editor.`}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Code`}),(0,I.jsx)(J,{children:`Severity`}),(0,I.jsx)(J,{children:`Message`}),(0,I.jsx)(J,{children:`How to fix`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0101`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsxs)(J,{children:[`Missing required `,(0,I.jsx)(`code`,{children:`@namespace`}),` directive`]}),(0,I.jsxs)(J,{children:[`Add `,(0,I.jsx)(`code`,{children:`@namespace Your.Namespace`}),` at the top of the file.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0102`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsxs)(J,{children:[`Missing required `,(0,I.jsx)(`code`,{children:`@component`}),` directive`]}),(0,I.jsxs)(J,{children:[`Add `,(0,I.jsx)(`code`,{children:`@component YourComponentName`}),` or use function-style syntax.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0103`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`@component name does not match filename`}),(0,I.jsxs)(J,{children:[`Rename `,(0,I.jsx)(`code`,{children:`@component`}),` to match the file name.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0104`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Duplicate sibling key`}),(0,I.jsxs)(J,{children:[`Ensure each sibling has a unique `,(0,I.jsx)(`code`,{children:`key`}),`.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0105`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Unknown element — no component found`}),(0,I.jsx)(J,{children:`Check the tag name or add the missing component to your project.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0106`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Element inside @foreach missing key`}),(0,I.jsxs)(J,{children:[`Add a `,(0,I.jsx)(`code`,{children:`key`}),` attribute for reconciliation.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0107`})}),(0,I.jsx)(J,{children:`Hint`}),(0,I.jsx)(J,{children:`Unreachable code after return / @break / @continue`}),(0,I.jsx)(J,{children:`Remove the unreachable code, or restructure control flow.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0108`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Multiple root elements`}),(0,I.jsx)(J,{children:`Wrap all root elements in a single container.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0109`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Unknown attribute on element`}),(0,I.jsx)(J,{children:`Check the attribute name against the element's props type.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0111`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Unused component parameter`}),(0,I.jsx)(J,{children:`Remove the unused parameter or use it in the component body.`})]})]})]})}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Parser Diagnostics`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`Emitted when the parser encounters malformed syntax.`}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Code`}),(0,I.jsx)(J,{children:`Severity`}),(0,I.jsx)(J,{children:`Title`}),(0,I.jsx)(J,{children:`How to fix`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0300`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Unexpected token`}),(0,I.jsx)(J,{children:`Check for typos or misplaced syntax near the reported line.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0301`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Unclosed tag`}),(0,I.jsxs)(J,{children:[`Add a matching closing tag or use self-closing syntax (`,(0,I.jsx)(`code`,{children:`/>`}),`).`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0302`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Mismatched closing tag`}),(0,I.jsx)(J,{children:`Ensure the closing tag matches the opening tag name.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0303`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Unexpected end of file`}),(0,I.jsx)(J,{children:`Close any open tags, braces, or expressions.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0304`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`Unclosed expression or block`}),(0,I.jsx)(J,{children:`Close the unclosed brace or parenthesis.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0305`})}),(0,I.jsx)(J,{children:`Warning`}),(0,I.jsx)(J,{children:`Unknown markup directive`}),(0,I.jsxs)(J,{children:[`Valid directives: `,(0,I.jsx)(`code`,{children:`@if`}),`, `,(0,I.jsx)(`code`,{children:`@else`}),`, `,(0,I.jsx)(`code`,{children:`@for`}),`, `,(0,I.jsx)(`code`,{children:`@foreach`}),`, `,(0,I.jsx)(`code`,{children:`@while`}),`, `,(0,I.jsx)(`code`,{children:`@switch`}),`, `,(0,I.jsx)(`code`,{children:`@case`}),`, `,(0,I.jsx)(`code`,{children:`@default`}),`, `,(0,I.jsx)(`code`,{children:`@break`}),`, `,(0,I.jsx)(`code`,{children:`@continue`}),`, `,(0,I.jsx)(`code`,{children:`@code`}),`.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`UITKX0306`})}),(0,I.jsx)(J,{children:`Error`}),(0,I.jsx)(J,{children:`@(expr) in setup code`}),(0,I.jsxs)(J,{children:[`Inline expressions `,(0,I.jsx)(`code`,{children:`@(...)`}),` are only valid inside markup, not in `,(0,I.jsx)(`code`,{children:`@code`}),` blocks.`]})]})]})]})})]}),Zx=()=>(0,I.jsxs)(W,{sx:oy.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Different from React`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`UITKX borrows React’s component-and-hooks mental model, but it runs on Unity UI Toolkit and a C# runtime. The biggest difference is that your authored code is markup-first, while the underlying runtime is still constrained by Unity’s VisualElement system, scheduling model, and C# semantics.`}),(0,I.jsxs)(W,{sx:oy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`State updates`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`useState`}),` behaves like React’s `,(0,I.jsx)(`code`,{children:`useState`}),`. You call the setter directly with either a value or an updater function, and UITKX lowers that into the runtime hook implementation for you.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`component StateCounterExample {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text={$"Count: {count}"} />
      <Button text="Increment" onClick={_ => setCount(previous => previous + 1)} />
      <Button text="Reset" onClick={_ => setCount(0)} />
    </VisualElement>
  );
}`})]}),(0,I.jsxs)(W,{sx:oy.section,children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Rendering model`}),(0,I.jsxs)(G,{sx:oy.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`ReactiveUITK’s fiber can schedule work asynchronously when a scheduler is present, including sliced render work and deferred passive effects.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`That does not mean UITKX promises a one-to-one clone of React’s full concurrent feature surface; the scheduler still operates inside Unity’s runtime constraints.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`The authored syntax is JSX-like, but it lowers into ReactiveUITK’s own runtime representation instead of a browser DOM model.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Interop with Unity controls, styles, and events is a first-class constraint, so some APIs deliberately differ from browser React conventions.`})})]})]})]}),Qx=()=>(0,I.jsxs)(W,{sx:Qv.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UITKX Getting Started`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`UITKX is the primary authoring model for ReactiveUIToolKit. You write function-style`,` `,(0,I.jsx)(`code`,{children:`.uitkx`}),` components and the source generator produces a complete C# class automatically — no boilerplate needed.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Install via Unity Package Manager`}),(0,I.jsxs)(G,{children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Open Package Manager in Unity.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Add package from Git URL:`})})]}),(0,I.jsx)(Z,{language:`tsx`,code:`https://github.com/yanivkalfa/ReactiveUIToolKit.git#dist`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`1. Create a UITKX component`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`A function-style UITKX component contains setup code at the top and returns markup. This is the default shape new users should learn.`}),(0,I.jsx)(Z,{language:`tsx`,code:`@namespace MyGame.UI

component HelloWorld {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text="Hello ReactiveUITK" />
      <Text text={$"Count: {count}"} />
      <Button text="Increment" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`On the next Unity compile the source generator emits a complete C# class (`,(0,I.jsx)(`code`,{children:`HelloWorld.uitkx.g.cs`}),`) with `,(0,I.jsx)(`code`,{children:`namespace`}),`,`,` `,(0,I.jsx)(`code`,{children:`public partial class`}),`, and a full `,(0,I.jsx)(`code`,{children:`Render()`}),` method. You don't need to create any companion file for this to work.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`2. Mount it`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Runtime mounting uses `,(0,I.jsx)(`code`,{children:`RootRenderer`}),` and `,(0,I.jsx)(`code`,{children:`V.Func(...)`}),`, but the authored UI stays in UITKX.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`using UnityEngine;
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
}`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Companion files (optional)`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The generator produces everything needed, but you can optionally add `,(0,I.jsx)(`code`,{children:`.cs`}),` files next to your `,(0,I.jsx)(`code`,{children:`.uitkx`}),` to share styles, types, or utilities across components. See the `,(0,I.jsx)(`strong`,{children:`Companion Files`}),` page for naming conventions and examples.`]})]});var $x=`component CounterCard {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text={$"Count: {count}"} />
      <Button text="+" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`;const eS=()=>(0,I.jsxs)(W,{sx:$_.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ReactiveUIToolKit`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit is a React-like UI Toolkit runtime for Unity, and UITKX is its primary authoring language. You write function-style components in `,(0,I.jsx)(`code`,{children:`.uitkx`}),`, use hooks for state and effects, and let the toolkit reconcile the resulting tree onto Unity`,(0,I.jsx)(`code`,{children:`VisualElement`}),`s.`]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The authored experience should feel markup-first. The underlying runtime is still C#, but the normal way to build UI is UITKX, not hand-built `,(0,I.jsx)(`code`,{children:`V.*`}),` trees.`]}),(0,I.jsx)(Z,{language:`tsx`,code:$x}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Highlights`}),(0,I.jsxs)(G,{children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Function-style UITKX components with hooks and typed props`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Reactive diffing and batched updates on top of UI Toolkit`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Router and Signals utilities that work naturally inside UITKX`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Generated C# output for production builds with no runtime codegen`})})]})]});var tS=`@namespace My.Game.UI
@using System.Collections.Generic
@component MyButton
@props MyButtonProps
@key "root-key"
@inject ILogger logger`,nS=`@using UnityEngine

component Counter(string label = "Count") {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Label text={$"{label}: {count}"} />
      <Button text="+" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`,rS=`<VisualElement>
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
</VisualElement>`,iS=`<Label text={$"Count: {count}"} />
<Button onClick={_ => setCount(count + 1)} />
<VisualElement>
  @(MyCustomComponent)
  {/* This is a JSX comment */}
</VisualElement>`;const aS=()=>(0,I.jsxs)(W,{sx:Wx.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UITKX Language Reference`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`Complete reference for the UITKX markup language — directives, syntax, control flow, and expressions.`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Header Directives`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Header directives appear at the top of a `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file, before any markup. They configure the generated C# class.`]}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Directive`}),(0,I.jsx)(J,{children:`Syntax`}),(0,I.jsx)(J,{children:`Description`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@namespace`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@namespace My.Game.UI`})}),(0,I.jsx)(J,{children:`C# namespace for the generated class`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@component`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@component MyButton`})}),(0,I.jsx)(J,{children:`Component class name (must match filename)`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@using`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@using System.Collections.Generic`})}),(0,I.jsx)(J,{children:`Adds a using directive to the generated file`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@props`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@props MyButtonProps`})}),(0,I.jsx)(J,{children:`Props type consumed by the component`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@key`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@key "root-key"`})}),(0,I.jsx)(J,{children:`Static key on the root element`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@inject`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@inject ILogger logger`})}),(0,I.jsx)(J,{children:`Dependency-injected field`})]})]})]})}),(0,I.jsx)(Z,{language:`tsx`,code:tS}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Function-Style Components`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Function-style components use a `,(0,I.jsxs)(`code`,{children:[`component Name `,`{ ... }`]}),` `,`syntax with optional typed parameters. They replace the directive-header form for most use cases.`]}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Feature`}),(0,I.jsx)(J,{children:`Syntax`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Declaration`}),(0,I.jsx)(J,{children:(0,I.jsxs)(`code`,{children:[`component Name `,`{ ... }`]})})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`With parameters`}),(0,I.jsx)(J,{children:(0,I.jsxs)(`code`,{children:[`component Name(string text = "default") `,`{ ... }`]})})]}),(0,I.jsxs)(Y,{children:[(0,I.jsxs)(J,{children:[`Preamble `,(0,I.jsx)(`code`,{children:`@using`})]}),(0,I.jsxs)(J,{children:[`Before the `,(0,I.jsx)(`code`,{children:`component`}),` keyword`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsxs)(J,{children:[`Preamble `,(0,I.jsx)(`code`,{children:`@namespace`})]}),(0,I.jsx)(J,{children:`Optional explicit namespace override`})]})]})]})}),(0,I.jsx)(Z,{language:`tsx`,code:nS}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Markup Control Flow`}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Directive`}),(0,I.jsx)(J,{children:`Syntax`}),(0,I.jsx)(J,{children:`Notes`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@if / @else if / @else`})}),(0,I.jsx)(J,{children:(0,I.jsxs)(`code`,{children:[`@if (cond) `,`{ ... }`,` @else `,`{ ... }`]})}),(0,I.jsx)(J,{children:`Conditional rendering`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@foreach`})}),(0,I.jsx)(J,{children:(0,I.jsxs)(`code`,{children:[`@foreach (var item in list) `,`{ ... }`]})}),(0,I.jsxs)(J,{children:[`Loop — direct children must have `,(0,I.jsx)(`code`,{children:`key`})]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@for`})}),(0,I.jsx)(J,{children:(0,I.jsxs)(`code`,{children:[`@for (int i = 0; i < n; i++) `,`{ ... }`]})}),(0,I.jsx)(J,{children:`C-style for loop`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@while`})}),(0,I.jsx)(J,{children:(0,I.jsxs)(`code`,{children:[`@while (cond) `,`{ ... }`]})}),(0,I.jsx)(J,{children:`While loop`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@switch / @case / @default`})}),(0,I.jsx)(J,{children:(0,I.jsxs)(`code`,{children:[`@switch (val) `,`{ @case "a": ... @default: ... }`]})}),(0,I.jsx)(J,{children:`Switch expression`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@break`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@break;`})}),(0,I.jsxs)(J,{children:[`Exit a `,(0,I.jsx)(`code`,{children:`@for`}),` or `,(0,I.jsx)(`code`,{children:`@while`}),` loop`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@continue`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@continue;`})}),(0,I.jsx)(J,{children:`Skip to the next iteration`})]})]})]})}),(0,I.jsx)(Z,{language:`tsx`,code:rS}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Expressions & Values`}),(0,I.jsx)(L_,{children:(0,I.jsxs)(S_,{size:`small`,sx:Wx.table,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Syntax`}),(0,I.jsx)(J,{children:`Example`}),(0,I.jsx)(J,{children:`Description`})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@(expr)`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`@(MyCustomComponent)`})}),(0,I.jsx)(J,{children:`Render a component or expression inline in markup children`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`{expr}`})}),(0,I.jsx)(J,{children:(0,I.jsxs)(`code`,{children:[`text=`,`{$"Count: {count}"}`]})}),(0,I.jsx)(J,{children:`C# expression as attribute value`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`"literal"`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`text="hello"`})}),(0,I.jsx)(J,{children:`Plain string attribute`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`{/* comment */}`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`{/* TODO */}`})}),(0,I.jsx)(J,{children:`JSX-style block comment`})]})]})]})}),(0,I.jsx)(Z,{language:`tsx`,code:iS}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:Wx.section,children:`Rules & Gotchas`}),(0,I.jsxs)(U,{component:`ul`,variant:`body2`,children:[(0,I.jsxs)(`li`,{children:[(0,I.jsx)(`code`,{children:`@namespace`}),` must appear before `,(0,I.jsx)(`code`,{children:`@component`}),` in directive-header form.`]}),(0,I.jsxs)(`li`,{children:[`Hook calls must be unconditional at component top level — not inside `,(0,I.jsx)(`code`,{children:`@if`}),`, `,(0,I.jsx)(`code`,{children:`@foreach`}),`, etc.`]}),(0,I.jsxs)(`li`,{children:[(0,I.jsx)(`code`,{children:`@break`}),` / `,(0,I.jsx)(`code`,{children:`@continue`}),` are only valid inside `,(0,I.jsx)(`code`,{children:`@for`}),` and `,(0,I.jsx)(`code`,{children:`@while`}),`.`]}),(0,I.jsxs)(`li`,{children:[`Direct children of `,(0,I.jsx)(`code`,{children:`@foreach`}),` need a `,(0,I.jsx)(`code`,{children:`key`}),` attribute for stable reconciliation.`]}),(0,I.jsx)(`li`,{children:`Components must have a single root element.`}),(0,I.jsxs)(`li`,{children:[`Component names must match the filename (e.g. `,(0,I.jsx)(`code`,{children:`MyButton.uitkx`}),` defines `,(0,I.jsx)(`code`,{children:`component MyButton`}),`).`]})]})]}),oS=()=>(0,I.jsxs)(W,{sx:ey.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Router`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`In UITKX, routing is authored directly in markup. You compose `,(0,I.jsx)(`code`,{children:`<Router>`}),`,`,(0,I.jsx)(`code`,{children:`<Route>`}),`, links, and routed child components as part of the same returned UI tree.`]}),(0,I.jsxs)(G,{sx:ey.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`<Router>`}),` establishes routing context for the subtree.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`<Route>`}),` matches paths and can render elements or child component trees.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks`}),` stay in setup code for imperative navigation, history control, params, query values, and navigation state.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),` pushes or replaces locations, while `,(0,I.jsx)(`code`,{children:`UseGo()`}),` and `,(0,I.jsx)(`code`,{children:`UseCanGo()`}),` drive back/forward UI.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseLocationInfo()`}),`, `,(0,I.jsx)(`code`,{children:`UseParams()`}),`, `,(0,I.jsx)(`code`,{children:`UseQuery()`}),`, and `,(0,I.jsx)(`code`,{children:`UseNavigationState()`}),` expose the active routed data.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`RouterHooks.UseBlocker()`}),` lets you intercept transitions when a screen has unsaved or guarded state.`]})})})]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The example below shows both styles together: declarative route composition in markup, and imperative setup-code helpers through `,(0,I.jsx)(`code`,{children:`RouterHooks`}),` for navigation and route data.`]}),(0,I.jsx)(Z,{language:`tsx`,code:`@using ReactiveUITK.Router

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
}`})]}),sS=()=>(0,I.jsxs)(W,{sx:ny.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Signals`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Signals remain the shared-state primitive underneath the UITKX authoring model. Outside components you work with the signal registry directly; inside UITKX components you typically read them with `,(0,I.jsx)(`code`,{children:`useSignal(...)`}),` and dispatch updates from event handlers.`]}),(0,I.jsxs)(W,{children:[(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Runtime access`}),(0,I.jsx)(Z,{language:`tsx`,code:`using ReactiveUITK.Signals;

SignalsRuntime.EnsureInitialized();
var counter = Signals.Get<int>("demo.counter", 0);
counter.Dispatch(previous => previous + 1);`})]}),(0,I.jsxs)(W,{children:[(0,I.jsx)(U,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Using signals inside UITKX`}),(0,I.jsxs)(G,{sx:ny.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Use a signal factory or registry lookup in setup code.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Read the current value with useSignal(...).`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Dispatch updates directly from UITKX event handlers.`})})]}),(0,I.jsx)(Z,{language:`tsx`,code:`@using ReactiveUITK.Signals
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
}`})]})]});var cS={root:{display:`flex`,flexDirection:`column`,gap:2},list:{pl:2},table:{my:1}},lS=({title:e,children:t})=>(0,I.jsxs)(W,{children:[(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:e}),t]});const uS=()=>(0,I.jsxs)(W,{sx:cS.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Hot Module Replacement`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Hot Module Replacement lets you edit `,(0,I.jsx)(`code`,{children:`.uitkx`}),` files and see changes instantly in the Unity Editor — without domain reload, without losing component state.`]}),(0,I.jsx)(lS,{title:`Quick Start`,children:(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Open `,(0,I.jsx)(`strong`,{children:`ReactiveUITK → HMR Mode`}),` from the Unity menu bar.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Click `,(0,I.jsx)(`strong`,{children:`Start HMR`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Edit and save any `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`The component updates in-place — hook state (counters, refs, effects) is preserved.`})})]})}),(0,I.jsxs)(lS,{title:`How It Works`,children:[(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`When HMR is active:`}),(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Assembly reloads are locked — no domain reload occurs on file saves.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`A `,(0,I.jsx)(`code`,{children:`FileSystemWatcher`}),` detects `,(0,I.jsx)(`code`,{children:`.uitkx`}),` changes under `,(0,I.jsx)(`code`,{children:`Assets/`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`The file is parsed and emitted to C# using `,(0,I.jsx)(`code`,{children:`ReactiveUITK.Language.dll`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`C# is compiled in-process via Roslyn (`,(0,I.jsx)(`code`,{children:`Microsoft.CodeAnalysis.CSharp`}),` 4.3.1), with automatic fallback to external `,(0,I.jsx)(`code`,{children:`csc.dll`}),` if Roslyn DLLs aren't available.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`The compiled assembly is loaded via `,(0,I.jsx)(`code`,{children:`Assembly.Load(byte[])`}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`The new `,(0,I.jsx)(`code`,{children:`Render`}),` delegate is swapped into all active `,(0,I.jsx)(`code`,{children:`RootRenderer`}),` instances.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`A re-render is triggered — hooks run against preserved state.`})})]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Total time: typically `,(0,I.jsx)(`strong`,{children:`25–100 ms`}),` compile + emit from save to visual update (first compile per session is ~1–1.5s due to Roslyn JIT warmup).`]})]}),(0,I.jsxs)(lS,{title:`State Preservation`,children:[(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`HMR preserves all hook state across swaps:`}),(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`useState`}),` — current values retained.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`useRef`}),` — ref objects preserved.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`useEffect`}),` — cleanup runs, effect re-runs with new closure.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`useMemo`}),` / `,(0,I.jsx)(`code`,{children:`useCallback`}),` — recomputed with new function body.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(`code`,{children:`useContext`}),` — context values preserved.`]})})})]}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`If the number or order of hooks changes between edits, HMR detects the mismatch, resets state for that component, and logs a warning.`})]}),(0,I.jsxs)(lS,{title:`Companion Files`,children:[(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Companion `,(0,I.jsx)(`code`,{children:`.cs`}),` files are `,(0,I.jsx)(`strong`,{children:`optional`}),`. The source generator produces a complete class from the `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file alone. However, you can add`,(0,I.jsx)(`code`,{children:`.cs`}),` files in the same directory to share styles, types, or utilities. When a`,` `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file changes, HMR automatically includes all `,(0,I.jsx)(`code`,{children:`.cs`}),` files in the same directory (excluding `,(0,I.jsx)(`code`,{children:`.g.cs`}),`) in the compilation:`]}),(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Style helpers (e.g. `,(0,I.jsx)(`code`,{children:`MyComponent.styles.cs`}),`)`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Type / prop definitions (e.g. `,(0,I.jsx)(`code`,{children:`MyComponent.types.cs`}),`)`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Shared utilities (e.g. `,(0,I.jsx)(`code`,{children:`MyComponent.utils.cs`}),`)`]})})})]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Companion `,(0,I.jsx)(`code`,{children:`.cs`}),` file changes also trigger HMR — saving a`,` `,(0,I.jsx)(`code`,{children:`.styles.cs`}),` or `,(0,I.jsx)(`code`,{children:`.utils.cs`}),` file automatically detects the associated `,(0,I.jsx)(`code`,{children:`.uitkx`}),` in the same directory, recompiles everything, and swaps the result in-place.`]}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`strong`,{children:`Creating new companion files`}),` works too — simply create a `,(0,I.jsx)(`code`,{children:`.cs`}),` `,`file in the same directory as your `,(0,I.jsx)(`code`,{children:`.uitkx`}),`. The file watcher detects new files and includes them in the next compilation.`]})]}),(0,I.jsxs)(lS,{title:`New Component Support`,children:[(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`HMR can compile and load `,(0,I.jsx)(`strong`,{children:`new`}),` `,(0,I.jsx)(`code`,{children:`.uitkx`}),` files that don't exist in any pre-compiled assembly:`]}),(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`When a parent component references an unknown child, CS0103 errors are caught.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`HMR scans the project for matching `,(0,I.jsx)(`code`,{children:`.uitkx`}),` files and compiles them first.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`The parent is automatically retried after the dependency resolves.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Cross-component references are managed via an assembly registry.`})})]})]}),(0,I.jsxs)(lS,{title:`HMR Window`,children:[(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The HMR window (`,(0,I.jsx)(`strong`,{children:`ReactiveUITK → HMR Mode`}),`) shows:`]}),(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Start / Stop button with status indicator.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Stats: swap count, error count, last component name and timing.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Timing breakdown: Parse, Emit, Compile, and Swap durations per step.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Settings: auto-stop on play mode, swap notifications.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Keyboard Shortcuts: configurable bindings (see below).`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Recent Errors: last 10 compilation errors (scrollable, copyable).`})})]})]}),(0,I.jsxs)(lS,{title:`Keyboard Shortcuts`,children:[(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`Shortcuts are not bound by default — configure them in the HMR window to avoid conflicting with your existing keybindings.`}),(0,I.jsx)(L_,{component:nd,variant:`outlined`,sx:cS.table,children:(0,I.jsxs)(S_,{size:`small`,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Action`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Description`})})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Toggle HMR`}),(0,I.jsx)(J,{children:`Start or stop the HMR session`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Open / Close Window`}),(0,I.jsx)(J,{children:`Show or hide the HMR window`})]})]})]})}),(0,I.jsx)(U,{variant:`body2`,sx:{mt:1},children:`Requirements: at least one modifier key (Ctrl, Alt, or Shift) plus one regular key.`})]}),(0,I.jsxs)(lS,{title:`Lifecycle`,children:[(0,I.jsx)(L_,{component:nd,variant:`outlined`,sx:cS.table,children:(0,I.jsxs)(S_,{size:`small`,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Event`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Behavior`})})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Start HMR`}),(0,I.jsx)(J,{children:`Assembly reload locked, file watcher started`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Stop HMR`}),(0,I.jsx)(J,{children:`Assembly reload unlocked, pending changes compile normally`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Enter / Exit Play Mode`}),(0,I.jsx)(J,{children:`Auto-stops HMR (configurable)`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Build (Player)`}),(0,I.jsx)(J,{children:`Auto-stops HMR`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Editor quit`}),(0,I.jsx)(J,{children:`Auto-stops HMR`})]})]})]})}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,sx:{mt:1},children:[`While HMR is active, `,(0,I.jsx)(`strong`,{children:`all compilation is deferred`}),` — not just`,` `,(0,I.jsx)(`code`,{children:`.uitkx`}),` changes. Any `,(0,I.jsx)(`code`,{children:`.cs`}),` edits accumulate and compile in one batch when HMR is stopped.`]})]}),(0,I.jsx)(lS,{title:`Limitations`,children:(0,I.jsx)(L_,{component:nd,variant:`outlined`,sx:cS.table,children:(0,I.jsxs)(S_,{size:`small`,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Limitation`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Details`})})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Old assemblies stay in memory`}),(0,I.jsx)(J,{children:`Mono cannot unload assemblies. ~10–30 KB per swap, cleared on domain reload.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`All compilation deferred`}),(0,I.jsxs)(J,{children:[`Non-UITKX `,(0,I.jsx)(`code`,{children:`.cs`}),` changes don't take effect until HMR stops. UX warning shown.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`First compile is slow`}),(0,I.jsx)(J,{children:`~1–1.5s on first HMR compile per session (Roslyn JIT warmup). Subsequent compiles are 25–100ms.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Requires NuGet cache`}),(0,I.jsxs)(J,{children:[`In-process Roslyn loads DLLs from `,(0,I.jsx)(`code`,{children:`~/.nuget/packages/`}),`. Falls back to external `,(0,I.jsx)(`code`,{children:`csc.dll`}),` if unavailable.`]})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Static field changes ignored`}),(0,I.jsx)(J,{children:`Statics live on the old assembly's type.`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Cross-assembly props`}),(0,I.jsx)(J,{children:`Props are read via reflection to handle type mismatches across assemblies.`})]})]})]})})}),(0,I.jsxs)(lS,{title:`Troubleshooting`,children:[(0,I.jsx)(U,{variant:`h6`,component:`h3`,gutterBottom:!0,children:`HMR doesn't start`}),(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Check the Console for initialization errors.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Ensure `,(0,I.jsx)(`code`,{children:`ReactiveUITK.Language.dll`}),` exists in the `,(0,I.jsx)(`code`,{children:`Analyzers/`}),` folder.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Verify Unity's Roslyn compiler is present at `,(0,I.jsxs)(`code`,{children:["${EditorPath}",`/Data/DotNetSdkRoslyn/csc.dll`]}),`.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Check for `,(0,I.jsx)(`code`,{children:`[HMR] In-process Roslyn compiler loaded successfully`}),` in Console.`]})})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`If Roslyn fails to load, verify that `,(0,I.jsx)(`code`,{children:`~/.nuget/packages/microsoft.codeanalysis.csharp/4.3.1/`}),` exists.`]})})})]}),(0,I.jsx)(U,{variant:`h6`,component:`h3`,gutterBottom:!0,sx:{mt:2},children:`Changes don't appear`}),(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Confirm the file is saved (not just modified).`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Check the HMR window for compilation errors.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Verify the file is under `,(0,I.jsx)(`code`,{children:`Assets/`}),` (the watched directory).`]})})})]}),(0,I.jsx)(U,{variant:`h6`,component:`h3`,gutterBottom:!0,sx:{mt:2},children:`State is lost after edit`}),(0,I.jsxs)(G,{sx:cS.list,children:[(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:`Hook order or count may have changed — this triggers automatic state reset.`})}),(0,I.jsx)(K,{disablePadding:!0,children:(0,I.jsx)(q,{primary:(0,I.jsxs)(I.Fragment,{children:[`Check Console for "`,(0,I.jsx)(`code`,{children:`[HMR] Hook mismatch`}),`" messages.`]})})})]})]})]});var dS={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:3},question:{mt:2,fontWeight:600}};const fS=()=>(0,I.jsxs)(W,{sx:dS.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Frequently Asked Questions`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:dS.section,children:`General`}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`What is UITKX?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`UITKX is a markup language for authoring Unity UI Toolkit components using a React-like model. You write `,(0,I.jsx)(`code`,{children:`.uitkx`}),` files with JSX-style markup, hooks, and control flow. A Roslyn source generator compiles them into standard C# that runs on the ReactiveUIToolKit runtime.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Which Unity versions are supported?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Unity `,(0,I.jsx)(`strong`,{children:`6.2`}),` and above. The framework relies on UI Toolkit APIs available from Unity 6.2 onward.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Does UITKX work with existing UI Toolkit code?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Yes. UITKX components render into the same `,(0,I.jsx)(`code`,{children:`VisualElement`}),` tree as hand-written UI Toolkit code. You can mount UITKX components alongside existing UI Toolkit panels, mix UITKX components with native elements, and interop through standard `,(0,I.jsx)(`code`,{children:`VisualElement`}),` references.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Does UITKX add runtime overhead?`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`The reconciliation scheduler adds a small per-frame cost similar to other retained-mode UI frameworks. In practice, the overhead is negligible for typical UI workloads. All generated code is standard C# — there is no runtime code generation or reflection.`}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Can I use UITKX in production builds?`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`Yes. The source generator produces plain C# at compile time. The generated output is included in your build like any other script. There is no interpreter or runtime codegen — UITKX is fully AOT-compatible.`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:dS.section,children:`IDE & Editor Extensions`}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Which editors are supported?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[(0,I.jsx)(`strong`,{children:`VS Code`}),` and `,(0,I.jsx)(`strong`,{children:`Visual Studio 2022`}),` have full, officially supported extensions with syntax highlighting, completions, hover documentation, diagnostics, and formatting. A`,` `,(0,I.jsx)(`strong`,{children:`JetBrains Rider`}),` plugin exists as a stub — source generation and `,(0,I.jsx)(`code`,{children:`#line`}),` mapping work via standard Roslyn support, but the full editing experience has not been fully verified. Rider is not officially supported in V1.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Do I need the VS Code extension to use UITKX?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`No. The source generator runs inside Unity regardless of your editor. The extension provides the editing experience — syntax highlighting, completions, error squiggles, and formatting. Without it you can still write `,(0,I.jsx)(`code`,{children:`.uitkx`}),` files, but you won't have language support.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`VS Code shows wrong colours briefly when I open a file — is that a bug?`}),(0,I.jsx)(U,{variant:`body2`,paragraph:!0,children:`This is expected. VS Code layers TextMate grammar colours first, then overrides them with LSP semantic tokens after ~200 ms. The brief flash (e.g. PascalCase names appearing green) is inherent to how VS Code works and resolves automatically.`}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`What .NET version does the language server need?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`The LSP server requires `,(0,I.jsx)(`strong`,{children:`.NET 8`}),` or later. Run`,` `,(0,I.jsx)(`code`,{children:`dotnet --version`}),` to verify. If you have a non-standard install location, set `,(0,I.jsx)(`code`,{children:`uitkx.server.dotnetPath`}),` in VS Code settings.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:dS.section,children:`Authoring`}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Can I use standard C# inside UITKX files?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Yes. The setup code section (before the `,(0,I.jsx)(`code`,{children:`return`}),`) is standard C#. You can declare variables, call methods, use LINQ, and access any type available via `,(0,I.jsx)(`code`,{children:`@using`}),` directives. Attribute values inside markup also accept C# expressions via the `,(0,I.jsx)(`code`,{children:`{expr}`}),` syntax.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Does HMR (Hot Module Replacement) affect build times?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`No. HMR bypasses Unity's normal compilation pipeline — it compiles only the changed `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file using Roslyn directly, loads the result via `,(0,I.jsx)(`code`,{children:`Assembly.Load`}),`, and swaps the render delegate. Typical save-to-visual-update time is 50–200 ms. When HMR is stopped, Unity compiles normally.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Why do hooks need to be at the top level?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`UITKX follows the same rules as React hooks — they must be called unconditionally at the component top level, never inside`,` `,(0,I.jsx)(`code`,{children:`@if`}),`, `,(0,I.jsx)(`code`,{children:`@foreach`}),`, or event handlers. This ensures hooks are called in the same order on every render, which is required for the reconciler to track state correctly.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,sx:dS.section,children:`Troubleshooting`}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`I see "Failed to resolve assembly: Assembly-CSharp-Editor" from Burst — what do I do?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Go to `,(0,I.jsx)(`strong`,{children:`Edit → Project Settings → Burst AOT Settings`}),` and add `,(0,I.jsx)(`code`,{children:`Assembly-CSharp-Editor`}),` to the exclusion list. This prevents Burst from trying to AOT-compile editor-only assemblies.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`Completions or hover stopped working — how do I debug?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Set `,(0,I.jsx)(`code`,{children:`uitkx.trace.server`}),` to `,(0,I.jsx)(`code`,{children:`"verbose"`}),` in VS Code settings, then open the Output panel (Ctrl+Shift+U) and select the "UITKX Language Server" channel. Check for error messages or missing responses. See the `,(0,I.jsx)(`em`,{children:`Debugging Guide`}),` page for more details.`]}),(0,I.jsx)(U,{variant:`body1`,sx:dS.question,children:`My component has red squiggles but the code looks correct — what's wrong?`}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[`Make sure the file is saved — the language server works on the last saved content. Also check that the `,(0,I.jsx)(`code`,{children:`component`}),` keyword is present and that any `,(0,I.jsx)(`code`,{children:`@using`}),` directives are correct.`]})]}),pS=()=>(0,I.jsxs)(W,{sx:Qv.root,children:[(0,I.jsx)(U,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Styling`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit provides a `,(0,I.jsxs)(`strong`,{children:[`typed `,(0,I.jsx)(`code`,{children:`Style`}),` class`]}),` with compile-time checked properties that map directly to Unity UI Toolkit's inline style system. A companion `,(0,I.jsx)(`strong`,{children:(0,I.jsx)(`code`,{children:`CssHelpers`})}),` static class provides terse shortcuts for lengths, colors, and enum values.`]}),(0,I.jsxs)(rf,{severity:`info`,sx:{mb:3},children:[`Both `,(0,I.jsx)(`code`,{children:`Style`}),` and `,(0,I.jsx)(`code`,{children:`CssHelpers`}),` live in the`,` `,(0,I.jsx)(`code`,{children:`ReactiveUITK.Props.Typed`}),` namespace.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Setup`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Add these directives at the top of your `,(0,I.jsx)(`code`,{children:`.uitkx`}),` file or companion `,(0,I.jsx)(`code`,{children:`.cs`}),`:`]}),(0,I.jsx)(Z,{language:`tsx`,code:`@using ReactiveUITK.Props.Typed
@using UnityEngine
@using UnityEngine.UIElements
@using static ReactiveUITK.Props.Typed.CssHelpers`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Before & After`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`The old tuple-based syntax is still supported but the new typed properties give you IntelliSense, compile errors on typos, and type checking on values:`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Old untyped tuple syntax (still works as escape hatch)
var panelStyle = new Style {
    (StyleKeys.Height, 80f),
    (StyleKeys.BorderRadius, 6f),
    (StyleKeys.BackgroundColor, new Color(0.2f, 0.2f, 0.25f, 0.9f)),
    (StyleKeys.JustifyContent, "center"),
    (StyleKeys.AlignItems, "center"),
    (StyleKeys.MarginBottom, 10f),
};`}),(0,I.jsx)(Z,{language:`tsx`,code:`// New typed property syntax — compile-time checked
var panelStyle = new Style {
    Height = 80f,
    BorderRadius = 6f,
    BackgroundColor = new Color(0.2f, 0.2f, 0.25f, 0.9f),
    JustifyContent = JustifyCenter,
    AlignItems = AlignCenter,
    MarginBottom = 10f,
};`}),(0,I.jsx)(Z,{language:`tsx`,code:`// Even more concise with CssHelpers
using static ReactiveUITK.Props.Typed.CssHelpers;

var panelStyle = new Style {
    Height = 80f,
    BorderRadius = Px(6),
    BackgroundColor = Rgba(0.2f, 0.2f, 0.25f, 0.9f),
    JustifyContent = JustifyCenter,
    AlignItems = AlignCenter,
    MarginBottom = 10f,
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Style property types`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Each property on `,(0,I.jsx)(`code`,{children:`Style`}),` accepts a specific Unity type. The compiler rejects mismatches immediately:`]}),(0,I.jsx)(L_,{component:nd,variant:`outlined`,sx:{mb:2},children:(0,I.jsxs)(S_,{size:`small`,children:[(0,I.jsx)(U_,{children:(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Category`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Type`})}),(0,I.jsx)(J,{children:(0,I.jsx)(`strong`,{children:`Examples`})})]})}),(0,I.jsxs)(k_,{children:[(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Layout & spacing`}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`StyleLength`})}),(0,I.jsx)(J,{children:`Width, Height, Margin, Padding, FlexBasis, FontSize, BorderRadius`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Flex & opacity`}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`StyleFloat`})}),(0,I.jsx)(J,{children:`FlexGrow, FlexShrink, Opacity, BorderWidth`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Colors`}),(0,I.jsx)(J,{children:(0,I.jsx)(`code`,{children:`Color`})}),(0,I.jsx)(J,{children:`TextColor, BackgroundColor, BorderColor (9 total)`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Enum styles`}),(0,I.jsx)(J,{children:`Unity enums`}),(0,I.jsx)(J,{children:`FlexDirection, JustifyContent, Position, Display, TextAlign (15 total)`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Background`}),(0,I.jsx)(J,{children:`Compound structs`}),(0,I.jsx)(J,{children:`BackgroundRepeat, BackgroundPositionX/Y, BackgroundSize`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Transforms`}),(0,I.jsxs)(J,{children:[(0,I.jsx)(`code`,{children:`float`}),` / struct`]}),(0,I.jsx)(J,{children:`Rotate, Scale, Translate, TransformOrigin`})]}),(0,I.jsxs)(Y,{children:[(0,I.jsx)(J,{children:`Assets`}),(0,I.jsxs)(J,{children:[(0,I.jsx)(`code`,{children:`Texture2D`}),` / `,(0,I.jsx)(`code`,{children:`Font`})]}),(0,I.jsx)(J,{children:`BackgroundImage, FontFamily`})]})]})]})}),(0,I.jsxs)(U,{variant:`body2`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`StyleLength`}),` has implicit conversions from `,(0,I.jsx)(`code`,{children:`float`}),`, `,(0,I.jsx)(`code`,{children:`int`}),`,`,` `,(0,I.jsx)(`code`,{children:`Length`}),`, and `,(0,I.jsx)(`code`,{children:`StyleKeyword`}),` — so `,(0,I.jsx)(`code`,{children:`Width = 100f`}),` and`,` `,(0,I.jsx)(`code`,{children:`Width = Pct(50)`}),` and `,(0,I.jsx)(`code`,{children:`Width = Auto`}),` all work.`]}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Layout & flexbox`}),(0,I.jsx)(Z,{language:`tsx`,code:`var cardStyle = new Style {
    Width = Pct(100),
    Height = Px(200),
    MinWidth = 120f,
    MaxHeight = Pct(50),
    FlexDirection = Column,
    JustifyContent = SpaceBetween,
    AlignItems = AlignCenter,
    FlexGrow = 1f,
    FlexWrap = WrapOn,
    Padding = 16f,
    Margin = 8f,
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Positioning`}),(0,I.jsx)(Z,{language:`tsx`,code:`var overlayStyle = new Style {
    Position = Absolute,
    Left = 0f,
    Top = 0f,
    Right = 0f,
    Bottom = 0f,
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Colors`}),(0,I.jsx)(Z,{language:`tsx`,code:`var headerStyle = new Style {
    TextColor = White,
    BackgroundColor = Hex("#1a1a2e"),
    BorderColor = Rgba(255, 200, 0),
    UnityTextOutlineColor = Black,
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Borders`}),(0,I.jsx)(Z,{language:`tsx`,code:`var cardStyle = new Style {
    BorderRadius = 8f,
    BorderTopLeftRadius = Px(12),
    BorderWidth = 2f,
    BorderColor = Rgba(255, 255, 255, 128),
    BorderLeftColor = Red,
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Text`}),(0,I.jsx)(Z,{language:`tsx`,code:`var titleStyle = new Style {
    FontSize = 24f,
    LetterSpacing = 1.5f,
    TextAlign = MiddleCenter,
    UnityFontStyle = Bold,
    TextOverflow = Ellipsis,
    WhiteSpace = Nowrap,
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Background (advanced)`}),(0,I.jsx)(Z,{language:`tsx`,code:`var bgStyle = new Style {
    BackgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
    BackgroundSize = new BackgroundSize(BackgroundSizeType.Cover),
    TransformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50), 0),
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Transforms`}),(0,I.jsx)(Z,{language:`tsx`,code:`var animatedStyle = new Style {
    Rotate = 45f,
    Scale = 1.2f,
    Translate = new Translate(Px(10), Px(-5), 0),
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Conditional styles`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[(0,I.jsx)(`code`,{children:`Style`}),` is a plain C# object — use ternaries, if/else, or any expression:`]}),(0,I.jsx)(Z,{language:`tsx`,code:`var buttonStyle = new Style {
    BackgroundColor = isHovered
        ? Rgba(0.3f, 0.85f, 0.45f)
        : Rgba(0.2f, 0.2f, 0.25f, 0.9f),
    Opacity = isEnabled ? 1f : 0.5f,
};`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Inline styles`}),(0,I.jsx)(Z,{language:`tsx`,code:`<Label text="Hello"
       style={new Style { TextColor = Green, FontSize = 18f }} />`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`CssHelpers reference`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`Import via `,(0,I.jsx)(`code`,{children:`using static ReactiveUITK.Props.Typed.CssHelpers;`}),` to use these directly without qualification:`]}),(0,I.jsx)(Z,{language:`text`,code:`// Length helpers
Pct(50)     → 50% (Length.Percent)
Px(100)     → 100px (Length.Pixel)

// Style keywords  
Auto        → StyleKeyword.Auto
None        → StyleKeyword.None
Initial     → StyleKeyword.Initial

// Color helpers
White, Black, Red, Green, Blue, Yellow, Cyan, Magenta, Grey, Transparent
Hex("#FF0000")          → Color from hex string
Rgba(255, 0, 0)         → Color from 0-255 byte values
Rgba(1f, 0f, 0f, 0.5f)  → Color from 0-1 float values`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Enum shortcuts`}),(0,I.jsx)(U,{variant:`body1`,paragraph:!0,children:`All enum values are available as static properties with concise names:`}),(0,I.jsx)(Z,{language:`text`,code:`// Flexbox enums
FlexDirection:  Row, Column, RowReverse, ColumnReverse
Justify:        JustifyStart, JustifyEnd, JustifyCenter, SpaceBetween, SpaceAround
Align:          AlignStart, AlignEnd, AlignCenter, Stretch, AlignAuto
Wrap:           WrapOn, NoWrap, WrapRev

// Layout enums
Position:       Relative, Absolute
DisplayStyle:   Flex, DisplayNone
Visibility:     Visible, Hidden
Overflow:       OverflowVisible, OverflowHidden
WhiteSpace:     Normal, Nowrap

// Text enums
TextOverflow:   Clip, Ellipsis
TextAnchor:     UpperLeft, UpperCenter, UpperRight,
                MiddleLeft, MiddleCenter, MiddleRight,
                LowerLeft, LowerCenter, LowerRight
FontStyle:      FontNormal, Bold, Italic, BoldItalic
TextOverflowPosition: OverflowStart, OverflowMiddle, OverflowEnd`}),(0,I.jsx)(U,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Dual API — typed & untyped`}),(0,I.jsxs)(U,{variant:`body1`,paragraph:!0,children:[`The typed properties are the recommended path. The old tuple syntax`,` `,(0,I.jsx)(`code`,{children:`(StyleKeys.Key, value)`}),` remains available as an escape hatch for edge cases (e.g. keys not yet exposed as typed properties):`]}),(0,I.jsx)(Z,{language:`tsx`,code:`// Typed properties — type-safe, IDE completion
var safe = new Style {
    Width = 100f,
    FlexDirection = Row,
};

// Tuple syntax — escape hatch for edge cases
var escape = new Style {
    ("width", 100f),
    ("flexDirection", "row"),
};

// Mix both in one style (not recommended but works)
var mixed = new Style {
    Width = 100f,
    ("customProperty", someValue),
};`})]});var mS=Nx.find(e=>e.id===`components`)?.pages??[],hS=(e,t)=>t===`/`?e:`${e}${t}`,gS=(e,t,n)=>t.map(t=>({id:`${e}-${t.id}`,title:t.title,track:e,pages:t.pages.map(t=>({id:`${e}-${t.id}`,canonicalId:t.id,title:t.title,path:hS(n,t.path),keywords:t.keywords,searchContent:t.searchContent,group:t.group,track:e,element:t.element}))}));const _S=[{id:`uitkx-intro`,title:`Introduction`,track:`uitkx`,pages:[{id:`uitkx-introduction`,canonicalId:`introduction`,title:`Introduction`,path:`/`,keywords:[`uitkx`,`introduction`,`markup`,`unity ui toolkit`],searchContent:`reactiveuitoolkit react-like ui toolkit runtime unity uitkx primary authoring language function-style components .uitkx hooks state effects reconcile visualelement markup-first c# v.* trees component countercard usestate return text button onclick highlights reactive diffing batched updates router signals utilities generated c# output production builds no runtime codegen var count setcount`,track:`uitkx`,element:()=>(0,I.jsx)(eS,{})}]},{id:`uitkx-getting-started`,title:`Getting Started`,track:`uitkx`,pages:[{id:`uitkx-getting-started-page`,canonicalId:`install`,title:`Install & Setup`,path:`/getting-started`,keywords:[`uitkx`,`install`,`setup`,`component`,`partial`],searchContent:`uitkx getting started primary authoring model reactiveuitoolkit function-style .uitkx components source generator produces complete class no boilerplate install via unity package manager open package manager add package from git url create a uitkx component setup code returned markup generator emits render mount rootrenderer v.func @namespace MyGame.UI component HelloWorld var count setCount useState return VisualElement Text Hello ReactiveUITK Button Increment onClick setCount count + 1 companion files optional styles types utils`,track:`uitkx`,element:()=>(0,I.jsx)(Qx,{})}]},{id:`uitkx-companion-files`,title:`Companion Files`,track:`uitkx`,pages:[{id:`uitkx-companion-files-page`,canonicalId:`companion-files`,title:`Companion Files`,path:`/companion-files`,keywords:[`uitkx`,`companion`,`styles`,`types`,`utils`,`partial class`],searchContent:`companion files optional .cs file styles types utils naming conventions directory layout source generator produces complete class no boilerplate needed MyComponent.styles.cs style constants helpers colours sizes MyComponent.types.cs enums structs DTOs MyComponent.utils.cs pure helper formatting functions hmr support editing companion triggers hmr creating new file detected instantly @code blocks when not to use simple components small helpers`,track:`uitkx`,element:()=>(0,I.jsx)(Hx,{})}]},{id:`uitkx-styling`,title:`Styling`,track:`uitkx`,pages:[{id:`uitkx-styling-page`,canonicalId:`styling`,title:`Styling`,path:`/styling`,keywords:[`uitkx`,`style`,`css`,`typed`,`CssHelpers`,`StyleKeys`,`layout`,`colors`,`flexbox`],searchContent:`styling typed style class compile-time checked properties inline style system CssHelpers static helpers Pct Px Auto None Initial length units color helpers Hex Rgba enum shortcuts FlexDirection Row Column JustifyContent JustifyCenter AlignItems AlignCenter Stretch SpaceBetween SpaceAround Position Absolute Relative Display Flex Visibility Hidden Overflow WhiteSpace TextOverflow TextAnchor FontStyle StyleLength StyleFloat StyleKeyword Width Height Margin Padding BorderRadius BackgroundColor TextColor BorderColor FlexGrow FlexShrink Opacity FontSize LetterSpacing BackgroundRepeat BackgroundSize BackgroundPosition TransformOrigin Rotate Scale Translate dual API typed properties tuple syntax escape hatch StyleKeys backward compatible`,track:`uitkx`,element:()=>(0,I.jsx)(pS,{})}]},{id:`uitkx-components`,title:`Components Overview`,track:`uitkx`,pages:[{id:`uitkx-components-overview`,canonicalId:`uitkx-components-overview`,title:`Components Overview`,path:`/components`,keywords:[`uitkx`,`components`,`intrinsic tags`,`custom components`],searchContent:`components in uitkx intrinsic tags visualelement button text router tags custom components pascalcase names native element consumers custom component name authoring guidelines prefer direct tag props hand-building props objects keep setup code small close to returned markup tree custom component names must not collide with native tag name`,track:`uitkx`,element:()=>(0,I.jsx)(Vx,{})}]},{id:`uitkx-component-reference`,title:`Components`,track:`uitkx`,pages:mS.map(e=>({id:`uitkx-${e.id}`,canonicalId:e.id,title:e.title,path:e.path,keywords:[`uitkx`,...e.keywords??[]],group:e.group,track:`uitkx`,element:()=>(0,I.jsx)(zx,{title:e.title})}))},{id:`uitkx-concepts`,title:`Concepts & Environment`,track:`uitkx`,pages:[{id:`uitkx-concepts-page`,canonicalId:`concepts-and-environment`,title:`Concepts & Environment`,path:`/concepts`,keywords:[`uitkx`,`concepts`,`environment`,`defines`],searchContent:`concepts and environment uitkx authoring layer reactiveuitk runtime layer components intrinsic tags hooks markup structure reconciliation scheduling adapter application mental model write ui uitkx setup code local component generator runtime bridge unity ui toolkit core authoring rules intrinsic uitkx native tag names reserved custom components distinct names function-style components default form setup code first single returned markup tree state setters called directly as functions setcount(count + 1) companion partial classes host generated output environment defines compile-time environment tracing symbols env_dev env_staging env_prod environment labeling ruitk_trace_verbose ruitk_trace_basic runtime diagnostics editor-only diagnostic helpers development symbols`,track:`uitkx`,element:()=>(0,I.jsx)(Ux,{})}]},{id:`uitkx-differences`,title:`Different from React`,track:`uitkx`,pages:[{id:`uitkx-differences-page`,canonicalId:`different-from-react`,title:`Different from React`,path:`/differences`,keywords:[`uitkx`,`react`,`hooks`,`rendering`],searchContent:`different from react uitkx borrows react component-and-hooks mental model unity ui toolkit c# runtime markup-first visualelement system scheduling model c# semantics state updates usestate behaves like react usestate setter directly value updater function uitkx lowers runtime hook implementation component rendering model reactiveuitk fiber schedule work asynchronously scheduler sliced render work deferred passive effects concurrent feature surface scheduler unity runtime constraints jsx-like syntax runtime representation browser dom model interop unity controls styles events first-class constraint apis differ from browser react conventions`,track:`uitkx`,element:()=>(0,I.jsx)(Zx,{})}]},{id:`uitkx-tooling`,title:`Tooling`,track:`uitkx`,pages:[{id:`uitkx-router-page`,canonicalId:`router`,title:`Router`,path:`/tooling/router`,keywords:[`uitkx`,`router`,`routes`,`navigation`],searchContent:`router uitkx routing authored directly in markup Router Route links routed child components returned UI tree Router establishes routing context subtree Route matches paths render elements RouterHooks setup code imperative navigation history control params query values navigation state RouterHooks.UseNavigate() pushes replaces locations UseGo() UseCanGo() back forward RouterHooks.UseLocationInfo() UseParams() UseQuery() UseNavigationState() expose active routed data RouterHooks.UseBlocker() intercept transitions unsaved guarded state declarative route composition imperative helpers @using ReactiveUITK.Router component RouterDemo var navigate RouterHooks.UseNavigate var parameters RouterHooks.UseParams var query RouterHooks.UseQuery RouterNavLink path label exact Route path /users/:id VisualElement Text User Not found`,track:`uitkx`,element:()=>(0,I.jsx)(oS,{})},{id:`uitkx-signals-page`,canonicalId:`signals`,title:`Signals`,path:`/tooling/signals`,keywords:[`uitkx`,`signals`,`shared state`],searchContent:`signals shared-state primitive uitkx authoring model signal registry useSignal dispatch updates event handlers SignalsRuntime.EnsureInitialized Signals.Get SignalFactory.Get useMemo @using ReactiveUITK.Signals System component SignalCounterDemo var counterSignal SignalFactory.Get int demo.counter var count useSignal counterSignal Text Signal Counter Count Button Increment onClick counterSignal.Dispatch v + 1 Reset Dispatch 0 Style StyleKeys.FlexDirection row`,track:`uitkx`,element:()=>(0,I.jsx)(sS,{})},{id:`uitkx-hmr-page`,canonicalId:`hmr`,title:`Hot Module Replacement`,path:`/tooling/hmr`,keywords:[`uitkx`,`hmr`,`hot reload`,`live editing`,`instant preview`],searchContent:`hot module replacement hmr edit .uitkx files changes instantly unity editor without domain reload component state quick start open reactiveuitk hmr mode start edit save updates in-place hook state counters refs effects preserved assembly reloads filesystemwatcher detects parsed emitted compiled in-process roslyn microsoft.codeanalysis.csharp csc.dll fallback assembly.load render delegate swapped rootrenderer instances re-render hooks state preservation usestate useref useeffect usememo usecallback usecontext companion files .cs partial class style types create new companion auto-detected new component support cs0103 dependency auto-discovery cross-component assembly registry hmr window stats swap count error timing parse emit compile keyboard shortcuts toggle start stop lifecycle limitations old assemblies memory mono unload 10-30 kb per swap first compile jit warmup nuget cache troubleshooting console errors`,track:`uitkx`,element:()=>(0,I.jsx)(uS,{})}]},{id:`uitkx-api`,title:`API`,track:`uitkx`,pages:[{id:`uitkx-api-page`,canonicalId:`api-reference`,title:`API Map`,path:`/api`,keywords:[`uitkx`,`api`,`hooks`,`runtime`],searchContent:`uitkx api map author markup hooks setup code runtime types mount integrate debug authoring surface .uitkx function-style components usestate useeffect usememo usesignal router context helpers intrinsic tags built-in reactiveuitk ui toolkit elements runtime layer virtualnode rootrenderer editorrootrendererutility props classes typed styles`,track:`uitkx`,element:()=>(0,I.jsx)(Px,{})}]},{id:`uitkx-reference-guides`,title:`Reference & Guides`,track:`uitkx`,pages:[{id:`uitkx-language-reference`,canonicalId:`language-reference`,title:`Language Reference`,path:`/reference`,keywords:[`uitkx`,`directives`,`syntax`,`control flow`,`expressions`],searchContent:`uitkx language reference directives syntax control flow expressions header directives @namespace My.Game.UI c# namespace generated class @component MyButton component class name must match filename @using System.Collections.Generic adds using directive generated file @props MyButtonProps props type consumed by the component @key root-key static key root element @inject ILogger logger dependency-injected field function-style components component keyword preamble declaration parameters typed optional default @using UnityEngine component Counter string label Count var count setCount useState return VisualElement Label text Button onClick setCount conditional rendering @if (isLoggedIn) Label Welcome back @else Button Log in login @foreach (var item in items) Label key item.Id text item.Name @switch (mode) @case dark Label Dark mode @default Label Light mode @for c-style for loop @while while loop @break exit loop @continue skip next iteration @(expr) @(MyCustomComponent) render component expression inline markup children {expr} c# expression attribute value literal plain string attribute {/* comment */} jsx-style block comment rules gotchas hook calls must be unconditional component top level single root element component names must match filename reconciliation`,track:`uitkx`,element:()=>(0,I.jsx)(aS,{})},{id:`uitkx-diagnostics`,canonicalId:`diagnostics`,title:`Diagnostics`,path:`/diagnostics`,keywords:[`uitkx`,`diagnostics`,`errors`,`warnings`,`codes`],searchContent:`diagnostics reference diagnostic code uitkx source generator language server severity meaning fix source generator diagnostics compile time roslyn processing .uitkx files uitkx0001 warning unknown built-in element tag name pascalcase button label uitkx0002 warning unknown attribute element attribute name property props type uitkx0005 error missing required directive @namespace @component uitkx0006 warning @component name mismatch rename file uitkx0008 warning unknown function component type exists public static render method uitkx0009 warning @foreach child missing key stable unique identifier uitkx0010 warning duplicate sibling key unique key value uitkx0012 error directive order error @namespace above @component uitkx0013 error hook in conditional hook call must be at component top level not inside @if branch uitkx0014 error hook in loop must be at component top level not inside @foreach loop uitkx0015 error hook in switch case must be at component top level uitkx0016 error hook in event handler must be at component top level uitkx0017 error multiple root elements wrap in single container element visualelement uitkx0018 warning useeffect missing dependency array explicit dependency array.empty uitkx0019 warning loop variable used as key stable unique identifier not loop index uitkx0020 error ref on component without ref param parameter remove ref attribute uitkx0021 error ref ambiguous multiple ref params explicit prop name structural diagnostics language server real time squiggly underlines editor uitkx0101 uitkx0102 uitkx0103 uitkx0104 uitkx0105 uitkx0106 uitkx0107 hint unreachable code uitkx0108 uitkx0109 uitkx0111 unused parameter parser diagnostics malformed syntax uitkx0300 unexpected token uitkx0301 unclosed tag uitkx0302 mismatched closing tag uitkx0303 unexpected end of file uitkx0304 unclosed expression or block uitkx0305 unknown markup directive uitkx0306 @(expr) in setup code inline expressions only valid inside markup`,track:`uitkx`,element:()=>(0,I.jsx)(Xx,{})},{id:`uitkx-config`,canonicalId:`configuration`,title:`Configuration`,path:`/config`,keywords:[`uitkx`,`config`,`settings`,`vscode`,`extension`],searchContent:`configuration reference configuration options uitkx editor extensions formatter vs code extension settings uitkx.server.path string absolute path to custom uitkxlanguageserver.dll leave empty to use bundled server uitkx.server.dotnetpath string path to dotnet executable .net 8+ sdk non-standard location uitkx.trace.server enum off controls lsp trace output messages verbose json-rpc traffic output panel uitkx language server channel editor defaults extension automatically configures editor settings for .uitkx files editor.defaultformatter reactiveuitk.uitkx uitkx formatter editor.formatonsave true auto-format on save recommended editor.tabsize 2 uitkx 2-space indentation editor.insertspaces true spaces not tabs editor.bracketpaircolorization false disabled conflicting colors uitkx semantic tokens`,track:`uitkx`,element:()=>(0,I.jsx)(Kx,{})},{id:`uitkx-debugging`,canonicalId:`debugging`,title:`Debugging Guide`,path:`/debugging`,keywords:[`uitkx`,`debugging`,`troubleshooting`,`logs`,`generated code`],searchContent:`debugging guide diagnose fix common issues uitkx inspecting generated code .uitkx .uitkx.g.cs roslyn source generator vs code definition f12 generated symbol generatedfiles folder analyzers output directory #line directives map errors original .uitkx file line number library packagecache com.reactiveuitk understanding #line directives c# compiler error generated code lsp server logs detailed lsp communication trace level vs code settings uitkx.trace.server verbose output panel ctrl+shift+u uitkx language server channel json-rpc requests responses missing completions stale diagnostics textdocument/publishdiagnostics server crashes formatter issues formatting unexpected results syntax errors red squiggles format-on-save editor.defaultformatter reactiveuitk.uitkx shift+alt+f reporting bugs minimal .uitkx file reproduces problem exact error message diagnostic code editor extension version lsp trace output`,track:`uitkx`,element:()=>(0,I.jsx)(Yx,{})}]},{id:`uitkx-faq`,title:`FAQ`,track:`uitkx`,pages:[{id:`uitkx-faq-page`,canonicalId:`faq-page`,title:`FAQ`,path:`/faq`,keywords:[`faq`,`frequently asked questions`,`help`],searchContent:`frequently asked questions what is uitkx markup language authoring unity ui toolkit components react-like model .uitkx jsx-style hooks control flow roslyn source generator which unity versions supported unity 6.2 does uitkx work with existing ui toolkit code visualelement does uitkx add runtime overhead reconciliation scheduler per-frame cost aot-compatible production builds plain c# no interpreter no runtime codegen which editors supported vs code visual studio 2022 full extensions syntax highlighting completions hover diagnostics formatting jetbrains rider stub not officially supported v1 do i need the vs code extension source generator runs inside unity wrong colours briefly textmate grammar lsp semantic tokens 200ms what .net version language server .net 8 dotnet directive-header form function-style components @namespace @component @props setup code c# @using hmr hot module replacement build times bypasses unity compilation roslyn assembly.load 50-200ms hooks top level unconditional @if @foreach reconciler burst assembly-csharp-editor project settings burst aot exclusion list completions hover stopped working uitkx.trace.server verbose output panel debugging guide red squiggles saved @namespace before @component`,track:`uitkx`,element:()=>(0,I.jsx)(fS,{})}]},{id:`uitkx-known-issues`,title:`Known Issues`,track:`uitkx`,pages:[{id:`uitkx-known-issues-page`,canonicalId:`known-issues-page`,title:`Known Issues`,path:`/known-issues`,keywords:[`issues`,`limitations`,`known issues`],searchContent:`known issues runtime multicolumnlistview briefly jump snap scrolling large data sets burst aot assembly resolution mono.cecil.assemblyresolutionexception failed resolve assembly assembly-csharp-editor project settings burst aot exclusion list editor-only assemblies uitkx types`,track:`uitkx`,element:()=>(0,I.jsx)(_x,{})}]},{id:`uitkx-roadmap`,title:`Roadmap`,track:`uitkx`,pages:[{id:`uitkx-roadmap-page`,canonicalId:`roadmap-page`,title:`Roadmap`,path:`/roadmap`,keywords:[`roadmap`,`future`,`plans`],searchContent:`roadmap documented future update planned features`,track:`uitkx`,element:()=>(0,I.jsx)(yx,{})}]}],vS=gS(`csharp`,Nx,`/csharp`),yS={uitkx:_S,csharp:vS};[..._S,...vS];const bS=e=>e===`/csharp`||e.startsWith(`/csharp/`)?`csharp`:`uitkx`,xS=e=>yS[e],SS=e=>yS[e].flatMap(e=>{if(e.title===`Components`){let t=e.pages.filter(e=>e.group===`basic`),n=e.pages.filter(e=>e.group===`advanced`||!e.group);return[...t,...n]}return e.pages}),CS=[...SS(`uitkx`),...SS(`csharp`)],wS=e=>e===`uitkx`?`/`:`/csharp`,TS=(e,t)=>CS.find(n=>n.track===e&&n.canonicalId===t)?.path??wS(e),ES=Nx.flatMap(e=>e.pages).filter(e=>e.path!==`/`).map(e=>({from:e.path,to:hS(`/csharp`,e.path)}));var DS=zl((0,I.jsx)(`path`,{d:`M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14`}),`Search`),OS=zl((0,I.jsx)(`path`,{d:`M12 1.27a11 11 0 00-3.48 21.46c.55.09.73-.28.73-.55v-1.84c-3.03.64-3.67-1.46-3.67-1.46-.55-1.29-1.28-1.65-1.28-1.65-.92-.65.1-.65.1-.65 1.1 0 1.73 1.1 1.73 1.1.92 1.65 2.57 1.2 3.21.92a2 2 0 01.64-1.47c-2.47-.27-5.04-1.19-5.04-5.5 0-1.1.46-2.1 1.2-2.84a3.76 3.76 0 010-2.93s.91-.28 3.11 1.1c1.8-.49 3.7-.49 5.5 0 2.1-1.38 3.02-1.1 3.02-1.1a3.76 3.76 0 010 2.93c.83.74 1.2 1.74 1.2 2.94 0 4.21-2.57 5.13-5.04 5.4.45.37.82.92.82 2.02v3.03c0 .27.1.64.73.55A11 11 0 0012 1.27`}),`GitHub`),kS={appBar:{borderBottom:1,borderColor:`divider`},toolbar:{display:`flex`,alignItems:`center`,gap:2},left:{display:`flex`,alignItems:`center`,gap:1.25},logo:{width:28,height:28,borderRadius:1},titleLink:{display:`flex`,alignItems:`center`,gap:.75,color:`inherit`,textDecoration:`none`},title:{fontWeight:600,letterSpacing:.3},center:{flex:1,display:`flex`,justifyContent:`center`},searchPaper:{p:`2px 8px`,display:`flex`,alignItems:`center`,gap:1,width:360,cursor:`text`},inputFlex:{flex:1},right:{ml:1,display:`flex`,alignItems:`center`,gap:1}};const AS=({onOpenSearch:e})=>{let{pathname:t}=Ve(),n=bS(t),r=CS.find(e=>e.path===t)?.canonicalId??`introduction`,i=TS(`uitkx`,r),a=TS(`csharp`,r);return(0,I.jsx)(gf,{position:`sticky`,color:`default`,elevation:0,sx:kS.appBar,children:(0,I.jsxs)(q_,{sx:kS.toolbar,children:[(0,I.jsxs)(W,{sx:kS.left,children:[(0,I.jsxs)(Pg,{component:nn,to:`/`,underline:`none`,sx:kS.titleLink,children:[(0,I.jsx)(W,{component:`img`,src:`/logo.png`,alt:`ReactiveUIToolKit logo`,sx:kS.logo}),(0,I.jsx)(U,{variant:`h6`,sx:kS.title,children:`ReactiveUIToolKit`})]}),(0,I.jsx)(Nm,{label:`v0.2.29`,size:`small`}),(0,I.jsx)(Nm,{label:`UITKX`,size:`small`,color:n===`uitkx`?`primary`:`default`,component:nn,to:i,clickable:!0}),(0,I.jsx)(Nm,{label:`C#`,size:`small`,color:n===`csharp`?`primary`:`default`,component:nn,to:a,clickable:!0})]}),(0,I.jsx)(W,{sx:kS.center,children:(0,I.jsxs)(nd,{sx:kS.searchPaper,variant:`outlined`,onClick:e,children:[(0,I.jsx)(DS,{fontSize:`small`}),(0,I.jsx)($m,{placeholder:`Search ${n===`uitkx`?`UITKX`:`C#`} docs…`,sx:kS.inputFlex,readOnly:!0,autoFocus:!0})]})}),(0,I.jsxs)(W,{sx:kS.right,children:[(0,I.jsx)(Nm,{label:`Unity 6.2+`,size:`small`}),(0,I.jsx)(Gd,{component:Pg,href:`https://github.com/yanivkalfa/ReactiveUIToolKit.git`,target:`_blank`,rel:`noreferrer`,children:(0,I.jsx)(OS,{})})]})]})})};var jS=zl((0,I.jsx)(`path`,{d:`m12 8-6 6 1.41 1.41L12 10.83l4.59 4.58L18 14z`}),`ExpandLess`),MS=zl((0,I.jsx)(`path`,{d:`M16.59 8.59 12 13.17 7.41 8.59 6 10l6 6 6-6z`}),`ExpandMore`),NS={root:{width:280,borderRight:1,borderColor:`divider`,height:`100%`,overflow:`auto`,"&::-webkit-scrollbar":{width:8},"&::-webkit-scrollbar-track":{backgroundColor:`transparent`},"&::-webkit-scrollbar-thumb":{backgroundColor:`rgba(25,118,210,0.4)`,borderRadius:999,border:`2px solid transparent`,backgroundClip:`padding-box`},"&::-webkit-scrollbar-thumb:hover":{backgroundColor:`rgba(25,118,210,0.7)`},scrollbarWidth:`thin`,scrollbarColor:`rgba(25,118,210,0.6) transparent`},childItem:{pl:4},sectionTitle:{fontWeight:700},subgroupHeader:{pl:4,pt:1,pb:.5,fontSize:11,textTransform:`uppercase`,letterSpacing:.5,color:`text.secondary`},subgroupDivider:{mt:.5,mb:.5,opacity:.4}};const PS=()=>{let e=Ve(),t=bS(e.pathname),n=xS(t).flatMap(e=>e.title===`Components`&&e.pages.some(e=>e.group)?[{...e,id:`components-common`,title:`Common Components`,pages:e.pages.filter(e=>e.group===`basic`)},{...e,id:`components-uncommon`,title:`Uncommon Components`,pages:e.pages.filter(e=>e.group===`advanced`||!e.group)}]:[e]),[r,i]=(0,x.useState)(()=>{let e={};return n.forEach((t,n)=>e[t.id]=n===0),e});return(0,I.jsxs)(W,{sx:NS.root,children:[(0,I.jsx)(W,{sx:{px:2,py:1.5},children:(0,I.jsx)(U,{variant:`overline`,color:`text.secondary`,children:t===`uitkx`?`UITKX Docs`:`C# Docs`})}),(0,I.jsx)(G,{disablePadding:!0,children:n.map(t=>{let n=!!r[t.id],a=t.pages.length===1,o=t.pages[0];return a?(0,I.jsxs)(W,{children:[(0,I.jsx)(Gg,{component:nn,to:o.path,selected:e.pathname===o.path,children:(0,I.jsx)(q,{primary:(0,I.jsx)(U,{sx:NS.sectionTitle,children:t.title})})}),(0,I.jsx)(Sg,{})]},t.id):(0,I.jsxs)(W,{children:[(0,I.jsxs)(Gg,{onClick:()=>i({...r,[t.id]:!r[t.id]}),children:[(0,I.jsx)(q,{primary:(0,I.jsx)(U,{sx:NS.sectionTitle,children:t.title})}),n?(0,I.jsx)(jS,{}):(0,I.jsx)(MS,{})]}),(0,I.jsx)(Qu,{in:n,timeout:`auto`,unmountOnExit:!0,children:(0,I.jsx)(G,{disablePadding:!0,children:t.pages.map(t=>(0,I.jsx)(Gg,{component:nn,to:t.path,selected:e.pathname===t.path,sx:NS.childItem,children:(0,I.jsx)(q,{primary:t.title})},t.id))})}),(0,I.jsx)(Sg,{})]},t.id)})})]})};var FS={root:{display:`flex`,justifyContent:`space-between`,borderTop:1,borderColor:`divider`,mt:4,pt:2}};const IS=()=>{let e=We(),{pathname:t}=Ve(),n=bS(t),r=(0,x.useMemo)(()=>SS(n),[n]),i=(0,x.useMemo)(()=>r.findIndex(e=>e.path===t),[r,t]),a=i>0?r[i-1]:void 0,o=i>=0&&i<r.length-1?r[i+1]:void 0;return(0,I.jsxs)(W,{sx:FS.root,children:[(0,I.jsx)(`span`,{children:a&&(0,I.jsxs)(vh,{onClick:()=>e(a.path),variant:`text`,children:[`← `,a.title]})}),(0,I.jsx)(`span`,{children:o&&(0,I.jsxs)(vh,{onClick:()=>e(o.path),variant:`text`,children:[o.title,` →`]})})]})};var LS=zl((0,I.jsx)(`path`,{d:`M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z`}),`Close`),RS=new Map,zS=[`children`,`code`,`codeRuntime`,`codeEditor`,`text`,`primary`,`secondary`,`label`,`title`,`placeholder`,`description`];function BS(e,t,n){if(e==null||typeof e==`boolean`)return;if(typeof e==`string`){t.push(e);return}if(typeof e==`number`){t.push(String(e));return}if(Array.isArray(e)){for(let r of e)BS(r,t,n);return}if(typeof e!=`object`)return;let r=e;if(r.props){if(typeof r.type==`function`&&n<8)try{BS(r.type(r.props),t,n+1);return}catch{}for(let e of zS){let i=r.props[e];i!=null&&BS(i,t,n)}}}function VS(e){let t=RS.get(e.id);if(t!==void 0)return t;let n=[];try{BS(e.element(),n,0)}catch{}let r=n.join(` `).toLowerCase();return RS.set(e.id,r),r}var HS={header:{display:`flex`,alignItems:`center`,gap:1,mb:1},inputPaper:{p:1,display:`flex`,alignItems:`center`,gap:1,flex:1},noResults:{p:2},content:{pt:1}};const US=({open:e,onClose:t})=>{let n=We(),{pathname:r}=Ve(),i=bS(r),a=(0,x.useMemo)(()=>SS(i),[i]),[o,s]=(0,x.useState)(``),[c,l]=(0,x.useState)(0),u=(0,x.useRef)(null);(0,x.useEffect)(()=>{if(e){let e=setTimeout(()=>u.current?.focus(),50);return()=>clearTimeout(e)}},[e]);let d=()=>{s(``),l(0),t()},f=(0,x.useMemo)(()=>{let e=o.trim().toLowerCase();if(!e)return[];let t=e.split(/\s+/).filter(Boolean);return a.filter(e=>{let n=[e.title,(e.keywords||[]).join(` `),e.searchContent||``,VS(e)].join(` `).toLowerCase();return t.every(e=>n.includes(e))})},[a,o]);return(0,x.useEffect)(()=>l(0),[f]),(0,I.jsx)(dg,{open:e,onClose:d,fullWidth:!0,maxWidth:`md`,children:(0,I.jsxs)(gg,{sx:HS.content,children:[(0,I.jsxs)(W,{sx:HS.header,children:[(0,I.jsxs)(nd,{sx:HS.inputPaper,variant:`outlined`,children:[(0,I.jsx)(DS,{}),(0,I.jsx)($m,{inputRef:u,placeholder:`Search ${i===`uitkx`?`UITKX`:`C#`} docs…`,value:o,onChange:e=>s(e.target.value),onKeyDown:e=>{e.key===`Escape`&&d(),e.key===`ArrowDown`&&(e.preventDefault(),l(e=>Math.min(e+1,f.length-1))),e.key===`ArrowUp`&&(e.preventDefault(),l(e=>Math.max(e-1,0))),e.key===`Enter`&&f[c]&&(d(),n(f[c].path))},sx:{flex:1}})]}),(0,I.jsx)(Gd,{onClick:d,"aria-label":`Close search`,children:(0,I.jsx)(LS,{})})]}),(0,I.jsxs)(G,{children:[f.map((e,t)=>(0,I.jsx)(Gg,{selected:t===c,onClick:()=>{d(),n(e.path)},children:(0,I.jsx)(q,{primary:e.title,secondary:(e.keywords||[]).join(`, `)})},e.id)),o&&f.length===0&&(0,I.jsx)(U,{sx:HS.noResults,color:`text.secondary`,children:`No results`})]})]})})};var WS={shell:{display:`grid`,gridTemplateRows:`auto 1fr`,height:`100vh`},grid:{display:`grid`,gridTemplateColumns:`280px 1fr`,minHeight:0},content:{p:3,overflow:`auto`},main:{maxWidth:980}};const GS=ml({palette:{mode:`dark`,background:{default:`#181c26`,paper:`#202532`},divider:`#343a4c`,primary:{main:`#4cc2ff`},text:{primary:`#e5e9f5`,secondary:`#a0a8c0`}},shape:{borderRadius:8},typography:{fontSize:14,body1:{lineHeight:1.3,color:`#a0a8c0`},body2:{lineHeight:1.3,color:`#a0a8c0`},h4:{fontSize:28,fontWeight:600,letterSpacing:.2,color:`#e5e9f5`},h5:{fontSize:20,fontWeight:600,letterSpacing:.15,marginTop:16,color:`#e5e9f5`}},components:{MuiCssBaseline:{styleOverrides:{code:{fontFamily:`ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace`,backgroundColor:`#202532`,borderRadius:4,padding:`2px 6px`,border:`1px solid #343a4c`,fontSize:`0.85em`}}}}});(0,Po.createRoot)(document.getElementById(`root`)).render((0,I.jsx)(x.StrictMode,{children:(0,I.jsx)($t,{children:(0,I.jsx)(()=>{let[e,t]=(0,x.useState)(!1);return(0,I.jsxs)(El,{theme:GS,children:[(0,I.jsx)(Dh,{}),(0,I.jsxs)(W,{sx:WS.shell,children:[(0,I.jsx)(AS,{onOpenSearch:()=>t(!0)}),(0,I.jsxs)(W,{sx:WS.grid,children:[(0,I.jsx)(PS,{}),(0,I.jsx)(W,{sx:WS.content,children:(0,I.jsxs)(gt,{children:[CS.map(e=>(0,I.jsx)(mt,{path:e.path,element:(0,I.jsxs)(W,{component:`main`,sx:WS.main,children:[e.element(),(0,I.jsx)(IS,{})]})},e.id)),ES.map(e=>(0,I.jsx)(mt,{path:e.from,element:(0,I.jsx)(pt,{to:e.to,replace:!0})},`legacy-${e.from}`)),(0,I.jsx)(mt,{path:`*`,element:(0,I.jsxs)(I.Fragment,{children:[(0,I.jsx)(U,{variant:`h5`,gutterBottom:!0,children:`Not Found`}),(0,I.jsx)(Pg,{component:nn,to:`/`,children:`Go to UITKX Introduction`})]})})]})})]})]}),(0,I.jsx)(US,{open:e,onClose:()=>t(!1)})]})},{})})}));
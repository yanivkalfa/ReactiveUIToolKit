var e=Object.create,t=Object.defineProperty,n=Object.getOwnPropertyDescriptor,r=Object.getOwnPropertyNames,i=Object.getPrototypeOf,a=Object.prototype.hasOwnProperty,o=(e,t)=>()=>(t||e((t={exports:{}}).exports,t),t.exports),s=(e,i,o,s)=>{if(i&&typeof i==`object`||typeof i==`function`)for(var c=r(i),l=0,u=c.length,d;l<u;l++)d=c[l],!a.call(e,d)&&d!==o&&t(e,d,{get:(e=>i[e]).bind(null,d),enumerable:!(s=n(i,d))||s.enumerable});return e},c=(n,r,a)=>(a=n==null?{}:e(i(n)),s(r||!n||!n.__esModule?t(a,`default`,{value:n,enumerable:!0}):a,n));(function(){let e=document.createElement(`link`).relList;if(e&&e.supports&&e.supports(`modulepreload`))return;for(let e of document.querySelectorAll(`link[rel="modulepreload"]`))n(e);new MutationObserver(e=>{for(let t of e)if(t.type===`childList`)for(let e of t.addedNodes)e.tagName===`LINK`&&e.rel===`modulepreload`&&n(e)}).observe(document,{childList:!0,subtree:!0});function t(e){let t={};return e.integrity&&(t.integrity=e.integrity),e.referrerPolicy&&(t.referrerPolicy=e.referrerPolicy),e.crossOrigin===`use-credentials`?t.credentials=`include`:e.crossOrigin===`anonymous`?t.credentials=`omit`:t.credentials=`same-origin`,t}function n(e){if(e.ep)return;e.ep=!0;let n=t(e);fetch(e.href,n)}})();var l=o((e=>{var t=Symbol.for(`react.transitional.element`),n=Symbol.for(`react.portal`),r=Symbol.for(`react.fragment`),i=Symbol.for(`react.strict_mode`),a=Symbol.for(`react.profiler`),o=Symbol.for(`react.consumer`),s=Symbol.for(`react.context`),c=Symbol.for(`react.forward_ref`),l=Symbol.for(`react.suspense`),u=Symbol.for(`react.memo`),d=Symbol.for(`react.lazy`),f=Symbol.for(`react.activity`),p=Symbol.iterator;function m(e){return typeof e!=`object`||!e?null:(e=p&&e[p]||e[`@@iterator`],typeof e==`function`?e:null)}var h={isMounted:function(){return!1},enqueueForceUpdate:function(){},enqueueReplaceState:function(){},enqueueSetState:function(){}},g=Object.assign,_={};function v(e,t,n){this.props=e,this.context=t,this.refs=_,this.updater=n||h}v.prototype.isReactComponent={},v.prototype.setState=function(e,t){if(typeof e!=`object`&&typeof e!=`function`&&e!=null)throw Error(`takes an object of state variables to update or a function which returns an object of state variables.`);this.updater.enqueueSetState(this,e,t,`setState`)},v.prototype.forceUpdate=function(e){this.updater.enqueueForceUpdate(this,e,`forceUpdate`)};function y(){}y.prototype=v.prototype;function b(e,t,n){this.props=e,this.context=t,this.refs=_,this.updater=n||h}var x=b.prototype=new y;x.constructor=b,g(x,v.prototype),x.isPureReactComponent=!0;var S=Array.isArray;function C(){}var w={H:null,A:null,T:null,S:null},T=Object.prototype.hasOwnProperty;function E(e,n,r){var i=r.ref;return{$$typeof:t,type:e,key:n,ref:i===void 0?null:i,props:r}}function D(e,t){return E(e.type,t,e.props)}function O(e){return typeof e==`object`&&!!e&&e.$$typeof===t}function k(e){var t={"=":`=0`,":":`=2`};return`$`+e.replace(/[=:]/g,function(e){return t[e]})}var A=/\/+/g;function j(e,t){return typeof e==`object`&&e&&e.key!=null?k(``+e.key):t.toString(36)}function M(e){switch(e.status){case`fulfilled`:return e.value;case`rejected`:throw e.reason;default:switch(typeof e.status==`string`?e.then(C,C):(e.status=`pending`,e.then(function(t){e.status===`pending`&&(e.status=`fulfilled`,e.value=t)},function(t){e.status===`pending`&&(e.status=`rejected`,e.reason=t)})),e.status){case`fulfilled`:return e.value;case`rejected`:throw e.reason}}throw e}function N(e,r,i,a,o){var s=typeof e;(s===`undefined`||s===`boolean`)&&(e=null);var c=!1;if(e===null)c=!0;else switch(s){case`bigint`:case`string`:case`number`:c=!0;break;case`object`:switch(e.$$typeof){case t:case n:c=!0;break;case d:return c=e._init,N(c(e._payload),r,i,a,o)}}if(c)return o=o(e),c=a===``?`.`+j(e,0):a,S(o)?(i=``,c!=null&&(i=c.replace(A,`$&/`)+`/`),N(o,r,i,``,function(e){return e})):o!=null&&(O(o)&&(o=D(o,i+(o.key==null||e&&e.key===o.key?``:(``+o.key).replace(A,`$&/`)+`/`)+c)),r.push(o)),1;c=0;var l=a===``?`.`:a+`:`;if(S(e))for(var u=0;u<e.length;u++)a=e[u],s=l+j(a,u),c+=N(a,r,i,s,o);else if(u=m(e),typeof u==`function`)for(e=u.call(e),u=0;!(a=e.next()).done;)a=a.value,s=l+j(a,u++),c+=N(a,r,i,s,o);else if(s===`object`){if(typeof e.then==`function`)return N(M(e),r,i,a,o);throw r=String(e),Error(`Objects are not valid as a React child (found: `+(r===`[object Object]`?`object with keys {`+Object.keys(e).join(`, `)+`}`:r)+`). If you meant to render a collection of children, use an array instead.`)}return c}function ee(e,t,n){if(e==null)return e;var r=[],i=0;return N(e,r,``,``,function(e){return t.call(n,e,i++)}),r}function P(e){if(e._status===-1){var t=e._result;t=t(),t.then(function(t){(e._status===0||e._status===-1)&&(e._status=1,e._result=t)},function(t){(e._status===0||e._status===-1)&&(e._status=2,e._result=t)}),e._status===-1&&(e._status=0,e._result=t)}if(e._status===1)return e._result.default;throw e._result}var F=typeof reportError==`function`?reportError:function(e){if(typeof window==`object`&&typeof window.ErrorEvent==`function`){var t=new window.ErrorEvent(`error`,{bubbles:!0,cancelable:!0,message:typeof e==`object`&&e&&typeof e.message==`string`?String(e.message):String(e),error:e});if(!window.dispatchEvent(t))return}else if(typeof process==`object`&&typeof process.emit==`function`){process.emit(`uncaughtException`,e);return}console.error(e)},I={map:ee,forEach:function(e,t,n){ee(e,function(){t.apply(this,arguments)},n)},count:function(e){var t=0;return ee(e,function(){t++}),t},toArray:function(e){return ee(e,function(e){return e})||[]},only:function(e){if(!O(e))throw Error(`React.Children.only expected to receive a single React element child.`);return e}};e.Activity=f,e.Children=I,e.Component=v,e.Fragment=r,e.Profiler=a,e.PureComponent=b,e.StrictMode=i,e.Suspense=l,e.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE=w,e.__COMPILER_RUNTIME={__proto__:null,c:function(e){return w.H.useMemoCache(e)}},e.cache=function(e){return function(){return e.apply(null,arguments)}},e.cacheSignal=function(){return null},e.cloneElement=function(e,t,n){if(e==null)throw Error(`The argument must be a React element, but you passed `+e+`.`);var r=g({},e.props),i=e.key;if(t!=null)for(a in t.key!==void 0&&(i=``+t.key),t)!T.call(t,a)||a===`key`||a===`__self`||a===`__source`||a===`ref`&&t.ref===void 0||(r[a]=t[a]);var a=arguments.length-2;if(a===1)r.children=n;else if(1<a){for(var o=Array(a),s=0;s<a;s++)o[s]=arguments[s+2];r.children=o}return E(e.type,i,r)},e.createContext=function(e){return e={$$typeof:s,_currentValue:e,_currentValue2:e,_threadCount:0,Provider:null,Consumer:null},e.Provider=e,e.Consumer={$$typeof:o,_context:e},e},e.createElement=function(e,t,n){var r,i={},a=null;if(t!=null)for(r in t.key!==void 0&&(a=``+t.key),t)T.call(t,r)&&r!==`key`&&r!==`__self`&&r!==`__source`&&(i[r]=t[r]);var o=arguments.length-2;if(o===1)i.children=n;else if(1<o){for(var s=Array(o),c=0;c<o;c++)s[c]=arguments[c+2];i.children=s}if(e&&e.defaultProps)for(r in o=e.defaultProps,o)i[r]===void 0&&(i[r]=o[r]);return E(e,a,i)},e.createRef=function(){return{current:null}},e.forwardRef=function(e){return{$$typeof:c,render:e}},e.isValidElement=O,e.lazy=function(e){return{$$typeof:d,_payload:{_status:-1,_result:e},_init:P}},e.memo=function(e,t){return{$$typeof:u,type:e,compare:t===void 0?null:t}},e.startTransition=function(e){var t=w.T,n={};w.T=n;try{var r=e(),i=w.S;i!==null&&i(n,r),typeof r==`object`&&r&&typeof r.then==`function`&&r.then(C,F)}catch(e){F(e)}finally{t!==null&&n.types!==null&&(t.types=n.types),w.T=t}},e.unstable_useCacheRefresh=function(){return w.H.useCacheRefresh()},e.use=function(e){return w.H.use(e)},e.useActionState=function(e,t,n){return w.H.useActionState(e,t,n)},e.useCallback=function(e,t){return w.H.useCallback(e,t)},e.useContext=function(e){return w.H.useContext(e)},e.useDebugValue=function(){},e.useDeferredValue=function(e,t){return w.H.useDeferredValue(e,t)},e.useEffect=function(e,t){return w.H.useEffect(e,t)},e.useEffectEvent=function(e){return w.H.useEffectEvent(e)},e.useId=function(){return w.H.useId()},e.useImperativeHandle=function(e,t,n){return w.H.useImperativeHandle(e,t,n)},e.useInsertionEffect=function(e,t){return w.H.useInsertionEffect(e,t)},e.useLayoutEffect=function(e,t){return w.H.useLayoutEffect(e,t)},e.useMemo=function(e,t){return w.H.useMemo(e,t)},e.useOptimistic=function(e,t){return w.H.useOptimistic(e,t)},e.useReducer=function(e,t,n){return w.H.useReducer(e,t,n)},e.useRef=function(e){return w.H.useRef(e)},e.useState=function(e){return w.H.useState(e)},e.useSyncExternalStore=function(e,t,n){return w.H.useSyncExternalStore(e,t,n)},e.useTransition=function(){return w.H.useTransition()},e.version=`19.2.0`})),u=o(((e,t)=>{t.exports=l()})),d=o((e=>{function t(e,t){var n=e.length;e.push(t);a:for(;0<n;){var r=n-1>>>1,a=e[r];if(0<i(a,t))e[r]=t,e[n]=a,n=r;else break a}}function n(e){return e.length===0?null:e[0]}function r(e){if(e.length===0)return null;var t=e[0],n=e.pop();if(n!==t){e[0]=n;a:for(var r=0,a=e.length,o=a>>>1;r<o;){var s=2*(r+1)-1,c=e[s],l=s+1,u=e[l];if(0>i(c,n))l<a&&0>i(u,c)?(e[r]=u,e[l]=n,r=l):(e[r]=c,e[s]=n,r=s);else if(l<a&&0>i(u,n))e[r]=u,e[l]=n,r=l;else break a}}return t}function i(e,t){var n=e.sortIndex-t.sortIndex;return n===0?e.id-t.id:n}if(e.unstable_now=void 0,typeof performance==`object`&&typeof performance.now==`function`){var a=performance;e.unstable_now=function(){return a.now()}}else{var o=Date,s=o.now();e.unstable_now=function(){return o.now()-s}}var c=[],l=[],u=1,d=null,f=3,p=!1,m=!1,h=!1,g=!1,_=typeof setTimeout==`function`?setTimeout:null,v=typeof clearTimeout==`function`?clearTimeout:null,y=typeof setImmediate<`u`?setImmediate:null;function b(e){for(var i=n(l);i!==null;){if(i.callback===null)r(l);else if(i.startTime<=e)r(l),i.sortIndex=i.expirationTime,t(c,i);else break;i=n(l)}}function x(e){if(h=!1,b(e),!m)if(n(c)!==null)m=!0,S||(S=!0,O());else{var t=n(l);t!==null&&j(x,t.startTime-e)}}var S=!1,C=-1,w=5,T=-1;function E(){return g?!0:!(e.unstable_now()-T<w)}function D(){if(g=!1,S){var t=e.unstable_now();T=t;var i=!0;try{a:{m=!1,h&&(h=!1,v(C),C=-1),p=!0;var a=f;try{b:{for(b(t),d=n(c);d!==null&&!(d.expirationTime>t&&E());){var o=d.callback;if(typeof o==`function`){d.callback=null,f=d.priorityLevel;var s=o(d.expirationTime<=t);if(t=e.unstable_now(),typeof s==`function`){d.callback=s,b(t),i=!0;break b}d===n(c)&&r(c),b(t)}else r(c);d=n(c)}if(d!==null)i=!0;else{var u=n(l);u!==null&&j(x,u.startTime-t),i=!1}}break a}finally{d=null,f=a,p=!1}i=void 0}}finally{i?O():S=!1}}}var O;if(typeof y==`function`)O=function(){y(D)};else if(typeof MessageChannel<`u`){var k=new MessageChannel,A=k.port2;k.port1.onmessage=D,O=function(){A.postMessage(null)}}else O=function(){_(D,0)};function j(t,n){C=_(function(){t(e.unstable_now())},n)}e.unstable_IdlePriority=5,e.unstable_ImmediatePriority=1,e.unstable_LowPriority=4,e.unstable_NormalPriority=3,e.unstable_Profiling=null,e.unstable_UserBlockingPriority=2,e.unstable_cancelCallback=function(e){e.callback=null},e.unstable_forceFrameRate=function(e){0>e||125<e?console.error(`forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported`):w=0<e?Math.floor(1e3/e):5},e.unstable_getCurrentPriorityLevel=function(){return f},e.unstable_next=function(e){switch(f){case 1:case 2:case 3:var t=3;break;default:t=f}var n=f;f=t;try{return e()}finally{f=n}},e.unstable_requestPaint=function(){g=!0},e.unstable_runWithPriority=function(e,t){switch(e){case 1:case 2:case 3:case 4:case 5:break;default:e=3}var n=f;f=e;try{return t()}finally{f=n}},e.unstable_scheduleCallback=function(r,i,a){var o=e.unstable_now();switch(typeof a==`object`&&a?(a=a.delay,a=typeof a==`number`&&0<a?o+a:o):a=o,r){case 1:var s=-1;break;case 2:s=250;break;case 5:s=1073741823;break;case 4:s=1e4;break;default:s=5e3}return s=a+s,r={id:u++,callback:i,priorityLevel:r,startTime:a,expirationTime:s,sortIndex:-1},a>o?(r.sortIndex=a,t(l,r),n(c)===null&&r===n(l)&&(h?(v(C),C=-1):h=!0,j(x,a-o))):(r.sortIndex=s,t(c,r),m||p||(m=!0,S||(S=!0,O()))),r},e.unstable_shouldYield=E,e.unstable_wrapCallback=function(e){var t=f;return function(){var n=f;f=t;try{return e.apply(this,arguments)}finally{f=n}}}})),f=o(((e,t)=>{t.exports=d()})),p=o((e=>{var t=u();function n(e){var t=`https://react.dev/errors/`+e;if(1<arguments.length){t+=`?args[]=`+encodeURIComponent(arguments[1]);for(var n=2;n<arguments.length;n++)t+=`&args[]=`+encodeURIComponent(arguments[n])}return`Minified React error #`+e+`; visit `+t+` for the full message or use the non-minified dev environment for full errors and additional helpful warnings.`}function r(){}var i={d:{f:r,r:function(){throw Error(n(522))},D:r,C:r,L:r,m:r,X:r,S:r,M:r},p:0,findDOMNode:null},a=Symbol.for(`react.portal`);function o(e,t,n){var r=3<arguments.length&&arguments[3]!==void 0?arguments[3]:null;return{$$typeof:a,key:r==null?null:``+r,children:e,containerInfo:t,implementation:n}}var s=t.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE;function c(e,t){if(e===`font`)return``;if(typeof t==`string`)return t===`use-credentials`?t:``}e.__DOM_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE=i,e.createPortal=function(e,t){var r=2<arguments.length&&arguments[2]!==void 0?arguments[2]:null;if(!t||t.nodeType!==1&&t.nodeType!==9&&t.nodeType!==11)throw Error(n(299));return o(e,t,null,r)},e.flushSync=function(e){var t=s.T,n=i.p;try{if(s.T=null,i.p=2,e)return e()}finally{s.T=t,i.p=n,i.d.f()}},e.preconnect=function(e,t){typeof e==`string`&&(t?(t=t.crossOrigin,t=typeof t==`string`?t===`use-credentials`?t:``:void 0):t=null,i.d.C(e,t))},e.prefetchDNS=function(e){typeof e==`string`&&i.d.D(e)},e.preinit=function(e,t){if(typeof e==`string`&&t&&typeof t.as==`string`){var n=t.as,r=c(n,t.crossOrigin),a=typeof t.integrity==`string`?t.integrity:void 0,o=typeof t.fetchPriority==`string`?t.fetchPriority:void 0;n===`style`?i.d.S(e,typeof t.precedence==`string`?t.precedence:void 0,{crossOrigin:r,integrity:a,fetchPriority:o}):n===`script`&&i.d.X(e,{crossOrigin:r,integrity:a,fetchPriority:o,nonce:typeof t.nonce==`string`?t.nonce:void 0})}},e.preinitModule=function(e,t){if(typeof e==`string`)if(typeof t==`object`&&t){if(t.as==null||t.as===`script`){var n=c(t.as,t.crossOrigin);i.d.M(e,{crossOrigin:n,integrity:typeof t.integrity==`string`?t.integrity:void 0,nonce:typeof t.nonce==`string`?t.nonce:void 0})}}else t??i.d.M(e)},e.preload=function(e,t){if(typeof e==`string`&&typeof t==`object`&&t&&typeof t.as==`string`){var n=t.as,r=c(n,t.crossOrigin);i.d.L(e,n,{crossOrigin:r,integrity:typeof t.integrity==`string`?t.integrity:void 0,nonce:typeof t.nonce==`string`?t.nonce:void 0,type:typeof t.type==`string`?t.type:void 0,fetchPriority:typeof t.fetchPriority==`string`?t.fetchPriority:void 0,referrerPolicy:typeof t.referrerPolicy==`string`?t.referrerPolicy:void 0,imageSrcSet:typeof t.imageSrcSet==`string`?t.imageSrcSet:void 0,imageSizes:typeof t.imageSizes==`string`?t.imageSizes:void 0,media:typeof t.media==`string`?t.media:void 0})}},e.preloadModule=function(e,t){if(typeof e==`string`)if(t){var n=c(t.as,t.crossOrigin);i.d.m(e,{as:typeof t.as==`string`&&t.as!==`script`?t.as:void 0,crossOrigin:n,integrity:typeof t.integrity==`string`?t.integrity:void 0})}else i.d.m(e)},e.requestFormReset=function(e){i.d.r(e)},e.unstable_batchedUpdates=function(e,t){return e(t)},e.useFormState=function(e,t,n){return s.H.useFormState(e,t,n)},e.useFormStatus=function(){return s.H.useHostTransitionStatus()},e.version=`19.2.0`})),m=o(((e,t)=>{function n(){if(!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__>`u`||typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE!=`function`))try{__REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(n)}catch(e){console.error(e)}}n(),t.exports=p()})),h=o((e=>{var t=f(),n=u(),r=m();function i(e){var t=`https://react.dev/errors/`+e;if(1<arguments.length){t+=`?args[]=`+encodeURIComponent(arguments[1]);for(var n=2;n<arguments.length;n++)t+=`&args[]=`+encodeURIComponent(arguments[n])}return`Minified React error #`+e+`; visit `+t+` for the full message or use the non-minified dev environment for full errors and additional helpful warnings.`}function a(e){return!(!e||e.nodeType!==1&&e.nodeType!==9&&e.nodeType!==11)}function o(e){var t=e,n=e;if(e.alternate)for(;t.return;)t=t.return;else{e=t;do t=e,t.flags&4098&&(n=t.return),e=t.return;while(e)}return t.tag===3?n:null}function s(e){if(e.tag===13){var t=e.memoizedState;if(t===null&&(e=e.alternate,e!==null&&(t=e.memoizedState)),t!==null)return t.dehydrated}return null}function c(e){if(e.tag===31){var t=e.memoizedState;if(t===null&&(e=e.alternate,e!==null&&(t=e.memoizedState)),t!==null)return t.dehydrated}return null}function l(e){if(o(e)!==e)throw Error(i(188))}function d(e){var t=e.alternate;if(!t){if(t=o(e),t===null)throw Error(i(188));return t===e?e:null}for(var n=e,r=t;;){var a=n.return;if(a===null)break;var s=a.alternate;if(s===null){if(r=a.return,r!==null){n=r;continue}break}if(a.child===s.child){for(s=a.child;s;){if(s===n)return l(a),e;if(s===r)return l(a),t;s=s.sibling}throw Error(i(188))}if(n.return!==r.return)n=a,r=s;else{for(var c=!1,u=a.child;u;){if(u===n){c=!0,n=a,r=s;break}if(u===r){c=!0,r=a,n=s;break}u=u.sibling}if(!c){for(u=s.child;u;){if(u===n){c=!0,n=s,r=a;break}if(u===r){c=!0,r=s,n=a;break}u=u.sibling}if(!c)throw Error(i(189))}}if(n.alternate!==r)throw Error(i(190))}if(n.tag!==3)throw Error(i(188));return n.stateNode.current===n?e:t}function p(e){var t=e.tag;if(t===5||t===26||t===27||t===6)return e;for(e=e.child;e!==null;){if(t=p(e),t!==null)return t;e=e.sibling}return null}var h=Object.assign,g=Symbol.for(`react.element`),_=Symbol.for(`react.transitional.element`),v=Symbol.for(`react.portal`),y=Symbol.for(`react.fragment`),b=Symbol.for(`react.strict_mode`),x=Symbol.for(`react.profiler`),S=Symbol.for(`react.consumer`),C=Symbol.for(`react.context`),w=Symbol.for(`react.forward_ref`),T=Symbol.for(`react.suspense`),E=Symbol.for(`react.suspense_list`),D=Symbol.for(`react.memo`),O=Symbol.for(`react.lazy`),k=Symbol.for(`react.activity`),A=Symbol.for(`react.memo_cache_sentinel`),j=Symbol.iterator;function M(e){return typeof e!=`object`||!e?null:(e=j&&e[j]||e[`@@iterator`],typeof e==`function`?e:null)}var N=Symbol.for(`react.client.reference`);function ee(e){if(e==null)return null;if(typeof e==`function`)return e.$$typeof===N?null:e.displayName||e.name||null;if(typeof e==`string`)return e;switch(e){case y:return`Fragment`;case x:return`Profiler`;case b:return`StrictMode`;case T:return`Suspense`;case E:return`SuspenseList`;case k:return`Activity`}if(typeof e==`object`)switch(e.$$typeof){case v:return`Portal`;case C:return e.displayName||`Context`;case S:return(e._context.displayName||`Context`)+`.Consumer`;case w:var t=e.render;return e=e.displayName,e||=(e=t.displayName||t.name||``,e===``?`ForwardRef`:`ForwardRef(`+e+`)`),e;case D:return t=e.displayName||null,t===null?ee(e.type)||`Memo`:t;case O:t=e._payload,e=e._init;try{return ee(e(t))}catch{}}return null}var P=Array.isArray,F=n.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE,I=r.__DOM_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE,te={pending:!1,data:null,method:null,action:null},ne=[],re=-1;function ie(e){return{current:e}}function ae(e){0>re||(e.current=ne[re],ne[re]=null,re--)}function L(e,t){re++,ne[re]=e.current,e.current=t}var oe=ie(null),se=ie(null),ce=ie(null),le=ie(null);function ue(e,t){switch(L(ce,t),L(se,e),L(oe,null),t.nodeType){case 9:case 11:e=(e=t.documentElement)&&(e=e.namespaceURI)?Jd(e):0;break;default:if(e=t.tagName,t=t.namespaceURI)t=Jd(t),e=Yd(t,e);else switch(e){case`svg`:e=1;break;case`math`:e=2;break;default:e=0}}ae(oe),L(oe,e)}function de(){ae(oe),ae(se),ae(ce)}function fe(e){e.memoizedState!==null&&L(le,e);var t=oe.current,n=Yd(t,e.type);t!==n&&(L(se,e),L(oe,n))}function pe(e){se.current===e&&(ae(oe),ae(se)),le.current===e&&(ae(le),ap._currentValue=te)}var me,he;function ge(e){if(me===void 0)try{throw Error()}catch(e){var t=e.stack.trim().match(/\n( *(at )?)/);me=t&&t[1]||``,he=-1<e.stack.indexOf(`
    at`)?` (<anonymous>)`:-1<e.stack.indexOf(`@`)?`@unknown:0:0`:``}return`
`+me+e+he}var _e=!1;function ve(e,t){if(!e||_e)return``;_e=!0;var n=Error.prepareStackTrace;Error.prepareStackTrace=void 0;try{var r={DetermineComponentFrameRoot:function(){try{if(t){var n=function(){throw Error()};if(Object.defineProperty(n.prototype,`props`,{set:function(){throw Error()}}),typeof Reflect==`object`&&Reflect.construct){try{Reflect.construct(n,[])}catch(e){var r=e}Reflect.construct(e,[],n)}else{try{n.call()}catch(e){r=e}e.call(n.prototype)}}else{try{throw Error()}catch(e){r=e}(n=e())&&typeof n.catch==`function`&&n.catch(function(){})}}catch(e){if(e&&r&&typeof e.stack==`string`)return[e.stack,r.stack]}return[null,null]}};r.DetermineComponentFrameRoot.displayName=`DetermineComponentFrameRoot`;var i=Object.getOwnPropertyDescriptor(r.DetermineComponentFrameRoot,`name`);i&&i.configurable&&Object.defineProperty(r.DetermineComponentFrameRoot,`name`,{value:`DetermineComponentFrameRoot`});var a=r.DetermineComponentFrameRoot(),o=a[0],s=a[1];if(o&&s){var c=o.split(`
`),l=s.split(`
`);for(i=r=0;r<c.length&&!c[r].includes(`DetermineComponentFrameRoot`);)r++;for(;i<l.length&&!l[i].includes(`DetermineComponentFrameRoot`);)i++;if(r===c.length||i===l.length)for(r=c.length-1,i=l.length-1;1<=r&&0<=i&&c[r]!==l[i];)i--;for(;1<=r&&0<=i;r--,i--)if(c[r]!==l[i]){if(r!==1||i!==1)do if(r--,i--,0>i||c[r]!==l[i]){var u=`
`+c[r].replace(` at new `,` at `);return e.displayName&&u.includes(`<anonymous>`)&&(u=u.replace(`<anonymous>`,e.displayName)),u}while(1<=r&&0<=i);break}}}finally{_e=!1,Error.prepareStackTrace=n}return(n=e?e.displayName||e.name:``)?ge(n):``}function ye(e,t){switch(e.tag){case 26:case 27:case 5:return ge(e.type);case 16:return ge(`Lazy`);case 13:return e.child!==t&&t!==null?ge(`Suspense Fallback`):ge(`Suspense`);case 19:return ge(`SuspenseList`);case 0:case 15:return ve(e.type,!1);case 11:return ve(e.type.render,!1);case 1:return ve(e.type,!0);case 31:return ge(`Activity`);default:return``}}function be(e){try{var t=``,n=null;do t+=ye(e,n),n=e,e=e.return;while(e);return t}catch(e){return`
Error generating stack: `+e.message+`
`+e.stack}}var xe=Object.prototype.hasOwnProperty,Se=t.unstable_scheduleCallback,Ce=t.unstable_cancelCallback,we=t.unstable_shouldYield,Te=t.unstable_requestPaint,Ee=t.unstable_now,De=t.unstable_getCurrentPriorityLevel,Oe=t.unstable_ImmediatePriority,ke=t.unstable_UserBlockingPriority,Ae=t.unstable_NormalPriority,je=t.unstable_LowPriority,Me=t.unstable_IdlePriority,Ne=t.log,Pe=t.unstable_setDisableYieldValue,Fe=null,Ie=null;function Le(e){if(typeof Ne==`function`&&Pe(e),Ie&&typeof Ie.setStrictMode==`function`)try{Ie.setStrictMode(Fe,e)}catch{}}var Re=Math.clz32?Math.clz32:Ve,ze=Math.log,Be=Math.LN2;function Ve(e){return e>>>=0,e===0?32:31-(ze(e)/Be|0)|0}var He=256,Ue=262144,We=4194304;function Ge(e){var t=e&42;if(t!==0)return t;switch(e&-e){case 1:return 1;case 2:return 2;case 4:return 4;case 8:return 8;case 16:return 16;case 32:return 32;case 64:return 64;case 128:return 128;case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:return e&261888;case 262144:case 524288:case 1048576:case 2097152:return e&3932160;case 4194304:case 8388608:case 16777216:case 33554432:return e&62914560;case 67108864:return 67108864;case 134217728:return 134217728;case 268435456:return 268435456;case 536870912:return 536870912;case 1073741824:return 0;default:return e}}function Ke(e,t,n){var r=e.pendingLanes;if(r===0)return 0;var i=0,a=e.suspendedLanes,o=e.pingedLanes;e=e.warmLanes;var s=r&134217727;return s===0?(s=r&~a,s===0?o===0?n||(n=r&~e,n!==0&&(i=Ge(n))):i=Ge(o):i=Ge(s)):(r=s&~a,r===0?(o&=s,o===0?n||(n=s&~e,n!==0&&(i=Ge(n))):i=Ge(o)):i=Ge(r)),i===0?0:t!==0&&t!==i&&(t&a)===0&&(a=i&-i,n=t&-t,a>=n||a===32&&n&4194048)?t:i}function qe(e,t){return(e.pendingLanes&~(e.suspendedLanes&~e.pingedLanes)&t)===0}function Je(e,t){switch(e){case 1:case 2:case 4:case 8:case 64:return t+250;case 16:case 32:case 128:case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:case 262144:case 524288:case 1048576:case 2097152:return t+5e3;case 4194304:case 8388608:case 16777216:case 33554432:return-1;case 67108864:case 134217728:case 268435456:case 536870912:case 1073741824:return-1;default:return-1}}function Ye(){var e=We;return We<<=1,!(We&62914560)&&(We=4194304),e}function Xe(e){for(var t=[],n=0;31>n;n++)t.push(e);return t}function Ze(e,t){e.pendingLanes|=t,t!==268435456&&(e.suspendedLanes=0,e.pingedLanes=0,e.warmLanes=0)}function Qe(e,t,n,r,i,a){var o=e.pendingLanes;e.pendingLanes=n,e.suspendedLanes=0,e.pingedLanes=0,e.warmLanes=0,e.expiredLanes&=n,e.entangledLanes&=n,e.errorRecoveryDisabledLanes&=n,e.shellSuspendCounter=0;var s=e.entanglements,c=e.expirationTimes,l=e.hiddenUpdates;for(n=o&~n;0<n;){var u=31-Re(n),d=1<<u;s[u]=0,c[u]=-1;var f=l[u];if(f!==null)for(l[u]=null,u=0;u<f.length;u++){var p=f[u];p!==null&&(p.lane&=-536870913)}n&=~d}r!==0&&$e(e,r,0),a!==0&&i===0&&e.tag!==0&&(e.suspendedLanes|=a&~(o&~t))}function $e(e,t,n){e.pendingLanes|=t,e.suspendedLanes&=~t;var r=31-Re(t);e.entangledLanes|=t,e.entanglements[r]=e.entanglements[r]|1073741824|n&261930}function et(e,t){var n=e.entangledLanes|=t;for(e=e.entanglements;n;){var r=31-Re(n),i=1<<r;i&t|e[r]&t&&(e[r]|=t),n&=~i}}function tt(e,t){var n=t&-t;return n=n&42?1:nt(n),(n&(e.suspendedLanes|t))===0?n:0}function nt(e){switch(e){case 2:e=1;break;case 8:e=4;break;case 32:e=16;break;case 256:case 512:case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:case 131072:case 262144:case 524288:case 1048576:case 2097152:case 4194304:case 8388608:case 16777216:case 33554432:e=128;break;case 268435456:e=134217728;break;default:e=0}return e}function rt(e){return e&=-e,2<e?8<e?e&134217727?32:268435456:8:2}function it(){var e=I.p;return e===0?(e=window.event,e===void 0?32:xp(e.type)):e}function at(e,t){var n=I.p;try{return I.p=e,t()}finally{I.p=n}}var ot=Math.random().toString(36).slice(2),st=`__reactFiber$`+ot,ct=`__reactProps$`+ot,lt=`__reactContainer$`+ot,ut=`__reactEvents$`+ot,dt=`__reactListeners$`+ot,ft=`__reactHandles$`+ot,pt=`__reactResources$`+ot,mt=`__reactMarker$`+ot;function ht(e){delete e[st],delete e[ct],delete e[ut],delete e[dt],delete e[ft]}function gt(e){var t=e[st];if(t)return t;for(var n=e.parentNode;n;){if(t=n[lt]||n[st]){if(n=t.alternate,t.child!==null||n!==null&&n.child!==null)for(e=vf(e);e!==null;){if(n=e[st])return n;e=vf(e)}return t}e=n,n=e.parentNode}return null}function _t(e){if(e=e[st]||e[lt]){var t=e.tag;if(t===5||t===6||t===13||t===31||t===26||t===27||t===3)return e}return null}function vt(e){var t=e.tag;if(t===5||t===26||t===27||t===6)return e.stateNode;throw Error(i(33))}function yt(e){var t=e[pt];return t||=e[pt]={hoistableStyles:new Map,hoistableScripts:new Map},t}function bt(e){e[mt]=!0}var xt=new Set,St={};function Ct(e,t){wt(e,t),wt(e+`Capture`,t)}function wt(e,t){for(St[e]=t,e=0;e<t.length;e++)xt.add(t[e])}var Tt=RegExp(`^[:A-Z_a-z\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD][:A-Z_a-z\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD\\-.0-9\\u00B7\\u0300-\\u036F\\u203F-\\u2040]*$`),Et={},Dt={};function Ot(e){return xe.call(Dt,e)?!0:xe.call(Et,e)?!1:Tt.test(e)?Dt[e]=!0:(Et[e]=!0,!1)}function kt(e,t,n){if(Ot(t))if(n===null)e.removeAttribute(t);else{switch(typeof n){case`undefined`:case`function`:case`symbol`:e.removeAttribute(t);return;case`boolean`:var r=t.toLowerCase().slice(0,5);if(r!==`data-`&&r!==`aria-`){e.removeAttribute(t);return}}e.setAttribute(t,``+n)}}function At(e,t,n){if(n===null)e.removeAttribute(t);else{switch(typeof n){case`undefined`:case`function`:case`symbol`:case`boolean`:e.removeAttribute(t);return}e.setAttribute(t,``+n)}}function jt(e,t,n,r){if(r===null)e.removeAttribute(n);else{switch(typeof r){case`undefined`:case`function`:case`symbol`:case`boolean`:e.removeAttribute(n);return}e.setAttributeNS(t,n,``+r)}}function Mt(e){switch(typeof e){case`bigint`:case`boolean`:case`number`:case`string`:case`undefined`:return e;case`object`:return e;default:return``}}function Nt(e){var t=e.type;return(e=e.nodeName)&&e.toLowerCase()===`input`&&(t===`checkbox`||t===`radio`)}function Pt(e,t,n){var r=Object.getOwnPropertyDescriptor(e.constructor.prototype,t);if(!e.hasOwnProperty(t)&&r!==void 0&&typeof r.get==`function`&&typeof r.set==`function`){var i=r.get,a=r.set;return Object.defineProperty(e,t,{configurable:!0,get:function(){return i.call(this)},set:function(e){n=``+e,a.call(this,e)}}),Object.defineProperty(e,t,{enumerable:r.enumerable}),{getValue:function(){return n},setValue:function(e){n=``+e},stopTracking:function(){e._valueTracker=null,delete e[t]}}}}function Ft(e){if(!e._valueTracker){var t=Nt(e)?`checked`:`value`;e._valueTracker=Pt(e,t,``+e[t])}}function It(e){if(!e)return!1;var t=e._valueTracker;if(!t)return!0;var n=t.getValue(),r=``;return e&&(r=Nt(e)?e.checked?`true`:`false`:e.value),e=r,e===n?!1:(t.setValue(e),!0)}function Lt(e){if(e||=typeof document<`u`?document:void 0,e===void 0)return null;try{return e.activeElement||e.body}catch{return e.body}}var Rt=/[\n"\\]/g;function zt(e){return e.replace(Rt,function(e){return`\\`+e.charCodeAt(0).toString(16)+` `})}function Bt(e,t,n,r,i,a,o,s){e.name=``,o!=null&&typeof o!=`function`&&typeof o!=`symbol`&&typeof o!=`boolean`?e.type=o:e.removeAttribute(`type`),t==null?o!==`submit`&&o!==`reset`||e.removeAttribute(`value`):o===`number`?(t===0&&e.value===``||e.value!=t)&&(e.value=``+Mt(t)):e.value!==``+Mt(t)&&(e.value=``+Mt(t)),t==null?n==null?r!=null&&e.removeAttribute(`value`):Ht(e,o,Mt(n)):Ht(e,o,Mt(t)),i==null&&a!=null&&(e.defaultChecked=!!a),i!=null&&(e.checked=i&&typeof i!=`function`&&typeof i!=`symbol`),s!=null&&typeof s!=`function`&&typeof s!=`symbol`&&typeof s!=`boolean`?e.name=``+Mt(s):e.removeAttribute(`name`)}function Vt(e,t,n,r,i,a,o,s){if(a!=null&&typeof a!=`function`&&typeof a!=`symbol`&&typeof a!=`boolean`&&(e.type=a),t!=null||n!=null){if(!(a!==`submit`&&a!==`reset`||t!=null)){Ft(e);return}n=n==null?``:``+Mt(n),t=t==null?n:``+Mt(t),s||t===e.value||(e.value=t),e.defaultValue=t}r??=i,r=typeof r!=`function`&&typeof r!=`symbol`&&!!r,e.checked=s?e.checked:!!r,e.defaultChecked=!!r,o!=null&&typeof o!=`function`&&typeof o!=`symbol`&&typeof o!=`boolean`&&(e.name=o),Ft(e)}function Ht(e,t,n){t===`number`&&Lt(e.ownerDocument)===e||e.defaultValue===``+n||(e.defaultValue=``+n)}function Ut(e,t,n,r){if(e=e.options,t){t={};for(var i=0;i<n.length;i++)t[`$`+n[i]]=!0;for(n=0;n<e.length;n++)i=t.hasOwnProperty(`$`+e[n].value),e[n].selected!==i&&(e[n].selected=i),i&&r&&(e[n].defaultSelected=!0)}else{for(n=``+Mt(n),t=null,i=0;i<e.length;i++){if(e[i].value===n){e[i].selected=!0,r&&(e[i].defaultSelected=!0);return}t!==null||e[i].disabled||(t=e[i])}t!==null&&(t.selected=!0)}}function Wt(e,t,n){if(t!=null&&(t=``+Mt(t),t!==e.value&&(e.value=t),n==null)){e.defaultValue!==t&&(e.defaultValue=t);return}e.defaultValue=n==null?``:``+Mt(n)}function Gt(e,t,n,r){if(t==null){if(r!=null){if(n!=null)throw Error(i(92));if(P(r)){if(1<r.length)throw Error(i(93));r=r[0]}n=r}n??=``,t=n}n=Mt(t),e.defaultValue=n,r=e.textContent,r===n&&r!==``&&r!==null&&(e.value=r),Ft(e)}function Kt(e,t){if(t){var n=e.firstChild;if(n&&n===e.lastChild&&n.nodeType===3){n.nodeValue=t;return}}e.textContent=t}var qt=new Set(`animationIterationCount aspectRatio borderImageOutset borderImageSlice borderImageWidth boxFlex boxFlexGroup boxOrdinalGroup columnCount columns flex flexGrow flexPositive flexShrink flexNegative flexOrder gridArea gridRow gridRowEnd gridRowSpan gridRowStart gridColumn gridColumnEnd gridColumnSpan gridColumnStart fontWeight lineClamp lineHeight opacity order orphans scale tabSize widows zIndex zoom fillOpacity floodOpacity stopOpacity strokeDasharray strokeDashoffset strokeMiterlimit strokeOpacity strokeWidth MozAnimationIterationCount MozBoxFlex MozBoxFlexGroup MozLineClamp msAnimationIterationCount msFlex msZoom msFlexGrow msFlexNegative msFlexOrder msFlexPositive msFlexShrink msGridColumn msGridColumnSpan msGridRow msGridRowSpan WebkitAnimationIterationCount WebkitBoxFlex WebKitBoxFlexGroup WebkitBoxOrdinalGroup WebkitColumnCount WebkitColumns WebkitFlex WebkitFlexGrow WebkitFlexPositive WebkitFlexShrink WebkitLineClamp`.split(` `));function Jt(e,t,n){var r=t.indexOf(`--`)===0;n==null||typeof n==`boolean`||n===``?r?e.setProperty(t,``):t===`float`?e.cssFloat=``:e[t]=``:r?e.setProperty(t,n):typeof n!=`number`||n===0||qt.has(t)?t===`float`?e.cssFloat=n:e[t]=(``+n).trim():e[t]=n+`px`}function Yt(e,t,n){if(t!=null&&typeof t!=`object`)throw Error(i(62));if(e=e.style,n!=null){for(var r in n)!n.hasOwnProperty(r)||t!=null&&t.hasOwnProperty(r)||(r.indexOf(`--`)===0?e.setProperty(r,``):r===`float`?e.cssFloat=``:e[r]=``);for(var a in t)r=t[a],t.hasOwnProperty(a)&&n[a]!==r&&Jt(e,a,r)}else for(var o in t)t.hasOwnProperty(o)&&Jt(e,o,t[o])}function Xt(e){if(e.indexOf(`-`)===-1)return!1;switch(e){case`annotation-xml`:case`color-profile`:case`font-face`:case`font-face-src`:case`font-face-uri`:case`font-face-format`:case`font-face-name`:case`missing-glyph`:return!1;default:return!0}}var Zt=new Map([[`acceptCharset`,`accept-charset`],[`htmlFor`,`for`],[`httpEquiv`,`http-equiv`],[`crossOrigin`,`crossorigin`],[`accentHeight`,`accent-height`],[`alignmentBaseline`,`alignment-baseline`],[`arabicForm`,`arabic-form`],[`baselineShift`,`baseline-shift`],[`capHeight`,`cap-height`],[`clipPath`,`clip-path`],[`clipRule`,`clip-rule`],[`colorInterpolation`,`color-interpolation`],[`colorInterpolationFilters`,`color-interpolation-filters`],[`colorProfile`,`color-profile`],[`colorRendering`,`color-rendering`],[`dominantBaseline`,`dominant-baseline`],[`enableBackground`,`enable-background`],[`fillOpacity`,`fill-opacity`],[`fillRule`,`fill-rule`],[`floodColor`,`flood-color`],[`floodOpacity`,`flood-opacity`],[`fontFamily`,`font-family`],[`fontSize`,`font-size`],[`fontSizeAdjust`,`font-size-adjust`],[`fontStretch`,`font-stretch`],[`fontStyle`,`font-style`],[`fontVariant`,`font-variant`],[`fontWeight`,`font-weight`],[`glyphName`,`glyph-name`],[`glyphOrientationHorizontal`,`glyph-orientation-horizontal`],[`glyphOrientationVertical`,`glyph-orientation-vertical`],[`horizAdvX`,`horiz-adv-x`],[`horizOriginX`,`horiz-origin-x`],[`imageRendering`,`image-rendering`],[`letterSpacing`,`letter-spacing`],[`lightingColor`,`lighting-color`],[`markerEnd`,`marker-end`],[`markerMid`,`marker-mid`],[`markerStart`,`marker-start`],[`overlinePosition`,`overline-position`],[`overlineThickness`,`overline-thickness`],[`paintOrder`,`paint-order`],[`panose-1`,`panose-1`],[`pointerEvents`,`pointer-events`],[`renderingIntent`,`rendering-intent`],[`shapeRendering`,`shape-rendering`],[`stopColor`,`stop-color`],[`stopOpacity`,`stop-opacity`],[`strikethroughPosition`,`strikethrough-position`],[`strikethroughThickness`,`strikethrough-thickness`],[`strokeDasharray`,`stroke-dasharray`],[`strokeDashoffset`,`stroke-dashoffset`],[`strokeLinecap`,`stroke-linecap`],[`strokeLinejoin`,`stroke-linejoin`],[`strokeMiterlimit`,`stroke-miterlimit`],[`strokeOpacity`,`stroke-opacity`],[`strokeWidth`,`stroke-width`],[`textAnchor`,`text-anchor`],[`textDecoration`,`text-decoration`],[`textRendering`,`text-rendering`],[`transformOrigin`,`transform-origin`],[`underlinePosition`,`underline-position`],[`underlineThickness`,`underline-thickness`],[`unicodeBidi`,`unicode-bidi`],[`unicodeRange`,`unicode-range`],[`unitsPerEm`,`units-per-em`],[`vAlphabetic`,`v-alphabetic`],[`vHanging`,`v-hanging`],[`vIdeographic`,`v-ideographic`],[`vMathematical`,`v-mathematical`],[`vectorEffect`,`vector-effect`],[`vertAdvY`,`vert-adv-y`],[`vertOriginX`,`vert-origin-x`],[`vertOriginY`,`vert-origin-y`],[`wordSpacing`,`word-spacing`],[`writingMode`,`writing-mode`],[`xmlnsXlink`,`xmlns:xlink`],[`xHeight`,`x-height`]]),Qt=/^[\u0000-\u001F ]*j[\r\n\t]*a[\r\n\t]*v[\r\n\t]*a[\r\n\t]*s[\r\n\t]*c[\r\n\t]*r[\r\n\t]*i[\r\n\t]*p[\r\n\t]*t[\r\n\t]*:/i;function $t(e){return Qt.test(``+e)?`javascript:throw new Error('React has blocked a javascript: URL as a security precaution.')`:e}function en(){}var tn=null;function nn(e){return e=e.target||e.srcElement||window,e.correspondingUseElement&&(e=e.correspondingUseElement),e.nodeType===3?e.parentNode:e}var rn=null,an=null;function on(e){var t=_t(e);if(t&&(e=t.stateNode)){var n=e[ct]||null;a:switch(e=t.stateNode,t.type){case`input`:if(Bt(e,n.value,n.defaultValue,n.defaultValue,n.checked,n.defaultChecked,n.type,n.name),t=n.name,n.type===`radio`&&t!=null){for(n=e;n.parentNode;)n=n.parentNode;for(n=n.querySelectorAll(`input[name="`+zt(``+t)+`"][type="radio"]`),t=0;t<n.length;t++){var r=n[t];if(r!==e&&r.form===e.form){var a=r[ct]||null;if(!a)throw Error(i(90));Bt(r,a.value,a.defaultValue,a.defaultValue,a.checked,a.defaultChecked,a.type,a.name)}}for(t=0;t<n.length;t++)r=n[t],r.form===e.form&&It(r)}break a;case`textarea`:Wt(e,n.value,n.defaultValue);break a;case`select`:t=n.value,t!=null&&Ut(e,!!n.multiple,t,!1)}}}var sn=!1;function cn(e,t,n){if(sn)return e(t,n);sn=!0;try{return e(t)}finally{if(sn=!1,(rn!==null||an!==null)&&(Eu(),rn&&(t=rn,e=an,an=rn=null,on(t),e)))for(t=0;t<e.length;t++)on(e[t])}}function ln(e,t){var n=e.stateNode;if(n===null)return null;var r=n[ct]||null;if(r===null)return null;n=r[t];a:switch(t){case`onClick`:case`onClickCapture`:case`onDoubleClick`:case`onDoubleClickCapture`:case`onMouseDown`:case`onMouseDownCapture`:case`onMouseMove`:case`onMouseMoveCapture`:case`onMouseUp`:case`onMouseUpCapture`:case`onMouseEnter`:(r=!r.disabled)||(e=e.type,r=!(e===`button`||e===`input`||e===`select`||e===`textarea`)),e=!r;break a;default:e=!1}if(e)return null;if(n&&typeof n!=`function`)throw Error(i(231,t,typeof n));return n}var un=!(typeof window>`u`||window.document===void 0||window.document.createElement===void 0),dn=!1;if(un)try{var fn={};Object.defineProperty(fn,`passive`,{get:function(){dn=!0}}),window.addEventListener(`test`,fn,fn),window.removeEventListener(`test`,fn,fn)}catch{dn=!1}var pn=null,mn=null,hn=null;function gn(){if(hn)return hn;var e,t=mn,n=t.length,r,i=`value`in pn?pn.value:pn.textContent,a=i.length;for(e=0;e<n&&t[e]===i[e];e++);var o=n-e;for(r=1;r<=o&&t[n-r]===i[a-r];r++);return hn=i.slice(e,1<r?1-r:void 0)}function _n(e){var t=e.keyCode;return`charCode`in e?(e=e.charCode,e===0&&t===13&&(e=13)):e=t,e===10&&(e=13),32<=e||e===13?e:0}function vn(){return!0}function yn(){return!1}function bn(e){function t(t,n,r,i,a){for(var o in this._reactName=t,this._targetInst=r,this.type=n,this.nativeEvent=i,this.target=a,this.currentTarget=null,e)e.hasOwnProperty(o)&&(t=e[o],this[o]=t?t(i):i[o]);return this.isDefaultPrevented=(i.defaultPrevented==null?!1===i.returnValue:i.defaultPrevented)?vn:yn,this.isPropagationStopped=yn,this}return h(t.prototype,{preventDefault:function(){this.defaultPrevented=!0;var e=this.nativeEvent;e&&(e.preventDefault?e.preventDefault():typeof e.returnValue!=`unknown`&&(e.returnValue=!1),this.isDefaultPrevented=vn)},stopPropagation:function(){var e=this.nativeEvent;e&&(e.stopPropagation?e.stopPropagation():typeof e.cancelBubble!=`unknown`&&(e.cancelBubble=!0),this.isPropagationStopped=vn)},persist:function(){},isPersistent:vn}),t}var xn={eventPhase:0,bubbles:0,cancelable:0,timeStamp:function(e){return e.timeStamp||Date.now()},defaultPrevented:0,isTrusted:0},Sn=bn(xn),Cn=h({},xn,{view:0,detail:0}),wn=bn(Cn),Tn,En,Dn,On=h({},Cn,{screenX:0,screenY:0,clientX:0,clientY:0,pageX:0,pageY:0,ctrlKey:0,shiftKey:0,altKey:0,metaKey:0,getModifierState:zn,button:0,buttons:0,relatedTarget:function(e){return e.relatedTarget===void 0?e.fromElement===e.srcElement?e.toElement:e.fromElement:e.relatedTarget},movementX:function(e){return`movementX`in e?e.movementX:(e!==Dn&&(Dn&&e.type===`mousemove`?(Tn=e.screenX-Dn.screenX,En=e.screenY-Dn.screenY):En=Tn=0,Dn=e),Tn)},movementY:function(e){return`movementY`in e?e.movementY:En}}),kn=bn(On),An=bn(h({},On,{dataTransfer:0})),jn=bn(h({},Cn,{relatedTarget:0})),Mn=bn(h({},xn,{animationName:0,elapsedTime:0,pseudoElement:0})),Nn=bn(h({},xn,{clipboardData:function(e){return`clipboardData`in e?e.clipboardData:window.clipboardData}})),Pn=bn(h({},xn,{data:0})),Fn={Esc:`Escape`,Spacebar:` `,Left:`ArrowLeft`,Up:`ArrowUp`,Right:`ArrowRight`,Down:`ArrowDown`,Del:`Delete`,Win:`OS`,Menu:`ContextMenu`,Apps:`ContextMenu`,Scroll:`ScrollLock`,MozPrintableKey:`Unidentified`},In={8:`Backspace`,9:`Tab`,12:`Clear`,13:`Enter`,16:`Shift`,17:`Control`,18:`Alt`,19:`Pause`,20:`CapsLock`,27:`Escape`,32:` `,33:`PageUp`,34:`PageDown`,35:`End`,36:`Home`,37:`ArrowLeft`,38:`ArrowUp`,39:`ArrowRight`,40:`ArrowDown`,45:`Insert`,46:`Delete`,112:`F1`,113:`F2`,114:`F3`,115:`F4`,116:`F5`,117:`F6`,118:`F7`,119:`F8`,120:`F9`,121:`F10`,122:`F11`,123:`F12`,144:`NumLock`,145:`ScrollLock`,224:`Meta`},Ln={Alt:`altKey`,Control:`ctrlKey`,Meta:`metaKey`,Shift:`shiftKey`};function Rn(e){var t=this.nativeEvent;return t.getModifierState?t.getModifierState(e):(e=Ln[e])?!!t[e]:!1}function zn(){return Rn}var Bn=bn(h({},Cn,{key:function(e){if(e.key){var t=Fn[e.key]||e.key;if(t!==`Unidentified`)return t}return e.type===`keypress`?(e=_n(e),e===13?`Enter`:String.fromCharCode(e)):e.type===`keydown`||e.type===`keyup`?In[e.keyCode]||`Unidentified`:``},code:0,location:0,ctrlKey:0,shiftKey:0,altKey:0,metaKey:0,repeat:0,locale:0,getModifierState:zn,charCode:function(e){return e.type===`keypress`?_n(e):0},keyCode:function(e){return e.type===`keydown`||e.type===`keyup`?e.keyCode:0},which:function(e){return e.type===`keypress`?_n(e):e.type===`keydown`||e.type===`keyup`?e.keyCode:0}})),Vn=bn(h({},On,{pointerId:0,width:0,height:0,pressure:0,tangentialPressure:0,tiltX:0,tiltY:0,twist:0,pointerType:0,isPrimary:0})),Hn=bn(h({},Cn,{touches:0,targetTouches:0,changedTouches:0,altKey:0,metaKey:0,ctrlKey:0,shiftKey:0,getModifierState:zn})),Un=bn(h({},xn,{propertyName:0,elapsedTime:0,pseudoElement:0})),Wn=bn(h({},On,{deltaX:function(e){return`deltaX`in e?e.deltaX:`wheelDeltaX`in e?-e.wheelDeltaX:0},deltaY:function(e){return`deltaY`in e?e.deltaY:`wheelDeltaY`in e?-e.wheelDeltaY:`wheelDelta`in e?-e.wheelDelta:0},deltaZ:0,deltaMode:0})),Gn=bn(h({},xn,{newState:0,oldState:0})),Kn=[9,13,27,32],qn=un&&`CompositionEvent`in window,Jn=null;un&&`documentMode`in document&&(Jn=document.documentMode);var Yn=un&&`TextEvent`in window&&!Jn,Xn=un&&(!qn||Jn&&8<Jn&&11>=Jn),Zn=` `,Qn=!1;function $n(e,t){switch(e){case`keyup`:return Kn.indexOf(t.keyCode)!==-1;case`keydown`:return t.keyCode!==229;case`keypress`:case`mousedown`:case`focusout`:return!0;default:return!1}}function er(e){return e=e.detail,typeof e==`object`&&`data`in e?e.data:null}var tr=!1;function nr(e,t){switch(e){case`compositionend`:return er(t);case`keypress`:return t.which===32?(Qn=!0,Zn):null;case`textInput`:return e=t.data,e===Zn&&Qn?null:e;default:return null}}function rr(e,t){if(tr)return e===`compositionend`||!qn&&$n(e,t)?(e=gn(),hn=mn=pn=null,tr=!1,e):null;switch(e){case`paste`:return null;case`keypress`:if(!(t.ctrlKey||t.altKey||t.metaKey)||t.ctrlKey&&t.altKey){if(t.char&&1<t.char.length)return t.char;if(t.which)return String.fromCharCode(t.which)}return null;case`compositionend`:return Xn&&t.locale!==`ko`?null:t.data;default:return null}}var ir={color:!0,date:!0,datetime:!0,"datetime-local":!0,email:!0,month:!0,number:!0,password:!0,range:!0,search:!0,tel:!0,text:!0,time:!0,url:!0,week:!0};function ar(e){var t=e&&e.nodeName&&e.nodeName.toLowerCase();return t===`input`?!!ir[e.type]:t===`textarea`}function or(e,t,n,r){rn?an?an.push(r):an=[r]:rn=r,t=Nd(t,`onChange`),0<t.length&&(n=new Sn(`onChange`,`change`,null,n,r),e.push({event:n,listeners:t}))}var sr=null,cr=null;function lr(e){Ed(e,0)}function ur(e){if(It(vt(e)))return e}function dr(e,t){if(e===`change`)return t}var fr=!1;if(un){var pr;if(un){var mr=`oninput`in document;if(!mr){var hr=document.createElement(`div`);hr.setAttribute(`oninput`,`return;`),mr=typeof hr.oninput==`function`}pr=mr}else pr=!1;fr=pr&&(!document.documentMode||9<document.documentMode)}function gr(){sr&&(sr.detachEvent(`onpropertychange`,_r),cr=sr=null)}function _r(e){if(e.propertyName===`value`&&ur(cr)){var t=[];or(t,cr,e,nn(e)),cn(lr,t)}}function vr(e,t,n){e===`focusin`?(gr(),sr=t,cr=n,sr.attachEvent(`onpropertychange`,_r)):e===`focusout`&&gr()}function yr(e){if(e===`selectionchange`||e===`keyup`||e===`keydown`)return ur(cr)}function br(e,t){if(e===`click`)return ur(t)}function xr(e,t){if(e===`input`||e===`change`)return ur(t)}function Sr(e,t){return e===t&&(e!==0||1/e==1/t)||e!==e&&t!==t}var Cr=typeof Object.is==`function`?Object.is:Sr;function wr(e,t){if(Cr(e,t))return!0;if(typeof e!=`object`||!e||typeof t!=`object`||!t)return!1;var n=Object.keys(e),r=Object.keys(t);if(n.length!==r.length)return!1;for(r=0;r<n.length;r++){var i=n[r];if(!xe.call(t,i)||!Cr(e[i],t[i]))return!1}return!0}function Tr(e){for(;e&&e.firstChild;)e=e.firstChild;return e}function Er(e,t){var n=Tr(e);e=0;for(var r;n;){if(n.nodeType===3){if(r=e+n.textContent.length,e<=t&&r>=t)return{node:n,offset:t-e};e=r}a:{for(;n;){if(n.nextSibling){n=n.nextSibling;break a}n=n.parentNode}n=void 0}n=Tr(n)}}function Dr(e,t){return e&&t?e===t?!0:e&&e.nodeType===3?!1:t&&t.nodeType===3?Dr(e,t.parentNode):`contains`in e?e.contains(t):e.compareDocumentPosition?!!(e.compareDocumentPosition(t)&16):!1:!1}function Or(e){e=e!=null&&e.ownerDocument!=null&&e.ownerDocument.defaultView!=null?e.ownerDocument.defaultView:window;for(var t=Lt(e.document);t instanceof e.HTMLIFrameElement;){try{var n=typeof t.contentWindow.location.href==`string`}catch{n=!1}if(n)e=t.contentWindow;else break;t=Lt(e.document)}return t}function kr(e){var t=e&&e.nodeName&&e.nodeName.toLowerCase();return t&&(t===`input`&&(e.type===`text`||e.type===`search`||e.type===`tel`||e.type===`url`||e.type===`password`)||t===`textarea`||e.contentEditable===`true`)}var Ar=un&&`documentMode`in document&&11>=document.documentMode,jr=null,Mr=null,Nr=null,Pr=!1;function Fr(e,t,n){var r=n.window===n?n.document:n.nodeType===9?n:n.ownerDocument;Pr||jr==null||jr!==Lt(r)||(r=jr,`selectionStart`in r&&kr(r)?r={start:r.selectionStart,end:r.selectionEnd}:(r=(r.ownerDocument&&r.ownerDocument.defaultView||window).getSelection(),r={anchorNode:r.anchorNode,anchorOffset:r.anchorOffset,focusNode:r.focusNode,focusOffset:r.focusOffset}),Nr&&wr(Nr,r)||(Nr=r,r=Nd(Mr,`onSelect`),0<r.length&&(t=new Sn(`onSelect`,`select`,null,t,n),e.push({event:t,listeners:r}),t.target=jr)))}function Ir(e,t){var n={};return n[e.toLowerCase()]=t.toLowerCase(),n[`Webkit`+e]=`webkit`+t,n[`Moz`+e]=`moz`+t,n}var Lr={animationend:Ir(`Animation`,`AnimationEnd`),animationiteration:Ir(`Animation`,`AnimationIteration`),animationstart:Ir(`Animation`,`AnimationStart`),transitionrun:Ir(`Transition`,`TransitionRun`),transitionstart:Ir(`Transition`,`TransitionStart`),transitioncancel:Ir(`Transition`,`TransitionCancel`),transitionend:Ir(`Transition`,`TransitionEnd`)},Rr={},zr={};un&&(zr=document.createElement(`div`).style,`AnimationEvent`in window||(delete Lr.animationend.animation,delete Lr.animationiteration.animation,delete Lr.animationstart.animation),`TransitionEvent`in window||delete Lr.transitionend.transition);function Br(e){if(Rr[e])return Rr[e];if(!Lr[e])return e;var t=Lr[e],n;for(n in t)if(t.hasOwnProperty(n)&&n in zr)return Rr[e]=t[n];return e}var Vr=Br(`animationend`),Hr=Br(`animationiteration`),Ur=Br(`animationstart`),Wr=Br(`transitionrun`),Gr=Br(`transitionstart`),Kr=Br(`transitioncancel`),qr=Br(`transitionend`),Jr=new Map,Yr=`abort auxClick beforeToggle cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel`.split(` `);Yr.push(`scrollEnd`);function Xr(e,t){Jr.set(e,t),Ct(t,[e])}var Zr=typeof reportError==`function`?reportError:function(e){if(typeof window==`object`&&typeof window.ErrorEvent==`function`){var t=new window.ErrorEvent(`error`,{bubbles:!0,cancelable:!0,message:typeof e==`object`&&e&&typeof e.message==`string`?String(e.message):String(e),error:e});if(!window.dispatchEvent(t))return}else if(typeof process==`object`&&typeof process.emit==`function`){process.emit(`uncaughtException`,e);return}console.error(e)},Qr=[],$r=0,ei=0;function ti(){for(var e=$r,t=ei=$r=0;t<e;){var n=Qr[t];Qr[t++]=null;var r=Qr[t];Qr[t++]=null;var i=Qr[t];Qr[t++]=null;var a=Qr[t];if(Qr[t++]=null,r!==null&&i!==null){var o=r.pending;o===null?i.next=i:(i.next=o.next,o.next=i),r.pending=i}a!==0&&ai(n,i,a)}}function ni(e,t,n,r){Qr[$r++]=e,Qr[$r++]=t,Qr[$r++]=n,Qr[$r++]=r,ei|=r,e.lanes|=r,e=e.alternate,e!==null&&(e.lanes|=r)}function ri(e,t,n,r){return ni(e,t,n,r),oi(e)}function ii(e,t){return ni(e,null,null,t),oi(e)}function ai(e,t,n){e.lanes|=n;var r=e.alternate;r!==null&&(r.lanes|=n);for(var i=!1,a=e.return;a!==null;)a.childLanes|=n,r=a.alternate,r!==null&&(r.childLanes|=n),a.tag===22&&(e=a.stateNode,e===null||e._visibility&1||(i=!0)),e=a,a=a.return;return e.tag===3?(a=e.stateNode,i&&t!==null&&(i=31-Re(n),e=a.hiddenUpdates,r=e[i],r===null?e[i]=[t]:r.push(t),t.lane=n|536870912),a):null}function oi(e){if(50<_u)throw _u=0,vu=null,Error(i(185));for(var t=e.return;t!==null;)e=t,t=e.return;return e.tag===3?e.stateNode:null}var si={};function ci(e,t,n,r){this.tag=e,this.key=n,this.sibling=this.child=this.return=this.stateNode=this.type=this.elementType=null,this.index=0,this.refCleanup=this.ref=null,this.pendingProps=t,this.dependencies=this.memoizedState=this.updateQueue=this.memoizedProps=null,this.mode=r,this.subtreeFlags=this.flags=0,this.deletions=null,this.childLanes=this.lanes=0,this.alternate=null}function li(e,t,n,r){return new ci(e,t,n,r)}function ui(e){return e=e.prototype,!(!e||!e.isReactComponent)}function di(e,t){var n=e.alternate;return n===null?(n=li(e.tag,t,e.key,e.mode),n.elementType=e.elementType,n.type=e.type,n.stateNode=e.stateNode,n.alternate=e,e.alternate=n):(n.pendingProps=t,n.type=e.type,n.flags=0,n.subtreeFlags=0,n.deletions=null),n.flags=e.flags&65011712,n.childLanes=e.childLanes,n.lanes=e.lanes,n.child=e.child,n.memoizedProps=e.memoizedProps,n.memoizedState=e.memoizedState,n.updateQueue=e.updateQueue,t=e.dependencies,n.dependencies=t===null?null:{lanes:t.lanes,firstContext:t.firstContext},n.sibling=e.sibling,n.index=e.index,n.ref=e.ref,n.refCleanup=e.refCleanup,n}function fi(e,t){e.flags&=65011714;var n=e.alternate;return n===null?(e.childLanes=0,e.lanes=t,e.child=null,e.subtreeFlags=0,e.memoizedProps=null,e.memoizedState=null,e.updateQueue=null,e.dependencies=null,e.stateNode=null):(e.childLanes=n.childLanes,e.lanes=n.lanes,e.child=n.child,e.subtreeFlags=0,e.deletions=null,e.memoizedProps=n.memoizedProps,e.memoizedState=n.memoizedState,e.updateQueue=n.updateQueue,e.type=n.type,t=n.dependencies,e.dependencies=t===null?null:{lanes:t.lanes,firstContext:t.firstContext}),e}function pi(e,t,n,r,a,o){var s=0;if(r=e,typeof e==`function`)ui(e)&&(s=1);else if(typeof e==`string`)s=Xf(e,n,oe.current)?26:e===`html`||e===`head`||e===`body`?27:5;else a:switch(e){case k:return e=li(31,n,t,a),e.elementType=k,e.lanes=o,e;case y:return mi(n.children,a,o,t);case b:s=8,a|=24;break;case x:return e=li(12,n,t,a|2),e.elementType=x,e.lanes=o,e;case T:return e=li(13,n,t,a),e.elementType=T,e.lanes=o,e;case E:return e=li(19,n,t,a),e.elementType=E,e.lanes=o,e;default:if(typeof e==`object`&&e)switch(e.$$typeof){case C:s=10;break a;case S:s=9;break a;case w:s=11;break a;case D:s=14;break a;case O:s=16,r=null;break a}s=29,n=Error(i(130,e===null?`null`:typeof e,``)),r=null}return t=li(s,n,t,a),t.elementType=e,t.type=r,t.lanes=o,t}function mi(e,t,n,r){return e=li(7,e,r,t),e.lanes=n,e}function hi(e,t,n){return e=li(6,e,null,t),e.lanes=n,e}function gi(e){var t=li(18,null,null,0);return t.stateNode=e,t}function _i(e,t,n){return t=li(4,e.children===null?[]:e.children,e.key,t),t.lanes=n,t.stateNode={containerInfo:e.containerInfo,pendingChildren:null,implementation:e.implementation},t}var vi=new WeakMap;function yi(e,t){if(typeof e==`object`&&e){var n=vi.get(e);return n===void 0?(t={value:e,source:t,stack:be(t)},vi.set(e,t),t):n}return{value:e,source:t,stack:be(t)}}var bi=[],xi=0,Si=null,Ci=0,wi=[],Ti=0,Ei=null,Di=1,Oi=``;function ki(e,t){bi[xi++]=Ci,bi[xi++]=Si,Si=e,Ci=t}function Ai(e,t,n){wi[Ti++]=Di,wi[Ti++]=Oi,wi[Ti++]=Ei,Ei=e;var r=Di;e=Oi;var i=32-Re(r)-1;r&=~(1<<i),n+=1;var a=32-Re(t)+i;if(30<a){var o=i-i%5;a=(r&(1<<o)-1).toString(32),r>>=o,i-=o,Di=1<<32-Re(t)+i|n<<i|r,Oi=a+e}else Di=1<<a|n<<i|r,Oi=e}function ji(e){e.return!==null&&(ki(e,1),Ai(e,1,0))}function Mi(e){for(;e===Si;)Si=bi[--xi],bi[xi]=null,Ci=bi[--xi],bi[xi]=null;for(;e===Ei;)Ei=wi[--Ti],wi[Ti]=null,Oi=wi[--Ti],wi[Ti]=null,Di=wi[--Ti],wi[Ti]=null}function Ni(e,t){wi[Ti++]=Di,wi[Ti++]=Oi,wi[Ti++]=Ei,Di=t.id,Oi=t.overflow,Ei=e}var Pi=null,Fi=null,Ii=!1,Li=null,Ri=!1,zi=Error(i(519));function R(e){throw Gi(yi(Error(i(418,1<arguments.length&&arguments[1]!==void 0&&arguments[1]?`text`:`HTML`,``)),e)),zi}function Bi(e){var t=e.stateNode,n=e.type,r=e.memoizedProps;switch(t[st]=e,t[ct]=r,n){case`dialog`:G(`cancel`,t),G(`close`,t);break;case`iframe`:case`object`:case`embed`:G(`load`,t);break;case`video`:case`audio`:for(n=0;n<wd.length;n++)G(wd[n],t);break;case`source`:G(`error`,t);break;case`img`:case`image`:case`link`:G(`error`,t),G(`load`,t);break;case`details`:G(`toggle`,t);break;case`input`:G(`invalid`,t),Vt(t,r.value,r.defaultValue,r.checked,r.defaultChecked,r.type,r.name,!0);break;case`select`:G(`invalid`,t);break;case`textarea`:G(`invalid`,t),Gt(t,r.value,r.defaultValue,r.children)}n=r.children,typeof n!=`string`&&typeof n!=`number`&&typeof n!=`bigint`||t.textContent===``+n||!0===r.suppressHydrationWarning||zd(t.textContent,n)?(r.popover!=null&&(G(`beforetoggle`,t),G(`toggle`,t)),r.onScroll!=null&&G(`scroll`,t),r.onScrollEnd!=null&&G(`scrollend`,t),r.onClick!=null&&(t.onclick=en),t=!0):t=!1,t||R(e,!0)}function Vi(e){for(Pi=e.return;Pi;)switch(Pi.tag){case 5:case 31:case 13:Ri=!1;return;case 27:case 3:Ri=!0;return;default:Pi=Pi.return}}function Hi(e){if(e!==Pi)return!1;if(!Ii)return Vi(e),Ii=!0,!1;var t=e.tag,n;if((n=t!==3&&t!==27)&&((n=t===5)&&(n=e.type,n=!(n!==`form`&&n!==`button`)||Xd(e.type,e.memoizedProps)),n=!n),n&&Fi&&R(e),Vi(e),t===13){if(e=e.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(317));Fi=_f(e)}else if(t===31){if(e=e.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(317));Fi=_f(e)}else t===27?(t=Fi,af(e.type)?(e=gf,gf=null,Fi=e):Fi=t):Fi=Pi?hf(e.stateNode.nextSibling):null;return!0}function Ui(){Fi=Pi=null,Ii=!1}function Wi(){var e=Li;return e!==null&&(ru===null?ru=e:ru.push.apply(ru,e),Li=null),e}function Gi(e){Li===null?Li=[e]:Li.push(e)}var Ki=ie(null),qi=null,Ji=null;function Yi(e,t,n){L(Ki,t._currentValue),t._currentValue=n}function Xi(e){e._currentValue=Ki.current,ae(Ki)}function Zi(e,t,n){for(;e!==null;){var r=e.alternate;if((e.childLanes&t)===t?r!==null&&(r.childLanes&t)!==t&&(r.childLanes|=t):(e.childLanes|=t,r!==null&&(r.childLanes|=t)),e===n)break;e=e.return}}function Qi(e,t,n,r){var a=e.child;for(a!==null&&(a.return=e);a!==null;){var o=a.dependencies;if(o!==null){var s=a.child;o=o.firstContext;a:for(;o!==null;){var c=o;o=a;for(var l=0;l<t.length;l++)if(c.context===t[l]){o.lanes|=n,c=o.alternate,c!==null&&(c.lanes|=n),Zi(o.return,n,e),r||(s=null);break a}o=c.next}}else if(a.tag===18){if(s=a.return,s===null)throw Error(i(341));s.lanes|=n,o=s.alternate,o!==null&&(o.lanes|=n),Zi(s,n,e),s=null}else s=a.child;if(s!==null)s.return=a;else for(s=a;s!==null;){if(s===e){s=null;break}if(a=s.sibling,a!==null){a.return=s.return,s=a;break}s=s.return}a=s}}function $i(e,t,n,r){e=null;for(var a=t,o=!1;a!==null;){if(!o){if(a.flags&524288)o=!0;else if(a.flags&262144)break}if(a.tag===10){var s=a.alternate;if(s===null)throw Error(i(387));if(s=s.memoizedProps,s!==null){var c=a.type;Cr(a.pendingProps.value,s.value)||(e===null?e=[c]:e.push(c))}}else if(a===le.current){if(s=a.alternate,s===null)throw Error(i(387));s.memoizedState.memoizedState!==a.memoizedState.memoizedState&&(e===null?e=[ap]:e.push(ap))}a=a.return}e!==null&&Qi(t,e,n,r),t.flags|=262144}function ea(e){for(e=e.firstContext;e!==null;){if(!Cr(e.context._currentValue,e.memoizedValue))return!0;e=e.next}return!1}function ta(e){qi=e,Ji=null,e=e.dependencies,e!==null&&(e.firstContext=null)}function na(e){return ia(qi,e)}function ra(e,t){return qi===null&&ta(e),ia(e,t)}function ia(e,t){var n=t._currentValue;if(t={context:t,memoizedValue:n,next:null},Ji===null){if(e===null)throw Error(i(308));Ji=t,e.dependencies={lanes:0,firstContext:t},e.flags|=524288}else Ji=Ji.next=t;return n}var aa=typeof AbortController<`u`?AbortController:function(){var e=[],t=this.signal={aborted:!1,addEventListener:function(t,n){e.push(n)}};this.abort=function(){t.aborted=!0,e.forEach(function(e){return e()})}},oa=t.unstable_scheduleCallback,sa=t.unstable_NormalPriority,ca={$$typeof:C,Consumer:null,Provider:null,_currentValue:null,_currentValue2:null,_threadCount:0};function la(){return{controller:new aa,data:new Map,refCount:0}}function ua(e){e.refCount--,e.refCount===0&&oa(sa,function(){e.controller.abort()})}var da=null,fa=0,pa=0,ma=null;function ha(e,t){if(da===null){var n=da=[];fa=0,pa=vd(),ma={status:`pending`,value:void 0,then:function(e){n.push(e)}}}return fa++,t.then(ga,ga),t}function ga(){if(--fa===0&&da!==null){ma!==null&&(ma.status=`fulfilled`);var e=da;da=null,pa=0,ma=null;for(var t=0;t<e.length;t++)(0,e[t])()}}function _a(e,t){var n=[],r={status:`pending`,value:null,reason:null,then:function(e){n.push(e)}};return e.then(function(){r.status=`fulfilled`,r.value=t;for(var e=0;e<n.length;e++)(0,n[e])(t)},function(e){for(r.status=`rejected`,r.reason=e,e=0;e<n.length;e++)(0,n[e])(void 0)}),r}var va=F.S;F.S=function(e,t){ou=Ee(),typeof t==`object`&&t&&typeof t.then==`function`&&ha(e,t),va!==null&&va(e,t)};var ya=ie(null);function ba(){var e=ya.current;return e===null?Hl.pooledCache:e}function xa(e,t){t===null?L(ya,ya.current):L(ya,t.pool)}function Sa(){var e=ba();return e===null?null:{parent:ca._currentValue,pool:e}}var Ca=Error(i(460)),wa=Error(i(474)),Ta=Error(i(542)),Ea={then:function(){}};function Da(e){return e=e.status,e===`fulfilled`||e===`rejected`}function Oa(e,t,n){switch(n=e[n],n===void 0?e.push(t):n!==t&&(t.then(en,en),t=n),t.status){case`fulfilled`:return t.value;case`rejected`:throw e=t.reason,Ma(e),e;default:if(typeof t.status==`string`)t.then(en,en);else{if(e=Hl,e!==null&&100<e.shellSuspendCounter)throw Error(i(482));e=t,e.status=`pending`,e.then(function(e){if(t.status===`pending`){var n=t;n.status=`fulfilled`,n.value=e}},function(e){if(t.status===`pending`){var n=t;n.status=`rejected`,n.reason=e}})}switch(t.status){case`fulfilled`:return t.value;case`rejected`:throw e=t.reason,Ma(e),e}throw Aa=t,Ca}}function ka(e){try{var t=e._init;return t(e._payload)}catch(e){throw typeof e==`object`&&e&&typeof e.then==`function`?(Aa=e,Ca):e}}var Aa=null;function ja(){if(Aa===null)throw Error(i(459));var e=Aa;return Aa=null,e}function Ma(e){if(e===Ca||e===Ta)throw Error(i(483))}var Na=null,Pa=0;function Fa(e){var t=Pa;return Pa+=1,Na===null&&(Na=[]),Oa(Na,e,t)}function Ia(e,t){t=t.props.ref,e.ref=t===void 0?null:t}function La(e,t){throw t.$$typeof===g?Error(i(525)):(e=Object.prototype.toString.call(t),Error(i(31,e===`[object Object]`?`object with keys {`+Object.keys(t).join(`, `)+`}`:e)))}function Ra(e){function t(t,n){if(e){var r=t.deletions;r===null?(t.deletions=[n],t.flags|=16):r.push(n)}}function n(n,r){if(!e)return null;for(;r!==null;)t(n,r),r=r.sibling;return null}function r(e){for(var t=new Map;e!==null;)e.key===null?t.set(e.index,e):t.set(e.key,e),e=e.sibling;return t}function a(e,t){return e=di(e,t),e.index=0,e.sibling=null,e}function o(t,n,r){return t.index=r,e?(r=t.alternate,r===null?(t.flags|=67108866,n):(r=r.index,r<n?(t.flags|=67108866,n):r)):(t.flags|=1048576,n)}function s(t){return e&&t.alternate===null&&(t.flags|=67108866),t}function c(e,t,n,r){return t===null||t.tag!==6?(t=hi(n,e.mode,r),t.return=e,t):(t=a(t,n),t.return=e,t)}function l(e,t,n,r){var i=n.type;return i===y?d(e,t,n.props.children,r,n.key):t!==null&&(t.elementType===i||typeof i==`object`&&i&&i.$$typeof===O&&ka(i)===t.type)?(t=a(t,n.props),Ia(t,n),t.return=e,t):(t=pi(n.type,n.key,n.props,null,e.mode,r),Ia(t,n),t.return=e,t)}function u(e,t,n,r){return t===null||t.tag!==4||t.stateNode.containerInfo!==n.containerInfo||t.stateNode.implementation!==n.implementation?(t=_i(n,e.mode,r),t.return=e,t):(t=a(t,n.children||[]),t.return=e,t)}function d(e,t,n,r,i){return t===null||t.tag!==7?(t=mi(n,e.mode,r,i),t.return=e,t):(t=a(t,n),t.return=e,t)}function f(e,t,n){if(typeof t==`string`&&t!==``||typeof t==`number`||typeof t==`bigint`)return t=hi(``+t,e.mode,n),t.return=e,t;if(typeof t==`object`&&t){switch(t.$$typeof){case _:return n=pi(t.type,t.key,t.props,null,e.mode,n),Ia(n,t),n.return=e,n;case v:return t=_i(t,e.mode,n),t.return=e,t;case O:return t=ka(t),f(e,t,n)}if(P(t)||M(t))return t=mi(t,e.mode,n,null),t.return=e,t;if(typeof t.then==`function`)return f(e,Fa(t),n);if(t.$$typeof===C)return f(e,ra(e,t),n);La(e,t)}return null}function p(e,t,n,r){var i=t===null?null:t.key;if(typeof n==`string`&&n!==``||typeof n==`number`||typeof n==`bigint`)return i===null?c(e,t,``+n,r):null;if(typeof n==`object`&&n){switch(n.$$typeof){case _:return n.key===i?l(e,t,n,r):null;case v:return n.key===i?u(e,t,n,r):null;case O:return n=ka(n),p(e,t,n,r)}if(P(n)||M(n))return i===null?d(e,t,n,r,null):null;if(typeof n.then==`function`)return p(e,t,Fa(n),r);if(n.$$typeof===C)return p(e,t,ra(e,n),r);La(e,n)}return null}function m(e,t,n,r,i){if(typeof r==`string`&&r!==``||typeof r==`number`||typeof r==`bigint`)return e=e.get(n)||null,c(t,e,``+r,i);if(typeof r==`object`&&r){switch(r.$$typeof){case _:return e=e.get(r.key===null?n:r.key)||null,l(t,e,r,i);case v:return e=e.get(r.key===null?n:r.key)||null,u(t,e,r,i);case O:return r=ka(r),m(e,t,n,r,i)}if(P(r)||M(r))return e=e.get(n)||null,d(t,e,r,i,null);if(typeof r.then==`function`)return m(e,t,n,Fa(r),i);if(r.$$typeof===C)return m(e,t,n,ra(t,r),i);La(t,r)}return null}function h(i,a,s,c){for(var l=null,u=null,d=a,h=a=0,g=null;d!==null&&h<s.length;h++){d.index>h?(g=d,d=null):g=d.sibling;var _=p(i,d,s[h],c);if(_===null){d===null&&(d=g);break}e&&d&&_.alternate===null&&t(i,d),a=o(_,a,h),u===null?l=_:u.sibling=_,u=_,d=g}if(h===s.length)return n(i,d),Ii&&ki(i,h),l;if(d===null){for(;h<s.length;h++)d=f(i,s[h],c),d!==null&&(a=o(d,a,h),u===null?l=d:u.sibling=d,u=d);return Ii&&ki(i,h),l}for(d=r(d);h<s.length;h++)g=m(d,i,h,s[h],c),g!==null&&(e&&g.alternate!==null&&d.delete(g.key===null?h:g.key),a=o(g,a,h),u===null?l=g:u.sibling=g,u=g);return e&&d.forEach(function(e){return t(i,e)}),Ii&&ki(i,h),l}function g(a,s,c,l){if(c==null)throw Error(i(151));for(var u=null,d=null,h=s,g=s=0,_=null,v=c.next();h!==null&&!v.done;g++,v=c.next()){h.index>g?(_=h,h=null):_=h.sibling;var y=p(a,h,v.value,l);if(y===null){h===null&&(h=_);break}e&&h&&y.alternate===null&&t(a,h),s=o(y,s,g),d===null?u=y:d.sibling=y,d=y,h=_}if(v.done)return n(a,h),Ii&&ki(a,g),u;if(h===null){for(;!v.done;g++,v=c.next())v=f(a,v.value,l),v!==null&&(s=o(v,s,g),d===null?u=v:d.sibling=v,d=v);return Ii&&ki(a,g),u}for(h=r(h);!v.done;g++,v=c.next())v=m(h,a,g,v.value,l),v!==null&&(e&&v.alternate!==null&&h.delete(v.key===null?g:v.key),s=o(v,s,g),d===null?u=v:d.sibling=v,d=v);return e&&h.forEach(function(e){return t(a,e)}),Ii&&ki(a,g),u}function b(e,r,o,c){if(typeof o==`object`&&o&&o.type===y&&o.key===null&&(o=o.props.children),typeof o==`object`&&o){switch(o.$$typeof){case _:a:{for(var l=o.key;r!==null;){if(r.key===l){if(l=o.type,l===y){if(r.tag===7){n(e,r.sibling),c=a(r,o.props.children),c.return=e,e=c;break a}}else if(r.elementType===l||typeof l==`object`&&l&&l.$$typeof===O&&ka(l)===r.type){n(e,r.sibling),c=a(r,o.props),Ia(c,o),c.return=e,e=c;break a}n(e,r);break}else t(e,r);r=r.sibling}o.type===y?(c=mi(o.props.children,e.mode,c,o.key),c.return=e,e=c):(c=pi(o.type,o.key,o.props,null,e.mode,c),Ia(c,o),c.return=e,e=c)}return s(e);case v:a:{for(l=o.key;r!==null;){if(r.key===l)if(r.tag===4&&r.stateNode.containerInfo===o.containerInfo&&r.stateNode.implementation===o.implementation){n(e,r.sibling),c=a(r,o.children||[]),c.return=e,e=c;break a}else{n(e,r);break}else t(e,r);r=r.sibling}c=_i(o,e.mode,c),c.return=e,e=c}return s(e);case O:return o=ka(o),b(e,r,o,c)}if(P(o))return h(e,r,o,c);if(M(o)){if(l=M(o),typeof l!=`function`)throw Error(i(150));return o=l.call(o),g(e,r,o,c)}if(typeof o.then==`function`)return b(e,r,Fa(o),c);if(o.$$typeof===C)return b(e,r,ra(e,o),c);La(e,o)}return typeof o==`string`&&o!==``||typeof o==`number`||typeof o==`bigint`?(o=``+o,r!==null&&r.tag===6?(n(e,r.sibling),c=a(r,o),c.return=e,e=c):(n(e,r),c=hi(o,e.mode,c),c.return=e,e=c),s(e)):n(e,r)}return function(e,t,n,r){try{Pa=0;var i=b(e,t,n,r);return Na=null,i}catch(t){if(t===Ca||t===Ta)throw t;var a=li(29,t,null,e.mode);return a.lanes=r,a.return=e,a}}}var za=Ra(!0),Ba=Ra(!1),Va=!1;function Ha(e){e.updateQueue={baseState:e.memoizedState,firstBaseUpdate:null,lastBaseUpdate:null,shared:{pending:null,lanes:0,hiddenCallbacks:null},callbacks:null}}function Ua(e,t){e=e.updateQueue,t.updateQueue===e&&(t.updateQueue={baseState:e.baseState,firstBaseUpdate:e.firstBaseUpdate,lastBaseUpdate:e.lastBaseUpdate,shared:e.shared,callbacks:null})}function Wa(e){return{lane:e,tag:0,payload:null,callback:null,next:null}}function Ga(e,t,n){var r=e.updateQueue;if(r===null)return null;if(r=r.shared,Vl&2){var i=r.pending;return i===null?t.next=t:(t.next=i.next,i.next=t),r.pending=t,t=oi(e),ai(e,null,n),t}return ni(e,r,t,n),oi(e)}function Ka(e,t,n){if(t=t.updateQueue,t!==null&&(t=t.shared,n&4194048)){var r=t.lanes;r&=e.pendingLanes,n|=r,t.lanes=n,et(e,n)}}function qa(e,t){var n=e.updateQueue,r=e.alternate;if(r!==null&&(r=r.updateQueue,n===r)){var i=null,a=null;if(n=n.firstBaseUpdate,n!==null){do{var o={lane:n.lane,tag:n.tag,payload:n.payload,callback:null,next:null};a===null?i=a=o:a=a.next=o,n=n.next}while(n!==null);a===null?i=a=t:a=a.next=t}else i=a=t;n={baseState:r.baseState,firstBaseUpdate:i,lastBaseUpdate:a,shared:r.shared,callbacks:r.callbacks},e.updateQueue=n;return}e=n.lastBaseUpdate,e===null?n.firstBaseUpdate=t:e.next=t,n.lastBaseUpdate=t}var Ja=!1;function Ya(){if(Ja){var e=ma;if(e!==null)throw e}}function Xa(e,t,n,r){Ja=!1;var i=e.updateQueue;Va=!1;var a=i.firstBaseUpdate,o=i.lastBaseUpdate,s=i.shared.pending;if(s!==null){i.shared.pending=null;var c=s,l=c.next;c.next=null,o===null?a=l:o.next=l,o=c;var u=e.alternate;u!==null&&(u=u.updateQueue,s=u.lastBaseUpdate,s!==o&&(s===null?u.firstBaseUpdate=l:s.next=l,u.lastBaseUpdate=c))}if(a!==null){var d=i.baseState;o=0,u=l=c=null,s=a;do{var f=s.lane&-536870913,p=f!==s.lane;if(p?(Ul&f)===f:(r&f)===f){f!==0&&f===pa&&(Ja=!0),u!==null&&(u=u.next={lane:0,tag:s.tag,payload:s.payload,callback:null,next:null});a:{var m=e,g=s;f=t;var _=n;switch(g.tag){case 1:if(m=g.payload,typeof m==`function`){d=m.call(_,d,f);break a}d=m;break a;case 3:m.flags=m.flags&-65537|128;case 0:if(m=g.payload,f=typeof m==`function`?m.call(_,d,f):m,f==null)break a;d=h({},d,f);break a;case 2:Va=!0}}f=s.callback,f!==null&&(e.flags|=64,p&&(e.flags|=8192),p=i.callbacks,p===null?i.callbacks=[f]:p.push(f))}else p={lane:f,tag:s.tag,payload:s.payload,callback:s.callback,next:null},u===null?(l=u=p,c=d):u=u.next=p,o|=f;if(s=s.next,s===null){if(s=i.shared.pending,s===null)break;p=s,s=p.next,p.next=null,i.lastBaseUpdate=p,i.shared.pending=null}}while(1);u===null&&(c=d),i.baseState=c,i.firstBaseUpdate=l,i.lastBaseUpdate=u,a===null&&(i.shared.lanes=0),Zl|=o,e.lanes=o,e.memoizedState=d}}function Za(e,t){if(typeof e!=`function`)throw Error(i(191,e));e.call(t)}function Qa(e,t){var n=e.callbacks;if(n!==null)for(e.callbacks=null,e=0;e<n.length;e++)Za(n[e],t)}var $a=ie(null),eo=ie(0);function to(e,t){e=Yl,L(eo,e),L($a,t),Yl=e|t.baseLanes}function no(){L(eo,Yl),L($a,$a.current)}function ro(){Yl=eo.current,ae($a),ae(eo)}var io=ie(null),ao=null;function oo(e){var t=e.alternate;L(fo,fo.current&1),L(io,e),ao===null&&(t===null||$a.current!==null||t.memoizedState!==null)&&(ao=e)}function so(e){L(fo,fo.current),L(io,e),ao===null&&(ao=e)}function co(e){e.tag===22?(L(fo,fo.current),L(io,e),ao===null&&(ao=e)):lo(e)}function lo(){L(fo,fo.current),L(io,io.current)}function uo(e){ae(io),ao===e&&(ao=null),ae(fo)}var fo=ie(0);function po(e){for(var t=e;t!==null;){if(t.tag===13){var n=t.memoizedState;if(n!==null&&(n=n.dehydrated,n===null||ff(n)||pf(n)))return t}else if(t.tag===19&&(t.memoizedProps.revealOrder===`forwards`||t.memoizedProps.revealOrder===`backwards`||t.memoizedProps.revealOrder===`unstable_legacy-backwards`||t.memoizedProps.revealOrder===`together`)){if(t.flags&128)return t}else if(t.child!==null){t.child.return=t,t=t.child;continue}if(t===e)break;for(;t.sibling===null;){if(t.return===null||t.return===e)return null;t=t.return}t.sibling.return=t.return,t=t.sibling}return null}var mo=0,z=null,ho=null,go=null,_o=!1,vo=!1,yo=!1,bo=0,xo=0,So=null,Co=0;function wo(){throw Error(i(321))}function To(e,t){if(t===null)return!1;for(var n=0;n<t.length&&n<e.length;n++)if(!Cr(e[n],t[n]))return!1;return!0}function Eo(e,t,n,r,i,a){return mo=a,z=t,t.memoizedState=null,t.updateQueue=null,t.lanes=0,F.H=e===null||e.memoizedState===null?Hs:Us,yo=!1,a=n(r,i),yo=!1,vo&&(a=Oo(t,n,r,i)),Do(e),a}function Do(e){F.H=Vs;var t=ho!==null&&ho.next!==null;if(mo=0,go=ho=z=null,_o=!1,xo=0,So=null,t)throw Error(i(300));e===null||oc||(e=e.dependencies,e!==null&&ea(e)&&(oc=!0))}function Oo(e,t,n,r){z=e;var a=0;do{if(vo&&(So=null),xo=0,vo=!1,25<=a)throw Error(i(301));if(a+=1,go=ho=null,e.updateQueue!=null){var o=e.updateQueue;o.lastEffect=null,o.events=null,o.stores=null,o.memoCache!=null&&(o.memoCache.index=0)}F.H=Ws,o=t(n,r)}while(vo);return o}function ko(){var e=F.H,t=e.useState()[0];return t=typeof t.then==`function`?Fo(t):t,e=e.useState()[0],(ho===null?null:ho.memoizedState)!==e&&(z.flags|=1024),t}function Ao(){var e=bo!==0;return bo=0,e}function jo(e,t,n){t.updateQueue=e.updateQueue,t.flags&=-2053,e.lanes&=~n}function Mo(e){if(_o){for(e=e.memoizedState;e!==null;){var t=e.queue;t!==null&&(t.pending=null),e=e.next}_o=!1}mo=0,go=ho=z=null,vo=!1,xo=bo=0,So=null}function No(){var e={memoizedState:null,baseState:null,baseQueue:null,queue:null,next:null};return go===null?z.memoizedState=go=e:go=go.next=e,go}function B(){if(ho===null){var e=z.alternate;e=e===null?null:e.memoizedState}else e=ho.next;var t=go===null?z.memoizedState:go.next;if(t!==null)go=t,ho=e;else{if(e===null)throw z.alternate===null?Error(i(467)):Error(i(310));ho=e,e={memoizedState:ho.memoizedState,baseState:ho.baseState,baseQueue:ho.baseQueue,queue:ho.queue,next:null},go===null?z.memoizedState=go=e:go=go.next=e}return go}function Po(){return{lastEffect:null,events:null,stores:null,memoCache:null}}function Fo(e){var t=xo;return xo+=1,So===null&&(So=[]),e=Oa(So,e,t),t=z,(go===null?t.memoizedState:go.next)===null&&(t=t.alternate,F.H=t===null||t.memoizedState===null?Hs:Us),e}function Io(e){if(typeof e==`object`&&e){if(typeof e.then==`function`)return Fo(e);if(e.$$typeof===C)return na(e)}throw Error(i(438,String(e)))}function Lo(e){var t=null,n=z.updateQueue;if(n!==null&&(t=n.memoCache),t==null){var r=z.alternate;r!==null&&(r=r.updateQueue,r!==null&&(r=r.memoCache,r!=null&&(t={data:r.data.map(function(e){return e.slice()}),index:0})))}if(t??={data:[],index:0},n===null&&(n=Po(),z.updateQueue=n),n.memoCache=t,n=t.data[t.index],n===void 0)for(n=t.data[t.index]=Array(e),r=0;r<e;r++)n[r]=A;return t.index++,n}function Ro(e,t){return typeof t==`function`?t(e):t}function zo(e){return Bo(B(),ho,e)}function Bo(e,t,n){var r=e.queue;if(r===null)throw Error(i(311));r.lastRenderedReducer=n;var a=e.baseQueue,o=r.pending;if(o!==null){if(a!==null){var s=a.next;a.next=o.next,o.next=s}t.baseQueue=a=o,r.pending=null}if(o=e.baseState,a===null)e.memoizedState=o;else{t=a.next;var c=s=null,l=null,u=t,d=!1;do{var f=u.lane&-536870913;if(f===u.lane?(mo&f)===f:(Ul&f)===f){var p=u.revertLane;if(p===0)l!==null&&(l=l.next={lane:0,revertLane:0,gesture:null,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null}),f===pa&&(d=!0);else if((mo&p)===p){u=u.next,p===pa&&(d=!0);continue}else f={lane:0,revertLane:u.revertLane,gesture:null,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null},l===null?(c=l=f,s=o):l=l.next=f,z.lanes|=p,Zl|=p;f=u.action,yo&&n(o,f),o=u.hasEagerState?u.eagerState:n(o,f)}else p={lane:f,revertLane:u.revertLane,gesture:u.gesture,action:u.action,hasEagerState:u.hasEagerState,eagerState:u.eagerState,next:null},l===null?(c=l=p,s=o):l=l.next=p,z.lanes|=f,Zl|=f;u=u.next}while(u!==null&&u!==t);if(l===null?s=o:l.next=c,!Cr(o,e.memoizedState)&&(oc=!0,d&&(n=ma,n!==null)))throw n;e.memoizedState=o,e.baseState=s,e.baseQueue=l,r.lastRenderedState=o}return a===null&&(r.lanes=0),[e.memoizedState,r.dispatch]}function Vo(e){var t=B(),n=t.queue;if(n===null)throw Error(i(311));n.lastRenderedReducer=e;var r=n.dispatch,a=n.pending,o=t.memoizedState;if(a!==null){n.pending=null;var s=a=a.next;do o=e(o,s.action),s=s.next;while(s!==a);Cr(o,t.memoizedState)||(oc=!0),t.memoizedState=o,t.baseQueue===null&&(t.baseState=o),n.lastRenderedState=o}return[o,r]}function Ho(e,t,n){var r=z,a=B(),o=Ii;if(o){if(n===void 0)throw Error(i(407));n=n()}else n=t();var s=!Cr((ho||a).memoizedState,n);if(s&&(a.memoizedState=n,oc=!0),a=a.queue,ps(Go.bind(null,r,a,e),[e]),a.getSnapshot!==t||s||go!==null&&go.memoizedState.tag&1){if(r.flags|=2048,cs(9,{destroy:void 0},Wo.bind(null,r,a,n,t),null),Hl===null)throw Error(i(349));o||mo&127||Uo(r,t,n)}return n}function Uo(e,t,n){e.flags|=16384,e={getSnapshot:t,value:n},t=z.updateQueue,t===null?(t=Po(),z.updateQueue=t,t.stores=[e]):(n=t.stores,n===null?t.stores=[e]:n.push(e))}function Wo(e,t,n,r){t.value=n,t.getSnapshot=r,Ko(t)&&qo(e)}function Go(e,t,n){return n(function(){Ko(t)&&qo(e)})}function Ko(e){var t=e.getSnapshot;e=e.value;try{var n=t();return!Cr(e,n)}catch{return!0}}function qo(e){var t=ii(e,2);t!==null&&xu(t,e,2)}function Jo(e){var t=No();if(typeof e==`function`){var n=e;if(e=n(),yo){Le(!0);try{n()}finally{Le(!1)}}}return t.memoizedState=t.baseState=e,t.queue={pending:null,lanes:0,dispatch:null,lastRenderedReducer:Ro,lastRenderedState:e},t}function Yo(e,t,n,r){return e.baseState=n,Bo(e,ho,typeof r==`function`?r:Ro)}function Xo(e,t,n,r,a){if(Rs(e))throw Error(i(485));if(e=t.action,e!==null){var o={payload:a,action:e,next:null,isTransition:!0,status:`pending`,value:null,reason:null,listeners:[],then:function(e){o.listeners.push(e)}};F.T===null?o.isTransition=!1:n(!0),r(o),n=t.pending,n===null?(o.next=t.pending=o,Zo(t,o)):(o.next=n.next,t.pending=n.next=o)}}function Zo(e,t){var n=t.action,r=t.payload,i=e.state;if(t.isTransition){var a=F.T,o={};F.T=o;try{var s=n(i,r),c=F.S;c!==null&&c(o,s),Qo(e,t,s)}catch(n){es(e,t,n)}finally{a!==null&&o.types!==null&&(a.types=o.types),F.T=a}}else try{a=n(i,r),Qo(e,t,a)}catch(n){es(e,t,n)}}function Qo(e,t,n){typeof n==`object`&&n&&typeof n.then==`function`?n.then(function(n){$o(e,t,n)},function(n){return es(e,t,n)}):$o(e,t,n)}function $o(e,t,n){t.status=`fulfilled`,t.value=n,ts(t),e.state=n,t=e.pending,t!==null&&(n=t.next,n===t?e.pending=null:(n=n.next,t.next=n,Zo(e,n)))}function es(e,t,n){var r=e.pending;if(e.pending=null,r!==null){r=r.next;do t.status=`rejected`,t.reason=n,ts(t),t=t.next;while(t!==r)}e.action=null}function ts(e){e=e.listeners;for(var t=0;t<e.length;t++)(0,e[t])()}function ns(e,t){return t}function rs(e,t){if(Ii){var n=Hl.formState;if(n!==null){a:{var r=z;if(Ii){if(Fi){b:{for(var i=Fi,a=Ri;i.nodeType!==8;){if(!a){i=null;break b}if(i=hf(i.nextSibling),i===null){i=null;break b}}a=i.data,i=a===`F!`||a===`F`?i:null}if(i){Fi=hf(i.nextSibling),r=i.data===`F!`;break a}}R(r)}r=!1}r&&(t=n[0])}}return n=No(),n.memoizedState=n.baseState=t,r={pending:null,lanes:0,dispatch:null,lastRenderedReducer:ns,lastRenderedState:t},n.queue=r,n=Fs.bind(null,z,r),r.dispatch=n,r=Jo(!1),a=Ls.bind(null,z,!1,r.queue),r=No(),i={state:t,dispatch:null,action:e,pending:null},r.queue=i,n=Xo.bind(null,z,i,a,n),i.dispatch=n,r.memoizedState=e,[t,n,!1]}function is(e){return as(B(),ho,e)}function as(e,t,n){if(t=Bo(e,t,ns)[0],e=zo(Ro)[0],typeof t==`object`&&t&&typeof t.then==`function`)try{var r=Fo(t)}catch(e){throw e===Ca?Ta:e}else r=t;t=B();var i=t.queue,a=i.dispatch;return n!==t.memoizedState&&(z.flags|=2048,cs(9,{destroy:void 0},os.bind(null,i,n),null)),[r,a,e]}function os(e,t){e.action=t}function ss(e){var t=B(),n=ho;if(n!==null)return as(t,n,e);B(),t=t.memoizedState,n=B();var r=n.queue.dispatch;return n.memoizedState=e,[t,r,!1]}function cs(e,t,n,r){return e={tag:e,create:n,deps:r,inst:t,next:null},t=z.updateQueue,t===null&&(t=Po(),z.updateQueue=t),n=t.lastEffect,n===null?t.lastEffect=e.next=e:(r=n.next,n.next=e,e.next=r,t.lastEffect=e),e}function ls(){return B().memoizedState}function us(e,t,n,r){var i=No();z.flags|=e,i.memoizedState=cs(1|t,{destroy:void 0},n,r===void 0?null:r)}function ds(e,t,n,r){var i=B();r=r===void 0?null:r;var a=i.memoizedState.inst;ho!==null&&r!==null&&To(r,ho.memoizedState.deps)?i.memoizedState=cs(t,a,n,r):(z.flags|=e,i.memoizedState=cs(1|t,a,n,r))}function fs(e,t){us(8390656,8,e,t)}function ps(e,t){ds(2048,8,e,t)}function ms(e){z.flags|=4;var t=z.updateQueue;if(t===null)t=Po(),z.updateQueue=t,t.events=[e];else{var n=t.events;n===null?t.events=[e]:n.push(e)}}function hs(e){var t=B().memoizedState;return ms({ref:t,nextImpl:e}),function(){if(Vl&2)throw Error(i(440));return t.impl.apply(void 0,arguments)}}function gs(e,t){return ds(4,2,e,t)}function _s(e,t){return ds(4,4,e,t)}function vs(e,t){if(typeof t==`function`){e=e();var n=t(e);return function(){typeof n==`function`?n():t(null)}}if(t!=null)return e=e(),t.current=e,function(){t.current=null}}function ys(e,t,n){n=n==null?null:n.concat([e]),ds(4,4,vs.bind(null,t,e),n)}function bs(){}function xs(e,t){var n=B();t=t===void 0?null:t;var r=n.memoizedState;return t!==null&&To(t,r[1])?r[0]:(n.memoizedState=[e,t],e)}function Ss(e,t){var n=B();t=t===void 0?null:t;var r=n.memoizedState;if(t!==null&&To(t,r[1]))return r[0];if(r=e(),yo){Le(!0);try{e()}finally{Le(!1)}}return n.memoizedState=[r,t],r}function Cs(e,t,n){return n===void 0||mo&1073741824&&!(Ul&261930)?e.memoizedState=t:(e.memoizedState=n,e=bu(),z.lanes|=e,Zl|=e,n)}function ws(e,t,n,r){return Cr(n,t)?n:$a.current===null?!(mo&42)||mo&1073741824&&!(Ul&261930)?(oc=!0,e.memoizedState=n):(e=bu(),z.lanes|=e,Zl|=e,t):(e=Cs(e,n,r),Cr(e,t)||(oc=!0),e)}function Ts(e,t,n,r,i){var a=I.p;I.p=a!==0&&8>a?a:8;var o=F.T,s={};F.T=s,Ls(e,!1,t,n);try{var c=i(),l=F.S;l!==null&&l(s,c),typeof c==`object`&&c&&typeof c.then==`function`?Is(e,t,_a(c,r),yu(e)):Is(e,t,r,yu(e))}catch(n){Is(e,t,{then:function(){},status:`rejected`,reason:n},yu())}finally{I.p=a,o!==null&&s.types!==null&&(o.types=s.types),F.T=o}}function Es(){}function Ds(e,t,n,r){if(e.tag!==5)throw Error(i(476));var a=Os(e).queue;Ts(e,a,t,te,n===null?Es:function(){return ks(e),n(r)})}function Os(e){var t=e.memoizedState;if(t!==null)return t;t={memoizedState:te,baseState:te,baseQueue:null,queue:{pending:null,lanes:0,dispatch:null,lastRenderedReducer:Ro,lastRenderedState:te},next:null};var n={};return t.next={memoizedState:n,baseState:n,baseQueue:null,queue:{pending:null,lanes:0,dispatch:null,lastRenderedReducer:Ro,lastRenderedState:n},next:null},e.memoizedState=t,e=e.alternate,e!==null&&(e.memoizedState=t),t}function ks(e){var t=Os(e);t.next===null&&(t=e.alternate.memoizedState),Is(e,t.next.queue,{},yu())}function As(){return na(ap)}function js(){return B().memoizedState}function Ms(){return B().memoizedState}function Ns(e){for(var t=e.return;t!==null;){switch(t.tag){case 24:case 3:var n=yu();e=Wa(n);var r=Ga(t,e,n);r!==null&&(xu(r,t,n),Ka(r,t,n)),t={cache:la()},e.payload=t;return}t=t.return}}function Ps(e,t,n){var r=yu();n={lane:r,revertLane:0,gesture:null,action:n,hasEagerState:!1,eagerState:null,next:null},Rs(e)?zs(t,n):(n=ri(e,t,n,r),n!==null&&(xu(n,e,r),Bs(n,t,r)))}function Fs(e,t,n){Is(e,t,n,yu())}function Is(e,t,n,r){var i={lane:r,revertLane:0,gesture:null,action:n,hasEagerState:!1,eagerState:null,next:null};if(Rs(e))zs(t,i);else{var a=e.alternate;if(e.lanes===0&&(a===null||a.lanes===0)&&(a=t.lastRenderedReducer,a!==null))try{var o=t.lastRenderedState,s=a(o,n);if(i.hasEagerState=!0,i.eagerState=s,Cr(s,o))return ni(e,t,i,0),Hl===null&&ti(),!1}catch{}if(n=ri(e,t,i,r),n!==null)return xu(n,e,r),Bs(n,t,r),!0}return!1}function Ls(e,t,n,r){if(r={lane:2,revertLane:vd(),gesture:null,action:r,hasEagerState:!1,eagerState:null,next:null},Rs(e)){if(t)throw Error(i(479))}else t=ri(e,n,r,2),t!==null&&xu(t,e,2)}function Rs(e){var t=e.alternate;return e===z||t!==null&&t===z}function zs(e,t){vo=_o=!0;var n=e.pending;n===null?t.next=t:(t.next=n.next,n.next=t),e.pending=t}function Bs(e,t,n){if(n&4194048){var r=t.lanes;r&=e.pendingLanes,n|=r,t.lanes=n,et(e,n)}}var Vs={readContext:na,use:Io,useCallback:wo,useContext:wo,useEffect:wo,useImperativeHandle:wo,useLayoutEffect:wo,useInsertionEffect:wo,useMemo:wo,useReducer:wo,useRef:wo,useState:wo,useDebugValue:wo,useDeferredValue:wo,useTransition:wo,useSyncExternalStore:wo,useId:wo,useHostTransitionStatus:wo,useFormState:wo,useActionState:wo,useOptimistic:wo,useMemoCache:wo,useCacheRefresh:wo};Vs.useEffectEvent=wo;var Hs={readContext:na,use:Io,useCallback:function(e,t){return No().memoizedState=[e,t===void 0?null:t],e},useContext:na,useEffect:fs,useImperativeHandle:function(e,t,n){n=n==null?null:n.concat([e]),us(4194308,4,vs.bind(null,t,e),n)},useLayoutEffect:function(e,t){return us(4194308,4,e,t)},useInsertionEffect:function(e,t){us(4,2,e,t)},useMemo:function(e,t){var n=No();t=t===void 0?null:t;var r=e();if(yo){Le(!0);try{e()}finally{Le(!1)}}return n.memoizedState=[r,t],r},useReducer:function(e,t,n){var r=No();if(n!==void 0){var i=n(t);if(yo){Le(!0);try{n(t)}finally{Le(!1)}}}else i=t;return r.memoizedState=r.baseState=i,e={pending:null,lanes:0,dispatch:null,lastRenderedReducer:e,lastRenderedState:i},r.queue=e,e=e.dispatch=Ps.bind(null,z,e),[r.memoizedState,e]},useRef:function(e){var t=No();return e={current:e},t.memoizedState=e},useState:function(e){e=Jo(e);var t=e.queue,n=Fs.bind(null,z,t);return t.dispatch=n,[e.memoizedState,n]},useDebugValue:bs,useDeferredValue:function(e,t){return Cs(No(),e,t)},useTransition:function(){var e=Jo(!1);return e=Ts.bind(null,z,e.queue,!0,!1),No().memoizedState=e,[!1,e]},useSyncExternalStore:function(e,t,n){var r=z,a=No();if(Ii){if(n===void 0)throw Error(i(407));n=n()}else{if(n=t(),Hl===null)throw Error(i(349));Ul&127||Uo(r,t,n)}a.memoizedState=n;var o={value:n,getSnapshot:t};return a.queue=o,fs(Go.bind(null,r,o,e),[e]),r.flags|=2048,cs(9,{destroy:void 0},Wo.bind(null,r,o,n,t),null),n},useId:function(){var e=No(),t=Hl.identifierPrefix;if(Ii){var n=Oi,r=Di;n=(r&~(1<<32-Re(r)-1)).toString(32)+n,t=`_`+t+`R_`+n,n=bo++,0<n&&(t+=`H`+n.toString(32)),t+=`_`}else n=Co++,t=`_`+t+`r_`+n.toString(32)+`_`;return e.memoizedState=t},useHostTransitionStatus:As,useFormState:rs,useActionState:rs,useOptimistic:function(e){var t=No();t.memoizedState=t.baseState=e;var n={pending:null,lanes:0,dispatch:null,lastRenderedReducer:null,lastRenderedState:null};return t.queue=n,t=Ls.bind(null,z,!0,n),n.dispatch=t,[e,t]},useMemoCache:Lo,useCacheRefresh:function(){return No().memoizedState=Ns.bind(null,z)},useEffectEvent:function(e){var t=No(),n={impl:e};return t.memoizedState=n,function(){if(Vl&2)throw Error(i(440));return n.impl.apply(void 0,arguments)}}},Us={readContext:na,use:Io,useCallback:xs,useContext:na,useEffect:ps,useImperativeHandle:ys,useInsertionEffect:gs,useLayoutEffect:_s,useMemo:Ss,useReducer:zo,useRef:ls,useState:function(){return zo(Ro)},useDebugValue:bs,useDeferredValue:function(e,t){return ws(B(),ho.memoizedState,e,t)},useTransition:function(){var e=zo(Ro)[0],t=B().memoizedState;return[typeof e==`boolean`?e:Fo(e),t]},useSyncExternalStore:Ho,useId:js,useHostTransitionStatus:As,useFormState:is,useActionState:is,useOptimistic:function(e,t){return Yo(B(),ho,e,t)},useMemoCache:Lo,useCacheRefresh:Ms};Us.useEffectEvent=hs;var Ws={readContext:na,use:Io,useCallback:xs,useContext:na,useEffect:ps,useImperativeHandle:ys,useInsertionEffect:gs,useLayoutEffect:_s,useMemo:Ss,useReducer:Vo,useRef:ls,useState:function(){return Vo(Ro)},useDebugValue:bs,useDeferredValue:function(e,t){var n=B();return ho===null?Cs(n,e,t):ws(n,ho.memoizedState,e,t)},useTransition:function(){var e=Vo(Ro)[0],t=B().memoizedState;return[typeof e==`boolean`?e:Fo(e),t]},useSyncExternalStore:Ho,useId:js,useHostTransitionStatus:As,useFormState:ss,useActionState:ss,useOptimistic:function(e,t){var n=B();return ho===null?(n.baseState=e,[e,n.queue.dispatch]):Yo(n,ho,e,t)},useMemoCache:Lo,useCacheRefresh:Ms};Ws.useEffectEvent=hs;function Gs(e,t,n,r){t=e.memoizedState,n=n(r,t),n=n==null?t:h({},t,n),e.memoizedState=n,e.lanes===0&&(e.updateQueue.baseState=n)}var Ks={enqueueSetState:function(e,t,n){e=e._reactInternals;var r=yu(),i=Wa(r);i.payload=t,n!=null&&(i.callback=n),t=Ga(e,i,r),t!==null&&(xu(t,e,r),Ka(t,e,r))},enqueueReplaceState:function(e,t,n){e=e._reactInternals;var r=yu(),i=Wa(r);i.tag=1,i.payload=t,n!=null&&(i.callback=n),t=Ga(e,i,r),t!==null&&(xu(t,e,r),Ka(t,e,r))},enqueueForceUpdate:function(e,t){e=e._reactInternals;var n=yu(),r=Wa(n);r.tag=2,t!=null&&(r.callback=t),t=Ga(e,r,n),t!==null&&(xu(t,e,n),Ka(t,e,n))}};function qs(e,t,n,r,i,a,o){return e=e.stateNode,typeof e.shouldComponentUpdate==`function`?e.shouldComponentUpdate(r,a,o):t.prototype&&t.prototype.isPureReactComponent?!wr(n,r)||!wr(i,a):!0}function Js(e,t,n,r){e=t.state,typeof t.componentWillReceiveProps==`function`&&t.componentWillReceiveProps(n,r),typeof t.UNSAFE_componentWillReceiveProps==`function`&&t.UNSAFE_componentWillReceiveProps(n,r),t.state!==e&&Ks.enqueueReplaceState(t,t.state,null)}function Ys(e,t){var n=t;if(`ref`in t)for(var r in n={},t)r!==`ref`&&(n[r]=t[r]);if(e=e.defaultProps)for(var i in n===t&&(n=h({},n)),e)n[i]===void 0&&(n[i]=e[i]);return n}function Xs(e){Zr(e)}function Zs(e){console.error(e)}function Qs(e){Zr(e)}function $s(e,t){try{var n=e.onUncaughtError;n(t.value,{componentStack:t.stack})}catch(e){setTimeout(function(){throw e})}}function ec(e,t,n){try{var r=e.onCaughtError;r(n.value,{componentStack:n.stack,errorBoundary:t.tag===1?t.stateNode:null})}catch(e){setTimeout(function(){throw e})}}function tc(e,t,n){return n=Wa(n),n.tag=3,n.payload={element:null},n.callback=function(){$s(e,t)},n}function nc(e){return e=Wa(e),e.tag=3,e}function rc(e,t,n,r){var i=n.type.getDerivedStateFromError;if(typeof i==`function`){var a=r.value;e.payload=function(){return i(a)},e.callback=function(){ec(t,n,r)}}var o=n.stateNode;o!==null&&typeof o.componentDidCatch==`function`&&(e.callback=function(){ec(t,n,r),typeof i!=`function`&&(lu===null?lu=new Set([this]):lu.add(this));var e=r.stack;this.componentDidCatch(r.value,{componentStack:e===null?``:e})})}function ic(e,t,n,r,a){if(n.flags|=32768,typeof r==`object`&&r&&typeof r.then==`function`){if(t=n.alternate,t!==null&&$i(t,n,a,!0),n=io.current,n!==null){switch(n.tag){case 31:case 13:return ao===null?Nu():n.alternate===null&&Xl===0&&(Xl=3),n.flags&=-257,n.flags|=65536,n.lanes=a,r===Ea?n.flags|=16384:(t=n.updateQueue,t===null?n.updateQueue=new Set([r]):t.add(r),Qu(e,r,a)),!1;case 22:return n.flags|=65536,r===Ea?n.flags|=16384:(t=n.updateQueue,t===null?(t={transitions:null,markerInstances:null,retryQueue:new Set([r])},n.updateQueue=t):(n=t.retryQueue,n===null?t.retryQueue=new Set([r]):n.add(r)),Qu(e,r,a)),!1}throw Error(i(435,n.tag))}return Qu(e,r,a),Nu(),!1}if(Ii)return t=io.current,t===null?(r!==zi&&(t=Error(i(423),{cause:r}),Gi(yi(t,n))),e=e.current.alternate,e.flags|=65536,a&=-a,e.lanes|=a,r=yi(r,n),a=tc(e.stateNode,r,a),qa(e,a),Xl!==4&&(Xl=2)):(!(t.flags&65536)&&(t.flags|=256),t.flags|=65536,t.lanes=a,r!==zi&&(e=Error(i(422),{cause:r}),Gi(yi(e,n)))),!1;var o=Error(i(520),{cause:r});if(o=yi(o,n),nu===null?nu=[o]:nu.push(o),Xl!==4&&(Xl=2),t===null)return!0;r=yi(r,n),n=t;do{switch(n.tag){case 3:return n.flags|=65536,e=a&-a,n.lanes|=e,e=tc(n.stateNode,r,e),qa(n,e),!1;case 1:if(t=n.type,o=n.stateNode,!(n.flags&128)&&(typeof t.getDerivedStateFromError==`function`||o!==null&&typeof o.componentDidCatch==`function`&&(lu===null||!lu.has(o))))return n.flags|=65536,a&=-a,n.lanes|=a,a=nc(a),rc(a,e,n,r),qa(n,a),!1}n=n.return}while(n!==null);return!1}var ac=Error(i(461)),oc=!1;function sc(e,t,n,r){t.child=e===null?Ba(t,null,n,r):za(t,e.child,n,r)}function cc(e,t,n,r,i){n=n.render;var a=t.ref;if(`ref`in r){var o={};for(var s in r)s!==`ref`&&(o[s]=r[s])}else o=r;return ta(t),r=Eo(e,t,n,o,a,i),s=Ao(),e!==null&&!oc?(jo(e,t,i),Mc(e,t,i)):(Ii&&s&&ji(t),t.flags|=1,sc(e,t,r,i),t.child)}function lc(e,t,n,r,i){if(e===null){var a=n.type;return typeof a==`function`&&!ui(a)&&a.defaultProps===void 0&&n.compare===null?(t.tag=15,t.type=a,uc(e,t,a,r,i)):(e=pi(n.type,null,r,t,t.mode,i),e.ref=t.ref,e.return=t,t.child=e)}if(a=e.child,!Nc(e,i)){var o=a.memoizedProps;if(n=n.compare,n=n===null?wr:n,n(o,r)&&e.ref===t.ref)return Mc(e,t,i)}return t.flags|=1,e=di(a,r),e.ref=t.ref,e.return=t,t.child=e}function uc(e,t,n,r,i){if(e!==null){var a=e.memoizedProps;if(wr(a,r)&&e.ref===t.ref)if(oc=!1,t.pendingProps=r=a,Nc(e,i))e.flags&131072&&(oc=!0);else return t.lanes=e.lanes,Mc(e,t,i)}return vc(e,t,n,r,i)}function dc(e,t,n,r){var i=r.children,a=e===null?null:e.memoizedState;if(e===null&&t.stateNode===null&&(t.stateNode={_visibility:1,_pendingMarkers:null,_retryCache:null,_transitions:null}),r.mode===`hidden`){if(t.flags&128){if(a=a===null?n:a.baseLanes|n,e!==null){for(r=t.child=e.child,i=0;r!==null;)i=i|r.lanes|r.childLanes,r=r.sibling;r=i&~a}else r=0,t.child=null;return pc(e,t,a,n,r)}if(n&536870912)t.memoizedState={baseLanes:0,cachePool:null},e!==null&&xa(t,a===null?null:a.cachePool),a===null?no():to(t,a),co(t);else return r=t.lanes=536870912,pc(e,t,a===null?n:a.baseLanes|n,n,r)}else a===null?(e!==null&&xa(t,null),no(),lo(t)):(xa(t,a.cachePool),to(t,a),lo(t),t.memoizedState=null);return sc(e,t,i,n),t.child}function fc(e,t){return e!==null&&e.tag===22||t.stateNode!==null||(t.stateNode={_visibility:1,_pendingMarkers:null,_retryCache:null,_transitions:null}),t.sibling}function pc(e,t,n,r,i){var a=ba();return a=a===null?null:{parent:ca._currentValue,pool:a},t.memoizedState={baseLanes:n,cachePool:a},e!==null&&xa(t,null),no(),co(t),e!==null&&$i(e,t,r,!0),t.childLanes=i,null}function mc(e,t){return t=Dc({mode:t.mode,children:t.children},e.mode),t.ref=e.ref,e.child=t,t.return=e,t}function hc(e,t,n){return za(t,e.child,null,n),e=mc(t,t.pendingProps),e.flags|=2,uo(t),t.memoizedState=null,e}function gc(e,t,n){var r=t.pendingProps,a=(t.flags&128)!=0;if(t.flags&=-129,e===null){if(Ii){if(r.mode===`hidden`)return e=mc(t,r),t.lanes=536870912,fc(null,e);if(so(t),(e=Fi)?(e=df(e,Ri),e=e!==null&&e.data===`&`?e:null,e!==null&&(t.memoizedState={dehydrated:e,treeContext:Ei===null?null:{id:Di,overflow:Oi},retryLane:536870912,hydrationErrors:null},n=gi(e),n.return=t,t.child=n,Pi=t,Fi=null)):e=null,e===null)throw R(t);return t.lanes=536870912,null}return mc(t,r)}var o=e.memoizedState;if(o!==null){var s=o.dehydrated;if(so(t),a)if(t.flags&256)t.flags&=-257,t=hc(e,t,n);else if(t.memoizedState!==null)t.child=e.child,t.flags|=128,t=null;else throw Error(i(558));else if(oc||$i(e,t,n,!1),a=(n&e.childLanes)!==0,oc||a){if(r=Hl,r!==null&&(s=tt(r,n),s!==0&&s!==o.retryLane))throw o.retryLane=s,ii(e,s),xu(r,e,s),ac;Nu(),t=hc(e,t,n)}else e=o.treeContext,Fi=hf(s.nextSibling),Pi=t,Ii=!0,Li=null,Ri=!1,e!==null&&Ni(t,e),t=mc(t,r),t.flags|=4096;return t}return e=di(e.child,{mode:r.mode,children:r.children}),e.ref=t.ref,t.child=e,e.return=t,e}function _c(e,t){var n=t.ref;if(n===null)e!==null&&e.ref!==null&&(t.flags|=4194816);else{if(typeof n!=`function`&&typeof n!=`object`)throw Error(i(284));(e===null||e.ref!==n)&&(t.flags|=4194816)}}function vc(e,t,n,r,i){return ta(t),n=Eo(e,t,n,r,void 0,i),r=Ao(),e!==null&&!oc?(jo(e,t,i),Mc(e,t,i)):(Ii&&r&&ji(t),t.flags|=1,sc(e,t,n,i),t.child)}function yc(e,t,n,r,i,a){return ta(t),t.updateQueue=null,n=Oo(t,r,n,i),Do(e),r=Ao(),e!==null&&!oc?(jo(e,t,a),Mc(e,t,a)):(Ii&&r&&ji(t),t.flags|=1,sc(e,t,n,a),t.child)}function bc(e,t,n,r,i){if(ta(t),t.stateNode===null){var a=si,o=n.contextType;typeof o==`object`&&o&&(a=na(o)),a=new n(r,a),t.memoizedState=a.state!==null&&a.state!==void 0?a.state:null,a.updater=Ks,t.stateNode=a,a._reactInternals=t,a=t.stateNode,a.props=r,a.state=t.memoizedState,a.refs={},Ha(t),o=n.contextType,a.context=typeof o==`object`&&o?na(o):si,a.state=t.memoizedState,o=n.getDerivedStateFromProps,typeof o==`function`&&(Gs(t,n,o,r),a.state=t.memoizedState),typeof n.getDerivedStateFromProps==`function`||typeof a.getSnapshotBeforeUpdate==`function`||typeof a.UNSAFE_componentWillMount!=`function`&&typeof a.componentWillMount!=`function`||(o=a.state,typeof a.componentWillMount==`function`&&a.componentWillMount(),typeof a.UNSAFE_componentWillMount==`function`&&a.UNSAFE_componentWillMount(),o!==a.state&&Ks.enqueueReplaceState(a,a.state,null),Xa(t,r,a,i),Ya(),a.state=t.memoizedState),typeof a.componentDidMount==`function`&&(t.flags|=4194308),r=!0}else if(e===null){a=t.stateNode;var s=t.memoizedProps,c=Ys(n,s);a.props=c;var l=a.context,u=n.contextType;o=si,typeof u==`object`&&u&&(o=na(u));var d=n.getDerivedStateFromProps;u=typeof d==`function`||typeof a.getSnapshotBeforeUpdate==`function`,s=t.pendingProps!==s,u||typeof a.UNSAFE_componentWillReceiveProps!=`function`&&typeof a.componentWillReceiveProps!=`function`||(s||l!==o)&&Js(t,a,r,o),Va=!1;var f=t.memoizedState;a.state=f,Xa(t,r,a,i),Ya(),l=t.memoizedState,s||f!==l||Va?(typeof d==`function`&&(Gs(t,n,d,r),l=t.memoizedState),(c=Va||qs(t,n,c,r,f,l,o))?(u||typeof a.UNSAFE_componentWillMount!=`function`&&typeof a.componentWillMount!=`function`||(typeof a.componentWillMount==`function`&&a.componentWillMount(),typeof a.UNSAFE_componentWillMount==`function`&&a.UNSAFE_componentWillMount()),typeof a.componentDidMount==`function`&&(t.flags|=4194308)):(typeof a.componentDidMount==`function`&&(t.flags|=4194308),t.memoizedProps=r,t.memoizedState=l),a.props=r,a.state=l,a.context=o,r=c):(typeof a.componentDidMount==`function`&&(t.flags|=4194308),r=!1)}else{a=t.stateNode,Ua(e,t),o=t.memoizedProps,u=Ys(n,o),a.props=u,d=t.pendingProps,f=a.context,l=n.contextType,c=si,typeof l==`object`&&l&&(c=na(l)),s=n.getDerivedStateFromProps,(l=typeof s==`function`||typeof a.getSnapshotBeforeUpdate==`function`)||typeof a.UNSAFE_componentWillReceiveProps!=`function`&&typeof a.componentWillReceiveProps!=`function`||(o!==d||f!==c)&&Js(t,a,r,c),Va=!1,f=t.memoizedState,a.state=f,Xa(t,r,a,i),Ya();var p=t.memoizedState;o!==d||f!==p||Va||e!==null&&e.dependencies!==null&&ea(e.dependencies)?(typeof s==`function`&&(Gs(t,n,s,r),p=t.memoizedState),(u=Va||qs(t,n,u,r,f,p,c)||e!==null&&e.dependencies!==null&&ea(e.dependencies))?(l||typeof a.UNSAFE_componentWillUpdate!=`function`&&typeof a.componentWillUpdate!=`function`||(typeof a.componentWillUpdate==`function`&&a.componentWillUpdate(r,p,c),typeof a.UNSAFE_componentWillUpdate==`function`&&a.UNSAFE_componentWillUpdate(r,p,c)),typeof a.componentDidUpdate==`function`&&(t.flags|=4),typeof a.getSnapshotBeforeUpdate==`function`&&(t.flags|=1024)):(typeof a.componentDidUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=4),typeof a.getSnapshotBeforeUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=1024),t.memoizedProps=r,t.memoizedState=p),a.props=r,a.state=p,a.context=c,r=u):(typeof a.componentDidUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=4),typeof a.getSnapshotBeforeUpdate!=`function`||o===e.memoizedProps&&f===e.memoizedState||(t.flags|=1024),r=!1)}return a=r,_c(e,t),r=(t.flags&128)!=0,a||r?(a=t.stateNode,n=r&&typeof n.getDerivedStateFromError!=`function`?null:a.render(),t.flags|=1,e!==null&&r?(t.child=za(t,e.child,null,i),t.child=za(t,null,n,i)):sc(e,t,n,i),t.memoizedState=a.state,e=t.child):e=Mc(e,t,i),e}function xc(e,t,n,r){return Ui(),t.flags|=256,sc(e,t,n,r),t.child}var Sc={dehydrated:null,treeContext:null,retryLane:0,hydrationErrors:null};function Cc(e){return{baseLanes:e,cachePool:Sa()}}function wc(e,t,n){return e=e===null?0:e.childLanes&~n,t&&(e|=eu),e}function Tc(e,t,n){var r=t.pendingProps,a=!1,o=(t.flags&128)!=0,s;if((s=o)||(s=e!==null&&e.memoizedState===null?!1:(fo.current&2)!=0),s&&(a=!0,t.flags&=-129),s=(t.flags&32)!=0,t.flags&=-33,e===null){if(Ii){if(a?oo(t):lo(t),(e=Fi)?(e=df(e,Ri),e=e!==null&&e.data!==`&`?e:null,e!==null&&(t.memoizedState={dehydrated:e,treeContext:Ei===null?null:{id:Di,overflow:Oi},retryLane:536870912,hydrationErrors:null},n=gi(e),n.return=t,t.child=n,Pi=t,Fi=null)):e=null,e===null)throw R(t);return pf(e)?t.lanes=32:t.lanes=536870912,null}var c=r.children;return r=r.fallback,a?(lo(t),a=t.mode,c=Dc({mode:`hidden`,children:c},a),r=mi(r,a,n,null),c.return=t,r.return=t,c.sibling=r,t.child=c,r=t.child,r.memoizedState=Cc(n),r.childLanes=wc(e,s,n),t.memoizedState=Sc,fc(null,r)):(oo(t),Ec(t,c))}var l=e.memoizedState;if(l!==null&&(c=l.dehydrated,c!==null)){if(o)t.flags&256?(oo(t),t.flags&=-257,t=Oc(e,t,n)):t.memoizedState===null?(lo(t),c=r.fallback,a=t.mode,r=Dc({mode:`visible`,children:r.children},a),c=mi(c,a,n,null),c.flags|=2,r.return=t,c.return=t,r.sibling=c,t.child=r,za(t,e.child,null,n),r=t.child,r.memoizedState=Cc(n),r.childLanes=wc(e,s,n),t.memoizedState=Sc,t=fc(null,r)):(lo(t),t.child=e.child,t.flags|=128,t=null);else if(oo(t),pf(c)){if(s=c.nextSibling&&c.nextSibling.dataset,s)var u=s.dgst;s=u,r=Error(i(419)),r.stack=``,r.digest=s,Gi({value:r,source:null,stack:null}),t=Oc(e,t,n)}else if(oc||$i(e,t,n,!1),s=(n&e.childLanes)!==0,oc||s){if(s=Hl,s!==null&&(r=tt(s,n),r!==0&&r!==l.retryLane))throw l.retryLane=r,ii(e,r),xu(s,e,r),ac;ff(c)||Nu(),t=Oc(e,t,n)}else ff(c)?(t.flags|=192,t.child=e.child,t=null):(e=l.treeContext,Fi=hf(c.nextSibling),Pi=t,Ii=!0,Li=null,Ri=!1,e!==null&&Ni(t,e),t=Ec(t,r.children),t.flags|=4096);return t}return a?(lo(t),c=r.fallback,a=t.mode,l=e.child,u=l.sibling,r=di(l,{mode:`hidden`,children:r.children}),r.subtreeFlags=l.subtreeFlags&65011712,u===null?(c=mi(c,a,n,null),c.flags|=2):c=di(u,c),c.return=t,r.return=t,r.sibling=c,t.child=r,fc(null,r),r=t.child,c=e.child.memoizedState,c===null?c=Cc(n):(a=c.cachePool,a===null?a=Sa():(l=ca._currentValue,a=a.parent===l?a:{parent:l,pool:l}),c={baseLanes:c.baseLanes|n,cachePool:a}),r.memoizedState=c,r.childLanes=wc(e,s,n),t.memoizedState=Sc,fc(e.child,r)):(oo(t),n=e.child,e=n.sibling,n=di(n,{mode:`visible`,children:r.children}),n.return=t,n.sibling=null,e!==null&&(s=t.deletions,s===null?(t.deletions=[e],t.flags|=16):s.push(e)),t.child=n,t.memoizedState=null,n)}function Ec(e,t){return t=Dc({mode:`visible`,children:t},e.mode),t.return=e,e.child=t}function Dc(e,t){return e=li(22,e,null,t),e.lanes=0,e}function Oc(e,t,n){return za(t,e.child,null,n),e=Ec(t,t.pendingProps.children),e.flags|=2,t.memoizedState=null,e}function kc(e,t,n){e.lanes|=t;var r=e.alternate;r!==null&&(r.lanes|=t),Zi(e.return,t,n)}function Ac(e,t,n,r,i,a){var o=e.memoizedState;o===null?e.memoizedState={isBackwards:t,rendering:null,renderingStartTime:0,last:r,tail:n,tailMode:i,treeForkCount:a}:(o.isBackwards=t,o.rendering=null,o.renderingStartTime=0,o.last=r,o.tail=n,o.tailMode=i,o.treeForkCount=a)}function jc(e,t,n){var r=t.pendingProps,i=r.revealOrder,a=r.tail;r=r.children;var o=fo.current,s=(o&2)!=0;if(s?(o=o&1|2,t.flags|=128):o&=1,L(fo,o),sc(e,t,r,n),r=Ii?Ci:0,!s&&e!==null&&e.flags&128)a:for(e=t.child;e!==null;){if(e.tag===13)e.memoizedState!==null&&kc(e,n,t);else if(e.tag===19)kc(e,n,t);else if(e.child!==null){e.child.return=e,e=e.child;continue}if(e===t)break a;for(;e.sibling===null;){if(e.return===null||e.return===t)break a;e=e.return}e.sibling.return=e.return,e=e.sibling}switch(i){case`forwards`:for(n=t.child,i=null;n!==null;)e=n.alternate,e!==null&&po(e)===null&&(i=n),n=n.sibling;n=i,n===null?(i=t.child,t.child=null):(i=n.sibling,n.sibling=null),Ac(t,!1,i,n,a,r);break;case`backwards`:case`unstable_legacy-backwards`:for(n=null,i=t.child,t.child=null;i!==null;){if(e=i.alternate,e!==null&&po(e)===null){t.child=i;break}e=i.sibling,i.sibling=n,n=i,i=e}Ac(t,!0,n,null,a,r);break;case`together`:Ac(t,!1,null,null,void 0,r);break;default:t.memoizedState=null}return t.child}function Mc(e,t,n){if(e!==null&&(t.dependencies=e.dependencies),Zl|=t.lanes,(n&t.childLanes)===0)if(e!==null){if($i(e,t,n,!1),(n&t.childLanes)===0)return null}else return null;if(e!==null&&t.child!==e.child)throw Error(i(153));if(t.child!==null){for(e=t.child,n=di(e,e.pendingProps),t.child=n,n.return=t;e.sibling!==null;)e=e.sibling,n=n.sibling=di(e,e.pendingProps),n.return=t;n.sibling=null}return t.child}function Nc(e,t){return(e.lanes&t)===0?(e=e.dependencies,!!(e!==null&&ea(e))):!0}function Pc(e,t,n){switch(t.tag){case 3:ue(t,t.stateNode.containerInfo),Yi(t,ca,e.memoizedState.cache),Ui();break;case 27:case 5:fe(t);break;case 4:ue(t,t.stateNode.containerInfo);break;case 10:Yi(t,t.type,t.memoizedProps.value);break;case 31:if(t.memoizedState!==null)return t.flags|=128,so(t),null;break;case 13:var r=t.memoizedState;if(r!==null)return r.dehydrated===null?(n&t.child.childLanes)===0?(oo(t),e=Mc(e,t,n),e===null?null:e.sibling):Tc(e,t,n):(oo(t),t.flags|=128,null);oo(t);break;case 19:var i=(e.flags&128)!=0;if(r=(n&t.childLanes)!==0,r||=($i(e,t,n,!1),(n&t.childLanes)!==0),i){if(r)return jc(e,t,n);t.flags|=128}if(i=t.memoizedState,i!==null&&(i.rendering=null,i.tail=null,i.lastEffect=null),L(fo,fo.current),r)break;return null;case 22:return t.lanes=0,dc(e,t,n,t.pendingProps);case 24:Yi(t,ca,e.memoizedState.cache)}return Mc(e,t,n)}function Fc(e,t,n){if(e!==null)if(e.memoizedProps!==t.pendingProps)oc=!0;else{if(!Nc(e,n)&&!(t.flags&128))return oc=!1,Pc(e,t,n);oc=!!(e.flags&131072)}else oc=!1,Ii&&t.flags&1048576&&Ai(t,Ci,t.index);switch(t.lanes=0,t.tag){case 16:a:{var r=t.pendingProps;if(e=ka(t.elementType),t.type=e,typeof e==`function`)ui(e)?(r=Ys(e,r),t.tag=1,t=bc(null,t,e,r,n)):(t.tag=0,t=vc(null,t,e,r,n));else{if(e!=null){var a=e.$$typeof;if(a===w){t.tag=11,t=cc(null,t,e,r,n);break a}else if(a===D){t.tag=14,t=lc(null,t,e,r,n);break a}}throw t=ee(e)||e,Error(i(306,t,``))}}return t;case 0:return vc(e,t,t.type,t.pendingProps,n);case 1:return r=t.type,a=Ys(r,t.pendingProps),bc(e,t,r,a,n);case 3:a:{if(ue(t,t.stateNode.containerInfo),e===null)throw Error(i(387));r=t.pendingProps;var o=t.memoizedState;a=o.element,Ua(e,t),Xa(t,r,null,n);var s=t.memoizedState;if(r=s.cache,Yi(t,ca,r),r!==o.cache&&Qi(t,[ca],n,!0),Ya(),r=s.element,o.isDehydrated)if(o={element:r,isDehydrated:!1,cache:s.cache},t.updateQueue.baseState=o,t.memoizedState=o,t.flags&256){t=xc(e,t,r,n);break a}else if(r!==a){a=yi(Error(i(424)),t),Gi(a),t=xc(e,t,r,n);break a}else{switch(e=t.stateNode.containerInfo,e.nodeType){case 9:e=e.body;break;default:e=e.nodeName===`HTML`?e.ownerDocument.body:e}for(Fi=hf(e.firstChild),Pi=t,Ii=!0,Li=null,Ri=!0,n=Ba(t,null,r,n),t.child=n;n;)n.flags=n.flags&-3|4096,n=n.sibling}else{if(Ui(),r===a){t=Mc(e,t,n);break a}sc(e,t,r,n)}t=t.child}return t;case 26:return _c(e,t),e===null?(n=If(t.type,null,t.pendingProps,null))?t.memoizedState=n:Ii||(n=t.type,e=t.pendingProps,r=qd(ce.current).createElement(n),r[st]=t,r[ct]=e,Hd(r,n,e),bt(r),t.stateNode=r):t.memoizedState=If(t.type,e.memoizedProps,t.pendingProps,e.memoizedState),null;case 27:return fe(t),e===null&&Ii&&(r=t.stateNode=yf(t.type,t.pendingProps,ce.current),Pi=t,Ri=!0,a=Fi,af(t.type)?(gf=a,Fi=hf(r.firstChild)):Fi=a),sc(e,t,t.pendingProps.children,n),_c(e,t),e===null&&(t.flags|=4194304),t.child;case 5:return e===null&&Ii&&((a=r=Fi)&&(r=lf(r,t.type,t.pendingProps,Ri),r===null?a=!1:(t.stateNode=r,Pi=t,Fi=hf(r.firstChild),Ri=!1,a=!0)),a||R(t)),fe(t),a=t.type,o=t.pendingProps,s=e===null?null:e.memoizedProps,r=o.children,Xd(a,o)?r=null:s!==null&&Xd(a,s)&&(t.flags|=32),t.memoizedState!==null&&(a=Eo(e,t,ko,null,null,n),ap._currentValue=a),_c(e,t),sc(e,t,r,n),t.child;case 6:return e===null&&Ii&&((e=n=Fi)&&(n=uf(n,t.pendingProps,Ri),n===null?e=!1:(t.stateNode=n,Pi=t,Fi=null,e=!0)),e||R(t)),null;case 13:return Tc(e,t,n);case 4:return ue(t,t.stateNode.containerInfo),r=t.pendingProps,e===null?t.child=za(t,null,r,n):sc(e,t,r,n),t.child;case 11:return cc(e,t,t.type,t.pendingProps,n);case 7:return sc(e,t,t.pendingProps,n),t.child;case 8:return sc(e,t,t.pendingProps.children,n),t.child;case 12:return sc(e,t,t.pendingProps.children,n),t.child;case 10:return r=t.pendingProps,Yi(t,t.type,r.value),sc(e,t,r.children,n),t.child;case 9:return a=t.type._context,r=t.pendingProps.children,ta(t),a=na(a),r=r(a),t.flags|=1,sc(e,t,r,n),t.child;case 14:return lc(e,t,t.type,t.pendingProps,n);case 15:return uc(e,t,t.type,t.pendingProps,n);case 19:return jc(e,t,n);case 31:return gc(e,t,n);case 22:return dc(e,t,n,t.pendingProps);case 24:return ta(t),r=na(ca),e===null?(a=ba(),a===null&&(a=Hl,o=la(),a.pooledCache=o,o.refCount++,o!==null&&(a.pooledCacheLanes|=n),a=o),t.memoizedState={parent:r,cache:a},Ha(t),Yi(t,ca,a)):((e.lanes&n)!==0&&(Ua(e,t),Xa(t,null,null,n),Ya()),a=e.memoizedState,o=t.memoizedState,a.parent===r?(r=o.cache,Yi(t,ca,r),r!==a.cache&&Qi(t,[ca],n,!0)):(a={parent:r,cache:r},t.memoizedState=a,t.lanes===0&&(t.memoizedState=t.updateQueue.baseState=a),Yi(t,ca,r))),sc(e,t,t.pendingProps.children,n),t.child;case 29:throw t.pendingProps}throw Error(i(156,t.tag))}function Ic(e){e.flags|=4}function Lc(e,t,n,r,i){if((t=(e.mode&32)!=0)&&(t=!1),t){if(e.flags|=16777216,(i&335544128)===i)if(e.stateNode.complete)e.flags|=8192;else if(Au())e.flags|=8192;else throw Aa=Ea,wa}else e.flags&=-16777217}function Rc(e,t){if(t.type!==`stylesheet`||t.state.loading&4)e.flags&=-16777217;else if(e.flags|=16777216,!Zf(t))if(Au())e.flags|=8192;else throw Aa=Ea,wa}function zc(e,t){t!==null&&(e.flags|=4),e.flags&16384&&(t=e.tag===22?536870912:Ye(),e.lanes|=t,tu|=t)}function Bc(e,t){if(!Ii)switch(e.tailMode){case`hidden`:t=e.tail;for(var n=null;t!==null;)t.alternate!==null&&(n=t),t=t.sibling;n===null?e.tail=null:n.sibling=null;break;case`collapsed`:n=e.tail;for(var r=null;n!==null;)n.alternate!==null&&(r=n),n=n.sibling;r===null?t||e.tail===null?e.tail=null:e.tail.sibling=null:r.sibling=null}}function Vc(e){var t=e.alternate!==null&&e.alternate.child===e.child,n=0,r=0;if(t)for(var i=e.child;i!==null;)n|=i.lanes|i.childLanes,r|=i.subtreeFlags&65011712,r|=i.flags&65011712,i.return=e,i=i.sibling;else for(i=e.child;i!==null;)n|=i.lanes|i.childLanes,r|=i.subtreeFlags,r|=i.flags,i.return=e,i=i.sibling;return e.subtreeFlags|=r,e.childLanes=n,t}function Hc(e,t,n){var r=t.pendingProps;switch(Mi(t),t.tag){case 16:case 15:case 0:case 11:case 7:case 8:case 12:case 9:case 14:return Vc(t),null;case 1:return Vc(t),null;case 3:return n=t.stateNode,r=null,e!==null&&(r=e.memoizedState.cache),t.memoizedState.cache!==r&&(t.flags|=2048),Xi(ca),de(),n.pendingContext&&=(n.context=n.pendingContext,null),(e===null||e.child===null)&&(Hi(t)?Ic(t):e===null||e.memoizedState.isDehydrated&&!(t.flags&256)||(t.flags|=1024,Wi())),Vc(t),null;case 26:var a=t.type,o=t.memoizedState;return e===null?(Ic(t),o===null?(Vc(t),Lc(t,a,null,r,n)):(Vc(t),Rc(t,o))):o?o===e.memoizedState?(Vc(t),t.flags&=-16777217):(Ic(t),Vc(t),Rc(t,o)):(e=e.memoizedProps,e!==r&&Ic(t),Vc(t),Lc(t,a,e,r,n)),null;case 27:if(pe(t),n=ce.current,a=t.type,e!==null&&t.stateNode!=null)e.memoizedProps!==r&&Ic(t);else{if(!r){if(t.stateNode===null)throw Error(i(166));return Vc(t),null}e=oe.current,Hi(t)?Bi(t,e):(e=yf(a,r,n),t.stateNode=e,Ic(t))}return Vc(t),null;case 5:if(pe(t),a=t.type,e!==null&&t.stateNode!=null)e.memoizedProps!==r&&Ic(t);else{if(!r){if(t.stateNode===null)throw Error(i(166));return Vc(t),null}if(o=oe.current,Hi(t))Bi(t,o);else{var s=qd(ce.current);switch(o){case 1:o=s.createElementNS(`http://www.w3.org/2000/svg`,a);break;case 2:o=s.createElementNS(`http://www.w3.org/1998/Math/MathML`,a);break;default:switch(a){case`svg`:o=s.createElementNS(`http://www.w3.org/2000/svg`,a);break;case`math`:o=s.createElementNS(`http://www.w3.org/1998/Math/MathML`,a);break;case`script`:o=s.createElement(`div`),o.innerHTML=`<script><\/script>`,o=o.removeChild(o.firstChild);break;case`select`:o=typeof r.is==`string`?s.createElement(`select`,{is:r.is}):s.createElement(`select`),r.multiple?o.multiple=!0:r.size&&(o.size=r.size);break;default:o=typeof r.is==`string`?s.createElement(a,{is:r.is}):s.createElement(a)}}o[st]=t,o[ct]=r;a:for(s=t.child;s!==null;){if(s.tag===5||s.tag===6)o.appendChild(s.stateNode);else if(s.tag!==4&&s.tag!==27&&s.child!==null){s.child.return=s,s=s.child;continue}if(s===t)break a;for(;s.sibling===null;){if(s.return===null||s.return===t)break a;s=s.return}s.sibling.return=s.return,s=s.sibling}t.stateNode=o;a:switch(Hd(o,a,r),a){case`button`:case`input`:case`select`:case`textarea`:r=!!r.autoFocus;break a;case`img`:r=!0;break a;default:r=!1}r&&Ic(t)}}return Vc(t),Lc(t,t.type,e===null?null:e.memoizedProps,t.pendingProps,n),null;case 6:if(e&&t.stateNode!=null)e.memoizedProps!==r&&Ic(t);else{if(typeof r!=`string`&&t.stateNode===null)throw Error(i(166));if(e=ce.current,Hi(t)){if(e=t.stateNode,n=t.memoizedProps,r=null,a=Pi,a!==null)switch(a.tag){case 27:case 5:r=a.memoizedProps}e[st]=t,e=!!(e.nodeValue===n||r!==null&&!0===r.suppressHydrationWarning||zd(e.nodeValue,n)),e||R(t,!0)}else e=qd(e).createTextNode(r),e[st]=t,t.stateNode=e}return Vc(t),null;case 31:if(n=t.memoizedState,e===null||e.memoizedState!==null){if(r=Hi(t),n!==null){if(e===null){if(!r)throw Error(i(318));if(e=t.memoizedState,e=e===null?null:e.dehydrated,!e)throw Error(i(557));e[st]=t}else Ui(),!(t.flags&128)&&(t.memoizedState=null),t.flags|=4;Vc(t),e=!1}else n=Wi(),e!==null&&e.memoizedState!==null&&(e.memoizedState.hydrationErrors=n),e=!0;if(!e)return t.flags&256?(uo(t),t):(uo(t),null);if(t.flags&128)throw Error(i(558))}return Vc(t),null;case 13:if(r=t.memoizedState,e===null||e.memoizedState!==null&&e.memoizedState.dehydrated!==null){if(a=Hi(t),r!==null&&r.dehydrated!==null){if(e===null){if(!a)throw Error(i(318));if(a=t.memoizedState,a=a===null?null:a.dehydrated,!a)throw Error(i(317));a[st]=t}else Ui(),!(t.flags&128)&&(t.memoizedState=null),t.flags|=4;Vc(t),a=!1}else a=Wi(),e!==null&&e.memoizedState!==null&&(e.memoizedState.hydrationErrors=a),a=!0;if(!a)return t.flags&256?(uo(t),t):(uo(t),null)}return uo(t),t.flags&128?(t.lanes=n,t):(n=r!==null,e=e!==null&&e.memoizedState!==null,n&&(r=t.child,a=null,r.alternate!==null&&r.alternate.memoizedState!==null&&r.alternate.memoizedState.cachePool!==null&&(a=r.alternate.memoizedState.cachePool.pool),o=null,r.memoizedState!==null&&r.memoizedState.cachePool!==null&&(o=r.memoizedState.cachePool.pool),o!==a&&(r.flags|=2048)),n!==e&&n&&(t.child.flags|=8192),zc(t,t.updateQueue),Vc(t),null);case 4:return de(),e===null&&kd(t.stateNode.containerInfo),Vc(t),null;case 10:return Xi(t.type),Vc(t),null;case 19:if(ae(fo),r=t.memoizedState,r===null)return Vc(t),null;if(a=(t.flags&128)!=0,o=r.rendering,o===null)if(a)Bc(r,!1);else{if(Xl!==0||e!==null&&e.flags&128)for(e=t.child;e!==null;){if(o=po(e),o!==null){for(t.flags|=128,Bc(r,!1),e=o.updateQueue,t.updateQueue=e,zc(t,e),t.subtreeFlags=0,e=n,n=t.child;n!==null;)fi(n,e),n=n.sibling;return L(fo,fo.current&1|2),Ii&&ki(t,r.treeForkCount),t.child}e=e.sibling}r.tail!==null&&Ee()>su&&(t.flags|=128,a=!0,Bc(r,!1),t.lanes=4194304)}else{if(!a)if(e=po(o),e!==null){if(t.flags|=128,a=!0,e=e.updateQueue,t.updateQueue=e,zc(t,e),Bc(r,!0),r.tail===null&&r.tailMode===`hidden`&&!o.alternate&&!Ii)return Vc(t),null}else 2*Ee()-r.renderingStartTime>su&&n!==536870912&&(t.flags|=128,a=!0,Bc(r,!1),t.lanes=4194304);r.isBackwards?(o.sibling=t.child,t.child=o):(e=r.last,e===null?t.child=o:e.sibling=o,r.last=o)}return r.tail===null?(Vc(t),null):(e=r.tail,r.rendering=e,r.tail=e.sibling,r.renderingStartTime=Ee(),e.sibling=null,n=fo.current,L(fo,a?n&1|2:n&1),Ii&&ki(t,r.treeForkCount),e);case 22:case 23:return uo(t),ro(),r=t.memoizedState!==null,e===null?r&&(t.flags|=8192):e.memoizedState!==null!==r&&(t.flags|=8192),r?n&536870912&&!(t.flags&128)&&(Vc(t),t.subtreeFlags&6&&(t.flags|=8192)):Vc(t),n=t.updateQueue,n!==null&&zc(t,n.retryQueue),n=null,e!==null&&e.memoizedState!==null&&e.memoizedState.cachePool!==null&&(n=e.memoizedState.cachePool.pool),r=null,t.memoizedState!==null&&t.memoizedState.cachePool!==null&&(r=t.memoizedState.cachePool.pool),r!==n&&(t.flags|=2048),e!==null&&ae(ya),null;case 24:return n=null,e!==null&&(n=e.memoizedState.cache),t.memoizedState.cache!==n&&(t.flags|=2048),Xi(ca),Vc(t),null;case 25:return null;case 30:return null}throw Error(i(156,t.tag))}function Uc(e,t){switch(Mi(t),t.tag){case 1:return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 3:return Xi(ca),de(),e=t.flags,e&65536&&!(e&128)?(t.flags=e&-65537|128,t):null;case 26:case 27:case 5:return pe(t),null;case 31:if(t.memoizedState!==null){if(uo(t),t.alternate===null)throw Error(i(340));Ui()}return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 13:if(uo(t),e=t.memoizedState,e!==null&&e.dehydrated!==null){if(t.alternate===null)throw Error(i(340));Ui()}return e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 19:return ae(fo),null;case 4:return de(),null;case 10:return Xi(t.type),null;case 22:case 23:return uo(t),ro(),e!==null&&ae(ya),e=t.flags,e&65536?(t.flags=e&-65537|128,t):null;case 24:return Xi(ca),null;case 25:return null;default:return null}}function Wc(e,t){switch(Mi(t),t.tag){case 3:Xi(ca),de();break;case 26:case 27:case 5:pe(t);break;case 4:de();break;case 31:t.memoizedState!==null&&uo(t);break;case 13:uo(t);break;case 19:ae(fo);break;case 10:Xi(t.type);break;case 22:case 23:uo(t),ro(),e!==null&&ae(ya);break;case 24:Xi(ca)}}function Gc(e,t){try{var n=t.updateQueue,r=n===null?null:n.lastEffect;if(r!==null){var i=r.next;n=i;do{if((n.tag&e)===e){r=void 0;var a=n.create,o=n.inst;r=a(),o.destroy=r}n=n.next}while(n!==i)}}catch(e){Zu(t,t.return,e)}}function Kc(e,t,n){try{var r=t.updateQueue,i=r===null?null:r.lastEffect;if(i!==null){var a=i.next;r=a;do{if((r.tag&e)===e){var o=r.inst,s=o.destroy;if(s!==void 0){o.destroy=void 0,i=t;var c=n,l=s;try{l()}catch(e){Zu(i,c,e)}}}r=r.next}while(r!==a)}}catch(e){Zu(t,t.return,e)}}function qc(e){var t=e.updateQueue;if(t!==null){var n=e.stateNode;try{Qa(t,n)}catch(t){Zu(e,e.return,t)}}}function Jc(e,t,n){n.props=Ys(e.type,e.memoizedProps),n.state=e.memoizedState;try{n.componentWillUnmount()}catch(n){Zu(e,t,n)}}function Yc(e,t){try{var n=e.ref;if(n!==null){switch(e.tag){case 26:case 27:case 5:var r=e.stateNode;break;case 30:r=e.stateNode;break;default:r=e.stateNode}typeof n==`function`?e.refCleanup=n(r):n.current=r}}catch(n){Zu(e,t,n)}}function Xc(e,t){var n=e.ref,r=e.refCleanup;if(n!==null)if(typeof r==`function`)try{r()}catch(n){Zu(e,t,n)}finally{e.refCleanup=null,e=e.alternate,e!=null&&(e.refCleanup=null)}else if(typeof n==`function`)try{n(null)}catch(n){Zu(e,t,n)}else n.current=null}function Zc(e){var t=e.type,n=e.memoizedProps,r=e.stateNode;try{a:switch(t){case`button`:case`input`:case`select`:case`textarea`:n.autoFocus&&r.focus();break a;case`img`:n.src?r.src=n.src:n.srcSet&&(r.srcset=n.srcSet)}}catch(t){Zu(e,e.return,t)}}function Qc(e,t,n){try{var r=e.stateNode;Ud(r,e.type,n,t),r[ct]=t}catch(t){Zu(e,e.return,t)}}function $c(e){return e.tag===5||e.tag===3||e.tag===26||e.tag===27&&af(e.type)||e.tag===4}function el(e){a:for(;;){for(;e.sibling===null;){if(e.return===null||$c(e.return))return null;e=e.return}for(e.sibling.return=e.return,e=e.sibling;e.tag!==5&&e.tag!==6&&e.tag!==18;){if(e.tag===27&&af(e.type)||e.flags&2||e.child===null||e.tag===4)continue a;e.child.return=e,e=e.child}if(!(e.flags&2))return e.stateNode}}function tl(e,t,n){var r=e.tag;if(r===5||r===6)e=e.stateNode,t?(n.nodeType===9?n.body:n.nodeName===`HTML`?n.ownerDocument.body:n).insertBefore(e,t):(t=n.nodeType===9?n.body:n.nodeName===`HTML`?n.ownerDocument.body:n,t.appendChild(e),n=n._reactRootContainer,n!=null||t.onclick!==null||(t.onclick=en));else if(r!==4&&(r===27&&af(e.type)&&(n=e.stateNode,t=null),e=e.child,e!==null))for(tl(e,t,n),e=e.sibling;e!==null;)tl(e,t,n),e=e.sibling}function nl(e,t,n){var r=e.tag;if(r===5||r===6)e=e.stateNode,t?n.insertBefore(e,t):n.appendChild(e);else if(r!==4&&(r===27&&af(e.type)&&(n=e.stateNode),e=e.child,e!==null))for(nl(e,t,n),e=e.sibling;e!==null;)nl(e,t,n),e=e.sibling}function V(e){var t=e.stateNode,n=e.memoizedProps;try{for(var r=e.type,i=t.attributes;i.length;)t.removeAttributeNode(i[0]);Hd(t,r,n),t[st]=e,t[ct]=n}catch(t){Zu(e,e.return,t)}}var rl=!1,il=!1,al=!1,ol=typeof WeakSet==`function`?WeakSet:Set,sl=null;function cl(e,t){if(e=e.containerInfo,Kd=mp,e=Or(e),kr(e)){if(`selectionStart`in e)var n={start:e.selectionStart,end:e.selectionEnd};else a:{n=(n=e.ownerDocument)&&n.defaultView||window;var r=n.getSelection&&n.getSelection();if(r&&r.rangeCount!==0){n=r.anchorNode;var a=r.anchorOffset,o=r.focusNode;r=r.focusOffset;try{n.nodeType,o.nodeType}catch{n=null;break a}var s=0,c=-1,l=-1,u=0,d=0,f=e,p=null;b:for(;;){for(var m;f!==n||a!==0&&f.nodeType!==3||(c=s+a),f!==o||r!==0&&f.nodeType!==3||(l=s+r),f.nodeType===3&&(s+=f.nodeValue.length),(m=f.firstChild)!==null;)p=f,f=m;for(;;){if(f===e)break b;if(p===n&&++u===a&&(c=s),p===o&&++d===r&&(l=s),(m=f.nextSibling)!==null)break;f=p,p=f.parentNode}f=m}n=c===-1||l===-1?null:{start:c,end:l}}else n=null}n||={start:0,end:0}}else n=null;for(K={focusedElem:e,selectionRange:n},mp=!1,sl=t;sl!==null;)if(t=sl,e=t.child,t.subtreeFlags&1028&&e!==null)e.return=t,sl=e;else for(;sl!==null;){switch(t=sl,o=t.alternate,e=t.flags,t.tag){case 0:if(e&4&&(e=t.updateQueue,e=e===null?null:e.events,e!==null))for(n=0;n<e.length;n++)a=e[n],a.ref.impl=a.nextImpl;break;case 11:case 15:break;case 1:if(e&1024&&o!==null){e=void 0,n=t,a=o.memoizedProps,o=o.memoizedState,r=n.stateNode;try{var h=Ys(n.type,a);e=r.getSnapshotBeforeUpdate(h,o),r.__reactInternalSnapshotBeforeUpdate=e}catch(e){Zu(n,n.return,e)}}break;case 3:if(e&1024){if(e=t.stateNode.containerInfo,n=e.nodeType,n===9)cf(e);else if(n===1)switch(e.nodeName){case`HEAD`:case`HTML`:case`BODY`:cf(e);break;default:e.textContent=``}}break;case 5:case 26:case 27:case 6:case 4:case 17:break;default:if(e&1024)throw Error(i(163))}if(e=t.sibling,e!==null){e.return=t.return,sl=e;break}sl=t.return}}function ll(e,t,n){var r=n.flags;switch(n.tag){case 0:case 11:case 15:Cl(e,n),r&4&&Gc(5,n);break;case 1:if(Cl(e,n),r&4)if(e=n.stateNode,t===null)try{e.componentDidMount()}catch(e){Zu(n,n.return,e)}else{var i=Ys(n.type,t.memoizedProps);t=t.memoizedState;try{e.componentDidUpdate(i,t,e.__reactInternalSnapshotBeforeUpdate)}catch(e){Zu(n,n.return,e)}}r&64&&qc(n),r&512&&Yc(n,n.return);break;case 3:if(Cl(e,n),r&64&&(e=n.updateQueue,e!==null)){if(t=null,n.child!==null)switch(n.child.tag){case 27:case 5:t=n.child.stateNode;break;case 1:t=n.child.stateNode}try{Qa(e,t)}catch(e){Zu(n,n.return,e)}}break;case 27:t===null&&r&4&&V(n);case 26:case 5:Cl(e,n),t===null&&r&4&&Zc(n),r&512&&Yc(n,n.return);break;case 12:Cl(e,n);break;case 31:Cl(e,n),r&4&&hl(e,n);break;case 13:Cl(e,n),r&4&&gl(e,n),r&64&&(e=n.memoizedState,e!==null&&(e=e.dehydrated,e!==null&&(n=td.bind(null,n),mf(e,n))));break;case 22:if(r=n.memoizedState!==null||rl,!r){t=t!==null&&t.memoizedState!==null||il,i=rl;var a=il;rl=r,(il=t)&&!a?wl(e,n,(n.subtreeFlags&8772)!=0):Cl(e,n),rl=i,il=a}break;case 30:break;default:Cl(e,n)}}function ul(e){var t=e.alternate;t!==null&&(e.alternate=null,ul(t)),e.child=null,e.deletions=null,e.sibling=null,e.tag===5&&(t=e.stateNode,t!==null&&ht(t)),e.stateNode=null,e.return=null,e.dependencies=null,e.memoizedProps=null,e.memoizedState=null,e.pendingProps=null,e.stateNode=null,e.updateQueue=null}var dl=null,fl=!1;function pl(e,t,n){for(n=n.child;n!==null;)ml(e,t,n),n=n.sibling}function ml(e,t,n){if(Ie&&typeof Ie.onCommitFiberUnmount==`function`)try{Ie.onCommitFiberUnmount(Fe,n)}catch{}switch(n.tag){case 26:il||Xc(n,t),pl(e,t,n),n.memoizedState?n.memoizedState.count--:n.stateNode&&(n=n.stateNode,n.parentNode.removeChild(n));break;case 27:il||Xc(n,t);var r=dl,i=fl;af(n.type)&&(dl=n.stateNode,fl=!1),pl(e,t,n),bf(n.stateNode),dl=r,fl=i;break;case 5:il||Xc(n,t);case 6:if(r=dl,i=fl,dl=null,pl(e,t,n),dl=r,fl=i,dl!==null)if(fl)try{(dl.nodeType===9?dl.body:dl.nodeName===`HTML`?dl.ownerDocument.body:dl).removeChild(n.stateNode)}catch(e){Zu(n,t,e)}else try{dl.removeChild(n.stateNode)}catch(e){Zu(n,t,e)}break;case 18:dl!==null&&(fl?(e=dl,of(e.nodeType===9?e.body:e.nodeName===`HTML`?e.ownerDocument.body:e,n.stateNode),Bp(e)):of(dl,n.stateNode));break;case 4:r=dl,i=fl,dl=n.stateNode.containerInfo,fl=!0,pl(e,t,n),dl=r,fl=i;break;case 0:case 11:case 14:case 15:Kc(2,n,t),il||Kc(4,n,t),pl(e,t,n);break;case 1:il||(Xc(n,t),r=n.stateNode,typeof r.componentWillUnmount==`function`&&Jc(n,t,r)),pl(e,t,n);break;case 21:pl(e,t,n);break;case 22:il=(r=il)||n.memoizedState!==null,pl(e,t,n),il=r;break;default:pl(e,t,n)}}function hl(e,t){if(t.memoizedState===null&&(e=t.alternate,e!==null&&(e=e.memoizedState,e!==null))){e=e.dehydrated;try{Bp(e)}catch(e){Zu(t,t.return,e)}}}function gl(e,t){if(t.memoizedState===null&&(e=t.alternate,e!==null&&(e=e.memoizedState,e!==null&&(e=e.dehydrated,e!==null))))try{Bp(e)}catch(e){Zu(t,t.return,e)}}function H(e){switch(e.tag){case 31:case 13:case 19:var t=e.stateNode;return t===null&&(t=e.stateNode=new ol),t;case 22:return e=e.stateNode,t=e._retryCache,t===null&&(t=e._retryCache=new ol),t;default:throw Error(i(435,e.tag))}}function _l(e,t){var n=H(e);t.forEach(function(t){if(!n.has(t)){n.add(t);var r=nd.bind(null,e,t);t.then(r,r)}})}function vl(e,t){var n=t.deletions;if(n!==null)for(var r=0;r<n.length;r++){var a=n[r],o=e,s=t,c=s;a:for(;c!==null;){switch(c.tag){case 27:if(af(c.type)){dl=c.stateNode,fl=!1;break a}break;case 5:dl=c.stateNode,fl=!1;break a;case 3:case 4:dl=c.stateNode.containerInfo,fl=!0;break a}c=c.return}if(dl===null)throw Error(i(160));ml(o,s,a),dl=null,fl=!1,o=a.alternate,o!==null&&(o.return=null),a.return=null}if(t.subtreeFlags&13886)for(t=t.child;t!==null;)bl(t,e),t=t.sibling}var yl=null;function bl(e,t){var n=e.alternate,r=e.flags;switch(e.tag){case 0:case 11:case 14:case 15:vl(t,e),xl(e),r&4&&(Kc(3,e,e.return),Gc(3,e),Kc(5,e,e.return));break;case 1:vl(t,e),xl(e),r&512&&(il||n===null||Xc(n,n.return)),r&64&&rl&&(e=e.updateQueue,e!==null&&(r=e.callbacks,r!==null&&(n=e.shared.hiddenCallbacks,e.shared.hiddenCallbacks=n===null?r:n.concat(r))));break;case 26:var a=yl;if(vl(t,e),xl(e),r&512&&(il||n===null||Xc(n,n.return)),r&4){var o=n===null?null:n.memoizedState;if(r=e.memoizedState,n===null)if(r===null)if(e.stateNode===null){a:{r=e.type,n=e.memoizedProps,a=a.ownerDocument||a;b:switch(r){case`title`:o=a.getElementsByTagName(`title`)[0],(!o||o[mt]||o[st]||o.namespaceURI===`http://www.w3.org/2000/svg`||o.hasAttribute(`itemprop`))&&(o=a.createElement(r),a.head.insertBefore(o,a.querySelector(`head > title`))),Hd(o,r,n),o[st]=e,bt(o),r=o;break a;case`link`:var s=Jf(`link`,`href`,a).get(r+(n.href||``));if(s){for(var c=0;c<s.length;c++)if(o=s[c],o.getAttribute(`href`)===(n.href==null||n.href===``?null:n.href)&&o.getAttribute(`rel`)===(n.rel==null?null:n.rel)&&o.getAttribute(`title`)===(n.title==null?null:n.title)&&o.getAttribute(`crossorigin`)===(n.crossOrigin==null?null:n.crossOrigin)){s.splice(c,1);break b}}o=a.createElement(r),Hd(o,r,n),a.head.appendChild(o);break;case`meta`:if(s=Jf(`meta`,`content`,a).get(r+(n.content||``))){for(c=0;c<s.length;c++)if(o=s[c],o.getAttribute(`content`)===(n.content==null?null:``+n.content)&&o.getAttribute(`name`)===(n.name==null?null:n.name)&&o.getAttribute(`property`)===(n.property==null?null:n.property)&&o.getAttribute(`http-equiv`)===(n.httpEquiv==null?null:n.httpEquiv)&&o.getAttribute(`charset`)===(n.charSet==null?null:n.charSet)){s.splice(c,1);break b}}o=a.createElement(r),Hd(o,r,n),a.head.appendChild(o);break;default:throw Error(i(468,r))}o[st]=e,bt(o),r=o}e.stateNode=r}else Yf(a,e.type,e.stateNode);else e.stateNode=Uf(a,r,e.memoizedProps);else o===r?r===null&&e.stateNode!==null&&Qc(e,e.memoizedProps,n.memoizedProps):(o===null?n.stateNode!==null&&(n=n.stateNode,n.parentNode.removeChild(n)):o.count--,r===null?Yf(a,e.type,e.stateNode):Uf(a,r,e.memoizedProps))}break;case 27:vl(t,e),xl(e),r&512&&(il||n===null||Xc(n,n.return)),n!==null&&r&4&&Qc(e,e.memoizedProps,n.memoizedProps);break;case 5:if(vl(t,e),xl(e),r&512&&(il||n===null||Xc(n,n.return)),e.flags&32){a=e.stateNode;try{Kt(a,``)}catch(t){Zu(e,e.return,t)}}r&4&&e.stateNode!=null&&(a=e.memoizedProps,Qc(e,a,n===null?a:n.memoizedProps)),r&1024&&(al=!0);break;case 6:if(vl(t,e),xl(e),r&4){if(e.stateNode===null)throw Error(i(162));r=e.memoizedProps,n=e.stateNode;try{n.nodeValue=r}catch(t){Zu(e,e.return,t)}}break;case 3:if(qf=null,a=yl,yl=Cf(t.containerInfo),vl(t,e),yl=a,xl(e),r&4&&n!==null&&n.memoizedState.isDehydrated)try{Bp(t.containerInfo)}catch(t){Zu(e,e.return,t)}al&&(al=!1,Sl(e));break;case 4:r=yl,yl=Cf(e.stateNode.containerInfo),vl(t,e),xl(e),yl=r;break;case 12:vl(t,e),xl(e);break;case 31:vl(t,e),xl(e),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,_l(e,r)));break;case 13:vl(t,e),xl(e),e.child.flags&8192&&e.memoizedState!==null!=(n!==null&&n.memoizedState!==null)&&(au=Ee()),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,_l(e,r)));break;case 22:a=e.memoizedState!==null;var l=n!==null&&n.memoizedState!==null,u=rl,d=il;if(rl=u||a,il=d||l,vl(t,e),il=d,rl=u,xl(e),r&8192)a:for(t=e.stateNode,t._visibility=a?t._visibility&-2:t._visibility|1,a&&(n===null||l||rl||il||U(e)),n=null,t=e;;){if(t.tag===5||t.tag===26){if(n===null){l=n=t;try{if(o=l.stateNode,a)s=o.style,typeof s.setProperty==`function`?s.setProperty(`display`,`none`,`important`):s.display=`none`;else{c=l.stateNode;var f=l.memoizedProps.style,p=f!=null&&f.hasOwnProperty(`display`)?f.display:null;c.style.display=p==null||typeof p==`boolean`?``:(``+p).trim()}}catch(e){Zu(l,l.return,e)}}}else if(t.tag===6){if(n===null){l=t;try{l.stateNode.nodeValue=a?``:l.memoizedProps}catch(e){Zu(l,l.return,e)}}}else if(t.tag===18){if(n===null){l=t;try{var m=l.stateNode;a?sf(m,!0):sf(l.stateNode,!1)}catch(e){Zu(l,l.return,e)}}}else if((t.tag!==22&&t.tag!==23||t.memoizedState===null||t===e)&&t.child!==null){t.child.return=t,t=t.child;continue}if(t===e)break a;for(;t.sibling===null;){if(t.return===null||t.return===e)break a;n===t&&(n=null),t=t.return}n===t&&(n=null),t.sibling.return=t.return,t=t.sibling}r&4&&(r=e.updateQueue,r!==null&&(n=r.retryQueue,n!==null&&(r.retryQueue=null,_l(e,n))));break;case 19:vl(t,e),xl(e),r&4&&(r=e.updateQueue,r!==null&&(e.updateQueue=null,_l(e,r)));break;case 30:break;case 21:break;default:vl(t,e),xl(e)}}function xl(e){var t=e.flags;if(t&2){try{for(var n,r=e.return;r!==null;){if($c(r)){n=r;break}r=r.return}if(n==null)throw Error(i(160));switch(n.tag){case 27:var a=n.stateNode;nl(e,el(e),a);break;case 5:var o=n.stateNode;n.flags&32&&(Kt(o,``),n.flags&=-33),nl(e,el(e),o);break;case 3:case 4:var s=n.stateNode.containerInfo;tl(e,el(e),s);break;default:throw Error(i(161))}}catch(t){Zu(e,e.return,t)}e.flags&=-3}t&4096&&(e.flags&=-4097)}function Sl(e){if(e.subtreeFlags&1024)for(e=e.child;e!==null;){var t=e;Sl(t),t.tag===5&&t.flags&1024&&t.stateNode.reset(),e=e.sibling}}function Cl(e,t){if(t.subtreeFlags&8772)for(t=t.child;t!==null;)ll(e,t.alternate,t),t=t.sibling}function U(e){for(e=e.child;e!==null;){var t=e;switch(t.tag){case 0:case 11:case 14:case 15:Kc(4,t,t.return),U(t);break;case 1:Xc(t,t.return);var n=t.stateNode;typeof n.componentWillUnmount==`function`&&Jc(t,t.return,n),U(t);break;case 27:bf(t.stateNode);case 26:case 5:Xc(t,t.return),U(t);break;case 22:t.memoizedState===null&&U(t);break;case 30:U(t);break;default:U(t)}e=e.sibling}}function wl(e,t,n){for(n&&=(t.subtreeFlags&8772)!=0,t=t.child;t!==null;){var r=t.alternate,i=e,a=t,o=a.flags;switch(a.tag){case 0:case 11:case 15:wl(i,a,n),Gc(4,a);break;case 1:if(wl(i,a,n),r=a,i=r.stateNode,typeof i.componentDidMount==`function`)try{i.componentDidMount()}catch(e){Zu(r,r.return,e)}if(r=a,i=r.updateQueue,i!==null){var s=r.stateNode;try{var c=i.shared.hiddenCallbacks;if(c!==null)for(i.shared.hiddenCallbacks=null,i=0;i<c.length;i++)Za(c[i],s)}catch(e){Zu(r,r.return,e)}}n&&o&64&&qc(a),Yc(a,a.return);break;case 27:V(a);case 26:case 5:wl(i,a,n),n&&r===null&&o&4&&Zc(a),Yc(a,a.return);break;case 12:wl(i,a,n);break;case 31:wl(i,a,n),n&&o&4&&hl(i,a);break;case 13:wl(i,a,n),n&&o&4&&gl(i,a);break;case 22:a.memoizedState===null&&wl(i,a,n),Yc(a,a.return);break;case 30:break;default:wl(i,a,n)}t=t.sibling}}function Tl(e,t){var n=null;e!==null&&e.memoizedState!==null&&e.memoizedState.cachePool!==null&&(n=e.memoizedState.cachePool.pool),e=null,t.memoizedState!==null&&t.memoizedState.cachePool!==null&&(e=t.memoizedState.cachePool.pool),e!==n&&(e!=null&&e.refCount++,n!=null&&ua(n))}function El(e,t){e=null,t.alternate!==null&&(e=t.alternate.memoizedState.cache),t=t.memoizedState.cache,t!==e&&(t.refCount++,e!=null&&ua(e))}function Dl(e,t,n,r){if(t.subtreeFlags&10256)for(t=t.child;t!==null;)Ol(e,t,n,r),t=t.sibling}function Ol(e,t,n,r){var i=t.flags;switch(t.tag){case 0:case 11:case 15:Dl(e,t,n,r),i&2048&&Gc(9,t);break;case 1:Dl(e,t,n,r);break;case 3:Dl(e,t,n,r),i&2048&&(e=null,t.alternate!==null&&(e=t.alternate.memoizedState.cache),t=t.memoizedState.cache,t!==e&&(t.refCount++,e!=null&&ua(e)));break;case 12:if(i&2048){Dl(e,t,n,r),e=t.stateNode;try{var a=t.memoizedProps,o=a.id,s=a.onPostCommit;typeof s==`function`&&s(o,t.alternate===null?`mount`:`update`,e.passiveEffectDuration,-0)}catch(e){Zu(t,t.return,e)}}else Dl(e,t,n,r);break;case 31:Dl(e,t,n,r);break;case 13:Dl(e,t,n,r);break;case 23:break;case 22:a=t.stateNode,o=t.alternate,t.memoizedState===null?a._visibility&2?Dl(e,t,n,r):(a._visibility|=2,kl(e,t,n,r,(t.subtreeFlags&10256)!=0||!1)):a._visibility&2?Dl(e,t,n,r):Al(e,t),i&2048&&Tl(o,t);break;case 24:Dl(e,t,n,r),i&2048&&El(t.alternate,t);break;default:Dl(e,t,n,r)}}function kl(e,t,n,r,i){for(i&&=(t.subtreeFlags&10256)!=0||!1,t=t.child;t!==null;){var a=e,o=t,s=n,c=r,l=o.flags;switch(o.tag){case 0:case 11:case 15:kl(a,o,s,c,i),Gc(8,o);break;case 23:break;case 22:var u=o.stateNode;o.memoizedState===null?(u._visibility|=2,kl(a,o,s,c,i)):u._visibility&2?kl(a,o,s,c,i):Al(a,o),i&&l&2048&&Tl(o.alternate,o);break;case 24:kl(a,o,s,c,i),i&&l&2048&&El(o.alternate,o);break;default:kl(a,o,s,c,i)}t=t.sibling}}function Al(e,t){if(t.subtreeFlags&10256)for(t=t.child;t!==null;){var n=e,r=t,i=r.flags;switch(r.tag){case 22:Al(n,r),i&2048&&Tl(r.alternate,r);break;case 24:Al(n,r),i&2048&&El(r.alternate,r);break;default:Al(n,r)}t=t.sibling}}var jl=8192;function Ml(e,t,n){if(e.subtreeFlags&jl)for(e=e.child;e!==null;)Nl(e,t,n),e=e.sibling}function Nl(e,t,n){switch(e.tag){case 26:Ml(e,t,n),e.flags&jl&&e.memoizedState!==null&&Qf(n,yl,e.memoizedState,e.memoizedProps);break;case 5:Ml(e,t,n);break;case 3:case 4:var r=yl;yl=Cf(e.stateNode.containerInfo),Ml(e,t,n),yl=r;break;case 22:e.memoizedState===null&&(r=e.alternate,r!==null&&r.memoizedState!==null?(r=jl,jl=16777216,Ml(e,t,n),jl=r):Ml(e,t,n));break;default:Ml(e,t,n)}}function Pl(e){var t=e.alternate;if(t!==null&&(e=t.child,e!==null)){t.child=null;do t=e.sibling,e.sibling=null,e=t;while(e!==null)}}function Fl(e){var t=e.deletions;if(e.flags&16){if(t!==null)for(var n=0;n<t.length;n++){var r=t[n];sl=r,Rl(r,e)}Pl(e)}if(e.subtreeFlags&10256)for(e=e.child;e!==null;)Il(e),e=e.sibling}function Il(e){switch(e.tag){case 0:case 11:case 15:Fl(e),e.flags&2048&&Kc(9,e,e.return);break;case 3:Fl(e);break;case 12:Fl(e);break;case 22:var t=e.stateNode;e.memoizedState!==null&&t._visibility&2&&(e.return===null||e.return.tag!==13)?(t._visibility&=-3,Ll(e)):Fl(e);break;default:Fl(e)}}function Ll(e){var t=e.deletions;if(e.flags&16){if(t!==null)for(var n=0;n<t.length;n++){var r=t[n];sl=r,Rl(r,e)}Pl(e)}for(e=e.child;e!==null;){switch(t=e,t.tag){case 0:case 11:case 15:Kc(8,t,t.return),Ll(t);break;case 22:n=t.stateNode,n._visibility&2&&(n._visibility&=-3,Ll(t));break;default:Ll(t)}e=e.sibling}}function Rl(e,t){for(;sl!==null;){var n=sl;switch(n.tag){case 0:case 11:case 15:Kc(8,n,t);break;case 23:case 22:if(n.memoizedState!==null&&n.memoizedState.cachePool!==null){var r=n.memoizedState.cachePool.pool;r!=null&&r.refCount++}break;case 24:ua(n.memoizedState.cache)}if(r=n.child,r!==null)r.return=n,sl=r;else a:for(n=e;sl!==null;){r=sl;var i=r.sibling,a=r.return;if(ul(r),r===n){sl=null;break a}if(i!==null){i.return=a,sl=i;break a}sl=a}}}var zl={getCacheForType:function(e){var t=na(ca),n=t.data.get(e);return n===void 0&&(n=e(),t.data.set(e,n)),n},cacheSignal:function(){return na(ca).controller.signal}},Bl=typeof WeakMap==`function`?WeakMap:Map,Vl=0,Hl=null,W=null,Ul=0,Wl=0,Gl=null,Kl=!1,ql=!1,Jl=!1,Yl=0,Xl=0,Zl=0,Ql=0,$l=0,eu=0,tu=0,nu=null,ru=null,iu=!1,au=0,ou=0,su=1/0,cu=null,lu=null,uu=0,du=null,fu=null,pu=0,mu=0,hu=null,gu=null,_u=0,vu=null;function yu(){return Vl&2&&Ul!==0?Ul&-Ul:F.T===null?it():vd()}function bu(){if(eu===0)if(!(Ul&536870912)||Ii){var e=Ue;Ue<<=1,!(Ue&3932160)&&(Ue=262144),eu=e}else eu=536870912;return e=io.current,e!==null&&(e.flags|=32),eu}function xu(e,t,n){(e===Hl&&(Wl===2||Wl===9)||e.cancelPendingCommit!==null)&&(Ou(e,0),Tu(e,Ul,eu,!1)),Ze(e,n),(!(Vl&2)||e!==Hl)&&(e===Hl&&(!(Vl&2)&&(Ql|=n),Xl===4&&Tu(e,Ul,eu,!1)),ud(e))}function Su(e,t,n){if(Vl&6)throw Error(i(327));var r=!n&&(t&127)==0&&(t&e.expiredLanes)===0||qe(e,t),a=r?Iu(e,t):Pu(e,t,!0),o=r;do{if(a===0){ql&&!r&&Tu(e,t,0,!1);break}else{if(n=e.current.alternate,o&&!wu(n)){a=Pu(e,t,!1),o=!1;continue}if(a===2){if(o=t,e.errorRecoveryDisabledLanes&o)var s=0;else s=e.pendingLanes&-536870913,s=s===0?s&536870912?536870912:0:s;if(s!==0){t=s;a:{var c=e;a=nu;var l=c.current.memoizedState.isDehydrated;if(l&&(Ou(c,s).flags|=256),s=Pu(c,s,!1),s!==2){if(Jl&&!l){c.errorRecoveryDisabledLanes|=o,Ql|=o,a=4;break a}o=ru,ru=a,o!==null&&(ru===null?ru=o:ru.push.apply(ru,o))}a=s}if(o=!1,a!==2)continue}}if(a===1){Ou(e,0),Tu(e,t,0,!0);break}a:{switch(r=e,o=a,o){case 0:case 1:throw Error(i(345));case 4:if((t&4194048)!==t)break;case 6:Tu(r,t,eu,!Kl);break a;case 2:ru=null;break;case 3:case 5:break;default:throw Error(i(329))}if((t&62914560)===t&&(a=au+300-Ee(),10<a)){if(Tu(r,t,eu,!Kl),Ke(r,0,!0)!==0)break a;pu=t,r.timeoutHandle=$d(Cu.bind(null,r,n,ru,cu,iu,t,eu,Ql,tu,Kl,o,`Throttled`,-0,0),a);break a}Cu(r,n,ru,cu,iu,t,eu,Ql,tu,Kl,o,null,-0,0)}}break}while(1);ud(e)}function Cu(e,t,n,r,i,a,o,s,c,l,u,d,f,p){if(e.timeoutHandle=-1,d=t.subtreeFlags,d&8192||(d&16785408)==16785408){d={stylesheets:null,count:0,imgCount:0,imgBytes:0,suspenseyImages:[],waitingForImages:!0,waitingForViewTransition:!1,unsuspend:en},Nl(t,a,d);var m=(a&62914560)===a?au-Ee():(a&4194048)===a?ou-Ee():0;if(m=ep(d,m),m!==null){pu=a,e.cancelPendingCommit=m(Uu.bind(null,e,t,a,n,r,i,o,s,c,u,d,null,f,p)),Tu(e,a,o,!l);return}}Uu(e,t,a,n,r,i,o,s,c)}function wu(e){for(var t=e;;){var n=t.tag;if((n===0||n===11||n===15)&&t.flags&16384&&(n=t.updateQueue,n!==null&&(n=n.stores,n!==null)))for(var r=0;r<n.length;r++){var i=n[r],a=i.getSnapshot;i=i.value;try{if(!Cr(a(),i))return!1}catch{return!1}}if(n=t.child,t.subtreeFlags&16384&&n!==null)n.return=t,t=n;else{if(t===e)break;for(;t.sibling===null;){if(t.return===null||t.return===e)return!0;t=t.return}t.sibling.return=t.return,t=t.sibling}}return!0}function Tu(e,t,n,r){t&=~$l,t&=~Ql,e.suspendedLanes|=t,e.pingedLanes&=~t,r&&(e.warmLanes|=t),r=e.expirationTimes;for(var i=t;0<i;){var a=31-Re(i),o=1<<a;r[a]=-1,i&=~o}n!==0&&$e(e,n,t)}function Eu(){return Vl&6?!0:(dd(0,!1),!1)}function Du(){if(W!==null){if(Wl===0)var e=W.return;else e=W,Ji=qi=null,Mo(e),Na=null,Pa=0,e=W;for(;e!==null;)Wc(e.alternate,e),e=e.return;W=null}}function Ou(e,t){var n=e.timeoutHandle;n!==-1&&(e.timeoutHandle=-1,ef(n)),n=e.cancelPendingCommit,n!==null&&(e.cancelPendingCommit=null,n()),pu=0,Du(),Hl=e,W=n=di(e.current,null),Ul=t,Wl=0,Gl=null,Kl=!1,ql=qe(e,t),Jl=!1,tu=eu=$l=Ql=Zl=Xl=0,ru=nu=null,iu=!1,t&8&&(t|=t&32);var r=e.entangledLanes;if(r!==0)for(e=e.entanglements,r&=t;0<r;){var i=31-Re(r),a=1<<i;t|=e[i],r&=~a}return Yl=t,ti(),n}function ku(e,t){z=null,F.H=Vs,t===Ca||t===Ta?(t=ja(),Wl=3):t===wa?(t=ja(),Wl=4):Wl=t===ac?8:typeof t==`object`&&t&&typeof t.then==`function`?6:1,Gl=t,W===null&&(Xl=1,$s(e,yi(t,e.current)))}function Au(){var e=io.current;return e===null?!0:(Ul&4194048)===Ul?ao===null:(Ul&62914560)===Ul||Ul&536870912?e===ao:!1}function ju(){var e=F.H;return F.H=Vs,e===null?Vs:e}function Mu(){var e=F.A;return F.A=zl,e}function Nu(){Xl=4,Kl||(Ul&4194048)!==Ul&&io.current!==null||(ql=!0),!(Zl&134217727)&&!(Ql&134217727)||Hl===null||Tu(Hl,Ul,eu,!1)}function Pu(e,t,n){var r=Vl;Vl|=2;var i=ju(),a=Mu();(Hl!==e||Ul!==t)&&(cu=null,Ou(e,t)),t=!1;var o=Xl;a:do try{if(Wl!==0&&W!==null){var s=W,c=Gl;switch(Wl){case 8:Du(),o=6;break a;case 3:case 2:case 9:case 6:io.current===null&&(t=!0);var l=Wl;if(Wl=0,Gl=null,Bu(e,s,c,l),n&&ql){o=0;break a}break;default:l=Wl,Wl=0,Gl=null,Bu(e,s,c,l)}}Fu(),o=Xl;break}catch(t){ku(e,t)}while(1);return t&&e.shellSuspendCounter++,Ji=qi=null,Vl=r,F.H=i,F.A=a,W===null&&(Hl=null,Ul=0,ti()),o}function Fu(){for(;W!==null;)Ru(W)}function Iu(e,t){var n=Vl;Vl|=2;var r=ju(),a=Mu();Hl!==e||Ul!==t?(cu=null,su=Ee()+500,Ou(e,t)):ql=qe(e,t);a:do try{if(Wl!==0&&W!==null){t=W;var o=Gl;b:switch(Wl){case 1:Wl=0,Gl=null,Bu(e,t,o,1);break;case 2:case 9:if(Da(o)){Wl=0,Gl=null,zu(t);break}t=function(){Wl!==2&&Wl!==9||Hl!==e||(Wl=7),ud(e)},o.then(t,t);break a;case 3:Wl=7;break a;case 4:Wl=5;break a;case 7:Da(o)?(Wl=0,Gl=null,zu(t)):(Wl=0,Gl=null,Bu(e,t,o,7));break;case 5:var s=null;switch(W.tag){case 26:s=W.memoizedState;case 5:case 27:var c=W;if(s?Zf(s):c.stateNode.complete){Wl=0,Gl=null;var l=c.sibling;if(l!==null)W=l;else{var u=c.return;u===null?W=null:(W=u,Vu(u))}break b}}Wl=0,Gl=null,Bu(e,t,o,5);break;case 6:Wl=0,Gl=null,Bu(e,t,o,6);break;case 8:Du(),Xl=6;break a;default:throw Error(i(462))}}Lu();break}catch(t){ku(e,t)}while(1);return Ji=qi=null,F.H=r,F.A=a,Vl=n,W===null?(Hl=null,Ul=0,ti(),Xl):0}function Lu(){for(;W!==null&&!we();)Ru(W)}function Ru(e){var t=Fc(e.alternate,e,Yl);e.memoizedProps=e.pendingProps,t===null?Vu(e):W=t}function zu(e){var t=e,n=t.alternate;switch(t.tag){case 15:case 0:t=yc(n,t,t.pendingProps,t.type,void 0,Ul);break;case 11:t=yc(n,t,t.pendingProps,t.type.render,t.ref,Ul);break;case 5:Mo(t);default:Wc(n,t),t=W=fi(t,Yl),t=Fc(n,t,Yl)}e.memoizedProps=e.pendingProps,t===null?Vu(e):W=t}function Bu(e,t,n,r){Ji=qi=null,Mo(t),Na=null,Pa=0;var i=t.return;try{if(ic(e,i,t,n,Ul)){Xl=1,$s(e,yi(n,e.current)),W=null;return}}catch(t){if(i!==null)throw W=i,t;Xl=1,$s(e,yi(n,e.current)),W=null;return}t.flags&32768?(Ii||r===1?e=!0:ql||Ul&536870912?e=!1:(Kl=e=!0,(r===2||r===9||r===3||r===6)&&(r=io.current,r!==null&&r.tag===13&&(r.flags|=16384))),Hu(t,e)):Vu(t)}function Vu(e){var t=e;do{if(t.flags&32768){Hu(t,Kl);return}e=t.return;var n=Hc(t.alternate,t,Yl);if(n!==null){W=n;return}if(t=t.sibling,t!==null){W=t;return}W=t=e}while(t!==null);Xl===0&&(Xl=5)}function Hu(e,t){do{var n=Uc(e.alternate,e);if(n!==null){n.flags&=32767,W=n;return}if(n=e.return,n!==null&&(n.flags|=32768,n.subtreeFlags=0,n.deletions=null),!t&&(e=e.sibling,e!==null)){W=e;return}W=e=n}while(e!==null);Xl=6,W=null}function Uu(e,t,n,r,a,o,s,c,l){e.cancelPendingCommit=null;do Ju();while(uu!==0);if(Vl&6)throw Error(i(327));if(t!==null){if(t===e.current)throw Error(i(177));if(o=t.lanes|t.childLanes,o|=ei,Qe(e,n,o,s,c,l),e===Hl&&(W=Hl=null,Ul=0),fu=t,du=e,pu=n,mu=o,hu=a,gu=r,t.subtreeFlags&10256||t.flags&10256?(e.callbackNode=null,e.callbackPriority=0,rd(Ae,function(){return Yu(),null})):(e.callbackNode=null,e.callbackPriority=0),r=(t.flags&13878)!=0,t.subtreeFlags&13878||r){r=F.T,F.T=null,a=I.p,I.p=2,s=Vl,Vl|=4;try{cl(e,t,n)}finally{Vl=s,I.p=a,F.T=r}}uu=1,Wu(),Gu(),Ku()}}function Wu(){if(uu===1){uu=0;var e=du,t=fu,n=(t.flags&13878)!=0;if(t.subtreeFlags&13878||n){n=F.T,F.T=null;var r=I.p;I.p=2;var i=Vl;Vl|=4;try{bl(t,e);var a=K,o=Or(e.containerInfo),s=a.focusedElem,c=a.selectionRange;if(o!==s&&s&&s.ownerDocument&&Dr(s.ownerDocument.documentElement,s)){if(c!==null&&kr(s)){var l=c.start,u=c.end;if(u===void 0&&(u=l),`selectionStart`in s)s.selectionStart=l,s.selectionEnd=Math.min(u,s.value.length);else{var d=s.ownerDocument||document,f=d&&d.defaultView||window;if(f.getSelection){var p=f.getSelection(),m=s.textContent.length,h=Math.min(c.start,m),g=c.end===void 0?h:Math.min(c.end,m);!p.extend&&h>g&&(o=g,g=h,h=o);var _=Er(s,h),v=Er(s,g);if(_&&v&&(p.rangeCount!==1||p.anchorNode!==_.node||p.anchorOffset!==_.offset||p.focusNode!==v.node||p.focusOffset!==v.offset)){var y=d.createRange();y.setStart(_.node,_.offset),p.removeAllRanges(),h>g?(p.addRange(y),p.extend(v.node,v.offset)):(y.setEnd(v.node,v.offset),p.addRange(y))}}}}for(d=[],p=s;p=p.parentNode;)p.nodeType===1&&d.push({element:p,left:p.scrollLeft,top:p.scrollTop});for(typeof s.focus==`function`&&s.focus(),s=0;s<d.length;s++){var b=d[s];b.element.scrollLeft=b.left,b.element.scrollTop=b.top}}mp=!!Kd,K=Kd=null}finally{Vl=i,I.p=r,F.T=n}}e.current=t,uu=2}}function Gu(){if(uu===2){uu=0;var e=du,t=fu,n=(t.flags&8772)!=0;if(t.subtreeFlags&8772||n){n=F.T,F.T=null;var r=I.p;I.p=2;var i=Vl;Vl|=4;try{ll(e,t.alternate,t)}finally{Vl=i,I.p=r,F.T=n}}uu=3}}function Ku(){if(uu===4||uu===3){uu=0,Te();var e=du,t=fu,n=pu,r=gu;t.subtreeFlags&10256||t.flags&10256?uu=5:(uu=0,fu=du=null,qu(e,e.pendingLanes));var i=e.pendingLanes;if(i===0&&(lu=null),rt(n),t=t.stateNode,Ie&&typeof Ie.onCommitFiberRoot==`function`)try{Ie.onCommitFiberRoot(Fe,t,void 0,(t.current.flags&128)==128)}catch{}if(r!==null){t=F.T,i=I.p,I.p=2,F.T=null;try{for(var a=e.onRecoverableError,o=0;o<r.length;o++){var s=r[o];a(s.value,{componentStack:s.stack})}}finally{F.T=t,I.p=i}}pu&3&&Ju(),ud(e),i=e.pendingLanes,n&261930&&i&42?e===vu?_u++:(_u=0,vu=e):_u=0,dd(0,!1)}}function qu(e,t){(e.pooledCacheLanes&=t)===0&&(t=e.pooledCache,t!=null&&(e.pooledCache=null,ua(t)))}function Ju(){return Wu(),Gu(),Ku(),Yu()}function Yu(){if(uu!==5)return!1;var e=du,t=mu;mu=0;var n=rt(pu),r=F.T,a=I.p;try{I.p=32>n?32:n,F.T=null,n=hu,hu=null;var o=du,s=pu;if(uu=0,fu=du=null,pu=0,Vl&6)throw Error(i(331));var c=Vl;if(Vl|=4,Il(o.current),Ol(o,o.current,s,n),Vl=c,dd(0,!1),Ie&&typeof Ie.onPostCommitFiberRoot==`function`)try{Ie.onPostCommitFiberRoot(Fe,o)}catch{}return!0}finally{I.p=a,F.T=r,qu(e,t)}}function Xu(e,t,n){t=yi(n,t),t=tc(e.stateNode,t,2),e=Ga(e,t,2),e!==null&&(Ze(e,2),ud(e))}function Zu(e,t,n){if(e.tag===3)Xu(e,e,n);else for(;t!==null;){if(t.tag===3){Xu(t,e,n);break}else if(t.tag===1){var r=t.stateNode;if(typeof t.type.getDerivedStateFromError==`function`||typeof r.componentDidCatch==`function`&&(lu===null||!lu.has(r))){e=yi(n,e),n=nc(2),r=Ga(t,n,2),r!==null&&(rc(n,r,t,e),Ze(r,2),ud(r));break}}t=t.return}}function Qu(e,t,n){var r=e.pingCache;if(r===null){r=e.pingCache=new Bl;var i=new Set;r.set(t,i)}else i=r.get(t),i===void 0&&(i=new Set,r.set(t,i));i.has(n)||(Jl=!0,i.add(n),e=$u.bind(null,e,t,n),t.then(e,e))}function $u(e,t,n){var r=e.pingCache;r!==null&&r.delete(t),e.pingedLanes|=e.suspendedLanes&n,e.warmLanes&=~n,Hl===e&&(Ul&n)===n&&(Xl===4||Xl===3&&(Ul&62914560)===Ul&&300>Ee()-au?!(Vl&2)&&Ou(e,0):$l|=n,tu===Ul&&(tu=0)),ud(e)}function ed(e,t){t===0&&(t=Ye()),e=ii(e,t),e!==null&&(Ze(e,t),ud(e))}function td(e){var t=e.memoizedState,n=0;t!==null&&(n=t.retryLane),ed(e,n)}function nd(e,t){var n=0;switch(e.tag){case 31:case 13:var r=e.stateNode,a=e.memoizedState;a!==null&&(n=a.retryLane);break;case 19:r=e.stateNode;break;case 22:r=e.stateNode._retryCache;break;default:throw Error(i(314))}r!==null&&r.delete(t),ed(e,n)}function rd(e,t){return Se(e,t)}var id=null,ad=null,od=!1,sd=!1,cd=!1,ld=0;function ud(e){e!==ad&&e.next===null&&(ad===null?id=ad=e:ad=ad.next=e),sd=!0,od||(od=!0,_d())}function dd(e,t){if(!cd&&sd){cd=!0;do for(var n=!1,r=id;r!==null;){if(!t)if(e!==0){var i=r.pendingLanes;if(i===0)var a=0;else{var o=r.suspendedLanes,s=r.pingedLanes;a=(1<<31-Re(42|e)+1)-1,a&=i&~(o&~s),a=a&201326741?a&201326741|1:a?a|2:0}a!==0&&(n=!0,gd(r,a))}else a=Ul,a=Ke(r,r===Hl?a:0,r.cancelPendingCommit!==null||r.timeoutHandle!==-1),!(a&3)||qe(r,a)||(n=!0,gd(r,a));r=r.next}while(n);cd=!1}}function fd(){pd()}function pd(){sd=od=!1;var e=0;ld!==0&&Qd()&&(e=ld);for(var t=Ee(),n=null,r=id;r!==null;){var i=r.next,a=md(r,t);a===0?(r.next=null,n===null?id=i:n.next=i,i===null&&(ad=n)):(n=r,(e!==0||a&3)&&(sd=!0)),r=i}uu!==0&&uu!==5||dd(e,!1),ld!==0&&(ld=0)}function md(e,t){for(var n=e.suspendedLanes,r=e.pingedLanes,i=e.expirationTimes,a=e.pendingLanes&-62914561;0<a;){var o=31-Re(a),s=1<<o,c=i[o];c===-1?((s&n)===0||(s&r)!==0)&&(i[o]=Je(s,t)):c<=t&&(e.expiredLanes|=s),a&=~s}if(t=Hl,n=Ul,n=Ke(e,e===t?n:0,e.cancelPendingCommit!==null||e.timeoutHandle!==-1),r=e.callbackNode,n===0||e===t&&(Wl===2||Wl===9)||e.cancelPendingCommit!==null)return r!==null&&r!==null&&Ce(r),e.callbackNode=null,e.callbackPriority=0;if(!(n&3)||qe(e,n)){if(t=n&-n,t===e.callbackPriority)return t;switch(r!==null&&Ce(r),rt(n)){case 2:case 8:n=ke;break;case 32:n=Ae;break;case 268435456:n=Me;break;default:n=Ae}return r=hd.bind(null,e),n=Se(n,r),e.callbackPriority=t,e.callbackNode=n,t}return r!==null&&r!==null&&Ce(r),e.callbackPriority=2,e.callbackNode=null,2}function hd(e,t){if(uu!==0&&uu!==5)return e.callbackNode=null,e.callbackPriority=0,null;var n=e.callbackNode;if(Ju()&&e.callbackNode!==n)return null;var r=Ul;return r=Ke(e,e===Hl?r:0,e.cancelPendingCommit!==null||e.timeoutHandle!==-1),r===0?null:(Su(e,r,t),md(e,Ee()),e.callbackNode!=null&&e.callbackNode===n?hd.bind(null,e):null)}function gd(e,t){if(Ju())return null;Su(e,t,!0)}function _d(){nf(function(){Vl&6?Se(Oe,fd):pd()})}function vd(){if(ld===0){var e=pa;e===0&&(e=He,He<<=1,!(He&261888)&&(He=256)),ld=e}return ld}function yd(e){return e==null||typeof e==`symbol`||typeof e==`boolean`?null:typeof e==`function`?e:$t(``+e)}function bd(e,t){var n=t.ownerDocument.createElement(`input`);return n.name=t.name,n.value=t.value,e.id&&n.setAttribute(`form`,e.id),t.parentNode.insertBefore(n,t),e=new FormData(e),n.parentNode.removeChild(n),e}function xd(e,t,n,r,i){if(t===`submit`&&n&&n.stateNode===i){var a=yd((i[ct]||null).action),o=r.submitter;o&&(t=(t=o[ct]||null)?yd(t.formAction):o.getAttribute(`formAction`),t!==null&&(a=t,o=null));var s=new Sn(`action`,`action`,null,r,i);e.push({event:s,listeners:[{instance:null,listener:function(){if(r.defaultPrevented){if(ld!==0){var e=o?bd(i,o):new FormData(i);Ds(n,{pending:!0,data:e,method:i.method,action:a},null,e)}}else typeof a==`function`&&(s.preventDefault(),e=o?bd(i,o):new FormData(i),Ds(n,{pending:!0,data:e,method:i.method,action:a},a,e))},currentTarget:i}]})}}for(var Sd=0;Sd<Yr.length;Sd++){var Cd=Yr[Sd];Xr(Cd.toLowerCase(),`on`+(Cd[0].toUpperCase()+Cd.slice(1)))}Xr(Vr,`onAnimationEnd`),Xr(Hr,`onAnimationIteration`),Xr(Ur,`onAnimationStart`),Xr(`dblclick`,`onDoubleClick`),Xr(`focusin`,`onFocus`),Xr(`focusout`,`onBlur`),Xr(Wr,`onTransitionRun`),Xr(Gr,`onTransitionStart`),Xr(Kr,`onTransitionCancel`),Xr(qr,`onTransitionEnd`),wt(`onMouseEnter`,[`mouseout`,`mouseover`]),wt(`onMouseLeave`,[`mouseout`,`mouseover`]),wt(`onPointerEnter`,[`pointerout`,`pointerover`]),wt(`onPointerLeave`,[`pointerout`,`pointerover`]),Ct(`onChange`,`change click focusin focusout input keydown keyup selectionchange`.split(` `)),Ct(`onSelect`,`focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange`.split(` `)),Ct(`onBeforeInput`,[`compositionend`,`keypress`,`textInput`,`paste`]),Ct(`onCompositionEnd`,`compositionend focusout keydown keypress keyup mousedown`.split(` `)),Ct(`onCompositionStart`,`compositionstart focusout keydown keypress keyup mousedown`.split(` `)),Ct(`onCompositionUpdate`,`compositionupdate focusout keydown keypress keyup mousedown`.split(` `));var wd=`abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting`.split(` `),Td=new Set(`beforetoggle cancel close invalid load scroll scrollend toggle`.split(` `).concat(wd));function Ed(e,t){t=(t&4)!=0;for(var n=0;n<e.length;n++){var r=e[n],i=r.event;r=r.listeners;a:{var a=void 0;if(t)for(var o=r.length-1;0<=o;o--){var s=r[o],c=s.instance,l=s.currentTarget;if(s=s.listener,c!==a&&i.isPropagationStopped())break a;a=s,i.currentTarget=l;try{a(i)}catch(e){Zr(e)}i.currentTarget=null,a=c}else for(o=0;o<r.length;o++){if(s=r[o],c=s.instance,l=s.currentTarget,s=s.listener,c!==a&&i.isPropagationStopped())break a;a=s,i.currentTarget=l;try{a(i)}catch(e){Zr(e)}i.currentTarget=null,a=c}}}}function G(e,t){var n=t[ut];n===void 0&&(n=t[ut]=new Set);var r=e+`__bubble`;n.has(r)||(Ad(t,e,2,!1),n.add(r))}function Dd(e,t,n){var r=0;t&&(r|=4),Ad(n,e,r,t)}var Od=`_reactListening`+Math.random().toString(36).slice(2);function kd(e){if(!e[Od]){e[Od]=!0,xt.forEach(function(t){t!==`selectionchange`&&(Td.has(t)||Dd(t,!1,e),Dd(t,!0,e))});var t=e.nodeType===9?e:e.ownerDocument;t===null||t[Od]||(t[Od]=!0,Dd(`selectionchange`,!1,t))}}function Ad(e,t,n,r){switch(xp(t)){case 2:var i=hp;break;case 8:i=gp;break;default:i=_p}n=i.bind(null,t,n,e),i=void 0,!dn||t!==`touchstart`&&t!==`touchmove`&&t!==`wheel`||(i=!0),r?i===void 0?e.addEventListener(t,n,!0):e.addEventListener(t,n,{capture:!0,passive:i}):i===void 0?e.addEventListener(t,n,!1):e.addEventListener(t,n,{passive:i})}function jd(e,t,n,r,i){var a=r;if(!(t&1)&&!(t&2)&&r!==null)a:for(;;){if(r===null)return;var s=r.tag;if(s===3||s===4){var c=r.stateNode.containerInfo;if(c===i)break;if(s===4)for(s=r.return;s!==null;){var l=s.tag;if((l===3||l===4)&&s.stateNode.containerInfo===i)return;s=s.return}for(;c!==null;){if(s=gt(c),s===null)return;if(l=s.tag,l===5||l===6||l===26||l===27){r=a=s;continue a}c=c.parentNode}}r=r.return}cn(function(){var r=a,i=nn(n),s=[];a:{var c=Jr.get(e);if(c!==void 0){var l=Sn,u=e;switch(e){case`keypress`:if(_n(n)===0)break a;case`keydown`:case`keyup`:l=Bn;break;case`focusin`:u=`focus`,l=jn;break;case`focusout`:u=`blur`,l=jn;break;case`beforeblur`:case`afterblur`:l=jn;break;case`click`:if(n.button===2)break a;case`auxclick`:case`dblclick`:case`mousedown`:case`mousemove`:case`mouseup`:case`mouseout`:case`mouseover`:case`contextmenu`:l=kn;break;case`drag`:case`dragend`:case`dragenter`:case`dragexit`:case`dragleave`:case`dragover`:case`dragstart`:case`drop`:l=An;break;case`touchcancel`:case`touchend`:case`touchmove`:case`touchstart`:l=Hn;break;case Vr:case Hr:case Ur:l=Mn;break;case qr:l=Un;break;case`scroll`:case`scrollend`:l=wn;break;case`wheel`:l=Wn;break;case`copy`:case`cut`:case`paste`:l=Nn;break;case`gotpointercapture`:case`lostpointercapture`:case`pointercancel`:case`pointerdown`:case`pointermove`:case`pointerout`:case`pointerover`:case`pointerup`:l=Vn;break;case`toggle`:case`beforetoggle`:l=Gn}var d=(t&4)!=0,f=!d&&(e===`scroll`||e===`scrollend`),p=d?c===null?null:c+`Capture`:c;d=[];for(var m=r,h;m!==null;){var g=m;if(h=g.stateNode,g=g.tag,g!==5&&g!==26&&g!==27||h===null||p===null||(g=ln(m,p),g!=null&&d.push(Md(m,g,h))),f)break;m=m.return}0<d.length&&(c=new l(c,u,null,n,i),s.push({event:c,listeners:d}))}}if(!(t&7)){a:{if(c=e===`mouseover`||e===`pointerover`,l=e===`mouseout`||e===`pointerout`,c&&n!==tn&&(u=n.relatedTarget||n.fromElement)&&(gt(u)||u[lt]))break a;if((l||c)&&(c=i.window===i?i:(c=i.ownerDocument)?c.defaultView||c.parentWindow:window,l?(u=n.relatedTarget||n.toElement,l=r,u=u?gt(u):null,u!==null&&(f=o(u),d=u.tag,u!==f||d!==5&&d!==27&&d!==6)&&(u=null)):(l=null,u=r),l!==u)){if(d=kn,g=`onMouseLeave`,p=`onMouseEnter`,m=`mouse`,(e===`pointerout`||e===`pointerover`)&&(d=Vn,g=`onPointerLeave`,p=`onPointerEnter`,m=`pointer`),f=l==null?c:vt(l),h=u==null?c:vt(u),c=new d(g,m+`leave`,l,n,i),c.target=f,c.relatedTarget=h,g=null,gt(i)===r&&(d=new d(p,m+`enter`,u,n,i),d.target=h,d.relatedTarget=f,g=d),f=g,l&&u)b:{for(d=Pd,p=l,m=u,h=0,g=p;g;g=d(g))h++;g=0;for(var _=m;_;_=d(_))g++;for(;0<h-g;)p=d(p),h--;for(;0<g-h;)m=d(m),g--;for(;h--;){if(p===m||m!==null&&p===m.alternate){d=p;break b}p=d(p),m=d(m)}d=null}else d=null;l!==null&&Fd(s,c,l,d,!1),u!==null&&f!==null&&Fd(s,f,u,d,!0)}}a:{if(c=r?vt(r):window,l=c.nodeName&&c.nodeName.toLowerCase(),l===`select`||l===`input`&&c.type===`file`)var v=dr;else if(ar(c))if(fr)v=xr;else{v=yr;var y=vr}else l=c.nodeName,!l||l.toLowerCase()!==`input`||c.type!==`checkbox`&&c.type!==`radio`?r&&Xt(r.elementType)&&(v=dr):v=br;if(v&&=v(e,r)){or(s,v,n,i);break a}y&&y(e,c,r),e===`focusout`&&r&&c.type===`number`&&r.memoizedProps.value!=null&&Ht(c,`number`,c.value)}switch(y=r?vt(r):window,e){case`focusin`:(ar(y)||y.contentEditable===`true`)&&(jr=y,Mr=r,Nr=null);break;case`focusout`:Nr=Mr=jr=null;break;case`mousedown`:Pr=!0;break;case`contextmenu`:case`mouseup`:case`dragend`:Pr=!1,Fr(s,n,i);break;case`selectionchange`:if(Ar)break;case`keydown`:case`keyup`:Fr(s,n,i)}var b;if(qn)b:{switch(e){case`compositionstart`:var x=`onCompositionStart`;break b;case`compositionend`:x=`onCompositionEnd`;break b;case`compositionupdate`:x=`onCompositionUpdate`;break b}x=void 0}else tr?$n(e,n)&&(x=`onCompositionEnd`):e===`keydown`&&n.keyCode===229&&(x=`onCompositionStart`);x&&(Xn&&n.locale!==`ko`&&(tr||x!==`onCompositionStart`?x===`onCompositionEnd`&&tr&&(b=gn()):(pn=i,mn=`value`in pn?pn.value:pn.textContent,tr=!0)),y=Nd(r,x),0<y.length&&(x=new Pn(x,e,null,n,i),s.push({event:x,listeners:y}),b?x.data=b:(b=er(n),b!==null&&(x.data=b)))),(b=Yn?nr(e,n):rr(e,n))&&(x=Nd(r,`onBeforeInput`),0<x.length&&(y=new Pn(`onBeforeInput`,`beforeinput`,null,n,i),s.push({event:y,listeners:x}),y.data=b)),xd(s,e,r,n,i)}Ed(s,t)})}function Md(e,t,n){return{instance:e,listener:t,currentTarget:n}}function Nd(e,t){for(var n=t+`Capture`,r=[];e!==null;){var i=e,a=i.stateNode;if(i=i.tag,i!==5&&i!==26&&i!==27||a===null||(i=ln(e,n),i!=null&&r.unshift(Md(e,i,a)),i=ln(e,t),i!=null&&r.push(Md(e,i,a))),e.tag===3)return r;e=e.return}return[]}function Pd(e){if(e===null)return null;do e=e.return;while(e&&e.tag!==5&&e.tag!==27);return e||null}function Fd(e,t,n,r,i){for(var a=t._reactName,o=[];n!==null&&n!==r;){var s=n,c=s.alternate,l=s.stateNode;if(s=s.tag,c!==null&&c===r)break;s!==5&&s!==26&&s!==27||l===null||(c=l,i?(l=ln(n,a),l!=null&&o.unshift(Md(n,l,c))):i||(l=ln(n,a),l!=null&&o.push(Md(n,l,c)))),n=n.return}o.length!==0&&e.push({event:t,listeners:o})}var Id=/\r\n?/g,Ld=/\u0000|\uFFFD/g;function Rd(e){return(typeof e==`string`?e:``+e).replace(Id,`
`).replace(Ld,``)}function zd(e,t){return t=Rd(t),Rd(e)===t}function Bd(e,t,n,r,a,o){switch(n){case`children`:typeof r==`string`?t===`body`||t===`textarea`&&r===``||Kt(e,r):(typeof r==`number`||typeof r==`bigint`)&&t!==`body`&&Kt(e,``+r);break;case`className`:At(e,`class`,r);break;case`tabIndex`:At(e,`tabindex`,r);break;case`dir`:case`role`:case`viewBox`:case`width`:case`height`:At(e,n,r);break;case`style`:Yt(e,r,o);break;case`data`:if(t!==`object`){At(e,`data`,r);break}case`src`:case`href`:if(r===``&&(t!==`a`||n!==`href`)){e.removeAttribute(n);break}if(r==null||typeof r==`function`||typeof r==`symbol`||typeof r==`boolean`){e.removeAttribute(n);break}r=$t(``+r),e.setAttribute(n,r);break;case`action`:case`formAction`:if(typeof r==`function`){e.setAttribute(n,`javascript:throw new Error('A React form was unexpectedly submitted. If you called form.submit() manually, consider using form.requestSubmit() instead. If you\\'re trying to use event.stopPropagation() in a submit event handler, consider also calling event.preventDefault().')`);break}else typeof o==`function`&&(n===`formAction`?(t!==`input`&&Bd(e,t,`name`,a.name,a,null),Bd(e,t,`formEncType`,a.formEncType,a,null),Bd(e,t,`formMethod`,a.formMethod,a,null),Bd(e,t,`formTarget`,a.formTarget,a,null)):(Bd(e,t,`encType`,a.encType,a,null),Bd(e,t,`method`,a.method,a,null),Bd(e,t,`target`,a.target,a,null)));if(r==null||typeof r==`symbol`||typeof r==`boolean`){e.removeAttribute(n);break}r=$t(``+r),e.setAttribute(n,r);break;case`onClick`:r!=null&&(e.onclick=en);break;case`onScroll`:r!=null&&G(`scroll`,e);break;case`onScrollEnd`:r!=null&&G(`scrollend`,e);break;case`dangerouslySetInnerHTML`:if(r!=null){if(typeof r!=`object`||!(`__html`in r))throw Error(i(61));if(n=r.__html,n!=null){if(a.children!=null)throw Error(i(60));e.innerHTML=n}}break;case`multiple`:e.multiple=r&&typeof r!=`function`&&typeof r!=`symbol`;break;case`muted`:e.muted=r&&typeof r!=`function`&&typeof r!=`symbol`;break;case`suppressContentEditableWarning`:case`suppressHydrationWarning`:case`defaultValue`:case`defaultChecked`:case`innerHTML`:case`ref`:break;case`autoFocus`:break;case`xlinkHref`:if(r==null||typeof r==`function`||typeof r==`boolean`||typeof r==`symbol`){e.removeAttribute(`xlink:href`);break}n=$t(``+r),e.setAttributeNS(`http://www.w3.org/1999/xlink`,`xlink:href`,n);break;case`contentEditable`:case`spellCheck`:case`draggable`:case`value`:case`autoReverse`:case`externalResourcesRequired`:case`focusable`:case`preserveAlpha`:r!=null&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,``+r):e.removeAttribute(n);break;case`inert`:case`allowFullScreen`:case`async`:case`autoPlay`:case`controls`:case`default`:case`defer`:case`disabled`:case`disablePictureInPicture`:case`disableRemotePlayback`:case`formNoValidate`:case`hidden`:case`loop`:case`noModule`:case`noValidate`:case`open`:case`playsInline`:case`readOnly`:case`required`:case`reversed`:case`scoped`:case`seamless`:case`itemScope`:r&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,``):e.removeAttribute(n);break;case`capture`:case`download`:!0===r?e.setAttribute(n,``):!1!==r&&r!=null&&typeof r!=`function`&&typeof r!=`symbol`?e.setAttribute(n,r):e.removeAttribute(n);break;case`cols`:case`rows`:case`size`:case`span`:r!=null&&typeof r!=`function`&&typeof r!=`symbol`&&!isNaN(r)&&1<=r?e.setAttribute(n,r):e.removeAttribute(n);break;case`rowSpan`:case`start`:r==null||typeof r==`function`||typeof r==`symbol`||isNaN(r)?e.removeAttribute(n):e.setAttribute(n,r);break;case`popover`:G(`beforetoggle`,e),G(`toggle`,e),kt(e,`popover`,r);break;case`xlinkActuate`:jt(e,`http://www.w3.org/1999/xlink`,`xlink:actuate`,r);break;case`xlinkArcrole`:jt(e,`http://www.w3.org/1999/xlink`,`xlink:arcrole`,r);break;case`xlinkRole`:jt(e,`http://www.w3.org/1999/xlink`,`xlink:role`,r);break;case`xlinkShow`:jt(e,`http://www.w3.org/1999/xlink`,`xlink:show`,r);break;case`xlinkTitle`:jt(e,`http://www.w3.org/1999/xlink`,`xlink:title`,r);break;case`xlinkType`:jt(e,`http://www.w3.org/1999/xlink`,`xlink:type`,r);break;case`xmlBase`:jt(e,`http://www.w3.org/XML/1998/namespace`,`xml:base`,r);break;case`xmlLang`:jt(e,`http://www.w3.org/XML/1998/namespace`,`xml:lang`,r);break;case`xmlSpace`:jt(e,`http://www.w3.org/XML/1998/namespace`,`xml:space`,r);break;case`is`:kt(e,`is`,r);break;case`innerText`:case`textContent`:break;default:(!(2<n.length)||n[0]!==`o`&&n[0]!==`O`||n[1]!==`n`&&n[1]!==`N`)&&(n=Zt.get(n)||n,kt(e,n,r))}}function Vd(e,t,n,r,a,o){switch(n){case`style`:Yt(e,r,o);break;case`dangerouslySetInnerHTML`:if(r!=null){if(typeof r!=`object`||!(`__html`in r))throw Error(i(61));if(n=r.__html,n!=null){if(a.children!=null)throw Error(i(60));e.innerHTML=n}}break;case`children`:typeof r==`string`?Kt(e,r):(typeof r==`number`||typeof r==`bigint`)&&Kt(e,``+r);break;case`onScroll`:r!=null&&G(`scroll`,e);break;case`onScrollEnd`:r!=null&&G(`scrollend`,e);break;case`onClick`:r!=null&&(e.onclick=en);break;case`suppressContentEditableWarning`:case`suppressHydrationWarning`:case`innerHTML`:case`ref`:break;case`innerText`:case`textContent`:break;default:if(!St.hasOwnProperty(n))a:{if(n[0]===`o`&&n[1]===`n`&&(a=n.endsWith(`Capture`),t=n.slice(2,a?n.length-7:void 0),o=e[ct]||null,o=o==null?null:o[n],typeof o==`function`&&e.removeEventListener(t,o,a),typeof r==`function`)){typeof o!=`function`&&o!==null&&(n in e?e[n]=null:e.hasAttribute(n)&&e.removeAttribute(n)),e.addEventListener(t,r,a);break a}n in e?e[n]=r:!0===r?e.setAttribute(n,``):kt(e,n,r)}}}function Hd(e,t,n){switch(t){case`div`:case`span`:case`svg`:case`path`:case`a`:case`g`:case`p`:case`li`:break;case`img`:G(`error`,e),G(`load`,e);var r=!1,a=!1,o;for(o in n)if(n.hasOwnProperty(o)){var s=n[o];if(s!=null)switch(o){case`src`:r=!0;break;case`srcSet`:a=!0;break;case`children`:case`dangerouslySetInnerHTML`:throw Error(i(137,t));default:Bd(e,t,o,s,n,null)}}a&&Bd(e,t,`srcSet`,n.srcSet,n,null),r&&Bd(e,t,`src`,n.src,n,null);return;case`input`:G(`invalid`,e);var c=o=s=a=null,l=null,u=null;for(r in n)if(n.hasOwnProperty(r)){var d=n[r];if(d!=null)switch(r){case`name`:a=d;break;case`type`:s=d;break;case`checked`:l=d;break;case`defaultChecked`:u=d;break;case`value`:o=d;break;case`defaultValue`:c=d;break;case`children`:case`dangerouslySetInnerHTML`:if(d!=null)throw Error(i(137,t));break;default:Bd(e,t,r,d,n,null)}}Vt(e,o,c,l,u,s,a,!1);return;case`select`:for(a in G(`invalid`,e),r=s=o=null,n)if(n.hasOwnProperty(a)&&(c=n[a],c!=null))switch(a){case`value`:o=c;break;case`defaultValue`:s=c;break;case`multiple`:r=c;default:Bd(e,t,a,c,n,null)}t=o,n=s,e.multiple=!!r,t==null?n!=null&&Ut(e,!!r,n,!0):Ut(e,!!r,t,!1);return;case`textarea`:for(s in G(`invalid`,e),o=a=r=null,n)if(n.hasOwnProperty(s)&&(c=n[s],c!=null))switch(s){case`value`:r=c;break;case`defaultValue`:a=c;break;case`children`:o=c;break;case`dangerouslySetInnerHTML`:if(c!=null)throw Error(i(91));break;default:Bd(e,t,s,c,n,null)}Gt(e,r,a,o);return;case`option`:for(l in n)if(n.hasOwnProperty(l)&&(r=n[l],r!=null))switch(l){case`selected`:e.selected=r&&typeof r!=`function`&&typeof r!=`symbol`;break;default:Bd(e,t,l,r,n,null)}return;case`dialog`:G(`beforetoggle`,e),G(`toggle`,e),G(`cancel`,e),G(`close`,e);break;case`iframe`:case`object`:G(`load`,e);break;case`video`:case`audio`:for(r=0;r<wd.length;r++)G(wd[r],e);break;case`image`:G(`error`,e),G(`load`,e);break;case`details`:G(`toggle`,e);break;case`embed`:case`source`:case`link`:G(`error`,e),G(`load`,e);case`area`:case`base`:case`br`:case`col`:case`hr`:case`keygen`:case`meta`:case`param`:case`track`:case`wbr`:case`menuitem`:for(u in n)if(n.hasOwnProperty(u)&&(r=n[u],r!=null))switch(u){case`children`:case`dangerouslySetInnerHTML`:throw Error(i(137,t));default:Bd(e,t,u,r,n,null)}return;default:if(Xt(t)){for(d in n)n.hasOwnProperty(d)&&(r=n[d],r!==void 0&&Vd(e,t,d,r,n,void 0));return}}for(c in n)n.hasOwnProperty(c)&&(r=n[c],r!=null&&Bd(e,t,c,r,n,null))}function Ud(e,t,n,r){switch(t){case`div`:case`span`:case`svg`:case`path`:case`a`:case`g`:case`p`:case`li`:break;case`input`:var a=null,o=null,s=null,c=null,l=null,u=null,d=null;for(m in n){var f=n[m];if(n.hasOwnProperty(m)&&f!=null)switch(m){case`checked`:break;case`value`:break;case`defaultValue`:l=f;default:r.hasOwnProperty(m)||Bd(e,t,m,null,r,f)}}for(var p in r){var m=r[p];if(f=n[p],r.hasOwnProperty(p)&&(m!=null||f!=null))switch(p){case`type`:o=m;break;case`name`:a=m;break;case`checked`:u=m;break;case`defaultChecked`:d=m;break;case`value`:s=m;break;case`defaultValue`:c=m;break;case`children`:case`dangerouslySetInnerHTML`:if(m!=null)throw Error(i(137,t));break;default:m!==f&&Bd(e,t,p,m,r,f)}}Bt(e,s,c,l,u,d,o,a);return;case`select`:for(o in m=s=c=p=null,n)if(l=n[o],n.hasOwnProperty(o)&&l!=null)switch(o){case`value`:break;case`multiple`:m=l;default:r.hasOwnProperty(o)||Bd(e,t,o,null,r,l)}for(a in r)if(o=r[a],l=n[a],r.hasOwnProperty(a)&&(o!=null||l!=null))switch(a){case`value`:p=o;break;case`defaultValue`:c=o;break;case`multiple`:s=o;default:o!==l&&Bd(e,t,a,o,r,l)}t=c,n=s,r=m,p==null?!!r!=!!n&&(t==null?Ut(e,!!n,n?[]:``,!1):Ut(e,!!n,t,!0)):Ut(e,!!n,p,!1);return;case`textarea`:for(c in m=p=null,n)if(a=n[c],n.hasOwnProperty(c)&&a!=null&&!r.hasOwnProperty(c))switch(c){case`value`:break;case`children`:break;default:Bd(e,t,c,null,r,a)}for(s in r)if(a=r[s],o=n[s],r.hasOwnProperty(s)&&(a!=null||o!=null))switch(s){case`value`:p=a;break;case`defaultValue`:m=a;break;case`children`:break;case`dangerouslySetInnerHTML`:if(a!=null)throw Error(i(91));break;default:a!==o&&Bd(e,t,s,a,r,o)}Wt(e,p,m);return;case`option`:for(var h in n)if(p=n[h],n.hasOwnProperty(h)&&p!=null&&!r.hasOwnProperty(h))switch(h){case`selected`:e.selected=!1;break;default:Bd(e,t,h,null,r,p)}for(l in r)if(p=r[l],m=n[l],r.hasOwnProperty(l)&&p!==m&&(p!=null||m!=null))switch(l){case`selected`:e.selected=p&&typeof p!=`function`&&typeof p!=`symbol`;break;default:Bd(e,t,l,p,r,m)}return;case`img`:case`link`:case`area`:case`base`:case`br`:case`col`:case`embed`:case`hr`:case`keygen`:case`meta`:case`param`:case`source`:case`track`:case`wbr`:case`menuitem`:for(var g in n)p=n[g],n.hasOwnProperty(g)&&p!=null&&!r.hasOwnProperty(g)&&Bd(e,t,g,null,r,p);for(u in r)if(p=r[u],m=n[u],r.hasOwnProperty(u)&&p!==m&&(p!=null||m!=null))switch(u){case`children`:case`dangerouslySetInnerHTML`:if(p!=null)throw Error(i(137,t));break;default:Bd(e,t,u,p,r,m)}return;default:if(Xt(t)){for(var _ in n)p=n[_],n.hasOwnProperty(_)&&p!==void 0&&!r.hasOwnProperty(_)&&Vd(e,t,_,void 0,r,p);for(d in r)p=r[d],m=n[d],!r.hasOwnProperty(d)||p===m||p===void 0&&m===void 0||Vd(e,t,d,p,r,m);return}}for(var v in n)p=n[v],n.hasOwnProperty(v)&&p!=null&&!r.hasOwnProperty(v)&&Bd(e,t,v,null,r,p);for(f in r)p=r[f],m=n[f],!r.hasOwnProperty(f)||p===m||p==null&&m==null||Bd(e,t,f,p,r,m)}function Wd(e){switch(e){case`css`:case`script`:case`font`:case`img`:case`image`:case`input`:case`link`:return!0;default:return!1}}function Gd(){if(typeof performance.getEntriesByType==`function`){for(var e=0,t=0,n=performance.getEntriesByType(`resource`),r=0;r<n.length;r++){var i=n[r],a=i.transferSize,o=i.initiatorType,s=i.duration;if(a&&s&&Wd(o)){for(o=0,s=i.responseEnd,r+=1;r<n.length;r++){var c=n[r],l=c.startTime;if(l>s)break;var u=c.transferSize,d=c.initiatorType;u&&Wd(d)&&(c=c.responseEnd,o+=u*(c<s?1:(s-l)/(c-l)))}if(--r,t+=8*(a+o)/(i.duration/1e3),e++,10<e)break}}if(0<e)return t/e/1e6}return navigator.connection&&(e=navigator.connection.downlink,typeof e==`number`)?e:5}var Kd=null,K=null;function qd(e){return e.nodeType===9?e:e.ownerDocument}function Jd(e){switch(e){case`http://www.w3.org/2000/svg`:return 1;case`http://www.w3.org/1998/Math/MathML`:return 2;default:return 0}}function Yd(e,t){if(e===0)switch(t){case`svg`:return 1;case`math`:return 2;default:return 0}return e===1&&t===`foreignObject`?0:e}function Xd(e,t){return e===`textarea`||e===`noscript`||typeof t.children==`string`||typeof t.children==`number`||typeof t.children==`bigint`||typeof t.dangerouslySetInnerHTML==`object`&&t.dangerouslySetInnerHTML!==null&&t.dangerouslySetInnerHTML.__html!=null}var Zd=null;function Qd(){var e=window.event;return e&&e.type===`popstate`?e===Zd?!1:(Zd=e,!0):(Zd=null,!1)}var $d=typeof setTimeout==`function`?setTimeout:void 0,ef=typeof clearTimeout==`function`?clearTimeout:void 0,tf=typeof Promise==`function`?Promise:void 0,nf=typeof queueMicrotask==`function`?queueMicrotask:tf===void 0?$d:function(e){return tf.resolve(null).then(e).catch(rf)};function rf(e){setTimeout(function(){throw e})}function af(e){return e===`head`}function of(e,t){var n=t,r=0;do{var i=n.nextSibling;if(e.removeChild(n),i&&i.nodeType===8)if(n=i.data,n===`/$`||n===`/&`){if(r===0){e.removeChild(i),Bp(t);return}r--}else if(n===`$`||n===`$?`||n===`$~`||n===`$!`||n===`&`)r++;else if(n===`html`)bf(e.ownerDocument.documentElement);else if(n===`head`){n=e.ownerDocument.head,bf(n);for(var a=n.firstChild;a;){var o=a.nextSibling,s=a.nodeName;a[mt]||s===`SCRIPT`||s===`STYLE`||s===`LINK`&&a.rel.toLowerCase()===`stylesheet`||n.removeChild(a),a=o}}else n===`body`&&bf(e.ownerDocument.body);n=i}while(n);Bp(t)}function sf(e,t){var n=e;e=0;do{var r=n.nextSibling;if(n.nodeType===1?t?(n._stashedDisplay=n.style.display,n.style.display=`none`):(n.style.display=n._stashedDisplay||``,n.getAttribute(`style`)===``&&n.removeAttribute(`style`)):n.nodeType===3&&(t?(n._stashedText=n.nodeValue,n.nodeValue=``):n.nodeValue=n._stashedText||``),r&&r.nodeType===8)if(n=r.data,n===`/$`){if(e===0)break;e--}else n!==`$`&&n!==`$?`&&n!==`$~`&&n!==`$!`||e++;n=r}while(n)}function cf(e){var t=e.firstChild;for(t&&t.nodeType===10&&(t=t.nextSibling);t;){var n=t;switch(t=t.nextSibling,n.nodeName){case`HTML`:case`HEAD`:case`BODY`:cf(n),ht(n);continue;case`SCRIPT`:case`STYLE`:continue;case`LINK`:if(n.rel.toLowerCase()===`stylesheet`)continue}e.removeChild(n)}}function lf(e,t,n,r){for(;e.nodeType===1;){var i=n;if(e.nodeName.toLowerCase()!==t.toLowerCase()){if(!r&&(e.nodeName!==`INPUT`||e.type!==`hidden`))break}else if(r){if(!e[mt])switch(t){case`meta`:if(!e.hasAttribute(`itemprop`))break;return e;case`link`:if(a=e.getAttribute(`rel`),a===`stylesheet`&&e.hasAttribute(`data-precedence`)||a!==i.rel||e.getAttribute(`href`)!==(i.href==null||i.href===``?null:i.href)||e.getAttribute(`crossorigin`)!==(i.crossOrigin==null?null:i.crossOrigin)||e.getAttribute(`title`)!==(i.title==null?null:i.title))break;return e;case`style`:if(e.hasAttribute(`data-precedence`))break;return e;case`script`:if(a=e.getAttribute(`src`),(a!==(i.src==null?null:i.src)||e.getAttribute(`type`)!==(i.type==null?null:i.type)||e.getAttribute(`crossorigin`)!==(i.crossOrigin==null?null:i.crossOrigin))&&a&&e.hasAttribute(`async`)&&!e.hasAttribute(`itemprop`))break;return e;default:return e}}else if(t===`input`&&e.type===`hidden`){var a=i.name==null?null:``+i.name;if(i.type===`hidden`&&e.getAttribute(`name`)===a)return e}else return e;if(e=hf(e.nextSibling),e===null)break}return null}function uf(e,t,n){if(t===``)return null;for(;e.nodeType!==3;)if((e.nodeType!==1||e.nodeName!==`INPUT`||e.type!==`hidden`)&&!n||(e=hf(e.nextSibling),e===null))return null;return e}function df(e,t){for(;e.nodeType!==8;)if((e.nodeType!==1||e.nodeName!==`INPUT`||e.type!==`hidden`)&&!t||(e=hf(e.nextSibling),e===null))return null;return e}function ff(e){return e.data===`$?`||e.data===`$~`}function pf(e){return e.data===`$!`||e.data===`$?`&&e.ownerDocument.readyState!==`loading`}function mf(e,t){var n=e.ownerDocument;if(e.data===`$~`)e._reactRetry=t;else if(e.data!==`$?`||n.readyState!==`loading`)t();else{var r=function(){t(),n.removeEventListener(`DOMContentLoaded`,r)};n.addEventListener(`DOMContentLoaded`,r),e._reactRetry=r}}function hf(e){for(;e!=null;e=e.nextSibling){var t=e.nodeType;if(t===1||t===3)break;if(t===8){if(t=e.data,t===`$`||t===`$!`||t===`$?`||t===`$~`||t===`&`||t===`F!`||t===`F`)break;if(t===`/$`||t===`/&`)return null}}return e}var gf=null;function _f(e){e=e.nextSibling;for(var t=0;e;){if(e.nodeType===8){var n=e.data;if(n===`/$`||n===`/&`){if(t===0)return hf(e.nextSibling);t--}else n!==`$`&&n!==`$!`&&n!==`$?`&&n!==`$~`&&n!==`&`||t++}e=e.nextSibling}return null}function vf(e){e=e.previousSibling;for(var t=0;e;){if(e.nodeType===8){var n=e.data;if(n===`$`||n===`$!`||n===`$?`||n===`$~`||n===`&`){if(t===0)return e;t--}else n!==`/$`&&n!==`/&`||t++}e=e.previousSibling}return null}function yf(e,t,n){switch(t=qd(n),e){case`html`:if(e=t.documentElement,!e)throw Error(i(452));return e;case`head`:if(e=t.head,!e)throw Error(i(453));return e;case`body`:if(e=t.body,!e)throw Error(i(454));return e;default:throw Error(i(451))}}function bf(e){for(var t=e.attributes;t.length;)e.removeAttributeNode(t[0]);ht(e)}var xf=new Map,Sf=new Set;function Cf(e){return typeof e.getRootNode==`function`?e.getRootNode():e.nodeType===9?e:e.ownerDocument}var wf=I.d;I.d={f:Tf,r:Ef,D:kf,C:Af,L:jf,m:Mf,X:Pf,S:Nf,M:Ff};function Tf(){var e=wf.f(),t=Eu();return e||t}function Ef(e){var t=_t(e);t!==null&&t.tag===5&&t.type===`form`?ks(t):wf.r(e)}var Df=typeof document>`u`?null:document;function Of(e,t,n){var r=Df;if(r&&typeof t==`string`&&t){var i=zt(t);i=`link[rel="`+e+`"][href="`+i+`"]`,typeof n==`string`&&(i+=`[crossorigin="`+n+`"]`),Sf.has(i)||(Sf.add(i),e={rel:e,crossOrigin:n,href:t},r.querySelector(i)===null&&(t=r.createElement(`link`),Hd(t,`link`,e),bt(t),r.head.appendChild(t)))}}function kf(e){wf.D(e),Of(`dns-prefetch`,e,null)}function Af(e,t){wf.C(e,t),Of(`preconnect`,e,t)}function jf(e,t,n){wf.L(e,t,n);var r=Df;if(r&&e&&t){var i=`link[rel="preload"][as="`+zt(t)+`"]`;t===`image`&&n&&n.imageSrcSet?(i+=`[imagesrcset="`+zt(n.imageSrcSet)+`"]`,typeof n.imageSizes==`string`&&(i+=`[imagesizes="`+zt(n.imageSizes)+`"]`)):i+=`[href="`+zt(e)+`"]`;var a=i;switch(t){case`style`:a=Lf(e);break;case`script`:a=Vf(e)}xf.has(a)||(e=h({rel:`preload`,href:t===`image`&&n&&n.imageSrcSet?void 0:e,as:t},n),xf.set(a,e),r.querySelector(i)!==null||t===`style`&&r.querySelector(Rf(a))||t===`script`&&r.querySelector(Hf(a))||(t=r.createElement(`link`),Hd(t,`link`,e),bt(t),r.head.appendChild(t)))}}function Mf(e,t){wf.m(e,t);var n=Df;if(n&&e){var r=t&&typeof t.as==`string`?t.as:`script`,i=`link[rel="modulepreload"][as="`+zt(r)+`"][href="`+zt(e)+`"]`,a=i;switch(r){case`audioworklet`:case`paintworklet`:case`serviceworker`:case`sharedworker`:case`worker`:case`script`:a=Vf(e)}if(!xf.has(a)&&(e=h({rel:`modulepreload`,href:e},t),xf.set(a,e),n.querySelector(i)===null)){switch(r){case`audioworklet`:case`paintworklet`:case`serviceworker`:case`sharedworker`:case`worker`:case`script`:if(n.querySelector(Hf(a)))return}r=n.createElement(`link`),Hd(r,`link`,e),bt(r),n.head.appendChild(r)}}}function Nf(e,t,n){wf.S(e,t,n);var r=Df;if(r&&e){var i=yt(r).hoistableStyles,a=Lf(e);t||=`default`;var o=i.get(a);if(!o){var s={loading:0,preload:null};if(o=r.querySelector(Rf(a)))s.loading=5;else{e=h({rel:`stylesheet`,href:e,"data-precedence":t},n),(n=xf.get(a))&&Gf(e,n);var c=o=r.createElement(`link`);bt(c),Hd(c,`link`,e),c._p=new Promise(function(e,t){c.onload=e,c.onerror=t}),c.addEventListener(`load`,function(){s.loading|=1}),c.addEventListener(`error`,function(){s.loading|=2}),s.loading|=4,Wf(o,t,r)}o={type:`stylesheet`,instance:o,count:1,state:s},i.set(a,o)}}}function Pf(e,t){wf.X(e,t);var n=Df;if(n&&e){var r=yt(n).hoistableScripts,i=Vf(e),a=r.get(i);a||(a=n.querySelector(Hf(i)),a||(e=h({src:e,async:!0},t),(t=xf.get(i))&&Kf(e,t),a=n.createElement(`script`),bt(a),Hd(a,`link`,e),n.head.appendChild(a)),a={type:`script`,instance:a,count:1,state:null},r.set(i,a))}}function Ff(e,t){wf.M(e,t);var n=Df;if(n&&e){var r=yt(n).hoistableScripts,i=Vf(e),a=r.get(i);a||(a=n.querySelector(Hf(i)),a||(e=h({src:e,async:!0,type:`module`},t),(t=xf.get(i))&&Kf(e,t),a=n.createElement(`script`),bt(a),Hd(a,`link`,e),n.head.appendChild(a)),a={type:`script`,instance:a,count:1,state:null},r.set(i,a))}}function If(e,t,n,r){var a=(a=ce.current)?Cf(a):null;if(!a)throw Error(i(446));switch(e){case`meta`:case`title`:return null;case`style`:return typeof n.precedence==`string`&&typeof n.href==`string`?(t=Lf(n.href),n=yt(a).hoistableStyles,r=n.get(t),r||(r={type:`style`,instance:null,count:0,state:null},n.set(t,r)),r):{type:`void`,instance:null,count:0,state:null};case`link`:if(n.rel===`stylesheet`&&typeof n.href==`string`&&typeof n.precedence==`string`){e=Lf(n.href);var o=yt(a).hoistableStyles,s=o.get(e);if(s||(a=a.ownerDocument||a,s={type:`stylesheet`,instance:null,count:0,state:{loading:0,preload:null}},o.set(e,s),(o=a.querySelector(Rf(e)))&&!o._p&&(s.instance=o,s.state.loading=5),xf.has(e)||(n={rel:`preload`,as:`style`,href:n.href,crossOrigin:n.crossOrigin,integrity:n.integrity,media:n.media,hrefLang:n.hrefLang,referrerPolicy:n.referrerPolicy},xf.set(e,n),o||Bf(a,e,n,s.state))),t&&r===null)throw Error(i(528,``));return s}if(t&&r!==null)throw Error(i(529,``));return null;case`script`:return t=n.async,n=n.src,typeof n==`string`&&t&&typeof t!=`function`&&typeof t!=`symbol`?(t=Vf(n),n=yt(a).hoistableScripts,r=n.get(t),r||(r={type:`script`,instance:null,count:0,state:null},n.set(t,r)),r):{type:`void`,instance:null,count:0,state:null};default:throw Error(i(444,e))}}function Lf(e){return`href="`+zt(e)+`"`}function Rf(e){return`link[rel="stylesheet"][`+e+`]`}function zf(e){return h({},e,{"data-precedence":e.precedence,precedence:null})}function Bf(e,t,n,r){e.querySelector(`link[rel="preload"][as="style"][`+t+`]`)?r.loading=1:(t=e.createElement(`link`),r.preload=t,t.addEventListener(`load`,function(){return r.loading|=1}),t.addEventListener(`error`,function(){return r.loading|=2}),Hd(t,`link`,n),bt(t),e.head.appendChild(t))}function Vf(e){return`[src="`+zt(e)+`"]`}function Hf(e){return`script[async]`+e}function Uf(e,t,n){if(t.count++,t.instance===null)switch(t.type){case`style`:var r=e.querySelector(`style[data-href~="`+zt(n.href)+`"]`);if(r)return t.instance=r,bt(r),r;var a=h({},n,{"data-href":n.href,"data-precedence":n.precedence,href:null,precedence:null});return r=(e.ownerDocument||e).createElement(`style`),bt(r),Hd(r,`style`,a),Wf(r,n.precedence,e),t.instance=r;case`stylesheet`:a=Lf(n.href);var o=e.querySelector(Rf(a));if(o)return t.state.loading|=4,t.instance=o,bt(o),o;r=zf(n),(a=xf.get(a))&&Gf(r,a),o=(e.ownerDocument||e).createElement(`link`),bt(o);var s=o;return s._p=new Promise(function(e,t){s.onload=e,s.onerror=t}),Hd(o,`link`,r),t.state.loading|=4,Wf(o,n.precedence,e),t.instance=o;case`script`:return o=Vf(n.src),(a=e.querySelector(Hf(o)))?(t.instance=a,bt(a),a):(r=n,(a=xf.get(o))&&(r=h({},n),Kf(r,a)),e=e.ownerDocument||e,a=e.createElement(`script`),bt(a),Hd(a,`link`,r),e.head.appendChild(a),t.instance=a);case`void`:return null;default:throw Error(i(443,t.type))}else t.type===`stylesheet`&&!(t.state.loading&4)&&(r=t.instance,t.state.loading|=4,Wf(r,n.precedence,e));return t.instance}function Wf(e,t,n){for(var r=n.querySelectorAll(`link[rel="stylesheet"][data-precedence],style[data-precedence]`),i=r.length?r[r.length-1]:null,a=i,o=0;o<r.length;o++){var s=r[o];if(s.dataset.precedence===t)a=s;else if(a!==i)break}a?a.parentNode.insertBefore(e,a.nextSibling):(t=n.nodeType===9?n.head:n,t.insertBefore(e,t.firstChild))}function Gf(e,t){e.crossOrigin??=t.crossOrigin,e.referrerPolicy??=t.referrerPolicy,e.title??=t.title}function Kf(e,t){e.crossOrigin??=t.crossOrigin,e.referrerPolicy??=t.referrerPolicy,e.integrity??=t.integrity}var qf=null;function Jf(e,t,n){if(qf===null){var r=new Map,i=qf=new Map;i.set(n,r)}else i=qf,r=i.get(n),r||(r=new Map,i.set(n,r));if(r.has(e))return r;for(r.set(e,null),n=n.getElementsByTagName(e),i=0;i<n.length;i++){var a=n[i];if(!(a[mt]||a[st]||e===`link`&&a.getAttribute(`rel`)===`stylesheet`)&&a.namespaceURI!==`http://www.w3.org/2000/svg`){var o=a.getAttribute(t)||``;o=e+o;var s=r.get(o);s?s.push(a):r.set(o,[a])}}return r}function Yf(e,t,n){e=e.ownerDocument||e,e.head.insertBefore(n,t===`title`?e.querySelector(`head > title`):null)}function Xf(e,t,n){if(n===1||t.itemProp!=null)return!1;switch(e){case`meta`:case`title`:return!0;case`style`:if(typeof t.precedence!=`string`||typeof t.href!=`string`||t.href===``)break;return!0;case`link`:if(typeof t.rel!=`string`||typeof t.href!=`string`||t.href===``||t.onLoad||t.onError)break;switch(t.rel){case`stylesheet`:return e=t.disabled,typeof t.precedence==`string`&&e==null;default:return!0}case`script`:if(t.async&&typeof t.async!=`function`&&typeof t.async!=`symbol`&&!t.onLoad&&!t.onError&&t.src&&typeof t.src==`string`)return!0}return!1}function Zf(e){return!(e.type===`stylesheet`&&!(e.state.loading&3))}function Qf(e,t,n,r){if(n.type===`stylesheet`&&(typeof r.media!=`string`||!1!==matchMedia(r.media).matches)&&!(n.state.loading&4)){if(n.instance===null){var i=Lf(r.href),a=t.querySelector(Rf(i));if(a){t=a._p,typeof t==`object`&&t&&typeof t.then==`function`&&(e.count++,e=tp.bind(e),t.then(e,e)),n.state.loading|=4,n.instance=a,bt(a);return}a=t.ownerDocument||t,r=zf(r),(i=xf.get(i))&&Gf(r,i),a=a.createElement(`link`),bt(a);var o=a;o._p=new Promise(function(e,t){o.onload=e,o.onerror=t}),Hd(a,`link`,r),n.instance=a}e.stylesheets===null&&(e.stylesheets=new Map),e.stylesheets.set(n,t),(t=n.state.preload)&&!(n.state.loading&3)&&(e.count++,n=tp.bind(e),t.addEventListener(`load`,n),t.addEventListener(`error`,n))}}var $f=0;function ep(e,t){return e.stylesheets&&e.count===0&&rp(e,e.stylesheets),0<e.count||0<e.imgCount?function(n){var r=setTimeout(function(){if(e.stylesheets&&rp(e,e.stylesheets),e.unsuspend){var t=e.unsuspend;e.unsuspend=null,t()}},6e4+t);0<e.imgBytes&&$f===0&&($f=62500*Gd());var i=setTimeout(function(){if(e.waitingForImages=!1,e.count===0&&(e.stylesheets&&rp(e,e.stylesheets),e.unsuspend)){var t=e.unsuspend;e.unsuspend=null,t()}},(e.imgBytes>$f?50:800)+t);return e.unsuspend=n,function(){e.unsuspend=null,clearTimeout(r),clearTimeout(i)}}:null}function tp(){if(this.count--,this.count===0&&(this.imgCount===0||!this.waitingForImages)){if(this.stylesheets)rp(this,this.stylesheets);else if(this.unsuspend){var e=this.unsuspend;this.unsuspend=null,e()}}}var np=null;function rp(e,t){e.stylesheets=null,e.unsuspend!==null&&(e.count++,np=new Map,t.forEach(ip,e),np=null,tp.call(e))}function ip(e,t){if(!(t.state.loading&4)){var n=np.get(e);if(n)var r=n.get(null);else{n=new Map,np.set(e,n);for(var i=e.querySelectorAll(`link[data-precedence],style[data-precedence]`),a=0;a<i.length;a++){var o=i[a];(o.nodeName===`LINK`||o.getAttribute(`media`)!==`not all`)&&(n.set(o.dataset.precedence,o),r=o)}r&&n.set(null,r)}i=t.instance,o=i.getAttribute(`data-precedence`),a=n.get(o)||r,a===r&&n.set(null,i),n.set(o,i),this.count++,r=tp.bind(this),i.addEventListener(`load`,r),i.addEventListener(`error`,r),a?a.parentNode.insertBefore(i,a.nextSibling):(e=e.nodeType===9?e.head:e,e.insertBefore(i,e.firstChild)),t.state.loading|=4}}var ap={$$typeof:C,Provider:null,Consumer:null,_currentValue:te,_currentValue2:te,_threadCount:0};function op(e,t,n,r,i,a,o,s,c){this.tag=1,this.containerInfo=e,this.pingCache=this.current=this.pendingChildren=null,this.timeoutHandle=-1,this.callbackNode=this.next=this.pendingContext=this.context=this.cancelPendingCommit=null,this.callbackPriority=0,this.expirationTimes=Xe(-1),this.entangledLanes=this.shellSuspendCounter=this.errorRecoveryDisabledLanes=this.expiredLanes=this.warmLanes=this.pingedLanes=this.suspendedLanes=this.pendingLanes=0,this.entanglements=Xe(0),this.hiddenUpdates=Xe(null),this.identifierPrefix=r,this.onUncaughtError=i,this.onCaughtError=a,this.onRecoverableError=o,this.pooledCache=null,this.pooledCacheLanes=0,this.formState=c,this.incompleteTransitions=new Map}function sp(e,t,n,r,i,a,o,s,c,l,u,d){return e=new op(e,t,n,o,c,l,u,d,s),t=1,!0===a&&(t|=24),a=li(3,null,null,t),e.current=a,a.stateNode=e,t=la(),t.refCount++,e.pooledCache=t,t.refCount++,a.memoizedState={element:r,isDehydrated:n,cache:t},Ha(a),e}function cp(e){return e?(e=si,e):si}function lp(e,t,n,r,i,a){i=cp(i),r.context===null?r.context=i:r.pendingContext=i,r=Wa(t),r.payload={element:n},a=a===void 0?null:a,a!==null&&(r.callback=a),n=Ga(e,r,t),n!==null&&(xu(n,e,t),Ka(n,e,t))}function up(e,t){if(e=e.memoizedState,e!==null&&e.dehydrated!==null){var n=e.retryLane;e.retryLane=n!==0&&n<t?n:t}}function dp(e,t){up(e,t),(e=e.alternate)&&up(e,t)}function fp(e){if(e.tag===13||e.tag===31){var t=ii(e,67108864);t!==null&&xu(t,e,67108864),dp(e,67108864)}}function pp(e){if(e.tag===13||e.tag===31){var t=yu();t=nt(t);var n=ii(e,t);n!==null&&xu(n,e,t),dp(e,t)}}var mp=!0;function hp(e,t,n,r){var i=F.T;F.T=null;var a=I.p;try{I.p=2,_p(e,t,n,r)}finally{I.p=a,F.T=i}}function gp(e,t,n,r){var i=F.T;F.T=null;var a=I.p;try{I.p=8,_p(e,t,n,r)}finally{I.p=a,F.T=i}}function _p(e,t,n,r){if(mp){var i=vp(r);if(i===null)jd(e,t,r,yp,n),Ap(e,r);else if(Mp(i,e,t,n,r))r.stopPropagation();else if(Ap(e,r),t&4&&-1<kp.indexOf(e)){for(;i!==null;){var a=_t(i);if(a!==null)switch(a.tag){case 3:if(a=a.stateNode,a.current.memoizedState.isDehydrated){var o=Ge(a.pendingLanes);if(o!==0){var s=a;for(s.pendingLanes|=2,s.entangledLanes|=2;o;){var c=1<<31-Re(o);s.entanglements[1]|=c,o&=~c}ud(a),!(Vl&6)&&(su=Ee()+500,dd(0,!1))}}break;case 31:case 13:s=ii(a,2),s!==null&&xu(s,a,2),Eu(),dp(a,2)}if(a=vp(r),a===null&&jd(e,t,r,yp,n),a===i)break;i=a}i!==null&&r.stopPropagation()}else jd(e,t,r,null,n)}}function vp(e){return e=nn(e),bp(e)}var yp=null;function bp(e){if(yp=null,e=gt(e),e!==null){var t=o(e);if(t===null)e=null;else{var n=t.tag;if(n===13){if(e=s(t),e!==null)return e;e=null}else if(n===31){if(e=c(t),e!==null)return e;e=null}else if(n===3){if(t.stateNode.current.memoizedState.isDehydrated)return t.tag===3?t.stateNode.containerInfo:null;e=null}else t!==e&&(e=null)}}return yp=e,null}function xp(e){switch(e){case`beforetoggle`:case`cancel`:case`click`:case`close`:case`contextmenu`:case`copy`:case`cut`:case`auxclick`:case`dblclick`:case`dragend`:case`dragstart`:case`drop`:case`focusin`:case`focusout`:case`input`:case`invalid`:case`keydown`:case`keypress`:case`keyup`:case`mousedown`:case`mouseup`:case`paste`:case`pause`:case`play`:case`pointercancel`:case`pointerdown`:case`pointerup`:case`ratechange`:case`reset`:case`resize`:case`seeked`:case`submit`:case`toggle`:case`touchcancel`:case`touchend`:case`touchstart`:case`volumechange`:case`change`:case`selectionchange`:case`textInput`:case`compositionstart`:case`compositionend`:case`compositionupdate`:case`beforeblur`:case`afterblur`:case`beforeinput`:case`blur`:case`fullscreenchange`:case`focus`:case`hashchange`:case`popstate`:case`select`:case`selectstart`:return 2;case`drag`:case`dragenter`:case`dragexit`:case`dragleave`:case`dragover`:case`mousemove`:case`mouseout`:case`mouseover`:case`pointermove`:case`pointerout`:case`pointerover`:case`scroll`:case`touchmove`:case`wheel`:case`mouseenter`:case`mouseleave`:case`pointerenter`:case`pointerleave`:return 8;case`message`:switch(De()){case Oe:return 2;case ke:return 8;case Ae:case je:return 32;case Me:return 268435456;default:return 32}default:return 32}}var Sp=!1,Cp=null,wp=null,Tp=null,Ep=new Map,Dp=new Map,Op=[],kp=`mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset`.split(` `);function Ap(e,t){switch(e){case`focusin`:case`focusout`:Cp=null;break;case`dragenter`:case`dragleave`:wp=null;break;case`mouseover`:case`mouseout`:Tp=null;break;case`pointerover`:case`pointerout`:Ep.delete(t.pointerId);break;case`gotpointercapture`:case`lostpointercapture`:Dp.delete(t.pointerId)}}function jp(e,t,n,r,i,a){return e===null||e.nativeEvent!==a?(e={blockedOn:t,domEventName:n,eventSystemFlags:r,nativeEvent:a,targetContainers:[i]},t!==null&&(t=_t(t),t!==null&&fp(t)),e):(e.eventSystemFlags|=r,t=e.targetContainers,i!==null&&t.indexOf(i)===-1&&t.push(i),e)}function Mp(e,t,n,r,i){switch(t){case`focusin`:return Cp=jp(Cp,e,t,n,r,i),!0;case`dragenter`:return wp=jp(wp,e,t,n,r,i),!0;case`mouseover`:return Tp=jp(Tp,e,t,n,r,i),!0;case`pointerover`:var a=i.pointerId;return Ep.set(a,jp(Ep.get(a)||null,e,t,n,r,i)),!0;case`gotpointercapture`:return a=i.pointerId,Dp.set(a,jp(Dp.get(a)||null,e,t,n,r,i)),!0}return!1}function Np(e){var t=gt(e.target);if(t!==null){var n=o(t);if(n!==null){if(t=n.tag,t===13){if(t=s(n),t!==null){e.blockedOn=t,at(e.priority,function(){pp(n)});return}}else if(t===31){if(t=c(n),t!==null){e.blockedOn=t,at(e.priority,function(){pp(n)});return}}else if(t===3&&n.stateNode.current.memoizedState.isDehydrated){e.blockedOn=n.tag===3?n.stateNode.containerInfo:null;return}}}e.blockedOn=null}function Pp(e){if(e.blockedOn!==null)return!1;for(var t=e.targetContainers;0<t.length;){var n=vp(e.nativeEvent);if(n===null){n=e.nativeEvent;var r=new n.constructor(n.type,n);tn=r,n.target.dispatchEvent(r),tn=null}else return t=_t(n),t!==null&&fp(t),e.blockedOn=n,!1;t.shift()}return!0}function Fp(e,t,n){Pp(e)&&n.delete(t)}function Ip(){Sp=!1,Cp!==null&&Pp(Cp)&&(Cp=null),wp!==null&&Pp(wp)&&(wp=null),Tp!==null&&Pp(Tp)&&(Tp=null),Ep.forEach(Fp),Dp.forEach(Fp)}function Lp(e,n){e.blockedOn===n&&(e.blockedOn=null,Sp||(Sp=!0,t.unstable_scheduleCallback(t.unstable_NormalPriority,Ip)))}var Rp=null;function zp(e){Rp!==e&&(Rp=e,t.unstable_scheduleCallback(t.unstable_NormalPriority,function(){Rp===e&&(Rp=null);for(var t=0;t<e.length;t+=3){var n=e[t],r=e[t+1],i=e[t+2];if(typeof r!=`function`){if(bp(r||n)===null)continue;break}var a=_t(n);a!==null&&(e.splice(t,3),t-=3,Ds(a,{pending:!0,data:i,method:n.method,action:r},r,i))}}))}function Bp(e){function t(t){return Lp(t,e)}Cp!==null&&Lp(Cp,e),wp!==null&&Lp(wp,e),Tp!==null&&Lp(Tp,e),Ep.forEach(t),Dp.forEach(t);for(var n=0;n<Op.length;n++){var r=Op[n];r.blockedOn===e&&(r.blockedOn=null)}for(;0<Op.length&&(n=Op[0],n.blockedOn===null);)Np(n),n.blockedOn===null&&Op.shift();if(n=(e.ownerDocument||e).$$reactFormReplay,n!=null)for(r=0;r<n.length;r+=3){var i=n[r],a=n[r+1],o=i[ct]||null;if(typeof a==`function`)o||zp(n);else if(o){var s=null;if(a&&a.hasAttribute(`formAction`)){if(i=a,o=a[ct]||null)s=o.formAction;else if(bp(i)!==null)continue}else s=o.action;typeof s==`function`?n[r+1]=s:(n.splice(r,3),r-=3),zp(n)}}}function Vp(){function e(e){e.canIntercept&&e.info===`react-transition`&&e.intercept({handler:function(){return new Promise(function(e){return i=e})},focusReset:`manual`,scroll:`manual`})}function t(){i!==null&&(i(),i=null),r||setTimeout(n,20)}function n(){if(!r&&!navigation.transition){var e=navigation.currentEntry;e&&e.url!=null&&navigation.navigate(e.url,{state:e.getState(),info:`react-transition`,history:`replace`})}}if(typeof navigation==`object`){var r=!1,i=null;return navigation.addEventListener(`navigate`,e),navigation.addEventListener(`navigatesuccess`,t),navigation.addEventListener(`navigateerror`,t),setTimeout(n,100),function(){r=!0,navigation.removeEventListener(`navigate`,e),navigation.removeEventListener(`navigatesuccess`,t),navigation.removeEventListener(`navigateerror`,t),i!==null&&(i(),i=null)}}}function Hp(e){this._internalRoot=e}Up.prototype.render=Hp.prototype.render=function(e){var t=this._internalRoot;if(t===null)throw Error(i(409));var n=t.current;lp(n,yu(),e,t,null,null)},Up.prototype.unmount=Hp.prototype.unmount=function(){var e=this._internalRoot;if(e!==null){this._internalRoot=null;var t=e.containerInfo;lp(e.current,2,null,e,null,null),Eu(),t[lt]=null}};function Up(e){this._internalRoot=e}Up.prototype.unstable_scheduleHydration=function(e){if(e){var t=it();e={blockedOn:null,target:e,priority:t};for(var n=0;n<Op.length&&t!==0&&t<Op[n].priority;n++);Op.splice(n,0,e),n===0&&Np(e)}};var Wp=n.version;if(Wp!==`19.2.0`)throw Error(i(527,Wp,`19.2.0`));I.findDOMNode=function(e){var t=e._reactInternals;if(t===void 0)throw typeof e.render==`function`?Error(i(188)):(e=Object.keys(e).join(`,`),Error(i(268,e)));return e=d(t),e=e===null?null:p(e),e=e===null?null:e.stateNode,e};var Gp={bundleType:0,version:`19.2.0`,rendererPackageName:`react-dom`,currentDispatcherRef:F,reconcilerVersion:`19.2.0`};if(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__<`u`){var Kp=__REACT_DEVTOOLS_GLOBAL_HOOK__;if(!Kp.isDisabled&&Kp.supportsFiber)try{Fe=Kp.inject(Gp),Ie=Kp}catch{}}e.createRoot=function(e,t){if(!a(e))throw Error(i(299));var n=!1,r=``,o=Xs,s=Zs,c=Qs;return t!=null&&(!0===t.unstable_strictMode&&(n=!0),t.identifierPrefix!==void 0&&(r=t.identifierPrefix),t.onUncaughtError!==void 0&&(o=t.onUncaughtError),t.onCaughtError!==void 0&&(s=t.onCaughtError),t.onRecoverableError!==void 0&&(c=t.onRecoverableError)),t=sp(e,1,!1,null,null,n,r,null,o,s,c,Vp),e[lt]=t.current,kd(e),new Hp(t)}})),g=o(((e,t)=>{function n(){if(!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__>`u`||typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE!=`function`))try{__REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(n)}catch(e){console.error(e)}}n(),t.exports=h()})),_=`modulepreload`,v=function(e){return`/`+e},y={};const b=function(e,t,n){let r=Promise.resolve();if(t&&t.length>0){let e=document.getElementsByTagName(`link`),i=document.querySelector(`meta[property=csp-nonce]`),a=i?.nonce||i?.getAttribute(`nonce`);function o(e){return Promise.all(e.map(e=>Promise.resolve(e).then(e=>({status:`fulfilled`,value:e}),e=>({status:`rejected`,reason:e}))))}r=o(t.map(t=>{if(t=v(t,n),t in y)return;y[t]=!0;let r=t.endsWith(`.css`),i=r?`[rel="stylesheet"]`:``;if(n)for(let n=e.length-1;n>=0;n--){let i=e[n];if(i.href===t&&(!r||i.rel===`stylesheet`))return}else if(document.querySelector(`link[href="${t}"]${i}`))return;let o=document.createElement(`link`);if(o.rel=r?`stylesheet`:_,r||(o.as=`script`),o.crossOrigin=``,o.href=t,a&&o.setAttribute(`nonce`,a),document.head.appendChild(o),r)return new Promise((e,n)=>{o.addEventListener(`load`,e),o.addEventListener(`error`,()=>n(Error(`Unable to preload CSS for ${t}`)))})}))}function i(e){let t=new Event(`vite:preloadError`,{cancelable:!0});if(t.payload=e,window.dispatchEvent(t),!t.defaultPrevented)throw e}return r.then(t=>{for(let e of t||[])e.status===`rejected`&&i(e.reason);return e().catch(i)})};var x=c(u(),1),S=`popstate`;function C(e={}){function t(e,t){let{pathname:n,search:r,hash:i}=e.location;return O(``,{pathname:n,search:r,hash:i},t.state&&t.state.usr||null,t.state&&t.state.key||`default`)}function n(e,t){return typeof t==`string`?t:k(t)}return j(t,n,null,e)}function w(e,t){if(e===!1||e==null)throw Error(t)}function T(e,t){if(!e){typeof console<`u`&&console.warn(t);try{throw Error(t)}catch{}}}function E(){return Math.random().toString(36).substring(2,10)}function D(e,t){return{usr:e.state,key:e.key,idx:t}}function O(e,t,n=null,r){return{pathname:typeof e==`string`?e:e.pathname,search:``,hash:``,...typeof t==`string`?A(t):t,state:n,key:t&&t.key||r||E()}}function k({pathname:e=`/`,search:t=``,hash:n=``}){return t&&t!==`?`&&(e+=t.charAt(0)===`?`?t:`?`+t),n&&n!==`#`&&(e+=n.charAt(0)===`#`?n:`#`+n),e}function A(e){let t={};if(e){let n=e.indexOf(`#`);n>=0&&(t.hash=e.substring(n),e=e.substring(0,n));let r=e.indexOf(`?`);r>=0&&(t.search=e.substring(r),e=e.substring(0,r)),e&&(t.pathname=e)}return t}function j(e,t,n,r={}){let{window:i=document.defaultView,v5Compat:a=!1}=r,o=i.history,s=`POP`,c=null,l=u();l??(l=0,o.replaceState({...o.state,idx:l},``));function u(){return(o.state||{idx:null}).idx}function d(){s=`POP`;let e=u(),t=e==null?null:e-l;l=e,c&&c({action:s,location:h.location,delta:t})}function f(e,t){s=`PUSH`;let r=O(h.location,e,t);n&&n(r,e),l=u()+1;let d=D(r,l),f=h.createHref(r);try{o.pushState(d,``,f)}catch(e){if(e instanceof DOMException&&e.name===`DataCloneError`)throw e;i.location.assign(f)}a&&c&&c({action:s,location:h.location,delta:1})}function p(e,t){s=`REPLACE`;let r=O(h.location,e,t);n&&n(r,e),l=u();let i=D(r,l),d=h.createHref(r);o.replaceState(i,``,d),a&&c&&c({action:s,location:h.location,delta:0})}function m(e){return M(e)}let h={get action(){return s},get location(){return e(i,o)},listen(e){if(c)throw Error(`A history only accepts one active listener`);return i.addEventListener(S,d),c=e,()=>{i.removeEventListener(S,d),c=null}},createHref(e){return t(i,e)},createURL:m,encodeLocation(e){let t=m(e);return{pathname:t.pathname,search:t.search,hash:t.hash}},push:f,replace:p,go(e){return o.go(e)}};return h}function M(e,t=!1){let n=`http://localhost`;typeof window<`u`&&(n=window.location.origin===`null`?window.location.href:window.location.origin),w(n,`No window.location.(origin|href) available to create URL`);let r=typeof e==`string`?e:k(e);return r=r.replace(/ $/,`%20`),!t&&r.startsWith(`//`)&&(r=n+r),new URL(r,n)}function N(e,t,n=`/`){return ee(e,t,n,!1)}function ee(e,t,n,r){let i=me((typeof t==`string`?A(t):t).pathname||`/`,n);if(i==null)return null;let a=F(e);te(a);let o=null;for(let e=0;o==null&&e<a.length;++e){let t=pe(i);o=ue(a[e],t,r)}return o}function P(e,t){let{route:n,pathname:r,params:i}=e;return{id:n.id,pathname:r,params:i,data:t[n.id],loaderData:t[n.id],handle:n.handle}}function F(e,t=[],n=[],r=``,i=!1){let a=(e,a,o=i,s)=>{let c={relativePath:s===void 0?e.path||``:s,caseSensitive:e.caseSensitive===!0,childrenIndex:a,route:e};if(c.relativePath.startsWith(`/`)){if(!c.relativePath.startsWith(r)&&o)return;w(c.relativePath.startsWith(r),`Absolute route path "${c.relativePath}" nested under path "${r}" is not valid. An absolute child route path must start with the combined path of all its parent routes.`),c.relativePath=c.relativePath.slice(r.length)}let l=Ce([r,c.relativePath]),u=n.concat(c);e.children&&e.children.length>0&&(w(e.index!==!0,`Index routes must not have child routes. Please remove all child routes from route path "${l}".`),F(e.children,t,u,l,o)),!(e.path==null&&!e.index)&&t.push({path:l,score:ce(l,e.index),routesMeta:u})};return e.forEach((e,t)=>{if(e.path===``||!e.path?.includes(`?`))a(e,t);else for(let n of I(e.path))a(e,t,!0,n)}),t}function I(e){let t=e.split(`/`);if(t.length===0)return[];let[n,...r]=t,i=n.endsWith(`?`),a=n.replace(/\?$/,``);if(r.length===0)return i?[a,``]:[a];let o=I(r.join(`/`)),s=[];return s.push(...o.map(e=>e===``?a:[a,e].join(`/`))),i&&s.push(...o),s.map(t=>e.startsWith(`/`)&&t===``?`/`:t)}function te(e){e.sort((e,t)=>e.score===t.score?le(e.routesMeta.map(e=>e.childrenIndex),t.routesMeta.map(e=>e.childrenIndex)):t.score-e.score)}var ne=/^:[\w-]+$/,re=3,ie=2,ae=1,L=10,oe=-2,se=e=>e===`*`;function ce(e,t){let n=e.split(`/`),r=n.length;return n.some(se)&&(r+=oe),t&&(r+=ie),n.filter(e=>!se(e)).reduce((e,t)=>e+(ne.test(t)?re:t===``?ae:L),r)}function le(e,t){return e.length===t.length&&e.slice(0,-1).every((e,n)=>e===t[n])?e[e.length-1]-t[t.length-1]:0}function ue(e,t,n=!1){let{routesMeta:r}=e,i={},a=`/`,o=[];for(let e=0;e<r.length;++e){let s=r[e],c=e===r.length-1,l=a===`/`?t:t.slice(a.length)||`/`,u=de({path:s.relativePath,caseSensitive:s.caseSensitive,end:c},l),d=s.route;if(!u&&c&&n&&!r[r.length-1].route.index&&(u=de({path:s.relativePath,caseSensitive:s.caseSensitive,end:!1},l)),!u)return null;Object.assign(i,u.params),o.push({params:i,pathname:Ce([a,u.pathname]),pathnameBase:we(Ce([a,u.pathnameBase])),route:d}),u.pathnameBase!==`/`&&(a=Ce([a,u.pathnameBase]))}return o}function de(e,t){typeof e==`string`&&(e={path:e,caseSensitive:!1,end:!0});let[n,r]=fe(e.path,e.caseSensitive,e.end),i=t.match(n);if(!i)return null;let a=i[0],o=a.replace(/(.)\/+$/,`$1`),s=i.slice(1);return{params:r.reduce((e,{paramName:t,isOptional:n},r)=>{if(t===`*`){let e=s[r]||``;o=a.slice(0,a.length-e.length).replace(/(.)\/+$/,`$1`)}let i=s[r];return n&&!i?e[t]=void 0:e[t]=(i||``).replace(/%2F/g,`/`),e},{}),pathname:a,pathnameBase:o,pattern:e}}function fe(e,t=!1,n=!0){T(e===`*`||!e.endsWith(`*`)||e.endsWith(`/*`),`Route path "${e}" will be treated as if it were "${e.replace(/\*$/,`/*`)}" because the \`*\` character must always follow a \`/\` in the pattern. To get rid of this warning, please change the route path to "${e.replace(/\*$/,`/*`)}".`);let r=[],i=`^`+e.replace(/\/*\*?$/,``).replace(/^\/*/,`/`).replace(/[\\.*+^${}|()[\]]/g,`\\$&`).replace(/\/:([\w-]+)(\?)?/g,(e,t,n)=>(r.push({paramName:t,isOptional:n!=null}),n?`/?([^\\/]+)?`:`/([^\\/]+)`)).replace(/\/([\w-]+)\?(\/|$)/g,`(/$1)?$2`);return e.endsWith(`*`)?(r.push({paramName:`*`}),i+=e===`*`||e===`/*`?`(.*)$`:`(?:\\/(.+)|\\/*)$`):n?i+=`\\/*$`:e!==``&&e!==`/`&&(i+=`(?:(?=\\/|$))`),[new RegExp(i,t?void 0:`i`),r]}function pe(e){try{return e.split(`/`).map(e=>decodeURIComponent(e).replace(/\//g,`%2F`)).join(`/`)}catch(t){return T(!1,`The URL path "${e}" could not be decoded because it is a malformed URL segment. This is probably due to a bad percent encoding (${t}).`),e}}function me(e,t){if(t===`/`)return e;if(!e.toLowerCase().startsWith(t.toLowerCase()))return null;let n=t.endsWith(`/`)?t.length-1:t.length,r=e.charAt(n);return r&&r!==`/`?null:e.slice(n)||`/`}var he=/^(?:[a-z][a-z0-9+.-]*:|\/\/)/i,ge=e=>he.test(e);function _e(e,t=`/`){let{pathname:n,search:r=``,hash:i=``}=typeof e==`string`?A(e):e,a;if(n)if(ge(n))a=n;else{if(n.includes(`//`)){let e=n;n=n.replace(/\/\/+/g,`/`),T(!1,`Pathnames cannot have embedded double slashes - normalizing ${e} -> ${n}`)}a=n.startsWith(`/`)?ve(n.substring(1),`/`):ve(n,t)}else a=t;return{pathname:a,search:Te(r),hash:Ee(i)}}function ve(e,t){let n=t.replace(/\/+$/,``).split(`/`);return e.split(`/`).forEach(e=>{e===`..`?n.length>1&&n.pop():e!==`.`&&n.push(e)}),n.length>1?n.join(`/`):`/`}function ye(e,t,n,r){return`Cannot include a '${e}' character in a manually specified \`to.${t}\` field [${JSON.stringify(r)}].  Please separate it out to the \`to.${n}\` field. Alternatively you may provide the full path as a string in <Link to="..."> and the router will parse it for you.`}function be(e){return e.filter((e,t)=>t===0||e.route.path&&e.route.path.length>0)}function xe(e){let t=be(e);return t.map((e,n)=>n===t.length-1?e.pathname:e.pathnameBase)}function Se(e,t,n,r=!1){let i;typeof e==`string`?i=A(e):(i={...e},w(!i.pathname||!i.pathname.includes(`?`),ye(`?`,`pathname`,`search`,i)),w(!i.pathname||!i.pathname.includes(`#`),ye(`#`,`pathname`,`hash`,i)),w(!i.search||!i.search.includes(`#`),ye(`#`,`search`,`hash`,i)));let a=e===``||i.pathname===``,o=a?`/`:i.pathname,s;if(o==null)s=n;else{let e=t.length-1;if(!r&&o.startsWith(`..`)){let t=o.split(`/`);for(;t[0]===`..`;)t.shift(),--e;i.pathname=t.join(`/`)}s=e>=0?t[e]:`/`}let c=_e(i,s),l=o&&o!==`/`&&o.endsWith(`/`),u=(a||o===`.`)&&n.endsWith(`/`);return!c.pathname.endsWith(`/`)&&(l||u)&&(c.pathname+=`/`),c}var Ce=e=>e.join(`/`).replace(/\/\/+/g,`/`),we=e=>e.replace(/\/+$/,``).replace(/^\/*/,`/`),Te=e=>!e||e===`?`?``:e.startsWith(`?`)?e:`?`+e,Ee=e=>!e||e===`#`?``:e.startsWith(`#`)?e:`#`+e;function De(e){return e!=null&&typeof e.status==`number`&&typeof e.statusText==`string`&&typeof e.internal==`boolean`&&`data`in e}Object.getOwnPropertyNames(Object.prototype).sort().join(`\0`);var Oe=x.createContext(null);Oe.displayName=`DataRouter`;var ke=x.createContext(null);ke.displayName=`DataRouterState`,x.createContext(!1);var Ae=x.createContext({isTransitioning:!1});Ae.displayName=`ViewTransition`;var je=x.createContext(new Map);je.displayName=`Fetchers`;var Me=x.createContext(null);Me.displayName=`Await`;var Ne=x.createContext(null);Ne.displayName=`Navigation`;var Pe=x.createContext(null);Pe.displayName=`Location`;var Fe=x.createContext({outlet:null,matches:[],isDataRoute:!1});Fe.displayName=`Route`;var Ie=x.createContext(null);Ie.displayName=`RouteError`;function Le(e,{relative:t}={}){w(Re(),`useHref() may be used only in the context of a <Router> component.`);let{basename:n,navigator:r}=x.useContext(Ne),{hash:i,pathname:a,search:o}=We(e,{relative:t}),s=a;return n!==`/`&&(s=a===`/`?n:Ce([n,a])),r.createHref({pathname:s,search:o,hash:i})}function Re(){return x.useContext(Pe)!=null}function ze(){return w(Re(),`useLocation() may be used only in the context of a <Router> component.`),x.useContext(Pe).location}var Be=`You should call navigate() in a React.useEffect(), not when your component is first rendered.`;function Ve(e){x.useContext(Ne).static||x.useLayoutEffect(e)}function He(){let{isDataRoute:e}=x.useContext(Fe);return e?st():Ue()}function Ue(){w(Re(),`useNavigate() may be used only in the context of a <Router> component.`);let e=x.useContext(Oe),{basename:t,navigator:n}=x.useContext(Ne),{matches:r}=x.useContext(Fe),{pathname:i}=ze(),a=JSON.stringify(xe(r)),o=x.useRef(!1);return Ve(()=>{o.current=!0}),x.useCallback((r,s={})=>{if(T(o.current,Be),!o.current)return;if(typeof r==`number`){n.go(r);return}let c=Se(r,JSON.parse(a),i,s.relative===`path`);e==null&&t!==`/`&&(c.pathname=c.pathname===`/`?t:Ce([t,c.pathname])),(s.replace?n.replace:n.push)(c,s.state,s)},[t,n,a,i,e])}x.createContext(null);function We(e,{relative:t}={}){let{matches:n}=x.useContext(Fe),{pathname:r}=ze(),i=JSON.stringify(xe(n));return x.useMemo(()=>Se(e,JSON.parse(i),r,t===`path`),[e,i,r,t])}function Ge(e,t){return Ke(e,t)}function Ke(e,t,n,r,i){w(Re(),`useRoutes() may be used only in the context of a <Router> component.`);let{navigator:a}=x.useContext(Ne),{matches:o}=x.useContext(Fe),s=o[o.length-1],c=s?s.params:{},l=s?s.pathname:`/`,u=s?s.pathnameBase:`/`,d=s&&s.route;{let e=d&&d.path||``;lt(l,!d||e.endsWith(`*`)||e.endsWith(`*?`),`You rendered descendant <Routes> (or called \`useRoutes()\`) at "${l}" (under <Route path="${e}">) but the parent route path has no trailing "*". This means if you navigate deeper, the parent won't match anymore and therefore the child routes will never render.

Please change the parent <Route path="${e}"> to <Route path="${e===`/`?`*`:`${e}/*`}">.`)}let f=ze(),p;if(t){let e=typeof t==`string`?A(t):t;w(u===`/`||e.pathname?.startsWith(u),`When overriding the location using \`<Routes location>\` or \`useRoutes(routes, location)\`, the location pathname must begin with the portion of the URL pathname that was matched by all parent routes. The current pathname base is "${u}" but pathname "${e.pathname}" was given in the \`location\` prop.`),p=e}else p=f;let m=p.pathname||`/`,h=m;if(u!==`/`){let e=u.replace(/^\//,``).split(`/`);h=`/`+m.replace(/^\//,``).split(`/`).slice(e.length).join(`/`)}let g=N(e,{pathname:h});T(d||g!=null,`No routes matched location "${p.pathname}${p.search}${p.hash}" `),T(g==null||g[g.length-1].route.element!==void 0||g[g.length-1].route.Component!==void 0||g[g.length-1].route.lazy!==void 0,`Matched leaf route at location "${p.pathname}${p.search}${p.hash}" does not have an element or Component. This means it will render an <Outlet /> with a null value by default resulting in an "empty" page.`);let _=Ze(g&&g.map(e=>Object.assign({},e,{params:Object.assign({},c,e.params),pathname:Ce([u,a.encodeLocation?a.encodeLocation(e.pathname.replace(/\?/g,`%3F`).replace(/#/g,`%23`)).pathname:e.pathname]),pathnameBase:e.pathnameBase===`/`?u:Ce([u,a.encodeLocation?a.encodeLocation(e.pathnameBase.replace(/\?/g,`%3F`).replace(/#/g,`%23`)).pathname:e.pathnameBase])})),o,n,r,i);return t&&_?x.createElement(Pe.Provider,{value:{location:{pathname:`/`,search:``,hash:``,state:null,key:`default`,...p},navigationType:`POP`}},_):_}function qe(){let e=ot(),t=De(e)?`${e.status} ${e.statusText}`:e instanceof Error?e.message:JSON.stringify(e),n=e instanceof Error?e.stack:null,r=`rgba(200,200,200, 0.5)`,i={padding:`0.5rem`,backgroundColor:r},a={padding:`2px 4px`,backgroundColor:r},o=null;return console.error(`Error handled by React Router default ErrorBoundary:`,e),o=x.createElement(x.Fragment,null,x.createElement(`p`,null,`💿 Hey developer 👋`),x.createElement(`p`,null,`You can provide a way better UX than this when your app throws errors by providing your own `,x.createElement(`code`,{style:a},`ErrorBoundary`),` or`,` `,x.createElement(`code`,{style:a},`errorElement`),` prop on your route.`)),x.createElement(x.Fragment,null,x.createElement(`h2`,null,`Unexpected Application Error!`),x.createElement(`h3`,{style:{fontStyle:`italic`}},t),n?x.createElement(`pre`,{style:i},n):null,o)}var Je=x.createElement(qe,null),Ye=class extends x.Component{constructor(e){super(e),this.state={location:e.location,revalidation:e.revalidation,error:e.error}}static getDerivedStateFromError(e){return{error:e}}static getDerivedStateFromProps(e,t){return t.location!==e.location||t.revalidation!==`idle`&&e.revalidation===`idle`?{error:e.error,location:e.location,revalidation:e.revalidation}:{error:e.error===void 0?t.error:e.error,location:t.location,revalidation:e.revalidation||t.revalidation}}componentDidCatch(e,t){this.props.onError?this.props.onError(e,t):console.error(`React Router caught the following error during render`,e)}render(){return this.state.error===void 0?this.props.children:x.createElement(Fe.Provider,{value:this.props.routeContext},x.createElement(Ie.Provider,{value:this.state.error,children:this.props.component}))}};function Xe({routeContext:e,match:t,children:n}){let r=x.useContext(Oe);return r&&r.static&&r.staticContext&&(t.route.errorElement||t.route.ErrorBoundary)&&(r.staticContext._deepestRenderedBoundaryId=t.route.id),x.createElement(Fe.Provider,{value:e},n)}function Ze(e,t=[],n=null,r=null,i=null){if(e==null){if(!n)return null;if(n.errors)e=n.matches;else if(t.length===0&&!n.initialized&&n.matches.length>0)e=n.matches;else return null}let a=e,o=n?.errors;if(o!=null){let e=a.findIndex(e=>e.route.id&&o?.[e.route.id]!==void 0);w(e>=0,`Could not find a matching route for errors on route IDs: ${Object.keys(o).join(`,`)}`),a=a.slice(0,Math.min(a.length,e+1))}let s=!1,c=-1;if(n)for(let e=0;e<a.length;e++){let t=a[e];if((t.route.HydrateFallback||t.route.hydrateFallbackElement)&&(c=e),t.route.id){let{loaderData:e,errors:r}=n,i=t.route.loader&&!e.hasOwnProperty(t.route.id)&&(!r||r[t.route.id]===void 0);if(t.route.lazy||i){s=!0,a=c>=0?a.slice(0,c+1):[a[0]];break}}}let l=n&&r?(e,t)=>{r(e,{location:n.location,params:n.matches?.[0]?.params??{},errorInfo:t})}:void 0;return a.reduceRight((e,r,i)=>{let u,d=!1,f=null,p=null;n&&(u=o&&r.route.id?o[r.route.id]:void 0,f=r.route.errorElement||Je,s&&(c<0&&i===0?(lt(`route-fallback`,!1,"No `HydrateFallback` element provided to render during initial hydration"),d=!0,p=null):c===i&&(d=!0,p=r.route.hydrateFallbackElement||null)));let m=t.concat(a.slice(0,i+1)),h=()=>{let t;return t=u?f:d?p:r.route.Component?x.createElement(r.route.Component,null):r.route.element?r.route.element:e,x.createElement(Xe,{match:r,routeContext:{outlet:e,matches:m,isDataRoute:n!=null},children:t})};return n&&(r.route.ErrorBoundary||r.route.errorElement||i===0)?x.createElement(Ye,{location:n.location,revalidation:n.revalidation,component:f,error:u,children:h(),routeContext:{outlet:null,matches:m,isDataRoute:!0},onError:l}):h()},null)}function Qe(e){return`${e} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`}function $e(e){let t=x.useContext(Oe);return w(t,Qe(e)),t}function et(e){let t=x.useContext(ke);return w(t,Qe(e)),t}function tt(e){let t=x.useContext(Fe);return w(t,Qe(e)),t}function nt(e){let t=tt(e),n=t.matches[t.matches.length-1];return w(n.route.id,`${e} can only be used on routes that contain a unique "id"`),n.route.id}function rt(){return nt(`useRouteId`)}function it(){return et(`useNavigation`).navigation}function at(){let{matches:e,loaderData:t}=et(`useMatches`);return x.useMemo(()=>e.map(e=>P(e,t)),[e,t])}function ot(){let e=x.useContext(Ie),t=et(`useRouteError`),n=nt(`useRouteError`);return e===void 0?t.errors?.[n]:e}function st(){let{router:e}=$e(`useNavigate`),t=nt(`useNavigate`),n=x.useRef(!1);return Ve(()=>{n.current=!0}),x.useCallback(async(r,i={})=>{T(n.current,Be),n.current&&(typeof r==`number`?e.navigate(r):await e.navigate(r,{fromRouteId:t,...i}))},[e,t])}var ct={};function lt(e,t,n){!t&&!ct[e]&&(ct[e]=!0,T(!1,n))}x.memo(ut);function ut({routes:e,future:t,state:n,unstable_onError:r}){return Ke(e,void 0,n,r,t)}function dt(e){w(!1,`A <Route> is only ever to be used as the child of <Routes> element, never rendered directly. Please wrap your <Route> in a <Routes>.`)}function ft({basename:e=`/`,children:t=null,location:n,navigationType:r=`POP`,navigator:i,static:a=!1}){w(!Re(),`You cannot render a <Router> inside another <Router>. You should never have more than one in your app.`);let o=e.replace(/^\/*/,`/`),s=x.useMemo(()=>({basename:o,navigator:i,static:a,future:{}}),[o,i,a]);typeof n==`string`&&(n=A(n));let{pathname:c=`/`,search:l=``,hash:u=``,state:d=null,key:f=`default`}=n,p=x.useMemo(()=>{let e=me(c,o);return e==null?null:{location:{pathname:e,search:l,hash:u,state:d,key:f},navigationType:r}},[o,c,l,u,d,f,r]);return T(p!=null,`<Router basename="${o}"> is not able to match the URL "${c}${l}${u}" because it does not start with the basename, so the <Router> won't render anything.`),p==null?null:x.createElement(Ne.Provider,{value:s},x.createElement(Pe.Provider,{children:t,value:p}))}function pt({children:e,location:t}){return Ge(mt(e),t)}function mt(e,t=[]){let n=[];return x.Children.forEach(e,(e,r)=>{if(!x.isValidElement(e))return;let i=[...t,r];if(e.type===x.Fragment){n.push.apply(n,mt(e.props.children,i));return}w(e.type===dt,`[${typeof e.type==`string`?e.type:e.type.name}] is not a <Route> component. All component children of <Routes> must be a <Route> or <React.Fragment>`),w(!e.props.index||!e.props.children,`An index route cannot have child routes.`);let a={id:e.props.id||i.join(`-`),caseSensitive:e.props.caseSensitive,element:e.props.element,Component:e.props.Component,index:e.props.index,path:e.props.path,middleware:e.props.middleware,loader:e.props.loader,action:e.props.action,hydrateFallbackElement:e.props.hydrateFallbackElement,HydrateFallback:e.props.HydrateFallback,errorElement:e.props.errorElement,ErrorBoundary:e.props.ErrorBoundary,hasErrorBoundary:e.props.hasErrorBoundary===!0||e.props.ErrorBoundary!=null||e.props.errorElement!=null,shouldRevalidate:e.props.shouldRevalidate,handle:e.props.handle,lazy:e.props.lazy};e.props.children&&(a.children=mt(e.props.children,i)),n.push(a)}),n}var ht=`get`,gt=`application/x-www-form-urlencoded`;function _t(e){return e!=null&&typeof e.tagName==`string`}function vt(e){return _t(e)&&e.tagName.toLowerCase()===`button`}function yt(e){return _t(e)&&e.tagName.toLowerCase()===`form`}function bt(e){return _t(e)&&e.tagName.toLowerCase()===`input`}function xt(e){return!!(e.metaKey||e.altKey||e.ctrlKey||e.shiftKey)}function St(e,t){return e.button===0&&(!t||t===`_self`)&&!xt(e)}var Ct=null;function wt(){if(Ct===null)try{new FormData(document.createElement(`form`),0),Ct=!1}catch{Ct=!0}return Ct}var Tt=new Set([`application/x-www-form-urlencoded`,`multipart/form-data`,`text/plain`]);function Et(e){return e!=null&&!Tt.has(e)?(T(!1,`"${e}" is not a valid \`encType\` for \`<Form>\`/\`<fetcher.Form>\` and will default to "${gt}"`),null):e}function Dt(e,t){let n,r,i,a,o;if(yt(e)){let o=e.getAttribute(`action`);r=o?me(o,t):null,n=e.getAttribute(`method`)||ht,i=Et(e.getAttribute(`enctype`))||gt,a=new FormData(e)}else if(vt(e)||bt(e)&&(e.type===`submit`||e.type===`image`)){let o=e.form;if(o==null)throw Error(`Cannot submit a <button> or <input type="submit"> without a <form>`);let s=e.getAttribute(`formaction`)||o.getAttribute(`action`);if(r=s?me(s,t):null,n=e.getAttribute(`formmethod`)||o.getAttribute(`method`)||ht,i=Et(e.getAttribute(`formenctype`))||Et(o.getAttribute(`enctype`))||gt,a=new FormData(o,e),!wt()){let{name:t,type:n,value:r}=e;if(n===`image`){let e=t?`${t}.`:``;a.append(`${e}x`,`0`),a.append(`${e}y`,`0`)}else t&&a.append(t,r)}}else if(_t(e))throw Error(`Cannot submit element that is not <form>, <button>, or <input type="submit|image">`);else n=ht,r=null,i=gt,o=e;return a&&i===`text/plain`&&(o=a,a=void 0),{action:r,method:n.toLowerCase(),encType:i,formData:a,body:o}}Object.getOwnPropertyNames(Object.prototype).sort().join(`\0`);function Ot(e,t){if(e===!1||e==null)throw Error(t)}function kt(e,t,n){let r=typeof e==`string`?new URL(e,typeof window>`u`?`server://singlefetch/`:window.location.origin):e;return r.pathname===`/`?r.pathname=`_root.${n}`:t&&me(r.pathname,t)===`/`?r.pathname=`${t.replace(/\/$/,``)}/_root.${n}`:r.pathname=`${r.pathname.replace(/\/$/,``)}.${n}`,r}async function At(e,t){if(e.id in t)return t[e.id];try{let n=await b(()=>import(e.module),[]);return t[e.id]=n,n}catch(t){return console.error(`Error loading route module \`${e.module}\`, reloading page...`),console.error(t),window.__reactRouterContext&&window.__reactRouterContext.isSpaMode,window.location.reload(),new Promise(()=>{})}}function jt(e){return e!=null&&typeof e.page==`string`}function Mt(e){return e==null?!1:e.href==null?e.rel===`preload`&&typeof e.imageSrcSet==`string`&&typeof e.imageSizes==`string`:typeof e.rel==`string`&&typeof e.href==`string`}async function Nt(e,t,n){return Rt((await Promise.all(e.map(async e=>{let r=t.routes[e.route.id];if(r){let e=await At(r,n);return e.links?e.links():[]}return[]}))).flat(1).filter(Mt).filter(e=>e.rel===`stylesheet`||e.rel===`preload`).map(e=>e.rel===`stylesheet`?{...e,rel:`prefetch`,as:`style`}:{...e,rel:`prefetch`}))}function Pt(e,t,n,r,i,a){let o=(e,t)=>n[t]?e.route.id!==n[t].route.id:!0,s=(e,t)=>n[t].pathname!==e.pathname||n[t].route.path?.endsWith(`*`)&&n[t].params[`*`]!==e.params[`*`];return a===`assets`?t.filter((e,t)=>o(e,t)||s(e,t)):a===`data`?t.filter((t,a)=>{let c=r.routes[t.route.id];if(!c||!c.hasLoader)return!1;if(o(t,a)||s(t,a))return!0;if(t.route.shouldRevalidate){let r=t.route.shouldRevalidate({currentUrl:new URL(i.pathname+i.search+i.hash,window.origin),currentParams:n[0]?.params||{},nextUrl:new URL(e,window.origin),nextParams:t.params,defaultShouldRevalidate:!0});if(typeof r==`boolean`)return r}return!0}):[]}function Ft(e,t,{includeHydrateFallback:n}={}){return It(e.map(e=>{let r=t.routes[e.route.id];if(!r)return[];let i=[r.module];return r.clientActionModule&&(i=i.concat(r.clientActionModule)),r.clientLoaderModule&&(i=i.concat(r.clientLoaderModule)),n&&r.hydrateFallbackModule&&(i=i.concat(r.hydrateFallbackModule)),r.imports&&(i=i.concat(r.imports)),i}).flat(1))}function It(e){return[...new Set(e)]}function Lt(e){let t={},n=Object.keys(e).sort();for(let r of n)t[r]=e[r];return t}function Rt(e,t){let n=new Set,r=new Set(t);return e.reduce((e,i)=>{if(t&&!jt(i)&&i.as===`script`&&i.href&&r.has(i.href))return e;let a=JSON.stringify(Lt(i));return n.has(a)||(n.add(a),e.push({key:a,link:i})),e},[])}function zt(){let e=x.useContext(Oe);return Ot(e,`You must render this element inside a <DataRouterContext.Provider> element`),e}function Bt(){let e=x.useContext(ke);return Ot(e,`You must render this element inside a <DataRouterStateContext.Provider> element`),e}var Vt=x.createContext(void 0);Vt.displayName=`FrameworkContext`;function Ht(){let e=x.useContext(Vt);return Ot(e,`You must render this element inside a <HydratedRouter> element`),e}function Ut(e,t){let n=x.useContext(Vt),[r,i]=x.useState(!1),[a,o]=x.useState(!1),{onFocus:s,onBlur:c,onMouseEnter:l,onMouseLeave:u,onTouchStart:d}=t,f=x.useRef(null);x.useEffect(()=>{if(e===`render`&&o(!0),e===`viewport`){let e=new IntersectionObserver(e=>{e.forEach(e=>{o(e.isIntersecting)})},{threshold:.5});return f.current&&e.observe(f.current),()=>{e.disconnect()}}},[e]),x.useEffect(()=>{if(r){let e=setTimeout(()=>{o(!0)},100);return()=>{clearTimeout(e)}}},[r]);let p=()=>{i(!0)},m=()=>{i(!1),o(!1)};return n?e===`intent`?[a,f,{onFocus:Wt(s,p),onBlur:Wt(c,m),onMouseEnter:Wt(l,p),onMouseLeave:Wt(u,m),onTouchStart:Wt(d,p)}]:[a,f,{}]:[!1,f,{}]}function Wt(e,t){return n=>{e&&e(n),n.defaultPrevented||t(n)}}function Gt({page:e,...t}){let{router:n}=zt(),r=x.useMemo(()=>N(n.routes,e,n.basename),[n.routes,e,n.basename]);return r?x.createElement(qt,{page:e,matches:r,...t}):null}function Kt(e){let{manifest:t,routeModules:n}=Ht(),[r,i]=x.useState([]);return x.useEffect(()=>{let r=!1;return Nt(e,t,n).then(e=>{r||i(e)}),()=>{r=!0}},[e,t,n]),r}function qt({page:e,matches:t,...n}){let r=ze(),{manifest:i,routeModules:a}=Ht(),{basename:o}=zt(),{loaderData:s,matches:c}=Bt(),l=x.useMemo(()=>Pt(e,t,c,i,r,`data`),[e,t,c,i,r]),u=x.useMemo(()=>Pt(e,t,c,i,r,`assets`),[e,t,c,i,r]),d=x.useMemo(()=>{if(e===r.pathname+r.search+r.hash)return[];let n=new Set,c=!1;if(t.forEach(e=>{let t=i.routes[e.route.id];!t||!t.hasLoader||(!l.some(t=>t.route.id===e.route.id)&&e.route.id in s&&a[e.route.id]?.shouldRevalidate||t.hasClientLoader?c=!0:n.add(e.route.id))}),n.size===0)return[];let u=kt(e,o,`data`);return c&&n.size>0&&u.searchParams.set(`_routes`,t.filter(e=>n.has(e.route.id)).map(e=>e.route.id).join(`,`)),[u.pathname+u.search]},[o,s,r,i,l,t,e,a]),f=x.useMemo(()=>Ft(u,i),[u,i]),p=Kt(u);return x.createElement(x.Fragment,null,d.map(e=>x.createElement(`link`,{key:e,rel:`prefetch`,as:`fetch`,href:e,...n})),f.map(e=>x.createElement(`link`,{key:e,rel:`modulepreload`,href:e,...n})),p.map(({key:e,link:t})=>x.createElement(`link`,{key:e,nonce:n.nonce,...t})))}function Jt(...e){return t=>{e.forEach(e=>{typeof e==`function`?e(t):e!=null&&(e.current=t)})}}var Yt=typeof window<`u`&&window.document!==void 0&&window.document.createElement!==void 0;try{Yt&&(window.__reactRouterVersion=`7.9.6`)}catch{}function Xt({basename:e,children:t,window:n}){let r=x.useRef();r.current??=C({window:n,v5Compat:!0});let i=r.current,[a,o]=x.useState({action:i.action,location:i.location}),s=x.useCallback(e=>{x.startTransition(()=>o(e))},[o]);return x.useLayoutEffect(()=>i.listen(s),[i,s]),x.createElement(ft,{basename:e,children:t,location:a.location,navigationType:a.action,navigator:i})}function Zt({basename:e,children:t,history:n}){let[r,i]=x.useState({action:n.action,location:n.location}),a=x.useCallback(e=>{x.startTransition(()=>i(e))},[i]);return x.useLayoutEffect(()=>n.listen(a),[n,a]),x.createElement(ft,{basename:e,children:t,location:r.location,navigationType:r.action,navigator:n})}Zt.displayName=`unstable_HistoryRouter`;var Qt=/^(?:[a-z][a-z0-9+.-]*:|\/\/)/i,$t=x.forwardRef(function({onClick:e,discover:t=`render`,prefetch:n=`none`,relative:r,reloadDocument:i,replace:a,state:o,target:s,to:c,preventScrollReset:l,viewTransition:u,...d},f){let{basename:p}=x.useContext(Ne),m=typeof c==`string`&&Qt.test(c),h,g=!1;if(typeof c==`string`&&m&&(h=c,Yt))try{let e=new URL(window.location.href),t=c.startsWith(`//`)?new URL(e.protocol+c):new URL(c),n=me(t.pathname,p);t.origin===e.origin&&n!=null?c=n+t.search+t.hash:g=!0}catch{T(!1,`<Link to="${c}"> contains an invalid URL which will probably break when clicked - please update to a valid URL path.`)}let _=Le(c,{relative:r}),[v,y,b]=Ut(n,d),S=sn(c,{replace:a,state:o,target:s,preventScrollReset:l,relative:r,viewTransition:u});function C(t){e&&e(t),t.defaultPrevented||S(t)}let w=x.createElement(`a`,{...d,...b,href:h||_,onClick:g||i?e:C,ref:Jt(f,y),target:s,"data-discover":!m&&t===`render`?`true`:void 0});return v&&!m?x.createElement(x.Fragment,null,w,x.createElement(Gt,{page:_})):w});$t.displayName=`Link`;var en=x.forwardRef(function({"aria-current":e=`page`,caseSensitive:t=!1,className:n=``,end:r=!1,style:i,to:a,viewTransition:o,children:s,...c},l){let u=We(a,{relative:c.relative}),d=ze(),f=x.useContext(ke),{navigator:p,basename:m}=x.useContext(Ne),h=f!=null&&_n(u)&&o===!0,g=p.encodeLocation?p.encodeLocation(u).pathname:u.pathname,_=d.pathname,v=f&&f.navigation&&f.navigation.location?f.navigation.location.pathname:null;t||(_=_.toLowerCase(),v=v?v.toLowerCase():null,g=g.toLowerCase()),v&&m&&(v=me(v,m)||v);let y=g!==`/`&&g.endsWith(`/`)?g.length-1:g.length,b=_===g||!r&&_.startsWith(g)&&_.charAt(y)===`/`,S=v!=null&&(v===g||!r&&v.startsWith(g)&&v.charAt(g.length)===`/`),C={isActive:b,isPending:S,isTransitioning:h},w=b?e:void 0,T;T=typeof n==`function`?n(C):[n,b?`active`:null,S?`pending`:null,h?`transitioning`:null].filter(Boolean).join(` `);let E=typeof i==`function`?i(C):i;return x.createElement($t,{...c,"aria-current":w,className:T,ref:l,style:E,to:a,viewTransition:o},typeof s==`function`?s(C):s)});en.displayName=`NavLink`;var tn=x.forwardRef(({discover:e=`render`,fetcherKey:t,navigate:n,reloadDocument:r,replace:i,state:a,method:o=ht,action:s,onSubmit:c,relative:l,preventScrollReset:u,viewTransition:d,...f},p)=>{let m=un(),h=dn(s,{relative:l}),g=o.toLowerCase()===`get`?`get`:`post`,_=typeof s==`string`&&Qt.test(s);return x.createElement(`form`,{ref:p,method:g,action:h,onSubmit:r?c:e=>{if(c&&c(e),e.defaultPrevented)return;e.preventDefault();let r=e.nativeEvent.submitter,s=r?.getAttribute(`formmethod`)||o;m(r||e.currentTarget,{fetcherKey:t,method:s,navigate:n,replace:i,state:a,relative:l,preventScrollReset:u,viewTransition:d})},...f,"data-discover":!_&&e===`render`?`true`:void 0})});tn.displayName=`Form`;function nn({getKey:e,storageKey:t,...n}){let r=x.useContext(Vt),{basename:i}=x.useContext(Ne),a=ze(),o=at();hn({getKey:e,storageKey:t});let s=x.useMemo(()=>{if(!r||!e)return null;let t=mn(a,o,i,e);return t===a.key?null:t},[]);if(!r||r.isSpaMode)return null;let c=((e,t)=>{if(!window.history.state||!window.history.state.key){let e=Math.random().toString(32).slice(2);window.history.replaceState({key:e},``)}try{let n=JSON.parse(sessionStorage.getItem(e)||`{}`)[t||window.history.state.key];typeof n==`number`&&window.scrollTo(0,n)}catch(t){console.error(t),sessionStorage.removeItem(e)}}).toString();return x.createElement(`script`,{...n,suppressHydrationWarning:!0,dangerouslySetInnerHTML:{__html:`(${c})(${JSON.stringify(t||fn)}, ${JSON.stringify(s)})`}})}nn.displayName=`ScrollRestoration`;function rn(e){return`${e} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`}function an(e){let t=x.useContext(Oe);return w(t,rn(e)),t}function on(e){let t=x.useContext(ke);return w(t,rn(e)),t}function sn(e,{target:t,replace:n,state:r,preventScrollReset:i,relative:a,viewTransition:o}={}){let s=He(),c=ze(),l=We(e,{relative:a});return x.useCallback(u=>{St(u,t)&&(u.preventDefault(),s(e,{replace:n===void 0?k(c)===k(l):n,state:r,preventScrollReset:i,relative:a,viewTransition:o}))},[c,s,l,n,r,t,e,i,a,o])}var cn=0,ln=()=>`__${String(++cn)}__`;function un(){let{router:e}=an(`useSubmit`),{basename:t}=x.useContext(Ne),n=rt();return x.useCallback(async(r,i={})=>{let{action:a,method:o,encType:s,formData:c,body:l}=Dt(r,t);if(i.navigate===!1){let t=i.fetcherKey||ln();await e.fetch(t,n,i.action||a,{preventScrollReset:i.preventScrollReset,formData:c,body:l,formMethod:i.method||o,formEncType:i.encType||s,flushSync:i.flushSync})}else await e.navigate(i.action||a,{preventScrollReset:i.preventScrollReset,formData:c,body:l,formMethod:i.method||o,formEncType:i.encType||s,replace:i.replace,state:i.state,fromRouteId:n,flushSync:i.flushSync,viewTransition:i.viewTransition})},[e,t,n])}function dn(e,{relative:t}={}){let{basename:n}=x.useContext(Ne),r=x.useContext(Fe);w(r,`useFormAction must be used inside a RouteContext`);let[i]=r.matches.slice(-1),a={...We(e||`.`,{relative:t})},o=ze();if(e==null){a.search=o.search;let e=new URLSearchParams(a.search),t=e.getAll(`index`);if(t.some(e=>e===``)){e.delete(`index`),t.filter(e=>e).forEach(t=>e.append(`index`,t));let n=e.toString();a.search=n?`?${n}`:``}}return(!e||e===`.`)&&i.route.index&&(a.search=a.search?a.search.replace(/^\?/,`?index&`):`?index`),n!==`/`&&(a.pathname=a.pathname===`/`?n:Ce([n,a.pathname])),k(a)}var fn=`react-router-scroll-positions`,pn={};function mn(e,t,n,r){let i=null;return r&&(i=r(n===`/`?e:{...e,pathname:me(e.pathname,n)||e.pathname},t)),i??=e.key,i}function hn({getKey:e,storageKey:t}={}){let{router:n}=an(`useScrollRestoration`),{restoreScrollPosition:r,preventScrollReset:i}=on(`useScrollRestoration`),{basename:a}=x.useContext(Ne),o=ze(),s=at(),c=it();x.useEffect(()=>(window.history.scrollRestoration=`manual`,()=>{window.history.scrollRestoration=`auto`}),[]),gn(x.useCallback(()=>{if(c.state===`idle`){let t=mn(o,s,a,e);pn[t]=window.scrollY}try{sessionStorage.setItem(t||fn,JSON.stringify(pn))}catch(e){T(!1,`Failed to save scroll positions in sessionStorage, <ScrollRestoration /> will not work properly (${e}).`)}window.history.scrollRestoration=`auto`},[c.state,e,a,o,s,t])),typeof document<`u`&&(x.useLayoutEffect(()=>{try{let e=sessionStorage.getItem(t||fn);e&&(pn=JSON.parse(e))}catch{}},[t]),x.useLayoutEffect(()=>{let t=n?.enableScrollRestoration(pn,()=>window.scrollY,e?(t,n)=>mn(t,n,a,e):void 0);return()=>t&&t()},[n,a,e]),x.useLayoutEffect(()=>{if(r!==!1){if(typeof r==`number`){window.scrollTo(0,r);return}try{if(o.hash){let e=document.getElementById(decodeURIComponent(o.hash.slice(1)));if(e){e.scrollIntoView();return}}}catch{T(!1,`"${o.hash.slice(1)}" is not a decodable element ID. The view will not scroll to it.`)}i!==!0&&window.scrollTo(0,0)}},[o,r,i]))}function gn(e,t){let{capture:n}=t||{};x.useEffect(()=>{let t=n==null?void 0:{capture:n};return window.addEventListener(`pagehide`,e,t),()=>{window.removeEventListener(`pagehide`,e,t)}},[e,n])}function _n(e,{relative:t}={}){let n=x.useContext(Ae);w(n!=null,"`useViewTransitionState` must be used within `react-router-dom`'s `RouterProvider`.  Did you accidentally import `RouterProvider` from `react-router`?");let{basename:r}=an(`useViewTransitionState`),i=We(e,{relative:t});if(!n.isTransitioning)return!1;let a=me(n.currentLocation.pathname,r)||n.currentLocation.pathname,o=me(n.nextLocation.pathname,r)||n.nextLocation.pathname;return de(i.pathname,o)!=null||de(i.pathname,a)!=null}var vn={black:`#000`,white:`#fff`},yn={50:`#ffebee`,100:`#ffcdd2`,200:`#ef9a9a`,300:`#e57373`,400:`#ef5350`,500:`#f44336`,600:`#e53935`,700:`#d32f2f`,800:`#c62828`,900:`#b71c1c`,A100:`#ff8a80`,A200:`#ff5252`,A400:`#ff1744`,A700:`#d50000`},bn={50:`#f3e5f5`,100:`#e1bee7`,200:`#ce93d8`,300:`#ba68c8`,400:`#ab47bc`,500:`#9c27b0`,600:`#8e24aa`,700:`#7b1fa2`,800:`#6a1b9a`,900:`#4a148c`,A100:`#ea80fc`,A200:`#e040fb`,A400:`#d500f9`,A700:`#aa00ff`},xn={50:`#e3f2fd`,100:`#bbdefb`,200:`#90caf9`,300:`#64b5f6`,400:`#42a5f5`,500:`#2196f3`,600:`#1e88e5`,700:`#1976d2`,800:`#1565c0`,900:`#0d47a1`,A100:`#82b1ff`,A200:`#448aff`,A400:`#2979ff`,A700:`#2962ff`},Sn={50:`#e1f5fe`,100:`#b3e5fc`,200:`#81d4fa`,300:`#4fc3f7`,400:`#29b6f6`,500:`#03a9f4`,600:`#039be5`,700:`#0288d1`,800:`#0277bd`,900:`#01579b`,A100:`#80d8ff`,A200:`#40c4ff`,A400:`#00b0ff`,A700:`#0091ea`},Cn={50:`#e8f5e9`,100:`#c8e6c9`,200:`#a5d6a7`,300:`#81c784`,400:`#66bb6a`,500:`#4caf50`,600:`#43a047`,700:`#388e3c`,800:`#2e7d32`,900:`#1b5e20`,A100:`#b9f6ca`,A200:`#69f0ae`,A400:`#00e676`,A700:`#00c853`},wn={50:`#fff3e0`,100:`#ffe0b2`,200:`#ffcc80`,300:`#ffb74d`,400:`#ffa726`,500:`#ff9800`,600:`#fb8c00`,700:`#f57c00`,800:`#ef6c00`,900:`#e65100`,A100:`#ffd180`,A200:`#ffab40`,A400:`#ff9100`,A700:`#ff6d00`},Tn={50:`#fafafa`,100:`#f5f5f5`,200:`#eeeeee`,300:`#e0e0e0`,400:`#bdbdbd`,500:`#9e9e9e`,600:`#757575`,700:`#616161`,800:`#424242`,900:`#212121`,A100:`#f5f5f5`,A200:`#eeeeee`,A400:`#bdbdbd`,A700:`#616161`};function En(e,...t){let n=new URL(`https://mui.com/production-error/?code=${e}`);return t.forEach(e=>n.searchParams.append(`args[]`,e)),`Minified MUI error #${e}; visit ${n} for the full message.`}var Dn=`$$material`;function On(){return On=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)({}).hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e},On.apply(null,arguments)}var kn=!1;function An(e){if(e.sheet)return e.sheet;for(var t=0;t<document.styleSheets.length;t++)if(document.styleSheets[t].ownerNode===e)return document.styleSheets[t]}function jn(e){var t=document.createElement(`style`);return t.setAttribute(`data-emotion`,e.key),e.nonce!==void 0&&t.setAttribute(`nonce`,e.nonce),t.appendChild(document.createTextNode(``)),t.setAttribute(`data-s`,``),t}var Mn=function(){function e(e){var t=this;this._insertTag=function(e){var n=t.tags.length===0?t.insertionPoint?t.insertionPoint.nextSibling:t.prepend?t.container.firstChild:t.before:t.tags[t.tags.length-1].nextSibling;t.container.insertBefore(e,n),t.tags.push(e)},this.isSpeedy=e.speedy===void 0?!kn:e.speedy,this.tags=[],this.ctr=0,this.nonce=e.nonce,this.key=e.key,this.container=e.container,this.prepend=e.prepend,this.insertionPoint=e.insertionPoint,this.before=null}var t=e.prototype;return t.hydrate=function(e){e.forEach(this._insertTag)},t.insert=function(e){this.ctr%(this.isSpeedy?65e3:1)==0&&this._insertTag(jn(this));var t=this.tags[this.tags.length-1];if(this.isSpeedy){var n=An(t);try{n.insertRule(e,n.cssRules.length)}catch{}}else t.appendChild(document.createTextNode(e));this.ctr++},t.flush=function(){this.tags.forEach(function(e){return e.parentNode?.removeChild(e)}),this.tags=[],this.ctr=0},e}(),Nn=`-ms-`,Pn=`-moz-`,Fn=`-webkit-`,In=`comm`,Ln=`rule`,Rn=`decl`,zn=`@import`,Bn=`@keyframes`,Vn=`@layer`,Hn=Math.abs,Un=String.fromCharCode,Wn=Object.assign;function Gn(e,t){return Xn(e,0)^45?(((t<<2^Xn(e,0))<<2^Xn(e,1))<<2^Xn(e,2))<<2^Xn(e,3):0}function Kn(e){return e.trim()}function qn(e,t){return(e=t.exec(e))?e[0]:e}function Jn(e,t,n){return e.replace(t,n)}function Yn(e,t){return e.indexOf(t)}function Xn(e,t){return e.charCodeAt(t)|0}function Zn(e,t,n){return e.slice(t,n)}function Qn(e){return e.length}function $n(e){return e.length}function er(e,t){return t.push(e),e}function tr(e,t){return e.map(t).join(``)}var nr=1,rr=1,ir=0,ar=0,or=0,sr=``;function cr(e,t,n,r,i,a,o){return{value:e,root:t,parent:n,type:r,props:i,children:a,line:nr,column:rr,length:o,return:``}}function lr(e,t){return Wn(cr(``,null,null,``,null,null,0),e,{length:-e.length},t)}function ur(){return or}function dr(){return or=ar>0?Xn(sr,--ar):0,rr--,or===10&&(rr=1,nr--),or}function fr(){return or=ar<ir?Xn(sr,ar++):0,rr++,or===10&&(rr=1,nr++),or}function pr(){return Xn(sr,ar)}function mr(){return ar}function hr(e,t){return Zn(sr,e,t)}function gr(e){switch(e){case 0:case 9:case 10:case 13:case 32:return 5;case 33:case 43:case 44:case 47:case 62:case 64:case 126:case 59:case 123:case 125:return 4;case 58:return 3;case 34:case 39:case 40:case 91:return 2;case 41:case 93:return 1}return 0}function _r(e){return nr=rr=1,ir=Qn(sr=e),ar=0,[]}function vr(e){return sr=``,e}function yr(e){return Kn(hr(ar-1,Sr(e===91?e+2:e===40?e+1:e)))}function br(e){for(;(or=pr())&&or<33;)fr();return gr(e)>2||gr(or)>3?``:` `}function xr(e,t){for(;--t&&fr()&&!(or<48||or>102||or>57&&or<65||or>70&&or<97););return hr(e,mr()+(t<6&&pr()==32&&fr()==32))}function Sr(e){for(;fr();)switch(or){case e:return ar;case 34:case 39:e!==34&&e!==39&&Sr(or);break;case 40:e===41&&Sr(e);break;case 92:fr();break}return ar}function Cr(e,t){for(;fr()&&e+or!==57&&!(e+or===84&&pr()===47););return`/*`+hr(t,ar-1)+`*`+Un(e===47?e:fr())}function wr(e){for(;!gr(pr());)fr();return hr(e,ar)}function Tr(e){return vr(Er(``,null,null,null,[``],e=_r(e),0,[0],e))}function Er(e,t,n,r,i,a,o,s,c){for(var l=0,u=0,d=o,f=0,p=0,m=0,h=1,g=1,_=1,v=0,y=``,b=i,x=a,S=r,C=y;g;)switch(m=v,v=fr()){case 40:if(m!=108&&Xn(C,d-1)==58){Yn(C+=Jn(yr(v),`&`,`&\f`),`&\f`)!=-1&&(_=-1);break}case 34:case 39:case 91:C+=yr(v);break;case 9:case 10:case 13:case 32:C+=br(m);break;case 92:C+=xr(mr()-1,7);continue;case 47:switch(pr()){case 42:case 47:er(Or(Cr(fr(),mr()),t,n),c);break;default:C+=`/`}break;case 123*h:s[l++]=Qn(C)*_;case 125*h:case 59:case 0:switch(v){case 0:case 125:g=0;case 59+u:_==-1&&(C=Jn(C,/\f/g,``)),p>0&&Qn(C)-d&&er(p>32?kr(C+`;`,r,n,d-1):kr(Jn(C,` `,``)+`;`,r,n,d-2),c);break;case 59:C+=`;`;default:if(er(S=Dr(C,t,n,l,u,i,s,y,b=[],x=[],d),a),v===123)if(u===0)Er(C,t,S,S,b,a,d,s,x);else switch(f===99&&Xn(C,3)===110?100:f){case 100:case 108:case 109:case 115:Er(e,S,S,r&&er(Dr(e,S,S,0,0,i,s,y,i,b=[],d),x),i,x,d,s,r?b:x);break;default:Er(C,S,S,S,[``],x,0,s,x)}}l=u=p=0,h=_=1,y=C=``,d=o;break;case 58:d=1+Qn(C),p=m;default:if(h<1){if(v==123)--h;else if(v==125&&h++==0&&dr()==125)continue}switch(C+=Un(v),v*h){case 38:_=u>0?1:(C+=`\f`,-1);break;case 44:s[l++]=(Qn(C)-1)*_,_=1;break;case 64:pr()===45&&(C+=yr(fr())),f=pr(),u=d=Qn(y=C+=wr(mr())),v++;break;case 45:m===45&&Qn(C)==2&&(h=0)}}return a}function Dr(e,t,n,r,i,a,o,s,c,l,u){for(var d=i-1,f=i===0?a:[``],p=$n(f),m=0,h=0,g=0;m<r;++m)for(var _=0,v=Zn(e,d+1,d=Hn(h=o[m])),y=e;_<p;++_)(y=Kn(h>0?f[_]+` `+v:Jn(v,/&\f/g,f[_])))&&(c[g++]=y);return cr(e,t,n,i===0?Ln:s,c,l,u)}function Or(e,t,n){return cr(e,t,n,In,Un(ur()),Zn(e,2,-2),0)}function kr(e,t,n,r){return cr(e,t,n,Rn,Zn(e,0,r),Zn(e,r+1,-1),r)}function Ar(e,t){for(var n=``,r=$n(e),i=0;i<r;i++)n+=t(e[i],i,e,t)||``;return n}function jr(e,t,n,r){switch(e.type){case Vn:if(e.children.length)break;case zn:case Rn:return e.return=e.return||e.value;case In:return``;case Bn:return e.return=e.value+`{`+Ar(e.children,r)+`}`;case Ln:e.value=e.props.join(`,`)}return Qn(n=Ar(e.children,r))?e.return=e.value+`{`+n+`}`:``}function Mr(e){var t=$n(e);return function(n,r,i,a){for(var o=``,s=0;s<t;s++)o+=e[s](n,r,i,a)||``;return o}}function Nr(e){return function(t){t.root||(t=t.return)&&e(t)}}function Pr(e){var t=Object.create(null);return function(n){return t[n]===void 0&&(t[n]=e(n)),t[n]}}var Fr=function(e,t,n){for(var r=0,i=0;r=i,i=pr(),r===38&&i===12&&(t[n]=1),!gr(i);)fr();return hr(e,ar)},Ir=function(e,t){var n=-1,r=44;do switch(gr(r)){case 0:r===38&&pr()===12&&(t[n]=1),e[n]+=Fr(ar-1,t,n);break;case 2:e[n]+=yr(r);break;case 4:if(r===44){e[++n]=pr()===58?`&\f`:``,t[n]=e[n].length;break}default:e[n]+=Un(r)}while(r=fr());return e},Lr=function(e,t){return vr(Ir(_r(e),t))},Rr=new WeakMap,zr=function(e){if(!(e.type!==`rule`||!e.parent||e.length<1)){for(var t=e.value,n=e.parent,r=e.column===n.column&&e.line===n.line;n.type!==`rule`;)if(n=n.parent,!n)return;if(!(e.props.length===1&&t.charCodeAt(0)!==58&&!Rr.get(n))&&!r){Rr.set(e,!0);for(var i=[],a=Lr(t,i),o=n.props,s=0,c=0;s<a.length;s++)for(var l=0;l<o.length;l++,c++)e.props[c]=i[s]?a[s].replace(/&\f/g,o[l]):o[l]+` `+a[s]}}},Br=function(e){if(e.type===`decl`){var t=e.value;t.charCodeAt(0)===108&&t.charCodeAt(2)===98&&(e.return=``,e.value=``)}};function Vr(e,t){switch(Gn(e,t)){case 5103:return Fn+`print-`+e+e;case 5737:case 4201:case 3177:case 3433:case 1641:case 4457:case 2921:case 5572:case 6356:case 5844:case 3191:case 6645:case 3005:case 6391:case 5879:case 5623:case 6135:case 4599:case 4855:case 4215:case 6389:case 5109:case 5365:case 5621:case 3829:return Fn+e+e;case 5349:case 4246:case 4810:case 6968:case 2756:return Fn+e+Pn+e+Nn+e+e;case 6828:case 4268:return Fn+e+Nn+e+e;case 6165:return Fn+e+Nn+`flex-`+e+e;case 5187:return Fn+e+Jn(e,/(\w+).+(:[^]+)/,Fn+`box-$1$2`+Nn+`flex-$1$2`)+e;case 5443:return Fn+e+Nn+`flex-item-`+Jn(e,/flex-|-self/,``)+e;case 4675:return Fn+e+Nn+`flex-line-pack`+Jn(e,/align-content|flex-|-self/,``)+e;case 5548:return Fn+e+Nn+Jn(e,`shrink`,`negative`)+e;case 5292:return Fn+e+Nn+Jn(e,`basis`,`preferred-size`)+e;case 6060:return Fn+`box-`+Jn(e,`-grow`,``)+Fn+e+Nn+Jn(e,`grow`,`positive`)+e;case 4554:return Fn+Jn(e,/([^-])(transform)/g,`$1`+Fn+`$2`)+e;case 6187:return Jn(Jn(Jn(e,/(zoom-|grab)/,Fn+`$1`),/(image-set)/,Fn+`$1`),e,``)+e;case 5495:case 3959:return Jn(e,/(image-set\([^]*)/,Fn+"$1$`$1");case 4968:return Jn(Jn(e,/(.+:)(flex-)?(.*)/,Fn+`box-pack:$3`+Nn+`flex-pack:$3`),/s.+-b[^;]+/,`justify`)+Fn+e+e;case 4095:case 3583:case 4068:case 2532:return Jn(e,/(.+)-inline(.+)/,Fn+`$1$2`)+e;case 8116:case 7059:case 5753:case 5535:case 5445:case 5701:case 4933:case 4677:case 5533:case 5789:case 5021:case 4765:if(Qn(e)-1-t>6)switch(Xn(e,t+1)){case 109:if(Xn(e,t+4)!==45)break;case 102:return Jn(e,/(.+:)(.+)-([^]+)/,`$1`+Fn+`$2-$3$1`+Pn+(Xn(e,t+3)==108?`$3`:`$2-$3`))+e;case 115:return~Yn(e,`stretch`)?Vr(Jn(e,`stretch`,`fill-available`),t)+e:e}break;case 4949:if(Xn(e,t+1)!==115)break;case 6444:switch(Xn(e,Qn(e)-3-(~Yn(e,`!important`)&&10))){case 107:return Jn(e,`:`,`:`+Fn)+e;case 101:return Jn(e,/(.+:)([^;!]+)(;|!.+)?/,`$1`+Fn+(Xn(e,14)===45?`inline-`:``)+`box$3$1`+Fn+`$2$3$1`+Nn+`$2box$3`)+e}break;case 5936:switch(Xn(e,t+11)){case 114:return Fn+e+Nn+Jn(e,/[svh]\w+-[tblr]{2}/,`tb`)+e;case 108:return Fn+e+Nn+Jn(e,/[svh]\w+-[tblr]{2}/,`tb-rl`)+e;case 45:return Fn+e+Nn+Jn(e,/[svh]\w+-[tblr]{2}/,`lr`)+e}return Fn+e+Nn+e+e}return e}var Hr=[function(e,t,n,r){if(e.length>-1&&!e.return)switch(e.type){case Rn:e.return=Vr(e.value,e.length);break;case Bn:return Ar([lr(e,{value:Jn(e.value,`@`,`@`+Fn)})],r);case Ln:if(e.length)return tr(e.props,function(t){switch(qn(t,/(::plac\w+|:read-\w+)/)){case`:read-only`:case`:read-write`:return Ar([lr(e,{props:[Jn(t,/:(read-\w+)/,`:`+Pn+`$1`)]})],r);case`::placeholder`:return Ar([lr(e,{props:[Jn(t,/:(plac\w+)/,`:`+Fn+`input-$1`)]}),lr(e,{props:[Jn(t,/:(plac\w+)/,`:`+Pn+`$1`)]}),lr(e,{props:[Jn(t,/:(plac\w+)/,Nn+`input-$1`)]})],r)}return``})}}],Ur=function(e){var t=e.key;if(t===`css`){var n=document.querySelectorAll(`style[data-emotion]:not([data-s])`);Array.prototype.forEach.call(n,function(e){e.getAttribute(`data-emotion`).indexOf(` `)!==-1&&(document.head.appendChild(e),e.setAttribute(`data-s`,``))})}var r=e.stylisPlugins||Hr,i={},a,o=[];a=e.container||document.head,Array.prototype.forEach.call(document.querySelectorAll(`style[data-emotion^="`+t+` "]`),function(e){for(var t=e.getAttribute(`data-emotion`).split(` `),n=1;n<t.length;n++)i[t[n]]=!0;o.push(e)});var s,c=[zr,Br],l,u=[jr,Nr(function(e){l.insert(e)})],d=Mr(c.concat(r,u)),f=function(e){return Ar(Tr(e),d)};s=function(e,t,n,r){l=n,f(e?e+`{`+t.styles+`}`:t.styles),r&&(p.inserted[t.name]=!0)};var p={key:t,sheet:new Mn({key:t,container:a,nonce:e.nonce,speedy:e.speedy,prepend:e.prepend,insertionPoint:e.insertionPoint}),nonce:e.nonce,inserted:i,registered:{},insert:s};return p.sheet.hydrate(o),p},Wr=o((e=>{var t=typeof Symbol==`function`&&Symbol.for,n=t?Symbol.for(`react.element`):60103,r=t?Symbol.for(`react.portal`):60106,i=t?Symbol.for(`react.fragment`):60107,a=t?Symbol.for(`react.strict_mode`):60108,o=t?Symbol.for(`react.profiler`):60114,s=t?Symbol.for(`react.provider`):60109,c=t?Symbol.for(`react.context`):60110,l=t?Symbol.for(`react.async_mode`):60111,u=t?Symbol.for(`react.concurrent_mode`):60111,d=t?Symbol.for(`react.forward_ref`):60112,f=t?Symbol.for(`react.suspense`):60113,p=t?Symbol.for(`react.suspense_list`):60120,m=t?Symbol.for(`react.memo`):60115,h=t?Symbol.for(`react.lazy`):60116,g=t?Symbol.for(`react.block`):60121,_=t?Symbol.for(`react.fundamental`):60117,v=t?Symbol.for(`react.responder`):60118,y=t?Symbol.for(`react.scope`):60119;function b(e){if(typeof e==`object`&&e){var t=e.$$typeof;switch(t){case n:switch(e=e.type,e){case l:case u:case i:case o:case a:case f:return e;default:switch(e&&=e.$$typeof,e){case c:case d:case h:case m:case s:return e;default:return t}}case r:return t}}}function x(e){return b(e)===u}e.AsyncMode=l,e.ConcurrentMode=u,e.ContextConsumer=c,e.ContextProvider=s,e.Element=n,e.ForwardRef=d,e.Fragment=i,e.Lazy=h,e.Memo=m,e.Portal=r,e.Profiler=o,e.StrictMode=a,e.Suspense=f,e.isAsyncMode=function(e){return x(e)||b(e)===l},e.isConcurrentMode=x,e.isContextConsumer=function(e){return b(e)===c},e.isContextProvider=function(e){return b(e)===s},e.isElement=function(e){return typeof e==`object`&&!!e&&e.$$typeof===n},e.isForwardRef=function(e){return b(e)===d},e.isFragment=function(e){return b(e)===i},e.isLazy=function(e){return b(e)===h},e.isMemo=function(e){return b(e)===m},e.isPortal=function(e){return b(e)===r},e.isProfiler=function(e){return b(e)===o},e.isStrictMode=function(e){return b(e)===a},e.isSuspense=function(e){return b(e)===f},e.isValidElementType=function(e){return typeof e==`string`||typeof e==`function`||e===i||e===u||e===o||e===a||e===f||e===p||typeof e==`object`&&!!e&&(e.$$typeof===h||e.$$typeof===m||e.$$typeof===s||e.$$typeof===c||e.$$typeof===d||e.$$typeof===_||e.$$typeof===v||e.$$typeof===y||e.$$typeof===g)},e.typeOf=b})),Gr=o(((e,t)=>{t.exports=Wr()})),Kr=o(((e,t)=>{var n=Gr(),r={childContextTypes:!0,contextType:!0,contextTypes:!0,defaultProps:!0,displayName:!0,getDefaultProps:!0,getDerivedStateFromError:!0,getDerivedStateFromProps:!0,mixins:!0,propTypes:!0,type:!0},i={name:!0,length:!0,prototype:!0,caller:!0,callee:!0,arguments:!0,arity:!0},a={$$typeof:!0,render:!0,defaultProps:!0,displayName:!0,propTypes:!0},o={$$typeof:!0,compare:!0,defaultProps:!0,displayName:!0,propTypes:!0,type:!0},s={};s[n.ForwardRef]=a,s[n.Memo]=o;function c(e){return n.isMemo(e)?o:s[e.$$typeof]||r}var l=Object.defineProperty,u=Object.getOwnPropertyNames,d=Object.getOwnPropertySymbols,f=Object.getOwnPropertyDescriptor,p=Object.getPrototypeOf,m=Object.prototype;function h(e,t,n){if(typeof t!=`string`){if(m){var r=p(t);r&&r!==m&&h(e,r,n)}var a=u(t);d&&(a=a.concat(d(t)));for(var o=c(e),s=c(t),g=0;g<a.length;++g){var _=a[g];if(!i[_]&&!(n&&n[_])&&!(s&&s[_])&&!(o&&o[_])){var v=f(t,_);try{l(e,_,v)}catch{}}}}return e}t.exports=h})),qr=!0;function Jr(e,t,n){var r=``;return n.split(` `).forEach(function(n){e[n]===void 0?n&&(r+=n+` `):t.push(e[n]+`;`)}),r}var Yr=function(e,t,n){var r=e.key+`-`+t.name;(n===!1||qr===!1)&&e.registered[r]===void 0&&(e.registered[r]=t.styles)},Xr=function(e,t,n){Yr(e,t,n);var r=e.key+`-`+t.name;if(e.inserted[t.name]===void 0){var i=t;do e.insert(t===i?`.`+r:``,i,e.sheet,!0),i=i.next;while(i!==void 0)}};function Zr(e){for(var t=0,n,r=0,i=e.length;i>=4;++r,i-=4)n=e.charCodeAt(r)&255|(e.charCodeAt(++r)&255)<<8|(e.charCodeAt(++r)&255)<<16|(e.charCodeAt(++r)&255)<<24,n=(n&65535)*1540483477+((n>>>16)*59797<<16),n^=n>>>24,t=(n&65535)*1540483477+((n>>>16)*59797<<16)^(t&65535)*1540483477+((t>>>16)*59797<<16);switch(i){case 3:t^=(e.charCodeAt(r+2)&255)<<16;case 2:t^=(e.charCodeAt(r+1)&255)<<8;case 1:t^=e.charCodeAt(r)&255,t=(t&65535)*1540483477+((t>>>16)*59797<<16)}return t^=t>>>13,t=(t&65535)*1540483477+((t>>>16)*59797<<16),((t^t>>>15)>>>0).toString(36)}var Qr={animationIterationCount:1,aspectRatio:1,borderImageOutset:1,borderImageSlice:1,borderImageWidth:1,boxFlex:1,boxFlexGroup:1,boxOrdinalGroup:1,columnCount:1,columns:1,flex:1,flexGrow:1,flexPositive:1,flexShrink:1,flexNegative:1,flexOrder:1,gridRow:1,gridRowEnd:1,gridRowSpan:1,gridRowStart:1,gridColumn:1,gridColumnEnd:1,gridColumnSpan:1,gridColumnStart:1,msGridRow:1,msGridRowSpan:1,msGridColumn:1,msGridColumnSpan:1,fontWeight:1,lineHeight:1,opacity:1,order:1,orphans:1,scale:1,tabSize:1,widows:1,zIndex:1,zoom:1,WebkitLineClamp:1,fillOpacity:1,floodOpacity:1,stopOpacity:1,strokeDasharray:1,strokeDashoffset:1,strokeMiterlimit:1,strokeOpacity:1,strokeWidth:1},$r=!1,ei=/[A-Z]|^ms/g,ti=/_EMO_([^_]+?)_([^]*?)_EMO_/g,ni=function(e){return e.charCodeAt(1)===45},ri=function(e){return e!=null&&typeof e!=`boolean`},ii=Pr(function(e){return ni(e)?e:e.replace(ei,`-$&`).toLowerCase()}),ai=function(e,t){switch(e){case`animation`:case`animationName`:if(typeof t==`string`)return t.replace(ti,function(e,t,n){return ui={name:t,styles:n,next:ui},t})}return Qr[e]!==1&&!ni(e)&&typeof t==`number`&&t!==0?t+`px`:t},oi=`Component selectors can only be used in conjunction with @emotion/babel-plugin, the swc Emotion plugin, or another Emotion-aware compiler transform.`;function si(e,t,n){if(n==null)return``;var r=n;if(r.__emotion_styles!==void 0)return r;switch(typeof n){case`boolean`:return``;case`object`:var i=n;if(i.anim===1)return ui={name:i.name,styles:i.styles,next:ui},i.name;var a=n;if(a.styles!==void 0){var o=a.next;if(o!==void 0)for(;o!==void 0;)ui={name:o.name,styles:o.styles,next:ui},o=o.next;return a.styles+`;`}return ci(e,t,n);case`function`:if(e!==void 0){var s=ui,c=n(e);return ui=s,si(e,t,c)}break}var l=n;if(t==null)return l;var u=t[l];return u===void 0?l:u}function ci(e,t,n){var r=``;if(Array.isArray(n))for(var i=0;i<n.length;i++)r+=si(e,t,n[i])+`;`;else for(var a in n){var o=n[a];if(typeof o!=`object`){var s=o;t!=null&&t[s]!==void 0?r+=a+`{`+t[s]+`}`:ri(s)&&(r+=ii(a)+`:`+ai(a,s)+`;`)}else{if(a===`NO_COMPONENT_SELECTOR`&&$r)throw Error(oi);if(Array.isArray(o)&&typeof o[0]==`string`&&(t==null||t[o[0]]===void 0))for(var c=0;c<o.length;c++)ri(o[c])&&(r+=ii(a)+`:`+ai(a,o[c])+`;`);else{var l=si(e,t,o);switch(a){case`animation`:case`animationName`:r+=ii(a)+`:`+l+`;`;break;default:r+=a+`{`+l+`}`}}}}return r}var li=/label:\s*([^\s;{]+)\s*(;|$)/g,ui;function di(e,t,n){if(e.length===1&&typeof e[0]==`object`&&e[0]!==null&&e[0].styles!==void 0)return e[0];var r=!0,i=``;ui=void 0;var a=e[0];a==null||a.raw===void 0?(r=!1,i+=si(n,t,a)):i+=a[0];for(var o=1;o<e.length;o++)i+=si(n,t,e[o]),r&&(i+=a[o]);li.lastIndex=0;for(var s=``,c;(c=li.exec(i))!==null;)s+=`-`+c[1];return{name:Zr(i)+s,styles:i,next:ui}}var fi=function(e){return e()},pi=x.useInsertionEffect?x.useInsertionEffect:!1,mi=pi||fi,hi=pi||x.useLayoutEffect,gi=x.createContext(typeof HTMLElement<`u`?Ur({key:`css`}):null);gi.Provider;var _i=function(e){return(0,x.forwardRef)(function(t,n){return e(t,(0,x.useContext)(gi),n)})},vi=x.createContext({}),yi={}.hasOwnProperty,bi=`__EMOTION_TYPE_PLEASE_DO_NOT_USE__`,xi=function(e,t){var n={};for(var r in t)yi.call(t,r)&&(n[r]=t[r]);return n[bi]=e,n},Si=function(e){var t=e.cache,n=e.serialized,r=e.isStringTag;return Yr(t,n,r),mi(function(){return Xr(t,n,r)}),null},Ci=_i(function(e,t,n){var r=e.css;typeof r==`string`&&t.registered[r]!==void 0&&(r=t.registered[r]);var i=e[bi],a=[r],o=``;typeof e.className==`string`?o=Jr(t.registered,a,e.className):e.className!=null&&(o=e.className+` `);var s=di(a,void 0,x.useContext(vi));o+=t.key+`-`+s.name;var c={};for(var l in e)yi.call(e,l)&&l!==`css`&&l!==bi&&(c[l]=e[l]);return c.className=o,n&&(c.ref=n),x.createElement(x.Fragment,null,x.createElement(Si,{cache:t,serialized:s,isStringTag:typeof i==`string`}),x.createElement(i,c))});Kr();var wi=function(e,t){var n=arguments;if(t==null||!yi.call(t,`css`))return x.createElement.apply(void 0,n);var r=n.length,i=Array(r);i[0]=Ci,i[1]=xi(e,t);for(var a=2;a<r;a++)i[a]=n[a];return x.createElement.apply(null,i)};(function(e){var t;(function(e){})(t||=e.JSX||={})})(wi||={});var Ti=_i(function(e,t){var n=e.styles,r=di([n],void 0,x.useContext(vi)),i=x.useRef();return hi(function(){var e=t.key+`-global`,n=new t.sheet.constructor({key:e,nonce:t.sheet.nonce,container:t.sheet.container,speedy:t.sheet.isSpeedy}),a=!1,o=document.querySelector(`style[data-emotion="`+e+` `+r.name+`"]`);return t.sheet.tags.length&&(n.before=t.sheet.tags[0]),o!==null&&(a=!0,o.setAttribute(`data-emotion`,e),n.hydrate([o])),i.current=[n,a],function(){n.flush()}},[t]),hi(function(){var e=i.current,n=e[0];if(e[1]){e[1]=!1;return}r.next!==void 0&&Xr(t,r.next,!0),n.tags.length&&(n.before=n.tags[n.tags.length-1].nextElementSibling,n.flush()),t.insert(``,r,n,!1)},[t,r.name]),null});function Ei(){return di([...arguments])}function Di(){var e=Ei.apply(void 0,arguments),t=`animation-`+e.name;return{name:t,styles:`@keyframes `+t+`{`+e.styles+`}`,anim:1,toString:function(){return`_EMO_`+this.name+`_`+this.styles+`_EMO_`}}}var Oi=/^((children|dangerouslySetInnerHTML|key|ref|autoFocus|defaultValue|defaultChecked|innerHTML|suppressContentEditableWarning|suppressHydrationWarning|valueLink|abbr|accept|acceptCharset|accessKey|action|allow|allowUserMedia|allowPaymentRequest|allowFullScreen|allowTransparency|alt|async|autoComplete|autoPlay|capture|cellPadding|cellSpacing|challenge|charSet|checked|cite|classID|className|cols|colSpan|content|contentEditable|contextMenu|controls|controlsList|coords|crossOrigin|data|dateTime|decoding|default|defer|dir|disabled|disablePictureInPicture|disableRemotePlayback|download|draggable|encType|enterKeyHint|fetchpriority|fetchPriority|form|formAction|formEncType|formMethod|formNoValidate|formTarget|frameBorder|headers|height|hidden|high|href|hrefLang|htmlFor|httpEquiv|id|inputMode|integrity|is|keyParams|keyType|kind|label|lang|list|loading|loop|low|marginHeight|marginWidth|max|maxLength|media|mediaGroup|method|min|minLength|multiple|muted|name|nonce|noValidate|open|optimum|pattern|placeholder|playsInline|popover|popoverTarget|popoverTargetAction|poster|preload|profile|radioGroup|readOnly|referrerPolicy|rel|required|reversed|role|rows|rowSpan|sandbox|scope|scoped|scrolling|seamless|selected|shape|size|sizes|slot|span|spellCheck|src|srcDoc|srcLang|srcSet|start|step|style|summary|tabIndex|target|title|translate|type|useMap|value|width|wmode|wrap|about|datatype|inlist|prefix|property|resource|typeof|vocab|autoCapitalize|autoCorrect|autoSave|color|incremental|fallback|inert|itemProp|itemScope|itemType|itemID|itemRef|on|option|results|security|unselectable|accentHeight|accumulate|additive|alignmentBaseline|allowReorder|alphabetic|amplitude|arabicForm|ascent|attributeName|attributeType|autoReverse|azimuth|baseFrequency|baselineShift|baseProfile|bbox|begin|bias|by|calcMode|capHeight|clip|clipPathUnits|clipPath|clipRule|colorInterpolation|colorInterpolationFilters|colorProfile|colorRendering|contentScriptType|contentStyleType|cursor|cx|cy|d|decelerate|descent|diffuseConstant|direction|display|divisor|dominantBaseline|dur|dx|dy|edgeMode|elevation|enableBackground|end|exponent|externalResourcesRequired|fill|fillOpacity|fillRule|filter|filterRes|filterUnits|floodColor|floodOpacity|focusable|fontFamily|fontSize|fontSizeAdjust|fontStretch|fontStyle|fontVariant|fontWeight|format|from|fr|fx|fy|g1|g2|glyphName|glyphOrientationHorizontal|glyphOrientationVertical|glyphRef|gradientTransform|gradientUnits|hanging|horizAdvX|horizOriginX|ideographic|imageRendering|in|in2|intercept|k|k1|k2|k3|k4|kernelMatrix|kernelUnitLength|kerning|keyPoints|keySplines|keyTimes|lengthAdjust|letterSpacing|lightingColor|limitingConeAngle|local|markerEnd|markerMid|markerStart|markerHeight|markerUnits|markerWidth|mask|maskContentUnits|maskUnits|mathematical|mode|numOctaves|offset|opacity|operator|order|orient|orientation|origin|overflow|overlinePosition|overlineThickness|panose1|paintOrder|pathLength|patternContentUnits|patternTransform|patternUnits|pointerEvents|points|pointsAtX|pointsAtY|pointsAtZ|preserveAlpha|preserveAspectRatio|primitiveUnits|r|radius|refX|refY|renderingIntent|repeatCount|repeatDur|requiredExtensions|requiredFeatures|restart|result|rotate|rx|ry|scale|seed|shapeRendering|slope|spacing|specularConstant|specularExponent|speed|spreadMethod|startOffset|stdDeviation|stemh|stemv|stitchTiles|stopColor|stopOpacity|strikethroughPosition|strikethroughThickness|string|stroke|strokeDasharray|strokeDashoffset|strokeLinecap|strokeLinejoin|strokeMiterlimit|strokeOpacity|strokeWidth|surfaceScale|systemLanguage|tableValues|targetX|targetY|textAnchor|textDecoration|textRendering|textLength|to|transform|u1|u2|underlinePosition|underlineThickness|unicode|unicodeBidi|unicodeRange|unitsPerEm|vAlphabetic|vHanging|vIdeographic|vMathematical|values|vectorEffect|version|vertAdvY|vertOriginX|vertOriginY|viewBox|viewTarget|visibility|widths|wordSpacing|writingMode|x|xHeight|x1|x2|xChannelSelector|xlinkActuate|xlinkArcrole|xlinkHref|xlinkRole|xlinkShow|xlinkTitle|xlinkType|xmlBase|xmlns|xmlnsXlink|xmlLang|xmlSpace|y|y1|y2|yChannelSelector|z|zoomAndPan|for|class|autofocus)|(([Dd][Aa][Tt][Aa]|[Aa][Rr][Ii][Aa]|x)-.*))$/,ki=Pr(function(e){return Oi.test(e)||e.charCodeAt(0)===111&&e.charCodeAt(1)===110&&e.charCodeAt(2)<91}),Ai=!1,ji=ki,Mi=function(e){return e!==`theme`},Ni=function(e){return typeof e==`string`&&e.charCodeAt(0)>96?ji:Mi},Pi=function(e,t,n){var r;if(t){var i=t.shouldForwardProp;r=e.__emotion_forwardProp&&i?function(t){return e.__emotion_forwardProp(t)&&i(t)}:i}return typeof r!=`function`&&n&&(r=e.__emotion_forwardProp),r},Fi=function(e){var t=e.cache,n=e.serialized,r=e.isStringTag;return Yr(t,n,r),mi(function(){return Xr(t,n,r)}),null},Ii=function e(t,n){var r=t.__emotion_real===t,i=r&&t.__emotion_base||t,a,o;n!==void 0&&(a=n.label,o=n.target);var s=Pi(t,n,r),c=s||Ni(i),l=!c(`as`);return function(){var u=arguments,d=r&&t.__emotion_styles!==void 0?t.__emotion_styles.slice(0):[];if(a!==void 0&&d.push(`label:`+a+`;`),u[0]==null||u[0].raw===void 0)d.push.apply(d,u);else{var f=u[0];d.push(f[0]);for(var p=u.length,m=1;m<p;m++)d.push(u[m],f[m])}var h=_i(function(e,t,n){var r=l&&e.as||i,a=``,u=[],f=e;if(e.theme==null){for(var p in f={},e)f[p]=e[p];f.theme=x.useContext(vi)}typeof e.className==`string`?a=Jr(t.registered,u,e.className):e.className!=null&&(a=e.className+` `);var m=di(d.concat(u),t.registered,f);a+=t.key+`-`+m.name,o!==void 0&&(a+=` `+o);var h=l&&s===void 0?Ni(r):c,g={};for(var _ in e)l&&_===`as`||h(_)&&(g[_]=e[_]);return g.className=a,n&&(g.ref=n),x.createElement(x.Fragment,null,x.createElement(Fi,{cache:t,serialized:m,isStringTag:typeof r==`string`}),x.createElement(r,g))});return h.displayName=a===void 0?`Styled(`+(typeof i==`string`?i:i.displayName||i.name||`Component`)+`)`:a,h.defaultProps=t.defaultProps,h.__emotion_real=h,h.__emotion_base=i,h.__emotion_styles=d,h.__emotion_forwardProp=s,Object.defineProperty(h,`toString`,{value:function(){return o===void 0&&Ai?`NO_COMPONENT_SELECTOR`:`.`+o}}),h.withComponent=function(t,r){return e(t,On({},n,r,{shouldForwardProp:Pi(h,r,!0)})).apply(void 0,d)},h}},Li=`a.abbr.address.area.article.aside.audio.b.base.bdi.bdo.big.blockquote.body.br.button.canvas.caption.cite.code.col.colgroup.data.datalist.dd.del.details.dfn.dialog.div.dl.dt.em.embed.fieldset.figcaption.figure.footer.form.h1.h2.h3.h4.h5.h6.head.header.hgroup.hr.html.i.iframe.img.input.ins.kbd.keygen.label.legend.li.link.main.map.mark.marquee.menu.menuitem.meta.meter.nav.noscript.object.ol.optgroup.option.output.p.param.picture.pre.progress.q.rp.rt.ruby.s.samp.script.section.select.small.source.span.strong.style.sub.summary.sup.table.tbody.td.textarea.tfoot.th.thead.time.title.tr.track.u.ul.var.video.wbr.circle.clipPath.defs.ellipse.foreignObject.g.image.line.linearGradient.mask.path.pattern.polygon.polyline.radialGradient.rect.stop.svg.text.tspan`.split(`.`),Ri=Ii.bind(null);Li.forEach(function(e){Ri[e]=Ri(e)});var zi=o((e=>{var t=Symbol.for(`react.transitional.element`),n=Symbol.for(`react.fragment`);function r(e,n,r){var i=null;if(r!==void 0&&(i=``+r),n.key!==void 0&&(i=``+n.key),`key`in n)for(var a in r={},n)a!==`key`&&(r[a]=n[a]);else r=n;return n=r.ref,{$$typeof:t,type:e,key:i,ref:n===void 0?null:n,props:r}}e.Fragment=n,e.jsx=r,e.jsxs=r})),R=o(((e,t)=>{t.exports=zi()}))();function Bi(e){return e==null||Object.keys(e).length===0}function Vi(e){let{styles:t,defaultTheme:n={}}=e;return(0,R.jsx)(Ti,{styles:typeof t==`function`?e=>t(Bi(e)?n:e):t})}function Hi(e,t){return Ri(e,t)}function Ui(e,t){Array.isArray(e.__emotion_styles)&&(e.__emotion_styles=t(e.__emotion_styles))}var Wi=[];function Gi(e){return Wi[0]=e,di(Wi)}var Ki=o((e=>{var t=Symbol.for(`react.fragment`),n=Symbol.for(`react.strict_mode`),r=Symbol.for(`react.profiler`),i=Symbol.for(`react.consumer`),a=Symbol.for(`react.context`),o=Symbol.for(`react.forward_ref`),s=Symbol.for(`react.suspense`),c=Symbol.for(`react.suspense_list`),l=Symbol.for(`react.memo`),u=Symbol.for(`react.lazy`),d=Symbol.for(`react.client.reference`);e.isValidElementType=function(e){return!!(typeof e==`string`||typeof e==`function`||e===t||e===r||e===n||e===s||e===c||typeof e==`object`&&e&&(e.$$typeof===u||e.$$typeof===l||e.$$typeof===a||e.$$typeof===i||e.$$typeof===o||e.$$typeof===d||e.getModuleId!==void 0))}})),qi=o(((e,t)=>{t.exports=Ki()}))();function Ji(e){if(typeof e!=`object`||!e)return!1;let t=Object.getPrototypeOf(e);return(t===null||t===Object.prototype||Object.getPrototypeOf(t)===null)&&!(Symbol.toStringTag in e)&&!(Symbol.iterator in e)}function Yi(e){if(x.isValidElement(e)||(0,qi.isValidElementType)(e)||!Ji(e))return e;let t={};return Object.keys(e).forEach(n=>{t[n]=Yi(e[n])}),t}function Xi(e,t,n={clone:!0}){let r=n.clone?{...e}:e;return Ji(e)&&Ji(t)&&Object.keys(t).forEach(i=>{x.isValidElement(t[i])||(0,qi.isValidElementType)(t[i])?r[i]=t[i]:Ji(t[i])&&Object.prototype.hasOwnProperty.call(e,i)&&Ji(e[i])?r[i]=Xi(e[i],t[i],n):n.clone?r[i]=Ji(t[i])?Yi(t[i]):t[i]:r[i]=t[i]}),r}var Zi=e=>{let t=Object.keys(e).map(t=>({key:t,val:e[t]}))||[];return t.sort((e,t)=>e.val-t.val),t.reduce((e,t)=>({...e,[t.key]:t.val}),{})};function Qi(e){let{values:t={xs:0,sm:600,md:900,lg:1200,xl:1536},unit:n=`px`,step:r=5,...i}=e,a=Zi(t),o=Object.keys(a);function s(e){return`@media (min-width:${typeof t[e]==`number`?t[e]:e}${n})`}function c(e){return`@media (max-width:${(typeof t[e]==`number`?t[e]:e)-r/100}${n})`}function l(e,i){let a=o.indexOf(i);return`@media (min-width:${typeof t[e]==`number`?t[e]:e}${n}) and (max-width:${(a!==-1&&typeof t[o[a]]==`number`?t[o[a]]:i)-r/100}${n})`}function u(e){return o.indexOf(e)+1<o.length?l(e,o[o.indexOf(e)+1]):s(e)}function d(e){let t=o.indexOf(e);return t===0?s(o[1]):t===o.length-1?c(o[t]):l(e,o[o.indexOf(e)+1]).replace(`@media`,`@media not all and`)}return{keys:o,values:a,up:s,down:c,between:l,only:u,not:d,unit:n,...i}}function $i(e,t){if(!e.containerQueries)return t;let n=Object.keys(t).filter(e=>e.startsWith(`@container`)).sort((e,t)=>{let n=/min-width:\s*([0-9.]+)/;return(e.match(n)?.[1]||0)-+(t.match(n)?.[1]||0)});return n.length?n.reduce((e,n)=>{let r=t[n];return delete e[n],e[n]=r,e},{...t}):t}function ea(e,t){return t===`@`||t.startsWith(`@`)&&(e.some(e=>t.startsWith(`@${e}`))||!!t.match(/^@\d/))}function ta(e,t){let n=t.match(/^@([^/]+)?\/?(.+)?$/);if(!n)return null;let[,r,i]=n,a=Number.isNaN(+r)?r||0:+r;return e.containerQueries(i).up(a)}function na(e){let t=(e,t)=>e.replace(`@media`,t?`@container ${t}`:`@container`);function n(n,r){n.up=(...n)=>t(e.breakpoints.up(...n),r),n.down=(...n)=>t(e.breakpoints.down(...n),r),n.between=(...n)=>t(e.breakpoints.between(...n),r),n.only=(...n)=>t(e.breakpoints.only(...n),r),n.not=(...n)=>{let i=t(e.breakpoints.not(...n),r);return i.includes(`not all and`)?i.replace(`not all and `,``).replace(`min-width:`,`width<`).replace(`max-width:`,`width>`).replace(`and`,`or`):i}}let r={},i=e=>(n(r,e),r);return n(i),{...e,containerQueries:i}}var ra={borderRadius:4};function ia(e,t){return t?Xi(e,t,{clone:!1}):e}var aa=ia;const oa={xs:0,sm:600,md:900,lg:1200,xl:1536};var sa={keys:[`xs`,`sm`,`md`,`lg`,`xl`],up:e=>`@media (min-width:${oa[e]}px)`},ca={containerQueries:e=>({up:t=>{let n=typeof t==`number`?t:oa[t]||t;return typeof n==`number`&&(n=`${n}px`),e?`@container ${e} (min-width:${n})`:`@container (min-width:${n})`}})};function la(e,t,n){let r=e.theme||{};if(Array.isArray(t)){let e=r.breakpoints||sa;return t.reduce((r,i,a)=>(r[e.up(e.keys[a])]=n(t[a]),r),{})}if(typeof t==`object`){let e=r.breakpoints||sa;return Object.keys(t).reduce((i,a)=>{if(ea(e.keys,a)){let e=ta(r.containerQueries?r:ca,a);e&&(i[e]=n(t[a],a))}else if(Object.keys(e.values||oa).includes(a)){let r=e.up(a);i[r]=n(t[a],a)}else{let e=a;i[e]=t[e]}return i},{})}return n(t)}function ua(e={}){return e.keys?.reduce((t,n)=>{let r=e.up(n);return t[r]={},t},{})||{}}function da(e,t){return e.reduce((e,t)=>{let n=e[t];return(!n||Object.keys(n).length===0)&&delete e[t],e},t)}function fa(e){if(typeof e!=`string`)throw Error(En(7));return e.charAt(0).toUpperCase()+e.slice(1)}function pa(e,t,n=!0){if(!t||typeof t!=`string`)return null;if(e&&e.vars&&n){let n=`vars.${t}`.split(`.`).reduce((e,t)=>e&&e[t]?e[t]:null,e);if(n!=null)return n}return t.split(`.`).reduce((e,t)=>e&&e[t]!=null?e[t]:null,e)}function ma(e,t,n,r=n){let i;return i=typeof e==`function`?e(n):Array.isArray(e)?e[n]||r:pa(e,n)||r,t&&(i=t(i,r,e)),i}function ha(e){let{prop:t,cssProperty:n=e.prop,themeKey:r,transform:i}=e,a=e=>{if(e[t]==null)return null;let a=e[t],o=e.theme,s=pa(o,r)||{};return la(e,a,e=>{let r=ma(s,i,e);return e===r&&typeof e==`string`&&(r=ma(s,i,`${t}${e===`default`?``:fa(e)}`,e)),n===!1?r:{[n]:r}})};return a.propTypes={},a.filterProps=[t],a}var ga=ha;function _a(e){let t={};return n=>(t[n]===void 0&&(t[n]=e(n)),t[n])}var va={m:`margin`,p:`padding`},ya={t:`Top`,r:`Right`,b:`Bottom`,l:`Left`,x:[`Left`,`Right`],y:[`Top`,`Bottom`]},ba={marginX:`mx`,marginY:`my`,paddingX:`px`,paddingY:`py`},xa=_a(e=>{if(e.length>2)if(ba[e])e=ba[e];else return[e];let[t,n]=e.split(``),r=va[t],i=ya[n]||``;return Array.isArray(i)?i.map(e=>r+e):[r+i]});const Sa=[`m`,`mt`,`mr`,`mb`,`ml`,`mx`,`my`,`margin`,`marginTop`,`marginRight`,`marginBottom`,`marginLeft`,`marginX`,`marginY`,`marginInline`,`marginInlineStart`,`marginInlineEnd`,`marginBlock`,`marginBlockStart`,`marginBlockEnd`],Ca=[`p`,`pt`,`pr`,`pb`,`pl`,`px`,`py`,`padding`,`paddingTop`,`paddingRight`,`paddingBottom`,`paddingLeft`,`paddingX`,`paddingY`,`paddingInline`,`paddingInlineStart`,`paddingInlineEnd`,`paddingBlock`,`paddingBlockStart`,`paddingBlockEnd`];var wa=[...Sa,...Ca];function Ta(e,t,n,r){let i=pa(e,t,!0)??n;return typeof i==`number`||typeof i==`string`?e=>typeof e==`string`?e:typeof i==`string`?i.startsWith(`var(`)&&e===0?0:i.startsWith(`var(`)&&e===1?i:`calc(${e} * ${i})`:i*e:Array.isArray(i)?e=>{if(typeof e==`string`)return e;let t=i[Math.abs(e)];return e>=0?t:typeof t==`number`?-t:typeof t==`string`&&t.startsWith(`var(`)?`calc(-1 * ${t})`:`-${t}`}:typeof i==`function`?i:()=>void 0}function Ea(e){return Ta(e,`spacing`,8,`spacing`)}function Da(e,t){return typeof t==`string`||t==null?t:e(t)}function Oa(e,t){return n=>e.reduce((e,r)=>(e[r]=Da(t,n),e),{})}function ka(e,t,n,r){if(!t.includes(n))return null;let i=Oa(xa(n),r),a=e[n];return la(e,a,i)}function Aa(e,t){let n=Ea(e.theme);return Object.keys(e).map(r=>ka(e,t,r,n)).reduce(aa,{})}function ja(e){return Aa(e,Sa)}ja.propTypes={},ja.filterProps=Sa;function Ma(e){return Aa(e,Ca)}Ma.propTypes={},Ma.filterProps=Ca;function Na(e){return Aa(e,wa)}Na.propTypes={},Na.filterProps=wa;function Pa(e=8,t=Ea({spacing:e})){if(e.mui)return e;let n=(...e)=>(e.length===0?[1]:e).map(e=>{let n=t(e);return typeof n==`number`?`${n}px`:n}).join(` `);return n.mui=!0,n}function Fa(...e){let t=e.reduce((e,t)=>(t.filterProps.forEach(n=>{e[n]=t}),e),{}),n=e=>Object.keys(e).reduce((n,r)=>t[r]?aa(n,t[r](e)):n,{});return n.propTypes={},n.filterProps=e.reduce((e,t)=>e.concat(t.filterProps),[]),n}var Ia=Fa;function La(e){return typeof e==`number`?`${e}px solid`:e}function Ra(e,t){return ga({prop:e,themeKey:`borders`,transform:t})}const za=Ra(`border`,La),Ba=Ra(`borderTop`,La),Va=Ra(`borderRight`,La),Ha=Ra(`borderBottom`,La),Ua=Ra(`borderLeft`,La),Wa=Ra(`borderColor`),Ga=Ra(`borderTopColor`),Ka=Ra(`borderRightColor`),qa=Ra(`borderBottomColor`),Ja=Ra(`borderLeftColor`),Ya=Ra(`outline`,La),Xa=Ra(`outlineColor`),Za=e=>{if(e.borderRadius!==void 0&&e.borderRadius!==null){let t=Ta(e.theme,`shape.borderRadius`,4,`borderRadius`);return la(e,e.borderRadius,e=>({borderRadius:Da(t,e)}))}return null};Za.propTypes={},Za.filterProps=[`borderRadius`],Ia(za,Ba,Va,Ha,Ua,Wa,Ga,Ka,qa,Ja,Za,Ya,Xa);const Qa=e=>{if(e.gap!==void 0&&e.gap!==null){let t=Ta(e.theme,`spacing`,8,`gap`);return la(e,e.gap,e=>({gap:Da(t,e)}))}return null};Qa.propTypes={},Qa.filterProps=[`gap`];const $a=e=>{if(e.columnGap!==void 0&&e.columnGap!==null){let t=Ta(e.theme,`spacing`,8,`columnGap`);return la(e,e.columnGap,e=>({columnGap:Da(t,e)}))}return null};$a.propTypes={},$a.filterProps=[`columnGap`];const eo=e=>{if(e.rowGap!==void 0&&e.rowGap!==null){let t=Ta(e.theme,`spacing`,8,`rowGap`);return la(e,e.rowGap,e=>({rowGap:Da(t,e)}))}return null};eo.propTypes={},eo.filterProps=[`rowGap`],Ia(Qa,$a,eo,ga({prop:`gridColumn`}),ga({prop:`gridRow`}),ga({prop:`gridAutoFlow`}),ga({prop:`gridAutoColumns`}),ga({prop:`gridAutoRows`}),ga({prop:`gridTemplateColumns`}),ga({prop:`gridTemplateRows`}),ga({prop:`gridTemplateAreas`}),ga({prop:`gridArea`}));function to(e,t){return t===`grey`?t:e}Ia(ga({prop:`color`,themeKey:`palette`,transform:to}),ga({prop:`bgcolor`,cssProperty:`backgroundColor`,themeKey:`palette`,transform:to}),ga({prop:`backgroundColor`,themeKey:`palette`,transform:to}));function no(e){return e<=1&&e!==0?`${e*100}%`:e}const ro=ga({prop:`width`,transform:no}),io=e=>e.maxWidth!==void 0&&e.maxWidth!==null?la(e,e.maxWidth,t=>{let n=e.theme?.breakpoints?.values?.[t]||oa[t];return n?e.theme?.breakpoints?.unit===`px`?{maxWidth:n}:{maxWidth:`${n}${e.theme.breakpoints.unit}`}:{maxWidth:no(t)}}):null;io.filterProps=[`maxWidth`];const ao=ga({prop:`minWidth`,transform:no}),oo=ga({prop:`height`,transform:no}),so=ga({prop:`maxHeight`,transform:no}),co=ga({prop:`minHeight`,transform:no});ga({prop:`size`,cssProperty:`width`,transform:no}),ga({prop:`size`,cssProperty:`height`,transform:no}),Ia(ro,io,ao,oo,so,co,ga({prop:`boxSizing`}));var lo={border:{themeKey:`borders`,transform:La},borderTop:{themeKey:`borders`,transform:La},borderRight:{themeKey:`borders`,transform:La},borderBottom:{themeKey:`borders`,transform:La},borderLeft:{themeKey:`borders`,transform:La},borderColor:{themeKey:`palette`},borderTopColor:{themeKey:`palette`},borderRightColor:{themeKey:`palette`},borderBottomColor:{themeKey:`palette`},borderLeftColor:{themeKey:`palette`},outline:{themeKey:`borders`,transform:La},outlineColor:{themeKey:`palette`},borderRadius:{themeKey:`shape.borderRadius`,style:Za},color:{themeKey:`palette`,transform:to},bgcolor:{themeKey:`palette`,cssProperty:`backgroundColor`,transform:to},backgroundColor:{themeKey:`palette`,transform:to},p:{style:Ma},pt:{style:Ma},pr:{style:Ma},pb:{style:Ma},pl:{style:Ma},px:{style:Ma},py:{style:Ma},padding:{style:Ma},paddingTop:{style:Ma},paddingRight:{style:Ma},paddingBottom:{style:Ma},paddingLeft:{style:Ma},paddingX:{style:Ma},paddingY:{style:Ma},paddingInline:{style:Ma},paddingInlineStart:{style:Ma},paddingInlineEnd:{style:Ma},paddingBlock:{style:Ma},paddingBlockStart:{style:Ma},paddingBlockEnd:{style:Ma},m:{style:ja},mt:{style:ja},mr:{style:ja},mb:{style:ja},ml:{style:ja},mx:{style:ja},my:{style:ja},margin:{style:ja},marginTop:{style:ja},marginRight:{style:ja},marginBottom:{style:ja},marginLeft:{style:ja},marginX:{style:ja},marginY:{style:ja},marginInline:{style:ja},marginInlineStart:{style:ja},marginInlineEnd:{style:ja},marginBlock:{style:ja},marginBlockStart:{style:ja},marginBlockEnd:{style:ja},displayPrint:{cssProperty:!1,transform:e=>({"@media print":{display:e}})},display:{},overflow:{},textOverflow:{},visibility:{},whiteSpace:{},flexBasis:{},flexDirection:{},flexWrap:{},justifyContent:{},alignItems:{},alignContent:{},order:{},flex:{},flexGrow:{},flexShrink:{},alignSelf:{},justifyItems:{},justifySelf:{},gap:{style:Qa},rowGap:{style:eo},columnGap:{style:$a},gridColumn:{},gridRow:{},gridAutoFlow:{},gridAutoColumns:{},gridAutoRows:{},gridTemplateColumns:{},gridTemplateRows:{},gridTemplateAreas:{},gridArea:{},position:{},zIndex:{themeKey:`zIndex`},top:{},right:{},bottom:{},left:{},boxShadow:{themeKey:`shadows`},width:{transform:no},maxWidth:{style:io},minWidth:{transform:no},height:{transform:no},maxHeight:{transform:no},minHeight:{transform:no},boxSizing:{},font:{themeKey:`font`},fontFamily:{themeKey:`typography`},fontSize:{themeKey:`typography`},fontStyle:{themeKey:`typography`},fontWeight:{themeKey:`typography`},letterSpacing:{},textTransform:{},lineHeight:{},textAlign:{},typography:{cssProperty:!1,themeKey:`typography`}};function uo(...e){let t=e.reduce((e,t)=>e.concat(Object.keys(t)),[]),n=new Set(t);return e.every(e=>n.size===Object.keys(e).length)}function fo(e,t){return typeof e==`function`?e(t):e}function po(){function e(e,t,n,r){let i={[e]:t,theme:n},a=r[e];if(!a)return{[e]:t};let{cssProperty:o=e,themeKey:s,transform:c,style:l}=a;if(t==null)return null;if(s===`typography`&&t===`inherit`)return{[e]:t};let u=pa(n,s)||{};return l?l(i):la(i,t,t=>{let n=ma(u,c,t);return t===n&&typeof t==`string`&&(n=ma(u,c,`${e}${t===`default`?``:fa(t)}`,t)),o===!1?n:{[o]:n}})}function t(n){let{sx:r,theme:i={},nested:a}=n||{};if(!r)return null;let o=i.unstable_sxConfig??lo;function s(n){let r=n;if(typeof n==`function`)r=n(i);else if(typeof n!=`object`)return n;if(!r)return null;let s=ua(i.breakpoints),c=Object.keys(s),l=s;return Object.keys(r).forEach(n=>{let a=fo(r[n],i);if(a!=null)if(typeof a==`object`)if(o[n])l=aa(l,e(n,a,i,o));else{let e=la({theme:i},a,e=>({[n]:e}));uo(e,a)?l[n]=t({sx:a,theme:i,nested:!0}):l=aa(l,e)}else l=aa(l,e(n,a,i,o))}),!a&&i.modularCssLayers?{"@layer sx":$i(i,da(c,l))}:$i(i,da(c,l))}return Array.isArray(r)?r.map(s):s(r)}return t}var mo=po();mo.filterProps=[`sx`];var z=mo;function ho(e,t){let n=this;if(n.vars){if(!n.colorSchemes?.[e]||typeof n.getColorSchemeSelector!=`function`)return{};let r=n.getColorSchemeSelector(e);return r===`&`?t:((r.includes(`data-`)||r.includes(`.`))&&(r=`*:where(${r.replace(/\s*&$/,``)}) &`),{[r]:t})}return n.palette.mode===e?t:{}}function go(e={},...t){let{breakpoints:n={},palette:r={},spacing:i,shape:a={},...o}=e,s=Qi(n),c=Pa(i),l=Xi({breakpoints:s,direction:`ltr`,components:{},palette:{mode:`light`,...r},spacing:c,shape:{...ra,...a}},o);return l=na(l),l.applyStyles=ho,l=t.reduce((e,t)=>Xi(e,t),l),l.unstable_sxConfig={...lo,...o?.unstable_sxConfig},l.unstable_sx=function(e){return z({sx:e,theme:this})},l}var _o=go;function vo(e){return Object.keys(e).length===0}function yo(e=null){let t=x.useContext(vi);return!t||vo(t)?e:t}var bo=yo;const xo=_o();function So(e=xo){return bo(e)}var Co=So;function wo(e){let t=Gi(e);return e!==t&&t.styles?(t.styles.match(/^@layer\s+[^{]*$/)||(t.styles=`@layer global{${t.styles}}`),t):e}function To({styles:e,themeId:t,defaultTheme:n={}}){let r=Co(n),i=t&&r[t]||r,a=typeof e==`function`?e(i):e;return i.modularCssLayers&&(a=Array.isArray(a)?a.map(e=>wo(typeof e==`function`?e(i):e)):wo(a)),(0,R.jsx)(Vi,{styles:a})}var Eo=To,Do=e=>{let t={systemProps:{},otherProps:{}},n=e?.theme?.unstable_sxConfig??lo;return Object.keys(e).forEach(r=>{n[r]?t.systemProps[r]=e[r]:t.otherProps[r]=e[r]}),t};function Oo(e){let{sx:t,...n}=e,{systemProps:r,otherProps:i}=Do(n),a;return a=Array.isArray(t)?[r,...t]:typeof t==`function`?(...e)=>{let n=t(...e);return Ji(n)?{...r,...n}:r}:{...r,...t},{...i,sx:a}}var ko=e=>e,Ao=(()=>{let e=ko;return{configure(t){e=t},generate(t){return e(t)},reset(){e=ko}}})(),jo=g();function Mo(e){var t,n,r=``;if(typeof e==`string`||typeof e==`number`)r+=e;else if(typeof e==`object`)if(Array.isArray(e)){var i=e.length;for(t=0;t<i;t++)e[t]&&(n=Mo(e[t]))&&(r&&(r+=` `),r+=n)}else for(n in e)e[n]&&(r&&(r+=` `),r+=n);return r}function No(){for(var e,t,n=0,r=``,i=arguments.length;n<i;n++)(e=arguments[n])&&(t=Mo(e))&&(r&&(r+=` `),r+=t);return r}var B=No;function Po(e={}){let{themeId:t,defaultTheme:n,defaultClassName:r=`MuiBox-root`,generateClassName:i}=e,a=Hi(`div`,{shouldForwardProp:e=>e!==`theme`&&e!==`sx`&&e!==`as`})(z);return x.forwardRef(function(e,o){let s=Co(n),{className:c,component:l=`div`,...u}=Oo(e);return(0,R.jsx)(a,{as:l,ref:o,className:B(c,i?i(r):r),theme:t&&s[t]||s,...u})})}const Fo={active:`active`,checked:`checked`,completed:`completed`,disabled:`disabled`,error:`error`,expanded:`expanded`,focused:`focused`,focusVisible:`focusVisible`,open:`open`,readOnly:`readOnly`,required:`required`,selected:`selected`};function Io(e,t,n=`Mui`){let r=Fo[t];return r?`${n}-${r}`:`${Ao.generate(e)}-${t}`}function Lo(e,t,n=`Mui`){let r={};return t.forEach(t=>{r[t]=Io(e,t,n)}),r}function Ro(e){let{variants:t,...n}=e,r={variants:t,style:Gi(n),isProcessed:!0};return r.style===n||t&&t.forEach(e=>{typeof e.style!=`function`&&(e.style=Gi(e.style))}),r}const zo=_o();function Bo(e){return e!==`ownerState`&&e!==`theme`&&e!==`sx`&&e!==`as`}function Vo(e,t){return t&&e&&typeof e==`object`&&e.styles&&!e.styles.startsWith(`@layer`)&&(e.styles=`@layer ${t}{${String(e.styles)}}`),e}function Ho(e){return e?(t,n)=>n[e]:null}function Uo(e,t,n){e.theme=qo(e.theme)?n:e.theme[t]||e.theme}function Wo(e,t,n){let r=typeof t==`function`?t(e):t;if(Array.isArray(r))return r.flatMap(t=>Wo(e,t,n));if(Array.isArray(r?.variants)){let t;if(r.isProcessed)t=n?Vo(r.style,n):r.style;else{let{variants:e,...i}=r;t=n?Vo(Gi(i),n):i}return Go(e,r.variants,[t],n)}return r?.isProcessed?n?Vo(Gi(r.style),n):r.style:n?Vo(Gi(r),n):r}function Go(e,t,n=[],r=void 0){let i;variantLoop:for(let a=0;a<t.length;a+=1){let o=t[a];if(typeof o.props==`function`){if(i??={...e,...e.ownerState,ownerState:e.ownerState},!o.props(i))continue}else for(let t in o.props)if(e[t]!==o.props[t]&&e.ownerState?.[t]!==o.props[t])continue variantLoop;typeof o.style==`function`?(i??={...e,...e.ownerState,ownerState:e.ownerState},n.push(r?Vo(Gi(o.style(i)),r):o.style(i))):n.push(r?Vo(Gi(o.style),r):o.style)}return n}function Ko(e={}){let{themeId:t,defaultTheme:n=zo,rootShouldForwardProp:r=Bo,slotShouldForwardProp:i=Bo}=e;function a(e){Uo(e,t,n)}return(e,t={})=>{Ui(e,e=>e.filter(e=>e!==z));let{name:n,slot:o,skipVariantsResolver:s,skipSx:c,overridesResolver:l=Ho(Yo(o)),...u}=t,d=n&&n.startsWith(`Mui`)||o?`components`:`custom`,f=s===void 0?o&&o!==`Root`&&o!==`root`||!1:s,p=c||!1,m=Bo;o===`Root`||o===`root`?m=r:o?m=i:Jo(e)&&(m=void 0);let h=Hi(e,{shouldForwardProp:m,label:void 0,...u}),g=e=>{if(e.__emotion_real===e)return e;if(typeof e==`function`)return function(t){return Wo(t,e,t.theme.modularCssLayers?d:void 0)};if(Ji(e)){let t=Ro(e);return function(e){return t.variants?Wo(e,t,e.theme.modularCssLayers?d:void 0):e.theme.modularCssLayers?Vo(t.style,d):t.style}}return e},_=(...t)=>{let r=[],i=t.map(g),o=[];if(r.push(a),n&&l&&o.push(function(e){let t=e.theme.components?.[n]?.styleOverrides;if(!t)return null;let r={};for(let n in t)r[n]=Wo(e,t[n],e.theme.modularCssLayers?`theme`:void 0);return l(e,r)}),n&&!f&&o.push(function(e){let t=e.theme?.components?.[n]?.variants;return t?Go(e,t,[],e.theme.modularCssLayers?`theme`:void 0):null}),p||o.push(z),Array.isArray(i[0])){let e=i.shift(),t=Array(r.length).fill(``),n=Array(o.length).fill(``),a;a=[...t,...e,...n],a.raw=[...t,...e.raw,...n],r.unshift(a)}let s=h(...r,...i,...o);return e.muiName&&(s.muiName=e.muiName),s};return h.withConfig&&(_.withConfig=h.withConfig),_}}function qo(e){for(let t in e)return!1;return!0}function Jo(e){return typeof e==`string`&&e.charCodeAt(0)>96}function Yo(e){return e&&e.charAt(0).toLowerCase()+e.slice(1)}function Xo(e,t,n=!1){let r={...t};for(let i in e)if(Object.prototype.hasOwnProperty.call(e,i)){let a=i;if(a===`components`||a===`slots`)r[a]={...e[a],...r[a]};else if(a===`componentsProps`||a===`slotProps`){let i=e[a],o=t[a];if(!o)r[a]=i||{};else if(!i)r[a]=o;else for(let e in r[a]={...o},i)if(Object.prototype.hasOwnProperty.call(i,e)){let t=e;r[a][t]=Xo(i[t],o[t],n)}}else a===`className`&&n&&t.className?r.className=B(e?.className,t?.className):a===`style`&&n&&t.style?r.style={...e?.style,...t?.style}:r[a]===void 0&&(r[a]=e[a])}return r}var Zo=typeof window<`u`?x.useLayoutEffect:x.useEffect;function Qo(e,t=-(2**53-1),n=2**53-1){return Math.max(t,Math.min(e,n))}var $o=Qo;function es(e,t=0,n=1){return $o(e,t,n)}function ts(e){e=e.slice(1);let t=RegExp(`.{1,${e.length>=6?2:1}}`,`g`),n=e.match(t);return n&&n[0].length===1&&(n=n.map(e=>e+e)),n?`rgb${n.length===4?`a`:``}(${n.map((e,t)=>t<3?parseInt(e,16):Math.round(parseInt(e,16)/255*1e3)/1e3).join(`, `)})`:``}function ns(e){if(e.type)return e;if(e.charAt(0)===`#`)return ns(ts(e));let t=e.indexOf(`(`),n=e.substring(0,t);if(![`rgb`,`rgba`,`hsl`,`hsla`,`color`].includes(n))throw Error(En(9,e));let r=e.substring(t+1,e.length-1),i;if(n===`color`){if(r=r.split(` `),i=r.shift(),r.length===4&&r[3].charAt(0)===`/`&&(r[3]=r[3].slice(1)),![`srgb`,`display-p3`,`a98-rgb`,`prophoto-rgb`,`rec-2020`].includes(i))throw Error(En(10,i))}else r=r.split(`,`);return r=r.map(e=>parseFloat(e)),{type:n,values:r,colorSpace:i}}const rs=e=>{let t=ns(e);return t.values.slice(0,3).map((e,n)=>t.type.includes(`hsl`)&&n!==0?`${e}%`:e).join(` `)},is=(e,t)=>{try{return rs(e)}catch{return e}};function as(e){let{type:t,colorSpace:n}=e,{values:r}=e;return t.includes(`rgb`)?r=r.map((e,t)=>t<3?parseInt(e,10):e):t.includes(`hsl`)&&(r[1]=`${r[1]}%`,r[2]=`${r[2]}%`),r=t.includes(`color`)?`${n} ${r.join(` `)}`:`${r.join(`, `)}`,`${t}(${r})`}function os(e){e=ns(e);let{values:t}=e,n=t[0],r=t[1]/100,i=t[2]/100,a=r*Math.min(i,1-i),o=(e,t=(e+n/30)%12)=>i-a*Math.max(Math.min(t-3,9-t,1),-1),s=`rgb`,c=[Math.round(o(0)*255),Math.round(o(8)*255),Math.round(o(4)*255)];return e.type===`hsla`&&(s+=`a`,c.push(t[3])),as({type:s,values:c})}function ss(e){e=ns(e);let t=e.type===`hsl`||e.type===`hsla`?ns(os(e)).values:e.values;return t=t.map(t=>(e.type!==`color`&&(t/=255),t<=.03928?t/12.92:((t+.055)/1.055)**2.4)),Number((.2126*t[0]+.7152*t[1]+.0722*t[2]).toFixed(3))}function cs(e,t){let n=ss(e),r=ss(t);return(Math.max(n,r)+.05)/(Math.min(n,r)+.05)}function ls(e,t){return e=ns(e),t=es(t),(e.type===`rgb`||e.type===`hsl`)&&(e.type+=`a`),e.type===`color`?e.values[3]=`/${t}`:e.values[3]=t,as(e)}function us(e,t,n){try{return ls(e,t)}catch{return e}}function ds(e,t){if(e=ns(e),t=es(t),e.type.includes(`hsl`))e.values[2]*=1-t;else if(e.type.includes(`rgb`)||e.type.includes(`color`))for(let n=0;n<3;n+=1)e.values[n]*=1-t;return as(e)}function fs(e,t,n){try{return ds(e,t)}catch{return e}}function ps(e,t){if(e=ns(e),t=es(t),e.type.includes(`hsl`))e.values[2]+=(100-e.values[2])*t;else if(e.type.includes(`rgb`))for(let n=0;n<3;n+=1)e.values[n]+=(255-e.values[n])*t;else if(e.type.includes(`color`))for(let n=0;n<3;n+=1)e.values[n]+=(1-e.values[n])*t;return as(e)}function ms(e,t,n){try{return ps(e,t)}catch{return e}}function hs(e,t=.15){return ss(e)>.5?ds(e,t):ps(e,t)}function gs(e,t,n){try{return hs(e,t)}catch{return e}}var _s=x.createContext(null);function vs(){return x.useContext(_s)}var ys=typeof Symbol==`function`&&Symbol.for?Symbol.for(`mui.nested`):`__THEME_NESTED__`;function bs(e,t){return typeof t==`function`?t(e):{...e,...t}}function xs(e){let{children:t,theme:n}=e,r=vs(),i=x.useMemo(()=>{let e=r===null?{...n}:bs(r,n);return e!=null&&(e[ys]=r!==null),e},[n,r]);return(0,R.jsx)(_s.Provider,{value:i,children:t})}var Ss=xs,Cs=x.createContext();function ws({value:e,...t}){return(0,R.jsx)(Cs.Provider,{value:e??!0,...t})}const Ts=()=>x.useContext(Cs)??!1;var Es=ws,Ds=x.createContext(void 0);function Os({value:e,children:t}){return(0,R.jsx)(Ds.Provider,{value:e,children:t})}function ks(e){let{theme:t,name:n,props:r}=e;if(!t||!t.components||!t.components[n])return r;let i=t.components[n];return i.defaultProps?Xo(i.defaultProps,r,t.components.mergeClassNameAndStyle):!i.styleOverrides&&!i.variants?Xo(i,r,t.components.mergeClassNameAndStyle):r}function As({props:e,name:t}){return ks({props:e,name:t,theme:{components:x.useContext(Ds)}})}var js=Os,Ms=0;function Ns(e){let[t,n]=x.useState(e),r=e||t;return x.useEffect(()=>{t??(Ms+=1,n(`mui-${Ms}`))},[t]),r}var Ps={...x}.useId;function Fs(e){if(Ps!==void 0){let t=Ps();return e??t}return Ns(e)}function Is(e){let t=bo(),n=Fs()||``,{modularCssLayers:r}=e,i=`mui.global, mui.components, mui.theme, mui.custom, mui.sx`;return i=!r||t!==null?``:typeof r==`string`?r.replace(/mui(?!\.)/g,i):`@layer ${i};`,Zo(()=>{let e=document.querySelector(`head`);if(!e)return;let t=e.firstChild;if(i){if(t&&t.hasAttribute?.(`data-mui-layer-order`)&&t.getAttribute(`data-mui-layer-order`)===n)return;let r=document.createElement(`style`);r.setAttribute(`data-mui-layer-order`,n),r.textContent=i,e.prepend(r)}else e.querySelector(`style[data-mui-layer-order="${n}"]`)?.remove()},[i,n]),i?(0,R.jsx)(Eo,{styles:i}):null}var Ls={};function Rs(e,t,n,r=!1){return x.useMemo(()=>{let i=e&&t[e]||t;if(typeof n==`function`){let a=n(i),o=e?{...t,[e]:a}:a;return r?()=>o:o}return e?{...t,[e]:n}:{...t,...n}},[e,t,n,r])}function zs(e){let{children:t,theme:n,themeId:r}=e,i=bo(Ls),a=vs()||Ls,o=Rs(r,i,n),s=Rs(r,a,n,!0),c=(r?o[r]:o).direction===`rtl`,l=Is(o);return(0,R.jsx)(Ss,{theme:s,children:(0,R.jsx)(vi.Provider,{value:o,children:(0,R.jsx)(Es,{value:c,children:(0,R.jsxs)(js,{value:r?o[r].components:o.components,children:[l,t]})})})})}var Bs=zs,Vs={theme:void 0};function Hs(e){let t,n;return function(r){let i=t;return(i===void 0||r.theme!==n)&&(Vs.theme=r.theme,i=Ro(e(Vs)),t=i,n=r.theme),i}}const Us=`mode`,Ws=`color-scheme`;function Gs(e){let{defaultMode:t=`system`,defaultLightColorScheme:n=`light`,defaultDarkColorScheme:r=`dark`,modeStorageKey:i=Us,colorSchemeStorageKey:a=Ws,attribute:o=`data-color-scheme`,colorSchemeNode:s=`document.documentElement`,nonce:c}=e||{},l=``,u=o;if(o===`class`&&(u=`.%s`),o===`data`&&(u=`[data-%s]`),u.startsWith(`.`)){let e=u.substring(1);l+=`${s}.classList.remove('${e}'.replace('%s', light), '${e}'.replace('%s', dark));
      ${s}.classList.add('${e}'.replace('%s', colorScheme));`}let d=u.match(/\[([^[\]]+)\]/);if(d){let[e,t]=d[1].split(`=`);t||(l+=`${s}.removeAttribute('${e}'.replace('%s', light));
      ${s}.removeAttribute('${e}'.replace('%s', dark));`),l+=`
      ${s}.setAttribute('${e}'.replace('%s', colorScheme), ${t?`${t}.replace('%s', colorScheme)`:`""`});`}else l+=`${s}.setAttribute('${u}', colorScheme);`;return(0,R.jsx)(`script`,{suppressHydrationWarning:!0,nonce:typeof window>`u`?c:``,dangerouslySetInnerHTML:{__html:`(function() {
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
} catch(e){}})();`}},`mui-color-scheme-init`)}function Ks(){}var qs=({key:e,storageWindow:t})=>(!t&&typeof window<`u`&&(t=window),{get(n){if(typeof window>`u`)return;if(!t)return n;let r;try{r=t.localStorage.getItem(e)}catch{}return r||n},set:n=>{if(t)try{t.localStorage.setItem(e,n)}catch{}},subscribe:n=>{if(!t)return Ks;let r=t=>{let r=t.newValue;t.key===e&&n(r)};return t.addEventListener(`storage`,r),()=>{t.removeEventListener(`storage`,r)}}});function Js(){}function Ys(e){if(typeof window<`u`&&typeof window.matchMedia==`function`&&e===`system`)return window.matchMedia(`(prefers-color-scheme: dark)`).matches?`dark`:`light`}function Xs(e,t){if(e.mode===`light`||e.mode===`system`&&e.systemMode===`light`)return t(`light`);if(e.mode===`dark`||e.mode===`system`&&e.systemMode===`dark`)return t(`dark`)}function Zs(e){return Xs(e,t=>{if(t===`light`)return e.lightColorScheme;if(t===`dark`)return e.darkColorScheme})}function Qs(e){let{defaultMode:t=`light`,defaultLightColorScheme:n,defaultDarkColorScheme:r,supportedColorSchemes:i=[],modeStorageKey:a=Us,colorSchemeStorageKey:o=Ws,storageWindow:s=typeof window>`u`?void 0:window,storageManager:c=qs,noSsr:l=!1}=e,u=i.join(`,`),d=i.length>1,f=x.useMemo(()=>c?.({key:a,storageWindow:s}),[c,a,s]),p=x.useMemo(()=>c?.({key:`${o}-light`,storageWindow:s}),[c,o,s]),m=x.useMemo(()=>c?.({key:`${o}-dark`,storageWindow:s}),[c,o,s]),[h,g]=x.useState(()=>{let e=f?.get(t)||t,i=p?.get(n)||n,a=m?.get(r)||r;return{mode:e,systemMode:Ys(e),lightColorScheme:i,darkColorScheme:a}}),[_,v]=x.useState(l||!d);x.useEffect(()=>{v(!0)},[]);let y=Zs(h),b=x.useCallback(e=>{g(n=>{if(e===n.mode)return n;let r=e??t;return f?.set(r),{...n,mode:r,systemMode:Ys(r)}})},[f,t]),S=x.useCallback(e=>{e?typeof e==`string`?e&&!u.includes(e)?console.error(`\`${e}\` does not exist in \`theme.colorSchemes\`.`):g(t=>{let n={...t};return Xs(t,t=>{t===`light`&&(p?.set(e),n.lightColorScheme=e),t===`dark`&&(m?.set(e),n.darkColorScheme=e)}),n}):g(t=>{let i={...t},a=e.light===null?n:e.light,o=e.dark===null?r:e.dark;return a&&(u.includes(a)?(i.lightColorScheme=a,p?.set(a)):console.error(`\`${a}\` does not exist in \`theme.colorSchemes\`.`)),o&&(u.includes(o)?(i.darkColorScheme=o,m?.set(o)):console.error(`\`${o}\` does not exist in \`theme.colorSchemes\`.`)),i}):g(e=>(p?.set(n),m?.set(r),{...e,lightColorScheme:n,darkColorScheme:r}))},[u,p,m,n,r]),C=x.useCallback(e=>{h.mode===`system`&&g(t=>{let n=e?.matches?`dark`:`light`;return t.systemMode===n?t:{...t,systemMode:n}})},[h.mode]),w=x.useRef(C);return w.current=C,x.useEffect(()=>{if(typeof window.matchMedia!=`function`||!d)return;let e=(...e)=>w.current(...e),t=window.matchMedia(`(prefers-color-scheme: dark)`);return t.addListener(e),e(t),()=>{t.removeListener(e)}},[d]),x.useEffect(()=>{if(d){let e=f?.subscribe(e=>{(!e||[`light`,`dark`,`system`].includes(e))&&b(e||t)})||Js,n=p?.subscribe(e=>{(!e||u.match(e))&&S({light:e})})||Js,r=m?.subscribe(e=>{(!e||u.match(e))&&S({dark:e})})||Js;return()=>{e(),n(),r()}}},[S,b,u,t,s,d,f,p,m]),{...h,mode:_?h.mode:void 0,systemMode:_?h.systemMode:void 0,colorScheme:_?y:void 0,setMode:b,setColorScheme:S}}function $s(e){let{themeId:t,theme:n={},modeStorageKey:r=Us,colorSchemeStorageKey:i=Ws,disableTransitionOnChange:a=!1,defaultColorScheme:o,resolveTheme:s}=e,c={allColorSchemes:[],colorScheme:void 0,darkColorScheme:void 0,lightColorScheme:void 0,mode:void 0,setColorScheme:()=>{},setMode:()=>{},systemMode:void 0},l=x.createContext(void 0),u=()=>x.useContext(l)||c,d={},f={};function p(e){let{children:c,theme:u,modeStorageKey:p=r,colorSchemeStorageKey:m=i,disableTransitionOnChange:h=a,storageManager:g,storageWindow:_=typeof window>`u`?void 0:window,documentNode:v=typeof document>`u`?void 0:document,colorSchemeNode:y=typeof document>`u`?void 0:document.documentElement,disableNestedContext:b=!1,disableStyleSheetGeneration:S=!1,defaultMode:C=`system`,forceThemeRerender:w=!1,noSsr:T}=e,E=x.useRef(!1),D=vs(),O=x.useContext(l),k=!!O&&!b,A=x.useMemo(()=>u||(typeof n==`function`?n():n),[u]),j=A[t],M=j||A,{colorSchemes:N=d,components:ee=f,cssVarPrefix:P}=M,F=Object.keys(N).filter(e=>!!N[e]).join(`,`),I=x.useMemo(()=>F.split(`,`),[F]),te=typeof o==`string`?o:o.light,ne=typeof o==`string`?o:o.dark,{mode:re,setMode:ie,systemMode:ae,lightColorScheme:L,darkColorScheme:oe,colorScheme:se,setColorScheme:ce}=Qs({supportedColorSchemes:I,defaultLightColorScheme:te,defaultDarkColorScheme:ne,modeStorageKey:p,colorSchemeStorageKey:m,defaultMode:N[te]&&N[ne]?C:N[M.defaultColorScheme]?.palette?.mode||M.palette?.mode,storageManager:g,storageWindow:_,noSsr:T}),le=re,ue=se;k&&(le=O.mode,ue=O.colorScheme);let de=ue||M.defaultColorScheme;M.vars&&!w&&(de=M.defaultColorScheme);let fe=x.useMemo(()=>{let e=M.generateThemeVars?.()||M.vars,t={...M,components:ee,colorSchemes:N,cssVarPrefix:P,vars:e};if(typeof t.generateSpacing==`function`&&(t.spacing=t.generateSpacing()),de){let e=N[de];e&&typeof e==`object`&&Object.keys(e).forEach(n=>{e[n]&&typeof e[n]==`object`?t[n]={...t[n],...e[n]}:t[n]=e[n]})}return s?s(t):t},[M,de,ee,N,P]),pe=M.colorSchemeSelector;Zo(()=>{if(ue&&y&&pe&&pe!==`media`){let e=pe,t=pe;if(e===`class`&&(t=`.%s`),e===`data`&&(t=`[data-%s]`),e?.startsWith(`data-`)&&!e.includes(`%s`)&&(t=`[${e}="%s"]`),t.startsWith(`.`))y.classList.remove(...I.map(e=>t.substring(1).replace(`%s`,e))),y.classList.add(t.substring(1).replace(`%s`,ue));else{let e=t.replace(`%s`,ue).match(/\[([^\]]+)\]/);if(e){let[t,n]=e[1].split(`=`);n||I.forEach(e=>{y.removeAttribute(t.replace(ue,e))}),y.setAttribute(t,n?n.replace(/"|'/g,``):``)}else y.setAttribute(t,ue)}}},[ue,pe,y,I]),x.useEffect(()=>{let e;if(h&&E.current&&v){let t=v.createElement(`style`);t.appendChild(v.createTextNode(`*{-webkit-transition:none!important;-moz-transition:none!important;-o-transition:none!important;-ms-transition:none!important;transition:none!important}`)),v.head.appendChild(t),window.getComputedStyle(v.body),e=setTimeout(()=>{v.head.removeChild(t)},1)}return()=>{clearTimeout(e)}},[ue,h,v]),x.useEffect(()=>(E.current=!0,()=>{E.current=!1}),[]);let me=x.useMemo(()=>({allColorSchemes:I,colorScheme:ue,darkColorScheme:oe,lightColorScheme:L,mode:le,setColorScheme:ce,setMode:ie,systemMode:ae}),[I,ue,oe,L,le,ce,ie,ae,fe.colorSchemeSelector]),he=!0;(S||M.cssVariables===!1||k&&D?.cssVarPrefix===P)&&(he=!1);let ge=(0,R.jsxs)(x.Fragment,{children:[(0,R.jsx)(Bs,{themeId:j?t:void 0,theme:fe,children:c}),he&&(0,R.jsx)(Vi,{styles:fe.generateStyleSheets?.()||[]})]});return k?ge:(0,R.jsx)(l.Provider,{value:me,children:ge})}let m=typeof o==`string`?o:o.light,h=typeof o==`string`?o:o.dark;return{CssVarsProvider:p,useColorScheme:u,getInitColorSchemeScript:e=>Gs({colorSchemeStorageKey:i,defaultLightColorScheme:m,defaultDarkColorScheme:h,modeStorageKey:r,...e})}}function ec(e=``){function t(...n){if(!n.length)return``;let r=n[0];return typeof r==`string`&&!r.match(/(#|\(|\)|(-?(\d*\.)?\d+)(px|em|%|ex|ch|rem|vw|vh|vmin|vmax|cm|mm|in|pt|pc))|^(-?(\d*\.)?\d+)$|(\d+ \d+ \d+)/)?`, var(--${e?`${e}-`:``}${r}${t(...n.slice(1))})`:`, ${r}`}return(n,...r)=>`var(--${e?`${e}-`:``}${n}${t(...r)})`}const tc=(e,t,n,r=[])=>{let i=e;t.forEach((e,a)=>{a===t.length-1?Array.isArray(i)?i[Number(e)]=n:i&&typeof i==`object`&&(i[e]=n):i&&typeof i==`object`&&(i[e]||(i[e]=r.includes(e)?[]:{}),i=i[e])})},nc=(e,t,n)=>{function r(e,i=[],a=[]){Object.entries(e).forEach(([e,o])=>{(!n||n&&!n([...i,e]))&&o!=null&&(typeof o==`object`&&Object.keys(o).length>0?r(o,[...i,e],Array.isArray(o)?[...a,e]:a):t([...i,e],o,a))})}r(e)};var rc=(e,t)=>typeof t==`number`?[`lineHeight`,`fontWeight`,`opacity`,`zIndex`].some(t=>e.includes(t))||e[e.length-1].toLowerCase().includes(`opacity`)?t:`${t}px`:t;function ic(e,t){let{prefix:n,shouldSkipGeneratingVar:r}=t||{},i={},a={},o={};return nc(e,(e,t,s)=>{if((typeof t==`string`||typeof t==`number`)&&(!r||!r(e,t))){let r=`--${n?`${n}-`:``}${e.join(`-`)}`,c=rc(e,t);Object.assign(i,{[r]:c}),tc(a,e,`var(${r})`,s),tc(o,e,`var(${r}, ${c})`,s)}},e=>e[0]===`vars`),{css:i,vars:a,varsWithDefaults:o}}function ac(e,t={}){let{getSelector:n=_,disableCssColorScheme:r,colorSchemeSelector:i,enableContrastVars:a}=t,{colorSchemes:o={},components:s,defaultColorScheme:c=`light`,...l}=e,{vars:u,css:d,varsWithDefaults:f}=ic(l,t),p=f,m={},{[c]:h,...g}=o;if(Object.entries(g||{}).forEach(([e,n])=>{let{vars:r,css:i,varsWithDefaults:a}=ic(n,t);p=Xi(p,a),m[e]={css:i,vars:r}}),h){let{css:e,vars:n,varsWithDefaults:r}=ic(h,t);p=Xi(p,r),m[c]={css:e,vars:n}}function _(t,n){let r=i;if(i===`class`&&(r=`.%s`),i===`data`&&(r=`[data-%s]`),i?.startsWith(`data-`)&&!i.includes(`%s`)&&(r=`[${i}="%s"]`),t){if(r===`media`)return e.defaultColorScheme===t?`:root`:{[`@media (prefers-color-scheme: ${o[t]?.palette?.mode||t})`]:{":root":n}};if(r)return e.defaultColorScheme===t?`:root, ${r.replace(`%s`,String(t))}`:r.replace(`%s`,String(t))}return`:root`}return{vars:p,generateThemeVars:()=>{let e={...u};return Object.entries(m).forEach(([,{vars:t}])=>{e=Xi(e,t)}),e},generateStyleSheets:()=>{let t=[],i=e.defaultColorScheme||`light`;function s(e,n){Object.keys(n).length&&t.push(typeof e==`string`?{[e]:{...n}}:e)}s(n(void 0,{...d}),d);let{[i]:c,...l}=m;if(c){let{css:e}=c,t=o[i]?.palette?.mode,a=!r&&t?{colorScheme:t,...e}:{...e};s(n(i,{...a}),a)}return Object.entries(l).forEach(([e,{css:t}])=>{let i=o[e]?.palette?.mode,a=!r&&i?{colorScheme:i,...t}:{...t};s(n(e,{...a}),a)}),a&&t.push({":root":{"--__l-threshold":`0.7`,"--__l":`clamp(0, (l / var(--__l-threshold) - 1) * -infinity, 1)`,"--__a":`clamp(0.87, (l / var(--__l-threshold) - 1) * -infinity, 1)`}}),t}}}var oc=ac;function sc(e){return function(t){return e===`media`?`@media (prefers-color-scheme: ${t})`:e?e.startsWith(`data-`)&&!e.includes(`%s`)?`[${e}="${t}"] &`:e===`class`?`.${t} &`:e===`data`?`[data-${t}] &`:`${e.replace(`%s`,t)} &`:`&`}}function cc(e,t,n=void 0){let r={};for(let i in e){let a=e[i],o=``,s=!0;for(let e=0;e<a.length;e+=1){let r=a[e];r&&(o+=(s===!0?``:` `)+t(r),s=!1,n&&n[r]&&(o+=` `+n[r]))}r[i]=o}return r}function lc(e,t){return x.isValidElement(e)&&t.indexOf(e.type.muiName??e.type?._payload?.value?.muiName)!==-1}function uc(){return{text:{primary:`rgba(0, 0, 0, 0.87)`,secondary:`rgba(0, 0, 0, 0.6)`,disabled:`rgba(0, 0, 0, 0.38)`},divider:`rgba(0, 0, 0, 0.12)`,background:{paper:vn.white,default:vn.white},action:{active:`rgba(0, 0, 0, 0.54)`,hover:`rgba(0, 0, 0, 0.04)`,hoverOpacity:.04,selected:`rgba(0, 0, 0, 0.08)`,selectedOpacity:.08,disabled:`rgba(0, 0, 0, 0.26)`,disabledBackground:`rgba(0, 0, 0, 0.12)`,disabledOpacity:.38,focus:`rgba(0, 0, 0, 0.12)`,focusOpacity:.12,activatedOpacity:.12}}}const dc=uc();function fc(){return{text:{primary:vn.white,secondary:`rgba(255, 255, 255, 0.7)`,disabled:`rgba(255, 255, 255, 0.5)`,icon:`rgba(255, 255, 255, 0.5)`},divider:`rgba(255, 255, 255, 0.12)`,background:{paper:`#121212`,default:`#121212`},action:{active:vn.white,hover:`rgba(255, 255, 255, 0.08)`,hoverOpacity:.08,selected:`rgba(255, 255, 255, 0.16)`,selectedOpacity:.16,disabled:`rgba(255, 255, 255, 0.3)`,disabledBackground:`rgba(255, 255, 255, 0.12)`,disabledOpacity:.38,focus:`rgba(255, 255, 255, 0.12)`,focusOpacity:.12,activatedOpacity:.24}}}const pc=fc();function mc(e,t,n,r){let i=r.light||r,a=r.dark||r*1.5;e[t]||(e.hasOwnProperty(n)?e[t]=e[n]:t===`light`?e.light=ps(e.main,i):t===`dark`&&(e.dark=ds(e.main,a)))}function hc(e,t,n,r,i){let a=i.light||i,o=i.dark||i*1.5;t[n]||(t.hasOwnProperty(r)?t[n]=t[r]:n===`light`?t.light=`color-mix(in ${e}, ${t.main}, #fff ${(a*100).toFixed(0)}%)`:n===`dark`&&(t.dark=`color-mix(in ${e}, ${t.main}, #000 ${(o*100).toFixed(0)}%)`))}function gc(e=`light`){return e===`dark`?{main:xn[200],light:xn[50],dark:xn[400]}:{main:xn[700],light:xn[400],dark:xn[800]}}function _c(e=`light`){return e===`dark`?{main:bn[200],light:bn[50],dark:bn[400]}:{main:bn[500],light:bn[300],dark:bn[700]}}function vc(e=`light`){return e===`dark`?{main:yn[500],light:yn[300],dark:yn[700]}:{main:yn[700],light:yn[400],dark:yn[800]}}function yc(e=`light`){return e===`dark`?{main:Sn[400],light:Sn[300],dark:Sn[700]}:{main:Sn[700],light:Sn[500],dark:Sn[900]}}function bc(e=`light`){return e===`dark`?{main:Cn[400],light:Cn[300],dark:Cn[700]}:{main:Cn[800],light:Cn[500],dark:Cn[900]}}function xc(e=`light`){return e===`dark`?{main:wn[400],light:wn[300],dark:wn[700]}:{main:`#ed6c02`,light:wn[500],dark:wn[900]}}function Sc(e){return`oklch(from ${e} var(--__l) 0 h / var(--__a))`}function Cc(e){let{mode:t=`light`,contrastThreshold:n=3,tonalOffset:r=.2,colorSpace:i,...a}=e,o=e.primary||gc(t),s=e.secondary||_c(t),c=e.error||vc(t),l=e.info||yc(t),u=e.success||bc(t),d=e.warning||xc(t);function f(e){return i?Sc(e):cs(e,pc.text.primary)>=n?pc.text.primary:dc.text.primary}let p=({color:e,name:t,mainShade:n=500,lightShade:a=300,darkShade:o=700})=>{if(e={...e},!e.main&&e[n]&&(e.main=e[n]),!e.hasOwnProperty(`main`))throw Error(En(11,t?` (${t})`:``,n));if(typeof e.main!=`string`)throw Error(En(12,t?` (${t})`:``,JSON.stringify(e.main)));return i?(hc(i,e,`light`,a,r),hc(i,e,`dark`,o,r)):(mc(e,`light`,a,r),mc(e,`dark`,o,r)),e.contrastText||=f(e.main),e},m;return t===`light`?m=uc():t===`dark`&&(m=fc()),Xi({common:{...vn},mode:t,primary:p({color:o,name:`primary`}),secondary:p({color:s,name:`secondary`,mainShade:`A400`,lightShade:`A200`,darkShade:`A700`}),error:p({color:c,name:`error`}),warning:p({color:d,name:`warning`}),info:p({color:l,name:`info`}),success:p({color:u,name:`success`}),grey:Tn,contrastThreshold:n,getContrastText:f,augmentColor:p,tonalOffset:r,...m},a)}function wc(e){let t={};return Object.entries(e).forEach(e=>{let[n,r]=e;typeof r==`object`&&(t[n]=`${r.fontStyle?`${r.fontStyle} `:``}${r.fontVariant?`${r.fontVariant} `:``}${r.fontWeight?`${r.fontWeight} `:``}${r.fontStretch?`${r.fontStretch} `:``}${r.fontSize||``}${r.lineHeight?`/${r.lineHeight} `:``}${r.fontFamily||``}`)}),t}function Tc(e,t){return{toolbar:{minHeight:56,[e.up(`xs`)]:{"@media (orientation: landscape)":{minHeight:48}},[e.up(`sm`)]:{minHeight:64}},...t}}function Ec(e){return Math.round(e*1e5)/1e5}var Dc={textTransform:`uppercase`},Oc=`"Roboto", "Helvetica", "Arial", sans-serif`;function kc(e,t){let{fontFamily:n=Oc,fontSize:r=14,fontWeightLight:i=300,fontWeightRegular:a=400,fontWeightMedium:o=500,fontWeightBold:s=700,htmlFontSize:c=16,allVariants:l,pxToRem:u,...d}=typeof t==`function`?t(e):t,f=r/14,p=u||(e=>`${e/c*f}rem`),m=(e,t,r,i,a)=>({fontFamily:n,fontWeight:e,fontSize:p(t),lineHeight:r,...n===Oc?{letterSpacing:`${Ec(i/t)}em`}:{},...a,...l});return Xi({htmlFontSize:c,pxToRem:p,fontFamily:n,fontSize:r,fontWeightLight:i,fontWeightRegular:a,fontWeightMedium:o,fontWeightBold:s,h1:m(i,96,1.167,-1.5),h2:m(i,60,1.2,-.5),h3:m(a,48,1.167,0),h4:m(a,34,1.235,.25),h5:m(a,24,1.334,0),h6:m(o,20,1.6,.15),subtitle1:m(a,16,1.75,.15),subtitle2:m(o,14,1.57,.1),body1:m(a,16,1.5,.15),body2:m(a,14,1.43,.15),button:m(o,14,1.75,.4,Dc),caption:m(a,12,1.66,.4),overline:m(a,12,2.66,1,Dc),inherit:{fontFamily:`inherit`,fontWeight:`inherit`,fontSize:`inherit`,lineHeight:`inherit`,letterSpacing:`inherit`}},d,{clone:!1})}var Ac=.2,jc=.14,Mc=.12;function Nc(...e){return[`${e[0]}px ${e[1]}px ${e[2]}px ${e[3]}px rgba(0,0,0,${Ac})`,`${e[4]}px ${e[5]}px ${e[6]}px ${e[7]}px rgba(0,0,0,${jc})`,`${e[8]}px ${e[9]}px ${e[10]}px ${e[11]}px rgba(0,0,0,${Mc})`].join(`,`)}var Pc=[`none`,Nc(0,2,1,-1,0,1,1,0,0,1,3,0),Nc(0,3,1,-2,0,2,2,0,0,1,5,0),Nc(0,3,3,-2,0,3,4,0,0,1,8,0),Nc(0,2,4,-1,0,4,5,0,0,1,10,0),Nc(0,3,5,-1,0,5,8,0,0,1,14,0),Nc(0,3,5,-1,0,6,10,0,0,1,18,0),Nc(0,4,5,-2,0,7,10,1,0,2,16,1),Nc(0,5,5,-3,0,8,10,1,0,3,14,2),Nc(0,5,6,-3,0,9,12,1,0,3,16,2),Nc(0,6,6,-3,0,10,14,1,0,4,18,3),Nc(0,6,7,-4,0,11,15,1,0,4,20,3),Nc(0,7,8,-4,0,12,17,2,0,5,22,4),Nc(0,7,8,-4,0,13,19,2,0,5,24,4),Nc(0,7,9,-4,0,14,21,2,0,5,26,4),Nc(0,8,9,-5,0,15,22,2,0,6,28,5),Nc(0,8,10,-5,0,16,24,2,0,6,30,5),Nc(0,8,11,-5,0,17,26,2,0,6,32,5),Nc(0,9,11,-5,0,18,28,2,0,7,34,6),Nc(0,9,12,-6,0,19,29,2,0,7,36,6),Nc(0,10,13,-6,0,20,31,3,0,8,38,7),Nc(0,10,13,-6,0,21,33,3,0,8,40,7),Nc(0,10,14,-6,0,22,35,3,0,8,42,7),Nc(0,11,14,-7,0,23,36,3,0,9,44,8),Nc(0,11,15,-7,0,24,38,3,0,9,46,8)];const Fc={easeInOut:`cubic-bezier(0.4, 0, 0.2, 1)`,easeOut:`cubic-bezier(0.0, 0, 0.2, 1)`,easeIn:`cubic-bezier(0.4, 0, 1, 1)`,sharp:`cubic-bezier(0.4, 0, 0.6, 1)`},Ic={shortest:150,shorter:200,short:250,standard:300,complex:375,enteringScreen:225,leavingScreen:195};function Lc(e){return`${Math.round(e)}ms`}function Rc(e){if(!e)return 0;let t=e/36;return Math.min(Math.round((4+15*t**.25+t/5)*10),3e3)}function zc(e){let t={...Fc,...e.easing},n={...Ic,...e.duration};return{getAutoHeightDuration:Rc,create:(e=[`all`],r={})=>{let{duration:i=n.standard,easing:a=t.easeInOut,delay:o=0,...s}=r;return(Array.isArray(e)?e:[e]).map(e=>`${e} ${typeof i==`string`?i:Lc(i)} ${a} ${typeof o==`string`?o:Lc(o)}`).join(`,`)},...e,easing:t,duration:n}}var Bc={mobileStepper:1e3,fab:1050,speedDial:1050,appBar:1100,drawer:1200,modal:1300,snackbar:1400,tooltip:1500};function Vc(e){return Ji(e)||e===void 0||typeof e==`string`||typeof e==`boolean`||typeof e==`number`||Array.isArray(e)}function Hc(e={}){let t={...e};function n(e){let t=Object.entries(e);for(let r=0;r<t.length;r++){let[i,a]=t[r];!Vc(a)||i.startsWith(`unstable_`)?delete e[i]:Ji(a)&&(e[i]={...a},n(e[i]))}}return n(t),`import { unstable_createBreakpoints as createBreakpoints, createTransitions } from '@mui/material/styles';

const theme = ${JSON.stringify(t,null,2)};

theme.breakpoints = createBreakpoints(theme.breakpoints || {});
theme.transitions = createTransitions(theme.transitions || {});

export default theme;`}function Uc(e){return typeof e==`number`?`${(e*100).toFixed(0)}%`:`calc((${e}) * 100%)`}var Wc=e=>{if(!Number.isNaN(+e))return+e;let t=e.match(/\d*\.?\d+/g);if(!t)return 0;let n=0;for(let e=0;e<t.length;e+=1)n+=+t[e];return n};function Gc(e){Object.assign(e,{alpha(t,n){let r=this||e;return r.colorSpace?`oklch(from ${t} l c h / ${typeof n==`string`?`calc(${n})`:n})`:r.vars?`rgba(${t.replace(/var\(--([^,\s)]+)(?:,[^)]+)?\)+/g,`var(--$1Channel)`)} / ${typeof n==`string`?`calc(${n})`:n})`:ls(t,Wc(n))},lighten(t,n){let r=this||e;return r.colorSpace?`color-mix(in ${r.colorSpace}, ${t}, #fff ${Uc(n)})`:ps(t,n)},darken(t,n){let r=this||e;return r.colorSpace?`color-mix(in ${r.colorSpace}, ${t}, #000 ${Uc(n)})`:ds(t,n)}})}function Kc(e={},...t){let{breakpoints:n,mixins:r={},spacing:i,palette:a={},transitions:o={},typography:s={},shape:c,colorSpace:l,...u}=e;if(e.vars&&e.generateThemeVars===void 0)throw Error(En(20));let d=Cc({...a,colorSpace:l}),f=_o(e),p=Xi(f,{mixins:Tc(f.breakpoints,r),palette:d,shadows:Pc.slice(),typography:kc(d,s),transitions:zc(o),zIndex:{...Bc}});return p=Xi(p,u),p=t.reduce((e,t)=>Xi(e,t),p),p.unstable_sxConfig={...lo,...u?.unstable_sxConfig},p.unstable_sx=function(e){return z({sx:e,theme:this})},p.toRuntimeSource=Hc,Gc(p),p}var qc=Kc;function Jc(e){let t;return t=e<1?5.11916*e**2:4.5*Math.log(e+1)+2,Math.round(t*10)/1e3}var Yc=[...Array(25)].map((e,t)=>{if(t===0)return`none`;let n=Jc(t);return`linear-gradient(rgba(255 255 255 / ${n}), rgba(255 255 255 / ${n}))`});function Xc(e){return{inputPlaceholder:e===`dark`?.5:.42,inputUnderline:e===`dark`?.7:.42,switchTrackDisabled:e===`dark`?.2:.12,switchTrack:e===`dark`?.3:.38}}function Zc(e){return e===`dark`?Yc:[]}function Qc(e){let{palette:t={mode:`light`},opacity:n,overlays:r,colorSpace:i,...a}=e,o=Cc({...t,colorSpace:i});return{palette:o,opacity:{...Xc(o.mode),...n},overlays:r||Zc(o.mode),...a}}function $c(e){return!!e[0].match(/(cssVarPrefix|colorSchemeSelector|modularCssLayers|rootSelector|typography|mixins|breakpoints|direction|transitions)/)||!!e[0].match(/sxConfig$/)||e[0]===`palette`&&!!e[1]?.match(/(mode|contrastThreshold|tonalOffset)/)}var el=e=>[...[...Array(25)].map((t,n)=>`--${e?`${e}-`:``}overlays-${n}`),`--${e?`${e}-`:``}palette-AppBar-darkBg`,`--${e?`${e}-`:``}palette-AppBar-darkColor`],tl=e=>(t,n)=>{let r=e.rootSelector||`:root`,i=e.colorSchemeSelector,a=i;if(i===`class`&&(a=`.%s`),i===`data`&&(a=`[data-%s]`),i?.startsWith(`data-`)&&!i.includes(`%s`)&&(a=`[${i}="%s"]`),e.defaultColorScheme===t){if(t===`dark`){let i={};return el(e.cssVarPrefix).forEach(e=>{i[e]=n[e],delete n[e]}),a===`media`?{[r]:n,"@media (prefers-color-scheme: dark)":{[r]:i}}:a?{[a.replace(`%s`,t)]:i,[`${r}, ${a.replace(`%s`,t)}`]:n}:{[r]:{...n,...i}}}if(a&&a!==`media`)return`${r}, ${a.replace(`%s`,String(t))}`}else if(t){if(a===`media`)return{[`@media (prefers-color-scheme: ${String(t)})`]:{[r]:n}};if(a)return a.replace(`%s`,String(t))}return r};function nl(e,t){t.forEach(t=>{e[t]||(e[t]={})})}function V(e,t,n){!e[t]&&n&&(e[t]=n)}function rl(e){return typeof e!=`string`||!e.startsWith(`hsl`)?e:os(e)}function il(e,t){`${t}Channel`in e||(e[`${t}Channel`]=is(rl(e[t]),`MUI: Can't create \`palette.${t}Channel\` because \`palette.${t}\` is not one of these formats: #nnn, #nnnnnn, rgb(), rgba(), hsl(), hsla(), color().
To suppress this warning, you need to explicitly provide the \`palette.${t}Channel\` as a string (in rgb format, for example "12 12 12") or undefined if you want to remove the channel token.`))}function al(e){return typeof e==`number`?`${e}px`:typeof e==`string`||typeof e==`function`||Array.isArray(e)?e:`8px`}var ol=e=>{try{return e()}catch{}};const sl=(e=`mui`)=>ec(e);function cl(e,t,n,r,i){if(!n)return;n=n===!0?{}:n;let a=i===`dark`?`dark`:`light`;if(!r){t[i]=Qc({...n,palette:{mode:a,...n?.palette},colorSpace:e});return}let{palette:o,...s}=qc({...r,palette:{mode:a,...n?.palette},colorSpace:e});return t[i]={...n,palette:o,opacity:{...Xc(a),...n?.opacity},overlays:n?.overlays||Zc(a)},s}function ll(e={},...t){let{colorSchemes:n={light:!0},defaultColorScheme:r,disableCssColorScheme:i=!1,cssVarPrefix:a=`mui`,nativeColor:o=!1,shouldSkipGeneratingVar:s=$c,colorSchemeSelector:c=n.light&&n.dark?`media`:void 0,rootSelector:l=`:root`,...u}=e,d=Object.keys(n)[0],f=r||(n.light&&d!==`light`?`light`:d),p=sl(a),{[f]:m,light:h,dark:g,..._}=n,v={..._},y=m;if((f===`dark`&&!(`dark`in n)||f===`light`&&!(`light`in n))&&(y=!0),!y)throw Error(En(21,f));let b;o&&(b=`oklch`);let x=cl(b,v,y,u,f);h&&!v.light&&cl(b,v,h,void 0,`light`),g&&!v.dark&&cl(b,v,g,void 0,`dark`);let S={defaultColorScheme:f,...x,cssVarPrefix:a,colorSchemeSelector:c,rootSelector:l,getCssVar:p,colorSchemes:v,font:{...wc(x.typography),...x.font},spacing:al(u.spacing)};Object.keys(S.colorSchemes).forEach(e=>{let t=S.colorSchemes[e].palette,n=e=>{let n=e.split(`-`),r=n[1],i=n[2];return p(e,t[r][i])};t.mode===`light`&&(V(t.common,`background`,`#fff`),V(t.common,`onBackground`,`#000`)),t.mode===`dark`&&(V(t.common,`background`,`#000`),V(t.common,`onBackground`,`#fff`));function r(e,t,n){if(b){let r;return e===us&&(r=`transparent ${((1-n)*100).toFixed(0)}%`),e===fs&&(r=`#000 ${(n*100).toFixed(0)}%`),e===ms&&(r=`#fff ${(n*100).toFixed(0)}%`),`color-mix(in ${b}, ${t}, ${r})`}return e(t,n)}if(nl(t,[`Alert`,`AppBar`,`Avatar`,`Button`,`Chip`,`FilledInput`,`LinearProgress`,`Skeleton`,`Slider`,`SnackbarContent`,`SpeedDialAction`,`StepConnector`,`StepContent`,`Switch`,`TableCell`,`Tooltip`]),t.mode===`light`){V(t.Alert,`errorColor`,r(fs,t.error.light,.6)),V(t.Alert,`infoColor`,r(fs,t.info.light,.6)),V(t.Alert,`successColor`,r(fs,t.success.light,.6)),V(t.Alert,`warningColor`,r(fs,t.warning.light,.6)),V(t.Alert,`errorFilledBg`,n(`palette-error-main`)),V(t.Alert,`infoFilledBg`,n(`palette-info-main`)),V(t.Alert,`successFilledBg`,n(`palette-success-main`)),V(t.Alert,`warningFilledBg`,n(`palette-warning-main`)),V(t.Alert,`errorFilledColor`,ol(()=>t.getContrastText(t.error.main))),V(t.Alert,`infoFilledColor`,ol(()=>t.getContrastText(t.info.main))),V(t.Alert,`successFilledColor`,ol(()=>t.getContrastText(t.success.main))),V(t.Alert,`warningFilledColor`,ol(()=>t.getContrastText(t.warning.main))),V(t.Alert,`errorStandardBg`,r(ms,t.error.light,.9)),V(t.Alert,`infoStandardBg`,r(ms,t.info.light,.9)),V(t.Alert,`successStandardBg`,r(ms,t.success.light,.9)),V(t.Alert,`warningStandardBg`,r(ms,t.warning.light,.9)),V(t.Alert,`errorIconColor`,n(`palette-error-main`)),V(t.Alert,`infoIconColor`,n(`palette-info-main`)),V(t.Alert,`successIconColor`,n(`palette-success-main`)),V(t.Alert,`warningIconColor`,n(`palette-warning-main`)),V(t.AppBar,`defaultBg`,n(`palette-grey-100`)),V(t.Avatar,`defaultBg`,n(`palette-grey-400`)),V(t.Button,`inheritContainedBg`,n(`palette-grey-300`)),V(t.Button,`inheritContainedHoverBg`,n(`palette-grey-A100`)),V(t.Chip,`defaultBorder`,n(`palette-grey-400`)),V(t.Chip,`defaultAvatarColor`,n(`palette-grey-700`)),V(t.Chip,`defaultIconColor`,n(`palette-grey-700`)),V(t.FilledInput,`bg`,`rgba(0, 0, 0, 0.06)`),V(t.FilledInput,`hoverBg`,`rgba(0, 0, 0, 0.09)`),V(t.FilledInput,`disabledBg`,`rgba(0, 0, 0, 0.12)`),V(t.LinearProgress,`primaryBg`,r(ms,t.primary.main,.62)),V(t.LinearProgress,`secondaryBg`,r(ms,t.secondary.main,.62)),V(t.LinearProgress,`errorBg`,r(ms,t.error.main,.62)),V(t.LinearProgress,`infoBg`,r(ms,t.info.main,.62)),V(t.LinearProgress,`successBg`,r(ms,t.success.main,.62)),V(t.LinearProgress,`warningBg`,r(ms,t.warning.main,.62)),V(t.Skeleton,`bg`,b?r(us,t.text.primary,.11):`rgba(${n(`palette-text-primaryChannel`)} / 0.11)`),V(t.Slider,`primaryTrack`,r(ms,t.primary.main,.62)),V(t.Slider,`secondaryTrack`,r(ms,t.secondary.main,.62)),V(t.Slider,`errorTrack`,r(ms,t.error.main,.62)),V(t.Slider,`infoTrack`,r(ms,t.info.main,.62)),V(t.Slider,`successTrack`,r(ms,t.success.main,.62)),V(t.Slider,`warningTrack`,r(ms,t.warning.main,.62));let e=b?r(fs,t.background.default,.6825):gs(t.background.default,.8);V(t.SnackbarContent,`bg`,e),V(t.SnackbarContent,`color`,ol(()=>b?pc.text.primary:t.getContrastText(e))),V(t.SpeedDialAction,`fabHoverBg`,gs(t.background.paper,.15)),V(t.StepConnector,`border`,n(`palette-grey-400`)),V(t.StepContent,`border`,n(`palette-grey-400`)),V(t.Switch,`defaultColor`,n(`palette-common-white`)),V(t.Switch,`defaultDisabledColor`,n(`palette-grey-100`)),V(t.Switch,`primaryDisabledColor`,r(ms,t.primary.main,.62)),V(t.Switch,`secondaryDisabledColor`,r(ms,t.secondary.main,.62)),V(t.Switch,`errorDisabledColor`,r(ms,t.error.main,.62)),V(t.Switch,`infoDisabledColor`,r(ms,t.info.main,.62)),V(t.Switch,`successDisabledColor`,r(ms,t.success.main,.62)),V(t.Switch,`warningDisabledColor`,r(ms,t.warning.main,.62)),V(t.TableCell,`border`,r(ms,r(us,t.divider,1),.88)),V(t.Tooltip,`bg`,r(us,t.grey[700],.92))}if(t.mode===`dark`){V(t.Alert,`errorColor`,r(ms,t.error.light,.6)),V(t.Alert,`infoColor`,r(ms,t.info.light,.6)),V(t.Alert,`successColor`,r(ms,t.success.light,.6)),V(t.Alert,`warningColor`,r(ms,t.warning.light,.6)),V(t.Alert,`errorFilledBg`,n(`palette-error-dark`)),V(t.Alert,`infoFilledBg`,n(`palette-info-dark`)),V(t.Alert,`successFilledBg`,n(`palette-success-dark`)),V(t.Alert,`warningFilledBg`,n(`palette-warning-dark`)),V(t.Alert,`errorFilledColor`,ol(()=>t.getContrastText(t.error.dark))),V(t.Alert,`infoFilledColor`,ol(()=>t.getContrastText(t.info.dark))),V(t.Alert,`successFilledColor`,ol(()=>t.getContrastText(t.success.dark))),V(t.Alert,`warningFilledColor`,ol(()=>t.getContrastText(t.warning.dark))),V(t.Alert,`errorStandardBg`,r(fs,t.error.light,.9)),V(t.Alert,`infoStandardBg`,r(fs,t.info.light,.9)),V(t.Alert,`successStandardBg`,r(fs,t.success.light,.9)),V(t.Alert,`warningStandardBg`,r(fs,t.warning.light,.9)),V(t.Alert,`errorIconColor`,n(`palette-error-main`)),V(t.Alert,`infoIconColor`,n(`palette-info-main`)),V(t.Alert,`successIconColor`,n(`palette-success-main`)),V(t.Alert,`warningIconColor`,n(`palette-warning-main`)),V(t.AppBar,`defaultBg`,n(`palette-grey-900`)),V(t.AppBar,`darkBg`,n(`palette-background-paper`)),V(t.AppBar,`darkColor`,n(`palette-text-primary`)),V(t.Avatar,`defaultBg`,n(`palette-grey-600`)),V(t.Button,`inheritContainedBg`,n(`palette-grey-800`)),V(t.Button,`inheritContainedHoverBg`,n(`palette-grey-700`)),V(t.Chip,`defaultBorder`,n(`palette-grey-700`)),V(t.Chip,`defaultAvatarColor`,n(`palette-grey-300`)),V(t.Chip,`defaultIconColor`,n(`palette-grey-300`)),V(t.FilledInput,`bg`,`rgba(255, 255, 255, 0.09)`),V(t.FilledInput,`hoverBg`,`rgba(255, 255, 255, 0.13)`),V(t.FilledInput,`disabledBg`,`rgba(255, 255, 255, 0.12)`),V(t.LinearProgress,`primaryBg`,r(fs,t.primary.main,.5)),V(t.LinearProgress,`secondaryBg`,r(fs,t.secondary.main,.5)),V(t.LinearProgress,`errorBg`,r(fs,t.error.main,.5)),V(t.LinearProgress,`infoBg`,r(fs,t.info.main,.5)),V(t.LinearProgress,`successBg`,r(fs,t.success.main,.5)),V(t.LinearProgress,`warningBg`,r(fs,t.warning.main,.5)),V(t.Skeleton,`bg`,b?r(us,t.text.primary,.13):`rgba(${n(`palette-text-primaryChannel`)} / 0.13)`),V(t.Slider,`primaryTrack`,r(fs,t.primary.main,.5)),V(t.Slider,`secondaryTrack`,r(fs,t.secondary.main,.5)),V(t.Slider,`errorTrack`,r(fs,t.error.main,.5)),V(t.Slider,`infoTrack`,r(fs,t.info.main,.5)),V(t.Slider,`successTrack`,r(fs,t.success.main,.5)),V(t.Slider,`warningTrack`,r(fs,t.warning.main,.5));let e=b?r(ms,t.background.default,.985):gs(t.background.default,.98);V(t.SnackbarContent,`bg`,e),V(t.SnackbarContent,`color`,ol(()=>b?dc.text.primary:t.getContrastText(e))),V(t.SpeedDialAction,`fabHoverBg`,gs(t.background.paper,.15)),V(t.StepConnector,`border`,n(`palette-grey-600`)),V(t.StepContent,`border`,n(`palette-grey-600`)),V(t.Switch,`defaultColor`,n(`palette-grey-300`)),V(t.Switch,`defaultDisabledColor`,n(`palette-grey-600`)),V(t.Switch,`primaryDisabledColor`,r(fs,t.primary.main,.55)),V(t.Switch,`secondaryDisabledColor`,r(fs,t.secondary.main,.55)),V(t.Switch,`errorDisabledColor`,r(fs,t.error.main,.55)),V(t.Switch,`infoDisabledColor`,r(fs,t.info.main,.55)),V(t.Switch,`successDisabledColor`,r(fs,t.success.main,.55)),V(t.Switch,`warningDisabledColor`,r(fs,t.warning.main,.55)),V(t.TableCell,`border`,r(fs,r(us,t.divider,1),.68)),V(t.Tooltip,`bg`,r(us,t.grey[700],.92))}il(t.background,`default`),il(t.background,`paper`),il(t.common,`background`),il(t.common,`onBackground`),il(t,`divider`),Object.keys(t).forEach(e=>{let n=t[e];e!==`tonalOffset`&&n&&typeof n==`object`&&(n.main&&V(t[e],`mainChannel`,is(rl(n.main))),n.light&&V(t[e],`lightChannel`,is(rl(n.light))),n.dark&&V(t[e],`darkChannel`,is(rl(n.dark))),n.contrastText&&V(t[e],`contrastTextChannel`,is(rl(n.contrastText))),e===`text`&&(il(t[e],`primary`),il(t[e],`secondary`)),e===`action`&&(n.active&&il(t[e],`active`),n.selected&&il(t[e],`selected`)))})}),S=t.reduce((e,t)=>Xi(e,t),S);let C={prefix:a,disableCssColorScheme:i,shouldSkipGeneratingVar:s,getSelector:tl(S),enableContrastVars:o},{vars:w,generateThemeVars:T,generateStyleSheets:E}=oc(S,C);return S.vars=w,Object.entries(S.colorSchemes[S.defaultColorScheme]).forEach(([e,t])=>{S[e]=t}),S.generateThemeVars=T,S.generateStyleSheets=E,S.generateSpacing=function(){return Pa(u.spacing,Ea(this))},S.getColorSchemeSelector=sc(c),S.spacing=S.generateSpacing(),S.shouldSkipGeneratingVar=s,S.unstable_sxConfig={...lo,...u?.unstable_sxConfig},S.unstable_sx=function(e){return z({sx:e,theme:this})},S.toRuntimeSource=Hc,S}function ul(e,t,n){e.colorSchemes&&n&&(e.colorSchemes[t]={...n!==!0&&n,palette:Cc({...n===!0?{}:n.palette,mode:t})})}function dl(e={},...t){let{palette:n,cssVariables:r=!1,colorSchemes:i=n?void 0:{light:!0},defaultColorScheme:a=n?.mode,...o}=e,s=a||`light`,c=i?.[s],l={...i,...n?{[s]:{...typeof c!=`boolean`&&c,palette:n}}:void 0};if(r===!1){if(!(`colorSchemes`in e))return qc(e,...t);let r=n;`palette`in e||l[s]&&(l[s]===!0?s===`dark`&&(r={mode:`dark`}):r=l[s].palette);let i=qc({...e,palette:r},...t);return i.defaultColorScheme=s,i.colorSchemes=l,i.palette.mode===`light`&&(i.colorSchemes.light={...l.light!==!0&&l.light,palette:i.palette},ul(i,`dark`,l.dark)),i.palette.mode===`dark`&&(i.colorSchemes.dark={...l.dark!==!0&&l.dark,palette:i.palette},ul(i,`light`,l.light)),i}return!n&&!(`light`in l)&&s===`light`&&(l.light=!0),ll({...o,colorSchemes:l,defaultColorScheme:s,...typeof r!=`boolean`&&r},...t)}var fl=dl();function pl(){let e=Co(fl);return e.$$material||e}function ml(e){return e!==`ownerState`&&e!==`theme`&&e!==`sx`&&e!==`as`}var hl=ml,gl=e=>hl(e)&&e!==`classes`,H=Ko({themeId:Dn,defaultTheme:fl,rootShouldForwardProp:gl});function _l({theme:e,...t}){let n=`$$material`in e?e[Dn]:void 0;return(0,R.jsx)(Bs,{...t,themeId:n?Dn:void 0,theme:n||e})}const vl={attribute:`data-mui-color-scheme`,colorSchemeStorageKey:`mui-color-scheme`,defaultLightColorScheme:`light`,defaultDarkColorScheme:`dark`,modeStorageKey:`mui-mode`};var{CssVarsProvider:yl,useColorScheme:bl,getInitColorSchemeScript:xl}=$s({themeId:Dn,theme:()=>dl({cssVariables:!0}),colorSchemeStorageKey:vl.colorSchemeStorageKey,modeStorageKey:vl.modeStorageKey,defaultColorScheme:{light:vl.defaultLightColorScheme,dark:vl.defaultDarkColorScheme},resolveTheme:e=>{let t={...e,typography:kc(e.palette,e.typography)};return t.unstable_sx=function(e){return z({sx:e,theme:this})},t}});const Sl=yl;function Cl({theme:e,...t}){let n=x.useMemo(()=>{if(typeof e==`function`)return e;let t=`$$material`in e?e[Dn]:e;return`colorSchemes`in t?null:`vars`in t?e:{...e,vars:null}},[e]);return n?(0,R.jsx)(_l,{theme:n,...t}):(0,R.jsx)(Sl,{theme:e,...t})}var U=fa;function wl(...e){return e.reduce((e,t)=>t==null?e:function(...n){e.apply(this,n),t.apply(this,n)},()=>{})}function Tl(e){return(0,R.jsx)(Eo,{...e,defaultTheme:fl,themeId:Dn})}var El=Tl;function Dl(e){return function(t){return(0,R.jsx)(El,{styles:typeof e==`function`?n=>e({theme:n,...t}):e})}}function Ol(){return Oo}var kl=Hs;function Al(e){return As(e)}function jl(e){return Io(`MuiSvgIcon`,e)}Lo(`MuiSvgIcon`,[`root`,`colorPrimary`,`colorSecondary`,`colorAction`,`colorError`,`colorDisabled`,`fontSizeInherit`,`fontSizeSmall`,`fontSizeMedium`,`fontSizeLarge`]);var Ml=e=>{let{color:t,fontSize:n,classes:r}=e;return cc({root:[`root`,t!==`inherit`&&`color${U(t)}`,`fontSize${U(n)}`]},jl,r)},Nl=H(`svg`,{name:`MuiSvgIcon`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.color!==`inherit`&&t[`color${U(n.color)}`],t[`fontSize${U(n.fontSize)}`]]}})(kl(({theme:e})=>({userSelect:`none`,width:`1em`,height:`1em`,display:`inline-block`,flexShrink:0,transition:e.transitions?.create?.(`fill`,{duration:(e.vars??e).transitions?.duration?.shorter}),variants:[{props:e=>!e.hasSvgAsChild,style:{fill:`currentColor`}},{props:{fontSize:`inherit`},style:{fontSize:`inherit`}},{props:{fontSize:`small`},style:{fontSize:e.typography?.pxToRem?.(20)||`1.25rem`}},{props:{fontSize:`medium`},style:{fontSize:e.typography?.pxToRem?.(24)||`1.5rem`}},{props:{fontSize:`large`},style:{fontSize:e.typography?.pxToRem?.(35)||`2.1875rem`}},...Object.entries((e.vars??e).palette).filter(([,e])=>e&&e.main).map(([t])=>({props:{color:t},style:{color:(e.vars??e).palette?.[t]?.main}})),{props:{color:`action`},style:{color:(e.vars??e).palette?.action?.active}},{props:{color:`disabled`},style:{color:(e.vars??e).palette?.action?.disabled}},{props:{color:`inherit`},style:{color:void 0}}]}))),Pl=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiSvgIcon`}),{children:r,className:i,color:a=`inherit`,component:o=`svg`,fontSize:s=`medium`,htmlColor:c,inheritViewBox:l=!1,titleAccess:u,viewBox:d=`0 0 24 24`,...f}=n,p=x.isValidElement(r)&&r.type===`svg`,m={...n,color:a,component:o,fontSize:s,instanceFontSize:e.fontSize,inheritViewBox:l,viewBox:d,hasSvgAsChild:p},h={};return l||(h.viewBox=d),(0,R.jsxs)(Nl,{as:o,className:B(Ml(m).root,i),focusable:`false`,color:c,"aria-hidden":u?void 0:!0,role:u?`img`:void 0,ref:t,...h,...f,...p&&r.props,ownerState:m,children:[p?r.props.children:r,u?(0,R.jsx)(`title`,{children:u}):null]})});Pl.muiName=`SvgIcon`;var Fl=Pl;function Il(e,t){function n(t,n){return(0,R.jsx)(Fl,{"data-testid":void 0,ref:n,...t,children:e})}return n.muiName=Fl.muiName,x.memo(x.forwardRef(n))}function Ll(e,t=166){let n;function r(...r){clearTimeout(n),n=setTimeout(()=>{e.apply(this,r)},t)}return r.clear=()=>{clearTimeout(n)},r}var Rl=lc;function zl(e){return e&&e.ownerDocument||document}function Bl(e){return zl(e).defaultView||window}function Vl(e,t){typeof e==`function`?e(t):e&&(e.current=t)}var Hl=Zo,W=Fs;function Ul(e){let{controlled:t,default:n,name:r,state:i=`value`}=e,{current:a}=x.useRef(t!==void 0),[o,s]=x.useState(n);return[a?t:o,x.useCallback(e=>{a||s(e)},[])]}var Wl=Ul;function Gl(e){let t=x.useRef(e);return Zo(()=>{t.current=e}),x.useRef((...e)=>(0,t.current)(...e)).current}var Kl=Gl,ql=Kl;function Jl(...e){let t=x.useRef(void 0),n=x.useCallback(t=>{let n=e.map(e=>{if(e==null)return null;if(typeof e==`function`){let n=e,r=n(t);return typeof r==`function`?r:()=>{n(null)}}return e.current=t,()=>{e.current=null}});return()=>{n.forEach(e=>e?.())}},e);return x.useMemo(()=>e.every(e=>e==null)?null:e=>{t.current&&=(t.current(),void 0),e!=null&&(t.current=n(e))},e)}var Yl=Jl;function Xl(e,t){if(e==null)return{};var n={};for(var r in e)if({}.hasOwnProperty.call(e,r)){if(t.indexOf(r)!==-1)continue;n[r]=e[r]}return n}function Zl(e,t){return Zl=Object.setPrototypeOf?Object.setPrototypeOf.bind():function(e,t){return e.__proto__=t,e},Zl(e,t)}function Ql(e,t){e.prototype=Object.create(t.prototype),e.prototype.constructor=e,Zl(e,t)}var $l={disabled:!1},eu=x.createContext(null),tu=function(e){return e.scrollTop},nu=c(m()),ru=`unmounted`,iu=`exited`,au=`entering`,ou=`entered`,su=`exiting`,cu=function(e){Ql(t,e);function t(t,n){var r=e.call(this,t,n)||this,i=n,a=i&&!i.isMounting?t.enter:t.appear,o;return r.appearStatus=null,t.in?a?(o=iu,r.appearStatus=au):o=ou:o=t.unmountOnExit||t.mountOnEnter?ru:iu,r.state={status:o},r.nextCallback=null,r}t.getDerivedStateFromProps=function(e,t){return e.in&&t.status===`unmounted`?{status:iu}:null};var n=t.prototype;return n.componentDidMount=function(){this.updateStatus(!0,this.appearStatus)},n.componentDidUpdate=function(e){var t=null;if(e!==this.props){var n=this.state.status;this.props.in?n!==`entering`&&n!==`entered`&&(t=au):(n===`entering`||n===`entered`)&&(t=su)}this.updateStatus(!1,t)},n.componentWillUnmount=function(){this.cancelNextCallback()},n.getTimeouts=function(){var e=this.props.timeout,t=n=r=e,n,r;return e!=null&&typeof e!=`number`&&(t=e.exit,n=e.enter,r=e.appear===void 0?n:e.appear),{exit:t,enter:n,appear:r}},n.updateStatus=function(e,t){if(e===void 0&&(e=!1),t!==null)if(this.cancelNextCallback(),t===`entering`){if(this.props.unmountOnExit||this.props.mountOnEnter){var n=this.props.nodeRef?this.props.nodeRef.current:nu.default.findDOMNode(this);n&&tu(n)}this.performEnter(e)}else this.performExit();else this.props.unmountOnExit&&this.state.status===`exited`&&this.setState({status:ru})},n.performEnter=function(e){var t=this,n=this.props.enter,r=this.context?this.context.isMounting:e,i=this.props.nodeRef?[r]:[nu.default.findDOMNode(this),r],a=i[0],o=i[1],s=this.getTimeouts(),c=r?s.appear:s.enter;if(!e&&!n||$l.disabled){this.safeSetState({status:ou},function(){t.props.onEntered(a)});return}this.props.onEnter(a,o),this.safeSetState({status:au},function(){t.props.onEntering(a,o),t.onTransitionEnd(c,function(){t.safeSetState({status:ou},function(){t.props.onEntered(a,o)})})})},n.performExit=function(){var e=this,t=this.props.exit,n=this.getTimeouts(),r=this.props.nodeRef?void 0:nu.default.findDOMNode(this);if(!t||$l.disabled){this.safeSetState({status:iu},function(){e.props.onExited(r)});return}this.props.onExit(r),this.safeSetState({status:su},function(){e.props.onExiting(r),e.onTransitionEnd(n.exit,function(){e.safeSetState({status:iu},function(){e.props.onExited(r)})})})},n.cancelNextCallback=function(){this.nextCallback!==null&&(this.nextCallback.cancel(),this.nextCallback=null)},n.safeSetState=function(e,t){t=this.setNextCallback(t),this.setState(e,t)},n.setNextCallback=function(e){var t=this,n=!0;return this.nextCallback=function(r){n&&(n=!1,t.nextCallback=null,e(r))},this.nextCallback.cancel=function(){n=!1},this.nextCallback},n.onTransitionEnd=function(e,t){this.setNextCallback(t);var n=this.props.nodeRef?this.props.nodeRef.current:nu.default.findDOMNode(this),r=e==null&&!this.props.addEndListener;if(!n||r){setTimeout(this.nextCallback,0);return}if(this.props.addEndListener){var i=this.props.nodeRef?[this.nextCallback]:[n,this.nextCallback],a=i[0],o=i[1];this.props.addEndListener(a,o)}e!=null&&setTimeout(this.nextCallback,e)},n.render=function(){var e=this.state.status;if(e===`unmounted`)return null;var t=this.props,n=t.children;t.in,t.mountOnEnter,t.unmountOnExit,t.appear,t.enter,t.exit,t.timeout,t.addEndListener,t.onEnter,t.onEntering,t.onEntered,t.onExit,t.onExiting,t.onExited,t.nodeRef;var r=Xl(t,[`children`,`in`,`mountOnEnter`,`unmountOnExit`,`appear`,`enter`,`exit`,`timeout`,`addEndListener`,`onEnter`,`onEntering`,`onEntered`,`onExit`,`onExiting`,`onExited`,`nodeRef`]);return x.createElement(eu.Provider,{value:null},typeof n==`function`?n(e,r):x.cloneElement(x.Children.only(n),r))},t}(x.Component);cu.contextType=eu,cu.propTypes={};function lu(){}cu.defaultProps={in:!1,mountOnEnter:!1,unmountOnExit:!1,appear:!1,enter:!0,exit:!0,onEnter:lu,onEntering:lu,onEntered:lu,onExit:lu,onExiting:lu,onExited:lu},cu.UNMOUNTED=ru,cu.EXITED=iu,cu.ENTERING=au,cu.ENTERED=ou,cu.EXITING=su;var uu=cu;function du(e){if(e===void 0)throw ReferenceError(`this hasn't been initialised - super() hasn't been called`);return e}function fu(e,t){var n=function(e){return t&&(0,x.isValidElement)(e)?t(e):e},r=Object.create(null);return e&&x.Children.map(e,function(e){return e}).forEach(function(e){r[e.key]=n(e)}),r}function pu(e,t){e||={},t||={};function n(n){return n in t?t[n]:e[n]}var r=Object.create(null),i=[];for(var a in e)a in t?i.length&&(r[a]=i,i=[]):i.push(a);var o,s={};for(var c in t){if(r[c])for(o=0;o<r[c].length;o++){var l=r[c][o];s[r[c][o]]=n(l)}s[c]=n(c)}for(o=0;o<i.length;o++)s[i[o]]=n(i[o]);return s}function mu(e,t,n){return n[t]==null?e.props[t]:n[t]}function hu(e,t){return fu(e.children,function(n){return(0,x.cloneElement)(n,{onExited:t.bind(null,n),in:!0,appear:mu(n,`appear`,e),enter:mu(n,`enter`,e),exit:mu(n,`exit`,e)})})}function gu(e,t,n){var r=fu(e.children),i=pu(t,r);return Object.keys(i).forEach(function(a){var o=i[a];if((0,x.isValidElement)(o)){var s=a in t,c=a in r,l=t[a],u=(0,x.isValidElement)(l)&&!l.props.in;c&&(!s||u)?i[a]=(0,x.cloneElement)(o,{onExited:n.bind(null,o),in:!0,exit:mu(o,`exit`,e),enter:mu(o,`enter`,e)}):!c&&s&&!u?i[a]=(0,x.cloneElement)(o,{in:!1}):c&&s&&(0,x.isValidElement)(l)&&(i[a]=(0,x.cloneElement)(o,{onExited:n.bind(null,o),in:l.props.in,exit:mu(o,`exit`,e),enter:mu(o,`enter`,e)}))}}),i}var _u=Object.values||function(e){return Object.keys(e).map(function(t){return e[t]})},vu={component:`div`,childFactory:function(e){return e}},yu=function(e){Ql(t,e);function t(t,n){var r=e.call(this,t,n)||this;return r.state={contextValue:{isMounting:!0},handleExited:r.handleExited.bind(du(r)),firstRender:!0},r}var n=t.prototype;return n.componentDidMount=function(){this.mounted=!0,this.setState({contextValue:{isMounting:!1}})},n.componentWillUnmount=function(){this.mounted=!1},t.getDerivedStateFromProps=function(e,t){var n=t.children,r=t.handleExited;return{children:t.firstRender?hu(e,r):gu(e,n,r),firstRender:!1}},n.handleExited=function(e,t){var n=fu(this.props.children);e.key in n||(e.props.onExited&&e.props.onExited(t),this.mounted&&this.setState(function(t){var n=On({},t.children);return delete n[e.key],{children:n}}))},n.render=function(){var e=this.props,t=e.component,n=e.childFactory,r=Xl(e,[`component`,`childFactory`]),i=this.state.contextValue,a=_u(this.state.children).map(n);return delete r.appear,delete r.enter,delete r.exit,t===null?x.createElement(eu.Provider,{value:i},a):x.createElement(eu.Provider,{value:i},x.createElement(t,r,a))},t}(x.Component);yu.propTypes={},yu.defaultProps=vu;var bu=yu,xu={};function Su(e,t){let n=x.useRef(xu);return n.current===xu&&(n.current=e(t)),n}var Cu=[];function wu(e){x.useEffect(e,Cu)}var Tu=class e{static create(){return new e}currentId=null;start(e,t){this.clear(),this.currentId=setTimeout(()=>{this.currentId=null,t()},e)}clear=()=>{this.currentId!==null&&(clearTimeout(this.currentId),this.currentId=null)};disposeEffect=()=>this.clear};function Eu(){let e=Su(Tu.create).current;return wu(e.disposeEffect),e}const Du=e=>e.scrollTop;function Ou(e,t){let{timeout:n,easing:r,style:i={}}=e;return{duration:i.transitionDuration??(typeof n==`number`?n:n[t.mode]||0),easing:i.transitionTimingFunction??(typeof r==`object`?r[t.mode]:r),delay:i.transitionDelay}}function ku(e){return typeof e==`string`}var Au=ku;function ju(e,t,n){return e===void 0||Au(e)?t:{...t,ownerState:{...t.ownerState,...n}}}var Mu=ju;function Nu(e,t,n){return typeof e==`function`?e(t,n):e}var Pu=Nu;function Fu(e,t=[]){if(e===void 0)return{};let n={};return Object.keys(e).filter(n=>n.match(/^on[A-Z]/)&&typeof e[n]==`function`&&!t.includes(n)).forEach(t=>{n[t]=e[t]}),n}var Iu=Fu;function Lu(e){if(e===void 0)return{};let t={};return Object.keys(e).filter(t=>!(t.match(/^on[A-Z]/)&&typeof e[t]==`function`)).forEach(n=>{t[n]=e[n]}),t}var Ru=Lu;function zu(e){let{getSlotProps:t,additionalProps:n,externalSlotProps:r,externalForwardedProps:i,className:a}=e;if(!t){let e=B(n?.className,a,i?.className,r?.className),t={...n?.style,...i?.style,...r?.style},o={...n,...i,...r};return e.length>0&&(o.className=e),Object.keys(t).length>0&&(o.style=t),{props:o,internalRef:void 0}}let o=Iu({...i,...r}),s=Ru(r),c=Ru(i),l=t(o),u=B(l?.className,n?.className,a,i?.className,r?.className),d={...l?.style,...n?.style,...i?.style,...r?.style},f={...l,...n,...c,...s};return u.length>0&&(f.className=u),Object.keys(d).length>0&&(f.style=d),{props:f,internalRef:l.ref}}var Bu=zu;function Vu(e,t){let{className:n,elementType:r,ownerState:i,externalForwardedProps:a,internalForwardedProps:o,shouldForwardComponentProp:s=!1,...c}=t,{component:l,slots:u={[e]:void 0},slotProps:d={[e]:void 0},...f}=a,p=u[e]||r,m=Pu(d[e],i),{props:{component:h,...g},internalRef:_}=Bu({className:n,...c,externalForwardedProps:e===`root`?f:void 0,externalSlotProps:m}),v=Jl(_,m?.ref,t.ref),y=e===`root`?h||l:h;return[p,Mu(p,{...e===`root`&&!l&&!u[e]&&o,...e!==`root`&&!u[e]&&o,...g,...y&&!s&&{as:y},...y&&s&&{component:y},ref:v},i)]}function Hu(e){return Io(`MuiCollapse`,e)}Lo(`MuiCollapse`,[`root`,`horizontal`,`vertical`,`entered`,`hidden`,`wrapper`,`wrapperInner`]);var Uu=e=>{let{orientation:t,classes:n}=e;return cc({root:[`root`,`${t}`],entered:[`entered`],hidden:[`hidden`],wrapper:[`wrapper`,`${t}`],wrapperInner:[`wrapperInner`,`${t}`]},Hu,n)},Wu=H(`div`,{name:`MuiCollapse`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.orientation],n.state===`entered`&&t.entered,n.state===`exited`&&!n.in&&n.collapsedSize===`0px`&&t.hidden]}})(kl(({theme:e})=>({height:0,overflow:`hidden`,transition:e.transitions.create(`height`),variants:[{props:{orientation:`horizontal`},style:{height:`auto`,width:0,transition:e.transitions.create(`width`)}},{props:{state:`entered`},style:{height:`auto`,overflow:`visible`}},{props:{state:`entered`,orientation:`horizontal`},style:{width:`auto`}},{props:({ownerState:e})=>e.state===`exited`&&!e.in&&e.collapsedSize===`0px`,style:{visibility:`hidden`}}]}))),Gu=H(`div`,{name:`MuiCollapse`,slot:`Wrapper`})({display:`flex`,width:`100%`,variants:[{props:{orientation:`horizontal`},style:{width:`auto`,height:`100%`}}]}),Ku=H(`div`,{name:`MuiCollapse`,slot:`WrapperInner`})({width:`100%`,variants:[{props:{orientation:`horizontal`},style:{width:`auto`,height:`100%`}}]}),qu=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiCollapse`}),{addEndListener:r,children:i,className:a,collapsedSize:o=`0px`,component:s,easing:c,in:l,onEnter:u,onEntered:d,onEntering:f,onExit:p,onExited:m,onExiting:h,orientation:g=`vertical`,slots:_={},slotProps:v={},style:y,timeout:b=Ic.standard,TransitionComponent:S=uu,...C}=n,w={...n,orientation:g,collapsedSize:o},T=Uu(w),E=pl(),D=Eu(),O=x.useRef(null),k=x.useRef(),A=typeof o==`number`?`${o}px`:o,j=g===`horizontal`,M=j?`width`:`height`,N=x.useRef(null),ee=Yl(t,N),P=e=>t=>{if(e){let n=N.current;t===void 0?e(n):e(n,t)}},F=()=>O.current?O.current[j?`clientWidth`:`clientHeight`]:0,I=P((e,t)=>{O.current&&j&&(O.current.style.position=`absolute`),e.style[M]=A,u&&u(e,t)}),te=P((e,t)=>{let n=F();O.current&&j&&(O.current.style.position=``);let{duration:r,easing:i}=Ou({style:y,timeout:b,easing:c},{mode:`enter`});if(b===`auto`){let t=E.transitions.getAutoHeightDuration(n);e.style.transitionDuration=`${t}ms`,k.current=t}else e.style.transitionDuration=typeof r==`string`?r:`${r}ms`;e.style[M]=`${n}px`,e.style.transitionTimingFunction=i,f&&f(e,t)}),ne=P((e,t)=>{e.style[M]=`auto`,d&&d(e,t)}),re=P(e=>{e.style[M]=`${F()}px`,p&&p(e)}),ie=P(m),ae=P(e=>{let t=F(),{duration:n,easing:r}=Ou({style:y,timeout:b,easing:c},{mode:`exit`});if(b===`auto`){let n=E.transitions.getAutoHeightDuration(t);e.style.transitionDuration=`${n}ms`,k.current=n}else e.style.transitionDuration=typeof n==`string`?n:`${n}ms`;e.style[M]=A,e.style.transitionTimingFunction=r,h&&h(e)}),L=e=>{b===`auto`&&D.start(k.current||0,e),r&&r(N.current,e)},oe={slots:_,slotProps:v,component:s},[se,ce]=Vu(`root`,{ref:ee,className:B(T.root,a),elementType:Wu,externalForwardedProps:oe,ownerState:w,additionalProps:{style:{[j?`minWidth`:`minHeight`]:A,...y}}}),[le,ue]=Vu(`wrapper`,{ref:O,className:T.wrapper,elementType:Gu,externalForwardedProps:oe,ownerState:w}),[de,fe]=Vu(`wrapperInner`,{className:T.wrapperInner,elementType:Ku,externalForwardedProps:oe,ownerState:w});return(0,R.jsx)(S,{in:l,onEnter:I,onEntered:ne,onEntering:te,onExit:re,onExited:ie,onExiting:ae,addEndListener:L,nodeRef:N,timeout:b===`auto`?null:b,...C,children:(e,{ownerState:t,...n})=>{let r={...w,state:e};return(0,R.jsx)(se,{...ce,className:B(ce.className,{entered:T.entered,exited:!l&&A===`0px`&&T.hidden}[e]),ownerState:r,...n,children:(0,R.jsx)(le,{...ue,ownerState:r,children:(0,R.jsx)(de,{...fe,ownerState:r,children:i})})})}})});qu&&(qu.muiSupportAuto=!0);var Ju=qu;function Yu(e){return Io(`MuiPaper`,e)}Lo(`MuiPaper`,`root.rounded.outlined.elevation.elevation0.elevation1.elevation2.elevation3.elevation4.elevation5.elevation6.elevation7.elevation8.elevation9.elevation10.elevation11.elevation12.elevation13.elevation14.elevation15.elevation16.elevation17.elevation18.elevation19.elevation20.elevation21.elevation22.elevation23.elevation24`.split(`.`));var Xu=e=>{let{square:t,elevation:n,variant:r,classes:i}=e;return cc({root:[`root`,r,!t&&`rounded`,r===`elevation`&&`elevation${n}`]},Yu,i)},Zu=H(`div`,{name:`MuiPaper`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],!n.square&&t.rounded,n.variant===`elevation`&&t[`elevation${n.elevation}`]]}})(kl(({theme:e})=>({backgroundColor:(e.vars||e).palette.background.paper,color:(e.vars||e).palette.text.primary,transition:e.transitions.create(`box-shadow`),variants:[{props:({ownerState:e})=>!e.square,style:{borderRadius:e.shape.borderRadius}},{props:{variant:`outlined`},style:{border:`1px solid ${(e.vars||e).palette.divider}`}},{props:{variant:`elevation`},style:{boxShadow:`var(--Paper-shadow)`,backgroundImage:`var(--Paper-overlay)`}}]}))),Qu=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiPaper`}),r=pl(),{className:i,component:a=`div`,elevation:o=1,square:s=!1,variant:c=`elevation`,...l}=n,u={...n,component:a,elevation:o,square:s,variant:c};return(0,R.jsx)(Zu,{as:a,ownerState:u,className:B(Xu(u).root,i),ref:t,...l,style:{...c===`elevation`&&{"--Paper-shadow":(r.vars||r).shadows[o],...r.vars&&{"--Paper-overlay":r.vars.overlays?.[o]},...!r.vars&&r.palette.mode===`dark`&&{"--Paper-overlay":`linear-gradient(${ls(`#fff`,Jc(o))}, ${ls(`#fff`,Jc(o))})`}},...l.style}})});function $u(e){try{return e.matches(`:focus-visible`)}catch{}return!1}var ed=class e{static create(){return new e}static use(){let t=Su(e.create).current,[n,r]=x.useState(!1);return t.shouldMount=n,t.setShouldMount=r,x.useEffect(t.mountEffect,[n]),t}constructor(){this.ref={current:null},this.mounted=null,this.didMount=!1,this.shouldMount=!1,this.setShouldMount=null}mount(){return this.mounted||(this.mounted=nd(),this.shouldMount=!0,this.setShouldMount(this.shouldMount)),this.mounted}mountEffect=()=>{this.shouldMount&&!this.didMount&&this.ref.current!==null&&(this.didMount=!0,this.mounted.resolve())};start(...e){this.mount().then(()=>this.ref.current?.start(...e))}stop(...e){this.mount().then(()=>this.ref.current?.stop(...e))}pulsate(...e){this.mount().then(()=>this.ref.current?.pulsate(...e))}};function td(){return ed.use()}function nd(){let e,t,n=new Promise((n,r)=>{e=n,t=r});return n.resolve=e,n.reject=t,n}function rd(e){let{className:t,classes:n,pulsate:r=!1,rippleX:i,rippleY:a,rippleSize:o,in:s,onExited:c,timeout:l}=e,[u,d]=x.useState(!1),f=B(t,n.ripple,n.rippleVisible,r&&n.ripplePulsate),p={width:o,height:o,top:-(o/2)+a,left:-(o/2)+i},m=B(n.child,u&&n.childLeaving,r&&n.childPulsate);return!s&&!u&&d(!0),x.useEffect(()=>{if(!s&&c!=null){let e=setTimeout(c,l);return()=>{clearTimeout(e)}}},[c,s,l]),(0,R.jsx)(`span`,{className:f,style:p,children:(0,R.jsx)(`span`,{className:m})})}var id=rd,ad=Lo(`MuiTouchRipple`,[`root`,`ripple`,`rippleVisible`,`ripplePulsate`,`child`,`childLeaving`,`childPulsate`]),od=550,sd=Di`
  0% {
    transform: scale(0);
    opacity: 0.1;
  }

  100% {
    transform: scale(1);
    opacity: 0.3;
  }
`,cd=Di`
  0% {
    opacity: 1;
  }

  100% {
    opacity: 0;
  }
`,ld=Di`
  0% {
    transform: scale(1);
  }

  50% {
    transform: scale(0.92);
  }

  100% {
    transform: scale(1);
  }
`;const ud=H(`span`,{name:`MuiTouchRipple`,slot:`Root`})({overflow:`hidden`,pointerEvents:`none`,position:`absolute`,zIndex:0,top:0,right:0,bottom:0,left:0,borderRadius:`inherit`}),dd=H(id,{name:`MuiTouchRipple`,slot:`Ripple`})`
  opacity: 0;
  position: absolute;

  &.${ad.rippleVisible} {
    opacity: 0.3;
    transform: scale(1);
    animation-name: ${sd};
    animation-duration: ${od}ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
  }

  &.${ad.ripplePulsate} {
    animation-duration: ${({theme:e})=>e.transitions.duration.shorter}ms;
  }

  & .${ad.child} {
    opacity: 1;
    display: block;
    width: 100%;
    height: 100%;
    border-radius: 50%;
    background-color: currentColor;
  }

  & .${ad.childLeaving} {
    opacity: 0;
    animation-name: ${cd};
    animation-duration: ${od}ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
  }

  & .${ad.childPulsate} {
    position: absolute;
    /* @noflip */
    left: 0px;
    top: 0;
    animation-name: ${ld};
    animation-duration: 2500ms;
    animation-timing-function: ${({theme:e})=>e.transitions.easing.easeInOut};
    animation-iteration-count: infinite;
    animation-delay: 200ms;
  }
`;var fd=x.forwardRef(function(e,t){let{center:n=!1,classes:r={},className:i,...a}=Al({props:e,name:`MuiTouchRipple`}),[o,s]=x.useState([]),c=x.useRef(0),l=x.useRef(null);x.useEffect(()=>{l.current&&=(l.current(),null)},[o]);let u=x.useRef(!1),d=Eu(),f=x.useRef(null),p=x.useRef(null),m=x.useCallback(e=>{let{pulsate:t,rippleX:n,rippleY:i,rippleSize:a,cb:o}=e;s(e=>[...e,(0,R.jsx)(dd,{classes:{ripple:B(r.ripple,ad.ripple),rippleVisible:B(r.rippleVisible,ad.rippleVisible),ripplePulsate:B(r.ripplePulsate,ad.ripplePulsate),child:B(r.child,ad.child),childLeaving:B(r.childLeaving,ad.childLeaving),childPulsate:B(r.childPulsate,ad.childPulsate)},timeout:od,pulsate:t,rippleX:n,rippleY:i,rippleSize:a},c.current)]),c.current+=1,l.current=o},[r]),h=x.useCallback((e={},t={},r=()=>{})=>{let{pulsate:i=!1,center:a=n||t.pulsate,fakeElement:o=!1}=t;if(e?.type===`mousedown`&&u.current){u.current=!1;return}e?.type===`touchstart`&&(u.current=!0);let s=o?null:p.current,c=s?s.getBoundingClientRect():{width:0,height:0,left:0,top:0},l,h,g;if(a||e===void 0||e.clientX===0&&e.clientY===0||!e.clientX&&!e.touches)l=Math.round(c.width/2),h=Math.round(c.height/2);else{let{clientX:t,clientY:n}=e.touches&&e.touches.length>0?e.touches[0]:e;l=Math.round(t-c.left),h=Math.round(n-c.top)}if(a)g=Math.sqrt((2*c.width**2+c.height**2)/3),g%2==0&&(g+=1);else{let e=Math.max(Math.abs((s?s.clientWidth:0)-l),l)*2+2,t=Math.max(Math.abs((s?s.clientHeight:0)-h),h)*2+2;g=Math.sqrt(e**2+t**2)}e?.touches?f.current===null&&(f.current=()=>{m({pulsate:i,rippleX:l,rippleY:h,rippleSize:g,cb:r})},d.start(80,()=>{f.current&&=(f.current(),null)})):m({pulsate:i,rippleX:l,rippleY:h,rippleSize:g,cb:r})},[n,m,d]),g=x.useCallback(()=>{h({},{pulsate:!0})},[h]),_=x.useCallback((e,t)=>{if(d.clear(),e?.type===`touchend`&&f.current){f.current(),f.current=null,d.start(0,()=>{_(e,t)});return}f.current=null,s(e=>e.length>0?e.slice(1):e),l.current=t},[d]);return x.useImperativeHandle(t,()=>({pulsate:g,start:h,stop:_}),[g,h,_]),(0,R.jsx)(ud,{className:B(ad.root,r.root,i),ref:p,...a,children:(0,R.jsx)(bu,{component:null,exit:!0,children:o})})});function pd(e){return Io(`MuiButtonBase`,e)}var md=Lo(`MuiButtonBase`,[`root`,`disabled`,`focusVisible`]),hd=e=>{let{disabled:t,focusVisible:n,focusVisibleClassName:r,classes:i}=e,a=cc({root:[`root`,t&&`disabled`,n&&`focusVisible`]},pd,i);return n&&r&&(a.root+=` ${r}`),a};const gd=H(`button`,{name:`MuiButtonBase`,slot:`Root`})({display:`inline-flex`,alignItems:`center`,justifyContent:`center`,position:`relative`,boxSizing:`border-box`,WebkitTapHighlightColor:`transparent`,backgroundColor:`transparent`,outline:0,border:0,margin:0,borderRadius:0,padding:0,cursor:`pointer`,userSelect:`none`,verticalAlign:`middle`,MozAppearance:`none`,WebkitAppearance:`none`,textDecoration:`none`,color:`inherit`,"&::-moz-focus-inner":{borderStyle:`none`},[`&.${md.disabled}`]:{pointerEvents:`none`,cursor:`default`},"@media print":{colorAdjust:`exact`}});var _d=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiButtonBase`}),{action:r,centerRipple:i=!1,children:a,className:o,component:s=`button`,disabled:c=!1,disableRipple:l=!1,disableTouchRipple:u=!1,focusRipple:d=!1,focusVisibleClassName:f,LinkComponent:p=`a`,onBlur:m,onClick:h,onContextMenu:g,onDragLeave:_,onFocus:v,onFocusVisible:y,onKeyDown:b,onKeyUp:S,onMouseDown:C,onMouseLeave:w,onMouseUp:T,onTouchEnd:E,onTouchMove:D,onTouchStart:O,tabIndex:k=0,TouchRippleProps:A,touchRippleRef:j,type:M,...N}=n,ee=x.useRef(null),P=td(),F=Yl(P.ref,j),[I,te]=x.useState(!1);c&&I&&te(!1),x.useImperativeHandle(r,()=>({focusVisible:()=>{te(!0),ee.current.focus()}}),[]);let ne=P.shouldMount&&!l&&!c;x.useEffect(()=>{I&&d&&!l&&P.pulsate()},[l,d,I,P]);let re=vd(P,`start`,C,u),ie=vd(P,`stop`,g,u),ae=vd(P,`stop`,_,u),L=vd(P,`stop`,T,u),oe=vd(P,`stop`,e=>{I&&e.preventDefault(),w&&w(e)},u),se=vd(P,`start`,O,u),ce=vd(P,`stop`,E,u),le=vd(P,`stop`,D,u),ue=vd(P,`stop`,e=>{$u(e.target)||te(!1),m&&m(e)},!1),de=ql(e=>{ee.current||=e.currentTarget,$u(e.target)&&(te(!0),y&&y(e)),v&&v(e)}),fe=()=>{let e=ee.current;return s&&s!==`button`&&!(e.tagName===`A`&&e.href)},pe=ql(e=>{d&&!e.repeat&&I&&e.key===` `&&P.stop(e,()=>{P.start(e)}),e.target===e.currentTarget&&fe()&&e.key===` `&&e.preventDefault(),b&&b(e),e.target===e.currentTarget&&fe()&&e.key===`Enter`&&!c&&(e.preventDefault(),h&&h(e))}),me=ql(e=>{d&&e.key===` `&&I&&!e.defaultPrevented&&P.stop(e,()=>{P.pulsate(e)}),S&&S(e),h&&e.target===e.currentTarget&&fe()&&e.key===` `&&!e.defaultPrevented&&h(e)}),he=s;he===`button`&&(N.href||N.to)&&(he=p);let ge={};he===`button`?(ge.type=M===void 0?`button`:M,ge.disabled=c):(!N.href&&!N.to&&(ge.role=`button`),c&&(ge[`aria-disabled`]=c));let _e=Yl(t,ee),ve={...n,centerRipple:i,component:s,disabled:c,disableRipple:l,disableTouchRipple:u,focusRipple:d,tabIndex:k,focusVisible:I},ye=hd(ve);return(0,R.jsxs)(gd,{as:he,className:B(ye.root,o),ownerState:ve,onBlur:ue,onClick:h,onContextMenu:ie,onFocus:de,onKeyDown:pe,onKeyUp:me,onMouseDown:re,onMouseLeave:oe,onMouseUp:L,onDragLeave:ae,onTouchEnd:ce,onTouchMove:le,onTouchStart:se,ref:_e,tabIndex:c?-1:k,type:M,...ge,...N,children:[a,ne?(0,R.jsx)(fd,{ref:F,center:i,...A}):null]})});function vd(e,t,n,r=!1){return ql(i=>(n&&n(i),r||e[t](i),!0))}var yd=_d;function bd(e){return typeof e.main==`string`}function xd(e,t=[]){if(!bd(e))return!1;for(let n of t)if(!e.hasOwnProperty(n)||typeof e[n]!=`string`)return!1;return!0}function Sd(e=[]){return([,t])=>t&&xd(t,e)}function Cd(e){return Io(`MuiCircularProgress`,e)}Lo(`MuiCircularProgress`,[`root`,`determinate`,`indeterminate`,`colorPrimary`,`colorSecondary`,`svg`,`track`,`circle`,`circleDeterminate`,`circleIndeterminate`,`circleDisableShrink`]);var wd=44,Td=Di`
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
`,Ed=Di`
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
`,G=typeof Td==`string`?null:Ei`
        animation: ${Td} 1.4s linear infinite;
      `,Dd=typeof Ed==`string`?null:Ei`
        animation: ${Ed} 1.4s ease-in-out infinite;
      `,Od=e=>{let{classes:t,variant:n,color:r,disableShrink:i}=e;return cc({root:[`root`,n,`color${U(r)}`],svg:[`svg`],track:[`track`],circle:[`circle`,`circle${U(n)}`,i&&`circleDisableShrink`]},Cd,t)},kd=H(`span`,{name:`MuiCircularProgress`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`color${U(n.color)}`]]}})(kl(({theme:e})=>({display:`inline-block`,variants:[{props:{variant:`determinate`},style:{transition:e.transitions.create(`transform`)}},{props:{variant:`indeterminate`},style:G||{animation:`${Td} 1.4s linear infinite`}},...Object.entries(e.palette).filter(Sd()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}}))]}))),Ad=H(`svg`,{name:`MuiCircularProgress`,slot:`Svg`})({display:`block`}),jd=H(`circle`,{name:`MuiCircularProgress`,slot:`Circle`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.circle,t[`circle${U(n.variant)}`],n.disableShrink&&t.circleDisableShrink]}})(kl(({theme:e})=>({stroke:`currentColor`,variants:[{props:{variant:`determinate`},style:{transition:e.transitions.create(`stroke-dashoffset`)}},{props:{variant:`indeterminate`},style:{strokeDasharray:`80px, 200px`,strokeDashoffset:0}},{props:({ownerState:e})=>e.variant===`indeterminate`&&!e.disableShrink,style:Dd||{animation:`${Ed} 1.4s ease-in-out infinite`}}]}))),Md=H(`circle`,{name:`MuiCircularProgress`,slot:`Track`})(kl(({theme:e})=>({stroke:`currentColor`,opacity:(e.vars||e).palette.action.activatedOpacity}))),Nd=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiCircularProgress`}),{className:r,color:i=`primary`,disableShrink:a=!1,enableTrackSlot:o=!1,size:s=40,style:c,thickness:l=3.6,value:u=0,variant:d=`indeterminate`,...f}=n,p={...n,color:i,disableShrink:a,size:s,thickness:l,value:u,variant:d,enableTrackSlot:o},m=Od(p),h={},g={},_={};if(d===`determinate`){let e=2*Math.PI*((wd-l)/2);h.strokeDasharray=e.toFixed(3),_[`aria-valuenow`]=Math.round(u),h.strokeDashoffset=`${((100-u)/100*e).toFixed(3)}px`,g.transform=`rotate(-90deg)`}return(0,R.jsx)(kd,{className:B(m.root,r),style:{width:s,height:s,...g,...c},ownerState:p,ref:t,role:`progressbar`,..._,...f,children:(0,R.jsxs)(Ad,{className:m.svg,ownerState:p,viewBox:`${wd/2} ${wd/2} ${wd} ${wd}`,children:[o?(0,R.jsx)(Md,{className:m.track,ownerState:p,cx:wd,cy:wd,r:(wd-l)/2,fill:`none`,strokeWidth:l,"aria-hidden":`true`}):null,(0,R.jsx)(jd,{className:m.circle,style:h,ownerState:p,cx:wd,cy:wd,r:(wd-l)/2,fill:`none`,strokeWidth:l})]})})});function Pd(e){return Io(`MuiIconButton`,e)}var Fd=Lo(`MuiIconButton`,[`root`,`disabled`,`colorInherit`,`colorPrimary`,`colorSecondary`,`colorError`,`colorInfo`,`colorSuccess`,`colorWarning`,`edgeStart`,`edgeEnd`,`sizeSmall`,`sizeMedium`,`sizeLarge`,`loading`,`loadingIndicator`,`loadingWrapper`]),Id=e=>{let{classes:t,disabled:n,color:r,edge:i,size:a,loading:o}=e;return cc({root:[`root`,o&&`loading`,n&&`disabled`,r!==`default`&&`color${U(r)}`,i&&`edge${U(i)}`,`size${U(a)}`],loadingIndicator:[`loadingIndicator`],loadingWrapper:[`loadingWrapper`]},Pd,t)},Ld=H(yd,{name:`MuiIconButton`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.loading&&t.loading,n.color!==`default`&&t[`color${U(n.color)}`],n.edge&&t[`edge${U(n.edge)}`],t[`size${U(n.size)}`]]}})(kl(({theme:e})=>({textAlign:`center`,flex:`0 0 auto`,fontSize:e.typography.pxToRem(24),padding:8,borderRadius:`50%`,color:(e.vars||e).palette.action.active,transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),variants:[{props:e=>!e.disableRipple,style:{"--IconButton-hoverBg":e.alpha((e.vars||e).palette.action.active,(e.vars||e).palette.action.hoverOpacity),"&:hover":{backgroundColor:`var(--IconButton-hoverBg)`,"@media (hover: none)":{backgroundColor:`transparent`}}}},{props:{edge:`start`},style:{marginLeft:-12}},{props:{edge:`start`,size:`small`},style:{marginLeft:-3}},{props:{edge:`end`},style:{marginRight:-12}},{props:{edge:`end`,size:`small`},style:{marginRight:-3}}]})),kl(({theme:e})=>({variants:[{props:{color:`inherit`},style:{color:`inherit`}},...Object.entries(e.palette).filter(Sd()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}})),...Object.entries(e.palette).filter(Sd()).map(([t])=>({props:{color:t},style:{"--IconButton-hoverBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)}})),{props:{size:`small`},style:{padding:5,fontSize:e.typography.pxToRem(18)}},{props:{size:`large`},style:{padding:12,fontSize:e.typography.pxToRem(28)}}],[`&.${Fd.disabled}`]:{backgroundColor:`transparent`,color:(e.vars||e).palette.action.disabled},[`&.${Fd.loading}`]:{color:`transparent`}}))),Rd=H(`span`,{name:`MuiIconButton`,slot:`LoadingIndicator`})(({theme:e})=>({display:`none`,position:`absolute`,visibility:`visible`,top:`50%`,left:`50%`,transform:`translate(-50%, -50%)`,color:(e.vars||e).palette.action.disabled,variants:[{props:{loading:!0},style:{display:`flex`}}]})),zd=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiIconButton`}),{edge:r=!1,children:i,className:a,color:o=`default`,disabled:s=!1,disableFocusRipple:c=!1,size:l=`medium`,id:u,loading:d=null,loadingIndicator:f,...p}=n,m=W(u),h=f??(0,R.jsx)(Nd,{"aria-labelledby":m,color:`inherit`,size:16}),g={...n,edge:r,color:o,disabled:s,disableFocusRipple:c,loading:d,loadingIndicator:h,size:l},_=Id(g);return(0,R.jsxs)(Ld,{id:d?m:u,className:B(_.root,a),centerRipple:!0,focusRipple:!c,disabled:s||d,ref:t,...p,ownerState:g,children:[typeof d==`boolean`&&(0,R.jsx)(`span`,{className:_.loadingWrapper,style:{display:`contents`},children:(0,R.jsx)(Rd,{className:_.loadingIndicator,ownerState:g,children:d&&h})}),i]})});function Bd(e){return Io(`MuiTypography`,e)}var Vd=Lo(`MuiTypography`,[`root`,`h1`,`h2`,`h3`,`h4`,`h5`,`h6`,`subtitle1`,`subtitle2`,`body1`,`body2`,`inherit`,`button`,`caption`,`overline`,`alignLeft`,`alignRight`,`alignCenter`,`alignJustify`,`noWrap`,`gutterBottom`,`paragraph`]),Hd={primary:!0,secondary:!0,error:!0,info:!0,success:!0,warning:!0,textPrimary:!0,textSecondary:!0,textDisabled:!0},Ud=Ol(),Wd=e=>{let{align:t,gutterBottom:n,noWrap:r,paragraph:i,variant:a,classes:o}=e;return cc({root:[`root`,a,e.align!==`inherit`&&`align${U(t)}`,n&&`gutterBottom`,r&&`noWrap`,i&&`paragraph`]},Bd,o)};const Gd=H(`span`,{name:`MuiTypography`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.variant&&t[n.variant],n.align!==`inherit`&&t[`align${U(n.align)}`],n.noWrap&&t.noWrap,n.gutterBottom&&t.gutterBottom,n.paragraph&&t.paragraph]}})(kl(({theme:e})=>({margin:0,variants:[{props:{variant:`inherit`},style:{font:`inherit`,lineHeight:`inherit`,letterSpacing:`inherit`}},...Object.entries(e.typography).filter(([e,t])=>e!==`inherit`&&t&&typeof t==`object`).map(([e,t])=>({props:{variant:e},style:t})),...Object.entries(e.palette).filter(Sd()).map(([t])=>({props:{color:t},style:{color:(e.vars||e).palette[t].main}})),...Object.entries(e.palette?.text||{}).filter(([,e])=>typeof e==`string`).map(([t])=>({props:{color:`text${U(t)}`},style:{color:(e.vars||e).palette.text[t]}})),{props:({ownerState:e})=>e.align!==`inherit`,style:{textAlign:`var(--Typography-textAlign)`}},{props:({ownerState:e})=>e.noWrap,style:{overflow:`hidden`,textOverflow:`ellipsis`,whiteSpace:`nowrap`}},{props:({ownerState:e})=>e.gutterBottom,style:{marginBottom:`0.35em`}},{props:({ownerState:e})=>e.paragraph,style:{marginBottom:16}}]})));var Kd={h1:`h1`,h2:`h2`,h3:`h3`,h4:`h4`,h5:`h5`,h6:`h6`,subtitle1:`h6`,subtitle2:`h6`,body1:`p`,body2:`p`,inherit:`p`},K=x.forwardRef(function(e,t){let{color:n,...r}=Al({props:e,name:`MuiTypography`}),i=!Hd[n],a=Ud({...r,...i&&{color:n}}),{align:o=`inherit`,className:s,component:c,gutterBottom:l=!1,noWrap:u=!1,paragraph:d=!1,variant:f=`body1`,variantMapping:p=Kd,...m}=a,h={...a,align:o,color:n,className:s,component:c,gutterBottom:l,noWrap:u,paragraph:d,variant:f,variantMapping:p};return(0,R.jsx)(Gd,{as:c||(d?`p`:p[f]||Kd[f])||`span`,ref:t,className:B(Wd(h).root,s),...m,ownerState:h,style:{...o!==`inherit`&&{"--Typography-textAlign":o},...m.style}})});function qd(e){return Io(`MuiAppBar`,e)}Lo(`MuiAppBar`,[`root`,`positionFixed`,`positionAbsolute`,`positionSticky`,`positionStatic`,`positionRelative`,`colorDefault`,`colorPrimary`,`colorSecondary`,`colorInherit`,`colorTransparent`,`colorError`,`colorInfo`,`colorSuccess`,`colorWarning`]);var Jd=e=>{let{color:t,position:n,classes:r}=e;return cc({root:[`root`,`color${U(t)}`,`position${U(n)}`]},qd,r)},Yd=(e,t)=>e?`${e?.replace(`)`,``)}, ${t})`:t,Xd=H(Qu,{name:`MuiAppBar`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[`position${U(n.position)}`],t[`color${U(n.color)}`]]}})(kl(({theme:e})=>({display:`flex`,flexDirection:`column`,width:`100%`,boxSizing:`border-box`,flexShrink:0,variants:[{props:{position:`fixed`},style:{position:`fixed`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0,"@media print":{position:`absolute`}}},{props:{position:`absolute`},style:{position:`absolute`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0}},{props:{position:`sticky`},style:{position:`sticky`,zIndex:(e.vars||e).zIndex.appBar,top:0,left:`auto`,right:0}},{props:{position:`static`},style:{position:`static`}},{props:{position:`relative`},style:{position:`relative`}},{props:{color:`inherit`},style:{"--AppBar-color":`inherit`}},{props:{color:`default`},style:{"--AppBar-background":e.vars?e.vars.palette.AppBar.defaultBg:e.palette.grey[100],"--AppBar-color":e.vars?e.vars.palette.text.primary:e.palette.getContrastText(e.palette.grey[100]),...e.applyStyles(`dark`,{"--AppBar-background":e.vars?e.vars.palette.AppBar.defaultBg:e.palette.grey[900],"--AppBar-color":e.vars?e.vars.palette.text.primary:e.palette.getContrastText(e.palette.grey[900])})}},...Object.entries(e.palette).filter(Sd([`contrastText`])).map(([t])=>({props:{color:t},style:{"--AppBar-background":(e.vars??e).palette[t].main,"--AppBar-color":(e.vars??e).palette[t].contrastText}})),{props:e=>e.enableColorOnDark===!0&&![`inherit`,`transparent`].includes(e.color),style:{backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`}},{props:e=>e.enableColorOnDark===!1&&![`inherit`,`transparent`].includes(e.color),style:{backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`,...e.applyStyles(`dark`,{backgroundColor:e.vars?Yd(e.vars.palette.AppBar.darkBg,`var(--AppBar-background)`):null,color:e.vars?Yd(e.vars.palette.AppBar.darkColor,`var(--AppBar-color)`):null})}},{props:{color:`transparent`},style:{"--AppBar-background":`transparent`,"--AppBar-color":`inherit`,backgroundColor:`var(--AppBar-background)`,color:`var(--AppBar-color)`,...e.applyStyles(`dark`,{backgroundImage:`none`})}}]}))),Zd=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiAppBar`}),{className:r,color:i=`primary`,enableColorOnDark:a=!1,position:o=`fixed`,...s}=n,c={...n,color:i,position:o,enableColorOnDark:a};return(0,R.jsx)(Xd,{square:!0,component:`header`,ownerState:c,elevation:4,className:B(Jd(c).root,r,o===`fixed`&&`mui-fixed`),ref:t,...s})}),Qd=`bottom`,$d=`right`,ef=`left`,tf=`auto`,nf=[`top`,Qd,$d,ef],rf=`start`,af=`clippingParents`,of=`viewport`,sf=`popper`,cf=`reference`,lf=nf.reduce(function(e,t){return e.concat([t+`-`+rf,t+`-end`])},[]),uf=[].concat(nf,[tf]).reduce(function(e,t){return e.concat([t,t+`-`+rf,t+`-end`])},[]),df=[`beforeRead`,`read`,`afterRead`,`beforeMain`,`main`,`afterMain`,`beforeWrite`,`write`,`afterWrite`];function ff(e){return e?(e.nodeName||``).toLowerCase():null}function pf(e){if(e==null)return window;if(e.toString()!==`[object Window]`){var t=e.ownerDocument;return t&&t.defaultView||window}return e}function mf(e){return e instanceof pf(e).Element||e instanceof Element}function hf(e){return e instanceof pf(e).HTMLElement||e instanceof HTMLElement}function gf(e){return typeof ShadowRoot>`u`?!1:e instanceof pf(e).ShadowRoot||e instanceof ShadowRoot}function _f(e){var t=e.state;Object.keys(t.elements).forEach(function(e){var n=t.styles[e]||{},r=t.attributes[e]||{},i=t.elements[e];!hf(i)||!ff(i)||(Object.assign(i.style,n),Object.keys(r).forEach(function(e){var t=r[e];t===!1?i.removeAttribute(e):i.setAttribute(e,t===!0?``:t)}))})}function vf(e){var t=e.state,n={popper:{position:t.options.strategy,left:`0`,top:`0`,margin:`0`},arrow:{position:`absolute`},reference:{}};return Object.assign(t.elements.popper.style,n.popper),t.styles=n,t.elements.arrow&&Object.assign(t.elements.arrow.style,n.arrow),function(){Object.keys(t.elements).forEach(function(e){var r=t.elements[e],i=t.attributes[e]||{},a=Object.keys(t.styles.hasOwnProperty(e)?t.styles[e]:n[e]).reduce(function(e,t){return e[t]=``,e},{});!hf(r)||!ff(r)||(Object.assign(r.style,a),Object.keys(i).forEach(function(e){r.removeAttribute(e)}))})}}var yf={name:`applyStyles`,enabled:!0,phase:`write`,fn:_f,effect:vf,requires:[`computeStyles`]};function bf(e){return e.split(`-`)[0]}var xf=Math.max,Sf=Math.min,Cf=Math.round;function wf(){var e=navigator.userAgentData;return e!=null&&e.brands&&Array.isArray(e.brands)?e.brands.map(function(e){return e.brand+`/`+e.version}).join(` `):navigator.userAgent}function Tf(){return!/^((?!chrome|android).)*safari/i.test(wf())}function Ef(e,t,n){t===void 0&&(t=!1),n===void 0&&(n=!1);var r=e.getBoundingClientRect(),i=1,a=1;t&&hf(e)&&(i=e.offsetWidth>0&&Cf(r.width)/e.offsetWidth||1,a=e.offsetHeight>0&&Cf(r.height)/e.offsetHeight||1);var o=(mf(e)?pf(e):window).visualViewport,s=!Tf()&&n,c=(r.left+(s&&o?o.offsetLeft:0))/i,l=(r.top+(s&&o?o.offsetTop:0))/a,u=r.width/i,d=r.height/a;return{width:u,height:d,top:l,right:c+u,bottom:l+d,left:c,x:c,y:l}}function Df(e){var t=Ef(e),n=e.offsetWidth,r=e.offsetHeight;return Math.abs(t.width-n)<=1&&(n=t.width),Math.abs(t.height-r)<=1&&(r=t.height),{x:e.offsetLeft,y:e.offsetTop,width:n,height:r}}function Of(e,t){var n=t.getRootNode&&t.getRootNode();if(e.contains(t))return!0;if(n&&gf(n)){var r=t;do{if(r&&e.isSameNode(r))return!0;r=r.parentNode||r.host}while(r)}return!1}function kf(e){return pf(e).getComputedStyle(e)}function Af(e){return[`table`,`td`,`th`].indexOf(ff(e))>=0}function jf(e){return((mf(e)?e.ownerDocument:e.document)||window.document).documentElement}function Mf(e){return ff(e)===`html`?e:e.assignedSlot||e.parentNode||(gf(e)?e.host:null)||jf(e)}function Nf(e){return!hf(e)||kf(e).position===`fixed`?null:e.offsetParent}function Pf(e){var t=/firefox/i.test(wf());if(/Trident/i.test(wf())&&hf(e)&&kf(e).position===`fixed`)return null;var n=Mf(e);for(gf(n)&&(n=n.host);hf(n)&&[`html`,`body`].indexOf(ff(n))<0;){var r=kf(n);if(r.transform!==`none`||r.perspective!==`none`||r.contain===`paint`||[`transform`,`perspective`].indexOf(r.willChange)!==-1||t&&r.willChange===`filter`||t&&r.filter&&r.filter!==`none`)return n;n=n.parentNode}return null}function Ff(e){for(var t=pf(e),n=Nf(e);n&&Af(n)&&kf(n).position===`static`;)n=Nf(n);return n&&(ff(n)===`html`||ff(n)===`body`&&kf(n).position===`static`)?t:n||Pf(e)||t}function If(e){return[`top`,`bottom`].indexOf(e)>=0?`x`:`y`}function Lf(e,t,n){return xf(e,Sf(t,n))}function Rf(e,t,n){var r=Lf(e,t,n);return r>n?n:r}function zf(){return{top:0,right:0,bottom:0,left:0}}function Bf(e){return Object.assign({},zf(),e)}function Vf(e,t){return t.reduce(function(t,n){return t[n]=e,t},{})}var Hf=function(e,t){return e=typeof e==`function`?e(Object.assign({},t.rects,{placement:t.placement})):e,Bf(typeof e==`number`?Vf(e,nf):e)};function Uf(e){var t,n=e.state,r=e.name,i=e.options,a=n.elements.arrow,o=n.modifiersData.popperOffsets,s=bf(n.placement),c=If(s),l=[`left`,`right`].indexOf(s)>=0?`height`:`width`;if(!(!a||!o)){var u=Hf(i.padding,n),d=Df(a),f=c===`y`?`top`:ef,p=c===`y`?Qd:$d,m=n.rects.reference[l]+n.rects.reference[c]-o[c]-n.rects.popper[l],h=o[c]-n.rects.reference[c],g=Ff(a),_=g?c===`y`?g.clientHeight||0:g.clientWidth||0:0,v=m/2-h/2,y=u[f],b=_-d[l]-u[p],x=_/2-d[l]/2+v,S=Lf(y,x,b),C=c;n.modifiersData[r]=(t={},t[C]=S,t.centerOffset=S-x,t)}}function Wf(e){var t=e.state,n=e.options.element,r=n===void 0?`[data-popper-arrow]`:n;r!=null&&(typeof r==`string`&&(r=t.elements.popper.querySelector(r),!r)||Of(t.elements.popper,r)&&(t.elements.arrow=r))}var Gf={name:`arrow`,enabled:!0,phase:`main`,fn:Uf,effect:Wf,requires:[`popperOffsets`],requiresIfExists:[`preventOverflow`]};function Kf(e){return e.split(`-`)[1]}var qf={top:`auto`,right:`auto`,bottom:`auto`,left:`auto`};function Jf(e,t){var n=e.x,r=e.y,i=t.devicePixelRatio||1;return{x:Cf(n*i)/i||0,y:Cf(r*i)/i||0}}function Yf(e){var t,n=e.popper,r=e.popperRect,i=e.placement,a=e.variation,o=e.offsets,s=e.position,c=e.gpuAcceleration,l=e.adaptive,u=e.roundOffsets,d=e.isFixed,f=o.x,p=f===void 0?0:f,m=o.y,h=m===void 0?0:m,g=typeof u==`function`?u({x:p,y:h}):{x:p,y:h};p=g.x,h=g.y;var _=o.hasOwnProperty(`x`),v=o.hasOwnProperty(`y`),y=ef,b=`top`,x=window;if(l){var S=Ff(n),C=`clientHeight`,w=`clientWidth`;if(S===pf(n)&&(S=jf(n),kf(S).position!==`static`&&s===`absolute`&&(C=`scrollHeight`,w=`scrollWidth`)),S=S,i===`top`||(i===`left`||i===`right`)&&a===`end`){b=Qd;var T=d&&S===x&&x.visualViewport?x.visualViewport.height:S[C];h-=T-r.height,h*=c?1:-1}if(i===`left`||(i===`top`||i===`bottom`)&&a===`end`){y=$d;var E=d&&S===x&&x.visualViewport?x.visualViewport.width:S[w];p-=E-r.width,p*=c?1:-1}}var D=Object.assign({position:s},l&&qf),O=u===!0?Jf({x:p,y:h},pf(n)):{x:p,y:h};if(p=O.x,h=O.y,c){var k;return Object.assign({},D,(k={},k[b]=v?`0`:``,k[y]=_?`0`:``,k.transform=(x.devicePixelRatio||1)<=1?`translate(`+p+`px, `+h+`px)`:`translate3d(`+p+`px, `+h+`px, 0)`,k))}return Object.assign({},D,(t={},t[b]=v?h+`px`:``,t[y]=_?p+`px`:``,t.transform=``,t))}function Xf(e){var t=e.state,n=e.options,r=n.gpuAcceleration,i=r===void 0?!0:r,a=n.adaptive,o=a===void 0?!0:a,s=n.roundOffsets,c=s===void 0?!0:s,l={placement:bf(t.placement),variation:Kf(t.placement),popper:t.elements.popper,popperRect:t.rects.popper,gpuAcceleration:i,isFixed:t.options.strategy===`fixed`};t.modifiersData.popperOffsets!=null&&(t.styles.popper=Object.assign({},t.styles.popper,Yf(Object.assign({},l,{offsets:t.modifiersData.popperOffsets,position:t.options.strategy,adaptive:o,roundOffsets:c})))),t.modifiersData.arrow!=null&&(t.styles.arrow=Object.assign({},t.styles.arrow,Yf(Object.assign({},l,{offsets:t.modifiersData.arrow,position:`absolute`,adaptive:!1,roundOffsets:c})))),t.attributes.popper=Object.assign({},t.attributes.popper,{"data-popper-placement":t.placement})}var Zf={name:`computeStyles`,enabled:!0,phase:`beforeWrite`,fn:Xf,data:{}},Qf={passive:!0};function $f(e){var t=e.state,n=e.instance,r=e.options,i=r.scroll,a=i===void 0?!0:i,o=r.resize,s=o===void 0?!0:o,c=pf(t.elements.popper),l=[].concat(t.scrollParents.reference,t.scrollParents.popper);return a&&l.forEach(function(e){e.addEventListener(`scroll`,n.update,Qf)}),s&&c.addEventListener(`resize`,n.update,Qf),function(){a&&l.forEach(function(e){e.removeEventListener(`scroll`,n.update,Qf)}),s&&c.removeEventListener(`resize`,n.update,Qf)}}var ep={name:`eventListeners`,enabled:!0,phase:`write`,fn:function(){},effect:$f,data:{}},tp={left:`right`,right:`left`,bottom:`top`,top:`bottom`};function np(e){return e.replace(/left|right|bottom|top/g,function(e){return tp[e]})}var rp={start:`end`,end:`start`};function ip(e){return e.replace(/start|end/g,function(e){return rp[e]})}function ap(e){var t=pf(e);return{scrollLeft:t.pageXOffset,scrollTop:t.pageYOffset}}function op(e){return Ef(jf(e)).left+ap(e).scrollLeft}function sp(e,t){var n=pf(e),r=jf(e),i=n.visualViewport,a=r.clientWidth,o=r.clientHeight,s=0,c=0;if(i){a=i.width,o=i.height;var l=Tf();(l||!l&&t===`fixed`)&&(s=i.offsetLeft,c=i.offsetTop)}return{width:a,height:o,x:s+op(e),y:c}}function cp(e){var t=jf(e),n=ap(e),r=e.ownerDocument?.body,i=xf(t.scrollWidth,t.clientWidth,r?r.scrollWidth:0,r?r.clientWidth:0),a=xf(t.scrollHeight,t.clientHeight,r?r.scrollHeight:0,r?r.clientHeight:0),o=-n.scrollLeft+op(e),s=-n.scrollTop;return kf(r||t).direction===`rtl`&&(o+=xf(t.clientWidth,r?r.clientWidth:0)-i),{width:i,height:a,x:o,y:s}}function lp(e){var t=kf(e),n=t.overflow,r=t.overflowX,i=t.overflowY;return/auto|scroll|overlay|hidden/.test(n+i+r)}function up(e){return[`html`,`body`,`#document`].indexOf(ff(e))>=0?e.ownerDocument.body:hf(e)&&lp(e)?e:up(Mf(e))}function dp(e,t){t===void 0&&(t=[]);var n=up(e),r=n===e.ownerDocument?.body,i=pf(n),a=r?[i].concat(i.visualViewport||[],lp(n)?n:[]):n,o=t.concat(a);return r?o:o.concat(dp(Mf(a)))}function fp(e){return Object.assign({},e,{left:e.x,top:e.y,right:e.x+e.width,bottom:e.y+e.height})}function pp(e,t){var n=Ef(e,!1,t===`fixed`);return n.top+=e.clientTop,n.left+=e.clientLeft,n.bottom=n.top+e.clientHeight,n.right=n.left+e.clientWidth,n.width=e.clientWidth,n.height=e.clientHeight,n.x=n.left,n.y=n.top,n}function mp(e,t,n){return t===`viewport`?fp(sp(e,n)):mf(t)?pp(t,n):fp(cp(jf(e)))}function hp(e){var t=dp(Mf(e)),n=[`absolute`,`fixed`].indexOf(kf(e).position)>=0&&hf(e)?Ff(e):e;return mf(n)?t.filter(function(e){return mf(e)&&Of(e,n)&&ff(e)!==`body`}):[]}function gp(e,t,n,r){var i=t===`clippingParents`?hp(e):[].concat(t),a=[].concat(i,[n]),o=a[0],s=a.reduce(function(t,n){var i=mp(e,n,r);return t.top=xf(i.top,t.top),t.right=Sf(i.right,t.right),t.bottom=Sf(i.bottom,t.bottom),t.left=xf(i.left,t.left),t},mp(e,o,r));return s.width=s.right-s.left,s.height=s.bottom-s.top,s.x=s.left,s.y=s.top,s}function _p(e){var t=e.reference,n=e.element,r=e.placement,i=r?bf(r):null,a=r?Kf(r):null,o=t.x+t.width/2-n.width/2,s=t.y+t.height/2-n.height/2,c;switch(i){case`top`:c={x:o,y:t.y-n.height};break;case Qd:c={x:o,y:t.y+t.height};break;case $d:c={x:t.x+t.width,y:s};break;case ef:c={x:t.x-n.width,y:s};break;default:c={x:t.x,y:t.y}}var l=i?If(i):null;if(l!=null){var u=l===`y`?`height`:`width`;switch(a){case rf:c[l]=c[l]-(t[u]/2-n[u]/2);break;case`end`:c[l]=c[l]+(t[u]/2-n[u]/2);break;default:}}return c}function vp(e,t){t===void 0&&(t={});var n=t,r=n.placement,i=r===void 0?e.placement:r,a=n.strategy,o=a===void 0?e.strategy:a,s=n.boundary,c=s===void 0?af:s,l=n.rootBoundary,u=l===void 0?of:l,d=n.elementContext,f=d===void 0?sf:d,p=n.altBoundary,m=p===void 0?!1:p,h=n.padding,g=h===void 0?0:h,_=Bf(typeof g==`number`?Vf(g,nf):g),v=f===`popper`?cf:sf,y=e.rects.popper,b=e.elements[m?v:f],x=gp(mf(b)?b:b.contextElement||jf(e.elements.popper),c,u,o),S=Ef(e.elements.reference),C=_p({reference:S,element:y,strategy:`absolute`,placement:i}),w=fp(Object.assign({},y,C)),T=f===`popper`?w:S,E={top:x.top-T.top+_.top,bottom:T.bottom-x.bottom+_.bottom,left:x.left-T.left+_.left,right:T.right-x.right+_.right},D=e.modifiersData.offset;if(f===`popper`&&D){var O=D[i];Object.keys(E).forEach(function(e){var t=[`right`,`bottom`].indexOf(e)>=0?1:-1,n=[`top`,`bottom`].indexOf(e)>=0?`y`:`x`;E[e]+=O[n]*t})}return E}function yp(e,t){t===void 0&&(t={});var n=t,r=n.placement,i=n.boundary,a=n.rootBoundary,o=n.padding,s=n.flipVariations,c=n.allowedAutoPlacements,l=c===void 0?uf:c,u=Kf(r),d=u?s?lf:lf.filter(function(e){return Kf(e)===u}):nf,f=d.filter(function(e){return l.indexOf(e)>=0});f.length===0&&(f=d);var p=f.reduce(function(t,n){return t[n]=vp(e,{placement:n,boundary:i,rootBoundary:a,padding:o})[bf(n)],t},{});return Object.keys(p).sort(function(e,t){return p[e]-p[t]})}function bp(e){if(bf(e)===`auto`)return[];var t=np(e);return[ip(e),t,ip(t)]}function xp(e){var t=e.state,n=e.options,r=e.name;if(!t.modifiersData[r]._skip){for(var i=n.mainAxis,a=i===void 0?!0:i,o=n.altAxis,s=o===void 0?!0:o,c=n.fallbackPlacements,l=n.padding,u=n.boundary,d=n.rootBoundary,f=n.altBoundary,p=n.flipVariations,m=p===void 0?!0:p,h=n.allowedAutoPlacements,g=t.options.placement,_=bf(g)===g,v=c||(_||!m?[np(g)]:bp(g)),y=[g].concat(v).reduce(function(e,n){return e.concat(bf(n)===`auto`?yp(t,{placement:n,boundary:u,rootBoundary:d,padding:l,flipVariations:m,allowedAutoPlacements:h}):n)},[]),b=t.rects.reference,x=t.rects.popper,S=new Map,C=!0,w=y[0],T=0;T<y.length;T++){var E=y[T],D=bf(E),O=Kf(E)===rf,k=[`top`,Qd].indexOf(D)>=0,A=k?`width`:`height`,j=vp(t,{placement:E,boundary:u,rootBoundary:d,altBoundary:f,padding:l}),M=k?O?$d:ef:O?Qd:`top`;b[A]>x[A]&&(M=np(M));var N=np(M),ee=[];if(a&&ee.push(j[D]<=0),s&&ee.push(j[M]<=0,j[N]<=0),ee.every(function(e){return e})){w=E,C=!1;break}S.set(E,ee)}if(C)for(var P=m?3:1,F=function(e){var t=y.find(function(t){var n=S.get(t);if(n)return n.slice(0,e).every(function(e){return e})});if(t)return w=t,`break`},I=P;I>0&&F(I)!==`break`;I--);t.placement!==w&&(t.modifiersData[r]._skip=!0,t.placement=w,t.reset=!0)}}var Sp={name:`flip`,enabled:!0,phase:`main`,fn:xp,requiresIfExists:[`offset`],data:{_skip:!1}};function Cp(e,t,n){return n===void 0&&(n={x:0,y:0}),{top:e.top-t.height-n.y,right:e.right-t.width+n.x,bottom:e.bottom-t.height+n.y,left:e.left-t.width-n.x}}function wp(e){return[`top`,$d,Qd,ef].some(function(t){return e[t]>=0})}function Tp(e){var t=e.state,n=e.name,r=t.rects.reference,i=t.rects.popper,a=t.modifiersData.preventOverflow,o=vp(t,{elementContext:`reference`}),s=vp(t,{altBoundary:!0}),c=Cp(o,r),l=Cp(s,i,a),u=wp(c),d=wp(l);t.modifiersData[n]={referenceClippingOffsets:c,popperEscapeOffsets:l,isReferenceHidden:u,hasPopperEscaped:d},t.attributes.popper=Object.assign({},t.attributes.popper,{"data-popper-reference-hidden":u,"data-popper-escaped":d})}var Ep={name:`hide`,enabled:!0,phase:`main`,requiresIfExists:[`preventOverflow`],fn:Tp};function Dp(e,t,n){var r=bf(e),i=[`left`,`top`].indexOf(r)>=0?-1:1,a=typeof n==`function`?n(Object.assign({},t,{placement:e})):n,o=a[0],s=a[1];return o||=0,s=(s||0)*i,[`left`,`right`].indexOf(r)>=0?{x:s,y:o}:{x:o,y:s}}function Op(e){var t=e.state,n=e.options,r=e.name,i=n.offset,a=i===void 0?[0,0]:i,o=uf.reduce(function(e,n){return e[n]=Dp(n,t.rects,a),e},{}),s=o[t.placement],c=s.x,l=s.y;t.modifiersData.popperOffsets!=null&&(t.modifiersData.popperOffsets.x+=c,t.modifiersData.popperOffsets.y+=l),t.modifiersData[r]=o}var kp={name:`offset`,enabled:!0,phase:`main`,requires:[`popperOffsets`],fn:Op};function Ap(e){var t=e.state,n=e.name;t.modifiersData[n]=_p({reference:t.rects.reference,element:t.rects.popper,strategy:`absolute`,placement:t.placement})}var jp={name:`popperOffsets`,enabled:!0,phase:`read`,fn:Ap,data:{}};function Mp(e){return e===`x`?`y`:`x`}function Np(e){var t=e.state,n=e.options,r=e.name,i=n.mainAxis,a=i===void 0?!0:i,o=n.altAxis,s=o===void 0?!1:o,c=n.boundary,l=n.rootBoundary,u=n.altBoundary,d=n.padding,f=n.tether,p=f===void 0?!0:f,m=n.tetherOffset,h=m===void 0?0:m,g=vp(t,{boundary:c,rootBoundary:l,padding:d,altBoundary:u}),_=bf(t.placement),v=Kf(t.placement),y=!v,b=If(_),x=Mp(b),S=t.modifiersData.popperOffsets,C=t.rects.reference,w=t.rects.popper,T=typeof h==`function`?h(Object.assign({},t.rects,{placement:t.placement})):h,E=typeof T==`number`?{mainAxis:T,altAxis:T}:Object.assign({mainAxis:0,altAxis:0},T),D=t.modifiersData.offset?t.modifiersData.offset[t.placement]:null,O={x:0,y:0};if(S){if(a){var k=b===`y`?`top`:ef,A=b===`y`?Qd:$d,j=b===`y`?`height`:`width`,M=S[b],N=M+g[k],ee=M-g[A],P=p?-w[j]/2:0,F=v===`start`?C[j]:w[j],I=v===`start`?-w[j]:-C[j],te=t.elements.arrow,ne=p&&te?Df(te):{width:0,height:0},re=t.modifiersData[`arrow#persistent`]?t.modifiersData[`arrow#persistent`].padding:zf(),ie=re[k],ae=re[A],L=Lf(0,C[j],ne[j]),oe=y?C[j]/2-P-L-ie-E.mainAxis:F-L-ie-E.mainAxis,se=y?-C[j]/2+P+L+ae+E.mainAxis:I+L+ae+E.mainAxis,ce=t.elements.arrow&&Ff(t.elements.arrow),le=ce?b===`y`?ce.clientTop||0:ce.clientLeft||0:0,ue=D?.[b]??0,de=M+oe-ue-le,fe=M+se-ue,pe=Lf(p?Sf(N,de):N,M,p?xf(ee,fe):ee);S[b]=pe,O[b]=pe-M}if(s){var me=b===`x`?`top`:ef,he=b===`x`?Qd:$d,ge=S[x],_e=x===`y`?`height`:`width`,ve=ge+g[me],ye=ge-g[he],be=[`top`,ef].indexOf(_)!==-1,xe=D?.[x]??0,Se=be?ve:ge-C[_e]-w[_e]-xe+E.altAxis,Ce=be?ge+C[_e]+w[_e]-xe-E.altAxis:ye,we=p&&be?Rf(Se,ge,Ce):Lf(p?Se:ve,ge,p?Ce:ye);S[x]=we,O[x]=we-ge}t.modifiersData[r]=O}}var Pp={name:`preventOverflow`,enabled:!0,phase:`main`,fn:Np,requiresIfExists:[`offset`]};function Fp(e){return{scrollLeft:e.scrollLeft,scrollTop:e.scrollTop}}function Ip(e){return e===pf(e)||!hf(e)?ap(e):Fp(e)}function Lp(e){var t=e.getBoundingClientRect(),n=Cf(t.width)/e.offsetWidth||1,r=Cf(t.height)/e.offsetHeight||1;return n!==1||r!==1}function Rp(e,t,n){n===void 0&&(n=!1);var r=hf(t),i=hf(t)&&Lp(t),a=jf(t),o=Ef(e,i,n),s={scrollLeft:0,scrollTop:0},c={x:0,y:0};return(r||!r&&!n)&&((ff(t)!==`body`||lp(a))&&(s=Ip(t)),hf(t)?(c=Ef(t,!0),c.x+=t.clientLeft,c.y+=t.clientTop):a&&(c.x=op(a))),{x:o.left+s.scrollLeft-c.x,y:o.top+s.scrollTop-c.y,width:o.width,height:o.height}}function zp(e){var t=new Map,n=new Set,r=[];e.forEach(function(e){t.set(e.name,e)});function i(e){n.add(e.name),[].concat(e.requires||[],e.requiresIfExists||[]).forEach(function(e){if(!n.has(e)){var r=t.get(e);r&&i(r)}}),r.push(e)}return e.forEach(function(e){n.has(e.name)||i(e)}),r}function Bp(e){var t=zp(e);return df.reduce(function(e,n){return e.concat(t.filter(function(e){return e.phase===n}))},[])}function Vp(e){var t;return function(){return t||=new Promise(function(n){Promise.resolve().then(function(){t=void 0,n(e())})}),t}}function Hp(e){var t=e.reduce(function(e,t){var n=e[t.name];return e[t.name]=n?Object.assign({},n,t,{options:Object.assign({},n.options,t.options),data:Object.assign({},n.data,t.data)}):t,e},{});return Object.keys(t).map(function(e){return t[e]})}var Up={placement:`bottom`,modifiers:[],strategy:`absolute`};function Wp(){return![...arguments].some(function(e){return!(e&&typeof e.getBoundingClientRect==`function`)})}function Gp(e){e===void 0&&(e={});var t=e,n=t.defaultModifiers,r=n===void 0?[]:n,i=t.defaultOptions,a=i===void 0?Up:i;return function(e,t,n){n===void 0&&(n=a);var i={placement:`bottom`,orderedModifiers:[],options:Object.assign({},Up,a),modifiersData:{},elements:{reference:e,popper:t},attributes:{},styles:{}},o=[],s=!1,c={state:i,setOptions:function(n){var o=typeof n==`function`?n(i.options):n;return u(),i.options=Object.assign({},a,i.options,o),i.scrollParents={reference:mf(e)?dp(e):e.contextElement?dp(e.contextElement):[],popper:dp(t)},i.orderedModifiers=Bp(Hp([].concat(r,i.options.modifiers))).filter(function(e){return e.enabled}),l(),c.update()},forceUpdate:function(){if(!s){var e=i.elements,t=e.reference,n=e.popper;if(Wp(t,n)){i.rects={reference:Rp(t,Ff(n),i.options.strategy===`fixed`),popper:Df(n)},i.reset=!1,i.placement=i.options.placement,i.orderedModifiers.forEach(function(e){return i.modifiersData[e.name]=Object.assign({},e.data)});for(var r=0;r<i.orderedModifiers.length;r++){if(i.reset===!0){i.reset=!1,r=-1;continue}var a=i.orderedModifiers[r],o=a.fn,l=a.options,u=l===void 0?{}:l,d=a.name;typeof o==`function`&&(i=o({state:i,options:u,name:d,instance:c})||i)}}}},update:Vp(function(){return new Promise(function(e){c.forceUpdate(),e(i)})}),destroy:function(){u(),s=!0}};if(!Wp(e,t))return c;c.setOptions(n).then(function(e){!s&&n.onFirstUpdate&&n.onFirstUpdate(e)});function l(){i.orderedModifiers.forEach(function(e){var t=e.name,n=e.options,r=n===void 0?{}:n,a=e.effect;if(typeof a==`function`){var s=a({state:i,name:t,instance:c,options:r});o.push(s||function(){})}})}function u(){o.forEach(function(e){return e()}),o=[]}return c}}var Kp=Gp({defaultModifiers:[ep,jp,Zf,yf,kp,Sp,Pp,Gf,Ep]});function qp(e){let{elementType:t,externalSlotProps:n,ownerState:r,skipResolvingSlotProps:i=!1,...a}=e,o=i?{}:Pu(n,r),{props:s,internalRef:c}=Bu({...a,externalSlotProps:o}),l=Jl(c,o?.ref,e.additionalProps?.ref);return Mu(t,{...s,ref:l},r)}var Jp=qp;function Yp(e){return e?.props?.ref||null}var Xp=c(m());function Zp(e){return typeof e==`function`?e():e}var Qp=x.forwardRef(function(e,t){let{children:n,container:r,disablePortal:i=!1}=e,[a,o]=x.useState(null),s=Jl(x.isValidElement(n)?Yp(n):null,t);if(Zo(()=>{i||o(Zp(r)||document.body)},[r,i]),Zo(()=>{if(a&&!i)return Vl(t,a),()=>{Vl(t,null)}},[t,a,i]),i){if(x.isValidElement(n)){let e={ref:s};return x.cloneElement(n,e)}return n}return a&&Xp.createPortal(n,a)});function $p(e){return Io(`MuiPopper`,e)}Lo(`MuiPopper`,[`root`]);function em(e,t){if(t===`ltr`)return e;switch(e){case`bottom-end`:return`bottom-start`;case`bottom-start`:return`bottom-end`;case`top-end`:return`top-start`;case`top-start`:return`top-end`;default:return e}}function tm(e){return typeof e==`function`?e():e}function nm(e){return e.nodeType!==void 0}var rm=e=>{let{classes:t}=e;return cc({root:[`root`]},$p,t)},im={},am=x.forwardRef(function(e,t){let{anchorEl:n,children:r,direction:i,disablePortal:a,modifiers:o,open:s,placement:c,popperOptions:l,popperRef:u,slotProps:d={},slots:f={},TransitionProps:p,ownerState:m,...h}=e,g=x.useRef(null),_=Jl(g,t),v=x.useRef(null),y=Jl(v,u),b=x.useRef(y);Zo(()=>{b.current=y},[y]),x.useImperativeHandle(u,()=>v.current,[]);let S=em(c,i),[C,w]=x.useState(S),[T,E]=x.useState(tm(n));x.useEffect(()=>{v.current&&v.current.forceUpdate()}),x.useEffect(()=>{n&&E(tm(n))},[n]),Zo(()=>{if(!T||!s)return;let e=e=>{w(e.placement)},t=[{name:`preventOverflow`,options:{altBoundary:a}},{name:`flip`,options:{altBoundary:a}},{name:`onUpdate`,enabled:!0,phase:`afterWrite`,fn:({state:t})=>{e(t)}}];o!=null&&(t=t.concat(o)),l&&l.modifiers!=null&&(t=t.concat(l.modifiers));let n=Kp(T,g.current,{placement:S,...l,modifiers:t});return b.current(n),()=>{n.destroy(),b.current(null)}},[T,a,o,s,l,S]);let D={placement:C};p!==null&&(D.TransitionProps=p);let O=rm(e),k=f.root??`div`;return(0,R.jsx)(k,{...Jp({elementType:k,externalSlotProps:d.root,externalForwardedProps:h,additionalProps:{role:`tooltip`,ref:_},ownerState:e,className:O.root}),children:typeof r==`function`?r(D):r})}),om=H(x.forwardRef(function(e,t){let{anchorEl:n,children:r,container:i,direction:a=`ltr`,disablePortal:o=!1,keepMounted:s=!1,modifiers:c,open:l,placement:u=`bottom`,popperOptions:d=im,popperRef:f,style:p,transition:m=!1,slotProps:h={},slots:g={},..._}=e,[v,y]=x.useState(!0),b=()=>{y(!1)},S=()=>{y(!0)};if(!s&&!l&&(!m||v))return null;let C;if(i)C=i;else if(n){let e=tm(n);C=e&&nm(e)?zl(e).body:zl(null).body}let w=!l&&s&&(!m||v)?`none`:void 0,T=m?{in:l,onEnter:b,onExited:S}:void 0;return(0,R.jsx)(Qp,{disablePortal:o,container:C,children:(0,R.jsx)(am,{anchorEl:n,direction:a,disablePortal:o,modifiers:c,ref:t,open:m?!v:l,placement:u,popperOptions:d,popperRef:f,slotProps:h,slots:g,..._,style:{position:`fixed`,top:0,left:0,display:w,...p},TransitionProps:T,children:r})})}),{name:`MuiPopper`,slot:`Root`})({}),sm=x.forwardRef(function(e,t){let n=Ts(),{anchorEl:r,component:i,components:a,componentsProps:o,container:s,disablePortal:c,keepMounted:l,modifiers:u,open:d,placement:f,popperOptions:p,popperRef:m,transition:h,slots:g,slotProps:_,...v}=Al({props:e,name:`MuiPopper`}),y=g?.root??a?.Root,b={anchorEl:r,container:s,disablePortal:c,keepMounted:l,modifiers:u,open:d,placement:f,popperOptions:p,popperRef:m,transition:h,...v};return(0,R.jsx)(om,{as:i,direction:n?`rtl`:`ltr`,slots:{root:y},slotProps:_??o,...b,ref:t})}),cm=Il((0,R.jsx)(`path`,{d:`M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z`}),`Cancel`);function lm(e){return Io(`MuiChip`,e)}var um=Lo(`MuiChip`,`root.sizeSmall.sizeMedium.colorDefault.colorError.colorInfo.colorPrimary.colorSecondary.colorSuccess.colorWarning.disabled.clickable.clickableColorPrimary.clickableColorSecondary.deletable.deletableColorPrimary.deletableColorSecondary.outlined.filled.outlinedPrimary.outlinedSecondary.filledPrimary.filledSecondary.avatar.avatarSmall.avatarMedium.avatarColorPrimary.avatarColorSecondary.icon.iconSmall.iconMedium.iconColorPrimary.iconColorSecondary.label.labelSmall.labelMedium.deleteIcon.deleteIconSmall.deleteIconMedium.deleteIconColorPrimary.deleteIconColorSecondary.deleteIconOutlinedColorPrimary.deleteIconOutlinedColorSecondary.deleteIconFilledColorPrimary.deleteIconFilledColorSecondary.focusVisible`.split(`.`)),dm=e=>{let{classes:t,disabled:n,size:r,color:i,iconColor:a,onDelete:o,clickable:s,variant:c}=e;return cc({root:[`root`,c,n&&`disabled`,`size${U(r)}`,`color${U(i)}`,s&&`clickable`,s&&`clickableColor${U(i)}`,o&&`deletable`,o&&`deletableColor${U(i)}`,`${c}${U(i)}`],label:[`label`,`label${U(r)}`],avatar:[`avatar`,`avatar${U(r)}`,`avatarColor${U(i)}`],icon:[`icon`,`icon${U(r)}`,`iconColor${U(a)}`],deleteIcon:[`deleteIcon`,`deleteIcon${U(r)}`,`deleteIconColor${U(i)}`,`deleteIcon${U(c)}Color${U(i)}`]},lm,t)},fm=H(`div`,{name:`MuiChip`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e,{color:r,iconColor:i,clickable:a,onDelete:o,size:s,variant:c}=n;return[{[`& .${um.avatar}`]:t.avatar},{[`& .${um.avatar}`]:t[`avatar${U(s)}`]},{[`& .${um.avatar}`]:t[`avatarColor${U(r)}`]},{[`& .${um.icon}`]:t.icon},{[`& .${um.icon}`]:t[`icon${U(s)}`]},{[`& .${um.icon}`]:t[`iconColor${U(i)}`]},{[`& .${um.deleteIcon}`]:t.deleteIcon},{[`& .${um.deleteIcon}`]:t[`deleteIcon${U(s)}`]},{[`& .${um.deleteIcon}`]:t[`deleteIconColor${U(r)}`]},{[`& .${um.deleteIcon}`]:t[`deleteIcon${U(c)}Color${U(r)}`]},t.root,t[`size${U(s)}`],t[`color${U(r)}`],a&&t.clickable,a&&r!==`default`&&t[`clickableColor${U(r)})`],o&&t.deletable,o&&r!==`default`&&t[`deletableColor${U(r)}`],t[c],t[`${c}${U(r)}`]]}})(kl(({theme:e})=>{let t=e.palette.mode===`light`?e.palette.grey[700]:e.palette.grey[300];return{maxWidth:`100%`,fontFamily:e.typography.fontFamily,fontSize:e.typography.pxToRem(13),display:`inline-flex`,alignItems:`center`,justifyContent:`center`,height:32,lineHeight:1.5,color:(e.vars||e).palette.text.primary,backgroundColor:(e.vars||e).palette.action.selected,borderRadius:32/2,whiteSpace:`nowrap`,transition:e.transitions.create([`background-color`,`box-shadow`]),cursor:`unset`,outline:0,textDecoration:`none`,border:0,padding:0,verticalAlign:`middle`,boxSizing:`border-box`,[`&.${um.disabled}`]:{opacity:(e.vars||e).palette.action.disabledOpacity,pointerEvents:`none`},[`& .${um.avatar}`]:{marginLeft:5,marginRight:-6,width:24,height:24,color:e.vars?e.vars.palette.Chip.defaultAvatarColor:t,fontSize:e.typography.pxToRem(12)},[`& .${um.avatarColorPrimary}`]:{color:(e.vars||e).palette.primary.contrastText,backgroundColor:(e.vars||e).palette.primary.dark},[`& .${um.avatarColorSecondary}`]:{color:(e.vars||e).palette.secondary.contrastText,backgroundColor:(e.vars||e).palette.secondary.dark},[`& .${um.avatarSmall}`]:{marginLeft:4,marginRight:-4,width:18,height:18,fontSize:e.typography.pxToRem(10)},[`& .${um.icon}`]:{marginLeft:5,marginRight:-6},[`& .${um.deleteIcon}`]:{WebkitTapHighlightColor:`transparent`,color:e.alpha((e.vars||e).palette.text.primary,.26),fontSize:22,cursor:`pointer`,margin:`0 5px 0 -6px`,"&:hover":{color:e.alpha((e.vars||e).palette.text.primary,.4)}},variants:[{props:{size:`small`},style:{height:24,[`& .${um.icon}`]:{fontSize:18,marginLeft:4,marginRight:-4},[`& .${um.deleteIcon}`]:{fontSize:16,marginRight:4,marginLeft:-4}}},...Object.entries(e.palette).filter(Sd([`contrastText`])).map(([t])=>({props:{color:t},style:{backgroundColor:(e.vars||e).palette[t].main,color:(e.vars||e).palette[t].contrastText,[`& .${um.deleteIcon}`]:{color:e.alpha((e.vars||e).palette[t].contrastText,.7),"&:hover, &:active":{color:(e.vars||e).palette[t].contrastText}}}})),{props:e=>e.iconColor===e.color,style:{[`& .${um.icon}`]:{color:e.vars?e.vars.palette.Chip.defaultIconColor:t}}},{props:e=>e.iconColor===e.color&&e.color!==`default`,style:{[`& .${um.icon}`]:{color:`inherit`}}},{props:{onDelete:!0},style:{[`&.${um.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)}}},...Object.entries(e.palette).filter(Sd([`dark`])).map(([t])=>({props:{color:t,onDelete:!0},style:{[`&.${um.focusVisible}`]:{background:(e.vars||e).palette[t].dark}}})),{props:{clickable:!0},style:{userSelect:`none`,WebkitTapHighlightColor:`transparent`,cursor:`pointer`,"&:hover":{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.hoverOpacity}`)},[`&.${um.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.action.selected,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)},"&:active":{boxShadow:(e.vars||e).shadows[1]}}},...Object.entries(e.palette).filter(Sd([`dark`])).map(([t])=>({props:{color:t,clickable:!0},style:{[`&:hover, &.${um.focusVisible}`]:{backgroundColor:(e.vars||e).palette[t].dark}}})),{props:{variant:`outlined`},style:{backgroundColor:`transparent`,border:e.vars?`1px solid ${e.vars.palette.Chip.defaultBorder}`:`1px solid ${e.palette.mode===`light`?e.palette.grey[400]:e.palette.grey[700]}`,[`&.${um.clickable}:hover`]:{backgroundColor:(e.vars||e).palette.action.hover},[`&.${um.focusVisible}`]:{backgroundColor:(e.vars||e).palette.action.focus},[`& .${um.avatar}`]:{marginLeft:4},[`& .${um.avatarSmall}`]:{marginLeft:2},[`& .${um.icon}`]:{marginLeft:4},[`& .${um.iconSmall}`]:{marginLeft:2},[`& .${um.deleteIcon}`]:{marginRight:5},[`& .${um.deleteIconSmall}`]:{marginRight:3}}},...Object.entries(e.palette).filter(Sd()).map(([t])=>({props:{variant:`outlined`,color:t},style:{color:(e.vars||e).palette[t].main,border:`1px solid ${e.alpha((e.vars||e).palette[t].main,.7)}`,[`&.${um.clickable}:hover`]:{backgroundColor:e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)},[`&.${um.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.focusOpacity)},[`& .${um.deleteIcon}`]:{color:e.alpha((e.vars||e).palette[t].main,.7),"&:hover, &:active":{color:(e.vars||e).palette[t].main}}}}))]}})),pm=H(`span`,{name:`MuiChip`,slot:`Label`,overridesResolver:(e,t)=>{let{ownerState:n}=e,{size:r}=n;return[t.label,t[`label${U(r)}`]]}})({overflow:`hidden`,textOverflow:`ellipsis`,paddingLeft:12,paddingRight:12,whiteSpace:`nowrap`,variants:[{props:{variant:`outlined`},style:{paddingLeft:11,paddingRight:11}},{props:{size:`small`},style:{paddingLeft:8,paddingRight:8}},{props:{size:`small`,variant:`outlined`},style:{paddingLeft:7,paddingRight:7}}]});function mm(e){return e.key===`Backspace`||e.key===`Delete`}var hm=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiChip`}),{avatar:r,className:i,clickable:a,color:o=`default`,component:s,deleteIcon:c,disabled:l=!1,icon:u,label:d,onClick:f,onDelete:p,onKeyDown:m,onKeyUp:h,size:g=`medium`,variant:_=`filled`,tabIndex:v,skipFocusWhenDisabled:y=!1,slots:b={},slotProps:S={},...C}=n,w=Yl(x.useRef(null),t),T=e=>{e.stopPropagation(),p&&p(e)},E=e=>{e.currentTarget===e.target&&mm(e)&&e.preventDefault(),m&&m(e)},D=e=>{e.currentTarget===e.target&&p&&mm(e)&&p(e),h&&h(e)},O=a!==!1&&f?!0:a,k=O||p?yd:s||`div`,A={...n,component:k,disabled:l,size:g,color:o,iconColor:x.isValidElement(u)&&u.props.color||o,onDelete:!!p,clickable:O,variant:_},j=dm(A),M=k===yd?{component:s||`div`,focusVisibleClassName:j.focusVisible,...p&&{disableRipple:!0}}:{},N=null;p&&(N=c&&x.isValidElement(c)?x.cloneElement(c,{className:B(c.props.className,j.deleteIcon),onClick:T}):(0,R.jsx)(cm,{className:j.deleteIcon,onClick:T}));let ee=null;r&&x.isValidElement(r)&&(ee=x.cloneElement(r,{className:B(j.avatar,r.props.className)}));let P=null;u&&x.isValidElement(u)&&(P=x.cloneElement(u,{className:B(j.icon,u.props.className)}));let F={slots:b,slotProps:S},[I,te]=Vu(`root`,{elementType:fm,externalForwardedProps:{...F,...C},ownerState:A,shouldForwardComponentProp:!0,ref:w,className:B(j.root,i),additionalProps:{disabled:O&&l?!0:void 0,tabIndex:y&&l?-1:v,...M},getSlotProps:e=>({...e,onClick:t=>{e.onClick?.(t),f?.(t)},onKeyDown:t=>{e.onKeyDown?.(t),E(t)},onKeyUp:t=>{e.onKeyUp?.(t),D(t)}})}),[ne,re]=Vu(`label`,{elementType:pm,externalForwardedProps:F,ownerState:A,className:j.label});return(0,R.jsxs)(I,{as:k,...te,children:[ee||P,(0,R.jsx)(ne,{...re,children:d}),N]})});function gm(e){return parseInt(e,10)||0}var _m={shadow:{visibility:`hidden`,position:`absolute`,overflow:`hidden`,height:0,top:0,left:0,transform:`translateZ(0)`}};function vm(e){for(let t in e)return!1;return!0}function ym(e){return vm(e)||e.outerHeightStyle===0&&!e.overflowing}var bm=x.forwardRef(function(e,t){let{onChange:n,maxRows:r,minRows:i=1,style:a,value:o,...s}=e,{current:c}=x.useRef(o!=null),l=x.useRef(null),u=Jl(t,l),d=x.useRef(null),f=x.useRef(null),p=x.useCallback(()=>{let t=l.current,n=f.current;if(!t||!n)return;let a=Bl(t).getComputedStyle(t);if(a.width===`0px`)return{outerHeightStyle:0,overflowing:!1};n.style.width=a.width,n.value=t.value||e.placeholder||`x`,n.value.slice(-1)===`
`&&(n.value+=` `);let o=a.boxSizing,s=gm(a.paddingBottom)+gm(a.paddingTop),c=gm(a.borderBottomWidth)+gm(a.borderTopWidth),u=n.scrollHeight;n.value=`x`;let d=n.scrollHeight,p=u;return i&&(p=Math.max(Number(i)*d,p)),r&&(p=Math.min(Number(r)*d,p)),p=Math.max(p,d),{outerHeightStyle:p+(o===`border-box`?s+c:0),overflowing:Math.abs(p-u)<=1}},[r,i,e.placeholder]),m=Kl(()=>{let e=l.current,t=p();if(!e||!t||ym(t))return!1;let n=t.outerHeightStyle;return d.current!=null&&d.current!==n}),h=x.useCallback(()=>{let e=l.current,t=p();if(!e||!t||ym(t))return;let n=t.outerHeightStyle;d.current!==n&&(d.current=n,e.style.height=`${n}px`),e.style.overflow=t.overflowing?`hidden`:``},[p]),g=x.useRef(-1);return Zo(()=>{let e=Ll(h),t=l?.current;if(!t)return;let n=Bl(t);n.addEventListener(`resize`,e);let r;return typeof ResizeObserver<`u`&&(r=new ResizeObserver(()=>{m()&&(r.unobserve(t),cancelAnimationFrame(g.current),h(),g.current=requestAnimationFrame(()=>{r.observe(t)}))}),r.observe(t)),()=>{e.clear(),cancelAnimationFrame(g.current),n.removeEventListener(`resize`,e),r&&r.disconnect()}},[p,h,m]),Zo(()=>{h()}),(0,R.jsxs)(x.Fragment,{children:[(0,R.jsx)(`textarea`,{value:o,onChange:e=>{c||h();let t=e.target,r=t.value.length,i=t.value.endsWith(`
`),a=t.selectionStart===r;i&&a&&t.setSelectionRange(r,r),n&&n(e)},ref:u,rows:i,style:a,...s}),(0,R.jsx)(`textarea`,{"aria-hidden":!0,className:e.className,readOnly:!0,ref:f,tabIndex:-1,style:{..._m.shadow,...a,paddingTop:0,paddingBottom:0}})]})});function xm({props:e,states:t,muiFormControl:n}){return t.reduce((t,r)=>(t[r]=e[r],n&&e[r]===void 0&&(t[r]=n[r]),t),{})}var Sm=x.createContext(void 0);function Cm(){return x.useContext(Sm)}function wm(e){return e!=null&&!(Array.isArray(e)&&e.length===0)}function Tm(e,t=!1){return e&&(wm(e.value)&&e.value!==``||t&&wm(e.defaultValue)&&e.defaultValue!==``)}function Em(e){return Io(`MuiInputBase`,e)}var Dm=Lo(`MuiInputBase`,[`root`,`formControl`,`focused`,`disabled`,`adornedStart`,`adornedEnd`,`error`,`sizeSmall`,`multiline`,`colorSecondary`,`fullWidth`,`hiddenLabel`,`readOnly`,`input`,`inputSizeSmall`,`inputMultiline`,`inputTypeSearch`,`inputAdornedStart`,`inputAdornedEnd`,`inputHiddenLabel`]),Om;const km=(e,t)=>{let{ownerState:n}=e;return[t.root,n.formControl&&t.formControl,n.startAdornment&&t.adornedStart,n.endAdornment&&t.adornedEnd,n.error&&t.error,n.size===`small`&&t.sizeSmall,n.multiline&&t.multiline,n.color&&t[`color${U(n.color)}`],n.fullWidth&&t.fullWidth,n.hiddenLabel&&t.hiddenLabel]},Am=(e,t)=>{let{ownerState:n}=e;return[t.input,n.size===`small`&&t.inputSizeSmall,n.multiline&&t.inputMultiline,n.type===`search`&&t.inputTypeSearch,n.startAdornment&&t.inputAdornedStart,n.endAdornment&&t.inputAdornedEnd,n.hiddenLabel&&t.inputHiddenLabel]};var jm=e=>{let{classes:t,color:n,disabled:r,error:i,endAdornment:a,focused:o,formControl:s,fullWidth:c,hiddenLabel:l,multiline:u,readOnly:d,size:f,startAdornment:p,type:m}=e;return cc({root:[`root`,`color${U(n)}`,r&&`disabled`,i&&`error`,c&&`fullWidth`,o&&`focused`,s&&`formControl`,f&&f!==`medium`&&`size${U(f)}`,u&&`multiline`,p&&`adornedStart`,a&&`adornedEnd`,l&&`hiddenLabel`,d&&`readOnly`],input:[`input`,r&&`disabled`,m===`search`&&`inputTypeSearch`,u&&`inputMultiline`,f===`small`&&`inputSizeSmall`,l&&`inputHiddenLabel`,p&&`inputAdornedStart`,a&&`inputAdornedEnd`,d&&`readOnly`]},Em,t)};const Mm=H(`div`,{name:`MuiInputBase`,slot:`Root`,overridesResolver:km})(kl(({theme:e})=>({...e.typography.body1,color:(e.vars||e).palette.text.primary,lineHeight:`1.4375em`,boxSizing:`border-box`,position:`relative`,cursor:`text`,display:`inline-flex`,alignItems:`center`,[`&.${Dm.disabled}`]:{color:(e.vars||e).palette.text.disabled,cursor:`default`},variants:[{props:({ownerState:e})=>e.multiline,style:{padding:`4px 0 5px`}},{props:({ownerState:e,size:t})=>e.multiline&&t===`small`,style:{paddingTop:1}},{props:({ownerState:e})=>e.fullWidth,style:{width:`100%`}}]}))),Nm=H(`input`,{name:`MuiInputBase`,slot:`Input`,overridesResolver:Am})(kl(({theme:e})=>{let t=e.palette.mode===`light`,n={color:`currentColor`,...e.vars?{opacity:e.vars.opacity.inputPlaceholder}:{opacity:t?.42:.5},transition:e.transitions.create(`opacity`,{duration:e.transitions.duration.shorter})},r={opacity:`0 !important`},i=e.vars?{opacity:e.vars.opacity.inputPlaceholder}:{opacity:t?.42:.5};return{font:`inherit`,letterSpacing:`inherit`,color:`currentColor`,padding:`4px 0 5px`,border:0,boxSizing:`content-box`,background:`none`,height:`1.4375em`,margin:0,WebkitTapHighlightColor:`transparent`,display:`block`,minWidth:0,width:`100%`,"&::-webkit-input-placeholder":n,"&::-moz-placeholder":n,"&::-ms-input-placeholder":n,"&:focus":{outline:0},"&:invalid":{boxShadow:`none`},"&::-webkit-search-decoration":{WebkitAppearance:`none`},[`label[data-shrink=false] + .${Dm.formControl} &`]:{"&::-webkit-input-placeholder":r,"&::-moz-placeholder":r,"&::-ms-input-placeholder":r,"&:focus::-webkit-input-placeholder":i,"&:focus::-moz-placeholder":i,"&:focus::-ms-input-placeholder":i},[`&.${Dm.disabled}`]:{opacity:1,WebkitTextFillColor:(e.vars||e).palette.text.disabled},variants:[{props:({ownerState:e})=>!e.disableInjectingGlobalStyles,style:{animationName:`mui-auto-fill-cancel`,animationDuration:`10ms`,"&:-webkit-autofill":{animationDuration:`5000s`,animationName:`mui-auto-fill`}}},{props:{size:`small`},style:{paddingTop:1}},{props:({ownerState:e})=>e.multiline,style:{height:`auto`,resize:`none`,padding:0,paddingTop:0}},{props:{type:`search`},style:{MozAppearance:`textfield`}}]}}));var Pm=Dl({"@keyframes mui-auto-fill":{from:{display:`block`}},"@keyframes mui-auto-fill-cancel":{from:{display:`block`}}}),Fm=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiInputBase`}),{"aria-describedby":r,autoComplete:i,autoFocus:a,className:o,color:s,components:c={},componentsProps:l={},defaultValue:u,disabled:d,disableInjectingGlobalStyles:f,endAdornment:p,error:m,fullWidth:h=!1,id:g,inputComponent:_=`input`,inputProps:v={},inputRef:y,margin:b,maxRows:S,minRows:C,multiline:w=!1,name:T,onBlur:E,onChange:D,onClick:O,onFocus:k,onKeyDown:A,onKeyUp:j,placeholder:M,readOnly:N,renderSuffix:ee,rows:P,size:F,slotProps:I={},slots:te={},startAdornment:ne,type:re=`text`,value:ie,...ae}=n,L=v.value==null?ie:v.value,{current:oe}=x.useRef(L!=null),se=x.useRef(),ce=x.useCallback(e=>{},[]),le=Yl(se,y,v.ref,ce),[ue,de]=x.useState(!1),fe=Cm(),pe=xm({props:n,muiFormControl:fe,states:[`color`,`disabled`,`error`,`hiddenLabel`,`size`,`required`,`filled`]});pe.focused=fe?fe.focused:ue,x.useEffect(()=>{!fe&&d&&ue&&(de(!1),E&&E())},[fe,d,ue,E]);let me=fe&&fe.onFilled,he=fe&&fe.onEmpty,ge=x.useCallback(e=>{Tm(e)?me&&me():he&&he()},[me,he]);Hl(()=>{oe&&ge({value:L})},[L,ge,oe]);let _e=e=>{k&&k(e),v.onFocus&&v.onFocus(e),fe&&fe.onFocus?fe.onFocus(e):de(!0)},ve=e=>{E&&E(e),v.onBlur&&v.onBlur(e),fe&&fe.onBlur?fe.onBlur(e):de(!1)},ye=(e,...t)=>{if(!oe){let t=e.target||se.current;if(t==null)throw Error(En(1));ge({value:t.value})}v.onChange&&v.onChange(e,...t),D&&D(e,...t)};x.useEffect(()=>{ge(se.current)},[]);let be=e=>{se.current&&e.currentTarget===e.target&&se.current.focus(),O&&O(e)},xe=_,Se=v;w&&xe===`input`&&(Se=P?{type:void 0,minRows:P,maxRows:P,...Se}:{type:void 0,maxRows:S,minRows:C,...Se},xe=bm);let Ce=e=>{ge(e.animationName===`mui-auto-fill-cancel`?se.current:{value:`x`})};x.useEffect(()=>{fe&&fe.setAdornedStart(!!ne)},[fe,ne]);let we={...n,color:pe.color||`primary`,disabled:pe.disabled,endAdornment:p,error:pe.error,focused:pe.focused,formControl:fe,fullWidth:h,hiddenLabel:pe.hiddenLabel,multiline:w,size:pe.size,startAdornment:ne,type:re},Te=jm(we),Ee=te.root||c.Root||Mm,De=I.root||l.root||{},Oe=te.input||c.Input||Nm;return Se={...Se,...I.input??l.input},(0,R.jsxs)(x.Fragment,{children:[!f&&typeof Pm==`function`&&(Om||=(0,R.jsx)(Pm,{})),(0,R.jsxs)(Ee,{...De,ref:t,onClick:be,...ae,...!Au(Ee)&&{ownerState:{...we,...De.ownerState}},className:B(Te.root,De.className,o,N&&`MuiInputBase-readOnly`),children:[ne,(0,R.jsx)(Sm.Provider,{value:null,children:(0,R.jsx)(Oe,{"aria-invalid":pe.error,"aria-describedby":r,autoComplete:i,autoFocus:a,defaultValue:u,disabled:pe.disabled,id:g,onAnimationStart:Ce,name:T,placeholder:M,readOnly:N,required:pe.required,rows:P,value:L,onKeyDown:A,onKeyUp:j,type:re,...Se,...!Au(Oe)&&{as:xe,ownerState:{...we,...Se.ownerState}},ref:le,className:B(Te.input,Se.className,N&&`MuiInputBase-readOnly`),onBlur:ve,onChange:ye,onFocus:_e})}),p,ee?ee({...pe,startAdornment:ne}):null]})]})}),Im={entering:{opacity:1},entered:{opacity:1}},Lm=x.forwardRef(function(e,t){let n=pl(),r={enter:n.transitions.duration.enteringScreen,exit:n.transitions.duration.leavingScreen},{addEndListener:i,appear:a=!0,children:o,easing:s,in:c,onEnter:l,onEntered:u,onEntering:d,onExit:f,onExited:p,onExiting:m,style:h,timeout:g=r,TransitionComponent:_=uu,...v}=e,y=x.useRef(null),b=Yl(y,Yp(o),t),S=e=>t=>{if(e){let n=y.current;t===void 0?e(n):e(n,t)}},C=S(d),w=S((e,t)=>{Du(e);let r=Ou({style:h,timeout:g,easing:s},{mode:`enter`});e.style.webkitTransition=n.transitions.create(`opacity`,r),e.style.transition=n.transitions.create(`opacity`,r),l&&l(e,t)}),T=S(u),E=S(m),D=S(e=>{let t=Ou({style:h,timeout:g,easing:s},{mode:`exit`});e.style.webkitTransition=n.transitions.create(`opacity`,t),e.style.transition=n.transitions.create(`opacity`,t),f&&f(e)}),O=S(p);return(0,R.jsx)(_,{appear:a,in:c,nodeRef:y,onEnter:w,onEntered:T,onEntering:C,onExit:D,onExited:O,onExiting:E,addEndListener:e=>{i&&i(y.current,e)},timeout:g,...v,children:(e,{ownerState:t,...n})=>x.cloneElement(o,{style:{opacity:0,visibility:e===`exited`&&!c?`hidden`:void 0,...Im[e],...h,...o.props.style},ref:b,...n})})});function Rm(e){return Io(`MuiBackdrop`,e)}Lo(`MuiBackdrop`,[`root`,`invisible`]);var zm=e=>{let{classes:t,invisible:n}=e;return cc({root:[`root`,n&&`invisible`]},Rm,t)},Bm=H(`div`,{name:`MuiBackdrop`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.invisible&&t.invisible]}})({position:`fixed`,display:`flex`,alignItems:`center`,justifyContent:`center`,right:0,bottom:0,top:0,left:0,backgroundColor:`rgba(0, 0, 0, 0.5)`,WebkitTapHighlightColor:`transparent`,variants:[{props:{invisible:!0},style:{backgroundColor:`transparent`}}]}),Vm=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiBackdrop`}),{children:r,className:i,component:a=`div`,invisible:o=!1,open:s,components:c={},componentsProps:l={},slotProps:u={},slots:d={},TransitionComponent:f,transitionDuration:p,...m}=n,h={...n,component:a,invisible:o},g=zm(h),_={component:a,slots:{transition:f,root:c.Root,...d},slotProps:{...l,...u}},[v,y]=Vu(`root`,{elementType:Bm,externalForwardedProps:_,className:B(g.root,i),ownerState:h}),[b,x]=Vu(`transition`,{elementType:Lm,externalForwardedProps:_,ownerState:h});return(0,R.jsx)(b,{in:s,timeout:p,...m,...x,children:(0,R.jsx)(v,{"aria-hidden":!0,...y,classes:g,ref:t,children:r})})}),Hm=Lo(`MuiBox`,[`root`]),q=Po({themeId:Dn,defaultTheme:dl(),defaultClassName:Hm.root,generateClassName:Ao.generate});function Um(e){return Io(`MuiButton`,e)}var Wm=Lo(`MuiButton`,`root.text.textInherit.textPrimary.textSecondary.textSuccess.textError.textInfo.textWarning.outlined.outlinedInherit.outlinedPrimary.outlinedSecondary.outlinedSuccess.outlinedError.outlinedInfo.outlinedWarning.contained.containedInherit.containedPrimary.containedSecondary.containedSuccess.containedError.containedInfo.containedWarning.disableElevation.focusVisible.disabled.colorInherit.colorPrimary.colorSecondary.colorSuccess.colorError.colorInfo.colorWarning.textSizeSmall.textSizeMedium.textSizeLarge.outlinedSizeSmall.outlinedSizeMedium.outlinedSizeLarge.containedSizeSmall.containedSizeMedium.containedSizeLarge.sizeMedium.sizeSmall.sizeLarge.fullWidth.startIcon.endIcon.icon.iconSizeSmall.iconSizeMedium.iconSizeLarge.loading.loadingWrapper.loadingIconPlaceholder.loadingIndicator.loadingPositionCenter.loadingPositionStart.loadingPositionEnd`.split(`.`)),Gm=x.createContext({}),Km=x.createContext(void 0),qm=e=>{let{color:t,disableElevation:n,fullWidth:r,size:i,variant:a,loading:o,loadingPosition:s,classes:c}=e,l=cc({root:[`root`,o&&`loading`,a,`${a}${U(t)}`,`size${U(i)}`,`${a}Size${U(i)}`,`color${U(t)}`,n&&`disableElevation`,r&&`fullWidth`,o&&`loadingPosition${U(s)}`],startIcon:[`icon`,`startIcon`,`iconSize${U(i)}`],endIcon:[`icon`,`endIcon`,`iconSize${U(i)}`],loadingIndicator:[`loadingIndicator`],loadingWrapper:[`loadingWrapper`]},Um,c);return{...c,...l}},Jm=[{props:{size:`small`},style:{"& > *:nth-of-type(1)":{fontSize:18}}},{props:{size:`medium`},style:{"& > *:nth-of-type(1)":{fontSize:20}}},{props:{size:`large`},style:{"& > *:nth-of-type(1)":{fontSize:22}}}],Ym=H(yd,{shouldForwardProp:e=>gl(e)||e===`classes`,name:`MuiButton`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[n.variant],t[`${n.variant}${U(n.color)}`],t[`size${U(n.size)}`],t[`${n.variant}Size${U(n.size)}`],n.color===`inherit`&&t.colorInherit,n.disableElevation&&t.disableElevation,n.fullWidth&&t.fullWidth,n.loading&&t.loading]}})(kl(({theme:e})=>{let t=e.palette.mode===`light`?e.palette.grey[300]:e.palette.grey[800],n=e.palette.mode===`light`?e.palette.grey.A100:e.palette.grey[700];return{...e.typography.button,minWidth:64,padding:`6px 16px`,border:0,borderRadius:(e.vars||e).shape.borderRadius,transition:e.transitions.create([`background-color`,`box-shadow`,`border-color`,`color`],{duration:e.transitions.duration.short}),"&:hover":{textDecoration:`none`},[`&.${Wm.disabled}`]:{color:(e.vars||e).palette.action.disabled},variants:[{props:{variant:`contained`},style:{color:`var(--variant-containedColor)`,backgroundColor:`var(--variant-containedBg)`,boxShadow:(e.vars||e).shadows[2],"&:hover":{boxShadow:(e.vars||e).shadows[4],"@media (hover: none)":{boxShadow:(e.vars||e).shadows[2]}},"&:active":{boxShadow:(e.vars||e).shadows[8]},[`&.${Wm.focusVisible}`]:{boxShadow:(e.vars||e).shadows[6]},[`&.${Wm.disabled}`]:{color:(e.vars||e).palette.action.disabled,boxShadow:(e.vars||e).shadows[0],backgroundColor:(e.vars||e).palette.action.disabledBackground}}},{props:{variant:`outlined`},style:{padding:`5px 15px`,border:`1px solid currentColor`,borderColor:`var(--variant-outlinedBorder, currentColor)`,backgroundColor:`var(--variant-outlinedBg)`,color:`var(--variant-outlinedColor)`,[`&.${Wm.disabled}`]:{border:`1px solid ${(e.vars||e).palette.action.disabledBackground}`}}},{props:{variant:`text`},style:{padding:`6px 8px`,color:`var(--variant-textColor)`,backgroundColor:`var(--variant-textBg)`}},...Object.entries(e.palette).filter(Sd()).map(([t])=>({props:{color:t},style:{"--variant-textColor":(e.vars||e).palette[t].main,"--variant-outlinedColor":(e.vars||e).palette[t].main,"--variant-outlinedBorder":e.alpha((e.vars||e).palette[t].main,.5),"--variant-containedColor":(e.vars||e).palette[t].contrastText,"--variant-containedBg":(e.vars||e).palette[t].main,"@media (hover: hover)":{"&:hover":{"--variant-containedBg":(e.vars||e).palette[t].dark,"--variant-textBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity),"--variant-outlinedBorder":(e.vars||e).palette[t].main,"--variant-outlinedBg":e.alpha((e.vars||e).palette[t].main,(e.vars||e).palette.action.hoverOpacity)}}}})),{props:{color:`inherit`},style:{color:`inherit`,borderColor:`currentColor`,"--variant-containedBg":e.vars?e.vars.palette.Button.inheritContainedBg:t,"@media (hover: hover)":{"&:hover":{"--variant-containedBg":e.vars?e.vars.palette.Button.inheritContainedHoverBg:n,"--variant-textBg":e.alpha((e.vars||e).palette.text.primary,(e.vars||e).palette.action.hoverOpacity),"--variant-outlinedBg":e.alpha((e.vars||e).palette.text.primary,(e.vars||e).palette.action.hoverOpacity)}}}},{props:{size:`small`,variant:`text`},style:{padding:`4px 5px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`text`},style:{padding:`8px 11px`,fontSize:e.typography.pxToRem(15)}},{props:{size:`small`,variant:`outlined`},style:{padding:`3px 9px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`outlined`},style:{padding:`7px 21px`,fontSize:e.typography.pxToRem(15)}},{props:{size:`small`,variant:`contained`},style:{padding:`4px 10px`,fontSize:e.typography.pxToRem(13)}},{props:{size:`large`,variant:`contained`},style:{padding:`8px 22px`,fontSize:e.typography.pxToRem(15)}},{props:{disableElevation:!0},style:{boxShadow:`none`,"&:hover":{boxShadow:`none`},[`&.${Wm.focusVisible}`]:{boxShadow:`none`},"&:active":{boxShadow:`none`},[`&.${Wm.disabled}`]:{boxShadow:`none`}}},{props:{fullWidth:!0},style:{width:`100%`}},{props:{loadingPosition:`center`},style:{transition:e.transitions.create([`background-color`,`box-shadow`,`border-color`],{duration:e.transitions.duration.short}),[`&.${Wm.loading}`]:{color:`transparent`}}}]}})),Xm=H(`span`,{name:`MuiButton`,slot:`StartIcon`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.startIcon,n.loading&&t.startIconLoadingStart,t[`iconSize${U(n.size)}`]]}})(({theme:e})=>({display:`inherit`,marginRight:8,marginLeft:-4,variants:[{props:{size:`small`},style:{marginLeft:-2}},{props:{loadingPosition:`start`,loading:!0},style:{transition:e.transitions.create([`opacity`],{duration:e.transitions.duration.short}),opacity:0}},{props:{loadingPosition:`start`,loading:!0,fullWidth:!0},style:{marginRight:-8}},...Jm]})),Zm=H(`span`,{name:`MuiButton`,slot:`EndIcon`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.endIcon,n.loading&&t.endIconLoadingEnd,t[`iconSize${U(n.size)}`]]}})(({theme:e})=>({display:`inherit`,marginRight:-4,marginLeft:8,variants:[{props:{size:`small`},style:{marginRight:-2}},{props:{loadingPosition:`end`,loading:!0},style:{transition:e.transitions.create([`opacity`],{duration:e.transitions.duration.short}),opacity:0}},{props:{loadingPosition:`end`,loading:!0,fullWidth:!0},style:{marginLeft:-8}},...Jm]})),Qm=H(`span`,{name:`MuiButton`,slot:`LoadingIndicator`})(({theme:e})=>({display:`none`,position:`absolute`,visibility:`visible`,variants:[{props:{loading:!0},style:{display:`flex`}},{props:{loadingPosition:`start`},style:{left:14}},{props:{loadingPosition:`start`,size:`small`},style:{left:10}},{props:{variant:`text`,loadingPosition:`start`},style:{left:6}},{props:{loadingPosition:`center`},style:{left:`50%`,transform:`translate(-50%)`,color:(e.vars||e).palette.action.disabled}},{props:{loadingPosition:`end`},style:{right:14}},{props:{loadingPosition:`end`,size:`small`},style:{right:10}},{props:{variant:`text`,loadingPosition:`end`},style:{right:6}},{props:{loadingPosition:`start`,fullWidth:!0},style:{position:`relative`,left:-10}},{props:{loadingPosition:`end`,fullWidth:!0},style:{position:`relative`,right:-10}}]})),$m=H(`span`,{name:`MuiButton`,slot:`LoadingIconPlaceholder`})({display:`inline-block`,width:`1em`,height:`1em`}),eh=x.forwardRef(function(e,t){let n=x.useContext(Gm),r=x.useContext(Km),i=Al({props:Xo(n,e),name:`MuiButton`}),{children:a,color:o=`primary`,component:s=`button`,className:c,disabled:l=!1,disableElevation:u=!1,disableFocusRipple:d=!1,endIcon:f,focusVisibleClassName:p,fullWidth:m=!1,id:h,loading:g=null,loadingIndicator:_,loadingPosition:v=`center`,size:y=`medium`,startIcon:b,type:S,variant:C=`text`,...w}=i,T=W(h),E=_??(0,R.jsx)(Nd,{"aria-labelledby":T,color:`inherit`,size:16}),D={...i,color:o,component:s,disabled:l,disableElevation:u,disableFocusRipple:d,fullWidth:m,loading:g,loadingIndicator:E,loadingPosition:v,size:y,type:S,variant:C},O=qm(D),k=(b||g&&v===`start`)&&(0,R.jsx)(Xm,{className:O.startIcon,ownerState:D,children:b||(0,R.jsx)($m,{className:O.loadingIconPlaceholder,ownerState:D})}),A=(f||g&&v===`end`)&&(0,R.jsx)(Zm,{className:O.endIcon,ownerState:D,children:f||(0,R.jsx)($m,{className:O.loadingIconPlaceholder,ownerState:D})}),j=r||``,M=typeof g==`boolean`?(0,R.jsx)(`span`,{className:O.loadingWrapper,style:{display:`contents`},children:g&&(0,R.jsx)(Qm,{className:O.loadingIndicator,ownerState:D,children:E})}):null;return(0,R.jsxs)(Ym,{ownerState:D,className:B(n.className,O.root,c,j),component:s,disabled:l||g,focusRipple:!d,focusVisibleClassName:B(O.focusVisible,p),ref:t,type:S,id:g?T:h,...w,classes:O,children:[k,v!==`end`&&M,a,v===`end`&&M,A]})}),th=typeof Dl({})==`function`;const nh=(e,t)=>({WebkitFontSmoothing:`antialiased`,MozOsxFontSmoothing:`grayscale`,boxSizing:`border-box`,WebkitTextSizeAdjust:`100%`,...t&&!e.vars&&{colorScheme:e.palette.mode}}),rh=e=>({color:(e.vars||e).palette.text.primary,...e.typography.body1,backgroundColor:(e.vars||e).palette.background.default,"@media print":{backgroundColor:(e.vars||e).palette.common.white}}),ih=(e,t=!1)=>{let n={};t&&e.colorSchemes&&typeof e.getColorSchemeSelector==`function`&&Object.entries(e.colorSchemes).forEach(([t,r])=>{let i=e.getColorSchemeSelector(t);i.startsWith(`@`)?n[i]={":root":{colorScheme:r.palette?.mode}}:n[i.replace(/\s*&/,``)]={colorScheme:r.palette?.mode}});let r={html:nh(e,t),"*, *::before, *::after":{boxSizing:`inherit`},"strong, b":{fontWeight:e.typography.fontWeightBold},body:{margin:0,...rh(e),"&::backdrop":{backgroundColor:(e.vars||e).palette.background.default}},...n},i=e.components?.MuiCssBaseline?.styleOverrides;return i&&(r=[r,i]),r};var ah=`mui-ecs`,oh=e=>{let t=ih(e,!1),n=Array.isArray(t)?t[0]:t;return!e.vars&&n&&(n.html[`:root:has(${ah})`]={colorScheme:e.palette.mode}),e.colorSchemes&&Object.entries(e.colorSchemes).forEach(([t,r])=>{let i=e.getColorSchemeSelector(t);i.startsWith(`@`)?n[i]={[`:root:not(:has(.${ah}))`]:{colorScheme:r.palette?.mode}}:n[i.replace(/\s*&/,``)]={[`&:not(:has(.${ah}))`]:{colorScheme:r.palette?.mode}}}),t},sh=Dl(th?({theme:e,enableColorScheme:t})=>ih(e,t):({theme:e})=>oh(e));function ch(e){let{children:t,enableColorScheme:n=!1}=Al({props:e,name:`MuiCssBaseline`});return(0,R.jsxs)(x.Fragment,{children:[th&&(0,R.jsx)(sh,{enableColorScheme:n}),!th&&!n&&(0,R.jsx)(`span`,{className:ah,style:{display:`none`}}),t]})}var lh=ch;function uh(e=window){let t=e.document.documentElement.clientWidth;return e.innerWidth-t}function dh(e){let t=zl(e);return t.body===e?Bl(e).innerWidth>t.documentElement.clientWidth:e.scrollHeight>e.clientHeight}function fh(e,t){t?e.setAttribute(`aria-hidden`,`true`):e.removeAttribute(`aria-hidden`)}function ph(e){return parseInt(Bl(e).getComputedStyle(e).paddingRight,10)||0}function mh(e){let t=[`TEMPLATE`,`SCRIPT`,`STYLE`,`LINK`,`MAP`,`META`,`NOSCRIPT`,`PICTURE`,`COL`,`COLGROUP`,`PARAM`,`SLOT`,`SOURCE`,`TRACK`].includes(e.tagName),n=e.tagName===`INPUT`&&e.getAttribute(`type`)===`hidden`;return t||n}function hh(e,t,n,r,i){let a=[t,n,...r];[].forEach.call(e.children,e=>{let t=!a.includes(e),n=!mh(e);t&&n&&fh(e,i)})}function gh(e,t){let n=-1;return e.some((e,r)=>t(e)?(n=r,!0):!1),n}function _h(e,t){let n=[],r=e.container;if(!t.disableScrollLock){if(dh(r)){let e=uh(Bl(r));n.push({value:r.style.paddingRight,property:`padding-right`,el:r}),r.style.paddingRight=`${ph(r)+e}px`;let t=zl(r).querySelectorAll(`.mui-fixed`);[].forEach.call(t,t=>{n.push({value:t.style.paddingRight,property:`padding-right`,el:t}),t.style.paddingRight=`${ph(t)+e}px`})}let e;if(r.parentNode instanceof DocumentFragment)e=zl(r).body;else{let t=r.parentElement,n=Bl(r);e=t?.nodeName===`HTML`&&n.getComputedStyle(t).overflowY===`scroll`?t:r}n.push({value:e.style.overflow,property:`overflow`,el:e},{value:e.style.overflowX,property:`overflow-x`,el:e},{value:e.style.overflowY,property:`overflow-y`,el:e}),e.style.overflow=`hidden`}return()=>{n.forEach(({value:e,el:t,property:n})=>{e?t.style.setProperty(n,e):t.style.removeProperty(n)})}}function vh(e){let t=[];return[].forEach.call(e.children,e=>{e.getAttribute(`aria-hidden`)===`true`&&t.push(e)}),t}var yh=class{constructor(){this.modals=[],this.containers=[]}add(e,t){let n=this.modals.indexOf(e);if(n!==-1)return n;n=this.modals.length,this.modals.push(e),e.modalRef&&fh(e.modalRef,!1);let r=vh(t);hh(t,e.mount,e.modalRef,r,!0);let i=gh(this.containers,e=>e.container===t);return i===-1?(this.containers.push({modals:[e],container:t,restore:null,hiddenSiblings:r}),n):(this.containers[i].modals.push(e),n)}mount(e,t){let n=gh(this.containers,t=>t.modals.includes(e)),r=this.containers[n];r.restore||=_h(r,t)}remove(e,t=!0){let n=this.modals.indexOf(e);if(n===-1)return n;let r=gh(this.containers,t=>t.modals.includes(e)),i=this.containers[r];if(i.modals.splice(i.modals.indexOf(e),1),this.modals.splice(n,1),i.modals.length===0)i.restore&&i.restore(),e.modalRef&&fh(e.modalRef,t),hh(i.container,e.mount,e.modalRef,i.hiddenSiblings,!1),this.containers.splice(r,1);else{let e=i.modals[i.modals.length-1];e.modalRef&&fh(e.modalRef,!1)}return n}isTopModal(e){return this.modals.length>0&&this.modals[this.modals.length-1]===e}},bh=[`input`,`select`,`textarea`,`a[href]`,`button`,`[tabindex]`,`audio[controls]`,`video[controls]`,`[contenteditable]:not([contenteditable="false"])`].join(`,`);function xh(e){let t=parseInt(e.getAttribute(`tabindex`)||``,10);return Number.isNaN(t)?e.contentEditable===`true`||(e.nodeName===`AUDIO`||e.nodeName===`VIDEO`||e.nodeName===`DETAILS`)&&e.getAttribute(`tabindex`)===null?0:e.tabIndex:t}function Sh(e){if(e.tagName!==`INPUT`||e.type!==`radio`||!e.name)return!1;let t=t=>e.ownerDocument.querySelector(`input[type="radio"]${t}`),n=t(`[name="${e.name}"]:checked`);return n||=t(`[name="${e.name}"]`),n!==e}function Ch(e){return!(e.disabled||e.tagName===`INPUT`&&e.type===`hidden`||Sh(e))}function wh(e){let t=[],n=[];return Array.from(e.querySelectorAll(bh)).forEach((e,r)=>{let i=xh(e);i===-1||!Ch(e)||(i===0?t.push(e):n.push({documentOrder:r,tabIndex:i,node:e}))}),n.sort((e,t)=>e.tabIndex===t.tabIndex?e.documentOrder-t.documentOrder:e.tabIndex-t.tabIndex).map(e=>e.node).concat(t)}function Th(){return!0}function Eh(e){let{children:t,disableAutoFocus:n=!1,disableEnforceFocus:r=!1,disableRestoreFocus:i=!1,getTabbable:a=wh,isEnabled:o=Th,open:s}=e,c=x.useRef(!1),l=x.useRef(null),u=x.useRef(null),d=x.useRef(null),f=x.useRef(null),p=x.useRef(!1),m=x.useRef(null),h=Jl(Yp(t),m),g=x.useRef(null);x.useEffect(()=>{!s||!m.current||(p.current=!n)},[n,s]),x.useEffect(()=>{if(!s||!m.current)return;let e=zl(m.current);return m.current.contains(e.activeElement)||(m.current.hasAttribute(`tabIndex`)||m.current.setAttribute(`tabIndex`,`-1`),p.current&&m.current.focus()),()=>{i||(d.current&&d.current.focus&&(c.current=!0,d.current.focus()),d.current=null)}},[s]),x.useEffect(()=>{if(!s||!m.current)return;let e=zl(m.current),t=t=>{g.current=t,!(r||!o()||t.key!==`Tab`)&&e.activeElement===m.current&&t.shiftKey&&(c.current=!0,u.current&&u.current.focus())},n=()=>{let t=m.current;if(t===null)return;if(!e.hasFocus()||!o()||c.current){c.current=!1;return}if(t.contains(e.activeElement)||r&&e.activeElement!==l.current&&e.activeElement!==u.current)return;if(e.activeElement!==f.current)f.current=null;else if(f.current!==null)return;if(!p.current)return;let n=[];if((e.activeElement===l.current||e.activeElement===u.current)&&(n=a(m.current)),n.length>0){let e=!!(g.current?.shiftKey&&g.current?.key===`Tab`),t=n[0],r=n[n.length-1];typeof t!=`string`&&typeof r!=`string`&&(e?r.focus():t.focus())}else t.focus()};e.addEventListener(`focusin`,n),e.addEventListener(`keydown`,t,!0);let i=setInterval(()=>{e.activeElement&&e.activeElement.tagName===`BODY`&&n()},50);return()=>{clearInterval(i),e.removeEventListener(`focusin`,n),e.removeEventListener(`keydown`,t,!0)}},[n,r,i,o,s,a]);let _=e=>{d.current===null&&(d.current=e.relatedTarget),p.current=!0,f.current=e.target;let n=t.props.onFocus;n&&n(e)},v=e=>{d.current===null&&(d.current=e.relatedTarget),p.current=!0};return(0,R.jsxs)(x.Fragment,{children:[(0,R.jsx)(`div`,{tabIndex:s?0:-1,onFocus:v,ref:l,"data-testid":`sentinelStart`}),x.cloneElement(t,{ref:h,onFocus:_}),(0,R.jsx)(`div`,{tabIndex:s?0:-1,onFocus:v,ref:u,"data-testid":`sentinelEnd`})]})}var Dh=Eh;function Oh(e){return typeof e==`function`?e():e}function kh(e){return e?e.props.hasOwnProperty(`in`):!1}var Ah=()=>{},jh=new yh;function Mh(e){let{container:t,disableEscapeKeyDown:n=!1,disableScrollLock:r=!1,closeAfterTransition:i=!1,onTransitionEnter:a,onTransitionExited:o,children:s,onClose:c,open:l,rootRef:u}=e,d=x.useRef({}),f=x.useRef(null),p=x.useRef(null),m=Jl(p,u),[h,g]=x.useState(!l),_=kh(s),v=!0;(e[`aria-hidden`]===`false`||e[`aria-hidden`]===!1)&&(v=!1);let y=()=>zl(f.current),b=()=>(d.current.modalRef=p.current,d.current.mount=f.current,d.current),S=()=>{jh.mount(b(),{disableScrollLock:r}),p.current&&(p.current.scrollTop=0)},C=Kl(()=>{let e=Oh(t)||y().body;jh.add(b(),e),p.current&&S()}),w=()=>jh.isTopModal(b()),T=Kl(e=>{f.current=e,e&&(l&&w()?S():p.current&&fh(p.current,v))}),E=x.useCallback(()=>{jh.remove(b(),v)},[v]);x.useEffect(()=>()=>{E()},[E]),x.useEffect(()=>{l?C():(!_||!i)&&E()},[l,E,_,i,C]);let D=e=>t=>{e.onKeyDown?.(t),!(t.key!==`Escape`||t.which===229||!w())&&(n||(t.stopPropagation(),c&&c(t,`escapeKeyDown`)))},O=e=>t=>{e.onClick?.(t),t.target===t.currentTarget&&c&&c(t,`backdropClick`)};return{getRootProps:(t={})=>{let n=Iu(e);delete n.onTransitionEnter,delete n.onTransitionExited;let r={...n,...t};return{role:`presentation`,...r,onKeyDown:D(r),ref:m}},getBackdropProps:(e={})=>{let t=e;return{"aria-hidden":!0,...t,onClick:O(t),open:l}},getTransitionProps:()=>({onEnter:wl(()=>{g(!1),a&&a()},s?.props.onEnter??Ah),onExited:wl(()=>{g(!0),o&&o(),i&&E()},s?.props.onExited??Ah)}),rootRef:m,portalRef:T,isTopModal:w,exited:h,hasTransition:_}}var Nh=Mh;function Ph(e){return Io(`MuiModal`,e)}Lo(`MuiModal`,[`root`,`hidden`,`backdrop`]);var Fh=e=>{let{open:t,exited:n,classes:r}=e;return cc({root:[`root`,!t&&n&&`hidden`],backdrop:[`backdrop`]},Ph,r)},Ih=H(`div`,{name:`MuiModal`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.open&&n.exited&&t.hidden]}})(kl(({theme:e})=>({position:`fixed`,zIndex:(e.vars||e).zIndex.modal,right:0,bottom:0,top:0,left:0,variants:[{props:({ownerState:e})=>!e.open&&e.exited,style:{visibility:`hidden`}}]}))),Lh=H(Vm,{name:`MuiModal`,slot:`Backdrop`})({zIndex:-1}),Rh=x.forwardRef(function(e,t){let n=Al({name:`MuiModal`,props:e}),{BackdropComponent:r=Lh,BackdropProps:i,classes:a,className:o,closeAfterTransition:s=!1,children:c,container:l,component:u,components:d={},componentsProps:f={},disableAutoFocus:p=!1,disableEnforceFocus:m=!1,disableEscapeKeyDown:h=!1,disablePortal:g=!1,disableRestoreFocus:_=!1,disableScrollLock:v=!1,hideBackdrop:y=!1,keepMounted:b=!1,onClose:S,onTransitionEnter:C,onTransitionExited:w,open:T,slotProps:E={},slots:D={},theme:O,...k}=n,A={...n,closeAfterTransition:s,disableAutoFocus:p,disableEnforceFocus:m,disableEscapeKeyDown:h,disablePortal:g,disableRestoreFocus:_,disableScrollLock:v,hideBackdrop:y,keepMounted:b},{getRootProps:j,getBackdropProps:M,getTransitionProps:N,portalRef:ee,isTopModal:P,exited:F,hasTransition:I}=Nh({...A,rootRef:t}),te={...A,exited:F},ne=Fh(te),re={};if(c.props.tabIndex===void 0&&(re.tabIndex=`-1`),I){let{onEnter:e,onExited:t}=N();re.onEnter=e,re.onExited=t}let ie={slots:{root:d.Root,backdrop:d.Backdrop,...D},slotProps:{...f,...E}},[ae,L]=Vu(`root`,{ref:t,elementType:Ih,externalForwardedProps:{...ie,...k,component:u},getSlotProps:j,ownerState:te,className:B(o,ne?.root,!te.open&&te.exited&&ne?.hidden)}),[oe,se]=Vu(`backdrop`,{ref:i?.ref,elementType:r,externalForwardedProps:ie,shouldForwardComponentProp:!0,additionalProps:i,getSlotProps:e=>M({...e,onClick:t=>{e?.onClick&&e.onClick(t)}}),className:B(i?.className,ne?.backdrop),ownerState:te});return!b&&!T&&(!I||F)?null:(0,R.jsx)(Qp,{ref:ee,container:l,disablePortal:g,children:(0,R.jsxs)(ae,{...L,children:[!y&&r?(0,R.jsx)(oe,{...se}):null,(0,R.jsx)(Dh,{disableEnforceFocus:m,disableAutoFocus:p,disableRestoreFocus:_,isEnabled:P,open:T,children:x.cloneElement(c,re)})]})})});function zh(e){return Io(`MuiDialog`,e)}var Bh=Lo(`MuiDialog`,[`root`,`scrollPaper`,`scrollBody`,`container`,`paper`,`paperScrollPaper`,`paperScrollBody`,`paperWidthFalse`,`paperWidthXs`,`paperWidthSm`,`paperWidthMd`,`paperWidthLg`,`paperWidthXl`,`paperFullWidth`,`paperFullScreen`]),Vh=x.createContext({}),Hh=H(Vm,{name:`MuiDialog`,slot:`Backdrop`,overrides:(e,t)=>t.backdrop})({zIndex:-1}),Uh=e=>{let{classes:t,scroll:n,maxWidth:r,fullWidth:i,fullScreen:a}=e;return cc({root:[`root`],container:[`container`,`scroll${U(n)}`],paper:[`paper`,`paperScroll${U(n)}`,`paperWidth${U(String(r))}`,i&&`paperFullWidth`,a&&`paperFullScreen`]},zh,t)},Wh=H(Rh,{name:`MuiDialog`,slot:`Root`})({"@media print":{position:`absolute !important`}}),Gh=H(`div`,{name:`MuiDialog`,slot:`Container`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.container,t[`scroll${U(n.scroll)}`]]}})({height:`100%`,"@media print":{height:`auto`},outline:0,variants:[{props:{scroll:`paper`},style:{display:`flex`,justifyContent:`center`,alignItems:`center`}},{props:{scroll:`body`},style:{overflowY:`auto`,overflowX:`hidden`,textAlign:`center`,"&::after":{content:`""`,display:`inline-block`,verticalAlign:`middle`,height:`100%`,width:`0`}}}]}),Kh=H(Qu,{name:`MuiDialog`,slot:`Paper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.paper,t[`scrollPaper${U(n.scroll)}`],t[`paperWidth${U(String(n.maxWidth))}`],n.fullWidth&&t.paperFullWidth,n.fullScreen&&t.paperFullScreen]}})(kl(({theme:e})=>({margin:32,position:`relative`,overflowY:`auto`,"@media print":{overflowY:`visible`,boxShadow:`none`},variants:[{props:{scroll:`paper`},style:{display:`flex`,flexDirection:`column`,maxHeight:`calc(100% - 64px)`}},{props:{scroll:`body`},style:{display:`inline-block`,verticalAlign:`middle`,textAlign:`initial`}},{props:({ownerState:e})=>!e.maxWidth,style:{maxWidth:`calc(100% - 64px)`}},{props:{maxWidth:`xs`},style:{maxWidth:e.breakpoints.unit===`px`?Math.max(e.breakpoints.values.xs,444):`max(${e.breakpoints.values.xs}${e.breakpoints.unit}, 444px)`,[`&.${Bh.paperScrollBody}`]:{[e.breakpoints.down(Math.max(e.breakpoints.values.xs,444)+64)]:{maxWidth:`calc(100% - 64px)`}}}},...Object.keys(e.breakpoints.values).filter(e=>e!==`xs`).map(t=>({props:{maxWidth:t},style:{maxWidth:`${e.breakpoints.values[t]}${e.breakpoints.unit}`,[`&.${Bh.paperScrollBody}`]:{[e.breakpoints.down(e.breakpoints.values[t]+64)]:{maxWidth:`calc(100% - 64px)`}}}})),{props:({ownerState:e})=>e.fullWidth,style:{width:`calc(100% - 64px)`}},{props:({ownerState:e})=>e.fullScreen,style:{margin:0,width:`100%`,maxWidth:`100%`,height:`100%`,maxHeight:`none`,borderRadius:0,[`&.${Bh.paperScrollBody}`]:{margin:0,maxWidth:`100%`}}}]}))),qh=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiDialog`}),r=pl(),i={enter:r.transitions.duration.enteringScreen,exit:r.transitions.duration.leavingScreen},{"aria-describedby":a,"aria-labelledby":o,"aria-modal":s=!0,BackdropComponent:c,BackdropProps:l,children:u,className:d,disableEscapeKeyDown:f=!1,fullScreen:p=!1,fullWidth:m=!1,maxWidth:h=`sm`,onClick:g,onClose:_,open:v,PaperComponent:y=Qu,PaperProps:b={},scroll:S=`paper`,slots:C={},slotProps:w={},TransitionComponent:T=Lm,transitionDuration:E=i,TransitionProps:D,...O}=n,k={...n,disableEscapeKeyDown:f,fullScreen:p,fullWidth:m,maxWidth:h,scroll:S},A=Uh(k),j=x.useRef(),M=e=>{j.current=e.target===e.currentTarget},N=e=>{g&&g(e),j.current&&(j.current=null,_&&_(e,`backdropClick`))},ee=Fs(o),P=x.useMemo(()=>({titleId:ee}),[ee]),F={slots:{transition:T,...C},slotProps:{transition:D,paper:b,backdrop:l,...w}},[I,te]=Vu(`root`,{elementType:Wh,shouldForwardComponentProp:!0,externalForwardedProps:F,ownerState:k,className:B(A.root,d),ref:t}),[ne,re]=Vu(`backdrop`,{elementType:Hh,shouldForwardComponentProp:!0,externalForwardedProps:F,ownerState:k}),[ie,ae]=Vu(`paper`,{elementType:Kh,shouldForwardComponentProp:!0,externalForwardedProps:F,ownerState:k,className:B(A.paper,b.className)}),[L,oe]=Vu(`container`,{elementType:Gh,externalForwardedProps:F,ownerState:k,className:A.container}),[se,ce]=Vu(`transition`,{elementType:Lm,externalForwardedProps:F,ownerState:k,additionalProps:{appear:!0,in:v,timeout:E,role:`presentation`}});return(0,R.jsx)(I,{closeAfterTransition:!0,slots:{backdrop:ne},slotProps:{backdrop:{transitionDuration:E,as:c,...re}},disableEscapeKeyDown:f,onClose:_,open:v,onClick:N,...te,...O,children:(0,R.jsx)(se,{...ce,children:(0,R.jsx)(L,{onMouseDown:M,...oe,children:(0,R.jsx)(ie,{as:y,elevation:24,role:`dialog`,"aria-describedby":a,"aria-labelledby":ee,"aria-modal":s,...ae,children:(0,R.jsx)(Vh.Provider,{value:P,children:u})})})})})});function Jh(e){return Io(`MuiDialogContent`,e)}Lo(`MuiDialogContent`,[`root`,`dividers`]);var Yh=Lo(`MuiDialogTitle`,[`root`]),Xh=e=>{let{classes:t,dividers:n}=e;return cc({root:[`root`,n&&`dividers`]},Jh,t)},Zh=H(`div`,{name:`MuiDialogContent`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.dividers&&t.dividers]}})(kl(({theme:e})=>({flex:`1 1 auto`,WebkitOverflowScrolling:`touch`,overflowY:`auto`,padding:`20px 24px`,variants:[{props:({ownerState:e})=>e.dividers,style:{padding:`16px 24px`,borderTop:`1px solid ${(e.vars||e).palette.divider}`,borderBottom:`1px solid ${(e.vars||e).palette.divider}`}},{props:({ownerState:e})=>!e.dividers,style:{[`.${Yh.root} + &`]:{paddingTop:0}}}]}))),Qh=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiDialogContent`}),{className:r,dividers:i=!1,...a}=n,o={...n,dividers:i};return(0,R.jsx)(Zh,{className:B(Xh(o).root,r),ownerState:o,ref:t,...a})});function $h(e){return Io(`MuiDivider`,e)}Lo(`MuiDivider`,[`root`,`absolute`,`fullWidth`,`inset`,`middle`,`flexItem`,`light`,`vertical`,`withChildren`,`withChildrenVertical`,`textAlignRight`,`textAlignLeft`,`wrapper`,`wrapperVertical`]);var eg=e=>{let{absolute:t,children:n,classes:r,flexItem:i,light:a,orientation:o,textAlign:s,variant:c}=e;return cc({root:[`root`,t&&`absolute`,c,a&&`light`,o===`vertical`&&`vertical`,i&&`flexItem`,n&&`withChildren`,n&&o===`vertical`&&`withChildrenVertical`,s===`right`&&o!==`vertical`&&`textAlignRight`,s===`left`&&o!==`vertical`&&`textAlignLeft`],wrapper:[`wrapper`,o===`vertical`&&`wrapperVertical`]},$h,r)},tg=H(`div`,{name:`MuiDivider`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.absolute&&t.absolute,t[n.variant],n.light&&t.light,n.orientation===`vertical`&&t.vertical,n.flexItem&&t.flexItem,n.children&&t.withChildren,n.children&&n.orientation===`vertical`&&t.withChildrenVertical,n.textAlign===`right`&&n.orientation!==`vertical`&&t.textAlignRight,n.textAlign===`left`&&n.orientation!==`vertical`&&t.textAlignLeft]}})(kl(({theme:e})=>({margin:0,flexShrink:0,borderWidth:0,borderStyle:`solid`,borderColor:(e.vars||e).palette.divider,borderBottomWidth:`thin`,variants:[{props:{absolute:!0},style:{position:`absolute`,bottom:0,left:0,width:`100%`}},{props:{light:!0},style:{borderColor:e.alpha((e.vars||e).palette.divider,.08)}},{props:{variant:`inset`},style:{marginLeft:72}},{props:{variant:`middle`,orientation:`horizontal`},style:{marginLeft:e.spacing(2),marginRight:e.spacing(2)}},{props:{variant:`middle`,orientation:`vertical`},style:{marginTop:e.spacing(1),marginBottom:e.spacing(1)}},{props:{orientation:`vertical`},style:{height:`100%`,borderBottomWidth:0,borderRightWidth:`thin`}},{props:{flexItem:!0},style:{alignSelf:`stretch`,height:`auto`}},{props:({ownerState:e})=>!!e.children,style:{display:`flex`,textAlign:`center`,border:0,borderTopStyle:`solid`,borderLeftStyle:`solid`,"&::before, &::after":{content:`""`,alignSelf:`center`}}},{props:({ownerState:e})=>e.children&&e.orientation!==`vertical`,style:{"&::before, &::after":{width:`100%`,borderTop:`thin solid ${(e.vars||e).palette.divider}`,borderTopStyle:`inherit`}}},{props:({ownerState:e})=>e.orientation===`vertical`&&e.children,style:{flexDirection:`column`,"&::before, &::after":{height:`100%`,borderLeft:`thin solid ${(e.vars||e).palette.divider}`,borderLeftStyle:`inherit`}}},{props:({ownerState:e})=>e.textAlign===`right`&&e.orientation!==`vertical`,style:{"&::before":{width:`90%`},"&::after":{width:`10%`}}},{props:({ownerState:e})=>e.textAlign===`left`&&e.orientation!==`vertical`,style:{"&::before":{width:`10%`},"&::after":{width:`90%`}}}]}))),ng=H(`span`,{name:`MuiDivider`,slot:`Wrapper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.wrapper,n.orientation===`vertical`&&t.wrapperVertical]}})(kl(({theme:e})=>({display:`inline-block`,paddingLeft:`calc(${e.spacing(1)} * 1.2)`,paddingRight:`calc(${e.spacing(1)} * 1.2)`,whiteSpace:`nowrap`,variants:[{props:{orientation:`vertical`},style:{paddingTop:`calc(${e.spacing(1)} * 1.2)`,paddingBottom:`calc(${e.spacing(1)} * 1.2)`}}]}))),rg=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiDivider`}),{absolute:r=!1,children:i,className:a,orientation:o=`horizontal`,component:s=i||o===`vertical`?`div`:`hr`,flexItem:c=!1,light:l=!1,role:u=s===`hr`?void 0:`separator`,textAlign:d=`center`,variant:f=`fullWidth`,...p}=n,m={...n,absolute:r,component:s,flexItem:c,light:l,orientation:o,role:u,textAlign:d,variant:f},h=eg(m);return(0,R.jsx)(tg,{as:s,className:B(h.root,a),role:u,ref:t,ownerState:m,"aria-orientation":u===`separator`&&(s!==`hr`||o===`vertical`)?o:void 0,...p,children:i?(0,R.jsx)(ng,{className:h.wrapper,ownerState:m,children:i}):null})});rg&&(rg.muiSkipListHighlight=!0);var ig=rg;function ag(e){return`scale(${e}, ${e**2})`}var og={entering:{opacity:1,transform:ag(1)},entered:{opacity:1,transform:`none`}},sg=typeof navigator<`u`&&/^((?!chrome|android).)*(safari|mobile)/i.test(navigator.userAgent)&&/(os |version\/)15(.|_)4/i.test(navigator.userAgent),cg=x.forwardRef(function(e,t){let{addEndListener:n,appear:r=!0,children:i,easing:a,in:o,onEnter:s,onEntered:c,onEntering:l,onExit:u,onExited:d,onExiting:f,style:p,timeout:m=`auto`,TransitionComponent:h=uu,...g}=e,_=Eu(),v=x.useRef(),y=pl(),b=x.useRef(null),S=Yl(b,Yp(i),t),C=e=>t=>{if(e){let n=b.current;t===void 0?e(n):e(n,t)}},w=C(l),T=C((e,t)=>{Du(e);let{duration:n,delay:r,easing:i}=Ou({style:p,timeout:m,easing:a},{mode:`enter`}),o;m===`auto`?(o=y.transitions.getAutoHeightDuration(e.clientHeight),v.current=o):o=n,e.style.transition=[y.transitions.create(`opacity`,{duration:o,delay:r}),y.transitions.create(`transform`,{duration:sg?o:o*.666,delay:r,easing:i})].join(`,`),s&&s(e,t)}),E=C(c),D=C(f),O=C(e=>{let{duration:t,delay:n,easing:r}=Ou({style:p,timeout:m,easing:a},{mode:`exit`}),i;m===`auto`?(i=y.transitions.getAutoHeightDuration(e.clientHeight),v.current=i):i=t,e.style.transition=[y.transitions.create(`opacity`,{duration:i,delay:n}),y.transitions.create(`transform`,{duration:sg?i:i*.666,delay:sg?n:n||i*.333,easing:r})].join(`,`),e.style.opacity=0,e.style.transform=ag(.75),u&&u(e)}),k=C(d);return(0,R.jsx)(h,{appear:r,in:o,nodeRef:b,onEnter:T,onEntered:E,onEntering:w,onExit:O,onExited:k,onExiting:D,addEndListener:e=>{m===`auto`&&_.start(v.current||0,e),n&&n(b.current,e)},timeout:m===`auto`?null:m,...g,children:(e,{ownerState:t,...n})=>x.cloneElement(i,{style:{opacity:0,transform:ag(.75),visibility:e===`exited`&&!o?`hidden`:void 0,...og[e],...p,...i.props.style},ref:S,...n})})});cg&&(cg.muiSupportAuto=!0);var lg=cg;function ug(e){return Io(`MuiLink`,e)}var dg=Lo(`MuiLink`,[`root`,`underlineNone`,`underlineHover`,`underlineAlways`,`button`,`focusVisible`]),fg=({theme:e,ownerState:t})=>{let n=t.color;if(`colorSpace`in e&&e.colorSpace){let r=pa(e,`palette.${n}.main`)||pa(e,`palette.${n}`)||t.color;return e.alpha(r,.4)}let r=pa(e,`palette.${n}.main`,!1)||pa(e,`palette.${n}`,!1)||t.color,i=pa(e,`palette.${n}.mainChannel`)||pa(e,`palette.${n}Channel`);return`vars`in e&&i?`rgba(${i} / 0.4)`:ls(r,.4)},pg={primary:!0,secondary:!0,error:!0,info:!0,success:!0,warning:!0,textPrimary:!0,textSecondary:!0,textDisabled:!0},mg=e=>{let{classes:t,component:n,focusVisible:r,underline:i}=e;return cc({root:[`root`,`underline${U(i)}`,n===`button`&&`button`,r&&`focusVisible`]},ug,t)},hg=H(K,{name:`MuiLink`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,t[`underline${U(n.underline)}`],n.component===`button`&&t.button]}})(kl(({theme:e})=>({variants:[{props:{underline:`none`},style:{textDecoration:`none`}},{props:{underline:`hover`},style:{textDecoration:`none`,"&:hover":{textDecoration:`underline`}}},{props:{underline:`always`},style:{textDecoration:`underline`,"&:hover":{textDecorationColor:`inherit`}}},{props:({underline:e,ownerState:t})=>e===`always`&&t.color!==`inherit`,style:{textDecorationColor:`var(--Link-underlineColor)`}},{props:({underline:e,ownerState:t})=>e===`always`&&t.color===`inherit`,style:e.colorSpace?{textDecorationColor:e.alpha(`currentColor`,.4)}:null},...Object.entries(e.palette).filter(Sd()).map(([t])=>({props:{underline:`always`,color:t},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette[t].main,.4)}})),{props:{underline:`always`,color:`textPrimary`},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette.text.primary,.4)}},{props:{underline:`always`,color:`textSecondary`},style:{"--Link-underlineColor":e.alpha((e.vars||e).palette.text.secondary,.4)}},{props:{underline:`always`,color:`textDisabled`},style:{"--Link-underlineColor":(e.vars||e).palette.text.disabled}},{props:{component:`button`},style:{position:`relative`,WebkitTapHighlightColor:`transparent`,backgroundColor:`transparent`,outline:0,border:0,margin:0,borderRadius:0,padding:0,cursor:`pointer`,userSelect:`none`,verticalAlign:`middle`,MozAppearance:`none`,WebkitAppearance:`none`,"&::-moz-focus-inner":{borderStyle:`none`},[`&.${dg.focusVisible}`]:{outline:`auto`}}}]}))),gg=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiLink`}),r=pl(),{className:i,color:a=`primary`,component:o=`a`,onBlur:s,onFocus:c,TypographyClasses:l,underline:u=`always`,variant:d=`inherit`,sx:f,...p}=n,[m,h]=x.useState(!1),g=e=>{$u(e.target)||h(!1),s&&s(e)},_=e=>{$u(e.target)&&h(!0),c&&c(e)},v={...n,color:a,component:o,focusVisible:m,underline:u,variant:d};return(0,R.jsx)(hg,{color:a,className:B(mg(v).root,i),classes:l,component:o,onBlur:g,onFocus:_,ref:t,ownerState:v,variant:d,...p,sx:[...pg[a]===void 0?[{color:a}]:[],...Array.isArray(f)?f:[f]],style:{...p.style,...u===`always`&&a!==`inherit`&&!pg[a]&&{"--Link-underlineColor":fg({theme:r,ownerState:v})}}})}),_g=x.createContext({});function vg(e){return Io(`MuiList`,e)}Lo(`MuiList`,[`root`,`padding`,`dense`,`subheader`]);var yg=e=>{let{classes:t,disablePadding:n,dense:r,subheader:i}=e;return cc({root:[`root`,!n&&`padding`,r&&`dense`,i&&`subheader`]},vg,t)},bg=H(`ul`,{name:`MuiList`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.disablePadding&&t.padding,n.dense&&t.dense,n.subheader&&t.subheader]}})({listStyle:`none`,margin:0,padding:0,position:`relative`,variants:[{props:({ownerState:e})=>!e.disablePadding,style:{paddingTop:8,paddingBottom:8}},{props:({ownerState:e})=>e.subheader,style:{paddingTop:0}}]}),xg=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiList`}),{children:r,className:i,component:a=`ul`,dense:o=!1,disablePadding:s=!1,subheader:c,...l}=n,u=x.useMemo(()=>({dense:o}),[o]),d={...n,component:a,dense:o,disablePadding:s},f=yg(d);return(0,R.jsx)(_g.Provider,{value:u,children:(0,R.jsxs)(bg,{as:a,className:B(f.root,i),ref:t,ownerState:d,...l,children:[c,r]})})});function Sg(e){return Io(`MuiListItem`,e)}Lo(`MuiListItem`,[`root`,`container`,`dense`,`alignItemsFlexStart`,`divider`,`gutters`,`padding`,`secondaryAction`]);function Cg(e){return Io(`MuiListItemButton`,e)}var wg=Lo(`MuiListItemButton`,[`root`,`focusVisible`,`dense`,`alignItemsFlexStart`,`disabled`,`divider`,`gutters`,`selected`]);const Tg=(e,t)=>{let{ownerState:n}=e;return[t.root,n.dense&&t.dense,n.alignItems===`flex-start`&&t.alignItemsFlexStart,n.divider&&t.divider,!n.disableGutters&&t.gutters]};var Eg=e=>{let{alignItems:t,classes:n,dense:r,disabled:i,disableGutters:a,divider:o,selected:s}=e,c=cc({root:[`root`,r&&`dense`,!a&&`gutters`,o&&`divider`,i&&`disabled`,t===`flex-start`&&`alignItemsFlexStart`,s&&`selected`]},Cg,n);return{...n,...c}},Dg=H(yd,{shouldForwardProp:e=>gl(e)||e===`classes`,name:`MuiListItemButton`,slot:`Root`,overridesResolver:Tg})(kl(({theme:e})=>({display:`flex`,flexGrow:1,justifyContent:`flex-start`,alignItems:`center`,position:`relative`,textDecoration:`none`,minWidth:0,boxSizing:`border-box`,textAlign:`left`,paddingTop:8,paddingBottom:8,transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),"&:hover":{textDecoration:`none`,backgroundColor:(e.vars||e).palette.action.hover,"@media (hover: none)":{backgroundColor:`transparent`}},[`&.${wg.selected}`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,(e.vars||e).palette.action.selectedOpacity),[`&.${wg.focusVisible}`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.focusOpacity}`)}},[`&.${wg.selected}:hover`]:{backgroundColor:e.alpha((e.vars||e).palette.primary.main,`${(e.vars||e).palette.action.selectedOpacity} + ${(e.vars||e).palette.action.hoverOpacity}`),"@media (hover: none)":{backgroundColor:e.alpha((e.vars||e).palette.primary.main,(e.vars||e).palette.action.selectedOpacity)}},[`&.${wg.focusVisible}`]:{backgroundColor:(e.vars||e).palette.action.focus},[`&.${wg.disabled}`]:{opacity:(e.vars||e).palette.action.disabledOpacity},variants:[{props:({ownerState:e})=>e.divider,style:{borderBottom:`1px solid ${(e.vars||e).palette.divider}`,backgroundClip:`padding-box`}},{props:{alignItems:`flex-start`},style:{alignItems:`flex-start`}},{props:({ownerState:e})=>!e.disableGutters,style:{paddingLeft:16,paddingRight:16}},{props:({ownerState:e})=>e.dense,style:{paddingTop:4,paddingBottom:4}}]}))),Og=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiListItemButton`}),{alignItems:r=`center`,autoFocus:i=!1,component:a=`div`,children:o,dense:s=!1,disableGutters:c=!1,divider:l=!1,focusVisibleClassName:u,selected:d=!1,className:f,...p}=n,m=x.useContext(_g),h=x.useMemo(()=>({dense:s||m.dense||!1,alignItems:r,disableGutters:c}),[r,m.dense,s,c]),g=x.useRef(null);Hl(()=>{i&&g.current&&g.current.focus()},[i]);let _={...n,alignItems:r,dense:h.dense,disableGutters:c,divider:l,selected:d},v=Eg(_),y=Yl(g,t);return(0,R.jsx)(_g.Provider,{value:h,children:(0,R.jsx)(Dg,{ref:y,href:p.href||p.to,component:(p.href||p.to)&&a===`div`?`button`:a,focusVisibleClassName:B(v.focusVisible,u),ownerState:_,className:B(v.root,f),...p,classes:v,children:o})})});function kg(e){return Io(`MuiListItemSecondaryAction`,e)}Lo(`MuiListItemSecondaryAction`,[`root`,`disableGutters`]);var Ag=e=>{let{disableGutters:t,classes:n}=e;return cc({root:[`root`,t&&`disableGutters`]},kg,n)},jg=H(`div`,{name:`MuiListItemSecondaryAction`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,n.disableGutters&&t.disableGutters]}})({position:`absolute`,right:16,top:`50%`,transform:`translateY(-50%)`,variants:[{props:({ownerState:e})=>e.disableGutters,style:{right:0}}]}),Mg=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiListItemSecondaryAction`}),{className:r,...i}=n,a=x.useContext(_g),o={...n,disableGutters:a.disableGutters};return(0,R.jsx)(jg,{className:B(Ag(o).root,r),ownerState:o,ref:t,...i})});Mg.muiName=`ListItemSecondaryAction`;var Ng=Mg;const Pg=(e,t)=>{let{ownerState:n}=e;return[t.root,n.dense&&t.dense,n.alignItems===`flex-start`&&t.alignItemsFlexStart,n.divider&&t.divider,!n.disableGutters&&t.gutters,!n.disablePadding&&t.padding,n.hasSecondaryAction&&t.secondaryAction]};var Fg=e=>{let{alignItems:t,classes:n,dense:r,disableGutters:i,disablePadding:a,divider:o,hasSecondaryAction:s}=e;return cc({root:[`root`,r&&`dense`,!i&&`gutters`,!a&&`padding`,o&&`divider`,t===`flex-start`&&`alignItemsFlexStart`,s&&`secondaryAction`],container:[`container`]},Sg,n)};const Ig=H(`div`,{name:`MuiListItem`,slot:`Root`,overridesResolver:Pg})(kl(({theme:e})=>({display:`flex`,justifyContent:`flex-start`,alignItems:`center`,position:`relative`,textDecoration:`none`,width:`100%`,boxSizing:`border-box`,textAlign:`left`,variants:[{props:({ownerState:e})=>!e.disablePadding,style:{paddingTop:8,paddingBottom:8}},{props:({ownerState:e})=>!e.disablePadding&&e.dense,style:{paddingTop:4,paddingBottom:4}},{props:({ownerState:e})=>!e.disablePadding&&!e.disableGutters,style:{paddingLeft:16,paddingRight:16}},{props:({ownerState:e})=>!e.disablePadding&&!!e.secondaryAction,style:{paddingRight:48}},{props:({ownerState:e})=>!!e.secondaryAction,style:{[`& > .${wg.root}`]:{paddingRight:48}}},{props:{alignItems:`flex-start`},style:{alignItems:`flex-start`}},{props:({ownerState:e})=>e.divider,style:{borderBottom:`1px solid ${(e.vars||e).palette.divider}`,backgroundClip:`padding-box`}},{props:({ownerState:e})=>e.button,style:{transition:e.transitions.create(`background-color`,{duration:e.transitions.duration.shortest}),"&:hover":{textDecoration:`none`,backgroundColor:(e.vars||e).palette.action.hover,"@media (hover: none)":{backgroundColor:`transparent`}}}},{props:({ownerState:e})=>e.hasSecondaryAction,style:{paddingRight:48}}]})));var Lg=H(`li`,{name:`MuiListItem`,slot:`Container`})({position:`relative`}),J=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiListItem`}),{alignItems:r=`center`,children:i,className:a,component:o,components:s={},componentsProps:c={},ContainerComponent:l=`li`,ContainerProps:{className:u,...d}={},dense:f=!1,disableGutters:p=!1,disablePadding:m=!1,divider:h=!1,secondaryAction:g,slotProps:_={},slots:v={},...y}=n,b=x.useContext(_g),S=x.useMemo(()=>({dense:f||b.dense||!1,alignItems:r,disableGutters:p}),[r,b.dense,f,p]),C=x.useRef(null),w=x.Children.toArray(i),T=w.length&&Rl(w[w.length-1],[`ListItemSecondaryAction`]),E={...n,alignItems:r,dense:S.dense,disableGutters:p,disablePadding:m,divider:h,hasSecondaryAction:T},D=Fg(E),O=Yl(C,t),k=v.root||s.Root||Ig,A=_.root||c.root||{},j={className:B(D.root,A.className,a),...y},M=o||`li`;return T?(M=!j.component&&!o?`div`:M,l===`li`&&(M===`li`?M=`div`:j.component===`li`&&(j.component=`div`)),(0,R.jsx)(_g.Provider,{value:S,children:(0,R.jsxs)(Lg,{as:l,className:B(D.container,u),ref:O,ownerState:E,...d,children:[(0,R.jsx)(k,{...A,...!Au(k)&&{as:M,ownerState:{...E,...A.ownerState}},...j,children:w}),w.pop()]})})):(0,R.jsx)(_g.Provider,{value:S,children:(0,R.jsxs)(k,{...A,as:M,ref:O,...!Au(k)&&{ownerState:{...E,...A.ownerState}},...j,children:[w,g&&(0,R.jsx)(Ng,{children:g})]})})});function Rg(e){return Io(`MuiListItemText`,e)}var zg=Lo(`MuiListItemText`,[`root`,`multiline`,`dense`,`inset`,`primary`,`secondary`]),Bg=e=>{let{classes:t,inset:n,primary:r,secondary:i,dense:a}=e;return cc({root:[`root`,n&&`inset`,a&&`dense`,r&&i&&`multiline`],primary:[`primary`],secondary:[`secondary`]},Rg,t)},Vg=H(`div`,{name:`MuiListItemText`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[{[`& .${zg.primary}`]:t.primary},{[`& .${zg.secondary}`]:t.secondary},t.root,n.inset&&t.inset,n.primary&&n.secondary&&t.multiline,n.dense&&t.dense]}})({flex:`1 1 auto`,minWidth:0,marginTop:4,marginBottom:4,[`.${Vd.root}:where(& .${zg.primary})`]:{display:`block`},[`.${Vd.root}:where(& .${zg.secondary})`]:{display:`block`},variants:[{props:({ownerState:e})=>e.primary&&e.secondary,style:{marginTop:6,marginBottom:6}},{props:({ownerState:e})=>e.inset,style:{paddingLeft:56}}]}),Y=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiListItemText`}),{children:r,className:i,disableTypography:a=!1,inset:o=!1,primary:s,primaryTypographyProps:c,secondary:l,secondaryTypographyProps:u,slots:d={},slotProps:f={},...p}=n,{dense:m}=x.useContext(_g),h=s??r,g=l,_={...n,disableTypography:a,inset:o,primary:!!h,secondary:!!g,dense:m},v=Bg(_),y={slots:d,slotProps:{primary:c,secondary:u,...f}},[b,S]=Vu(`root`,{className:B(v.root,i),elementType:Vg,externalForwardedProps:{...y,...p},ownerState:_,ref:t}),[C,w]=Vu(`primary`,{className:v.primary,elementType:K,externalForwardedProps:y,ownerState:_}),[T,E]=Vu(`secondary`,{className:v.secondary,elementType:K,externalForwardedProps:y,ownerState:_});return h!=null&&h.type!==K&&!a&&(h=(0,R.jsx)(C,{variant:m?`body2`:`body1`,component:w?.variant?void 0:`span`,...w,children:h})),g!=null&&g.type!==K&&!a&&(g=(0,R.jsx)(T,{variant:`body2`,color:`textSecondary`,...E,children:g})),(0,R.jsxs)(b,{...S,children:[h,g]})});function Hg(e){return Io(`MuiTooltip`,e)}var Ug=Lo(`MuiTooltip`,[`popper`,`popperInteractive`,`popperArrow`,`popperClose`,`tooltip`,`tooltipArrow`,`touch`,`tooltipPlacementLeft`,`tooltipPlacementRight`,`tooltipPlacementTop`,`tooltipPlacementBottom`,`arrow`]);function Wg(e){return Math.round(e*1e5)/1e5}var Gg=e=>{let{classes:t,disableInteractive:n,arrow:r,touch:i,placement:a}=e;return cc({popper:[`popper`,!n&&`popperInteractive`,r&&`popperArrow`],tooltip:[`tooltip`,r&&`tooltipArrow`,i&&`touch`,`tooltipPlacement${U(a.split(`-`)[0])}`],arrow:[`arrow`]},Hg,t)},Kg=H(sm,{name:`MuiTooltip`,slot:`Popper`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.popper,!n.disableInteractive&&t.popperInteractive,n.arrow&&t.popperArrow,!n.open&&t.popperClose]}})(kl(({theme:e})=>({zIndex:(e.vars||e).zIndex.tooltip,pointerEvents:`none`,variants:[{props:({ownerState:e})=>!e.disableInteractive,style:{pointerEvents:`auto`}},{props:({open:e})=>!e,style:{pointerEvents:`none`}},{props:({ownerState:e})=>e.arrow,style:{[`&[data-popper-placement*="bottom"] .${Ug.arrow}`]:{top:0,marginTop:`-0.71em`,"&::before":{transformOrigin:`0 100%`}},[`&[data-popper-placement*="top"] .${Ug.arrow}`]:{bottom:0,marginBottom:`-0.71em`,"&::before":{transformOrigin:`100% 0`}},[`&[data-popper-placement*="right"] .${Ug.arrow}`]:{height:`1em`,width:`0.71em`,"&::before":{transformOrigin:`100% 100%`}},[`&[data-popper-placement*="left"] .${Ug.arrow}`]:{height:`1em`,width:`0.71em`,"&::before":{transformOrigin:`0 0`}}}},{props:({ownerState:e})=>e.arrow&&!e.isRtl,style:{[`&[data-popper-placement*="right"] .${Ug.arrow}`]:{left:0,marginLeft:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!!e.isRtl,style:{[`&[data-popper-placement*="right"] .${Ug.arrow}`]:{right:0,marginRight:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!e.isRtl,style:{[`&[data-popper-placement*="left"] .${Ug.arrow}`]:{right:0,marginRight:`-0.71em`}}},{props:({ownerState:e})=>e.arrow&&!!e.isRtl,style:{[`&[data-popper-placement*="left"] .${Ug.arrow}`]:{left:0,marginLeft:`-0.71em`}}}]}))),qg=H(`div`,{name:`MuiTooltip`,slot:`Tooltip`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.tooltip,n.touch&&t.touch,n.arrow&&t.tooltipArrow,t[`tooltipPlacement${U(n.placement.split(`-`)[0])}`]]}})(kl(({theme:e})=>({backgroundColor:e.vars?e.vars.palette.Tooltip.bg:e.alpha(e.palette.grey[700],.92),borderRadius:(e.vars||e).shape.borderRadius,color:(e.vars||e).palette.common.white,fontFamily:e.typography.fontFamily,padding:`4px 8px`,fontSize:e.typography.pxToRem(11),maxWidth:300,margin:2,wordWrap:`break-word`,fontWeight:e.typography.fontWeightMedium,[`.${Ug.popper}[data-popper-placement*="left"] &`]:{transformOrigin:`right center`},[`.${Ug.popper}[data-popper-placement*="right"] &`]:{transformOrigin:`left center`},[`.${Ug.popper}[data-popper-placement*="top"] &`]:{transformOrigin:`center bottom`,marginBottom:`14px`},[`.${Ug.popper}[data-popper-placement*="bottom"] &`]:{transformOrigin:`center top`,marginTop:`14px`},variants:[{props:({ownerState:e})=>e.arrow,style:{position:`relative`,margin:0}},{props:({ownerState:e})=>e.touch,style:{padding:`8px 16px`,fontSize:e.typography.pxToRem(14),lineHeight:`${Wg(16/14)}em`,fontWeight:e.typography.fontWeightRegular}},{props:({ownerState:e})=>!e.isRtl,style:{[`.${Ug.popper}[data-popper-placement*="left"] &`]:{marginRight:`14px`},[`.${Ug.popper}[data-popper-placement*="right"] &`]:{marginLeft:`14px`}}},{props:({ownerState:e})=>!e.isRtl&&e.touch,style:{[`.${Ug.popper}[data-popper-placement*="left"] &`]:{marginRight:`24px`},[`.${Ug.popper}[data-popper-placement*="right"] &`]:{marginLeft:`24px`}}},{props:({ownerState:e})=>!!e.isRtl,style:{[`.${Ug.popper}[data-popper-placement*="left"] &`]:{marginLeft:`14px`},[`.${Ug.popper}[data-popper-placement*="right"] &`]:{marginRight:`14px`}}},{props:({ownerState:e})=>!!e.isRtl&&e.touch,style:{[`.${Ug.popper}[data-popper-placement*="left"] &`]:{marginLeft:`24px`},[`.${Ug.popper}[data-popper-placement*="right"] &`]:{marginRight:`24px`}}},{props:({ownerState:e})=>e.touch,style:{[`.${Ug.popper}[data-popper-placement*="top"] &`]:{marginBottom:`24px`}}},{props:({ownerState:e})=>e.touch,style:{[`.${Ug.popper}[data-popper-placement*="bottom"] &`]:{marginTop:`24px`}}}]}))),Jg=H(`span`,{name:`MuiTooltip`,slot:`Arrow`})(kl(({theme:e})=>({overflow:`hidden`,position:`absolute`,width:`1em`,height:`0.71em`,boxSizing:`border-box`,color:e.vars?e.vars.palette.Tooltip.bg:e.alpha(e.palette.grey[700],.9),"&::before":{content:`""`,margin:`auto`,display:`block`,width:`100%`,height:`100%`,backgroundColor:`currentColor`,transform:`rotate(45deg)`}}))),Yg=!1,Xg=new Tu,Zg={x:0,y:0};function Qg(e,t){return(n,...r)=>{t&&t(n,...r),e(n,...r)}}var $g=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiTooltip`}),{arrow:r=!1,children:i,classes:a,components:o={},componentsProps:s={},describeChild:c=!1,disableFocusListener:l=!1,disableHoverListener:u=!1,disableInteractive:d=!1,disableTouchListener:f=!1,enterDelay:p=100,enterNextDelay:m=0,enterTouchDelay:h=700,followCursor:g=!1,id:_,leaveDelay:v=0,leaveTouchDelay:y=1500,onClose:b,onOpen:S,open:C,placement:w=`bottom`,PopperComponent:T,PopperProps:E={},slotProps:D={},slots:O={},title:k,TransitionComponent:A,TransitionProps:j,...M}=n,N=x.isValidElement(i)?i:(0,R.jsx)(`span`,{children:i}),ee=pl(),P=Ts(),[F,I]=x.useState(),[te,ne]=x.useState(null),re=x.useRef(!1),ie=d||g,ae=Eu(),L=Eu(),oe=Eu(),se=Eu(),[ce,le]=Wl({controlled:C,default:!1,name:`Tooltip`,state:`open`}),ue=ce,de=W(_),fe=x.useRef(),pe=ql(()=>{fe.current!==void 0&&(document.body.style.WebkitUserSelect=fe.current,fe.current=void 0),se.clear()});x.useEffect(()=>pe,[pe]);let me=e=>{Xg.clear(),Yg=!0,le(!0),S&&!ue&&S(e)},he=ql(e=>{Xg.start(800+v,()=>{Yg=!1}),le(!1),b&&ue&&b(e),ae.start(ee.transitions.duration.shortest,()=>{re.current=!1})}),ge=e=>{re.current&&e.type!==`touchstart`||(F&&F.removeAttribute(`title`),L.clear(),oe.clear(),p||Yg&&m?L.start(Yg?m:p,()=>{me(e)}):me(e))},_e=e=>{L.clear(),oe.start(v,()=>{he(e)})},[,ve]=x.useState(!1),ye=e=>{$u(e.target)||(ve(!1),_e(e))},be=e=>{F||I(e.currentTarget),$u(e.target)&&(ve(!0),ge(e))},xe=e=>{re.current=!0;let t=N.props;t.onTouchStart&&t.onTouchStart(e)},Se=e=>{xe(e),oe.clear(),ae.clear(),pe(),fe.current=document.body.style.WebkitUserSelect,document.body.style.WebkitUserSelect=`none`,se.start(h,()=>{document.body.style.WebkitUserSelect=fe.current,ge(e)})},Ce=e=>{N.props.onTouchEnd&&N.props.onTouchEnd(e),pe(),oe.start(y,()=>{he(e)})};x.useEffect(()=>{if(!ue)return;function e(e){e.key===`Escape`&&he(e)}return document.addEventListener(`keydown`,e),()=>{document.removeEventListener(`keydown`,e)}},[he,ue]);let we=Yl(Yp(N),I,t);!k&&k!==0&&(ue=!1);let Te=x.useRef(),Ee=e=>{let t=N.props;t.onMouseMove&&t.onMouseMove(e),Zg={x:e.clientX,y:e.clientY},Te.current&&Te.current.update()},De={},Oe=typeof k==`string`;c?(De.title=!ue&&Oe&&!u?k:null,De[`aria-describedby`]=ue?de:null):(De[`aria-label`]=Oe?k:null,De[`aria-labelledby`]=ue&&!Oe?de:null);let ke={...De,...M,...N.props,className:B(M.className,N.props.className),onTouchStart:xe,ref:we,...g?{onMouseMove:Ee}:{}},Ae={};f||(ke.onTouchStart=Se,ke.onTouchEnd=Ce),u||(ke.onMouseOver=Qg(ge,ke.onMouseOver),ke.onMouseLeave=Qg(_e,ke.onMouseLeave),ie||(Ae.onMouseOver=ge,Ae.onMouseLeave=_e)),l||(ke.onFocus=Qg(be,ke.onFocus),ke.onBlur=Qg(ye,ke.onBlur),ie||(Ae.onFocus=be,Ae.onBlur=ye));let je={...n,isRtl:P,arrow:r,disableInteractive:ie,placement:w,PopperComponentProp:T,touch:re.current},Me=typeof D.popper==`function`?D.popper(je):D.popper,Ne=x.useMemo(()=>{let e=[{name:`arrow`,enabled:!!te,options:{element:te,padding:4}}];return E.popperOptions?.modifiers&&(e=e.concat(E.popperOptions.modifiers)),Me?.popperOptions?.modifiers&&(e=e.concat(Me.popperOptions.modifiers)),{...E.popperOptions,...Me?.popperOptions,modifiers:e}},[te,E.popperOptions,Me?.popperOptions]),Pe=Gg(je),Fe=typeof D.transition==`function`?D.transition(je):D.transition,Ie={slots:{popper:o.Popper,transition:o.Transition??A,tooltip:o.Tooltip,arrow:o.Arrow,...O},slotProps:{arrow:D.arrow??s.arrow,popper:{...E,...Me??s.popper},tooltip:D.tooltip??s.tooltip,transition:{...j,...Fe??s.transition}}},[Le,Re]=Vu(`popper`,{elementType:Kg,externalForwardedProps:Ie,ownerState:je,className:B(Pe.popper,E?.className)}),[ze,Be]=Vu(`transition`,{elementType:lg,externalForwardedProps:Ie,ownerState:je}),[Ve,He]=Vu(`tooltip`,{elementType:qg,className:Pe.tooltip,externalForwardedProps:Ie,ownerState:je}),[Ue,We]=Vu(`arrow`,{elementType:Jg,className:Pe.arrow,externalForwardedProps:Ie,ownerState:je,ref:ne});return(0,R.jsxs)(x.Fragment,{children:[x.cloneElement(N,ke),(0,R.jsx)(Le,{as:T??sm,placement:w,anchorEl:g?{getBoundingClientRect:()=>({top:Zg.y,left:Zg.x,right:Zg.x,bottom:Zg.y,width:0,height:0})}:F,popperRef:Te,open:F?ue:!1,id:de,transition:!0,...Ae,...Re,popperOptions:Ne,children:({TransitionProps:e})=>(0,R.jsx)(ze,{timeout:ee.transitions.duration.shorter,...e,...Be,children:(0,R.jsxs)(Ve,{...He,children:[k,r?(0,R.jsx)(Ue,{...We}):null]})})})]})});function e_(e){return Io(`MuiToolbar`,e)}Lo(`MuiToolbar`,[`root`,`gutters`,`regular`,`dense`]);var t_=e=>{let{classes:t,disableGutters:n,variant:r}=e;return cc({root:[`root`,!n&&`gutters`,r]},e_,t)},n_=H(`div`,{name:`MuiToolbar`,slot:`Root`,overridesResolver:(e,t)=>{let{ownerState:n}=e;return[t.root,!n.disableGutters&&t.gutters,t[n.variant]]}})(kl(({theme:e})=>({position:`relative`,display:`flex`,alignItems:`center`,variants:[{props:({ownerState:e})=>!e.disableGutters,style:{paddingLeft:e.spacing(2),paddingRight:e.spacing(2),[e.breakpoints.up(`sm`)]:{paddingLeft:e.spacing(3),paddingRight:e.spacing(3)}}},{props:{variant:`dense`},style:{minHeight:48}},{props:{variant:`regular`},style:e.mixins.toolbar}]}))),r_=x.forwardRef(function(e,t){let n=Al({props:e,name:`MuiToolbar`}),{className:r,component:i=`div`,disableGutters:a=!1,variant:o=`regular`,...s}=n,c={...n,component:i,disableGutters:a,variant:o};return(0,R.jsx)(n_,{as:i,className:B(t_(c).root,r),ref:t,ownerState:c,...s})}),i_={root:{}};const a_=()=>(0,R.jsxs)(q,{sx:i_.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ReactiveUIToolKit`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit brings a React-like component model to Unity UI Toolkit using a virtual node tree, typed props, and reconciliation logic that runs in C#. You build your UI from`,` `,(0,R.jsx)(`code`,{children:`V.*`}),` helpers and function components, and the reconciler updates the underlying`,(0,R.jsx)(`code`,{children:`VisualElement`}),` hierarchy for you.`]}),(0,R.jsx)(K,{variant:`body1`,paragraph:!0,children:`The toolkit is designed to work both in the Unity Editor and at runtime, and to feel familiar if you have used React, while still fitting naturally into Unity's component model and UI Toolkit controls.`}),(0,R.jsxs)(K,{variant:`body2`,paragraph:!0,children:[(0,R.jsx)(`strong`,{children:`P.S.`}),` ReactiveUIToolKit runs entirely in C# on top of Unity UI Toolkit. There is no JavaScript engine or bridge layer involved.`]}),(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Highlights`}),(0,R.jsxs)(xg,{children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`VirtualNode diffing and batched updates for UI Toolkit trees`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Typed props and adapters for most built-in UI Toolkit controls`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Router and Signals utilities for navigation and shared state`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Editor-only elements are UNITY_EDITOR guarded`})})]})]});var o_=Object.create,s_=Object.defineProperty,c_=Object.defineProperties,l_=Object.getOwnPropertyDescriptor,u_=Object.getOwnPropertyDescriptors,d_=Object.getOwnPropertyNames,f_=Object.getOwnPropertySymbols,p_=Object.getPrototypeOf,m_=Object.prototype.hasOwnProperty,h_=Object.prototype.propertyIsEnumerable,g_=(e,t,n)=>t in e?s_(e,t,{enumerable:!0,configurable:!0,writable:!0,value:n}):e[t]=n,__=(e,t)=>{for(var n in t||={})m_.call(t,n)&&g_(e,n,t[n]);if(f_)for(var n of f_(t))h_.call(t,n)&&g_(e,n,t[n]);return e},v_=(e,t)=>c_(e,u_(t)),y_=(e,t)=>{var n={};for(var r in e)m_.call(e,r)&&t.indexOf(r)<0&&(n[r]=e[r]);if(e!=null&&f_)for(var r of f_(e))t.indexOf(r)<0&&h_.call(e,r)&&(n[r]=e[r]);return n},b_=(e,t)=>function(){return t||(0,e[d_(e)[0]])((t={exports:{}}).exports,t),t.exports},x_=(e,t)=>{for(var n in t)s_(e,n,{get:t[n],enumerable:!0})},S_=(e,t,n,r)=>{if(t&&typeof t==`object`||typeof t==`function`)for(let i of d_(t))!m_.call(e,i)&&i!==n&&s_(e,i,{get:()=>t[i],enumerable:!(r=l_(t,i))||r.enumerable});return e},X=((e,t,n)=>(n=e==null?{}:o_(p_(e)),S_(t||!e||!e.__esModule?s_(n,`default`,{value:e,enumerable:!0}):n,e)))(b_({"../../node_modules/.pnpm/prismjs@1.29.0_patch_hash=vrxx3pzkik6jpmgpayxfjunetu/node_modules/prismjs/prism.js"(e,t){var n=function(){var e=/(?:^|\s)lang(?:uage)?-([\w-]+)(?=\s|$)/i,t=0,n={},r={util:{encode:function e(t){return t instanceof i?new i(t.type,e(t.content),t.alias):Array.isArray(t)?t.map(e):t.replace(/&/g,`&amp;`).replace(/</g,`&lt;`).replace(/\u00a0/g,` `)},type:function(e){return Object.prototype.toString.call(e).slice(8,-1)},objId:function(e){return e.__id||Object.defineProperty(e,`__id`,{value:++t}),e.__id},clone:function e(t,n){n||={};var i,a;switch(r.util.type(t)){case`Object`:if(a=r.util.objId(t),n[a])return n[a];for(var o in i={},n[a]=i,t)t.hasOwnProperty(o)&&(i[o]=e(t[o],n));return i;case`Array`:return a=r.util.objId(t),n[a]?n[a]:(i=[],n[a]=i,t.forEach(function(t,r){i[r]=e(t,n)}),i);default:return t}},getLanguage:function(t){for(;t;){var n=e.exec(t.className);if(n)return n[1].toLowerCase();t=t.parentElement}return`none`},setLanguage:function(t,n){t.className=t.className.replace(RegExp(e,`gi`),``),t.classList.add(`language-`+n)},isActive:function(e,t,n){for(var r=`no-`+t;e;){var i=e.classList;if(i.contains(t))return!0;if(i.contains(r))return!1;e=e.parentElement}return!!n}},languages:{plain:n,plaintext:n,text:n,txt:n,extend:function(e,t){var n=r.util.clone(r.languages[e]);for(var i in t)n[i]=t[i];return n},insertBefore:function(e,t,n,i){i||=r.languages;var a=i[e],o={};for(var s in a)if(a.hasOwnProperty(s)){if(s==t)for(var c in n)n.hasOwnProperty(c)&&(o[c]=n[c]);n.hasOwnProperty(s)||(o[s]=a[s])}var l=i[e];return i[e]=o,r.languages.DFS(r.languages,function(t,n){n===l&&t!=e&&(this[t]=o)}),o},DFS:function e(t,n,i,a){a||={};var o=r.util.objId;for(var s in t)if(t.hasOwnProperty(s)){n.call(t,s,t[s],i||s);var c=t[s],l=r.util.type(c);l===`Object`&&!a[o(c)]?(a[o(c)]=!0,e(c,n,null,a)):l===`Array`&&!a[o(c)]&&(a[o(c)]=!0,e(c,n,s,a))}}},plugins:{},highlight:function(e,t,n){var a={code:e,grammar:t,language:n};if(r.hooks.run(`before-tokenize`,a),!a.grammar)throw Error(`The language "`+a.language+`" has no grammar.`);return a.tokens=r.tokenize(a.code,a.grammar),r.hooks.run(`after-tokenize`,a),i.stringify(r.util.encode(a.tokens),a.language)},tokenize:function(e,t){var n=t.rest;if(n){for(var r in n)t[r]=n[r];delete t.rest}var i=new s;return c(i,i.head,e),o(e,i,t,i.head,0),u(i)},hooks:{all:{},add:function(e,t){var n=r.hooks.all;n[e]=n[e]||[],n[e].push(t)},run:function(e,t){var n=r.hooks.all[e];if(!(!n||!n.length))for(var i=0,a;a=n[i++];)a(t)}},Token:i};function i(e,t,n,r){this.type=e,this.content=t,this.alias=n,this.length=(r||``).length|0}i.stringify=function e(t,n){if(typeof t==`string`)return t;if(Array.isArray(t)){var i=``;return t.forEach(function(t){i+=e(t,n)}),i}var a={type:t.type,content:e(t.content,n),tag:`span`,classes:[`token`,t.type],attributes:{},language:n},o=t.alias;o&&(Array.isArray(o)?Array.prototype.push.apply(a.classes,o):a.classes.push(o)),r.hooks.run(`wrap`,a);var s=``;for(var c in a.attributes)s+=` `+c+`="`+(a.attributes[c]||``).replace(/"/g,`&quot;`)+`"`;return`<`+a.tag+` class="`+a.classes.join(` `)+`"`+s+`>`+a.content+`</`+a.tag+`>`};function a(e,t,n,r){e.lastIndex=t;var i=e.exec(n);if(i&&r&&i[1]){var a=i[1].length;i.index+=a,i[0]=i[0].slice(a)}return i}function o(e,t,n,s,u,d){for(var f in n)if(!(!n.hasOwnProperty(f)||!n[f])){var p=n[f];p=Array.isArray(p)?p:[p];for(var m=0;m<p.length;++m){if(d&&d.cause==f+`,`+m)return;var h=p[m],g=h.inside,_=!!h.lookbehind,v=!!h.greedy,y=h.alias;if(v&&!h.pattern.global){var b=h.pattern.toString().match(/[imsuy]*$/)[0];h.pattern=RegExp(h.pattern.source,b+`g`)}for(var x=h.pattern||h,S=s.next,C=u;S!==t.tail&&!(d&&C>=d.reach);C+=S.value.length,S=S.next){var w=S.value;if(t.length>e.length)return;if(!(w instanceof i)){var T=1,E;if(v){if(E=a(x,C,e,_),!E||E.index>=e.length)break;var D=E.index,O=E.index+E[0].length,k=C;for(k+=S.value.length;D>=k;)S=S.next,k+=S.value.length;if(k-=S.value.length,C=k,S.value instanceof i)continue;for(var A=S;A!==t.tail&&(k<O||typeof A.value==`string`);A=A.next)T++,k+=A.value.length;T--,w=e.slice(C,k),E.index-=C}else if(E=a(x,0,w,_),!E)continue;var D=E.index,j=E[0],M=w.slice(0,D),N=w.slice(D+j.length),ee=C+w.length;d&&ee>d.reach&&(d.reach=ee);var P=S.prev;M&&(P=c(t,P,M),C+=M.length),l(t,P,T);var F=new i(f,g?r.tokenize(j,g):j,y,j);if(S=c(t,P,F),N&&c(t,S,N),T>1){var I={cause:f+`,`+m,reach:ee};o(e,t,n,S.prev,C,I),d&&I.reach>d.reach&&(d.reach=I.reach)}}}}}}function s(){var e={value:null,prev:null,next:null},t={value:null,prev:e,next:null};e.next=t,this.head=e,this.tail=t,this.length=0}function c(e,t,n){var r=t.next,i={value:n,prev:t,next:r};return t.next=i,r.prev=i,e.length++,i}function l(e,t,n){for(var r=t.next,i=0;i<n&&r!==e.tail;i++)r=r.next;t.next=r,r.prev=t,e.length-=i}function u(e){for(var t=[],n=e.head.next;n!==e.tail;)t.push(n.value),n=n.next;return t}return r}();t.exports=n,n.default=n}})());X.languages.markup={comment:{pattern:/<!--(?:(?!<!--)[\s\S])*?-->/,greedy:!0},prolog:{pattern:/<\?[\s\S]+?\?>/,greedy:!0},doctype:{pattern:/<!DOCTYPE(?:[^>"'[\]]|"[^"]*"|'[^']*')+(?:\[(?:[^<"'\]]|"[^"]*"|'[^']*'|<(?!!--)|<!--(?:[^-]|-(?!->))*-->)*\]\s*)?>/i,greedy:!0,inside:{"internal-subset":{pattern:/(^[^\[]*\[)[\s\S]+(?=\]>$)/,lookbehind:!0,greedy:!0,inside:null},string:{pattern:/"[^"]*"|'[^']*'/,greedy:!0},punctuation:/^<!|>$|[[\]]/,"doctype-tag":/^DOCTYPE/i,name:/[^\s<>'"]+/}},cdata:{pattern:/<!\[CDATA\[[\s\S]*?\]\]>/i,greedy:!0},tag:{pattern:/<\/?(?!\d)[^\s>\/=$<%]+(?:\s(?:\s*[^\s>\/=]+(?:\s*=\s*(?:"[^"]*"|'[^']*'|[^\s'">=]+(?=[\s>]))|(?=[\s/>])))+)?\s*\/?>/,greedy:!0,inside:{tag:{pattern:/^<\/?[^\s>\/]+/,inside:{punctuation:/^<\/?/,namespace:/^[^\s>\/:]+:/}},"special-attr":[],"attr-value":{pattern:/=\s*(?:"[^"]*"|'[^']*'|[^\s'">=]+)/,inside:{punctuation:[{pattern:/^=/,alias:`attr-equals`},{pattern:/^(\s*)["']|["']$/,lookbehind:!0}]}},punctuation:/\/?>/,"attr-name":{pattern:/[^\s>\/]+/,inside:{namespace:/^[^\s>\/:]+:/}}}},entity:[{pattern:/&[\da-z]{1,8};/i,alias:`named-entity`},/&#x?[\da-f]{1,8};/i]},X.languages.markup.tag.inside[`attr-value`].inside.entity=X.languages.markup.entity,X.languages.markup.doctype.inside[`internal-subset`].inside=X.languages.markup,X.hooks.add(`wrap`,function(e){e.type===`entity`&&(e.attributes.title=e.content.replace(/&amp;/,`&`))}),Object.defineProperty(X.languages.markup.tag,`addInlined`,{value:function(e,t){var n={},n=(n[`language-`+t]={pattern:/(^<!\[CDATA\[)[\s\S]+?(?=\]\]>$)/i,lookbehind:!0,inside:X.languages[t]},n.cdata=/^<!\[CDATA\[|\]\]>$/i,{"included-cdata":{pattern:/<!\[CDATA\[[\s\S]*?\]\]>/i,inside:n}}),t=(n[`language-`+t]={pattern:/[\s\S]+/,inside:X.languages[t]},{});t[e]={pattern:RegExp(`(<__[^>]*>)(?:<!\\[CDATA\\[(?:[^\\]]|\\](?!\\]>))*\\]\\]>|(?!<!\\[CDATA\\[)[\\s\\S])*?(?=<\\/__>)`.replace(/__/g,function(){return e}),`i`),lookbehind:!0,greedy:!0,inside:n},X.languages.insertBefore(`markup`,`cdata`,t)}}),Object.defineProperty(X.languages.markup.tag,`addAttribute`,{value:function(e,t){X.languages.markup.tag.inside[`special-attr`].push({pattern:RegExp(`(^|["'\\s])(?:`+e+`)\\s*=\\s*(?:"[^"]*"|'[^']*'|[^\\s'">=]+(?=[\\s>]))`,`i`),lookbehind:!0,inside:{"attr-name":/^[^\s=]+/,"attr-value":{pattern:/=[\s\S]+/,inside:{value:{pattern:/(^=\s*(["']|(?!["'])))\S[\s\S]*(?=\2$)/,lookbehind:!0,alias:[t,`language-`+t],inside:X.languages[t]},punctuation:[{pattern:/^=/,alias:`attr-equals`},/"|'/]}}}})}}),X.languages.html=X.languages.markup,X.languages.mathml=X.languages.markup,X.languages.svg=X.languages.markup,X.languages.xml=X.languages.extend(`markup`,{}),X.languages.ssml=X.languages.xml,X.languages.atom=X.languages.xml,X.languages.rss=X.languages.xml,function(e){var t={pattern:/\\[\\(){}[\]^$+*?|.]/,alias:`escape`},n=/\\(?:x[\da-fA-F]{2}|u[\da-fA-F]{4}|u\{[\da-fA-F]+\}|0[0-7]{0,2}|[123][0-7]{2}|c[a-zA-Z]|.)/,r=`(?:[^\\\\-]|`+n.source+`)`,r=RegExp(r+`-`+r),i={pattern:/(<|')[^<>']+(?=[>']$)/,lookbehind:!0,alias:`variable`};e.languages.regex={"char-class":{pattern:/((?:^|[^\\])(?:\\\\)*)\[(?:[^\\\]]|\\[\s\S])*\]/,lookbehind:!0,inside:{"char-class-negation":{pattern:/(^\[)\^/,lookbehind:!0,alias:`operator`},"char-class-punctuation":{pattern:/^\[|\]$/,alias:`punctuation`},range:{pattern:r,inside:{escape:n,"range-punctuation":{pattern:/-/,alias:`operator`}}},"special-escape":t,"char-set":{pattern:/\\[wsd]|\\p\{[^{}]+\}/i,alias:`class-name`},escape:n}},"special-escape":t,"char-set":{pattern:/\.|\\[wsd]|\\p\{[^{}]+\}/i,alias:`class-name`},backreference:[{pattern:/\\(?![123][0-7]{2})[1-9]/,alias:`keyword`},{pattern:/\\k<[^<>']+>/,alias:`keyword`,inside:{"group-name":i}}],anchor:{pattern:/[$^]|\\[ABbGZz]/,alias:`function`},escape:n,group:[{pattern:/\((?:\?(?:<[^<>']+>|'[^<>']+'|[>:]|<?[=!]|[idmnsuxU]+(?:-[idmnsuxU]+)?:?))?/,alias:`punctuation`,inside:{"group-name":i}},{pattern:/\)/,alias:`punctuation`}],quantifier:{pattern:/(?:[+*?]|\{\d+(?:,\d*)?\})[?+]?/,alias:`number`},alternation:{pattern:/\|/,alias:`keyword`}}}(X),X.languages.clike={comment:[{pattern:/(^|[^\\])\/\*[\s\S]*?(?:\*\/|$)/,lookbehind:!0,greedy:!0},{pattern:/(^|[^\\:])\/\/.*/,lookbehind:!0,greedy:!0}],string:{pattern:/(["'])(?:\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,greedy:!0},"class-name":{pattern:/(\b(?:class|extends|implements|instanceof|interface|new|trait)\s+|\bcatch\s+\()[\w.\\]+/i,lookbehind:!0,inside:{punctuation:/[.\\]/}},keyword:/\b(?:break|catch|continue|do|else|finally|for|function|if|in|instanceof|new|null|return|throw|try|while)\b/,boolean:/\b(?:false|true)\b/,function:/\b\w+(?=\()/,number:/\b0x[\da-f]+\b|(?:\b\d+(?:\.\d*)?|\B\.\d+)(?:e[+-]?\d+)?/i,operator:/[<>]=?|[!=]=?=?|--?|\+\+?|&&?|\|\|?|[?*/~^%]/,punctuation:/[{}[\];(),.:]/},X.languages.javascript=X.languages.extend(`clike`,{"class-name":[X.languages.clike[`class-name`],{pattern:/(^|[^$\w\xA0-\uFFFF])(?!\s)[_$A-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\.(?:constructor|prototype))/,lookbehind:!0}],keyword:[{pattern:/((?:^|\})\s*)catch\b/,lookbehind:!0},{pattern:/(^|[^.]|\.\.\.\s*)\b(?:as|assert(?=\s*\{)|async(?=\s*(?:function\b|\(|[$\w\xA0-\uFFFF]|$))|await|break|case|class|const|continue|debugger|default|delete|do|else|enum|export|extends|finally(?=\s*(?:\{|$))|for|from(?=\s*(?:['"]|$))|function|(?:get|set)(?=\s*(?:[#\[$\w\xA0-\uFFFF]|$))|if|implements|import|in|instanceof|interface|let|new|null|of|package|private|protected|public|return|static|super|switch|this|throw|try|typeof|undefined|var|void|while|with|yield)\b/,lookbehind:!0}],function:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*(?:\.\s*(?:apply|bind|call)\s*)?\()/,number:{pattern:RegExp(`(^|[^\\w$])(?:NaN|Infinity|0[bB][01]+(?:_[01]+)*n?|0[oO][0-7]+(?:_[0-7]+)*n?|0[xX][\\dA-Fa-f]+(?:_[\\dA-Fa-f]+)*n?|\\d+(?:_\\d+)*n|(?:\\d+(?:_\\d+)*(?:\\.(?:\\d+(?:_\\d+)*)?)?|\\.\\d+(?:_\\d+)*)(?:[Ee][+-]?\\d+(?:_\\d+)*)?)(?![\\w$])`),lookbehind:!0},operator:/--|\+\+|\*\*=?|=>|&&=?|\|\|=?|[!=]==|<<=?|>>>?=?|[-+*/%&|^!=<>]=?|\.{3}|\?\?=?|\?\.?|[~:]/}),X.languages.javascript[`class-name`][0].pattern=/(\b(?:class|extends|implements|instanceof|interface|new)\s+)[\w.\\]+/,X.languages.insertBefore(`javascript`,`keyword`,{regex:{pattern:RegExp(`((?:^|[^$\\w\\xA0-\\uFFFF."'\\])\\s]|\\b(?:return|yield))\\s*)\\/(?:(?:\\[(?:[^\\]\\\\\\r\\n]|\\\\.)*\\]|\\\\.|[^/\\\\\\[\\r\\n])+\\/[dgimyus]{0,7}|(?:\\[(?:[^[\\]\\\\\\r\\n]|\\\\.|\\[(?:[^[\\]\\\\\\r\\n]|\\\\.|\\[(?:[^[\\]\\\\\\r\\n]|\\\\.)*\\])*\\])*\\]|\\\\.|[^/\\\\\\[\\r\\n])+\\/[dgimyus]{0,7}v[dgimyus]{0,7})(?=(?:\\s|\\/\\*(?:[^*]|\\*(?!\\/))*\\*\\/)*(?:$|[\\r\\n,.;:})\\]]|\\/\\/))`),lookbehind:!0,greedy:!0,inside:{"regex-source":{pattern:/^(\/)[\s\S]+(?=\/[a-z]*$)/,lookbehind:!0,alias:`language-regex`,inside:X.languages.regex},"regex-delimiter":/^\/|\/$/,"regex-flags":/^[a-z]+$/}},"function-variable":{pattern:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*[=:]\s*(?:async\s*)?(?:\bfunction\b|(?:\((?:[^()]|\([^()]*\))*\)|(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)\s*=>))/,alias:`function`},parameter:[{pattern:/(function(?:\s+(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)?\s*\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\))/,lookbehind:!0,inside:X.languages.javascript},{pattern:/(^|[^$\w\xA0-\uFFFF])(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*=>)/i,lookbehind:!0,inside:X.languages.javascript},{pattern:/(\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\)\s*=>)/,lookbehind:!0,inside:X.languages.javascript},{pattern:/((?:\b|\s|^)(?!(?:as|async|await|break|case|catch|class|const|continue|debugger|default|delete|do|else|enum|export|extends|finally|for|from|function|get|if|implements|import|in|instanceof|interface|let|new|null|of|package|private|protected|public|return|set|static|super|switch|this|throw|try|typeof|undefined|var|void|while|with|yield)(?![$\w\xA0-\uFFFF]))(?:(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*\s*)\(\s*|\]\s*\(\s*)(?!\s)(?:[^()\s]|\s+(?![\s)])|\([^()]*\))+(?=\s*\)\s*\{)/,lookbehind:!0,inside:X.languages.javascript}],constant:/\b[A-Z](?:[A-Z_]|\dx?)*\b/}),X.languages.insertBefore(`javascript`,`string`,{hashbang:{pattern:/^#!.*/,greedy:!0,alias:`comment`},"template-string":{pattern:/`(?:\\[\s\S]|\$\{(?:[^{}]|\{(?:[^{}]|\{[^}]*\})*\})+\}|(?!\$\{)[^\\`])*`/,greedy:!0,inside:{"template-punctuation":{pattern:/^`|`$/,alias:`string`},interpolation:{pattern:/((?:^|[^\\])(?:\\{2})*)\$\{(?:[^{}]|\{(?:[^{}]|\{[^}]*\})*\})+\}/,lookbehind:!0,inside:{"interpolation-punctuation":{pattern:/^\$\{|\}$/,alias:`punctuation`},rest:X.languages.javascript}},string:/[\s\S]+/}},"string-property":{pattern:/((?:^|[,{])[ \t]*)(["'])(?:\\(?:\r\n|[\s\S])|(?!\2)[^\\\r\n])*\2(?=\s*:)/m,lookbehind:!0,greedy:!0,alias:`property`}}),X.languages.insertBefore(`javascript`,`operator`,{"literal-property":{pattern:/((?:^|[,{])[ \t]*)(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*:)/m,lookbehind:!0,alias:`property`}}),X.languages.markup&&(X.languages.markup.tag.addInlined(`script`,`javascript`),X.languages.markup.tag.addAttribute(`on(?:abort|blur|change|click|composition(?:end|start|update)|dblclick|error|focus(?:in|out)?|key(?:down|up)|load|mouse(?:down|enter|leave|move|out|over|up)|reset|resize|scroll|select|slotchange|submit|unload|wheel)`,`javascript`)),X.languages.js=X.languages.javascript,X.languages.actionscript=X.languages.extend(`javascript`,{keyword:/\b(?:as|break|case|catch|class|const|default|delete|do|dynamic|each|else|extends|final|finally|for|function|get|if|implements|import|in|include|instanceof|interface|internal|is|namespace|native|new|null|override|package|private|protected|public|return|set|static|super|switch|this|throw|try|typeof|use|var|void|while|with)\b/,operator:/\+\+|--|(?:[+\-*\/%^]|&&?|\|\|?|<<?|>>?>?|[!=]=?)=?|[~?@]/}),X.languages.actionscript[`class-name`].alias=`function`,delete X.languages.actionscript.parameter,delete X.languages.actionscript[`literal-property`],X.languages.markup&&X.languages.insertBefore(`actionscript`,`string`,{xml:{pattern:/(^|[^.])<\/?\w+(?:\s+[^\s>\/=]+=("|')(?:\\[\s\S]|(?!\2)[^\\])*\2)*\s*\/?>/,lookbehind:!0,inside:X.languages.markup}}),function(e){var t=/#(?!\{).+/,n={pattern:/#\{[^}]+\}/,alias:`variable`};e.languages.coffeescript=e.languages.extend(`javascript`,{comment:t,string:[{pattern:/'(?:\\[\s\S]|[^\\'])*'/,greedy:!0},{pattern:/"(?:\\[\s\S]|[^\\"])*"/,greedy:!0,inside:{interpolation:n}}],keyword:/\b(?:and|break|by|catch|class|continue|debugger|delete|do|each|else|extend|extends|false|finally|for|if|in|instanceof|is|isnt|let|loop|namespace|new|no|not|null|of|off|on|or|own|return|super|switch|then|this|throw|true|try|typeof|undefined|unless|until|when|while|window|with|yes|yield)\b/,"class-member":{pattern:/@(?!\d)\w+/,alias:`variable`}}),e.languages.insertBefore(`coffeescript`,`comment`,{"multiline-comment":{pattern:/###[\s\S]+?###/,alias:`comment`},"block-regex":{pattern:/\/{3}[\s\S]*?\/{3}/,alias:`regex`,inside:{comment:t,interpolation:n}}}),e.languages.insertBefore(`coffeescript`,`string`,{"inline-javascript":{pattern:/`(?:\\[\s\S]|[^\\`])*`/,inside:{delimiter:{pattern:/^`|`$/,alias:`punctuation`},script:{pattern:/[\s\S]+/,alias:`language-javascript`,inside:e.languages.javascript}}},"multiline-string":[{pattern:/'''[\s\S]*?'''/,greedy:!0,alias:`string`},{pattern:/"""[\s\S]*?"""/,greedy:!0,alias:`string`,inside:{interpolation:n}}]}),e.languages.insertBefore(`coffeescript`,`keyword`,{property:/(?!\d)\w+(?=\s*:(?!:))/}),delete e.languages.coffeescript[`template-string`],e.languages.coffee=e.languages.coffeescript}(X),function(e){var t=e.languages.javadoclike={parameter:{pattern:/(^[\t ]*(?:\/{3}|\*|\/\*\*)\s*@(?:arg|arguments|param)\s+)\w+/m,lookbehind:!0},keyword:{pattern:/(^[\t ]*(?:\/{3}|\*|\/\*\*)\s*|\{)@[a-z][a-zA-Z-]+\b/m,lookbehind:!0},punctuation:/[{}]/};Object.defineProperty(t,`addSupport`,{value:function(t,n){(t=typeof t==`string`?[t]:t).forEach(function(t){var r=function(e){e.inside||={},e.inside.rest=n},i=`doc-comment`;if(a=e.languages[t]){var a,o=a[i];if((o||=(a=e.languages.insertBefore(t,`comment`,{"doc-comment":{pattern:/(^|[^\\])\/\*\*[^/][\s\S]*?(?:\*\/|$)/,lookbehind:!0,alias:`comment`}}))[i])instanceof RegExp&&(o=a[i]={pattern:o}),Array.isArray(o))for(var s=0,c=o.length;s<c;s++)o[s]instanceof RegExp&&(o[s]={pattern:o[s]}),r(o[s]);else r(o)}})}}),t.addSupport([`java`,`javascript`,`php`],t)}(X),function(e){var t=/(?:"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"|'(?:\\(?:\r\n|[\s\S])|[^'\\\r\n])*')/,t=(e.languages.css={comment:/\/\*[\s\S]*?\*\//,atrule:{pattern:RegExp(`@[\\w-](?:[^;{\\s"']|\\s+(?!\\s)|`+t.source+`)*?(?:;|(?=\\s*\\{))`),inside:{rule:/^@[\w-]+/,"selector-function-argument":{pattern:/(\bselector\s*\(\s*(?![\s)]))(?:[^()\s]|\s+(?![\s)])|\((?:[^()]|\([^()]*\))*\))+(?=\s*\))/,lookbehind:!0,alias:`selector`},keyword:{pattern:/(^|[^\w-])(?:and|not|only|or)(?![\w-])/,lookbehind:!0}}},url:{pattern:RegExp(`\\burl\\((?:`+t.source+`|(?:[^\\\\\\r\\n()"']|\\\\[\\s\\S])*)\\)`,`i`),greedy:!0,inside:{function:/^url/i,punctuation:/^\(|\)$/,string:{pattern:RegExp(`^`+t.source+`$`),alias:`url`}}},selector:{pattern:RegExp(`(^|[{}\\s])[^{}\\s](?:[^{};"'\\s]|\\s+(?![\\s{])|`+t.source+`)*(?=\\s*\\{)`),lookbehind:!0},string:{pattern:t,greedy:!0},property:{pattern:/(^|[^-\w\xA0-\uFFFF])(?!\s)[-_a-z\xA0-\uFFFF](?:(?!\s)[-\w\xA0-\uFFFF])*(?=\s*:)/i,lookbehind:!0},important:/!important\b/i,function:{pattern:/(^|[^-a-z0-9])[-a-z0-9]+(?=\()/i,lookbehind:!0},punctuation:/[(){};:,]/},e.languages.css.atrule.inside.rest=e.languages.css,e.languages.markup);t&&(t.tag.addInlined(`style`,`css`),t.tag.addAttribute(`style`,`css`))}(X),function(e){var t=/("|')(?:\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,t=(e.languages.css.selector={pattern:e.languages.css.selector.pattern,lookbehind:!0,inside:t={"pseudo-element":/:(?:after|before|first-letter|first-line|selection)|::[-\w]+/,"pseudo-class":/:[-\w]+/,class:/\.[-\w]+/,id:/#[-\w]+/,attribute:{pattern:RegExp(`\\[(?:[^[\\]"']|`+t.source+`)*\\]`),greedy:!0,inside:{punctuation:/^\[|\]$/,"case-sensitivity":{pattern:/(\s)[si]$/i,lookbehind:!0,alias:`keyword`},namespace:{pattern:/^(\s*)(?:(?!\s)[-*\w\xA0-\uFFFF])*\|(?!=)/,lookbehind:!0,inside:{punctuation:/\|$/}},"attr-name":{pattern:/^(\s*)(?:(?!\s)[-\w\xA0-\uFFFF])+/,lookbehind:!0},"attr-value":[t,{pattern:/(=\s*)(?:(?!\s)[-\w\xA0-\uFFFF])+(?=\s*$)/,lookbehind:!0}],operator:/[|~*^$]?=/}},"n-th":[{pattern:/(\(\s*)[+-]?\d*[\dn](?:\s*[+-]\s*\d+)?(?=\s*\))/,lookbehind:!0,inside:{number:/[\dn]+/,operator:/[+-]/}},{pattern:/(\(\s*)(?:even|odd)(?=\s*\))/i,lookbehind:!0}],combinator:/>|\+|~|\|\|/,punctuation:/[(),]/}},e.languages.css.atrule.inside[`selector-function-argument`].inside=t,e.languages.insertBefore(`css`,`property`,{variable:{pattern:/(^|[^-\w\xA0-\uFFFF])--(?!\s)[-_a-z\xA0-\uFFFF](?:(?!\s)[-\w\xA0-\uFFFF])*/i,lookbehind:!0}}),{pattern:/(\b\d+)(?:%|[a-z]+(?![\w-]))/,lookbehind:!0}),n={pattern:/(^|[^\w.-])-?(?:\d+(?:\.\d+)?|\.\d+)/,lookbehind:!0};e.languages.insertBefore(`css`,`function`,{operator:{pattern:/(\s)[+\-*\/](?=\s)/,lookbehind:!0},hexcode:{pattern:/\B#[\da-f]{3,8}\b/i,alias:`color`},color:[{pattern:/(^|[^\w-])(?:AliceBlue|AntiqueWhite|Aqua|Aquamarine|Azure|Beige|Bisque|Black|BlanchedAlmond|Blue|BlueViolet|Brown|BurlyWood|CadetBlue|Chartreuse|Chocolate|Coral|CornflowerBlue|Cornsilk|Crimson|Cyan|DarkBlue|DarkCyan|DarkGoldenRod|DarkGr[ae]y|DarkGreen|DarkKhaki|DarkMagenta|DarkOliveGreen|DarkOrange|DarkOrchid|DarkRed|DarkSalmon|DarkSeaGreen|DarkSlateBlue|DarkSlateGr[ae]y|DarkTurquoise|DarkViolet|DeepPink|DeepSkyBlue|DimGr[ae]y|DodgerBlue|FireBrick|FloralWhite|ForestGreen|Fuchsia|Gainsboro|GhostWhite|Gold|GoldenRod|Gr[ae]y|Green|GreenYellow|HoneyDew|HotPink|IndianRed|Indigo|Ivory|Khaki|Lavender|LavenderBlush|LawnGreen|LemonChiffon|LightBlue|LightCoral|LightCyan|LightGoldenRodYellow|LightGr[ae]y|LightGreen|LightPink|LightSalmon|LightSeaGreen|LightSkyBlue|LightSlateGr[ae]y|LightSteelBlue|LightYellow|Lime|LimeGreen|Linen|Magenta|Maroon|MediumAquaMarine|MediumBlue|MediumOrchid|MediumPurple|MediumSeaGreen|MediumSlateBlue|MediumSpringGreen|MediumTurquoise|MediumVioletRed|MidnightBlue|MintCream|MistyRose|Moccasin|NavajoWhite|Navy|OldLace|Olive|OliveDrab|Orange|OrangeRed|Orchid|PaleGoldenRod|PaleGreen|PaleTurquoise|PaleVioletRed|PapayaWhip|PeachPuff|Peru|Pink|Plum|PowderBlue|Purple|RebeccaPurple|Red|RosyBrown|RoyalBlue|SaddleBrown|Salmon|SandyBrown|SeaGreen|SeaShell|Sienna|Silver|SkyBlue|SlateBlue|SlateGr[ae]y|Snow|SpringGreen|SteelBlue|Tan|Teal|Thistle|Tomato|Transparent|Turquoise|Violet|Wheat|White|WhiteSmoke|Yellow|YellowGreen)(?![\w-])/i,lookbehind:!0},{pattern:/\b(?:hsl|rgb)\(\s*\d{1,3}\s*,\s*\d{1,3}%?\s*,\s*\d{1,3}%?\s*\)\B|\b(?:hsl|rgb)a\(\s*\d{1,3}\s*,\s*\d{1,3}%?\s*,\s*\d{1,3}%?\s*,\s*(?:0|0?\.\d+|1)\s*\)\B/i,inside:{unit:t,number:n,function:/[\w-]+(?=\()/,punctuation:/[(),]/}}],entity:/\\[\da-f]{1,8}/i,unit:t,number:n})}(X),function(e){var t=/[*&][^\s[\]{},]+/,n=/!(?:<[\w\-%#;/?:@&=+$,.!~*'()[\]]+>|(?:[a-zA-Z\d-]*!)?[\w\-%#;/?:@&=+$.~*'()]+)?/,r=`(?:`+n.source+`(?:[ 	]+`+t.source+`)?|`+t.source+`(?:[ 	]+`+n.source+`)?)`,i=`(?:[^\\s\\x00-\\x08\\x0e-\\x1f!"#%&'*,\\-:>?@[\\]\`{|}\\x7f-\\x84\\x86-\\x9f\\ud800-\\udfff\\ufffe\\uffff]|[?:-]<PLAIN>)(?:[ \\t]*(?:(?![#:])<PLAIN>|:<PLAIN>))*`.replace(/<PLAIN>/g,function(){return`[^\\s\\x00-\\x08\\x0e-\\x1f,[\\]{}\\x7f-\\x84\\x86-\\x9f\\ud800-\\udfff\\ufffe\\uffff]`}),a=`"(?:[^"\\\\\\r\\n]|\\\\.)*"|'(?:[^'\\\\\\r\\n]|\\\\.)*'`;function o(e,t){t=(t||``).replace(/m/g,``)+`m`;var n=`([:\\-,[{]\\s*(?:\\s<<prop>>[ \\t]+)?)(?:<<value>>)(?=[ \\t]*(?:$|,|\\]|\\}|(?:[\\r\\n]\\s*)?#))`.replace(/<<prop>>/g,function(){return r}).replace(/<<value>>/g,function(){return e});return RegExp(n,t)}e.languages.yaml={scalar:{pattern:RegExp(`([\\-:]\\s*(?:\\s<<prop>>[ \\t]+)?[|>])[ \\t]*(?:((?:\\r?\\n|\\r)[ \\t]+)\\S[^\\r\\n]*(?:\\2[^\\r\\n]+)*)`.replace(/<<prop>>/g,function(){return r})),lookbehind:!0,alias:`string`},comment:/#.*/,key:{pattern:RegExp(`((?:^|[:\\-,[{\\r\\n?])[ \\t]*(?:<<prop>>[ \\t]+)?)<<key>>(?=\\s*:\\s)`.replace(/<<prop>>/g,function(){return r}).replace(/<<key>>/g,function(){return`(?:`+i+`|`+a+`)`})),lookbehind:!0,greedy:!0,alias:`atrule`},directive:{pattern:/(^[ \t]*)%.+/m,lookbehind:!0,alias:`important`},datetime:{pattern:o(`\\d{4}-\\d\\d?-\\d\\d?(?:[tT]|[ \\t]+)\\d\\d?:\\d{2}:\\d{2}(?:\\.\\d*)?(?:[ \\t]*(?:Z|[-+]\\d\\d?(?::\\d{2})?))?|\\d{4}-\\d{2}-\\d{2}|\\d\\d?:\\d{2}(?::\\d{2}(?:\\.\\d*)?)?`),lookbehind:!0,alias:`number`},boolean:{pattern:o(`false|true`,`i`),lookbehind:!0,alias:`important`},null:{pattern:o(`null|~`,`i`),lookbehind:!0,alias:`important`},string:{pattern:o(a),lookbehind:!0,greedy:!0},number:{pattern:o(`[+-]?(?:0x[\\da-f]+|0o[0-7]+|(?:\\d+(?:\\.\\d*)?|\\.\\d+)(?:e[+-]?\\d+)?|\\.inf|\\.nan)`,`i`),lookbehind:!0},tag:n,important:t,punctuation:/---|[:[\]{}\-,|>?]|\.\.\./},e.languages.yml=e.languages.yaml}(X),function(e){var t=`(?:\\\\.|[^\\\\\\n\\r]|(?:\\n|\\r\\n?)(?![\\r\\n]))`;function n(e){return e=e.replace(/<inner>/g,function(){return t}),RegExp(`((?:^|[^\\\\])(?:\\\\{2})*)(?:`+e+`)`)}var r="(?:\\\\.|``(?:[^`\\r\\n]|`(?!`))+``|`[^`\\r\\n]+`|[^\\\\|\\r\\n`])+",i=`\\|?__(?:\\|__)+\\|?(?:(?:\\n|\\r\\n?)|(?![\\s\\S]))`.replace(/__/g,function(){return r}),a=`\\|?[ \\t]*:?-{3,}:?[ \\t]*(?:\\|[ \\t]*:?-{3,}:?[ \\t]*)+\\|?(?:\\n|\\r\\n?)`,o=(e.languages.markdown=e.languages.extend(`markup`,{}),e.languages.insertBefore(`markdown`,`prolog`,{"front-matter-block":{pattern:/(^(?:\s*[\r\n])?)---(?!.)[\s\S]*?[\r\n]---(?!.)/,lookbehind:!0,greedy:!0,inside:{punctuation:/^---|---$/,"front-matter":{pattern:/\S+(?:\s+\S+)*/,alias:[`yaml`,`language-yaml`],inside:e.languages.yaml}}},blockquote:{pattern:/^>(?:[\t ]*>)*/m,alias:`punctuation`},table:{pattern:RegExp(`^`+i+a+`(?:`+i+`)*`,`m`),inside:{"table-data-rows":{pattern:RegExp(`^(`+i+a+`)(?:`+i+`)*$`),lookbehind:!0,inside:{"table-data":{pattern:RegExp(r),inside:e.languages.markdown},punctuation:/\|/}},"table-line":{pattern:RegExp(`^(`+i+`)`+a+`$`),lookbehind:!0,inside:{punctuation:/\||:?-{3,}:?/}},"table-header-row":{pattern:RegExp(`^`+i+`$`),inside:{"table-header":{pattern:RegExp(r),alias:`important`,inside:e.languages.markdown},punctuation:/\|/}}}},code:[{pattern:/((?:^|\n)[ \t]*\n|(?:^|\r\n?)[ \t]*\r\n?)(?: {4}|\t).+(?:(?:\n|\r\n?)(?: {4}|\t).+)*/,lookbehind:!0,alias:`keyword`},{pattern:/^```[\s\S]*?^```$/m,greedy:!0,inside:{"code-block":{pattern:/^(```.*(?:\n|\r\n?))[\s\S]+?(?=(?:\n|\r\n?)^```$)/m,lookbehind:!0},"code-language":{pattern:/^(```).+/,lookbehind:!0},punctuation:/```/}}],title:[{pattern:/\S.*(?:\n|\r\n?)(?:==+|--+)(?=[ \t]*$)/m,alias:`important`,inside:{punctuation:/==+$|--+$/}},{pattern:/(^\s*)#.+/m,lookbehind:!0,alias:`important`,inside:{punctuation:/^#+|#+$/}}],hr:{pattern:/(^\s*)([*-])(?:[\t ]*\2){2,}(?=\s*$)/m,lookbehind:!0,alias:`punctuation`},list:{pattern:/(^\s*)(?:[*+-]|\d+\.)(?=[\t ].)/m,lookbehind:!0,alias:`punctuation`},"url-reference":{pattern:/!?\[[^\]]+\]:[\t ]+(?:\S+|<(?:\\.|[^>\\])+>)(?:[\t ]+(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\)))?/,inside:{variable:{pattern:/^(!?\[)[^\]]+/,lookbehind:!0},string:/(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\))$/,punctuation:/^[\[\]!:]|[<>]/},alias:`url`},bold:{pattern:n(`\\b__(?:(?!_)<inner>|_(?:(?!_)<inner>)+_)+__\\b|\\*\\*(?:(?!\\*)<inner>|\\*(?:(?!\\*)<inner>)+\\*)+\\*\\*`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^..)[\s\S]+(?=..$)/,lookbehind:!0,inside:{}},punctuation:/\*\*|__/}},italic:{pattern:n(`\\b_(?:(?!_)<inner>|__(?:(?!_)<inner>)+__)+_\\b|\\*(?:(?!\\*)<inner>|\\*\\*(?:(?!\\*)<inner>)+\\*\\*)+\\*`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^.)[\s\S]+(?=.$)/,lookbehind:!0,inside:{}},punctuation:/[*_]/}},strike:{pattern:n(`(~~?)(?:(?!~)<inner>)+\\2`),lookbehind:!0,greedy:!0,inside:{content:{pattern:/(^~~?)[\s\S]+(?=\1$)/,lookbehind:!0,inside:{}},punctuation:/~~?/}},"code-snippet":{pattern:/(^|[^\\`])(?:``[^`\r\n]+(?:`[^`\r\n]+)*``(?!`)|`[^`\r\n]+`(?!`))/,lookbehind:!0,greedy:!0,alias:[`code`,`keyword`]},url:{pattern:n(`!?\\[(?:(?!\\])<inner>)+\\](?:\\([^\\s)]+(?:[\\t ]+"(?:\\\\.|[^"\\\\])*")?\\)|[ \\t]?\\[(?:(?!\\])<inner>)+\\])`),lookbehind:!0,greedy:!0,inside:{operator:/^!/,content:{pattern:/(^\[)[^\]]+(?=\])/,lookbehind:!0,inside:{}},variable:{pattern:/(^\][ \t]?\[)[^\]]+(?=\]$)/,lookbehind:!0},url:{pattern:/(^\]\()[^\s)]+/,lookbehind:!0},string:{pattern:/(^[ \t]+)"(?:\\.|[^"\\])*"(?=\)$)/,lookbehind:!0}}}}),[`url`,`bold`,`italic`,`strike`].forEach(function(t){[`url`,`bold`,`italic`,`strike`,`code-snippet`].forEach(function(n){t!==n&&(e.languages.markdown[t].inside.content.inside[n]=e.languages.markdown[n])})}),e.hooks.add(`after-tokenize`,function(e){e.language!==`markdown`&&e.language!==`md`||function e(t){if(t&&typeof t!=`string`)for(var n=0,r=t.length;n<r;n++){var i,a=t[n];a.type===`code`?(i=a.content[1],a=a.content[3],i&&a&&i.type===`code-language`&&a.type===`code-block`&&typeof i.content==`string`&&(i=i.content.replace(/\b#/g,`sharp`).replace(/\b\+\+/g,`pp`),i=`language-`+(i=(/[a-z][\w-]*/i.exec(i)||[``])[0].toLowerCase()),a.alias?typeof a.alias==`string`?a.alias=[a.alias,i]:a.alias.push(i):a.alias=[i])):e(a.content)}}(e.tokens)}),e.hooks.add(`wrap`,function(t){if(t.type===`code-block`){for(var n=``,r=0,i=t.classes.length;r<i;r++){var a=t.classes[r],a=/language-(.+)/.exec(a);if(a){n=a[1];break}}var l,u=e.languages[n];u?t.content=e.highlight(function(e){return e=e.replace(o,``),e=e.replace(/&(\w{1,8}|#x?[\da-f]{1,8});/gi,function(e,t){var n;return(t=t.toLowerCase())[0]===`#`?(n=t[1]===`x`?parseInt(t.slice(2),16):Number(t.slice(1)),c(n)):s[t]||e})}(t.content),u,n):n&&n!==`none`&&e.plugins.autoloader&&(l=`md-`+new Date().valueOf()+`-`+Math.floor(0x2386f26fc10000*Math.random()),t.attributes.id=l,e.plugins.autoloader.loadLanguages(n,function(){var t=document.getElementById(l);t&&(t.innerHTML=e.highlight(t.textContent,e.languages[n],n))}))}}),RegExp(e.languages.markup.tag.pattern.source,`gi`)),s={amp:`&`,lt:`<`,gt:`>`,quot:`"`},c=String.fromCodePoint||String.fromCharCode;e.languages.md=e.languages.markdown}(X),X.languages.graphql={comment:/#.*/,description:{pattern:/(?:"""(?:[^"]|(?!""")")*"""|"(?:\\.|[^\\"\r\n])*")(?=\s*[a-z_])/i,greedy:!0,alias:`string`,inside:{"language-markdown":{pattern:/(^"(?:"")?)(?!\1)[\s\S]+(?=\1$)/,lookbehind:!0,inside:X.languages.markdown}}},string:{pattern:/"""(?:[^"]|(?!""")")*"""|"(?:\\.|[^\\"\r\n])*"/,greedy:!0},number:/(?:\B-|\b)\d+(?:\.\d+)?(?:e[+-]?\d+)?\b/i,boolean:/\b(?:false|true)\b/,variable:/\$[a-z_]\w*/i,directive:{pattern:/@[a-z_]\w*/i,alias:`function`},"attr-name":{pattern:/\b[a-z_]\w*(?=\s*(?:\((?:[^()"]|"(?:\\.|[^\\"\r\n])*")*\))?:)/i,greedy:!0},"atom-input":{pattern:/\b[A-Z]\w*Input\b/,alias:`class-name`},scalar:/\b(?:Boolean|Float|ID|Int|String)\b/,constant:/\b[A-Z][A-Z_\d]*\b/,"class-name":{pattern:/(\b(?:enum|implements|interface|on|scalar|type|union)\s+|&\s*|:\s*|\[)[A-Z_]\w*/,lookbehind:!0},fragment:{pattern:/(\bfragment\s+|\.{3}\s*(?!on\b))[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},"definition-mutation":{pattern:/(\bmutation\s+)[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},"definition-query":{pattern:/(\bquery\s+)[a-zA-Z_]\w*/,lookbehind:!0,alias:`function`},keyword:/\b(?:directive|enum|extend|fragment|implements|input|interface|mutation|on|query|repeatable|scalar|schema|subscription|type|union)\b/,operator:/[!=|&]|\.{3}/,"property-query":/\w+(?=\s*\()/,object:/\w+(?=\s*\{)/,punctuation:/[!(){}\[\]:=,]/,property:/\w+/},X.hooks.add(`after-tokenize`,function(e){if(e.language===`graphql`)for(var t=e.tokens.filter(function(e){return typeof e!=`string`&&e.type!==`comment`&&e.type!==`scalar`}),n=0;n<t.length;){var r=t[n++];if(r.type===`keyword`&&r.content===`mutation`){var i=[];if(d([`definition-mutation`,`punctuation`])&&u(1).content===`(`){n+=2;var a=f(/^\($/,/^\)$/);if(a===-1)continue;for(;n<a;n++){var o=u(0);o.type===`variable`&&(p(o,`variable-input`),i.push(o.content))}n=a+1}if(d([`punctuation`,`property-query`])&&u(0).content===`{`&&(n++,p(u(0),`property-mutation`),0<i.length)){var s=f(/^\{$/,/^\}$/);if(s!==-1)for(var c=n;c<s;c++){var l=t[c];l.type===`variable`&&0<=i.indexOf(l.content)&&p(l,`variable-input`)}}}}function u(e){return t[n+e]}function d(e,t){t||=0;for(var n=0;n<e.length;n++){var r=u(n+t);if(!r||r.type!==e[n])return}return 1}function f(e,r){for(var i=1,a=n;a<t.length;a++){var o=t[a],s=o.content;if(o.type===`punctuation`&&typeof s==`string`){if(e.test(s))i++;else if(r.test(s)&&--i===0)return a}}return-1}function p(e,t){var n=e.alias;n?Array.isArray(n)||(e.alias=n=[n]):e.alias=n=[],n.push(t)}}),X.languages.sql={comment:{pattern:/(^|[^\\])(?:\/\*[\s\S]*?\*\/|(?:--|\/\/|#).*)/,lookbehind:!0},variable:[{pattern:/@(["'`])(?:\\[\s\S]|(?!\1)[^\\])+\1/,greedy:!0},/@[\w.$]+/],string:{pattern:/(^|[^@\\])("|')(?:\\[\s\S]|(?!\2)[^\\]|\2\2)*\2/,greedy:!0,lookbehind:!0},identifier:{pattern:/(^|[^@\\])`(?:\\[\s\S]|[^`\\]|``)*`/,greedy:!0,lookbehind:!0,inside:{punctuation:/^`|`$/}},function:/\b(?:AVG|COUNT|FIRST|FORMAT|LAST|LCASE|LEN|MAX|MID|MIN|MOD|NOW|ROUND|SUM|UCASE)(?=\s*\()/i,keyword:/\b(?:ACTION|ADD|AFTER|ALGORITHM|ALL|ALTER|ANALYZE|ANY|APPLY|AS|ASC|AUTHORIZATION|AUTO_INCREMENT|BACKUP|BDB|BEGIN|BERKELEYDB|BIGINT|BINARY|BIT|BLOB|BOOL|BOOLEAN|BREAK|BROWSE|BTREE|BULK|BY|CALL|CASCADED?|CASE|CHAIN|CHAR(?:ACTER|SET)?|CHECK(?:POINT)?|CLOSE|CLUSTERED|COALESCE|COLLATE|COLUMNS?|COMMENT|COMMIT(?:TED)?|COMPUTE|CONNECT|CONSISTENT|CONSTRAINT|CONTAINS(?:TABLE)?|CONTINUE|CONVERT|CREATE|CROSS|CURRENT(?:_DATE|_TIME|_TIMESTAMP|_USER)?|CURSOR|CYCLE|DATA(?:BASES?)?|DATE(?:TIME)?|DAY|DBCC|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFINER|DELAYED|DELETE|DELIMITERS?|DENY|DESC|DESCRIBE|DETERMINISTIC|DISABLE|DISCARD|DISK|DISTINCT|DISTINCTROW|DISTRIBUTED|DO|DOUBLE|DROP|DUMMY|DUMP(?:FILE)?|DUPLICATE|ELSE(?:IF)?|ENABLE|ENCLOSED|END|ENGINE|ENUM|ERRLVL|ERRORS|ESCAPED?|EXCEPT|EXEC(?:UTE)?|EXISTS|EXIT|EXPLAIN|EXTENDED|FETCH|FIELDS|FILE|FILLFACTOR|FIRST|FIXED|FLOAT|FOLLOWING|FOR(?: EACH ROW)?|FORCE|FOREIGN|FREETEXT(?:TABLE)?|FROM|FULL|FUNCTION|GEOMETRY(?:COLLECTION)?|GLOBAL|GOTO|GRANT|GROUP|HANDLER|HASH|HAVING|HOLDLOCK|HOUR|IDENTITY(?:COL|_INSERT)?|IF|IGNORE|IMPORT|INDEX|INFILE|INNER|INNODB|INOUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|INVOKER|ISOLATION|ITERATE|JOIN|KEYS?|KILL|LANGUAGE|LAST|LEAVE|LEFT|LEVEL|LIMIT|LINENO|LINES|LINESTRING|LOAD|LOCAL|LOCK|LONG(?:BLOB|TEXT)|LOOP|MATCH(?:ED)?|MEDIUM(?:BLOB|INT|TEXT)|MERGE|MIDDLEINT|MINUTE|MODE|MODIFIES|MODIFY|MONTH|MULTI(?:LINESTRING|POINT|POLYGON)|NATIONAL|NATURAL|NCHAR|NEXT|NO|NONCLUSTERED|NULLIF|NUMERIC|OFF?|OFFSETS?|ON|OPEN(?:DATASOURCE|QUERY|ROWSET)?|OPTIMIZE|OPTION(?:ALLY)?|ORDER|OUT(?:ER|FILE)?|OVER|PARTIAL|PARTITION|PERCENT|PIVOT|PLAN|POINT|POLYGON|PRECEDING|PRECISION|PREPARE|PREV|PRIMARY|PRINT|PRIVILEGES|PROC(?:EDURE)?|PUBLIC|PURGE|QUICK|RAISERROR|READS?|REAL|RECONFIGURE|REFERENCES|RELEASE|RENAME|REPEAT(?:ABLE)?|REPLACE|REPLICATION|REQUIRE|RESIGNAL|RESTORE|RESTRICT|RETURN(?:ING|S)?|REVOKE|RIGHT|ROLLBACK|ROUTINE|ROW(?:COUNT|GUIDCOL|S)?|RTREE|RULE|SAVE(?:POINT)?|SCHEMA|SECOND|SELECT|SERIAL(?:IZABLE)?|SESSION(?:_USER)?|SET(?:USER)?|SHARE|SHOW|SHUTDOWN|SIMPLE|SMALLINT|SNAPSHOT|SOME|SONAME|SQL|START(?:ING)?|STATISTICS|STATUS|STRIPED|SYSTEM_USER|TABLES?|TABLESPACE|TEMP(?:ORARY|TABLE)?|TERMINATED|TEXT(?:SIZE)?|THEN|TIME(?:STAMP)?|TINY(?:BLOB|INT|TEXT)|TOP?|TRAN(?:SACTIONS?)?|TRIGGER|TRUNCATE|TSEQUAL|TYPES?|UNBOUNDED|UNCOMMITTED|UNDEFINED|UNION|UNIQUE|UNLOCK|UNPIVOT|UNSIGNED|UPDATE(?:TEXT)?|USAGE|USE|USER|USING|VALUES?|VAR(?:BINARY|CHAR|CHARACTER|YING)|VIEW|WAITFOR|WARNINGS|WHEN|WHERE|WHILE|WITH(?: ROLLUP|IN)?|WORK|WRITE(?:TEXT)?|YEAR)\b/i,boolean:/\b(?:FALSE|NULL|TRUE)\b/i,number:/\b0x[\da-f]+\b|\b\d+(?:\.\d*)?|\B\.\d+\b/i,operator:/[-+*\/=%^~]|&&?|\|\|?|!=?|<(?:=>?|<|>)?|>[>=]?|\b(?:AND|BETWEEN|DIV|ILIKE|IN|IS|LIKE|NOT|OR|REGEXP|RLIKE|SOUNDS LIKE|XOR)\b/i,punctuation:/[;[\]()`,.]/},function(e){var t=e.languages.javascript[`template-string`],n=t.pattern.source,r=t.inside.interpolation,i=r.inside[`interpolation-punctuation`],a=r.pattern.source;function o(t,r){if(e.languages[t])return{pattern:RegExp(`((?:`+r+`)\\s*)`+n),lookbehind:!0,greedy:!0,inside:{"template-punctuation":{pattern:/^`|`$/,alias:`string`},"embedded-code":{pattern:/[\s\S]+/,alias:t}}}}function s(t,n,r){return t={code:t,grammar:n,language:r},e.hooks.run(`before-tokenize`,t),t.tokens=e.tokenize(t.code,t.grammar),e.hooks.run(`after-tokenize`,t),t.tokens}function c(t,n,o){var c=e.tokenize(t,{interpolation:{pattern:RegExp(a),lookbehind:!0}}),l=0,u={},c=s(c.map(function(e){if(typeof e==`string`)return e;for(var n,r,e=e.content;t.indexOf((r=l++,n=`___`+o.toUpperCase()+`_`+r+`___`))!==-1;);return u[n]=e,n}).join(``),n,o),d=Object.keys(u);return l=0,function t(n){for(var a=0;a<n.length;a++){if(l>=d.length)return;var o,c,f,p,m,h,g,_=n[a];typeof _==`string`||typeof _.content==`string`?(o=d[l],(g=(h=typeof _==`string`?_:_.content).indexOf(o))!==-1&&(++l,c=h.substring(0,g),m=u[o],f=void 0,(p={})[`interpolation-punctuation`]=i,(p=e.tokenize(m,p)).length===3&&((f=[1,1]).push.apply(f,s(p[1],e.languages.javascript,`javascript`)),p.splice.apply(p,f)),f=new e.Token(`interpolation`,p,r.alias,m),p=h.substring(g+o.length),m=[],c&&m.push(c),m.push(f),p&&(t(h=[p]),m.push.apply(m,h)),typeof _==`string`?(n.splice.apply(n,[a,1].concat(m)),a+=m.length-1):_.content=m)):(g=_.content,t(Array.isArray(g)?g:[g]))}}(c),new e.Token(o,c,`language-`+o,t)}e.languages.javascript[`template-string`]=[o(`css`,`\\b(?:styled(?:\\([^)]*\\))?(?:\\s*\\.\\s*\\w+(?:\\([^)]*\\))*)*|css(?:\\s*\\.\\s*(?:global|resolve))?|createGlobalStyle|keyframes)`),o(`html`,`\\bhtml|\\.\\s*(?:inner|outer)HTML\\s*\\+?=`),o(`svg`,`\\bsvg`),o(`markdown`,`\\b(?:markdown|md)`),o(`graphql`,`\\b(?:gql|graphql(?:\\s*\\.\\s*experimental)?)`),o(`sql`,`\\bsql`),t].filter(Boolean);var l={javascript:!0,js:!0,typescript:!0,ts:!0,jsx:!0,tsx:!0};function u(e){return typeof e==`string`?e:Array.isArray(e)?e.map(u).join(``):u(e.content)}e.hooks.add(`after-tokenize`,function(t){t.language in l&&function t(n){for(var r=0,i=n.length;r<i;r++){var a,o,s,l=n[r];typeof l!=`string`&&(a=l.content,Array.isArray(a)?l.type===`template-string`?(l=a[1],a.length===3&&typeof l!=`string`&&l.type===`embedded-code`&&(o=u(l),l=l.alias,l=Array.isArray(l)?l[0]:l,s=e.languages[l])&&(a[1]=c(o,s,l))):t(a):typeof a!=`string`&&t([a]))}}(t.tokens)})}(X),function(e){e.languages.typescript=e.languages.extend(`javascript`,{"class-name":{pattern:/(\b(?:class|extends|implements|instanceof|interface|new|type)\s+)(?!keyof\b)(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?:\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)?/,lookbehind:!0,greedy:!0,inside:null},builtin:/\b(?:Array|Function|Promise|any|boolean|console|never|number|string|symbol|unknown)\b/}),e.languages.typescript.keyword.push(/\b(?:abstract|declare|is|keyof|readonly|require)\b/,/\b(?:asserts|infer|interface|module|namespace|type)\b(?=\s*(?:[{_$a-zA-Z\xA0-\uFFFF]|$))/,/\btype\b(?=\s*(?:[\{*]|$))/),delete e.languages.typescript.parameter,delete e.languages.typescript[`literal-property`];var t=e.languages.extend(`typescript`,{});delete t[`class-name`],e.languages.typescript[`class-name`].inside=t,e.languages.insertBefore(`typescript`,`function`,{decorator:{pattern:/@[$\w\xA0-\uFFFF]+/,inside:{at:{pattern:/^@/,alias:`operator`},function:/^[\s\S]+/}},"generic-function":{pattern:/#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>(?=\s*\()/,greedy:!0,inside:{function:/^#?(?!\s)[_$a-zA-Z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*/,generic:{pattern:/<[\s\S]+/,alias:`class-name`,inside:t}}}}),e.languages.ts=e.languages.typescript}(X),function(e){var t=e.languages.javascript,n=`\\{(?:[^{}]|\\{(?:[^{}]|\\{[^{}]*\\})*\\})+\\}`,r=`(@(?:arg|argument|param|property)\\s+(?:`+n+`\\s+)?)`;e.languages.jsdoc=e.languages.extend(`javadoclike`,{parameter:{pattern:RegExp(r+`(?:(?!\\s)[$\\w\\xA0-\\uFFFF.])+(?=\\s|$)`),lookbehind:!0,inside:{punctuation:/\./}}}),e.languages.insertBefore(`jsdoc`,`keyword`,{"optional-parameter":{pattern:RegExp(r+`\\[(?:(?!\\s)[$\\w\\xA0-\\uFFFF.])+(?:=[^[\\]]+)?\\](?=\\s|$)`),lookbehind:!0,inside:{parameter:{pattern:/(^\[)[$\w\xA0-\uFFFF\.]+/,lookbehind:!0,inside:{punctuation:/\./}},code:{pattern:/(=)[\s\S]*(?=\]$)/,lookbehind:!0,inside:t,alias:`language-javascript`},punctuation:/[=[\]]/}},"class-name":[{pattern:RegExp(`(@(?:augments|class|extends|interface|memberof!?|template|this|typedef)\\s+(?:<TYPE>\\s+)?)[A-Z]\\w*(?:\\.[A-Z]\\w*)*`.replace(/<TYPE>/g,function(){return n})),lookbehind:!0,inside:{punctuation:/\./}},{pattern:RegExp(`(@[a-z]+\\s+)`+n),lookbehind:!0,inside:{string:t.string,number:t.number,boolean:t.boolean,keyword:e.languages.typescript.keyword,operator:/=>|\.\.\.|[&|?:*]/,punctuation:/[.,;=<>{}()[\]]/}}],example:{pattern:/(@example\s+(?!\s))(?:[^@\s]|\s+(?!\s))+?(?=\s*(?:\*\s*)?(?:@\w|\*\/))/,lookbehind:!0,inside:{code:{pattern:/^([\t ]*(?:\*\s*)?)\S.*$/m,lookbehind:!0,inside:t,alias:`language-javascript`}}}}),e.languages.javadoclike.addSupport(`javascript`,e.languages.jsdoc)}(X),function(e){e.languages.flow=e.languages.extend(`javascript`,{}),e.languages.insertBefore(`flow`,`keyword`,{type:[{pattern:/\b(?:[Bb]oolean|Function|[Nn]umber|[Ss]tring|[Ss]ymbol|any|mixed|null|void)\b/,alias:`class-name`}]}),e.languages.flow[`function-variable`].pattern=/(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*(?=\s*=\s*(?:function\b|(?:\([^()]*\)(?:\s*:\s*\w+)?|(?!\s)[_$a-z\xA0-\uFFFF](?:(?!\s)[$\w\xA0-\uFFFF])*)\s*=>))/i,delete e.languages.flow.parameter,e.languages.insertBefore(`flow`,`operator`,{"flow-punctuation":{pattern:/\{\||\|\}/,alias:`punctuation`}}),Array.isArray(e.languages.flow.keyword)||(e.languages.flow.keyword=[e.languages.flow.keyword]),e.languages.flow.keyword.unshift({pattern:/(^|[^$]\b)(?:Class|declare|opaque|type)\b(?!\$)/,lookbehind:!0},{pattern:/(^|[^$]\B)\$(?:Diff|Enum|Exact|Keys|ObjMap|PropertyType|Record|Shape|Subtype|Supertype|await)\b(?!\$)/,lookbehind:!0})}(X),X.languages.n4js=X.languages.extend(`javascript`,{keyword:/\b(?:Array|any|boolean|break|case|catch|class|const|constructor|continue|debugger|declare|default|delete|do|else|enum|export|extends|false|finally|for|from|function|get|if|implements|import|in|instanceof|interface|let|module|new|null|number|package|private|protected|public|return|set|static|string|super|switch|this|throw|true|try|typeof|var|void|while|with|yield)\b/}),X.languages.insertBefore(`n4js`,`constant`,{annotation:{pattern:/@+\w+/,alias:`operator`}}),X.languages.n4jsd=X.languages.n4js,function(e){function t(e,t){return RegExp(e.replace(/<ID>/g,function(){return`(?!\\s)[_$a-zA-Z\\xA0-\\uFFFF](?:(?!\\s)[$\\w\\xA0-\\uFFFF])*`}),t)}e.languages.insertBefore(`javascript`,`function-variable`,{"method-variable":{pattern:RegExp(`(\\.\\s*)`+e.languages.javascript[`function-variable`].pattern.source),lookbehind:!0,alias:[`function-variable`,`method`,`function`,`property-access`]}}),e.languages.insertBefore(`javascript`,`function`,{method:{pattern:RegExp(`(\\.\\s*)`+e.languages.javascript.function.source),lookbehind:!0,alias:[`function`,`property-access`]}}),e.languages.insertBefore(`javascript`,`constant`,{"known-class-name":[{pattern:/\b(?:(?:Float(?:32|64)|(?:Int|Uint)(?:8|16|32)|Uint8Clamped)?Array|ArrayBuffer|BigInt|Boolean|DataView|Date|Error|Function|Intl|JSON|(?:Weak)?(?:Map|Set)|Math|Number|Object|Promise|Proxy|Reflect|RegExp|String|Symbol|WebAssembly)\b/,alias:`class-name`},{pattern:/\b(?:[A-Z]\w*)Error\b/,alias:`class-name`}]}),e.languages.insertBefore(`javascript`,`keyword`,{imports:{pattern:t(`(\\bimport\\b\\s*)(?:<ID>(?:\\s*,\\s*(?:\\*\\s*as\\s+<ID>|\\{[^{}]*\\}))?|\\*\\s*as\\s+<ID>|\\{[^{}]*\\})(?=\\s*\\bfrom\\b)`),lookbehind:!0,inside:e.languages.javascript},exports:{pattern:t(`(\\bexport\\b\\s*)(?:\\*(?:\\s*as\\s+<ID>)?(?=\\s*\\bfrom\\b)|\\{[^{}]*\\})`),lookbehind:!0,inside:e.languages.javascript}}),e.languages.javascript.keyword.unshift({pattern:/\b(?:as|default|export|from|import)\b/,alias:`module`},{pattern:/\b(?:await|break|catch|continue|do|else|finally|for|if|return|switch|throw|try|while|yield)\b/,alias:`control-flow`},{pattern:/\bnull\b/,alias:[`null`,`nil`]},{pattern:/\bundefined\b/,alias:`nil`}),e.languages.insertBefore(`javascript`,`operator`,{spread:{pattern:/\.{3}/,alias:`operator`},arrow:{pattern:/=>/,alias:`operator`}}),e.languages.insertBefore(`javascript`,`punctuation`,{"property-access":{pattern:t(`(\\.\\s*)#?<ID>`),lookbehind:!0},"maybe-class-name":{pattern:/(^|[^$\w\xA0-\uFFFF])[A-Z][$\w\xA0-\uFFFF]+/,lookbehind:!0},dom:{pattern:/\b(?:document|(?:local|session)Storage|location|navigator|performance|window)\b/,alias:`variable`},console:{pattern:/\bconsole(?=\s*\.)/,alias:`class-name`}});for(var n=[`function`,`function-variable`,`method`,`method-variable`,`property-access`],r=0;r<n.length;r++){var i=n[r],a=e.languages.javascript[i],i=(a=e.util.type(a)===`RegExp`?e.languages.javascript[i]={pattern:a}:a).inside||{};(a.inside=i)[`maybe-class-name`]=/^[A-Z][\s\S]*/}}(X),function(e){var t=e.util.clone(e.languages.javascript),n=`(?:\\s|\\/\\/.*(?!.)|\\/\\*(?:[^*]|\\*(?!\\/))\\*\\/)`,r=`(?:\\{(?:\\{(?:\\{[^{}]*\\}|[^{}])*\\}|[^{}])*\\})`,i=`(?:\\{<S>*\\.{3}(?:[^{}]|<BRACES>)*\\})`;function a(e,t){return e=e.replace(/<S>/g,function(){return n}).replace(/<BRACES>/g,function(){return r}).replace(/<SPREAD>/g,function(){return i}),RegExp(e,t)}i=a(i).source,e.languages.jsx=e.languages.extend(`markup`,t),e.languages.jsx.tag.pattern=a(`<\\/?(?:[\\w.:-]+(?:<S>+(?:[\\w.:$-]+(?:=(?:"(?:\\\\[\\s\\S]|[^\\\\"])*"|'(?:\\\\[\\s\\S]|[^\\\\'])*'|[^\\s{'"/>=]+|<BRACES>))?|<SPREAD>))*<S>*\\/?)?>`),e.languages.jsx.tag.inside.tag.pattern=/^<\/?[^\s>\/]*/,e.languages.jsx.tag.inside[`attr-value`].pattern=/=(?!\{)(?:"(?:\\[\s\S]|[^\\"])*"|'(?:\\[\s\S]|[^\\'])*'|[^\s'">]+)/,e.languages.jsx.tag.inside.tag.inside[`class-name`]=/^[A-Z]\w*(?:\.[A-Z]\w*)*$/,e.languages.jsx.tag.inside.comment=t.comment,e.languages.insertBefore(`inside`,`attr-name`,{spread:{pattern:a(`<SPREAD>`),inside:e.languages.jsx}},e.languages.jsx.tag),e.languages.insertBefore(`inside`,`special-attr`,{script:{pattern:a(`=<BRACES>`),alias:`language-javascript`,inside:{"script-punctuation":{pattern:/^=(?=\{)/,alias:`punctuation`},rest:e.languages.jsx}}},e.languages.jsx.tag);function o(t){for(var n=[],r=0;r<t.length;r++){var i=t[r],a=!1;typeof i!=`string`&&(i.type===`tag`&&i.content[0]&&i.content[0].type===`tag`?i.content[0].content[0].content===`</`?0<n.length&&n[n.length-1].tagName===s(i.content[0].content[1])&&n.pop():i.content[i.content.length-1].content!==`/>`&&n.push({tagName:s(i.content[0].content[1]),openedBraces:0}):0<n.length&&i.type===`punctuation`&&i.content===`{`?n[n.length-1].openedBraces++:0<n.length&&0<n[n.length-1].openedBraces&&i.type===`punctuation`&&i.content===`}`?n[n.length-1].openedBraces--:a=!0),(a||typeof i==`string`)&&0<n.length&&n[n.length-1].openedBraces===0&&(a=s(i),r<t.length-1&&(typeof t[r+1]==`string`||t[r+1].type===`plain-text`)&&(a+=s(t[r+1]),t.splice(r+1,1)),0<r&&(typeof t[r-1]==`string`||t[r-1].type===`plain-text`)&&(a=s(t[r-1])+a,t.splice(r-1,1),r--),t[r]=new e.Token(`plain-text`,a,null,a)),i.content&&typeof i.content!=`string`&&o(i.content)}}var s=function(e){return e?typeof e==`string`?e:typeof e.content==`string`?e.content:e.content.map(s).join(``):``};e.hooks.add(`after-tokenize`,function(e){e.language!==`jsx`&&e.language!==`tsx`||o(e.tokens)})}(X),function(e){var t=e.util.clone(e.languages.typescript),t=(e.languages.tsx=e.languages.extend(`jsx`,t),delete e.languages.tsx.parameter,delete e.languages.tsx[`literal-property`],e.languages.tsx.tag);t.pattern=RegExp(`(^|[^\\w$]|(?=<\\/))(?:`+t.pattern.source+`)`,t.pattern.flags),t.lookbehind=!0}(X),X.languages.swift={comment:{pattern:/(^|[^\\:])(?:\/\/.*|\/\*(?:[^/*]|\/(?!\*)|\*(?!\/)|\/\*(?:[^*]|\*(?!\/))*\*\/)*\*\/)/,lookbehind:!0,greedy:!0},"string-literal":[{pattern:RegExp(`(^|[^"#])(?:"(?:\\\\(?:\\((?:[^()]|\\([^()]*\\))*\\)|\\r\\n|[^(])|[^\\\\\\r\\n"])*"|"""(?:\\\\(?:\\((?:[^()]|\\([^()]*\\))*\\)|[^(])|[^\\\\"]|"(?!""))*""")(?!["#])`),lookbehind:!0,greedy:!0,inside:{interpolation:{pattern:/(\\\()(?:[^()]|\([^()]*\))*(?=\))/,lookbehind:!0,inside:null},"interpolation-punctuation":{pattern:/^\)|\\\($/,alias:`punctuation`},punctuation:/\\(?=[\r\n])/,string:/[\s\S]+/}},{pattern:RegExp(`(^|[^"#])(#+)(?:"(?:\\\\(?:#+\\((?:[^()]|\\([^()]*\\))*\\)|\\r\\n|[^#])|[^\\\\\\r\\n])*?"|"""(?:\\\\(?:#+\\((?:[^()]|\\([^()]*\\))*\\)|[^#])|[^\\\\])*?""")\\2`),lookbehind:!0,greedy:!0,inside:{interpolation:{pattern:/(\\#+\()(?:[^()]|\([^()]*\))*(?=\))/,lookbehind:!0,inside:null},"interpolation-punctuation":{pattern:/^\)|\\#+\($/,alias:`punctuation`},string:/[\s\S]+/}}],directive:{pattern:RegExp(`#(?:(?:elseif|if)\\b(?:[ 	]*(?:![ \\t]*)?(?:\\b\\w+\\b(?:[ \\t]*\\((?:[^()]|\\([^()]*\\))*\\))?|\\((?:[^()]|\\([^()]*\\))*\\))(?:[ \\t]*(?:&&|\\|\\|))?)+|(?:else|endif)\\b)`),alias:`property`,inside:{"directive-name":/^#\w+/,boolean:/\b(?:false|true)\b/,number:/\b\d+(?:\.\d+)*\b/,operator:/!|&&|\|\||[<>]=?/,punctuation:/[(),]/}},literal:{pattern:/#(?:colorLiteral|column|dsohandle|file(?:ID|Literal|Path)?|function|imageLiteral|line)\b/,alias:`constant`},"other-directive":{pattern:/#\w+\b/,alias:`property`},attribute:{pattern:/@\w+/,alias:`atrule`},"function-definition":{pattern:/(\bfunc\s+)\w+/,lookbehind:!0,alias:`function`},label:{pattern:/\b(break|continue)\s+\w+|\b[a-zA-Z_]\w*(?=\s*:\s*(?:for|repeat|while)\b)/,lookbehind:!0,alias:`important`},keyword:/\b(?:Any|Protocol|Self|Type|actor|as|assignment|associatedtype|associativity|async|await|break|case|catch|class|continue|convenience|default|defer|deinit|didSet|do|dynamic|else|enum|extension|fallthrough|fileprivate|final|for|func|get|guard|higherThan|if|import|in|indirect|infix|init|inout|internal|is|isolated|lazy|left|let|lowerThan|mutating|none|nonisolated|nonmutating|open|operator|optional|override|postfix|precedencegroup|prefix|private|protocol|public|repeat|required|rethrows|return|right|safe|self|set|some|static|struct|subscript|super|switch|throw|throws|try|typealias|unowned|unsafe|var|weak|where|while|willSet)\b/,boolean:/\b(?:false|true)\b/,nil:{pattern:/\bnil\b/,alias:`constant`},"short-argument":/\$\d+\b/,omit:{pattern:/\b_\b/,alias:`keyword`},number:/\b(?:[\d_]+(?:\.[\de_]+)?|0x[a-f0-9_]+(?:\.[a-f0-9p_]+)?|0b[01_]+|0o[0-7_]+)\b/i,"class-name":/\b[A-Z](?:[A-Z_\d]*[a-z]\w*)?\b/,function:/\b[a-z_]\w*(?=\s*\()/i,constant:/\b(?:[A-Z_]{2,}|k[A-Z][A-Za-z_]+)\b/,operator:/[-+*/%=!<>&|^~?]+|\.[.\-+*/%=!<>&|^~?]+/,punctuation:/[{}[\]();,.:\\]/},X.languages.swift[`string-literal`].forEach(function(e){e.inside.interpolation.inside=X.languages.swift}),function(e){e.languages.kotlin=e.languages.extend(`clike`,{keyword:{pattern:/(^|[^.])\b(?:abstract|actual|annotation|as|break|by|catch|class|companion|const|constructor|continue|crossinline|data|do|dynamic|else|enum|expect|external|final|finally|for|fun|get|if|import|in|infix|init|inline|inner|interface|internal|is|lateinit|noinline|null|object|open|operator|out|override|package|private|protected|public|reified|return|sealed|set|super|suspend|tailrec|this|throw|to|try|typealias|val|var|vararg|when|where|while)\b/,lookbehind:!0},function:[{pattern:/(?:`[^\r\n`]+`|\b\w+)(?=\s*\()/,greedy:!0},{pattern:/(\.)(?:`[^\r\n`]+`|\w+)(?=\s*\{)/,lookbehind:!0,greedy:!0}],number:/\b(?:0[xX][\da-fA-F]+(?:_[\da-fA-F]+)*|0[bB][01]+(?:_[01]+)*|\d+(?:_\d+)*(?:\.\d+(?:_\d+)*)?(?:[eE][+-]?\d+(?:_\d+)*)?[fFL]?)\b/,operator:/\+[+=]?|-[-=>]?|==?=?|!(?:!|==?)?|[\/*%<>]=?|[?:]:?|\.\.|&&|\|\||\b(?:and|inv|or|shl|shr|ushr|xor)\b/}),delete e.languages.kotlin[`class-name`];var t={"interpolation-punctuation":{pattern:/^\$\{?|\}$/,alias:`punctuation`},expression:{pattern:/[\s\S]+/,inside:e.languages.kotlin}};e.languages.insertBefore(`kotlin`,`string`,{"string-literal":[{pattern:/"""(?:[^$]|\$(?:(?!\{)|\{[^{}]*\}))*?"""/,alias:`multiline`,inside:{interpolation:{pattern:/\$(?:[a-z_]\w*|\{[^{}]*\})/i,inside:t},string:/[\s\S]+/}},{pattern:/"(?:[^"\\\r\n$]|\\.|\$(?:(?!\{)|\{[^{}]*\}))*"/,alias:`singleline`,inside:{interpolation:{pattern:/((?:^|[^\\])(?:\\{2})*)\$(?:[a-z_]\w*|\{[^{}]*\})/i,lookbehind:!0,inside:t},string:/[\s\S]+/}}],char:{pattern:/'(?:[^'\\\r\n]|\\(?:.|u[a-fA-F0-9]{0,4}))'/,greedy:!0}}),delete e.languages.kotlin.string,e.languages.insertBefore(`kotlin`,`keyword`,{annotation:{pattern:/\B@(?:\w+:)?(?:[A-Z]\w*|\[[^\]]+\])/,alias:`builtin`}}),e.languages.insertBefore(`kotlin`,`function`,{label:{pattern:/\b\w+@|@\w+\b/,alias:`symbol`}}),e.languages.kt=e.languages.kotlin,e.languages.kts=e.languages.kotlin}(X),X.languages.c=X.languages.extend(`clike`,{comment:{pattern:/\/\/(?:[^\r\n\\]|\\(?:\r\n?|\n|(?![\r\n])))*|\/\*[\s\S]*?(?:\*\/|$)/,greedy:!0},string:{pattern:/"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"/,greedy:!0},"class-name":{pattern:/(\b(?:enum|struct)\s+(?:__attribute__\s*\(\([\s\S]*?\)\)\s*)?)\w+|\b[a-z]\w*_t\b/,lookbehind:!0},keyword:/\b(?:_Alignas|_Alignof|_Atomic|_Bool|_Complex|_Generic|_Imaginary|_Noreturn|_Static_assert|_Thread_local|__attribute__|asm|auto|break|case|char|const|continue|default|do|double|else|enum|extern|float|for|goto|if|inline|int|long|register|return|short|signed|sizeof|static|struct|switch|typedef|typeof|union|unsigned|void|volatile|while)\b/,function:/\b[a-z_]\w*(?=\s*\()/i,number:/(?:\b0x(?:[\da-f]+(?:\.[\da-f]*)?|\.[\da-f]+)(?:p[+-]?\d+)?|(?:\b\d+(?:\.\d*)?|\B\.\d+)(?:e[+-]?\d+)?)[ful]{0,4}/i,operator:/>>=?|<<=?|->|([-+&|:])\1|[?:~]|[-+*/%&|^!=<>]=?/}),X.languages.insertBefore(`c`,`string`,{char:{pattern:/'(?:\\(?:\r\n|[\s\S])|[^'\\\r\n]){0,32}'/,greedy:!0}}),X.languages.insertBefore(`c`,`string`,{macro:{pattern:/(^[\t ]*)#\s*[a-z](?:[^\r\n\\/]|\/(?!\*)|\/\*(?:[^*]|\*(?!\/))*\*\/|\\(?:\r\n|[\s\S]))*/im,lookbehind:!0,greedy:!0,alias:`property`,inside:{string:[{pattern:/^(#\s*include\s*)<[^>]+>/,lookbehind:!0},X.languages.c.string],char:X.languages.c.char,comment:X.languages.c.comment,"macro-name":[{pattern:/(^#\s*define\s+)\w+\b(?!\()/i,lookbehind:!0},{pattern:/(^#\s*define\s+)\w+\b(?=\()/i,lookbehind:!0,alias:`function`}],directive:{pattern:/^(#\s*)[a-z]+/,lookbehind:!0,alias:`keyword`},"directive-hash":/^#/,punctuation:/##|\\(?=[\r\n])/,expression:{pattern:/\S[\s\S]*/,inside:X.languages.c}}}}),X.languages.insertBefore(`c`,`function`,{constant:/\b(?:EOF|NULL|SEEK_CUR|SEEK_END|SEEK_SET|__DATE__|__FILE__|__LINE__|__TIMESTAMP__|__TIME__|__func__|stderr|stdin|stdout)\b/}),delete X.languages.c.boolean,X.languages.objectivec=X.languages.extend(`c`,{string:{pattern:/@?"(?:\\(?:\r\n|[\s\S])|[^"\\\r\n])*"/,greedy:!0},keyword:/\b(?:asm|auto|break|case|char|const|continue|default|do|double|else|enum|extern|float|for|goto|if|in|inline|int|long|register|return|self|short|signed|sizeof|static|struct|super|switch|typedef|typeof|union|unsigned|void|volatile|while)\b|(?:@interface|@end|@implementation|@protocol|@class|@public|@protected|@private|@property|@try|@catch|@finally|@throw|@synthesize|@dynamic|@selector)\b/,operator:/-[->]?|\+\+?|!=?|<<?=?|>>?=?|==?|&&?|\|\|?|[~^%?*\/@]/}),delete X.languages.objectivec[`class-name`],X.languages.objc=X.languages.objectivec,X.languages.reason=X.languages.extend(`clike`,{string:{pattern:/"(?:\\(?:\r\n|[\s\S])|[^\\\r\n"])*"/,greedy:!0},"class-name":/\b[A-Z]\w*/,keyword:/\b(?:and|as|assert|begin|class|constraint|do|done|downto|else|end|exception|external|for|fun|function|functor|if|in|include|inherit|initializer|lazy|let|method|module|mutable|new|nonrec|object|of|open|or|private|rec|sig|struct|switch|then|to|try|type|val|virtual|when|while|with)\b/,operator:/\.{3}|:[:=]|\|>|->|=(?:==?|>)?|<=?|>=?|[|^?'#!~`]|[+\-*\/]\.?|\b(?:asr|land|lor|lsl|lsr|lxor|mod)\b/}),X.languages.insertBefore(`reason`,`class-name`,{char:{pattern:/'(?:\\x[\da-f]{2}|\\o[0-3][0-7][0-7]|\\\d{3}|\\.|[^'\\\r\n])'/,greedy:!0},constructor:/\b[A-Z]\w*\b(?!\s*\.)/,label:{pattern:/\b[a-z]\w*(?=::)/,alias:`symbol`}}),delete X.languages.reason.function,function(e){for(var t=`\\/\\*(?:[^*/]|\\*(?!\\/)|\\/(?!\\*)|<self>)*\\*\\/`,n=0;n<2;n++)t=t.replace(/<self>/g,function(){return t});t=t.replace(/<self>/g,function(){return`[^\\s\\S]`}),e.languages.rust={comment:[{pattern:RegExp(`(^|[^\\\\])`+t),lookbehind:!0,greedy:!0},{pattern:/(^|[^\\:])\/\/.*/,lookbehind:!0,greedy:!0}],string:{pattern:/b?"(?:\\[\s\S]|[^\\"])*"|b?r(#*)"(?:[^"]|"(?!\1))*"\1/,greedy:!0},char:{pattern:/b?'(?:\\(?:x[0-7][\da-fA-F]|u\{(?:[\da-fA-F]_*){1,6}\}|.)|[^\\\r\n\t'])'/,greedy:!0},attribute:{pattern:/#!?\[(?:[^\[\]"]|"(?:\\[\s\S]|[^\\"])*")*\]/,greedy:!0,alias:`attr-name`,inside:{string:null}},"closure-params":{pattern:/([=(,:]\s*|\bmove\s*)\|[^|]*\||\|[^|]*\|(?=\s*(?:\{|->))/,lookbehind:!0,greedy:!0,inside:{"closure-punctuation":{pattern:/^\||\|$/,alias:`punctuation`},rest:null}},"lifetime-annotation":{pattern:/'\w+/,alias:`symbol`},"fragment-specifier":{pattern:/(\$\w+:)[a-z]+/,lookbehind:!0,alias:`punctuation`},variable:/\$\w+/,"function-definition":{pattern:/(\bfn\s+)\w+/,lookbehind:!0,alias:`function`},"type-definition":{pattern:/(\b(?:enum|struct|trait|type|union)\s+)\w+/,lookbehind:!0,alias:`class-name`},"module-declaration":[{pattern:/(\b(?:crate|mod)\s+)[a-z][a-z_\d]*/,lookbehind:!0,alias:`namespace`},{pattern:/(\b(?:crate|self|super)\s*)::\s*[a-z][a-z_\d]*\b(?:\s*::(?:\s*[a-z][a-z_\d]*\s*::)*)?/,lookbehind:!0,alias:`namespace`,inside:{punctuation:/::/}}],keyword:[/\b(?:Self|abstract|as|async|await|become|box|break|const|continue|crate|do|dyn|else|enum|extern|final|fn|for|if|impl|in|let|loop|macro|match|mod|move|mut|override|priv|pub|ref|return|self|static|struct|super|trait|try|type|typeof|union|unsafe|unsized|use|virtual|where|while|yield)\b/,/\b(?:bool|char|f(?:32|64)|[ui](?:8|16|32|64|128|size)|str)\b/],function:/\b[a-z_]\w*(?=\s*(?:::\s*<|\())/,macro:{pattern:/\b\w+!/,alias:`property`},constant:/\b[A-Z_][A-Z_\d]+\b/,"class-name":/\b[A-Z]\w*\b/,namespace:{pattern:/(?:\b[a-z][a-z_\d]*\s*::\s*)*\b[a-z][a-z_\d]*\s*::(?!\s*<)/,inside:{punctuation:/::/}},number:/\b(?:0x[\dA-Fa-f](?:_?[\dA-Fa-f])*|0o[0-7](?:_?[0-7])*|0b[01](?:_?[01])*|(?:(?:\d(?:_?\d)*)?\.)?\d(?:_?\d)*(?:[Ee][+-]?\d+)?)(?:_?(?:f32|f64|[iu](?:8|16|32|64|size)?))?\b/,boolean:/\b(?:false|true)\b/,punctuation:/->|\.\.=|\.{1,3}|::|[{}[\];(),:]/,operator:/[-+*\/%!^]=?|=[=>]?|&[&=]?|\|[|=]?|<<?=?|>>?=?|[@?]/},e.languages.rust[`closure-params`].inside.rest=e.languages.rust,e.languages.rust.attribute.inside.string=e.languages.rust.string}(X),X.languages.go=X.languages.extend(`clike`,{string:{pattern:/(^|[^\\])"(?:\\.|[^"\\\r\n])*"|`[^`]*`/,lookbehind:!0,greedy:!0},keyword:/\b(?:break|case|chan|const|continue|default|defer|else|fallthrough|for|func|go(?:to)?|if|import|interface|map|package|range|return|select|struct|switch|type|var)\b/,boolean:/\b(?:_|false|iota|nil|true)\b/,number:[/\b0(?:b[01_]+|o[0-7_]+)i?\b/i,/\b0x(?:[a-f\d_]+(?:\.[a-f\d_]*)?|\.[a-f\d_]+)(?:p[+-]?\d+(?:_\d+)*)?i?(?!\w)/i,/(?:\b\d[\d_]*(?:\.[\d_]*)?|\B\.\d[\d_]*)(?:e[+-]?[\d_]+)?i?(?!\w)/i],operator:/[*\/%^!=]=?|\+[=+]?|-[=-]?|\|[=|]?|&(?:=|&|\^=?)?|>(?:>=?|=)?|<(?:<=?|=|-)?|:=|\.\.\./,builtin:/\b(?:append|bool|byte|cap|close|complex|complex(?:64|128)|copy|delete|error|float(?:32|64)|u?int(?:8|16|32|64)?|imag|len|make|new|panic|print(?:ln)?|real|recover|rune|string|uintptr)\b/}),X.languages.insertBefore(`go`,`string`,{char:{pattern:/'(?:\\.|[^'\\\r\n]){0,10}'/,greedy:!0}}),delete X.languages.go[`class-name`],function(e){var t=/\b(?:alignas|alignof|asm|auto|bool|break|case|catch|char|char16_t|char32_t|char8_t|class|co_await|co_return|co_yield|compl|concept|const|const_cast|consteval|constexpr|constinit|continue|decltype|default|delete|do|double|dynamic_cast|else|enum|explicit|export|extern|final|float|for|friend|goto|if|import|inline|int|int16_t|int32_t|int64_t|int8_t|long|module|mutable|namespace|new|noexcept|nullptr|operator|override|private|protected|public|register|reinterpret_cast|requires|return|short|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|throw|try|typedef|typeid|typename|uint16_t|uint32_t|uint64_t|uint8_t|union|unsigned|using|virtual|void|volatile|wchar_t|while)\b/,n=`\\b(?!<keyword>)\\w+(?:\\s*\\.\\s*\\w+)*\\b`.replace(/<keyword>/g,function(){return t.source});e.languages.cpp=e.languages.extend(`c`,{"class-name":[{pattern:RegExp(`(\\b(?:class|concept|enum|struct|typename)\\s+)(?!<keyword>)\\w+`.replace(/<keyword>/g,function(){return t.source})),lookbehind:!0},/\b[A-Z]\w*(?=\s*::\s*\w+\s*\()/,/\b[A-Z_]\w*(?=\s*::\s*~\w+\s*\()/i,/\b\w+(?=\s*<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>\s*::\s*\w+\s*\()/],keyword:t,number:{pattern:/(?:\b0b[01']+|\b0x(?:[\da-f']+(?:\.[\da-f']*)?|\.[\da-f']+)(?:p[+-]?[\d']+)?|(?:\b[\d']+(?:\.[\d']*)?|\B\.[\d']+)(?:e[+-]?[\d']+)?)[ful]{0,4}/i,greedy:!0},operator:/>>=?|<<=?|->|--|\+\+|&&|\|\||[?:~]|<=>|[-+*/%&|^!=<>]=?|\b(?:and|and_eq|bitand|bitor|not|not_eq|or|or_eq|xor|xor_eq)\b/,boolean:/\b(?:false|true)\b/}),e.languages.insertBefore(`cpp`,`string`,{module:{pattern:RegExp(`(\\b(?:import|module)\\s+)(?:"(?:\\\\(?:\\r\\n|[\\s\\S])|[^"\\\\\\r\\n])*"|<[^<>\\r\\n]*>|`+`<mod-name>(?:\\s*:\\s*<mod-name>)?|:\\s*<mod-name>`.replace(/<mod-name>/g,function(){return n})+`)`),lookbehind:!0,greedy:!0,inside:{string:/^[<"][\s\S]+/,operator:/:/,punctuation:/\./}},"raw-string":{pattern:/R"([^()\\ ]{0,16})\([\s\S]*?\)\1"/,alias:`string`,greedy:!0}}),e.languages.insertBefore(`cpp`,`keyword`,{"generic-function":{pattern:/\b(?!operator\b)[a-z_]\w*\s*<(?:[^<>]|<[^<>]*>)*>(?=\s*\()/i,inside:{function:/^\w+/,generic:{pattern:/<[\s\S]+/,alias:`class-name`,inside:e.languages.cpp}}}}),e.languages.insertBefore(`cpp`,`operator`,{"double-colon":{pattern:/::/,alias:`punctuation`}}),e.languages.insertBefore(`cpp`,`class-name`,{"base-clause":{pattern:/(\b(?:class|struct)\s+\w+\s*:\s*)[^;{}"'\s]+(?:\s+[^;{}"'\s]+)*(?=\s*[;{])/,lookbehind:!0,greedy:!0,inside:e.languages.extend(`cpp`,{})}}),e.languages.insertBefore(`inside`,`double-colon`,{"class-name":/\b[a-z_]\w*\b(?!\s*::)/i},e.languages.cpp[`base-clause`])}(X),X.languages.python={comment:{pattern:/(^|[^\\])#.*/,lookbehind:!0,greedy:!0},"string-interpolation":{pattern:/(?:f|fr|rf)(?:("""|''')[\s\S]*?\1|("|')(?:\\.|(?!\2)[^\\\r\n])*\2)/i,greedy:!0,inside:{interpolation:{pattern:/((?:^|[^{])(?:\{\{)*)\{(?!\{)(?:[^{}]|\{(?!\{)(?:[^{}]|\{(?!\{)(?:[^{}])+\})+\})+\}/,lookbehind:!0,inside:{"format-spec":{pattern:/(:)[^:(){}]+(?=\}$)/,lookbehind:!0},"conversion-option":{pattern:/![sra](?=[:}]$)/,alias:`punctuation`},rest:null}},string:/[\s\S]+/}},"triple-quoted-string":{pattern:/(?:[rub]|br|rb)?("""|''')[\s\S]*?\1/i,greedy:!0,alias:`string`},string:{pattern:/(?:[rub]|br|rb)?("|')(?:\\.|(?!\1)[^\\\r\n])*\1/i,greedy:!0},function:{pattern:/((?:^|\s)def[ \t]+)[a-zA-Z_]\w*(?=\s*\()/g,lookbehind:!0},"class-name":{pattern:/(\bclass\s+)\w+/i,lookbehind:!0},decorator:{pattern:/(^[\t ]*)@\w+(?:\.\w+)*/m,lookbehind:!0,alias:[`annotation`,`punctuation`],inside:{punctuation:/\./}},keyword:/\b(?:_(?=\s*:)|and|as|assert|async|await|break|case|class|continue|def|del|elif|else|except|exec|finally|for|from|global|if|import|in|is|lambda|match|nonlocal|not|or|pass|print|raise|return|try|while|with|yield)\b/,builtin:/\b(?:__import__|abs|all|any|apply|ascii|basestring|bin|bool|buffer|bytearray|bytes|callable|chr|classmethod|cmp|coerce|compile|complex|delattr|dict|dir|divmod|enumerate|eval|execfile|file|filter|float|format|frozenset|getattr|globals|hasattr|hash|help|hex|id|input|int|intern|isinstance|issubclass|iter|len|list|locals|long|map|max|memoryview|min|next|object|oct|open|ord|pow|property|range|raw_input|reduce|reload|repr|reversed|round|set|setattr|slice|sorted|staticmethod|str|sum|super|tuple|type|unichr|unicode|vars|xrange|zip)\b/,boolean:/\b(?:False|None|True)\b/,number:/\b0(?:b(?:_?[01])+|o(?:_?[0-7])+|x(?:_?[a-f0-9])+)\b|(?:\b\d+(?:_\d+)*(?:\.(?:\d+(?:_\d+)*)?)?|\B\.\d+(?:_\d+)*)(?:e[+-]?\d+(?:_\d+)*)?j?(?!\w)/i,operator:/[-+%=]=?|!=|:=|\*\*?=?|\/\/?=?|<[<=>]?|>[=>]?|[&|^~]/,punctuation:/[{}[\];(),.:]/},X.languages.python[`string-interpolation`].inside.interpolation.inside.rest=X.languages.python,X.languages.py=X.languages.python,X.languages.json={property:{pattern:/(^|[^\\])"(?:\\.|[^\\"\r\n])*"(?=\s*:)/,lookbehind:!0,greedy:!0},string:{pattern:/(^|[^\\])"(?:\\.|[^\\"\r\n])*"(?!\s*:)/,lookbehind:!0,greedy:!0},comment:{pattern:/\/\/.*|\/\*[\s\S]*?(?:\*\/|$)/,greedy:!0},number:/-?\b\d+(?:\.\d+)?(?:e[+-]?\d+)?\b/i,punctuation:/[{}[\],]/,operator:/:/,boolean:/\b(?:false|true)\b/,null:{pattern:/\bnull\b/,alias:`keyword`}},X.languages.webmanifest=X.languages.json;var C_={};x_(C_,{dracula:()=>w_,duotoneDark:()=>T_,duotoneLight:()=>E_,github:()=>D_,gruvboxMaterialDark:()=>U_,gruvboxMaterialLight:()=>W_,jettwaveDark:()=>z_,jettwaveLight:()=>B_,nightOwl:()=>O_,nightOwlLight:()=>k_,oceanicNext:()=>j_,okaidia:()=>M_,oneDark:()=>V_,oneLight:()=>H_,palenight:()=>N_,shadesOfPurple:()=>P_,synthwave84:()=>F_,ultramin:()=>I_,vsDark:()=>L_,vsLight:()=>R_});var w_={plain:{color:`#F8F8F2`,backgroundColor:`#282A36`},styles:[{types:[`prolog`,`constant`,`builtin`],style:{color:`rgb(189, 147, 249)`}},{types:[`inserted`,`function`],style:{color:`rgb(80, 250, 123)`}},{types:[`deleted`],style:{color:`rgb(255, 85, 85)`}},{types:[`changed`],style:{color:`rgb(255, 184, 108)`}},{types:[`punctuation`,`symbol`],style:{color:`rgb(248, 248, 242)`}},{types:[`string`,`char`,`tag`,`selector`],style:{color:`rgb(255, 121, 198)`}},{types:[`keyword`,`variable`],style:{color:`rgb(189, 147, 249)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(98, 114, 164)`}},{types:[`attr-name`],style:{color:`rgb(241, 250, 140)`}}]},T_={plain:{backgroundColor:`#2a2734`,color:`#9a86fd`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`,`punctuation`],style:{color:`#6c6783`}},{types:[`namespace`],style:{opacity:.7}},{types:[`tag`,`operator`,`number`],style:{color:`#e09142`}},{types:[`property`,`function`],style:{color:`#9a86fd`}},{types:[`tag-id`,`selector`,`atrule-id`],style:{color:`#eeebff`}},{types:[`attr-name`],style:{color:`#c4b9fe`}},{types:[`boolean`,`string`,`entity`,`url`,`attr-value`,`keyword`,`control`,`directive`,`unit`,`statement`,`regex`,`atrule`,`placeholder`,`variable`],style:{color:`#ffcc99`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`#c4b9fe`}}]},E_={plain:{backgroundColor:`#faf8f5`,color:`#728fcb`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`,`punctuation`],style:{color:`#b6ad9a`}},{types:[`namespace`],style:{opacity:.7}},{types:[`tag`,`operator`,`number`],style:{color:`#063289`}},{types:[`property`,`function`],style:{color:`#b29762`}},{types:[`tag-id`,`selector`,`atrule-id`],style:{color:`#2d2006`}},{types:[`attr-name`],style:{color:`#896724`}},{types:[`boolean`,`string`,`entity`,`url`,`attr-value`,`keyword`,`control`,`directive`,`unit`,`statement`,`regex`,`atrule`],style:{color:`#728fcb`}},{types:[`placeholder`,`variable`],style:{color:`#93abdc`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`#896724`}}]},D_={plain:{color:`#393A34`,backgroundColor:`#f6f8fa`},styles:[{types:[`comment`,`prolog`,`doctype`,`cdata`],style:{color:`#999988`,fontStyle:`italic`}},{types:[`namespace`],style:{opacity:.7}},{types:[`string`,`attr-value`],style:{color:`#e3116c`}},{types:[`punctuation`,`operator`],style:{color:`#393A34`}},{types:[`entity`,`url`,`symbol`,`number`,`boolean`,`variable`,`constant`,`property`,`regex`,`inserted`],style:{color:`#36acaa`}},{types:[`atrule`,`keyword`,`attr-name`,`selector`],style:{color:`#00a4db`}},{types:[`function`,`deleted`,`tag`],style:{color:`#d73a49`}},{types:[`function-variable`],style:{color:`#6f42c1`}},{types:[`tag`,`selector`,`keyword`],style:{color:`#00009f`}}]},O_={plain:{color:`#d6deeb`,backgroundColor:`#011627`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`,fontStyle:`italic`}},{types:[`inserted`,`attr-name`],style:{color:`rgb(173, 219, 103)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(99, 119, 119)`,fontStyle:`italic`}},{types:[`string`,`url`],style:{color:`rgb(173, 219, 103)`}},{types:[`variable`],style:{color:`rgb(214, 222, 235)`}},{types:[`number`],style:{color:`rgb(247, 140, 108)`}},{types:[`builtin`,`char`,`constant`,`function`],style:{color:`rgb(130, 170, 255)`}},{types:[`punctuation`],style:{color:`rgb(199, 146, 234)`}},{types:[`selector`,`doctype`],style:{color:`rgb(199, 146, 234)`,fontStyle:`italic`}},{types:[`class-name`],style:{color:`rgb(255, 203, 139)`}},{types:[`tag`,`operator`,`keyword`],style:{color:`rgb(127, 219, 202)`}},{types:[`boolean`],style:{color:`rgb(255, 88, 116)`}},{types:[`property`],style:{color:`rgb(128, 203, 196)`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`}}]},k_={plain:{color:`#403f53`,backgroundColor:`#FBFBFB`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`,fontStyle:`italic`}},{types:[`inserted`,`attr-name`],style:{color:`rgb(72, 118, 214)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`rgb(152, 159, 177)`,fontStyle:`italic`}},{types:[`string`,`builtin`,`char`,`constant`,`url`],style:{color:`rgb(72, 118, 214)`}},{types:[`variable`],style:{color:`rgb(201, 103, 101)`}},{types:[`number`],style:{color:`rgb(170, 9, 130)`}},{types:[`punctuation`],style:{color:`rgb(153, 76, 195)`}},{types:[`function`,`selector`,`doctype`],style:{color:`rgb(153, 76, 195)`,fontStyle:`italic`}},{types:[`class-name`],style:{color:`rgb(17, 17, 17)`}},{types:[`tag`],style:{color:`rgb(153, 76, 195)`}},{types:[`operator`,`property`,`keyword`,`namespace`],style:{color:`rgb(12, 150, 155)`}},{types:[`boolean`],style:{color:`rgb(188, 84, 84)`}}]},A_={char:`#D8DEE9`,comment:`#999999`,keyword:`#c5a5c5`,primitive:`#5a9bcf`,string:`#8dc891`,variable:`#d7deea`,boolean:`#ff8b50`,punctuation:`#5FB3B3`,tag:`#fc929e`,function:`#79b6f2`,className:`#FAC863`,method:`#6699CC`,operator:`#fc929e`},j_={plain:{backgroundColor:`#282c34`,color:`#ffffff`},styles:[{types:[`attr-name`],style:{color:A_.keyword}},{types:[`attr-value`],style:{color:A_.string}},{types:[`comment`,`block-comment`,`prolog`,`doctype`,`cdata`,`shebang`],style:{color:A_.comment}},{types:[`property`,`number`,`function-name`,`constant`,`symbol`,`deleted`],style:{color:A_.primitive}},{types:[`boolean`],style:{color:A_.boolean}},{types:[`tag`],style:{color:A_.tag}},{types:[`string`],style:{color:A_.string}},{types:[`punctuation`],style:{color:A_.string}},{types:[`selector`,`char`,`builtin`,`inserted`],style:{color:A_.char}},{types:[`function`],style:{color:A_.function}},{types:[`operator`,`entity`,`url`,`variable`],style:{color:A_.variable}},{types:[`keyword`],style:{color:A_.keyword}},{types:[`atrule`,`class-name`],style:{color:A_.className}},{types:[`important`],style:{fontWeight:`400`}},{types:[`bold`],style:{fontWeight:`bold`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`namespace`],style:{opacity:.7}}]},M_={plain:{color:`#f8f8f2`,backgroundColor:`#272822`},styles:[{types:[`changed`],style:{color:`rgb(162, 191, 252)`,fontStyle:`italic`}},{types:[`deleted`],style:{color:`#f92672`,fontStyle:`italic`}},{types:[`inserted`],style:{color:`rgb(173, 219, 103)`,fontStyle:`italic`}},{types:[`comment`],style:{color:`#8292a2`,fontStyle:`italic`}},{types:[`string`,`url`],style:{color:`#a6e22e`}},{types:[`variable`],style:{color:`#f8f8f2`}},{types:[`number`],style:{color:`#ae81ff`}},{types:[`builtin`,`char`,`constant`,`function`,`class-name`],style:{color:`#e6db74`}},{types:[`punctuation`],style:{color:`#f8f8f2`}},{types:[`selector`,`doctype`],style:{color:`#a6e22e`,fontStyle:`italic`}},{types:[`tag`,`operator`,`keyword`],style:{color:`#66d9ef`}},{types:[`boolean`],style:{color:`#ae81ff`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`,opacity:.7}},{types:[`tag`,`property`],style:{color:`#f92672`}},{types:[`attr-name`],style:{color:`#a6e22e !important`}},{types:[`doctype`],style:{color:`#8292a2`}},{types:[`rule`],style:{color:`#e6db74`}}]},N_={plain:{color:`#bfc7d5`,backgroundColor:`#292d3e`},styles:[{types:[`comment`],style:{color:`rgb(105, 112, 152)`,fontStyle:`italic`}},{types:[`string`,`inserted`],style:{color:`rgb(195, 232, 141)`}},{types:[`number`],style:{color:`rgb(247, 140, 108)`}},{types:[`builtin`,`char`,`constant`,`function`],style:{color:`rgb(130, 170, 255)`}},{types:[`punctuation`,`selector`],style:{color:`rgb(199, 146, 234)`}},{types:[`variable`],style:{color:`rgb(191, 199, 213)`}},{types:[`class-name`,`attr-name`],style:{color:`rgb(255, 203, 107)`}},{types:[`tag`,`deleted`],style:{color:`rgb(255, 85, 114)`}},{types:[`operator`],style:{color:`rgb(137, 221, 255)`}},{types:[`boolean`],style:{color:`rgb(255, 88, 116)`}},{types:[`keyword`],style:{fontStyle:`italic`}},{types:[`doctype`],style:{color:`rgb(199, 146, 234)`,fontStyle:`italic`}},{types:[`namespace`],style:{color:`rgb(178, 204, 214)`}},{types:[`url`],style:{color:`rgb(221, 221, 221)`}}]},P_={plain:{color:`#9EFEFF`,backgroundColor:`#2D2A55`},styles:[{types:[`changed`],style:{color:`rgb(255, 238, 128)`}},{types:[`deleted`],style:{color:`rgba(239, 83, 80, 0.56)`}},{types:[`inserted`],style:{color:`rgb(173, 219, 103)`}},{types:[`comment`],style:{color:`rgb(179, 98, 255)`,fontStyle:`italic`}},{types:[`punctuation`],style:{color:`rgb(255, 255, 255)`}},{types:[`constant`],style:{color:`rgb(255, 98, 140)`}},{types:[`string`,`url`],style:{color:`rgb(165, 255, 144)`}},{types:[`variable`],style:{color:`rgb(255, 238, 128)`}},{types:[`number`,`boolean`],style:{color:`rgb(255, 98, 140)`}},{types:[`attr-name`],style:{color:`rgb(255, 180, 84)`}},{types:[`keyword`,`operator`,`property`,`namespace`,`tag`,`selector`,`doctype`],style:{color:`rgb(255, 157, 0)`}},{types:[`builtin`,`char`,`constant`,`function`,`class-name`],style:{color:`rgb(250, 208, 0)`}}]},F_={plain:{backgroundColor:`linear-gradient(to bottom, #2a2139 75%, #34294f)`,backgroundImage:`#34294f`,color:`#f92aad`,textShadow:`0 0 2px #100c0f, 0 0 5px #dc078e33, 0 0 10px #fff3`},styles:[{types:[`comment`,`block-comment`,`prolog`,`doctype`,`cdata`],style:{color:`#495495`,fontStyle:`italic`}},{types:[`punctuation`],style:{color:`#ccc`}},{types:[`tag`,`attr-name`,`namespace`,`number`,`unit`,`hexcode`,`deleted`],style:{color:`#e2777a`}},{types:[`property`,`selector`],style:{color:`#72f1b8`,textShadow:`0 0 2px #100c0f, 0 0 10px #257c5575, 0 0 35px #21272475`}},{types:[`function-name`],style:{color:`#6196cc`}},{types:[`boolean`,`selector-id`,`function`],style:{color:`#fdfdfd`,textShadow:`0 0 2px #001716, 0 0 3px #03edf975, 0 0 5px #03edf975, 0 0 8px #03edf975`}},{types:[`class-name`,`maybe-class-name`,`builtin`],style:{color:`#fff5f6`,textShadow:`0 0 2px #000, 0 0 10px #fc1f2c75, 0 0 5px #fc1f2c75, 0 0 25px #fc1f2c75`}},{types:[`constant`,`symbol`],style:{color:`#f92aad`,textShadow:`0 0 2px #100c0f, 0 0 5px #dc078e33, 0 0 10px #fff3`}},{types:[`important`,`atrule`,`keyword`,`selector-class`],style:{color:`#f4eee4`,textShadow:`0 0 2px #393a33, 0 0 8px #f39f0575, 0 0 2px #f39f0575`}},{types:[`string`,`char`,`attr-value`,`regex`,`variable`],style:{color:`#f87c32`}},{types:[`parameter`],style:{fontStyle:`italic`}},{types:[`entity`,`url`],style:{color:`#67cdcc`}},{types:[`operator`],style:{color:`ffffffee`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`entity`],style:{cursor:`help`}},{types:[`inserted`],style:{color:`green`}}]},I_={plain:{color:`#282a2e`,backgroundColor:`#ffffff`},styles:[{types:[`comment`],style:{color:`rgb(197, 200, 198)`}},{types:[`string`,`number`,`builtin`,`variable`],style:{color:`rgb(150, 152, 150)`}},{types:[`class-name`,`function`,`tag`,`attr-name`],style:{color:`rgb(40, 42, 46)`}}]},L_={plain:{color:`#9CDCFE`,backgroundColor:`#1E1E1E`},styles:[{types:[`prolog`],style:{color:`rgb(0, 0, 128)`}},{types:[`comment`],style:{color:`rgb(106, 153, 85)`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`rgb(86, 156, 214)`}},{types:[`number`,`inserted`],style:{color:`rgb(181, 206, 168)`}},{types:[`constant`],style:{color:`rgb(100, 102, 149)`}},{types:[`attr-name`,`variable`],style:{color:`rgb(156, 220, 254)`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`rgb(206, 145, 120)`}},{types:[`selector`],style:{color:`rgb(215, 186, 125)`}},{types:[`tag`],style:{color:`rgb(78, 201, 176)`}},{types:[`tag`],languages:[`markup`],style:{color:`rgb(86, 156, 214)`}},{types:[`punctuation`,`operator`],style:{color:`rgb(212, 212, 212)`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`rgb(220, 220, 170)`}},{types:[`class-name`],style:{color:`rgb(78, 201, 176)`}},{types:[`char`],style:{color:`rgb(209, 105, 105)`}}]},R_={plain:{color:`#000000`,backgroundColor:`#ffffff`},styles:[{types:[`comment`],style:{color:`rgb(0, 128, 0)`}},{types:[`builtin`],style:{color:`rgb(0, 112, 193)`}},{types:[`number`,`variable`,`inserted`],style:{color:`rgb(9, 134, 88)`}},{types:[`operator`],style:{color:`rgb(0, 0, 0)`}},{types:[`constant`,`char`],style:{color:`rgb(129, 31, 63)`}},{types:[`tag`],style:{color:`rgb(128, 0, 0)`}},{types:[`attr-name`],style:{color:`rgb(255, 0, 0)`}},{types:[`deleted`,`string`],style:{color:`rgb(163, 21, 21)`}},{types:[`changed`,`punctuation`],style:{color:`rgb(4, 81, 165)`}},{types:[`function`,`keyword`],style:{color:`rgb(0, 0, 255)`}},{types:[`class-name`],style:{color:`rgb(38, 127, 153)`}}]},z_={plain:{color:`#f8fafc`,backgroundColor:`#011627`},styles:[{types:[`prolog`],style:{color:`#000080`}},{types:[`comment`],style:{color:`#6A9955`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`#569CD6`}},{types:[`number`,`inserted`],style:{color:`#B5CEA8`}},{types:[`constant`],style:{color:`#f8fafc`}},{types:[`attr-name`,`variable`],style:{color:`#9CDCFE`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`#cbd5e1`}},{types:[`selector`],style:{color:`#D7BA7D`}},{types:[`tag`],style:{color:`#0ea5e9`}},{types:[`tag`],languages:[`markup`],style:{color:`#0ea5e9`}},{types:[`punctuation`,`operator`],style:{color:`#D4D4D4`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`#7dd3fc`}},{types:[`class-name`],style:{color:`#0ea5e9`}},{types:[`char`],style:{color:`#D16969`}}]},B_={plain:{color:`#0f172a`,backgroundColor:`#f1f5f9`},styles:[{types:[`prolog`],style:{color:`#000080`}},{types:[`comment`],style:{color:`#6A9955`}},{types:[`builtin`,`changed`,`keyword`,`interpolation-punctuation`],style:{color:`#0c4a6e`}},{types:[`number`,`inserted`],style:{color:`#B5CEA8`}},{types:[`constant`],style:{color:`#0f172a`}},{types:[`attr-name`,`variable`],style:{color:`#0c4a6e`}},{types:[`deleted`,`string`,`attr-value`,`template-punctuation`],style:{color:`#64748b`}},{types:[`selector`],style:{color:`#D7BA7D`}},{types:[`tag`],style:{color:`#0ea5e9`}},{types:[`tag`],languages:[`markup`],style:{color:`#0ea5e9`}},{types:[`punctuation`,`operator`],style:{color:`#475569`}},{types:[`punctuation`],languages:[`markup`],style:{color:`#808080`}},{types:[`function`],style:{color:`#0e7490`}},{types:[`class-name`],style:{color:`#0ea5e9`}},{types:[`char`],style:{color:`#D16969`}}]},V_={plain:{backgroundColor:`hsl(220, 13%, 18%)`,color:`hsl(220, 14%, 71%)`,textShadow:`0 1px rgba(0, 0, 0, 0.3)`},styles:[{types:[`comment`,`prolog`,`cdata`],style:{color:`hsl(220, 10%, 40%)`}},{types:[`doctype`,`punctuation`,`entity`],style:{color:`hsl(220, 14%, 71%)`}},{types:[`attr-name`,`class-name`,`maybe-class-name`,`boolean`,`constant`,`number`,`atrule`],style:{color:`hsl(29, 54%, 61%)`}},{types:[`keyword`],style:{color:`hsl(286, 60%, 67%)`}},{types:[`property`,`tag`,`symbol`,`deleted`,`important`],style:{color:`hsl(355, 65%, 65%)`}},{types:[`selector`,`string`,`char`,`builtin`,`inserted`,`regex`,`attr-value`],style:{color:`hsl(95, 38%, 62%)`}},{types:[`variable`,`operator`,`function`],style:{color:`hsl(207, 82%, 66%)`}},{types:[`url`],style:{color:`hsl(187, 47%, 55%)`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`hsl(220, 14%, 71%)`}}]},H_={plain:{backgroundColor:`hsl(230, 1%, 98%)`,color:`hsl(230, 8%, 24%)`},styles:[{types:[`comment`,`prolog`,`cdata`],style:{color:`hsl(230, 4%, 64%)`}},{types:[`doctype`,`punctuation`,`entity`],style:{color:`hsl(230, 8%, 24%)`}},{types:[`attr-name`,`class-name`,`boolean`,`constant`,`number`,`atrule`],style:{color:`hsl(35, 99%, 36%)`}},{types:[`keyword`],style:{color:`hsl(301, 63%, 40%)`}},{types:[`property`,`tag`,`symbol`,`deleted`,`important`],style:{color:`hsl(5, 74%, 59%)`}},{types:[`selector`,`string`,`char`,`builtin`,`inserted`,`regex`,`attr-value`,`punctuation`],style:{color:`hsl(119, 34%, 47%)`}},{types:[`variable`,`operator`,`function`],style:{color:`hsl(221, 87%, 60%)`}},{types:[`url`],style:{color:`hsl(198, 99%, 37%)`}},{types:[`deleted`],style:{textDecorationLine:`line-through`}},{types:[`inserted`],style:{textDecorationLine:`underline`}},{types:[`italic`],style:{fontStyle:`italic`}},{types:[`important`,`bold`],style:{fontWeight:`bold`}},{types:[`important`],style:{color:`hsl(230, 8%, 24%)`}}]},U_={plain:{color:`#ebdbb2`,backgroundColor:`#292828`},styles:[{types:[`imports`,`class-name`,`maybe-class-name`,`constant`,`doctype`,`builtin`,`function`],style:{color:`#d8a657`}},{types:[`property-access`],style:{color:`#7daea3`}},{types:[`tag`],style:{color:`#e78a4e`}},{types:[`attr-name`,`char`,`url`,`regex`],style:{color:`#a9b665`}},{types:[`attr-value`,`string`],style:{color:`#89b482`}},{types:[`comment`,`prolog`,`cdata`,`operator`,`inserted`],style:{color:`#a89984`}},{types:[`delimiter`,`boolean`,`keyword`,`selector`,`important`,`atrule`,`property`,`variable`,`deleted`],style:{color:`#ea6962`}},{types:[`entity`,`number`,`symbol`],style:{color:`#d3869b`}}]},W_={plain:{color:`#654735`,backgroundColor:`#f9f5d7`},styles:[{types:[`delimiter`,`boolean`,`keyword`,`selector`,`important`,`atrule`,`property`,`variable`,`deleted`],style:{color:`#af2528`}},{types:[`imports`,`class-name`,`maybe-class-name`,`constant`,`doctype`,`builtin`],style:{color:`#b4730e`}},{types:[`string`,`attr-value`],style:{color:`#477a5b`}},{types:[`property-access`],style:{color:`#266b79`}},{types:[`function`,`attr-name`,`char`,`url`],style:{color:`#72761e`}},{types:[`tag`],style:{color:`#b94c07`}},{types:[`comment`,`prolog`,`cdata`,`operator`,`inserted`],style:{color:`#a89984`}},{types:[`entity`,`number`,`symbol`],style:{color:`#924f79`}}]},G_=e=>(0,x.useCallback)(t=>{var n=t,{className:r,style:i,line:a}=n;let o=v_(__({},y_(n,[`className`,`style`,`line`])),{className:B(`token-line`,r)});return typeof e==`object`&&`plain`in e&&(o.style=e.plain),typeof i==`object`&&(o.style=__(__({},o.style||{}),i)),o},[e]),K_=e=>{let t=(0,x.useCallback)(({types:t,empty:n})=>{if(e!=null)return t.length===1&&t[0]===`plain`?n==null?void 0:{display:`inline-block`}:t.length===1&&n!=null?e[t[0]]:Object.assign(n==null?{}:{display:`inline-block`},...t.map(t=>e[t]))},[e]);return(0,x.useCallback)(e=>{var n=e,{token:r,className:i,style:a}=n;let o=v_(__({},y_(n,[`token`,`className`,`style`])),{className:B(`token`,...r.types,i),children:r.content,style:t(r)});return a!=null&&(o.style=__(__({},o.style||{}),a)),o},[t])},q_=/\r\n|\r|\n/,J_=e=>{e.length===0?e.push({types:[`plain`],content:`
`,empty:!0}):e.length===1&&e[0].content===``&&(e[0].content=`
`,e[0].empty=!0)},Y_=(e,t)=>{let n=e.length;return n>0&&e[n-1]===t?e:e.concat(t)},X_=e=>{let t=[[]],n=[e],r=[0],i=[e.length],a=0,o=0,s=[],c=[s];for(;o>-1;){for(;(a=r[o]++)<i[o];){let e,l=t[o],u=n[o][a];if(typeof u==`string`?(l=o>0?l:[`plain`],e=u):(l=Y_(l,u.type),u.alias&&(l=Y_(l,u.alias)),e=u.content),typeof e!=`string`){o++,t.push(l),n.push(e),r.push(0),i.push(e.length);continue}let d=e.split(q_),f=d.length;s.push({types:l,content:d[0]});for(let e=1;e<f;e++)J_(s),c.push(s=[]),s.push({types:l,content:d[e]})}o--,t.pop(),n.pop(),r.pop(),i.pop()}return J_(s),c},Z_=({prism:e,code:t,grammar:n,language:r})=>(0,x.useMemo)(()=>{if(n==null)return X_([t]);let i={code:t,grammar:n,language:r,tokens:[]};return e.hooks.run(`before-tokenize`,i),i.tokens=e.tokenize(t,n),e.hooks.run(`after-tokenize`,i),X_(i.tokens)},[t,n,r,e]),Q_=(e,t)=>{let{plain:n}=e,r=e.styles.reduce((e,n)=>{let{languages:r,style:i}=n;return r&&!r.includes(t)||n.types.forEach(t=>{e[t]=__(__({},e[t]),i)}),e},{});return r.root=n,r.plain=v_(__({},n),{backgroundColor:void 0}),r},$_=({children:e,language:t,code:n,theme:r,prism:i})=>{let a=t.toLowerCase(),o=Q_(r,a),s=G_(o),c=K_(o),l=i.languages[a];return e({tokens:Z_({prism:i,language:a,code:n,grammar:l}),className:`prism-code language-${a}`,style:o==null?{}:o.root,getLineProps:s,getTokenProps:c})},ev=e=>(0,x.createElement)($_,v_(__({},e),{prism:e.prism||X,theme:e.theme||L_,code:e.code,language:e.language})),tv=Il((0,R.jsx)(`path`,{d:`M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2m0 16H8V7h11z`}),`ContentCopy`),nv={wrapper:{borderRadius:1,overflow:`hidden`,border:1,borderColor:`divider`,bgcolor:`background.paper`},header:{display:`flex`,alignItems:`center`,justifyContent:`space-between`,px:1.5,py:.5,borderBottom:1,borderColor:`divider`,bgcolor:`rgba(255,255,255,0.02)`},headerNoTabs:{display:`flex`,alignItems:`center`,justifyContent:`flex-end`,px:1.5,py:.5,borderBottom:1,borderColor:`divider`,bgcolor:`rgba(255,255,255,0.02)`},tabs:{display:`flex`,alignItems:`center`,gap:.5},tab:{px:1,py:.25,borderRadius:999,fontSize:12,color:`text.secondary`,cursor:`pointer`},tabActive:{px:1,py:.25,borderRadius:999,fontSize:12,bgcolor:`primary.main`,color:`#0b0f1a`,cursor:`pointer`},copyBtn:{color:`text.secondary`},body:{p:0,bgcolor:`#21252e`},preCustom:{margin:0,padding:10}};const Z=({code:e,codeRuntime:t,codeEditor:n,language:r=`tsx`})=>{let[i,a]=(0,x.useState)(!1),o=(0,x.useCallback)(()=>{let r=(t??n??e??``).trim();navigator.clipboard.writeText(r).then(()=>{a(!0),setTimeout(()=>a(!1),1200)})},[e,t,n]),s=!!(t||n),[c,l]=(0,x.useState)(t?`runtime`:`editor`),u=(()=>s?c===`runtime`?(t??n??``).trim():(n??t??``).trim():(e??``).trim())();return(0,R.jsxs)(q,{sx:nv.wrapper,children:[(0,R.jsxs)(q,{sx:s?nv.header:nv.headerNoTabs,children:[s&&(0,R.jsxs)(q,{sx:nv.tabs,children:[t&&(0,R.jsx)(q,{sx:c===`runtime`?nv.tabActive:nv.tab,onClick:()=>l(`runtime`),children:(0,R.jsx)(K,{variant:`caption`,children:`Runtime`})}),n&&(0,R.jsx)(q,{sx:c===`editor`?nv.tabActive:nv.tab,onClick:()=>l(`editor`),children:(0,R.jsx)(K,{variant:`caption`,children:`Editor`})})]}),(0,R.jsx)($g,{title:i?`Copied`:`Copy`,children:(0,R.jsx)(zd,{size:`small`,onClick:o,sx:nv.copyBtn,children:(0,R.jsx)(tv,{fontSize:`inherit`})})})]}),(0,R.jsx)(q,{sx:nv.body,children:(0,R.jsx)(ev,{theme:C_.oneDark,code:u,language:r,children:({className:e,style:t,tokens:n,getLineProps:r,getTokenProps:i})=>(0,R.jsx)(`pre`,{className:e,style:{...t,...nv.preCustom},children:n.map((e,t)=>(0,R.jsx)(`div`,{...r({line:e}),children:e.map((e,t)=>(0,R.jsx)(`span`,{...i({token:e})},t))},t))})})})]})};var rv={root:{}};const iv=()=>(0,R.jsxs)(q,{sx:rv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Getting Started`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Supported Unity versions: `,(0,R.jsx)(`strong`,{children:`Unity 6.2+`})]}),(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Install via Unity Package Manager`}),(0,R.jsxs)(xg,{children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Open Package Manager in Unity.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Add package from Git URL:`})})]}),(0,R.jsx)(Z,{language:`tsx`,code:`https://github.com/yanivkalfa/ReactiveUIToolKit.git#dist`}),(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Hello World (Editor)`}),(0,R.jsx)(Z,{language:`tsx`,codeEditor:`using UnityEditor;
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
}`})]});var av={root:{display:`flex`,flexDirection:`column`,gap:2},list:{pl:2}};const ov=()=>(0,R.jsxs)(q,{sx:av.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Router`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit includes a lightweight, in-memory router inspired by React Router. It routes based on the current path and lets you nest routes and links inside your `,(0,R.jsx)(`code`,{children:`VirtualNode`}),` `,`tree.`]}),(0,R.jsxs)(q,{children:[(0,R.jsx)(K,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Core concepts`}),(0,R.jsxs)(xg,{sx:av.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Use `,(0,R.jsx)(`code`,{children:`V.Router(...)`}),` at the root of a subtree to set up routing context and history.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Use `,(0,R.jsx)(`code`,{children:`V.Route(path, exact, element, children)`}),` to match the current path and decide what to render.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Use `,(0,R.jsx)(`code`,{children:`V.Link`}),` and `,(0,R.jsx)(`code`,{children:`RouterHooks.UseNavigate(replace)`}),` to perform navigation from code or UI.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Use `,(0,R.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),`, `,(0,R.jsx)(`code`,{children:`RouterHooks.UseParams()`}),`, and`,` `,(0,R.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` to access path, parameters, and query-string values.`]})})})]})]}),(0,R.jsx)(K,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Basic example`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`The example below shows the same router tree hosted in an editor window and in a runtime function component. Inside the matched routes you can use `,(0,R.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),` `,`and `,(0,R.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` to read the active path and parameters.`]}),(0,R.jsx)(Z,{language:`tsx`,codeEditor:`using System.Collections.Generic;
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
// rootRenderer.Render(V.Func(RouterDemoFunc.Example));`}),(0,R.jsx)(K,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Navigation and history`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`By default `,(0,R.jsx)(`code`,{children:`V.Router`}),` uses an in-memory history implementation. You can provide a custom `,(0,R.jsx)(`code`,{children:`IRouterHistory`}),` instance if you want to control how locations are stored or synchronized. Inside components, use `,(0,R.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),` to push or replace locations, and `,(0,R.jsx)(`code`,{children:`RouterHooks.UseGo()`}),` / `,(0,R.jsx)(`code`,{children:`RouterHooks.UseCanGo()`}),` to implement back/forward UI. You can also use `,(0,R.jsx)(`code`,{children:`RouterHooks.UseBlocker()`}),` to prevent navigation while a confirmation dialog is open.`]}),(0,R.jsx)(K,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Links and route data`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Use `,(0,R.jsx)(`code`,{children:`V.Link`}),` to render navigation buttons bound to specific paths. Inside routed components, use `,(0,R.jsx)(`code`,{children:`RouterHooks.UseLocationInfo()`}),` for the full location payload,`,(0,R.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` for path parameters, `,(0,R.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` `,`for query-string values, and `,(0,R.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` for any state object passed when navigating.`]}),(0,R.jsx)(K,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Links, params, query, and state (example)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`The example below demonstrates how to combine `,(0,R.jsx)(`code`,{children:`V.Link`}),`,`,` `,(0,R.jsx)(`code`,{children:`RouterHooks.UseNavigate()`}),`, `,(0,R.jsx)(`code`,{children:`RouterHooks.UseGo()`}),`,`,` `,(0,R.jsx)(`code`,{children:`RouterHooks.UseParams()`}),`, `,(0,R.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),`, and`,` `,(0,R.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` to build a small navigation bar that can move back and forth and read route data.`]}),(0,R.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
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
      V.Label(new LabelProps { Text = $"Query keys: {string.Join(", ", query.Keys)}" }),
      V.Label(new LabelProps { Text = $"Nav state type: {navState?.GetType().Name ?? "(none)"}" })
    );
  }
}`})]});var sv={root:{display:`flex`,flexDirection:`column`,gap:2},list:{pl:2}};const cv=()=>(0,R.jsxs)(q,{sx:sv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Signals`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`Signals`}),` are lightweight, named reactive values that live in a process-wide registry. They behave like a small observable store with a simple API and are ideal whenever you want a single source of truth with a single point of entry for reading and updating state (for example: selection, filters, or global preferences).`]}),(0,R.jsxs)(q,{children:[(0,R.jsx)(K,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Concepts`}),(0,R.jsxs)(xg,{sx:sv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`Signals`}),` live in a global registry keyed by `,(0,R.jsx)(`code`,{children:`string`}),`.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Call `,(0,R.jsx)(`code`,{children:`Signals.Get<T>(key, initialValue)`}),` to create or return a`,` `,(0,R.jsx)(`code`,{children:`Signal<T>`}),` instance.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Call `,(0,R.jsx)(`code`,{children:`signal.Subscribe(...)`}),` to watch changes outside of components; use`,` `,(0,R.jsx)(`code`,{children:`Hooks.UseSignal(...)`}),` inside function components.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Use `,(0,R.jsx)(`code`,{children:`Dispatch(prev => next)`}),` or `,(0,R.jsx)(`code`,{children:`Dispatch(value)`}),` to update the value and notify listeners.`]})})})]})]}),(0,R.jsx)(K,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Runtime usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`using System;
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
}`}),(0,R.jsx)(K,{variant:`h5`,component:`h3`,gutterBottom:!0,children:`Using signals from components`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Inside function components, use `,(0,R.jsx)(`code`,{children:`Hooks.UseSignal`}),` or the selector overload`,` `,(0,R.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(...)`}),` to read a signal and re-render when it changes. The example below shows a simple counter bound to the global `,(0,R.jsx)(`code`,{children:`demo-counter`}),` `,`signal, but you can also project a slice of a more complex signal value and compare with a custom equality comparer for performance.`]}),(0,R.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
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
}`})]});var lv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const uv=()=>(0,R.jsxs)(q,{sx:lv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Concepts & Environment`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit aims to feel familiar if you know React, while still fitting naturally into Unity's UI Toolkit and C# ecosystem. You build trees from `,(0,R.jsx)(`code`,{children:`V.*`}),` helpers and function components, use hooks to manage state, and let the reconciler diff and update the underlying `,(0,R.jsx)(`code`,{children:`VisualElement`}),` hierarchy for you.`]}),(0,R.jsx)(K,{variant:`body1`,paragraph:!0,children:`Where Unity or UI Toolkit impose different constraints (for example: layout system, event model, or platform concerns), the library deliberately diverges from React to provide a more idiomatic Unity experience. The routing, signals, and safe-area helpers are examples of features that don't exist in core React but are important here.`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`The package also ships with a rich demo set under `,(0,R.jsx)(`code`,{children:`Assets/ReactiveUIToolKit/Samples`}),` `,`(editor windows and runtime scenes) that you can import into your project. These demos show real-world usage of components, hooks, routing, signals, and more, and are a great way to see the concepts on this page in action.`]}),(0,R.jsxs)(q,{sx:lv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Scripting define symbols (environment & tracing)`}),(0,R.jsxs)(K,{variant:`body2`,paragraph:!0,children:[`Set these in `,(0,R.jsx)(`strong`,{children:`Project Settings → Player → Scripting Define Symbols`}),`. They control environment labels and diagnostics at compile time.`]}),(0,R.jsxs)(xg,{sx:lv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ENV_DEV`}),` — development environment. Enables dev-oriented defaults such as Basic trace level and compiles editor diagnostics helpers.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ENV_STAGING`}),` — staging environment label (no implicit tracing changes).`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ENV_PROD`}),` — production environment label. This is the implied default if no `,(0,R.jsx)(`code`,{children:`ENV_*`}),` symbol is defined.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RUITK_TRACE_VERBOSE`}),` — force reconciler trace level to`,` `,(0,R.jsx)(`strong`,{children:`Verbose`}),`.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RUITK_TRACE_BASIC`}),` — force reconciler trace level to`,` `,(0,R.jsx)(`strong`,{children:`Basic`}),`.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RUITK_DIFF_TRACING`}),` — force `,(0,R.jsx)(`code`,{children:`Reconciler.EnableDiffTracing`}),` `,`to `,(0,R.jsx)(`code`,{children:`true`}),` for detailed diff diagnostics.`]})})})]}),(0,R.jsx)(K,{variant:`body2`,paragraph:!0,sx:lv.section,children:(0,R.jsx)(`strong`,{children:`Behavior summary`})}),(0,R.jsxs)(xg,{sx:lv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Environment is resolved to `,(0,R.jsx)(`code`,{children:`development`}),`, `,(0,R.jsx)(`code`,{children:`staging`}),`, or`,` `,(0,R.jsx)(`code`,{children:`production`}),` via the `,(0,R.jsx)(`code`,{children:`ENV_*`}),` defines and is exposed at runtime as `,(0,R.jsx)(`code`,{children:`HostContext.Environment["env"]`}),`.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Trace level resolution priority:`,` `,(0,R.jsx)(`code`,{children:`RUITK_TRACE_VERBOSE`}),` > `,(0,R.jsx)(`code`,{children:`RUITK_TRACE_BASIC`}),` >`,` `,(0,R.jsx)(`code`,{children:`ENV_DEV`}),` (Basic) > none.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Editor-only diagnostic utilities are compiled only when ENV_DEV is defined.`})})]})]})]});var dv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const fv=()=>(0,R.jsxs)(q,{sx:dv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Different from React`}),(0,R.jsx)(K,{variant:`body1`,paragraph:!0,children:`ReactiveUIToolKit feels familiar if you know React, but there are important differences in how rendering and scheduling behave when you are working in C# and Unity instead of JavaScript and the browser. This section focuses on the places where your mental model should be adjusted rather than re-explaining core concepts.`}),(0,R.jsxs)(q,{sx:dv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`State updates with UseState (parity)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`Hooks.UseState`}),` matches React's mental model: you get a value and a setter, and you can call the setter with either a value or a function of the previous value (for example `,(0,R.jsx)(`code`,{children:`set(value)`}),` or `,(0,R.jsx)(`code`,{children:`set(prev => next)`}),`).`]}),(0,R.jsxs)(xg,{sx:dv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`The setter is a delegate (`,(0,R.jsx)(`code`,{children:`StateSetter<T>`}),`), not an instance method, but you call it just like a normal function.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`You can either call `,(0,R.jsx)(`code`,{children:`set(value)`}),` / `,(0,R.jsx)(`code`,{children:`set(prev => next)`}),` `,`(React-style) or use the optional extension helpers`,` `,(0,R.jsx)(`code`,{children:`StateSetterExtensions.Set(value)`}),` /`,` `,(0,R.jsx)(`code`,{children:`StateSetterExtensions.Set(prev => next)`}),` if you prefer a fluent style.`]})})})]}),(0,R.jsx)(Z,{language:`tsx`,code:`using System.Collections.Generic;
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
}`})]}),(0,R.jsxs)(q,{sx:dv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Sync rendering vs React concurrent mode`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`ReactiveUIToolKit's Fiber reconciler currently runs in a single, synchronous mode per Unity frame. There is no React 18-style concurrent rendering yet: no`,` `,(0,R.jsx)(`code`,{children:`startTransition`}),`, no transition priorities, and no cooperative time-slicing of large trees.`]}),(0,R.jsxs)(xg,{sx:dv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsx)(R.Fragment,{children:`All updates scheduled in a frame are processed synchronously; there is no partial rendering or preemption between high- and low-priority updates.`})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsx)(R.Fragment,{children:`This behaves like legacy React (pre-18) "sync mode": your components and hooks logic are the same, but you should not expect concurrent features such as transitions or suspenseful background rendering.`})})})]})]})]});var pv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const mv=()=>(0,R.jsxs)(q,{sx:pv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`API Reference`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`This section gives a high-level map of the main namespaces and types you will use when working with ReactiveUIToolKit. Use it as a guide when you are looking for where a particular class (for example `,(0,R.jsx)(`code`,{children:`ButtonProps`}),`) lives.`]}),(0,R.jsxs)(q,{sx:pv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Core`}),(0,R.jsxs)(xg,{sx:pv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Core.V`}),` – static factory for building`,` `,(0,R.jsx)(`code`,{children:`VirtualNode`}),` trees (for example `,(0,R.jsx)(`code`,{children:`V.VisualElement`}),`,`,` `,(0,R.jsx)(`code`,{children:`V.VisualElementSafe`}),`, `,(0,R.jsx)(`code`,{children:`V.Label`}),`, `,(0,R.jsx)(`code`,{children:`V.Button`}),`,`,` `,(0,R.jsx)(`code`,{children:`V.Router`}),`, `,(0,R.jsx)(`code`,{children:`V.TabView`}),`).`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Core.Hooks`}),` – hook functions for function components, such as `,(0,R.jsx)(`code`,{children:`UseState`}),`, `,(0,R.jsx)(`code`,{children:`UseReducer`}),`, `,(0,R.jsx)(`code`,{children:`UseEffect`}),`,`,` `,(0,R.jsx)(`code`,{children:`UseMemo`}),`, `,(0,R.jsx)(`code`,{children:`UseSignal`}),`, and context helpers.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Core.StateSetterExtensions`}),` – helpers for working with state setters (for example `,(0,R.jsx)(`code`,{children:`set.Set(value)`}),` /`,` `,(0,R.jsx)(`code`,{children:`set.Set(prev => next)`}),`).`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Core.RootRenderer`}),` – runtime component that mounts a`,` `,(0,R.jsx)(`code`,{children:`VirtualNode`}),` tree into a `,(0,R.jsx)(`code`,{children:`UIDocument`}),` root.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Core.RenderScheduler`}),` – runtime scheduler used by the reconciler to batch updates per frame.`]})})})]})]}),(0,R.jsxs)(q,{sx:pv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props & Styles`}),(0,R.jsxs)(xg,{sx:pv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Props.Typed`}),` – typed props for UI Toolkit controls. Each control has a corresponding `,(0,R.jsx)(`code`,{children:`*Props`}),` class (for example`,` `,(0,R.jsx)(`code`,{children:`ButtonProps`}),`, `,(0,R.jsx)(`code`,{children:`LabelProps`}),`, `,(0,R.jsx)(`code`,{children:`ListViewProps`}),`,`,` `,(0,R.jsx)(`code`,{children:`ScrollViewProps`}),`).`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Props.Typed.Style`}),` – strongly typed wrapper around a style dictionary used by many props (`,(0,R.jsx)(`code`,{children:`Style`}),` is often passed as`,` `,(0,R.jsx)(`code`,{children:`props.Style`}),`).`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Props.Typed.StyleKeys`}),` – constants used as keys inside`,` `,(0,R.jsx)(`code`,{children:`Style`}),` (for example `,(0,R.jsx)(`code`,{children:`StyleKeys.MarginTop`}),`,`,` `,(0,R.jsx)(`code`,{children:`StyleKeys.FlexDirection`}),`).`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[`Most field and layout controls follow the same pattern:`,(0,R.jsx)(`code`,{children:`V.FloatField(new FloatFieldProps { ... })`}),`,`,` `,(0,R.jsx)(`code`,{children:`V.ListView(new ListViewProps { ... })`}),`, and so on.`]})})})]})]}),(0,R.jsxs)(q,{sx:pv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Router`}),(0,R.jsxs)(xg,{sx:pv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Router.RouterHooks`}),` – hook helpers for routing:`,` `,(0,R.jsx)(`code`,{children:`UseRouter()`}),`, `,(0,R.jsx)(`code`,{children:`UseLocation()`}),`, `,(0,R.jsx)(`code`,{children:`UseLocationInfo()`}),`, `,(0,R.jsx)(`code`,{children:`UseParams()`}),`, `,(0,R.jsx)(`code`,{children:`UseQuery()`}),`,`,` `,(0,R.jsx)(`code`,{children:`UseNavigationState()`}),`, `,(0,R.jsx)(`code`,{children:`UseNavigate()`}),`, `,(0,R.jsx)(`code`,{children:`UseGo()`}),`, `,(0,R.jsx)(`code`,{children:`UseCanGo()`}),`, `,(0,R.jsx)(`code`,{children:`UseBlocker()`}),`.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Router.IRouterHistory`}),`, `,(0,R.jsx)(`code`,{children:`MemoryHistory`}),` – the history abstraction used by `,(0,R.jsx)(`code`,{children:`V.Router`}),`. You can supply your own history implementation by passing an `,(0,R.jsx)(`code`,{children:`IRouterHistory`}),` instance to`,` `,(0,R.jsx)(`code`,{children:`V.Router`}),`.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Router.RouterLocation`}),`, `,(0,R.jsx)(`code`,{children:`RouterPath`}),`,`,` `,(0,R.jsx)(`code`,{children:`RouteMatch`}),` – types that describe the current location, parsed path, and the result of route matching.`]})})})]})]}),(0,R.jsxs)(q,{sx:pv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signals`}),(0,R.jsxs)(xg,{sx:pv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Signals.Signals`}),` – entry point for working with signals via`,` `,(0,R.jsx)(`code`,{children:`Signals.Get<T>(key, initialValue)`}),` and`,` `,(0,R.jsx)(`code`,{children:`Signals.TryGet<T>(key, out signal)`}),`.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Signals.Signal<T>`}),` – concrete signal type with`,` `,(0,R.jsx)(`code`,{children:`Value`}),`, `,(0,R.jsx)(`code`,{children:`Subscribe(...)`}),`, `,(0,R.jsx)(`code`,{children:`Set(value)`}),`, and`,` `,(0,R.jsx)(`code`,{children:`Dispatch(update)`}),` methods.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Signals.SignalsRuntime`}),` – bootstraps the runtime registry and hidden host GameObject. Call `,(0,R.jsx)(`code`,{children:`SignalsRuntime.EnsureInitialized()`}),` at startup if you are using signals outside of components.`]})})})]})]}),(0,R.jsxs)(q,{sx:pv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Editor support`}),(0,R.jsxs)(xg,{sx:pv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.EditorSupport.EditorRootRendererUtility`}),` – helper for mounting a `,(0,R.jsx)(`code`,{children:`VirtualNode`}),` tree into an EditorWindow`,` `,(0,R.jsx)(`code`,{children:`VisualElement`}),`. Used from editor samples and your own tools.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.EditorSupport.EditorRenderScheduler`}),` – scheduler used in the editor for batched updates.`]})})})]})]}),(0,R.jsxs)(q,{sx:pv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Elements & registry`}),(0,R.jsxs)(xg,{sx:pv.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Elements.ElementRegistry`}),` – maps element names (for example`,` `,(0,R.jsx)(`code`,{children:`"Button"`}),`, `,(0,R.jsx)(`code`,{children:`"ListView"`}),`) to concrete adapters and is used by the reconciler when creating and updating UI Toolkit elements.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`ReactiveUITK.Elements.ElementRegistryProvider`}),` – static helpers for obtaining the default registry used by both runtime and editor hosts.`]})})})]})]})]}),hv={AnimateProps:`using System.Collections.Generic;\r
using ReactiveUITK.Core.Animation;\r
using UnityEngine;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class AnimateProps\r
    {\r
        public List<AnimateTrack> Tracks { get; set; }\r
        public bool Autoplay { get; set; } = true;\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,BoundsFieldProps:`using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class BoundsFieldProps\r
    {\r
        public Bounds? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,BoundsIntFieldProps:`using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class BoundsIntFieldProps\r
    {\r
        public BoundsInt? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,BoxProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class BoxProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> ContentContainer { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ButtonProps:`using System;\r
using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ButtonProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public bool? Enabled { get; set; }\r
        public Action OnClick { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ColorFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ColorFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Color? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<Color>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,DoubleFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class DoubleFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public double? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<double>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,DropdownFieldProps:`using System;\r
using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class DropdownFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public List<string> Choices { get; set; }\r
        public string Value { get; set; }\r
        public int? SelectedIndex { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
        public Action<UnityEngine.UIElements.ChangeEvent<string>> OnChange { get; set; }\r
\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
\r
\r
    }\r
}`,EnumFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class EnumFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Enum Value { get; set; }\r
        public string EnumType { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,EnumFlagsFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class EnumFlagsFieldProps\r
    {\r
        public Enum Value { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ErrorBoundaryProps:`using System;\r
using ReactiveUITK.Core;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ErrorBoundaryProps\r
    {\r
        public VirtualNode Fallback { get; set; }\r
        public Action<Exception> OnError { get; set; }\r
        public string ResetKey { get; set; }\r
    }\r
}`,FloatFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class FloatFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public float? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<float>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,FoldoutProps:`using System;\r
using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class FoldoutProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public bool? Value { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
        public Action<UnityEngine.UIElements.ChangeEvent<bool>> OnChange { get; set; }\r
\r
        public Dictionary<string, object> ContentContainer { get; set; }\r
        public Dictionary<string, object> Header { get; set; }\r
\r
\r
    }\r
}`,GroupBoxProps:`using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class GroupBoxProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> ContentContainer { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,Hash128FieldProps:`using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class Hash128FieldProps\r
    {\r
        public Hash128? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,HelpBoxProps:`using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class HelpBoxProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public string MessageType { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ImageProps:`using System.Collections.Generic;\r
using UnityEngine;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ImageProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Texture2D Texture { get; set; }\r
        public Sprite Sprite { get; set; }\r
        public string ScaleMode { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,IMGUIContainerProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class IMGUIContainerProps\r
    {\r
        public Action OnGUI { get; set; }\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,IntegerFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class IntegerFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public int? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<int>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,LabelProps:`using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class LabelProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ListViewProps:`using System;\r
using System.Collections;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ListViewProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public IList Items { get; set; }\r
        public int? SelectedIndex { get; set; }\r
        public float? FixedItemHeight { get; set; }\r
        public System.Func<VisualElement> MakeItem { get; set; }\r
        public System.Action<VisualElement, int> BindItem { get; set; }\r
        public System.Action<VisualElement, int> UnbindItem { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
        public string ViewDataKey { get; set; }\r
\r
        public System.Func<int, object, ReactiveUITK.Core.VirtualNode> Row { get; set; }\r
        public SelectionType? Selection { get; set; }\r
\r
        public Dictionary<string, object> ContentContainer { get; set; }\r
        public Dictionary<string, object> ScrollView { get; set; }\r
\r
\r
    }\r
}`,LongFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class LongFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public long? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<long>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,MinMaxSliderProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class MinMaxSliderProps\r
    {\r
        public float? MinValue { get; set; }\r
        public float? MaxValue { get; set; }\r
        public float? LowLimit { get; set; }\r
        public float? HighLimit { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,MultiColumnListViewProps:`using System;\r
using System.Collections;\r
using System.Collections.Generic;\r
using ReactiveUITK.Core;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class MultiColumnListViewProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public IList Items { get; set; }\r
        public int? SelectedIndex { get; set; }\r
        public float? FixedItemHeight { get; set; }\r
        public SelectionType? Selection { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
        public string ViewDataKey { get; set; }\r
\r
        public List<ColumnDef> Columns { get; set; }\r
\r
        public List<SortedColumnDef> SortedColumns { get; set; }\r
        public object SortingMode { get; set; }\r
        public Delegate ColumnSortingChanged { get; set; }\r
        public Dictionary<string, float> ColumnWidths { get; set; }\r
        public Dictionary<string, bool> ColumnVisibility { get; set; }\r
        public Dictionary<string, int> ColumnDisplayIndex { get; set; }\r
        public Delegate ColumnLayoutChanged { get; set; }\r
\r
        public sealed class ColumnDef\r
        {\r
            public string Name { get; set; }\r
            public string Title { get; set; }\r
            public float? Width { get; set; }\r
            public float? MinWidth { get; set; }\r
            public float? MaxWidth { get; set; }\r
            public bool? Resizable { get; set; }\r
            public bool? Stretchable { get; set; }\r
            public bool? Sortable { get; set; }\r
            public Func<int, object, ReactiveUITK.Core.VirtualNode> Cell { get; set; }\r
\r
\r
        }\r
\r
        public sealed class SortedColumnDef\r
        {\r
            public string Name { get; set; }\r
            public SortDirection? Direction { get; set; }\r
            public int? Index { get; set; }\r
\r
\r
        }\r
\r
        public sealed class ColumnLayoutState\r
        {\r
            public Dictionary<string, float> ColumnWidths { get; set; }\r
            public Dictionary<string, bool> ColumnVisibility { get; set; }\r
            public Dictionary<string, int> ColumnDisplayIndex { get; set; }\r
        }\r
\r
\r
    }\r
}`,MultiColumnTreeViewProps:`using System;\r
using System.Collections;\r
using System.Collections.Generic;\r
using ReactiveUITK.Core;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class MultiColumnTreeViewProps\r
    {\r
        public IList RootItems { get; set; }\r
        public float? FixedItemHeight { get; set; }\r
        public SelectionType? Selection { get; set; }\r
        public int? SelectedIndex { get; set; }\r
        public List<ColumnDef> Columns { get; set; }\r
        public IList<int> ExpandedItemIds { get; set; }\r
        public bool? StopTrackingUserChange { get; set; }\r
        public Dictionary<string, float> ColumnWidths { get; set; }\r
        public Dictionary<string, bool> ColumnVisibility { get; set; }\r
        public Dictionary<string, int> ColumnDisplayIndex { get; set; }\r
        public List<SortedColumnDef> SortedColumns { get; set; }\r
        public object SortingMode { get; set; }\r
        public Delegate ColumnSortingChanged { get; set; }\r
        public Delegate ColumnLayoutChanged { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
        public string ViewDataKey { get; set; }\r
\r
        public sealed class ColumnDef\r
        {\r
            public string Name { get; set; }\r
            public string Title { get; set; }\r
            public float? Width { get; set; }\r
            public float? MinWidth { get; set; }\r
            public float? MaxWidth { get; set; }\r
            public bool? Resizable { get; set; }\r
            public bool? Stretchable { get; set; }\r
            public bool? Sortable { get; set; }\r
            public Func<int, object, ReactiveUITK.Core.VirtualNode> Cell { get; set; }\r
\r
\r
        }\r
\r
        public sealed class SortedColumnDef\r
        {\r
            public string Name { get; set; }\r
            public SortDirection? Direction { get; set; }\r
            public int? Index { get; set; }\r
\r
\r
        }\r
\r
        public sealed class ColumnLayoutState\r
        {\r
            public Dictionary<string, float> ColumnWidths { get; set; }\r
            public Dictionary<string, bool> ColumnVisibility { get; set; }\r
            public Dictionary<string, int> ColumnDisplayIndex { get; set; }\r
        }\r
\r
\r
    }\r
}`,ObjectFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ObjectFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public UnityEngine.Object Value { get; set; }\r
        public string ObjectType { get; set; }\r
        public bool? AllowSceneObjects { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ProgressBarProps:`using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ProgressBarProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public float? Value { get; set; }\r
        public string Title { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Progress { get; set; }\r
        public Dictionary<string, object> TitleElement { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,PropertyInspectorProps:`using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class PropertyFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Object Target { get; set; }\r
        public string BindingPath { get; set; }\r
        public string Label { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
\r
    public sealed class InspectorElementProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Object Target { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,RadioButtonGroupProps:`using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class RadioButtonGroupProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public IList<string> Choices { get; set; }\r
        public string Value { get; set; }\r
        public int? Index { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> ContentContainer { get; set; }\r
        public System.Action<UnityEngine.UIElements.ChangeEvent<int>> OnChange { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,RadioButtonProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class RadioButtonProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public bool? Value { get; set; }\r
        public string Text { get; set; }\r
        public Style Style { get; set; }\r
        public System.Action<ChangeEvent<bool>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,RectFieldProps:`using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class RectFieldProps\r
    {\r
        public Rect? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,RectIntFieldProps:`using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class RectIntFieldProps\r
    {\r
        public RectInt? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,RepeatButtonProps:`using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class RepeatButtonProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public System.Action OnClick { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ScrollerProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ScrollerProps\r
    {\r
        public float? LowValue { get; set; }\r
        public float? HighValue { get; set; }\r
        public float? Value { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ScrollViewProps:`using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ScrollViewProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Mode { get; set; }\r
        public ScrollerVisibility? VerticalScrollerVisibility { get; set; }\r
        public ScrollerVisibility? HorizontalScrollerVisibility { get; set; }\r
        public Vector2? ScrollOffset { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
        public Dictionary<string, object> ContentContainer { get; set; }\r
\r
\r
    }\r
}`,SliderIntProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class SliderIntProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public int? LowValue { get; set; }\r
        public int? HighValue { get; set; }\r
        public int? Value { get; set; }\r
        public string Direction { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
        public Action<ChangeEvent<int>> OnChange { get; set; }\r
\r
\r
    }\r
}`,SliderProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class SliderProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public float? LowValue { get; set; }\r
        public float? HighValue { get; set; }\r
        public float? Value { get; set; }\r
        public string Direction { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
        public Action<ChangeEvent<float>> OnChange { get; set; }\r
\r
\r
    }\r
}`,TabProps:`using System.Collections.Generic;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class TabProps\r
    {\r
        public string Text { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,TabViewProps:`using System;\r
using System.Collections.Generic;\r
using ReactiveUITK.Core;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class TabViewProps\r
    {\r
        public int? SelectedIndex { get; set; }\r
        public int? SelectedTabIndex { get; set; }\r
        public List<TabDef> Tabs { get; set; }\r
        public Style Style { get; set; }\r
        public Delegate SelectedIndexChanged { get; set; }\r
        public Delegate ActiveTabChanged { get; set; }\r
        public object Ref { get; set; }\r
\r
        public sealed class TabDef\r
        {\r
            public string Title { get; set; }\r
            public Func<VirtualNode> Content { get; set; }\r
            public VirtualNode StaticContent { get; set; }\r
\r
\r
        }\r
\r
\r
    }\r
}`,TemplateContainerProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class TemplateContainerProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> ContentContainer { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,TextElementProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class TextElementProps\r
    {\r
        public string Text { get; set; }\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,TextFieldProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class TextFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Value { get; set; }\r
        public bool? Multiline { get; set; }\r
        public bool? Password { get; set; }\r
        public bool? ReadOnly { get; set; }\r
        public int? MaxLength { get; set; }\r
        public string Placeholder { get; set; }\r
        public bool? HidePlaceholderOnFocus { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> Input { get; set; }\r
        public Dictionary<string, object> TextElement { get; set; }\r
\r
        public System.Action<ChangeEvent<string>> OnChange { get; set; }\r
\r
        public string LabelText { get; set; }\r
\r
\r
    }\r
}`,ToggleButtonGroupProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ToggleButtonGroupProps\r
    {\r
        public int? Value { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ToggleProps:`using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ToggleProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public bool? Value { get; set; }\r
        public string Text { get; set; }\r
        public Style Style { get; set; }\r
        public System.Action<ChangeEvent<bool>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> Input { get; set; }\r
        public Dictionary<string, object> Checkmark { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,ToolbarProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class ToolbarProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
\r
    public sealed class ToolbarButtonProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public Action OnClick { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
\r
    public sealed class ToolbarToggleProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public bool? Value { get; set; }\r
        public Action<ChangeEvent<bool>> OnChange { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
\r
    public sealed class ToolbarMenuProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Text { get; set; }\r
        public Action<DropdownMenu> PopulateMenu { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
\r
    public sealed class ToolbarBreadcrumbsProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public IEnumerable<string> Items { get; set; }\r
        public Action<int> OnItem { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
\r
    public sealed class ToolbarPopupSearchFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Value { get; set; }\r
        public Action<ChangeEvent<string>> OnChange { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
\r
    public sealed class ToolbarSearchFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public string Value { get; set; }\r
        public Action<ChangeEvent<string>> OnChange { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
\r
    public sealed class ToolbarSpacerProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,TreeViewProps:`using System;\r
using System.Collections;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class TreeViewProps\r
    {\r
        public IList RootItems { get; set; }\r
        public float? FixedItemHeight { get; set; }\r
        public SelectionType? Selection { get; set; }\r
        public int? SelectedIndex { get; set; }\r
        public System.Func<int, object, ReactiveUITK.Core.VirtualNode> Row { get; set; }\r
        public IList<int> ExpandedItemIds { get; set; }\r
        public bool? StopTrackingUserChange { get; set; }\r
        public Delegate ItemExpandedChanged { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
        public string ViewDataKey { get; set; }\r
\r
\r
    }\r
}`,TwoPaneSplitViewProps:`#if UNITY_EDITOR\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class TwoPaneSplitViewProps\r
    {\r
        public string Orientation { get; set; } // "horizontal" | "vertical"\r
        public int? FixedPaneIndex { get; set; }\r
        public float? FixedPaneInitialDimension { get; set; }\r
        public Style Style { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}\r
#endif`,UnsignedIntegerFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class UnsignedIntegerFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public uint? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<uint>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,UnsignedLongFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class UnsignedLongFieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public ulong? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<ulong>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,Vector2FieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class Vector2FieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Vector2? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<Vector2>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,Vector2IntFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class Vector2IntFieldProps\r
    {\r
        public Vector2Int? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,Vector3FieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class Vector3FieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Vector3? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<Vector3>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,Vector3IntFieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class Vector3IntFieldProps\r
    {\r
        public Vector3Int? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`,Vector4FieldProps:`using System;\r
using System.Collections.Generic;\r
using UnityEngine;\r
using UnityEngine.UIElements;\r
\r
namespace ReactiveUITK.Props.Typed\r
{\r
    public sealed class Vector4FieldProps\r
    {\r
        public string Name { get; set; }\r
        public string ClassName { get; set; }\r
        public Vector4? Value { get; set; }\r
        public Style Style { get; set; }\r
        public Action<ChangeEvent<Vector4>> OnChange { get; set; }\r
        public Dictionary<string, object> Label { get; set; }\r
        public Dictionary<string, object> VisualInput { get; set; }\r
        public object Ref { get; set; }\r
\r
\r
    }\r
}`},Q=e=>hv[e]??``;var gv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const _v={BoundsField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-BoundsField.html`},BoundsIntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-BoundsIntField.html`},Box:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Box.html`},Button:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Button.html`},ColorField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ColorField.html`},DoubleField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-DoubleField.html`},DropdownField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-DropdownField.html`},EnumField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-EnumField.html`},EnumFlagsField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-EnumFlagsField.html`},FloatField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-FloatField.html`},Foldout:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Foldout.html`},GroupBox:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-GroupBox.html`},Hash128Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Hash128Field.html`},HelpBox:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-HelpBox.html`},IMGUIContainer:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-IMGUIContainer.html`},Image:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Image.html`},IntegerField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-IntegerField.html`},Label:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Label.html`},ListView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ListView.html`},LongField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-LongField.html`},MinMaxSlider:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MinMaxSlider.html`},MultiColumnListView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MultiColumnListView.html`},MultiColumnTreeView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-MultiColumnTreeView.html`},ObjectField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ObjectField.html`},ProgressBar:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ProgressBar.html`},PropertyInspector:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-InspectorElement.html`,label:`InspectorElement entry`,note:`ReactiveUITK.PropertyInspector wraps Unity’s InspectorElement to embed serialized-object inspectors.`},RadioButton:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RadioButton.html`},RadioButtonGroup:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RadioButtonGroup.html`},RectField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RectField.html`},RectIntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RectIntField.html`},RepeatButton:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-RepeatButton.html`},ScrollView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ScrollView.html`},Scroller:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Scroller.html`},Slider:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Slider.html`},SliderInt:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-SliderInt.html`},Tab:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Tab.html`},TabView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TabView.html`},TemplateContainer:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TemplateContainer.html`},TextElement:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TextElement.html`},TextField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TextField.html`},Toggle:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Toggle.html`},ToggleButtonGroup:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ToggleButtonGroup.html`},Toolbar:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Toolbar.html`},TreeView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TreeView.html`},TwoPaneSplitView:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-TwoPaneSplitView.html`},UnsignedIntegerField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-UnsignedIntegerField.html`},UnsignedLongField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-UnsignedLongField.html`},Vector2Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector2Field.html`},Vector2IntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector2IntField.html`},Vector3Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector3Field.html`},Vector3IntField:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector3IntField.html`},Vector4Field:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-Vector4Field.html`},VisualElement:{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-VisualElement.html`}},$=({componentName:e})=>{let t=_v[e];if(!t)return null;let n=t.label??`${e} entry`;return(0,R.jsxs)(q,{sx:{mt:2},children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Unity docs`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Review the`,` `,(0,R.jsx)(gg,{href:t.href,target:`_blank`,rel:`noreferrer`,children:n}),` `,`in the Unity manual for the official UI Toolkit reference.`]}),t.note&&(0,R.jsx)(K,{variant:`body2`,color:`text.secondary`,paragraph:!0,children:t.note})]})},vv=()=>(0,R.jsxs)(q,{sx:gv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`BoundsField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.BoundsField`}),` wraps the Unity `,(0,R.jsx)(`code`,{children:`BoundsField`}),` control using`,` `,(0,R.jsx)(`code`,{children:`BoundsFieldProps`}),`. It is useful for editing `,(0,R.jsx)(`code`,{children:`Bounds`}),` values in both runtime UI and editor tools.`]}),(0,R.jsxs)(q,{sx:gv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`BoundsFieldProps`)})]}),(0,R.jsxs)(q,{sx:gv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,R.jsx)(`code`,{children:`BoundsFieldProps`}),` instance to `,(0,R.jsx)(`code`,{children:`V.BoundsField`}),`. The`,` `,(0,R.jsx)(`code`,{children:`Value`}),` property controls the current bounds.`]}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:gv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,R.jsxs)(K,{variant:`body1`,children:[(0,R.jsx)(`code`,{children:`BoundsField`}),` does not accept child nodes; all configuration is done through`,` `,(0,R.jsx)(`code`,{children:`BoundsFieldProps`}),`.`]})]}),(0,R.jsxs)(q,{sx:gv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,R.jsx)(`code`,{children:`Label`}),` and `,(0,R.jsx)(`code`,{children:`VisualInput`}),` properties to style the label and the internal input container. Both expect dictionaries – you can compose them using other typed props (for example `,(0,R.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),`) or by building a`,` `,(0,R.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,R.jsxs)(q,{sx:gv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Controlled value`}),(0,R.jsxs)(K,{variant:`body1`,children:[`Use `,(0,R.jsx)(`code`,{children:`Hooks.UseState`}),` (or a signal) to hold the current `,(0,R.jsx)(`code`,{children:`Bounds`}),` and update it from a change handler. The example above uses a local state tuple and updates the value via `,(0,R.jsx)(`code`,{children:`setBounds(evt.newValue)`}),` (you can also use the optional`,` `,(0,R.jsx)(`code`,{children:`StateSetterExtensions.Set`}),` helper if you prefer method syntax).`]})]}),(0,R.jsx)($,{componentName:`BoundsField`})]});var yv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const bv=()=>(0,R.jsxs)(q,{sx:yv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`BoundsIntField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.BoundsIntField`}),` wraps the Unity `,(0,R.jsx)(`code`,{children:`BoundsIntField`}),` control using`,` `,(0,R.jsx)(`code`,{children:`BoundsIntFieldProps`}),` for working with integer bounds in both runtime UI and editor tools.`]}),(0,R.jsxs)(q,{sx:yv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`BoundsIntFieldProps`)})]}),(0,R.jsxs)(q,{sx:yv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,R.jsx)(`code`,{children:`BoundsIntFieldProps`}),` with an initial `,(0,R.jsx)(`code`,{children:`BoundsInt`}),` to render the field. Combine it with `,(0,R.jsx)(`code`,{children:`Hooks.UseState`}),` or signals to keep the value controlled.`]}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:yv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,R.jsxs)(K,{variant:`body1`,children:[(0,R.jsx)(`code`,{children:`BoundsIntField`}),` does not support child nodes. Use the label slot to add context.`]})]}),(0,R.jsxs)(q,{sx:yv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,R.jsx)(`code`,{children:`Label`}),` and `,(0,R.jsx)(`code`,{children:`VisualInput`}),` properties on`,` `,(0,R.jsx)(`code`,{children:`BoundsIntFieldProps`}),` to configure the label and the internal input container. Both expect dictionaries; for example, you can build a label with`,` `,(0,R.jsx)(`code`,{children:`new LabelProps { Text = "BoundsInt" }.ToDictionary()`}),` or provide a`,(0,R.jsx)(`code`,{children:`VisualInput`}),` dictionary that contains a nested `,(0,R.jsx)(`code`,{children:`Style`}),`.`]})]}),(0,R.jsx)($,{componentName:`BoundsIntField`})]});var xv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Sv=()=>(0,R.jsxs)(q,{sx:xv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Box`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Box`}),` renders a boxed container element with optional content. It is useful for grouping related controls with a background and padding.`]}),(0,R.jsxs)(q,{sx:xv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`BoxProps`)})]}),(0,R.jsxs)(q,{sx:xv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Pass a `,(0,R.jsx)(`code`,{children:`BoxProps`}),` instance to `,(0,R.jsx)(`code`,{children:`V.Box`}),` and supply children as additional arguments.`]}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:xv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,R.jsx)(K,{variant:`body1`,children:`Children are rendered inside the box's content container. Use this to create sections of your UI that share common styling.`})]}),(0,R.jsxs)(q,{sx:xv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (contentContainer)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Use the `,(0,R.jsx)(`code`,{children:`ContentContainer`}),` property on `,(0,R.jsx)(`code`,{children:`BoxProps`}),` to style or configure the box's `,(0,R.jsx)(`code`,{children:`contentContainer`}),`. This property expects a dictionary, allowing you to pass a nested `,(0,R.jsx)(`code`,{children:`Style`}),` or additional props that should be applied to the content container element.`]})]}),(0,R.jsx)($,{componentName:`Box`})]});var Cv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const wv=()=>(0,R.jsxs)(q,{sx:Cv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Button`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Button`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Button`}),` element with`,` `,(0,R.jsx)(`code`,{children:`ButtonProps`}),`. Use it for clickable actions.`]}),(0,R.jsxs)(q,{sx:Cv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ButtonProps`)})]}),(0,R.jsxs)(q,{sx:Cv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,R.jsx)(`code`,{children:`Text`}),`, optional `,(0,R.jsx)(`code`,{children:`Style`}),`, and an `,(0,R.jsx)(`code`,{children:`OnClick`}),` handler in `,(0,R.jsx)(`code`,{children:`ButtonProps`}),`. Combine with `,(0,R.jsx)(`code`,{children:`Hooks.UseState`}),` to build controlled buttons.`]}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Button`})]});var Tv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ev=()=>(0,R.jsxs)(q,{sx:Tv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ColorField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.ColorField`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`ColorField`}),` element using`,` `,(0,R.jsx)(`code`,{children:`ColorFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Tv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ColorFieldProps`)})]}),(0,R.jsxs)(q,{sx:Tv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:Tv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Use `,(0,R.jsx)(`code`,{children:`ColorFieldProps.Label`}),` to configure the label element, and`,` `,(0,R.jsx)(`code`,{children:`ColorFieldProps.VisualInput`}),` to style the input container (for example, padding or background). Both properties accept dictionaries; in most cases you construct them from other typed props or by nesting a `,(0,R.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,R.jsx)($,{componentName:`ColorField`})]});var Dv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ov=()=>(0,R.jsxs)(q,{sx:Dv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`DoubleField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.DoubleField`}),` exposes a double-precision numeric field via`,` `,(0,R.jsx)(`code`,{children:`DoubleFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Dv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`DoubleFieldProps`)})]}),(0,R.jsxs)(q,{sx:Dv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:Dv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`DoubleFieldProps.Label`}),` and `,(0,R.jsx)(`code`,{children:`DoubleFieldProps.VisualInput`}),` follow the same pattern as other numeric fields. Use a label dictionary (often built from`,` `,(0,R.jsx)(`code`,{children:`LabelProps`}),`) and a visual input dictionary that can contain a nested`,` `,(0,R.jsx)(`code`,{children:`Style`}),` for the inner input container.`]})]}),(0,R.jsx)($,{componentName:`DoubleField`})]});var kv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Av=()=>(0,R.jsxs)(q,{sx:kv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`DropdownField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.DropdownField`}),` renders a text-based dropdown using `,(0,R.jsx)(`code`,{children:`DropdownFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:kv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`DropdownFieldProps`)})]}),(0,R.jsxs)(q,{sx:kv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:kv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`DropdownFieldProps.Label`}),` and `,(0,R.jsx)(`code`,{children:`DropdownFieldProps.VisualInput`}),` mirror the slots on the underlying UI Toolkit control. Use `,(0,R.jsx)(`code`,{children:`Label`}),` to configure the label element, and `,(0,R.jsx)(`code`,{children:`VisualInput`}),` to style the internal input area via a dictionary that can contain a nested `,(0,R.jsx)(`code`,{children:`Style`}),`.`]})]}),(0,R.jsx)($,{componentName:`DropdownField`})]});var jv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Mv=()=>(0,R.jsxs)(q,{sx:jv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`EnumField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.EnumField`}),` binds to any enum type via `,(0,R.jsx)(`code`,{children:`EnumFieldProps`}),`. Provide the enum's assembly-qualified type name and an initial `,(0,R.jsx)(`code`,{children:`Value`}),`.`]}),(0,R.jsxs)(q,{sx:jv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`EnumFieldProps`)})]}),(0,R.jsxs)(q,{sx:jv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:jv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`EnumFieldProps.Label`}),` and `,(0,R.jsx)(`code`,{children:`EnumFieldProps.VisualInput`}),` configure the label and input slots respectively. As with other fields, both expect dictionaries; label dictionaries are often created from `,(0,R.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),`, while visual input dictionaries typically wrap a `,(0,R.jsx)(`code`,{children:`Style`}),` instance.`]})]}),(0,R.jsx)($,{componentName:`EnumField`})]});var Nv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Pv=()=>(0,R.jsxs)(q,{sx:Nv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`EnumFlagsField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.EnumFlagsField`}),` is similar to `,(0,R.jsx)(`code`,{children:`V.EnumField`}),` but supports`,` `,(0,R.jsx)(`code`,{children:`[Flags]`}),` enums.`]}),(0,R.jsxs)(q,{sx:Nv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`EnumFlagsFieldProps`)})]}),(0,R.jsxs)(q,{sx:Nv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:Nv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`EnumFlagsFieldProps.Label`}),` and `,(0,R.jsx)(`code`,{children:`EnumFlagsFieldProps.VisualInput`}),`behave the same as on `,(0,R.jsx)(`code`,{children:`EnumFieldProps`}),`, allowing you to style the label element and the embedded input area via dictionaries that can contain nested `,(0,R.jsx)(`code`,{children:`Style`}),` `,`objects.`]})]}),(0,R.jsx)($,{componentName:`EnumFlagsField`})]});var Fv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Iv=()=>(0,R.jsxs)(q,{sx:Fv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`FloatField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.FloatField`}),` represents a single-precision numeric field, backed by`,` `,(0,R.jsx)(`code`,{children:`FloatFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Fv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`FloatFieldProps`)})]}),(0,R.jsxs)(q,{sx:Fv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:Fv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / visual input)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`FloatFieldProps.Label`}),` and `,(0,R.jsx)(`code`,{children:`FloatFieldProps.VisualInput`}),` let you customize the label element and the inner input container. Both accept dictionaries: build a label via `,(0,R.jsx)(`code`,{children:`LabelProps.ToDictionary()`}),` and pass a dictionary with a nested`,` `,(0,R.jsx)(`code`,{children:`Style`}),` object to `,(0,R.jsx)(`code`,{children:`VisualInput`}),` to style the input.`]})]}),(0,R.jsx)($,{componentName:`FloatField`})]});var Lv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Rv=()=>(0,R.jsxs)(q,{sx:Lv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Foldout`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Foldout`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Foldout`}),` element using`,` `,(0,R.jsx)(`code`,{children:`FoldoutProps`}),`. It is useful for expandable sections of UI that reveal more content when open.`]}),(0,R.jsxs)(q,{sx:Lv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`FoldoutProps`)})]}),(0,R.jsxs)(q,{sx:Lv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,R.jsx)(`code`,{children:`Text`}),`, an optional initial `,(0,R.jsx)(`code`,{children:`Value`}),`, and an`,` `,(0,R.jsx)(`code`,{children:`OnChange`}),` handler. The example below also shows children rendered inside the foldout when it is expanded.`]}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:Lv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Children passed to `,(0,R.jsx)(`code`,{children:`V.Foldout`}),` are rendered inside the foldout's content area and are shown or hidden based on the current `,(0,R.jsx)(`code`,{children:`Value`}),`.`]})]}),(0,R.jsxs)(q,{sx:Lv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (header / contentContainer)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Use `,(0,R.jsx)(`code`,{children:`FoldoutProps.Header`}),` and `,(0,R.jsx)(`code`,{children:`FoldoutProps.ContentContainer`}),` to style the header bar and inner content container. Both accept dictionaries; commonly a nested`,` `,(0,R.jsx)(`code`,{children:`Style`}),` is provided under the `,(0,R.jsx)(`code`,{children:`"style"`}),` key.`]})]}),(0,R.jsxs)(q,{sx:Lv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Controlled value`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`For controlled foldouts, track a `,(0,R.jsx)(`code`,{children:`bool`}),` with `,(0,R.jsx)(`code`,{children:`Hooks.UseState`}),` (or a signal) and update it in `,(0,R.jsx)(`code`,{children:`OnChange`}),`. The `,(0,R.jsx)(`code`,{children:`Value`}),` property will then always reflect your source of truth.`]})]}),(0,R.jsx)($,{componentName:`Foldout`})]});var zv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Bv=()=>(0,R.jsxs)(q,{sx:zv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`GroupBox`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.GroupBox`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`GroupBox`}),` element using`,` `,(0,R.jsx)(`code`,{children:`GroupBoxProps`}),`. It is useful for grouping related controls under a titled header.`]}),(0,R.jsxs)(q,{sx:zv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`GroupBoxProps`)})]}),(0,R.jsxs)(q,{sx:zv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Provide `,(0,R.jsx)(`code`,{children:`Text`}),` for the group title, a `,(0,R.jsx)(`code`,{children:`Style`}),` for layout, and add children that will appear inside the group.`]}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:zv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Children`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Children passed to `,(0,R.jsx)(`code`,{children:`V.GroupBox`}),` are rendered inside the group's content container, below the labeled header.`]})]}),(0,R.jsxs)(q,{sx:zv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Slots (label / contentContainer)`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Use `,(0,R.jsx)(`code`,{children:`GroupBoxProps.Label`}),` and `,(0,R.jsx)(`code`,{children:`GroupBoxProps.ContentContainer`}),` to style the header label and the inner content container. Both properties accept dictionaries, often containing nested `,(0,R.jsx)(`code`,{children:`Style`}),` objects.`]})]}),(0,R.jsx)($,{componentName:`GroupBox`})]});var Vv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Hv=()=>(0,R.jsxs)(q,{sx:Vv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Hash128Field`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Hash128Field`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Hash128Field`}),` for editing`,` `,(0,R.jsx)(`code`,{children:`Hash128`}),` values.`]}),(0,R.jsxs)(q,{sx:Vv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`Hash128FieldProps`)})]}),(0,R.jsxs)(q,{sx:Vv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Hash128Field`})]});var Uv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Wv=()=>(0,R.jsxs)(q,{sx:Uv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`HelpBox`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.HelpBox`}),` wraps the standard UI Toolkit `,(0,R.jsx)(`code`,{children:`HelpBox`}),` for displaying informational, warning, or error messages.`]}),(0,R.jsxs)(q,{sx:Uv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`HelpBoxProps`)})]}),(0,R.jsxs)(q,{sx:Uv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`HelpBox`})]});var Gv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Kv=()=>(0,R.jsxs)(q,{sx:Gv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`IMGUIContainer`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.IMGUIContainer`}),` lets you embed IMGUI content inside a UI Toolkit layout by providing an `,(0,R.jsx)(`code`,{children:`OnGUI`}),` callback in `,(0,R.jsx)(`code`,{children:`IMGUIContainerProps`}),`. This is primarily an editor-only pattern.`]}),(0,R.jsxs)(q,{sx:Gv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`IMGUIContainerProps`)})]}),(0,R.jsxs)(q,{sx:Gv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

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
}`})]}),(0,R.jsx)($,{componentName:`IMGUIContainer`})]});var qv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Jv=()=>(0,R.jsxs)(q,{sx:qv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Image`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Image`}),` renders a UI Toolkit `,(0,R.jsx)(`code`,{children:`Image`}),` using `,(0,R.jsx)(`code`,{children:`ImageProps`}),`. It supports both `,(0,R.jsx)(`code`,{children:`Texture2D`}),` and `,(0,R.jsx)(`code`,{children:`Sprite`}),` sources.`]}),(0,R.jsxs)(q,{sx:qv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ImageProps`)})]}),(0,R.jsxs)(q,{sx:qv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Image`})]});var Yv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Xv=()=>(0,R.jsxs)(q,{sx:Yv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`IntegerField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.IntegerField`}),` represents an integer numeric field using `,(0,R.jsx)(`code`,{children:`IntegerFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Yv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`IntegerFieldProps`)})]}),(0,R.jsxs)(q,{sx:Yv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`IntegerField`})]});var Zv={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Qv=()=>(0,R.jsxs)(q,{sx:Zv.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Label`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Label`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Label`}),` element via `,(0,R.jsx)(`code`,{children:`LabelProps`}),`. It is the primary way to render text in your component trees.`]}),(0,R.jsxs)(q,{sx:Zv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`LabelProps`)})]}),(0,R.jsxs)(q,{sx:Zv.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Label`})]});var $v={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ey=()=>(0,R.jsxs)(q,{sx:$v.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`LongField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.LongField`}),` represents a 64-bit integer field using `,(0,R.jsx)(`code`,{children:`LongFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:$v.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`LongFieldProps`)})]}),(0,R.jsxs)(q,{sx:$v.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`LongField`})]});var ty={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ny=()=>(0,R.jsxs)(q,{sx:ty.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ProgressBar`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.ProgressBar`}),` renders a UI Toolkit `,(0,R.jsx)(`code`,{children:`ProgressBar`}),` using`,` `,(0,R.jsx)(`code`,{children:`ProgressBarProps`}),`. It is typically driven by state changes elsewhere in your UI.`]}),(0,R.jsxs)(q,{sx:ty.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ProgressBarProps`)})]}),(0,R.jsxs)(q,{sx:ty.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsxs)(q,{sx:ty.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Styling track and fill`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`The`,` `,(0,R.jsx)(`a`,{href:`https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ProgressBar.html`,target:`_blank`,rel:`noreferrer`,children:`Unity ProgressBar documentation`}),` `,`highlights that the root element is the visible track, while the inner`,` `,(0,R.jsx)(`code`,{children:`.unity-progress-bar__progress`}),` child renders the filled portion.`]}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Assign styles to the track via `,(0,R.jsx)(`code`,{children:`ProgressBarProps.Style`}),` (for border, unfilled background, size, etc.) and target the fill through the `,(0,R.jsx)(`code`,{children:`Progress`}),` slot. You can also style the caption by populating `,(0,R.jsx)(`code`,{children:`TitleElement`}),`. The example above uses this pattern to create a progress bar with a dark green track, a lighter fill, and centered text.`]})]}),(0,R.jsx)($,{componentName:`ProgressBar`})]});var ry={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const iy=()=>(0,R.jsxs)(q,{sx:ry.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ListView`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.ListView`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`ListView`}),` control using`,` `,(0,R.jsx)(`code`,{children:`ListViewProps`}),`. It can use either the standard `,(0,R.jsx)(`code`,{children:`makeItem/bindItem`}),` `,`properties or the higher-level `,(0,R.jsx)(`code`,{children:`Row`}),` function that returns a `,(0,R.jsx)(`code`,{children:`VirtualNode`}),`.`]}),(0,R.jsxs)(q,{sx:ry.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ListViewProps`)})]}),(0,R.jsxs)(q,{sx:ry.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`ListView`})]});var ay={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const oy=()=>(0,R.jsxs)(q,{sx:ay.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MinMaxSlider`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.MinMaxSlider`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`MinMaxSlider`}),` element using`,` `,(0,R.jsx)(`code`,{children:`MinMaxSliderProps`}),` for selecting a range between two limits.`]}),(0,R.jsxs)(q,{sx:ay.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`MinMaxSliderProps`)})]}),(0,R.jsxs)(q,{sx:ay.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`MinMaxSlider`})]});var sy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const cy=()=>(0,R.jsxs)(q,{sx:sy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ObjectField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.ObjectField`}),` wraps the editor-only UI Toolkit `,(0,R.jsx)(`code`,{children:`ObjectField`}),` element using `,(0,R.jsx)(`code`,{children:`ObjectFieldProps`}),`. It is typically used in custom inspectors and tools.`]}),(0,R.jsxs)(q,{sx:sy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ObjectFieldProps`)})]}),(0,R.jsxs)(q,{sx:sy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

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
}`})]}),(0,R.jsx)($,{componentName:`ObjectField`})]});var ly={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const uy=()=>(0,R.jsxs)(q,{sx:ly.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RadioButton`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.RadioButton`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`RadioButton`}),` element using`,` `,(0,R.jsx)(`code`,{children:`RadioButtonProps`}),`. It is usually used within a `,(0,R.jsx)(`code`,{children:`RadioButtonGroup`}),`.`]}),(0,R.jsxs)(q,{sx:ly.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`RadioButtonProps`)})]}),(0,R.jsxs)(q,{sx:ly.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`RadioButton`})]});var dy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const fy=()=>(0,R.jsxs)(q,{sx:dy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RadioButtonGroup`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.RadioButtonGroup`}),` wraps UI Toolkit's `,(0,R.jsx)(`code`,{children:`RadioButtonGroup`}),` using`,` `,(0,R.jsx)(`code`,{children:`RadioButtonGroupProps`}),`. It manages a set of mutually exclusive choices.`]}),(0,R.jsxs)(q,{sx:dy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`RadioButtonGroupProps`)})]}),(0,R.jsxs)(q,{sx:dy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`RadioButtonGroup`})]});var py={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const my=()=>(0,R.jsxs)(q,{sx:py.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RepeatButton`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.RepeatButton`}),` wraps UI Toolkit's `,(0,R.jsx)(`code`,{children:`RepeatButton`}),`, invoking`,` `,(0,R.jsx)(`code`,{children:`OnClick`}),` repeatedly while the button is held.`]}),(0,R.jsxs)(q,{sx:py.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`RepeatButtonProps`)})]}),(0,R.jsxs)(q,{sx:py.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`RepeatButton`})]});var hy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const gy=()=>(0,R.jsxs)(q,{sx:hy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ScrollView`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.ScrollView`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`ScrollView`}),` element using`,` `,(0,R.jsx)(`code`,{children:`ScrollViewProps`}),`. It is the primary way to add scrolling regions to your layouts.`]}),(0,R.jsxs)(q,{sx:hy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ScrollViewProps`)})]}),(0,R.jsxs)(q,{sx:hy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`ScrollView`})]});var _y={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const vy=()=>(0,R.jsxs)(q,{sx:_y.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Slider`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Slider`}),` renders a float slider using `,(0,R.jsx)(`code`,{children:`SliderProps`}),`.`]}),(0,R.jsxs)(q,{sx:_y.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`SliderProps`)})]}),(0,R.jsxs)(q,{sx:_y.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
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

    return V.Slider(
      new SliderProps
      {
        LowValue = 0f,
        HighValue = 1f,
        Value = value,
        Direction = "Horizontal",
      }
    );
  }
}`})]}),(0,R.jsx)($,{componentName:`Slider`})]});var yy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const by=()=>(0,R.jsxs)(q,{sx:yy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`SliderInt`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.SliderInt`}),` renders an integer slider using `,(0,R.jsx)(`code`,{children:`SliderIntProps`}),`.`]}),(0,R.jsxs)(q,{sx:yy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`SliderIntProps`)})]}),(0,R.jsxs)(q,{sx:yy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`SliderInt`})]});var xy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Sy=()=>(0,R.jsxs)(q,{sx:xy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Toggle`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Toggle`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Toggle`}),` control using `,(0,R.jsx)(`code`,{children:`ToggleProps`}),`.`]}),(0,R.jsxs)(q,{sx:xy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ToggleProps`)})]}),(0,R.jsxs)(q,{sx:xy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Toggle`})]});var Cy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const wy=()=>(0,R.jsxs)(q,{sx:Cy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TreeView`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.TreeView`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`TreeView`}),` control using`,` `,(0,R.jsx)(`code`,{children:`TreeViewProps`}),`, allowing you to render hierarchical data with a`,` `,(0,R.jsx)(`code`,{children:`Row`}),` function that returns `,(0,R.jsx)(`code`,{children:`VirtualNode`}),` instances.`]}),(0,R.jsxs)(q,{sx:Cy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`TreeViewProps`)})]}),(0,R.jsxs)(q,{sx:Cy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`TreeView`})]});var Ty={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ey=()=>(0,R.jsxs)(q,{sx:Ty.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Tab`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Tab`}),` renders an individual tab using `,(0,R.jsx)(`code`,{children:`TabProps`}),`. In most cases you will use it indirectly via `,(0,R.jsx)(`code`,{children:`TabView`}),`, but you can also construct tab strips manually.`]}),(0,R.jsxs)(q,{sx:Ty.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`TabProps`)})]}),(0,R.jsxs)(q,{sx:Ty.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Tab`})]});var Dy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Oy=()=>(0,R.jsxs)(q,{sx:Dy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TabView`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.TabView`}),` renders a tab strip and tab content using `,(0,R.jsx)(`code`,{children:`TabViewProps`}),`. Each tab is defined by a `,(0,R.jsx)(`code`,{children:`TabViewProps.TabDef`}),`, which can provide either static content or a factory function.`]}),(0,R.jsxs)(q,{sx:Dy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`TabViewProps`)})]}),(0,R.jsxs)(q,{sx:Dy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`TabView`})]});var ky={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ay=()=>(0,R.jsxs)(q,{sx:ky.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ToggleButtonGroup`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.ToggleButtonGroup`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`ToggleButtonGroup`}),` element using `,(0,R.jsx)(`code`,{children:`ToggleButtonGroupProps`}),` and child buttons as options.`]}),(0,R.jsxs)(q,{sx:ky.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ToggleButtonGroupProps`)})]}),(0,R.jsxs)(q,{sx:ky.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class ToggleButtonGroupExamples
{
  // Function component – pass ToggleButtonGroupExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(1);

    return V.ToggleButtonGroup(
      new ToggleButtonGroupProps { Value = value },
      key: null,
      V.Button(new ButtonProps { Text = "One" }),
      V.Button(new ButtonProps { Text = "Two" }),
      V.Button(new ButtonProps { Text = "Three" })
    );
  }
}`})]}),(0,R.jsx)($,{componentName:`ToggleButtonGroup`})]});var jy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const My=()=>(0,R.jsxs)(q,{sx:jy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TextField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.TextField`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`TextField`}),` using`,` `,(0,R.jsx)(`code`,{children:`TextFieldProps`}),`, with support for slots like `,(0,R.jsx)(`code`,{children:`Label`}),`,`,` `,(0,R.jsx)(`code`,{children:`Input`}),`, and `,(0,R.jsx)(`code`,{children:`TextElement`}),`.`]}),(0,R.jsxs)(q,{sx:jy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`TextFieldProps`)})]}),(0,R.jsxs)(q,{sx:jy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`TextField`})]});var Ny={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Py=()=>(0,R.jsxs)(q,{sx:Ny.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Toolbar`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Toolbar`}),` and related helpers (`,(0,R.jsx)(`code`,{children:`V.ToolbarButton`}),`,`,` `,(0,R.jsx)(`code`,{children:`V.ToolbarToggle`}),`, `,(0,R.jsx)(`code`,{children:`V.ToolbarMenu`}),`, etc.) wrap the UI Toolkit editor toolbar elements using the `,(0,R.jsx)(`code`,{children:`ToolbarProps`}),` family.`]}),(0,R.jsxs)(q,{sx:Ny.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ToolbarProps`)})]}),(0,R.jsxs)(q,{sx:Ny.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage (Editor)`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Editor

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
}`})]}),(0,R.jsx)($,{componentName:`Toolbar`})]});var Fy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Iy=()=>(0,R.jsxs)(q,{sx:Fy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RectField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.RectField`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`RectField`}),` control using`,` `,(0,R.jsx)(`code`,{children:`RectFieldProps`}),`. It is available in both runtime and editor UIs.`]}),(0,R.jsxs)(q,{sx:Fy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`RectFieldProps`)})]}),(0,R.jsxs)(q,{sx:Fy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`RectField`})]});var Ly={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ry=()=>(0,R.jsxs)(q,{sx:Ly.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`RectIntField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.RectIntField`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`RectIntField`}),` control using`,` `,(0,R.jsx)(`code`,{children:`RectIntFieldProps`}),`. It is available in both runtime and editor UIs.`]}),(0,R.jsxs)(q,{sx:Ly.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`RectIntFieldProps`)})]}),(0,R.jsxs)(q,{sx:Ly.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`RectIntField`})]});var zy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const By=()=>(0,R.jsxs)(q,{sx:zy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UnsignedIntegerField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.UnsignedIntegerField`}),` represents a `,(0,R.jsx)(`code`,{children:`uint`}),` numeric field using`,` `,(0,R.jsx)(`code`,{children:`UnsignedIntegerFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:zy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`UnsignedIntegerFieldProps`)})]}),(0,R.jsxs)(q,{sx:zy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`UnsignedIntegerField`})]});var Vy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Hy=()=>(0,R.jsxs)(q,{sx:Vy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`UnsignedLongField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.UnsignedLongField`}),` represents a `,(0,R.jsx)(`code`,{children:`ulong`}),` numeric field using`,` `,(0,R.jsx)(`code`,{children:`UnsignedLongFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Vy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`UnsignedLongFieldProps`)})]}),(0,R.jsxs)(q,{sx:Vy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`UnsignedLongField`})]});var Uy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Wy=()=>(0,R.jsxs)(q,{sx:Uy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector2Field`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Vector2Field`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Vector2Field`}),` control using`,` `,(0,R.jsx)(`code`,{children:`Vector2FieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Uy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`Vector2FieldProps`)})]}),(0,R.jsxs)(q,{sx:Uy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Vector2Field`})]});var Gy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Ky=()=>(0,R.jsxs)(q,{sx:Gy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector2IntField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Vector2IntField`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Vector2IntField`}),` control using`,` `,(0,R.jsx)(`code`,{children:`Vector2IntFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Gy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`Vector2IntFieldProps`)})]}),(0,R.jsxs)(q,{sx:Gy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Vector2IntField`})]});var qy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Jy=()=>(0,R.jsxs)(q,{sx:qy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector3Field`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Vector3Field`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Vector3Field`}),` control using`,` `,(0,R.jsx)(`code`,{children:`Vector3FieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:qy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`Vector3FieldProps`)})]}),(0,R.jsxs)(q,{sx:qy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Vector3Field`})]});var Yy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Xy=()=>(0,R.jsxs)(q,{sx:Yy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector3IntField`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Vector3IntField`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Vector3IntField`}),` control using`,` `,(0,R.jsx)(`code`,{children:`Vector3IntFieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Yy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`Vector3IntFieldProps`)})]}),(0,R.jsxs)(q,{sx:Yy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Vector3IntField`})]});var Zy={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const Qy=()=>(0,R.jsxs)(q,{sx:Zy.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Vector4Field`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Vector4Field`}),` wraps the UI Toolkit `,(0,R.jsx)(`code`,{children:`Vector4Field`}),` control using`,` `,(0,R.jsx)(`code`,{children:`Vector4FieldProps`}),`.`]}),(0,R.jsxs)(q,{sx:Zy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`Vector4FieldProps`)})]}),(0,R.jsxs)(q,{sx:Zy.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Vector4Field`})]});var $y={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const eb=()=>(0,R.jsxs)(q,{sx:$y.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TemplateContainer`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.TemplateContainer`}),` wraps UI Toolkit `,(0,R.jsx)(`code`,{children:`TemplateContainer`}),` and exposes a`,` `,(0,R.jsx)(`code`,{children:`ContentContainer`}),` slot through `,(0,R.jsx)(`code`,{children:`TemplateContainerProps`}),`.`]}),(0,R.jsxs)(q,{sx:$y.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`TemplateContainerProps`)})]}),(0,R.jsxs)(q,{sx:$y.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`TemplateContainer`})]});var tb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const nb=()=>(0,R.jsxs)(q,{sx:tb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`VisualElement`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.VisualElement`}),` creates a generic container element styled via a `,(0,R.jsx)(`code`,{children:`Style`}),` `,`instance, and is often used as the top-level layout node for your component trees.`]}),(0,R.jsxs)(q,{sx:tb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signature`}),(0,R.jsx)(Z,{language:`tsx`,code:`public static VirtualNode VisualElement(
  Style style,
  string key = null,
  params VirtualNode[] children
);

public static VirtualNode VisualElement(
  IReadOnlyDictionary<string, object> elementProperties = null,
  string key = null,
  params VirtualNode[] children
);`})]}),(0,R.jsxs)(q,{sx:tb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic container`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`VisualElement`})]});var rb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ib=()=>(0,R.jsxs)(q,{sx:rb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`VisualElementSafe`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.VisualElementSafe`}),` is a safe-area-aware variant of `,(0,R.jsx)(`code`,{children:`V.VisualElement`}),` `,`that merges its padding with safe-area insets from `,(0,R.jsx)(`code`,{children:`SafeAreaUtility`}),`. Use it as a top-level container on devices with notches or system UI overlays.`]}),(0,R.jsxs)(q,{sx:rb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Signature`}),(0,R.jsx)(Z,{language:`tsx`,code:`public static VirtualNode VisualElementSafe(
  Style style = null,
  string key = null,
  params VirtualNode[] children
);`})]}),(0,R.jsxs)(q,{sx:rb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Safe-area aware container`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
    return V.VisualElementSafe(
      SafeStyle,
      null,
      V.Label(new LabelProps { Text = "Safe-area aware root" })
    );
  }
}`})]})]});var ab={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ob=()=>(0,R.jsxs)(q,{sx:ab.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Animate`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Animate`}),` wraps a child subtree and drives one or more animation tracks on its root `,(0,R.jsx)(`code`,{children:`VisualElement`}),`. It is a thin, declarative wrapper around`,` `,(0,R.jsx)(`code`,{children:`Hooks.UseAnimate`}),` and the underlying `,(0,R.jsx)(`code`,{children:`Animator`}),` helpers.`]}),(0,R.jsxs)(q,{sx:ab.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`AnimateProps`)})]}),(0,R.jsxs)(q,{sx:ab.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,R.jsxs)(xg,{sx:ab.section,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Tracks are defined via AnimateTrack helpers and target individual style properties (for example, backgroundColor, opacity, size).`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Each track specifies from/to values, duration, easing, and optional delay.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`When the Animate node mounts or its dependencies change, tracks are played; they are stopped and cleaned up automatically when unmounting.`})})]})]}),(0,R.jsxs)(q,{sx:ab.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]})]});var sb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const cb=()=>(0,R.jsxs)(q,{sx:sb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`ErrorBoundary`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.ErrorBoundary`}),` catches exceptions from its descendants and renders the`,` `,(0,R.jsx)(`code`,{children:`Fallback`}),` `,(0,R.jsx)(`code`,{children:`VirtualNode`}),` from `,(0,R.jsx)(`code`,{children:`ErrorBoundaryProps`}),`.`]}),(0,R.jsxs)(q,{sx:sb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ErrorBoundaryProps`)})]}),(0,R.jsxs)(q,{sx:sb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]})]});var lb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const ub=()=>(0,R.jsxs)(q,{sx:lb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MultiColumnListView`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.MultiColumnListView`}),` displays tabular data with columns configured via`,` `,(0,R.jsx)(`code`,{children:`MultiColumnListViewProps`}),`. It is backed by Unity's`,` `,(0,R.jsx)(`code`,{children:`MultiColumnListView`}),` control and supports large, virtualized data sets.`]}),(0,R.jsxs)(q,{sx:lb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`MultiColumnListViewProps`)})]}),(0,R.jsxs)(q,{sx:lb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,R.jsxs)(xg,{sx:lb.section,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Items are provided as an IList; rows are virtualized by the underlying control for performance.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Columns are defined via MultiColumnListViewColumn objects, each with a name, width, and Cell callback.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`The Cell callback receives the strongly-typed row item and index so you can render arbitrary content per column.`})})]})]}),(0,R.jsxs)(q,{sx:lb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`MultiColumnListView`})]});var db={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const fb=()=>(0,R.jsxs)(q,{sx:db.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`MultiColumnTreeView`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.MultiColumnTreeView`}),` renders hierarchical data across multiple columns via`,` `,(0,R.jsx)(`code`,{children:`MultiColumnTreeViewProps`}),`. It is backed by Unity's`,` `,(0,R.jsx)(`code`,{children:`MultiColumnTreeView`}),` control and is suitable for project browser–style views.`]}),(0,R.jsxs)(q,{sx:db.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`MultiColumnTreeViewProps`)})]}),(0,R.jsxs)(q,{sx:db.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Concepts`}),(0,R.jsxs)(xg,{sx:db.section,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Items are provided as a tree of nodes; the adapter flattens and expands them based on TreeView state.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Columns are defined via MultiColumnTreeViewColumn objects, just like MultiColumnListView.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Each Cell callback receives the node item and index so you can render per-column content (labels, badges, icons).`})})]})]}),(0,R.jsxs)(q,{sx:db.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`MultiColumnTreeView`})]});var pb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const mb=()=>(0,R.jsxs)(q,{sx:pb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Scroller`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.Scroller`}),` wraps the low-level UI Toolkit `,(0,R.jsx)(`code`,{children:`Scroller`}),` element using`,` `,(0,R.jsx)(`code`,{children:`ScrollerProps`}),`.`]}),(0,R.jsxs)(q,{sx:pb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`ScrollerProps`)})]}),(0,R.jsxs)(q,{sx:pb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`Scroller`})]});var hb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const gb=()=>(0,R.jsxs)(q,{sx:hb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TextElement`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[(0,R.jsx)(`code`,{children:`V.TextElement`}),` is a low-level text node wrapper using `,(0,R.jsx)(`code`,{children:`TextElementProps`}),`.`]}),(0,R.jsxs)(q,{sx:hb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`TextElementProps`)})]}),(0,R.jsxs)(q,{sx:hb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`TextElement`})]});var _b={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const vb=()=>(0,R.jsxs)(q,{sx:_b.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`PropertyField & InspectorElement`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Editor-only helpers that wrap Unity's `,(0,R.jsx)(`code`,{children:`PropertyField`}),` and `,(0,R.jsx)(`code`,{children:`InspectorElement`}),` `,`via `,(0,R.jsx)(`code`,{children:`PropertyFieldProps`}),` and `,(0,R.jsx)(`code`,{children:`InspectorElementProps`}),`.`]}),(0,R.jsxs)(q,{sx:_b.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`PropertyInspectorProps`)})]}),(0,R.jsxs)(q,{sx:_b.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`PropertyInspector`})]});var yb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2}};const bb=()=>(0,R.jsxs)(q,{sx:yb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`TwoPaneSplitView`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Editor-only splitter layout wrapping Unity's `,(0,R.jsx)(`code`,{children:`TwoPaneSplitView`}),` via`,` `,(0,R.jsx)(`code`,{children:`TwoPaneSplitViewProps`}),`.`]}),(0,R.jsxs)(q,{sx:yb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Props`}),(0,R.jsx)(Z,{language:`tsx`,code:Q(`TwoPaneSplitViewProps`)})]}),(0,R.jsxs)(q,{sx:yb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Basic usage`}),(0,R.jsx)(Z,{language:`tsx`,code:`// Example namespace: ReactiveUITK.Samples.Components

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
}`})]}),(0,R.jsx)($,{componentName:`TwoPaneSplitView`})]});var xb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const Sb=()=>(0,R.jsxs)(q,{sx:xb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Known Issues`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`There is a known issue where `,(0,R.jsx)(`code`,{children:`MultiColumnListView`}),` can briefly jump or snap when scrolling large data sets; this will be addressed in a future update.`]})]});var Cb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}};const wb=()=>(0,R.jsxs)(q,{sx:Cb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Roadmap`}),(0,R.jsx)(K,{variant:`body1`,paragraph:!0,children:`The roadmap will be documented here in a future update.`})]});var Tb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},Eb=`using System.Collections.Generic;
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
}`,Db=`using System.Collections.Generic;
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
}`;const Ob=()=>(0,R.jsxs)(q,{sx:Tb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special animation hooks`}),(0,R.jsx)(K,{variant:`body1`,paragraph:!0,children:`ReactiveUIToolKit exposes animation-specific hooks that do not exist in React's core API. These hooks are designed to drive UI Toolkit animations in a frame-accurate way while still fitting into the normal function component lifecycle.`}),(0,R.jsxs)(q,{sx:Tb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,R.jsx)(`code`,{children:`Hooks.UseAnimate`})}),(0,R.jsxs)(xg,{sx:Tb.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Starts one or more AnimateTrack definitions on the component's VisualElement container.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Tracks are created with ReactiveUITK.Core.Animation.AnimateTrack helpers (for example, animating background color or size).`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Plays animations when dependencies change, and stops/cleans them up when the component unmounts or the effect is re-run.`})})]}),(0,R.jsx)(Z,{language:`tsx`,code:Eb})]}),(0,R.jsxs)(q,{sx:Tb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,R.jsx)(`code`,{children:`Hooks.UseTweenFloat`})}),(0,R.jsxs)(xg,{sx:Tb.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Tweens a single float value over time with easing and an optional delay.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Calls an onUpdate callback every frame with the eased value, and an onComplete callback when finished.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Uses UI Toolkit's scheduler and integrates with the component's lifecycle; cancelling on unmount.`})})]}),(0,R.jsx)(Z,{language:`tsx`,code:Db})]}),(0,R.jsxs)(K,{variant:`body2`,sx:Tb.section,children:[`For a higher-level API, see the `,(0,R.jsx)(`code`,{children:`Animate`}),` component documented under Components → Common/Uncommon Components. It builds on top of these hooks and the underlying`,` `,(0,R.jsx)(`code`,{children:`Animator`}),` utilities.`]})]});var kb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},Ab=`using System.Collections.Generic;
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
}`;const jb=()=>(0,R.jsxs)(q,{sx:kb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special router hooks`}),(0,R.jsx)(K,{variant:`body1`,paragraph:!0,children:`The router in ReactiveUIToolKit ships with a set of hooks that mirror React Router's ergonomics but are implemented entirely in C# for Unity UI Toolkit.`}),(0,R.jsxs)(q,{sx:kb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Reading router state`}),(0,R.jsxs)(xg,{sx:kb.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RouterHooks.UseLocation()`}),` / `,(0,R.jsx)(`code`,{children:`UseLocationInfo()`}),` – current path, query, and optional navigation state.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RouterHooks.UseParams()`}),` – path parameters extracted from the active route template.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RouterHooks.UseQuery()`}),` – parsed query-string key/value pairs.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RouterHooks.UseNavigationState()`}),` – arbitrary state object provided when navigating.`]})})})]})]}),(0,R.jsxs)(q,{sx:kb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Navigation helpers`}),(0,R.jsxs)(xg,{sx:kb.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RouterHooks.UseNavigate(replace = false)`}),` – imperative navigation, similar to React Router's `,(0,R.jsx)(`code`,{children:`useNavigate`}),`.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RouterHooks.UseGo()`}),` – navigate relative to the history stack (for example, `,(0,R.jsx)(`code`,{children:`go(-1)`}),`, `,(0,R.jsx)(`code`,{children:`go(1)`}),`).`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RouterHooks.UseCanGo(delta)`}),` – returns whether a given delta is available for back/forward UI.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`RouterHooks.UseBlocker(blocker, enabled)`}),` – intercepts transitions to implement confirmation prompts.`]})})})]}),(0,R.jsx)(Z,{language:`tsx`,code:Ab})]}),(0,R.jsxs)(K,{variant:`body2`,sx:kb.section,children:[`See the main Router documentation for complete examples of composing `,(0,R.jsx)(`code`,{children:`V.Router`}),`,`,` `,(0,R.jsx)(`code`,{children:`V.Route`}),`, `,(0,R.jsx)(`code`,{children:`V.Link`}),`, and these hooks in editor and runtime apps.`]})]});var Mb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},Nb=`using System.Collections.Generic;
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
}`;const Pb=()=>(0,R.jsxs)(q,{sx:Mb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Special signal hooks`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`Signals provide a small, global, observable state primitive. The`,` `,(0,R.jsx)(`code`,{children:`Hooks.UseSignal`}),` family gives you fine-grained reactivity from function components, something React does not have out of the box.`]}),(0,R.jsxs)(q,{sx:Mb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,R.jsx)(`code`,{children:`Hooks.UseSignal`})}),(0,R.jsxs)(xg,{sx:Mb.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`Hooks.UseSignal(Signal<T>)`}),` – subscribe to a`,` `,(0,R.jsx)(`code`,{children:`Signal<T>`}),` and re-render when it changes.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`Hooks.UseSignal<T>(key, initialValue)`}),` – shorthand that resolves a`,` `,(0,R.jsx)(`code`,{children:`Signal<T>`}),` from the global registry by key.`]})})})]}),(0,R.jsx)(Z,{language:`tsx`,code:Nb})]}),(0,R.jsxs)(q,{sx:Mb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:`Selector overloads`}),(0,R.jsxs)(xg,{sx:Mb.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(signal, selector, comparer)`}),` – project a slice of a signal value and control equality.`]})})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(`code`,{children:`Hooks.UseSignal<T, TSlice>(key, selector, comparer, initialValue)`}),` `,`– keyed variant that creates/resolves the signal for you.`]})})})]})]}),(0,R.jsxs)(K,{variant:`body2`,sx:Mb.section,children:[`For an end-to-end walkthrough, see the Signals page, which shows how to combine`,` `,(0,R.jsx)(`code`,{children:`Signals.Get`}),`, `,(0,R.jsx)(`code`,{children:`Hooks.UseSignal`}),`, and dispatch helpers in real UIs.`]})]});var Fb={root:{display:`flex`,flexDirection:`column`,gap:2},section:{mt:2},list:{pl:2}},Ib=`using System.Collections.Generic;
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
}`;const Lb=()=>(0,R.jsxs)(q,{sx:Fb.root,children:[(0,R.jsx)(K,{variant:`h4`,component:`h1`,gutterBottom:!0,children:`Safe area hooks`}),(0,R.jsxs)(K,{variant:`body1`,paragraph:!0,children:[`When targeting mobile or platforms with notches and system insets, the`,` `,(0,R.jsx)(`code`,{children:`Hooks.UseSafeArea`}),` hook and `,(0,R.jsx)(`code`,{children:`V.VisualElementSafe`}),` helper work together to keep your layout inside the safe region.`]}),(0,R.jsxs)(q,{sx:Fb.section,children:[(0,R.jsx)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:(0,R.jsx)(`code`,{children:`Hooks.UseSafeArea`})}),(0,R.jsxs)(xg,{sx:Fb.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Returns SafeAreaInsets (top, bottom, left, right) based on Unity's Screen.safeArea.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Records a hook usage so that changes to the safe area can trigger re-rendering.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Accepts an optional tolerance parameter to avoid flicker when the reported insets change only slightly.`})})]}),(0,R.jsx)(Z,{language:`tsx`,code:Ib})]}),(0,R.jsxs)(q,{sx:Fb.section,children:[(0,R.jsxs)(K,{variant:`h5`,component:`h2`,gutterBottom:!0,children:[(0,R.jsx)(`code`,{children:`V.VisualElementSafe`}),` helper`]}),(0,R.jsxs)(xg,{sx:Fb.list,children:[(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`V.VisualElementSafe(style, key, children) – wraps a VisualElement and automatically applies padding based on SafeAreaInsets.`})}),(0,R.jsx)(J,{disablePadding:!0,children:(0,R.jsx)(Y,{primary:`Merges your own padding with the safe-area padding so you keep control over layout while staying visible on all devices.`})})]})]}),(0,R.jsxs)(K,{variant:`body2`,sx:Fb.section,children:[`Combine `,(0,R.jsx)(`code`,{children:`Hooks.UseSafeArea`}),` when you need direct access to inset values with`,` `,(0,R.jsx)(`code`,{children:`V.VisualElementSafe`}),` when you want a drop-in, safe-area-aware container.`]})]}),Rb=[{id:`intro`,title:`Introduction`,pages:[{id:`introduction`,title:`Introduction`,path:`/`,keywords:[`overview`,`unity 6.2`,`reactive`,`ui toolkit`],element:()=>(0,R.jsx)(a_,{})}]},{id:`getting-started`,title:`Getting Started`,pages:[{id:`install`,title:`Install & Setup`,path:`/getting-started`,keywords:[`install`,`setup`,`unity package manager`,`dist`],element:()=>(0,R.jsx)(iv,{})}]},{id:`concepts`,title:`Concepts & Environment`,pages:[{id:`concepts-and-environment`,title:`Concepts & Environment`,path:`/concepts`,keywords:[`concepts`,`environment`,`defines`,`trace`,`react differences`],element:()=>(0,R.jsx)(uv,{})}]},{id:`differences`,title:`Different from React`,pages:[{id:`different-from-react`,title:`Different from React`,path:`/differences`,keywords:[`react`,`usestate`,`signals`,`differences`],element:()=>(0,R.jsx)(fv,{})}]},{id:`tooling`,title:`Tooling`,pages:[{id:`router`,title:`Router`,path:`/tooling/router`,keywords:[`navigation`,`routes`],element:()=>(0,R.jsx)(ov,{})},{id:`signals`,title:`Signals`,path:`/tooling/signals`,keywords:[`state`,`observable`],element:()=>(0,R.jsx)(cv,{})}]},{id:`components`,title:`Components`,pages:[{id:`component-bounds-field`,title:`BoundsField`,path:`/components/bounds-field`,keywords:[`bounds`,`field`,`BoundsField`],group:`advanced`,element:()=>(0,R.jsx)(vv,{})},{id:`component-bounds-int-field`,title:`BoundsIntField`,path:`/components/bounds-int-field`,keywords:[`boundsint`,`field`,`BoundsIntField`],element:()=>(0,R.jsx)(bv,{})},{id:`component-box`,title:`Box`,path:`/components/box`,keywords:[`box`,`container`],group:`basic`,element:()=>(0,R.jsx)(Sv,{})},{id:`component-button`,title:`Button`,path:`/components/button`,keywords:[`button`,`click`],group:`basic`,element:()=>(0,R.jsx)(wv,{})},{id:`component-color-field`,title:`ColorField`,path:`/components/color-field`,keywords:[`color`,`field`,`ColorField`],group:`advanced`,element:()=>(0,R.jsx)(Ev,{})},{id:`component-double-field`,title:`DoubleField`,path:`/components/double-field`,keywords:[`double`,`field`,`DoubleField`],group:`advanced`,element:()=>(0,R.jsx)(Ov,{})},{id:`component-dropdown-field`,title:`DropdownField`,path:`/components/dropdown-field`,keywords:[`dropdown`,`field`,`choices`],group:`basic`,element:()=>(0,R.jsx)(Av,{})},{id:`component-enum-field`,title:`EnumField`,path:`/components/enum-field`,keywords:[`enum`,`field`,`EnumField`],group:`basic`,element:()=>(0,R.jsx)(Mv,{})},{id:`component-enum-flags-field`,title:`EnumFlagsField`,path:`/components/enum-flags-field`,keywords:[`enum`,`flags`,`EnumFlagsField`],group:`advanced`,element:()=>(0,R.jsx)(Pv,{})},{id:`component-float-field`,title:`FloatField`,path:`/components/float-field`,keywords:[`float`,`field`,`FloatField`],group:`basic`,element:()=>(0,R.jsx)(Iv,{})},{id:`component-foldout`,title:`Foldout`,path:`/components/foldout`,keywords:[`foldout`,`toggle`,`collapsible`],group:`basic`,element:()=>(0,R.jsx)(Rv,{})},{id:`component-group-box`,title:`GroupBox`,path:`/components/group-box`,keywords:[`group`,`groupbox`],group:`basic`,element:()=>(0,R.jsx)(Bv,{})},{id:`component-hash128-field`,title:`Hash128Field`,path:`/components/hash128-field`,keywords:[`hash128`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Hv,{})},{id:`component-help-box`,title:`HelpBox`,path:`/components/help-box`,keywords:[`helpbox`,`message`],group:`basic`,element:()=>(0,R.jsx)(Wv,{})},{id:`component-imgui-container`,title:`IMGUIContainer`,path:`/components/imgui-container`,keywords:[`imgui`,`editor`],group:`advanced`,element:()=>(0,R.jsx)(Kv,{})},{id:`component-image`,title:`Image`,path:`/components/image`,keywords:[`image`,`texture`,`sprite`],group:`basic`,element:()=>(0,R.jsx)(Jv,{})},{id:`component-integer-field`,title:`IntegerField`,path:`/components/integer-field`,keywords:[`integer`,`field`,`int`],group:`basic`,element:()=>(0,R.jsx)(Xv,{})},{id:`component-label`,title:`Label`,path:`/components/label`,keywords:[`label`,`text`],group:`basic`,element:()=>(0,R.jsx)(Qv,{})},{id:`component-long-field`,title:`LongField`,path:`/components/long-field`,keywords:[`long`,`field`,`LongField`],group:`advanced`,element:()=>(0,R.jsx)(ey,{})},{id:`component-progress-bar`,title:`ProgressBar`,path:`/components/progress-bar`,keywords:[`progress`,`bar`],group:`basic`,element:()=>(0,R.jsx)(ny,{})},{id:`component-list-view`,title:`ListView`,path:`/components/list-view`,keywords:[`list`,`ListView`],group:`basic`,element:()=>(0,R.jsx)(iy,{})},{id:`component-minmax-slider`,title:`MinMaxSlider`,path:`/components/minmax-slider`,keywords:[`minmax`,`slider`],group:`advanced`,element:()=>(0,R.jsx)(oy,{})},{id:`component-object-field`,title:`ObjectField`,path:`/components/object-field`,keywords:[`object`,`field`],group:`advanced`,element:()=>(0,R.jsx)(cy,{})},{id:`component-radio-button`,title:`RadioButton`,path:`/components/radio-button`,keywords:[`radio`,`button`],group:`basic`,element:()=>(0,R.jsx)(uy,{})},{id:`component-radio-button-group`,title:`RadioButtonGroup`,path:`/components/radio-button-group`,keywords:[`radio`,`group`],group:`basic`,element:()=>(0,R.jsx)(fy,{})},{id:`component-rect-field`,title:`RectField`,path:`/components/rect-field`,keywords:[`rect`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Iy,{})},{id:`component-rect-int-field`,title:`RectIntField`,path:`/components/rect-int-field`,keywords:[`rectint`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Ry,{})},{id:`component-repeat-button`,title:`RepeatButton`,path:`/components/repeat-button`,keywords:[`repeat`,`button`],group:`basic`,element:()=>(0,R.jsx)(my,{})},{id:`component-scroll-view`,title:`ScrollView`,path:`/components/scroll-view`,keywords:[`scroll`,`view`],group:`basic`,element:()=>(0,R.jsx)(gy,{})},{id:`component-slider`,title:`Slider`,path:`/components/slider`,keywords:[`slider`,`float`],group:`basic`,element:()=>(0,R.jsx)(vy,{})},{id:`component-slider-int`,title:`SliderInt`,path:`/components/slider-int`,keywords:[`slider`,`int`],group:`basic`,element:()=>(0,R.jsx)(by,{})},{id:`component-toggle`,title:`Toggle`,path:`/components/toggle`,keywords:[`toggle`,`checkbox`],group:`basic`,element:()=>(0,R.jsx)(Sy,{})},{id:`component-tree-view`,title:`TreeView`,path:`/components/tree-view`,keywords:[`tree`,`TreeView`],group:`basic`,element:()=>(0,R.jsx)(wy,{})},{id:`component-tab`,title:`Tab`,path:`/components/tab`,keywords:[`tab`],group:`basic`,element:()=>(0,R.jsx)(Ey,{})},{id:`component-tab-view`,title:`TabView`,path:`/components/tab-view`,keywords:[`tab`,`TabView`],group:`basic`,element:()=>(0,R.jsx)(Oy,{})},{id:`component-toggle-button-group`,title:`ToggleButtonGroup`,path:`/components/toggle-button-group`,keywords:[`toggle`,`buttons`,`group`],group:`advanced`,element:()=>(0,R.jsx)(Ay,{})},{id:`component-text-field`,title:`TextField`,path:`/components/text-field`,keywords:[`text`,`field`],group:`basic`,element:()=>(0,R.jsx)(My,{})},{id:`component-toolbar`,title:`Toolbar`,path:`/components/toolbar`,keywords:[`toolbar`,`editor`],group:`advanced`,element:()=>(0,R.jsx)(Py,{})},{id:`component-template-container`,title:`TemplateContainer`,path:`/components/template-container`,keywords:[`template`,`container`],group:`advanced`,element:()=>(0,R.jsx)(eb,{})},{id:`component-visual-element`,title:`VisualElement`,path:`/components/visual-element`,keywords:[`visualelement`,`container`,`safe`],group:`basic`,element:()=>(0,R.jsx)(nb,{})},{id:`component-visual-element-safe`,title:`VisualElementSafe`,path:`/components/visual-element-safe`,keywords:[`visualelementsafe`,`safe-area`,`container`],group:`basic`,element:()=>(0,R.jsx)(ib,{})},{id:`component-unsigned-integer-field`,title:`UnsignedIntegerField`,path:`/components/unsigned-integer-field`,keywords:[`uint`,`field`],group:`advanced`,element:()=>(0,R.jsx)(By,{})},{id:`component-unsigned-long-field`,title:`UnsignedLongField`,path:`/components/unsigned-long-field`,keywords:[`ulong`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Hy,{})},{id:`component-vector2-field`,title:`Vector2Field`,path:`/components/vector2-field`,keywords:[`vector2`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Wy,{})},{id:`component-vector2-int-field`,title:`Vector2IntField`,path:`/components/vector2-int-field`,keywords:[`vector2int`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Ky,{})},{id:`component-vector3-field`,title:`Vector3Field`,path:`/components/vector3-field`,keywords:[`vector3`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Jy,{})},{id:`component-vector3-int-field`,title:`Vector3IntField`,path:`/components/vector3-int-field`,keywords:[`vector3int`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Xy,{})},{id:`component-vector4-field`,title:`Vector4Field`,path:`/components/vector4-field`,keywords:[`vector4`,`field`],group:`advanced`,element:()=>(0,R.jsx)(Qy,{})},{id:`component-animate`,title:`Animate`,path:`/components/animate`,keywords:[`animate`,`animation`],group:`basic`,element:()=>(0,R.jsx)(ob,{})},{id:`component-error-boundary`,title:`ErrorBoundary`,path:`/components/error-boundary`,keywords:[`error`,`boundary`],group:`advanced`,element:()=>(0,R.jsx)(cb,{})},{id:`component-multi-column-list-view`,title:`MultiColumnListView`,path:`/components/multi-column-list-view`,keywords:[`list`,`multi`,`columns`],group:`basic`,element:()=>(0,R.jsx)(ub,{})},{id:`component-multi-column-tree-view`,title:`MultiColumnTreeView`,path:`/components/multi-column-tree-view`,keywords:[`tree`,`multi`,`columns`],group:`basic`,element:()=>(0,R.jsx)(fb,{})},{id:`component-scroller`,title:`Scroller`,path:`/components/scroller`,keywords:[`scroller`],group:`advanced`,element:()=>(0,R.jsx)(mb,{})},{id:`component-text-element`,title:`TextElement`,path:`/components/text-element`,keywords:[`text`,`TextElement`],group:`advanced`,element:()=>(0,R.jsx)(gb,{})},{id:`component-property-inspector`,title:`PropertyField & InspectorElement`,path:`/components/property-inspector`,keywords:[`propertyfield`,`inspectorelement`,`editor`],group:`advanced`,element:()=>(0,R.jsx)(vb,{})},{id:`component-two-pane-split-view`,title:`TwoPaneSplitView`,path:`/components/two-pane-split-view`,keywords:[`split`,`editor`],group:`advanced`,element:()=>(0,R.jsx)(bb,{})}]},{id:`special-hooks`,title:`Special Hooks`,pages:[{id:`special-hooks-animation`,title:`Animation hooks`,path:`/special-hooks/animation`,keywords:[`hooks`,`animation`,`UseAnimate`,`UseTweenFloat`],element:()=>(0,R.jsx)(Ob,{})},{id:`special-hooks-router`,title:`Router hooks`,path:`/special-hooks/router`,keywords:[`hooks`,`router`,`RouterHooks`],element:()=>(0,R.jsx)(jb,{})},{id:`special-hooks-signals`,title:`Signal hooks`,path:`/special-hooks/signals`,keywords:[`hooks`,`signals`,`UseSignal`],element:()=>(0,R.jsx)(Pb,{})},{id:`special-hooks-safe-area`,title:`Safe area hooks`,path:`/special-hooks/safe-area`,keywords:[`hooks`,`safe area`,`UseSafeArea`,`VisualElementSafe`],element:()=>(0,R.jsx)(Lb,{})}]},{id:`api`,title:`API`,pages:[{id:`api-reference`,title:`API Reference`,path:`/api`,keywords:[`api`,`namespace`,`props`,`hooks`,`router`,`signals`],element:()=>(0,R.jsx)(mv,{})}]},{id:`known-issues`,title:`Known Issues`,pages:[{id:`known-issues-page`,title:`Known Issues`,path:`/known-issues`,keywords:[`issues`,`limitations`,`known issues`],element:()=>(0,R.jsx)(Sb,{})}]},{id:`roadmap`,title:`Roadmap`,pages:[{id:`roadmap-page`,title:`Roadmap`,path:`/roadmap`,keywords:[`roadmap`,`future`,`plans`],element:()=>(0,R.jsx)(wb,{})}]}],zb=Rb.flatMap(e=>{if(e.id===`components`){let t=e.pages.filter(e=>e.group===`basic`),n=e.pages.filter(e=>e.group===`advanced`||!e.group);return[...t,...n]}return e.pages});var Bb=Il((0,R.jsx)(`path`,{d:`M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14`}),`Search`),Vb=Il((0,R.jsx)(`path`,{d:`M12 1.27a11 11 0 00-3.48 21.46c.55.09.73-.28.73-.55v-1.84c-3.03.64-3.67-1.46-3.67-1.46-.55-1.29-1.28-1.65-1.28-1.65-.92-.65.1-.65.1-.65 1.1 0 1.73 1.1 1.73 1.1.92 1.65 2.57 1.2 3.21.92a2 2 0 01.64-1.47c-2.47-.27-5.04-1.19-5.04-5.5 0-1.1.46-2.1 1.2-2.84a3.76 3.76 0 010-2.93s.91-.28 3.11 1.1c1.8-.49 3.7-.49 5.5 0 2.1-1.38 3.02-1.1 3.02-1.1a3.76 3.76 0 010 2.93c.83.74 1.2 1.74 1.2 2.94 0 4.21-2.57 5.13-5.04 5.4.45.37.82.92.82 2.02v3.03c0 .27.1.64.73.55A11 11 0 0012 1.27`}),`GitHub`),Hb={appBar:{borderBottom:1,borderColor:`divider`},toolbar:{display:`flex`,alignItems:`center`,gap:2},left:{display:`flex`,alignItems:`center`,gap:1.25},logo:{width:28,height:28,borderRadius:1},titleLink:{display:`flex`,alignItems:`center`,gap:.75,color:`inherit`,textDecoration:`none`},title:{fontWeight:600,letterSpacing:.3},center:{flex:1,display:`flex`,justifyContent:`center`},searchPaper:{p:`2px 8px`,display:`flex`,alignItems:`center`,gap:1,width:360,cursor:`text`},inputFlex:{flex:1},right:{ml:1,display:`flex`,alignItems:`center`,gap:1}};const Ub=({onOpenSearch:e})=>(0,R.jsx)(Zd,{position:`sticky`,color:`default`,elevation:0,sx:Hb.appBar,children:(0,R.jsxs)(r_,{sx:Hb.toolbar,children:[(0,R.jsxs)(q,{sx:Hb.left,children:[(0,R.jsxs)(gg,{component:$t,to:`/`,underline:`none`,sx:Hb.titleLink,children:[(0,R.jsx)(q,{component:`img`,src:`/logo.png`,alt:`ReactiveUIToolKit logo`,sx:Hb.logo}),(0,R.jsx)(K,{variant:`h6`,sx:Hb.title,children:`ReactiveUIToolKit`})]}),(0,R.jsx)(hm,{label:`v0.0.31`,size:`small`})]}),(0,R.jsx)(q,{sx:Hb.center,children:(0,R.jsxs)(Qu,{sx:Hb.searchPaper,variant:`outlined`,onClick:e,children:[(0,R.jsx)(Bb,{fontSize:`small`}),(0,R.jsx)(Fm,{placeholder:`Search docs…`,sx:Hb.inputFlex,readOnly:!0,autoFocus:!0})]})}),(0,R.jsxs)(q,{sx:Hb.right,children:[(0,R.jsx)(hm,{label:`Unity 6.2+`,size:`small`}),(0,R.jsx)(zd,{component:gg,href:`https://github.com/yanivkalfa/ReactiveUIToolKit.git`,target:`_blank`,rel:`noreferrer`,children:(0,R.jsx)(Vb,{})})]})]})});var Wb=Il((0,R.jsx)(`path`,{d:`m12 8-6 6 1.41 1.41L12 10.83l4.59 4.58L18 14z`}),`ExpandLess`),Gb=Il((0,R.jsx)(`path`,{d:`M16.59 8.59 12 13.17 7.41 8.59 6 10l6 6 6-6z`}),`ExpandMore`),Kb={root:{width:280,borderRight:1,borderColor:`divider`,height:`100%`,overflow:`auto`,"&::-webkit-scrollbar":{width:8},"&::-webkit-scrollbar-track":{backgroundColor:`transparent`},"&::-webkit-scrollbar-thumb":{backgroundColor:`rgba(25,118,210,0.4)`,borderRadius:999,border:`2px solid transparent`,backgroundClip:`padding-box`},"&::-webkit-scrollbar-thumb:hover":{backgroundColor:`rgba(25,118,210,0.7)`},scrollbarWidth:`thin`,scrollbarColor:`rgba(25,118,210,0.6) transparent`},childItem:{pl:4},sectionTitle:{fontWeight:700},subgroupHeader:{pl:4,pt:1,pb:.5,fontSize:11,textTransform:`uppercase`,letterSpacing:.5,color:`text.secondary`},subgroupDivider:{mt:.5,mb:.5,opacity:.4}};const qb=()=>{let e=Rb.flatMap(e=>e.id===`components`?[{...e,id:`components-common`,title:`Common Components`,pages:e.pages.filter(e=>e.group===`basic`)},{...e,id:`components-uncommon`,title:`Uncommon Components`,pages:e.pages.filter(e=>e.group===`advanced`||!e.group)}]:[e]),t=ze(),[n,r]=(0,x.useState)(()=>{let t={};return e.forEach((e,n)=>t[e.id]=n===0),t});return(0,R.jsx)(q,{sx:Kb.root,children:(0,R.jsx)(xg,{disablePadding:!0,children:e.map(e=>{let i=!!n[e.id],a=e.pages.length===1,o=e.pages[0];return a?(0,R.jsxs)(q,{children:[(0,R.jsx)(Og,{component:$t,to:o.path,selected:t.pathname===o.path,children:(0,R.jsx)(Y,{primary:(0,R.jsx)(K,{sx:Kb.sectionTitle,children:e.title})})}),(0,R.jsx)(ig,{})]},e.id):(0,R.jsxs)(q,{children:[(0,R.jsxs)(Og,{onClick:()=>r({...n,[e.id]:!n[e.id]}),children:[(0,R.jsx)(Y,{primary:(0,R.jsx)(K,{sx:Kb.sectionTitle,children:e.title})}),i?(0,R.jsx)(Wb,{}):(0,R.jsx)(Gb,{})]}),(0,R.jsx)(Ju,{in:i,timeout:`auto`,unmountOnExit:!0,children:(0,R.jsx)(xg,{disablePadding:!0,children:e.pages.map(e=>(0,R.jsx)(Og,{component:$t,to:e.path,selected:t.pathname===e.path,sx:Kb.childItem,children:(0,R.jsx)(Y,{primary:e.title})},e.id))})}),(0,R.jsx)(ig,{})]},e.id)})})})};var Jb={root:{display:`flex`,justifyContent:`space-between`,borderTop:1,borderColor:`divider`,mt:4,pt:2}};const Yb=()=>{let e=He(),{pathname:t}=ze(),n=(0,x.useMemo)(()=>zb.findIndex(e=>e.path===t),[t]),r=n>0?zb[n-1]:void 0,i=n>=0&&n<zb.length-1?zb[n+1]:void 0;return(0,R.jsxs)(q,{sx:Jb.root,children:[(0,R.jsx)(`span`,{children:r&&(0,R.jsxs)(eh,{onClick:()=>e(r.path),variant:`text`,children:[`← `,r.title]})}),(0,R.jsx)(`span`,{children:i&&(0,R.jsxs)(eh,{onClick:()=>e(i.path),variant:`text`,children:[i.title,` →`]})})]})};var Xb=Il((0,R.jsx)(`path`,{d:`M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z`}),`Close`),Zb={header:{display:`flex`,alignItems:`center`,gap:1,mb:1},inputPaper:{p:1,display:`flex`,alignItems:`center`,gap:1,flex:1},noResults:{p:2},content:{pt:1}};const Qb=({open:e,onClose:t})=>{let n=He(),[r,i]=(0,x.useState)(``),a=(0,x.useMemo)(()=>{let e=r.trim().toLowerCase();return e?zb.filter(t=>t.title.toLowerCase().includes(e)||(t.keywords||[]).some(t=>t.toLowerCase().includes(e))):[]},[r]);return(0,x.useEffect)(()=>{e||i(``)},[e]),(0,R.jsx)(qh,{open:e,onClose:t,fullWidth:!0,maxWidth:`md`,children:(0,R.jsxs)(Qh,{sx:Zb.content,children:[(0,R.jsxs)(q,{sx:Zb.header,children:[(0,R.jsxs)(Qu,{sx:Zb.inputPaper,variant:`outlined`,children:[(0,R.jsx)(Bb,{}),(0,R.jsx)(Fm,{autoFocus:!0,placeholder:`Search docs…`,value:r,onChange:e=>i(e.target.value),onKeyDown:e=>{e.key===`Escape`&&t(),e.key===`Enter`&&a[0]&&(t(),n(a[0].path))},sx:{flex:1}})]}),(0,R.jsx)(zd,{onClick:t,"aria-label":`Close search`,children:(0,R.jsx)(Xb,{})})]}),(0,R.jsxs)(xg,{children:[a.map(e=>(0,R.jsx)(Og,{onClick:()=>{t(),n(e.path)},children:(0,R.jsx)(Y,{primary:e.title,secondary:(e.keywords||[]).join(`, `)})},e.id)),r&&a.length===0&&(0,R.jsx)(K,{sx:Zb.noResults,color:`text.secondary`,children:`No results`})]})]})})};var $b={shell:{display:`grid`,gridTemplateRows:`auto 1fr`,height:`100vh`},grid:{display:`grid`,gridTemplateColumns:`280px 1fr`,minHeight:0},content:{p:3,overflow:`auto`},main:{maxWidth:980}};const ex=dl({palette:{mode:`dark`,background:{default:`#181c26`,paper:`#202532`},divider:`#343a4c`,primary:{main:`#4cc2ff`},text:{primary:`#e5e9f5`,secondary:`#a0a8c0`}},shape:{borderRadius:8},typography:{fontSize:14,body1:{lineHeight:1.3,color:`#a0a8c0`},body2:{lineHeight:1.3,color:`#a0a8c0`},h4:{fontSize:28,fontWeight:600,letterSpacing:.2,color:`#e5e9f5`},h5:{fontSize:20,fontWeight:600,letterSpacing:.15,marginTop:16,color:`#e5e9f5`}},components:{MuiCssBaseline:{styleOverrides:{code:{fontFamily:`ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace`,backgroundColor:`#202532`,borderRadius:4,padding:`2px 6px`,border:`1px solid #343a4c`,fontSize:`0.85em`}}}}});(0,jo.createRoot)(document.getElementById(`root`)).render((0,R.jsx)(x.StrictMode,{children:(0,R.jsx)(Xt,{children:(0,R.jsx)(()=>{let[e,t]=(0,x.useState)(!1);return(0,R.jsxs)(Cl,{theme:ex,children:[(0,R.jsx)(lh,{}),(0,R.jsxs)(q,{sx:$b.shell,children:[(0,R.jsx)(Ub,{onOpenSearch:()=>t(!0)}),(0,R.jsxs)(q,{sx:$b.grid,children:[(0,R.jsx)(qb,{}),(0,R.jsx)(q,{sx:$b.content,children:(0,R.jsxs)(pt,{children:[Rb.flatMap(e=>e.pages).map(e=>(0,R.jsx)(dt,{path:e.path,element:(0,R.jsxs)(q,{component:`main`,sx:$b.main,children:[e.element(),(0,R.jsx)(Yb,{})]})},e.id)),(0,R.jsx)(dt,{path:`*`,element:(0,R.jsxs)(R.Fragment,{children:[(0,R.jsx)(K,{variant:`h5`,gutterBottom:!0,children:`Not Found`}),(0,R.jsx)(gg,{component:$t,to:`/`,children:`Go to Introduction`})]})})]})})]})]}),(0,R.jsx)(Qb,{open:e,onClose:()=>t(!1)})]})},{})})}));
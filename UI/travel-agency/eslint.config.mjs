import js from '@eslint/js';
import react from 'eslint-plugin-react';
import globals from 'globals';
export default [
  {ignores:['build/**','node_modules/**','test-results/**']},
  js.configs.recommended,
  {files:['src/**/*.{js,jsx}'],
    languageOptions:{ecmaVersion:'latest',sourceType:'module',parserOptions:{ecmaFeatures:{jsx:true}},
      globals:{...globals.browser,...globals.node,...globals.jest,vi:'readonly'}},
    plugins:{react},settings:{react:{version:'detect'}},
    rules:{'react/jsx-uses-react':'error','react/jsx-uses-vars':'error','no-unused-vars':['warn',{argsIgnorePattern:'^_'}]}
  }
];

import { useState } from 'react';
import { Provider } from 'react-redux';
import { store } from './store';

import Container from 'react-bootstrap/Container';

import Sheet1 from './components/sheet-1/Sheet1'
import Sheet2 from './components/sheet-2/Sheet2';
import Sheet3 from './components/sheet-3/Sheet3';

function App() {
  return (
    <Provider store={store}>
      <Container>
        <Sheet1 />
        <Sheet2 />
        <Sheet3 />
      </Container>
    </Provider>
  );
}

export default App;
import { useState } from 'react';
import { Provider } from 'react-redux';
import { store } from './store';

import Container from 'react-bootstrap/Container';

import Sheet1 from './components/sheet-1/Sheet1.jsx'
import Sheet2 from './components/sheet-2/Sheet2.jsx';

function App() {
  return (
    <Provider store={store}>
      <Container>
        <Sheet1 />
        <Sheet2 />
      </Container>
    </Provider>
  );
}

export default App;
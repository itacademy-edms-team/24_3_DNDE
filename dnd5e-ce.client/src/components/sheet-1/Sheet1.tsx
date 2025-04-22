import Sheet1Header from './Sheet1Header'
import Sheet1Body from './Sheet1Body';

import Form from 'react-bootstrap/Form';
import Container  from 'react-bootstrap/Container';

function Sheet1() {
  return (
    <Form>
      <Container>
        <Sheet1Header/>
        <Sheet1Body/>
      </Container>
    </Form>
  );
}

export default Sheet1;
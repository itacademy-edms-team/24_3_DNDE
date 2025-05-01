import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import Card from 'react-bootstrap/Card';
import Accordion from 'react-bootstrap/Accordion'
import InputGroup from 'react-bootstrap/InputGroup';

import SpellCardComponent from './SpellCardComponent';


import CharacterSpellcastingCantripsSVG from './assets/CharacterSpellcastingCantrips.svg';
import CharacterSpellcastingLevel1SVG from './assets/CharacterSpellcastingLevel1.svg';
import CharacterSpellcastingLevel2SVG from './assets/CharacterSpellcastingLevel2.svg';
import CharacterSpellcastingLevel3SVG from './assets/CharacterSpellcastingLevel3.svg';
import CharacterSpellcastingLevel4SVG from './assets/CharacterSpellcastingLevel4.svg';
import CharacterSpellcastingLevel5SVG from './assets/CharacterSpellcastingLevel5.svg';
import CharacterSpellcastingLevel6SVG from './assets/CharacterSpellcastingLevel6.svg';
import CharacterSpellcastingLevel7SVG from './assets/CharacterSpellcastingLevel7.svg';
import CharacterSpellcastingLevel8SVG from './assets/CharacterSpellcastingLevel8.svg';
import CharacterSpellcastingLevel9SVG from './assets/CharacterSpellcastingLevel9.svg';


const Sheet3Body: React.FC = () => {
  return (
    <Container className='mt-4'>
      <Row>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <SpellCardComponent
            spellLevel={0}
            title={"Заговоры"}
            svgImage={CharacterSpellcastingCantripsSVG}            
          />
          <SpellCardComponent
            spellLevel={1}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel1SVG}
          />
          <SpellCardComponent
            spellLevel={2}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel2SVG}
          />
          <SpellCardComponent
            spellLevel={3}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel3SVG}
          />
        </Col>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <SpellCardComponent
            spellLevel={4}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel4SVG}
          />
          <SpellCardComponent
            spellLevel={5}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel5SVG}
          />
          <SpellCardComponent
            spellLevel={6}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel6SVG}
          />
        </Col>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <SpellCardComponent
            spellLevel={7}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel7SVG}
          />
          <SpellCardComponent
            spellLevel={8}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel8SVG}
          />
          <SpellCardComponent
            spellLevel={9}
            title={"Заклинания"}
            svgImage={CharacterSpellcastingLevel9SVG}
          />
        </Col>
      </Row>
    </Container>
  );
};

export default Sheet3Body;
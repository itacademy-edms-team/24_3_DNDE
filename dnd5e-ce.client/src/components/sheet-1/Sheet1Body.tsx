import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Button from 'react-bootstrap/Button';

import CharacteristicList from './CharacteristicList';
import InspirationCard from './InspirationCard';
import ProficiencyBonusCard from './ProficiencyBonusCard';
import SaveThrowList from './SaveThrowList';
import SkillList from './SkillList';

import PassivePerceptionCard from './PassivePerceptionCard';
import ToolProficienciesAndCustomSkillsCard from './ToolProficienciesAndCustomSkillsCard';
import OtherProficienciesAndCustomSkillsCard from './OtherProficienciesAndCustomSkillsCard';

import ArmorClassCard from './ArmorClassCard';
import CharacterInitiativeCard from './CharacterInitiativeCard';
import CharacterSpeedCard from './CharacterSpeedCard';
import CurrentHitsCard from './CurrentHitsCard';
import TempHitsCard from './TempHitsCard';
import HitDiceCard from './HitDiceCard';
import DeathSaveThrowsCard from './DeathSaveThrowsCard';
import AttacksCard from './AttacksCard';
import GlobalDamageModifiersCard from './GlobalDamageModifiersCard';
import InventoryCard from './InventoryCard';

import PersonalityTraitsCard from './PersonalityTraitsCard';
import CharacterIdeals from './CharacterIdeals';
import CharacterBonds from './CharacterBonds';
import CharacterFlaws from './CharacterFlaws';
import CharacterResourceList from './CharacterResourceList';
//import ClassResource from './ClassResource';


const Sheet1Body: React.FC = () => {
  return (
    <Container className='mt-4'>
      <Row>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <Row>
            <Col md={4} className="d-flex flex-column gap-3">
              <CharacteristicList/>
            </Col>
            <Col md={8} className="d-flex flex-column gap-3">
              <InspirationCard/>
              <ProficiencyBonusCard/>
              <SaveThrowList/>
              <SkillList/>
            </Col>
          </Row>
          <Row className="d-flex flex-column gap-3">
            <PassivePerceptionCard/>
            <ToolProficienciesAndCustomSkillsCard/>
            <OtherProficienciesAndCustomSkillsCard/>
          </Row>
        </Col>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <Row>
            <Col md={4}>
              <ArmorClassCard/>
            </Col>
            <Col md={4}>
              <CharacterInitiativeCard/>
            </Col>
            <Col md={4}>
              <CharacterSpeedCard/>
            </Col>
          </Row>
          <Row>
            <Col className="d-flex flex-column gap-3">
              <CurrentHitsCard/>
              <TempHitsCard/>
              <Row>
                <Col md={6}>
                  <HitDiceCard/>
                </Col>
                <Col md={6}>
                  <DeathSaveThrowsCard />
                </Col>
              </Row>
            </Col>
          </Row>
          <Row>
            <Col className="d-flex flex-column gap-3">
              <AttacksCard />
              <GlobalDamageModifiersCard />
              <InventoryCard />
            </Col>
          </Row>
        </Col>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <Row>
            <Col className="d-flex flex-column gap-3">
              <PersonalityTraitsCard />
              <CharacterIdeals />
              <CharacterBonds />
              <CharacterFlaws />
            </Col>
          </Row>
          <Row>
            <Col>
              <Button
                variant="primary"
                className="h-100"
              >
                Короткий отдых
              </Button>
            </Col>
            <Col>
              <Button
                variant="primary"
                className="h-100"
              >
                Длительный отдых
              </Button>
            </Col>
          </Row>
          <Row>
            <CharacterResourceList />
          </Row>
        </Col>
      </Row>
    </Container>
  );
}

export default Sheet1Body;
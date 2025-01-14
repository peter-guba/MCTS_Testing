/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package ai.abstraction;

import ai.abstraction.pathfinding.PathFinding;
import rts.GameState;
import rts.PhysicalGameState;
import rts.ResourceUsage;
import rts.UnitAction;
import rts.units.Unit;
import util.XMLWriter;

/**
 *
 * @author santi
 */
public class Attack extends AbstractAction  {
    public Unit target;
    public PathFinding pf;
    
    public Attack(Unit u, Unit a_target, PathFinding a_pf) {
        super(u);
        target = a_target;
        pf = a_pf;
    }
    
    
    public boolean completed(GameState gs) {
        PhysicalGameState pgs = gs.getPhysicalGameState();
        return !pgs.getUnits().contains(target);
    }
    
    
    public boolean equals(Object o)
    {
        if (!(o instanceof Attack)) return false;
        Attack a = (Attack)o;
        return target.getID() == a.target.getID() && pf.getClass() == a.pf.getClass();
    }

    
    public void toxml(XMLWriter w)
    {
        w.tagWithAttributes("Attack","unitID=\""+unit.getID()+"\" target=\""+target.getID()+"\" pathfinding=\""+pf.getClass().getSimpleName()+"\"");
        w.tag("/Attack");
    }
    

    public UnitAction execute(GameState gs, ResourceUsage ru) {
        
        int dx = target.getX()-unit.getX();
        int dy = target.getY()-unit.getY();
        double d = Math.sqrt(dx*dx+dy*dy);
        if (d<=unit.getAttackRange()) {
            return new UnitAction(UnitAction.TYPE_ATTACK_LOCATION,target.getX(),target.getY());
        } else {
            // move towards the unit:
    //        System.out.println("AStarAttak returns: " + move);
            UnitAction move = pf.findPathToPositionInRange(unit, target.getX()+target.getY()*gs.getPhysicalGameState().getWidth(), unit.getAttackRange(), gs, ru);
            if (move!=null && gs.isUnitActionAllowed(unit, move)) return move;
            return null;
        }        
    }
}

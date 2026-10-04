import sys
def load(p):
    d={'back':set(),'memo':{},'order':[],'hdr':{}}
    for l in open(p):
        l=l.rstrip('\n'); k,_,v=l.partition(' ')
        if k=='back': d['back'].add(v)
        elif k=='memo': s,_,r=v.partition(' '); d['memo'][r]=int(s)
        elif k=='order': d['order'].append(v)
        elif k=='memoWords': d['hdr']['memoWords']=v
        elif k=='rules': d['hdr']['rules']=v
        elif k=='publications': d['hdr']['pubs']=v
    return d
host_before=load(sys.argv[1]); host_after=load(sys.argv[2]); guests=[load(p) for p in sys.argv[3:]]
hr=set(host_before['order'])
print('host before:',host_before['hdr']['rules'],'memoWords',host_before['hdr']['memoWords'],'memo',len(host_before['memo']),'back',len(host_before['back']))
print('host after: ',host_after['hdr']['rules'],'memoWords',host_after['hdr']['memoWords'],'memo',len(host_after['memo']),'back',len(host_after['back']))
hb=host_after['back']; 
def frm(e): return e.split(' -> ')[0]
print('host-rule back edges identical:', {e for e in hb if frm(e) in hr}==host_before['back'])
print('back edges added (from non-host rules):', sorted(e for e in hb if frm(e) not in hr))
print('host-rule memo slots identical:', {r:s for r,s in host_after['memo'].items() if r in hr}==host_before['memo'])
print('memo added:', {r:s for r,s in host_after['memo'].items() if r not in hr})
print('host-rule order identical (prefix):', host_after['order'][:len(host_before['order'])]==host_before['order'])
print('appended rules:', host_after['order'][len(host_before['order']):])
for g in guests:
    print('guest',g['hdr']['pubs'][:80],'rules',g['hdr']['rules'],'memoWords',g['hdr']['memoWords'],'memo',len(g['memo']),'back',len(g['back']))
    gr=set(g['order'])
    # guest's own back edges vs host's back edges restricted to guest rules
    hg={e for e in hb if frm(e) in gr and e.split(' -> ')[1] in gr}
    print('  back edges among guest rules: guest-alone',len(g['back']),'in host',len(hg),'only-guest',len(g['back']-hg),'only-host',len(hg-g['back']))
    print('  memo rules among guest rules: guest-alone',len(g['memo']),'in host',sum(1 for r in host_after['memo'] if r in gr))
